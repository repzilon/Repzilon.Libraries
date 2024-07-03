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
#if !(NET40 || NET35 || NET20)
using System.Collections.ObjectModel;
#endif
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
		byte cationCountAtPh1AndHalf, float pKaR)
		{
			if (cationCountAtPh1AndHalf > 2) {
				throw new ArgumentOutOfRangeException("cationCountAtPh1AndHalf", cationCountAtPh1AndHalf,
				 "The lateral chain of a amino acid can only form a cation, a dication or no cation at all under very acidic conditions.");
			} else {
				float sum;
				if (cationCountAtPh1AndHalf == 2) {
					sum = pKa2 + pKaR;
				} else if ((cationCountAtPh1AndHalf == 1) && (pKaR < pKa2)) {
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
			const int k = 19;
			var dicMasses = new Dictionary<string, float>(k);
			var karSymbols = new string[k]
				{ "C", "H", "O", "N", "P", "S", "Na", "Mg", "K", "Ca", "F", "Cl", "Br", "I", "B", "Fe", "Co", "Cu", "Zn" };
			var karMasses = new float[k] 
				{ 12.011f, 1.008f, 15.999f, 14.007f, 30.974f, 32.06f, 22.99f, 24.305f, 39.098f, 40.078f, 18.998f, 35.45f, 79.904f, 126.90f, 10.81f, 55.845f, 58.933f, 63.546f, 65.38f };
			for (int i = 0; i < k; i++) {
				dicMasses.Add(karSymbols[i], karMasses[i]);
			}

#if NET40 || NET35 || NET20
			return dicMasses;
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
			float mass = 0;
			int c;
			// The following line transforms hydration to subgroup
			formula = Regex.Replace(formula, "[.•]([0-9]+)\\s*([A-Za-z0-9<>/=-]+)$", "-($2)<sub>$1</sub>");
			var mccChemicalGroups = MatchChemicalGroups(formula, out c);
			for (int i = 0; i < c; i++) {
				var grcIter = mccChemicalGroups[i].Groups;
				mass += CoreMolarMass(grcIter[1].Value) * Int32.Parse(grcIter[2].Value);
			}
#if DEBUG
			formula = RemoveChemicalGroups(formula, mccChemicalGroups, c);
			// Add what is left
			mass += CoreMolarMass(formula);
			return (float)Math.Round(mass, 3);
#else
			// Add what is left
			return (float)Math.Round(mass + CoreMolarMass(RemoveChemicalGroups(formula, mccChemicalGroups, c)), 3);
#endif
		}

		private static float CoreMolarMass(string formula)
		{
			float mass = 0;
			var mccElements = Regex.Matches(formula, @"([A-Z][a-z]?)(?:<sub>([0-9]+)</sub>)?");
			var c = mccElements.Count;
			for (int i = 0; i < c; i++) {
				var grcElement = mccElements[i].Groups;
				var strElementCount = grcElement[2].Value;
#if DEBUG
				var intElementCount = String.IsNullOrEmpty(strElementCount) ? 1 : Int32.Parse(strElementCount);
				mass += ElementMasses[grcElement[1].Value] * intElementCount;
#else
				mass += ElementMasses[grcElement[1].Value] * (String.IsNullOrEmpty(strElementCount) ? 1 : Int32.Parse(strElementCount));
#endif
			}
			return mass;
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
				var strSymbol = grcElement[1].Value;
				int e;
				counts.TryGetValue(strSymbol, out e);
#if DEBUG				
				var intElementCount = String.IsNullOrEmpty(strElementCount) ? 1 : Int32.Parse(strElementCount);
				counts[strSymbol] = e + (intElementCount * multiplicator);
#else
				counts[strSymbol] = e + ((String.IsNullOrEmpty(strElementCount) ? 1 : Int32.Parse(strElementCount)) * multiplicator);
#endif				
			}
		}
	}
}
