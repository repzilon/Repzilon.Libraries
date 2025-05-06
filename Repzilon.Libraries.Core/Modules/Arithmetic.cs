//
//  Arithmetic.cs
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
#if !NET20
using System.Linq.Expressions;
#endif

namespace Repzilon.Libraries.Core
{
	public static class Arithmetic<T>
	where T : struct, IFormattable, IEquatable<T>
	{
#if !NET20
		internal static readonly Func<T, T, T> Adder = BuildAdder();
		internal static readonly Func<T, T, T> Sub = BuildSubtractor();
		internal static readonly Func<T, T, T> MulT = BuildMultiplier<T>();

		internal static Func<TScalar, T, T> BuildMultiplier<TScalar>()
		where TScalar : struct
		{
			// Declare the parameters
			var paramA = Expression.Parameter(typeof(TScalar), "a");
			var paramB = Expression.Parameter(typeof(T), "b");

			// Add the parameters together and compile it
			return Expression.Lambda<Func<TScalar, T, T>>(Expression.Multiply(paramA, paramB),
			 paramA, paramB).Compile();
		}

		private static Func<T, T, T> BuildAdder()
		{
			var type = typeof(T);
			// Declare the parameters
			var paramA = Expression.Parameter(type, "a");
			var paramB = Expression.Parameter(type, "b");

			// Add the parameters together and compile it
			return Expression.Lambda<Func<T, T, T>>(Expression.Add(paramA, paramB), paramA, paramB).Compile();
		}

		private static Func<T, T, T> BuildSubtractor()
		{
			var type = typeof(T);
			// Declare the parameters
			var paramA = Expression.Parameter(type, "a");
			var paramB = Expression.Parameter(type, "b");

			// Add the parameters together and compile it
			return Expression.Lambda<Func<T, T, T>>(Expression.Subtract(paramA, paramB), paramA, paramB).Compile();
		}
#endif

		public static T AddScalars(T a, T b)
		{
#if NET20
			return ExtraMath.ConvertTo<T>(Convert.ToDouble(a) + Convert.ToDouble(b));
#else
			return Adder(a, b);
#endif
		}

		public static T SubtractScalars(T a, T b)
		{
#if NET20
			return ExtraMath.ConvertTo<T>(Convert.ToDouble(a) - Convert.ToDouble(b));
#else
			return Sub(a, b);
#endif
		}

		public static T MultiplyScalars(T a, T b)
		{
#if NET20
			return ExtraMath.ConvertTo<T>(Convert.ToDouble(a) * Convert.ToDouble(b));
#else
			return MulT(a, b);
#endif
		}

		public static T MultiplyScalars(T a, T b, T c)
		{
#if NET20
			return ExtraMath.ConvertTo<T>(Convert.ToDouble(a) * Convert.ToDouble(b) * Convert.ToDouble(c));
#else
			var mult = MulT;
			return mult(mult(a, b), c);
#endif
		}
	}
}
