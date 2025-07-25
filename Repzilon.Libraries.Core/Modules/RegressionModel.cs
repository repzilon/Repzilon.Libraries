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
	public enum RegressionOption : byte
	{
		None,
		OmitAnyNonPositive,
		AddZeroAtOrigin
	}

	public static class RegressionModel
	{
		public static RegressionModel<double> Compute(params PointD[] points)
		{
			return Compute(RegressionOption.None, points as IList<PointD>);
		}

		/*
		private static RegressionModel<double> Compute(RegressionOption option, params PointD[] points)
		{
			return Compute(option, points as IList<PointD>);
		}// */

		public static RegressionModel<double> Compute(IEnumerable<PointD> points)
		{
			return Compute(RegressionOption.None, points);
		}

		private static RegressionModel<double> Compute(RegressionOption option, IEnumerable<PointD> points)
		{
			int c;
			var lstarAll = InitLists(option, new List<PointD>(points), out c);

			for (var i = 0; i < c; i++) {
				AddDataPoint(option, lstarAll, lstarAll[(int)MathematicalModel.Affine][i]);
			}

			return FinishCompute(lstarAll);
		}

		public static RegressionModel<double> Compute(IList<PointD> points)
		{
			return Compute(RegressionOption.None, points);
		}

		private static RegressionModel<double> Compute(RegressionOption option, IList<PointD> points)
		{
			int c;
			var lstarAll = InitLists(option, points, out c);

			for (var i = 0; i < c; i++) {
				AddDataPoint(option, lstarAll, points[i]);
			}

			return FinishCompute(lstarAll);
		}

		private static IList<T>[] InitLists<T>(RegressionOption option, IList<T> points, out int c)
		where T: new()
		{
			if (points == null) {
				throw new ArgumentNullException(nameof(points));
			}

			var lstarAll = new IList<T>[4];
			c = points.Count;
			if (option == RegressionOption.AddZeroAtOrigin) {
				var l1 = new List<T>(checked(c + 1));
				l1.Add(new T());
				l1.AddRange(points);
				lstarAll[(int)MathematicalModel.Affine] = l1;
			} else {
				lstarAll[(int)MathematicalModel.Affine] = points;
			}
			for (var i = 1; i < 4; i++) {
				lstarAll[i] = new List<T>(c);
			}
			return lstarAll;
		}

		private static void AddDataPoint(RegressionOption option, IList<PointD>[] lstarAll, PointD pt)
		{
			var c = 0;
			var x = pt.X;
			var y = pt.Y;
			var log10X = Double.NaN;
			var log10Y = log10X;
			var blnDontExcludeZero = (option != RegressionOption.OmitAnyNonPositive);

			if (blnDontExcludeZero || (x > 0)) {
				log10X = Math.Log10(x);
				lstarAll[(int)MathematicalModel.SemiLogX].Add(new PointD(log10X, y));
				c++;
			}
			if (blnDontExcludeZero || (y > 0)) {
				log10Y = Math.Log10(y);
				lstarAll[(int)MathematicalModel.SemiLogY].Add(new PointD(x, log10Y));
				c++;
			}
			if (blnDontExcludeZero || (c == 2)) {
				lstarAll[(int)MathematicalModel.LogLog].Add(new PointD(log10X, log10Y));
			}
		}

		private static RegressionModel<double> FinishCompute(IList<PointD>[] allModelPoints)
		{
			var rmarAll = new RegressionModel<double>[4];
			for (var i = 0; i < 4; i++) {
				rmarAll[i] = LinearRegression.Compute(allModelPoints[i]).ChangeModel((MathematicalModel)i);
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
			return Compute(RegressionOption.None, points as IList<PointM>);
		}

		/*
		private static RegressionModel<decimal> Compute(RegressionOption option, params PointM[] points)
		{
			return Compute(option, points as IList<PointM>);
		}// */

		public static RegressionModel<decimal> Compute(IEnumerable<PointM> points)
		{
			return Compute(RegressionOption.None, points);
		}

		private static RegressionModel<decimal> Compute(RegressionOption option, IEnumerable<PointM> points)
		{
			int c;
			var lstarAll = InitLists(option, new List<PointM>(points), out c);

			for (var i = 0; i < c; i++) {
				AddDataPoint(option, lstarAll, lstarAll[(int)MathematicalModel.Affine][i]);
			}

			return FinishCompute(lstarAll);
		}

		public static RegressionModel<decimal> Compute(IList<PointM> points)
		{
			return Compute(RegressionOption.None, points);
		}

		private static RegressionModel<decimal> Compute(RegressionOption option, IList<PointM> points)
		{
			int c;
			var lstarAll = InitLists(option, points, out c);

			for (var i = 0; i < c; i++) {
				AddDataPoint(option, lstarAll, points[i]);
			}

			return FinishCompute(lstarAll);
		}

		private static void AddDataPoint(RegressionOption option, IList<PointM>[] lstarAll, PointM pt)
		{
			var c      = 0;
			var x      = pt.X;
			var y      = pt.Y;
			var log10X = 0m;
			var log10Y = 0m;
			var blnDontExcludeZero = (option != RegressionOption.OmitAnyNonPositive);

			if (blnDontExcludeZero || (x > 0)) {
				log10X = ExtraMath.Log10(x);
				lstarAll[(int)MathematicalModel.SemiLogX].Add(new PointM(log10X, y));
				c++;
			}
			if (blnDontExcludeZero || (y > 0)) {
				log10Y = ExtraMath.Log10(y);
				lstarAll[(int)MathematicalModel.SemiLogY].Add(new PointM(x, log10Y));
				c++;
			}
			if (blnDontExcludeZero || (c == 2)) {
				lstarAll[(int)MathematicalModel.LogLog].Add(new PointM(log10X, log10Y));
			}
		}

		private static RegressionModel<decimal> FinishCompute(IList<PointM>[] allModelPoints)
		{
			var rmarAll = new RegressionModel<decimal>[4];
			for (var i = 0; i < 4; i++) {
				rmarAll[i] = LinearRegression.Compute(allModelPoints[i]).ChangeModel((MathematicalModel)i);
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
