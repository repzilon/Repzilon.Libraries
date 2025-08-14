//
//  ExtensionCustomFormatter.cs
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
using System.Globalization;

namespace Repzilon.Tests.ForCoreLibrary
{
	public abstract class ExtensionCustomFormatter : IFormatProvider, ICustomFormatter
	{
		// IFormatProvider.GetFormat implementation.
		public object GetFormat(Type formatType)
		{
			// Determine whether custom formatting object is requested.
			return formatType == typeof(ICustomFormatter) ? this : null;
		}

		protected static string HandleOtherFormatsWrapped(string format, object arg)
		{
			try {
				return HandleOtherFormats(format, arg);
			} catch (FormatException e) {
				throw new FormatException("The format specifier " + format + " is not valid.", e);
			}
		}

		private static string HandleOtherFormats(string format, object arg)
		{
			var formattable = arg as IFormattable;
			if (formattable != null) {
				return formattable.ToString(format, CultureInfo.CurrentCulture);
			} else {
				return arg != null ? arg.ToString() : "";
			}
		}

		public abstract string Format(string format, object arg, IFormatProvider formatProvider);
	}
}
