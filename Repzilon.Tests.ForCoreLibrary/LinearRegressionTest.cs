//
//  LinearRegressionTest.cs
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
using System.Globalization;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class LinearRegressionTest
	{
		internal static void Run(string[] args)
		{
			const double kTalpha0_025n4 = 2.7764;

			var lrp = LinearRegression.Compute(
				new PointD(2.00, 2.1),
				new PointD(4.00, 4.4),
				new PointD(6.00, 6.5),
				new PointD(8.00, 8.6),
				new PointD(10.00, 10.8),
				new PointD(12.00, 12.9)
			);
			Console.WriteLine("Double data type");
			Console.WriteLine("----------------");
			Program.OutputSizeOf<PointD>();
			Program.OutputSizeOf<LinearRegressionResult>();
			OutputLinearRegression2(lrp, kTalpha0_025n4, "G", true, 8.25f, 3.4);
			// x can also be 7 or 8, and y can also be 7.5

			var dlrp = LinearRegression.Compute(
				new PointM(2.00m, 2.1m),
				new PointM(4.00m, 4.4m),
				new PointM(6.00m, 6.5m),
				new PointM(8.00m, 8.6m),
				new PointM(10.00m, 10.8m),
				new PointM(12.00m, 12.9m)
			);
			Console.Write(Environment.NewLine);
			Console.WriteLine("Decimal data type");
			Console.WriteLine("-----------------");
			Program.OutputSizeOf<PointM>();
			Program.OutputSizeOf<DecimalLinearRegressionResult>();
			OutputLinearRegression2(dlrp, (decimal)kTalpha0_025n4, "G18", true, 7, 7.5m);
			Console.WriteLine("a - 0.02 = {0}", dlrp.Intercept - 0.02m);

			Console.Write(Environment.NewLine);
			Console.WriteLine("Revision");
			Console.WriteLine("--------");
			var lrrRev5 = LinearRegression.Compute(
				new PointM(0, 0.06m),
				new PointM(5, 1.25m),
				new PointM(10, 2.38m),
				new PointM(15, 3.58m),
				new PointM(20, 4.61m)
			);
			OutputLinearRegression2(lrrRev5, 3.1824m, "G7", false, 12, 4.154m);

			Console.Write(Environment.NewLine);
			Console.WriteLine("Math I Example 38");
			Console.WriteLine("-----------------");
			var lrrM1Ex38 = LinearRegression.Compute(
				PointD.LogLog(100.0, 0.240),
				PointD.LogLog(150.0, 0.295),
				PointD.LogLog(250.0, 0.380),
				PointD.LogLog(300.0, 0.415),
				PointD.LogLog(400.0, 0.480),
				PointD.LogLog(550.0, 0.560)
			);
			var rmdMEx38 = lrrM1Ex38.ChangeModel(MathematicalModel.LogLog);
			OutputRegressionModel(rmdMEx38);

			Console.Write(Environment.NewLine);
			Console.WriteLine("Math I Exercise");
			Console.WriteLine("---------------");
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

			Console.Write(Environment.NewLine);
			Console.WriteLine("Biochemistry II ch. 1 pp. 15-16");
			Console.WriteLine("-------------------------------");
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
				new PointD(12.5, 0.037),
				new PointD(20, 0.050),
				new PointD(25, 0.055),
				new PointD(50, 0.073),
				new PointD(100, 0.091)
			);
			OutputRegressionModel(rmdBC2Ch1p16);

			Console.Write(Environment.NewLine);
			Console.WriteLine("Biochemistry II ch. 1 exercise 3");
			Console.WriteLine("--------------------------------");
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

			Console.Write(Environment.NewLine);
			Console.WriteLine("Biochemistry II ch. 1 exercise 4");
			Console.WriteLine("--------------------------------");
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

			Console.Write(Environment.NewLine);
			Console.WriteLine("Biochemistry II ch. 1 exercise 5");
			Console.WriteLine("--------------------------------");
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

			Console.Write(Environment.NewLine);
			Console.WriteLine("Cellular culture II Wound healing");
			Console.WriteLine("---------------------------------");
			var rmdCC2Healing = RegressionModel.Compute(
				new PointD(0, -0.2779),
				new PointD(1, 0.2434),
				new PointD(10, 1.1257)
			);
			OutputRegressionModel(rmdCC2Healing);
		}

		private static void OutputLinearRegression2<T>(ILinearRegressionResult<T> lrp, T studentLawValue,
		string numberFormat, bool checkBiaises, T? xForYExtrapolation, T? yForXExtrapolation)
		where T : struct, IConvertible, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			var ciCu = CultureInfo.CurrentCulture;
			Console.WriteLine(lrp.ToString(numberFormat, ciCu));

			T b = lrp.Slope;
			T sr = lrp.ResidualStdDev();

			Console.Write("r = {0}\tr^2 = {1}", lrp.Correlation.ToString(numberFormat, ciCu), lrp.Determination().ToString(numberFormat, ciCu));
#if NET20
			Console.Write(Environment.NewLine);
#else
			if (checkBiaises) {
				Console.WriteLine("\trelative bias: {0:p}", GenericArithmetic<T>.SubtractScalars(b, MatrixExtensionMethods.ConvertTo<T>(1)));
			} else {
				Console.Write(Environment.NewLine);
			}
#endif
			Console.WriteLine("SCT: {0}\tSCreg: {1}\tSCres: {2}", lrp.TotalVariation().ToString(numberFormat, ciCu), lrp.ExplainedVariation().ToString(numberFormat, ciCu), lrp.UnexplainedVariation().ToString(numberFormat, ciCu));
			Console.WriteLine("Std. dev.: residual {0}\tslope {1}\tintercept {2}", sr.ToString(numberFormat, ciCu), lrp.SlopeStdDev().ToString(numberFormat, ciCu), lrp.InterceptStdDev().ToString(numberFormat, ciCu));
#if !NET20
			Console.WriteLine("b = {0}", new ErrorMargin<T>(b, GenericArithmetic<T>.MultiplyScalars(studentLawValue, lrp.SlopeStdDev())).ToString(numberFormat, ciCu));
			Console.WriteLine("a = {0}", new ErrorMargin<T>(lrp.Intercept, GenericArithmetic<T>.MultiplyScalars(studentLawValue, lrp.InterceptStdDev())).ToString(numberFormat, ciCu));
#endif
			if (xForYExtrapolation.HasValue) {
				var x = xForYExtrapolation.Value;
#if !NET20
				OutputYExtrapolation(lrp, studentLawValue, numberFormat, ciCu, x, sr, true);
				OutputYExtrapolation(lrp, studentLawValue, numberFormat, ciCu, x, sr, false);
#endif
				if (checkBiaises) {
					Console.WriteLine("x = {0}\t\ttotal error: {1}\trelative bias: {2}",
					 x.ToString(numberFormat, ciCu),
					 lrp.TotalError(x).ToString(numberFormat, ciCu),
					 lrp.RelativeBias(x).ToString(numberFormat, ciCu));
				}
			}
#if !NET20
			if (yForXExtrapolation.HasValue) {
				var yc = yForXExtrapolation.Value;
				OutputXExtrapolation(lrp, studentLawValue, numberFormat, ciCu, yc, 5, b);

			}
#endif
		}

#if !NET20
		private static void OutputYExtrapolation<T>(ILinearRegressionResult<T> lrp, T studentLawValue,
		string numberFormat, IFormatProvider culture, T x, T sr, bool repeated)
		where T : struct, IConvertible, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			Console.WriteLine("x = {0} k = {1}\ty^ = {2}",
			 x.ToString(numberFormat, culture),
			 repeated ? "Infinity" : "1\t",
			 new ErrorMargin<T>(lrp.InterpolateY(x),
			 GenericArithmetic<T>.MultiplyScalars(studentLawValue, sr, lrp.YExtrapolationConfidenceFactor(x, repeated))).ToString(numberFormat, culture));
		}

		private static void OutputXExtrapolation<T>(ILinearRegressionResult<T> lrp, T studentLawValue,
		string numberFormat, IFormatProvider culture, T yc, int k, T b)
		where T : struct, IConvertible, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			Console.WriteLine("yc= {0} k = {1}\t\tx0 = {2}",
			 yc.ToString(numberFormat, culture),
			 k.ToString(numberFormat, culture),
			 new ErrorMargin<T>(Divide(GenericArithmetic<T>.SubtractScalars(yc, lrp.Intercept), b), GenericArithmetic<T>.MultiplyScalars(studentLawValue, lrp.StdDevForYc(yc, k))).ToString(numberFormat, culture));
		}

		private static T Divide<T>(T dividend, T divisor) where T : struct, IConvertible
		{
			var tc = dividend.GetTypeCode();
			if (tc == TypeCode.Double) {
				return (dividend.ConvertTo<double>() / divisor.ConvertTo<double>()).ConvertTo<T>();
			} else if (tc == TypeCode.Decimal) {
				return Decimal.Divide(dividend.ConvertTo<decimal>(), divisor.ConvertTo<decimal>()).ConvertTo<T>();
			} else {
				throw new NotSupportedException();
			}
		}
#endif

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
