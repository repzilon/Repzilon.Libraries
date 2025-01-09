//
//  ChemistryTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2025 René Rhéaume
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
using Repzilon.Libraries.Core.Biochemistry;
using Repzilon.Libraries.Core.Regression;
// ReSharper disable InconsistentNaming

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class ChemistryTest
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
#pragma warning disable CC0021 // Use nameof
#pragma warning disable RECS0163 // Suggest the usage of the nameof operator
					throw new ArgumentNullException("name");
				}
				if (meltingPointInCelsius < -273.15) {
					throw new ArgumentOutOfRangeException("meltingPointInCelsius");
				}
#pragma warning restore RECS0163 // Suggest the usage of the nameof operator
#pragma warning restore CC0021 // Use nameof

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
				"C<sub>10</sub>H<sub>14</sub>N<sub>2</sub>Na<sub>2</sub>O<sub>8</sub>.2 H<sub>2</sub>O",
				"NO<sub>3</sub>", "NaNO<sub>3</sub>", "PO<sub>4</sub>",
				"Na<sub>3</sub>PO<sub>4</sub>•12 H<sub>2</sub>O", "SO<sub>4</sub>", "Na<sub>2</sub>SO<sub>4</sub>"
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
			var n = 0;
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
			var rmdMM = RegressionModel.Compute(ptdarMM);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.MichaelisMenten, true, rmdMM, ptdarMM);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.LineweaverBurk, true, rmdMM, ptdarLB_raw);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.LineweaverBurk, true, rmdMM, ptdarLB_table);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.EadieHofstee, true, rmdMM, ptdarEH_raw);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.EadieHofstee, true, rmdMM, ptdarEH_table);
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.HanesWoolf, true, rmdMM, ptdarHW_raw);

			//OutputRoundedEnzymeKinematic(Enzyme.Speed("mmol/L", A240By30s, ptdarMM), rmdMM);
			OutputRoundedEnzymeKinematic(Enzyme.DirectLinearPlot("mmol/L", A240By30s, ptdarMM), rmdMM);
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
			OutputEnzymeKinematic(EnzymeSpeedRepresentation.MichaelisMenten, true, ptmarMM);
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

		private static void OutputEnzymeKinematic(EnzymeSpeedRepresentation representation, bool withKinematic,
		RegressionModel<double> experimental, params PointD[] dataPoints)
		{
			if (withKinematic) {
				OutputRoundedEnzymeKinematic(Enzyme.Speed("mmol/L", A240By30s, representation, dataPoints), experimental);
			} else {
				LinearRegressionTest.OutputRegressionModel(RegressionModel.Compute(dataPoints));
			}
		}

		private static void OutputEnzymeKinematic(EnzymeSpeedRepresentation representation, bool withKinematic,
		PointM[] dataPoints)
		{
			if (withKinematic) {
				var kinematic = Enzyme.Speed("mmol/L", A240By30s, representation, dataPoints);
				OutputEnzymeKinematic(EnzymeKinematicExtension.RoundedToPrecision(kinematic, 4));
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

		private static void OutputRoundedEnzymeKinematic(EnzymeKinematic<double> kinematic,
		RegressionModel<double> michaelisMenten)
		{
			OutputEnzymeKinematic(EnzymeKinematicExtension.RoundedToPrecision(kinematic, 4));
			//*
			var z1 = FindCrossingInInterval(12.5, 1.07 * kinematic.Km.Key, michaelisMenten, kinematic);
			var x1 = FindNewtonCrossing(kinematic, michaelisMenten, kinematic.Km.Key);
			Console.WriteLine("Croisement autour de Km à\t{0,18:f15} {1} avec Newton, {2,18:f15} avec les quarts", x1, kinematic.Km.Value, z1);

			var z0 = FindCrossingInInterval(0, 12.5, michaelisMenten, kinematic);
			var x0 = FindNewtonCrossing(kinematic, michaelisMenten, 12.5 * 0.5);
			Console.WriteLine("Croisement bas à\t\t{0,18:f15} {1} avec Newton, {2,18:f15} avec les quarts", x0, kinematic.Km.Value, z0);

			var z2 = FindCrossingInInterval(1.07 * kinematic.Km.Key, 100, michaelisMenten, kinematic);
			var x2 = FindNewtonCrossing(kinematic, michaelisMenten, (kinematic.Km.Key + 100) * 0.5);
			Console.WriteLine("Croisement haut à\t\t{0,18:f15} {1} avec Newton, {2,18:f15} avec les quarts", x2, kinematic.Km.Value, z2);
			// */
		}

		private static double FindNewtonCrossing(EnzymeKinematic<double> kinematic,
		RegressionModel<double> michaelisMenten, double candidate)
		{
			double fx = Double.NaN, dx;
			int k = 1;
			var dcmTargetDelta = NormalLawTest.FinalTargetDelta();
			do {
				fx = ExperimentalMinusTheorical(michaelisMenten, kinematic, candidate);
				dx = ExperimentalMinusTheoricalDerivative(michaelisMenten, kinematic, candidate);
				candidate -= fx / dx;
				k++;
			} while ((decimal)Math.Abs(fx) > dcmTargetDelta);
			Console.WriteLine("Newton: différence de {0} après {1} itérations", fx, k);
			return candidate;
		}

		private static double FindCrossingInInterval(double min, double max,
		RegressionModel<double> experimental, EnzymeKinematic<double> theorical)
		{
			var dcmTargetDelta = NormalLawTest.FinalTargetDelta();
			while (!RoundOff.AreEqual(max - min, 0)) {
				var x1of4 = (3 * min + max) * 0.25;
				var x3of4 = (min + 3 * max) * 0.25;
				var y1of4 = ExperimentalMinusTheorical(experimental, theorical, x1of4);
				var y3of4 = ExperimentalMinusTheorical(experimental, theorical, x3of4);
				if ((decimal)Math.Abs(y1of4) < dcmTargetDelta) {
					return x1of4;
				} else if ((decimal)Math.Abs(y3of4) < dcmTargetDelta) { // RoundOff.AreEqual(y3of4, 0)
					return x3of4;
				} else if (Math.Abs(y1of4) < Math.Abs(y3of4)) {
					max = (min + max) * 0.5;
				} else {
					min = (min + max) * 0.5;
				}
			}
			return Double.NaN;
		}

		private static double ExperimentalMinusTheorical(RegressionModel<double> experimental,
		EnzymeKinematic<double> theorical, double x)
		{
			return experimental.Evaluate(x) - (theorical.Vmax.Key * x / (x + theorical.Km.Key));
		}

		private static double ExperimentalMinusTheoricalDerivative(RegressionModel<double> experimental,
		EnzymeKinematic<double> theorical, double x)
		{
			var km = theorical.Km.Key;
			return (experimental.A / (Math.Log(10) * x)) - (theorical.Vmax.Key * km / ((x + km) * (x + km)));
		}
	}
}
