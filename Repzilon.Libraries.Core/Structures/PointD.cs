﻿//
//  PointD.cs
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

namespace Repzilon.Libraries.Core
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct PointD : IPoint<double>,
	IEquatable<PointD>, IEquatable<IPoint<double>>, IEquatable<PointM>, IEquatable<IPoint<decimal>>
	{
		public double X { get; private set; }
		public double Y { get; private set; }

		public PointD(double x, double y) : this()
		{
			X = x;
			Y = y;
		}

		#region ICloneable members
		public PointD(PointD source) : this(source.X, source.Y) { }

		public PointD Clone()
		{
			return new PointD(X, Y);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public PointM ToDecimal()
		{
			return new PointM((decimal)this.X, (decimal)this.Y);
		}

		#region Equals
		public override bool Equals(object obj)
		{
			if (obj is PointD) {
				return Equals((PointD)obj);
			} else if (obj is PointM) {
				return Equals((PointM)obj);
			} else if (obj is IPoint<double>) {
				return Equals((IPoint<double>)obj);
			} else if (obj is IPoint<decimal>) {
				return Equals((IPoint<decimal>)obj);
			} else {
				return false;
			}
		}

		public bool Equals(PointD other)
		{
			return RoundOff.Equals(X, other.X) && RoundOff.Equals(Y, other.Y);
		}

		public bool Equals(IPoint<double> other)
		{
			return (other != null) &&  RoundOff.Equals(X, other.X) && RoundOff.Equals(Y, other.Y);
		}

		public bool Equals(PointM other)
		{
			return ((decimal)X == other.X) && ((decimal)Y == other.Y);
		}

		public bool Equals(IPoint<decimal> other)
		{
			return (other != null) && ((decimal)X == other.X) && ((decimal)Y == other.Y);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (1861411795 * -1521134295) + X.GetHashCode();
				return (hashCode * -1521134295) + Y.GetHashCode();
			}
		}

		public static bool operator ==(PointD left, PointD right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PointD left, PointD right)
		{
			return !(left == right);
		}

		public static bool operator ==(PointD left, PointM right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PointD left, PointM right)
		{
			return !(left == right);
		}
		#endregion

		#region ToString
		public override string ToString()
		{
			return ToString(null, null);
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
			stbCoordinates.Append('{').Append(this.X.ToString(format, formatProvider)).Append("; ")
			 .Append(this.Y.ToString(format, formatProvider)).Append('}');
			return stbCoordinates.ToString();
		}
		#endregion

		public static PointD SemiLogX(double x, double y)
		{
			return new PointD(Math.Log10(x), y);
		}

		public static PointD SemiLogY(double x, double y)
		{
			return new PointD(x, Math.Log10(y));
		}

		public static PointD LogLog(double x, double y)
		{
			return new PointD(Math.Log10(x), Math.Log10(y));
		}
	}
}
