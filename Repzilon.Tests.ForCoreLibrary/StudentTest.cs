//
//  StudentTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024 René Rhéaume
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
			Console.WriteLine("Distributions de Student");
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
			TimeSpan tsNew = DateTime.UtcNow - dtmStart;
			Console.WriteLine("Implémentation accélérée de GammaRatio : {0,6:n0} Hz", kStudentLoop / tsNew.TotalSeconds);

			byte[] karLiberties = new byte[] { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233 };
			TenthTableHeader(" k={0,-6}", karLiberties);
			for (x = -30; x <= 30; x++) {
				z = TenthTableLineHeader(x);
				for (k = 0; k < karLiberties.Length; k++) {
					Console.Write(" {0:f6}",
					 ProbabilityDistributions.Student(z, karLiberties[k], false));
				}
				Console.Write(Environment.NewLine);
			}

			Console.WriteLine("Intégrales de Student de faibles degrés de liberté");
			TenthTableHeader(" k={0} S      k={0} C     ", 1, 2, 3, 4, 5);
			for (x = -30; x <= 30; x++) {
				z = TenthTableLineHeader(x);
				for (k = 1; k <= 5; k++) {
					Console.Write(" {0:f8} {1:f8}", ProbabilityDistributions.Student(z, (byte)k, true),
					 ProbabilityDistributions.CumulativeStudent(z, (byte)k));
				}
				Console.Write(Environment.NewLine);
			}
		}

		private static void TenthTableHeader(string format, params byte[] liberties)
		{
			Console.Write("x    ");
			for (int i = 0; i < liberties.Length; i++) {
				Console.Write(format, liberties[i]);
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
