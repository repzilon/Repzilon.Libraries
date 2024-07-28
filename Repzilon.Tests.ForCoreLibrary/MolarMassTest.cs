//
//  MolarMassTest.cs
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
using System.Globalization;
using System.Runtime.InteropServices;
using Repzilon.Libraries.Core;
// ReSharper disable InconsistentNaming

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class MolarMassTest
	{
#if DEBUG
		[StructLayout(LayoutKind.Sequential)]
#else
		[StructLayout(LayoutKind.Auto)]
#endif
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
#if NET35 || NET20
				if (RetroCompat.IsNullOrWhiteSpace(name)) {
#else
				if (String.IsNullOrWhiteSpace(name)) {
#endif
					throw new ArgumentNullException("name");
				}
				if (meltingPointInCelsius < -273.15) {
					throw new ArgumentOutOfRangeException("meltingPointInCelsius");
				}

				var fat = new FattyAcid {
					Name = name.Trim(),
					MeltingPoint = meltingPointInCelsius
				};
				fat.SetFormula(formula);
				return fat;
			}
		}

		private static readonly string A240By30s = IsMacOsX() ? "A₂₄₀/30 s" : "A<sub>240</sub>/30 s";

		private static bool IsMacOsX()
		{
#if NETFRAMEWORK
			return Environment.OSVersion.Platform == PlatformID.MacOSX;
#else
			return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif
		}

		internal static void Run(string[] args)
		{
			var karFormulas = new string[]
			{
				"Ca(OH)<sub>2</sub>",
				"KH<sub>2</sub>PO<sub>4</sub>", "K<sub>2</sub>HPO<sub>4</sub>",
				"C<sub>6</sub>H<sub>5</sub>COOH", "HOC<sub>6</sub>H<sub>4</sub>NO<sub>2</sub>",
				"CH<sub>3</sub>COOH", "(CH<sub>2</sub>COOH)<sub>2</sub>",
				"(CH<sub>3</sub>)<sub>2</sub>CO", "CHCl<sub>3</sub>",
				"HOOC-COOH•2 H<sub>2</sub>O",
				"C<sub>10</sub>H<sub>14</sub>N<sub>2</sub>Na<sub>2</sub>O<sub>8</sub>.2 H<sub>2</sub>O"
			};
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
				Console.WriteLine("{0} {1} {2,-20} {3,4:f1} {4,4} {5,4:f1} {6,5:f2} {7,7}g/mol {8}",
				 aa.Letter, aa.Symbol, aa.Name, aa.pKa1, Nanable(aa.pKa2, "f1"), aa.pKaR, aa.Isoelectric(),
				 Nanable(aa.MolarMass, "f3"), aa.Formula);
			}

			const string kBovineSerumAlbuminPeptides = /*"MKWVTFISLLLLFSSAYSRGVFRR" +*/ @"DTHKSEIAHRFKDLGEEHFKGLVLIAFSQYLQQCPF
DEHVKLVNELTEFAKTCVADESHAGCEKSLHTLFGDELCKVASLRETYGDMADCCEKQEP
ERNECFLSHKDDSPDLPKLKPDPNTLCDEFKADEKKFWGKYLYEIARRHPYFYAPELLYY
ANKYNGVFQECCQAEDKGACLLPKIETMREKVLASSARQRLRCASIQKFGERALKAWSVA
RLSQKFPKAEFVEVTKLVTDLTKVHKECCHGDLLECADDRADLAKYICDNQDTISSKLKE
CCDKPLLEKSHCIAEVEKDAIPENLPPLTADFAEDKDVCKNYQEAKDAFLGSFLYEYSRR
HPEYAVSVLLRLAKEYEATLEECCAKDDPHACYSTVFDKLKHLVDEPQNLIKQNCDQFEK
LGEYGFQNALIVRYTRKVPQVSTPTLVEVSRSLGKVGTRCCTKPESERMPCTEDYLSLIL
NRLCVLHEKTPVSEKVTKCCTESLVNRRPCFSALTPDETYVPKAFDEKLFTFHADICTLP
DTEKQIKKQTALVELLKHKPKATEEQLKTVMENFVAFVDKCCAADDKEACFAVEGPKLVV
STQTALA";
			Console.WriteLine("Molar mass of BSA (Bovine Serum Albumin) is {0:n3} g/mol", PolypeptideMass(kBovineSerumAlbuminPeptides.ToCharArray()));

			Program.OutputSizeOf<FattyAcid>();
			var lstFats = new List<FattyAcid>
			{
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
				FattyAcid.Create("Acide palmitoléique", -0.5f,
				 "CH<sub>3</sub>(CH<sub>2</sub>)<sub>5</sub>CH=CH(CH<sub>2</sub>)<sub>7</sub>COOH"),
				FattyAcid.Create("Acide oléique", 13.4f,
				 "CH<sub>3</sub>(CH<sub>2</sub>)<sub>7</sub>CH=CH(CH<sub>2</sub>)<sub>7</sub>COOH"),
				FattyAcid.Create("Acide linoléique", -5.0f,
				 "CH<sub>3</sub>(CH<sub>2</sub>)<sub>4</sub>(CH=CH-CH<sub>2</sub>)<sub>2</sub>(CH<sub>2</sub>)<sub>6</sub>COOH"),
				FattyAcid.Create("Acide α-linolénique", -11.0f,
				 "CH<sub>3</sub>CH<sub>2</sub>(CH=CH-CH<sub>2</sub>)<sub>3</sub>(CH<sub>2</sub>)<sub>6</sub>COOH"),
				FattyAcid.Create("Acide arachidonique", -49.5f,
				 "CH<sub>3</sub>(CH<sub>2</sub>)<sub>4</sub>(CH=CH-CH<sub>2</sub>)<sub>4</sub>(CH<sub>2</sub>)<sub>2</sub>COOH"),
				FattyAcid.Create("Acide eicosapentaénoïque", -53.5f,
				 "CH<sub>3</sub>CH<sub>2</sub>(CH=CH-CH<sub>2</sub>)<sub>5</sub>(CH<sub>2</sub>)<sub>2</sub>COOH"),
				FattyAcid.Create("Acide docosahexaénoïque", -44.6f,
				 "CH<sub>3</sub>CH<sub>2</sub>(CH=CH-CH<sub>2</sub>)<sub>6</sub>CH<sub>2</sub>COOH")
			};
			var MH = Chemistry.ElementMasses["H"];
			foreach (var fat in lstFats) {
				var dicElems = Chemistry.ElementComposition(fat.Formula);
				var nC = dicElems["C"];
				var nH = dicElems["H"];
				var nO = dicElems["O"];
				Console.WriteLine(
				 "{0,-25} {1,5:f1}°C {2,5:f1}g/mol C{3,-2}H{4,-2}O{5} {6,4:f1}%H/mol {7,4:f1}%H/g {8:f4}%H/mol/K {9:f4}%C/mol/K {10:f4}%H-C/mol/K",
				 fat.Name, fat.MeltingPoint, fat.MolarMass, nC, nH, nO,
				 100.0 * nH / (1.0 * (nC + nH + nO)), MH * nH * 100 / fat.MolarMass,
				 100.0 * nH / ((273.15 + fat.MeltingPoint) * (nC + nH + nO)),
				 100.0 * nC / ((273.15 + fat.MeltingPoint) * (nC + nH + nO)),
				 100.0 * (nH - nC) / ((273.15 + fat.MeltingPoint) * (nC + nH + nO)));
			}

			EnzymeSpeedFloat();
			EnzymeSpeedDecimal();
		}

		private static float PolypeptideMass(char[] peptideSequenceLetters)
		{
			float mass = 0;
			int n = 0;
			var aal = AminoAcid.AlphaLookup;
			for (int i = 0; i < peptideSequenceLetters.Length; i++) {
				var l = peptideSequenceLetters[i];
				AminoAcid aa;
				if (Char.IsLetter(l) && aal.TryGetValue((AlphaAminoAcid)Char.ToUpperInvariant(l), out aa)) {
					mass += aa.MolarMass;
					n++;
				}
			}
			return (float)Math.Round(mass - ((n - 1) * 18.015f), 3); // a peptidic bond formation is a dehydration
		}

		private static void EnzymeSpeedFloat()
		{
			const int kPoints = 5;
			// ReSharper disable RedundantExplicitArraySize
			var karSubstrate    = new float[kPoints] { 12.5f, 20, 25, 50, 100 };
			var karVelocity     = new float[kPoints] { 0.037f, 0.050f, 0.055f, 0.073f, 0.091f };
			var karSubstrateInv = new float[kPoints] { 0.08f, 0.05f, 0.04f, 0.02f, 0.01f };
			var karVelocityInv  = new float[kPoints] { 27, 20, 18.2f, 13.7f, 11 };
			var karVbyS         = new float[kPoints] { 0.0030f, 0.0025f, 0.0022f, 0.0015f, 0.00091f };
			// ReSharper restore RedundantExplicitArraySize
			var ptdarMM       = new PointD[kPoints];
			var ptdarLB_raw   = new PointD[kPoints];
			var ptdarLB_table = new PointD[kPoints];
			var ptdarEH_raw   = new PointD[kPoints];
			var ptdarEH_table = new PointD[kPoints];
			var ptdarHW_raw   = new PointD[kPoints];

			for (int i = 0; i < kPoints; i++) {
				var v0 = Math.Round(karVelocity[i], 3); // stupid C# compiler
				ptdarMM[i]     = new PointD(karSubstrate[i], v0);
				ptdarLB_raw[i] = new PointD(1.0 / karSubstrate[i], 1.0 / v0);
				ptdarLB_table[i] = new PointD(Math.Round(karSubstrateInv[i], 2), Math.Round(karVelocityInv[i], 1));
				ptdarEH_raw[i]   = new PointD(v0 / karSubstrate[i], v0);
				ptdarEH_table[i] = new PointD(Math.Round(karVbyS[i], 5), v0);
				ptdarHW_raw[i]   = new PointD(karSubstrate[i], karSubstrate[i] / v0);
			}

			Program.OutputSizeOf<RegressionModel<double>>();
			Program.OutputSizeOf<EnzymeKinematic<double>>();
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.MichaelisMenten, false, ptdarMM);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.LineweaverBurk, true, ptdarLB_raw);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.LineweaverBurk, true, ptdarLB_table);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.EadieHofstee, true, ptdarEH_raw);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.EadieHofstee, true, ptdarEH_table);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.HanesWoolf, true, ptdarHW_raw);

			OutputRoundedEnzymeKinematic(Chemistry.SpeedOfEnzyme("mmol/L", A240By30s, ptdarMM));
			OutputRoundedEnzymeKinematic(Chemistry.DirectLinearPlot("mmol/L", A240By30s, ptdarMM));
		}

		private static void EnzymeSpeedDecimal()
		{
			const int kPoints = 5;
			// ReSharper disable RedundantExplicitArraySize
			var karSubstrate    = new decimal[kPoints] { 12.5m, 20, 25, 50, 100 };
			var karVelocity     = new decimal[kPoints] { 0.037m, 0.050m, 0.055m, 0.073m, 0.091m };
			var karSubstrateInv = new decimal[kPoints] { 0.08m, 0.05m, 0.04m, 0.02m, 0.01m };
			var karVelocityInv  = new decimal[kPoints] { 27, 20, 18.2m, 13.7m, 11 };
			var karVbyS         = new decimal[kPoints] { 0.0030m, 0.0025m, 0.0022m, 0.0015m, 0.00091m };
			// ReSharper restore RedundantExplicitArraySize
			var ptmarMM       = new PointM[kPoints];
			var ptmarLB_raw   = new PointM[kPoints];
			var ptmarLB_table = new PointM[kPoints];
			var ptmarEH_raw   = new PointM[kPoints];
			var ptmarEH_table = new PointM[kPoints];
			var ptmarHW_raw   = new PointM[kPoints];

			for (int i = 0; i < kPoints; i++) {
				var v0 = karVelocity[i];
				ptmarMM[i]     = new PointM(karSubstrate[i], v0);
				ptmarLB_raw[i] = new PointM(1.0m / karSubstrate[i], 1.0m / v0);
				ptmarLB_table[i] = new PointM(karSubstrateInv[i], karVelocityInv[i]);
				ptmarEH_raw[i]   = new PointM(v0 / karSubstrate[i], v0);
				ptmarEH_table[i] = new PointM(karVbyS[i], v0);
				ptmarHW_raw[i]   = new PointM(karSubstrate[i], karSubstrate[i] / v0);
			}

			Program.OutputSizeOf<RegressionModel<decimal>>();
			Program.OutputSizeOf<EnzymeKinematic<decimal>>();
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.MichaelisMenten, false, ptmarMM);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.LineweaverBurk, true, ptmarLB_raw);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.LineweaverBurk, true, ptmarLB_table);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.EadieHofstee, true, ptmarEH_raw);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.EadieHofstee, true, ptmarEH_table);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.HanesWoolf, true, ptmarHW_raw);
		}

		private static string Nanable(float value, string format)
		{
			// «Non numérique» is too long
			return Single.IsNaN(value) && CultureInfo.CurrentCulture.Name.StartsWith("fr") ?
			 "!Num" : value.ToString(format);
		}

		private static void OutputEnzymeKinematic(EnzymeSpeedRepresentation representation, bool withKinematic, params PointD[] dataPoints)
		{
			if (withKinematic) {
				OutputRoundedEnzymeKinematic(Chemistry.SpeedOfEnzyme("mmol/L", A240By30s, representation, dataPoints));
			} else {
				LinearRegressionTest.OutputRegressionModel(RegressionModel.Compute(dataPoints));
			}
		}

		private static void OutputEnzymeKinematic(EnzymeSpeedRepresentation representation, bool withKinematic, params PointM[] dataPoints)
		{
			if (withKinematic) {
				OutputEnzymeKinematic(EnzymeKinematicExtension.RoundedToPrecision(Chemistry.SpeedOfEnzyme(
				 "mmol/L", A240By30s, representation, dataPoints), 4));
			} else {
				LinearRegressionTest.OutputRegressionModel(RegressionModel.Compute(dataPoints));
			}
		}

		private static void OutputEnzymeKinematic<T>(EnzymeKinematic<T> kinematic)
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IFormattable
		{
			var strKinematic = kinematic.ToString("g", CultureInfo.CurrentCulture);
			if (IsMacOsX()) {
				strKinematic = strKinematic.Replace("<sub>max</sub>", "ₘₐₓ").Replace("<sub>m</sub>", "ₘ");
			}
			Console.WriteLine(strKinematic);
		}

		private static void OutputRoundedEnzymeKinematic(EnzymeKinematic<double> kinematic)
		{
			OutputEnzymeKinematic(EnzymeKinematicExtension.RoundedToPrecision(kinematic, 4));
		}
	}
}
