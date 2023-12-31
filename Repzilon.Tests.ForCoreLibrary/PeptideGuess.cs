﻿//
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
			/*
			var lstAminoAcids = AminoAcid.AlphaList;
			var dicAminoAcids = new SortedDictionary<string, AminoAcid>();
			int i;
			for (i = 0; i < lstAminoAcids.Count; i++) {
				dicAminoAcids.Add(lstAminoAcids[i].Symbol, lstAminoAcids[i]);
			}// */

			const int kIterations = 440;

			List<List<AlphaAminoAcid>> lstRev10Enum = null;
			DateTime dtmStart = DateTime.UtcNow;
			for (int i = 0; i < kIterations; i++) {
				lstRev10Enum = SolveRevision10WithEnum();
			}
			TimeSpan tsEnum = DateTime.UtcNow - dtmStart;

			OutputArrangements("Révision #10 :", lstRev10Enum);
			Console.WriteLine("{0} itérations en {1:f0}ms avec les enums, {2:f0}Hz",
			 kIterations, tsEnum.TotalMilliseconds, Math.Floor(kIterations / tsEnum.TotalSeconds));
		}

		private static List<List<AlphaAminoAcid>> SolveRevision10WithEnum()
		{
			var lstAllowed = new List<AlphaAminoAcid>(9) {
				AlphaAminoAcid.Asp, AlphaAminoAcid.Val, AlphaAminoAcid.Gly,
				AlphaAminoAcid.Tyr, AlphaAminoAcid.Ile, AlphaAminoAcid.Cys,
				AlphaAminoAcid.Arg, AlphaAminoAcid.Leu, AlphaAminoAcid.Ala
			};

			var rev10_tetra = new List<AlphaAminoAcid>[4];
			rev10_tetra[rev10_tetra.Length - 1] = PutSymbols(/*AlphaAminoAcid.Lys,*/ AlphaAminoAcid.Arg, AlphaAminoAcid.Ala);
			Fill(rev10_tetra, lstAllowed);
			//RestrictToAvailable(rev10_tetra, rev10_allowed);

			var rev10_hexa_c2 = new List<AlphaAminoAcid>[2];
			rev10_hexa_c2[0] = PutSymbols(/*AlphaAminoAcid.Phe,*/ AlphaAminoAcid.Tyr, /*AlphaAminoAcid.Trp,*/ AlphaAminoAcid.Ile/*, AlphaAminoAcid.Ser, AlphaAminoAcid.Thr*/);
			rev10_hexa_c2[1] = PutSymbols(/*AlphaAminoAcid.Lys,*/ AlphaAminoAcid.Arg, AlphaAminoAcid.Ala, /*AlphaAminoAcid.Phe,*/ AlphaAminoAcid.Tyr/*, AlphaAminoAcid.Trp*/);
			//RestrictToAvailable(rev10_hexa_c2, rev10_allowed);

			var rev10_hexa_c4 = new List<AlphaAminoAcid>[4];
			rev10_hexa_c4[0] = PutSymbols(AlphaAminoAcid.Asp/*, AlphaAminoAcid.Glu*/);
			rev10_hexa_c4[1] = PutSymbols(AlphaAminoAcid.Val);
			rev10_hexa_c4[rev10_hexa_c4.Length - 1] = PutSymbols(/*AlphaAminoAcid.Lys,*/ AlphaAminoAcid.Arg, AlphaAminoAcid.Ala, /*AlphaAminoAcid.Phe,*/ AlphaAminoAcid.Tyr/*, AlphaAminoAcid.Trp*/);
			Fill(rev10_hexa_c4, lstAllowed);
			//RestrictToAvailable(rev10_hexa_c4, rev10_allowed);

			var lstRev10_tetra = PermutationsForQuadSlots(rev10_tetra).Where(x => {
				return x.Exists(AlphaAminoAcidExtension.IsAlkali);
			}).Where(HasPartRev10Sequence).ToList();
			var lstRev10_hexa_c4 = PermutationsForQuadSlots(rev10_hexa_c4);
			var lstRev10_hexa_c2 = PermutationsForTwoSlots(rev10_hexa_c2);
			var lstRev10_hexa = ConcatenatePermutations(4 + 2, lstRev10_hexa_c4, lstRev10_hexa_c2).Where(HasPartRev10Sequence).ToList();
			return ConcatenatePermutations(4 + 6, lstRev10_tetra, lstRev10_hexa).Where(HasKeyRev10Sequence).ToList();
		}

		private static void OutputArrangements(string title, List<List<AlphaAminoAcid>> allArrangements)
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

		private static List<List<AlphaAminoAcid>> ConcatenatePermutations(int peptideLength,
		List<List<AlphaAminoAcid>> firstPermutations, List<List<AlphaAminoAcid>> secondPermutations)
		{
			var lstOutput = new List<List<AlphaAminoAcid>>();
			for (int i = 0; i < firstPermutations.Count; i++) {
				for (int j = 0; j < secondPermutations.Count; j++) {
					var candidate = new List<AlphaAminoAcid>(peptideLength);
					candidate.AddRange(firstPermutations[i]);
					candidate.AddRange(secondPermutations[j]);
					if (UniqueExceptGly(candidate)) {
						lstOutput.Add(candidate);
					}

					candidate = new List<AlphaAminoAcid>(peptideLength);
					candidate.AddRange(secondPermutations[j]);
					candidate.AddRange(firstPermutations[i]);
					if (UniqueExceptGly(candidate)) {
						lstOutput.Add(candidate);
					}
				}
			}
			return lstOutput;
		}

		private static List<List<AlphaAminoAcid>> PermutationsForQuadSlots(List<AlphaAminoAcid>[] quadSlotted)
		{
			if (quadSlotted.Length != 4) {
				throw new ArgumentException("quadSlotted");
			}

			var lstOutput = new List<List<AlphaAminoAcid>>();
			for (int s0 = 0; s0 < quadSlotted[0].Count; s0++) {
				for (int s1 = 0; s1 < quadSlotted[1].Count; s1++) {
					for (int s2 = 0; s2 < quadSlotted[2].Count; s2++) {
						for (int s3 = 0; s3 < quadSlotted[3].Count; s3++) {
							var candidate = new List<AlphaAminoAcid>(4) {
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

		private static List<List<AlphaAminoAcid>> PermutationsForTwoSlots(List<AlphaAminoAcid>[] doubleSlotted)
		{
			if (doubleSlotted.Length != 2) {
				throw new ArgumentException("doubleSlotted");
			}

			var lstOutput = new List<List<AlphaAminoAcid>>();
			for (int s0 = 0; s0 < doubleSlotted[0].Count; s0++) {
				for (int s1 = 0; s1 < doubleSlotted[1].Count; s1++) {
					var candidate = new List<AlphaAminoAcid>(2) {
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

		private static List<AlphaAminoAcid> PutSymbols(params AlphaAminoAcid[] symbols)
		{
			return new List<AlphaAminoAcid>(symbols);
		}

		private static void Fill(List<AlphaAminoAcid>[] destination, IReadOnlyList<AlphaAminoAcid> source)
		{
			for (var i = 0; i < destination.Length; i++) {
				if (destination[i] == null) {
					destination[i] = source.ToList();
				}
			}
		}

		/* RestrictToAvailable is no longer needed if you restrict beforehand
		private static void RestrictToAvailable(List<string>[] slots, params string[] allowed)
		{
			for (int i = 0; i < slots.Length; i++) {
				slots[i].RemoveAll(x => !allowed.Contains(x));
			}
		}// */

		/*
		private static long CountPermutations(List<AminoAcid>[] slots)
		{
			long permutations = 1;
			for (int i = 0; i < slots.Length; i++) {
				permutations *= slots[i].Count;
			}
			return permutations;
		}// */

		private static bool UniqueExceptGly(List<AlphaAminoAcid> candidateSequence)
		{
			var dicCounts = new Dictionary<AlphaAminoAcid, int>();
			int freq;
			for (int i = 0; i < candidateSequence.Count; i++) {
				var symbol = candidateSequence[i];
				dicCounts.TryGetValue(symbol, out freq);
				dicCounts[symbol] = freq + 1;
			}
			foreach (var kvp in dicCounts) {
				freq = kvp.Value;
				if (freq > 1) {
					if ((kvp.Key != AlphaAminoAcid.Gly) || (freq > 2)) {
						return false;
					}
				}
			}
			return true;
		}

		private static bool HasKeyRev10Sequence(List<AlphaAminoAcid> x)
		{
			var x2 = x[2];
			return (x[0] == AlphaAminoAcid.Gly) && (x[1] == AlphaAminoAcid.Leu) &&
				 ((x2 == AlphaAminoAcid.Cys) || (x2 == AlphaAminoAcid.Met)) &&
				 (x[9] == AlphaAminoAcid.Ala);
		}

		private static bool HasPartRev10Sequence(List<AlphaAminoAcid> x)
		{
			// Important : do not replace x[x.Count - 1] with a fixed index like above,
			// because it is used to test fragments, not the full sequence,
			// which have different lengths.
			if (x[x.Count - 1] == AlphaAminoAcid.Ala) {
				return true;
			} else {
				var x2 = x[2];
				return (x[0] == AlphaAminoAcid.Gly) && (x[1] == AlphaAminoAcid.Leu) &&
				 ((x2 == AlphaAminoAcid.Cys) || (x2 == AlphaAminoAcid.Met));
			}
		}
	}
}
