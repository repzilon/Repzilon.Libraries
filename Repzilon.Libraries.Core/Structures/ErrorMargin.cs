//
//  ErrorMargin.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2024 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Globalization;
using System.Text;

namespace Repzilon.Libraries.Core
{
	public struct ErrorMargin<T> : IEquatable<ErrorMargin<T>>, IFormattable,
	IComparableErrorMargin, IEquatable<IComparableErrorMargin>
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IEquatable<T>, IComparable
	{
		public T Middle { get; private set; }
		public T Margin { get; private set; }

		IComparable IComparableErrorMargin.Middle {
			get { return this.Middle; }
		}

		IComparable IComparableErrorMargin.Margin {
			get { return this.Margin; }
		}

		public ErrorMargin(T middle, T margin)
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
			return new ErrorMargin<TOut>(this.Middle.ConvertTo<TOut>(), this.Margin.ConvertTo<TOut>());
		}

		public T Min()
		{
			return GenericArithmetic<T>.sub(Middle, Margin);
		}

		public T Max()
		{
			return GenericArithmetic<T>.adder(Middle, Margin);
		}

		#region ToString
		public override string ToString()
		{
			return this.ToString(null, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
#if NET35
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
				stbInterval.Append(this.Middle.ToString(format, formatProvider)).Append(" ± ").Append(this.Margin.ToString(format, formatProvider));
			}
			if (format.StartsWith("G")) {
				stbInterval.Append(" -> ");
			}
			if (!format.StartsWith("g")) {
				stbInterval.Append('[').Append(this.Min().ToString(format, formatProvider)).Append("; ").Append(this.Max().ToString(format, formatProvider)).Append(']');
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
	}
}
