//
//  InstrumentalAnalysisTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using Repzilon.Libraries.Core;
using Repzilon.Libraries.Core.Regression;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class InstrumentalAnalysisTest
	{
		internal static void Run(string[] args)
		{
			Program.OutputHeading("Multiple Headspace Extraction laboratory 5");
			var mhe = new MultipleHeadspaceExtraction();
			mhe.AddLevel(6062.5 * 49.89 / 1000000, 1696.2f, 1062.5f, 759.7f, 551.8f);
			mhe.AddLevel(12125 * 49.89 / 1000000, 3548.8f, 2268.4f, 1585.1f, 1151.0f);
			mhe.AddLevel(24250 * 49.89 / 1000000, 8261.8f, 4990.1f, 3471.2f, 2484.8f);
			mhe.AddLevel(48500 * 49.89 / 1000000, 16737.4f, 10249.7f, 7044.3f, 5064.2f);
			mhe.AddLevel(97000 * 49.89 / 1000000, 31959.7f, 20252.2f, 14009.6f, 10081.2f);
			mhe.Calibrate();
			Console.WriteLine("Calibration  : {0}", mhe);
			var dblVolume = mhe.InterpolateVolume(13403.6f, 8522.6f, 6027.7f, 4435.8f);
			var dblConcentration = dblVolume * (1000000 / 49.89);
			Console.WriteLine("Control Cegep: V = {0:f2}µL C = {1:f0}µL/L Δ = {2:p1}",
			 dblVolume, dblConcentration, (dblConcentration - 48500) * (1 / 48500.0));
			dblVolume = mhe.InterpolateVolume(14099.1f, 19096.6f, 12900.0f, 9222.6f);
			Console.WriteLine("Zeste {0:f1}mg : V = {1:f2}µL C = {2:f2}% m/m", 99f, dblVolume,
			 dblVolume * (0.1 * 0.8411 / 0.0990));
			dblVolume = mhe.InterpolateVolume(12788.7f, 14791.9f, 10267.6f, 7333.2f);
			Console.WriteLine("Zeste {0:f1}mg : V = {1:f2}µL C = {2:f2}% m/m", 99.5f, dblVolume,
			 dblVolume * (0.1 * 0.8411 / 0.0995));

			Program.OutputHeading("Multiple Headspace Extraction chapter 1");
			var strNewLine = Environment.NewLine;
			Console.WriteLine("Exercise 2a: Aₜ = {1:n0}pA•s{0}Exercise 2b: Aₜ = {2:n0}pA•s", strNewLine,
			 MultipleHeadspaceExtraction.TotalArea(74608, 47099, 30946, 20131),
			 MultipleHeadspaceExtraction.TotalArea(56478, 47099, 30946, 20131));
			var karStandard = new float[] { 55912, 37674, 26377, 18920 };
			var karControl = new float[] { 59175, 40349, 28623, 20541 };
			const double kStandardVolume = 48500 * 50 * 0.000001;
			const double kControlVolume = 55000 * 50 * 0.000001;
			var dblArea = MultipleHeadspaceExtraction.TotalArea(karStandard);
			var dblAreaC = MultipleHeadspaceExtraction.TotalArea(karControl);
			Console.WriteLine("Exercise 3a: Standard Aₜ = {1:n0}pA•s{0}             Control  Aₜ = {2:n0}pA•s",
			 strNewLine, dblArea, dblAreaC);
			var dblAreaZ = MultipleHeadspaceExtraction.TotalArea(72714, 63614, 46056, 32198);
			Console.WriteLine("             Zeste    Aₜ = {0:n0}pA•s", dblAreaZ);
			Console.WriteLine("Exercise 3b: Standard V = {1,-5}µL{0}             Control  V = {2,-5}µL",
			 strNewLine, kStandardVolume, kControlVolume);
			mhe = new MultipleHeadspaceExtraction();
			mhe.AddLevel(kStandardVolume, karStandard);
			try {
				dblVolume = mhe.InterpolateVolume(karControl);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
				dblVolume = kStandardVolume * dblAreaC / dblArea;
			}
			Console.WriteLine("Exercise 3c: Control  V = {0,-5:g4}µL Δ = {1:p1}", dblVolume,
			 (dblVolume - kControlVolume) * (1.0 / kControlVolume));
			dblVolume = kStandardVolume * dblAreaZ / dblArea;
			Console.WriteLine("Exercise 3d: Zeste    V = {0,-5:g4}µL C = {1:g3}% m/m", dblVolume,
			 dblVolume * ((1.0 / 0.1260) * 0.001 * 0.8402 * 100));

			Program.OutputHeading("Theoretical plates and resolution between peaks in laboratory 9B");
			OutputGasChromatographyPeakMetrics(3.037f, 5.432f, 0.0368f, 0.0588f, "Initial");
			OutputGasChromatographyPeakMetrics(1.593f, 1.764f, 0.0234f, 0.0263f, "Optimised");

			Program.OutputHeading("Three Points Drop Line chapter 8");
			Output3PointsDropLine(InstrumentalAnalysis.ThreePointsDropLine(1.2395f, 1.2870f, 257, 303),
			 1.5030f, 273, (1031.4 / 50) / 0.99476, "Example p.19");
			Output3PointsDropLine(InstrumentalAnalysis.ThreePointsDropLine(1f, 1.33f, 293, 406),
			 2.27f, 350, 27.3 / 1.25, "Exercise 11");
		}

		private static void OutputGasChromatographyPeakMetrics(float retentionTime1, float retentionTime2,
		float width1, float width2, /*PeakWidthFrame reference,*/ string conditionName)
		{
			Console.Write("{2,-9}: tᵣ₁ = {0}min tᵣ₂ = {1}min ", retentionTime1, retentionTime2, conditionName);
			Console.WriteLine("N₁ = {0:n0} N₂ = {1:n0} R = {2,5:g4}",
			 InstrumentalAnalysis.TheoricalPlates(retentionTime1, width1, PeakWidthFrame.Delta),
			 InstrumentalAnalysis.TheoricalPlates(retentionTime2, width2, PeakWidthFrame.Delta),
			 InstrumentalAnalysis.ResolutionBetweenPeaks(retentionTime1, retentionTime2, width1, width2, PeakWidthFrame.Delta));
		}

		private static void Output3PointsDropLine(RegressionModel<double> dropLine, float measuredAbsorbance,
		short optimalWaveLength, double concentrationMultiplier, string conditionName)
		{
			var dblAfinal = measuredAbsorbance - dropLine.Evaluate(optimalWaveLength);
			Console.WriteLine("{1,-12}: {0}", dropLine, conditionName);
			Console.WriteLine("              A = {0,-5:g3} C = {1:g3}mg/L", dblAfinal, dblAfinal * concentrationMultiplier);
		}
	}
}
