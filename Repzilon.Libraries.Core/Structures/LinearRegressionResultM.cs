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
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	[StructLayout(LayoutKind.Auto)]
	public struct DecimalLinearRegressionResult : ILinearRegressionResult<decimal>,
	IEquatable<DecimalLinearRegressionResult>, IFormattable
	{
		public int Count;
		public decimal Slope { get; set; }
		public decimal Intercept { get; set; }
		public decimal Correlation { get; set; }
		public decimal StdDevOfY;
		public decimal StdDevOfX;
		public decimal AverageX;
		public decimal AverageY;

		#region ToString
		public override string ToString()
		{
			return this.ToString(null, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrWhiteSpace(format)) {
				format = "G";
			}
			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}
			var stbFormula = new StringBuilder();
			stbFormula.Append("y = ").Append(this.Intercept.ToString(format, formatProvider)).Append(" + ").Append(this.Slope.ToString(format, formatProvider)).Append("x");
			return stbFormula.ToString();
		}
		#endregion

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
			return RoundOff.Error(r * r * this.TotalVariation()); // Eat dirt
		}

		public decimal UnexplainedVariation()
		{
			var r = this.Correlation;
			return RoundOff.Error((1 - (r * r)) * this.TotalVariation()); // Eat dirt
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

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is DecimalLinearRegressionResult && Equals((DecimalLinearRegressionResult)obj);
		}

		public bool Equals(DecimalLinearRegressionResult other)
		{
			return Count == other.Count &&
				   Slope == other.Slope &&
				   Intercept == other.Intercept &&
				   Correlation == other.Correlation &&
				   StdDevOfY == other.StdDevOfY &&
				   StdDevOfX == other.StdDevOfX &&
				   AverageX == other.AverageX &&
				   AverageY == other.AverageY;
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = 338248910;
				hashCode = hashCode * -1521134295 + Count;
				hashCode = hashCode * -1521134295 + Slope.GetHashCode();
				hashCode = hashCode * -1521134295 + Intercept.GetHashCode();
				hashCode = hashCode * -1521134295 + Correlation.GetHashCode();
				hashCode = hashCode * -1521134295 + StdDevOfY.GetHashCode();
				hashCode = hashCode * -1521134295 + StdDevOfX.GetHashCode();
				hashCode = hashCode * -1521134295 + AverageX.GetHashCode();
				hashCode = hashCode * -1521134295 + AverageY.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(DecimalLinearRegressionResult left, DecimalLinearRegressionResult right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DecimalLinearRegressionResult left, DecimalLinearRegressionResult right)
		{
			return !(left == right);
		}
		#endregion
	}
}
