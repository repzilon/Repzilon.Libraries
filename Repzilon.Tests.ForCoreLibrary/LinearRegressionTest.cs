//
//  LinearRegressionTest.cs
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
using Repzilon.Libraries.Core;
using Repzilon.Libraries.Core.Regression;
// ReSharper disable InconsistentNaming

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class LinearRegressionTest
	{
		private const byte MaxFactorial = 27;

		internal static void Run(string[] args)
		{
			const int kBenchIterationsDecimal = 7000;
			const int kBenchIterationsDouble = 25 * kBenchIterationsDecimal;

			var ptarDouble = new PointD[] {
				new PointD(2, 2.1f), new PointD(4, 4.4f), new PointD(6, 6.5f), new PointD(8, 8.6f),
				new PointD(10, 10.8f), new PointD(12, 12.9f)
			};
			Program.OutputHeading("Double data type");
			Program.OutputSizeOf<PointD>();
			Program.OutputSizeOf<LinearRegressionResult>();
			Program.OutputSizeOf<ErrorMargin<double>>();
			OutputLinearRegression2<LinearRegressionResult, double>(LinearRegression.Compute(ptarDouble),
			 "G", true, 8.25f, 3.4); // Rounding 3.4f takes more place
									 // x can also be 7 or 8, and y can also be 7.5
			int j;
			var dtmStart = DateTime.UtcNow;
			for (j = 0; j < kBenchIterationsDouble; j++) {
				LinearRegression.Compute((IEnumerable<PointD>)ptarDouble);
			}
			var tsEnumerable = DateTime.UtcNow - dtmStart;
			dtmStart = DateTime.UtcNow;
			for (j = 0; j < kBenchIterationsDouble; j++) {
				LinearRegression.Compute((IList<PointD>)ptarDouble);
			}
			var tsList = DateTime.UtcNow - dtmStart;
			OutputBenchResults<PointD>(kBenchIterationsDouble, tsEnumerable, tsList);

			var ptarDecimal = new PointM[] {
				new PointM(2, 2.1m), new PointM(4, 4.4m), new PointM(6, 6.5m), new PointM(8, 8.6m),
				new PointM(10, 10.8m), new PointM(12, 12.9m)
			};
			Program.OutputHeading("Decimal data type");
			Program.OutputSizeOf<PointM>();
			Program.OutputSizeOf<DecimalLinearRegressionResult>();
			Program.OutputSizeOf<ErrorMargin<decimal>>();
			var dlrp = LinearRegression.Compute(ptarDecimal);
			OutputLinearRegression2<DecimalLinearRegressionResult, decimal>(
			 dlrp, "G18", true, 7m, 7.5m);
			Console.WriteLine("a - {1} = {0}", dlrp.Intercept - 0.02m, 0.02m);
			dtmStart = DateTime.UtcNow;
			for (j = 0; j < kBenchIterationsDecimal; j++) {
				LinearRegression.Compute((IEnumerable<PointM>)ptarDecimal);
			}
			tsEnumerable = DateTime.UtcNow - dtmStart;
			dtmStart = DateTime.UtcNow;
			for (j = 0; j < kBenchIterationsDecimal; j++) {
				LinearRegression.Compute((IList<PointM>)ptarDecimal);
			}
			tsList = DateTime.UtcNow - dtmStart;
			OutputBenchResults<PointM>(kBenchIterationsDecimal, tsEnumerable, tsList);

			Program.OutputHeading("Revision");
			OutputLinearRegression2<DecimalLinearRegressionResult, decimal>(
			 LinearRegression.Compute(new PointM(0, 0.06m), new PointM(5, 1.25m),
			 new PointM(10, 2.38m), new PointM(15, 3.58m), new PointM(20, 4.61m)),
			  "G7", false, 12, 4.154m);

			Program.OutputHeading("Math I Example 38");
			Program.OutputSizeOf<RegressionModel<double>>();
			OutputRegressionModel(LinearRegression.Compute(PointD.LogLog(100, 0.240f),
			 PointD.LogLog(150, 0.295f), PointD.LogLog(250, 0.380f), PointD.LogLog(300, 0.415f),
			 PointD.LogLog(400, 0.480f), PointD.LogLog(550, 0.560f)).ChangeModel(MathematicalModel.LogLog));

			Program.OutputHeading("Math I Exercise");
			OutputRegressionModel(LinearRegression.Compute(PointD.SemiLogY(8, 9858), PointD.SemiLogY(14, 9416),
			 PointD.SemiLogY(18, 7234), PointD.SemiLogY(24, 5426), PointD.SemiLogY(37.5f, 2789),
			 PointD.SemiLogY(41, 2251), PointD.SemiLogY(71, 564)).ChangeModel(MathematicalModel.Exponential));

			Program.OutputHeading("Biochemistry II ch. 1 pp. 15-16");
			OutputRegressionModel(LinearRegression.Compute(PointD.LogLog(12.5f, 0.037f), PointD.LogLog(20, 0.050f),
			 PointD.LogLog(25, 0.055f), PointD.LogLog(50, 0.073f),
			 PointD.LogLog(100, 0.091f)).ChangeModel(MathematicalModel.Power));

			ptarDouble = new PointD[] {
				new PointD(12.5f, 0.037f), new PointD(20, 0.050f), new PointD(25, 0.055f), new PointD(50, 0.073f),
				new PointD(100, 0.091f)
			};
			OutputRegressionModel(RegressionModel.Compute(ptarDouble));
			dtmStart = DateTime.UtcNow;
			for (j = 0; j < kBenchIterationsDouble; j++) {
				RegressionModel.Compute((IEnumerable<PointD>)ptarDouble);
			}
			tsEnumerable = DateTime.UtcNow - dtmStart;
			dtmStart = DateTime.UtcNow;
			for (j = 0; j < kBenchIterationsDouble; j++) {
				RegressionModel.Compute((IList<PointD>)ptarDouble);
			}
			tsList = DateTime.UtcNow - dtmStart;
			OutputBenchResults<PointD>(kBenchIterationsDouble, tsEnumerable, tsList);
			ptarDecimal = new PointM[] {
				new PointM(12.5m, 0.037m), new PointM(20, 0.050m), new PointM(25, 0.055m), new PointM(50, 0.073m),
				new PointM(100, 0.091m)
			};
			OutputRegressionModel(RegressionModel.Compute(ptarDecimal));
			dtmStart = DateTime.UtcNow;
			for (j = 0; j < kBenchIterationsDecimal; j++) {
				RegressionModel.Compute((IEnumerable<PointM>)ptarDecimal);
			}
			tsEnumerable = DateTime.UtcNow - dtmStart;
			dtmStart = DateTime.UtcNow;
			for (j = 0; j < kBenchIterationsDecimal; j++) {
				RegressionModel.Compute((IList<PointM>)ptarDecimal);
			}
			tsList = DateTime.UtcNow - dtmStart;
			OutputBenchResults<PointM>(kBenchIterationsDecimal, tsEnumerable, tsList);

			Program.OutputHeading("Biochemistry II ch. 1 pp. 22-23");
			OutputRegressionModel(RegressionModel.Compute(new PointD(1.0f, 31.25f), new PointD(0.4f, 18.18f),
			 new PointD(0.2f, 13.89f), new PointD(0.1f, 11.11f)));
			OutputRegressionModel(RegressionModel.Compute(new PointD(1.0f, 47.62f), new PointD(0.4f, 24.39f),
			 new PointD(0.2f, 16.95f), new PointD(0.1f, 12.99f)));
			OutputRegressionModel(RegressionModel.Compute(new PointD(1.0f, 47.62f), new PointD(0.4f, 26.32f),
			 new PointD(0.2f, 20f), new PointD(0.1f, 16.39f)));

			Program.OutputHeading("Biochemistry II ch. 1 exercise 3");
			var lrr0 = LinearRegression.Compute(
				new PointD(1000000, RoundedInverse("1,16")), new PointD(100000, RoundedInverse("8,46")),
				new PointD(10000, RoundedInverse("24,94")), new PointD(1000, RoundedInverse("27,94")),
				new PointD(100, RoundedInverse("29,95"))
			);
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			var vmax0          = 1.0 / lrr0.Intercept;
			var Km0            = lrr0.Slope * vmax0;
			var blnPartialCode = Program.UnicodeTerminal != SupportLevel.None;
			var strVmax        = blnPartialCode ? "vₘₐₓ" : "vmax";
			var strKm          = blnPartialCode ? "kₘ" : "Km";
			Console.Write("{0} = {1:g4} nmol/min\t{2} = ", strVmax, vmax0 / 60, strKm);
			Console.WriteLine("{0:g4} mol/L", Km0);

			Program.OutputHeading("Biochemistry II ch. 1 exercise 4");
			lrr0 = LinearRegression.Compute(
				new PointD(100, RoundedInverse("16,7")), new PointD(Math.Round(100 / 1.33, 1), RoundedInverse("20")),
				new PointD(50, RoundedInverse("25")), new PointD(40, RoundedInverse("27")),
				new PointD(20, RoundedInverse("35,7")), new PointD(10, RoundedInverse("41,7"))
			);
			var lrr1 = LinearRegression.Compute(
				new PointD(100, RoundedInverse("10")), new PointD(Math.Round(100 / 1.33, 1), RoundedInverse("12,5")),
				new PointD(50, RoundedInverse("16,7")), new PointD(40, RoundedInverse("19,2")),
				new PointD(20, RoundedInverse("27,8")), new PointD(10, RoundedInverse("35,7"))
			);
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			OutputRegressionModel(lrr1.ChangeModel(MathematicalModel.Affine));
			vmax0 = 1.0 / lrr0.Intercept;
			Km0 = lrr0.Slope * vmax0;
			var strUnit = blnPartialCode ? "µmol/L•min" : "umol/L*min";
			Console.Write("{0}  = {1,-4:g3} {2}  ", strVmax, vmax0, strUnit);
			Console.WriteLine("{0}  = {1:g3} mol/L", strKm, Km0);
			var vmax1 = 1.0 / lrr1.Intercept;
			var Km1 = lrr1.Slope * vmax1;
			Console.Write("{0}' = {1,-4:g3} {2}  ", strVmax, vmax1, strUnit);
			Console.WriteLine("{0}' = {1:g3} mol/L", strKm, Km1);
			Console.WriteLine("{1} = {0:g3} mol/L", 0.02 / ((Km1 / Km0) - 1), blnPartialCode ? "kᵢ" : "Ki");

			Program.OutputHeading("Biochemistry II ch. 1 exercise 5");
			OutputRegressionModel(LinearRegression.Compute(new PointD(RoundedInverse("0,010"), RoundedInverse("0,27")),
			 new PointD(RoundedInverse("0,022"), RoundedInverse("0,50")), new PointD(RoundedInverse("0,046"),
			 RoundedInverse("0,80")), new PointD(RoundedInverse("0,200"),
			 RoundedInverse("1,50"))).ChangeModel(MathematicalModel.Affine));
			OutputRegressionModel(LinearRegression.Compute(new PointD(RoundedInverse("0,010"), RoundedInverse("0,21")),
			 new PointD(RoundedInverse("0,022"), RoundedInverse("0,40")), new PointD(RoundedInverse("0,046"),
			 RoundedInverse("0,65")), new PointD(RoundedInverse("0,200"),
			 RoundedInverse("1,18"))).ChangeModel(MathematicalModel.Affine));

			Program.OutputHeading("Biochemistry II ch. 1 exercise 6");
			lrr0 = LinearRegression.Compute(
				new PointD(4 / 1.5, 4), new PointD(6 / 2.5, 6), new PointD(7.5 / 3.5, 7.5),
				new PointD(10.4 / 6, 10.4), new PointD(14 / 12.0, 14)
			);
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			vmax0 = SignificantDigits.Round(lrr0.Intercept, 2);
			Km0 = SignificantDigits.Round(-lrr0.Slope, 2);
			Console.Write("{0}  = {1,-4} mUI  {2}  = ", strVmax, vmax0, strKm);
			Console.WriteLine("{0} mmol/L", Km0);

			Program.OutputHeading("Biochemistry II laboratory 3");
			lrr0 = LinearRegression.Compute(
				new PointD(10, 0.174f), new PointD(20, 0.285f), new PointD(30, 0.387f),
				new PointD(40, 0.511f), new PointD(51, 0.659f)
			);
			Console.Write("Absorbance: ");
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			lrr1 = LinearRegression.Compute(
				 new PointD(1.0 / 0.00150, 1.0 / 0.0071), new PointD(1.0 / 0.00090, 1.0 / 0.0044),
				 new PointD(1.0 / 0.00076, 1.0 / 0.0038), new PointD(1.0 / 0.00045, 1.0 / 0.0026),
				 new PointD(1.0 / 0.00030, 1.0 / 0.0018)
			);
			Console.Write("Reaction  : ");
			OutputRegressionModel(lrr1.ChangeModel(MathematicalModel.Affine));
			vmax0 = 1.0 / lrr1.Intercept;
			Km0 = vmax0 * lrr1.Slope;
			vmax1 = vmax0 * 60 / lrr0.Slope;
			Console.Write("{0}  = {1} A405/s  {2}  = ", strVmax, SignificantDigits.Round(vmax0, 5), strKm);
			Console.WriteLine("{0} mol/L", SignificantDigits.Round(Km0, 4));
			Console.WriteLine("{0}  = {1} {2}", strVmax, SignificantDigits.Round(vmax1, 3), strUnit);

			Program.OutputHeading("Biochemistry II laboratory 4");
			lrr0 = LinearRegression.Compute(
				new PointD(0.0100f, 0.142f), new PointD(0.020f, 0.251f),
				new PointD(0.030f, 0.390f), new PointD(0.040f, 0.520f)
			);
			Console.Write("Calibration    : ");
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			Km0 = lrr0.SlopeStdDev() * ProbabilityDistributions.InverseStudent(RoundOff.Error(1 - 0.025f), (byte)(lrr0.Count - 2));
			Km0 = SignificantDigits.Round(Km0, 1);
			Console.WriteLine("ε para-nitrophenol={0}±{1} A405*mL/µmol", Math.Round(lrr0.Slope), Km0);
			vmax1 = 60 / lrr0.Slope;
			var lstSpeeds = new List<PointD>(5);
			lrr1 = LinearRegression.Compute(new PointD(0, 0.01830f), new PointD(9.77f, 0.01830f),
			 new PointD(19.77f, 0.01970f), new PointD(29.78f, 0.02100f), new PointD(39.80f, 0.02070f),
			 new PointD(49.82f, 0.02270f), new PointD(59.83f, 0.02290f), new PointD(69.88f, 0.02460f),
			 new PointD(79.90f, 0.02520f), new PointD(89.91f, 0.02550f), new PointD(99.92f, 0.02650f),
			 new PointD(109.93f, 0.02770f), new PointD(119.94f, 0.02830f));
			vmax0 = lrr1.Slope;
			Console.Write("A@{0,-6} mg/mL : ", 0);
			OutputRegressionModel(lrr1.ChangeModel(MathematicalModel.Affine));
			Km1 = SpecificActivity(0.0061f, vmax0, vmax1, lstSpeeds, new PointD(0, 0.02640f), new PointD(9.77f, 0.03220f),
			 new PointD(19.78f, 0.03840f), new PointD(29.79f, 0.04510f), new PointD(39.80f, 0.05100f),
			 new PointD(49.82f, 0.05740f), new PointD(59.83f, 0.06410f), new PointD(69.88f, 0.06920f),
			 new PointD(79.90f, 0.07660f), new PointD(89.91f, 0.08170f), new PointD(99.92f, 0.08770f),
			 new PointD(109.93f, 0.09430f), new PointD(119.94f, 0.1013f));
			Km1 += SpecificActivity(0.0122f, vmax0, vmax1, lstSpeeds, new PointD(0, 0.03500f), new PointD(9.77f, 0.04580f),
			 new PointD(19.77f, 0.05740f), new PointD(29.79f, 0.06870f), new PointD(39.80f, 0.08010f),
			 new PointD(49.82f, 0.09110f), new PointD(59.83f, 0.1034f), new PointD(69.88f, 0.1145f),
			 new PointD(79.90f, 0.1271f), new PointD(89.91f, 0.1380f), new PointD(99.92f, 0.1499f),
			 new PointD(109.93f, 0.1623f), new PointD(119.94f, 0.1723f));
			Km1 += SpecificActivity(0.018f, vmax0, vmax1, lstSpeeds, new PointD(0, 0.05280f), new PointD(9.77f, 0.07520f),
			 new PointD(19.78f, 0.09810f), new PointD(29.79f, 0.1197f), new PointD(39.80f, 0.1425f),
			 new PointD(49.82f, 0.1641f), new PointD(59.83f, 0.1883f), new PointD(69.89f, 0.2096f),
			 new PointD(79.90f, 0.2306f), new PointD(89.91f, 0.2551f), new PointD(99.92f, 0.2766f),
			 new PointD(109.93f, 0.2996f), new PointD(119.95f, 0.3242f));
			Km1 += SpecificActivity(0.024f, vmax0, vmax1, lstSpeeds, new PointD(0, 0.04950f), new PointD(9.77f, 0.07090f),
			 new PointD(19.78f, 0.09200f), new PointD(29.79f, 0.1118f), new PointD(39.80f, 0.1358f),
			 new PointD(49.82f, 0.1563f), new PointD(59.83f, 0.1797f), new PointD(69.88f, 0.2015f),
			 new PointD(79.90f, 0.2233f), new PointD(89.91f, 0.2462f), new PointD(99.92f, 0.2699f),
			 new PointD(109.93f, 0.2909f), new PointD(119.94f, 0.3136f));
			Km1 += SpecificActivity(0.031f, vmax0, vmax1, lstSpeeds, new PointD(0, 0.07860f), new PointD(9.77f, 0.1106f),
			 new PointD(19.77f, 0.1451f), new PointD(29.78f, 0.1780f), new PointD(39.80f, 0.2115f),
			 new PointD(49.82f, 0.2445f), new PointD(59.83f, 0.2787f), new PointD(69.88f, 0.3117f),
			 new PointD(79.90f, 0.3452f), new PointD(89.91f, 0.3800f), new PointD(99.92f, 0.4161f),
			 new PointD(109.93f, 0.4514f), new PointD(119.94f, 0.4884f));
			Console.Write(blnPartialCode ? "Speed [dA/(dt•dC)]: " : "Speed [dA/(dt*dC)]: ");
			lrr1 = LinearRegression.Compute(lstSpeeds);
			OutputRegressionModel(lrr1.ChangeModel(MathematicalModel.Affine));
			Console.Write(blnPartialCode ? "Speed [dA/(dt•dC)]: " : "Speed [dA/(dt*dC)]: ");
			var rm = RegressionModel.Compute(lstSpeeds);
			OutputRegressionModel(rm);
			Km1 *= 0.2;
			const double kEnzymeIU2Katal = 0.000001 / 60;
			strUnit = blnPartialCode ? "BCA•min" : "BCA*min";
			Console.WriteLine(
			 "Specific activity average={0,-6:f3} µmol PNPA/mg {2} or {0,-6:f3} UI/mg BCA or {1:g3} katal/mg",
			 Km1, Km1 * kEnzymeIU2Katal, strUnit);
			Console.WriteLine(
			 "Specific activity linear ={0,-6:f4} µmol PNPA/mg {2} or {0,-6:f4} UI/mg BCA or {1:g3} katal/mg",
			 lrr1.Slope * vmax1, lrr1.Slope * vmax1 * kEnzymeIU2Katal, strUnit);
			Km0 = 0;
			for (j = 0; j < lstSpeeds.Count; j++) {
				Km0 += rm.Evaluate(lstSpeeds[j].X) / lstSpeeds[j].X;
			}
			Km0 *= 0.2 * vmax1;
			Console.WriteLine("Specific activity x_power={0,-6:f3} µmol PNPA/mg {1}", Km0, strUnit);
			Console.WriteLine("Specific activity ∫[0; {1}] power={0,-6:f3} µmol PNPA/mg {2}",
			 SpecificActivity(rm, 0, lstSpeeds[4].X, vmax1), lstSpeeds[4].X, strUnit);
			Console.Write("Specific activity ∫[{2}; {1}] power={0,-6:f3} µmol PNPA/mg ",
			 SpecificActivity(rm, lstSpeeds[0].X, lstSpeeds[4].X, vmax1), lstSpeeds[4].X, lstSpeeds[0].X);
			Console.WriteLine(strUnit);
			Console.WriteLine("Specific activity trapeze={0,-6:f3} µmol PNPA/mg {1}",
			 SpecificActivity(lstSpeeds, vmax1), strUnit);
			Km0 = (Km1 + (lrr1.Slope * vmax1)) * 0.5 * 30;
			Console.WriteLine("Molecular activity={0:f3} µmol PNPA/µmol {2} or {0:f3} UI/µmol BCA or {1:g3} katal/µmol",
			 Km0, Km0 * kEnzymeIU2Katal, strUnit);

			Program.OutputHeading("Cellular culture II Wound healing");
			OutputRegressionModel(RegressionModel.Compute(new PointD(0, -0.2779f),
			 new PointD(1, 0.2434f), new PointD(10, 1.1257f)));

#if !NET20
			Program.OutputHeading("Molecular biology laboratories");
			Program.OutputSizeOf<AgaroseRetention>();
			OutputAgaroseRetention("3B :", 27491, 9416, 6682, 2322, 2024, 564);
			OutputAgaroseRetention("6A :", 247, 280, 393, 234);
			OutputAgaroseRetention("6B :", 525);
			OutputAgaroseRetention("8  :", 2997, 647);
			OutputAgaroseRetention("9  :", 97);
#endif

			Program.OutputHeading("Immunotechniques lab 1C");
			OutputRegressionModel(RegressionModel.Compute(
				new PointD(1, Math.Round(0.772f, 3)), new PointD(1, Math.Round(0.744f, 3)), new PointD(1, Math.Round(0.787f, 3)),
				new PointD(0.5f, Math.Round(0.407f, 3)), new PointD(0.5f, Math.Round(0.394f, 3)), new PointD(0.5f, Math.Round(0.387f, 3)),
				new PointD(0.25f, Math.Round(0.211f, 3)), new PointD(0.25f, Math.Round(0.247f, 3)), new PointD(0.25f, Math.Round(0.216f, 3)),
				new PointD(0.125f, 0.126f), new PointD(0.125f, 0.131f), new PointD(0.125f, 0.133f),
				new PointD(0.0625f, 0.078f), new PointD(0.0625f, 0.081f), new PointD(0.0625f, 0.084f),
				new PointD(0.03125f, 0.054f), new PointD(0.03125f, 0.060f), new PointD(0.03125f, 0.059f),
				new PointD(0.0078125f, 0.043f), new PointD(0.0078125f, 0.047f), new PointD(0.0078125f, 0.044f)
			));

			Program.OutputHeading("Instrumental analysis II lab 8");
			const float kBloodPartition = 2100f / 2573f;
			OutputRegressionModel(RegressionModel.Compute(
				new PointD(0, 0), new PointD(0.0434f * kBloodPartition, 0.0319f),
				new PointD(0.0616f * kBloodPartition, 0.059f), new PointD(0.0853f * kBloodPartition, 0.078f),
				new PointD(0.1063f * kBloodPartition, 0.065f), new PointD(0.1213f * kBloodPartition, 0.071f),
				new PointD(0.1853f * kBloodPartition, 0.1617f)
			));

			byte i;

			Program.OutputHeading("Immunotechniques lab 2");
			Console.WriteLine("Concentration from absorbance");
			const float kBlank = 0.265f / 3;
			OutputRegressionModel(RegressionModel.Compute(
				new PointD(2, SignificantDigits.Round(0.323f - kBlank, 3)),
				new PointD(1, SignificantDigits.Round(0.212f - kBlank, 3)),
				new PointD(0.5f, SignificantDigits.Round(0.151f - kBlank, 3)),
				new PointD(0.25f, SignificantDigits.Round(0.1107f - kBlank, 3)),
				new PointD(0.125f, SignificantDigits.Round(0.0967f - kBlank, 3)),
				new PointD(0.0625f, SignificantDigits.Round(0.091f - kBlank, 3)),
				new PointD(0.03125f, SignificantDigits.Round(.087f - kBlank, 3))
			));
			Console.WriteLine("Molecular weight from relative mobility");
			const float kFrontDistance = 53.0f;
			ptarDouble = new PointD[] {
				new PointD(250000, 3.0f/kFrontDistance), new PointD(150000, 5.0f/kFrontDistance),
				new PointD(100000, 8.0f/kFrontDistance), new PointD(75000, 10.5f/kFrontDistance),
				new PointD(50000, 16.0f/kFrontDistance), new PointD(37000, 21.0f/kFrontDistance),
				new PointD(25000, 29.5f/kFrontDistance), new PointD(20000, 32.5f/kFrontDistance),
				new PointD(15000, 40.5f/kFrontDistance), new PointD(10000, 47.0f/kFrontDistance)
			};
			RegressionModel<double> rmdImmunoLabWeightLogOuter, rmdImmunoLabWeightLogInner;
			CalibrateGelElectrophoresis(ptarDouble,
			 out rmdImmunoLabWeightLogOuter, out rmdImmunoLabWeightLogInner, out rm);

			strUnit = blnPartialCode ?
			 "Pit Distance  Rf   M from log₁₀ w. tails M from log₁₀ no tail M from power" :
			 "Pit Distance  Rf   M from log10 w. tails M from log10 no tail M from power";

			Console.WriteLine(strUnit);
			InterpolateMolecularWeight(9, 53.5f, new[] { 30.5f, 17 }, rmdImmunoLabWeightLogOuter, rmdImmunoLabWeightLogInner, rm);

			Program.OutputHeading("Immunotechniques lab 4");
			const float kFrontDistance4Std = 90.5f;
			ptarDouble = new PointD[] {
				new PointD(250000, 20.5f/kFrontDistance4Std), new PointD(150000, 27.0f/kFrontDistance4Std),
				new PointD(100000, 32.5f/kFrontDistance4Std), new PointD(75000,  36.5f/kFrontDistance4Std),
				new PointD(50000,  44.5f/kFrontDistance4Std), new PointD(37000,  51.0f/kFrontDistance4Std),
				new PointD(25000,  59.0f/kFrontDistance4Std), new PointD(20000,  63.0f/kFrontDistance4Std),
				new PointD(15000,  70.5f/kFrontDistance4Std),
			};
			CalibrateGelElectrophoresis(ptarDouble,
			 out rmdImmunoLabWeightLogOuter, out rmdImmunoLabWeightLogInner, out rm);
			Console.WriteLine(strUnit);
			InterpolateMolecularWeight(2, kFrontDistance4Std, new float[] { 59, 66 }, rmdImmunoLabWeightLogOuter, rmdImmunoLabWeightLogInner, rm);
			InterpolateMolecularWeight(3, 90, new float[] { 59, 65 }, rmdImmunoLabWeightLogOuter, rmdImmunoLabWeightLogInner, rm);
			InterpolateMolecularWeight(4, 90, new float[] { 57.5f, 66 }, rmdImmunoLabWeightLogOuter, rmdImmunoLabWeightLogInner, rm);
			InterpolateMolecularWeight(5, 90, new float[] { 57.5f, 66.5f }, rmdImmunoLabWeightLogOuter, rmdImmunoLabWeightLogInner, rm);
			InterpolateMolecularWeight(6, kFrontDistance4Std, new float[] { 59, 65 }, rmdImmunoLabWeightLogOuter, rmdImmunoLabWeightLogInner, rm);

			Program.OutputHeading("Biofermentation week 3 exercise");
			ptarDouble = new PointD[] {
				new PointD(0, 1.5f), new PointD(5, 2), new PointD(9, 3.5f), new PointD(13, 6.2f),
				new PointD(16, 8.2f), new PointD(20, 9.4f), new PointD(24, 9.8f), new PointD(28, 9.9f)
			};
			for (i = 2; i <= ptarDouble.Length; i++) {
				Console.Write("{0,2}h : ", ptarDouble[i - 1].X);
				OutputRegressionModel(RegressionModel.Compute(Take(i, ptarDouble)));
			}
			var ptarLn = new PointD[ptarDouble.Length];
			for (i = 0; i < ptarDouble.Length; i++) {
				ptarLn[i] = new PointD(ptarDouble[i].X, Math.Log(ptarDouble[i].Y));
			}
			vmax1 = 0;
			for (i = 0; i < ptarDouble.Length - 1; i++) {
				vmax0 = LinearRegression.Compute(ptarLn[i], ptarLn[i + 1]).Slope;
				if (vmax0 > vmax1) {
					vmax1 = vmax0;
				}
			}
			Console.WriteLine(blnPartialCode ? "µₘₐₓ = {0:g2} h^-1\tG = {1:g2} h" : "umax = {0:g2} h^-1\tG = {1:g2} h",
			 vmax1, Math.Log(2) / vmax1);
			Km0   = ptarDouble[ptarDouble.Length - 1].Y - ptarDouble[0].Y;
			vmax0 = Km0 / (250 - 20);
			Km1   = (90f - 6.25f) / Km0;
			Console.WriteLine("Yx/s = {0:g2} %\tYp/s = {1:g2} %\tYp/x = {2} %",
			 100 * vmax0, 100 * ((90f - 6.25f) / (250 - 20)), SignificantDigits.Round(100 * Km1, 2));
			Console.WriteLine(blnPartialCode ?
			 "Pₓ tot = {0:g2} g/(L•h)\tPₓ ₘₐₓ = {1:g2} g/(L•h)" : "Px tot = {0:g2} g/(L*h)\tPx max = {1:g2} g/(L*h)",
			 Km0 / 28f, (8.2f - 1.5f) / 16f);
			Console.WriteLine(blnPartialCode ?
			 "Pp = {0:g2} g/(L•h)\tQp ₘₐₓ = {1:g2} h^-1" : "Pp = {0:g2} g/(L*h)\tQp max = {1:g2} h^-1",
			 (90f - 6.25f) / 28f, Km1 * vmax1);

			Program.OutputHeading("Ecotoxicology Microtox");
			var karSnowI0 = new byte[10] { 96, 88, 87, 86, 87, 92, 88, 87, 88, 80 };
			var karPO4WasteI0 = new byte[10] { 92, 96, 95, 95, 98, 94, 97, 98, 94, 94 };
			OutputMicrotox("Neige sale", 5, karSnowI0, new byte[10] { 125, 122, 129, 123, 126, 128, 116, 121, 98, 67 });
			OutputMicrotox("Neige sale", 15, karSnowI0, new byte[10] { 123, 118, 125, 121, 125, 123, 115, 114, 92, 65 });
			strUnit = blnPartialCode ? "Rejets PO₄" : "Rejets PO4";
			OutputMicrotox(strUnit, 5, karPO4WasteI0, new byte[10] { 126, 0, 0, 1, 0, 0, 0, 0, 0, 0 });
			OutputMicrotox(strUnit, 15, karPO4WasteI0, new byte[10] { 120, 0, 0, 1, 0, 0, 0, 0, 2, 0 });

			Program.OutputHeading("Instrumental analysis II Mass spectroscopy mean travel");
			ptarDouble = new PointD[] {
				new PointD(101325, 0.000006f), new PointD(130, 0.0045f), new PointD(0.13f, 4.5f),
				new PointD(0.013f, 45f), new PointD(0.0013f, 450f), new PointD(0.00013f, 4500f),
				new PointD(1.3e-5f, 45000f), new PointD(1.3e-7, 4500000)
			};
			OutputRegressionModel(RegressionModel.Compute(ptarDouble));
			PointD ptd;
			for (i = 0; i < ptarDouble.Length; i++) {
				ptd           = ptarDouble[i];
				ptarDouble[i] = new PointD(1.0 / ptd.X, ptd.Y);
			}
			rm = RegressionModel.Compute(ptarDouble);
			Console.Write("y = {1:g6} + {0:g6} * x^-1\tr={2,-9:g6} S/N=", rm.B, rm.A, rm.R);
			Console.WriteLine("{0:g6} dB", -10 * Math.Log10(1 - rm.R));

			Program.OutputHeading("Factorial (1 to " + MaxFactorial + ")");
			var factorialSuite = new List<PointM>(MaxFactorial);
			for (i = 1; i <= MaxFactorial; i++) {
				var exact = ExtraMath.BigFactorial(i);
				var approximate = ExtraMath.StirlingApproximateFactorial(i, StirlingMode.Rounded);
				if (approximate != exact) {
					factorialSuite.Add(new PointM(i, exact / approximate));
				}
				Console.WriteLine("{0,2}! is {1,38:n0} ≈ {2,38:n0}", i, exact, approximate);
			}
			Program.OutputSizeOf<RegressionModel<decimal>>();
			OutputRegressionModel(RegressionModel.Compute(factorialSuite));
			for (i = 1; i <= MaxFactorial; i++) {
				Console.WriteLine("{0,2}! is {1,38:n0} ≈ {2,38:n0}", i, ExtraMath.BigFactorial(i),
				 ExtraMath.StirlingApproximateFactorial(i, StirlingMode.Corrected));
			}
			Console.WriteLine("28! ≈ {0,39:n0}", ExtraMath.StirlingApproximateFactorial(28.0, StirlingMode.Corrected));

			Program.OutputHeading("German imperialists are out of luck");
			rm = RegressionModel.Compute(new PointD(1, 1006), new PointD(2, 1918 - 1871), new PointD(3, 1945 - 1933));
			OutputRegressionModel(rm);
			Console.WriteLine("The fourth reich would only last {0:g4} years.", rm.Evaluate(4));
		}

		private static void OutputBenchResults<T>(int iterations, TimeSpan withEnumerable, TimeSpan withList)
		{
			var strType = typeof(T).Name;
			Console.Write("{0,9:n0} iterations in {1:f3}s as IEnumerable<{2}>, ",
			 iterations, withEnumerable.TotalSeconds, strType);
			Console.WriteLine("{0:f3}s as IList<{1}>", withList.TotalSeconds, strType);
		}

		private static void OutputMicrotox(string sample, byte minutes, byte[] i0, byte[] it)
		{
			const double kMicrotoxMaxC = 81.9;
			var dblRefGamma = it[0] / (double)i0[0];
#if true
			// Like my Excel data
			var ptdarDeltas = new PointD[9];
			for (byte i = 1; i < 10; i++) {
				ptdarDeltas[i - 1] = new PointD(Math.Pow(2, i - 9) * kMicrotoxMaxC,
				 (dblRefGamma - (it[i] / (double)i0[i])) / dblRefGamma);
			}
#else
			// More like the generated report
			var ptdarDeltas = new List<PointD>(9);
			for (byte i = 1; i < 10; i++) {
				var inhibition = (dblRefGamma - ((double)it[i] / (double)i0[i])) / dblRefGamma;
				if ((inhibition > 0) || (i == 1)) {
					ptdarDeltas.Add(new PointD(Math.Pow(2, i - 9) * kMicrotoxMaxC, inhibition));
				}
			}
#endif
			Console.Write("{0} {1,2} min. ", sample, minutes);
			var rmdMicrotox = LinearRegression.Compute(ptdarDeltas).ChangeModel(MathematicalModel.Affine);
			OutputRegressionModel(rmdMicrotox);
			Console.WriteLine("\t\tCI50 = {0:g4}\tn = {1}", Effective50(rmdMicrotox, ptdarDeltas), ptdarDeltas.Length);
		}

		private static double Effective50(RegressionModel<double> line, IList<PointD> points)
		{
			// With external data, I would first sort the points by X, but I already filled lists that way.
			var c = points.Count;
			for (var i = 0; i < c; i++) {
				var pt = points[i];
				if (pt.Y > 0.5f) {
					return pt.X;
				}
			}
			return line.Solve(0.5f);
		}

		private static void InterpolateMolecularWeight(byte pitNumber, float migrationFront, float[] bands,
		RegressionModel<double> logarithmicFullModel, RegressionModel<double> logarithmicInnerModel,
		RegressionModel<double> powerModel)
		{
			for (var i = 0; i < bands.Length; i++) {
				var band = bands[i];
				var rf = band / migrationFront;
				Console.Write("{0,2}    {1,4:f1}   {2:f3}\t   ", pitNumber, band, rf);
				Console.WriteLine("{0:f0}\t\t{1:f0}\t\t  {2:f0}",
				 SignificantDigits.Round(logarithmicInnerModel.Solve(rf), 3),
				 SignificantDigits.Round(logarithmicFullModel.Solve(rf), 3),
				 SignificantDigits.Round(powerModel.Solve(rf), 3));
			}
		}

		private static void CalibrateGelElectrophoresis(
		PointD[] molarWeightsAndRelativeMobility,
		out RegressionModel<double> logarithmicFullModel,
		out RegressionModel<double> logarithmicInnerModel,
		out RegressionModel<double> powerModel)
		{
			int i;
			var c = molarWeightsAndRelativeMobility.Length;
			var ptdarImmunoLab2WeightLogOuter = new PointD[c];
			var ptdarImmunoLab2WeightLogInner = new PointD[checked(c - 2)];
			var ptdarImmunoLab2WeightPow = new PointD[c];
			for (i = 0; i < c; i++) {
				var ptd = molarWeightsAndRelativeMobility[i];
				var log10 = Math.Log10(ptd.X);
				var ptdLogLin = new PointD(log10, ptd.Y);
				var ptdLogLog = new PointD(log10, Math.Log10(ptd.Y));
				ptdarImmunoLab2WeightLogOuter[i] = ptdLogLin;
				if ((i != 0) && (i < c - 1)) {
					ptdarImmunoLab2WeightLogInner[i - 1] = ptdLogLin;
				}
				ptdarImmunoLab2WeightPow[i] = ptdLogLog;
			}
			var lrdImmunoLab2WeightLogOuter = LinearRegression.Compute(ptdarImmunoLab2WeightLogOuter);
			var lrdImmunoLab2WeightLogInner = LinearRegression.Compute(ptdarImmunoLab2WeightLogInner);
			var lrdImmunoLab2WeightPow = LinearRegression.Compute(ptdarImmunoLab2WeightPow);
			logarithmicFullModel = lrdImmunoLab2WeightLogOuter.ChangeModel(MathematicalModel.Logarithmic);
			logarithmicInnerModel = lrdImmunoLab2WeightLogInner.ChangeModel(MathematicalModel.Logarithmic);
			powerModel = lrdImmunoLab2WeightPow.ChangeModel(MathematicalModel.Power);
			OutputRegression("Log. with tails", lrdImmunoLab2WeightLogOuter, logarithmicFullModel);
			OutputRegression("Log. no tails", lrdImmunoLab2WeightLogInner, logarithmicInnerModel);
			OutputRegression("Power", lrdImmunoLab2WeightPow, powerModel);
		}

#if !NET20
		private static void OutputAgaroseRetention(string heading, params short[] fragmentLengths)
		{
			var agars = MolecularBiology.AgaroseConcentration(fragmentLengths);
			Console.Write(heading);
			if (agars.Length != 1) {
				Console.WriteLine();
			}
			for (var i = 0; i < agars.Length; i++) {
				if (!String.IsNullOrEmpty(heading)) {
					Console.Write('\t');
				}
				Console.WriteLine(agars[i]);
			}
		}
#endif

		private static void OutputLinearRegression2<TRegression, TStorage>(TRegression lrp,
		string numberFormat, bool checkBiases, TStorage xForYExtrapolation, TStorage yForXExtrapolation)
		where TRegression : struct, ILinearRegressionResult<TStorage>
		where TStorage : struct, IConvertible, IFormattable, IComparable<TStorage>, IEquatable<TStorage>, IComparable
		{
			var ciCu = CultureInfo.CurrentCulture;
			var b = lrp.Slope;
			var sr = lrp.ResidualStdDev();
			var studentLawValue = ExtraMath.ConvertTo<TStorage>(
			 ProbabilityDistributions.InverseStudent(RoundOff.Error(1 - 0.025f), checked((byte)(lrp.Count - 2))));
			OutputLinearRegression2(numberFormat, ciCu, b, studentLawValue, checkBiases, lrp, sr);

			OutputYExtrapolation(lrp, numberFormat, ciCu, xForYExtrapolation, studentLawValue, sr, true);
			OutputYExtrapolation(lrp, numberFormat, ciCu, xForYExtrapolation, studentLawValue, sr, false);
			if (checkBiases) {
				OutputLine(numberFormat, ciCu, "x = {0,-5} total error: {1}\trelative bias: {2}",
				 xForYExtrapolation, lrp.TotalError(xForYExtrapolation), lrp.RelativeBias(xForYExtrapolation));
			}

			OutputXExtrapolation(lrp, numberFormat, ciCu, yForXExtrapolation, studentLawValue, b);
		}

		private static void OutputLinearRegression2<TRegression, TStorage>(string numberFormat, CultureInfo culture,
		TStorage b, TStorage studentLawValue, bool checkBiases, TRegression lrp, TStorage sr)
		where TRegression : struct, ILinearRegressionResult<TStorage>
		where TStorage : struct, IConvertible, IFormattable, IComparable<TStorage>, IEquatable<TStorage>, IComparable
		{
			Console.WriteLine(lrp.ToString(numberFormat, culture));
#pragma warning disable U2U1104
			Console.Write("r = {0}\t{1} = {2}", lrp.Correlation.ToString(numberFormat, culture),
			 Program.UnicodeTerminal != SupportLevel.None ? "R²" : "r^2", lrp.Determination().ToString(numberFormat, culture));
#pragma warning restore U2U1104
			if (checkBiases) {
				// ReSharper disable once InvokeAsExtensionMethod
				Console.Write("\trelative bias: {0:p}",
				 Arithmetic<TStorage>.SubtractScalars(b, ExtraMath.ConvertTo<TStorage>(1)));
			}
			OutputLine(numberFormat, culture,
			 "␤SCT: {0}\tSCreg: {1}\tSCres: {2}␤Std. dev.: residual {3}\tslope {4}\tintercept {5}",
			 lrp.TotalVariation(), lrp.ExplainedVariation(), lrp.UnexplainedVariation(),
			 sr, lrp.SlopeStdDev(), lrp.InterceptStdDev());
			OutputParameterMargin(numberFormat, culture, 'b', b, lrp.SlopeStdDev(), studentLawValue);
			OutputParameterMargin(numberFormat, culture, 'a', lrp.Intercept, lrp.SlopeStdDev(), studentLawValue);
		}

		private static void OutputParameterMargin<T>(string numberFormat, IFormatProvider culture, char argument,
		T number, T standardDeviation, T studentLawValue) where T : struct, IEquatable<T>, IFormattable, IComparable
		{
			var em = new ErrorMargin<T>(number, Arithmetic<T>.MultiplyScalars(studentLawValue, standardDeviation));
			Console.WriteLine("{0} = {1} => {2}", argument, em.ToString(numberFormat, culture),
			 em.Round().ToString(numberFormat, culture));
		}

		private static void OutputYExtrapolation<T>(ILinearRegressionResult<T> lrp, string numberFormat,
		IFormatProvider culture, T x, T studentLawValue, T sr, bool repeated)
		where T : struct, IConvertible, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			var em = new ErrorMargin<T>(lrp.InterpolateY(x),
			 Arithmetic<T>.MultiplyScalars(studentLawValue, sr, lrp.YExtrapolationConfidenceFactor(x, repeated)));
			var    blnPartialCode = Program.UnicodeTerminal != SupportLevel.None;
			string condition;
			if (repeated) {
				condition = "1";
			} else {
				condition = blnPartialCode ? "∞" :
				 InternationalizationExtension.FindNumberFormat(culture).PositiveInfinitySymbol.Replace("+", "");
			}
			Console.Write(blnPartialCode ?
			 "x = {0,-5} k = {1} ŷ  = {2} => " : "x = {0} k = {1,-8}  y^ = {2} => ",
			 x.ToString(numberFormat, culture), condition, em.ToString(numberFormat, culture));
			Console.WriteLine(em.Round().ToString(numberFormat, culture));
		}

		private static void OutputXExtrapolation<T>(ILinearRegressionResult<T> lrp, string numberFormat,
		IFormatProvider culture, T yc, T studentLawValue, T b)
		where T : struct, IConvertible, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			var k = lrp.Count - 1;
			var em = new ErrorMargin<T>(Arithmetic<T>.DivideScalars(Arithmetic<T>.SubtractScalars(yc, lrp.Intercept), b),
			 Arithmetic<T>.MultiplyScalars(studentLawValue, lrp.StdDevForYc(yc, k)));
			OutputLine(numberFormat, culture,
			 Program.UnicodeTerminal != SupportLevel.None ? "yc= {0,-5} k = {1} x₀ = {2} => {3}" : "yc= {0} k = {1,-8}\tx0 = {2} => {3}",
			 yc, k, em, em.Round());
		}

		/// <summary>
		/// Formats numbers using their IFormattable interface, then perform composite formatting, so formatting can
		/// be a customised without building a composite format string at run-time, the latter being a security risk.
		/// </summary>
		/// <param name="numberFormat">Formatting code applied to each number</param>
		/// <param name="culture">Formatting provider for both number and composite formatting</param>
		/// <param name="compositeFormat">
		/// Composite format string passed to String.Format, with a twist: the 'symbol for newline' character will
		/// be replaced with the platform-dependent new line sequence, after String.Format.
		/// </param>
		/// <param name="arguments">
		/// An array of IFormattable instances (of a least 4 items to see an IL size benefit) to be formatted
		/// according to numberFormat and culture, before being part of the final composite formatting.
		/// </param>
		/// <returns>A formatted string</returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// When arguments is null or empty, which is generally an error from the programmer.
		/// </exception>
		private static void OutputLine(string numberFormat, IFormatProvider culture, string compositeFormat,
		params IFormattable[] arguments)
		{
			if (arguments == null) {
				throw new ArgumentNullException(nameof(arguments));
			}
			var c = arguments.Length;
			if (c < 1) {
				throw new ArgumentNullException(nameof(arguments));
			}
			var objarArgs = new object[c];
			for (var i = 0; i < c; i++) {
				objarArgs[i] = arguments[i].ToString(numberFormat, culture);
			}
			Console.WriteLine(String.Format(culture, compositeFormat, objarArgs).Replace("␤", Environment.NewLine));
		}

		internal static void OutputRegressionModel<T>(RegressionModel<T> mathModel)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>
		{
			var r = mathModel.R;
			Console.Write("{0:g6}\tr={1,-9:g6}", mathModel, r);
			if (mathModel.Determination().CompareTo(ExtraMath.ConvertTo<T>(0.9998f)) > 0) {
				Console.WriteLine(" S/N={0:g6} dB", -10 * Math.Log10(1 - Math.Abs(Convert.ToDouble(r))));
			} else {
				Console.WriteLine();
			}
			var kind = mathModel.Model;
			if (kind == MathematicalModel.Exponential) {
				Console.WriteLine("{0:eg6}\tGrowth rate: {1:p2}", mathModel, mathModel.GrowthRate());
			} else if (kind == MathematicalModel.Logarithmic) {
				Console.WriteLine("{0:eg6}", mathModel);
			}
		}

		private static double RoundedInverse(string valueAsText)
		{
			var ciFrCa = new CultureInfo("fr-CA");
			return SignificantDigits.Round(1.0 / Double.Parse(valueAsText, ciFrCa),
			 SignificantDigits.Count(valueAsText, ciFrCa));
		}

		private static void OutputRegression<T>(string header,
		ILinearRegressionResult<T> linearized, RegressionModel<T> reformed)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>
		{
			Console.Write("{0,-15}: {1:g6}  r={2:g6}\t", header, linearized, linearized.Correlation);
			Console.WriteLine("{0:g6}  {1}={2:g6}",
			 reformed, Program.UnicodeTerminal != SupportLevel.None ? "R²" : "r^2", reformed.Determination());
		}

		private static T[] Take<T>(int howMany, params T[] from)
		{
			if (from == null) {
				return new T[0];
			} else {
				var c = Math.Min(howMany, from.Length);
				var objarOut = new T[c];
				for (var i = 0; i < c; i++) {
					objarOut[i] = from[i];
				}
				return objarOut;
			}
		}

		private static double SpecificActivity(float bcaConcentration, double bufferSpeed,
		double multiplier, IList<PointD> speedsByConcentration, params PointD[] absorbancesAtSeconds)
		{
			var cacb = Math.Round(bcaConcentration, 4);
			var lrr = LinearRegression.Compute(absorbancesAtSeconds);
			var venz = lrr.Slope - bufferSpeed;
			speedsByConcentration.Add(new PointD(cacb, venz));
			Console.Write("A@{0,-6} mg/mL : ", cacb);
			OutputRegressionModel(lrr.ChangeModel(MathematicalModel.Affine));
			return venz * multiplier / cacb;
		}

		private static double SpecificActivity(RegressionModel<double> rm, double x0, double x1, double multiplier)
		{
			var shapeBase = x1 - x0;
			return 2 * (rm.EvaluatePrimitive(x1) - rm.EvaluatePrimitive(x0) - rm.Evaluate(x0) * shapeBase) / (shapeBase * shapeBase) * multiplier;
		}

		private static double SpecificActivity(IList<PointD> speedsByConcentration, double multiplier)
		{
			int i;
			double totalArea = 0;
			// ReSharper disable once JoinDeclarationAndInitializer
			double shapeBase;
			var c = speedsByConcentration.Count;
			PointD pt0, pt1 = default(PointD);
			for (i = 0; i < c - 1; i++) {
				pt0 = speedsByConcentration[i];
				pt1 = speedsByConcentration[i + 1];
				totalArea += (pt1.X - pt0.X) * (pt1.Y + pt0.Y);
			}
			totalArea *= 0.5f;
			pt0 = speedsByConcentration[0];
			shapeBase = pt1.X - pt0.X;
			return 2 * (totalArea - pt0.Y * shapeBase) / (shapeBase * shapeBase) * multiplier;
		}
	}
}
