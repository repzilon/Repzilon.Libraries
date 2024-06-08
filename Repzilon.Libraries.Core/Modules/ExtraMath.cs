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
		public const decimal PI = 3.141592653589793238462643383327950m;

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
		/// <remarks>Based on a average speech rate of 130 words per minute,
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
			return (e > 0) ? (double)r : 1.0 / r;
		}

#if !NET20
		public static T Summation<T>(int m, int n, Func<int, T> forEach)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			if (forEach == null) {
				throw new ArgumentNullException("forEach");
			}

			T sum = default(T);
			for (var k = m; k <= n; k++) {
#if DEBUG
				T value = forEach(k);
				sum = GenericArithmetic<T>.adder(sum, value);
#else
				sum = GenericArithmetic<T>.adder(sum, forEach(k));
#endif
			}
			return sum;
		}

		public static T DifferenceOfPrimitives<T>(T a, T b, Func<T, T> expression)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			return GenericArithmetic<T>.sub(expression(b), expression(a));
		}
#endif

		/// <summary>
		/// Computes iteratively the factorial of a natural number.
		/// </summary>
		/// <param name="n">A natural number</param>
		/// <returns>The factorial</returns>
		/// <remarks>
		/// Anyone using recursion for this function must stay away from programming forever.
		/// You are not showing any elite programming, you just put trash into the CPU stack,
		/// and make the source code unreadable. This is not to say recursion is useless, it
		/// is just because you are able to use it that you should use it everywhere.
		/// </remarks>
		public static long Factorial(byte n)
		{
			if (n > 20) {
				throw new ArgumentOutOfRangeException("n", n, "The factorial of 21 overflows a 64-bit integer.");
			}
			long bang = n;
			for (byte i = 3; i <= n; i++) {
				bang *= i;
			}
			return bang;
		}
	}
}
