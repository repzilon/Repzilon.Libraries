//
//  LogisticRegression.cs
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
	[Flags]
	public enum LogisticRegressionOptions : byte
	{
		MidwaySecant = 1,
		Linearization = 2,
		Average = MidwaySecant | Linearization,
		StretchToSource = 4
	}

	public static class LogisticRegression
	{
		public static DecimalLogisticRegressionResult Compute(LogisticRegressionOptions options, params PointM[] points)
		{
			if (options == 0) {
				RetroCompat.NewUndefinedEnumException(nameof(options), options);
			}

			var lowTwoBits = options & LogisticRegressionOptions.Average;
			var blnStretch = (options & LogisticRegressionOptions.StretchToSource) != 0;

			DecimalLogisticRegressionResult result = new DecimalLogisticRegressionResult();
			if (lowTwoBits == LogisticRegressionOptions.MidwaySecant) {
				result = MidwaySecant(points);
			} else if (lowTwoBits == LogisticRegressionOptions.Linearization) {
				result = Linearization(points);
			} else {
				result = MidwaySecant(points);
				var result2 = Linearization(points);
				// TODO : recalculate correlation for average
				result = new DecimalLogisticRegressionResult(result.Count, result.Intercept, result.Amplitude,
				 (result.Location + result2.Location) / 2, (result.Scale + result2.Scale) / 2,
				 (result.Correlation + result2.Correlation) / 2,
				 result.MinX, result.MaxX, result.MinY, result2.MaxY);
			}

			if (blnStretch) {
				var p0 = points[0];
				var pn = points[points.Length - 1];
				var lrrSecant = LinearRegression.Compute(
				 new PointM(result.InterpolateY(p0.X), p0.Y), new PointM(result.InterpolateY(pn.X), pn.Y));
				result = new DecimalLogisticRegressionResult(result, lrrSecant);
				// TODO : recalculate correlation after stretching
			}

			return result;
		}

		private static DecimalLogisticRegressionResult MidwaySecant(params PointM[] points)
		{
			var iLast     = points.Length - 1;
			var last      = points[iLast].Y;
			var intercept = Math.Min(last, points[0].Y);
			var amplitude = Math.Abs(last - points[0].Y);
			// Find the point which is the observed middle
			var yMid   = 0.5m * (last + points[0].Y);
			var iMid   = points.Length / 2;
			var ixyMid = -1;
			int k;
			last = Decimal.MaxValue;
			for (k = iMid - 1; k <= iMid + 1; k++) {
				var diff = Math.Abs(points[k].Y - yMid);
				if (diff < last) {
					last   = diff;
					ixyMid = k;
				}
			}

			var lrrSecant = LinearRegression.Compute(points[ixyMid - 1], points[ixyMid], points[ixyMid + 1]);
			var location  = lrrSecant.InterpolateX(yMid);
			var lrr       = LinearRegression.Compute(points);
			last = lrr.Slope;
			// The scale parameter is the hardest to adjust
			var scale = Math.Sign(last) * Math.Max(lrrSecant.Slope, last) / Math.Min(lrrSecant.Slope, last);

			return new DecimalLogisticRegressionResult(points.Length, intercept, amplitude, location, scale,
			 lrr.Correlation, points[0].X, points[iLast].X, Math.Min(points[0].Y, last), Math.Max(points[0].Y, last));
		}

		private static DecimalLogisticRegressionResult Linearization(params PointM[] points)
		{
			var iLast = points.Length - 1;
			var last  = points[iLast].Y;
			// A. Find intercept and amplitude
			var intercept = Math.Min(last, points[0].Y);
			var amplitude = Math.Abs(last - points[0].Y);
			// B. Normalize by removing effects of intercept and amplitude
			var lstNorm = new List<PointM>(points.Length);
			int i;
			for (i = 0; i < points.Length; i++) {
				lstNorm.Add(new PointM(points[i].X, (points[i].Y - intercept) / amplitude));
			}
			// C. Linearize the logistic model to be able to perform a linear regression next
			// From		y = c + a*[1/(1+e^((x-n)/-s)] where c: intercept, a: amplitude, n: location, s: scale
			// To		x = [-s * ln((1-y)/y)] - n
			// Mapped	Y' = A'*X' + B'	where Y'=x  A'=-s  X'=ln((1-y)/y)  B'=n
			lstNorm.RemoveAll(ZeroOrOneY);
			for (i = 0; i < lstNorm.Count; i++) {
				var y = lstNorm[i].Y;
				lstNorm[i] = new PointM(ExtraMath.Ln((1 - y) / y), lstNorm[i].X);
			}
			var lrr = LinearRegression.Compute(lstNorm);

			return new DecimalLogisticRegressionResult(lrr.Count, intercept, amplitude, lrr.Intercept, -lrr.Slope,
			 -lrr.Correlation, points[0].X, points[iLast].X, Math.Min(points[0].Y, last), Math.Max(points[0].Y, last));
		}

		private static bool ZeroOrOneY(PointM obj)
		{
			var y = obj.Y;
			return (y == 0) || (y == 1);
		}
	}
}
