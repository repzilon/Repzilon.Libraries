//
//  PointM.cs
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
	public struct PointM : IPoint<decimal>,
	IEquatable<PointD>, IEquatable<IPoint<double>>, IEquatable<PointM>, IEquatable<IPoint<decimal>>,
	IFormattable
	{
		public decimal X { get; private set; }
		public decimal Y { get; private set; }

		public PointM(decimal x, decimal y)
		{
			X = x;
			Y = y;
		}

		#region ICloneable members
		public PointM(PointM source) : this(source.X, source.Y) { }

		public PointM Clone()
		{
			return new PointM(X, Y);
		}

#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public PointD ToDouble()
		{
			return new PointD((double)this.X, (double)this.Y);
		}

		#region Equals
		public override bool Equals(object obj)
		{
			if (obj is PointM) {
				return Equals((PointM)obj);
			} else if (obj is PointD) {
				return Equals((PointD)obj);
			} else if (obj is IPoint<double>) {
				return Equals((IPoint<double>)obj);
			} else if (obj is IPoint<decimal>) {
				return Equals((IPoint<decimal>)obj);
			} else {
				return false;
			}
		}

		public bool Equals(PointM other)
		{
			return (X == other.X) && (Y == other.Y);
		}

		public bool Equals(IPoint<double> other)
		{
			return (other != null) && (X == (decimal)other.X) && (Y == (decimal)other.Y);
		}

		public bool Equals(PointD other)
		{
			return (X == (decimal)other.X) && (Y == (decimal)other.Y);
		}

		public bool Equals(IPoint<decimal> other)
		{
			return (other != null) && (X == other.X) && (Y == other.Y);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = 1861411795 * -1521134295 + X.GetHashCode();
				return hashCode * -1521134295 + Y.GetHashCode();
			}
		}	

		public static bool operator ==(PointM left, PointM right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PointM left, PointM right)
		{
			return !(left == right);
		}

		public static bool operator ==(PointM left, PointD right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PointM left, PointD right)
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
			var stbCoord = new StringBuilder();
			stbCoord.Append('{').Append(this.X.ToString(format, formatProvider)).Append("; ")
			 .Append(this.Y.ToString(format, formatProvider)).Append('}');
			return stbCoord.ToString();
		}
		#endregion
	}
}
