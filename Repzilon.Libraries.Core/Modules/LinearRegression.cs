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
			double b;
			double dblAverageX = 0, dblAverageY = 0, dblSumXy = 0;
			var dblMinX = Double.MaxValue;
			var dblMinY = Double.MaxValue;
			var dblMaxX = Double.MinValue;
			var dblMaxY = Double.MinValue;
			foreach (var pt in points) {
				n++;
				var x = pt.X;
				var y = pt.Y;
				dblAverageX += x;
				dblAverageY += y;
				dblSumXy += x * y;
				dblMinX = Math.Min(dblMinX, x);
				dblMaxX = Math.Max(dblMaxX, x);
				dblMinY = Math.Min(dblMinY, y);
				dblMaxY = Math.Max(dblMaxY, y);
			}
			if (n < 1) {
				throw new ArgumentNullException("points");
			}
			dblAverageX = dblAverageX / n;
			dblAverageY = dblAverageY / n;
			double dblStdDevX = 0;
			double dblStdDevY = 0;
			foreach (var pt in points) {
				b = pt.X - dblAverageX;
				dblStdDevX += b * b;
				b = pt.Y - dblAverageY;
				dblStdDevY += b * b;
			}
			dblStdDevX = Math.Sqrt(dblStdDevX / (n - 1));
			dblStdDevY = Math.Sqrt(dblStdDevY / (n - 1));
			b = (dblSumXy - (n * dblAverageX * dblAverageY)) / ((n - 1) * dblStdDevX * dblStdDevX);
			return new LinearRegressionResult(n,
			 RoundOff.Error(dblAverageY - (b * dblAverageX)), RoundOff.Error(b), RoundOff.Error(b * dblStdDevX / dblStdDevY),
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
			decimal b;
			decimal dcmAverageX = 0, dcmAverageY = 0, dcmSumXy = 0;
			var dcmMinX = Decimal.MaxValue;
			var dcmMinY = Decimal.MaxValue;
			var dcmMaxX = Decimal.MinValue;
			var dcmMaxY = Decimal.MinValue;
			foreach (var pt in points) {
				n++;
				var x = pt.X;
				var y = pt.Y;
				dcmAverageX += x;
				dcmAverageY += y;
				dcmSumXy += x * y;
				dcmMinX = Math.Min(dcmMinX, x);
				dcmMaxX = Math.Max(dcmMaxX, x);
				dcmMinY = Math.Min(dcmMinY, y);
				dcmMaxY = Math.Max(dcmMaxY, y);
			}
			if (n < 1) {
				throw new ArgumentNullException("points");
			}
			dcmAverageX = dcmAverageX / n;
			dcmAverageY = dcmAverageY / n;
			decimal dcmStdDevX = 0;
			decimal dblStdDevY = 0;
			foreach (var pt in points) {
				b = pt.X - dcmAverageX;
				dcmStdDevX += b * b;
				b = pt.Y - dcmAverageY;
				dblStdDevY += b * b;
			}
			dcmStdDevX = ExtraMath.Sqrt(dcmStdDevX / (n - 1));
			dblStdDevY = ExtraMath.Sqrt(dblStdDevY / (n - 1));
			b = (dcmSumXy - (n * dcmAverageX * dcmAverageY)) / ((n - 1) * dcmStdDevX * dcmStdDevX);
			return new DecimalLinearRegressionResult(n,
			 RoundOff.Error(dcmAverageY - (b * dcmAverageX)), b, b * dcmStdDevX / dblStdDevY,
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
			var lstarAll = new List<PointD>[4];
			lstarAll[(int)MathematicalModel.Affine] = new List<PointD>(points);
			int i;
			var c = lstarAll[(int)MathematicalModel.Affine].Count;
			for (i = 1; i < 4; i++) {
				lstarAll[i] = new List<PointD>(c);
			}
			for (i = 0; i < c; i++) {
				var pt = lstarAll[(int)MathematicalModel.Affine][i];
				var x = pt.X;
				var y = pt.Y;
				var log10X = Math.Log10(x);
				var log10Y = Math.Log10(y);
				lstarAll[(int)MathematicalModel.SemiLogX].Add(new PointD(log10X, y));
				lstarAll[(int)MathematicalModel.SemiLogY].Add(new PointD(x, log10Y));
				lstarAll[(int)MathematicalModel.LogLog].Add(new PointD(log10X, log10Y));
			}
			var rmarAll = new RegressionModel<double>[4];
			for (i = 0; i < 4; i++) {
				rmarAll[i] = LinearRegression.Compute(lstarAll[i]).ChangeModel((MathematicalModel)i);
			}
			Array.Sort(rmarAll, OrderByDeterminationDesc);
			return rmarAll[0];
		}

		private static int OrderByDeterminationDesc(RegressionModel<double> x, RegressionModel<double> y)
		{
			var xR = x.R;
			var yR = y.R;
			return -1 * (xR * xR).CompareTo(yR * yR);
		}

		public static RegressionModel<decimal> Compute(params PointM[] points)
		{
			return Compute((IEnumerable<PointM>)points);
		}

		public static RegressionModel<decimal> Compute(IEnumerable<PointM> points)
		{
			if (points == null) {
				throw new ArgumentNullException("points");
			}
			var lstarAll = new List<PointM>[4];
			lstarAll[(int)MathematicalModel.Affine] = new List<PointM>(points);
			int i;
			var c = lstarAll[(int)MathematicalModel.Affine].Count;
			for (i = 1; i < 4; i++) {
				lstarAll[i] = new List<PointM>(c);
			}
			for (i = 0; i < c; i++) {
				var pt = lstarAll[(int)MathematicalModel.Affine][i];
				var x = pt.X;
				var y = pt.Y;
				var log10X = (decimal)Math.Log10((double)x);
				var log10Y = (decimal)Math.Log10((double)y);
				lstarAll[(int)MathematicalModel.SemiLogX].Add(new PointM(log10X, y));
				lstarAll[(int)MathematicalModel.SemiLogY].Add(new PointM(x, log10Y));
				lstarAll[(int)MathematicalModel.LogLog].Add(new PointM(log10X, log10Y));
			}
			var rmarAll = new RegressionModel<decimal>[4];
			for (i = 0; i < 4; i++) {
				rmarAll[i] = LinearRegression.Compute(lstarAll[i]).ChangeModel((MathematicalModel)i);
			}
			Array.Sort(rmarAll, OrderByDeterminationDesc);
			return rmarAll[0];
		}

		private static int OrderByDeterminationDesc(RegressionModel<decimal> x, RegressionModel<decimal> y)
		{
			var xR = x.R;
			var yR = y.R;
			return -1 * (xR * xR).CompareTo(yR * yR);
		}
	}
}
