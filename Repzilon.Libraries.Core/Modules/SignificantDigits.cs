//
//  SignificantDigits.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2024 René Rhéaume
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
			var blnLessThanOne = sngAbsolute < 1;
			var sngDigitalPart = blnLessThanOne ? sngAbsolute : RoundOff.Error((float)(sngAbsolute - Math.Floor(sngAbsolute)));
			while ((value % 10) == 0) {
				value *= 0.1f;
			}
			var bytDigits = IntegerPartDigits(value, blnLessThanOne, RoundOff.Equals(sngAbsolute , 1), sngDigitalPart);
			bytDigits += DecimalDigits(blnLessThanOne, sngDigitalPart, "R", false);
			return bytDigits;
		}

		public static byte Count(double value)
		{
			if (value == 0) {
				return 1;
			}
			var dblAbsolute = Math.Abs(value);
			var blnLessThanOne = dblAbsolute < 1;
			var dblDigitalPart = blnLessThanOne ? dblAbsolute : RoundOff.Error(dblAbsolute - Math.Floor(dblAbsolute));
			var bytDigits = IntegerPartDigits(value, dblAbsolute, dblDigitalPart);
			bytDigits += DecimalDigits(blnLessThanOne, dblDigitalPart, "R", false);
			return bytDigits;
		}

		public static byte Count(decimal value)
		{
			if (value == 0) {
				return 1;
			}
			var dcmAbsolute = Math.Abs(value);
			var blnLessThanOne = dcmAbsolute < 1;
			var dcmDigitalPart = dcmAbsolute - Math.Floor(dcmAbsolute);
			var kTen = 10m;
			while ((value % kTen) == 0) {
				value /= kTen;
			}
			var bytDigits = IntegerPartDigits(value, blnLessThanOne, dcmAbsolute == 1, dcmDigitalPart);
			bytDigits += DecimalDigits(blnLessThanOne, dcmDigitalPart, "f17", true);
			return bytDigits;
		}

		private static byte IntegerPartDigits<T>(T value, bool absoluteLessThanOne, bool absoluteEqualsOne, T digitalPart) where T : IEquatable<T>
#if !NETSTANDARD1_1
		, IConvertible
#endif
		{
			if (absoluteLessThanOne) {
				var z = default(T);
				return digitalPart.Equals(z) ? (byte)1 : (byte)0;
			} else if (absoluteEqualsOne) {
				return 1;
			} else {
				return Magnitude(Convert.ToDouble(value));
			}
		}

		private static byte DecimalDigits<T>(bool absoluteLessThanOne, T digitalPart, string roundTrip, bool removeTrailingZeros) where T : IFormattable, IEquatable<T>
		{
			// Microsoft recommends G9 instead of R for Single and G17 for Double, but they cause trouble
			var kDigitZero = new char[] { '0' };
			var z = default(T);
			if (!digitalPart.Equals(z)) {
				var strForCount = digitalPart.ToString(roundTrip, CultureInfo.InvariantCulture).Replace("0.", "");
				if (removeTrailingZeros) {
					strForCount = strForCount.TrimEnd(kDigitZero);
				}
				if (absoluteLessThanOne) {
					strForCount = strForCount.TrimStart(kDigitZero);
				}
				return (byte)strForCount.Length;
			} else {
				return 0;
			}
		}
		#endregion

		#region Count IConvertible dispatch
#if !NETSTANDARD1_1
		[CLSCompliant(false)]
		public static byte Count(IConvertible value)
		{
			if (value == null) {
				throw new ArgumentNullException("value");
			}
			// ReSharper disable once InconsistentNaming
			var enuTC = value.GetTypeCode();
			if (enuTC == TypeCode.String) {
				return Count((string)value);
			} else if (enuTC == TypeCode.Int32) {
				return Count((int)value);
			} else if (enuTC >= TypeCode.SByte && enuTC <= TypeCode.UInt16) {
				return Count(Convert.ToInt32(value));
			} else if (enuTC == TypeCode.Int64) {
				return Count((long)value);
			} else if (enuTC == TypeCode.UInt32) {
				return Count(Convert.ToInt64(value));
			} else if (enuTC == TypeCode.UInt64) {
				// ReSharper disable once PossibleInvalidCastException
				return ((ulong)value > Int64.MaxValue) ? Count(Convert.ToDecimal(value)) : Count((long)value);
			} else if (enuTC == TypeCode.Double) {
				return Count((double)value);
			} else if (enuTC == TypeCode.Single) {
				return Count((float)value);
			} else if (enuTC == TypeCode.Decimal) {
				return Count((decimal)value);
			} else {
				throw new ArgumentException("The argument is neither a number nor a string.", "value");
			}
		}
#endif
		#endregion

		#region Count String overloads
		public static byte Count(string value)
		{
#if NET35 || NET20
			if (RetroCompat.IsNullOrWhiteSpace(value)) {
#else
			if (String.IsNullOrWhiteSpace(value)) {
#endif
				return 0;
			} else {
				CultureInfo ci;
				var dblValue = ParseQty(value, out ci);
				return (dblValue == 0) ? (byte)1 : Count(value, dblValue, ci);
			}
		}

		public static byte Count(string value, CultureInfo culture)
		{
			const NumberStyles kNumberStyles = NumberStyles.Number | NumberStyles.AllowExponent | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowThousands;
			double dblValue;
#if NET35 || NET20
			if (RetroCompat.IsNullOrWhiteSpace(value)) {
#else
			if (String.IsNullOrWhiteSpace(value)) {
#endif
				return 0;
			} else if (Double.TryParse(value, kNumberStyles, culture, out dblValue)) {
				if (dblValue == 0) {
					return 1;
				}
				if (culture == null) {
					culture = CultureInfo.CurrentCulture;
				}
				return Count(value, dblValue, culture);
			} else {
				return 0;
			}
		}

		private static byte Count(string value, double asDouble, CultureInfo culture)
		{
			var dblAbsolute = Math.Abs(asDouble);
			var strTrimmed  = value.Trim().TrimStart('0');
			var nf          = culture.NumberFormat;
			var strDecSep   = nf.NumberDecimalSeparator;
			if (strTrimmed.IndexOf(strDecSep, Equals(culture, CultureInfo.InvariantCulture) ? StringComparison.Ordinal : StringComparison.CurrentCulture) != -1) {
				var intExponent = strTrimmed.IndexOfAny("eE".ToCharArray());
				if (intExponent != -1) {
					strTrimmed = strTrimmed.Substring(0, intExponent);
				}
				strTrimmed = strTrimmed.Replace(strDecSep, "").Replace(" ", "").Replace(nf.NumberGroupSeparator, "");
				return (dblAbsolute < 1) ? (byte)strTrimmed.TrimStart('0').Length : (byte)strTrimmed.Length;
			} else {
				return IntegerPartDigits(asDouble, dblAbsolute, RoundOff.Error(dblAbsolute - Math.Floor(dblAbsolute)));
			}
		}

		private static double ParseQty(string value, out CultureInfo foundCulture)
		{
			const NumberStyles kNumberStyles = NumberStyles.Number | NumberStyles.AllowExponent | NumberStyles.AllowCurrencySymbol;

			if (value != null) {
				value = value.Trim().Replace(" ", "");
			} else {
				throw new ArgumentNullException("value");
			}

#if NETFRAMEWORK || NETSTANDARD2_0 || NET50 || NET60
			var karCultures = new CultureInfo[] { CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture, CultureInfo.InstalledUICulture, CultureInfo.InvariantCulture };
#else
			var karCultures = new CultureInfo[] { CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture, CultureInfo.InvariantCulture };
#endif
			var i = 0;
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
			if (absolute > 1) {
				while (RoundOff.Equals(value % 10, 0)) {
					value *= 0.1;
				}
				return Magnitude(value);
			} else {
				return (RoundOff.Equals(absolute, 1) || RoundOff.Equals(digitalPart, 0)) ? (byte)1 : (byte)0;
			}
		}

		private static byte Magnitude(double value)
		{
			return (byte)Math.Ceiling(Math.Log10(Math.Abs(value)));
		}
		#endregion

		#region Method Round
		public static decimal Round(decimal value, byte figures, RoundingMode rounding)
		{
			if (figures == 0) {
				figures = 1;
			}
			var power = (decimal)PowerOf((double)value);
			var mantissa = (decimal)RoundWithMode((double)(value / power), figures - 1, rounding);
			return mantissa * power;
		}

		public static double Round(double value, byte figures, RoundingMode rounding)
		{
			if (figures == 0) {
				figures = 1;
			}
			var power = PowerOf(value);
			var mantissa = RoundWithMode(value / power, figures - 1, rounding);
			var rounded = mantissa * power;
			if (Math.Abs(rounded) > 1e-13) {
				rounded = RoundOff.Error(rounded);
			}
			return rounded;
		}

		public static float Round(float value, byte figures, RoundingMode rounding)
		{
			if (figures == 0) {
				figures = 1;
			}
			var power = (float)PowerOf(value);
			var mantissa = (float)RoundWithMode(value / power, figures - 1, rounding);
			return mantissa * power;
		}

		private static double PowerOf(double value)
		{
			return Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))));
		}

		private static double RoundWithMode(double value, int digits, RoundingMode rounding)
		{
			var kTen = 10.0;
			double bubble;
#pragma warning disable RECS0012 // 'if' statement can be re-written as 'switch' statement
#pragma warning disable CC0019 // Use 'switch'
			if (rounding == RoundingMode.AwayFromZero) {
				return Math.Round(value, digits, MidpointRounding.AwayFromZero);
			} else if (rounding == RoundingMode.ToEven) {
				return Math.Round(value, digits, MidpointRounding.ToEven);
			} else if (rounding == RoundingMode.Ceiling) {
				if (digits == 0) {
					return Math.Ceiling(value);
				} else {
					bubble = Math.Pow(kTen, digits);
					return Math.Ceiling(value * bubble) / bubble;
				}
			} else if (rounding == RoundingMode.Floor) {
				if (digits == 0) {
					return Math.Floor(value);
				} else {
					bubble = Math.Pow(kTen, digits);
					return Math.Floor(value * bubble) / bubble;
				}
			} else {
				throw RetroCompat.NewUndefinedEnumException("rounding", rounding);
			}
#pragma warning restore CC0019 // Use 'switch'
#pragma warning restore RECS0012 // 'if' statement can be re-written as 'switch' statement
		}
		#endregion
	}
}
