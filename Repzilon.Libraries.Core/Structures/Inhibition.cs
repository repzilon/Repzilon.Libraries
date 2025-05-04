//
//  Inhibition.cs
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core.Biochemistry
{
	public enum InhibitionKind : byte
	{
		Absent,
		NonCompetitive,
		Uncompetitive,
		Competitive,
		Mixed
	}

#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct Inhibition<T> : IEquatable<Inhibition<T>>, IFormattable
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
	{
		public readonly T Value;
		public readonly string Unit;
		public readonly InhibitionKind Kind;

		public Inhibition(InhibitionKind kind)
		{
			ValidateKind(kind);
			this.Kind = kind;
			this.Value = default(T);
			this.Unit = "";
		}

		public Inhibition(InhibitionKind kind, T value, string unit)
		{
			ValidateKind(kind);
			if (value.CompareTo(default(T)) < 0) {
				throw new ArgumentOutOfRangeException("value", value, "Inhibition constant cannot be negative.");
			}
			this.Kind = kind;
			this.Value = value;
			this.Unit = unit;
		}

		#region ICloneable members
		public Inhibition(Inhibition<T> source) : this(source.Kind, source.Value, source.Unit)
		{
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif

		public Inhibition<T> Clone()
		{
			return new Inhibition<T>(this);
		}
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is Inhibition<T> && Equals((Inhibition<T>)obj);
		}

		public bool Equals(Inhibition<T> other)
		{
			return this.Value.Equals(other.Value) && (Kind == other.Kind);
		}

		public override int GetHashCode()
		{
			unchecked {
				var magic = -1521134295;
				var hashCode = -864145315 * -1521134295 + Value.GetHashCode();
				hashCode = (hashCode * magic) + (int)Kind;
				return (hashCode * magic) + Unit.GetHashCode();
			}
		}

		public static bool operator ==(Inhibition<T> left, Inhibition<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Inhibition<T> left, Inhibition<T> right)
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
			var stbOut = new StringBuilder();
			stbOut.Append("Inhibition: ");
			stbOut.Append(this.Kind.ToString().ToLower(formatProvider as CultureInfo ?? CultureInfo.CurrentCulture));
			if (!this.Value.Equals(default(T))) {
				stbOut.Append(" Ki=").Append(this.Value.ToString(format, formatProvider));
				stbOut.Append(' ').Append(this.Unit);
			}
			return stbOut.ToString();
		}
		#endregion

		public Inhibition<TOut> Cast<TOut>()
		where TOut : struct, IComparable<TOut>, IFormattable, IEquatable<TOut>, IComparable
		{
			return new Inhibition<TOut>(this.Kind, ExtraMath.ConvertTo<TOut>(this.Value), this.Unit);
		}

		private static void ValidateKind(InhibitionKind kind)
		{
			if (kind > InhibitionKind.Mixed) {
				throw RetroCompat.NewUndefinedEnumException("kind", kind);
			}
		}
	}

	public static class InhibitionExtensions
	{
#if NET20
		public static Inhibition<float> RoundedToPrecision(Inhibition<float> self, byte significantDigits)
#else
		public static Inhibition<float> RoundedToPrecision(this Inhibition<float> self, byte significantDigits)
#endif
		{
			return new Inhibition<float>(self.Kind,
			 SignificantDigits.Round(self.Value, significantDigits, RoundingMode.ToEven), self.Unit);
		}

#if NET20
		public static Inhibition<double> RoundedToPrecision(Inhibition<double> self, byte significantDigits)
#else
		public static Inhibition<double> RoundedToPrecision(this Inhibition<double> self, byte significantDigits)
#endif
		{
			return new Inhibition<double>(self.Kind,
			 SignificantDigits.Round(self.Value, significantDigits, RoundingMode.ToEven), self.Unit);
		}

#if NET20
		public static Inhibition<decimal> RoundedToPrecision(Inhibition<decimal> self, byte significantDigits)
#else
		public static Inhibition<decimal> RoundedToPrecision(this Inhibition<decimal> self, byte significantDigits)
#endif
		{
			return new Inhibition<decimal>(self.Kind,
			 SignificantDigits.Round(self.Value, significantDigits, RoundingMode.ToEven), self.Unit);
		}
	}
}
