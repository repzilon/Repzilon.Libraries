//
//  MatrixTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Collections.Generic;
using Repzilon.Libraries.Core;
// ReSharper disable InvokeAsExtensionMethod
// ReSharper disable InconsistentNaming

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class MatrixTest
	{
		internal static void Run(string[] args)
		{
#if !NET20
			Program.OutputHeading("Exemple 80");
			var ex80_a = new Matrix<short>(3, 3, 2, -1, 5, -3, 4, 7, 1, -1, 0);
			var ex80_b = new Matrix<short>(3, 3, 1, 4, 0, -3, 6, -5, 1, 0, -1);
			var ex80_result = (3 * ex80_a) + ex80_b;
			OutputMatrix(ex80_a);
			OutputMatrix(ex80_b);
			OutputMatrix(ex80_result);
#endif

			Program.OutputHeading("Exemple 81");
			var ex81_a = new Matrix<short>(2, 2, 1, 3, -2, 1);
			var ex81_b = new Matrix<short>(2, 3, 1, 2, 1, -3, 4, -5);
			try {
				OutputMatrix(ex81_a * ex81_b);
				OutputMatrix(ex81_b * ex81_a);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			}

			Program.OutputHeading("Exemple 82");
			var ex82_m = new Matrix<double>(3, 3, 1.4, 1.2, 4.1, 1.4, 2.2, 3.7, 1.8, 3.2, 3.9);
			var ex82_a = new Matrix<double>(3, 3, 0.3, 0.3, 0.3, 0.7, 0.7, 0.7, -0.2, -0.2, -0.2);
			var ex82_ma = ex82_m + ex82_a;
			MatrixExtensionMethods.RoundErrors(ex82_ma);
			OutputMatrix(ex82_ma);
#if !NET20
			var ex82_3m = 3 * ex82_m;
			MatrixExtensionMethods.RoundErrors(ex82_3m);
			var ex82_l = new Matrix<short>(3, 1, 5, 15, 20).Cast<double>();
			OutputMatrix(ex82_3m);
			OutputMatrix(ex82_m * ex82_l);
#endif

			Program.OutputHeading("Exercices papier");
			var pap_m = new Matrix<short>(3, 3, 4, 3, 5, -3, -7, 1, 8, 0, 0);
			var pap_n = new Matrix<short>(3, 3, 4, 5, 1, 0, 1, -4, 6, 2, -1);
			OutputMatrix(pap_m + pap_n);
#if !NET20
			OutputMatrix((2 * pap_n) - (5 * pap_m));
#endif
			OutputMatrix(pap_m * pap_n);
			OutputMatrix(pap_n * pap_m);

			Program.OutputHeading("Exemple 83");
			var ex83_c = new Matrix<short>(2, 2, 4, 3, 2, -1);
			var ex83_s = new Matrix<short>(2, 1, -7, 9);
			var ex83_plus = ex83_c | ex83_s;
			ex83_plus.RunCommand(1, 1, -2);
			OutputMatrix(ex83_plus);
			TrySolve("", ex83_c, ex83_s, 'x', 'y');

			Program.OutputHeading("Exemple 84");
			var ex84_ac = new Matrix<short>(3, 3, 1, -1, 1, -1, 2, 2, 2, 1, 3);
			var ex84_as = new Matrix<short>(3, 1, -2, 1, 1);
			var ex84_aa = ex84_ac | ex84_as;
			ex84_aa.RunCommand(1, 1, 1, null);
			ex84_aa.RunCommand(2, -2, null, 1);
			ex84_aa.RunCommand(2, null, -3, 1);
			OutputMatrix(ex84_aa);
			var ex84_bc = new Matrix<short>(3, 3, 3, 2, -2, 1, -1, 4, 8, 7, -10);
			var ex84_bs = new Matrix<short>(3, 1, 1, -9, 5);
			var ex84_ba = ex84_bc | ex84_bs;
			ex84_ba.RunCommand(1, 1, -3, null);
			ex84_ba.RunCommand(2, 8, null, -3);
			ex84_ba.RunCommand(2, null, 1, 1);
			OutputMatrix(ex84_ba);
			var ex84_cc = new Matrix<short>(3, 3, 4, 9, -1, 1, 2, -1, 2, 5, 1);
			var ex84_cs = new Matrix<short>(3, 1, 17, 4, 9);
			var ex84_ca = ex84_cc | ex84_cs;
			ex84_ca.RunCommand(1, 1, -4, null);
			ex84_ca.RunCommand(2, 1, null, -2);
			ex84_ca.RunCommand(2, null, 1, 1);
			OutputMatrix(ex84_ca);
			TrySolve("a) ", ex84_ac, ex84_as, 'x', 'y', 'z');
			TrySolve("b) ", ex84_bc, ex84_bs, 'x', 'y', 'z');
			TrySolve("c) ", ex84_cc, ex84_cs, 'x', 'y', 'z');

			Program.OutputHeading("Exemple 85");
			var ex85_c = new Matrix<float>(3, 4, 4, 0, -1, 0, 10, 0, 0, -2, 0, 2, -2, -1);
			var ex85_s = new Matrix<float>(3, 1, 0, 0, 0);
			var ex85_a = ex85_c | ex85_s;
			ex85_a.SwapLines(1, 2);
			ex85_a.RunCommand(2, -10, null, 4);
			OutputMatrix(ex85_a);
			TrySolve("", ex85_c, ex85_s, 'x', 'y', 'z', 'w');

			Program.OutputHeading("Exemple 86");
			var ex86_s = new Matrix<double>(3, 1, 61.6, 68.4, 84.8);
			var ex86_a = ex82_m | ex86_s;
			ex86_a.RunCommand(1, -1, 1, null);
			ex86_a.RunCommand(2, 9, null, -7);
			ex86_a.RunCommand(1, null, 0.5f, null);
			ex86_a.RunCommand(2, null, null, 0.25f);
			ex86_a.RunCommand(2, null, 29, 5);
			MatrixExtensionMethods.RoundErrors(ex86_a);
			OutputMatrix(ex86_a);
			TrySolve("", ex82_m.Cast<decimal>(), ex86_s.Cast<decimal>(), 'x', 'y', 'z');

			Program.OutputHeading("Exemple 87");
			var ex87_a = new Matrix<short>(3, 3, 2, 1, -1, 3, -3, 1, 1, -2, 1);
			var ex87_ai = MatrixExtensionMethods.AugmentWithIdentity(ex87_a);
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
			OutputMatrix(ex87_aif);
			OutputMatrix(~ex87_a);
			var ex87_b = new Matrix<short>(3, 3, 2, 1, -1, 0, -2, 1, 6, 1, -2);
			var ex87_bi = MatrixExtensionMethods.AugmentWithIdentity(ex87_b);
			ex87_bi.RunCommand(2, -3, null, 1);
			ex87_bi.RunCommand(2, null, 1, -1);
			OutputMatrix(ex87_bi);
			OutputMatrix(~ex87_b);

			Program.OutputHeading("Exemple 88");
			var ex88_a = new Matrix<short>(3, 3, 2, 1, -1, 3, -3, 1, 1, -2, 1);
			var ex88_b = new Matrix<short>(3, 1, 1, 16, 9);
			var ex88_plus = ex88_a | ex88_b;
			OutputMatrix(ex88_plus);
			OutputMatrix(MatrixExtensionMethods.AugmentWithIdentity(ex88_a));
			var ex88_m1 = ~ex88_a;
			OutputMatrix(ex88_m1);
			OutputMatrix(ex88_m1 * ex88_b);
			TrySolve("", ex88_a, ex88_b, 'x', 'y', 'z');

			Program.OutputHeading("Exemple 89");
			var ex89_a = new Matrix<short>(3, 3, 2, 1, -4, 3, 1, 5, -2, 8, 7);
			OutputExample89(ex89_a, (short)2);
			OutputExample89(ex89_a, (short)5);

			Program.OutputHeading("Exemple 90");
			var ex_89 = new Matrix<short>(2, 2, 3, 7, 2, 4);
			Console.WriteLine("{0} det(M) = {1}", ToStringCompat(ex_89), ex_89.Determinant());

			Program.OutputHeading("Exemple 91");
			Console.WriteLine("{0} det(M) = {1}", ToStringCompat(ex88_a), ex88_a.Determinant());
			OutputMatrix(Matrix<short>.Signature(3));

			Program.OutputHeading("Exemple 92");
			OutputSolution("a) ", ex83_c.Solve(ex83_s, 'x', 'y'));
			OutputSolution("b) ", ex88_a.Solve(ex88_b, 'x', 'y', 'z'));

			Program.OutputHeading("Travail 2 no 7");
			var t2_7a_c = new Matrix<short>(3, 3, 3, -1, -2, 2, 6, -9, 1, -7, 7);
			var t2_7a_r = new Matrix<short>(3, 1, 19, 68, -49);
			var t2_7a_a = t2_7a_c | t2_7a_r;
			t2_7a_a.RunCommand(1, -2, 3, null);
			t2_7a_a.RunCommand(2, 1, null, -3);
			t2_7a_a.RunCommand(2, null, 1, -1);
			OutputMatrix(t2_7a_a);

			var t2_7b_c = new Matrix<short>(3, 3, 3, -1, -2, 2, 6, -9, -12, 6, 7);
			var t2_7b_r = new Matrix<short>(3, 1, 9, 68, -20);
			var t2_7b_a = t2_7b_c | t2_7b_r;
			t2_7b_a.RunCommand(1, 2, -3, null);
			t2_7b_a.RunCommand(2, 4, null, 1);
			t2_7b_a.RunCommand(2, null, 1, 10);
			OutputMatrix(t2_7b_a);

			Program.OutputHeading("Travail 2 no 8");
			var t2_8A = new Matrix<short>(3, 3, 1, 3, 4, -3, 5, 7, 4, 0, -1);
			var t2_8B = new Matrix<short>(3, 3, 7, -4, 3, 2, -7, -5, -5, 2, 4);
			var t2_8C = new Matrix<short>(1, 3, 1, 2, 4);
			var t2_8D = new Matrix<short>(3, 1, 27, 34, 3);
			OutputMatrix(t2_8A + t2_8B);
			try {
				OutputMatrix(t2_8A + t2_8C);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			}
			OutputMatrix(t2_8C * t2_8A);
			try {
				OutputMatrix(t2_8D * t2_8A);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			}
			var t2_8re = MatrixExtensionMethods.AugmentWithIdentity(t2_8A);
			t2_8re.RunCommand(1, 3, 1, null);
			t2_8re.RunCommand(2, -4, null, 1);
			t2_8re.RunCommand(2, null, 6, 7);
			t2_8re.RunCommand(0, 5, null, 4);
			t2_8re.RunCommand(1, null, 5, 19);
			t2_8re.RunCommand(0, 14, -3, null);
			OutputMatrix(t2_8re);
			var t2_8ref = t2_8re.Cast<float>();
			t2_8ref.RunCommand(0, 1.0f / 70, null, null);
			t2_8ref.RunCommand(1, null, 1.0f / 70, null);
			t2_8ref.RunCommand(2, null, null, -0.2f);
			OutputMatrix(t2_8ref);
			var t2_8rf = t2_8ref.Right() * t2_8D.Cast<float>();
			MatrixExtensionMethods.RoundErrors(t2_8rf);
			OutputMatrix(t2_8rf);

			Program.OutputHeading("Physicochimie laboratoire 10");
			var fql10_coef = new Matrix<float>(3, 4, 7, 0, -1, 0, 6, 0, 0, -2, 2, 2, -2, -1);
			var fql10_k = new Matrix<float>(3, 1, 0, 0, 0);
			var fql10_a = fql10_coef | fql10_k;
			OutputMatrix(fql10_a);
			fql10_a.RunCommand(1, null, 1, -3);
			fql10_a.RunCommand(2, 2, null, -7);
			fql10_a.RunCommand(2, null, -7, 3);
			OutputMatrix(fql10_a);
			TrySolve("", fql10_coef, fql10_k, 'b', 'o', 'c', 'h');
		}

		private static void TrySolve<T>(string prefix, Matrix<T> coefficients, Matrix<T> constants,
		params char[] variables)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			try {
				OutputSolution(prefix, coefficients.Solve(constants, variables));
			} catch (Exception ex) {
				Console.Error.Write(prefix);
				Console.Error.WriteLine(ex.Message);
			}
		}

		private static void OutputSolution<T>(string prefix, IReadOnlyDictionary<char, T> solution)
		{
			Console.Write(prefix);
			if ((solution == null) || (solution.Count < 1)) {
				Console.Write("Aucune solution");
			} else {
				foreach (var kvp in solution) {
					Console.Write("{0}={1}; ", kvp.Key, kvp.Value);
				}
			}
			Console.WriteLine();
		}

		private static void OutputExample89<T>(Matrix<T> matrix, T valueToFind)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			var coords = matrix.Find(valueToFind);
			if (coords != null) {
				var x = coords.Value.X;
				var y = coords.Value.Y;
				Console.WriteLine("a({1};{2})={0} sig={3} M({1};{2})=", valueToFind, x + 1, y + 1,
				 MatrixExtensionMethods.Signature(x, y));
				OutputMatrix(matrix.Minor(x, y));
			}
		}

		private static void OutputMatrix<T>(Matrix<T> matrix)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			Console.WriteLine(ToStringCompat(matrix));
		}

		private static void OutputMatrix<T>(Matrix<T>? matrix)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			if (matrix.HasValue) {
				OutputMatrix(matrix.Value);
			} else {
				Console.WriteLine("null");
			}
		}

		private static string ToStringCompat<T>(Matrix<T> matrix)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			var strOut =  matrix.ToString();
			if (Program.UnicodeTerminal != SupportLevel.Complete) {
				strOut = strOut.Replace('⎡', '/').Replace('⎤', '\\').Replace('⎣', '\\').Replace('⎦', '/');
			}
			return strOut;
		}
	}
}
