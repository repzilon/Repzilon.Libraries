﻿//
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

namespace Repzilon.Libraries.Core
{
	// TODO : Add serial dilution
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
	}
}
