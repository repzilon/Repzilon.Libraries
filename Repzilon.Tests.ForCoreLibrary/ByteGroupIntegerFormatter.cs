//
//  ByteGroupIntegerFormatter.cs
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
using System.Text;

namespace Repzilon.Tests.ForCoreLibrary
{
	public sealed class ByteGroupIntegerFormatter : ExtensionCustomFormatter
	{
		// Format number in binary (B), octal (O), or hexadecimal (H).
		public override string Format(string format, object arg, IFormatProvider formatProvider)
		{
			// Handle null or empty format string, string with precision specifier.
			// Extract first character of format string (precision specifiers are not supported).
			var thisFmt = !String.IsNullOrEmpty(format) ? Char.ToUpper(format[0]) : 'G';

			// Get a byte array representing the numeric value.
			byte[] bytes;
			if (arg is sbyte) {
				bytes = new byte[] { Byte.Parse(((sbyte)arg).ToString("X2"), NumberStyles.HexNumber) };
			} else if (arg is byte) {
				bytes = new byte[] { (byte)arg };
			} else if (arg is short) {
				bytes = BitConverter.GetBytes((short)arg);
			} else if (arg is int) {
				bytes = BitConverter.GetBytes((int)arg);
			} else if (arg is long) {
				bytes = BitConverter.GetBytes((long)arg);
			} else if (arg is ushort) {
				bytes = BitConverter.GetBytes((ushort)arg);
			} else if (arg is uint) {
				bytes = BitConverter.GetBytes((uint)arg);
			} else if (arg is ulong) {
				bytes = BitConverter.GetBytes((ulong)arg);
			} else {
				return HandleOtherFormatsWrapped(format, arg);
			}

			int baseNumber, charsPerByte;
			if (thisFmt == 'B') {	// Binary formatting.
				baseNumber = 2;
				charsPerByte = 8;
			} else if (thisFmt == 'O') {	// Octal
				baseNumber = 8;
				charsPerByte = 3;
			} else if (thisFmt == 'H') {	// Hexadecimal
				baseNumber = 16;
				charsPerByte = 2;
			} else {
				// Handle unsupported format strings.
				return HandleOtherFormatsWrapped(format, arg);
			}

			// Return a formatted string.
			var numericString = new StringBuilder();
			for (var ctr = bytes.GetUpperBound(0); ctr >= bytes.GetLowerBound(0); ctr--) {
				var byteString = Convert.ToString(bytes[ctr], baseNumber);
				numericString.Append(new String('0', charsPerByte - byteString.Length)).Append(byteString).Append(' ');
			}

			numericString.Length--; // Trim final space
			return numericString.ToString();
		}
	}
}
