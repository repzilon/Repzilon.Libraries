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

			DecimalLogisticRegressionResult result;
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

		#region Area between observed trapezes and modelized curve
		// ReSharper disable once ConvertToConstant.Local
		private static readonly decimal TargetIntersectDelta = 1e-19m;

#if NET20
		public static decimal AreaBetween(DecimalLogisticRegressionResult logistic, params PointM[] points)
#else
		public static decimal AreaBetween(this DecimalLogisticRegressionResult logistic, params PointM[] points)
#endif
		{
			decimal areaBetween = 0;
			for (var j = 0; j < points.Length - 1; j++) {
				var secantReal = LinearRegression.Compute(points[j], points[j + 1]);
				var lerpPoints = new PointM[] { LerpPoint(logistic, points[j]), LerpPoint(logistic, points[j + 1]) };
				var secantLine = LinearRegression.Compute(lerpPoints);
				decimal? crossing = null;
				decimal? crossing2 = null;
				var x = points[j].X;

				if (secantLine.Slope != secantReal.Slope) { // They could cross
					crossing = (secantLine.Intercept - secantReal.Intercept) / (secantReal.Slope - secantLine.Slope);
					if ((crossing >= x) && (crossing <= points[j + 1].X)) { // secant crosses
						// Find the Newton crossing using the secant crossing as first estimate
						crossing = FindBoundCrossing(crossing.Value, secantReal, logistic, points, j);
						if (crossing == null) {
							crossing = FindBoundCrossing(0.5m * (x + points[j + 1].X), secantReal, logistic, points, j);
						}
					} else {
						// Using only the secant of the segment may miss a concave curve inside the segment
						var w = points[j + 1].X - x;
						crossing  = FindBoundCrossing(x + 0.25m * w, secantReal, logistic, points, j);
						crossing2 = FindBoundCrossing(x + 0.75m * w, secantReal, logistic, points, j);
						if (crossing2.HasValue && (RoundOff.Error(crossing2.Value) == points[j + 1].X)) {
							crossing2 = null;
						}
					}
				}

#if NETFRAMEWORK
				Converter<decimal, decimal> funcFirst, funcSecond;
#else
				Func<decimal, decimal> funcFirst, funcSecond;
#endif
				if (points[j].Y > lerpPoints[0].Y) {
					funcFirst  = secantReal.Primitive;
					funcSecond = logistic.Primitive;
				} else {
					funcFirst  = logistic.Primitive;
					funcSecond = secantReal.Primitive;
				}

				if (crossing2.HasValue) {
					try {
						areaBetween += AreaBetweenCrosses(x, points[j + 1].X, crossing.Value, crossing2.Value, funcFirst, funcSecond);
					} catch (OverflowException) {
						if (RoundOff.Error(crossing.Value) == x) {
							areaBetween += AreaBetweenCrosses(x, points[j + 1].X, crossing2.Value, funcSecond, funcFirst);
						} else {
							throw;
						}
					}
				} else if (crossing.HasValue) {
					try {
						areaBetween += AreaBetweenCrosses(x, points[j + 1].X, crossing.Value, funcFirst, funcSecond);
					} catch (OverflowException) {
						if (crossing.Value == x) {
							areaBetween += AreaBetweenCrosses(x, points[j + 1].X, crossing.Value, funcSecond, funcFirst);
						} else {
							throw;
						}
					}
				} else {
					areaBetween += DifferenceOfPrimitives(x, points[j + 1].X, funcFirst, funcSecond);
				}
			}

			return areaBetween;
		}

		private static decimal? FindBoundCrossing(decimal candidate, DecimalLinearRegressionResult observedSecant,
		DecimalLogisticRegressionResult logisticModel, PointM[] points, int j)
		{
			try {
				var found = BoundCrossing(FindCrossing(candidate, observedSecant, logisticModel), points, j);
				if (found.HasValue) {
					var val = found.Value;
					var fx  = observedSecant.InterpolateY(val);
					var gx  = logisticModel.InterpolateY(val);
					if (Math.Abs(fx - gx) > TargetIntersectDelta) {
						found = null;
					}
				}
				return found;
			} catch (OverflowException) {
				return null;
			}
		}

		private static decimal? BoundCrossing(decimal? crossing, PointM[] points, int j)
		{
			if ((crossing < points[j].X) || (crossing > points[j + 1].X)) {
				crossing = null;
			}
			return crossing;
		}

		private static decimal FindCrossing(decimal candidate, DecimalLinearRegressionResult observedSecant,
		DecimalLogisticRegressionResult logisticModel)
		{
			return Differential.NewtonCrossing(candidate, TargetIntersectDelta, observedSecant.ExtrapolateY,
			 logisticModel.ExtrapolateY, observedSecant.Derivative, logisticModel.Derivative);
		}

#if NET20
		private static PointM LerpPoint(DecimalLogisticRegressionResult logistic, PointM forX)
#else
		private static PointM LerpPoint(this DecimalLogisticRegressionResult logistic, PointM forX)
#endif
		{
			var x = forX.X;
			return new PointM(x, logistic.InterpolateY(x));
		}

		private static decimal DifferenceOfPrimitives(decimal a, decimal b,
#if NETFRAMEWORK
		Converter<decimal, decimal> top, Converter<decimal, decimal> bottom)
#else
		Func<decimal, decimal> top, Func<decimal, decimal> bottom)
#endif
		{
			var area = Integral.DifferenceOfPrimitives(a, b, top) - Integral.DifferenceOfPrimitives(a, b, bottom);
			if (area < 0) {
				throw new OverflowException(String.Format("Underflow, area for [{0};{1}] is {2}", a, b, area));
			}
			return area;
		}

		private static decimal AreaBetweenCrosses(decimal a, decimal b, decimal crossing,
#if NETFRAMEWORK
		Converter<decimal, decimal> firstTop, Converter<decimal, decimal> secondTopOrFirstBottom)
#else
		Func<decimal, decimal> firstTop, Func<decimal, decimal> secondTopOrFirstBottom)
#endif
		{
			return DifferenceOfPrimitives(a, crossing, firstTop, secondTopOrFirstBottom) +
				   DifferenceOfPrimitives(crossing, b, secondTopOrFirstBottom, firstTop);
		}

		private static decimal AreaBetweenCrosses(decimal a, decimal b, decimal crossing1, decimal crossing2,
#if NETFRAMEWORK
		Converter<decimal, decimal> firstTop, Converter<decimal, decimal> secondTopOrFirstBottom)
#else
		Func<decimal, decimal> firstTop, Func<decimal, decimal> secondTopOrFirstBottom)
#endif
		{
			return DifferenceOfPrimitives(a, crossing1, firstTop, secondTopOrFirstBottom) +
				   DifferenceOfPrimitives(crossing1, crossing2, secondTopOrFirstBottom, firstTop) +
				   DifferenceOfPrimitives(crossing2, b, firstTop, secondTopOrFirstBottom);
		}
		#endregion

#if NET20
		public static KeyValuePair<PointM, decimal> MaxProductivity(DecimalLogisticRegressionResult model, decimal x0)
#else
		public static KeyValuePair<PointM, decimal> MaxProductivity(this DecimalLogisticRegressionResult model, decimal x0)
#endif
		{
			// Productivity P = (Xm - X0) / (tm - t0) where Xm = f(m) [our model at time m], t0 = 0, tm = m
			// becomes P = (f(m) - X0) / m. The maximum productivity is a local maximum, and occurs when the
			// derivative of P changes its sign. So we are going to solve 0 = d/dx[(f(m) - X0) / m]
			var tm = Differential.NewtonCrossing(model.Location * ExtraMath.E * 0.5m, TargetIntersectDelta,
				model.ProductivityFirstDerivative, 0, model.ProductivitySecondDerivative);
			var xm = model.InterpolateY(tm);
			return new KeyValuePair<PointM, decimal>(new PointM(tm, xm), (xm - x0) / tm);
		}
	}
}
