//
//  Vector.cs
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

namespace Repzilon.Libraries.Core.Vectors
{
	public static class Vector
	{
		public static TwoDVector<T> New<T>(T x, T y)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
		{
			return new TwoDVector<T>(x, y);
		}

		public static ThreeDVector<T> New<T>(T x, T y, T z)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
		{
			return new ThreeDVector<T>(x, y, z);
		}

		public static PolarVector<T> New<T>(T norm, T angle, AngleUnit unit)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
		{
			return new PolarVector<T>(norm, angle, unit);
		}

		public static PolarVector<T> New<T>(T norm, Angle<T> angle)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
		{
			return new PolarVector<T>(norm, angle);
		}
	}

	public static class Vector<T> where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
	{
#if !NET20
		private static readonly Angle<T> HalfCircle = new Angle<T>(180.ConvertTo<T>(), AngleUnit.Degree);

		public static T Sum(T norm1, T norm2, Angle<T> between)
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<T>();
			var addi = GenericArithmetic<T>.Adder;
			var squaredResult = addi(addi(mult(norm1, norm1), mult(norm2, norm2)),
			 mult(mult(mult(norm1, norm2), (HalfCircle - between).Cos().ConvertTo<T>()), (-2).ConvertTo<T>()));
#if NETSTANDARD1_1
			if (squaredResult is decimal) {
#else
			if (((IConvertible)squaredResult).GetTypeCode() == TypeCode.Decimal) {
#endif
				return ExtraMath.Sqrt(Convert.ToDecimal(squaredResult)).ConvertTo<T>();
			} else {
				return Math.Sqrt(Convert.ToDouble(squaredResult)).ConvertTo<T>();
			}
		}

		public static T Sum(T norm1, T norm2, T angleBetween, AngleUnit unit)
		{
			return Sum(norm1, norm2, new Angle<T>(angleBetween, unit));
		}

		public static T Dot(T norm1, T norm2, Angle<T> between)
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<T>();
			return mult(mult(norm1, norm2), between.Cos().ConvertTo<T>());
		}

		public static T Dot(T norm1, T norm2, T angleBetween, AngleUnit unit)
		{
			return Dot(norm1, norm2, new Angle<T>(angleBetween, unit));
		}
#endif
	}
}

