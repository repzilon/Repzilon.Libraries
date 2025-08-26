//
//  MultipleLinearRegression.cs
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

namespace Repzilon.Libraries.Core.Regression
{
	public static class MultipleLinearRegression
	{
		public static T[] Compute<T>(params TwoXPoint<T>[] dataPoints)
		where T : struct, IFormattable, IEquatable<T>
		{
			if (dataPoints == null) {
				throw new ArgumentNullException(nameof(dataPoints));
			}

			T   sumX1   = default(T), sumX2   = default(T);
			T   sumX1X1 = default(T), sumX2X2 = default(T), sumX1X2 = default(T);
			T   sumY    = default(T), sumX1Y  = default(T), sumX2Y  = default(T);
			var n       = dataPoints.Length;
#if !NET20
			var add = Arithmetic<T>.Adder;
#endif

			for (var i = 0; i < n; i++) {
				var pt = dataPoints[i];
#if NET20
				sumX1   = Arithmetic<T>.AddScalars(sumX1, pt.X1);
				sumX2   = Arithmetic<T>.AddScalars(sumX2, pt.X2);
				sumX1X1 = Arithmetic<T>.AddScalars(sumX1X1, pt.X1Squared());
				sumX1X2 = Arithmetic<T>.AddScalars(sumX1X2, pt.X1X2());
				sumX2X2 = Arithmetic<T>.AddScalars(sumX2X2, pt.X2Squared());
				sumY    = Arithmetic<T>.AddScalars(sumY, pt.Y);
				sumX1Y  = Arithmetic<T>.AddScalars(sumX1Y, pt.X1Y());
				sumX2Y  = Arithmetic<T>.AddScalars(sumX2Y, pt.X2Y());
#else
				sumX1   = add(sumX1, pt.X1);
				sumX2   = add(sumX2, pt.X2);
				sumX1X1 = add(sumX1X1, pt.X1Squared());
				sumX1X2 = add(sumX1X2, pt.X1X2());
				sumX2X2 = add(sumX2X2, pt.X2Squared());
				sumY    = add(sumY, pt.Y);
				sumX1Y  = add(sumX1Y, pt.X1Y());
				sumX2Y  = add(sumX2Y, pt.X2Y());
#endif
			}

			var nt = ExtraMath.ConvertTo<T>(n);
#if !NET20
			var mul = Arithmetic<T>.MulT;
			var sub = Arithmetic<T>.Sub;
#endif

#if NET20
			LowercaseVariableSum(ref sumX1X1, sumX1, nt);
			LowercaseVariableSum(ref sumX2X2, sumX2, nt);
			LowercaseVariableSum(ref sumX1X2, sumX1, sumX2, nt);
			LowercaseVariableSum(ref sumX1Y, sumX1, sumY, nt);
			LowercaseVariableSum(ref sumX2Y, sumX2, sumY, nt);
#else
			LowercaseVariableSum(ref sumX1X1, sub, mul, sumX1, nt);
			LowercaseVariableSum(ref sumX2X2, sub, mul, sumX2, nt);
			LowercaseVariableSum(ref sumX1X2, sub, mul, sumX1, sumX2, nt);
			LowercaseVariableSum(ref sumX1Y, sub, mul, sumX1, sumY, nt);
			LowercaseVariableSum(ref sumX2Y, sub, mul, sumX2, sumY, nt);
#endif

#if NET20
			var denom = Arithmetic<T>.SubtractScalars(Arithmetic<T>.MultiplyScalars(sumX1X1, sumX2X2),
			 Arithmetic<T>.MultiplyScalars(sumX1X2, sumX1X2));
			var b2    = Coefficient(sumX1X1, sumX2Y, sumX1Y, sumX1X2, denom);
			var b1    = Coefficient(sumX2X2, sumX1Y, sumX2Y, sumX1X2, denom);
#else
			var denom = sub(mul(sumX1X1, sumX2X2), mul(sumX1X2, sumX1X2));
			var b2    = Coefficient(sumX1X1, sumX2Y, sumX1Y, sub, mul, sumX1X2, denom);
			var b1    = Coefficient(sumX2X2, sumX1Y, sumX2Y, sub, mul, sumX1X2, denom);
#endif

			var b0 = Arithmetic<T>.DivideScalars(sumY, nt);
#if NET20
			b0 = Arithmetic<T>.SubtractScalars(b0, ProductOfAverage(b1, sumX1, nt));
			b0 = Arithmetic<T>.SubtractScalars(b0, ProductOfAverage(b2, sumX2, nt));
#else
			b0 = sub(b0, ProductOfAverage(b1, sumX1, nt, mul));
			b0 = sub(b0, ProductOfAverage(b2, sumX2, nt, mul));
#endif

			return new T[] { b0, b1, b2 };
		}

#if NET20
		private static void LowercaseVariableSum<T>(ref T accumulator, T sumX, T n)
		where T : struct, IFormattable, IEquatable<T>
		{
			accumulator = Arithmetic<T>.SubtractScalars(accumulator,
			 Arithmetic<T>.DivideScalars(Arithmetic<T>.MultiplyScalars(sumX, sumX), n));
		}

		private static void LowercaseVariableSum<T>(ref T accumulator, T sumX1, T sumY, T n)
		where T : struct, IFormattable, IEquatable<T>
		{
			accumulator = Arithmetic<T>.SubtractScalars(accumulator,
			 Arithmetic<T>.DivideScalars(Arithmetic<T>.MultiplyScalars(sumX1, sumY), n));
		}

		private static T Coefficient<T>(T squared, T second, T third, T sumX1X2, T denom)
		where T : struct, IFormattable, IEquatable<T>
		{
			return Arithmetic<T>.DivideScalars(Arithmetic<T>.SubtractScalars(
			 Arithmetic<T>.MultiplyScalars(squared, second), Arithmetic<T>.MultiplyScalars(sumX1X2, third)), denom);
		}

		private static T ProductOfAverage<T>(T b, T sum, T n) where T : struct, IFormattable, IEquatable<T>
		{
			return Arithmetic<T>.MultiplyScalars(Arithmetic<T>.DivideScalars(sum, n), b);
		}
#else
		private static void LowercaseVariableSum<T>(ref T accumulator, Func<T, T, T> subX, Func<T, T, T> mulX,
		T sumX, T n) where T : struct, IFormattable, IEquatable<T>
		{
			accumulator = subX(accumulator, Arithmetic<T>.DivideScalars(mulX(sumX, sumX), n));
		}

		private static void LowercaseVariableSum<T>(ref T accumulator, Func<T, T, T> subY, Func<T, T, T> mulXY,
		T sumX1, T sumY, T n) where T : struct, IFormattable, IEquatable<T>
		{
			accumulator = subY(accumulator, Arithmetic<T>.DivideScalars(mulXY(sumX1, sumY), n));
		}

		private static T Coefficient<T>(T squared, T second, T third, Func<T, T, T> subY, Func<T, T, T> mulXY,
		T sumX1X2, T denom) where T : struct, IFormattable, IEquatable<T>
		{
			return Arithmetic<T>.DivideScalars(subY(mulXY(squared, second), mulXY(sumX1X2, third)), denom);
		}

		private static T ProductOfAverage<T>(T b, T sum, T n, Func<T, T, T> mulXY)
		where T : struct, IFormattable, IEquatable<T>
		{
			return mulXY(Arithmetic<T>.DivideScalars(sum, n), b);
		}
#endif
	}
}
