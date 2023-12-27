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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	// TODO : Replace Extrapolate method by Interpolate methods which will have bounds checkings
	// TODO : Have something to convert the formula when the regression was used with a non-linear model
	// TODO : Implement ICloneable
	[StructLayout(LayoutKind.Auto)]
	public struct LinearRegressionResult : ILinearRegressionResult<double>,
	IEquatable<LinearRegressionResult>, IEquatable<DecimalLinearRegressionResult>,
	IFormattable
	{
		public int Count;
		public double Slope { get; set; }
		public double Intercept { get; set; }
		public double Correlation { get; set; }
		public double StdDevOfY;
		public double StdDevOfX;
		public double AverageX;
		public double AverageY;

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

		public DecimalLinearRegressionResult ToDecimal()
		{
			var dlrr = new DecimalLinearRegressionResult();
			dlrr.Count = this.Count;
			dlrr.Slope = (decimal)this.Slope;
			dlrr.Intercept = (decimal)this.Intercept;
			dlrr.Correlation = (decimal)this.Correlation;
			dlrr.StdDevOfX = (decimal)this.StdDevOfX;
			dlrr.StdDevOfY = (decimal)this.StdDevOfY;
			dlrr.AverageX = (decimal)this.AverageX;
			dlrr.AverageY = (decimal)this.AverageY;
			return dlrr;
		}

		#region Science-related methods
		public double ExtrapolateY(double x)
		{
			return this.Intercept + (x * this.Slope);
		}

		public double ExtrapolateX(double y)
		{
			return (y - this.Intercept) / this.Slope;
		}
		
		public double Determination()
		{
			var r = this.Correlation;
			return r * r;
		}
		#endregion

		#region Statistics-related methods
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
			return RoundOff.Error(r * r * this.TotalVariation());
		}

		public double UnexplainedVariation()
		{
			var r = this.Correlation;
			return RoundOff.Error((1 - (r * r)) * this.TotalVariation());
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

		public bool Equals(LinearRegressionResult other)
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

		public bool Equals(DecimalLinearRegressionResult other)
		{
			return Count == other.Count &&
				   (decimal)Slope == other.Slope &&
				   (decimal)Intercept == other.Intercept &&
				   (decimal)Correlation == other.Correlation &&
				   (decimal)StdDevOfY == other.StdDevOfY &&
				   (decimal)StdDevOfX == other.StdDevOfX &&
				   (decimal)AverageX == other.AverageX &&
				   (decimal)AverageY == other.AverageY;
		}

		public static bool operator ==(LinearRegressionResult left, LinearRegressionResult right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LinearRegressionResult left, LinearRegressionResult right)
		{
			return !(left == right);
		}

		public static bool operator ==(LinearRegressionResult left, DecimalLinearRegressionResult right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LinearRegressionResult left, DecimalLinearRegressionResult right)
		{
			return !(left == right);
		}
		#endregion
	}
}
