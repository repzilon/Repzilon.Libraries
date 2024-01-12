//
//  AlphaAminoAcid.cs
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

namespace Repzilon.Libraries.Core
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
	public enum AlphaAminoAcid : byte
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

	public static class AlphaAminoAcidExtension
	{
		public static bool IsAlkali(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Lys) || (y == AlphaAminoAcid.Arg) || (y == AlphaAminoAcid.His);
		}

		public static bool IsAcidic(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Asp) || (y == AlphaAminoAcid.Glu);
		}

		public static bool IsBranched(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Val) || (y == AlphaAminoAcid.Leu) || (y == AlphaAminoAcid.Ile);
		}

		public static bool HasAlcohol(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Ser) || (y == AlphaAminoAcid.Thr);
		}

		public static bool HasTwoAsymetricCarbons(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Ser) || (y == AlphaAminoAcid.Thr) || (y == AlphaAminoAcid.Ile);
		}

		public static bool IsAromatic(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Phe) || (y == AlphaAminoAcid.Tyr) || (y == AlphaAminoAcid.Trp);
		}

		public static bool HasSulfur(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Cys) || (y == AlphaAminoAcid.Met);
		}

		public static bool HasAmide(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Asn) || (y == AlphaAminoAcid.Gln);
		}

		public static bool HasPkaR(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Lys) || (y == AlphaAminoAcid.Arg) || (y == AlphaAminoAcid.His) ||
			 (y == AlphaAminoAcid.Asp) || (y == AlphaAminoAcid.Glu) ||
			 (y == AlphaAminoAcid.Tyr) || (y == AlphaAminoAcid.Cys);
		}

		public static byte CationCountWhenVeryAcidic(this AlphaAminoAcid y)
		{
			return y.IsAlkali() ? (byte)2 : (byte)1;
		}

		public static byte AsymetricCarbonCount(this AlphaAminoAcid y)
		{
			if (y.HasTwoAsymetricCarbons()) {
				return 2;
			} else if (y == AlphaAminoAcid.Gly) {
				return 0;
			} else {
				return 1;
			}
		}

		public static bool IsEssentialForAdults(this AlphaAminoAcid y)
		{
			return (y == AlphaAminoAcid.Leu) || (y == AlphaAminoAcid.Ile) || (y == AlphaAminoAcid.Phe) ||
			 (y == AlphaAminoAcid.Trp) || (y == AlphaAminoAcid.Thr) || (y == AlphaAminoAcid.Lys) ||
			 (y == AlphaAminoAcid.Val) || (y == AlphaAminoAcid.Met);
		}

		public static bool IsEssentialForKids(this AlphaAminoAcid y)
		{
			return IsEssentialForAdults(y) || (y == AlphaAminoAcid.His) || (y == AlphaAminoAcid.Arg);
		}
	}
}
