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

		public static double OldStudent(double x, byte liberties, bool cumulative)
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
				return GammaRatio(liberties) * lastPower;
#else
				return GammaRatio(liberties) * Math.Pow(1 + (x * x / liberties), -0.5 * (liberties + 1));
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
		/// of the developments of gamma for positive integers and halves.
		/// </summary>
		/// <param name="k">Degrees of liberties</param>
		/// <remarks>
		/// Doing with the simplification of the fraction allows more degrees of liberty than dividing factorials
		/// </remarks>
		[Obsolete("FastGammaRatio is a faster equivalent implementation.")]
		private static double GammaRatio(byte k)
		{
			var ciC = CultureInfo.InvariantCulture;
			// TODO : In GammaRatio, replace strings with a token structure
			List<string> numerators = new List<string>(/*new string[] { "1", "1" }*/); // 1 is the multiplication neutral term
			List<string> denominators = new List<string>(new string[] { "√k", "√π" });

			if (k % 2 == 0) { // k is even
				AddGammaHalves(k, ciC, numerators, denominators);
				AddGammaIntegers(k >> 1, ciC, denominators);
			} else { // k is odd
				AddGammaIntegers((k + 1) >> 1, ciC, numerators);
				AddGammaHalves(k, ciC, denominators, numerators); // yes, in reverse order
			}

			// Simplify fraction
			RemoveIdenticalFactors(numerators, denominators);

			if (k >= 29) {
				RemoveDividableFactors(2, ciC, numerators, denominators);
			}
			if (k >= 37) {
				RemoveDividableFactors(2, ciC, numerators, denominators);
				RemoveDividableFactors(3, ciC, numerators, denominators);
			}
			if (k >= 54) {
				RemoveDividableFactors(2, ciC, numerators, denominators);
				RemoveDividableFactors(5, ciC, numerators, denominators);
				RemoveDividableFactors(7, ciC, numerators, denominators);
			}
			if (k >= 92) {
				RemoveDividableFactors(2, ciC, numerators, denominators);
				RemoveDividableFactors(3, ciC, numerators, denominators);
			}
			if (k >= 116) {
				RemoveDividableFactors(3, ciC, numerators, denominators);
			}
			if (k >= 130) {
				RemoveDividableFactors(2, ciC, numerators, denominators);
			}
			if (k >= 138) {
				RemoveDividableFactors(11, ciC, numerators, denominators);
			}
			if (k >= 186) {
				RemoveDividableFactors(13, ciC, numerators, denominators);
			}

#if (DEBUG)
			if (k >= 94) {
				var nk = HugeProduct(k, numerators);
				var dk = HugeProduct(k, denominators);
				if (Double.IsNaN(nk.Value) && Double.IsNaN(dk.Value)) {
					return Convert.ToDouble(nk.Key / dk.Key);
				} else if (Double.IsNaN(nk.Value) == Double.IsNaN(dk.Value)) {
					return nk.Value / dk.Value;
				} else if (Double.IsNaN(nk.Value)) {
					return Convert.ToDouble(nk.Key) / dk.Value;
				} else {
					return nk.Value / Convert.ToDouble(dk.Key);
				}
			} else if (k >= 64) {
				var nd = BigProduct(k, numerators);
				var dd = BigProduct(k, denominators);
				return Convert.ToDouble(nd / dd);
			} else {
				var n = Product(k, numerators);
				var d = Product(k, denominators);
				return n / d;
			}
#else
			if (k >= 94) {
				var nk = HugeProduct(k, numerators);
				var dk = HugeProduct(k, denominators);
				var numeratorNaN = Double.IsNaN(nk.Value);
				if (numeratorNaN && Double.IsNaN(dk.Value)) {
					return Convert.ToDouble(nk.Key / dk.Key);
				} else if (numeratorNaN == Double.IsNaN(dk.Value)) {
					return nk.Value / dk.Value;
				} else if (numeratorNaN) {
					return Convert.ToDouble(nk.Key) / dk.Value;
				} else {
					return nk.Value / Convert.ToDouble(dk.Key);
				}
			} else if (k >= 64) {
				return Convert.ToDouble(BigProduct(k, numerators) / BigProduct(k, denominators));
			} else {
				return Product(k, numerators) / Product(k, denominators);
			}
#endif
		}

		private static void RemoveDividableFactors(byte by, CultureInfo invariant, List<string> numerators, List<string> denominators)
		{
			SplitDividableBy(by, invariant, numerators);
			SplitDividableBy(by, invariant, denominators);
			RemoveIdenticalFactors(numerators, denominators);
		}

		private static void SplitDividableBy(byte by, CultureInfo invariant, List<string> numbers)
		{
			int c = numbers.Count;
			for (int i = 0; i < c; i++) {
				long v;
				if (Int64.TryParse(numbers[i], out v) && (v > by) && (v % by == 0)) {
					numbers[i] = (v / by).ToString(invariant);
					numbers.Add(by.ToString(invariant));
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

		private static double Product(byte k, List<string> numbers)
		{
			double nf = ProductOfIntegers(numbers);
			var c = numbers.Count;
			var invariant = CultureInfo.InvariantCulture;
			for (var i = 0; i < c; i++) {
				if (numbers[i] == "√k") {
					nf *= Math.Sqrt(k);
				} else if (numbers[i] == "√π") {
					nf *= Math.Sqrt(Math.PI);
				} else if (numbers[i].Contains("E")) {
					nf *= Double.Parse(numbers[i], invariant);
				}
			}
			return nf;
		}

		private static ulong ProductOfIntegers(List<string> numbers)
		{
			ulong n = 1;
			var c = numbers.Count;
			for (var i = 0; i < c; i++) {
				ulong v;
				if (UInt64.TryParse(numbers[i], out v)) {
					checked {
						n *= v;
					}
				}
			}
			return n;
		}

		private static decimal BigProduct(byte k, List<string> numbers)
		{
			decimal nd = 1;
			var c = numbers.Count;
			var invariant = CultureInfo.InvariantCulture;
			for (var i = 0; i < c; i++) {
				if (numbers[i] == "√k") {
					nd *= ExtraMath.Sqrt(k);
				} else if (numbers[i] == "√π") {
					nd *= ExtraMath.Sqrt(ExtraMath.Pi);
				} else {
					nd *= Decimal.Parse(numbers[i], NumberStyles.Any, invariant);
				}
			}
			return nd;
		}

		private static KeyValuePair<decimal, double> HugeProduct(byte k, List<string> numbers)
		{
			decimal nd = 1;
			double nf = Double.NaN;
			var c = numbers.Count;
			bool inDouble = false;
			var invariant = CultureInfo.InvariantCulture;

			for (var i = 0; i < c; i++) {
				if (numbers[i] == "√k") {
					if (inDouble) {
						nf *= Math.Sqrt(k);
					} else {
						nd *= ExtraMath.Sqrt(k);
					}
				} else if (numbers[i] == "√π") {
					if (inDouble) {
						nf *= Math.Sqrt(Math.PI);
					} else {
						nd *= ExtraMath.Sqrt(ExtraMath.Pi);
					}
				} else if (inDouble) {
					nf *= Double.Parse(numbers[i], invariant);
				} else {
					try {
						nd *= Decimal.Parse(numbers[i], NumberStyles.Any, invariant);
					} catch (OverflowException) {
						inDouble = true;
						nf = (double)nd * Double.Parse(numbers[i], invariant);
					}
				}
			}
			return new KeyValuePair<decimal, double>(nd, nf);
		}

		private static void AddGammaHalves(byte k, CultureInfo invariant, List<string> numerators, List<string> denominators)
		{
			// GAMMA(0.5(k+1)) = GAMMA(0.5k + 0.5) = GAMMA(n + 0.5) = sqrt(pi) * (2n)! / (2^2n * n!)
			int i;
			var n = k >> 1;
			numerators.Add("√π");
			for (i = n * 2; i >= 2; i--) { // (2n)!
				numerators.Add(i.ToString(invariant));
			}
			// 2^2n
			denominators.Add(n >= 16 ? Math.Pow(2, 2 * n).ToString(invariant) : (1 << (2 * n)).ToString(invariant));
			for (i = n; i >= 2; i--) { // n!
				denominators.Add(i.ToString(invariant));
			}
		}

		private static void AddGammaIntegers(int n, CultureInfo invariant, List<string> destination)
		{
			// GAMMA(0.5k) = (0.5k - 1)! That bang is the factorial
			for (int i = n - 1; i >= 2; i--) {
				destination.Add(i.ToString(invariant));
			}
		}
		#endregion

		#region Faster Student distribution
		/// <summary>
		/// Computes the part of the Student dealing with the ratio of Gamma functions
		/// [1/sqrt(k*pi) * GAMMA(0.5*/(k+1)) / GAMMA(0.5k)] by simplifying the numerators and denominators
		/// of the developments of gamma for positive integers and halves AND leaving
		/// irrational numbers outside the developments
		/// </summary>
		/// <returns></returns>
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

		private static void AddGammaFactors(List<int> destination, int max, byte min)
		{
			for (int k = max; k >= min; k -= 2) {
				destination.Add(k);
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
