//
//  ExtraMath.cs
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Repzilon.Libraries.Core
{
	public static class ExtraMath
	{
		/// <summary>
		/// Solves a quadratic equation axx + bx + c = 0 .
		/// </summary>
		/// <param name="a">Coefficient for x squared</param>
		/// <param name="b">Coefficient for x</param>
		/// <param name="c">Final constant of the equation</param>
		/// <returns>The possible solutions, when they exist.</returns>
		/// <remarks>
		/// An overload with Double data type for arguments and return values
		/// will not be implemented. Additional precision is needed to handle
		/// very small numbers, in the 10^-20 range.
		/// </remarks>
		public static KeyValuePair<decimal, decimal>? SolveQuadratic(decimal a, decimal b, decimal c)
		{
			if (a == 0) {
				throw new ArgumentOutOfRangeException("a", a, "a = 0 would cause a division by zero.");
			}

			var determinant = (b * b) - (4 * a * c);
			if (determinant >= 0) {
				decimal sqrt = Sqrt(determinant);
				var halfA = 0.5m * a; // Avoid the SLOW division instruction on every CPU and FPU
				return new KeyValuePair<decimal, decimal>(
				 (sqrt - b) * halfA, // That's (-b + sqrt(d)) / 2a in fewer operations,
				 ((-1 * b) - sqrt) * halfA); // (-b - sqrt(d)) / 2a);
			} else {
				return null;
			}
		}

		// https://www.csharp-console-examples.com/general/math-sqrt-decimal-in-c/
		public static decimal Sqrt(decimal square)
		{
			if (square < 0) {
				throw new ArgumentOutOfRangeException("square", square, "Cannot extract the square root of a negative number.");
			}

			decimal root = square / 3;
			for (int i = 0; i < 32; i++) {
				root = (root + square / root) * 0.5m;
			}
			return root;
		}

		/// <summary>
		/// Computes a rough estimate to say some text.
		/// </summary>
		/// <param name="text">The text to say.</param>
		/// <returns>The time to say the text, or TimeSpan.Zero if the argument is empty text.</returns>
		/// <remarks>Based on a average speech rate of 130 words per minute,
		/// with each word being 6.61 characters (space included) long on average.</remarks>
		public static TimeSpan SpeechDuration(string text)
		{
			return String.IsNullOrWhiteSpace(text) ? TimeSpan.Zero :
			 TimeSpan.FromMilliseconds(Math.Max(700, 400 + (text.Trim().Length * 70)));
		}

		public static double Hypoth(double a, double b)
		{
			return Math.Sqrt((a * a) + (b * b));
		}

		public static double Hypoth(double a, double b, double c)
		{
			return Math.Sqrt((a * a) + (b * b) + (c * c));
		}

		public static decimal Hypoth(decimal a, decimal b)
		{
			return Sqrt((a * a) + (b * b));
		}

		public static decimal Hypoth(decimal a, decimal b, decimal c)
		{
			return Sqrt((a * a) + (b * b) + (c * c));
		}

		/// <summary>
		/// Computes the force in newtons between two charges in an electric field
		/// </summary>
		/// <param name="qi">first electric charge (in coulombs)</param>
		/// <param name="qj">second electric charge (in coulombs)</param>
		/// <param name="rij">distance between 2 charges (in metres)</param>
		/// <returns>The force between the two charges in newtons.</returns>
		public static double CoulombLab(double qi, double qj, double rij)
		{
			return 9000000000.0 * Math.Abs(qi * qj) / (rij * rij);
		}

		public static decimal CoulombLab(decimal qi, decimal qj, decimal rij)
		{
			return 9000000000 * Math.Abs(qi * qj) / (rij * rij);
		}

		public static float CoulombLab(float qi, float qj, float rij)
		{
			return 9000000000.0f * Math.Abs(qi * qj) / (rij * rij);
		}

		public static Exp CoulombLab(Exp qi, Exp qj, Exp rij)
		{
			return new Exp(9, 10, 9) * Abs(qi * qj) / (rij * rij);
		}

		public static Exp Abs(Exp number)
		{
			return new Exp(Math.Abs(number.Mantissa), number.Base, number.Exponent);
		}

		[CLSCompliant(false)]
		public static double Pow(byte b, sbyte e)
		{
			var ae = Math.Abs(e);
			long r = 1;
			for (var i = 1; i <= ae; i++) {
				r *= b;
			}
			return (e > 0) ? (double)r : 1.0 / r;
		}

		public static int[][] PascalTriangle(byte n)
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

		public static long[][] BigPascalTriangle(byte n)
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

		public static decimal[][] HugePascalTriangle(byte n)
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
	}
}
