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
			var dtmStart = DateTime.UtcNow;
			for (var x = -300; x <= 300; x++) {
				var z = RoundOff.Error(x * 0.01);
				for (var k = 1; k <= 255; k++) {
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
			int i;
			Console.Write("x    ");
			for (i = 0; i < karLiberties.Length; i++) {
				Console.Write(" k={0,-6}", karLiberties[i]);
			}
			Console.Write(Environment.NewLine);
			for (var x = -30; x <= 30; x++) {
				var z = RoundOff.Error(x * 0.1);
				if (x >= 0) {
					Console.Write(' ');
				}
				Console.Write(z.ToString("f1"));
				Console.Write(' ');
				for (i = 0; i < karLiberties.Length; i++) {
					Console.Write(" {0:f6}", ProbabilityDistributions.Student(z, karLiberties[i], false));
				}
				Console.Write(Environment.NewLine);
			}
		}
	}
}

