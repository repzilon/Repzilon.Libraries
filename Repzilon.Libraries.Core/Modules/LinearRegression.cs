//
//  LinearRegression.cs
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
using System.Collections.Generic;

namespace Repzilon.Libraries.Core
{
	public static class LinearRegression
	{
		internal static readonly double OneOfLn10 = 1.0 / Math.Log(10);

		public static LinearRegressionResult Compute(params PointD[] points)
		{
			return Compute((IEnumerable<PointD>)points);
		}

		public static LinearRegressionResult Compute(IEnumerable<PointD> points)
		{
			if (points == null) {
				throw new ArgumentNullException("points");
			}
			var n = 0;
			double dblSumX = 0, dblSumY = 0, dblSumXY = 0;
			var dblMinX = Double.MaxValue;
			var dblMinY = Double.MaxValue;
			var dblMaxX = Double.MinValue;
			var dblMaxY = Double.MinValue;
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
			var dblAverageX = dblSumX / n;
			var dblAverageY = dblSumY / n;
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

			return new LinearRegressionResult(n, RoundOff.Error(a), RoundOff.Error(b), RoundOff.Error(r),
			 dblMinX, dblMinY, dblMaxX, dblMaxY, dblAverageX, RoundOff.Error(dblAverageY), dblStdDevX, dblStdDevY);
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
			var n = 0;
			decimal dcmSumX = 0, dcmSumY = 0, dcmSumXY = 0;
			var dcmMinX = Decimal.MaxValue;
			var dcmMinY = Decimal.MaxValue;
			var dcmMaxX = Decimal.MinValue;
			var dcmMaxY = Decimal.MinValue;
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
			var dcmAverageX = dcmSumX / n;
			var dcmAverageY = dcmSumY / n;
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
			return new DecimalLinearRegressionResult(n, RoundOff.Error(a), b, r,
			 dcmMinX, dcmMinY, dcmMaxX, dcmMaxY, dcmAverageX, dcmAverageY, dcmStdDevX, dblStdDevY);
		}
	}

	public static class RegressionModel
	{
		public static RegressionModel<double> Compute(params PointD[] points)
		{
			return Compute((IEnumerable<PointD>)points);
		}

		public static RegressionModel<double> Compute(IEnumerable<PointD> points)
		{
			if (points == null) {
				throw new ArgumentNullException("points");
			}
			var lstSemiLogX = new List<PointD>();
			var lstSemiLogY = new List<PointD>();
			var lstLogLog = new List<PointD>();
			var i = 0;
			foreach (var pt in points) {
				var log10x = Math.Log10(pt.X);
				var log10y = Math.Log10(pt.Y);
				lstSemiLogX.Add(new PointD(log10x, pt.Y));
				lstSemiLogY.Add(new PointD(pt.X, log10y));
				lstLogLog.Add(new PointD(log10x, log10y));
				i++;
			}
			var rmarAll = new RegressionModel<double>[] {
				LinearRegression.Compute(points).ChangeModel(MathematicalModel.Affine),
				LinearRegression.Compute(lstSemiLogX).ChangeModel(MathematicalModel.SemiLogX),
				LinearRegression.Compute(lstSemiLogY).ChangeModel(MathematicalModel.SemiLogY),
				LinearRegression.Compute(lstLogLog).ChangeModel(MathematicalModel.LogLog)
			};
			Array.Sort(rmarAll, delegate (RegressionModel<double> x, RegressionModel<double> y) {
				return -1 * x.R.CompareTo(y.R);
			});
			return rmarAll[0];
		}
	}
}
