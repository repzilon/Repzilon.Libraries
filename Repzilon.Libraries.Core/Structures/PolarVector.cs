//
//  PolarVector.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023 René Rhéaume
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

namespace Repzilon.Libraries.Core
{
	[StructLayout(LayoutKind.Auto)]
	public struct PolarVector<T> : IFormattable, IEquatable<PolarVector<T>>
#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
	, ICloneable
#endif
	where T : struct, IFormattable, IEquatable<T>, IComparable<T>
#if (!NETSTANDARD1_1)
	, IConvertible
#endif
	{
		public readonly T Norm;
		public readonly Angle<T> Angle;

		public PolarVector(T norm, Angle<T> angle)
		{
			Norm = norm;
			Angle = angle.Normalize();
		}

		public PolarVector(T norm, T angle, AngleUnit unit) : this(norm, new Angle<T>(angle, unit))
		{
		}

		public PolarVector(T norm, Angle<T> angle, AngleUnit newUnit) : this(norm, angle.ConvertTo<T>(newUnit, true))
		{
		}

		#region ICloneable members
		public PolarVector<T> Clone()
		{
			return new PolarVector<T>(Norm, Angle);
		}

#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public PolarVector<TOut> Cast<TOut>()
		where TOut : struct, IFormattable, IEquatable<TOut>, IComparable<TOut>
#if (!NETSTANDARD1_1)
	, IConvertible
#endif
		{
			var a = this.Angle;
			return new PolarVector<TOut>(this.Norm.ConvertTo<TOut>(), a.Value.ConvertTo<TOut>(), a.Unit);
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
			return obj is PolarVector<T> && Equals((PolarVector<T>)obj);
		}

		public bool Equals(PolarVector<T> other)
		{
			return Norm.Equals(other.Norm) && Angle.Equals(other.Angle);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = (1227039071 * -1521134295) + ((IComparable<T>)Norm).GetHashCode();
				return hashCode * -1521134295 + ((IComparable)Angle).GetHashCode();
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
			stbVector.Append(this.Norm.ToString(format, formatProvider));
			stbVector.Append('∠').Append(this.Angle.ToString(format, formatProvider));
			return stbVector.ToString();
		}
		#endregion

		#region Operators
		public static TwoDVector<T> operator +(PolarVector<T> a, PolarVector<T> b)
		{
			return new TwoDVector<T>(a) + new TwoDVector<T>(b);
		}

		public static TwoDVector<T> operator -(PolarVector<T> a, PolarVector<T> b)
		{
			return new TwoDVector<T>(a) - new TwoDVector<T>(b);
		}

		public static TwoDVector<T> operator +(PolarVector<T> a, TwoDVector<T> b)
		{
			return new TwoDVector<T>(a) + b;
		}

		public static TwoDVector<T> operator -(PolarVector<T> a, TwoDVector<T> b)
		{
			return new TwoDVector<T>(a) - b;
		}
		#endregion
	}
}
