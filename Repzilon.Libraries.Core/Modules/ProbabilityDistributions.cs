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
		private static readonly double OneOfRootOfTwoPi = 1.0 / Math.Sqrt(2 * Math.PI);

		public static double Normal(double x, double mean, double standardDeviation, bool cumulative)
		{
			if (standardDeviation <= 0) {
				throw new ArgumentOutOfRangeException("standardDeviation", standardDeviation,
				 "A standard deviation cannot be neither zero nor a negative number.");
			}
			if (cumulative) {
				throw new NotImplementedException("Cumulative mode for normal distribution is not yet implemented " +
				  "(Integral calculus is not part of base libraries of any general purpose programming language).");
			} else {
				var z = (x - mean) / standardDeviation;
				return OneOfRootOfTwoPi / standardDeviation * Math.Exp(-0.5 * z * z);
			}
		}

		public static double Normal(double z, bool cumulative)
		{
			if (cumulative) {
				throw new NotImplementedException("Cumulative mode for normal distribution is not yet implemented " +
				 "(Integral calculus is not part of base libraries of any general purpose programming language).");
			} else {
				return OneOfRootOfTwoPi * Math.Exp(-0.5 * z * z);
			}
		}

		public static double Student(double x, byte liberties, bool cumulative)
		{
			if (cumulative) {
				throw new NotImplementedException("Cumulative mode for Student distribution is not yet implemented " +
				  "(Integral calculus is not part of base libraries of any general purpose programming language).");
			} else {
#if (DEBUG)
				var lastPower = Math.Pow(1 + (x * x / liberties), -0.5 * (liberties + 1));
				return GammaRatio(liberties) * lastPower;
#else
				return GammaRatio(liberties) * Math.Pow(1 + (x * x / liberties), -0.5 * (liberties + 1)); ;
#endif
			}
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
		private static double GammaRatio(byte k)
		{
			var ciC = CultureInfo.InvariantCulture;
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

		private static void RemoveIdenticalFactors(List<string> numerators, List<string> denominators)
		{
			int c = denominators.Count;
			int i = 1;
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
			for (var i = 0; i < c; i++) {
				if (numbers[i] == "√k") {
					nf *= Math.Sqrt(k);
				} else if (numbers[i] == "√π") {
					nf *= Math.Sqrt(Math.PI);
				} else if (numbers[i].Contains("E")) {
					nf *= Double.Parse(numbers[i]);
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
			for (var i = 0; i < c; i++) {
				if (numbers[i] == "√k") {
					nd *= ExtraMath.Sqrt(k);
				} else if (numbers[i] == "√π") {
					nd *= ExtraMath.Sqrt(ExtraMath.Pi);
				} else {
					nd *= Decimal.Parse(numbers[i], NumberStyles.Any);
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
					nf *= Double.Parse(numbers[i]);
				} else {
					try {
						nd *= Decimal.Parse(numbers[i], NumberStyles.Any);
					} catch (OverflowException) {
						inDouble = true;
						nf = (double)nd * Double.Parse(numbers[i]);
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
	}
}
