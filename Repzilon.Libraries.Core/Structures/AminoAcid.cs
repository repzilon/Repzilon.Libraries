﻿//
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

namespace Repzilon.Libraries.Core.Biochemistry
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct AminoAcid : IEquatable<AminoAcid>
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	{
		public float MolarMass;
		public float pKa1;
		public float pKa2;
		public float pKaR;
		public readonly char Letter;
		public bool DicationWhenVeryAcid;
		public readonly string Symbol;
		public readonly string Name;
		public string Formula;

		public AminoAcid(char letter, string code, string name)
		{
			if (!Char.IsLetter(letter)) {
				throw new ArgumentOutOfRangeException("letter");
			}
#if NET35 || NET20
			if (RetroCompat.IsNullOrWhiteSpace(code)) {
				throw new ArgumentNullException("code");
			}
			if (RetroCompat.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException("name");
			}
#else
			if (String.IsNullOrWhiteSpace(code)) {
				throw new ArgumentNullException("code");
			}
			if (String.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException("name");
			}
#endif
			if (code.Trim().Length != 3) {
				throw new ArgumentException("An amino acid symbol is made of three letters.", "code");
			}

			this.Letter = letter;
			this.Symbol = code;
			this.Name = name.Trim();

			this.MolarMass = Single.NaN;
			this.pKa1 = Single.NaN;
			this.pKa2 = Single.NaN;
			this.pKaR = Single.NaN;
			this.DicationWhenVeryAcid = false;
			this.Formula = null;
		}

		#region ICloneable members
		public AminoAcid(AminoAcid other)
		{
			this.Letter = other.Letter;
			this.Symbol = other.Symbol;
			this.Name = other.Name;

			this.MolarMass = other.MolarMass;
			this.pKa1 = other.pKa1;
			this.pKa2 = other.pKa2;
			this.pKaR = other.pKaR;
			this.DicationWhenVeryAcid = other.DicationWhenVeryAcid;
			this.Formula = other.Formula;
		}

		public AminoAcid Clone()
		{
			return new AminoAcid(this);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public AminoAcid SetPkas(float pKa1NewValue, float pKa2NewValue)
		{
			return this.SetPkas(pKa1NewValue, pKa2NewValue, Single.NaN, false);
		}

		public AminoAcid SetPkas(float pKa1NewValue, float pKa2NewValue, float pKaRnewValue, bool isDicationWhenVeryAcid)
		{
			if (Single.IsNaN(pKa1NewValue) || (pKa1NewValue < 1.5f) || (pKa1NewValue >= 14)) {
				throw new ArgumentOutOfRangeException("pKa1NewValue");
			}
			if (Single.IsNaN(pKa2NewValue) || (pKa2NewValue < 8) || (pKa2NewValue >= 14)) {
				throw new ArgumentOutOfRangeException("pKa2NewValue");
			}
			if (!Single.IsNaN(pKaRnewValue)) {
				if ((pKaRnewValue < 3) || (pKaRnewValue >= 14)) {
					throw new ArgumentOutOfRangeException("pKaRnewValue");
				}
			} else if (isDicationWhenVeryAcid) {
				throw new ArgumentException("An amino acid that is a dication under very acidic conditions must have a pKaR.", "isDicationWhenVeryAcid");
			}

			this.pKa1 = pKa1NewValue;
			this.pKa2 = pKa2NewValue;
			this.pKaR = pKaRnewValue;
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
			 Isoelectric(a1, a2) :
			 Isoelectric(a1, a2, this.DicationWhenVeryAcid ? (byte)2 : (byte)1, ar);
		}

		#region Equals
		public override bool Equals(object obj)
		{
			return (obj is AminoAcid) && Equals((AminoAcid)obj);
		}

		public bool Equals(AminoAcid other)
		{
			return RoundOff.Equals(MolarMass, other.MolarMass) &&
				   RoundOff.Equals(pKa1, other.pKa1) &&
				   RoundOff.Equals(pKa2, other.pKa2) &&
				   RoundOff.Equals(pKaR, other.pKaR) &&
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

#if NET40 || NET35 || NET20
		public static readonly ReadOnlyCollection<AminoAcid> AlphaList = MakeAlphaList();
#else
		public static readonly IReadOnlyList<AminoAcid> AlphaList = MakeAlphaList();
#endif

#if NET40 || NET35 || NET20
		private static ReadOnlyCollection<AminoAcid> MakeAlphaList()
#else
		private static IReadOnlyList<AminoAcid> MakeAlphaList()
#endif
		{
			var lstAminoAcids = new List<AminoAcid>(20)
			{
				new AminoAcid('D', "Asp", "Acide aspartique").SetPkas(2.1f, 9.8f, 3.9f, false).SetFormula("HOOC-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('E', "Glu", "Acide glutamique").SetPkas(2.2f, 9.7f, 4.2f, false).SetFormula("HOOC-(CH<sub>2</sub>)<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('A', "Ala", "Alanine").SetPkas(2.3f, 9.9f).SetFormula("CH<sub>3</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('R', "Arg", "Arginine").SetPkas(2.2f, 9.0f, 12.5f, true).SetFormula("(H<sub>2</sub>N)<sub>2</sub>-C-NH-(CH<sub>2</sub>)<sub>3</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('N', "Asn", "Asparagine").SetPkas(2.0f, 8.8f).SetFormula("H<sub>2</sub>N-C=O-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('C', "Cys", "Cystéine").SetPkas(1.7f, 10.8f, 8.3f, false).SetFormula("HS-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('Q', "Gln", "Glutamine").SetPkas(2.1f, 9.1f).SetFormula("H<sub>2</sub>N-C=O-(CH<sub>2</sub>)<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('G', "Gly", "Glycine").SetPkas(2.4f, 9.7f).SetFormula("CH<sub>2</sub>-NH<sub>2</sub>-COOH"),
				new AminoAcid('H', "His", "Histidine").SetPkas(1.8f, 9.2f, 6.0f, true).SetFormula("C<sub>6</sub>H<sub>9</sub>N<sub>3</sub>O<sub>2</sub>"),
				new AminoAcid('I', "Ile", "Isoleucine").SetPkas(2.4f, 9.7f).SetFormula("C<sub>6</sub>H<sub>13</sub>NO<sub>2</sub>"),
				new AminoAcid('L', "Leu", "Leucine").SetPkas(2.4f, 9.6f).SetFormula("(H<sub>3</sub>C)<sub>2</sub>-CH-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('K', "Lys", "Lysine").SetPkas(2.2f, 8.9f, 10.5f, true).SetFormula("H<sub>2</sub>N-(CH<sub>2</sub>)<sub>4</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('M', "Mét", "Méthionine").SetPkas(2.3f, 9.2f).SetFormula("H<sub>3</sub>C-S-(CH<sub>2</sub>)<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('F', "Phé", "Phénylalanine").SetPkas(2.6f, 9.2f).SetFormula("C<sub>9</sub>H<sub>11</sub>NO<sub>2</sub>"),
				new AminoAcid('P', "Pro", "Proline").SetPkas(2.0f, 10.6f).SetFormula("C<sub>5</sub>H<sub>9</sub>NO<sub>2</sub>"),
				new AminoAcid('S', "Sér", "Sérine").SetPkas(2.2f, 9.2f).SetFormula("HO-CH<sub>2</sub>-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('T', "Thr", "Thréonine").SetPkas(2.6f, 10.4f).SetFormula("H<sub>3</sub>C-OH-CH-CH-NH<sub>2</sub>-COOH"),
				new AminoAcid('W', "Trp", "Tryptophane").SetPkas(2.4f, 9.4f).SetFormula("C<sub>11</sub>H<sub>12</sub>N<sub>2</sub>O<sub>2</sub>"),
				new AminoAcid('Y', "Tyr", "Tyrosine").SetPkas(2.2f, 9.1f, 10.0f, false).SetFormula("C<sub>9</sub>H<sub>11</sub>NO<sub>3</sub>"),
				new AminoAcid('V', "Val", "Valine").SetPkas(2.3f, 9.7f).SetFormula("(H<sub>3</sub>C)<sub>2</sub>-CH-CH-NH<sub>2</sub>-COOH")
			};
			return new ReadOnlyCollection<AminoAcid>(lstAminoAcids);
		}

#if NET40 || NET35 || NET20 || NETSTANDARD1_1
		public static readonly IDictionary<AlphaAminoAcid, AminoAcid> AlphaLookup = MakeAlphaLookup();
#else
		public static readonly IReadOnlyDictionary<AlphaAminoAcid, AminoAcid> AlphaLookup = MakeAlphaLookup();
#endif

#if NET40 || NET35 || NET20 || NETSTANDARD1_1
		private static IDictionary<AlphaAminoAcid, AminoAcid> MakeAlphaLookup()
#else
		private static IReadOnlyDictionary<AlphaAminoAcid, AminoAcid> MakeAlphaLookup()
#endif
		{
			var lstAminoAcids = AminoAcid.AlphaList;
			var dicAminoAcids = new SortedDictionary<AlphaAminoAcid, AminoAcid>();
			for (int i = 0; i < lstAminoAcids.Count; i++) {
#if NETFRAMEWORK || NETCOREAPP1_0 || NETSTANDARD
				var enuSymbol = (AlphaAminoAcid)Enum.Parse(typeof(AlphaAminoAcid), lstAminoAcids[i].Symbol.Replace('é', 'e'));
#else
				var enuSymbol = Enum.Parse<AlphaAminoAcid>(lstAminoAcids[i].Symbol.Replace('é', 'e'));
#endif
				dicAminoAcids.Add(enuSymbol, lstAminoAcids[i]);
			}
#if NET46 || NETCOREAPP || NETSTANDARD
			return new ReadOnlyDictionary<AlphaAminoAcid, AminoAcid>(dicAminoAcids);
#else
			return dicAminoAcids;
#endif
		}

		public float WeightedCharge(float pH)
		{
			if ((pH < 1) || (pH > 14)) {
				throw new ArgumentOutOfRangeException("pH");
			}
			bool dicat = this.DicationWhenVeryAcid;
			var ar = this.pKaR;
			var pkI = this.Isoelectric();
			float am;
			if (RoundOff.Equals(pH, pkI)) {
				return 0;
			} else if (RoundOff.Equals(pH, this.pKa1)) {
				return dicat ? 1.5f : 0.5f;
			} else if (Single.IsNaN(ar)) { // without lateral chain
				if (RoundOff.Equals(pH, this.pKa2)) {
					return -0.5f;
				} else if ((Math.Abs(pH - this.pKa1) <= 1.0f) || (Math.Abs(pH - this.pKa2) <= 1.0f)) {
					if (pH < pkI) {
						return ProtonationRatio(pH, this.pKa1);
					} else {
						return ChargeOfLateral(pH, this.pKa2, false, 0 + 1);
					}
				} else {
					return -1 + ProtonationRatio(pH, this.pKa1) + ProtonationRatio(pH, this.pKa2);
				}
			} else { // with lateral chain
				var a2Ltar = this.pKa2 < ar;
				if (RoundOff.Equals(pH, this.pKa2)) {
					return ChargeOfLateral(false, a2Ltar, dicat);
				} else if (RoundOff.Equals(pH, ar)) {
					return ChargeOfLateral(true, a2Ltar, dicat);
				} else if (RoundOff.Equals(pH, (dicat ? a2Ltar ? this.pKa1 + this.pKa2 : this.pKa1 + ar : this.pKa2 + ar) * 0.5f)) {
					return dicat ? 1 : -1;
				} else if ((Math.Abs(pH - this.pKa1) <= 1.0f) || (Math.Abs(pH - this.pKa2) <= 1.0f) || (Math.Abs(pH - this.pKaR) <= 1.0f)) {
					am = Math.Min(this.pKa2, ar);
					var ah = Math.Max(this.pKa2, ar);
					if (2 * pH < this.pKa1 + am) { // pH < 0.5f * (this.pKa1 + am)
						return ChargeOfLateral(pH, this.pKa1, dicat, 2);
					} else if (2 * pH < am + ah) {
						return ChargeOfLateral(pH, am, dicat, 1);
					} else {
						return ChargeOfLateral(pH, ah, dicat, 0);
					}
				} else {
					return (dicat ? -1 : -2) + ProtonationRatio(pH, this.pKa1) + ProtonationRatio(pH, this.pKa2) +
					 ProtonationRatio(pH, ar);
				}
			}
		}

		private static float ProtonationRatio(float pH, float pKa)
		{
			var dblPower = Math.Pow(10, pKa - pH);
			return (float)(dblPower / (dblPower + 1));
		}

		private static float ChargeOfLateral(float pH, float pKa, bool dicat, short mostAcidicCharge)
		{
			var dblAlkaliRatio = Math.Pow(10, pH - pKa);
			var c = dicat ? mostAcidicCharge : mostAcidicCharge - 1;
			return (float)(c - (dblAlkaliRatio / (dblAlkaliRatio + 1)));
		}

		private static float ChargeOfLateral(bool pHEqualsPkar, bool pKa2LessThanPkar, bool dicat)
		{
			var blnEquals = pHEqualsPkar == pKa2LessThanPkar;
			return dicat ? blnEquals ? -0.5f : 0.5f : blnEquals ? -1.5f : -0.5f;
		}

		private static float Isoelectric(float pKa1, float pKa2, byte cationCountAtPh1AndHalf, float pKaR)
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

		private static float Isoelectric(float pKa1, float pKa2)
		{
			return RoundOff.Error(0.5f * (pKa1 + pKa2));
		}
	}
}
