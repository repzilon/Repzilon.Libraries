//
//  TwoXPoint.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2025 René Rhéaume
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

namespace Repzilon.Libraries.Core.Regression
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct TwoXPoint<T> : IEquatable<TwoXPoint<T>>, IFormattable
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IEquatable<T>
	{
		// Y and all X must have the same data type because of the multiplication operator
		public readonly T Y;
		public readonly T X1;
		public readonly T X2;

		public TwoXPoint(T x1, T x2, T y)
		{
			this.X1 = x1;
			this.X2 = x2;
			this.Y = y;
		}

		#region ICloneable members
		public TwoXPoint(TwoXPoint<T> source) : this(source.X1, source.X2, source.Y) { }

		public TwoXPoint<T> Clone()
		{
			return new TwoXPoint<T>(X1, X2, Y);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is TwoXPoint<T> point && Equals(point);
		}

		public bool Equals(TwoXPoint<T> other)
		{
			var eq = EqualityComparer<T>.Default;
			return eq.Equals(Y, other.Y) && eq.Equals(X1, other.X1) && eq.Equals(X2, other.X2);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = 2077371093;
				hashCode = hashCode * -1521134295 + Y.GetHashCode();
				hashCode = hashCode * -1521134295 + X1.GetHashCode();
				hashCode = hashCode * -1521134295 + X2.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(TwoXPoint<T> left, TwoXPoint<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TwoXPoint<T> left, TwoXPoint<T> right)
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
			var stbCoordinates = new StringBuilder();
			stbCoordinates.Append("{{").Append(this.X1.ToString(format, formatProvider)).Append("; ")
			 .Append(this.X2.ToString(format, formatProvider)).Append("}; ")
			 .Append(this.Y.ToString(format, formatProvider)).Append("}");
			return stbCoordinates.ToString();
		}
		#endregion

		#region Regression-related methods
		internal T X1Squared()
		{
			return Mul(X1, X1);
		}

		internal T X2Squared()
		{
			return Mul(X2, X2);
		}

		internal T X1X2()
		{
			return Mul(X1, X2);
		}

		private static T Mul(T x1, T x2)
		{
#if NET20
			return Arithmetic<T>.MultiplyScalars(x1, x2);
#else
			return Arithmetic<T>.MulT(x1, x2);
#endif
		}

		internal T X1Y()
		{
			return Mul(X1, Y);
		}

		internal T X2Y()
		{
			return Mul(X2, Y);
		}
		#endregion

		public TwoXPoint<TOut> ConvertTo<TOut>() where TOut: struct, IFormattable, IEquatable<TOut>
		{
			return new TwoXPoint<TOut>(ExtraMath.ConvertTo<TOut>(X1), ExtraMath.ConvertTo<TOut>(X2),
			 ExtraMath.ConvertTo<TOut>(Y));
		}
	}
}
