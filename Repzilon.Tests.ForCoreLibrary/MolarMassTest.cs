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
	internal static class MolarMassTest
	{
		[StructLayout(LayoutKind.Auto)]
		private struct FattyAcid
		{
			public string Name;
			public string Formula;
			public float MolarMass;
			public float MeltingPoint;

			public FattyAcid SetFormula(string formula)
			{
				this.MolarMass = Chemistry.MolarMass(formula);
				this.Formula = formula;
				return this;
			}

			public static FattyAcid Create(string name, float meltingPointInCelsius, string formula)
			{
				if (String.IsNullOrWhiteSpace(name)) {
					throw new ArgumentNullException("name");
				}
				if (meltingPointInCelsius < -273.15) {
					throw new ArgumentOutOfRangeException("meltingPointInCelsius");
				}

				var fat = new FattyAcid
				{
					Name = name.Trim(),
					MeltingPoint = meltingPointInCelsius
				};
				fat.SetFormula(formula);
				return fat;
			}
		}

		internal static void Run(string[] args)
		{
			var karFormulas = new string[] { "Ca(OH)<sub>2</sub>",
			 "KH<sub>2</sub>PO<sub>4</sub>", "K<sub>2</sub>HPO<sub>4</sub>",
			 "C<sub>6</sub>H<sub>5</sub>COOH", "HOC<sub>6</sub>H<sub>4</sub>NO<sub>2</sub>",
			 "CH<sub>3</sub>COOH" };
			for (var i = 0; i < karFormulas.Length; i++) {
				Console.WriteLine("{0,9:n3} {1}", Chemistry.MolarMass(karFormulas[i]), karFormulas[i]);
			}

			Program.OutputSizeOf<AminoAcid>();
			var lstAminoAcids = AminoAcid.AlphaList;

			var dicAminoAcids = new SortedDictionary<string, AminoAcid>();
			for (var i = 0; i < lstAminoAcids.Count; i++) {
				dicAminoAcids.Add(lstAminoAcids[i].Name, lstAminoAcids[i]);
			}

			foreach (var aa in dicAminoAcids.Values) {
				Console.WriteLine("{0} {1} {2,-20} {3,4:f1} {4,4:f1} {5,4:f1} {6,5:f2}",
				 aa.Letter, aa.Symbol, aa.Name, aa.pKa1, aa.pKa2, aa.pKaR, aa.Isoelectric());
			}

			Program.OutputSizeOf<FattyAcid>();
			var lstFats = new List<FattyAcid> {
				FattyAcid.Create("Acide butyrique", -7.9f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>2</sub>COOH"),
				FattyAcid.Create("Acide caproïque", -3.5f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>4</sub>COOH"),
				FattyAcid.Create("Acide caprique", 31.6f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>8</sub>COOH"),
				FattyAcid.Create("Acide laurique", 44.2f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>10</sub>COOH"),
				FattyAcid.Create("Acide myristique", 53.9f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>12</sub>COOH"),
				FattyAcid.Create("Acide palmitique", 63.1f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>14</sub>COOH"),
				FattyAcid.Create("Acide stéarique", 69.6f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>16</sub>COOH"),
				FattyAcid.Create("Acide arachidique", 76.5f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>18</sub>COOH"),
				FattyAcid.Create("Acide béhénique", 80.0f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>20</sub>COOH"),
				FattyAcid.Create("Acide lignocérique", 86.0f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>22</sub>COOH"),
				FattyAcid.Create("Acide palmitoléique", -0.5f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>5</sub>CH=CH(CH<sub>2</sub>)<sub>7</sub>COOH"),
				FattyAcid.Create("Acide oléique", 13.4f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>7</sub>CH=CH(CH<sub>2</sub>)<sub>7</sub>COOH"),
				FattyAcid.Create("Acide linoléique", -5.0f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>4</sub>(CH=CH-CH<sub>2</sub>)<sub>2</sub>(CH<sub>2</sub>)<sub>6</sub>COOH"),
				FattyAcid.Create("Acide α-linolénique", -11.0f, "CH<sub>3</sub>CH<sub>2</sub>(CH=CH-CH<sub>2</sub>)<sub>3</sub>(CH<sub>2</sub>)<sub>6</sub>COOH"),
				FattyAcid.Create("Acide arachidonique", -49.5f, "CH<sub>3</sub>(CH<sub>2</sub>)<sub>4</sub>(CH=CH-CH<sub>2</sub>)<sub>4</sub>(CH<sub>2</sub>)<sub>2</sub>COOH"),
				FattyAcid.Create("Acide eicosapentaénoïque", -53.5f, "CH<sub>3</sub>CH<sub>2</sub>(CH=CH-CH<sub>2</sub>)<sub>5</sub>(CH<sub>2</sub>)<sub>2</sub>COOH"),
				FattyAcid.Create("Acide docosahexaénoïque", -44.6f, "CH<sub>3</sub>CH<sub>2</sub>(CH=CH-CH<sub>2</sub>)<sub>6</sub>CH<sub>2</sub>COOH")
			};
			var MH = Chemistry.ElementMasses["H"];
			foreach (var fat in lstFats) {
				var dicElems = Chemistry.ElementComposition(fat.Formula);
				var nC = dicElems["C"];
				var nH = dicElems["H"];
				var nO = dicElems["O"];
				Console.WriteLine("{0,-25} {1,5:f1}°C {2,5:f1}g/mol C{3,-2}H{4,-2}O{5} {6,4:f1}%H/mol {7,4:f1}%H/g {8:f4}%H/mol/K {9:f4}%C/mol/K {10:f4}%H-C/mol/K",
				 fat.Name, fat.MeltingPoint, fat.MolarMass, nC, nH, nO,
				 100.0 * nH / (1.0 * (nC + nH + nO)), MH * nH * 100 / fat.MolarMass,
				 100.0 * nH / ((273.15 + fat.MeltingPoint) * (nC + nH + nO)),
				 100.0 * nC / ((273.15 + fat.MeltingPoint) * (nC + nH + nO)),
				 100.0 * (nH - nC) / ((273.15 + fat.MeltingPoint) * (nC + nH + nO)));
			}
		}
	}
}
