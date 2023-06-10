//
//  TwoDVector.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2023 René Rhéaume
//
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v. 2.0. If a copy of the MPL was 
// not distributed with this file, You can obtain one at 
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	[StructLayout(LayoutKind.Auto)]
	public struct TwoDVector<T> : ICloneable, IFormattable, IEquatable<TwoDVector<T>>, IEquatable<PolarVector<T>>
	where T : struct, IConvertible, IFormattable, IEquatable<T>, IComparable<T>
	{
		public readonly T X;
		public readonly T Y;

		public TwoDVector(T x, T y)
		{
			X = x;
			Y = y;
		}

		public TwoDVector(PolarVector<T> vector)
		{
			double n = Convert.ToDouble(vector.Norm);
			if (vector.Angle.Unit == AngleUnit.Degree) {
				var vav = Convert.ToDouble(vector.Angle.Normalize().Value);
				T nt = n.ConvertTo<T>();
				T zt = 0.ConvertTo<T>();
				T mnt = (-1 * n).ConvertTo<T>();
				if (vav == 0) {
					X = nt;
					Y = zt;
				} else if (vav == 90) {
					X = zt;
					Y = nt;
				} else if (vav == 180) {
					X = (-1 * n).ConvertTo<T>();
					Y = zt;
				} else if (vav == 270) {
					X = zt;
					Y = (-1 * n).ConvertTo<T>();
				} else {
					double theta = vector.Angle.ConvertTo<double>(AngleUnit.Radian, true).Value;
					X = (n * Math.Cos(theta)).ConvertTo<T>();
					Y = (n * Math.Sin(theta)).ConvertTo<T>();
				}
			} else if (vector.Angle.Unit == AngleUnit.Gradian) {
				var vav = Convert.ToDouble(vector.Angle.Normalize().Value);
				T nt = n.ConvertTo<T>();
				T zt = 0.ConvertTo<T>();
				T mnt = (-1 * n).ConvertTo<T>();
				if (vav == 0) {
					X = nt;
					Y = zt;
				} else if (vav == 100) {
					X = zt;
					Y = nt;
				} else if (vav == 200) {
					X = (-1 * n).ConvertTo<T>();
					Y = zt;
				} else if (vav == 300) {
					X = zt;
					Y = (-1 * n).ConvertTo<T>();
				} else {
					double theta = vector.Angle.ConvertTo<double>(AngleUnit.Radian, true).Value;
					X = (n * Math.Cos(theta)).ConvertTo<T>();
					Y = (n * Math.Sin(theta)).ConvertTo<T>();
				}
			} else {
				double theta = vector.Angle.ConvertTo<double>(AngleUnit.Radian, true).Value;
				X = (n * Math.Cos(theta)).ConvertTo<T>();
				Y = (n * Math.Sin(theta)).ConvertTo<T>();
			}
		}

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

		#region ToPolar
		public double Norm()
		{
			if (typeof(T) == typeof(decimal)) {
				return (double)ExtraMath.Hypoth(Convert.ToDecimal(X), Convert.ToDecimal(Y));
			} else {
				return ExtraMath.Hypoth(Convert.ToDouble(X), Convert.ToDouble(Y));
			}
		}

		public Angle<double> Angle()
		{
			return new Angle<double>(Math.Atan2(Convert.ToDouble(Y), Convert.ToDouble(X)), AngleUnit.Radian).Normalize();
		}

		public PolarVector<double> ToPolar()
		{
			return new PolarVector<double>(Norm(), Angle());
		}

		public PolarVector<TOut> ToPolar<TOut>()
		where TOut : struct, IConvertible, IFormattable, IEquatable<TOut>, IComparable<TOut>
		{
			var tc = this.X.GetTypeCode();
			var au = ((tc == TypeCode.Decimal) || (tc == TypeCode.Double) || (tc == TypeCode.Single)) ? AngleUnit.Radian : AngleUnit.Degree;
			return new PolarVector<TOut>(Norm().ConvertTo<TOut>(), Angle().ConvertTo<TOut>(au, false));
		}
		#endregion

		public TwoDVector<TOut> Cast<TOut>()
		where TOut : struct, IConvertible, IFormattable, IEquatable<TOut>, IComparable<TOut>
		{
			return new TwoDVector<TOut>(X.ConvertTo<TOut>(), Y.ConvertTo<TOut>());
		}

		public TwoDVector<T> ToUnitary()
		{
			var f = 1.0 / this.Norm();
			return new TwoDVector<T>((f * Convert.ToDouble(X)).ConvertTo<T>(), (f * Convert.ToDouble(Y)).ConvertTo<T>());
		}

		#region Equals
		public override bool Equals(object obj)
		{
			if (obj is TwoDVector<T>) {
				return Equals((TwoDVector<T>)obj);
			} else if (obj is PolarVector<T>) {
				return Equals((PolarVector<T>)obj);
			} else {
				return false;
			}
		}

		public bool Equals(TwoDVector<T> other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		public bool Equals(PolarVector<T> other)
		{
			return Equals(new TwoDVector<T>(other));
		}

		public override int GetHashCode()
		{
			int hashCode = 1861411795;
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(TwoDVector<T> left, TwoDVector<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TwoDVector<T> left, TwoDVector<T> right)
		{
			return !(left == right);
		}

		public static bool operator ==(TwoDVector<T> left, PolarVector<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TwoDVector<T> left, PolarVector<T> right)
		{
			return !(left == right);
		}
		#endregion

		#region ToString
		public override string ToString()
		{
			return this.ToString(null, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrWhiteSpace(format)) {
				format = "G";
			}
			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}
			var stbVector = new StringBuilder();
			stbVector.Append('(').Append(X.ToString(format, formatProvider)).Append("; ");
			stbVector.Append(Y.ToString(format, formatProvider)).Append(')');
			return stbVector.ToString();
		}
		#endregion

		#region Operators
		public static TwoDVector<T> operator +(TwoDVector<T> a, TwoDVector<T> b)
		{
			return new TwoDVector<T>(Matrix<T>.add(a.X, b.X), Matrix<T>.add(a.Y, b.Y));
		}

		public static TwoDVector<T> operator -(TwoDVector<T> a, TwoDVector<T> b)
		{
			return new TwoDVector<T>(Matrix<T>.sub(a.X, b.X), Matrix<T>.sub(a.Y, b.Y));
		}

		public static TwoDVector<T> operator +(TwoDVector<T> a, PolarVector<T> b)
		{
			return a + new TwoDVector<T>(b);
		}

		public static TwoDVector<T> operator -(TwoDVector<T> a, PolarVector<T> b)
		{
			return a - new TwoDVector<T>(b);
		}

		public static TwoDVector<T> operator *(T k, TwoDVector<T> v)
		{
			return TwoDVectorExtensions.Multiply(v, k);
		}

		// TODO : scalar product operator
		// TODO : vector product operator
		// TODO : implement operators between TwoDVector and PolarVector
		#endregion

		// TODO : implement AreParallel static method
		// TODO : implement ArePerpendicular static method
		// TODO : implement AngleBetween static method

		public static T Sum(T norm1, T norm2, Angle<T> between)
		{
			var mul = Matrix<T>.BuildMultiplier<T>();
			var add = Matrix<T>.add;
			T squaredResult = add(mul(norm1, norm1), mul(norm2, norm2));
			var cos = (new Angle<T>(180.ConvertTo<T>(), AngleUnit.Degree) - between).Cos();
			var third = mul(mul(mul(norm1, norm2), cos.ConvertTo<T>()), (-2).ConvertTo<T>());
			squaredResult = add(squaredResult, third);
			if (squaredResult.GetTypeCode() == TypeCode.Decimal) {
				return ExtraMath.Sqrt(squaredResult.ConvertTo<decimal>()).ConvertTo<T>();
			} else {
				return Math.Sqrt(squaredResult.ConvertTo<double>()).ConvertTo<T>();
			}
		}

		public static T Sum(T norm1, T norm2, T angleBetween, AngleUnit unit)
		{
			return Sum(norm1, norm2, new Angle<T>(angleBetween, unit));
		}
	}

	public static class TwoDVectorExtensions
	{
		public static TwoDVector<T> Multiply<T, TScalar>(this TwoDVector<T> v, TScalar k)
		where T : struct, IConvertible, IFormattable, IEquatable<T>, IComparable<T>
		where TScalar : struct, IConvertible, IEquatable<TScalar>
		{
			var mul = Matrix<T>.BuildMultiplier<TScalar>();
			return new TwoDVector<T>(mul(k, v.X), mul(k, v.Y));
		}

		public static TwoDVector<float> RoundError(this TwoDVector<float> v)
		{
			return new TwoDVector<float>(Round.Error(v.X), Round.Error(v.Y));
		}

		public static TwoDVector<double> RoundError(this TwoDVector<double> v)
		{
			return new TwoDVector<double>(Round.Error(v.X), Round.Error(v.Y));
		}
	}
}
