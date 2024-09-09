//
//  PolarVector.cs
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
	public struct PolarVector<T> : IFormattable,
	IEquatable<PolarVector<T>>, IEquatable<TwoDVector<T>>,
	IComparablePolarVector, IEquatable<IComparablePolarVector>, IEquatable<IComparableTwoDVector>
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
	{
		public readonly T Norm;
		public readonly Angle<T> Angle;

		IComparable IComparablePolarVector.Norm
		{
			get { return this.Norm; }
		}

		IAngle IComparablePolarVector.Angle
		{
			get { return this.Angle; }
		}

		public PolarVector(T norm, Angle<T> angle)
		{
			Norm = norm;
#if NET20
			Angle = angle;
#else
			Angle = angle.Normalize();
#endif
		}

		public PolarVector(T norm, T angle, AngleUnit unit) : this(norm, new Angle<T>(angle, unit))
		{
		}

#if NET20
		public PolarVector(T norm, Angle<T> angle, AngleUnit newUnit) : this(norm, angle.ConvertTo<T>(newUnit))
#else
		public PolarVector(T norm, Angle<T> angle, AngleUnit newUnit) : this(norm, angle.ConvertTo<T>(newUnit, true))
#endif
		{
		}

		#region ICloneable members
		public PolarVector<T> Clone()
		{
			return new PolarVector<T>(Norm, Angle);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public PolarVector<TOut> Cast<TOut>()
		where TOut : struct, IFormattable, IEquatable<TOut>, IComparable<TOut>, IComparable
		{
			var a = this.Angle;
			return new PolarVector<TOut>(ExtraMath.ConvertTo<TOut>(this.Norm), ExtraMath.ConvertTo<TOut>(a.Value), a.Unit);
		}

		public TwoDVector<T> ToCartesian()
		{
			return new TwoDVector<T>(this);
		}

		public PolarVector<T> ConvertTo(AngleUnit unit)
		{
			return new PolarVector<T>(Norm, Angle, unit);
		}

		#region Equals
		public override bool Equals(object obj)
		{
			if (obj is PolarVector<T>) {
				return Equals((PolarVector<T>)obj);
			} else if (obj is TwoDVector<T>) {
				return Equals((TwoDVector<T>)obj);
			} else {
				var polar = obj as IComparablePolarVector;
				return (polar != null) ? Equals(polar) : Equals(obj as IComparableTwoDVector);
			}
		}

		public bool Equals(PolarVector<T> other)
		{
			return Norm.Equals(other.Norm) && Angle.Equals(other.Angle);
		}

		public bool Equals(TwoDVector<T> other)
		{
			return this.Cast<double>().Equals(other.ToPolar());
		}

		private bool Equals(IComparablePolarVector other)
		{
			return (other != null) && (this.Norm.CompareTo(Convert.ChangeType(other.Norm, typeof(T))) == 0) &&
			 this.Angle.Equals(other.Angle);
		}

		bool IEquatable<IComparablePolarVector>.Equals(IComparablePolarVector other)
		{
			return this.Equals(other);
		}

		private bool Equals(IComparableTwoDVector other)
		{
			return (other != null) && this.Equals(this.ToCartesian());
		}

		bool IEquatable<IComparableTwoDVector>.Equals(IComparableTwoDVector other)
		{
			return this.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (1227039071 * -1521134295) + Norm.GetHashCode();
				return (hashCode * -1521134295) + Angle.GetHashCode();
			}
		}

		public static bool operator ==(PolarVector<T> left, PolarVector<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PolarVector<T> left, PolarVector<T> right)
		{
			return !(left == right);
		}

		public static bool operator ==(PolarVector<T> left, TwoDVector<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PolarVector<T> left, TwoDVector<T> right)
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
			stbVector.Append(this.Norm.ToString(format, formatProvider));
			stbVector.Append('∠').Append(this.Angle.ToString(format, formatProvider));
			return stbVector.ToString();
		}
		#endregion

		#region Operators
		// TODO : make addition and subtraction of polar vector return a polar vector
#if !NET20
		public static TwoDVector<T> operator +(PolarVector<T> u, PolarVector<T> v)
		{
			return new TwoDVector<T>(u) + new TwoDVector<T>(v);
		}

		public static TwoDVector<T> operator -(PolarVector<T> u, PolarVector<T> v)
		{
			return new TwoDVector<T>(u) - new TwoDVector<T>(v);
		}

		public static TwoDVector<T> operator +(PolarVector<T> u, TwoDVector<T> v)
		{
			return new TwoDVector<T>(u) + v;
		}

		public static TwoDVector<T> operator -(PolarVector<T> u, TwoDVector<T> v)
		{
			return new TwoDVector<T>(u) - v;
		}

		public static T operator *(PolarVector<T> u, PolarVector<T> v)
		{
			return Vector<T>.Dot(u.Norm, v.Norm, AngleBetween(u, v));
		}
#endif
		#endregion

		public static Angle<T> AngleBetween(PolarVector<T> u, PolarVector<T> v)
		{
#if NET20
			return (v.Angle - u.Angle).Cast<T>();
#else
			return (v.Angle.Normalize() - u.Angle.Normalize()).Normalize().Cast<T>();
#endif
		}

		public static bool AreParallel(PolarVector<T> u, PolarVector<T> v)
		{
			return AngleBetween(u, v).Value.Equals(default(T));
		}

		public static bool ArePerpendicular(PolarVector<T> u, PolarVector<T> v)
		{
			return AngleBetween(u, v) == RightAngle;
		}

		private static readonly Angle<T> RightAngle = new Angle<T>(ExtraMath.ConvertTo<T>(90), AngleUnit.Degree);
	}
}
