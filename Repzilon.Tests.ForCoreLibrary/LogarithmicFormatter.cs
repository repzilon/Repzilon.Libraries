//
//  LogarithmicFormatter.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//

using System;
using System.Text;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	public sealed class LogarithmicFormatter : ExtensionCustomFormatter
	{
		public override string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (!String.IsNullOrEmpty(format) && ((format[0] == 'L') || (format[0] == 'l'))) {
				var chrE     = format[0] == 'L' ? 'E' : 'e';
				var strExtra = (format.Length > 1) ? format.Substring(1) : "";
				if (arg is decimal) {
					var number = (decimal)arg;
					var log10  = (number == Decimal.Zero) ? 0 : ExtraMath.Log10(Math.Abs(number));
					return DoFormat(Math.Sign(number), chrE, log10, log10 >= 0 ? " " : "", strExtra, formatProvider);
				} else if (arg is double) {
					return DoFormat(chrE, (double)arg, strExtra, formatProvider);
				} else if (arg is IConvertible) {
					return DoFormat(chrE, Convert.ToDouble(arg), strExtra, formatProvider);
				}
			}

			return HandleOtherFormatsWrapped(format, arg);
		}

		private static string DoFormat(char exponent, double number, string extraFormat, IFormatProvider formatProvider)
		{
			var log10 = (number == 0) ? 0 : Math.Log10(Math.Abs(number));
			return DoFormat(Math.Sign(number), exponent, log10, log10 >= 0 ? " " : "", extraFormat, formatProvider);
		}

		private static string DoFormat(int signNum, char exponent, IFormattable log10, string log10Sign,
		string extraFormat, IFormatProvider formatProvider)
		{
			string strSign;
			if (signNum > 0) {
				strSign = "+1";
			} else if (signNum < 0) {
				strSign = "-1";
			} else {
				strSign = " 0";
			}

			return new StringBuilder().Append(strSign).Append(exponent).Append(log10Sign)
				.Append(log10.ToString("f" + extraFormat, formatProvider)).ToString();
		}
	}
}
