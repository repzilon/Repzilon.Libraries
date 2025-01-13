//
//  Vector.cs
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
		internal static readonly Angle<T> HalfCircle = Angle<T>.Degrees(ExtraMath.ConvertTo<T>(180));

		public static T Sum(T norm1, T norm2, Angle<T> between)
		{
#if NET20
			var squaredResult = Arithmetic<T>.AddScalars(Arithmetic<T>.AddScalars(
			 Arithmetic<T>.MultiplyScalars(norm1, norm1), Arithmetic<T>.MultiplyScalars(norm2, norm2)),
			 Arithmetic<T>.MultiplyScalars(Arithmetic<T>.MultiplyScalars(
			 Arithmetic<T>.MultiplyScalars(norm1, norm2),
			 ExtraMath.ConvertTo<T>((HalfCircle - between).Cos())), ExtraMath.ConvertTo<T>(-2)));
#else
			var mult = Arithmetic<T>.MulT;
			var addi = Arithmetic<T>.Adder;
			var squaredResult = addi(addi(mult(norm1, norm1), mult(norm2, norm2)),
			 mult(mult(mult(norm1, norm2), (HalfCircle - between).Cos().ConvertTo<T>()), (-2).ConvertTo<T>()));
#endif
#if NETSTANDARD1_1
			if (squaredResult is decimal) {
#else
			if (((IConvertible)squaredResult).GetTypeCode() == TypeCode.Decimal) {
#endif
				return ExtraMath.ConvertTo<T>(ExtraMath.Sqrt(Convert.ToDecimal(squaredResult)));
			} else {
				return ExtraMath.ConvertTo<T>(Math.Sqrt(Convert.ToDouble(squaredResult)));
			}
		}

		public static T Sum(T norm1, T norm2, T angleBetween, AngleUnit unit)
		{
			return Sum(norm1, norm2, new Angle<T>(angleBetween, unit));
		}

		public static T Dot(T norm1, T norm2, Angle<T> between)
		{
#if NET20
			return Arithmetic<T>.MultiplyScalars(Arithmetic<T>.MultiplyScalars(norm1, norm2),
			 ExtraMath.ConvertTo<T>(between.Cos()));
#else
			var mult = Arithmetic<T>.MulT;
			return mult(mult(norm1, norm2), between.Cos().ConvertTo<T>());
#endif
		}

		public static T Dot(T norm1, T norm2, T angleBetween, AngleUnit unit)
		{
			return Dot(norm1, norm2, new Angle<T>(angleBetween, unit));
		}
	}
}

