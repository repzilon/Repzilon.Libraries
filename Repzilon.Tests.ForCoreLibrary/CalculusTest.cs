//
//  CalculusTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Collections.Generic;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class CalculusTest
	{
		private static readonly decimal OneOfRootOfTwoPi = 1.0m / ExtraMath.Sqrt(2 * ExtraMath.Pi);

		internal static void Run(string[] args)
		{
			Console.WriteLine("Calcul intégral travail 1 #2");
			SummationTest(10000, 729, "Math.Pow", CalculusWork1No2Fp);
			SummationTest(10000, 729, "Pow(i32, u16)", CalculusWork1No2Int64);
			SummationTest(10000, 729, "IIf", CalculusWork1No2IIf);
			SummationTest(10000, 729, "IIfn", CalculusWork1No2IIfn);
			SummationTest(10000, 729, "IIfd", CalculusWork1No2IIfd);

			Console.WriteLine("Distributions de Student");
			byte[] karLiberties = new byte[] { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233 };
			int i;
			Console.Write("x");
			for (i = 0; i < karLiberties.Length; i++) {
				Console.Write("\tk=");
				Console.Write(karLiberties[i]);
			}
			Console.Write(Environment.NewLine);
			for (var x = -30; x <= 30; x++) {
				var z = RoundOff.Error(x * 0.1);
				Console.Write(z.ToString("f1"));
				for (i = 0; i < karLiberties.Length; i++) {
					var v = ProbabilityDistributions.Student(z, karLiberties[i], false);
					Console.Write('\t');
					Console.Write(v.ToString("f5"));
				}
				Console.Write(Environment.NewLine);
			}

			Console.WriteLine("20! vaut {0}", ExtraMath.Factorial(20));

			Console.WriteLine("Intégrale d'une loi normale centrée réduite");
			var karZ = new float[] { 1, 1.23f, 1.96f, 2, 3 };
			var karExpected = new double[] {
				0.8413447460685429485852325456320379224779129667266043909873944502429914419872048295008849184056393275,
				0.8906514475743080619676160383928930592976734158609286700391849984625801751987538510935072839844455582,
				0.9750021048517795658634157309591628099775002209381166089142828958711815739963335013205260350450632762,
				0.9772498680518207927997173628334665625282237762983215660163339998695237096472242516517308479242103851,
				0.9986501019683699054733481852324050226221706318416193506357780146441942792354278997319614187139957829
			};
			var dblOneOfRoot2Pi = 1.0 / Math.Sqrt(2 * Math.PI);
			var dblIntegral = 1 + ExponentialSeries(1.0) - ExponentialSeries(0.0);
			var dblTargetDelta = (dblOneOfRoot2Pi * dblIntegral) - karExpected[0] + 0.5;
			Console.WriteLine("∫[0; 1][𝒩(0; 1)]\t≈ {0:f16}  Δ = {1:e7}\tSérie de MacLaurin  (o=30 fonctionne qu'avec z=1)",
			 dblOneOfRoot2Pi * dblIntegral, dblTargetDelta);

			OutputNormalIntegral(1, karExpected[0], 0.5 + Integral.Riemann(0, 1.0, ProbabilityDistributions.RiemannIterations, NonCumulativeNormal), "Somme de Riemann                  (o=n=" + ProbabilityDistributions.RiemannIterations + ")");

			const int n = 1482 * 14; // must be a multiple of 6
			for (i = 0; i < karZ.Length; i++) {
				var z = Math.Round(karZ[i], 2);
				OutputNormalIntegral(z, karExpected[i], ProbabilityDistributions.Normal(z, true), "Somme de Riemann embarquée        (o=n=" + ProbabilityDistributions.RiemannIterations + ")");
				OutputNormalIntegral(z, karExpected[i], 0.5 + Integral.SimpsonComposite(0, z, n, NonCumulativeNormal), "Méthode composite de Simpson      (n=" + n + " o=" + (n + 1) + ")");
				OutputNormalIntegral(z, karExpected[i], 0.5 + Integral.SimpsonCompositeThreeEights(0, z, n, NonCumulativeNormal), "Méthode 3/8e composite de Simpson (n=" + n + " o=" + (n + 1) + ")");
				OutputNormalIntegral(z, karExpected[i], 0.5 + Integral.SimpsonThreeEights(0, z, NonCumulativeNormal), "Méthode 3/8e de Simpson           (o=4)");
				OutputNormalIntegral(z, karExpected[i], 0.5 + Integral.SimpsonFirst(0, z, NonCumulativeNormal), "1re méthode de Simpson            (o=3)");
			}

			Console.WriteLine("Détermination du nombre d'itérations idéales pour estimer l'intégrale (Double)");
			FindBestIterationCountForNormalLawIntegral(karZ, karExpected, Math.Abs(dblTargetDelta));

			var karExpectedDecimal = new decimal[] {
				0.8413447460685429485852325456320379224779129667266043909873944502429914419872048295008849184056393275m,
				0.8906514475743080619676160383928930592976734158609286700391849984625801751987538510935072839844455582m,
				0.9750021048517795658634157309591628099775002209381166089142828958711815739963335013205260350450632762m,
				0.9772498680518207927997173628334665625282237762983215660163339998695237096472242516517308479242103851m,
				0.9986501019683699054733481852324050226221706318416193506357780146441942792354278997319614187139957829m
			};
			var dcmIntegral = 1 + ExponentialSeries(1.0m) - ExponentialSeries(0.0m);
			var dcmTargetDelta = (OneOfRootOfTwoPi * dcmIntegral) - karExpectedDecimal[0] + 0.5m;
			Console.Write(Environment.NewLine);
			Console.WriteLine("∫[0; 1][𝒩(0; 1)]\t≈ {0}  Δ = {1:e}\tSérie de MacLaurin  (o=30 fonctionne qu'avec z=1)",
			 OneOfRootOfTwoPi * dcmIntegral, dcmTargetDelta);
			Console.WriteLine("Détermination du nombre d'itérations idéales pour estimer l'intégrale (Decimal)");
			FindBestIterationCountForNormalLawIntegral(karZ, karExpectedDecimal, Math.Abs(dcmTargetDelta));
		}

		private static int FindBestIterationCountForNormalLawIntegral(float[] allZ, double[] expected, double targetDelta)
		{
			int i;
			var lstIterations = new List<int>();
			for (i = 0; i < allZ.Length; i++) {
				var z = Math.Round(allZ[i], 2);
				bool blnFound = false;
				for (int n = 30; (!blnFound) && (n <= 9999996); n += 6) {
					var r = 0.5 + Integral.Riemann(0, z, n, NonCumulativeNormal);
					var s1 = 0.5 + Integral.SimpsonComposite(0, z, n, NonCumulativeNormal);
					var s2 = 0.5 + Integral.SimpsonCompositeThreeEights(0, z, n, NonCumulativeNormal);
					if (MoreExact(r, s1, s2, expected[i], targetDelta)) {
						blnFound = true;
						lstIterations.Add(n);
						OutputNormalIntegral(z, expected[i], r, "Somme de Riemann                  (o=n=" + n + ")");
						OutputNormalIntegral(z, expected[i], s1, "Méthode composite de Simpson      (n=" + n + " o=" + (n + 1) + ")");
						OutputNormalIntegral(z, expected[i], s2, "Méthode 3/8e composite de Simpson (n=" + n + " o=" + (n + 1) + ")");
					}
				}
			}
			double average = 0;
			for (i = 0; i < allZ.Length; i++) {
				average += lstIterations[i];
			}
			average /= allZ.Length;
			double stddev = 0;
			for (i = 0; i < allZ.Length; i++) {
				var d = lstIterations[i] - average;
				stddev += d * d;
			}
			stddev /= (allZ.Length - 1);
			stddev = Math.Sqrt(stddev);
			const float kT99percent_4degrees = 4.60409f;
			var ideal = average + kT99percent_4degrees * stddev;
			ideal = Math.Ceiling(ideal / 6) * 6;
			Console.WriteLine("x_={0} itérations  s={1}  n={2}  t99={3}  x^={4} itérations", average, stddev, lstIterations.Count, kT99percent_4degrees, ideal);
			return Convert.ToInt32(ideal);
		}

		private static int FindBestIterationCountForNormalLawIntegral(float[] allZ, decimal[] expected, decimal targetDelta)
		{
			int i;
			var lstIterations = new List<int>();
			for (i = 0; i < allZ.Length; i++) {
				decimal z = (decimal)Math.Round(allZ[i], 2);
				bool blnFound = false;
				for (int n = 30; (!blnFound) && (n <= 65532); n += 6) {
					var r = 0.5m + Integral.Riemann(0, z, n, NonCumulativeNormal);
					var s1 = 0.5m + Integral.SimpsonComposite(0, z, n, NonCumulativeNormal);
					var s2 = 0.5m + Integral.SimpsonCompositeThreeEights(0, z, n, NonCumulativeNormal);
					if (MoreExact(r, s1, s2, expected[i], targetDelta)) {
						blnFound = true;
						lstIterations.Add(n);
						OutputNormalIntegral(z, expected[i], r, "Somme de Riemann                  (o=n=" + n + ")");
						OutputNormalIntegral(z, expected[i], s1, "Méthode composite de Simpson      (n=" + n + " o=" + (n + 1) + ")");
						OutputNormalIntegral(z, expected[i], s2, "Méthode 3/8e composite de Simpson (n=" + n + " o=" + (n + 1) + ")");
					}
				}
			}
			decimal average = 0;
			for (i = 0; i < allZ.Length; i++) {
				average += lstIterations[i];
			}
			average /= allZ.Length;
			decimal stddev = 0;
			for (i = 0; i < allZ.Length; i++) {
				var d = lstIterations[i] - average;
				stddev += d * d;
			}
			stddev /= (allZ.Length - 1);
			stddev = ExtraMath.Sqrt(stddev);
			const decimal kT99percent_4degrees = 4.60409m;
			var ideal = average + kT99percent_4degrees * stddev;
			ideal = Math.Ceiling(ideal / 6) * 6;
			Console.WriteLine("x_={0} itérations  s={1}  n={2}  t99={3}  x^={4} itérations", average, stddev, lstIterations.Count, kT99percent_4degrees, ideal);
			return Convert.ToInt32(ideal);
		}

		private static bool MoreExact(double r, double s1, double s2, double expected, double target)
		{
			return MoreExact(s1, expected, target) || MoreExact(s2, expected, target) || MoreExact(r, expected, target);
		}

		private static bool MoreExact(double value, double expected, double target)
		{
			var delta = Math.Abs(value - expected);
			return (delta < target); // && !delta.ToString("e7").EndsWith("0000000e+000");
		}

		private static void OutputNormalIntegral(double z, double expected, double integral, string source)
		{
			Console.WriteLine("∫[-∞; {0}][𝒩(0; 1)]\t≈ {1:f16}  Δ = {2:e7}\t{3}", z, integral, integral - expected, source);
		}

		private static bool MoreExact(decimal r, decimal s1, decimal s2, decimal expected, decimal target)
		{
			return MoreExact(s1, expected, target) || MoreExact(s2, expected, target) || MoreExact(r, expected, target);
		}

		private static bool MoreExact(decimal value, decimal expected, decimal target) 
		{
			var delta = Math.Abs(value - expected);
			return (delta < target); // && !delta.ToString("e7").EndsWith("0000000e+000");
		}

		private static void OutputNormalIntegral(decimal z, decimal expected, decimal integral, string source)
		{
			Console.WriteLine("∫[-∞; {0}][𝒩(0; 1)]\t≈ {1:f16}  Δ = {2:e7}\t{3}", z, integral, integral - expected, source);
		}

		private static double NonCumulativeNormal(double z)
		{
			return ProbabilityDistributions.Normal(z, false);
		}

		private static decimal NonCumulativeNormal(decimal z)
		{
			//return (decimal)ProbabilityDistributions.Normal((double)z, false);
			return OneOfRootOfTwoPi * (decimal)Math.Exp((double)(-0.5m * z * z));
		}

		private static double ExponentialSeries(double x)
		{
			double sum = 0;
			for (var k = 1; k <= 16 - 1; k++) {
				sum += ExponentialSuite(x, k);
			}
			return sum;
		}

		private static decimal ExponentialSeries(decimal x)
		{
			decimal sum = 0;
			for (var k = 1; k <= 16 - 1; k++) {
				sum += ExponentialSuite(x, k);
			}
			return sum;
		}

		private static double ExponentialSuite(double x, int k)
		{
			var odd = (2 * k) + 1;
#if DEBUG
			var t = Math.Pow(-1, k) * Math.Pow(x, odd);
			var d = checked(odd * (1 << k) * ExtraMath.Factorial((byte)k));
			var r = t / d;
			Console.WriteLine("x={0} k={1} {2}/{3}={4}", x, k, t, d, r);
			return r;
#else
			return (Math.Pow(-1, k) * Math.Pow(x, odd)) / (odd * (1 << k) * ExtraMath.Factorial((byte)k));
#endif
		}

		private static decimal ExponentialSuite(decimal x, int k)
		{
			var odd = (2 * k) + 1;
			return (decimal)((Math.Pow(-1, k) * Math.Pow((double)x, odd)) / (odd * (1 << k) * ExtraMath.Factorial((byte)k)));
		}

#if NETFRAMEWORK
		private static void SummationTest(int benchLoops, int summationUpper, string legend, Converter<int, long> forEach)
#else
		private static void SummationTest(int benchLoops, int summationUpper, string legend, Func<int, long> forEach)
#endif
		{
			var dtmStart = DateTime.UtcNow;
			for (int i = 0; i < benchLoops; i++) {
				Integral.Summation(1, summationUpper, forEach);
			}
			var tsDuration = DateTime.UtcNow - dtmStart;
			long result = Integral.Summation(1, summationUpper, forEach);
			Console.WriteLine("={0}\t{2,-16} {1,7:n0} Hz", result, benchLoops / tsDuration.TotalSeconds, legend);
		}

		private static long CalculusWork1No2Fp(int k)
		{
			return 3 * k * (long)Math.Pow(-1, k - 1);
		}

		// The checked part here has almost no performance penalty
		private static long CalculusWork1No2Int64(int k)
		{
			return 3 * k * Pow(-1, checked((ushort)(k - 1)));
		}

		private static long CalculusWork1No2IIf(int k)
		{
			return 3 * k * (((k - 1) % 2 == 0) ? 1 : -1);
		}

		private static long CalculusWork1No2IIfn(int k)
		{
			return 3 * k * ((k % 2 == 0) ? -1 : 1);
		}

		private static long CalculusWork1No2IIfd(int k)
		{
			return 3 * k * ((k % 2 != 0) ? 1 : -1);
		}

		[Obsolete("15 times slower than Math.Pow.")]
		private static long Pow(int b, ushort e)
		{
			long r = 1;
			for (var i = 1; i <= e; i++) {
				r *= b;
			}
			return r;
		}
	}
}
