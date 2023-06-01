//
//  TwoDVector.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022 René Rhéaume
//
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v. 2.0. If a copy of the MPL was 
// not distributed with this file, You can obtain one at 
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Repzilon.Libraries.Core
{
	// TODO : Implement IEquatable<TwoDVector<T>> to TwoDVector
	// TODO : Implement IEquatable<PolarVector<T>> to TwoDVector
	// TODO : Implement IFormattable to TwoDVector
	[StructLayout(LayoutKind.Auto)]
	public struct TwoDVector<T> : ICloneable
	where T : struct, IConvertible, IEquatable<T>
	{
		public readonly T X;
		public readonly T Y;

		public TwoDVector(T x, T y)
		{
			X = x;
			Y = y;
		}

		// TODO : Implement constructor with a PolarVector argument

		#region ICloneable members
		public TwoDVector<T> Clone()
		{
			return new TwoDVector<T>(X, Y);
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
		#endregion

		public double Norm()
		{
			double xd = Convert.ToDouble(X);
			double yd = Convert.ToDouble(Y);
			return Math.Sqrt((xd * xd) + (yd * yd));
		}

		public Angle<double> Angle()
		{
			return new Angle<double>(Math.Atan2(Convert.ToDouble(Y), Convert.ToDouble(X)), AngleUnit.Radian).Normalize();
		}

		// TODO : Implement ToPolar() method

		#region Operators
		public static TwoDVector<T> operator +(TwoDVector<T> a, TwoDVector<T> b)
		{
			return new TwoDVector<T>(Matrix<T>.add(a.X, b.X), Matrix<T>.add(a.Y, b.Y));
		}

		public static TwoDVector<T> operator -(TwoDVector<T> a, TwoDVector<T> b)
		{
			return new TwoDVector<T>(Matrix<T>.sub(a.X, b.X), Matrix<T>.sub(a.Y, b.Y));
		}

		public static TwoDVector<T> operator *(T k, TwoDVector<T> v)
		{
			return TwoDVectorExtensionMethods.Multiply(v, k);
		}

		// TODO : scalar product operator
		// TODO : vector product operator
		// TODO : implement operators between TwoDVector and PolarVector
		#endregion

		// TODO : implement AreParallel static method
		// TODO : implement ArePerpendicular static method
		// TODO : implement AngleBetween static method
	}

	public static class TwoDVectorExtensionMethods
	{
		public static TwoDVector<T> Multiply<T, TScalar>(this TwoDVector<T> v, TScalar k)
		where T : struct, IConvertible, IEquatable<T>
		where TScalar : struct, IConvertible, IEquatable<TScalar>
		{
			var mul = Matrix<T>.BuildMultiplier<TScalar>();
			return new TwoDVector<T>(mul(k, v.X), mul(k, v.Y));
		}

		public static TwoDVector<TOut> Cast<TIn, TOut>(this TwoDVector<TIn> source)
		where TIn : struct, IConvertible, IEquatable<TIn>
		where TOut : struct, IConvertible, IEquatable<TOut>
		{
			return new TwoDVector<TOut>(source.X.ConvertTo<TOut>(), source.Y.ConvertTo<TOut>());
		}
	}
}
