//
//  ThreeDVector.cs
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
	public struct ThreeDVector<T> : ICartesianVector<T>, IEquatable<ThreeDVector<T>>
	where T : struct, IFormattable, IEquatable<T>, IComparable<T>
#if (!NETSTANDARD1_1)
	, IConvertible
#endif
	{
		public T X { get; private set; }
		public T Y { get; private set; }
		public T Z { get; private set; }

		public ThreeDVector(T x, T y, T z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		#region ICloneable members
		public ThreeDVector(ThreeDVector<T> source) : this(source.X, source.Y, source.Z) { }

		public ThreeDVector<T> Clone()
		{
			return new ThreeDVector<T>(X, Y, Z);
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
#if (NETSTANDARD1_1)
			object cx = this.X;
			object cy = this.Y;
			object cz = this.Z;
			if (cx is decimal) {
#else
			IConvertible cx = this.X;
			IConvertible cy = this.Y;
			IConvertible cz = this.Z;
			if (cx.GetTypeCode() == TypeCode.Decimal) {
#endif
				return Convert.ToDouble(ExtraMath.Hypoth((decimal)cx, (decimal)cy, (decimal)cz));
			} else {
				return ExtraMath.Hypoth(Convert.ToDouble(cx), Convert.ToDouble(cy), Convert.ToDouble(cz));
			}
		}

		public ThreeDVector<TOut> Cast<TOut>()
		where TOut : struct, IFormattable, IEquatable<TOut>, IComparable<TOut>
#if (!NETSTANDARD1_1)
	, IConvertible
#endif
		{
			return new ThreeDVector<TOut>(X.ConvertTo<TOut>(), Y.ConvertTo<TOut>(), Z.ConvertTo<TOut>());
		}

		ICartesianVector<TOut> ICartesianVector<T>.Cast<TOut>()
		{
			return this.Cast<TOut>();
		}

		ICartesianVector<T> ICartesianVector<T>.ToUnitary()
		{
			return this.ToUnitary();
		}

		public ThreeDVector<T> ToUnitary()
		{
			var f = 1.0 / this.Norm();
			return new ThreeDVector<T>(
			 (f * Convert.ToDouble(X)).ConvertTo<T>(),
			 (f * Convert.ToDouble(Y)).ConvertTo<T>(),
			 (f * Convert.ToDouble(Z)).ConvertTo<T>());
		}
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is ThreeDVector<T> && Equals((ThreeDVector<T>)obj);
		}

		public bool Equals(ThreeDVector<T> other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = -307843816;
				hashCode = hashCode * -1521134295 + X.GetHashCode();
				hashCode = hashCode * -1521134295 + Y.GetHashCode();
				hashCode = hashCode * -1521134295 + Z.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(ThreeDVector<T> left, ThreeDVector<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ThreeDVector<T> left, ThreeDVector<T> right)
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
			stbVector.Append(Y.ToString(format, formatProvider)).Append("; ");
			stbVector.Append(Z.ToString(format, formatProvider)).Append(')');
			return stbVector.ToString();
		}
		#endregion

		#region Operators
		public static ThreeDVector<T> operator +(ThreeDVector<T> a, ThreeDVector<T> b)
		{
			return new ThreeDVector<T>(Matrix<T>.add(a.X, b.X), Matrix<T>.add(a.Y, b.Y), Matrix<T>.add(a.Z, b.Z));
		}

		public static ThreeDVector<T> operator -(ThreeDVector<T> a, ThreeDVector<T> b)
		{
			return new ThreeDVector<T>(Matrix<T>.sub(a.X, b.X), Matrix<T>.sub(a.Y, b.Y), Matrix<T>.sub(a.Z, b.Z));
		}

		public static ThreeDVector<T> operator *(T k, ThreeDVector<T> v)
		{
			return ThreeDVectorExtensions.Multiply(v, k);
		}

		// TODO : scalar product operator
		// TODO : vector product operator
		// TODO : implement operators between TwoDVector and PolarVector
		#endregion
	}

	public static class ThreeDVectorExtensions
	{
		public static ThreeDVector<T> Multiply<T, TScalar>(this ThreeDVector<T> v, TScalar k)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>
#if (!NETSTANDARD1_1)
	, IConvertible
#endif
		where TScalar : struct, IEquatable<TScalar>
		{
			var mul = Matrix<T>.BuildMultiplier<TScalar>();
			return new ThreeDVector<T>(mul(k, v.X), mul(k, v.Y), mul(k, v.Z));
		}

		public static ThreeDVector<float> RoundError(this ThreeDVector<float> v)
		{
			return new ThreeDVector<float>(RoundOff.Error(v.X), RoundOff.Error(v.Y), RoundOff.Error(v.Z));
		}

		public static ThreeDVector<double> RoundError(this ThreeDVector<double> v)
		{
			return new ThreeDVector<double>(RoundOff.Error(v.X), RoundOff.Error(v.Y), RoundOff.Error(v.Z));
		}

		public static ThreeDVector<decimal> RoundError(this ThreeDVector<decimal> v)
		{
			return new ThreeDVector<decimal>(RoundOff.Error(v.X), RoundOff.Error(v.Y), RoundOff.Error(v.Z));
		}
	}
}
