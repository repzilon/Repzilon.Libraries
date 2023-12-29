//
//  PeptideGuess.cs
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
using System.Collections.Generic;
using System.Linq;

namespace Repzilon.Tests.ForCoreLibrary
{
	static class PeptideGuess
	{
		internal static void Run(string[] args)
		{
			var lstAminoAcids = AminoAcid.AlphaList;
			var dicAminoAcids = new SortedDictionary<string, AminoAcid>();
			for (var i = 0; i < lstAminoAcids.Count; i++) {
				dicAminoAcids.Add(lstAminoAcids[i].Symbol, lstAminoAcids[i]);
			}

			List<List<string>> lstRev10Sym = SolveRevision10WithSymbol(lstAminoAcids, dicAminoAcids);
			OutputArrangements("Révision #10 :", lstRev10Sym);
		}

		private static List<List<string>> SolveRevision10WithSymbol(IReadOnlyList<AminoAcid> lstAminoAcids, SortedDictionary<string, AminoAcid> dicAminoAcids)
		{
			string[] rev10_allowed = new string[] { "Asp", "Val", "Gly", "Tyr", "Ile", "Cys", "Arg", "Leu", "Ala" };

			var rev10_tetra = new List<string>[4];
			rev10_tetra[rev10_tetra.Length - 1] = ExtractSymbols(dicAminoAcids, "Lys", "Arg", "Ala");
			Fill(rev10_tetra, lstAminoAcids);
			RestrictToAvailable(rev10_tetra, rev10_allowed);

			var rev10_hexa_c2 = new List<string>[2];
			rev10_hexa_c2[0] = ExtractSymbols(dicAminoAcids, "Phé", "Tyr", "Trp", "Ile", "Sér", "Thr");
			rev10_hexa_c2[1] = ExtractSymbols(dicAminoAcids, "Lys", "Arg", "Ala", "Phé", "Tyr", "Trp");
			RestrictToAvailable(rev10_hexa_c2, rev10_allowed);

			var rev10_hexa_c4 = new List<string>[4];
			rev10_hexa_c4[0] = ExtractSymbols(dicAminoAcids, "Asp", "Glu");
			rev10_hexa_c4[1] = ExtractSymbols(dicAminoAcids, "Val");
			rev10_hexa_c4[rev10_hexa_c4.Length - 1] = ExtractSymbols(dicAminoAcids, "Lys", "Arg", "Ala", "Phé", "Tyr", "Trp");
			Fill(rev10_hexa_c4, lstAminoAcids);
			RestrictToAvailable(rev10_hexa_c4, rev10_allowed);

			var lstRev10_tetra = PermutationsForQuadSlots(rev10_tetra).Where(x => {
				return x.Exists(y => {
					return (y == "Lys") || (y == "Arg") || (y == "His");
				});
			}).ToList();
			var lstRev10_hexa_c4 = PermutationsForQuadSlots(rev10_hexa_c4);
			var lstRev10_hexa_c2 = PermutationsForTwoSlots(rev10_hexa_c2);
			var lstRev10_hexa = ConcatenatePermutations(lstRev10_hexa_c4, lstRev10_hexa_c2);
			var lstRev10 = ConcatenatePermutations(lstRev10_tetra, lstRev10_hexa).Where(x => {
				return (x[0] == "Gly") && (x[1] == "Leu") &&
				 ((x[2] == "Cys") || (x[2] == "Mét")) &&
				 (x[x.Count - 1] == "Ala");
			}).ToList();
			return lstRev10;
		}

		private static void OutputArrangements(string title, List<List<string>> allArrangements)
		{
			Console.WriteLine(title);
			int n = 1;
			foreach (var candidate in allArrangements) {
				Console.Write("#{0} : ", n);
				for (var i = 0; i < candidate.Count; i++) {
					if (i > 0) {
						Console.Write('-');
					}
					Console.Write(candidate[i]);
				}
				Console.Write(Environment.NewLine);
				n++;
			}
		}

		private static List<List<string>> ConcatenatePermutations(
		List<List<string>> firstPermutations, List<List<string>> secondPermutations)
		{
			var lstOutput = new List<List<string>>();
			for (int i = 0; i < firstPermutations.Count; i++) {
				for (int j = 0; j < secondPermutations.Count; j++) {
					var candidate = new List<string>();
					candidate.AddRange(firstPermutations[i]);
					candidate.AddRange(secondPermutations[j]);
					if (UniqueExceptGly(candidate)) {
						lstOutput.Add(candidate);
					}

					candidate = new List<string>();
					candidate.AddRange(secondPermutations[j]);
					candidate.AddRange(firstPermutations[i]);
					if (UniqueExceptGly(candidate)) {
						lstOutput.Add(candidate);
					}
				}
			}
			return lstOutput;
		}

		private static List<List<string>> PermutationsForQuadSlots(List<string>[] quadSlotted)
		{
			if (quadSlotted.Length != 4) {
				throw new ArgumentException("quadSlotted");
			}

			var lstOutput = new List<List<string>>();
			for (int s0 = 0; s0 < quadSlotted[0].Count; s0++) {
				for (int s1 = 0; s1 < quadSlotted[1].Count; s1++) {
					for (int s2 = 0; s2 < quadSlotted[2].Count; s2++) {
						for (int s3 = 0; s3 < quadSlotted[3].Count; s3++) {
							var candidate = new List<string>(4) {
								quadSlotted[0][s0],
								quadSlotted[1][s1],
								quadSlotted[2][s2],
								quadSlotted[3][s3]
							};
							if (UniqueExceptGly(candidate)) {
								lstOutput.Add(candidate);
							}
						}
					}
				}
			}
			return lstOutput;
		}

		private static List<List<string>> PermutationsForTwoSlots(List<string>[] doubleSlotted)
		{
			if (doubleSlotted.Length != 2) {
				throw new ArgumentException("doubleSlotted");
			}

			var lstOutput = new List<List<string>>();
			for (int s0 = 0; s0 < doubleSlotted[0].Count; s0++) {
				for (int s1 = 0; s1 < doubleSlotted[1].Count; s1++) {
					var candidate = new List<string>(2) {
						doubleSlotted[0][s0],
						doubleSlotted[1][s1]
					};
					if (UniqueExceptGly(candidate)) {
						lstOutput.Add(candidate);
					}
				}
			}
			return lstOutput;
		}

		private static List<string> ExtractSymbols(IDictionary<string, AminoAcid> keyed, params string[] codes)
		{
			var lstExtracted = new List<string>(codes.Length);
			for (int i = 0; i < codes.Length; i++) {
				lstExtracted.Add(keyed[codes[i]].Symbol);
			}
			return lstExtracted;
		}

		private static void Fill(List<string>[] destination, IReadOnlyList<AminoAcid> source)
		{
			for (int i = 0; i < destination.Length; i++) {
				if (destination[i] == null) {
					destination[i] = source.Select(x => x.Symbol).ToList();
				}
			}
		}

		private static void RestrictToAvailable(List<string>[] slots, params string[] allowed)
		{
			for (int i = 0; i < slots.Length; i++) {
				slots[i].RemoveAll(x => !allowed.Contains(x));
			}
		}

		/*
		private static long CountPermutations(List<AminoAcid>[] slots)
		{
			long permutations = 1;
			for (int i = 0; i < slots.Length; i++) {
				permutations *= slots[i].Count;
			}
			return permutations;
		}// */

		private static bool UniqueExceptGly(List<string> candidateSequence)
		{
			var dicCounts = new Dictionary<string, int>();
			for (int i = 0; i < candidateSequence.Count; i++) {
				int freq;
				var symbol = candidateSequence[i];
				if (dicCounts.TryGetValue(symbol, out freq)) {
					dicCounts[symbol]++;
				} else {
					dicCounts.Add(symbol, 1);
				}
			}
			foreach (var kvp in dicCounts) {
				if (kvp.Value > 1) {
					if (kvp.Key == "Gly") {
						if (kvp.Value > 2) {
							return false;
						}
					} else {
						return false;
					}
				}
			}
			return true;
		}
	}
}
