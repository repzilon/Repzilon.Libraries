﻿//
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
	IEquatable<DecimalLinearRegressionResult>, IEquatable<LinearRegressionResult>,
	IFormattable
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

		public LinearRegressionResult ToDouble()
		{
			var lrr = new LinearRegressionResult();
			lrr.Count = this.Count;
			lrr.Slope = (double)this.Slope;
			lrr.Intercept = (double)this.Intercept;
			lrr.Correlation = (double)this.Correlation;
			lrr.StdDevOfX = (double)this.StdDevOfX;
			lrr.StdDevOfY = (double)this.StdDevOfY;
			lrr.AverageX = (double)this.AverageX;
			lrr.AverageY = (double)this.AverageY;
			return lrr;
		}

		#region Science-related methods
		public decimal ExtrapolateY(decimal x)
		{
			return this.Intercept + (x * this.Slope);
		}

		public decimal ExtrapolateX(decimal y)
		{
			return (y - this.Intercept) / this.Slope;
		}

		public decimal Determination()
		{
			var r = this.Correlation;
			return r * r;
		}
		#endregion

		#region Statistics-related methods
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
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			if (obj is LinearRegressionResult) {
				return Equals((LinearRegressionResult)obj);
			} else if (obj is DecimalLinearRegressionResult) {
				return Equals((DecimalLinearRegressionResult)obj);
			} else {
				return false;
			}
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
				int magic = -1521134295;
				int hashCode = 338248910 * -1521134295 + Count;
				hashCode = hashCode * magic + Slope.GetHashCode();
				hashCode = hashCode * magic + Intercept.GetHashCode();
				hashCode = hashCode * magic + Correlation.GetHashCode();
				hashCode = hashCode * magic + StdDevOfY.GetHashCode();
				hashCode = hashCode * magic + StdDevOfX.GetHashCode();
				hashCode = hashCode * magic + AverageX.GetHashCode();
				return hashCode * magic + AverageY.GetHashCode();
			}
		}

		public bool Equals(LinearRegressionResult other)
		{
			return Count == other.Count &&
				   Slope == (decimal)other.Slope &&
				   Intercept == (decimal)other.Intercept &&
				   Correlation == (decimal)other.Correlation &&
				   StdDevOfY == (decimal)other.StdDevOfY &&
				   StdDevOfX == (decimal)other.StdDevOfX &&
				   AverageX == (decimal)other.AverageX &&
				   AverageY == (decimal)other.AverageY;
		}

		public static bool operator ==(DecimalLinearRegressionResult left, DecimalLinearRegressionResult right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DecimalLinearRegressionResult left, DecimalLinearRegressionResult right)
		{
			return !(left == right);
		}

		public static bool operator ==(DecimalLinearRegressionResult left, LinearRegressionResult right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DecimalLinearRegressionResult left, LinearRegressionResult right)
		{
			return !(left == right);
		}
		#endregion
	}
}
