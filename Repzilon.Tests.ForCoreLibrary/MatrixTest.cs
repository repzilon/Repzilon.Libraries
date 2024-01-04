//
//  MatrixTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2024 René Rhéaume
//
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v. 2.0. If a copy of the MPL was 
// not distributed with this file, You can obtain one at 
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Collections.Generic;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	static class MatrixTest
	{
		internal static void Run(string[] args)
		{
			Console.WriteLine("Exemple 80 :");
			var ex80_a = new Matrix<short>(3, 3, 2, -1, 5, -3, 4, 7, 1, -1, 0);
			var ex80_b = new Matrix<short>(3, 3, 1, 4, 0, -3, 6, -5, 1, 0, -1);
			var ex80_result = (3 * ex80_a) + ex80_b;
			Console.WriteLine(ex80_a);
			Console.WriteLine(ex80_b);
			Console.WriteLine(ex80_result);

			Console.WriteLine("Exemple 81 :");
			var ex81_a = new Matrix<short>(2, 2, 1, 3, -2, 1);
			var ex81_b = new Matrix<short>(2, 3, 1, 2, 1, -3, 4, -5);
			try {
				var ex81_ab = ex81_a * ex81_b;
				Console.WriteLine(ex81_ab);
				var ex81_ba = ex81_b * ex81_a;
				Console.WriteLine(ex81_ba);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			}

			Console.WriteLine("Exemple 82 :");
			var ex82_m = new Matrix<double>(3, 3, 1.4, 1.2, 4.1, 1.4, 2.2, 3.7, 1.8, 3.2, 3.9);
			var ex82_a = new Matrix<double>(3, 3, 0.3, 0.3, 0.3, 0.7, 0.7, 0.7, -0.2, -0.2, -0.2);
			var ex82_ma = ex82_m + ex82_a;
			var ex82_3m = 3 * ex82_m;
			ex82_ma.RoundErrors();
			ex82_3m.RoundErrors();
			var ex82_l = new Matrix<short>(3, 1, 5, 15, 20).Cast<double>();
			var ex82_mxl = ex82_m * ex82_l;
			Console.WriteLine(ex82_ma);
			Console.WriteLine(ex82_3m);
			Console.WriteLine(ex82_mxl);

			Console.WriteLine("Exercices papier :");
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

			Console.WriteLine("Exemple 83 :");
			var ex83_c = new Matrix<short>(2, 2, 4, 3, 2, -1);
			var ex83_s = new Matrix<short>(2, 1, -7, 9);
			var ex83_plus = ex83_c | ex83_s;
			ex83_plus.RunCommand(1, 1, -2);
			Console.WriteLine(ex83_plus);

			Console.WriteLine("Exemple 84 :");
			var ex84_ac = new Matrix<short>(3, 3, 1, -1, 1, -1, 2, 2, 2, 1, 3);
			var ex84_as = new Matrix<short>(3, 1, -2, 1, 1);
			var ex84_aa = ex84_ac | ex84_as;
			ex84_aa.RunCommand(1, 1, 1, null);
			ex84_aa.RunCommand(2, -2, null, 1);
			ex84_aa.RunCommand(2, null, -3, 1);
			Console.WriteLine(ex84_aa);
			var ex84_bc = new Matrix<short>(3, 3, 3, 2, -2, 1, -1, 4, 8, 7, -10);
			var ex84_bs = new Matrix<short>(3, 1, 1, -9, 5);
			var ex84_ba = ex84_bc | ex84_bs;
			ex84_ba.RunCommand(1, 1, -3, null);
			ex84_ba.RunCommand(2, 8, null, -3);
			ex84_ba.RunCommand(2, null, 1, 1);
			Console.WriteLine(ex84_ba);
			var ex84_cc = new Matrix<short>(3, 3, 4, 9, -1, 1, 2, -1, 2, 5, 1);
			var ex84_cs = new Matrix<short>(3, 1, 17, 4, 9);
			var ex84_ca = ex84_cc | ex84_cs;
			ex84_ca.RunCommand(1, 1, -4, null);
			ex84_ca.RunCommand(2, 1, null, -2);
			ex84_ca.RunCommand(2, null, 1, 1);
			Console.WriteLine(ex84_ca);
			TrySolve("a) ", ex84_ac, ex84_as, "x", "y", "z");
			TrySolve("b) ", ex84_bc, ex84_bs, "x", "y", "z");
			TrySolve("c) ", ex84_cc, ex84_cs, "x", "y", "z");

			Console.WriteLine("Exemple 85 :");
			var ex85_c = new Matrix<short>(3, 4, 4, 0, -1, 0, 10, 0, 0, -2, 0, 2, -2, -1);
			var ex85_s = new Matrix<short>(3, 1, 0, 0, 0);
			var ex85_a = ex85_c | ex85_s;
			ex85_a.SwapLines(1, 2);
			ex85_a.RunCommand(2, -10, null, 4);
			Console.WriteLine(ex85_a);
			TrySolve("", ex85_c, ex85_s, "x", "y", "z", "w");

			Console.WriteLine("Exemple 86 :");
			var ex86_s = new Matrix<double>(3, 1, 61.6, 68.4, 84.8);
			var ex86_a = ex82_m | ex86_s;
			ex86_a.RunCommand(1, -1, 1, null);
			ex86_a.RunCommand(2, 9, null, -7);
			ex86_a.RunCommand(1, null, 0.5f, null);
			ex86_a.RunCommand(2, null, null, 0.25f);
			ex86_a.RunCommand(2, null, 29, 5);
			ex86_a.RoundErrors();
			Console.WriteLine(ex86_a);
			TrySolve("", ex82_m.Cast<decimal>(), ex86_s.Cast<decimal>(), "x", "y", "z");

			Console.WriteLine("Exemple 87 :");
			var ex87_a = new Matrix<short>(3, 3, 2, 1, -1, 3, -3, 1, 1, -2, 1);
			var ex87_ai = ex87_a.AugmentWithIdentity();
			ex87_ai.RunCommand(1, -3, 2, null);
			ex87_ai.RunCommand(2, -1, null, 2);
			ex87_ai.RunCommand(2, null, -5, 9);
			ex87_ai.RunCommand(0, 2, null, 1);
			ex87_ai.RunCommand(1, null, 2, -5);
			ex87_ai.RunCommand(0, 9, 1, null);
			var ex87_aif = ex87_ai.Cast<float>();
			ex87_aif.RunCommand(0, 1.0f / 36, null, null);
			ex87_aif.RunCommand(1, null, -1.0f / 18, null);
			ex87_aif.RunCommand(2, null, null, 0.5f);
			Console.WriteLine(ex87_aif);
			Console.WriteLine(~ex87_a);
			var ex87_b = new Matrix<short>(3, 3, 2, 1, -1, 0, -2, 1, 6, 1, -2);
			var ex87_bi = ex87_b.AugmentWithIdentity();
			ex87_bi.RunCommand(2, -3, null, 1);
			ex87_bi.RunCommand(2, null, 1, -1);
			Console.WriteLine(ex87_bi);
			Console.WriteLine(~ex87_b);

			Console.WriteLine("Exemple 88 :");
			var ex88_a = new Matrix<short>(3, 3, 2, 1, -1, 3, -3, 1, 1, -2, 1);
			var ex88_b = new Matrix<short>(3, 1, 1, 16, 9);
			var ex88_plus = ex88_a | ex88_b;
			Console.WriteLine(ex88_plus);
			Console.WriteLine(ex88_a.AugmentWithIdentity());
			var ex88_m1 = ~ex88_a;
			Console.WriteLine(ex88_m1);
			Console.WriteLine(ex88_m1 * ex88_b);
			TrySolve("", ex88_a, ex88_b, "x", "y", "z");

			Console.WriteLine("Exemple 89 :");
			var ex89_a = new Matrix<short>(3, 3, 2, 1, -4, 3, 1, 5, -2, 8, 7);
			OutputExample89(ex89_a, (short)2);
			OutputExample89(ex89_a, (short)5);

			Console.WriteLine("Exemple 90 :");
			var ex_89 = new Matrix<short>(2, 2, 3, 7, 2, 4);
			Console.WriteLine("{0} det(M) = {1}", ex_89, ex_89.Determinant());

			Console.WriteLine("Exemple 91 :");
			Console.WriteLine("{0} det(M) = {1}", ex88_a, ex88_a.Determinant());
			Console.WriteLine(Matrix<short>.Signature(3));

			Console.WriteLine("Exemple 92 :");
			OutputSolution("a) ", ex83_c.Solve(ex83_s, "x", "y"));
			OutputSolution("b) ", ex88_a.Solve(ex88_b, "x", "y", "z"));

			Console.WriteLine("Travail 2 #7 :");
			var t2_7a_c = new Matrix<short>(3, 3, 3, -1, -2, 2, 6, -9, 1, -7, 7);
			var t2_7a_r = new Matrix<short>(3, 1, 19, 68, -49);
			var t2_7a_a = t2_7a_c | t2_7a_r;
			t2_7a_a.RunCommand(1, -2, 3, null);
			t2_7a_a.RunCommand(2, 1, null, -3);
			t2_7a_a.RunCommand(2, null, 1, -1);
			Console.WriteLine(t2_7a_a);

			var t2_7b_c = new Matrix<short>(3, 3, 3, -1, -2, 2, 6, -9, -12, 6, 7);
			var t2_7b_r = new Matrix<short>(3, 1, 9, 68, -20);
			var t2_7b_a = t2_7b_c | t2_7b_r;
			t2_7b_a.RunCommand(1, 2, -3, null);
			t2_7b_a.RunCommand(2, 4, null, 1);
			t2_7b_a.RunCommand(2, null, 1, 10);
			Console.WriteLine(t2_7b_a);

			Console.WriteLine("Travail 2 #8 :");
			var t2_8A = new Matrix<short>(3, 3, 1, 3, 4, -3, 5, 7, 4, 0, -1);
			var t2_8B = new Matrix<short>(3, 3, 7, -4, 3, 2, -7, -5, -5, 2, 4);
			var t2_8C = new Matrix<short>(1, 3, 1, 2, 4);
			var t2_8D = new Matrix<short>(3, 1, 27, 34, 3);
			var t2_8ra = t2_8A + t2_8B;
			Console.WriteLine(t2_8ra);
			try {
				var t2_8rb = t2_8A + t2_8C;
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			}
			var t2_8rc = t2_8C * t2_8A;
			Console.WriteLine(t2_8rc);
			try {
				var t3_8rd = t2_8D * t2_8A;
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			}
			var t2_8re = t2_8A.AugmentWithIdentity();
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

		private static void TrySolve<T>(string prefix, Matrix<T> coefficients, Matrix<T> constants,
		params string[] variables)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			try {
				OutputSolution(prefix, coefficients.Solve(constants, variables));
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			}
		}

		private static void OutputSolution<T>(string prefix, IReadOnlyDictionary<string, T> solution)
		{
			Console.Write(prefix);
			if ((solution == null) || (solution.Count < 1)) {
				Console.Write("Aucune solution");
			} else {
				foreach (var kvp in solution) {
					Console.Write("{0}={1}; ", kvp.Key, kvp.Value);
				}
			}
			Console.Write(Environment.NewLine);
		}

		private static void OutputExample89<T>(Matrix<T> matrix, T valueToFind)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			var coords = matrix.Find(valueToFind);
			if (coords != null) {
				Console.WriteLine("a({1};{2})={0} sig={3} M({1};{2})=", valueToFind, coords[0] + 1, coords[1] + 1,
				 Matrix<short>.Signature(coords[0], coords[1]));
				Console.WriteLine(matrix.Minor(coords[0], coords[1]));
			}
		}
	}
}
