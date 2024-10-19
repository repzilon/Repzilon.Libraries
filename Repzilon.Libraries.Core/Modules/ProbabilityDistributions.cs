//
//  ProbabilityDistributions.cs
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

namespace Repzilon.Libraries.Core
{
	public static class ProbabilityDistributions
	{
		#region Normal distribution
		private const byte MacLaurinIterations = 22; // 16 for Int64+Double, 22 for Decimal (higher accuracy)
		private const float MacLaurinBreakpoint = 2.07f;
		private static readonly decimal DecimalOneOfRootOfTwoPi = 1.0m / ExtraMath.Sqrt(2 * ExtraMath.Pi);
		private static readonly double DoubleOneOfRootOfTwoPi = 1.0 / Math.Sqrt(2 * Math.PI);

		public static double Normal(double x, double mean, double standardDeviation, bool cumulative)
		{
			if (standardDeviation <= 0) {
				throw new ArgumentOutOfRangeException("standardDeviation", standardDeviation,
				 "A standard deviation cannot be neither zero nor a negative number.");
			}
			if (cumulative) {
				// TODO: Implement Cumulative mode for normal distribution
				throw new NotImplementedException("Cumulative mode for normal distribution is not yet implemented " +
				 "(Integral calculus is not part of base libraries of any general purpose programming language).");
			} else {
				var z = (x - mean) / standardDeviation;
				return DoubleOneOfRootOfTwoPi / standardDeviation * Math.Exp(-0.5 * z * z);
			}
		}

		public static double Normal(double z, bool cumulative)
		{
			if (!cumulative) {
				return NonCumulativeNormal(z);
			} else if (z == 0) {
				return 0.5;
			} else if (Double.IsNegativeInfinity(z)) {
				return 0;
			} else if (Double.IsPositiveInfinity(z)) {
				return 1;
			} else if (z < -1 * MacLaurinBreakpoint) {
				return 0.5 - SimpsonForNormal(-1 * z);
			} else if (z < 0) {
				return (double)(0.5m - MacLaurinPositiveNormalIntegral(-1 * (decimal)z));
			} else if (z > MacLaurinBreakpoint) {
				return 0.5 + SimpsonForNormal(z);
			} else {
				return (double)(0.5m + MacLaurinPositiveNormalIntegral((decimal)z));
			}
		}

		public static int Iterations(double z)
		{
			var abs = Math.Abs(z);
			return abs > MacLaurinBreakpoint ? SimpsonIterations(abs) : MacLaurinIterations;
		}

		public static int SimpsonIterations(double z)
		{
			// y = 1887.12382957702 * 1.3860204248157^x	 r=0.9971790746821102570364573442
			var n = 1887.12382957702 * Math.Pow(1.3860204248157, z);
			const double kOneSixth = 1.0 / 6;
			return (int)Math.Ceiling(n * kOneSixth) * 6;
		}

		private static double SimpsonForNormal(double b)
		{
			const double kOneThird = 1.0 / 3;
			var n = SimpsonIterations(b);
			var h = b / n;
			var sum = DoubleOneOfRootOfTwoPi + NonCumulativeNormal(b); // OneOfRootOfTwoPi == NonCumulativeNormal(0) && a == 0
			for (int i = 1; i < n; i++) {
				sum += NonCumulativeNormal(i * h) * ((i % 2 == 1) ? 4 : 2);
			}
			return kOneThird * h * sum;
		}

		private static double NonCumulativeNormal(double z)
		{
			return DoubleOneOfRootOfTwoPi * Math.Exp(-0.5 * z * z);
		}

		private static decimal MacLaurinPositiveNormalIntegral(decimal x)
		{
			var sum = x;
			for (byte k = 1; k <= MacLaurinIterations - 1; k++) {
				var odd = (2 * k) + 1;
#if DEBUG
				var t = ExtraMath.Minus1Pow(k) * Math.Pow((double)x, odd);
				var b = odd * (1 << k) * ExtraMath.BigFactorial(k);
				sum += (decimal)t / b;
#else
				sum += (decimal)(ExtraMath.Minus1Pow(k) * Math.Pow((double)x, odd)) / checked(odd * (1 << k) * ExtraMath.BigFactorial(k));
#endif
			}
			return DecimalOneOfRootOfTwoPi * sum;
		}
		#endregion

		#region Student distribution
		public static double Student(double x, byte liberties, bool cumulative)
		{
			if (cumulative) {
				if (x == 0) {
					return 0.5;
				} else if (Double.IsNegativeInfinity(x)) {
					return 0;
				} else if (Double.IsPositiveInfinity(x)) {
					return 1;
				} else if (x < 0) {
					return 0.5 - SimpsonForStudent(-1 * x, liberties);
				} else {
					return 0.5 + SimpsonForStudent(x, liberties);
				}
			} else {
#if DEBUG
				var lastPower = Math.Pow(1 + (x * x / liberties), -0.5 * (liberties + 1));
				return CachedGammaRatio(liberties) * lastPower;
#else
				return CachedGammaRatio(liberties) * Math.Pow(1 + (x * x / liberties), -0.5 * (liberties + 1));
#endif
			}
		}

		private static double SimpsonForStudent(double b, byte k)
		{
			const double kOneThird = 1.0 / 3;
			var n = SimpsonIterations(b);
			var h = b / n;
			var sum = Student(0, k, false) + Student(b, k, false);
			for (int i = 1; i < n; i++) {
				sum += Student(i * h, k, false) * ((i % 2 == 1) ? 4 : 2);
			}
			return kOneThird * h * sum;
		}

		/// <summary>
		/// Even with a six times faster algorithm, the gamma ratio is still the slowest part of
		/// the Student probability distribution function. Cache the output. It will be very useful
		/// for the computation of its integral with the Simpson rule, which will call the function
		/// thousands of times for a single numeric integration. This array only takes 2 kilobytes.
		/// </summary>
		private static readonly double[] StudentGammaRatioCache = new double[255];

		private static double CachedGammaRatio(byte k)
		{
			var ratio = StudentGammaRatioCache[k - 1];
			if (ratio == 0) {
				ratio = FastGammaRatio(k);
				StudentGammaRatioCache[k - 1] = ratio;
			}
			return ratio;
		}

		/// <summary>
		/// Computes the part of the Student dealing with the ratio of Gamma functions
		/// [1/sqrt(k*pi) * GAMMA(0.5*/(k+1)) / GAMMA(0.5k)] by simplifying the numerators and denominators
		/// of the developments of gamma for positive integers and halves AND leaving
		/// irrational numbers outside the developments
		/// </summary>
		/// <remarks>https://en.wikipedia.org/wiki/Student%27s_t-distribution#Probability_density_function</remarks>
		private static double FastGammaRatio(byte k)
		{
			List<int> numerators = new List<int>(k);
			List<int> denominators = new List<int>(k);

			int c = k % 2; // c means "oddity of k" here
			var multiplier = 1.0 / Math.Sqrt(k) * (c == 0 ? 0.5 : 1 / Math.PI);
			AddGammaFactors(numerators, k - 1, 3 - c);
			AddGammaFactors(denominators, k - 2, 2 + c);

			// Simplify fraction
			if (k >= 35) {
				RemoveDividableFactors(2, numerators, denominators);
				RemoveDividableFactors(3, numerators, denominators);
				RemoveDividableFactors(5, numerators, denominators);
				RemoveDividableFactors(7, numerators, denominators);
			}
			if (k >= 36) {
				RemoveDividableFactors(3, numerators, denominators);
			}
			if (k >= 45) {
				byte q = (byte)(k / 4);
				for (byte p = 11; p <= q; p += 2) {
					RemoveDividableFactors(p, numerators, denominators);
				}
			}

			if (k >= 66) {
				Regroup(numerators);
				Regroup(denominators);
				c = denominators.Count; // meaning changed for variable c
				if (c == numerators.Count) {
					return MultiplyByFractions(numerators, denominators, multiplier, c);
				} else if (numerators.Count == c + 1) {
					return MultiplyByFractions(numerators, denominators, multiplier, c) * numerators[c];
				} else {
					return MultiplyByFractions(numerators, denominators, multiplier, c - 1) / denominators[c - 1];
				}
			} else {
				return multiplier * Product(numerators) / Product(denominators);
			}
		}

		private static void RemoveDividableFactors(byte by, List<int> numerators, List<int> denominators)
		{
			SplitDividableBy(by, numerators);
			SplitDividableBy(by, denominators);
			RemoveIdenticalFactors(numerators, denominators, 0);
		}

		private static void SplitDividableBy(byte by, List<int> numbers)
		{
			int c = numbers.Count;
			for (int i = 0; i < c; i++) {
				var v = numbers[i];
				if ((v > by) && (v % by == 0)) {
					numbers[i] = v / by;
					numbers.Add(by);
				}
			}
		}

		private static void RemoveIdenticalFactors<T>(List<T> numerators, List<T> denominators, int startAt)
		{
			int c = denominators.Count;
			int i = startAt;
			while (i < c) {
				int posInNumerator = numerators.IndexOf(denominators[i]);
				if (posInNumerator >= 0) {
					numerators.RemoveAt(posInNumerator);
					denominators.RemoveAt(i);
					c--;
				} else {
					i++;
				}
			}
		}

		private static ulong Product(List<int> numbers)
		{
			ulong n = 1;
			var c = numbers.Count;
			for (var i = 0; i < c; i++) {
#if DEBUG
				checked {
#endif
					n *= (uint)numbers[i];
#if DEBUG
				}
#endif
			}
			return n;
		}

		private static void AddGammaFactors(List<int> destination, int max, int min)
		{
			for (int k = max; k >= min; k -= 2) {
				destination.Add(k);
			}
		}

		private static void Regroup(List<int> factors)
		{
			int i = 0;
			int c = factors.Count - 1;
			while (i < c) {
				int v;
				if (TryMultiply(factors[i], factors[i + 1], out v)) {
					factors.RemoveAt(i + 1);
					factors[i] = v;
					c--;
				} else {
					i++;
				}
			}
		}

		/// <summary>
		/// Multiplies two POSITIVE integers and checks for possible overflow without checked arithmetic and
		/// especially without raising exceptions, which are a significant performance hog.
		/// </summary>
		/// <param name="x">Positive integer.
		/// When you repeatedly call this method, put the accumulated product here.</param>
		/// <param name="y">Positive integer between 1 and 255.
		/// It could have been of type Byte, but it hurts performance to have it as Byte.</param>
		/// <param name="v">Outputs an unchecked product which should only be used when this method returns True.</param>
		/// <returns>False when an overflow is detected</returns>
		private static bool TryMultiply(int x, int y, out int v)
		{
			v = x * y;
			return v >= 0 && x <= 8421501 && (v > x) && (v > y); // The constant is Int32.MaxValue / 255
		}

		private static double MultiplyByFractions(List<int> numerators, List<int> denominators, double multiplier, int commonCount)
		{
			for (int i = 0; i < commonCount; i++) {
				multiplier *= 1.0 * numerators[i] / denominators[i];
			}
			return multiplier;
		}
		#endregion

		#region Logistic distribution
		private static readonly double LogisticQ = Math.Sqrt(3) / Math.PI;

		/// <summary>
		/// Logistic distribution function
		/// </summary>
		/// <param name="x">Value on the X axis</param>
		/// <param name="mean">Mean of the distribution</param>
		/// <param name="scale">Scale factor of the distrbution</param>
		/// <param name="cumulative">If true, returns the evaluation of the logistic function.
		/// Otherwise, return its partial derivative</param>
		/// <returns>The y value or the cumulative value of a logistic distribution</returns>
		/// <remarks>It is easier to derivate than to integrate a function</remarks>
		public static double Logistic(double x, double mean, double scale, bool cumulative)
		{
			var expr = Math.Exp((mean - x) / scale); // µ-x is the simplification of -(x-µ)
			var exprp1 = 1 + expr;
			return cumulative ? 1.0 / exprp1 : expr / (scale * exprp1 * exprp1);
		}

		/// <summary>
		/// Standard logistic distribution function (that is of mean 0 and scale 1)
		/// </summary>
		public static double Logistic(double x, bool cumulative)
		{
			var expr = Math.Exp(-1 * x); // µ-x is the simplification of -(x-µ)
			var exprp1 = 1 + expr;
			return cumulative ? 1.0 / exprp1 : expr / (exprp1 * exprp1);
		}

		/// <summary>
		/// The logit function, inverse of the logistic function
		/// </summary>
		/// <param name="p">Probability that is looked for</param>
		/// <param name="mean">Mean of the distribution</param>
		/// <param name="scale">Scale factor of the distrbution</param>
		public static double InverseLogistic(double p, double mean, double scale)
		{
			return mean + scale * InverseLogistic(p); // Math.Log(x) is ln(x)
		}

		public static double InverseLogistic(double p)
		{
			return Math.Log(p / (1 - p));
		}

		public static double LogisticV(double x, double mean, double standardDeviation, bool cumulative)
		{
			return Logistic(x, mean, LogisticQ * standardDeviation, cumulative);
		}

		/// <summary>
		/// Logistic distribution function of mean 0 and variance 1, to mimic a standard normal distribution
		/// </summary>
		public static double LogisticV(double x, bool cumulative)
		{
			var q = LogisticQ;
			var expr = Math.Exp(-1 * x / q);
			var exprp1 = 1 + expr;
			return cumulative ? 1.0 / exprp1 : expr / (q * exprp1 * exprp1);
		}

		public static double InverseLogisticV(double p, double mean, double standardDeviation)
		{
			return mean + LogisticQ * standardDeviation * InverseLogistic(p);
		}

		public static double InverseLogisticV(double p)
		{
			return LogisticQ * InverseLogistic(p);
		}
		#endregion
	}
}
