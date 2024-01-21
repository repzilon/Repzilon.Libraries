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
		public static LinearRegressionResult Compute(params PointD[] points)
		{
			return Compute((IEnumerable<PointD>)points);
		}

		public static LinearRegressionResult Compute(IEnumerable<PointD> points)
		{
			if (points == null) {
				throw new ArgumentNullException("points");
			}
			int n = 0;
			double dblSumX = 0, dblSumY = 0, dblSumXY = 0;
			double dblMinX = Double.MaxValue, dblMinY = Double.MaxValue;
			double dblMaxX = Double.MinValue, dblMaxY = Double.MinValue;
			foreach (var pt in points) {
				n++;
				dblSumX += pt.X;
				dblSumY += pt.Y;
				dblSumXY += pt.X * pt.Y;
				dblMinX = Math.Min(dblMinX, pt.X);
				dblMaxX = Math.Max(dblMaxX, pt.X);
				dblMinY = Math.Min(dblMinY, pt.Y);
				dblMaxY = Math.Max(dblMaxY, pt.Y);
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
			lrp.MinX = dblMinX;
			lrp.MaxX = dblMaxX;
			lrp.MinY = dblMinY;
			lrp.MaxY = dblMaxY;
			return lrp;
		}

		public static DecimalLinearRegressionResult Compute(params PointM[] points)
		{
			return Compute((IEnumerable<PointM>)points);
		}

		public static DecimalLinearRegressionResult Compute(IEnumerable<PointM> points)
		{
			if (points == null) {
				throw new ArgumentNullException("points");
			}
			int n = 0;
			decimal dcmSumX = 0, dcmSumY = 0, dcmSumXY = 0;
			decimal dcmMinX = Decimal.MaxValue, dcmMinY = Decimal.MaxValue;
			decimal dcmMaxX = Decimal.MinValue, dcmMaxY = Decimal.MinValue;
			foreach (var pt in points) {
				n++;
				dcmSumX += pt.X;
				dcmSumY += pt.Y;
				dcmSumXY += pt.X * pt.Y;
				dcmMinX = Math.Min(dcmMinX, pt.X);
				dcmMaxX = Math.Max(dcmMaxX, pt.X);
				dcmMinY = Math.Min(dcmMinY, pt.Y);
				dcmMaxY = Math.Max(dcmMaxY, pt.Y);
			}
			if (n < 1) {
				throw new ArgumentNullException("points");
			}
			decimal dcmAverageX = dcmSumX / n;
			decimal dcmAverageY = dcmSumY / n;
			decimal dcmStdDevX = 0;
			decimal dblStdDevY = 0;
			foreach (var pt in points) {
				var d = pt.X - dcmAverageX;
				dcmStdDevX += d * d;
				d = pt.Y - dcmAverageY;
				dblStdDevY += d * d;
			}
			dcmStdDevX = ExtraMath.Sqrt(dcmStdDevX / (n - 1));
			dblStdDevY = ExtraMath.Sqrt(dblStdDevY / (n - 1));
			var b = (dcmSumXY - (n * dcmAverageX * dcmAverageY)) / ((n - 1) * dcmStdDevX * dcmStdDevX);
			var a = dcmAverageY - (b * dcmAverageX);
			var r = b * dcmStdDevX / dblStdDevY;

			var lrp = new DecimalLinearRegressionResult();
			lrp.Count = n;
			lrp.Slope = b;
			lrp.Intercept = RoundOff.Error(a); // Eat dirt
			lrp.Correlation = r;
			lrp.StdDevOfY = dblStdDevY;
			lrp.StdDevOfX = dcmStdDevX;
			lrp.AverageX = dcmAverageX;
			lrp.AverageY = dcmAverageY;
			lrp.MinX = dcmMinX;
			lrp.MaxX = dcmMaxX;
			lrp.MinY = dcmMinY;
			lrp.MaxY = dcmMaxY;
			return lrp;
		}
	}
}
