//
//  NormalLawTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024-2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Collections.Generic;
using Repzilon.Libraries.Core;
using Repzilon.Libraries.Core.Regression;
// ReSharper disable RedundantExplicitArrayCreation

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class NormalLawTest
	{
		private static readonly decimal DecimalOneOfRootOfTwoPi = Decimal.One / ExtraMath.Sqrt(ExtraMath.Tau);
		private static readonly double DoubleOneOfRootOfTwoPi;
		private static readonly double SqrtEighthOfPi;
		internal static readonly Dictionary<int, decimal> StudentT99TwoSidedScores;

#pragma warning disable S3963 // "static" fields should be initialized inline
		static NormalLawTest()
		{
#pragma warning disable U2U1000 // Local variable can be inlined or declared const
			// ReSharper disable once ConvertToConstant.Local
			/*const*/ byte kTwo = 2;
#pragma warning restore U2U1000 // Local variable can be inlined or declared const
			var sqrtPi = Math.Sqrt(Math.PI);
			var sqrt2 = Math.Sqrt(kTwo);
			DoubleOneOfRootOfTwoPi = sqrt2 / (kTwo * sqrtPi);	// 1÷√2π equals to √2÷(2√π)
			SqrtEighthOfPi = sqrtPi / (kTwo * sqrt2);			// √(π÷8) = √π÷√8 = √π÷(2√2)
			var dicStudent = new Dictionary<int, decimal>();
			dicStudent.Add(4, 4.604094871349993225385464412853251257128286533876668465m);
			StudentT99TwoSidedScores = dicStudent;
		}
#pragma warning restore S3963 // "static" fields should be initialized inline

		#region Target delta
		internal static decimal FinalTargetDelta()
		{
#if true
			return (decimal)Math.Abs(DoubleTargetDelta());
#else
			return Math.Abs(DecimalTargetDelta());
#endif
		}

		private static double IntegralInDouble()
		{
			return 1 + ExponentialSeries(1.0) - ExponentialSeries(0.0);
		}

		private static double DoubleTargetDelta()
		{
			const double kExpected = 0.8413447460685429485852325456320379224779129667266043909873944502429914419872048295008849184056393275;
			return (DoubleOneOfRootOfTwoPi * IntegralInDouble()) - kExpected + 0.5;
		}

		private static decimal IntegralInDecimal()
		{
			return 1 + ExponentialSeries(Decimal.One) - ExponentialSeries(0.0m);
		}

		private static decimal DecimalTargetDelta()
		{
			const decimal kExpected = 0.8413447460685429485852325456320379224779129667266043909873944502429914419872048295008849184056393275m;
			return (DecimalOneOfRootOfTwoPi * IntegralInDecimal()) - kExpected + 0.5m;
		}
		#endregion

		internal static void Run(string[] args)
		{
			int i, j;
			// ReSharper disable once TooWideLocalVariableScope
			double z, p;

			Program.OutputHeading("Biofermentation semaine 3 exercice");
			LogisticModel(new PointM(0, 1.5m), new PointM(5, 2), new PointM(9, 3.5m), new PointM(13, 6.2m),
			 new PointM(16, 8.2m), new PointM(20, 9.4m), new PointM(24, 9.8m), new PointM(28, 9.9m));

			Program.OutputHeading("Biofermentation laboratoire 5 saturation en oxygène");
			LogisticModel(new PointM(0, 105.1m), new PointM(0.5m, 103.7m), new PointM(1, 98.4m),
			 new PointM(1.5m, 85.5m), new PointM(2, 51.6m), new PointM(2.5m, 1), new PointM(3, 0.8m),
			 new PointM(3.5m, 0.3m));

			Program.OutputHeading("Intégrale d'une loi normale centrée réduite");
			var karZ = new float[] { 1, 1.23f, 1.96f, 2, 3 };
			var karExpected = new double[] {
				0.8413447460685429485852325456320379224779129667266043909873944502429914419872048295008849184056393275,
				0.8906514475743080619676160383928930592976734158609286700391849984625801751987538510935072839844455582,
				0.9750021048517795658634157309591628099775002209381166089142828958711815739963335013205260350450632762,
				0.9772498680518207927997173628334665625282237762983215660163339998695237096472242516517308479242103851,
				0.9986501019683699054733481852324050226221706318416193506357780146441942792354278997319614187139957829
			};
			var dblTargetDelta = DoubleTargetDelta();
			var blnUnicode     = Program.UnicodeTerminal == SupportLevel.Complete;
			Console.WriteLine(blnUnicode ?
			 "∫[0; 1][𝒩(0; 1)]\t≈ {0:f16}   Δ =  {1:e7}   Série de MacLaurin (n=16 o=30 z=1 seulement)" :
			 "S[0; 1][N(0; 1)]\t~= {0:f16} delta= {1:e7}   Série de MacLaurin (n=16 o=30 z=1 seulement)",
			 DoubleOneOfRootOfTwoPi * IntegralInDouble(), dblTargetDelta);

			const int n = 7968; // must be a multiple of 6
			for (i = 0; i < karZ.Length; i++) {
				z = Math.Round(karZ[i], 2);
				var iter = ProbabilityDistributions.Iterations(z);
				var expected = karExpected[i];
				OutputNormalIntegral(z, expected, ProbabilityDistributions.Normal(z, true),
				 "MacLaurin ou Simpson composite", iter, iter > 1000 ? iter + 1 : iter);
				OutputNormalIntegral(z, expected, 0.5 + Integral.Simpson(0, z, n, NonCumulativeNormal),
				 "Méthode composite de Simpson", n, n + 1);
				OutputNormalIntegral(z, expected, 0.5 + Integral.SimpsonThreeEights(0, z, n, NonCumulativeNormal),
				 "Méthode 3/8e composite de Simpson", n, n + 1);
				OutputNormalIntegral(z, expected, 0.5 + Integral.SimpsonThreeEights(0, z, NonCumulativeNormal),
				 "Méthode 3/8e de Simpson", 3, 4);
				OutputNormalIntegral(z, expected, 0.5 + Integral.Simpson(0, z, NonCumulativeNormal),
				 "1re méthode de Simpson", 2, 3);
				OutputNormalIntegral(z, expected, 0.5 + Integral.Riemann(0, z, n, NonCumulativeNormal),
				 "Somme de Riemann", n, n);
				OutputNormalIntegral(z, expected, 0.5 + MacLaurinPositiveNormalIntegral(z, 16),
				 "Série de MacLaurin corrigée", 16, 15);
			}

			Program.OutputHeading("Détermination du nombre d'itérations idéales pour estimer l'intégrale (Double)");
			FindBestIterationCountForNormalLawIntegral(karZ, karExpected, Math.Abs(dblTargetDelta));

			var karExpectedDecimal = new decimal[] {
				0.8413447460685429485852325456320379224779129667266043909873944502429914419872048295008849184056393275m,
				0.8906514475743080619676160383928930592976734158609286700391849984625801751987538510935072839844455582m,
				0.9750021048517795658634157309591628099775002209381166089142828958711815739963335013205260350450632762m,
				0.9772498680518207927997173628334665625282237762983215660163339998695237096472242516517308479242103851m,
				0.9986501019683699054733481852324050226221706318416193506357780146441942792354278997319614187139957829m
			};
			Console.WriteLine();
			Console.WriteLine(blnUnicode ?
			 "∫[0; 1][𝒩(0; 1)]\t≈ {0} Δ = {1:e} Série de MacLaurin (n=16 o=30 z=1 seulement)" :
			 "S[0; 1][N(0; 1)]\t~= {0} delta= {1:e} Série de MacLaurin (n=16 o=30 z=1 seulement)",
			 DecimalOneOfRootOfTwoPi * IntegralInDecimal(), DecimalTargetDelta());

			var dcmFinalTargetDelta = FinalTargetDelta();
			Program.OutputHeading("Détermination du nombre d'itérations idéales pour estimer l'intégrale (Decimal)");
			FindBestIterationCountForNormalLawIntegral(karZ, karExpectedDecimal, dcmFinalTargetDelta);

			Program.OutputHeading("Détermination du point de cassure de la série de MacLaurin");
			FindMacLaurinBreakpointForNormalLawIntegral(dcmFinalTargetDelta);

			Program.OutputHeading("Estimation et détermination du nombre d'itérations idéales pour probit");
			var karExpectedProbits = new decimal[] {
				1.64485362695147271486384890799163213608319574427532207176967209440410635m,
				1.69539771027213631465960937404132749234638104498567357232875989576081084m,
				1.75068607125216997943487110308096830985265504343499870163109660001745452m,
				1.81191067295259771489800018414045073060417685389309113599549222501024417m,
				1.88079360815125093886829379770751370802215317028624831698183978454066066m,
				1.95996398454005423552459443052055152795555007786954839847695264636163527m,
				2.05374891063182305293735165774045344641624739190087656349114460961889002m,
				2.17009037758456052973251367322761986640786005854751326159732915449022886m,
				2.32634787404084110088560616334691172335181714153201306906564024789087663m,
				2.57582930354890076097857674860381411730601763427631737646048621886255121m
			};
			Console.WriteLine(
			 "α     P-value logit(P)           logit(P) * √(π/8)  ≈Φ^-1              Φ^-1               Δ               n");
			const int kBigProbitIter = 1500;
			var ptdarProbitIter = new PointD[karExpectedProbits.Length];
			double probit;
			for (i = 950, j = 0; i < 1000; i += 5, j++) {
				p = RoundOff.Error(i * 0.001);
				probit = 0;
				decimal delta = 1;
				short withNewDelta = 0;
				var repetitions = 0;
				for (var m = checked((short)(3 * i - 2740)); m <= kBigProbitIter && (Math.Abs(delta) > dcmFinalTargetDelta) && (repetitions < 45); m++) {
					var previousDelta = delta;
					probit = ProbabilityDistributions.InverseNormal(p, m);
					delta = (decimal)probit - karExpectedProbits[j];
					if (Math.Abs(delta) < Math.Abs(previousDelta)) {
						withNewDelta = m;
						repetitions = 1;
					} else if (delta == previousDelta) {
						repetitions++;
					}
#if DEBUG
					Console.Error.WriteLine("p={0} n={1} delta={2:e}", p, m, delta);
#endif
				}

				OutputProbitEstimate(p, probit, delta, withNewDelta);
				ptdarProbitIter[j] = new PointD(Math.Abs(p - 0.5), withNewDelta);
			}
			//*
			for (i = 950, j = 0; i < 1000; i += 5, j++) {
				p = RoundOff.Error(i * 0.001);
				probit = ProbabilityDistributions.InverseNormal(p, kBigProbitIter);
				OutputProbitEstimate(p, probit, (decimal)probit - karExpectedProbits[j], kBigProbitIter);
			}// */
			var rm = RegressionModel.Compute(ptdarProbitIter);
			Console.WriteLine("{0} quand x = |p - 0,5| r={1}", rm, rm.R);
			var ptdarProbitIter2 = new PointD[karExpectedProbits.Length];
			for (i = 950, j = 0; i < 1000; i += 5, j++) {
				p = RoundOff.Error(i * 0.001);
				var theN = ProbabilityDistributions.IterationsForInverse(p);
				probit = ProbabilityDistributions.InverseNormal(p, theN);
				OutputProbitEstimate(p, probit, (decimal)probit - karExpectedProbits[j], theN);
				ptdarProbitIter2[j] = new PointD(theN, ptdarProbitIter[j].Y);
			}// */
			rm = RegressionModel.Compute(ptdarProbitIter2);
			Console.WriteLine("{0} quand x est la 1re estimation d'itérations r={1}", rm, rm.R);

			probit = ProbabilityDistributions.InverseNormal(RoundOff.Error(0.995)) /
			 ProbabilityDistributions.InverseLogistic(RoundOff.Error(0.995));
			p = (Math.Log10(probit / SqrtEighthOfPi) / -2) / RoundOff.Error(0.495);
			Console.WriteLine("Valeur candidate pour la pente de la puissance du facteur logit -> probit: m={0}", p);
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
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
			if (x == 0) {
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
				return 0;
			}

			var odd = (2 * k) + 1;
#if DEBUG
			var t = ExtraMath.Minus1Pow(k) * Math.Pow(x, odd);
			var d = checked(odd * (1 << k) * ExtraMath.Factorial((byte)k));
			var r = t / d;
			Console.WriteLine("x={0} k={1} {2}/{3}={4}", x, k, t, d, r);
			return r;
#else
			return ExtraMath.Minus1Pow(k) * Math.Pow(x, odd) / (odd * (1 << k) * ExtraMath.Factorial((byte)k));
#endif
		}

		private static decimal ExponentialSuite(decimal x, int k)
		{
			if (x == 0) {
				return 0;
			}

			var odd = (2 * k) + 1;
			return (decimal)(ExtraMath.Minus1Pow(k) * Math.Pow((double)x, odd) /
			 (odd * (1 << k) * ExtraMath.Factorial((byte)k)));
		}

		private static void OutputNormalIntegral(double z, double expected, double integral, string algorithm,
		int n, int o)
		{
			var delta = integral - expected;
			// When the FP subtraction gives 0, it is not really zero here,
			// it is just so small it cannot be computed correctly on a FPU.
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
			if (delta == 0) {
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
				delta = 1e-18;
			}
			BeginOutputNormalIntegral(z, integral, delta, delta >= 0);
			EndOutputNormalIntegral(algorithm, n, o);
		}

		private static void OutputNormalIntegral(decimal z, decimal expected, decimal integral, string algorithm,
		int n, int o)
		{
			var delta = integral - expected;
			BeginOutputNormalIntegral(z, integral, delta, delta >= 0);
			EndOutputNormalIntegral(algorithm, n, o);
		}

		private static void BeginOutputNormalIntegral<T>(T z, T integral, T delta, bool nonNegativeDelta)
		{
			Console.Write(Program.UnicodeTerminal == SupportLevel.Complete ?
			 "∫[-∞; {0}][𝒩(0; 1)]\t≈ {1:f16}   Δ = {2}" : "S[-∞; {0}][N(0; 1)]\t~= {1:f16} delta={2}",
			 z, integral, nonNegativeDelta ? " " : "");
			Console.Write("{0:e7}   ", delta);
		}

		private static void EndOutputNormalIntegral(string algorithm, int n, int o)
		{
			Console.WriteLine("{0,-33} (n={1,4} o={2,4})", algorithm, n, o);
		}

		private static double NonCumulativeNormal(double z)
		{
			return ProbabilityDistributions.Normal(z, false);
		}

		private static decimal NonCumulativeNormal(decimal z)
		{
			return DecimalOneOfRootOfTwoPi * (decimal)Math.Exp((double)(-0.5m * z * z));
		}

		private static double MacLaurinPositiveNormalIntegral(double x, byte n)
		{
			var sum = x;
			for (byte k = 1; k <= n - 1; k++) {
				var odd = (2 * k) + 1;
#if DEBUG
				var t = ExtraMath.Minus1Pow(k) * Math.Pow(x, odd);
				var b = odd * (1 << k) * ExtraMath.Factorial(k);
				sum += t / b;
#else
				sum += ExtraMath.Minus1Pow(k) * Math.Pow(x, odd) / checked(odd * (1 << k) * ExtraMath.Factorial(k));
#endif
			}
			return DoubleOneOfRootOfTwoPi * sum;
		}

		private static decimal MacLaurinPositiveNormalIntegral(decimal x, byte n)
		{
			var sum = x;
			for (byte k = 1; k <= n - 1; k++) {
				var odd = (2 * k) + 1;
#if DEBUG
				var t = ExtraMath.Minus1Pow(k) * Math.Pow((double)x, odd);
				var b = odd * (1 << k) * ExtraMath.BigFactorial(k);
				sum += (decimal)t / b;
#else
				sum += (decimal)(ExtraMath.Minus1Pow(k) * Math.Pow((double)x, odd)) /
				 checked(odd * (1 << k) * ExtraMath.BigFactorial(k));
#endif
			}
			return DecimalOneOfRootOfTwoPi * sum;
		}

		private static bool MoreExact(double r, double s1, double s2, double expected, double target)
		{
			return MoreExact(s1, expected, target) || MoreExact(s2, expected, target) || MoreExact(r, expected, target);
		}

		private static bool MoreExact(double value, double expected, double target)
		{
			return Math.Abs(value - expected) < target;
		}

		private static bool MoreExact(decimal r, decimal s1, decimal s2, decimal expected, decimal target)
		{
			return MoreExact(s1, expected, target) || MoreExact(s2, expected, target) || MoreExact(r, expected, target);
		}

		private static bool MoreExact(decimal value, decimal expected, decimal target)
		{
			return Math.Abs(value - expected) < target;
		}

		private static void FindBestIterationCountForNormalLawIntegral(float[] allZ, double[] expected,
		double targetDelta)
		{
			// ReSharper disable once TooWideLocalVariableScope
			int i, n;
			var c = allZ.Length;
			double z;
			double stddev = 0;
			var intarIterations = new int[c];
			for (i = 0; i < c; i++) {
				z = Math.Round(allZ[i], 2);
				var blnFound = false;
				var ex = expected[i];
				for (n = 30; (!blnFound) && (n <= 32766); n += 6) {
					var r0 = 0.5 + Integral.Riemann(0, z, n, NonCumulativeNormal);
					var s1 = 0.5 + Integral.Simpson(0, z, n, NonCumulativeNormal);
					var s2 = 0.5 + Integral.SimpsonThreeEights(0, z, n, NonCumulativeNormal);
					if (MoreExact(r0, s1, s2, ex, targetDelta)) {
						blnFound = true;
						intarIterations[i] = n;
						OutputNormalIntegral(z, ex, r0, "Somme de Riemann", n, n);
						OutputNormalIntegral(z, ex, s1, "Méthode composite de Simpson", n, n + 1);
						OutputNormalIntegral(z, ex, s2, "Méthode 3/8e composite de Simpson", n, n + 1);
					}
				}
			}
			double average = 0;
			for (i = 0; i < c; i++) {
				average += intarIterations[i];
			}
			average /= c;
			for (i = 0; i < c; i++) {
				z = intarIterations[i] - average;
				stddev += z * z;
			}
			stddev /= c - 1;
			stddev = Math.Sqrt(stddev);
			var dblT99Percent = (double)StudentT99TwoSidedScores[c - 1];
			var ideal         = Math.Ceiling((average + (dblT99Percent * stddev)) / 6) * 6;
			Console.Write("x_={0} itérations  s={1}  n={2}  ", average, stddev, c);
			Console.WriteLine(Program.UnicodeTerminal != SupportLevel.None ?
			 "t₉₉({2})={0}  x^={1} itérations" : "t(99;{2})={0}  x^={1} itérations",
			 dblT99Percent, ideal, c - 1);
		}

		private static void FindBestIterationCountForNormalLawIntegral(float[] allZ, decimal[] expected,
		decimal targetDelta)
		{
			// ReSharper disable once TooWideLocalVariableScope
			int i, n;
			var c = allZ.Length;
			decimal z;
			var intarIterations = new int[c];
			for (i = 0; i < c; i++) {
				z = (decimal)Math.Round(allZ[i], 2);
				var blnFound = false;
				var ex = expected[i];
				for (n = 30; (!blnFound) && (n <= 32766); n += 6) {
					var r0 = 0.5m + Integral.Riemann(0, z, n, NonCumulativeNormal);
					var s1 = 0.5m + Integral.Simpson(0, z, n, NonCumulativeNormal);
					var s2 = 0.5m + Integral.SimpsonThreeEights(0, z, n, NonCumulativeNormal);
					if (MoreExact(r0, s1, s2, ex, targetDelta)) {
						blnFound = true;
						intarIterations[i] = n;
						OutputNormalIntegral(z, ex, r0, "Somme de Riemann", n, n);
						OutputNormalIntegral(z, ex, s1, "Méthode composite de Simpson", n, n + 1);
						OutputNormalIntegral(z, ex, s2, "Méthode 3/8e composite de Simpson", n, n + 1);
					}
				}
				blnFound = false;
				var overflowAt = 0;
				decimal ml;
				for (n = 16; (overflowAt < 1) && (n <= 23); n++) {
					try {
						ml = 0.5m + MacLaurinPositiveNormalIntegral(z, (byte)n);
						if (MoreExact(ml, ex, targetDelta)) {
							blnFound = true;
							OutputNormalIntegral(z, ex, ml, "Série de MacLaurin corrigée", n, n - 1);
						}
					} catch (OverflowException) {
						overflowAt = n;
					}
				}
				if ((!blnFound) && (overflowAt > 0)) {
					ml = 0.5m + MacLaurinPositiveNormalIntegral(z, (byte)(overflowAt - 1));
					OutputNormalIntegral(z, ex, ml, "Série de MacLaurin corrigée°", overflowAt - 1,
					 overflowAt - 2);
				}
			}
			decimal average = 0;
			for (i = 0; i < c; i++) {
				average += intarIterations[i];
			}
			average /= c;
			decimal stddev = 0;
			for (i = 0; i < c; i++) {
				z = intarIterations[i] - average;
				stddev += z * z;
			}
			stddev /= c - 1;
			stddev = ExtraMath.Sqrt(stddev);
			var dcmT99Percent = StudentT99TwoSidedScores[c - 1];
			var ideal         = Math.Ceiling((average + (dcmT99Percent * stddev)) / 6) * 6;
			Console.Write("x_={0} itérations  s={1}  n={2}  ", average, stddev, c);
			Console.WriteLine(Program.UnicodeTerminal  != SupportLevel.None ?
			 "t₉₉({2})={0}  x^={1} itérations" : "t(99;{2})={0}  x^={1} itérations",
			 dcmT99Percent, ideal, c - 1);

			var ptmarIter = new PointM[c];
			for (i = 0; i < c; i++) {
				ptmarIter[i] = new PointM((decimal)Math.Round(allZ[i], 2), intarIterations[i]);
			}
			var rm = RegressionModel.Compute(ptmarIter);
			Console.WriteLine("{0}\t r={1}", rm, rm.R);
		}

		private static void FindMacLaurinBreakpointForNormalLawIntegral(decimal targetDelta)
		{
			var blnBroken = false;
			for (var i = 200; (!blnBroken) && (i <= 300); i++) {
				var z = i * 0.01m;
				var n = ProbabilityDistributions.SimpsonIterations((double)z);
				var simpson = Integral.Simpson(0, z, n, NonCumulativeNormal);
				decimal bestDelta = 1;
				byte bestK = 0;
				decimal ml, delta;
				for (byte k = 16; k <= 22; k++) {
					ml = MacLaurinPositiveNormalIntegral(z, k);
					delta = ml - simpson;

					if (Math.Abs(delta) < Math.Abs(bestDelta)) {
						bestDelta = delta;
						bestK = k;
					}
				}
				ml = MacLaurinPositiveNormalIntegral(z, bestK);
				delta = ml - simpson;
				Console.Write(Program.UnicodeTerminal == SupportLevel.Complete ?
				 "∫[0; {0:f2}][𝒩(0; 1)]\t≈ {1:f16}   Δ = {2}" : "S[0; {0:f2}][N(0; 1)]\t~= {1:f16} delta={2}",
				 z, ml, delta >= 0 ? " " : "");
				Console.WriteLine("{0:e7} (s={1} m={2})", delta, n, bestK);

				if (Math.Abs(delta) > Math.Abs(targetDelta) * 10) {
					blnBroken = true;
					Console.WriteLine("Cassure lorsque Z>{0}", (i - 1) * 0.01m);
				}
			}
		}

		private static void OutputProbitEstimate(double p, double probit, decimal delta, short iterations)
		{
			var logit = ProbabilityDistributions.InverseLogistic(p);
			Console.Write("{0,5:f2} {1,7:f3} {2:f16} ", RoundOff.Error(2 * (1 - p)), p, logit);
			Console.Write("{0:f16} {1:f16} {2:f16} ", logit * SqrtEighthOfPi,
			 ProbabilityDistributions.InverseNormalEstimate(p), probit);
			Console.WriteLine("{0}{1:e7} {2,4}", delta >= 0 ? " " : "", delta, iterations);
		}

		private static void LogisticModel(params PointM[] points)
		{
			var iLast = points.Length - 1;
			var last = points[iLast].Y;
			var intercept = Math.Min(last, points[0].Y);
			var amplitude = Math.Abs(last - points[0].Y);
			// Find the point which is the observed middle
			var yMid = 0.5m * (last + points[0].Y);
			var iMid = points.Length / 2;
			var ixyMid = -1;
			int k;
			last = Decimal.MaxValue;
			for (k = iMid - 1; k <= iMid + 1; k++) {
				var diff = Math.Abs(points[k].Y - yMid);
				if (diff < last) {
					last = diff;
					ixyMid = k;
				}
			}

			var lrrSecant = LinearRegression.Compute(points[ixyMid - 1], points[ixyMid], points[ixyMid + 1]);
			var location = lrrSecant.InterpolateX(yMid);
			last = LinearRegression.Compute(points).Slope;
			// The scale parameter is the hardest to adjust
			var scale = Math.Sign(last) * Math.Max(lrrSecant.Slope, last) / Math.Min(lrrSecant.Slope, last);

			var ptmarRoughModel = new PointM[points.Length];
			EvaluateLogisticModel(points, intercept, amplitude, location, scale, ptmarRoughModel);
			lrrSecant = LinearRegression.Compute(ptmarRoughModel[0], ptmarRoughModel[iLast]);
			Console.WriteLine("{0}\tr={1}", lrrSecant, RoundOff.Error(lrrSecant.Correlation));

			intercept = lrrSecant.Intercept + lrrSecant.Slope * intercept;
			amplitude *= lrrSecant.Slope;
			EvaluateLogisticModel(points, intercept, amplitude, location, scale, ptmarRoughModel);
		}

		private static void EvaluateLogisticModel(PointM[] points,
		decimal intercept, decimal amplitude, decimal location, decimal scale, PointM[] ptmarRoughModel)
		{
			for (var k = 0; k < points.Length; k++) {
				var exponent = (points[k].X - location) / -scale;
				var unscaled = 1 / (1 + (decimal)Math.Exp((double)exponent));
				ptmarRoughModel[k] = new PointM(intercept + (amplitude * unscaled), points[k].Y);
				Console.WriteLine("{0}\t{1}\t{2:f1}", points[k].X, points[k].Y, ptmarRoughModel[k].X);
			}
			Console.Write("y = {0} + {1}*[1/(1+e^((x-{2})/-", intercept, amplitude, location);
			Console.WriteLine("{0}))]", scale);
		}
	}
}
