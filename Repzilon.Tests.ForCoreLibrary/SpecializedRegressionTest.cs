//
//  LogisticModelTest.cs
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
// ReSharper disable InvokeAsExtensionMethod

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class SpecializedRegressionTest
	{
		internal static void Run(string[] args)
		{
			var blnPartialCode = Program.UnicodeTerminal != SupportLevel.None;

			Program.OutputHeading("Biofermentation semaine 3 exercice");
			Program.OutputSizeOf<DecimalLogisticRegressionResult>();
			var ptmarBiofermW3 = new PointM[] {
				new PointM(0, 1.5m), new PointM(5, 2), new PointM(9, 3.5m), new PointM(13, 6.2m), new PointM(16, 8.2m),
				new PointM(20, 9.4m), new PointM(24, 9.8m), new PointM(28, 9.9m)
			};
			LogisticModelSix(blnPartialCode, ptmarBiofermW3);
			var lorBest = LogisticRegression.Compute(LogisticRegressionOptions.Linearization, ptmarBiofermW3);
			var x0      = ptmarBiofermW3[0].Y;
			TryMaxProductivity(lorBest, x0, 16, blnPartialCode);
			SearchMaxProductivity(lorBest, x0, Math.Round(lorBest.Location, 1), ptmarBiofermW3[5].X, 0.1m,
			 blnPartialCode);
			SearchMaxProductivity(lorBest, x0, 16.6m, 16.9m, 0.01m, blnPartialCode);
			SearchMaxProductivity(lorBest, x0, 16.76m, 16.79m, 0.001m, blnPartialCode);
			var kvp = LogisticRegression.MaxProductivity(lorBest, x0);
			OutputMaxProductivity(kvp.Value, kvp.Key, blnPartialCode);

			Program.OutputHeading("Biofermentation laboratoire 5 saturation en oxygène");
			LogisticModelSix(blnPartialCode, new PointM(0, 105.1m), new PointM(0.5m, 103.7m),
			 new PointM(1, 98.4m), new PointM(1.5m, 85.5m), new PointM(2, 51.6m), new PointM(2.5m, 1),
			 new PointM(3, 0.8m), new PointM(3.5m, 0.3m));

			// https://www.youtube.com/watch?v=m-k84cCves8
			Program.OutputHeading("Multiple Regression: Two Independent Variables Case - Part 1");
			Program.OutputSizeOf<TwoXPoint<float>>();
			var coeffs =  MultipleLinearRegression.Compute(X1X2Y(3,8,-3.7f),
			 X1X2Y(4,5,3.5f), X1X2Y(5,7,2.5f), X1X2Y(6,3,11.5f), X1X2Y(2,1,5.7f));
			Console.WriteLine(blnPartialCode ? "y = {0} + {1}•x₁ + {2}•x₂" : "y = {0} + {1}*x1 + {2}*x2", coeffs[0], coeffs[1], coeffs[2]);

			Program.OutputHeading("Analyse instrumentale 2 laboratoire EC");
			CapillaryElectrophoresisTimeFunction(blnPartialCode, "butylparabène",
			 5.881f, 4.721f, 3.922f, 4.989f, 3.952f, 3.277f, 4.284f, 3.401f, 2.815f);
			CapillaryElectrophoresisTimeFunction(blnPartialCode, "éthylparabène",
			 6.078f, 4.881f, 4.055f, 5.157f, 4.085f, 3.388f, 4.428f, 3.517f, 2.911f);
			CapillaryElectrophoresisTimeFunction(blnPartialCode, "méthylparabène",
			 6.258f, 5.025f, 4.176f, 5.306f, 4.205f, 3.487f, 4.558f, 3.620f, 2.996f);
			Console.WriteLine(blnPartialCode ? "tᵣ[s] T°[K] ΔE[V]" : "tr[s] T[K] E[V]");
		}

		private static void LogisticModelSix(bool useSymbols, params PointM[] points)
		{
			var karValidCombos = new byte[] { 1, 2, 3, 5, 6, 7 };
			var lorarAll       = new DecimalLogisticRegressionResult[6];
			int i;
			for (i = 0; i < 6; i++) {
				lorarAll[i] = LogisticRegression.Compute((LogisticRegressionOptions)karValidCombos[i], points);
			}

			Console.WriteLine("X\tY\ty m\ty l\ty a\ty ms\ty ls\ty as");
			int j;
			for (i = 0; i < points.Length; i++) {
				Console.Write("{0}\t{1,5:f1}", points[i].X, points[i].Y);
				for (j = 0; j < 6; j++) {
					Console.Write("\t{0,6:f2}", lorarAll[j].InterpolateY(points[i].X));
				}
				Console.WriteLine();
			}

			var strFormat = useSymbols ? "{0,-30}  r={1}\tΔ²={2} u²" : "{0,-30}  r={1}\tA={2} u^2";
			for (i = 0; i < 6; i++) {
				Console.WriteLine(strFormat, (LogisticRegressionOptions)karValidCombos[i],
				 lorarAll[i].Correlation, LogisticRegression.AreaBetween(lorarAll[i], points));
				Console.WriteLine(lorarAll[i]);
			}
		}

		private static void SearchMaxProductivity(DecimalLogisticRegressionResult model, decimal x0,
		decimal from, decimal to, decimal step, bool useSymbols)
		{
			var pxMax    = Decimal.Zero;
			var ptmPxmax = new PointM(0, 0);
			for (var t = from; t <= to; t += step) {
				var xm = model.InterpolateY(t);
				var px = (xm - x0) / t;
				if (px > pxMax) {
					ptmPxmax = new PointM(t, xm);
					pxMax    = px;
				}
			}
			OutputMaxProductivity(pxMax, ptmPxmax, useSymbols);
		}

		private static void TryMaxProductivity(DecimalLogisticRegressionResult model, decimal x0, decimal t,
		bool useSymbols)
		{
			var xm = model.InterpolateY(t);
			OutputMaxProductivity((xm - x0) / t, new PointM(t, xm), useSymbols);
		}

		private static void OutputMaxProductivity(decimal pxMax, PointM coords, bool useSymbols)
		{
			Console.WriteLine(useSymbols ? "Pₓ ₘₐₓ = {0:f6} g/(L•h) à {1} h" : "Px max = {0:f6} g/(L*h) à {1} h",
			 pxMax, coords.X);
		}

		private static TwoXPoint<T> X1X2Y<T>(byte x1, byte x2, T y)
		where T: struct, IFormattable, IEquatable<T>
		{
			return new TwoXPoint<T>(ExtraMath.ConvertTo<T>(x1), ExtraMath.ConvertTo<T>(x2), ExtraMath.ConvertTo<T>(y));
		}

		private static TwoXPoint<float>[] MakeCapillaryElectrophoresisPoints(params float[] retentionTimes)
		{
			var kTemperatures = new byte[] { 15, 25, 35 };
			var kKiloVoltages = new byte[] { 20, 25, 30 };
			var c             = retentionTimes.Length;
			var txparOut      = new TwoXPoint<float>[c];
			for (var i = 0; i < c; i++) {
				txparOut[i] = new TwoXPoint<float>(kTemperatures[i / 3] + 273.15f, kKiloVoltages[i % 3] * 1000, retentionTimes[i] * 60);
			}
			return txparOut;
		}

		private static void CapillaryElectrophoresisTimeFunction(bool useSymbols, string substance, params float[] retentionTimes)
		{
			var coeffs = MultipleLinearRegression.Compute(MakeCapillaryElectrophoresisPoints(retentionTimes));
			Console.Write(substance);
			Console.WriteLine(useSymbols ? "\ttᵣ = {0:g4} + {1:g4}•T° + {2:g4}•ΔE" : "\ttr = {0:g4} + {1:g4}*T + {2:g4}*E",
			 coeffs[0], coeffs[1], coeffs[2]);
		}
	}
}
