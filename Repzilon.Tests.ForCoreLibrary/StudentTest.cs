//
//  StudentTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024-2025 René Rhéaume
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
	internal static class StudentTest
	{
		internal static void Run(string[] args)
		{
			Program.OutputHeading("Distributions de Student");
			const int kStudentLoop = (300 - -300 + 1) * (255 - 1 + 1);
			int k, x;
			double z;
			var dtmStart = DateTime.UtcNow;
			for (x = -300; x <= 300; x++) {
				z = RoundOff.Error(x * 0.01);
				for (k = 1; k <= 255; k++) {
					try {
						var t = ProbabilityDistributions.Student(z, (byte)k, false);
					} catch (OverflowException exO) {
#if NETCOREAPP1_0
						throw new Exception(
#else
						throw new ApplicationException(
#endif
						 String.Format("Got an overflow with Student({0:f2}, {1})", z, k), exO);
					}
				}
			}
			var tsNew = DateTime.UtcNow - dtmStart;
			Console.WriteLine("Implémentation accélérée de GammaRatio : {0,6:n0} Hz", kStudentLoop / tsNew.TotalSeconds);

			var karLiberties = new byte[] { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233 };
			TenthTableHeader(" k={0,-6}", karLiberties);
			for (x = -30; x <= 30; x++) {
				z = TenthTableLineHeader(x);
				for (k = 0; k < karLiberties.Length; k++) {
					Console.Write(" {0:f6}",
					 ProbabilityDistributions.Student(z, karLiberties[k], false));
				}
				Console.Write(Environment.NewLine);
			}

			Program.OutputHeading("Intégrales de Student de faibles degrés de liberté");
			TenthTableHeader(" k={0} e-X        ", 1, 2, 3, 4, 5, 6, 7);
			decimal totalDiff = 0;
			for (x = -30; x <= 30; x++) {
				z = TenthTableLineHeader(x);
				for (k = 1; k <= 7; k++) {
					var delta = ProbabilityDistributions.CumulativeStudentEstimate(z, (byte)k) -
					 ProbabilityDistributions.Student(z, (byte)k, true);
					totalDiff += (decimal)Math.Abs(delta);
					Console.Write(" {1}{0,12:e7}", delta, delta < 0 ? "" : " ");
				}
				Console.Write(Environment.NewLine);
			}
			totalDiff /= (61 * 7);
			var dcmTarget = NormalLawTest.FinalTargetDelta();
			Console.WriteLine("Moyenne des différences : {0:e} i.e. {2} fois la cible de {1:e}",
			 totalDiff, dcmTarget, totalDiff / dcmTarget);

			Program.OutputHeading("Réciproques d'intégrales de distributions de Student");
			// ReSharper disable once RedundantExplicitArrayCreation
			var karAlphas = new float[] { 0.4f, 0.25f, 0.1f, 0.05f, 0.025f, 0.010f, 0.005f, 0.0025f, 0.001f, 0.0005f };
			TableHeader("nu/p", " {0,9:f4}", karAlphas);
			for (k = 1; k <= 40; k++) {
				InverseStudentTableRow(k, karAlphas);
			}
			for (k = 60; k <= 120; k += 20) {
				InverseStudentTableRow(k, karAlphas);
			}
		}

		private static void InverseStudentTableRow(int k, float[] karAlphas)
		{
			Console.Write("{0,4}", k);
			for (int x = 0; x < karAlphas.Length; x++) {
				Console.Write(" {0,9:g6}", ProbabilityDistributions.InverseStudent(RoundOff.Error(karAlphas[x]), (byte)k));
			}
			Console.Write(Environment.NewLine);
		}

		private static void TenthTableHeader(string format, params byte[] liberties)
		{
			TableHeader("x    ", format, liberties);
		}

		private static void TableHeader<T>(string corner, string format, params T[] columns)
		{
			Console.Write(corner);
			for (int i = 0; i < columns.Length; i++) {
				Console.Write(format, columns[i]);
			}
			Console.Write(Environment.NewLine);
		}

		private static double TenthTableLineHeader(int x)
		{
			var z = RoundOff.Error(x * 0.1);
			if (x >= 0) {
				Console.Write(' ');
			}
			Console.Write(z.ToString("f1"));
			Console.Write(' ');
			return z;
		}
	}
}
