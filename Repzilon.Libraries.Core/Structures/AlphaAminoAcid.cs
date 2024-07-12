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
	/// List of alpha amino acid three-letter codes
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
#if NET20
		public static bool IsAlkali(AlphaAminoAcid y)
#else
		public static bool IsAlkali(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Lys) || (y == AlphaAminoAcid.Arg) || (y == AlphaAminoAcid.His);
		}

#if NET20
		public static bool IsAcidic(AlphaAminoAcid y)
#else
		public static bool IsAcidic(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Asp) || (y == AlphaAminoAcid.Glu);
		}

#if NET20
		public static bool IsBranched(AlphaAminoAcid y)
#else
		public static bool IsBranched(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Val) || (y == AlphaAminoAcid.Leu) || (y == AlphaAminoAcid.Ile);
		}

#if NET20
		public static bool HasAlcohol(AlphaAminoAcid y)
#else
		public static bool HasAlcohol(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Ser) || (y == AlphaAminoAcid.Thr);
		}

#if NET20
		public static bool HasTwoAsymmetricCarbons(AlphaAminoAcid y)
#else
		public static bool HasTwoAsymmetricCarbons(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Ser) || (y == AlphaAminoAcid.Thr) || (y == AlphaAminoAcid.Ile);
		}

#if NET20
		public static bool IsAromatic(AlphaAminoAcid y)
#else
		public static bool IsAromatic(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Phe) || (y == AlphaAminoAcid.Tyr) || (y == AlphaAminoAcid.Trp);
		}

#if NET20
		public static bool HasSulfur(AlphaAminoAcid y)
#else
		public static bool HasSulfur(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Cys) || (y == AlphaAminoAcid.Met);
		}

#if NET20
		public static bool HasAmide(AlphaAminoAcid y)
#else
		public static bool HasAmide(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Asn) || (y == AlphaAminoAcid.Gln);
		}

#if NET20
		public static bool HasPkaR(AlphaAminoAcid y)
#else
		public static bool HasPkaR(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Lys) || (y == AlphaAminoAcid.Arg) || (y == AlphaAminoAcid.His) ||
			 (y == AlphaAminoAcid.Asp) || (y == AlphaAminoAcid.Glu) ||
			 (y == AlphaAminoAcid.Tyr) || (y == AlphaAminoAcid.Cys);
		}

#if NET20
		public static byte CationCountWhenVeryAcidic(AlphaAminoAcid y)
#else
		public static byte CationCountWhenVeryAcidic(this AlphaAminoAcid y)
#endif
		{
			return IsAlkali(y) ? (byte)2 : (byte)1;
		}

#if NET20
		public static byte AsymmetricCarbonCount(AlphaAminoAcid y)
#else
		public static byte AsymmetricCarbonCount(this AlphaAminoAcid y)
#endif
		{
			if (HasTwoAsymmetricCarbons(y)) {
				return 2;
#pragma warning disable IDE0046 // Convert to conditional expression
#pragma warning disable CC0013 // Use ternary operator
			} else if (y == AlphaAminoAcid.Gly) {
#pragma warning restore CC0013 // Use ternary operator
#pragma warning restore IDE0046 // Convert to conditional expression
				return 0;
			} else {
				return 1;
			}
		}

#if NET20
		public static bool IsEssentialForAdults(AlphaAminoAcid y)
#else
		public static bool IsEssentialForAdults(this AlphaAminoAcid y)
#endif
		{
			return (y == AlphaAminoAcid.Leu) || (y == AlphaAminoAcid.Ile) || (y == AlphaAminoAcid.Phe) ||
			 (y == AlphaAminoAcid.Trp) || (y == AlphaAminoAcid.Thr) || (y == AlphaAminoAcid.Lys) ||
			 (y == AlphaAminoAcid.Val) || (y == AlphaAminoAcid.Met);
		}

#if NET20
		public static bool IsEssentialForKids(AlphaAminoAcid y)
#else
		public static bool IsEssentialForKids(this AlphaAminoAcid y)
#endif
		{
			return IsEssentialForAdults(y) || (y == AlphaAminoAcid.His) || (y == AlphaAminoAcid.Arg);
		}
	}
}
