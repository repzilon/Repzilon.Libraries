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
			var exa17_m = Solution.Init(20, "%", 50, "mL");
			var exa17_f = Solution.InitMany(200, "mL", "%", 2, 1);
			var exa17_out = Dilution.Direct(ref exa17_m, exa17_f);
			OutputDirectDilution("Exemple 17 :", exa17_out);

			var exa18_m = Solution.Init(5, "g/L");
			var exa18_s1 = Solution.Init(2, "g/L", 0.15, "L");
			var exa18_s2 = Solution.Init(0.5, "g/L", 0.10, "L");
			var exa18_out = Dilution.Serial(ref exa18_m, exa18_s1, exa18_s2);
			OutputSerialDilution("Exemple 18 :", exa18_out);

			var exer1_12_m = Solution.Init(20, "mol/L");
			var exer1_12_f = Solution.InitMany(15, "mL", "mol/L", 8, 5);
			var exer1_12a_out = Dilution.Direct(ref exer1_12_m, exer1_12_f);
			OutputDirectDilution("Exercices 1 #12 a) :", exer1_12a_out);
			var exer1_12b_out = Dilution.Serial(ref exer1_12_m, exer1_12_f);
			OutputSerialDilution("Exercices 1 #12 b) :", exer1_12b_out);

			var exer1_13_m = Solution.Init(30, "%");
			var exer1_13_f = Solution.InitMany(0.5, "L", "%", 7, 5, 2);
			var exer1_13_out = Dilution.Serial(ref exer1_13_m, exer1_13_f);
			OutputSerialDilution("Exercices 1 #13 :", exer1_13_out);

			var exer1_14_m = Solution.Init(1, "Cm");
			var exer1_14_f = Solution.InitMany(10, "µL", "Cm", 0.25, 0.2, 0.1);
			var exer1_14_out = Dilution.Direct(ref exer1_14_m, exer1_14_f);
			OutputDirectDilution("Exercices 1 #14 ", exer1_14_out);

			var exer1_15_m = Solution.Init(1, "Cm");
			var exer1_15_f = Solution.InitMany(150, "mL", "Cm", 0.5, 0.2, 0.05, 0.02);
			var exer1_15_out = Dilution.Serial(ref exer1_15_m, exer1_15_f);
			OutputSerialDilution("Exercices 1 #15 ", exer1_15_out);
		}

		private static void OutputDirectDilution(string title, params Solution[] children)
		{
			Console.WriteLine(title);
			for (int i = 0; i < children.Length; i++) {
				var child = children[i];
				Console.WriteLine("Pour la solution fille {0}, diluer {1} {2} de solution mère dans {3} {2} de solvant.",
				 i + 1, child.SoluteVolume.Value, child.SolutionVolume.Value.Key, child.SolventVolume.Value);
			}
		}

		private static void OutputSerialDilution(string title, params Solution[] children)
		{
			Console.WriteLine(title);
			for (int i = 0; i < children.Length; i++) {
				var child = children[i];
				Console.WriteLine("Pour la solution fille {0}, prélever {1} {2} de la solution {3} pour la diluer dans {4} {2} de solvant.",
				 i + 1, child.SoluteVolume.Value, child.SolutionVolume.Value.Key,
				 (i < 1) ? "mère" : "fille " + i, child.SolventVolume.Value);
			}
		}
	}
}
