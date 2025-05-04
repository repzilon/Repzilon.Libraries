//
//  TwoDVector.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2025 René Rhéaume
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
// ReSharper disable InvokeAsExtensionMethod

namespace Repzilon.Libraries.Core.Vectors
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct TwoDVector<T> : ICartesianVector<T>, IEquatable<TwoDVector<T>>, IEquatable<PolarVector<T>>,
	IComparableTwoDVector, IEquatable<IComparableTwoDVector>, IEquatable<IComparablePolarVector>
	where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
	{
		public T X { get; private set; }
		public T Y { get; private set; }

		IComparable IComparableTwoDVector.X
		{
			get { return X; }
		}

		IComparable IComparableTwoDVector.Y
		{
			get { return Y; }
		}

		public TwoDVector(T x, T y) : this()
		{
			X = x;
			Y = y;
		}

		public TwoDVector(PolarVector<T> vector) : this()
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
			var mnt = ExtraMath.ConvertTo<T>(-1 * n);
			if (RoundOff.AreEqual(vav, 0)) {
				return new KeyValuePair<T, T>(nt, zt);
			} else if (RoundOff.AreEqual(vav, quarterTurn)) {
				return new KeyValuePair<T, T>(zt, nt);
			} else if (RoundOff.AreEqual(vav, 2 * quarterTurn)) {
				return new KeyValuePair<T, T>(mnt, zt);
			} else if (RoundOff.AreEqual(vav, 3 * quarterTurn)) {
				return new KeyValuePair<T, T>(zt, mnt);
			} else {
				return ToCartesian(n, va);
			}
		}

		private static KeyValuePair<T, T> ToCartesian(double n, Angle<T> va)
		{
			var theta = va.ConvertTo<double>(AngleUnit.Radian, true).Value;
			return new KeyValuePair<T, T>(
			 ExtraMath.ConvertTo<T>(n * Math.Cos(theta)),
			 ExtraMath.ConvertTo<T>(n * Math.Sin(theta)));
		}

		#region ICloneable members
		public TwoDVector(TwoDVector<T> source) : this(source.X, source.Y) { }

		public TwoDVector<T> Clone()
		{
			return new TwoDVector<T>(X, Y);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
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
			return new TwoDVector<TOut>(
				ExtraMath.ConvertTo<TOut>(X),
				ExtraMath.ConvertTo<TOut>(Y));
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
			return new TwoDVector<T>(
			 ExtraMath.ConvertTo<T>(f * Convert.ToDouble(X)),
			 ExtraMath.ConvertTo<T>(f * Convert.ToDouble(Y)));
		}
		#endregion

		#region ToPolar
		public Angle<double> Angle()
		{
			return Angle<double>.Radians(Math.Atan2(Convert.ToDouble(Y), Convert.ToDouble(X))).Normalize();
		}

		public PolarVector<double> ToPolar()
		{
			return new PolarVector<double>(Norm(), Angle());
		}

		public PolarVector<TOut> ToPolar<TOut>()
		where TOut : struct, IFormattable, IEquatable<TOut>, IComparable<TOut>, IComparable
		{
#if NETSTANDARD1_1
			return new PolarVector<TOut>(Norm().ConvertTo<TOut>(), Angle().ConvertTo<TOut>(
			 (X is decimal) || (X is double) || (X is float) ? AngleUnit.Radian : AngleUnit.Degree, false));
#else
			// casting to IConvertible reduces IL size
			var tc = ((IConvertible)this.X).GetTypeCode();
			// Between Decimal and Single, we have Single, Double and Decimal, which are what we are looking for
			return new PolarVector<TOut>(ExtraMath.ConvertTo<TOut>(this.Norm()),
			 Angle().ConvertTo<TOut>(
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
				var twoD = obj as IComparableTwoDVector;
				return twoD != null ? Equals(twoD) : Equals(obj as IComparablePolarVector);
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

		private bool Equals(IComparablePolarVector other)
		{
			if (other != null) {
				var n = this.Norm();
				if (RoundOff.Equals(n, Convert.ToDouble(other.Norm))) {
					var oa = other.Angle;
					return Equals(new TwoDVector<T>(
					 ExtraMath.ConvertTo<T>(n * oa.Cos()),
					 ExtraMath.ConvertTo<T>(n * oa.Sin())));
				}
			}
			return false;
		}

		bool IEquatable<IComparablePolarVector>.Equals(IComparablePolarVector other)
		{
			return this.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (1861411795 * -1521134295) + X.GetHashCode();
				return (hashCode * -1521134295) + Y.GetHashCode();
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
#if NET35 || NET20
			if (RetroCompat.IsNullOrWhiteSpace(format)) {
#else
			if (String.IsNullOrWhiteSpace(format)) {
#endif
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
#if NET20
			return new TwoDVector<T>(Arithmetic<T>.AddScalars(u.X, v.X),
			 Arithmetic<T>.AddScalars(u.Y, v.Y));
#else
			var addi = Arithmetic<T>.Adder;
			return new TwoDVector<T>(addi(u.X, v.X), addi(u.Y, v.Y));
#endif
		}

		public static TwoDVector<T> operator -(TwoDVector<T> u, TwoDVector<T> v)
		{
#if NET20
			return new TwoDVector<T>(Arithmetic<T>.SubtractScalars(u.X, v.X),
			 Arithmetic<T>.SubtractScalars(u.Y, v.Y));
#else
			var sub = Arithmetic<T>.Sub;
			return new TwoDVector<T>(sub(u.X, v.X), sub(u.Y, v.Y));
#endif
		}

		public static TwoDVector<T> operator +(TwoDVector<T> u, PolarVector<T> v)
		{
			return u + new TwoDVector<T>(v);
		}

		public static TwoDVector<T> operator -(TwoDVector<T> u, PolarVector<T> v)
		{
			return u - new TwoDVector<T>(v);
		}

#if !NET20
		// For product operators, see https://www.haroldserrano.com/blog/developing-a-math-engine-in-c-implementing-vectors

		public static TwoDVector<T> operator *(T k, TwoDVector<T> v)
		{
			return v.Multiply(k);
		}

		public static T operator *(TwoDVector<T> u, TwoDVector<T> v)
		{
			return Dot(u, v);
		}

		public static T operator *(TwoDVector<T> u, PolarVector<T> v)
		{
			return Dot(u, v.ToCartesian());
		}

		public static ThreeDVector<T> operator %(TwoDVector<T> u, TwoDVector<T> v)
		{
			return Cross(u, v);
		}

		public static ThreeDVector<T> operator %(TwoDVector<T> u, PolarVector<T> v)
		{
			return Cross(u, v.ToCartesian());
		}
#endif
		#endregion

		public static T Dot(TwoDVector<T> u, TwoDVector<T> v)
		{
#if NET20
			return Arithmetic<T>.AddScalars(
			 Arithmetic<T>.MultiplyScalars(u.X, v.X), Arithmetic<T>.MultiplyScalars(u.Y, v.Y));
#else
			var mult = Arithmetic<T>.MulT;
			return Arithmetic<T>.Adder(mult(u.X, v.X), mult(u.Y, v.Y));
#endif
		}

		public static bool ArePerpendicular(TwoDVector<T> u, TwoDVector<T> v)
		{
			return Dot(u, v).Equals(default(T));
		}

		public static bool AreParallel(TwoDVector<T> u, TwoDVector<T> v)
		{
			var bu = Convert.ToDecimal(u.Y) / Convert.ToDecimal(u.X);
			var bv = Convert.ToDecimal(v.Y) / Convert.ToDecimal(v.X);
			return bu == bv; // identical slope
		}

		public static ThreeDVector<T> Cross(TwoDVector<T> u, TwoDVector<T> v)
		{
#if NET20
			return new ThreeDVector<T>(default(T), default(T),
			 Arithmetic<T>.SubtractScalars(
			 Arithmetic<T>.MultiplyScalars(u.X, v.Y), Arithmetic<T>.MultiplyScalars(u.Y, v.X)));
#else
			var mult = Arithmetic<T>.MulT;
			return new ThreeDVector<T>(default(T), default(T),
			 Arithmetic<T>.Sub(mult(u.X, v.Y), mult(u.Y, v.X)));
#endif
		}

		public static Angle<double> AngleBetween(TwoDVector<T> u, TwoDVector<T> v)
		{
			return Angle<double>.Radians(Math.Acos(Convert.ToDouble(Dot(u, v)) / (u.Norm() * v.Norm())));
		}
	}

	public static class TwoDVectorExtensions
	{
#if !NET20
		public static TwoDVector<T> Multiply<T, TScalar>(this TwoDVector<T> v, TScalar k)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
		where TScalar : struct, IEquatable<TScalar>
		{
			var mult = Arithmetic<T>.BuildMultiplier<TScalar>();
			return new TwoDVector<T>(mult(k, v.X), mult(k, v.Y));
		}
#endif

#if NET20
		public static TwoDVector<float> RoundError(TwoDVector<float> v)
#else
		public static TwoDVector<float> RoundError(this TwoDVector<float> v)
#endif
		{
			return new TwoDVector<float>(RoundOff.Error(v.X), RoundOff.Error(v.Y));
		}

#if NET20
		public static TwoDVector<double> RoundError(TwoDVector<double> v)
#else
		public static TwoDVector<double> RoundError(this TwoDVector<double> v)
#endif
		{
			return new TwoDVector<double>(RoundOff.Error(v.X), RoundOff.Error(v.Y));
		}

#if NET20
		public static TwoDVector<decimal> RoundError(TwoDVector<decimal> v)
#else
		public static TwoDVector<decimal> RoundError(this TwoDVector<decimal> v)
#endif
		{
			return new TwoDVector<decimal>(RoundOff.Error(v.X), RoundOff.Error(v.Y));
		}

		[CLSCompliant(false)]
#if NET20
		public static TwoDVector<Exp> RoundError(TwoDVector<Exp> v)
#else
		public static TwoDVector<Exp> RoundError(this TwoDVector<Exp> v)
#endif
		{
			return new TwoDVector<Exp>(RoundOff.Error(v.X), RoundOff.Error(v.Y));
		}

		[CLSCompliant(false)]
#if NET20
		public static TwoDVector<Exp18> RoundError(TwoDVector<Exp18> v)
#else
		public static TwoDVector<Exp18> RoundError(this TwoDVector<Exp18> v)
#endif
		{
			return new TwoDVector<Exp18>(RoundOff.Error(v.X), RoundOff.Error(v.Y));
		}
	}
}
