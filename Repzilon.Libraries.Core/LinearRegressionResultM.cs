//
//  LinearRegressionResult.cs
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
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	[StructLayout(LayoutKind.Auto)]
	public struct DecimalLinearRegressionResult : ILinearRegressionResult<decimal>
	{
		public int Count;
		public decimal Slope;
		public decimal Intercept;
		public decimal Correlation;
		public decimal StdDevOfY;
		public decimal StdDevOfX;
		public decimal AverageX;
		public decimal AverageY;

		public decimal GetSlope()
		{
			return this.Slope;
		}

		public decimal GetIntercept()
		{
			return this.Intercept;
		}

		public decimal GetCorrelation()
		{
			return this.Correlation;
		}

		public override string ToString()
		{
			var stbFormula = new StringBuilder();
			stbFormula.Append("y = ").Append(this.Intercept).Append(" + ").Append(this.Slope).Append("x");
			return stbFormula.ToString();
		}

		public decimal ExtrapolateY(decimal x)
		{
			return this.Intercept + (x * this.Slope);
		}

		public decimal ExtrapolateX(decimal y)
		{
			return (y - this.Intercept) / this.Slope;
		}

		public decimal TotalError(decimal x)
		{
			return RoundOff.Error(ExtrapolateY(x) - x); // Eat dirt
		}

		public decimal RelativeBias(decimal x)
		{
			return RoundOff.Error((this.Slope - 1) * x); // Eat dirt
		}

		public decimal TotalVariation()
		{
			var sy = this.StdDevOfY;
			return RoundOff.Error((this.Count - 1) * sy * sy); // Eat dirt
		}

		public decimal ExplainedVariation()
		{
			var r = this.Correlation;
			return r * r * this.TotalVariation();
		}

		public decimal UnexplainedVariation()
		{
			var r = this.Correlation;
			return (1 - (r * r)) * this.TotalVariation();
		}

		public decimal Determination()
		{
			var r = this.Correlation;
			return r * r;
		}

		public decimal ResidualStdDev()
		{
			var n = this.Count;
			var r = this.Correlation;
			var sy = this.StdDevOfY;
			return ExtraMath.Sqrt(1.0m / (n - 2) * (1 - (r * r)) * (n - 1) * sy * sy);
		}

		public decimal SlopeStdDev()
		{
			var sx = this.StdDevOfX;
			return this.ResidualStdDev() / ExtraMath.Sqrt((this.Count - 1) * sx * sx);
		}

		public decimal InterceptStdDev()
		{
			var n = this.Count;
			var x_ = this.AverageX;
			var sx = this.StdDevOfX;
			return this.ResidualStdDev() * ExtraMath.Sqrt((1.0m / n) + (x_ * x_ / ((n - 1) * sx * sx)));
		}

		public decimal YExtrapolationConfidenceFactor(decimal x0, bool repeated)
		{
			var n = this.Count;
			var diff = x0 - this.AverageX;
			var sx = this.StdDevOfX;
			decimal f = repeated ? 0 : 1;
			return ExtraMath.Sqrt(f + (1.0m / n) + (diff * diff / ((n - 1) * sx * sx)));
		}

		public decimal StdDevForYc(decimal yc, int k)
		{
			var diff = yc - this.AverageY;
			var b = this.Slope;
			var n = this.Count;
			var sx = this.StdDevOfX;
			return this.ResidualStdDev() / b * ExtraMath.Sqrt((1.0m / k) + (1.0m / n) + (diff * diff / ((n - 1) * b * b * sx * sx)));
		}
	}
}
