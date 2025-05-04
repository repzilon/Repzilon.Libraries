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
			var dblTalpha0_025n4 = ProbabilityDistributions.InverseStudent(RoundOff.Error(1 - 0.025f), 6 - 2);
			Console.WriteLine("t{0} = {1}", 6 - 2, dblTalpha0_025n4);

			var lrp = LinearRegression.Compute(
				new PointD(2, 2.1f),
				new PointD(4, 4.4f),
				new PointD(6, 6.5f),
				new PointD(8, 8.6f),
				new PointD(10, 10.8f),
				new PointD(12, 12.9f)
			);
			Program.OutputHeading("Double data type");
			Program.OutputSizeOf<PointD>();
			Program.OutputSizeOf<LinearRegressionResult>();
			Program.OutputSizeOf<ErrorMargin<double>>();
			OutputLinearRegression2(lrp, dblTalpha0_025n4, "G", true, 8.25f, 3.4);
			// x can also be 7 or 8, and y can also be 7.5

			var dlrp = LinearRegression.Compute(
				new PointM(2, 2.1m),
				new PointM(4, 4.4m),
				new PointM(6, 6.5m),
				new PointM(8, 8.6m),
				new PointM(10, 10.8m),
				new PointM(12, 12.9m)
			);
			Program.OutputHeading("Decimal data type");
			Program.OutputSizeOf<PointM>();
			Program.OutputSizeOf<DecimalLinearRegressionResult>();
			Program.OutputSizeOf<ErrorMargin<decimal>>();
			OutputLinearRegression2(dlrp, (decimal)dblTalpha0_025n4, "G18", true, 7, 7.5m);
			Console.WriteLine("a - 0.02 = {0}", dlrp.Intercept - 0.02m);

			Program.OutputHeading("Revision");
			var lrrRev5 = LinearRegression.Compute(
				new PointM(0, 0.06m),
				new PointM(5, 1.25m),
				new PointM(10, 2.38m),
				new PointM(15, 3.58m),
				new PointM(20, 4.61m)
			);
			OutputLinearRegression2(lrrRev5, 3.18245m, "G7", false, 12, 4.154m);

			Program.OutputHeading("Math I Example 38");
			var lrrM1Ex38 = LinearRegression.Compute(
				PointD.LogLog(100.0, 0.240),
				PointD.LogLog(150.0, 0.295),
				PointD.LogLog(250.0, 0.380),
				PointD.LogLog(300.0, 0.415),
				PointD.LogLog(400.0, 0.480),
				PointD.LogLog(550.0, 0.560)
			);
			var rmdMEx38 = lrrM1Ex38.ChangeModel(MathematicalModel.LogLog);
			Program.OutputSizeOf<RegressionModel<double>>();
			OutputRegressionModel(rmdMEx38);

			Program.OutputHeading("Math I Exercise");
			var lrrM1Exer = LinearRegression.Compute(
				PointD.SemiLogY(8.0, 9858),
				PointD.SemiLogY(14.0, 9416),
				PointD.SemiLogY(18.0, 7234),
				PointD.SemiLogY(24.0, 5426),
				PointD.SemiLogY(37.5, 2789),
				PointD.SemiLogY(41.0, 2251),
				PointD.SemiLogY(71.0, 564)
			);
			var rmdM1Exer = lrrM1Exer.ChangeModel(MathematicalModel.Exponential);
			OutputRegressionModel(rmdM1Exer);

			Program.OutputHeading("Biochemistry II ch. 1 pp. 15-16");
			var lrrBC2Ch1p15 = LinearRegression.Compute(
				PointD.LogLog(12.5, 0.037),
				PointD.LogLog(20, 0.050),
				PointD.LogLog(25, 0.055),
				PointD.LogLog(50, 0.073),
				PointD.LogLog(100, 0.091)
			);
			var rmdBC2Ch1p15 = lrrBC2Ch1p15.ChangeModel(MathematicalModel.Power);
			OutputRegressionModel(rmdBC2Ch1p15);

			var rmdBC2Ch1p16 = RegressionModel.Compute(
				new PointD(12.5f, 0.037f),
				new PointD(20, 0.050f),
				new PointD(25, 0.055f),
				new PointD(50, 0.073f),
				new PointD(100, 0.091f)
			);
			OutputRegressionModel(rmdBC2Ch1p16);

			Program.OutputHeading("Biochemistry II ch. 1 pp. 22-23");
			var rmdBC2Ch1p22V0 = RegressionModel.Compute(
				new PointD(1.0f, 31.25f),
				new PointD(0.4f, 18.18f),
				new PointD(0.2f, 13.89f),
				new PointD(0.1f, 11.11f)
			);
			OutputRegressionModel(rmdBC2Ch1p22V0);
			var rmdBC2Ch1p22VI = RegressionModel.Compute(
				new PointD(1.0f, 47.62f),
				new PointD(0.4f, 24.39f),
				new PointD(0.2f, 16.95f),
				new PointD(0.1f, 12.99f)
			);
			OutputRegressionModel(rmdBC2Ch1p22VI);
			var rmdBC2Ch1p22VIp = RegressionModel.Compute(
				new PointD(1.0f, 47.62f),
				new PointD(0.4f, 26.32f),
				new PointD(0.2f, 20f),
				new PointD(0.1f, 16.39f)
			);
			OutputRegressionModel(rmdBC2Ch1p22VIp);

			Program.OutputHeading("Biochemistry II ch. 1 exercise 3");
			var lrrBC2Ch1Ex3 = LinearRegression.Compute(
				new PointD(1e6, SignificantDigits.Round(1.0 / 1.16, 3, RoundingMode.ToEven)),
				new PointD(1e5, SignificantDigits.Round(1.0 / 8.46, 3, RoundingMode.ToEven)),
				new PointD(1e4, SignificantDigits.Round(1.0 / 24.94, 4, RoundingMode.ToEven)),
				new PointD(1e3, SignificantDigits.Round(1.0 / 27.94, 4, RoundingMode.ToEven)),
				new PointD(1e2, SignificantDigits.Round(1.0 / 29.95, 4, RoundingMode.ToEven))
			);
			OutputRegressionModel(lrrBC2Ch1Ex3.ChangeModel(MathematicalModel.Affine));
			var vmax0 = 1.0 / lrrBC2Ch1Ex3.Intercept;
			var Km0 = lrrBC2Ch1Ex3.Slope * vmax0;
			Console.WriteLine("Vmax = {0:g4} nmol/min\tKm = {1:g4} mol/L", vmax0 / 60, Km0);

			Program.OutputHeading("Biochemistry II ch. 1 exercise 4");
			var lrrBC2Ch1Ex4_0 = LinearRegression.Compute(
				new PointD(100, SignificantDigits.Round(1.0 / 16.7, 3, RoundingMode.ToEven)),
				new PointD(Math.Round(100 / 1.33, 1), SignificantDigits.Round(1.0 / 20, 3, RoundingMode.ToEven)),
				new PointD(50, SignificantDigits.Round(1.0 / 25, 3, RoundingMode.ToEven)),
				new PointD(40, SignificantDigits.Round(1.0 / 27, 3, RoundingMode.ToEven)),
				new PointD(20, SignificantDigits.Round(1.0 / 35.7, 3, RoundingMode.ToEven)),
				new PointD(10, SignificantDigits.Round(1.0 / 41.7, 3, RoundingMode.ToEven))
			);
			var lrrBC2Ch1Ex4_1 = LinearRegression.Compute(
				new PointD(100, SignificantDigits.Round(1.0 / 10, 3, RoundingMode.ToEven)),
				new PointD(Math.Round(100 / 1.33, 1), SignificantDigits.Round(1.0 / 12.5, 3, RoundingMode.ToEven)),
				new PointD(50, SignificantDigits.Round(1.0 / 16.7, 3, RoundingMode.ToEven)),
				new PointD(40, SignificantDigits.Round(1.0 / 19.2, 3, RoundingMode.ToEven)),
				new PointD(20, SignificantDigits.Round(1.0 / 27.8, 3, RoundingMode.ToEven)),
				new PointD(10, SignificantDigits.Round(1.0 / 35.7, 3, RoundingMode.ToEven))
			);
			OutputRegressionModel(lrrBC2Ch1Ex4_0.ChangeModel(MathematicalModel.Affine));
			OutputRegressionModel(lrrBC2Ch1Ex4_1.ChangeModel(MathematicalModel.Affine));
			vmax0 = 1.0 / lrrBC2Ch1Ex4_0.Intercept;
			Km0 = lrrBC2Ch1Ex4_0.Slope * vmax0;
			Console.WriteLine("Vmax  = {0,-4:g3} µmol/min*L  Km  = {1:g3} mol/L", vmax0, Km0);
			var vmax1 = 1.0 / lrrBC2Ch1Ex4_1.Intercept;
			var Km1 = lrrBC2Ch1Ex4_1.Slope * vmax1;
			Console.WriteLine("Vmax' = {0,-4:g3} µmol/min*L  Km' = {1:g3} mol/L", vmax1, Km1);
			var ki = 0.02 / ((Km1 / Km0) - 1);
			Console.WriteLine("Ki = {0:g3} mol/L", ki);

			Program.OutputHeading("Biochemistry II ch. 1 exercise 5");
			var lrrBC2Ch1Ex5_0 = LinearRegression.Compute(
				new PointD(RoundedInverse("0,010"), RoundedInverse("0,27")),
				new PointD(RoundedInverse("0,022"), RoundedInverse("0,50")),
				new PointD(RoundedInverse("0,046"), RoundedInverse("0,80")),
				new PointD(RoundedInverse("0,200"), RoundedInverse("1,50"))
			);
			var lrrBC2Ch1Ex5_1 = LinearRegression.Compute(
				new PointD(RoundedInverse("0,010"), RoundedInverse("0,21")),
				new PointD(RoundedInverse("0,022"), RoundedInverse("0,40")),
				new PointD(RoundedInverse("0,046"), RoundedInverse("0,65")),
				new PointD(RoundedInverse("0,200"), RoundedInverse("1,18"))
			);
			OutputRegressionModel(lrrBC2Ch1Ex5_0.ChangeModel(MathematicalModel.Affine));
			OutputRegressionModel(lrrBC2Ch1Ex5_1.ChangeModel(MathematicalModel.Affine));

			Program.OutputHeading("Biochemistry II ch. 1 exercise 6");
			var lrrBC2Ch1Ex6 = LinearRegression.Compute(
				new PointD(4 / 1.5, 4),
				new PointD(6 / 2.5, 6),
				new PointD(7.5 / 3.5, 7.5),
				new PointD(10.4 / 6, 10.4),
				new PointD(14 / 12.0, 14)
			);
			OutputRegressionModel(lrrBC2Ch1Ex6.ChangeModel(MathematicalModel.Affine));
			vmax0 = SignificantDigits.Round(lrrBC2Ch1Ex6.Intercept, 2, RoundingMode.ToEven);
			Km0 = SignificantDigits.Round(-lrrBC2Ch1Ex6.Slope, 2, RoundingMode.ToEven);
			Console.WriteLine("Vmax  = {0,-4} mUI  Km  = {1} mmol/L", vmax0, Km0);

			Program.OutputHeading("Biochemistry II laboratory 3");
			var lrrBC2Lab3_a = LinearRegression.Compute(
				new PointD(10, 0.174f),
				new PointD(20, 0.285f),
				new PointD(30, 0.387f),
				new PointD(40, 0.511f),
				new PointD(51, 0.659f)
			);
			Console.Write("Absorbance: ");
			OutputRegressionModel(lrrBC2Lab3_a.ChangeModel(MathematicalModel.Affine));
			var lrrBC2Lab3_r = LinearRegression.Compute(
				 new PointD(1.0 / 0.00150, 1.0 / 0.0071),
				 new PointD(1.0 / 0.00090, 1.0 / 0.0044),
				 new PointD(1.0 / 0.00076, 1.0 / 0.0038),
				 new PointD(1.0 / 0.00045, 1.0 / 0.0026),
				 new PointD(1.0 / 0.00030, 1.0 / 0.0018)
			);
			Console.Write("Reaction  : ");
			OutputRegressionModel(lrrBC2Lab3_r.ChangeModel(MathematicalModel.Affine));
			vmax0 = 1.0 / lrrBC2Lab3_r.Intercept;
			Km0 = vmax0 * lrrBC2Lab3_r.Slope;
			vmax1 = vmax0 * 60 / lrrBC2Lab3_a.Slope;
			Console.WriteLine("Vmax  = {1} A405/s  Km  = {2} mol/L{0}Vmax  = {3} µmol/min*L",
			 Environment.NewLine,
			 SignificantDigits.Round(vmax0, 5, RoundingMode.ToEven),
			 SignificantDigits.Round(Km0, 4, RoundingMode.ToEven),
			 SignificantDigits.Round(vmax1, 3, RoundingMode.ToEven));

			Program.OutputHeading("Cellular culture II Wound healing");
			var rmdCC2Healing = RegressionModel.Compute(
				new PointD(0, -0.2779f),
				new PointD(1, 0.2434f),
				new PointD(10, 1.1257f)
			);
			OutputRegressionModel(rmdCC2Healing);

#if !NET20
			Program.OutputHeading("Molecular biology laboratories");
			Program.OutputSizeOf<AgaroseRetention>();
			OutputAgaroseRetention("3B :", 27491, 9416, 6682, 2322, 2024, 564);
			OutputAgaroseRetention("6A :", 247, 280, 393, 234);
			OutputAgaroseRetention("6B :", 525);
			OutputAgaroseRetention("8  :", 2997, 647);
			OutputAgaroseRetention("9  :", 97);
#endif

			Program.OutputHeading("Factorial (1 to " + MaxFactorial + ")");
			var factorialSuite = new List<PointM>(MaxFactorial);
			for (byte i = 1; i <= MaxFactorial; i++) {
				var exact = ExtraMath.BigFactorial(i);
				var approximative = ExtraMath.StirlingApproximateFactorial(i, StirlingMode.Rounded);
				if (approximative != exact) {
					factorialSuite.Add(new PointM(i, exact / approximative));
				}
				Console.WriteLine("{0,2}! is {1,38:n0} ≈ {2,38:n0}", i, exact, approximative);
			}
			var rmmFactorial = RegressionModel.Compute(factorialSuite);
			Program.OutputSizeOf<RegressionModel<decimal>>();
			OutputRegressionModel(rmmFactorial);
			for (byte i = 1; i <= MaxFactorial; i++) {
				Console.WriteLine("{0,2}! is {1,38:n0} ≈ {2,38:n0}", i, ExtraMath.BigFactorial(i),
				 ExtraMath.StirlingApproximateFactorial(i, StirlingMode.Corrected));
			}
			Console.WriteLine("28! ≈ {0,39:n0}", ExtraMath.StirlingApproximateFactorial(28.0, StirlingMode.Corrected));

			Program.OutputHeading("German imperialists are out of luck");
			var rm = RegressionModel.Compute(new PointD(1, 1006), new PointD(2, 47), new PointD(3, 12));
			Console.WriteLine(rm);
			Console.WriteLine("The fourth reich would only last {0} years.", rm.Evaluate(4));
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
		where T : struct, IFormattable, IEquatable<T>
		{
			Console.WriteLine("{0:g6}\tr={1:g6}", mathModel, mathModel.R);
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
	}
}
