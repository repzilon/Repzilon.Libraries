//
//  Differential.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
#if !NET20
using System.Linq;
#endif

namespace Repzilon.Libraries.Core
{
	public static class Differential
	{
		/// <summary>
		/// Finds an intersection between two functions using the Newton algorithm
		/// </summary>
		/// <typeparam name="T">Numeric data type for functions</typeparam>
		/// <param name="candidate">First estimate of the intersection</param>
		/// <param name="targetDelta">Very small value that will be used as the stop value</param>
		/// <param name="functionF">f(x). If you have an experimental and a theorical function, use this as the experimental function</param>
		/// <param name="functionG">g(x). If you have an experimental and a theorical function, use this as the theorical function</param>
		/// <param name="derivativeF">f'(x), i.e. the first derivative of f(x)</param>
		/// <param name="derivativeG">g'(x), i.e. the first derivative of g(x)</param>
		/// <returns>x value at with g(x) equals f(x)</returns>
		public static T NewtonCrossing<T>(T candidate, T targetDelta,
#if NET20
		Converter<T, T> functionF, Converter<T, T> functionG, Converter<T, T> derivativeF, Converter<T, T> derivativeG)
#else
		Func<T, T> functionF, Func<T, T> functionG, Func<T, T> derivativeF, Func<T, T> derivativeG)
#endif
		where T : struct, IEquatable<T>, IFormattable, IComparable<T>
		{
#if !NET20
			var sub = Arithmetic<T>.Sub;
#endif
			T fx;
			int n = 1;

			do {
#if NET20
				fx = Arithmetic<T>.SubtractScalars(functionF(candidate), functionG(candidate));
				T dx = Arithmetic<T>.SubtractScalars(derivativeF(candidate), derivativeG(candidate));
				candidate = Arithmetic<T>.SubtractScalars(candidate, Arithmetic<T>.DivideScalars(fx, dx));
#else
				fx = sub(functionF(candidate), functionG(candidate));
				T dx = sub(derivativeF(candidate), derivativeG(candidate));
				candidate = sub(candidate, Arithmetic<T>.DivideScalars(fx, dx));
#endif
				n++;
			} while ((n <= 100) && GreaterThan(Abs(fx), targetDelta));
			return candidate;
		}

		/// <summary>
		/// Solves f(x) = k for x using the Newton algorithm, useful when f(x) is not an algebric function.
		/// </summary>
		/// <param name="candidate">First estimate of the value of x</param>
		/// <param name="targetDelta">Very small value that will be used as the stop value</param>
		/// <param name="functionF">The function f(x) as a delegate</param>
		/// <param name="constant">The constant k</param>
		/// <param name="derivativeF">f'(x) i.e. the first derivative of f(x)</param>
		/// <typeparam name="T">Numeric data type for functions</typeparam>
		/// <returns>The value of x satisfying f(x) = k, plus or minus targetDelta at most</returns>
		public static T NewtonCrossing<T>(T candidate, T targetDelta,
#if NET20
		Converter<T, T> functionF, T constant, Converter<T, T> derivativeF)
#else
		Func<T, T> functionF, T constant, Func<T, T> derivativeF)
#endif
		where T : struct, IEquatable<T>, IFormattable, IComparable<T>
		{
#if !NET20
			var sub = Arithmetic<T>.Sub;
#endif
			T fx;
			int n = 1;

			do {
				T dx = derivativeF(candidate);
#if NET20
				fx = Arithmetic<T>.SubtractScalars(functionF(candidate), constant);
				candidate = Arithmetic<T>.SubtractScalars(candidate, Arithmetic<T>.DivideScalars(fx, dx));
#else
				fx = sub(functionF(candidate), constant);
				candidate = sub(candidate, Arithmetic<T>.DivideScalars(fx, dx));
#endif
				n++;
			} while ((n <= 100) && GreaterThan(Abs(fx), targetDelta));
			return candidate;
		}

		private static T Abs<T>(T value) where T : struct
		{
			if (value is double) {
				return ExtraMath.ConvertTo<T>(Math.Abs(Convert.ToDouble(value)));
			} else if (value is decimal) {
				return ExtraMath.ConvertTo<T>(Math.Abs(Convert.ToDecimal(value)));
			} else if (value is float) {
				return ExtraMath.ConvertTo<T>(Math.Abs(Convert.ToSingle(value)));
			} else {
				return ExtraMath.ConvertTo<T>(Math.Abs(Convert.ToInt64(value)));
			}
		}

		private static bool GreaterThan<T>(T value, T than) where T : struct, IComparable<T>
		{
			return value.CompareTo(than) > 0;
		}
	}
}
