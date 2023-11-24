//
//  LinerarRegressionTest.cs
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
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	static class LinearRegressionTest
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
			OutputLinearRegression(kTalpha0_025n4, lrp);

			var dlrp = LinearRegression.Compute(
				new PointM(2.00m, 2.1m),
				new PointM(4.00m, 4.4m),
				new PointM(6.00m, 6.5m),
				new PointM(8.00m, 8.6m),
				new PointM(10.00m, 10.8m),
				new PointM(12.00m, 12.9m)
			);
			Console.WriteLine("Decimal data type");
			Console.WriteLine("-----------------");
			OutputLinearRegression((decimal)kTalpha0_025n4, dlrp);
			Console.WriteLine("a - 0.02 = {0}", dlrp.Intercept - 0.02m);
		}

		private static void OutputLinearRegression<T>(T studentLawValue, ILinearRegressionResult<T> lrp) where T : struct, IConvertible, IFormattable
		{
			Console.WriteLine(lrp);

			T b = lrp.GetSlope();
			T sr = lrp.ResidualStdDev();

			Console.WriteLine("r = {0}", lrp.GetCorrelation());
			Console.WriteLine("x = {0} y^ = {1}", 8.25f, lrp.ExtrapolateY(8.25f.ConvertTo<T>()));
			Console.WriteLine("y = {0} x^ = {1}", 3.4f, lrp.ExtrapolateX(3.4f.ConvertTo<T>()));
			Console.WriteLine("Relative bias: {0:p}", Matrix<T>.SubtractScalars(b, 1.ConvertTo<T>()));
			Console.WriteLine("x = {0} total error: {1} relative bias: {2}", 7, lrp.TotalError(7.ConvertTo<T>()), lrp.RelativeBias(7.ConvertTo<T>()));
			Console.WriteLine("SCT: {0} SCreg: {1} SCres: {2}", lrp.TotalVariation(), lrp.ExplainedVariation(), lrp.UnexplainedVariation());
			Console.WriteLine("Determination: {0}", lrp.Determination());
			Console.WriteLine("Std. dev.: residual {0} slope {1} intercept {2}", sr, lrp.SlopeStdDev(), lrp.InterceptStdDev());
			Console.WriteLine("Confidence interval for b: {0}", new ErrorMargin<T>(b, Matrix<T>.MultiplyScalars(studentLawValue, lrp.SlopeStdDev())));
			Console.WriteLine("Confidence interval for a: {0}", new ErrorMargin<T>(lrp.GetIntercept(), Matrix<T>.MultiplyScalars(studentLawValue, lrp.InterceptStdDev())));
			Console.WriteLine("Confidence interval for y^ when x={0}, repeated: {1}", 8,
			 new ErrorMargin<T>(lrp.ExtrapolateY(8.ConvertTo<T>()), Matrix<T>.MultiplyScalars(studentLawValue, sr, lrp.YExtrapolationConfidenceFactor(8.ConvertTo<T>(), true))));
			Console.WriteLine("Confidence interval for y^ when x={0}, once: {1}", 8,
			 new ErrorMargin<T>(lrp.ExtrapolateY(8.ConvertTo<T>()), Matrix<T>.MultiplyScalars(studentLawValue, sr, lrp.YExtrapolationConfidenceFactor(8.ConvertTo<T>(), false))));
			Console.WriteLine("Confidence interval for yc={0} k={1}: {2}", 7.5f, 5,
			 new ErrorMargin<T>(Divide(Matrix<T>.SubtractScalars(7.5f.ConvertTo<T>(), lrp.GetIntercept()), b), Matrix<T>.MultiplyScalars(studentLawValue, lrp.StdDevForYc(7.5f.ConvertTo<T>(), 5))));
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
	}
}
