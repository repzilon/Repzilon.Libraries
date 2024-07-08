//
//  EnzymeKinematic.cs
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
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	public enum EnzymeSpeedRepresentation : byte
	{
		MichaelisMenten,
		LineweaverBurk,
		EadieHofstee,
		HanesWoolf
	}

	[StructLayout(LayoutKind.Auto)]
	public struct EnzymeKinematic<T> : IEquatable<EnzymeKinematic<T>>, IFormattable,
	IComparableEnzymeKinematic, IEquatable<IComparableEnzymeKinematic>
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
	{
		public readonly KeyValuePair<T, string> Vmax;
		public readonly KeyValuePair<T, string> Km;
		public readonly T Correlation;

		IComparable IComparableEnzymeKinematic.VmaxNumber
		{
			get { return Vmax.Key; }
		}

		string IComparableEnzymeKinematic.VmaxUnit
		{
			get { return Vmax.Value; }
		}

		IComparable IComparableEnzymeKinematic.KmNumber
		{
			get { return Km.Key; }
		}

		string IComparableEnzymeKinematic.KmUnit
		{
			get { return Km.Value; }
		}

		IComparable IComparableEnzymeKinematic.Correlation
		{
			get { return Correlation; }
		}

		public EnzymeKinematic(KeyValuePair<T, string> vmax, KeyValuePair<T, string> km, T correlation)
		{
			Vmax = vmax;
			Km = km;
			Correlation = correlation;
		}

		public EnzymeKinematic(T vmaxValue, string vmaxUnit, T kmValue, string kmUnit, T correlation)
		{
			Vmax = new KeyValuePair<T, string>(vmaxValue, vmaxUnit);
			Km = new KeyValuePair<T, string>(kmValue, kmUnit);
			Correlation = correlation;
		}

		public EnzymeKinematic(EnzymeKinematic<T> source) : this(source.Vmax, source.Km, source.Correlation)
		{
		}

		public EnzymeKinematic<T> Clone()
		{
			return new EnzymeKinematic<T>(this);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif

		public bool Equals(EnzymeKinematic<T> other)
		{
			var eqcKVP = EqualityComparer<KeyValuePair<T, string>>.Default;
			return eqcKVP.Equals(Vmax, other.Vmax) && eqcKVP.Equals(Km, other.Km) &&
			 EqualityComparer<T>.Default.Equals(Correlation, other.Correlation);
		}

		private bool Equals(IComparableEnzymeKinematic other)
		{
			if (other == null) {
				return false;
			} else {
				var kvpVmax = this.Vmax;
				var kvpKm = this.Km;
				return ((IComparable)kvpVmax.Key == other.VmaxNumber) && (kvpVmax.Value == other.VmaxUnit) &&
				 ((IComparable)kvpKm.Key == other.KmNumber) && (kvpKm.Value == other.KmUnit) &&
				 ((IComparable)this.Correlation == other.Correlation);
			}
		}

		bool IEquatable<IComparableEnzymeKinematic>.Equals(IComparableEnzymeKinematic other)
		{
			return this.Equals(other);
		}

		public override bool Equals(object obj)
		{
			return obj is EnzymeKinematic<T> ? Equals((EnzymeKinematic<T>)obj) : Equals(obj as IComparableEnzymeKinematic);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = 667060969;
				hashCode = hashCode * -1521134295 + Vmax.GetHashCode();
				hashCode = hashCode * -1521134295 + Km.GetHashCode();
				hashCode = hashCode * -1521134295 + Correlation.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(EnzymeKinematic<T> left, EnzymeKinematic<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(EnzymeKinematic<T> left, EnzymeKinematic<T> right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			var kvpVmax = this.Vmax;
			var kvpKm = this.Km;
			return String.Format("v<sub>max</sub>:\xA0{0}\xA0{1}; k<sub>m</sub>:\xA0{2}\xA0{3} (r={4:f6})",
			 kvpVmax.Key, kvpVmax.Value, kvpKm.Key, kvpKm.Value, Correlation);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			var stbAsString = new StringBuilder();
			AppendMeasure(stbAsString.Append("v<sub>max</sub>:\xA0"), Vmax, format, formatProvider);
			AppendMeasure(stbAsString.Append("; k<sub>m</sub>:\xA0"), Km, format, formatProvider);
			return stbAsString.Append(" (r=").Append(Correlation.ToString(format, formatProvider)).Append(')').ToString();
		}

		private static void AppendMeasure(StringBuilder buffer, KeyValuePair<T, string> measure, string format,
		IFormatProvider formatProvider)
		{
			buffer.Append(measure.Key.ToString(format, formatProvider)).Append(' ').Append(measure.Value);
		}
	}

	public static class EnzymeKinematicExtension
	{
#if NET20
		public static EnzymeKinematic<float> RoundedToPrecision(EnzymeKinematic<float> self, byte significantDigits)
#else
		public static EnzymeKinematic<float> RoundedToPrecision(this EnzymeKinematic<float> self, byte significantDigits)
#endif
		{
			return RoundedToPrecision(self, significantDigits, significantDigits);
		}

#if NET20
		public static EnzymeKinematic<float> RoundedToPrecision(EnzymeKinematic<float> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<float> RoundedToPrecision(this EnzymeKinematic<float> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<float>(
			 SignificantDigits.Round(kvpVmax.Key, forSpeed, RoundingMode.ToEven), kvpVmax.Value,
			 SignificantDigits.Round(kvpKm.Key, forConcentration, RoundingMode.ToEven), kvpKm.Value, self.Correlation);
		}

#if NET20
		public static EnzymeKinematic<double> RoundedToPrecision(EnzymeKinematic<double> self, byte significantDigits)
#else
		public static EnzymeKinematic<double> RoundedToPrecision(this EnzymeKinematic<double> self, byte significantDigits)
#endif
		{
			return RoundedToPrecision(self, significantDigits, significantDigits);
		}

#if NET20
		public static EnzymeKinematic<double> RoundedToPrecision(EnzymeKinematic<double> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<double> RoundedToPrecision(this EnzymeKinematic<double> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<double>(
			 SignificantDigits.Round(kvpVmax.Key, forSpeed, RoundingMode.ToEven), kvpVmax.Value,
			 SignificantDigits.Round(kvpKm.Key, forConcentration, RoundingMode.ToEven), kvpKm.Value, self.Correlation);
		}

#if NET20
		public static EnzymeKinematic<decimal> RoundedToPrecision(EnzymeKinematic<decimal> self, byte significantDigits)
#else
		public static EnzymeKinematic<decimal> RoundedToPrecision(this EnzymeKinematic<decimal> self, byte significantDigits)
#endif
		{
			return RoundedToPrecision(self, significantDigits, significantDigits);
		}

#if NET20
		public static EnzymeKinematic<decimal> RoundedToPrecision(EnzymeKinematic<decimal> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<decimal> RoundedToPrecision(this EnzymeKinematic<decimal> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<decimal>(
			 SignificantDigits.Round(kvpVmax.Key, forSpeed, RoundingMode.ToEven), kvpVmax.Value,
			 SignificantDigits.Round(kvpKm.Key, forConcentration, RoundingMode.ToEven), kvpKm.Value, self.Correlation);
		}

#if NET20
		public static EnzymeKinematic<float> RoundedToDecimals(EnzymeKinematic<float> self, byte decimals)
#else
		public static EnzymeKinematic<float> RoundedToDecimals(this EnzymeKinematic<float> self, byte decimals)
#endif
		{
			return RoundedToDecimals(self, decimals, decimals);
		}

#if NET20
		public static EnzymeKinematic<float> RoundedToDecimals(EnzymeKinematic<float> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<float> RoundedToDecimals(this EnzymeKinematic<float> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<float>(
			 (float)Math.Round(kvpVmax.Key, forSpeed, MidpointRounding.ToEven), kvpVmax.Value,
			 (float)Math.Round(kvpKm.Key, forConcentration, MidpointRounding.ToEven), kvpKm.Value, self.Correlation);
		}

#if NET20
		public static EnzymeKinematic<double> RoundedToDecimals(EnzymeKinematic<double> self, byte decimals)
#else
		public static EnzymeKinematic<double> RoundedToDecimals(this EnzymeKinematic<double> self, byte decimals)
#endif
		{
			return RoundedToDecimals(self, decimals, decimals);
		}

#if NET20
		public static EnzymeKinematic<double> RoundedToDecimals(EnzymeKinematic<double> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<double> RoundedToDecimals(this EnzymeKinematic<double> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<double>(
			 Math.Round(kvpVmax.Key, forSpeed, MidpointRounding.ToEven), kvpVmax.Value,
			 Math.Round(kvpKm.Key, forConcentration, MidpointRounding.ToEven), kvpKm.Value, self.Correlation);
		}

#if NET20
		public static EnzymeKinematic<decimal> RoundedToDecimals(EnzymeKinematic<decimal> self, byte decimals)
#else
		public static EnzymeKinematic<decimal> RoundedToDecimals(this EnzymeKinematic<decimal> self, byte decimals)
#endif
		{
			return RoundedToDecimals(self, decimals, decimals);
		}

#if NET20
		public static EnzymeKinematic<decimal> RoundedToDecimals(EnzymeKinematic<decimal> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<decimal> RoundedToDecimals(this EnzymeKinematic<decimal> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<decimal>(
			 Math.Round(kvpVmax.Key, forSpeed, MidpointRounding.ToEven), kvpVmax.Value,
			 Math.Round(kvpKm.Key, forConcentration, MidpointRounding.ToEven), kvpKm.Value, self.Correlation);
		}
	}
}
