//
//  RegressionModel.cs
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
using System.Collections.Generic;

namespace Repzilon.Libraries.Core.Regression
{
	public static class RegressionModel
	{
		public static RegressionModel<double> Compute(params PointD[] points)
		{
			return Compute(points as IList<PointD>);
		}

		public static RegressionModel<double> Compute(IEnumerable<PointD> points)
		{
			int c;
			var lstarAll = InitLists(new List<PointD>(points), out c);

			for (int i = 0; i < c; i++) {
				AddDataPoint(lstarAll, lstarAll[(int)MathematicalModel.Affine][i]);
			}

			return FinishCompute(lstarAll);
		}

		public static RegressionModel<double> Compute(IList<PointD> points)
		{
			int c;
			var lstarAll = InitLists(points, out c);

			for (int i = 0; i < c; i++) {
				AddDataPoint(lstarAll, points[i]);
			}

			return FinishCompute(lstarAll);
		}

		private static IList<T>[] InitLists<T>(IList<T> points, out int c)
		{
			if (points == null) {
				throw new ArgumentNullException("points");
			}

			var lstarAll = new IList<T>[4];
			lstarAll[(int)MathematicalModel.Affine] = points;
			c = points.Count;
			for (int i = 1; i < 4; i++) {
				lstarAll[i] = new List<T>(c);
			}
			return lstarAll;
		}

		private static void AddDataPoint(IList<PointD>[] lstarAll, PointD pt)
		{
			var x = pt.X;
			var y = pt.Y;
			var log10X = Math.Log10(x);
			var log10Y = Math.Log10(y);
			lstarAll[(int)MathematicalModel.SemiLogX].Add(new PointD(log10X, y));
			lstarAll[(int)MathematicalModel.SemiLogY].Add(new PointD(x, log10Y));
			lstarAll[(int)MathematicalModel.LogLog].Add(new PointD(log10X, log10Y));
		}

		private static RegressionModel<double> FinishCompute(IList<PointD>[] lstarAll)
		{
			var rmarAll = new RegressionModel<double>[4];
			for (int i = 0; i < 4; i++) {
				rmarAll[i] = LinearRegression.Compute(lstarAll[i]).ChangeModel((MathematicalModel)i);
			}

			Array.Sort(rmarAll, OrderByDeterminationDesc);
			return rmarAll[0];
		}

		private static int OrderByDeterminationDesc(RegressionModel<double> x, RegressionModel<double> y)
		{
			var xR = x.R;
			var yR = y.R;
			return -(xR * xR).CompareTo(yR * yR);
		}

		public static RegressionModel<decimal> Compute(params PointM[] points)
		{
			return Compute(points as IList<PointM>);
		}

		public static RegressionModel<decimal> Compute(IEnumerable<PointM> points)
		{
			int c;
			var lstarAll = InitLists(new List<PointM>(points), out c);

			for (int i = 0; i < c; i++) {
				AddDataPoint(lstarAll, lstarAll[(int)MathematicalModel.Affine][i]);
			}

			return FinishCompute(lstarAll);
		}

		public static RegressionModel<decimal> Compute(IList<PointM> points)
		{
			int c;
			var lstarAll = InitLists(points, out c);

			for (int i = 0; i < c; i++) {
				AddDataPoint(lstarAll, points[i]);
			}

			return FinishCompute(lstarAll);
		}

		private static void AddDataPoint(IList<PointM>[] lstarAll, PointM pt)
		{
			var x = pt.X;
			var y = pt.Y;
			var log10X = (decimal)Math.Log10((double)x);
			var log10Y = (decimal)Math.Log10((double)y);
			lstarAll[(int)MathematicalModel.SemiLogX].Add(new PointM(log10X, y));
			lstarAll[(int)MathematicalModel.SemiLogY].Add(new PointM(x, log10Y));
			lstarAll[(int)MathematicalModel.LogLog].Add(new PointM(log10X, log10Y));
		}

		private static RegressionModel<decimal> FinishCompute(IList<PointM>[] lstarAll)
		{
			var rmarAll = new RegressionModel<decimal>[4];
			for (int i = 0; i < 4; i++) {
				rmarAll[i] = LinearRegression.Compute(lstarAll[i]).ChangeModel((MathematicalModel)i);
			}

			Array.Sort(rmarAll, OrderByDeterminationDesc);
			return rmarAll[0];
		}

		private static int OrderByDeterminationDesc(RegressionModel<decimal> x, RegressionModel<decimal> y)
		{
			var xR = x.R;
			var yR = y.R;
			return -(xR * xR).CompareTo(yR * yR);
		}
	}
}
