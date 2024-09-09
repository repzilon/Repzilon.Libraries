//
//  ExtraMath.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2024 René Rhéaume
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
	public static class ExtraMath
	{
		public const decimal Pi = 3.141592653589793238462643383327950m;

		/// <summary>
		/// Solves a quadratic equation axx + bx + c = 0 .
		/// </summary>
		/// <param name="a">Coefficient for x squared</param>
		/// <param name="b">Coefficient for x</param>
		/// <param name="c">Final constant of the equation</param>
		/// <returns>The possible solutions, when they exist.</returns>
		/// <remarks>
		/// An overload with Double data type for arguments and return values
		/// will not be implemented. Additional precision is needed to handle
		/// very small numbers, in the 10^-20 range.
		/// </remarks>
		public static KeyValuePair<decimal, decimal>? SolveQuadratic(decimal a, decimal b, decimal c)
		{
			if (a == 0) {
				throw new ArgumentOutOfRangeException("a", a, "a = 0 would cause a division by zero.");
			}

			var determinant = (b * b) - (4 * a * c);
			if (determinant >= 0) {
				var sqrt = Sqrt(determinant);
				var halfA = 0.5m * a; // Avoid the SLOW division instruction on every CPU and FPU
				return new KeyValuePair<decimal, decimal>(
				 (sqrt - b) * halfA, // That's (-b + sqrt(d)) / 2a in fewer operations,
				 ((-1 * b) - sqrt) * halfA); // (-b - sqrt(d)) / 2a);
			} else {
				return null;
			}
		}

		// https://www.csharp-console-examples.com/general/math-sqrt-decimal-in-c/
		public static decimal Sqrt(decimal square)
		{
			if (square < 0) {
				throw new ArgumentOutOfRangeException("square", square, "Cannot extract the square root of a negative number.");
			}

			var root = square / 3;
			for (int i = 0; i < 32; i++) {
				root = (root + (square / root)) * 0.5m;
			}
			return root;
		}

		/// <summary>
		/// Computes a rough estimate to say some text.
		/// </summary>
		/// <param name="text">The text to say.</param>
		/// <returns>The time to say the text, or TimeSpan.Zero if the argument is empty text.</returns>
		/// <remarks>Based on an average speech rate of 130 words per minute,
		/// with each word being 6.61 characters (space included) long on average.</remarks>
		public static TimeSpan SpeechDuration(string text)
		{
#if NET35 || NET20
			return RetroCompat.IsNullOrWhiteSpace(text) ?
#else
			return String.IsNullOrWhiteSpace(text) ?
#endif
			 TimeSpan.Zero :
			 TimeSpan.FromMilliseconds(Math.Max(700, 400 + (text.Trim().Length * 70)));
		}

		public static double Hypoth(double a, double b)
		{
			return Math.Sqrt((a * a) + (b * b));
		}

		public static double Hypoth(double a, double b, double c)
		{
			return Math.Sqrt((a * a) + (b * b) + (c * c));
		}

		public static decimal Hypoth(decimal a, decimal b)
		{
			return Sqrt((a * a) + (b * b));
		}

		public static decimal Hypoth(decimal a, decimal b, decimal c)
		{
			return Sqrt((a * a) + (b * b) + (c * c));
		}

		[CLSCompliant(false)]
		public static Exp Abs(Exp number)
		{
			return new Exp(Math.Abs(number.Mantissa), number.Base, number.Exponent);
		}

		[CLSCompliant(false)]
		public static double Pow(byte b, sbyte e)
		{
			var ae = Math.Abs(e);
			long r = 1;
			for (var i = 1; i <= ae; i++) {
				r *= b;
			}
			return (e > 0) ? r : 1.0 / r;
		}

		/// <summary>
		/// Computes iteratively the factorial of a natural number.
		/// </summary>
		/// <param name="n">A natural number</param>
		/// <returns>The factorial</returns>
		/// <remarks>
		/// Anyone using recursion for this function must stay away from programming forever.
		/// You are not showing any elite skills, you are just puting trash into the CPU stack
		/// and making the source code unreadable. This is not to say recursion is useless, it
		/// is just not because you are able to use something you should use it everywhere.
		/// </remarks>
		public static long Factorial(byte n)
		{
#if !DEBUG
			if (n > 20) {
				throw new ArgumentOutOfRangeException("n", n, "The factorial of 21 overflows a 64-bit integer.");
			}
#endif
			if (n <= 2) {
				return n;
			} else {
				long bang = 6; // 3! is 6
				for (byte i = 4; i <= n; i++) {
#if DEBUG
					checked {
						bang *= i;
					}
#else
					bang *= i;
#endif
				}
				return bang;
			}
		}

		private static decimal BigFactorialCore(byte n)
		{
			if (n > 27) {
				throw new ArgumentOutOfRangeException("n", n, "The factorial of 28 overflows a decimal.");
			}
			if (n <= 2) {
				return n;
			} else {
				decimal bang = 6; // 3! is 6
				for (byte i = 4; i <= n; i++) {
					bang *= i;
				}
				return bang;
			}
		}

		public static decimal BigFactorial(byte n)
		{
			return n > 20 ? BigFactorialCore(n) : Factorial(n);
		}

		/// <summary>
		/// Faster alternative to Math.Pow(-1, k) when k is an integer
		/// </summary>
		/// <param name="k">Exponent to raise -1</param>
		/// <returns>1 or -1</returns>
		public static int Minus1Pow(int k)
		{
			return (k % 2 != 0) ? -1 : 1;
		}

#if NET20
		public static TOut ConvertTo<TOut>(ValueType value) where TOut : struct
#else
		public static TOut ConvertTo<TOut>(this ValueType value) where TOut : struct
#endif
		{
			return (TOut)Convert.ChangeType(value, typeof(TOut));
		}
	}
}
