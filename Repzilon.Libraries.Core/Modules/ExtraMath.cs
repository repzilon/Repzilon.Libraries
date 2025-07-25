//
//  ExtraMath.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2025 René Rhéaume
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
	public enum StirlingMode : byte
	{
		Raw,
		Rounded,
		Corrected
	}

	public static partial class ExtraMath
	{
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
				throw new ArgumentOutOfRangeException(nameof(a), a, "a = 0 would cause a division by zero.");
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
		public static Exp18 Abs(Exp18 number)
		{
			return new Exp18(Math.Abs(number.Mantissa), number.Base, number.Exponent);
		}

		[CLSCompliant(false)]
		public static double Pow(byte radix, sbyte exponent)
		{
			var ae = Math.Abs(exponent);
			long r = 1;
			for (var i = 1; i <= ae; i++) {
				r *= radix;
			}
			return (exponent > 0) ? r : 1.0 / r;
		}

		private static decimal Pow(decimal radix, byte exponent)
		{
			decimal power = 1;
			for (byte i = 1; i <= exponent; i++) {
				power *= radix;
			}
			return power;
		}

		/// <summary>
		/// Computes iteratively the factorial of a natural number.
		/// </summary>
		/// <param name="n">A natural number</param>
		/// <returns>The factorial</returns>
		/// <remarks>
		/// Anyone using recursion for this function must stay away from programming forever.
		/// You are not showing any elite skills, you are just putting trash into the CPU stack
		/// and making the source code unreadable. This is not to say recursion is useless, it
		/// is just not because you are able to use something you should use it everywhere.
		/// </remarks>
		public static long Factorial(byte n)
		{
#if !DEBUG
			if (n > 20) {
				throw new ArgumentOutOfRangeException(nameof(n), n, "The factorial of 21 overflows a 64-bit integer.");
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
				throw new ArgumentOutOfRangeException(nameof(n), n, "The factorial of 28 overflows a decimal.");
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

		public static decimal StirlingApproximateFactorial(byte n, StirlingMode mode)
		{
			if (n > 27) {
				throw new ArgumentOutOfRangeException(nameof(n), n, "The factorial of 28 overflows a decimal.");
			}
			var value = Sqrt(Tau * n) * Pow(n / E, n);
			if (mode >= StirlingMode.Rounded) {
				value = n > 1 ? RoundToMultiple(value, 2) : n;
			}
			if ((n > 4) && (mode >= StirlingMode.Corrected)) {
				// The regression was built with rounding to unit applied instead of multiple of 2.
				value = RoundToMultiple(value * 1.02640407335314m * (decimal)Math.Pow(n, -0.0073060504686907103980413362), 2);
			}
			return value;
		}

		public static double StirlingApproximateFactorial(double n, StirlingMode mode)
		{
			var value = Math.Sqrt(RetroCompat.Tau * n) * Math.Pow(n / Math.E, n);
			// A coarse comparison is what we were looking for
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			var blnNisInteger = Math.Round(n) == n;
			if (blnNisInteger && (mode >= StirlingMode.Rounded)) {
				value = n > 1 ? RoundToMultiple(value, 2) : n;
			}
			if ((!blnNisInteger || (n > 4)) && (mode >= StirlingMode.Corrected)) {
				// The regression was built with rounding to unit applied instead of multiple of 2.
				value = value * 1.02640407335314 * Math.Pow(n, -0.0073060504686907103980413362);
				if (blnNisInteger) {
					value = RoundToMultiple(value, 2);
				}
			}
			return value;
		}

		public static double RoundToMultiple(double value, int multiple)
		{
			return Math.Round(value / multiple, MidpointRounding.ToEven) * multiple;
		}

		public static decimal RoundToMultiple(decimal value, int multiple)
		{
			return Math.Round(value / multiple, MidpointRounding.ToEven) * multiple;
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
			var typOut = typeof(TOut);
			if (typOut == typeof(Exp)) {
				return (TOut)(object)(new Exp(Convert.ToDouble(value)));
			} else if (typOut == typeof(Exp18)) {
				return (TOut)(object)(new Exp18(Convert.ToDouble(value)));
			} else {
				return (TOut)Convert.ChangeType(value, typOut);
			}
		}
	}
}
