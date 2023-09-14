//
//  PascalTriangle.cs
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
using System;
using System.Globalization;
using System.Text;

namespace Repzilon.Libraries.Core
{
	public static class PascalTriangle
	{
		public static int[][] Make(byte n)
		{
			if (n > 33) {
				throw new ArgumentOutOfRangeException("n", n, "A number of rows over 33 causes an aritmetic overflow in individual value calculation.");
			}

			var intjarPascal = new int[n + 1][];
			intjarPascal[0] = new int[] { 1 };
			if (n > 0) {
				intjarPascal[1] = new int[] { 1, 1 };
			}
			for (int r = 2; r <= n; r++) {
				var intarRow = new int[r + 1];
				intarRow[0] = 1;
				intarRow[r] = 1;
				for (int c = 1; c < r; c++) {
					intarRow[c] = intjarPascal[r - 1][c - 1] + intjarPascal[r - 1][c];
				}
				intjarPascal[r] = intarRow;
			}
			return intjarPascal;
		}

		public static long[][] MakeBig(byte n)
		{
			if (n > 66) {
				throw new ArgumentOutOfRangeException("n", n, "A number of rows over 66 causes an aritmetic overflow in individual value calculation.");
			}

			var lngjarPascal = new long[n + 1][];
			lngjarPascal[0] = new long[] { 1 };
			if (n > 0) {
				lngjarPascal[1] = new long[] { 1, 1 };
			}
			for (int r = 2; r <= n; r++) {
				var lngarRow = new long[r + 1];
				lngarRow[0] = 1;
				lngarRow[r] = 1;
				for (int c = 1; c < r; c++) {
					lngarRow[c] = lngjarPascal[r - 1][c - 1] + lngjarPascal[r - 1][c];
				}
				lngjarPascal[r] = lngarRow;
			}
			return lngjarPascal;
		}

		public static decimal[][] MakeHuge(byte n)
		{
			if (n > 99) {
				throw new ArgumentOutOfRangeException("n", n, "A number of rows over 99 causes an aritmetic overflow in individual value calculation.");
			}

			var dcmjarPascal = new decimal[n + 1][];
			dcmjarPascal[0] = new decimal[] { 1 };
			if (n > 0) {
				dcmjarPascal[1] = new decimal[] { 1, 1 };
			}
			for (int r = 2; r <= n; r++) {
				var dcmarRow = new decimal[r + 1];
				dcmarRow[0] = 1;
				dcmarRow[r] = 1;
				for (int c = 1; c < r; c++) {
					dcmarRow[c] = dcmjarPascal[r - 1][c - 1] + dcmjarPascal[r - 1][c];
				}
				dcmjarPascal[r] = dcmarRow;
			}
			return dcmjarPascal;
		}

		public static string ToString<T>(this T[][] jagged, CultureInfo culture) where T : IFormattable
		{
			var stbJagged = new StringBuilder();
			if (jagged != null) {
				stbJagged.Append('[');
				var ls = culture.TextInfo.ListSeparator;
				for (int r = 0; r < jagged.Length; r++) {
					if (r > 0) {
						stbJagged.AppendLine(ls);
					}
					stbJagged.Append('[');
					var cn = jagged[r].Length;
					for (int c = 0; c < cn; c++) {
						if (c > 0) {
							stbJagged.Append(ls);
						}
						stbJagged.Append(jagged[r][c].ToString(null, culture));
					}
					stbJagged.Append(']');
				}
				stbJagged.Append(']');
			}
			return stbJagged.ToString();
		}

		public static string Format<T>(T[][] pascal) where T : IFormattable
		{
			var c = pascal.Length;
			var digits = Math.Truncate(Math.Log10(Convert.ToDouble(pascal[c - 1][c / 2]))) + 1;
			var format = "{0," + digits + ":g" + digits + "}";
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

