//
//  Enzyme.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024 René Rhéaume
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

		private static readonly double TwoOnLn10 = 2.0 / Math.Log(10);
		private static readonly double FourOnLn10 = 4.0 / Math.Log(10);

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
				vmax  = slope * FourOnLn10;
				km    = Math.Pow(10, TwoOnLn10 - (rm.B / slope));
				goto returnKinematic; // FIXME : Replace goto without duplicating EnzymeKinematic ctor call
			} else if (rm.Model != MathematicalModel.Affine) {
				throw new NotSupportedException(NonLinearUnsupported);
			}

			slope = rm.B;
			var intercept = rm.A;
			if (representation == EnzymeSpeedRepresentation.EadieHofstee) {
				vmax = intercept;
				km   = -1 * slope;
			} else if (representation == EnzymeSpeedRepresentation.LineweaverBurk) {
				vmax = 1.0 / intercept;
				km   = vmax * slope;
			} else if (representation == EnzymeSpeedRepresentation.MichaelisMenten) {
				throw new NotSupportedException(LinearMichaelisMenten);
			} else if (representation == EnzymeSpeedRepresentation.HanesWoolf) {
				vmax = 1.0 / slope;
				km   = vmax * intercept;
			} else {
				throw RetroCompat.NewUndefinedEnumException("representation", representation);
			}

		returnKinematic:
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
				vmax  = slope * (decimal)FourOnLn10;
				km    = (decimal)Math.Pow(10, TwoOnLn10 - (double)(rm.B / slope));
				goto returnDecimalKinematic; // FIXME : Replace goto without duplicating EnzymeKinematic ctor call
			} else if (rm.Model != MathematicalModel.Affine) {
				throw new NotSupportedException(NonLinearUnsupported);
			}

			slope = rm.B;
			var intercept = rm.A;
			if (representation == EnzymeSpeedRepresentation.EadieHofstee) {
				vmax = intercept;
				km   = -1 * slope;
			} else if (representation == EnzymeSpeedRepresentation.LineweaverBurk) {
				vmax = 1.0m / intercept;
				km   = vmax * slope;
			} else if (representation == EnzymeSpeedRepresentation.MichaelisMenten) {
				throw new NotSupportedException(LinearMichaelisMenten);
			} else if (representation == EnzymeSpeedRepresentation.HanesWoolf) {
				vmax = 1.0m / slope;
				km   = vmax * intercept;
			} else {
				throw RetroCompat.NewUndefinedEnumException("representation", representation);
			}

		returnDecimalKinematic:
			return new EnzymeKinematic<decimal>(vmax, speedUnit, km, concentrationUnit, rm.R, representation);
		}

		public static EnzymeKinematic<double> Speed(string concentrationUnit, string speedUnit,
		params PointD[] michaelisMentenDataPoints)
		{
			int    i;
			var    c = michaelisMentenDataPoints.Length;
			double s, v0;
			var    ptdMatrix = new PointD[4][];

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
			s = 0;  // best absolute correlation
			c = -1; // index of regression model having the best correlation
			for (i = 0; i < 4; i++) {
				var model = rmdarAll[i].Model;
				if ((model == MathematicalModel.Affine) || ((i == 0) && (model == MathematicalModel.Logarithmic))) {
					v0 = Math.Abs(rmdarAll[i].R); // absolute correlation at current index
					if (v0 > s) {
						s = v0;
						c = i;
					}
				}
			}

			if (c >= 0) {
				return Speed(concentrationUnit, speedUnit, (EnzymeSpeedRepresentation)c, rmdarAll[c]);
			} else {
				throw new Exception("Unable to transform to a linearized model for computing enzyme kinematics.");
			}
		}

		public static EnzymeKinematic<double> DirectLinearPlot(string concentrationUnit, string speedUnit,
		params PointD[] michaelisMentenDataPoints)
		{
			int i;
			int k = 0;
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
						s = RoundOff.Error((michaelisMentenDataPoints[j].Y - pt.Y) / (dblarSlopes[i] - dblarSlopes[j]));
						ptdarIntersections[k] = new PointD(s, RoundOff.Error((dblarSlopes[i] * s) + pt.Y));
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
	}
}
