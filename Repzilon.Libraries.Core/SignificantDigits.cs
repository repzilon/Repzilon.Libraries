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

		public static byte Count(float value)
		{
			if (value == 0) {
				return 1;
			}
			var sngAbsolute = Math.Abs(value);
			var sngDigitalPart = Round.Error((float)(sngAbsolute - Math.Floor(sngAbsolute)));
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
			var dblDigitalPart = Round.Error(dblAbsolute - Math.Floor(dblAbsolute));
			while ((value % 10) == 0) {
				value /= 10;
			}
			byte bytDigits;
			if (dblAbsolute < 1) {
				bytDigits = (dblDigitalPart != 0) ? (byte)0 : (byte)1;
			} else if (dblAbsolute == 1) {
				bytDigits = 1;
			} else {
				// Do not replace Math.Abs(value) with sngAbsolute, because we changed value with the loop above
				bytDigits = (byte)Math.Ceiling(Math.Log10(Math.Abs(value)));
			}
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
				// Do not replace Math.Abs(value) with sngAbsolute, because we changed value with the loop above
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

#if !NETSTANDARD1_1
		public static byte Count(IConvertible value)
		{
			if (value == null) {
				throw new ArgumentNullException("value");
			}
			var enuTC = value.GetTypeCode();
			if (enuTC == TypeCode.String) {
				throw new NotImplementedException("String support is planned.");
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

#if (false)
		public static byte Count(string value)
		{

		}

		public static byte Count(string value, CultureInfo culture)
		{

		}

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

		private static double ParseQty(string value)
		{
			const NumberStyles kNumberStyles = NumberStyles.Number | NumberStyles.AllowExponent | NumberStyles.AllowCurrencySymbol;
			double dblValue;

			if (!Double.TryParse(value, kNumberStyles, CultureInfo.CurrentCulture, out dblValue)) {
				if (!Double.TryParse(value, kNumberStyles, CultureInfo.CurrentUICulture, out dblValue)) {
#if NETSTANDARD2_0
					if (!Double.TryParse(value, kNumberStyles, CultureInfo.InstalledUICulture, out dblValue)) {
#endif
						if (!Double.TryParse(value, kNumberStyles, CultureInfo.InvariantCulture, out dblValue)) {
							throw new FormatException("Unable to parse text as a number.");
						}
#if NETSTANDARD2_0
					}
#endif
				}
			}
			return dblValue;
		}
	}
}

