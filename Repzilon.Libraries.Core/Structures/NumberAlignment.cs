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
			get { return (this.Flags & flag) != 0; }
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
			var ci = formatProvider as CultureInfo;
			if (ci != null) {
				return ci.NumberFormat;
			} else {
				var nfi = formatProvider as NumberFormatInfo;
				return (nfi == null) ? NumberFormatInfo.CurrentInfo : nfi;
			}
		}

		public void FromNumeric(string numberText, IFormatProvider formatProvider)
		{
			FromNumeric(numberText, FindNumberFormat(formatProvider));
		}

		public void FromNumeric(string numberText, NumberFormatInfo numberFormat)
		{
			if (numberFormat == null) {
				numberFormat = NumberFormatInfo.CurrentInfo;
			}
			if (!String.IsNullOrEmpty(numberText)) {
				numberText = numberText.Trim();
				int posOfE, posOfSep;
				bool blnNegative = false;
				if (numberText.StartsWith(numberFormat.NegativeSign)) {
					this[NumberAlignmentFlags.NegativeMantissa] = true;
					blnNegative = true;
				}
				posOfE = numberText.IndexOf("e", StringComparison.CurrentCultureIgnoreCase);
				if (posOfE > -1) {
					this[NumberAlignmentFlags.Exponent] = true;
					var negativeExponent = 0;
					if (numberText[posOfE + 1].ToString() == numberFormat.NegativeSign) {
						this[NumberAlignmentFlags.NegativeExponent] = true;
						negativeExponent = 1;
					}
					this.ExponentDigits = Math.Max(this.ExponentDigits,
					 (byte)(numberText.Length - posOfE - negativeExponent));
				}
				var nds = numberFormat.NumberDecimalSeparator;
				posOfSep = numberText.IndexOf(nds);
				posOfE = (posOfE > -1) ? posOfE : numberText.Length;
				if (posOfSep > -1) {
					this[NumberAlignmentFlags.DecimalSeparator] = true;
					this.DecimalDigits = Math.Max(this.DecimalDigits,
					 (byte)(posOfE - posOfSep - nds.Length));
					posOfE = posOfSep;
				}
				this.IntegerDigits = Math.Max(this.IntegerDigits,
				 blnNegative ? (byte)(posOfE - 1) : (byte)posOfE);
			}
		}

		public string Format(IFormattable number, string format, IFormatProvider formatProvider)
		{
			var numberText = number.ToString(format, formatProvider);
			var nfi = FindNumberFormat(formatProvider);
			var nds = nfi.NumberDecimalSeparator;
			var allDecimals = this.DecimalDigits;
			var numberIsNegative = numberText.StartsWith(nfi.NegativeSign);
			if (this[NumberAlignmentFlags.NegativeMantissa] && !numberIsNegative) {
				numberText = " " + numberText;
			}

			var posOfSep = numberText.IndexOf(nds);
			if (posOfSep > -1) {
				numberText = InsertIntegerSpaces(numberText, numberIsNegative, nds.Length, posOfSep, nfi);

				int decimalDigits = numberText.Substring(numberText.IndexOf(nds) + 1).Length;
				if (decimalDigits < allDecimals) {
					numberText += new String(' ', allDecimals - decimalDigits);
				}
			} else {
				numberText = InsertIntegerSpaces(numberText, numberIsNegative, nfi.NegativeSign.Length, numberText.Length, nfi);
				if (allDecimals > 0) {
					numberText += new String(' ', allDecimals + nds.Length);
				}
			}
			// FIXME : Format exponents

			return numberText;
		}

		private string InsertIntegerSpaces(string numberText, bool numberIsNegative, int startWhenNegative, int endBound, NumberFormatInfo nfi)
		{
			var numberDigits = numberIsNegative ? startWhenNegative : 0;
			numberDigits = numberText.Substring(numberDigits, endBound - numberDigits).Trim().Length;

			var allDigits = this.IntegerDigits;
			if (numberDigits < allDigits) {
				var spaces = new String(' ', allDigits - numberDigits);
				if (numberIsNegative) {
					var minus = nfi.NegativeSign;
					return minus + spaces + numberText.Substring(minus.Length);
				} else {
					return spaces + numberText;
				}
			} else {
				return numberText;
			}
		}
	}
}
