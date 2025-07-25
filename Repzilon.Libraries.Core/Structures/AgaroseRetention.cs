//
//  AgaroseRetention.cs
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
using System.Collections.Generic;
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
	public struct AgaroseRetention : IEquatable<AgaroseRetention>, IFormattable
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	{
		public readonly float LowerMassVolumeConcentration;
		public readonly float UpperMassVolumeConcentration;
		public readonly short[] FragmentLengths;

		public AgaroseRetention(float minConcentration, params short[] fragmentLengths)
		{
			this.UpperMassVolumeConcentration = Single.NaN;
			this.LowerMassVolumeConcentration = minConcentration;
			this.FragmentLengths = fragmentLengths;
		}

		public AgaroseRetention(float minConcentration, float maxConcentration, params short[] fragmentLengths)
		{
			this.UpperMassVolumeConcentration = maxConcentration;
			this.LowerMassVolumeConcentration = minConcentration;
			this.FragmentLengths = fragmentLengths;
		}

		public AgaroseRetention(AgaroseRetention old, float newMaxConcentration)
		{
			this.UpperMassVolumeConcentration = newMaxConcentration;
			this.LowerMassVolumeConcentration = old.LowerMassVolumeConcentration;
			this.FragmentLengths = old.FragmentLengths;
		}

		#region Equals and GetHashCode
		public override bool Equals(object obj)
		{
			return obj is AgaroseRetention && Equals((AgaroseRetention)obj);
		}

		public bool Equals(AgaroseRetention other)
		{
			return RoundOff.AreEqual(LowerMassVolumeConcentration, other.LowerMassVolumeConcentration) &&
				   RoundOff.AreEqual(UpperMassVolumeConcentration, other.UpperMassVolumeConcentration) &&
				   EqualityComparer<short[]>.Default.Equals(FragmentLengths, other.FragmentLengths);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (-20320107 * -1521134295) + LowerMassVolumeConcentration.GetHashCode();
				hashCode = (hashCode * -1521134295) + UpperMassVolumeConcentration.GetHashCode();
				return (hashCode * -1521134295) + EqualityComparer<short[]>.Default.GetHashCode(FragmentLengths);
			}
		}

		public static bool operator ==(AgaroseRetention left, AgaroseRetention right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AgaroseRetention left, AgaroseRetention right)
		{
			return !(left == right);
		}
		#endregion

		#region ICloneable members
		public AgaroseRetention Clone()
		{
			var c = this.FragmentLengths.Length;
			var shrarCopy = new short[c];
			for (var i = 0; i < c; i++) {
				shrarCopy[i] = this.FragmentLengths[i];
			}
			return new AgaroseRetention(this.LowerMassVolumeConcentration, this.UpperMassVolumeConcentration, shrarCopy);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

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

			var stbAgarose = new StringBuilder();
			stbAgarose.Append(this.LowerMassVolumeConcentration.ToString(format, formatProvider));
			if (!Double.IsNaN(this.UpperMassVolumeConcentration)) {
				stbAgarose.Append(" to ").Append(this.UpperMassVolumeConcentration.ToString(format, formatProvider));
			}
			stbAgarose.Append(" % m/v agarose will migrate fragment lengths ");
			var c = this.FragmentLengths.Length;
			for (var i = 0; i < c; i++) {
				if (i > 0) {
					stbAgarose.Append(", ");
				}
				stbAgarose.Append(this.FragmentLengths[i].ToString(format, formatProvider));
			}
			return stbAgarose.ToString();
		}
	}
}
