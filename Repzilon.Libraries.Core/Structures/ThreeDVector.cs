//
//  ThreeDVector.cs
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

namespace Repzilon.Libraries.Core.Vectors
{
	[StructLayout(LayoutKind.Auto)]
	public struct ThreeDVector<T> : ICartesianVector<T>, IEquatable<ThreeDVector<T>>,
	IComparableThreeDVector, IEquatable<IComparableThreeDVector>
	where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
	{
		public T X { get; private set; }
		public T Y { get; private set; }
		public T Z { get; private set; }

		IComparable IComparableThreeDVector.X {
			get { return this.X; }
		}

		IComparable IComparableThreeDVector.Y {
			get { return this.Y; }
		}

		IComparable IComparableThreeDVector.Z {
			get { return this.Z; }
		}

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
			ValueType cz = this.Z;
			if (cx is decimal) {
				return Convert.ToDouble(ExtraMath.Hypoth((decimal)cx, (decimal)cy, (decimal)cz));
			} else {
				return ExtraMath.Hypoth(Convert.ToDouble(cx), Convert.ToDouble(cy), Convert.ToDouble(cz));
			}
		}

		public ThreeDVector<TOut> Cast<TOut>()
		where TOut : struct, IFormattable, IEquatable<TOut>, IComparable<TOut>, IComparable
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
			return obj is ThreeDVector<T> ? Equals((ThreeDVector<T>)obj) : Equals(obj as IComparableThreeDVector);
		}

		public bool Equals(ThreeDVector<T> other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		private bool Equals(IComparableThreeDVector other)
		{
			var typT = typeof(T);
			return (other != null) && (this.X.CompareTo(Convert.ChangeType(other.X, typT)) == 0) &&
			 (this.Y.CompareTo(Convert.ChangeType(other.Y, typT)) == 0) &&
			 (this.Z.CompareTo(Convert.ChangeType(other.Z, typT)) == 0);
		}

		bool IEquatable<IComparableThreeDVector>.Equals(IComparableThreeDVector other)
		{
			return this.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked {
				var magic = -1521134295;
				var hashCode = (-307843816 * -1521134295) + X.GetHashCode();
				hashCode = (hashCode * magic) + Y.GetHashCode();
				return (hashCode * magic) + Z.GetHashCode();
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
		public static ThreeDVector<T> operator +(ThreeDVector<T> u, ThreeDVector<T> v)
		{
			var addi = GenericArithmetic<T>.adder;
			return new ThreeDVector<T>(addi(u.X, v.X), addi(u.Y, v.Y), addi(u.Z, v.Z));
		}

		public static ThreeDVector<T> operator -(ThreeDVector<T> u, ThreeDVector<T> v)
		{
			var sub = GenericArithmetic<T>.sub;
			return new ThreeDVector<T>(sub(u.X, v.X), sub(u.Y, v.Y), sub(u.Z, v.Z));
		}

		public static ThreeDVector<T> operator *(T k, ThreeDVector<T> v)
		{
			return v.Multiply(k);
		}

		public static T operator *(ThreeDVector<T> u, ThreeDVector<T> v)
		{
			return Dot(u, v);
		}

		public static ThreeDVector<T> operator %(ThreeDVector<T> u, ThreeDVector<T> v)
		{
			return Cross(u, v);
		}

		public static T Dot(ThreeDVector<T> u, ThreeDVector<T> v)
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<T>();
			var addi = GenericArithmetic<T>.adder;
			return addi(addi(mult(u.X, v.X), mult(u.Y, v.Y)), mult(u.Z, v.Z));
		}

		public static ThreeDVector<T> Cross(ThreeDVector<T> u, ThreeDVector<T> v)
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<T>();
			var sub = GenericArithmetic<T>.sub;
			return new ThreeDVector<T>(
			 sub(mult(u.Y, v.Z), mult(u.Z, v.Y)),
			 sub(mult(u.Z, v.X), mult(u.X, v.Z)), // - (u1v3 - u3v1) = u3v1 - u1v3 [negation no longer needed]
			 sub(mult(u.X, v.Y), mult(u.Y, v.X)));
		}
		#endregion

		public static bool ArePerpendicular(ThreeDVector<T> u, ThreeDVector<T> v)
		{
			return Dot(u, v).Equals(default(T));
		}

		public static bool AreParallel(ThreeDVector<T> u, ThreeDVector<T> v)
		{
			var ux = u.X.ConvertTo<decimal>();
			var vx = v.X.ConvertTo<decimal>();
			decimal k;
			if (Math.Abs(vx) < Math.Abs(ux)) {
				k = ux / vx;
				return k * v.Cast<decimal>() == u.Cast<decimal>();
			} else {
				k = vx / ux;
				return k * u.Cast<decimal>() == v.Cast<decimal>();
			}
		}

		public static Angle<double> AngleBetween(ThreeDVector<T> u, ThreeDVector<T> v)
		{
			return new Angle<double>(Math.Acos(Dot(u, v).ConvertTo<double>() / (u.Norm() * v.Norm())), AngleUnit.Radian);
		}
	}

	public static class ThreeDVectorExtensions
	{
		public static ThreeDVector<T> Multiply<T, TScalar>(this ThreeDVector<T> v, TScalar k)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
		where TScalar : struct, IEquatable<TScalar>
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<TScalar>();
			return new ThreeDVector<T>(mult(k, v.X), mult(k, v.Y), mult(k, v.Z));
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
