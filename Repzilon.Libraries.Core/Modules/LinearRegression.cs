//
//  LinearRegression.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2025 René Rhéaume
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
	public static class LinearRegression
	{
		internal static readonly double OneOfLn10;

#pragma warning disable S3963 // "static" fields should be initialized inline
		static LinearRegression()
		{
#pragma warning disable U2U1000
#pragma warning disable CC0105 // You should use 'var' whenever possible.
			// ReSharper disable ConvertToConstant.Local
			// ReSharper disable SuggestVarOrType_BuiltInTypes
			/*const*/ byte kTen = 10;
			/*const*/ float kOne = 1.0f;
			// ReSharper restore SuggestVarOrType_BuiltInTypes
			// ReSharper restore ConvertToConstant.Local
#pragma warning restore CC0105 // You should use 'var' whenever possible.
#pragma warning restore U2U1000
			OneOfLn10 = kOne / Math.Log(kTen);
		}
#pragma warning restore S3963 // "static" fields should be initialized inline

		public static LinearRegressionResult Compute(params PointD[] points)
		{
			return Compute(points as IList<PointD>);
		}

		public static LinearRegressionResult Compute(IEnumerable<PointD> points)
		{
			if (points == null) {
				throw new ArgumentNullException(nameof(points));
			}

			var n = 0;

			double dblAverageX = 0, dblSumXy = dblAverageX;
			var dblMinX = Double.MaxValue;
			var dblAverageY = dblAverageX;
			var dblStdDevX = dblAverageX;
			var dblStdDevY = dblAverageX;

			var dblMinY = dblMinX;
			var dblMaxX = -dblMinX;
			var dblMaxY = dblMaxX;

			foreach (var pt in points) {
				n++;
				var x = pt.X;
				var y = pt.Y;
				Aggregate(x, ref dblAverageX, ref dblStdDevX, ref dblMinX, ref dblMaxX, n);
				Aggregate(y, ref dblAverageY, ref dblStdDevY, ref dblMinY, ref dblMaxY, n);
				dblSumXy += x * y;
			}
			if (n < 1) {
				throw new ArgumentNullException(nameof(points));
			}
			return FinishCompute(dblStdDevX, dblStdDevY, n, dblAverageX, dblAverageY,
			 dblSumXy, dblMinX, dblMinY, dblMaxX, dblMaxY);
		}

#if NET20 || NET35 || NET40
		public static LinearRegressionResult Compute(IList<PointD> points)
#else
		public static LinearRegressionResult Compute(IReadOnlyList<PointD> points)
#endif
		{
			if ((points == null) || (points.Count < 1)) {
				throw new ArgumentNullException(nameof(points));
			}
			var n = points.Count;
			if (n < 1) {
				throw new ArgumentNullException(nameof(points));
			}

			var n0 = 0;

			double dblAverageX = 0, dblAverageY = dblAverageX, dblSumXy = dblAverageX;
			var dblStdDevX = dblAverageX;
			var dblStdDevY = dblAverageX;

			var dblMinX = Double.MaxValue;
			var dblMinY = dblMinX;
			var dblMaxX = -dblMinX;
			var dblMaxY = dblMaxX;

			for (var i = 0; i < n; i++) {
				var x = points[i].X;
				var y = points[i].Y;
				n0++;
				Aggregate(x, ref dblAverageX, ref dblStdDevX, ref dblMinX, ref dblMaxX, n0);
				Aggregate(y, ref dblAverageY, ref dblStdDevY, ref dblMinY, ref dblMaxY, n0);
				dblSumXy += x * y;
			}
			return FinishCompute(dblStdDevX, dblStdDevY, n, dblAverageX, dblAverageY,
			 dblSumXy, dblMinX, dblMinY, dblMaxX, dblMaxY);
		}

		public static DecimalLinearRegressionResult Compute(params PointM[] points)
		{
			return Compute(points as IList<PointM>);
		}

		public static DecimalLinearRegressionResult Compute(IEnumerable<PointM> points)
		{
			if (points == null) {
				throw new ArgumentNullException(nameof(points));
			}

			var n = 0;

			decimal dcmAverageX = 0, dcmAverageY = dcmAverageX, dcmSumXy = dcmAverageX;
			var dcmStdDevX = dcmAverageX;
			var dcmStdDevY = dcmAverageX;

			var dcmMinX = Decimal.MaxValue;
			var dcmMinY = dcmMinX;
			var dcmMaxX = Decimal.MinValue;
			var dcmMaxY = dcmMaxX;

			foreach (var pt in points) {
				n++;
				var x = pt.X;
				var y = pt.Y;
				Aggregate(x, ref dcmAverageX, ref dcmStdDevX, ref dcmMinX, ref dcmMaxX, n);
				Aggregate(y, ref dcmAverageY, ref dcmStdDevY, ref dcmMinY, ref dcmMaxY, n);
				dcmSumXy += x * y;
			}
			if (n < 1) {
				throw new ArgumentNullException(nameof(points));
			}
			return FinishCompute(dcmStdDevX, dcmStdDevY, n, dcmAverageX, dcmAverageY,
			 dcmSumXy, dcmMinX, dcmMinY, dcmMaxX, dcmMaxY);
		}

#if NET20 || NET35 || NET40
		public static DecimalLinearRegressionResult Compute(IList<PointM> points)
#else
		public static DecimalLinearRegressionResult Compute(IReadOnlyList<PointM> points)
#endif
		{
			if (points == null) {
				throw new ArgumentNullException(nameof(points));
			}
			var n = points.Count;
			if (n < 1) {
				throw new ArgumentNullException(nameof(points));
			}

			var n0 = 0;

			decimal dcmAverageX = 0, dcmAverageY = dcmAverageX, dcmSumXy = dcmAverageX;
			var dcmStdDevX = dcmAverageX;
			var dcmStdDevY = dcmAverageX;

			var dcmMinX = Decimal.MaxValue;
			var dcmMinY = dcmMinX;
			var dcmMaxX = Decimal.MinValue;
			var dcmMaxY = dcmMaxX;

			for (var i = 0; i < n; i++) {
				var x = points[i].X;
				var y = points[i].Y;
				n0++;
				Aggregate(x, ref dcmAverageX, ref dcmStdDevX, ref dcmMinX, ref dcmMaxX, n0);
				Aggregate(y, ref dcmAverageY, ref dcmStdDevY, ref dcmMinY, ref dcmMaxY, n0);
				dcmSumXy += x * y;
			}
			return FinishCompute(dcmStdDevX, dcmStdDevY, n, dcmAverageX, dcmAverageY,
			 dcmSumXy, dcmMinX, dcmMinY, dcmMaxX, dcmMaxY);
		}

		private static void Aggregate(decimal newValue, ref decimal average, ref decimal m2,
		ref decimal minimum, ref decimal maximum, int n)
		{
			var delta = newValue - average;
			average += delta / n;
			var delta2 = newValue - average;
			m2 += delta * delta2;

			minimum = Math.Min(minimum, newValue);
			maximum = Math.Max(maximum, newValue);
		}

		private static void Aggregate(double newValue, ref double average, ref double m2,
		ref double minimum, ref double maximum, int n)
		{
			var delta = newValue - average;
			average += delta / n;
			var delta2 = newValue - average;
			m2 += delta * delta2;

			minimum = Math.Min(minimum, newValue);
			maximum = Math.Max(maximum, newValue);
		}

		private static LinearRegressionResult FinishCompute(double stdDevX, double stdDevY, int n,
		double averageX, double averageY, double sumXy, double minX, double minY, double maxX, double maxY)
		{
			stdDevX = Math.Sqrt(stdDevX / (n - 1));
			stdDevY = Math.Sqrt(stdDevY / (n - 1));
			var b = (sumXy - (n * averageX * averageY)) / ((n - 1) * stdDevX * stdDevX);
			return new LinearRegressionResult(n, RoundOff.Error(averageY - (b * averageX)), RoundOff.Error(b),
			 RoundOff.Error(b * stdDevX / stdDevY), minX, minY, maxX, maxY, averageX,
			 RoundOff.Error(averageY), stdDevX, stdDevY);
		}

		private static DecimalLinearRegressionResult FinishCompute(decimal stdDevX, decimal stdDevY, int n,
		decimal averageX, decimal averageY, decimal sumXy, decimal minX, decimal minY, decimal maxX, decimal maxY)
		{
			stdDevX = ExtraMath.Sqrt(stdDevX / (n - 1));
			stdDevY = ExtraMath.Sqrt(stdDevY / (n - 1));
			var b = (sumXy - (n * averageX * averageY)) / ((n - 1) * stdDevX * stdDevX);
			return new DecimalLinearRegressionResult(n, RoundOff.Error(averageY - (b * averageX)), b,
			 b * stdDevX / stdDevY, minX, minY, maxX, maxY, averageX,
			 averageY, stdDevX, stdDevY);
		}
	}
}
