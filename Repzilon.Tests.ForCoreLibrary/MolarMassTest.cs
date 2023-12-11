//
//  MolarMassTest.cs
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
using System.Runtime.InteropServices;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	static class MolarMassTest
	{
		[StructLayout(LayoutKind.Auto)]
		struct AminoAcid : IEquatable<AminoAcid>
		{
			public float MolarMass;
			public float pKa1;
			public float pKa2;
			public float pKaR;
			public char Letter;
			public bool DicationWhenVeryAcid;
			public string Symbol;
			public string Name;
			public string Formula;

			public AminoAcid SetPkas(float pKa1, float pKa2)
			{
				return this.SetPkas(pKa1, pKa2, Single.NaN, false);
			}

			public AminoAcid SetPkas(float pKa1, float pKa2, float pKaR, bool isDicationWhenVeryAcid)
			{
				if (Single.IsNaN(pKa1) || (pKa1 < 1.5f) || (pKa1 >= 14)) {
					throw new ArgumentOutOfRangeException("pKa1");
				}
				if (Single.IsNaN(pKa2) || (pKa2 < 8) || (pKa2 >= 14)) {
					throw new ArgumentOutOfRangeException("pKa2");
				}
				if (!Single.IsNaN(pKaR)) {
					if ((pKaR < 3) || (pKaR >= 14)) {
						throw new ArgumentOutOfRangeException("pKaR");
					}
				} else if (isDicationWhenVeryAcid) {
					throw new ArgumentException("An amino acid that is a dication under very acidic conditions must have a pKaR.", "isDicationWhenVeryAcid");
				}

				this.pKa1 = pKa1;
				this.pKa2 = pKa2;
				this.pKaR = pKaR;
				this.DicationWhenVeryAcid = isDicationWhenVeryAcid;
				return this;
			}

			public AminoAcid SetFormula(string formula)
			{
				this.MolarMass = Chemistry.MolarMass(formula);
				this.Formula = formula;
				return this;
			}

			public float Isoelectric()
			{
				return Single.IsNaN(this.pKaR) ?
				 Chemistry.AminoAcidIsoelectric(this.pKa1, this.pKa2) :
				 Chemistry.AminoAcidIsoelectric(this.pKa1, this.pKa2, this.DicationWhenVeryAcid ? (byte)2 : (byte)1, this.pKaR);
			}

			#region Equals
			public override bool Equals(object obj)
			{
				return obj is AminoAcid acid && Equals(acid);
			}

			public bool Equals(AminoAcid other)
			{
				return MolarMass == other.MolarMass &&
					   pKa1 == other.pKa1 &&
					   pKa2 == other.pKa2 &&
					   pKaR == other.pKaR &&
					   Letter == other.Letter &&
					   DicationWhenVeryAcid == other.DicationWhenVeryAcid &&
					   Symbol == other.Symbol &&
					   Name == other.Name &&
					   Formula == other.Formula;
			}

			public override int GetHashCode()
			{
				HashCode hash = new HashCode();
				hash.Add(MolarMass);
				hash.Add(pKa1);
				hash.Add(pKa2);
				hash.Add(pKaR);
				hash.Add(Letter);
				hash.Add(DicationWhenVeryAcid);
				hash.Add(Symbol);
				hash.Add(Name);
				hash.Add(Formula);
				return hash.ToHashCode();
			}

			public static bool operator ==(AminoAcid left, AminoAcid right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(AminoAcid left, AminoAcid right)
			{
				return !(left == right);
			}
			#endregion

			public static AminoAcid Create(char letter, string code, string name)
			{
				if (!Char.IsLetter(letter)) {
					throw new ArgumentOutOfRangeException("letter");
				}
				if (String.IsNullOrWhiteSpace(code)) {
					throw new ArgumentNullException("code");
				}
				if (String.IsNullOrWhiteSpace(name)) {
					throw new ArgumentNullException("name");
				}
				if (code.Trim().Length != 3) {
					throw new ArgumentException("An amino acid symbol is made of three letters.", "code");
				}

				var aa = new AminoAcid();
				aa.Letter = letter;
				aa.Symbol = code;
				aa.Name = name.Trim();
				return aa;
			}
		}

		internal static void Run(string[] args)
		{
			var karFormulas = new string[] { "Ca(OH)<sub>2</sub>" };
			for (var i = 0; i < karFormulas.Length; i++) {
				Console.WriteLine("{0,9:n3} {1}", Chemistry.MolarMass(karFormulas[i]), karFormulas[i]);
			}

			var lstAminoAcids = new List<AminoAcid>(20)
			{
				AminoAcid.Create('D', "Asp", "Acide aspartique").SetPkas(2.1f, 9.8f, 3.9f, false),
				AminoAcid.Create('E', "Glu", "Acide glutamique").SetPkas(2.2f, 9.7f, 4.2f, false),
				AminoAcid.Create('A', "Ala", "Alanine").SetPkas(2.3f, 9.9f),
				AminoAcid.Create('R', "Arg", "Arginine").SetPkas(2.2f, 9.0f, 12.5f, true),
				AminoAcid.Create('N', "Asn", "Asparagine").SetPkas(2.0f, 8.8f),
				AminoAcid.Create('C', "Cys", "Cystéine").SetPkas(1.7f, 10.8f, 8.3f, false),
				AminoAcid.Create('Q', "Gln", "Glutamine").SetPkas(2.1f, 9.1f),
				AminoAcid.Create('G', "Gly", "Glycine").SetPkas(2.4f, 9.7f),
				AminoAcid.Create('H', "His", "Histidine").SetPkas(1.8f, 9.2f, 6.0f, true),
				AminoAcid.Create('I', "Ile", "Isoleucine").SetPkas(2.4f, 9.7f),
				AminoAcid.Create('L', "Leu", "Leucine").SetPkas(2.4f, 9.6f),
				AminoAcid.Create('K', "Lys", "Lysine").SetPkas(2.2f, 8.9f, 10.5f, true),
				AminoAcid.Create('M', "Mét", "Méthionine").SetPkas(2.3f, 9.2f),
				AminoAcid.Create('F', "Phé", "Phénylalanine").SetPkas(2.6f, 9.2f),
				AminoAcid.Create('P', "Pro", "Proline").SetPkas(2.0f, 10.6f),
				AminoAcid.Create('S', "Sér", "Sérine").SetPkas(2.2f, 9.2f),
				AminoAcid.Create('T', "Thr", "Thréonine").SetPkas(2.6f, 10.4f),
				AminoAcid.Create('W', "Trp", "Tryptophane").SetPkas(2.4f, 9.4f),
				AminoAcid.Create('Y', "Tyr", "Tyrosine").SetPkas(2.2f, 9.1f, 10.0f, false),
				AminoAcid.Create('V', "Val", "Valine").SetPkas(2.3f, 9.7f)
			};

			var dicAminoAcids = new SortedDictionary<string, AminoAcid>();
			for (var i = 0; i < lstAminoAcids.Count; i++) {
				dicAminoAcids.Add(lstAminoAcids[i].Name, lstAminoAcids[i]);
			}

			foreach (var aa in dicAminoAcids.Values) {
				Console.WriteLine("{0} {1} {2,-20} {3,4:f1} {4,4:f1} {5,4:f1} {6,5:f2}",
				 aa.Letter, aa.Symbol, aa.Name, aa.pKa1, aa.pKa2, aa.pKaR, aa.Isoelectric());
			}
		}
	}
}

