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
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	[StructLayout(LayoutKind.Auto)]
	public struct PointD : IEquatable<PointD>
	{
		public double X;
		public double Y;

		public PointD(double x, double y)
		{
			X = x;
			Y = y;
		}

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
			int hashCode = 1861411795;
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			return hashCode;
		}

		public override string ToString()
		{
			var stbCoord = new StringBuilder();
			stbCoord.Append('{').Append(this.X).Append("; ").Append(this.Y).Append('}');
			return stbCoord.ToString();
		}

		public static bool operator ==(PointD left, PointD right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PointD left, PointD right)
		{
			return !(left == right);
		}
	}
}
