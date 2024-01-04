//
//  RegressionModel.cs
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
using System.Globalization;
using System.Text;

namespace Repzilon.Libraries.Core
{
	public enum MathematicalModel : byte
	{
		Affine = 0,
		Power = 1,
		LogLog = 1,
		Exponential = 2,
		SemiLogY = 2,
		LinLog = 2,
		Logarithmic = 3,
		SemiLogX = 3,
		LogLin = 3
	}

	public struct RegressionModel<T> : IEquatable<RegressionModel<T>>, IFormattable
#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
	, ICloneable
#endif
	where T : struct, IFormattable, IEquatable<T>
	{
		/// <summary>In an affine model, value of the intercept.</summary>
		T A;

		/// <summary>In an affine model, value of the slope.</summary>
		T B;

		/// <summary>Coefficient of correlation</summary>
		T R;

		MathematicalModel Model;

		public RegressionModel(T a, T b, T r, MathematicalModel model)
		{
			A = a;
			B = b;
			R = r;
			Model = model;
		}

		#region Clone
		public RegressionModel(RegressionModel<T> other) : this(other.A, other.B, other.R, other.Model) { }

		public RegressionModel<T> Clone()
		{
			return new RegressionModel<T>(this);
		}

#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public T Determination()
		{
			return GenericArithmetic<T>.BuildMultiplier<T>()(R, R);
		}

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is RegressionModel<T> && Equals((RegressionModel<T>)obj);
		}

		public bool Equals(RegressionModel<T> other)
		{
			return A.Equals(other.A) && B.Equals(other.B) && R.Equals(other.R) && (Model == other.Model);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = -1053832008 * -1521134295 + A.GetHashCode();
				hashCode = hashCode * -1521134295 + B.GetHashCode();
				hashCode = hashCode * -1521134295 + R.GetHashCode();
				return hashCode * -1521134295 + (int)Model;
			}
		}

		public static bool operator ==(RegressionModel<T> left, RegressionModel<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(RegressionModel<T> left, RegressionModel<T> right)
		{
			return !(left == right);
		}
		#endregion

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
			stbFormula.Append("y = ");
			var enuModel = this.Model;
			var strA = this.A.ToString(format, formatProvider);
			var strB = this.B.ToString(format, formatProvider);
			if (enuModel == MathematicalModel.Affine) {
				stbFormula.Append(strA).Append(" + ").Append(strB).Append("x");
			} else if (enuModel == MathematicalModel.Power) {
				stbFormula.Append(strA).Append(" * x^").Append(strB);
			} else if (enuModel == MathematicalModel.Exponential) {
				// To derivate, b must be converted to a multiple of constant e
				stbFormula.Append(strA).Append(" * ").Append(strB).Append("^x");
			} else if (enuModel == MathematicalModel.Logarithmic) {
				// To derivate, the base 10 log must be converted to natural log
				stbFormula.Append(strA).Append(" * log10(x) + ").Append(strB);
			} else {
				stbFormula.Append('?');
			}
			return stbFormula.ToString();
		}
		#endregion

		public double GrowthRate()
		{
			if (this.Model != MathematicalModel.Exponential) {
				throw new InvalidOperationException("A growth rate requires an exponential model.");
			}
			return Convert.ToDouble(B) - 1;
		}

		// TODO : Have something to make all formulas derivable
	}
}