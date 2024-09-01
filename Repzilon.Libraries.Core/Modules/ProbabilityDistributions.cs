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
using System.Globalization;

namespace Repzilon.Libraries.Core
{
	public static class ProbabilityDistributions
	{
		#region Normal distribution
		private const byte kMacLaurinIterations = 22; // 16 for Int64+Double, 22 for Decimal (higher accuracy)
		private const float kMacLaurinBreakpoint = 2.07f;
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
			} else if (z < -1 * kMacLaurinBreakpoint) {
				return 0.5 - SimpsonForNormal(-1 * z);
			} else if (z < 0) {
				return (double)(0.5m - MacLaurinPositiveNormalIntegral(-1 * (decimal)z));
			} else if (z > kMacLaurinBreakpoint) {
				return 0.5 + SimpsonForNormal(z);
			} else {
				return (double)(0.5m + MacLaurinPositiveNormalIntegral((decimal)z));
			}
		}

		public static int Iterations(double z)
		{
			var abs = Math.Abs(z);
			return abs > kMacLaurinBreakpoint ? SimpsonIterations(abs) : kMacLaurinIterations;
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
			for (byte k = 1; k <= kMacLaurinIterations - 1; k++) {
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
#if (DEBUG)
				var lastPower = Math.Pow(1 + (x * x / liberties), -0.5 * (liberties + 1));
				return FastGammaRatio(liberties) * lastPower;
#else
				return FastGammaRatio(liberties) * Math.Pow(1 + (x * x / liberties), -0.5 * (liberties + 1));
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

			var multiplier = 1.0 / Math.Sqrt(k);
			if (k % 2 == 0) { // k is even
				AddGammaFactors(numerators, k - 1, 3);
				AddGammaFactors(denominators, k - 2, 2);
				multiplier = 0.5 * multiplier;
			} else { // k is odd
				AddGammaFactors(numerators, k - 1, 2);
				AddGammaFactors(denominators, k - 2, 3);
				multiplier = (1 / Math.PI) * multiplier;
			}

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
				var dc = denominators.Count;
				if (dc == numerators.Count) {
					return MultiplyByFractions(numerators, denominators, multiplier, dc);
				} else if (numerators.Count == dc + 1) {
					return MultiplyByFractions(numerators, denominators, multiplier, dc) * numerators[dc];
				} else {
					return MultiplyByFractions(numerators, denominators, multiplier, dc - 1) / denominators[dc - 1];
				}
			} else {
				return (multiplier * Product(numerators)) / Product(denominators);
			}
		}

		private static void RemoveDividableFactors(byte by, List<int> numerators, List<int> denominators)
		{
			SplitDividableBy(by, numerators);
			SplitDividableBy(by, denominators);
			RemoveIdenticalFactors(numerators, denominators);
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

		private static void RemoveIdenticalFactors<T>(List<T> numerators, List<T> denominators)
		{
			int c = denominators.Count;
			int i = typeof(T) == typeof(string) ? 1 : 0;
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
				checked {
					n *= (uint)numbers[i];
				}
			}
			return n;
		}

		private static void AddGammaFactors(List<int> destination, int max, byte min)
		{
			for (int k = max; k >= min; k -= 2) {
				destination.Add(k);
			}
		}

		private static void Regroup(List<int> factors)
		{
			int i = 0;
			while (i < factors.Count - 1) {
				int v;
				if (TryMultiply(factors[i], factors[i + 1], out v)) {
					factors.RemoveAt(i + 1);
					factors[i] = v;
				} else {
					i++;
				}
			}
		}

		/// <summary>
		/// Multiplies two POSITIVE integers and checks for possible overflow
		/// without checked arithmetic and especially without raising exceptions,
		/// whaich are a significant performance hog.
		/// </summary>
		/// <param name="x">Positive integer. When you call multiple times this method, put the accumulated product here.</param>
		/// <param name="y">Positive integer between 1 and 255. It could have been of type Byte but it hurts perfomance to have it as Byte.</param>
		/// <param name="v">Outputs an unchecked product which should only be used when this method returns True.</param>
		/// <returns>False when an overflow is detected</returns>
		private static bool TryMultiply(int x, int y, out int v)
		{
			v = x * y;
			if ((v < 0) || (x > 8421501)) { // The constant here is Int32.MaxValue / 255
				return false;
			} else if ((v > x) && (v > y)) {
				return true;
			} else {
				return false;
			}
		}

		private static double MultiplyByFractions(List<int> numerators, List<int> denominators, double multiplier, int commonCount)
		{
			for (int i = 0; i < commonCount; i++) {
				multiplier *= 1.0 * numerators[i] / denominators[i];
			}
			return multiplier;
		}
		#endregion
	}
}
