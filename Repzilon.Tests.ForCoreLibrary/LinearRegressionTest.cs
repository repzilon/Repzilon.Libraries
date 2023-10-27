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
using System.Text;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal struct ErrorMargin
	{
		public double Middle;
		public double Margin;

		public ErrorMargin(double middle, double margin)
		{
			Middle = middle;
			Margin = margin;
		}

		public double Min()
		{
			return Middle - Margin;
		}

		public double Max()
		{
			return Middle + Margin;
		}

		public override string ToString()
		{
			var stbInterval = new StringBuilder();
			stbInterval.Append(this.Middle).Append(" ± ").Append(this.Margin).Append(" -> [").Append(this.Min()).Append("; ").Append(this.Max()).Append(']');
			return stbInterval.ToString();
		}
	}

	static class LinearRegressionTest
	{
		internal static void Run(string[] args)
		{
			const double kTalpha0_025n4 = 2.7764;
			var lrp = LinearRegression.Compute(new PointD[] {
				new PointD(2.00, 2.1),
				new PointD(4.00, 4.4),
				new PointD(6.00, 6.5),
				new PointD(8.00, 8.6),
				new PointD(10.00, 10.8),
				new PointD(12.00, 12.9)
			});
			Console.WriteLine(lrp);

			Console.WriteLine("r = {0}", lrp.Correlation);
			Console.WriteLine("x = {0} y^ = {1}", 8.25, lrp.ExtrapolateY(8.25));
			Console.WriteLine("y^ = {0} x^ = {1}", 3.4, lrp.ExtrapolateX(3.4));
			Console.WriteLine("Relative bias: {0:p}", lrp.Slope - 1);
			Console.WriteLine("x = {0} total error: {1} relative bias: {2}", 7, lrp.TotalError(7), lrp.RelativeBias(7));
			Console.WriteLine("SCT: {0} SCreg: {1} SCres: {2}", lrp.TotalVariation(), lrp.ExplainedVariation(), lrp.UnexplainedVariation());
			Console.WriteLine("Determination: {0}", lrp.Determination());
			Console.WriteLine("Std. dev.: residual {0} slope {1} intercept {2}", lrp.ResidualStdDev(), lrp.SlopeStdDev(), lrp.InterceptStdDev());
			Console.WriteLine("Confidence interval for b: {0}", new ErrorMargin(lrp.Slope, kTalpha0_025n4 * lrp.SlopeStdDev()));
			Console.WriteLine("Confidence interval for a: {0}", new ErrorMargin(lrp.Intercept, kTalpha0_025n4 * lrp.InterceptStdDev()));
			Console.WriteLine("Confidence interval for y^ when x={0}: {1}", 8,
			 new ErrorMargin(lrp.ExtrapolateY(8), kTalpha0_025n4 * lrp.ResidualStdDev() * lrp.YExtrapolationConfidenceFactor(8)));
			Console.WriteLine("Confidence interval for yc={0} k={1}: {2}", 7.5, 5,
			 new ErrorMargin((7.5 - lrp.Intercept) / lrp.Slope, kTalpha0_025n4 * lrp.StdDevForYc(7.5, 5)));
		}
	}
}
