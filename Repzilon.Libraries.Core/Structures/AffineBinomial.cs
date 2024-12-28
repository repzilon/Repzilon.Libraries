//
//  AffineBinomial.cs
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
using System.Text;

namespace Repzilon.Libraries.Core
{
	public struct AffineBinomial<T> : IEquatable<AffineBinomial<T>>, IFormattable
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
	{
		public readonly T Slope;
		public readonly char Variable;
		public readonly T Constant;

		public AffineBinomial(T slope, char variable, T constant)
		{
			Slope = slope;
			Variable = variable;
			Constant = constant;
		}

		public AffineBinomial(T constant)
		{
			Slope = default(T);
			Variable = 'x';
			Constant = constant;
		}

		public AffineBinomial(AffineBinomial<T> source) : this(source.Slope, source.Variable, source.Constant)
		{
		}

		#region ICloneable members
		public AffineBinomial<T> Clone()
		{
			return new AffineBinomial<T>(this);
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
			return obj is AffineBinomial<T> && Equals((AffineBinomial<T>)obj);
		}

		public bool Equals(AffineBinomial<T> other)
		{
			var eq = EqualityComparer<T>.Default;
			return eq.Equals(Slope, other.Slope) && (Variable == other.Variable) && eq.Equals(Constant, other.Constant);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = 725852250;
				hashCode = hashCode * -1521134295 + Slope.GetHashCode();
				hashCode = hashCode * -1521134295 + Variable.GetHashCode();
				hashCode = hashCode * -1521134295 + Constant.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(AffineBinomial<T> left, AffineBinomial<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AffineBinomial<T> left, AffineBinomial<T> right)
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
			var stbDesc = new StringBuilder();
			if (!Slope.Equals(default(T))) {
				stbDesc.Append(Slope.ToString(format, formatProvider)).Append(Variable);
				if (!Constant.Equals(default(T))) {
					stbDesc.Append(' ');
					if (Constant.CompareTo(default(T)) > 0) {
						stbDesc.Append('+');
					}
					AppendConstant(stbDesc, format, formatProvider);
				}
			} else {
				AppendConstant(stbDesc, format, formatProvider);
			}

			return stbDesc.ToString();
		}

		private void AppendConstant(StringBuilder destination, string format, IFormatProvider formatProvider)
		{
			destination.Append(Constant.ToString(format, formatProvider));
		}
		#endregion
	}

#if !NET20
	internal static class AffineBinomialExtensions
	{
		internal static void AddConstant<K, T>(this IDictionary<K, AffineBinomial<T>> dictionary, K key, double value)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			dictionary.Add(key, new AffineBinomial<T>(value.ConvertTo<T>()));
		}
	}
#endif
}
