//
//  TwoDVector.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2024 René Rhéaume
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
	// TODO : Implement IEquatable<PolarVector<TOther>>
	[StructLayout(LayoutKind.Auto)]
	public struct TwoDVector<T> : ICartesianVector<T>, IEquatable<TwoDVector<T>>, IEquatable<PolarVector<T>>,
	IComparableTwoDVector, IEquatable<IComparableTwoDVector>
	where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
	{
		public T X { get; private set; }
		public T Y { get; private set; }

		IComparable IComparableTwoDVector.X {
			get { return this.X; }
		}

		IComparable IComparableTwoDVector.Y {
			get { return this.Y; }
		}

		public TwoDVector(T x, T y)
		{
			X = x;
			Y = y;
		}

		public TwoDVector(PolarVector<T> vector)
		{
			var nt = vector.Norm;
			var va = vector.Angle;
			KeyValuePair<T, T> kvp;
			var vau = va.Unit;
			if (vau == AngleUnit.Degree) {
				kvp = ToCartesian(90, nt, va);
			} else if (vau == AngleUnit.Gradian) {
				kvp = ToCartesian(100, nt, va);
			} else {
				kvp = ToCartesian(Convert.ToDouble(nt), va);
			}
			X = kvp.Key;
			Y = kvp.Value;
		}

		private static KeyValuePair<T, T> ToCartesian(byte quarterTurn, T nt, Angle<T> va)
		{
			var vav = Convert.ToDouble(va.Value);
			var zt = default(T);
			var n = Convert.ToDouble(nt);
			var mnt = (-1 * n).ConvertTo<T>();
			if (vav == 0) {
				return new KeyValuePair<T, T>(nt, zt);
			} else if (vav == quarterTurn) {
				return new KeyValuePair<T, T>(zt, nt);
			} else if (vav == 2 * quarterTurn) {
				return new KeyValuePair<T, T>(mnt, zt);
			} else if (vav == 3 * quarterTurn) {
				return new KeyValuePair<T, T>(zt, mnt);
			} else {
				return ToCartesian(n, va);
			}
		}

		private static KeyValuePair<T, T> ToCartesian(double n, Angle<T> va)
		{
			double theta = va.ConvertTo<double>(AngleUnit.Radian, true).Value;
			return new KeyValuePair<T, T>((n * Math.Cos(theta)).ConvertTo<T>(),
			 (n * Math.Sin(theta)).ConvertTo<T>());
		}

		#region ICloneable members
		public TwoDVector(TwoDVector<T> source) : this(source.X, source.Y) { }

		public TwoDVector<T> Clone()
		{
			return new TwoDVector<T>(X, Y);
		}

#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		#region ICartesianVector members
		public double Norm()
		{
			ValueType cx = this.X;
			ValueType cy = this.Y;
			if (cx is decimal) {
				return Convert.ToDouble(ExtraMath.Hypoth((decimal)cx, (decimal)cy));
			} else {
				return ExtraMath.Hypoth(Convert.ToDouble(cx), Convert.ToDouble(cy));
			}
		}

		public TwoDVector<TOut> Cast<TOut>()
		where TOut : struct, IFormattable, IEquatable<TOut>, IComparable<TOut>, IComparable
		{
			return new TwoDVector<TOut>(X.ConvertTo<TOut>(), Y.ConvertTo<TOut>());
		}

		ICartesianVector<TOut> ICartesianVector<T>.Cast<TOut>()
		{
			return this.Cast<TOut>();
		}

		ICartesianVector<T> ICartesianVector<T>.ToUnitary()
		{
			return this.ToUnitary();
		}

		public TwoDVector<T> ToUnitary()
		{
			var f = 1.0 / this.Norm();
			return new TwoDVector<T>((f * Convert.ToDouble(X)).ConvertTo<T>(), (f * Convert.ToDouble(Y)).ConvertTo<T>());
		}
		#endregion

		#region ToPolar
		public Angle<double> Angle()
		{
			return new Angle<double>(Math.Atan2(Convert.ToDouble(Y), Convert.ToDouble(X)), AngleUnit.Radian).Normalize();
		}

		public PolarVector<double> ToPolar()
		{
			return new PolarVector<double>(Norm(), Angle());
		}

		public PolarVector<TOut> ToPolar<TOut>()
		where TOut : struct, IFormattable, IEquatable<TOut>, IComparable<TOut>, IComparable
		{
#if (NETSTANDARD1_1)
			return new PolarVector<TOut>(Norm().ConvertTo<TOut>(), Angle().ConvertTo<TOut>(
			 (X is decimal) || (X is double) || (X is float) ? AngleUnit.Radian : AngleUnit.Degree, false));
#else
			// casting to IConvertible reduces IL size
			var tc = ((IConvertible)this.X).GetTypeCode();
			// Between Decimal and Single, we have Single, Double and Decimal, which are what we are looking for
			return new PolarVector<TOut>(Norm().ConvertTo<TOut>(), Angle().ConvertTo<TOut>(
			 (tc <= TypeCode.Decimal) && (tc >= TypeCode.Single) ? AngleUnit.Radian : AngleUnit.Degree, false));
#endif
		}
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			if (obj is TwoDVector<T>) {
				return Equals((TwoDVector<T>)obj);
			} else if (obj is PolarVector<T>) {
				return Equals((PolarVector<T>)obj);
			} else {
				return Equals(obj as IComparableTwoDVector);
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

		private bool Equals(IComparableTwoDVector other)
		{
			var typT = typeof(T);
			return (other != null) && (this.X.CompareTo(Convert.ChangeType(other.X, typT)) == 0) &&
			 (this.Y.CompareTo(Convert.ChangeType(other.Y, typT)) == 0);
		}

		bool IEquatable<IComparableTwoDVector>.Equals(IComparableTwoDVector other)
		{
			return this.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = (1861411795 * -1521134295) + X.GetHashCode();
				return hashCode * -1521134295 + Y.GetHashCode();
			}
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
		public static TwoDVector<T> operator +(TwoDVector<T> u, TwoDVector<T> v)
		{
			var addi = GenericArithmetic<T>.adder;
			return new TwoDVector<T>(addi(u.X, v.X), addi(u.Y, v.Y));
		}

		public static TwoDVector<T> operator -(TwoDVector<T> u, TwoDVector<T> v)
		{
			var sub = GenericArithmetic<T>.sub;
			return new TwoDVector<T>(sub(u.X, v.X), sub(u.Y, v.Y));
		}

		public static TwoDVector<T> operator +(TwoDVector<T> u, PolarVector<T> v)
		{
			return u + new TwoDVector<T>(v);
		}

		public static TwoDVector<T> operator -(TwoDVector<T> u, PolarVector<T> v)
		{
			return u - new TwoDVector<T>(v);
		}

		// For product operators, see https://www.haroldserrano.com/blog/developing-a-math-engine-in-c-implementing-vectors

		public static TwoDVector<T> operator *(T k, TwoDVector<T> v)
		{
			return TwoDVectorExtensions.Multiply(v, k);
		}

		public static T operator *(TwoDVector<T> u, TwoDVector<T> v)
		{
			return Dot(u, v);
		}

		public static ThreeDVector<T> operator %(TwoDVector<T> u, TwoDVector<T> v)
		{
			return Cross(u, v);
		}

		// TODO : implement operators between TwoDVector and PolarVector
		#endregion

		// TODO : implement AreParallel static method

		public static T Sum(T norm1, T norm2, Angle<T> between)
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<T>();
			var addi = GenericArithmetic<T>.adder;
			var squaredResult = addi(addi(mult(norm1, norm1), mult(norm2, norm2)),
			 mult(mult(mult(norm1, norm2), (new Angle<T>(180.ConvertTo<T>(), AngleUnit.Degree) - between).Cos().ConvertTo<T>()), (-2).ConvertTo<T>()));
#if (NETSTANDARD1_1)
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

		public static T Dot(TwoDVector<T> u, TwoDVector<T> v)
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<T>();
			return GenericArithmetic<T>.adder(mult(u.X, v.X), mult(u.Y, v.Y));
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

		public static bool ArePerpendicular(TwoDVector<T> u, TwoDVector<T> v)
		{
			return Dot(u, v).Equals(default(T));
		}

		public static ThreeDVector<T> Cross(TwoDVector<T> u, TwoDVector<T> v)
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<T>();
			return new ThreeDVector<T>(default(T), default(T),
			 GenericArithmetic<T>.sub(mult(u.X, v.Y), mult(u.Y, v.X)));
		}

		public static Angle<double> AngleBetween(TwoDVector<T> u, TwoDVector<T> v)
		{
			return new Angle<double>(Math.Acos(Dot(u, v).ConvertTo<double>() / (u.Norm() * v.Norm())), AngleUnit.Radian);
		}
	}

	public static class TwoDVectorExtensions
	{
		public static TwoDVector<T> Multiply<T, TScalar>(this TwoDVector<T> v, TScalar k)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
		where TScalar : struct, IEquatable<TScalar>
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<TScalar>();
			return new TwoDVector<T>(mult(k, v.X), mult(k, v.Y));
		}

		public static TwoDVector<float> RoundError(this TwoDVector<float> v)
		{
			return new TwoDVector<float>(RoundOff.Error(v.X), RoundOff.Error(v.Y));
		}

		public static TwoDVector<double> RoundError(this TwoDVector<double> v)
		{
			return new TwoDVector<double>(RoundOff.Error(v.X), RoundOff.Error(v.Y));
		}

		public static TwoDVector<decimal> RoundError(this TwoDVector<decimal> v)
		{
			return new TwoDVector<decimal>(RoundOff.Error(v.X), RoundOff.Error(v.Y));
		}
	}
}
