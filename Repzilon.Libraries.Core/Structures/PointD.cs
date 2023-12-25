//
//  PointD.cs
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
	public struct PointD : IEquatable<PointD>, IFormattable, IPoint<double>
	{
		public double X { get; private set; }
		public double Y { get; private set; }

		public PointD(double x, double y)
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

#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is PointD && Equals((PointD)obj);
		}

		public bool Equals(PointD other)
		{
			return X == other.X && Y == other.Y;
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = 1861411795;
				hashCode = hashCode * -1521134295 + X.GetHashCode();
				hashCode = hashCode * -1521134295 + Y.GetHashCode();
				return hashCode;
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
		#endregion

		#region ToString
		public override string ToString()
		{
			return this.ToString(null, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrWhiteSpace(format))
			{
				format = "G";
			}
			if (formatProvider == null)
			{
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
