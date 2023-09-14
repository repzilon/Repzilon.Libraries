//
//  Program.cs
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
using System.Text;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.Pascal
{
	internal static class Program
	{
		static void Main()
		{
			for (byte i = 4; i <= 12; i++) {
				Console.WriteLine(FormatPascalTriangle(ExtraMath.PascalTriangle(i)));
			}

			Console.WriteLine("Press Enter to exit...");
			Console.ReadLine();
		}

		private static string FormatPascalTriangle(int[][] pascal)
		{
			var c = pascal.Length;
			var max = pascal[c - 1][c / 2];
			var digits = Math.Truncate(Math.Log10(max)) + 1;
			var format = "{0," + digits.ToString() + ":g" + digits.ToString() + "}";
			var digitLengths = new short[c];
			byte r;
			for (r = 0; r < c; r++) {
				digitLengths[r] = checked((short)((digits * (r + 1)) + r));
			}
			var stbPascal = new StringBuilder();
			var last = digitLengths[c - 1];
			for (r = 0; r < c; r++) {
				if (r > 0) {
					stbPascal.Append(Environment.NewLine);
				}
				stbPascal.Append(' ', (last - digitLengths[r]) / 2);
				for (byte k = 0; k <= r; k++) {
					if (k > 0) {
						stbPascal.Append(' ');
					}
					stbPascal.AppendFormat(format, pascal[r][k]);
				}
			}
			return stbPascal.ToString();
		}
	}
}
