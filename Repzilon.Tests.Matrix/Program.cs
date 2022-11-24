//
//  Program.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022 René Rhéaume
//
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v. 2.0. If a copy of the MPL was 
// not distributed with this file, You can obtain one at 
// https://mozilla.org/MPL/2.0/.
//
using System;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.Matrix
{
	public static class Program
	{
		static void Main()
		{
			var ex80_a = new Matrix<short>(3, 3, 2, -1, 5, -3, 4, 7, 1, -1, 0);
			var ex80_b = new Matrix<short>(3, 3, 1, 4, 0, -3, 6, -5, 1, 0, -1);
			var ex80_result = (3 * ex80_a) + ex80_b;
			Console.WriteLine(ex80_a);
			Console.WriteLine(ex80_b);
			Console.WriteLine(ex80_result);

			var ex81_a = new Matrix<short>(2, 2, 1, 3, -2, 1);
			var ex81_b = new Matrix<short>(2, 3, 1, 2, 1, -3, 4, -5);
			try {
				var ex81_ab = ex81_a * ex81_b;
				Console.WriteLine(ex81_ab);
				var ex81_ba = ex81_b * ex81_a;
				Console.WriteLine(ex81_ba);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.ToString());
			}

			var pap_m = new Matrix<short>(3, 3, 4, 3, 5, -3, -7, 1, 8, 0, 0);
			var pap_n = new Matrix<short>(3, 3, 4, 5, 1, 0, 1, -4, 6, 2, -1);
			var pap_a = pap_m + pap_n;
			var pap_b = (2 * pap_n) - (5 * pap_m);
			var pap_c = pap_m * pap_n;
			var pap_d = pap_n * pap_m;

			Console.WriteLine(pap_a);
			Console.WriteLine(pap_b);
			Console.WriteLine(pap_c);
			Console.WriteLine(pap_d);

			var ex88_a = new Matrix<short>(3, 3, 2, 1, -1, 3, -3, 1, 1, -2, 1);
			var ex88_b = new Matrix<short>(3, 1, 1, 16, 9);
			var ex88_plus = ex88_a.Augment(ex88_b);
			Console.WriteLine(ex88_a);
			Console.WriteLine(ex88_b);
			Console.WriteLine(ex88_plus);

			var ex88_forinvert = ex88_a.Augment(Matrix<short>.Identity(ex88_a.Lines));
			Console.WriteLine(ex88_forinvert);

			Console.WriteLine("Press Enter to exit...");
			Console.ReadLine();
		}
	}
}

