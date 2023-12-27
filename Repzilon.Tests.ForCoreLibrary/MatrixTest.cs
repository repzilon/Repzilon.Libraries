//
//  MatrixTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2023 René Rhéaume
//
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v. 2.0. If a copy of the MPL was 
// not distributed with this file, You can obtain one at 
// https://mozilla.org/MPL/2.0/.
//
using System;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	static class MatrixTest
	{
		internal static void Run(string[] args)
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
				Console.Error.WriteLine(ex);
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

			var ex_89 = new Matrix<short>(2, 2, 3, 7, 2, 4);
			Console.WriteLine("{0} det(M) = {1}", ex_89, ex_89.Determinant());
			var ex_90 = new Matrix<short>(3, 3, 2, 1, -1, 3, -3, 1, 1, -2, 1);
			Console.WriteLine("{0} det(M) = {1}", ex_90, ex_90.Determinant());
			Console.WriteLine("{0} det(M) = {1}", ex88_a, ex88_a.Determinant());

			Console.WriteLine(Matrix<short>.Signature(3).ToString());

			var t2_7a_c = new Matrix<short>(3, 3, 3, -1, -2, 2, 6, -9, 1, -7, 7);
			var t2_7a_r = new Matrix<short>(3, 1, 19, 68, -49);
			var t2_7a_a = t2_7a_c.Augment(t2_7a_r);
			t2_7a_a.RunCommand(1, -2, 3, null);
			t2_7a_a.RunCommand(2, 1, null, -3);
			t2_7a_a.RunCommand(2, null, 1, -1);
			Console.WriteLine(t2_7a_a);

			var t2_7b_c = new Matrix<short>(3, 3, 3, -1, -2, 2, 6, -9, -12, 6, 7);
			var t2_7b_r = new Matrix<short>(3, 1, 9, 68, -20);
			var t2_7b_a = t2_7b_c.Augment(t2_7b_r);
			t2_7b_a.RunCommand(1, 2, -3, null);
			t2_7b_a.RunCommand(2, 4, null, 1);
			t2_7b_a.RunCommand(2, null, 1, 10);
			Console.WriteLine(t2_7b_a);

			var t2_8A = new Matrix<short>(3, 3, 1, 3, 4, -3, 5, 7, 4, 0, -1);
			var t2_8B = new Matrix<short>(3, 3, 7, -4, 3, 2, -7, -5, -5, 2, 4);
			var t2_8C = new Matrix<short>(1, 3, 1, 2, 4);
			var t2_8D = new Matrix<short>(3, 1, 27, 34, 3);
			var t2_8ra = t2_8A + t2_8B;
			Console.WriteLine(t2_8ra);
			try {
				var t2_8rb = t2_8A + t2_8C;
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
			}
			var t2_8rc = t2_8C * t2_8A;
			Console.WriteLine(t2_8rc);
			try {
				var t3_8rd = t2_8D * t2_8A;
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
			}
			var t2_8re = t2_8A.Augment(Matrix<short>.Identity(t2_8A.Lines));
			t2_8re.RunCommand(1, 3, 1, null);
			t2_8re.RunCommand(2, -4, null, 1);
			t2_8re.RunCommand(2, null, 6, 7);
			t2_8re.RunCommand(0, 5, null, 4);
			t2_8re.RunCommand(1, null, 5, 19);
			t2_8re.RunCommand(0, 14, -3, null);
			Console.WriteLine(t2_8re);
			var t2_8ref = t2_8re.Cast<float>();
			t2_8ref.RunCommand(0, 1.0f / 70, null, null);
			t2_8ref.RunCommand(1, null, 1.0f / 70, null);
			t2_8ref.RunCommand(2, null, null, -0.2f);
			//t2_8ref.RoundErrors();
			Console.WriteLine(t2_8ref);
			var t2_8rf = t2_8ref.Right() * t2_8D.Cast<float>();
			t2_8rf.RoundErrors();
			Console.WriteLine(t2_8rf);
		}
	}
}
