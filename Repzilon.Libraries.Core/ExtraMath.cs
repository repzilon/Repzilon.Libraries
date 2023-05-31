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
		public static ValueTuple<decimal, decimal>? SolveQuadratic(decimal a, decimal b, decimal c)
		{
			if (a == 0) {
				throw new ArgumentOutOfRangeException("a");
			}

			var determinant = (b * b) - (4 * a * c);
			if (determinant >= 0) {
				decimal sqrt = Sqrt(determinant);
				var halfA = 0.5m * a; // Avoid the SLOW division instruction on every CPU and FPU
				var x1 = (sqrt - b) * halfA; // That's (-b + sqrt(d)) / 2a in fewer operations
				var x2 = ((-1 * b) - sqrt) * halfA; // (-b - sqrt(d)) / 2a
				return new ValueTuple<decimal, decimal>(x1, x2);
			} else {
				return null;
			}
		}

		// https://www.csharp-console-examples.com/general/math-sqrt-decimal-in-c/
		public static decimal Sqrt(decimal square)
		{
			if (square < 0) {
				throw new ArgumentOutOfRangeException("square");
			}

			decimal root = square / 3;
			for (int i = 0; i < 32; i++) {
				root = (root + square / root) * 0.5m;
			}
			return root;
		}
	}
}
