//
//  ErrorMargin.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2025 René Rhéaume
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
// ReSharper disable InvokeAsExtensionMethod

namespace Repzilon.Libraries.Core
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct ErrorMargin<T> : IEquatable<ErrorMargin<T>>, IFormattable,
	IComparableErrorMargin, IEquatable<IComparableErrorMargin>
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IEquatable<T>, IComparable
	{
		public T Middle { get; private set; }
		public T Margin { get; private set; }

		IComparable IComparableErrorMargin.Middle
		{
			get { return Middle; }
		}

		IComparable IComparableErrorMargin.Margin
		{
			get { return Margin; }
		}

		public ErrorMargin(T middle, T margin) : this()
		{
			Middle = middle;
			Margin = margin;
		}

		#region ICloneable members
		public ErrorMargin(ErrorMargin<T> source) : this(source.Middle, source.Margin) { }

		public ErrorMargin<T> Clone()
		{
			return new ErrorMargin<T>(Middle, Margin);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public ErrorMargin<TOut> Cast<TOut>()
		where TOut : struct, IFormattable, IEquatable<TOut>, IComparable
		{
			return new ErrorMargin<TOut>(ExtraMath.ConvertTo<TOut>(this.Middle),
			 ExtraMath.ConvertTo<TOut>(this.Margin));
		}

		public T Min()
		{
#if NET20
			return RoundOff.Error(Arithmetic<T>.SubtractScalars(Middle, Margin));
#else
			return RoundOff.Error(Arithmetic<T>.Sub(Middle, Margin));
#endif
		}

		public T Max()
		{
#if NET20
			return RoundOff.Error(Arithmetic<T>.AddScalars(Middle, Margin));
#else
			return RoundOff.Error(Arithmetic<T>.Adder(Middle, Margin));
#endif
		}

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
			var stbInterval = new StringBuilder();
			if (format.StartsWith("G") || format.StartsWith("g")) {
				stbInterval.Append(this.Middle.ToString(format, formatProvider)).Append(" ± ")
				 .Append(this.Margin.ToString(format, formatProvider));
			}
			if (format.StartsWith("G")) {
				stbInterval.Append(" -> ");
			}
			if (!format.StartsWith("g")) {
				stbInterval.Append('[').Append(this.Min().ToString(format, formatProvider)).Append("; ")
				 .Append(this.Max().ToString(format, formatProvider)).Append(']');
			}
			return stbInterval.ToString();
		}
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is ErrorMargin<T> ? Equals((ErrorMargin<T>)obj) : Equals(obj as IComparableErrorMargin);
		}

		public bool Equals(ErrorMargin<T> other)
		{
			return this.Middle.Equals(other.Middle) && this.Margin.Equals(other.Margin);
		}

		private bool Equals(IComparableErrorMargin other)
		{
			var typT = typeof(T);
			return (other != null) && (this.Middle.CompareTo(Convert.ChangeType(other.Middle, typT)) == 0) &&
				   (this.Margin.CompareTo(Convert.ChangeType(other.Margin, typT)) == 0);
		}

		bool IEquatable<IComparableErrorMargin>.Equals(IComparableErrorMargin other)
		{
			return this.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (1348611219 * -1521134295) + Middle.GetHashCode();
				return (hashCode * -1521134295) + Margin.GetHashCode();
			}
		}

		public static bool operator ==(ErrorMargin<T> left, ErrorMargin<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ErrorMargin<T> left, ErrorMargin<T> right)
		{
			return !(left == right);
		}
		#endregion

		public ErrorMargin<T> Round()
		{
			var numMargin = this.Margin;
			var numValue = this.Middle;
			if (numValue is double) {
				double dblIncert = SignificantDigits.Ceil(Convert.ToDouble(numMargin));
				return NewFrom(Math.Round(Convert.ToDouble(numValue), Decimals(dblIncert), MidpointRounding.ToEven), dblIncert);
			} else if (numValue is decimal) {
				decimal dcmIncert = SignificantDigits.Ceil(Convert.ToDecimal(numMargin));
				return NewFrom(Math.Round(Convert.ToDecimal(numValue), Decimals((double)dcmIncert), MidpointRounding.ToEven), dcmIncert);
			} else if (numValue is float) {
				float sngIncert = SignificantDigits.Ceil(Convert.ToSingle(numMargin));
				return NewFrom((float)Math.Round(Convert.ToSingle(numValue), Decimals(sngIncert), MidpointRounding.ToEven), sngIncert);
			} else {
				throw new NotSupportedException();
			}
		}

		private static byte Decimals(double number)
		{
			return (byte)(-Math.Floor(Math.Log10(Math.Abs(number))));
		}

		private static ErrorMargin<T> NewFrom<TIn>(TIn middle, TIn margin)
		where TIn : struct
		{
			return new ErrorMargin<T>(ExtraMath.ConvertTo<T>(middle),
			 ExtraMath.ConvertTo<T>(margin));
		}
	}
}
