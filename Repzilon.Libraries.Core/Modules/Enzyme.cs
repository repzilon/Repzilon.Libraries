//
//  Enzyme.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024-2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using Repzilon.Libraries.Core.Regression;

namespace Repzilon.Libraries.Core.Biochemistry
{
	public static class Enzyme
	{
		private const string LinearMichaelisMenten = "The Speed method cannot solve a linear Michaelis-Menten model.";
		private const string NonLinearUnsupported = "The Speed method is unable to solve non-linear equations.";

		private static readonly double TwoOnLn10;
		private static readonly double FourOnLn10;

#pragma warning disable S3963 // "static" fields should be initialized inline
		static Enzyme()
		{
#pragma warning disable U2U1000
#pragma warning disable CC0105 // You should use 'var' whenever possible.
			// ReSharper disable ConvertToConstant.Local
			// ReSharper disable SuggestVarOrType_BuiltInTypes
			/*const*/ byte kTen = 10;
			/*const*/ float kTwo = 2.0f;
			// ReSharper restore SuggestVarOrType_BuiltInTypes
			// ReSharper restore ConvertToConstant.Local
#pragma warning restore CC0105 // You should use 'var' whenever possible.
#pragma warning restore U2U1000
			var ln10 = Math.Log(kTen);
			TwoOnLn10 = kTwo / ln10;
			FourOnLn10 = kTwo * kTwo / ln10;
		}
#pragma warning restore S3963 // "static" fields should be initialized inline

		#region Speed method
		public static EnzymeKinematic<double> Speed(string concentrationUnit, string speedUnit,
		EnzymeSpeedRepresentation representation, params PointD[] dataPoints)
		{
			return Speed(concentrationUnit, speedUnit, representation, RegressionModel.Compute(dataPoints));
		}

		private static EnzymeKinematic<double> Speed(string concentrationUnit, string speedUnit,
		EnzymeSpeedRepresentation representation, RegressionModel<double> rm)
		{
			double slope, vmax, km;
			if ((representation == EnzymeSpeedRepresentation.MichaelisMenten) &&
			(rm.Model == MathematicalModel.Logarithmic)) {
				slope = rm.A;
				vmax = slope * FourOnLn10;
				km = Math.Pow(10, TwoOnLn10 - (rm.B / slope));
			} else {
				if (rm.Model != MathematicalModel.Affine) {
					throw new NotSupportedException(NonLinearUnsupported);
				}

				slope = rm.B;
				var intercept = rm.A;
				if (representation == EnzymeSpeedRepresentation.EadieHofstee) {
					vmax = intercept;
					km = -1 * slope;
				} else if (representation == EnzymeSpeedRepresentation.LineweaverBurk) {
					vmax = 1.0 / intercept;
					km = vmax * slope;
				} else if (representation == EnzymeSpeedRepresentation.MichaelisMenten) {
					throw new NotSupportedException(LinearMichaelisMenten);
				} else if (representation == EnzymeSpeedRepresentation.HanesWoolf) {
					vmax = 1.0 / slope;
					km = vmax * intercept;
				} else {
					throw RetroCompat.NewUndefinedEnumException(nameof(representation), representation);
				}
			}

			return new EnzymeKinematic<double>(vmax, speedUnit, km, concentrationUnit, rm.R, representation);
		}

		public static EnzymeKinematic<decimal> Speed(string concentrationUnit, string speedUnit,
		EnzymeSpeedRepresentation representation, params PointM[] dataPoints)
		{
			var rm = RegressionModel.Compute(dataPoints);
			decimal slope, vmax, km;
			if ((representation == EnzymeSpeedRepresentation.MichaelisMenten) &&
			(rm.Model == MathematicalModel.Logarithmic)) {
				slope = rm.A;
				vmax = slope * (decimal)FourOnLn10;
				km = (decimal)Math.Pow(10, TwoOnLn10 - (double)(rm.B / slope));
			} else {
				if (rm.Model != MathematicalModel.Affine) {
					throw new NotSupportedException(NonLinearUnsupported);
				}

				slope = rm.B;
				var intercept = rm.A;
				if (representation == EnzymeSpeedRepresentation.EadieHofstee) {
					vmax = intercept;
					km = -1 * slope;
				} else if (representation == EnzymeSpeedRepresentation.LineweaverBurk) {
					vmax = 1 / intercept;
					km = vmax * slope;
				} else if (representation == EnzymeSpeedRepresentation.MichaelisMenten) {
					throw new NotSupportedException(LinearMichaelisMenten);
				} else if (representation == EnzymeSpeedRepresentation.HanesWoolf) {
					vmax = 1 / slope;
					km = vmax * intercept;
				} else {
					throw RetroCompat.NewUndefinedEnumException(nameof(representation), representation);
				}
			}
			return new EnzymeKinematic<decimal>(vmax, speedUnit, km, concentrationUnit, rm.R, representation);
		}

		public static EnzymeKinematic<double> Speed(string concentrationUnit, string speedUnit,
		params PointD[] michaelisMentenDataPoints)
		{
			int    i;
			double s, v0;
			var    ptdMatrix = new PointD[4][];
			var	   c = michaelisMentenDataPoints.Length;

			for (i = 0; i < 4; i++) {
				ptdMatrix[i] = new PointD[c];
			}

			for (i = 0; i < c; i++) {
				var mmdp = michaelisMentenDataPoints[i];
				s               = mmdp.X;
				v0              = mmdp.Y;
				ptdMatrix[0][i] = mmdp;
				ptdMatrix[1][i] = new PointD(1.0 / s, 1.0 / v0);
				ptdMatrix[2][i] = new PointD(v0 / s, v0);
				ptdMatrix[3][i] = new PointD(s, s / v0);
			}

			var rmdarAll = new RegressionModel<double>[4];
			for (i = 0; i < 4; i++) {
				rmdarAll[i] = RegressionModel.Compute(ptdMatrix[i]);
			}

			// Meaning for s, c and v0 change here
			s = Double.MaxValue;  // smallest area between curves
			c = -1; // index of regression model having the best correlation
			EnzymeKinematic<double> ek;
			var ekBest = new EnzymeKinematic<double>(); // empty one
			var rmdMichMen = rmdarAll[0];
			for (i = 0; i < 4; i++) {
				var model = rmdarAll[i].Model;
				if (((i != 0) && (model == MathematicalModel.Affine)) || ((i == 0) && (model == MathematicalModel.Logarithmic))) {
					ek = Speed(concentrationUnit, speedUnit, (EnzymeSpeedRepresentation)i, rmdarAll[i]);
					v0 = EnzymeKinematicExtension.AreaBetween(ek, rmdMichMen, false);
					if (v0 < s) {
						s = v0;
						c = i;
						ekBest = ek;
					}
				}
			}

			ek = DirectLinearPlot(concentrationUnit, speedUnit, michaelisMentenDataPoints);
			v0 = EnzymeKinematicExtension.AreaBetween(ek, rmdMichMen, false);
			return (c >= 0) && (v0 >= 0) ? ekBest : ek;
		}

		public static EnzymeKinematic<double> DirectLinearPlot(string concentrationUnit, string speedUnit,
		params PointD[] michaelisMentenDataPoints)
		{
			int i;
			var k = 0;
			var c = michaelisMentenDataPoints.Length;
			var ptdarIntersections = new PointD[checked(c * (c - 1) / 2)];
			// 1. Find the parameter space equations
			var dblarSlopes = new double[c];
			PointD pt;
			for (i = 0; i < c; i++) {
				pt = michaelisMentenDataPoints[i];
				dblarSlopes[i] = pt.Y / pt.X;
			}

			// 2. Find the intersection of all equation pairs
			double s;
			for (i = 0; i < c; i++) {
				for (var j = 0; j < c; j++) {
					if (j > i) {
						pt = michaelisMentenDataPoints[i];
						var m = dblarSlopes[i];
						s = RoundOff.Error((michaelisMentenDataPoints[j].Y - pt.Y) / (m - dblarSlopes[j]));
						ptdarIntersections[k] = new PointD(s, RoundOff.Error((m * s) + pt.Y));
						k++;
					}
				}
			}

			// 4. Do a median to get Km and Vmax
			Array.Sort(ptdarIntersections, OrderByX);
			pt = ptdarIntersections[k / 2];
			s  = k % 2 == 1 ? pt.X : 0.5 * (ptdarIntersections[(k / 2) - 1].X + pt.X);
			Array.Sort(ptdarIntersections, OrderByY);
			pt = ptdarIntersections[k / 2];

			// 5. Return value
			return new EnzymeKinematic<double>(k % 2 == 1 ? pt.Y : 0.5 * (ptdarIntersections[(k / 2) - 1].Y + pt.Y),
			 speedUnit, s /* cannot inline because of the 2nd sorting side effect */, concentrationUnit,
			 LinearRegression.Compute(ptdarIntersections).Correlation, EnzymeSpeedRepresentation.DirectLinearMedian);
		}

		private static int OrderByX(PointD a, PointD b)
		{
			return Math.Sign(a.X - b.X);
		}

		private static int OrderByY(PointD a, PointD b)
		{
			return Math.Sign(a.Y - b.Y);
		}
		#endregion

		#region Compare method
		public static Inhibition<double> Compare(EnzymeKinematic<double> original,
		EnzymeKinematic<double> inhibited)
		{
			return Compare(original, inhibited, Double.NaN);
		}

		public static Inhibition<double> Compare(EnzymeKinematic<double> original,
		EnzymeKinematic<double> inhibited, double inhibitorConcentration)
		{
			var intVmaxDiff = ApproximateRelativeDifferenceSign(original.Vmax.Key, inhibited.Vmax.Key);
			var intKmDiff = ApproximateRelativeDifferenceSign(original.Km.Key, inhibited.Km.Key);
			double dblKi = 0;

			if ((intVmaxDiff == 0) && (intKmDiff == 0)) {
				return new Inhibition<double>(InhibitionKind.Absent, dblKi, "");
			} else if ((intKmDiff > 0) && (intVmaxDiff == 0)) {
				if (inhibitorConcentration > 0) {
					dblKi = inhibitorConcentration / ((inhibited.Km.Key / original.Km.Key) - 1);
				}
				return new Inhibition<double>(InhibitionKind.Competitive, dblKi, inhibited.Km.Value);
			} else if ((intVmaxDiff < 0) && (intKmDiff < 0)) {
				return new Inhibition<double>(InhibitionKind.Uncompetitive, dblKi, "");
			} else if ((intKmDiff == 0) && (intVmaxDiff < 0)) {
				if (inhibitorConcentration > 0) {
					dblKi = (inhibited.Vmax.Key * inhibitorConcentration) / (original.Vmax.Key - inhibited.Vmax.Key);
				}
				return new Inhibition<double>(InhibitionKind.NonCompetitive, dblKi, inhibited.Vmax.Value);
			} else if ((intKmDiff != 0) && (intVmaxDiff < 0)) {
				return new Inhibition<double>(InhibitionKind.Mixed, dblKi, "");
			} else {
				throw new NotSupportedException();
			}
		}

		public static Inhibition<decimal> Compare(EnzymeKinematic<decimal> original,
		EnzymeKinematic<decimal> inhibited)
		{
			return Compare(original, inhibited, 0);
		}

		public static Inhibition<decimal> Compare(EnzymeKinematic<decimal> original,
		EnzymeKinematic<decimal> inhibited, decimal inhibitorConcentration)
		{
			var intVmaxDiff = ApproximateRelativeDifferenceSign(original.Vmax.Key, inhibited.Vmax.Key);
			var intKmDiff = ApproximateRelativeDifferenceSign(original.Km.Key, inhibited.Km.Key);
			decimal dcmKi = 0;

			if ((intVmaxDiff == 0) && (intKmDiff == 0)) {
				return new Inhibition<decimal>(InhibitionKind.Absent, dcmKi, "");
			} else if ((intKmDiff > 0) && (intVmaxDiff == 0)) {
				if (inhibitorConcentration > 0) {
					dcmKi = inhibitorConcentration / ((inhibited.Km.Key / original.Km.Key) - 1);
				}
				return new Inhibition<decimal>(InhibitionKind.Competitive, dcmKi, inhibited.Km.Value);
			} else if ((intVmaxDiff < 0) && (intKmDiff < 0)) {
				return new Inhibition<decimal>(InhibitionKind.Uncompetitive, dcmKi, "");
			} else if ((intKmDiff == 0) && (intVmaxDiff < 0)) {
				if (inhibitorConcentration > 0) {
					dcmKi = (inhibited.Vmax.Key * inhibitorConcentration) / (original.Vmax.Key - inhibited.Vmax.Key);
				}
				return new Inhibition<decimal>(InhibitionKind.NonCompetitive, dcmKi, inhibited.Vmax.Value);
			} else if ((intKmDiff != 0) && (intVmaxDiff < 0)) {
				return new Inhibition<decimal>(InhibitionKind.Mixed, dcmKi, "");
			} else {
				throw new NotSupportedException();
			}
		}

		private static int ApproximateRelativeDifferenceSign(double original, double inhibited)
		{
			// FIXME : Have something less hardcoded than -8 and 8
			var dblRelativeDiff = RoundOff.Error(100 * (inhibited - original) / original);
			return (dblRelativeDiff > -8) && (dblRelativeDiff < 8) ? 0 : Math.Sign(dblRelativeDiff);
		}

		private static int ApproximateRelativeDifferenceSign(decimal original, decimal inhibited)
		{
			var dcmRelativeDiff = RoundOff.Error(100 * (inhibited - original) / original);
			return (dcmRelativeDiff > -8) && (dcmRelativeDiff < 8) ? 0 : Math.Sign(dcmRelativeDiff);
		}
		#endregion
	}
}
