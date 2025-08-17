//
//  LinearRegressionResultM.cs
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core.Regression
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct DecimalLinearRegressionResult : ILinearRegressionResult<decimal>,
	IEquatable<DecimalLinearRegressionResult>, IEquatable<LinearRegressionResult>
	{
		private readonly int m_intCount;
		public decimal Slope { get; private set; }
		public decimal Intercept { get; private set; }
		public decimal Correlation { get; private set; }
		public readonly decimal StdDevOfY;
		public readonly decimal StdDevOfX;
		public readonly decimal AverageX;
		public readonly decimal AverageY;
		public readonly decimal MinX;
		public readonly decimal MaxX;
		public readonly decimal MinY;
		public readonly decimal MaxY;

		internal DecimalLinearRegressionResult(int n, decimal intercept, decimal slope, decimal correlation,
		decimal minX, decimal minY, decimal maxX, decimal maxY, decimal averageX, decimal averageY, decimal stdDevX,
		decimal stdDevY) : this()
		{
			this.m_intCount = n;
			this.Slope = slope;
			this.Intercept = intercept;
			this.Correlation = correlation;
			this.StdDevOfY = stdDevY;
			this.StdDevOfX = stdDevX;
			this.AverageX = averageX;
			this.AverageY = averageY;
			this.MinX = minX;
			this.MaxX = maxX;
			this.MinY = minY;
			this.MaxY = maxY;
		}

		#region ICloneable members
		public DecimalLinearRegressionResult(DecimalLinearRegressionResult other) : this()
		{
			this.m_intCount = other.m_intCount;
			this.Slope = other.Slope;
			this.Intercept = other.Intercept;
			this.Correlation = other.Correlation;
			this.StdDevOfX = other.StdDevOfX;
			this.StdDevOfY = other.StdDevOfY;
			this.AverageX = other.AverageX;
			this.AverageY = other.AverageY;
			this.MinX = other.MinX;
			this.MaxX = other.MaxX;
			this.MinY = other.MinY;
			this.MaxY = other.MaxX;
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif

		public DecimalLinearRegressionResult Clone()
		{
			return new DecimalLinearRegressionResult(this);
		}
		#endregion

		public int Count
        {
        	get { return m_intCount; }
        }

		public LinearRegressionResult ToDouble()
		{
			return new LinearRegressionResult(this.m_intCount, (double)this.Intercept, (double)this.Slope,
			 (double)this.Correlation, (double)this.MinX, (double)this.MinY, (double)this.MaxX, (double)this.MaxY,
			 (double)this.AverageX, (double)this.AverageY, (double)this.StdDevOfX, (double)this.StdDevOfY);
		}

		#region ToString
		public override string ToString()
		{
			return this.ToString(null, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
#if NET35 || NET20
			if (RetroCompat.IsNullOrWhiteSpace(format)) {
#else
			if (String.IsNullOrWhiteSpace(format)) {
#endif
				format = "G";
			}
			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}
			var stbFormula = new StringBuilder();
			stbFormula.Append("y = ").Append(this.Intercept.ToString(format, formatProvider)).Append(" + ")
			 .Append(this.Slope.ToString(format, formatProvider)).Append('x');
			return stbFormula.ToString();
		}
		#endregion

		#region Science-related methods
		public decimal InterpolateY(decimal x)
		{
			var min = this.MinX;
			var max = this.MaxX;
			if ((x < min) || (x > max)) {
				throw new ArgumentOutOfRangeException(nameof(x),
				 String.Format("x is outside the range [{0}; {1}]", min, max));
			}
			return RoundOff.Error(this.Intercept + (x * this.Slope));
		}

		public decimal InterpolateX(decimal y)
		{
			var min = this.MinY;
			var max = this.MaxY;
			if ((y < min) || (y > max)) {
				throw new ArgumentOutOfRangeException(nameof(y),
				 String.Format("y is outside the range [{0}; {1}]", min, max));
			}
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
			return RoundOff.Error(InterpolateY(x) - x); // Eat dirt
		}

		public decimal RelativeBias(decimal x)
		{
			return RoundOff.Error((this.Slope - 1) * x); // Eat dirt
		}

		public decimal TotalVariation()
		{
			var sy = this.StdDevOfY;
			return RoundOff.Error((this.m_intCount - 1) * sy * sy); // Eat dirt
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
			var n = this.m_intCount;
			var r = this.Correlation;
			var sy = this.StdDevOfY;
			return ExtraMath.Sqrt(Decimal.One / (n - 2) * (1 - (r * r)) * (n - 1) * sy * sy);
		}

		public decimal SlopeStdDev()
		{
			var sx = this.StdDevOfX;
			return this.ResidualStdDev() / ExtraMath.Sqrt((this.m_intCount - 1) * sx * sx);
		}

		public decimal InterceptStdDev()
		{
			var n = this.m_intCount;
			// ReSharper disable once InconsistentNaming
			var x_ = this.AverageX;
			var sx = this.StdDevOfX;
			return this.ResidualStdDev() * ExtraMath.Sqrt((Decimal.One / n) + (x_ * x_ / ((n - 1) * sx * sx)));
		}

		public decimal YExtrapolationConfidenceFactor(decimal x0, bool repeated)
		{
			var n = this.m_intCount;
			var diff = x0 - this.AverageX;
			var sx = this.StdDevOfX;
			decimal f = repeated ? 0 : 1;
			return ExtraMath.Sqrt(f + (Decimal.One / n) + (diff * diff / ((n - 1) * sx * sx)));
		}

		public decimal StdDevForYc(decimal yc, int k)
		{
			var diff = yc - this.AverageY;
			var b = this.Slope;
			var n = this.m_intCount;
			var sx = this.StdDevOfX;
			return this.ResidualStdDev() / b *
				   ExtraMath.Sqrt((Decimal.One / k) + (Decimal.One / n) + (diff * diff / ((n - 1) * b * b * sx * sx)));
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
			return m_intCount == other.m_intCount &&
				   Slope == other.Slope &&
				   Intercept == other.Intercept &&
				   Correlation == other.Correlation &&
				   StdDevOfY == other.StdDevOfY &&
				   StdDevOfX == other.StdDevOfX &&
				   AverageX == other.AverageX &&
				   AverageY == other.AverageY &&
				   MinX == other.MinX &&
				   MaxY == other.MaxX &&
				   MinY == other.MinY &&
				   MaxY == other.MaxY;
		}

		public bool Equals(LinearRegressionResult other)
		{
			return m_intCount == other.Count &&
				   Slope == (decimal)other.Slope &&
				   Intercept == (decimal)other.Intercept &&
				   Correlation == (decimal)other.Correlation &&
				   StdDevOfY == (decimal)other.StdDevOfY &&
				   StdDevOfX == (decimal)other.StdDevOfX &&
				   AverageX == (decimal)other.AverageX &&
				   AverageY == (decimal)other.AverageY &&
				   MinX == (decimal)other.MinX &&
				   MaxY == (decimal)other.MaxX &&
				   MinY == (decimal)other.MinY &&
				   MaxY == (decimal)other.MaxY;
		}

		public override int GetHashCode()
		{
			unchecked {
#pragma warning disable U2U1000 // Local variable can be inlined or declared const
#pragma warning disable CC0105 // You should use 'var' whenever possible.
				// ReSharper disable once ConvertToConstant.Local
				// ReSharper disable once SuggestVarOrType_BuiltInTypes
				/*const*/ int magic = -1521134295;
#pragma warning restore CC0105 // You should use 'var' whenever possible.
#pragma warning restore U2U1000 // Local variable can be inlined or declared const
				var hashCode = (338248910 * -1521134295) + m_intCount;
				hashCode = (hashCode * magic) + Slope.GetHashCode();
				hashCode = (hashCode * magic) + Intercept.GetHashCode();
				hashCode = (hashCode * magic) + Correlation.GetHashCode();
				hashCode = (hashCode * magic) + StdDevOfY.GetHashCode();
				hashCode = (hashCode * magic) + StdDevOfX.GetHashCode();
				hashCode = (hashCode * magic) + AverageX.GetHashCode();
				hashCode = (hashCode * magic) + AverageY.GetHashCode();
				hashCode = (hashCode * magic) + MinX.GetHashCode();
				hashCode = (hashCode * magic) + MaxX.GetHashCode();
				hashCode = (hashCode * magic) + MinY.GetHashCode();
				return (hashCode * magic) + MaxY.GetHashCode();
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

		public static bool operator ==(DecimalLinearRegressionResult left, LinearRegressionResult right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DecimalLinearRegressionResult left, LinearRegressionResult right)
		{
			return !(left == right);
		}
		#endregion

		public RegressionModel<decimal> ChangeModel(MathematicalModel newModel)
		{
			return ChangeModel(this.Intercept, this.Slope, this.Correlation, newModel, this.MinX, this.MaxX);
		}

		private static RegressionModel<decimal> ChangeModel(
		decimal a, decimal b, decimal r, MathematicalModel newModel, decimal minX, decimal maxX)
		{
#pragma warning disable RECS0012 // 'if' statement can be re-written as 'switch' statement
#pragma warning disable CC0019   // Use 'switch'
			var na = a;
			var nb = b;
			if ((newModel == MathematicalModel.Power) || (newModel == MathematicalModel.Exponential)) {
				na = (decimal)Math.Pow(10, (double)a);
			}
			if (newModel == MathematicalModel.Exponential) {
				nb = (decimal)Math.Pow(10, (double)b);
			} else if (newModel == MathematicalModel.Logarithmic) { // this is weird
				na = b;
				nb = a;
			} else if ((newModel != MathematicalModel.Affine) && (newModel != MathematicalModel.Power)) {
				throw new ArgumentOutOfRangeException(nameof(newModel));
			}
			if ((newModel == MathematicalModel.Logarithmic) || (newModel == MathematicalModel.Power)) {
				minX = (decimal)RoundOff.Error(Math.Pow(10, (double)minX));
				maxX = (decimal)RoundOff.Error(Math.Pow(10, (double)maxX));
			}
			return new RegressionModel<decimal>(na, nb, r, newModel, minX, maxX);
#pragma warning restore CC0019   // Use 'switch'
#pragma warning restore RECS0012 // 'if' statement can be re-written as 'switch' statement
		}
	}
}
