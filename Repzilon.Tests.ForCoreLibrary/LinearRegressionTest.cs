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
#if !NET20
using System.Linq;
#endif
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
			const int kBenchIterationsDecimal = 70000;
			const int kBenchIterationsDouble = 25 * kBenchIterationsDecimal;

			var dblTalpha0_025n4 = ProbabilityDistributions.InverseStudent(RoundOff.Error(1 - 0.025f), 6 - 2);
			Console.WriteLine("t{0} = {1}", 6 - 2, dblTalpha0_025n4);
			var ptarDouble = new PointD[] {
				new PointD(2, 2.1f), new PointD(4, 4.4f), new PointD(6, 6.5f), new PointD(8, 8.6f),
				new PointD(10, 10.8f), new PointD(12, 12.9f)
			};
			Program.OutputHeading("Double data type");
			Program.OutputSizeOf<PointD>();
			Program.OutputSizeOf<LinearRegressionResult>();
			Program.OutputSizeOf<ErrorMargin<double>>();
			OutputLinearRegression2(LinearRegression.Compute(ptarDouble),
			 dblTalpha0_025n4, "G", true, 8.25f, 3.4);
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
			OutputBenchResults(kBenchIterationsDouble, tsEnumerable, tsList);
			// x can also be 7 or 8, and y can also be 7.5

			var ptarDecimal = new PointM[] {
				new PointM(2, 2.1m), new PointM(4, 4.4m), new PointM(6, 6.5m), new PointM(8, 8.6m),
				new PointM(10, 10.8m), new PointM(12, 12.9m)
			};
			Program.OutputHeading("Decimal data type");
			Program.OutputSizeOf<PointM>();
			Program.OutputSizeOf<DecimalLinearRegressionResult>();
			Program.OutputSizeOf<ErrorMargin<decimal>>();
			var dlrp = LinearRegression.Compute(ptarDecimal);
			OutputLinearRegression2(dlrp, (decimal)dblTalpha0_025n4, "G18", true, 7, 7.5m);
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
			OutputBenchResults(kBenchIterationsDecimal, tsEnumerable, tsList);

			Program.OutputHeading("Revision");
			OutputLinearRegression2(LinearRegression.Compute(new PointM(0, 0.06m), new PointM(5, 1.25m),
			 new PointM(10, 2.38m), new PointM(15, 3.58m), new PointM(20, 4.61m)),
			 3.18245m, "G7", false, 12, 4.154m);

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
			OutputBenchResults(kBenchIterationsDouble, tsEnumerable, tsList);
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
			OutputBenchResults(kBenchIterationsDecimal, tsEnumerable, tsList);

			Program.OutputHeading("Biochemistry II ch. 1 pp. 22-23");
			OutputRegressionModel(RegressionModel.Compute(new PointD(1.0f, 31.25f), new PointD(0.4f, 18.18f),
			 new PointD(0.2f, 13.89f), new PointD(0.1f, 11.11f)));
			OutputRegressionModel(RegressionModel.Compute(new PointD(1.0f, 47.62f), new PointD(0.4f, 24.39f),
			 new PointD(0.2f, 16.95f), new PointD(0.1f, 12.99f)));
			OutputRegressionModel(RegressionModel.Compute(new PointD(1.0f, 47.62f), new PointD(0.4f, 26.32f),
			 new PointD(0.2f, 20f), new PointD(0.1f, 16.39f)));

			Program.OutputHeading("Biochemistry II ch. 1 exercise 3");
			var lrr0 = LinearRegression.Compute(
				new PointD(1000000, RoundedInverse("1,16")),
				new PointD(100000, RoundedInverse("8,46")),
				new PointD(10000, RoundedInverse("24,94")),
				new PointD(1000, RoundedInverse("27,94")),
				new PointD(100, RoundedInverse("29,95"))
			);
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			var vmax0 = 1.0 / lrr0.Intercept;
			var Km0 = lrr0.Slope * vmax0;
			Console.WriteLine("Vmax = {0:g4} nmol/min\tKm = {1:g4} mol/L", vmax0 / 60, Km0);

			Program.OutputHeading("Biochemistry II ch. 1 exercise 4");
			lrr0 = LinearRegression.Compute(
				new PointD(100, RoundedInverse("16,7")),
				new PointD(Math.Round(100 / 1.33, 1), RoundedInverse("20")),
				new PointD(50, RoundedInverse("25")),
				new PointD(40, RoundedInverse("27")),
				new PointD(20, RoundedInverse("35,7")),
				new PointD(10, RoundedInverse("41,7"))
			);
			var lrr1 = LinearRegression.Compute(
				new PointD(100, RoundedInverse("10")),
				new PointD(Math.Round(100 / 1.33, 1), RoundedInverse("12,5")),
				new PointD(50, RoundedInverse("16,7")),
				new PointD(40, RoundedInverse("19,2")),
				new PointD(20, RoundedInverse("27,8")),
				new PointD(10, RoundedInverse("35,7"))
			);
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			OutputRegressionModel(lrr1.ChangeModel(MathematicalModel.Affine));
			vmax0 = 1.0 / lrr0.Intercept;
			Km0 = lrr0.Slope * vmax0;
			Console.WriteLine("Vmax  = {0,-4:g3} µmol/min*L  Km  = {1:g3} mol/L", vmax0, Km0);
			var vmax1 = 1.0 / lrr1.Intercept;
			var Km1 = lrr1.Slope * vmax1;
			Console.WriteLine("Vmax' = {0,-4:g3} µmol/min*L  Km' = {1:g3} mol/L", vmax1, Km1);
			Console.WriteLine("Ki = {0:g3} mol/L", 0.02 / ((Km1 / Km0) - 1));

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
				new PointD(4 / 1.5, 4),
				new PointD(6 / 2.5, 6),
				new PointD(7.5 / 3.5, 7.5),
				new PointD(10.4 / 6, 10.4),
				new PointD(14 / 12.0, 14)
			);
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			vmax0 = SignificantDigits.Round(lrr0.Intercept, 2, RoundingMode.ToEven);
			Km0 = SignificantDigits.Round(-lrr0.Slope, 2, RoundingMode.ToEven);
			Console.WriteLine("Vmax  = {0,-4} mUI  Km  = {1} mmol/L", vmax0, Km0);

			Program.OutputHeading("Biochemistry II laboratory 3");
			lrr0 = LinearRegression.Compute(
				new PointD(10, 0.174f),
				new PointD(20, 0.285f),
				new PointD(30, 0.387f),
				new PointD(40, 0.511f),
				new PointD(51, 0.659f)
			);
			Console.Write("Absorbance: ");
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			lrr1 = LinearRegression.Compute(
				 new PointD(1.0 / 0.00150, 1.0 / 0.0071),
				 new PointD(1.0 / 0.00090, 1.0 / 0.0044),
				 new PointD(1.0 / 0.00076, 1.0 / 0.0038),
				 new PointD(1.0 / 0.00045, 1.0 / 0.0026),
				 new PointD(1.0 / 0.00030, 1.0 / 0.0018)
			);
			Console.Write("Reaction  : ");
			OutputRegressionModel(lrr1.ChangeModel(MathematicalModel.Affine));
			vmax0 = 1.0 / lrr1.Intercept;
			Km0 = vmax0 * lrr1.Slope;
			vmax1 = vmax0 * 60 / lrr0.Slope;
			Console.WriteLine("Vmax  = {1} A405/s  Km  = {2} mol/L{0}Vmax  = {3} µmol/min*L",
			 Environment.NewLine,
			 SignificantDigits.Round(vmax0, 5, RoundingMode.ToEven),
			 SignificantDigits.Round(Km0, 4, RoundingMode.ToEven),
			 SignificantDigits.Round(vmax1, 3, RoundingMode.ToEven));

			Program.OutputHeading("Biochemistry II laboratory 4");
			lrr0 = LinearRegression.Compute(
				new PointD(0.0100f, 0.142f),
				new PointD(0.020f, 0.251f),
				new PointD(0.030f, 0.390f),
				new PointD(0.040f, 0.520f)
			);
			Console.Write("Calibration    : ");
			OutputRegressionModel(lrr0.ChangeModel(MathematicalModel.Affine));
			Km0 = lrr0.SlopeStdDev() * ProbabilityDistributions.InverseStudent(RoundOff.Error(1 - 0.025f), (byte)(lrr0.Count - 2));
			Km0 = SignificantDigits.Round(Km0, 1, RoundingMode.Ceiling);
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
			Km1 = SpecificActivity(0.0061, vmax0, vmax1, lstSpeeds, new PointD(0, 0.02640f), new PointD(9.77f, 0.03220f),
			 new PointD(19.78f, 0.03840f), new PointD(29.79f, 0.04510f), new PointD(39.80f, 0.05100f),
			 new PointD(49.82f, 0.05740f), new PointD(59.83f, 0.06410f), new PointD(69.88f, 0.06920f),
			 new PointD(79.90f, 0.07660f), new PointD(89.91f, 0.08170f), new PointD(99.92f, 0.08770f),
			 new PointD(109.93f, 0.09430f), new PointD(119.94f, 0.1013f));
			Km1 += SpecificActivity(0.0122, vmax0, vmax1, lstSpeeds, new PointD(0, 0.03500f), new PointD(9.77f, 0.04580f),
			 new PointD(19.77f, 0.05740f), new PointD(29.79f, 0.06870f), new PointD(39.80f, 0.08010f),
			 new PointD(49.82f, 0.09110f), new PointD(59.83f, 0.1034f), new PointD(69.88f, 0.1145f),
			 new PointD(79.90f, 0.1271f), new PointD(89.91f, 0.1380f), new PointD(99.92f, 0.1499f),
			 new PointD(109.93f, 0.1623f), new PointD(119.94f, 0.1723f));
			Km1 += SpecificActivity(0.018, vmax0, vmax1, lstSpeeds, new PointD(0, 0.05280f), new PointD(9.77f, 0.07520f),
			 new PointD(19.78f, 0.09810f), new PointD(29.79f, 0.1197f), new PointD(39.80f, 0.1425f),
			 new PointD(49.82f, 0.1641f), new PointD(59.83f, 0.1883f), new PointD(69.89f, 0.2096f),
			 new PointD(79.90f, 0.2306f), new PointD(89.91f, 0.2551f), new PointD(99.92f, 0.2766f),
			 new PointD(109.93f, 0.2996f), new PointD(119.95f, 0.3242f));
			Km1 += SpecificActivity(0.024, vmax0, vmax1, lstSpeeds, new PointD(0, 0.04950f), new PointD(9.77f, 0.07090f),
			 new PointD(19.78f, 0.09200f), new PointD(29.79f, 0.1118f), new PointD(39.80f, 0.1358f),
			 new PointD(49.82f, 0.1563f), new PointD(59.83f, 0.1797f), new PointD(69.88f, 0.2015f),
			 new PointD(79.90f, 0.2233f), new PointD(89.91f, 0.2462f), new PointD(99.92f, 0.2699f),
			 new PointD(109.93f, 0.2909f), new PointD(119.94f, 0.3136f));
			Km1 += SpecificActivity(0.031, vmax0, vmax1, lstSpeeds, new PointD(0, 0.07860f), new PointD(9.77f, 0.1106f),
			 new PointD(19.77f, 0.1451f), new PointD(29.78f, 0.1780f), new PointD(39.80f, 0.2115f),
			 new PointD(49.82f, 0.2445f), new PointD(59.83f, 0.2787f), new PointD(69.88f, 0.3117f),
			 new PointD(79.90f, 0.3452f), new PointD(89.91f, 0.3800f), new PointD(99.92f, 0.4161f),
			 new PointD(109.93f, 0.4514f), new PointD(119.94f, 0.4884f));
			Console.Write("Speed [dA/(dt*dC)]: ");
			lrr1 = LinearRegression.Compute(lstSpeeds);
			OutputRegressionModel(lrr1.ChangeModel(MathematicalModel.Affine));
			Console.Write("Speed [dA/(dt*dC)]: ");
			var rm = RegressionModel.Compute(lstSpeeds);
			OutputRegressionModel(rm);
			Km1 *= 0.2;
			Console.WriteLine("Specific activity average={0,-6:f3} µmol PNPA/mg BCA*min", Km1);
			Console.WriteLine("Specific activity linear ={0,-6:f4} µmol PNPA/mg BCA*min", lrr1.Slope * vmax1);
			Km0 = 0;
			for (j = 0; j < lstSpeeds.Count; j++) {
				Km0 += rm.Evaluate(lstSpeeds[j].X) / lstSpeeds[j].X;
			}
			Km0 *= 0.2 * vmax1;
			Console.WriteLine("Specific activity x_power={0,-6:f3} µmol PNPA/mg BCA*min", Km0);
			Km0 = (Km1 + (lrr1.Slope * vmax1)) * 0.5 * 30;
			Console.WriteLine("Molecular activity={0:f3} µmol PNPA/µmol BCA*min", Km0);
			Console.WriteLine("{0:g3} UI", 1.0 / Km0);

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

			Program.OutputHeading("Instrumental analysis II Mass spectroscopy mean travel");
			ptarDouble = new PointD[] {
				new PointD(101325, 0.000006f), new PointD(130, 0.0045f),
				new PointD(0.13f, 4.5f), new PointD(0.013f, 45f),
				new PointD(0.0013f, 450f), new PointD(0.00013f, 4500f),
				new PointD(1.3e-5f, 45000f), new PointD(1.3e-7, 4500000)
			};
			Console.Write("  ");
			OutputRegressionModel(RegressionModel.Compute(ptarDouble));
			PointD ptd;
			byte   i;
			for (i = 0; i < ptarDouble.Length; i++)  {
				ptd = ptarDouble[i];
				ptarDouble[i] = new PointD(ptd.X, 1.0 / ptd.Y);
			}
			Console.Write("1/");
			rm = RegressionModel.Compute(ptarDouble);
			OutputRegressionModel(rm);
			var b = 1.0 / rm.B;
			for (i = 0; i < ptarDouble.Length; i++) {
				ptd = ptarDouble[i];
				ptarDouble[i] = new PointD(b / ptd.X, 1.0 / ptd.Y);
			}
			rm = RegressionModel.Compute(ptarDouble);
			Console.WriteLine("  y = {1:g6} + {0:g6} * x^-1 r={2,-8:g6} S/N={3:g6} dB", b * rm.B, rm.A,
			 rm.R, -10 * Math.Log10(1 - rm.R));

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

		private static void OutputBenchResults(int iterations, TimeSpan withEnumerable, TimeSpan withList)
		{
			Console.WriteLine("{0,9:n0} iterations in {1:f3} s as IEnumerable<PointD>, {2:f3} s as IList<PointD>",
			 iterations, withEnumerable.TotalSeconds, withList.TotalSeconds);
		}

#if !NET20
		private static void OutputAgaroseRetention(string heading, params short[] fragmentLengths)
		{
			var agars = MolecularBiology.AgaroseConcentration(fragmentLengths);
			Console.Write(heading);
			if (agars.Length != 1) {
				Console.Write(Environment.NewLine);
			}
			for (int i = 0; i < agars.Length; i++) {
				if (!String.IsNullOrEmpty(heading)) {
					Console.Write('\t');
				}
				Console.WriteLine(agars[i]);
			}
		}
#endif

		private static void OutputLinearRegression2<TRegression, TStorage>(TRegression lrp, TStorage studentLawValue,
		string numberFormat, bool checkBiases, TStorage? xForYExtrapolation, TStorage? yForXExtrapolation)
		where TRegression : struct, ILinearRegressionResult<TStorage>
		where TStorage : struct, IConvertible, IFormattable, IComparable<TStorage>, IEquatable<TStorage>, IComparable
		{
			var ciCu = CultureInfo.CurrentCulture;
			Console.WriteLine(lrp.ToString(numberFormat, ciCu));
			var b = lrp.Slope;
			var sr = lrp.ResidualStdDev();

			Console.Write("r = {0}\tr^2 = {1}", lrp.Correlation.ToString(numberFormat, ciCu), lrp.Determination().ToString(numberFormat, ciCu));
			Console.Write(Environment.NewLine);
			if (checkBiases) {
				// ReSharper disable once InvokeAsExtensionMethod
				Console.WriteLine("\trelative bias: {0:p}",
				 Arithmetic<TStorage>.SubtractScalars(b, ExtraMath.ConvertTo<TStorage>(1)));
			} else {
				Console.Write(Environment.NewLine);
			}
			Console.WriteLine("SCT: {0}\tSCreg: {1}\tSCres: {2}", lrp.TotalVariation().ToString(numberFormat, ciCu),
			 lrp.ExplainedVariation().ToString(numberFormat, ciCu),
			 lrp.UnexplainedVariation().ToString(numberFormat, ciCu));
			Console.WriteLine("Std. dev.: residual {0}\tslope {1}\tintercept {2}", sr.ToString(numberFormat, ciCu),
			 lrp.SlopeStdDev().ToString(numberFormat, ciCu), lrp.InterceptStdDev().ToString(numberFormat, ciCu));
			Console.WriteLine("b = {0}", new ErrorMargin<TStorage>(b,
			 Arithmetic<TStorage>.MultiplyScalars(studentLawValue, lrp.SlopeStdDev())).ToString(numberFormat, ciCu));
			Console.WriteLine("a = {0}", new ErrorMargin<TStorage>(lrp.Intercept,
			 Arithmetic<TStorage>.MultiplyScalars(studentLawValue, lrp.InterceptStdDev())).ToString(numberFormat, ciCu));
			if (xForYExtrapolation.HasValue) {
				var x = xForYExtrapolation.Value;
				OutputYExtrapolation(lrp, studentLawValue, numberFormat, ciCu, x, sr, true);
				OutputYExtrapolation(lrp, studentLawValue, numberFormat, ciCu, x, sr, false);
				if (checkBiases) {
					Console.WriteLine("x = {0}\t\ttotal error: {1}\trelative bias: {2}",
					 x.ToString(numberFormat, ciCu),
					 lrp.TotalError(x).ToString(numberFormat, ciCu),
					 lrp.RelativeBias(x).ToString(numberFormat, ciCu));
				}
			}
			if (yForXExtrapolation.HasValue) {
				var yc = yForXExtrapolation.Value;
				OutputXExtrapolation(lrp, studentLawValue, numberFormat, ciCu, yc, 5, b);
			}
		}

		private static void OutputYExtrapolation<T>(ILinearRegressionResult<T> lrp, T studentLawValue,
		string numberFormat, IFormatProvider culture, T x, T sr, bool repeated)
		where T : struct, IConvertible, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			Console.WriteLine("x = {0} k = {1}\ty^ = {2}",
			 x.ToString(numberFormat, culture),
			 repeated ? "Infinity" : "1\t",
			 new ErrorMargin<T>(lrp.InterpolateY(x),
			 Arithmetic<T>.MultiplyScalars(studentLawValue, sr, lrp.YExtrapolationConfidenceFactor(x, repeated))).ToString(numberFormat, culture));
		}

		private static void OutputXExtrapolation<T>(ILinearRegressionResult<T> lrp, T studentLawValue,
		string numberFormat, IFormatProvider culture, T yc, int k, T b)
		where T : struct, IConvertible, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			Console.WriteLine("yc= {0} k = {1}\t\tx0 = {2}",
			 yc.ToString(numberFormat, culture),
			 k.ToString(numberFormat, culture),
			 new ErrorMargin<T>(Divide(Arithmetic<T>.SubtractScalars(yc, lrp.Intercept), b), Arithmetic<T>.MultiplyScalars(studentLawValue, lrp.StdDevForYc(yc, k))).ToString(numberFormat, culture));
		}

		private static T Divide<T>(T dividend, T divisor) where T : struct, IConvertible
		{
			var tc = dividend.GetTypeCode();
			if (tc == TypeCode.Double) {
				return ExtraMath.ConvertTo<T>(Convert.ToDouble(dividend) / Convert.ToDouble(divisor));
			} else if (tc == TypeCode.Decimal) {
				return ExtraMath.ConvertTo<T>(Decimal.Divide(Convert.ToDecimal(dividend), Convert.ToDecimal(divisor)));
			} else {
				throw new NotSupportedException();
			}
		}

		internal static void OutputRegressionModel<T>(RegressionModel<T> mathModel)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>
		{
			var r = mathModel.R;
			Console.Write("{0:g6}\tr={1,-9:g6}", mathModel, r);
			if (mathModel.Determination().CompareTo(ExtraMath.ConvertTo<T>(0.9998f)) > 0) {
				Console.WriteLine(" S/N={0:g6} dB", -10 * Math.Log10(1 - Math.Abs(Convert.ToDouble(r))));
			} else {
				Console.Write(Environment.NewLine);
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
			 SignificantDigits.Count(valueAsText, ciFrCa), RoundingMode.ToEven);
		}

		private static double SpecificActivity(double bcaConcentration, double bufferSpeed,
		double multiplier, IList<PointD> speedsByConcentration, params PointD[] absorbancesAtSeconds)
		{
			var lrr = LinearRegression.Compute(absorbancesAtSeconds);
			var venz = lrr.Slope - bufferSpeed;
			speedsByConcentration.Add(new PointD(bcaConcentration, venz));
			Console.Write("A@{0,-6} mg/mL : ", bcaConcentration);
			OutputRegressionModel(lrr.ChangeModel(MathematicalModel.Affine));
			return venz * multiplier / bcaConcentration;
		}
	}
}
