//
//  NumberAlignment.cs
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
using System.Globalization;
using System.Runtime.InteropServices;

namespace Repzilon.Libraries.Core
{
	[Flags]
	internal enum NumberAlignmentFlags : byte
	{
		NegativeMantissa = 1,
		DecimalSeparator = 2,
		Exponent = 4,
		NegativeExponent = 8
	}

	[StructLayout(LayoutKind.Auto)]
	internal struct NumberAlignment
	{
		public NumberAlignmentFlags Flags;
		public byte IntegerDigits;
		public byte DecimalDigits;
		public byte ExponentDigits;

		public bool this[NumberAlignmentFlags flag] {
			get { return this.Flags.HasFlag(flag); }
			set {
				if (value) {
					this.Flags |= flag;
				} else {
					this.Flags &= ~flag;
				}
			}
		}

		private static NumberFormatInfo FindNumberFormat(IFormatProvider formatProvider)
		{
			if (formatProvider == null) {
				return CultureInfo.CurrentCulture.NumberFormat;
			} else {
				var nfi = formatProvider as NumberFormatInfo;
				if (nfi == null) {
					var ci = formatProvider as CultureInfo;
					if (ci != null) {
						nfi = ci.NumberFormat;
					}
				}
				return (nfi == null) ? CultureInfo.CurrentCulture.NumberFormat : nfi;
			}
		}

		public void FromNumeric(string numberAsString, IFormatProvider formatProvider)
		{
			FromNumeric(numberAsString, FindNumberFormat(formatProvider));
		}

		public void FromNumeric(string numberAsString, NumberFormatInfo numberFormat)
		{
			if (numberFormat == null) {
				numberFormat = CultureInfo.CurrentCulture.NumberFormat;
			}
			if (!String.IsNullOrEmpty(numberAsString)) {
				numberAsString = numberAsString.Trim();
				bool blnNegative = false;
				if (numberAsString.StartsWith(numberFormat.NegativeSign)) {
					this[NumberAlignmentFlags.NegativeMantissa] = true;
					blnNegative = true;
				}
				var posOfE = numberAsString.IndexOf("e", StringComparison.CurrentCultureIgnoreCase);
				if (posOfE > -1) {
					this[NumberAlignmentFlags.Exponent] = true;
					if (numberAsString[posOfE + 1].ToString() == numberFormat.NegativeSign) {
						this[NumberAlignmentFlags.NegativeExponent] = true;
						this.ExponentDigits = Math.Max(this.ExponentDigits,
						 (byte)(numberAsString.Length - posOfE - 1));
					} else {
						this.ExponentDigits = Math.Max(this.ExponentDigits,
						 (byte)(numberAsString.Length - posOfE));
					}
				}
				var posOfSep = numberAsString.IndexOf(numberFormat.NumberDecimalSeparator);
				if (posOfSep > -1) {
					this[NumberAlignmentFlags.DecimalSeparator] = true;
					this.DecimalDigits = Math.Max(this.DecimalDigits,
					 posOfE > -1 ? (byte)(posOfE - posOfSep - numberFormat.NumberDecimalSeparator.Length) :
					 (byte)(numberAsString.Length - posOfSep - numberFormat.NumberDecimalSeparator.Length));
					this.IntegerDigits = Math.Max(this.IntegerDigits,
					 blnNegative ? (byte)(posOfSep - 1) : (byte)posOfSep);
				} else {
					if (posOfE > -1) {
						this.IntegerDigits = Math.Max(this.IntegerDigits,
						 blnNegative ? (byte)(posOfE - 1) : (byte)posOfE);
					} else {
						this.IntegerDigits = Math.Max(this.IntegerDigits,
						 blnNegative ? (byte)(numberAsString.Length - 1) : (byte)numberAsString.Length);
					}
				}
			}
		}

		public string Format(IFormattable number, string format, IFormatProvider formatProvider)
		{
			var numberAsString = number.ToString(format, formatProvider);
			var nfi = FindNumberFormat(formatProvider);
			var numberIsNegative = numberAsString.StartsWith(nfi.NegativeSign);
			if (this[NumberAlignmentFlags.NegativeMantissa] && !numberIsNegative) {
				numberAsString = " " + numberAsString;
			}
			var posOfSep = numberAsString.IndexOf(nfi.NumberDecimalSeparator);
			if (posOfSep > -1) {
				int integerDigits;
				if (numberIsNegative) {
					integerDigits = numberAsString.Substring(nfi.NumberDecimalSeparator.Length, posOfSep).Trim().Length;
				} else {
					integerDigits = numberAsString.Substring(0, posOfSep).Trim().Length;
				}
				numberAsString = InsertSpaces(numberAsString, integerDigits, numberIsNegative, nfi);

				posOfSep = numberAsString.IndexOf(nfi.NumberDecimalSeparator);
				int decimalDigits = numberAsString.Substring(posOfSep + 1).Length;
				if (decimalDigits < this.DecimalDigits) {
					numberAsString += new String(' ', this.DecimalDigits - decimalDigits);
				}
			} else {
				int numberDigits;
				if (numberIsNegative) {
					numberDigits = numberAsString.Substring(nfi.NegativeSign.Length).Trim().Length;
				} else {
					numberDigits = numberAsString.Trim().Length;
				}
				numberAsString = InsertSpaces(numberAsString, numberDigits, numberIsNegative, nfi);
				if (this.DecimalDigits > 0) {
					numberAsString += new String(' ', this.DecimalDigits + nfi.NumberDecimalSeparator.Length);
				}
			}
			// TODO : Format exponents

			return numberAsString;
		}

		private string InsertSpaces(string numberAsString, int numberDigits, bool numberIsNegative, NumberFormatInfo nfi)
		{
			if (numberDigits < this.IntegerDigits) {
				var spaces = new String(' ', this.IntegerDigits - numberDigits);
				if (numberIsNegative) {
					return nfi.NegativeSign + spaces + numberAsString.Substring(nfi.NegativeSign.Length);
				} else {
					return spaces + numberAsString;
				}
			} else {
				return numberAsString;
			}
		}
	}
}
