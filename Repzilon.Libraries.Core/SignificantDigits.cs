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
			return (value >= -1) && (value <= 1) ? (byte)1 : (byte)Math.Ceiling(Math.Log10(Math.Abs(value)));
		}

		public static byte Count(long value)
		{
			if (value == 0) {
				return 1;
			}
			while ((value % 10) == 0) {
				value /= 10;
			}
			return (value >= -1) && (value <= 1) ? (byte)1 : (byte)Math.Ceiling(Math.Log10(Math.Abs(value)));
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
			byte bytDigits;
			if (sngAbsolute < 1) {
				bytDigits = (sngDigitalPart != 0) ? (byte)0 : (byte)1;
			} else if (sngAbsolute == 1) {
				bytDigits = 1;
			} else {
				// Do not replace Math.Abs(value) with sngAbsolute, because we changed value with the loop above
				bytDigits = (byte)Math.Ceiling(Math.Log10(Math.Abs(value)));
			}
			if (sngDigitalPart != 0) {
				// Microsoft recommends G9 instead of R for Single, but G9 causes trouble in fact.
				var strForCount = sngDigitalPart.ToString("R", CultureInfo.InvariantCulture).Replace("0.", "");
				if (sngAbsolute < 1) {
					strForCount = strForCount.TrimStart('0');
				}
				bytDigits += (byte)strForCount.Length;
			}
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
			if (dblDigitalPart != 0) {
				// Microsoft recommends G17 instead of R for Double, but G17 causes trouble in fact.
				var strForCount = dblDigitalPart.ToString("R", CultureInfo.InvariantCulture).Replace("0.", "");
				if (dblAbsolute < 1) {
					strForCount = strForCount.TrimStart('0');
				}
				bytDigits += (byte)strForCount.Length;
			}
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
			byte bytDigits;
			if (dcmAbsolute < 1) {
				bytDigits = (dcmDigitalPart != 0) ? (byte)0 : (byte)1;
			} else if (dcmAbsolute == 1) {
				bytDigits = 1;
			} else {
				// Do not replace Math.Abs(value) with dcmAbsolute, because we changed value with the loop above
				bytDigits = (byte)Math.Ceiling(Math.Log10(Math.Abs((double)value)));
			}
			if (dcmDigitalPart != 0) {
				// Format with maximum decimals, then remove trailing zeros
				var strForCount = dcmDigitalPart.ToString("f17", CultureInfo.InvariantCulture).Replace("0.", "").TrimEnd('0');
				if (dcmAbsolute < 1) {
					strForCount = strForCount.TrimStart('0');
				}
				bytDigits += (byte)strForCount.Length;
			}
			return bytDigits;
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
				double dblValue = ParseQty(value);
				if (dblValue == 0) {
					return 1;
				}
				// FIXME : Get the real decimal separator
				return Count(value, dblValue, new CultureInfo("fr-CA").NumberFormat);
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
		#endregion

		private static byte IntegerPartDigits(double value, double dblAbsolute, double dblDigitalPart)
		{
			while ((value % 10) == 0) {
				value /= 10;
			}
			byte bytDigits;
			if (dblAbsolute < 1) {
				bytDigits = (dblDigitalPart != 0) ? (byte)0 : (byte)1;
			} else if (dblAbsolute == 1) {
				bytDigits = 1;
			} else {
				// Do not replace Math.Abs(value) with dblAbsolute, because we changed value with the loop above
				bytDigits = (byte)Math.Ceiling(Math.Log10(Math.Abs(value)));
			}

			return bytDigits;
		}

		private static double ParseQty(string value)
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
					return dblValue;
				} else {
					i++;
				}
			}
			throw new FormatException("Unable to parse text as a number.");
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
