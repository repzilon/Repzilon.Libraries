//
//  AffineBinomial.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024-2025 René Rhéaume
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
				var hashCode = 725852250;
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
				if (!Slope.Equals(ExtraMath.ConvertTo<T>(1))) {
					stbDesc.Append(Slope.ToString(format, formatProvider));
				}
				stbDesc.Append(Variable);
				if (!Constant.Equals(default(T))) {
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

		#region Operators
		public static AffineBinomial<T> operator +(AffineBinomial<T> binomial, T scalar)
		{
			return new AffineBinomial<T>(binomial.Slope, binomial.Variable,
			 GenericArithmetic<T>.AddScalars(binomial.Constant, scalar));
		}

		public static AffineBinomial<T> operator +(AffineBinomial<T> first, AffineBinomial<T> second)
		{
			if (second.Variable == first.Variable) {
#if NET20
				return new AffineBinomial<T>(GenericArithmetic<T>.AddScalars(first.Slope, second.Slope),
				 first.Variable, GenericArithmetic<T>.AddScalars(first.Constant, second.Constant));
#else
				var add = GenericArithmetic<T>.Adder;
				return new AffineBinomial<T>(add(first.Slope, second.Slope), first.Variable,
				 add(first.Constant, second.Constant));
#endif
			} else {
				throw new InvalidOperationException(String.Format(
				 "Variables {0} and {1} cannot be added into the same affine binomial", first.Variable, second.Variable));
			}
		}

		public static AffineBinomial<T> operator -(AffineBinomial<T> first, AffineBinomial<T> second)
		{
			if (second.Variable == first.Variable) {
#if NET20
				return new AffineBinomial<T>(GenericArithmetic<T>.SubtractScalars(first.Slope, second.Slope),
				 first.Variable, GenericArithmetic<T>.SubtractScalars(first.Constant, second.Constant));
#else
				var sub = GenericArithmetic<T>.Sub;
				return new AffineBinomial<T>(sub(first.Slope, second.Slope), first.Variable,
				 sub(first.Constant, second.Constant));
#endif
			} else {
				throw new InvalidOperationException(String.Format(
				 "Variables {0} and {1} cannot be added into the same affine binomial", first.Variable, second.Variable));
			}
		}

		public static AffineBinomial<T> operator -(AffineBinomial<T> binomial, T scalar)
		{
			return new AffineBinomial<T>(binomial.Slope, binomial.Variable,
			 GenericArithmetic<T>.SubtractScalars(binomial.Constant, scalar));
		}

		public static AffineBinomial<T> operator *(AffineBinomial<T> binomial, T scalar)
		{
#if NET20
			return new AffineBinomial<T>(GenericArithmetic<T>.MultiplyScalars(binomial.Slope, scalar),
			 binomial.Variable, GenericArithmetic<T>.MultiplyScalars(binomial.Constant, scalar));
#else
			var mul = GenericArithmetic<T>.MulT;
			return new AffineBinomial<T>(mul(binomial.Slope, scalar), binomial.Variable, mul(binomial.Constant, scalar));
#endif
		}

		public static AffineBinomial<double> operator /(AffineBinomial<T> binomial, T scalar)
		{
			var dblScalar = Convert.ToDouble(scalar);
			return new AffineBinomial<double>(Convert.ToDouble(binomial.Slope) / dblScalar, binomial.Variable,
			 Convert.ToDouble(binomial.Constant) / dblScalar);
		}
		#endregion

		public AffineBinomial<TOut> Cast<TOut>()
		where TOut : struct, IFormattable, IComparable<TOut>, IEquatable<TOut>, IComparable
		{
			return new AffineBinomial<TOut>(ExtraMath.ConvertTo<TOut>(this.Slope), this.Variable,
			 ExtraMath.ConvertTo<TOut>(this.Constant));
		}
	}

	internal static class AffineBinomialExtensions
	{
#if NET20
		internal static void AddConstant<K, T>(IDictionary<K, AffineBinomial<T>> dictionary, K key, double value)
#else
		internal static void AddConstant<K, T>(this IDictionary<K, AffineBinomial<T>> dictionary, K key, double value)
#endif
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			dictionary.Add(key, new AffineBinomial<T>(ExtraMath.ConvertTo<T>(value)));
		}
	}
}
