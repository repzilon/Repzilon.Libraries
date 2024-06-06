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
				if (liberties > 91) {
					throw new ArgumentOutOfRangeException("liberties", liberties,
					 "The current implementation of Student distribution is unable to go higher than 91 degrees of liberty.");
				}
				var lastPower = Math.Pow(1 + (x * x / liberties), -0.5 * (liberties + 1));
				return GammaRatio(liberties) * lastPower;
			}
		}

		/// <summary>
		/// Computes the part of the Student dealing with the ratio of Gamma functions
		/// [1/sqrt(k*pi) * GAMMA(0.5*/(k+1)) / GAMMA(0.5k)] by simplifying the numerators and denominators
		/// of the of developments of gamma for positive integers and halves.
		/// </summary>
		/// <param name="k">Degrees of liberties</param>
		/// <remarks>
		/// Doing with the simplification of the fraction allows more degrees of liberty than dividing factorials
		/// </remarks>
		private static double GammaRatio(byte k)
		{
			var ciC = CultureInfo.InvariantCulture;
			List<string> numerators = new List<string>(new string[] { "1", "1" });
			List<string> denominators = new List<string>(new string[] { "√k", "√π" });

			if (k % 2 == 0) { // k is even
				AddGammaHalves(k, ciC, numerators, denominators);

				// GAMMA(0.5k) = (0.5k - 1)! That bang is the factorial
				AddGammaIntegers(k >> 1, ciC, denominators);
			} else { // k is odd
				AddGammaIntegers((k + 1) >> 1, ciC, numerators);

				AddGammaHalves(k, ciC, denominators, numerators); // yes, in reverse order
			}

			// Simplify fraction
			RemoveIdenticalFactors(numerators, denominators);

			if (k >= 29) {
				// Find even numbers and split them as 2 * half
				SplitEvenNumbers(ciC, numerators);
				SplitEvenNumbers(ciC, denominators);
				RemoveIdenticalFactors(numerators, denominators);
			}
			if (k >= 32) {
				SplitEvenNumbers(ciC, numerators);
				SplitEvenNumbers(ciC, denominators);
				RemoveIdenticalFactors(numerators, denominators);

				SplitDividableByThree(ciC, numerators);
				SplitDividableByThree(ciC, denominators);
				RemoveIdenticalFactors(numerators, denominators);
			}
			if (k >= 54) {
				SplitEvenNumbers(ciC, numerators);
				SplitEvenNumbers(ciC, denominators);
				RemoveIdenticalFactors(numerators, denominators);

				SplitDividableBy(5, ciC, numerators);
				SplitDividableBy(5, ciC, denominators);
				RemoveIdenticalFactors(numerators, denominators);

				SplitDividableBy(7, ciC, numerators);
				SplitDividableBy(7, ciC, denominators);
				RemoveIdenticalFactors(numerators, denominators);
			}

#if (DEBUG)
			if (k >= 64) {
				var nd = BigProduct(k, numerators);
				var dd = BigProduct(k, denominators);
				return Convert.ToDouble(nd / dd);
			} else {
				var n = Product(k, numerators);
				var d = Product(k, denominators);
				return n / d;
			}
#else
			if (k >= 64) {
				return Convert.ToDouble(BigProduct(k, numerators) / BigProduct(k, denominators));
			} else {
				return Product(k, numerators) / Product(k, denominators);
			}
#endif
		}

		private static void SplitEvenNumbers(CultureInfo invariant, List<string> numbers)
		{
			int c = numbers.Count;
			for (int i = 0; i < c; i++) {
				long v;
				if (Int64.TryParse(numbers[i], out v) && (v % 2 == 0)) { // even number
					numbers[i] = (v >> 1).ToString(invariant);
					numbers.Add("2");
				}
			}
		}

		private static void SplitDividableByThree(CultureInfo invariant, List<string> numbers)
		{
			int c = numbers.Count;
			for (int i = 0; i < c; i++) {
				int v;
				if (Int32.TryParse(numbers[i], out v) && (v % 3 == 0)) { // even number
					numbers[i] = (v / 3).ToString(invariant);
					numbers.Add("3");
				}
			}
		}

		private static void SplitDividableBy(byte by, CultureInfo invariant, List<string> numbers)
		{
			int c = numbers.Count;
			for (int i = 0; i < c; i++) {
				int v;
				if (Int32.TryParse(numbers[i], out v) && (v % by == 0)) { // even number
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
					nd *= ExtraMath.Sqrt(ExtraMath.PI);
				} else {
					nd *= Decimal.Parse(numbers[i], NumberStyles.Any);
				}
			}
			return nd;
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
			if (n >= 16) {
				denominators.Add(Math.Pow(2, 2 * n).ToString(invariant));
			} else {
				denominators.Add((1 << (2 * n)).ToString(invariant));
			}
			for (i = n; i >= 2; i--) { // n!
				denominators.Add(i.ToString(invariant));
			}
		}

		private static void AddGammaIntegers(int n, CultureInfo invariant, List<string> destination)
		{
			for (int i = n - 1; i >= 2; i--) {
				destination.Add(i.ToString(invariant));
			}
		}
	}
}
