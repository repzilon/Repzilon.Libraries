//
//  MicroscopeObjective.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024 René Rhéaume
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
	public struct MicroscopeObjective : IEquatable<MicroscopeObjective>, IFormattable
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	{
		public readonly float FocalLengthInMeters;
		public readonly float FocalNumber;

		public MicroscopeObjective(float focalLengthInMeters, float focalNumber)
		{
			if (focalLengthInMeters <= 0) {
				throw new ArgumentOutOfRangeException("focalLengthInMeters");
			}
			if (focalNumber <= 0) {
				throw new ArgumentOutOfRangeException("focalNumber");
			}
			FocalLengthInMeters = focalLengthInMeters;
			FocalNumber = focalNumber;
		}

		#region ICloneable members
		public MicroscopeObjective(MicroscopeObjective source) : this(source.FocalLengthInMeters, source.FocalNumber)
		{
		}

		public object Clone()
		{
			return new MicroscopeObjective(this);
		}
		#endregion

		#region Equals and GetHashCode
		public override bool Equals(object obj)
		{
			return obj is MicroscopeObjective && Equals((MicroscopeObjective)obj);
		}

		public bool Equals(MicroscopeObjective other)
		{
			return RoundOff.Equals(FocalLengthInMeters, other.FocalLengthInMeters) &&
				   RoundOff.Equals(FocalNumber, other.FocalNumber);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = (1696355206 * -1521134295) + FocalLengthInMeters.GetHashCode();
				return (hashCode * -1521134295) + FocalNumber.GetHashCode();
			}
		}

		public static bool operator ==(MicroscopeObjective left, MicroscopeObjective right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MicroscopeObjective left, MicroscopeObjective right)
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
			var stbLens = new StringBuilder();
			return stbLens.Append("ƒ=").Append(this.FocalLengthInMeters.ToString(format, formatProvider))
			 .Append(" m  ƒ/").Append(this.FocalNumber.ToString(format, formatProvider)).ToString();
		}
		#endregion

		public float Aperture()
		{
			return this.FocalLengthInMeters / this.FocalNumber;
		}
	}
}
