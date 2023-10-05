//
//  SignificantDigits.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2023 René Rhéaume
//
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v. 2.0. If a copy of the MPL was 
// not distributed with this file, You can obtain one at 
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Globalization;

namespace Repzilon.Libraries.Core
{
	public enum RoundingMode : byte
	{
		Floor,
		ToEven,
		AwayFromZero,
		Ceiling
	}

	public static class SignificantDigits
	{
		#region Method Count
		#region Count integer types overloads
		public static byte Count(int value)
		{
			if (value == 0) {
				return 1;
			}
			while ((value % 10) == 0) {
				value /= 10;
			}
			return (value >= -1) && (value <= 1) ? (byte)1 : Magnitude(value);
		}

		public static byte Count(long value)
		{
			if (value == 0) {
				return 1;
			}
			while ((value % 10) == 0) {
				value /= 10;
			}
			return (value >= -1) && (value <= 1) ? (byte)1 : Magnitude(value);
		}
		#endregion

		#region Count real number types overloads
		public static byte Count(float value)
		{
			if (value == 0) {
				return 1;
			}
			var sngAbsolute = Math.Abs(value);
			var sngDigitalPart = (sngAbsolute < 1) ? sngAbsolute : Round.Error((float)(sngAbsolute - Math.Floor(sngAbsolute)));
			while ((value % 10) == 0) {
				value /= 10;
			}
			byte bytDigits = IntegerPartDigits(value, sngAbsolute < 1, sngAbsolute == 1, sngDigitalPart);
			bytDigits += DecimalDigits(sngAbsolute < 1, sngDigitalPart, "R", false);
			return bytDigits;
		}

		public static byte Count(double value)
		{
			if (value == 0) {
				return 1;
			}
			var dblAbsolute = Math.Abs(value);
			var dblDigitalPart = (dblAbsolute < 1) ? dblAbsolute : Round.Error(dblAbsolute - Math.Floor(dblAbsolute));
			byte bytDigits = IntegerPartDigits(value, dblAbsolute, dblDigitalPart);
			bytDigits += DecimalDigits(dblAbsolute < 1, dblDigitalPart, "R", false);
			return bytDigits;
		}

		public static byte Count(decimal value)
		{
			if (value == 0) {
				return 1;
			}
			var dcmAbsolute = Math.Abs(value);
			var dcmDigitalPart = dcmAbsolute - Math.Floor(dcmAbsolute);
			while ((value % 10) == 0) {
				value /= 10;
			}
			byte bytDigits = IntegerPartDigits(value, dcmAbsolute < 1, dcmAbsolute == 1, dcmDigitalPart);
			bytDigits += DecimalDigits(dcmAbsolute < 1, dcmDigitalPart, "f17", true);
			return bytDigits;
		}

		private static byte IntegerPartDigits<T>(T value, bool absoluteLessThanOne, bool absoluteEqualsOne, T digitalPart) where T : IEquatable<T>
#if !NETSTANDARD1_1
		, IConvertible
#endif
		{
			if (absoluteLessThanOne) {
				return digitalPart.Equals(default(T)) ? (byte)1 : (byte)0;
			} else if (absoluteEqualsOne) {
				return 1;
			} else {
				return Magnitude(Convert.ToDouble(value));
			}
		}

		private static byte DecimalDigits<T>(bool absoluteLessThanOne, T digitalPart, string roundTrip, bool removeTrailingZeros) where T : IFormattable, IEquatable<T>
		{
			// Microsoft recommends G9 instead of R for Single and G17 for Double, but they cause trouble
			if (!digitalPart.Equals(default(T))) {
				var strForCount = digitalPart.ToString(roundTrip, CultureInfo.InvariantCulture).Replace("0.", "");
				if (removeTrailingZeros) {
					strForCount = strForCount.TrimEnd('0');
				}
				if (absoluteLessThanOne) {
					strForCount = strForCount.TrimStart('0');
				}
				return (byte)strForCount.Length;
			} else {
				return 0;
			}
		}
		#endregion

		#region Count IConvertible dispatch
#if !NETSTANDARD1_1
		public static byte Count(IConvertible value)
		{
			if (value == null) {
				throw new ArgumentNullException("value");
			}
			var enuTC = value.GetTypeCode();
			if (enuTC == TypeCode.String) {
				return Count((string)value);
			} else if (enuTC == TypeCode.Boolean || enuTC == TypeCode.Char || enuTC == TypeCode.DateTime || enuTC == TypeCode.Empty || enuTC == TypeCode.Object) {
				throw new ArgumentException("The argument is neither a number nor a string.", "value");
			} else if (enuTC == TypeCode.Int32) {
				return Count((int)value);
			} else if (enuTC == TypeCode.Byte || enuTC == TypeCode.Int16 || enuTC == TypeCode.SByte || enuTC == TypeCode.UInt16) {
				return Count(Convert.ToInt32(value));
			} else if (enuTC == TypeCode.Int64) {
				return Count((long)value);
			} else if (enuTC == TypeCode.UInt32) {
				return Count(Convert.ToInt64(value));
			} else if (enuTC == TypeCode.UInt64) {
				return ((ulong)value > Int64.MaxValue) ? Count(Convert.ToDecimal(value)) : Count((long)value);
			} else if (enuTC == TypeCode.Double) {
				return Count((double)value);
			} else if (enuTC == TypeCode.Single) {
				return Count((float)value);
			} else if (enuTC == TypeCode.Decimal) {
				return Count((decimal)value);
			} else {
				throw new NotSupportedException("The argument is of an unknown type.");
			}
		}
#endif
		#endregion

		#region Count String overloads
		public static byte Count(string value)
		{
			if (String.IsNullOrWhiteSpace(value)) {
				return 0;
			} else {
				CultureInfo ci;
				double dblValue = ParseQty(value, out ci);
				if (dblValue == 0) {
					return 1;
				}
				return Count(value, dblValue, ci.NumberFormat);
			}
		}

		public static byte Count(string value, CultureInfo culture)
		{
			const NumberStyles kNumberStyles = NumberStyles.Number | NumberStyles.AllowExponent | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowThousands;
			double dblValue;
			if (String.IsNullOrWhiteSpace(value)) {
				return 0;
			} else if (Double.TryParse(value, kNumberStyles, culture, out dblValue)) {
				if (dblValue == 0) {
					return 1;
				}
				if (culture == null) {
					culture = CultureInfo.CurrentCulture;
				}
				return Count(value, dblValue, culture.NumberFormat);
			} else {
				return 0;
			}
		}

		private static byte Count(string value, double asDouble, NumberFormatInfo nf)
		{
			var dblAbsolute = Math.Abs(asDouble);
			var strTrimmed = value.Trim().TrimStart('0');
			var strDecSep = nf.NumberDecimalSeparator;
			var intDec = strTrimmed.IndexOf(strDecSep);
			if (intDec != -1) {
				var intExponent = strTrimmed.IndexOfAny(new char[] { 'e', 'E' });
				if (intExponent != -1) {
					strTrimmed = strTrimmed.Substring(0, intExponent);
				}
				strTrimmed = strTrimmed.Replace(strDecSep, "").Replace(" ", "").Replace(nf.NumberGroupSeparator, "");
				return (dblAbsolute < 1) ? (byte)strTrimmed.TrimStart('0').Length : (byte)strTrimmed.Length;
			} else {
				var dblDigitalPart = Round.Error(dblAbsolute - Math.Floor(dblAbsolute));
				return IntegerPartDigits(asDouble, dblAbsolute, dblDigitalPart);
			}
		}

		private static double ParseQty(string value, out CultureInfo foundCulture)
		{
			const NumberStyles kNumberStyles = NumberStyles.Number | NumberStyles.AllowExponent | NumberStyles.AllowCurrencySymbol;

			if (value != null) {
				value = value.Trim().Replace(" ", "");
			}

#if NETSTANDARD2_0
			var karCultures = new CultureInfo[] { CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture, CultureInfo.InstalledUICulture, CultureInfo.InvariantCulture };
#else
			var karCultures = new CultureInfo[] { CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture, CultureInfo.InvariantCulture };
#endif
			int i = 0;
			while (i < karCultures.Length) {
				var ci = karCultures[i];
				double dblValue;
				if (Double.TryParse(value.Replace(ci.NumberFormat.NumberGroupSeparator, ""), kNumberStyles, ci, out dblValue)) {
					foundCulture = ci;
					return dblValue;
				} else {
					i++;
				}
			}
			throw new FormatException("Unable to parse text as a number.");
		}
		#endregion

		private static byte IntegerPartDigits(double value, double absolute, double digitalPart)
		{
			while ((value % 10) == 0) {
				value /= 10;
			}
			if (absolute < 1) {
				return (digitalPart != 0) ? (byte)0 : (byte)1;
			} else if (absolute == 1) {
				return 1;
			} else {
				return Magnitude(value);
			}
		}

		private static byte Magnitude(double value)
		{
			return (byte)Math.Ceiling(Math.Log10(Math.Abs(value)));
		}
		#endregion

#if (false)
		public static decimal Round(decimal value, byte figures, RoundingMode rounding)
		{
			
		}

		public static double Round(double value, byte figures, RoundingMode rounding)
		{

		}

		public static float Round(float value, byte figures, RoundingMode rounding)
		{

		}
#endif
	}
}
