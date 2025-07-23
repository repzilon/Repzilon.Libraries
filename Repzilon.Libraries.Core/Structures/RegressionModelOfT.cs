//
//  RegressionModelOfT.cs
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core.Regression
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

#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct RegressionModel<T> : IEquatable<RegressionModel<T>>, IFormattable
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IEquatable<T>, IComparable<T>
	{
		/// <summary>In an affine model, value of the intercept.</summary>
		public readonly T A;

		/// <summary>In an affine model, value of the slope.</summary>
		public readonly T B;

		/// <summary>Coefficient of correlation</summary>
		public readonly T R;

		public readonly T MinX;

		public readonly T MaxX;

		public readonly MathematicalModel Model;

		public RegressionModel(T a, T b, T r, MathematicalModel model, T minX, T maxX)
		{
			A = a;
			B = b;
			R = r;
			Model = model;
			MinX = minX;
			MaxX = maxX;
		}

		internal static RegressionModel<T> Affine(T a, T b, T minX, T maxX)
		{
			return new RegressionModel<T>(a, b, ExtraMath.ConvertTo<T>(b.CompareTo(default(T))),
			 MathematicalModel.Affine, minX, maxX);
		}

		#region Clone
		public RegressionModel(RegressionModel<T> source) :
		this(source.A, source.B, source.R, source.Model, source.MinX, source.MaxX) { }

		public RegressionModel<T> Clone()
		{
			return new RegressionModel<T>(this);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public T Determination()
		{
#if NET20
			return Arithmetic<T>.MultiplyScalars(R, R);
#else
			return Arithmetic<T>.MulT(R, R);
#endif
		}

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is RegressionModel<T> && Equals((RegressionModel<T>)obj);
		}

		public bool Equals(RegressionModel<T> other)
		{
			return A.Equals(other.A) && B.Equals(other.B) && R.Equals(other.R) && (Model == other.Model) &&
			 MinX.Equals(other.MinX) && MaxX.Equals(other.MaxX);
		}

		public override int GetHashCode()
		{
			unchecked {
#pragma warning disable U2U1000 // Local variable can be inlined or declared const
				// ReSharper disable once SuggestVarOrType_BuiltInTypes
				// ReSharper disable once ConvertToConstant.Local
				/*const*/  int magic = -1521134295;
#pragma warning restore U2U1000 // Local variable can be inlined or declared const
				var hashCode = (-1053832008 * -1521134295) + A.GetHashCode();
				hashCode = (hashCode * magic) + B.GetHashCode();
				hashCode = (hashCode * magic) + R.GetHashCode();
				hashCode = (hashCode * magic) + MinX.GetHashCode();
				hashCode = (hashCode * magic) + MaxX.GetHashCode();
				return (hashCode * magic) + (int)Model;
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
		/// A .NET format specification to format numbers, with the following extra:
		/// If there are two letters, the first one being 'e' or 'E', the formula will be expressed so that you
		/// can derivate it (with calculus) later on. Watch out, a single 'e' will keep the normal behavior,
		/// which is to express numbers with the "scientific" notation.
		/// </param>
		/// <param name="formatProvider">
		/// NumberFormatInfo or CultureInfo object giving parameters such as decimal separator or digit groupings.
		/// </param>
		/// <returns>
		/// The formula as a plain text string using the ASCII character set for the operators.
		/// * will be used for multiplication, ^ for exponentiation.
		/// </returns>
		/// <remarks>
		/// To imitate what Microsoft Excel outputs for the equation of a trend line on a graph, use "eg4" for format.
		/// </remarks>
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
			stbFormula.Append("y = ");
			var blnDerivable = false;
			if ((format.Length >= 2) && ((format[0] == 'e') || (format[0] == 'E')) &&
			Char.IsLetter(format[1])) {
				blnDerivable = true;
				format = format.Substring(1);
			}
			var strA = this.A.ToString(format, formatProvider);
			var strB = this.B.ToString(format, formatProvider);

			var enuModel = this.Model;

#pragma warning disable RECS0012 // 'if' statement can be re-written as 'switch' statement
#pragma warning disable CC0019 // Use 'switch'
			if (enuModel == MathematicalModel.Affine) {
				stbFormula.Append(strA).Append(" + ").Append(strB).Append('x');
			} else if (enuModel == MathematicalModel.Power) {
				stbFormula.Append(strA).Append(" * x^").Append(strB);
			} else if (enuModel == MathematicalModel.Exponential) {
				// To derivate, b^x must be converted to base e
				stbFormula.Append(strA);
				if (blnDerivable) {
					strB = Math.Log(Convert.ToDouble(B)).ToString(format, formatProvider);
					stbFormula.Append(" * e^").Append(strB).Append('x');
				} else {
					stbFormula.Append(" * ").Append(strB).Append("^x");
				}
			} else if (enuModel == MathematicalModel.Logarithmic) {
				// To derivate, the base 10 log must be converted to natural log
				if (blnDerivable) {
					var newA = Convert.ToDouble(A) * LinearRegression.OneOfLn10;
					strA = newA.ToString(format, formatProvider);
				}
				stbFormula.Append(strA).Append(blnDerivable ? " * ln(x) + " : " * log10(x) + ").Append(strB);
			} else {
				stbFormula.Append('?');
			}
#pragma warning restore CC0019 // Use 'switch'
#pragma warning restore RECS0012 // 'if' statement can be re-written as 'switch' statement

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

		public T Evaluate(T x)
		{
			var model = this.Model;
			var b = this.B;
#if !NET20
			var mul = Arithmetic<T>.MulT;
#endif
			var dblX = Convert.ToDouble(x);
#if !NET20
			var add = Arithmetic<T>.Adder;
#endif
			if (model == MathematicalModel.Affine) {
#if NET20
				return Arithmetic<T>.AddScalars(A, Arithmetic<T>.MultiplyScalars(b, x));
#else
				return add(A, mul(b, x));
#endif
			} else if (model == MathematicalModel.Exponential) {
#if NET20
				return RisingConcaveUpwards(Convert.ToDouble(b), dblX);
#else
				return RisingConcaveUpwards(mul, Convert.ToDouble(b), dblX);
#endif
			} else if (model == MathematicalModel.Logarithmic) {
#if NET20
				return Arithmetic<T>.AddScalars(
				 Arithmetic<T>.MultiplyScalars(A, ExtraMath.ConvertTo<T>(Math.Log10(dblX))), b);
#else
				return add(mul(A, ExtraMath.ConvertTo<T>(Math.Log10(dblX))), b);
#endif
			} else if (model == MathematicalModel.Power) {
#if NET20
				return RisingConcaveUpwards(dblX, Convert.ToDouble(b));
#else
				return RisingConcaveUpwards(mul, dblX, Convert.ToDouble(b));
#endif
			} else {
				throw new NotSupportedException();
			}
		}

#if NET20
		private T RisingConcaveUpwards(double radix, double exponent)
		{
			return Arithmetic<T>.MultiplyScalars(A, ExtraMath.ConvertTo<T>(Math.Pow(radix, exponent)));
		}
#else
		private T RisingConcaveUpwards(Func<T, T, T> mul, double radix, double exponent)
		{
			if (mul == null) {
				throw new ArgumentNullException("mul");
			}
			return mul(A, ExtraMath.ConvertTo<T>(Math.Pow(radix, exponent)));
		}
#endif

		public T Solve(T y)
		{
			var model = this.Model;
			var yDivA = Convert.ToDouble(y) / Convert.ToDouble(A);
			var dblB = Convert.ToDouble(B);
			double dblSolution;
			if (model == MathematicalModel.Affine) {
#if NET20
				dblSolution = Convert.ToDouble(Arithmetic<T>.SubtractScalars(y, A)) / dblB;
#else
				dblSolution = Convert.ToDouble(Arithmetic<T>.Sub(y, A)) / dblB;
#endif
			} else if (model == MathematicalModel.Exponential) {
				dblSolution = Math.Log(yDivA, dblB);
			} else if (model == MathematicalModel.Logarithmic) {
#if NET20
				dblSolution = Math.Pow(10, Convert.ToDouble(Arithmetic<T>.SubtractScalars(y, B)) / Convert.ToDouble(A));
#else
				dblSolution = Math.Pow(10, Convert.ToDouble(Arithmetic<T>.Sub(y, B)) / Convert.ToDouble(A));
#endif
			} else if (model == MathematicalModel.Power) {
				dblSolution = Math.Pow(yDivA, 1.0 / dblB);
			} else {
				throw new NotSupportedException();
			}
			return ExtraMath.ConvertTo<T>(dblSolution);
		}

		public T EvaluateDerivative(T x)
		{
			var model = this.Model;
#if !NET20
			var mul = Arithmetic<T>.MulT;
#endif
			var dblB = Convert.ToDouble(B);
			var dblX = Convert.ToDouble(x);

			if (model == MathematicalModel.Affine) {
				return this.B;
			} else if (model == MathematicalModel.Exponential) {
#if NET20
				return Arithmetic<T>.MultiplyScalars(this.A,
				 ExtraMath.ConvertTo<T>(Math.Pow(dblB, dblX) * Math.Log(dblB)));
#else
				return mul(this.A, ExtraMath.ConvertTo<T>(Math.Pow(dblB, dblX) * Math.Log(dblB)));
#endif
			} else if (model == MathematicalModel.Logarithmic) {
				return ExtraMath.ConvertTo<T>(Convert.ToDouble(A) / (Math.Log(10) * dblX));
			} else if (model == MathematicalModel.Power) {
#if NET20
				return Arithmetic<T>.MultiplyScalars(Arithmetic<T>.MultiplyScalars(this.A, this.B),
				 ExtraMath.ConvertTo<T>(Math.Pow(dblX, dblB - 1)));
#else
				return mul(mul(this.A, this.B), ExtraMath.ConvertTo<T>(Math.Pow(dblX, dblB - 1)));
#endif
			} else {
				throw new NotSupportedException();
			}
		}

		public T EvaluatePrimitive(T x)
		{
			var model = this.Model;
#if !NET20
			var mul = Arithmetic<T>.MulT;
#endif
			double coeff;
			var dblX = Convert.ToDouble(x);
			var dblB = Convert.ToDouble(B);
#if !NET20
			var add = Arithmetic<T>.Adder;
#endif
			if (model == MathematicalModel.Affine) {
#if NET20
				return Arithmetic<T>.MultiplyScalars(x, Arithmetic<T>.AddScalars(A,
				 Arithmetic<T>.MultiplyScalars(ExtraMath.ConvertTo<T>(0.5), B, x)));
#else
				return mul(x, add(A, mul(mul(ExtraMath.ConvertTo<T>(0.5), B), x)));
#endif
			} else if (model == MathematicalModel.Exponential) {
#if NET20
				return Arithmetic<T>.MultiplyScalars(this.A, ExtraMath.ConvertTo<T>(Math.Pow(dblB, dblX) / Math.Log(10)));
#else
				return mul(this.A, ExtraMath.ConvertTo<T>(Math.Pow(dblB, dblX) / Math.Log(10)));
#endif
			} else if (model == MathematicalModel.Logarithmic) {
				coeff = Convert.ToDouble(this.A) / Math.Log(10);
#if NET20
				return Arithmetic<T>.MultiplyScalars(x,
				 ExtraMath.ConvertTo<T>(coeff * Math.Log(dblX) - coeff + dblB));
#else
				return mul(x, ExtraMath.ConvertTo<T>(coeff * Math.Log(dblX) - coeff + dblB));
#endif
			} else if (model == MathematicalModel.Power) {
#if NET20
				coeff = Convert.ToDouble(Arithmetic<T>.AddScalars(B, ExtraMath.ConvertTo<T>(1)));
				return Arithmetic<T>.MultiplyScalars(this.A,
				 ExtraMath.ConvertTo<T>(Math.Pow(dblX, coeff) / coeff));
#else
				coeff = Convert.ToDouble(add(B, ExtraMath.ConvertTo<T>(1)));
				return mul(this.A, ExtraMath.ConvertTo<T>(Math.Pow(dblX, coeff) / coeff));
#endif
			} else {
				throw new NotSupportedException();
			}
		}
	}
}
