//
//  GenericArithmetic.cs
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
using System.Linq.Expressions;

namespace Repzilon.Libraries.Core
{
	public static class GenericArithmetic<T> where T: struct
	{
		internal static Func<TScalar, T, T> BuildMultiplier<TScalar>()
		where TScalar : struct
		{
			// Declare the parameters
			var paramA = Expression.Parameter(typeof(TScalar), "a");
			var paramB = Expression.Parameter(typeof(T), "b");

			// Add the parameters together
			BinaryExpression body = Expression.Multiply(paramA, paramB);

			// Compile it
			return Expression.Lambda<Func<TScalar, T, T>>(body, paramA, paramB).Compile();
		}

		#region Static members
		internal static readonly Func<T, T, T> adder = BuildAdder();
		internal static readonly Func<T, T, T> sub = BuildSubtractor();

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

		public static T AddScalars(T a, T b)
		{
			return adder(a, b);
		}

		public static T SubtractScalars(T a, T b)
		{
			return sub(a, b);
		}

		public static T MultiplyScalars(T a, T b)
		{
			return BuildMultiplier<T>()(a, b);
		}

		public static T MultiplyScalars(T a, T b, T c)
		{
			var mult = BuildMultiplier<T>();
			return mult(mult(a, b), c);
		}
		#endregion
	}
}

