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
	/*	This was partly built using Regex.Replace
		Match pattern:
		AminoAcid.Create[(]'([A-Z])', "([A-Za-zé]+)", "([A-Za-z é]+)"[)].SetPkas[(][0-9.false,tru ]+[)]
		Replace pattern:
		///<summary>$3</summary>
		$2 = (byte)'$1'
	 */
	/// <summary>
	/// List of alpha amino acid three letter codes
	/// </summary>
	/// <remarks>
	/// The numeric representation is the ASCII/ISO-8859-1/Unicode
	/// code point of the single capital letter code.
	/// </remarks>
	enum AlphaAminoAcid : byte
	{
		///<summary>Aspartic acid</summary>
		Asp = (byte)'D',

		///<summary>Glutamic acid</summary>
		Glu = (byte)'E',

		///<summary>Alanine</summary>
		Ala = (byte)'A',

		///<summary>Arginine</summary>
		Arg = (byte)'R',

		///<summary>Asparagine</summary>
		Asn = (byte)'N',

		///<summary>Cysteine</summary>
		Cys = (byte)'C',

		///<summary>Glutamine</summary>
		Gln = (byte)'Q',

		///<summary>Glycine</summary>
		Gly = (byte)'G',

		///<summary>Histidine</summary>
		His = (byte)'H',

		///<summary>Isoleucine</summary>
		Ile = (byte)'I',

		///<summary>Leucine</summary>
		Leu = (byte)'L',

		///<summary>Lysine</summary>
		Lys = (byte)'K',

		///<summary>Methionine</summary>
		Met = (byte)'M',

		///<summary>Phenylalanine</summary>
		Phe = (byte)'F',

		///<summary>Proline</summary>
		Pro = (byte)'P',

		///<summary>Serine</summary>
		Ser = (byte)'S',

		///<summary>Threonine</summary>
		Thr = (byte)'T',

		///<summary>Tryptophane</summary>
		Trp = (byte)'W',

		///<summary>Tyrosine</summary>
		Tyr = (byte)'Y',

		///<summary>Valine</summary>
		Val = (byte)'V'
	}

	static class PeptideGuess
	{
		internal static void Run(string[] args)
		{
			var lstAminoAcids = AminoAcid.AlphaList;
			var dicAminoAcids = new SortedDictionary<string, AminoAcid>();
			int i;
			for (i = 0; i < lstAminoAcids.Count; i++) {
				dicAminoAcids.Add(lstAminoAcids[i].Symbol, lstAminoAcids[i]);
			}

			const int kIterations = 270;

			List<List<string>> lstRev10Sym = null;
			DateTime dtmStart = DateTime.UtcNow;
			for (i = 0; i < kIterations; i++) {
				lstRev10Sym = SolveRevision10WithSymbol(dicAminoAcids);
			}
			TimeSpan tsSymbol = DateTime.UtcNow - dtmStart;

			List<List<AlphaAminoAcid>> lstRev10Enum = null;
			dtmStart = DateTime.UtcNow;
			for (i = 0; i < kIterations; i++) {
				lstRev10Enum = SolveRevision10WithEnum();
			}
			TimeSpan tsEnum = DateTime.UtcNow - dtmStart;

			OutputArrangements("Révision #10 :", lstRev10Sym);
			Console.WriteLine("{0} itérations en {1:n0}ms avec les codes, {2:n0}ms avec les enums",
			 kIterations, tsSymbol.TotalMilliseconds, tsEnum.TotalMilliseconds);
		}

		private static List<List<string>> SolveRevision10WithSymbol(IReadOnlyDictionary<string, AminoAcid> dicAminoAcids)
		{
			string[] rev10_allowed = new string[] { "Asp", "Val", "Gly", "Tyr", "Ile", "Cys", "Arg", "Leu", "Ala" };

			var c = rev10_allowed.Length;
			var lstAllowed = new List<AminoAcid>(c);
			int i;
			for (i = 0; i < c; i++) {
				lstAllowed.Add(dicAminoAcids[rev10_allowed[i]]);
			}

			var rev10_tetra = new List<string>[4];
			rev10_tetra[rev10_tetra.Length - 1] = ExtractSymbols(dicAminoAcids, /*"Lys",*/ "Arg", "Ala");
			Fill(rev10_tetra, lstAllowed);
			//RestrictToAvailable(rev10_tetra, rev10_allowed);

			var rev10_hexa_c2 = new List<string>[2];
			rev10_hexa_c2[0] = ExtractSymbols(dicAminoAcids, /*"Phé",*/ "Tyr", /*"Trp",*/ "Ile"/*, "Sér", "Thr"*/);
			rev10_hexa_c2[1] = ExtractSymbols(dicAminoAcids, /*"Lys",*/ "Arg", "Ala", /*"Phé",*/ "Tyr"/*, "Trp"*/);
			//RestrictToAvailable(rev10_hexa_c2, rev10_allowed);

			var rev10_hexa_c4 = new List<string>[4];
			rev10_hexa_c4[0] = ExtractSymbols(dicAminoAcids, "Asp"/*, "Glu"*/);
			rev10_hexa_c4[1] = ExtractSymbols(dicAminoAcids, "Val");
			rev10_hexa_c4[rev10_hexa_c4.Length - 1] = ExtractSymbols(dicAminoAcids, /*"Lys",*/ "Arg", "Ala", /*"Phé",*/ "Tyr"/*, "Trp"*/);
			Fill(rev10_hexa_c4, lstAllowed);
			//RestrictToAvailable(rev10_hexa_c4, rev10_allowed);

			var lstRev10_tetra = PermutationsForQuadSlots(rev10_tetra).Where(x => {
				return x.Exists(IsAlkaliAminoSymbol);
			}).Where(HasPartRev10Sequence).ToList();
			var lstRev10_hexa_c4 = PermutationsForQuadSlots(rev10_hexa_c4);
			var lstRev10_hexa_c2 = PermutationsForTwoSlots(rev10_hexa_c2);
			var lstRev10_hexa = ConcatenatePermutations(4 + 2, lstRev10_hexa_c4, lstRev10_hexa_c2).Where(HasPartRev10Sequence).ToList();
			return ConcatenatePermutations(4 + 6, lstRev10_tetra, lstRev10_hexa).Where(HasKeyRev10Sequence).ToList();
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
				return x.Exists(IsAlkaliAminoSymbol);
			}).Where(HasPartRev10Sequence).ToList();
			var lstRev10_hexa_c4 = PermutationsForQuadSlots(rev10_hexa_c4);
			var lstRev10_hexa_c2 = PermutationsForTwoSlots(rev10_hexa_c2);
			var lstRev10_hexa = ConcatenatePermutations(4 + 2, lstRev10_hexa_c4, lstRev10_hexa_c2).Where(HasPartRev10Sequence).ToList();
			return ConcatenatePermutations(4 + 6, lstRev10_tetra, lstRev10_hexa).Where(HasKeyRev10Sequence).ToList();
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

		[Obsolete]
		private static List<List<string>> ConcatenatePermutations(int peptideLength,
		List<List<string>> firstPermutations, List<List<string>> secondPermutations)
		{
			var lstOutput = new List<List<string>>();
			for (int i = 0; i < firstPermutations.Count; i++) {
				for (int j = 0; j < secondPermutations.Count; j++) {
					var candidate = new List<string>(peptideLength);
					candidate.AddRange(firstPermutations[i]);
					candidate.AddRange(secondPermutations[j]);
					if (UniqueExceptGly(candidate)) {
						lstOutput.Add(candidate);
					}

					candidate = new List<string>(peptideLength);
					candidate.AddRange(secondPermutations[j]);
					candidate.AddRange(firstPermutations[i]);
					if (UniqueExceptGly(candidate)) {
						lstOutput.Add(candidate);
					}
				}
			}
			return lstOutput;
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

		[Obsolete]
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

		[Obsolete]
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

		[Obsolete]
		private static List<string> ExtractSymbols(IReadOnlyDictionary<string, AminoAcid> keyed, params string[] codes)
		{
			var lstExtracted = new List<string>(codes.Length);
			for (int i = 0; i < codes.Length; i++) {
				lstExtracted.Add(keyed[codes[i]].Symbol);
			}
			return lstExtracted;
		}

		[Obsolete]
		private static List<string> ExtractSymbols(IReadOnlyDictionary<string, AminoAcid> keyed, string singleCode)
		{
			return new List<string>(1) { keyed[singleCode].Symbol };
		}

		[Obsolete]
		private static List<string> ExtractSymbols(IReadOnlyDictionary<string, AminoAcid> keyed, string first, string second)
		{
			return new List<string>(2) { keyed[first].Symbol, keyed[second].Symbol };
		}

		private static List<AlphaAminoAcid> PutSymbols(params AlphaAminoAcid[] symbols)
		{
			return new List<AlphaAminoAcid>(symbols);
		}

		[Obsolete]
		private static void Fill(List<string>[] destination, IReadOnlyList<AminoAcid> source)
		{
			var c = source.Count;
			List<string> lstSymbols = new List<string>(c);
			int i;
			for (i = 0; i < c; i++) {
				lstSymbols.Add(source[i].Symbol);
			}
			for (i = 0; i < destination.Length; i++) {
				if (destination[i] == null) {
					destination[i] = lstSymbols;
				}
			}
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

		[Obsolete]
		private static bool UniqueExceptGly(List<string> candidateSequence)
		{
			var dicCounts = new Dictionary<string, int>();
			int freq;
			for (int i = 0; i < candidateSequence.Count; i++) {
				var symbol = candidateSequence[i];
				dicCounts.TryGetValue(symbol, out freq);
				dicCounts[symbol] = freq + 1;
			}
			foreach (var kvp in dicCounts) {
				freq = kvp.Value;
				if (freq > 1) {
					if ((kvp.Key != "Gly") || (freq > 2)) {
						return false;
					}
				}
			}
			return true;
		}

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

		[Obsolete]
		private static bool IsAlkaliAminoSymbol(string y)
		{
			return (y == "Lys") || (y == "Arg") || (y == "His");
		}

		private static bool IsAlkaliAminoSymbol(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Lys) || (y == AlphaAminoAcid.Arg) || (y == AlphaAminoAcid.His);
		}

		[Obsolete]
		private static bool HasKeyRev10Sequence(List<string> x)
		{
			var x2 = x[2];
			return (x[0] == "Gly") && (x[1] == "Leu") &&
				 ((x2 == "Cys") || (x2 == "Mét")) &&
				 (x[9] == "Ala");
		}

		private static bool HasKeyRev10Sequence(List<AlphaAminoAcid> x)
		{
			var x2 = x[2];
			return (x[0] == AlphaAminoAcid.Gly) && (x[1] == AlphaAminoAcid.Leu) &&
				 ((x2 == AlphaAminoAcid.Cys) || (x2 == AlphaAminoAcid.Met)) &&
				 (x[9] == AlphaAminoAcid.Ala);
		}

		[Obsolete]
		private static bool HasPartRev10Sequence(List<string> x)
		{
			// Important : do not replace x[x.Count - 1] with a fixed index like above,
			// because it is used to test fragments, not the full sequence,
			// which have different lengths.
			if (x[x.Count - 1] == "Ala") {
				return true;
			} else {
				var x2 = x[2];
				return (x[0] == "Gly") && (x[1] == "Leu") &&
				 ((x2 == "Cys") || (x2 == "Mét"));
			}
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
