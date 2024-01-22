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
				new PointD(Math.Log10(100.0), Math.Log10(0.240)),
				new PointD(Math.Log10(150.0), Math.Log10(0.295)),
				new PointD(Math.Log10(250.0), Math.Log10(0.380)),
				new PointD(Math.Log10(300.0), Math.Log10(0.415)),
				new PointD(Math.Log10(400.0), Math.Log10(0.480)),
				new PointD(Math.Log10(550.0), Math.Log10(0.560))
			);
			var rmdMEx38 = lrrM1Ex38.ChangeModel(MathematicalModel.LogLog);
			OutputRegressionModel(rmdMEx38);

			Console.Write(Environment.NewLine);
			Console.WriteLine("Math I Exercise");
			Console.WriteLine("---------------");
			var lrrM1Exer = LinearRegression.Compute(
				new PointD(8.0, Math.Log10(9858)),
				new PointD(14.0, Math.Log10(9416)),
				new PointD(18.0, Math.Log10(7234)),
				new PointD(24.0, Math.Log10(5426)),
				new PointD(37.5, Math.Log10(2789)),
				new PointD(41.0, Math.Log10(2251)),
				new PointD(71.0, Math.Log10(564))
			);
			var rmdM1Exer = lrrM1Exer.ChangeModel(MathematicalModel.Exponential);
			OutputRegressionModel(rmdM1Exer);
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
			if (checkBiaises) {
				Console.WriteLine("\trelative bias: {0:p}", GenericArithmetic<T>.SubtractScalars(b, 1.ConvertTo<T>()));
			} else {
				Console.Write(Environment.NewLine);
			}
			Console.WriteLine("SCT: {0}\tSCreg: {1}\tSCres: {2}", lrp.TotalVariation().ToString(numberFormat, ciCu), lrp.ExplainedVariation().ToString(numberFormat, ciCu), lrp.UnexplainedVariation().ToString(numberFormat, ciCu));
			Console.WriteLine("Std. dev.: residual {0}\tslope {1}\tintercept {2}", sr.ToString(numberFormat, ciCu), lrp.SlopeStdDev().ToString(numberFormat, ciCu), lrp.InterceptStdDev().ToString(numberFormat, ciCu));
			Console.WriteLine("b = {0}", new ErrorMargin<T>(b, GenericArithmetic<T>.MultiplyScalars(studentLawValue, lrp.SlopeStdDev())).ToString(numberFormat, ciCu));
			Console.WriteLine("a = {0}", new ErrorMargin<T>(lrp.Intercept, GenericArithmetic<T>.MultiplyScalars(studentLawValue, lrp.InterceptStdDev())).ToString(numberFormat, ciCu));
			if (xForYExtrapolation.HasValue) {
				var x = xForYExtrapolation.Value;
				OutputYExtrapolation(lrp, studentLawValue, numberFormat, ciCu, x, sr, true);
				OutputYExtrapolation(lrp, studentLawValue, numberFormat, ciCu, x, sr, false);
				if (checkBiaises) {
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

		private static void OutputRegressionModel(RegressionModel<double> mathModel)
		{
			Console.WriteLine("{0:g6}\tr={1:g6}", mathModel, mathModel.R);
			var kind = mathModel.Model;
			if (kind == MathematicalModel.Exponential) {
				Console.WriteLine("{0:eg6}\tGrowth rate: {1:p2}", mathModel, mathModel.GrowthRate());
			} else if (kind == MathematicalModel.Logarithmic) {
				Console.WriteLine("{0:eg6}", mathModel);
			}
		}
	}
}
