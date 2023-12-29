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

			string[] rev10_allowed = new string[] { "Asp", "Val", "Gly", "Tyr", "Ile", "Cys", "Arg", "Leu", "Ala" };

			var rev10_tetra = new List<AminoAcid>[4];
			rev10_tetra[rev10_tetra.Length - 1] = Extract(dicAminoAcids, "Lys", "Arg", "Ala");
			Fill(rev10_tetra, lstAminoAcids);
			RestrictToAvailable(rev10_tetra, rev10_allowed);

			var rev10_hexa_c2 = new List<AminoAcid>[2];
			rev10_hexa_c2[0] = Extract(dicAminoAcids, "Phé", "Tyr", "Trp", "Ile", "Sér", "Thr");
			rev10_hexa_c2[1] = Extract(dicAminoAcids, "Lys", "Arg", "Ala", "Phé", "Tyr", "Trp");
			RestrictToAvailable(rev10_hexa_c2, rev10_allowed);

			var rev10_hexa_c4 = new List<AminoAcid>[4];
			rev10_hexa_c4[0] = Extract(dicAminoAcids, "Asp", "Glu");
			rev10_hexa_c4[1] = Extract(dicAminoAcids, "Val");
			rev10_hexa_c4[rev10_hexa_c4.Length - 1] = Extract(dicAminoAcids, "Lys", "Arg", "Ala", "Phé", "Tyr", "Trp");
			Fill(rev10_hexa_c4, lstAminoAcids);
			RestrictToAvailable(rev10_hexa_c4, rev10_allowed);

			var lstRev10_tetra = PermutationsForQuadSlots(rev10_tetra).Where(x => {
				return x.Exists(y => {
					var s = y.Symbol;
					return (s == "Lys") || (s == "Arg") || (s == "His");
				});
			}).ToList();
			var lstRev10_hexa_c4 = PermutationsForQuadSlots(rev10_hexa_c4);
			var lstRev10_hexa_c2 = PermutationsForTwoSlots(rev10_hexa_c2);
			var lstRev10_hexa = ConcatenatePermutations(lstRev10_hexa_c4, lstRev10_hexa_c2);
			var lstRev10 = ConcatenatePermutations(lstRev10_tetra, lstRev10_hexa).Where(x => {
				return (x[0].Symbol == "Gly") && (x[1].Symbol == "Leu") &&
				 ((x[2].Symbol == "Cys") || (x[2].Symbol == "Mét")) &&
				 (x[x.Count - 1].Symbol == "Ala");
			}).ToList();

			OutputArrangements("Révision #10 :", lstRev10);
		}

		private static void OutputArrangements(string title, List<List<AminoAcid>> allArrangements)
		{
			Console.WriteLine(title);
			int n = 1;
			foreach (var candidate in allArrangements) {
				Console.Write("#{0} : ", n);
				for (var i = 0; i < candidate.Count; i++) {
					if (i > 0) {
						Console.Write('-');
					}
					Console.Write(candidate[i].Symbol);
				}
				Console.Write(Environment.NewLine);
				n++;
			}
		}

		private static List<List<AminoAcid>> ConcatenatePermutations(
		List<List<AminoAcid>> firstPermutations, List<List<AminoAcid>> secondPermutations)
		{
			var lstOutput = new List<List<AminoAcid>>();
			for (int i = 0; i < firstPermutations.Count; i++) {
				for (int j = 0; j < secondPermutations.Count; j++) {
					var candidate = new List<AminoAcid>();
					candidate.AddRange(firstPermutations[i]);
					candidate.AddRange(secondPermutations[j]);
					if (UniqueExceptGly(candidate)) {
						lstOutput.Add(candidate);
					}

					candidate = new List<AminoAcid>();
					candidate.AddRange(secondPermutations[j]);
					candidate.AddRange(firstPermutations[i]);
					if (UniqueExceptGly(candidate)) {
						lstOutput.Add(candidate);
					}
				}
			}
			return lstOutput;
		}

		private static List<List<AminoAcid>> PermutationsForQuadSlots(List<AminoAcid>[] quadSlotted)
		{
			if (quadSlotted.Length != 4) {
				throw new ArgumentException("quadSlotted");
			}

			var lstOutput = new List<List<AminoAcid>>();
			for (int s0 = 0; s0 < quadSlotted[0].Count; s0++) {
				for (int s1 = 0; s1 < quadSlotted[1].Count; s1++) {
					for (int s2 = 0; s2 < quadSlotted[2].Count; s2++) {
						for (int s3 = 0; s3 < quadSlotted[3].Count; s3++) {
							var candidate = new List<AminoAcid>(4) {
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

		private static List<List<AminoAcid>> PermutationsForTwoSlots(List<AminoAcid>[] doubleSlotted)
		{
			if (doubleSlotted.Length != 2) {
				throw new ArgumentException("doubleSlotted");
			}

			var lstOutput = new List<List<AminoAcid>>();
			for (int s0 = 0; s0 < doubleSlotted[0].Count; s0++) {
				for (int s1 = 0; s1 < doubleSlotted[1].Count; s1++) {
					var candidate = new List<AminoAcid>(2) {
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

		private static List<AminoAcid> Extract(IDictionary<string, AminoAcid> keyed, params string[] codes)
		{
			var lstExtracted = new List<AminoAcid>(codes.Length);
			for (int i = 0; i < codes.Length; i++) {
				lstExtracted.Add(keyed[codes[i]]);
			}
			return lstExtracted;
		}

		private static void Fill(List<AminoAcid>[] destination, IReadOnlyList<AminoAcid> source)
		{
			for (int i = 0; i < destination.Length; i++) {
				if (destination[i] == null) {
					destination[i] = source.ToList();
				}
			}
		}

		private static void RestrictToAvailable(List<AminoAcid>[] slots, params string[] allowed)
		{
			for (int i = 0; i < slots.Length; i++) {
				slots[i].RemoveAll(x => !allowed.Contains(x.Symbol));
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

		private static bool UniqueExceptGly(List<AminoAcid> candidateSequence)
		{
			var dicCounts = new Dictionary<string, int>();
			for (int i = 0; i < candidateSequence.Count; i++) {
				int freq;
				var symbol = candidateSequence[i].Symbol;
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
