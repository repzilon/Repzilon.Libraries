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
		private static readonly double k1ofLn10 = 1.0 / Math.Log(10);

		/// <summary>In an affine model, value of the intercept.</summary>
		public T A;

		/// <summary>In an affine model, value of the slope.</summary>
		public T B;

		/// <summary>Coefficient of correlation</summary>
		public T R;

		public MathematicalModel Model;

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

		/// <summary>
		/// Gives the formula according to the model and values of a and b.
		/// </summary>
		/// <param name="format">
		/// A .NET format specification to format numbers, with the follwing extra:
		/// If there are two letters, the first one being 'e' or 'E', the formula will be expressed so that you
		/// can derivate it (with calculus) later on. Watch out, a single 'e' will keep the nornal behavior,
		/// which is to express numbers with the "scientific" notation.
		/// </param>
		/// <param name="formatProvider">
		/// NumberFormatInfo or CultureInfo object giving parameters such as decimal separator or digit groupings.
		/// </param>
		/// <returns>
		/// The formula as a plain text string using the ASCII character set for the operators.
		/// * will be used for multiplication, ^ for exponentiation.
		/// </returns>
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
			var strA = this.A.ToString(format, formatProvider);
			var strB = this.B.ToString(format, formatProvider);
			bool blnDerivable = false;
			if ((format.Length >= 2) && ((format[0] == 'e') || (format[0] == 'E')) &&
			Char.IsLetter(format[1])) {
				blnDerivable = true;
				format = format.Substring(1);
			}

			var enuModel = this.Model;

			if (enuModel == MathematicalModel.Affine) {
				stbFormula.Append(strA).Append(" + ").Append(strB).Append("x");
			} else if (enuModel == MathematicalModel.Power) {
				stbFormula.Append(strA).Append(" * x^").Append(strB);
			} else if (enuModel == MathematicalModel.Exponential) {
				// To derivate, b^x must be converted to base e
				stbFormula.Append(strA);
				if (blnDerivable) {
					stbFormula.Append(" * e^(ln(").Append(strB).Append("^x))");
				} else {
					stbFormula.Append(" * ").Append(strB).Append("^x");
				}
			} else if (enuModel == MathematicalModel.Logarithmic) {
				// To derivate, the base 10 log must be converted to natural log
				if (blnDerivable) {
					var newA = Convert.ToDouble(A) * k1ofLn10;
					strA = newA.ToString(format, formatProvider);
				}
				stbFormula.Append(strA).Append(blnDerivable ? " * ln(x) + " : " * log10(x) + ").Append(strB);
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
	}
}