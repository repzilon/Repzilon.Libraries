//
//  PeptideGuess.cs
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
#if !NET20
using System;
using System.Collections.Generic;
#if NET6_0 || NETCOREAPP3_1
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq;
using Repzilon.Libraries.Core;
using Repzilon.Libraries.Core.Biochemistry;
#endif
// ReSharper disable InconsistentNaming

#if !NET20
namespace Repzilon.Tests.ForCoreLibrary
{
	internal sealed class PolypeptideComparer : IEqualityComparer<List<AlphaAminoAcid>>
	{
		public static readonly PolypeptideComparer Singleton = new PolypeptideComparer();

		private PolypeptideComparer() { }

#pragma warning disable CC0091 // Use static method
		public bool Equals(List<AlphaAminoAcid> x, List<AlphaAminoAcid> y)
#pragma warning restore CC0091 // Use static method
		{
			// Check whether the compared objects reference the same data.
			if (Object.ReferenceEquals(x, y)) {
				return true;
			}
			// Check whether any of the compared objects is null.
			if ((x == null) || (y == null)) {
				return false;
			}

			var c = x.Count;
			if (y.Count == c) {
				for (int i = 0; i < c; i++) {
					if (y[i] != x[i]) {
						return false;
					}
				}
				return true;
			} else {
				return false;
			}
		}

#pragma warning disable CC0091 // Use static method
#if NET6_0 || NETCOREAPP3_1
		public int GetHashCode([DisallowNull] List<AlphaAminoAcid> obj)
#else
		public int GetHashCode(List<AlphaAminoAcid> obj)
#endif
#pragma warning restore CC0091 // Use static method
		{
			if (obj == null) {
				return 0;
			}
			int magic = -1521134295;
			int hashCode = -918342670;
			unchecked {
				for (int i = 0; i < obj.Count; i++) {
					hashCode = (hashCode * magic) + (int)obj[i];
				}
			}
			return hashCode;
		}
	}

	internal static class PeptideGuess
	{
		internal static void Run(string[] args)
		{
			var lstLateral = Enum.GetValues(typeof(AlphaAminoAcid)).Cast<AlphaAminoAcid>().Where(x => x.HasPkaR()).Select(x => x.ToString()).ToList();
			var saaarLateral = AminoAcid.AlphaList.Where(x => lstLateral.Contains(x.Symbol)).ToArray();
			int a;
			Console.Write("pH");
			for (a = 0; a < saaarLateral.Length; a++) {
				Console.Write('\t');
				Console.Write(saaarLateral[a].Symbol);
			}
			Console.Write(Environment.NewLine);
			for (var f = 100; f <= 1400; f += 5) {
				Console.Write("{0,5:f2}", f * 0.01f);
				for (a = 0; a < saaarLateral.Length; a++) {
					Console.Write("\t{0,6:f3}", saaarLateral[a].WeightedCharge(RoundOff.Error(f * 0.01f)));
				}
				Console.Write(Environment.NewLine);
			}

			Console.Write("datasets: [");
			for (a = 0; a < saaarLateral.Length; a++) {
				if (a > 0) {
					Console.Write(',');
				}
#pragma warning disable UD0011 // The composite format string is not valid
				Console.WriteLine('{');
#pragma warning restore UD0011 // The composite format string is not valid
				Console.Write("\tlabel: '");
				Console.Write(saaarLateral[a].Symbol);
				Console.WriteLine("',");
				Console.Write("\tdata: [");
				var n = 0;
				for (var f = 100; f <= 1400; f += 5) {
					var pH = RoundOff.Error(f * 0.01f);
					var q = saaarLateral[a].WeightedCharge(pH);
					if (!float.IsNaN(q)) {
						if (n > 0) {
							Console.Write(',');
						}
						Console.Write("{{x: {0:f2}, y: {1:f3}}}", pH, q);
						n++;
					}
				}
				Console.Write("]}");
			}
			Console.WriteLine(']');

			const int kIterations = 440;
			BenchmarkResolution(kIterations, "Révision #10 (avec enums) :", SolveRevision10WithEnum);
			BenchmarkResolution(kIterations, "Révision #11", SolveRevision11);
		}

		#region Revision exercise number 10
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

			// Using LINQ is faster than the FindAll method
			var lstRev10_tetra = PermutationsForQuadSlots(rev10_tetra, AlphaAminoAcid.Gly).Where(x => x.Exists(AlphaAminoAcidExtension.IsAlkali)).Where(HasPartRev10Sequence).ToList();
			var lstRev10_hexa_c4 = PermutationsForQuadSlots(rev10_hexa_c4, AlphaAminoAcid.Gly);
			var lstRev10_hexa_c2 = PermutationsForTwoSlots(rev10_hexa_c2, AlphaAminoAcid.Gly);
			var lstRev10_hexa = ConcatenatePermutations(4 + 2, AlphaAminoAcid.Gly, lstRev10_hexa_c4, lstRev10_hexa_c2, HasPartRev10Sequence);
			return ConcatenatePermutations(4 + 6, AlphaAminoAcid.Gly, lstRev10_tetra, lstRev10_hexa, HasKeyRev10Sequence);
		}

		private static List<List<AlphaAminoAcid>> ConcatenatePermutations(int peptideLength,
		AlphaAminoAcid dualAminoAcid,
		List<List<AlphaAminoAcid>> firstPermutations, List<List<AlphaAminoAcid>> secondPermutations,
		Predicate<List<AlphaAminoAcid>> conditions)
		{
			if ((conditions == null) && (checked(firstPermutations.Count * secondPermutations.Count * 2) > 1000000)) {
#if NETCOREAPP1_0
				throw new OutOfMemoryException();
#else
				throw new InsufficientMemoryException();
#endif
			}

			var lstOutput = new List<List<AlphaAminoAcid>>();
			for (int i = 0; i < firstPermutations.Count; i++) {
				for (int j = 0; j < secondPermutations.Count; j++) {
					var candidate = new List<AlphaAminoAcid>(peptideLength);
					candidate.AddRange(firstPermutations[i]);
					candidate.AddRange(secondPermutations[j]);
#pragma warning disable CC0031 // Check for null before calling a delegate
					if ((conditions == null) || conditions(candidate)) {
						if (UniqueExcept(candidate, dualAminoAcid)) {
							lstOutput.Add(candidate);
						}
					}

					candidate = new List<AlphaAminoAcid>(peptideLength);
					candidate.AddRange(secondPermutations[j]);
					candidate.AddRange(firstPermutations[i]);
					if ((conditions == null) || conditions(candidate)) {
#pragma warning restore CC0031 // Check for null before calling a delegate
						if (UniqueExcept(candidate, dualAminoAcid)) {
							lstOutput.Add(candidate);
						}
					}
				}
			}
			return lstOutput;
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
		#endregion

		#region Revision exercise number 11
		private static List<List<AlphaAminoAcid>> SolveRevision11()
		{
			var rev11_AspTyrPhe = new AlphaAminoAcid[] {
				AlphaAminoAcid.Asp, AlphaAminoAcid.Tyr, AlphaAminoAcid.Phe
			};
			var rev11_chymo_free = new List<AlphaAminoAcid>[1];
			rev11_chymo_free[0] = PutSymbols(AlphaAminoAcid.Tyr, AlphaAminoAcid.Phe);
			var rev11_chymo_dual = new List<AlphaAminoAcid>[2];
			rev11_chymo_dual[0] = PutSymbols(rev11_AspTyrPhe);
			rev11_chymo_dual[1] = PutSymbols(AlphaAminoAcid.Tyr, AlphaAminoAcid.Phe);
			var rev11_chymo_quad = new List<AlphaAminoAcid>[4];
			Fill(rev11_chymo_quad, AlphaAminoAcid.Leu, AlphaAminoAcid.Lys, AlphaAminoAcid.Met);
			var lstChymoDualPermutations = PermutationsForTwoSlots(rev11_chymo_dual, AlphaAminoAcid.Met);
			var lstChymoQuadPermutations = PermutationsForQuadSlots(rev11_chymo_quad, AlphaAminoAcid.Met);
			var lstChymoAllPermutations = ConcatenatePermutations(1 + 2 + 4, AlphaAminoAcid.Met,
			 Transpose(rev11_chymo_free), lstChymoDualPermutations, lstChymoQuadPermutations, HasKeyRev11Sequence);

			var rev11_exceptMet = new AlphaAminoAcid[] {
				AlphaAminoAcid.Asp, AlphaAminoAcid.Lys, AlphaAminoAcid.Tyr, AlphaAminoAcid.Phe, AlphaAminoAcid.Leu
			};
			var rev11_brcn_free = new List<AlphaAminoAcid>[1];
			rev11_brcn_free[0] = PutSymbols(rev11_exceptMet);
			var rev11_brcn_dual = new List<AlphaAminoAcid>[2];
			rev11_brcn_dual[0] = PutSymbols(rev11_exceptMet);
			rev11_brcn_dual[1] = PutSymbols(AlphaAminoAcid.Met);
			var rev11_brcn_quad = new List<AlphaAminoAcid>[4];
			rev11_brcn_quad[rev11_brcn_quad.Length - 1] = PutSymbols(AlphaAminoAcid.Met);
			Fill(rev11_brcn_quad, rev11_exceptMet);
			var lstBrCNDualPermutations = PermutationsForTwoSlots(rev11_brcn_dual, AlphaAminoAcid.Met);
			var lstBrCNQuadPermutations = PermutationsForQuadSlots(rev11_brcn_quad, AlphaAminoAcid.Met);
			var lstBrCNAllPermutations = ConcatenatePermutations(1 + 2 + 4, AlphaAminoAcid.Met,
			 Transpose(rev11_brcn_free), lstBrCNDualPermutations, lstBrCNQuadPermutations, HasKeyRev11Sequence);

			return lstChymoAllPermutations.Intersect(lstBrCNAllPermutations, PolypeptideComparer.Singleton).ToList();
		}

		private static List<List<AlphaAminoAcid>> Transpose(List<AlphaAminoAcid>[] singleSlotted)
		{
			var c = singleSlotted[0].Count;
			var lstPermutations = new List<List<AlphaAminoAcid>>(c);
			for (int i = 0; i < c; i++) {
				lstPermutations.Add(new List<AlphaAminoAcid>(1) { singleSlotted[0][i] });
			}
			return lstPermutations;
		}

		private static List<List<AlphaAminoAcid>> ConcatenatePermutations(
		int peptideLength, AlphaAminoAcid dualAminoAcid,
		List<List<AlphaAminoAcid>> firstPermutations, List<List<AlphaAminoAcid>> secondPermutations,
		List<List<AlphaAminoAcid>> thirdPermutations,
		Predicate<List<AlphaAminoAcid>> conditions)
		{
			if ((conditions == null) && (checked(firstPermutations.Count * secondPermutations.Count * 2) > 1000000)) {
#if NETCOREAPP1_0
				throw new OutOfMemoryException();
#else
				throw new InsufficientMemoryException();
#endif
			}

			var lstOutput = new List<List<AlphaAminoAcid>>();
			for (int i = 0; i < firstPermutations.Count; i++) {
				for (int j = 0; j < secondPermutations.Count; j++) {
					for (int k = 0; k < thirdPermutations.Count; k++) {
						var candidate = new List<AlphaAminoAcid>(peptideLength);
						candidate.AddRange(firstPermutations[i]);
						candidate.AddRange(secondPermutations[j]);
						candidate.AddRange(thirdPermutations[k]);
#pragma warning disable CC0031 // Check for null before calling a delegate
						if ((conditions == null) || conditions(candidate)) {
							if (UniqueExcept(candidate, dualAminoAcid)) {
								lstOutput.Add(candidate);
							}
						}

						candidate = new List<AlphaAminoAcid>(peptideLength);
						candidate.AddRange(firstPermutations[i]);
						candidate.AddRange(thirdPermutations[k]);
						candidate.AddRange(secondPermutations[j]);
						if ((conditions == null) || conditions(candidate)) {
							if (UniqueExcept(candidate, dualAminoAcid)) {
								lstOutput.Add(candidate);
							}
						}

						candidate = new List<AlphaAminoAcid>(peptideLength);
						candidate.AddRange(secondPermutations[j]);
						candidate.AddRange(firstPermutations[i]);
						candidate.AddRange(thirdPermutations[k]);
						if ((conditions == null) || conditions(candidate)) {
							if (UniqueExcept(candidate, dualAminoAcid)) {
								lstOutput.Add(candidate);
							}
						}

						candidate = new List<AlphaAminoAcid>(peptideLength);
						candidate.AddRange(secondPermutations[j]);
						candidate.AddRange(thirdPermutations[k]);
						candidate.AddRange(firstPermutations[i]);
						if ((conditions == null) || conditions(candidate)) {
							if (UniqueExcept(candidate, dualAminoAcid)) {
								lstOutput.Add(candidate);
							}
						}

						candidate = new List<AlphaAminoAcid>(peptideLength);
						candidate.AddRange(thirdPermutations[k]);
						candidate.AddRange(firstPermutations[i]);
						candidate.AddRange(secondPermutations[j]);
						if ((conditions == null) || conditions(candidate)) {
							if (UniqueExcept(candidate, dualAminoAcid)) {
								lstOutput.Add(candidate);
							}
						}

						candidate = new List<AlphaAminoAcid>(peptideLength);
						candidate.AddRange(thirdPermutations[k]);
						candidate.AddRange(secondPermutations[j]);
						candidate.AddRange(firstPermutations[i]);
						if ((conditions == null) || conditions(candidate)) {
#pragma warning restore CC0031 // Check for null before calling a delegate
							if (UniqueExcept(candidate, dualAminoAcid)) {
								lstOutput.Add(candidate);
							}
						}
					}
				}
			}
			return lstOutput;
		}

		private static bool HasKeyRev11Sequence(List<AlphaAminoAcid> x)
		{
			return (x[0] == AlphaAminoAcid.Phe) && (x[6] == AlphaAminoAcid.Lys);
		}
		#endregion

		#region Common code
		private static void BenchmarkResolution(int iterations, string title, Func<List<List<AlphaAminoAcid>>> solver)
		{
			List<List<AlphaAminoAcid>> lstResults = null;
			DateTime dtmStart = DateTime.UtcNow;
			for (int i = 0; i < iterations; i++) {
				lstResults = solver();
			}
			TimeSpan tsEnum = DateTime.UtcNow - dtmStart;

			OutputArrangements(title, lstResults);
			Console.WriteLine("{0} itérations en {1:f0}ms, {2:f0}Hz",
			 iterations, tsEnum.TotalMilliseconds, Math.Floor(iterations / tsEnum.TotalSeconds));
		}

		private static void OutputArrangements(string title, List<List<AlphaAminoAcid>> allArrangements)
		{
			Console.WriteLine(title);
			var n = 1;
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

		private static List<AlphaAminoAcid> PutSymbols(params AlphaAminoAcid[] symbols)
		{
			return new List<AlphaAminoAcid>(symbols);
		}

#if NET40 || NET35 || NET20
		private static void Fill(List<AlphaAminoAcid>[] destination, IList<AlphaAminoAcid> source)
#else
		private static void Fill(List<AlphaAminoAcid>[] destination, IReadOnlyList<AlphaAminoAcid> source)
#endif
		{
			for (var i = 0; i < destination.Length; i++) {
				if (destination[i] == null) {
					destination[i] = new List<AlphaAminoAcid>(source);
				}
			}
		}

		private static void Fill(List<AlphaAminoAcid>[] destination, params AlphaAminoAcid[] source)
		{
			for (var i = 0; i < destination.Length; i++) {
				if (destination[i] == null) {
					destination[i] = new List<AlphaAminoAcid>(source);
				}
			}
		}

		/* RestrictToAvailable is no longer needed if you restrict beforehand
		private static void RestrictToAvailable<T>(List<T>[] slots, params T[] allowed)
		{
			for (int i = 0; i < slots.Length; i++) {
				slots[i].RemoveAll(x => !allowed.Contains(x));
			}
		}// */

		/*
		private static long CountPermutations<T>(ICollection<T>[] slots)
		{
			long permutations = 1;
			for (int i = 0; i < slots.Length; i++) {
				permutations *= slots[i].Count;
			}
			return permutations;
		}// */

		private static List<List<AlphaAminoAcid>> PermutationsForQuadSlots(List<AlphaAminoAcid>[] quadSlotted,
		AlphaAminoAcid dualAminoAcid)
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
							if (UniqueExcept(candidate, dualAminoAcid)) {
								lstOutput.Add(candidate);
							}
						}
					}
				}
			}
			return lstOutput;
		}

		private static List<List<AlphaAminoAcid>> PermutationsForTwoSlots(List<AlphaAminoAcid>[] doubleSlotted,
		AlphaAminoAcid dualAminoAcid)
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
					if (UniqueExcept(candidate, dualAminoAcid)) {
						lstOutput.Add(candidate);
					}
				}
			}
			return lstOutput;
		}

		private static bool UniqueExcept(List<AlphaAminoAcid> candidateSequence, AlphaAminoAcid allowExactlyTwoOfThese)
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
					if ((kvp.Key != allowExactlyTwoOfThese) || (freq > 2)) {
						return false;
					}
				}
			}
			return true;
		}
		#endregion
	}
}
#endif
