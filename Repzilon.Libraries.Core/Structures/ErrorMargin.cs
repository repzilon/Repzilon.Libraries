//
//  ErrorMargin.cs
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
using System.Text;

namespace Repzilon.Libraries.Core
{
	public struct ErrorMargin<T> : IEquatable<ErrorMargin<T>>, IFormattable
#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
	, ICloneable
#endif
	where T : struct, IFormattable
#if (!NETSTANDARD1_1)
	, IConvertible
#endif
	{
		public T Middle { get; private set; }
		public T Margin { get; private set; }

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

#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public T Min()
		{
			return Matrix<T>.sub(Middle, Margin);
		}

		public T Max()
		{
			return Matrix<T>.add(Middle, Margin);
		}

		#region ToString
		public override string ToString()
		{
			return this.ToString(null, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrWhiteSpace(format)) {
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
			return obj is ErrorMargin<T> && Equals((ErrorMargin<T>)obj);
		}

		public bool Equals(ErrorMargin<T> other)
		{
			return this.Middle.Equals(other.Middle) && this.Margin.Equals(other.Margin);
		}

		public override int GetHashCode()
		{
			int hashCode = 1348611219;
			hashCode = hashCode * -1521134295 + Middle.GetHashCode();
			hashCode = hashCode * -1521134295 + Margin.GetHashCode();
			return hashCode;
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
