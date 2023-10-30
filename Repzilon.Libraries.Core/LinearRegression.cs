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
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	[StructLayout(LayoutKind.Auto)]
	public struct PointD : IEquatable<PointD>
	{
		public double X;
		public double Y;

		public PointD(double x, double y)
		{
			X = x;
			Y = y;
		}

		public override bool Equals(object obj)
		{
			return obj is PointD && Equals((PointD)obj);
		}

		public bool Equals(PointD other)
		{
			return X == other.X && Y == other.Y;
		}

		public override int GetHashCode()
		{
			int hashCode = 1861411795;
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			return hashCode;
		}

		public override string ToString()
		{
			var stbCoord = new StringBuilder();
			stbCoord.Append('{').Append(this.X).Append("; ").Append(this.Y).Append('}');
			return stbCoord.ToString();
		}

		public static bool operator ==(PointD left, PointD right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PointD left, PointD right)
		{
			return !(left == right);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	public struct LinearRegressionResult
	{
		public int Count;
		public double Slope;
		public double Intercept;
		public double Correlation;
		public double StdDevOfY;
		public double StdDevOfX;
		public double AverageX;
		public double AverageY;

		public override string ToString()
		{
			var stbFormula = new StringBuilder();
			stbFormula.Append("y = ").Append(this.Intercept).Append(" + ").Append(this.Slope).Append("x");
			return stbFormula.ToString();
		}

		public double ExtrapolateY(double x)
		{
			return this.Intercept + (x * this.Slope);
		}

		public double ExtrapolateX(double y)
		{
			return (y - this.Intercept) / this.Slope;
		}

		public double TotalError(double x)
		{
			return RoundOff.Error(ExtrapolateY(x) - x);
		}

		public double RelativeBias(double x)
		{
			return RoundOff.Error((this.Slope - 1) * x);
		}

		public double TotalVariation()
		{
			var sy = this.StdDevOfY;
			return RoundOff.Error((this.Count - 1) * sy * sy);
		}

		public double ExplainedVariation()
		{
			var r = this.Correlation;
			return r * r * this.TotalVariation();
		}

		public double UnexplainedVariation()
		{
			var r = this.Correlation;
			return (1 - (r * r)) * this.TotalVariation();
		}

		public double Determination()
		{
			var r = this.Correlation;
			return r * r;
		}

		public double ResidualStdDev()
		{
			var n = this.Count;
			var r = this.Correlation;
			var sy = this.StdDevOfY;
			return Math.Sqrt((1.0 / (n - 2)) * (1 - (r * r)) * (n - 1) * sy * sy);
		}

		public double SlopeStdDev()
		{
			var sx = this.StdDevOfX;
			return this.ResidualStdDev() / Math.Sqrt((this.Count - 1) * sx * sx);
		}

		public double InterceptStdDev()
		{
			var n = this.Count;
			var x_ = this.AverageX;
			var sx = this.StdDevOfX;
			return this.ResidualStdDev() * Math.Sqrt((1.0 / n) + (x_ * x_ / ((n - 1) * sx * sx)));
		}

		public double YExtrapolationConfidenceFactor(double x0, bool repeated)
		{
			var n = this.Count;
			var diff = x0 - this.AverageX;
			var sx = this.StdDevOfX;
			double f = repeated ? 0 : 1;
			return Math.Sqrt(f + (1.0 / n) + (diff * diff / ((n - 1) * sx * sx)));
		}

		public double StdDevForYc(double yc, int k)
		{
			var diff = yc - this.AverageY;
			var b = this.Slope;
			var n = this.Count;
			var sx = this.StdDevOfX;
			return this.ResidualStdDev() / b * Math.Sqrt((1.0 / k) + (1.0 / n) + (diff * diff / ((n - 1) * b * b * sx * sx)));

		}
	}

	public static class LinearRegression
	{
		public static LinearRegressionResult Compute(IEnumerable<PointD> points)
		{
			if (points == null) {
				throw new ArgumentNullException("points");
			}
			int n = 0;
			double dblSumX = 0, dblSumY = 0, dblSumXY = 0;
			foreach (var pt in points) {
				n++;
				dblSumX += pt.X;
				dblSumY += pt.Y;
				dblSumXY += pt.X * pt.Y;
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
			return lrp;
		}
	}
}
