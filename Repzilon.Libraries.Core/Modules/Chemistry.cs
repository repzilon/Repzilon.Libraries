//
//  Chemistry.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2024 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Repzilon.Libraries.Core
{
	public static class Chemistry
	{
#if NET40 || NET35 || NET20
		public static readonly IDictionary<string, float> ElementMasses = InitElementMasses();
#else
		public static readonly IReadOnlyDictionary<string, float> ElementMasses = InitElementMasses();
#endif

		public static float AminoAcidIsoelectric(float pKa1, float pKa2,
		byte cationCount_ph1andhalf, float pKaR)
		{
			if (cationCount_ph1andhalf > 2) {
				throw new ArgumentOutOfRangeException("cationCount_ph1andhalf", cationCount_ph1andhalf,
				 "The lateral chain of a amino acid can only form a cation, a dication or no cation at all under very acidic conditions.");
			} else {
				float sum;
				if (cationCount_ph1andhalf == 2) {
					sum = pKa2 + pKaR;
				} else if ((cationCount_ph1andhalf == 1) && (pKaR < pKa2)) {
					sum = pKa1 + pKaR;
				} else {
					sum = pKa1 + pKa2;
				}
				return RoundOff.Error(0.5f * sum);
			}
		}

		public static float AminoAcidIsoelectric(float pKa1, float pKa2)
		{
			return RoundOff.Error(0.5f * (pKa1 + pKa2));
		}

#if NET40 || NET35 || NET20
		private static IDictionary<string, float> InitElementMasses()
#else
		private static IReadOnlyDictionary<string, float> InitElementMasses()
#endif
		{
			var dicMasses = new Dictionary<string, float>
			{
				{ "C", 12.011f },
				{ "H", 1.008f },
				{ "O", 15.999f },
				{ "N", 14.007f },
				{ "P", 30.974f },
				{ "S", 32.06f },
				{ "Na", 22.99f },
				{ "Mg", 24.305f },
				{ "K", 39.098f },
				{ "Ca", 40.078f },
				{ "F", 18.998f },
				{ "Cl", 35.45f  },
				{ "Br", 79.904f },
				{ "I", 126.90f },
				{ "B", 10.81f },
				{ "Fe", 55.845f },
				{ "Co", 58.933f },
				{ "Cu", 63.546f },
				{ "Zn", 65.38f }
			};
#if NET40 || NET35 || NET20
			return new Dictionary<string, float>(dicMasses);
#else
			return new ReadOnlyDictionary<string, float>(dicMasses);
#endif
		}

		private static MatchCollection MatchChemicalGroups(string formula, out int c)
		{
			if (String.IsNullOrEmpty(formula)) {
				throw new ArgumentNullException("formula");
			}

			// Count chemical groups first
			var mccChemicalGroups = Regex.Matches(formula, @"[(]([A-Za-z0-9<>/=-]+)[)](?:<sub>([0-9]+)</sub>)?");
			c = mccChemicalGroups.Count;
			return mccChemicalGroups;
		}

		private static string RemoveChemicalGroups(string formula, MatchCollection mccChemicalGroups, int c)
		{
			// Remove them from the string
			for (int i = c - 1; i >= 0; i--) {
				var mtc = mccChemicalGroups[i];
				var ix = mtc.Index;
				formula = formula.Substring(0, ix) + formula.Substring(ix + mtc.Length);
			}
			return formula;
		}

		public static float MolarMass(string formula)
		{
			float M = 0;
			int c;
			// The following line transforms hydratation to sub-group
			formula = Regex.Replace(formula, "[.•]([0-9]+)\\s*([A-Za-z0-9<>/=-]+)$", "-($2)<sub>$1</sub>");
			var mccChemicalGroups = MatchChemicalGroups(formula, out c);
			for (int i = 0; i < c; i++) {
				var grcIter = mccChemicalGroups[i].Groups;
				M += CoreMolarMass(grcIter[1].Value) * Int32.Parse(grcIter[2].Value);
			}
			formula = RemoveChemicalGroups(formula, mccChemicalGroups, c);
			// Add what is left
			M += CoreMolarMass(formula);
			return (float)Math.Round(M, 3);
		}

		private static float CoreMolarMass(string formula)
		{
			float M = 0;
			var mccElements = Regex.Matches(formula, @"([A-Z][a-z]?)(?:<sub>([0-9]+)</sub>)?");
			var c = mccElements.Count;
			for (int i = 0; i < c; i++) {
				var grcElement = mccElements[i].Groups;
				var strElementCount = grcElement[2].Value;
				var intElementCount = String.IsNullOrEmpty(strElementCount) ? 1 : Int32.Parse(strElementCount);
				M += ElementMasses[grcElement[1].Value] * intElementCount;
			}
			return M;
		}

		public static IDictionary<string, int> ElementComposition(string formula)
		{
			IDictionary<string, int> dicElements = new Dictionary<string, int>();
			int c;
			var mccChemicalGroups = MatchChemicalGroups(formula, out c);
			for (int i = 0; i < c; i++) {
				var grcIter = mccChemicalGroups[i].Groups;
				CoreElementComposition(dicElements, grcIter[1].Value, Int32.Parse(grcIter[2].Value));
			}
			formula = RemoveChemicalGroups(formula, mccChemicalGroups, c);
			// Add what is left
			CoreElementComposition(dicElements, formula, 1);
			return dicElements;
		}

		private static void CoreElementComposition(IDictionary<string, int> counts, string formula, int multiplicator)
		{
			var mccElements = Regex.Matches(formula, @"([A-Z][a-z]?)(?:<sub>([0-9]+)</sub>)?");
			var c = mccElements.Count;
			for (int i = 0; i < c; i++) {
				var grcElement = mccElements[i].Groups;
				var strElementCount = grcElement[2].Value;
				var intElementCount = String.IsNullOrEmpty(strElementCount) ? 1 : Int32.Parse(strElementCount);
				var strSymbol = grcElement[1].Value;
				int e;
				if (counts.TryGetValue(strSymbol, out e)) {
					counts[strSymbol] = e + (intElementCount * multiplicator);
				} else {
					counts.Add(strSymbol, intElementCount * multiplicator);
				}
			}
		}
	}
}
