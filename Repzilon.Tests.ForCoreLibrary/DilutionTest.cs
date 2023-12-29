//
//  DilutionTest.cs
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
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	static class DilutionTest
	{
		internal static void Run(string[] args)
		{
			PerformDirectDilution("Exemple 17 :", Solution.Init(20, "%", 50, "mL"), true,
			 Solution.InitMany(200, "mL", "%", 2, 1));

			PerformSerialDilution("Exemple 18 :", Solution.Init(5, "g/L"),
			 Solution.Init(2, "g/L", 0.15f, "L"), Solution.Init(0.5f, "g/L", 0.10f, "L"));

			var exer1_12_m = Solution.Init(20, "mol/L");
			var exer1_12_f = Solution.InitMany(15, "mL", "mol/L", 8, 5);
			PerformDirectDilution("Exercices 1 #12 a) :", exer1_12_m, true, exer1_12_f);
			PerformSerialDilution("Exercices 1 #12 b) :", exer1_12_m, exer1_12_f);

			PerformSerialDilution("Exercices 1 #13 :", Solution.Init(30, "%"),
			 Solution.InitMany(0.5f, "L", "%", 7, 5, 2));

			PerformDirectDilution("Exercices 1 #14 ", Solution.Init(1, "Cm"), true,
			 Solution.InitMany(10, "µL", "Cm", 0.25f, 0.2f, 0.1f));

			PerformSerialDilution("Exercices 1 #15 ", Solution.Init(1, "Cm"),
			 Solution.InitMany(150, "mL", "Cm", 0.5f, 0.2f, 0.05f, 0.02f));
		}

		private static void PerformDirectDilution(string title, Solution mother, bool outputMother, params Solution[] children)
		{
			var blnInitialMotherVolume = mother.SolutionVolume.HasValue;
			OutputDilution(title,
			 "Pour la solution fille {0}, diluer {1} {2} de solution mère dans {3} {2} de solvant.",
			 Dilution.Direct(ref mother, children));
			if (outputMother) {
				var mutionv = mother.SolutionVolume.Value;
				Console.WriteLine("Volume de solution mère {0} : {1} {2}",
				 blnInitialMotherVolume ? "restant" : "requis",
				 mutionv.Value, mutionv.Key);
			}
		}

		private static void PerformSerialDilution(string title, Solution mother, params Solution[] children)
		{
			OutputDilution(title,
			 "Pour la solution fille {0}, prélever {1} {2} de la solution {4} pour la diluer dans {3} {2} de solvant.",
			 Dilution.Serial(ref mother, children));
		}

		private static void OutputDilution(string title, string textPattern, params Solution[] children)
		{
			Console.WriteLine(title);
			for (int i = 0; i < children.Length; i++) {
				var child = children[i];
				Console.WriteLine(textPattern,
				 i + 1, child.SoluteVolume, child.SolutionVolume.Value.Key, child.SolventVolume,
				 (i < 1) ? "mère" : "fille " + i);
			}
		}
	}
}
