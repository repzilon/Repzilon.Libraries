//
//  LinearRegression.cs
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
using System.Collections.Generic;

namespace Repzilon.Libraries.Core
{
	public static class LinearRegression
	{
		public static LinearRegressionResult Compute(IEnumerable<PointD> points)
		{
			if (points == null) {
				throw new ArgumentNullException("points");
			}
			int n = 0;
			double dblSumX = 0, dblSumY = 0, dblSumXY = 0;
			foreach (var pt in points) {
				n++;
				dblSumX += pt.X;
				dblSumY += pt.Y;
				dblSumXY += pt.X * pt.Y;
			}
			if (n < 1) {
				throw new ArgumentNullException("points");
			}
			double dblAverageX = dblSumX / n;
			double dblAverageY = dblSumY / n;
			double dblStdDevX = 0;
			double dblStdDevY = 0;
			foreach (var pt in points) {
				var d = pt.X - dblAverageX;
				dblStdDevX += d * d;
				d = pt.Y - dblAverageY;
				dblStdDevY += d * d;
			}
			dblStdDevX = Math.Sqrt(dblStdDevX / (n - 1));
			dblStdDevY = Math.Sqrt(dblStdDevY / (n - 1));
			var b = (dblSumXY - (n * dblAverageX * dblAverageY)) / ((n - 1) * dblStdDevX * dblStdDevX);
			var a = dblAverageY - (b * dblAverageX);
			var r = b * dblStdDevX / dblStdDevY;

			var lrp = new LinearRegressionResult();
			lrp.Count = n;
			lrp.Slope = RoundOff.Error(b);
			lrp.Intercept = RoundOff.Error(a);
			lrp.Correlation = RoundOff.Error(r);
			lrp.StdDevOfY = dblStdDevY;
			lrp.StdDevOfX = dblStdDevX;
			lrp.AverageX = dblAverageX;
			lrp.AverageY = RoundOff.Error(dblAverageY);
			return lrp;
		}
	}
}
