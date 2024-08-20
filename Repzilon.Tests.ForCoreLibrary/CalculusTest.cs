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
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class CalculusTest
	{
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

			Console.WriteLine("20! is {0}", ExtraMath.Factorial(20));

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
			Console.WriteLine("∫[0; 1][𝒩(0; 1)]\t≈ {0:f16}  Δ = {1:e7}\tSérie de MacLaurin  (o=30 fonctionne qu'avec z=1)",
			 dblOneOfRoot2Pi * dblIntegral, (dblOneOfRoot2Pi * dblIntegral) - karExpected[0] + 0.5);

			OutputNormalIntegral(1, karExpected[0], 0.5 + Integral.Riemann(0, 1, ProbabilityDistributions.RiemannIterations, NonCumulativeNormal), "Somme de Riemann                  (o=n=" + ProbabilityDistributions.RiemannIterations + ")");

			const int n = 1482 * 14; // must be a multiple of 6
			for (i = 0; i < karZ.Length; i++) {
				var z = Math.Round(karZ[i], 2);
				OutputNormalIntegral(z, karExpected[i], ProbabilityDistributions.Normal(z, true), "Somme de Riemann embarquée        (o=n=" + ProbabilityDistributions.RiemannIterations + ")");
				OutputNormalIntegral(z, karExpected[i], 0.5 + Integral.SimpsonComposite(0, z, n, NonCumulativeNormal), "Méthode composite de Simpson      (n=" + n + " o=" + (n + 1) + ")");
				OutputNormalIntegral(z, karExpected[i], 0.5 + Integral.SimpsonCompositeThreeEights(0, z, n, NonCumulativeNormal), "Méthode 3/8e composite de Simpson (n=" + n + " o=" + (n + 1) + ")");
				OutputNormalIntegral(z, karExpected[i], 0.5 + Integral.SimpsonThreeEights(0, z, NonCumulativeNormal), "Méthode 3/8e de Simpson           (o=4)");
				OutputNormalIntegral(z, karExpected[i], 0.5 + Integral.SimpsonFirst(0, z, NonCumulativeNormal), "1re méthode de Simpson            (o=3)");
			}
		}

		private static void OutputNormalIntegral(double z, double expected, double integral, string source)
		{
			Console.WriteLine("∫[-∞; {0}][𝒩(0; 1)]\t≈ {1:f16}  Δ = {2:e7}\t{3}", z, integral, integral - expected, source);
		}

		private static double NonCumulativeNormal(double z)
		{
			return ProbabilityDistributions.Normal(z, false);
		}

		private static double ExponentialSeries(double x)
		{
			double sum = 0;
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
