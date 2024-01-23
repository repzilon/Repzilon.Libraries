//
//  AminoAcid.cs
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
using System.Runtime.InteropServices;

namespace Repzilon.Libraries.Core
{
	[StructLayout(LayoutKind.Auto)]
	public struct AminoAcid : IEquatable<AminoAcid>
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

		public AminoAcid SetPkas(float pKa1, float pKa2, float pKaR, bool isDicationWhenVeryAcid)
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
			var ar = this.pKaR;
			var a1 = this.pKa1;
			var a2 = this.pKa2;
			return Single.IsNaN(ar) ?
			 Chemistry.AminoAcidIsoelectric(a1, a2) :
			 Chemistry.AminoAcidIsoelectric(a1, a2, this.DicationWhenVeryAcid ? (byte)2 : (byte)1, ar);
		}

		#region Equals
		public override bool Equals(object obj)
		{
			return (obj is AminoAcid) && Equals((AminoAcid)obj);
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
			unchecked {
				var magic = -1521134295;
				var hashCode = (-919792662 * magic) + MolarMass.GetHashCode();
				hashCode = (hashCode * magic) + pKa1.GetHashCode();
				hashCode = (hashCode * magic) + pKa2.GetHashCode();
				hashCode = (hashCode * magic) + pKaR.GetHashCode();
				hashCode = (hashCode * magic) + Letter.GetHashCode();
				hashCode = (hashCode * magic) + DicationWhenVeryAcid.GetHashCode();
				hashCode = (hashCode * magic) + Symbol.GetHashCode();
				hashCode = (hashCode * magic) + Name.GetHashCode();
				return (hashCode * magic) + Formula.GetHashCode();
			}
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

		public static readonly IReadOnlyList<AminoAcid> AlphaList = MakeAlphaList();

		private static IReadOnlyList<AminoAcid> MakeAlphaList()
		{
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
			return new ReadOnlyCollection<AminoAcid>(lstAminoAcids);
		}

		public float WeightedCharge(float pH)
		{
			if ((pH < 1) || (pH > 14)) {
				throw new ArgumentOutOfRangeException("pH");
			}
			bool dicat;
			var ar = this.pKaR;
			var pkI = this.Isoelectric();
			float am;
			if (Single.IsNaN(ar)) { // without lateral chain
				if (pH == pkI) {
					return 0;
				} else if (pH == this.pKa1) {
					return 0.5f;
				} else if (pH == this.pKa2) {
					return -0.5f;
				} else if (pH < pkI) {					
					return (float)(1.0 / (1 + Math.Pow(10, pH - this.pKa1)));
				} else {
					var dblAlkaliRatio = Math.Pow(10, pH - this.pKa2);
					return (float)((-1 * dblAlkaliRatio) / (1 + dblAlkaliRatio));
				}
			} else { // with lateral chain
				dicat = this.DicationWhenVeryAcid;
				var a2ltar = (this.pKa2 < ar);
				if (pH == pkI) {
					return 0;
				} else if (pH == this.pKa1) {
					return dicat ? 1.5f : 0.5f;
				} else if (pH == this.pKa2) {
					return ChargeOfLateral(false, a2ltar, dicat);
				} else if (pH == ar) {
					return ChargeOfLateral(true, a2ltar, dicat);
				} else {
					am = Math.Min(this.pKa2, ar);
					var ah = Math.Max(this.pKa2, ar);
					if (2 * pH < this.pKa1 + am) { // pH < 0.5f * (this.pKa1 + am)
						return ChargeOfLateral(pH, this.pKa1, dicat, 2);
					} else if (2 * pH < am + ah) {
						return ChargeOfLateral(pH, am, dicat, 1);
					} else {
						return ChargeOfLateral(pH, ah, dicat, 0);
					}
				}
			}
		}

		private static float ChargeOfLateral(float pH, float pKa, bool dicat, short mostAcidicCharge)
		{
			var dblAlkaliRatio = Math.Pow(10, pH - pKa);
			/*
			short shrMid = (short)(mostAcidicCharge - 1);
			short shrBase = (short)(mostAcidicCharge - 2);
			return (float)((((dicat ? mostAcidicCharge : shrMid) * 1.0) + ((dicat ? shrMid : shrBase) * dblAlkaliRatio)) / (1 + dblAlkaliRatio));
			// */
			var c = dicat ? mostAcidicCharge : mostAcidicCharge - 1;
			return (float)(c - (dblAlkaliRatio / (dblAlkaliRatio + 1)));
		}

		private static float ChargeOfLateral(bool pHEqualsPkar, bool pKa2LessThanPkar, bool dicat)
		{
			bool blnEquals = (pHEqualsPkar == pKa2LessThanPkar);
			if (dicat) {
				return blnEquals ? -0.5f : 0.5f;
			} else {
				return blnEquals ? -1.5f : -0.5f;
			}
		}
	}
}
