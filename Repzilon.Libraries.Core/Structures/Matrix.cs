﻿//
//  Matrix.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2024 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Collections.Generic;
#if !(NET20 || NET35 || NET40)
using System.Collections.ObjectModel;
#endif
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable InvokeAsExtensionMethod

namespace Repzilon.Libraries.Core
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct Matrix<T> : IEquatable<Matrix<T>>, IFormattable,
	IComparableMatrix, IEquatable<IComparableMatrix>
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
	{
		public readonly byte Lines;
		public readonly byte Columns;
		private readonly byte? m_bytAugmentedColumn;
		private readonly T[,] m_values;

		#region Constructors
		internal Matrix(byte lines, byte columns, byte? augmentedColumn)
		{
			if (lines < 1) {
				throw new ArgumentOutOfRangeException("lines", lines, "There must be at least one line in the matrix.");
			}
			if (columns < 1) {
				throw new ArgumentOutOfRangeException("columns", columns, "There must be at least one column in the matrix.");
			}
			Lines = lines;
			Columns = columns;
			m_values = new T[lines, columns];
			m_bytAugmentedColumn = augmentedColumn;
		}

		public Matrix(byte lines, byte columns) : this(lines, columns, (byte?)null)
		{
		}

		public Matrix(byte lines, byte columns, params T[] lineByLineValues) : this(lines, columns)
		{
			if ((lineByLineValues != null) && (lineByLineValues.Length > 0)) {
				var count = lines * columns;
				if (lineByLineValues.Length != count) {
					throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
					 "For this {0}x{1} matrix, {2} values are expected.",
					 lines, columns, count));
				}
				for (int i = 0; i < count; i++) {
					m_values[i / columns, i % columns] = lineByLineValues[i];
				}
			}
		}

		public static Matrix<T> Identity(byte size)
		{
			var m = new Matrix<T>(size, size);
			var one = ExtraMath.ConvertTo<T>(1);
			for (byte i = 0; i < size; i++) {
				m[i, i] = one;
			}
			return m;
		}

		public static Matrix<T> Signature(byte size)
		{
			var m = new Matrix<T>(size, size);
			var plusOne = ExtraMath.ConvertTo<T>(1);
			var minusOne = ExtraMath.ConvertTo<T>(-1);
			for (byte i = 0; i < size; i++) {
				for (byte j = 0; j < size; j++) {
					m[i, j] = (i + j) % 2 == 0 ? plusOne : minusOne;
				}
			}
			return m;
		}
		#endregion

		public T this[byte l, byte c]
		{
			get { return m_values[l, c]; }
			set { m_values[l, c] = value; }
		}

		IComparable IComparableMatrix.ValueAt(byte l, byte c)
		{
			return m_values[l, c];
		}

		public bool IsSquare
		{
			get { return Columns == Lines; }
		}

		byte IComparableMatrix.Lines
		{
			get { return this.Lines; }
		}

		byte IComparableMatrix.Columns
		{
			get { return this.Columns; }
		}

		byte? IComparableMatrix.AugmentedColumn
		{
			get { return m_bytAugmentedColumn; }
		}

		#region ICloneable members
		public Matrix<T> Clone()
		{
			var tc = this.Columns;
			var other = new Matrix<T>(this.Lines, tc, this.m_bytAugmentedColumn);
			for (byte i = 0; i < this.Lines; i++) {
				for (byte j = 0; j < tc; j++) {
					other[i, j] = this[i, j];
				}
			}
			return other;
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		public static Matrix<TOut> Cast<TIn, TOut>(Matrix<TIn> source)
		where TIn : struct, IFormattable, IComparable<TIn>, IEquatable<TIn>, IComparable
		where TOut : struct, IFormattable, IComparable<TOut>, IEquatable<TOut>, IComparable
		{
			var sc = source.Columns;
			var other = new Matrix<TOut>(source.Lines, sc, source.m_bytAugmentedColumn);
			for (byte i = 0; i < source.Lines; i++) {
				for (byte j = 0; j < sc; j++) {
					other[i, j] = ExtraMath.ConvertTo<TOut>(source[i, j]);
				}
			}
			return other;
		}

		public Matrix<TOut> Cast<TOut>() where TOut : struct, IFormattable, IComparable<TOut>, IEquatable<TOut>, IComparable
		{
			return Cast<T, TOut>(this);
		}

		#region Equals
		public bool Equals(Matrix<T> other)
		{
			if (this.SameSize(other) && MatrixExtensionMethods.Equals(this.m_bytAugmentedColumn, other.m_bytAugmentedColumn)) {
				for (byte i = 0; i < this.Lines; i++) {
					for (byte j = 0; j < this.Columns; j++) {
						if (!other[i, j].Equals(this[i, j])) {
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool Equals(IComparableMatrix other)
		{
			if ((other != null) && other.SameSize(this) && MatrixExtensionMethods.Equals(this.m_bytAugmentedColumn, other.AugmentedColumn)) {
				var typT = typeof(T);
				for (byte i = 0; i < this.Lines; i++) {
					for (byte j = 0; j < this.Columns; j++) {
						if (this[i, j].CompareTo(Convert.ChangeType(other.ValueAt(i, j), typT)) != 0) {
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		bool IEquatable<IComparableMatrix>.Equals(IComparableMatrix other)
		{
			return this.Equals(other);
		}

		public override bool Equals(object obj)
		{
			return obj is Matrix<T> ? this.Equals((Matrix<T>)obj) : this.Equals(obj as IComparableMatrix);
		}

		public override int GetHashCode()
		{
			unchecked {
				var magic = -1521134295;
				var hashCode = (1832363379 * -1521134295) + Lines;
				hashCode = (hashCode * magic) + Columns;
				hashCode = (hashCode * magic) + m_bytAugmentedColumn.GetValueOrDefault();
				return (hashCode * magic) + EqualityComparer<T[,]>.Default.GetHashCode(m_values);
			}
		}

		bool IComparableMatrix.SameSize(IComparableMatrix other)
		{
			return (other.Lines == this.Lines) && (other.Columns == this.Columns);
		}

		public bool SameSize(Matrix<T> other)
		{
			return (other.Lines == this.Lines) && (other.Columns == this.Columns);
		}

		public static bool operator ==(Matrix<T> left, Matrix<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Matrix<T> left, Matrix<T> right)
		{
			return !(left == right);
		}
		#endregion

		#region ToString
		public override string ToString()
		{
			return ToString(null, null);
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
			var stbDesc = new StringBuilder();
			byte i, j;
			var tl = this.Lines;

			var blnCi     = Equals(formatProvider, CultureInfo.InvariantCulture);
			var nalarCols = new NumberAlignment[this.Columns];
			for (j = 0; j < this.Columns; j++) {
				for (i = 0; i < tl; i++) {
					nalarCols[j].FromNumeric(this[i, j].ToString(format, formatProvider), formatProvider);
				}
			}

			for (i = 0; i < tl; i++) {
				if (tl == 1) {
					stbDesc.Append('[');
				} else if (i == 0) {
					stbDesc.Append(blnCi ? '/' : '⎡');
				} else if (i == tl - 1) {
					stbDesc.Append(blnCi ? '\\' : '⎣');
				} else {
					stbDesc.Append('|');
				}

				for (j = 0; j < this.Columns; j++) {
					if (m_bytAugmentedColumn.HasValue && (j == m_bytAugmentedColumn.Value)) {
						stbDesc.Append('|');
					}
					if (j > 0) {
						stbDesc.Append("  ");
					}
					stbDesc.Append(nalarCols[j].Format(this[i, j], format, formatProvider));
				}

				if (tl == 1) {
					stbDesc.Append(']');
				} else if (i == 0) {
					stbDesc.Append(blnCi ? '\\' : '⎤');
				} else if (i == tl - 1) {
					stbDesc.Append(blnCi ? '/' : '⎦');
				} else {
					stbDesc.Append('|');
				}

				if (i < tl - 1) {
					stbDesc.Append(Environment.NewLine);
				}
			}
			stbDesc.AppendFormat(" {0}x{1} of {2}", tl, this.Columns, typeof(T));
			return stbDesc.ToString();
		}
		#endregion

		#region Operators
#if !NET20
		public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
		{
			var ac = a.Columns;
			if (b.SameSize(a)) {
				var m = new Matrix<T>(a.Lines, ac);
				for (byte i = 0; i < a.Lines; i++) {
					for (byte j = 0; j < ac; j++) {
						m[i, j] = GenericArithmetic<T>.Adder(a[i, j], b[i, j]);
					}
				}
				return m;
			} else {
				throw new ArrayTypeMismatchException(String.Format(CultureInfo.CurrentCulture,
				 "To add matrices, their dimensions must be identical. They are {0}x{1} and {2}x{3}.",
				 a.Lines, ac, b.Lines, b.Columns));
			}
		}

		public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
		{
			var ac = a.Columns;
			if (b.SameSize(a)) {
				var m = new Matrix<T>(a.Lines, ac);
				for (byte i = 0; i < a.Lines; i++) {
					for (byte j = 0; j < ac; j++) {
						m[i, j] = GenericArithmetic<T>.Sub(a[i, j], b[i, j]);
					}
				}
				return m;
			} else {
				throw new ArrayTypeMismatchException(String.Format(CultureInfo.CurrentCulture,
				 "To subtract matrices, their dimensions must be identical. They are {0}x{1} and {2}x{3}.",
				 a.Lines, ac, b.Lines, b.Columns));
			}
		}

		private static Func<TScalar, T, T> BuildMultiplier<TScalar>() where TScalar : struct
		{
			return GenericArithmetic<T>.BuildMultiplier<TScalar>();
		}

		public static Matrix<T> operator *(T k, Matrix<T> m)
		{
			return m.Multiply(k);
		}

		/// <summary>
		/// Returns the scalar product of two matrices. Watch out, this operation is not commutative.
		/// </summary>
		/// <param name="a">First matrix</param>
		/// <param name="b">Second matrix</param>
		/// <returns>The scalar product in a new matrix.</returns>
		/// <exception cref="ArrayTypeMismatchException">Thrown when the number of columns of the first matrix is different from the number of lines of the second matrix.</exception>
		public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
		{
			if (a.Columns == b.Lines) {
				var mult = BuildMultiplier<T>();
				var c = new Matrix<T>(a.Lines, b.Columns);
				for (byte i = 0; i < c.Lines; i++) {
					for (byte j = 0; j < c.Columns; j++) {
						var sumOfCell = default(T);
						for (byte x = 0; x < b.Lines; x++) {
							sumOfCell = GenericArithmetic<T>.Adder(sumOfCell, mult(a[i, x], b[x, j]));
						}
						c[i, j] = sumOfCell;
					}
				}
				return c;
			} else {
				throw new ArrayTypeMismatchException(String.Format(CultureInfo.CurrentCulture,
				 "Cannot multiply a {0}x{1} matrix with a {2}x{3} matrix.",
				 a.Lines, a.Columns, b.Lines, b.Columns));
			}
		}
#endif

		public static Matrix<T> operator |(Matrix<T> coefficients, Matrix<T> values)
		{
			return MatrixExtensionMethods.Augment(coefficients, values);
		}

#if !NET20
		/// <summary>
		/// Inverts the matrix using the Gauss-Jordan technique.
		/// </summary>
		/// <param name="self">Input matrix</param>
		/// <returns>
		/// The multiplicative inverse of the input matrix, or an equivalent to null
		/// when the matrix cannot be inverted.
		/// </returns>
		public static Nullable<Matrix<T>> operator ~(Matrix<T> self)
		{
			if (!self.IsSquare) {
				throw new ArrayTypeMismatchException("Only square matrices can be inverted.");
			}
			byte l, c;
			var m = self.Lines;
			var augmented = self.AugmentWithIdentity();
			var minusOne = (-1).ConvertTo<T>();
			var zero = default(T);
			var mult = BuildMultiplier<T>();
			// Put zeros in the lower left corner
			PutZeroesInLowerLeft(self, m, ref augmented, minusOne, zero, mult);
			// Check if we can continue. If not return null
			if (augmented[(byte)(m - 1), (byte)(m - 1)].Equals(zero)) {
				return null;
			}

			// Put zeros in the upper right corner of the left side
			for (c = (byte)(self.Columns - 1); c >= 1; c--) {
				for (l = 0; l <= c - 1; l++) {
					if (!augmented[l, c].Equals(zero)) {
						AutoRun(augmented, l, c, minusOne, mult);
					}
				}
			}

			// Cast to double as we do not divide directly, but multiply with the inverse
			var matrixInDouble = augmented.Cast<double>();
			for (l = 0; l < m; l++) {
				var coeffs = new double?[m];
				coeffs[l] = 1.0 / Convert.ToDouble(matrixInDouble[l, l]);
				matrixInDouble.RunCommand(l, coeffs);
			}

			if (matrixInDouble.Left().Equals(Matrix<double>.Identity(matrixInDouble.Lines))) {
				matrixInDouble = matrixInDouble.Right();
				return matrixInDouble.Cast<T>();
			} else {
				return null;
			}
		}

		private static void PutZeroesInLowerLeft(Matrix<T> self, byte m, ref Matrix<T> augmented, T minusOne, T zero, Func<T, T, T> mult)
		{
			for (byte c = 0; c < self.Columns - 1; c++) {
				for (byte l = (byte)(c + 1); l < m; l++) {
					if (!augmented[l, c].Equals(zero)) {
						AutoRun(augmented, l, c, minusOne, mult);

#if DEBUG
						// Reduce the number of negative signs by multiplying by -1
						var np = 0;
						var nn = 0;
						for (byte k = 0; k < augmented.Columns; k++) {
							var v = augmented[l, k];
							if (v.CompareTo(zero) > 0) {
								np++;
							} else if (v.CompareTo(zero) < 0) {
								nn++;
							}
						}
						if (nn > np) {
							var coeffs = new T?[m];
							coeffs[l] = minusOne;
							augmented.RunCommand(l, coeffs);
						}
#endif
					}
				}
			}
		}

		private static void AutoRun(Matrix<T> augmented, byte l, byte c, T minusOne, Func<T, T, T> mult)
		{
			var coeffs = new T?[augmented.Lines];
			coeffs[c] = mult(augmented[l, c], minusOne);
			coeffs[l] = augmented[c, c];
			augmented.RunCommand(l, coeffs);
		}
#endif
		#endregion

#if !NET20
		/// <summary>
		/// Runs a line command on an augmented matrix. This method mutates the matrix.
		/// </summary>
		/// <param name="destinationLine">Zero-based destination line number</param>
		/// <param name="coefficients">Multiplying coefficients of source lines. Specify null for that line to not include it in calculations.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when destinationLine is bigger or equal than the number of lines of the matrix.</exception>
		/// <exception cref="ArgumentNullException">Thrown when coefficients array is null</exception>
		/// <exception cref="ArrayTypeMismatchException">Thrown when coefficients array does not have the same number of values as the number of lines of the matrix</exception>
		public void RunCommand(byte destinationLine, params Nullable<T>[] coefficients)
		{
			if (destinationLine >= this.Lines) {
				throw new ArgumentOutOfRangeException("destinationLine", destinationLine,
				 "The specified destination line is over the number of lines of the matrix.");
			}
			if (coefficients == null) {
				throw new ArgumentNullException("coefficients");
			}
			if (coefficients.Length != this.Lines) {
				throw new ArrayTypeMismatchException(String.Format(
				 "The coefficients array must have the same number of elements as the number " +
				 "of lines of the matrix, which is {0}. If a line is not involved in the " +
				 "command, pass null as its value.", this.Lines));
			} else {
				var accumulator = new T[this.Columns];
				var mult = BuildMultiplier<T>();
				byte j;
				for (byte i = 0; i < coefficients.Length; i++) {
					if (coefficients[i].HasValue) {
						for (j = 0; j < this.Columns; j++) {
							accumulator[j] = GenericArithmetic<T>.Adder(accumulator[j], mult(coefficients[i].Value, this[i, j]));
						}
					}
				}
				for (j = 0; j < this.Columns; j++) {
					this[destinationLine, j] = accumulator[j];
				}
			}
		}
#endif

		/// <summary>
		/// Swap two lines of matrix, generally an augmented one. This method mutates the matrix.
		/// </summary>
		/// <param name="first">Zero-based rank of one line to swap.</param>
		/// <param name="second">Zero-based rank of the other line to swap.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the line number are over the line count of the matrix</exception>
		public void SwapLines(byte first, byte second)
		{
			const string kOutOfRange = "The line index is bigger than the number of lines in the matrix.";
			var tl = this.Lines;
			if (first >= tl) {
				throw new ArgumentOutOfRangeException("first", first, kOutOfRange);
			}
			if (second >= tl) {
				throw new ArgumentOutOfRangeException("second", second, kOutOfRange);
			}
			if (second != first) {
				for (byte j = 0; j < this.Columns; j++) {
					var temp = this[first, j];
					this[first, j] = this[second, j];
					this[second, j] = temp;
				}
			}
		}

#if !NET20
		public T Determinant()
		{
			if (!this.IsSquare) {
				throw new ArrayTypeMismatchException("A determinant is possible for square matrices only.");
			} else {
				var l = this.Lines;
				Func<T, T, T> mult;
				if (l == 0) {
					throw new InvalidOperationException("This zero-sized matrix should not exist.");
				} else if (l == 1) {
					return m_values[0, 0];
				} else if (l == 2) { // we already know it is a square matrix
					mult = BuildMultiplier<T>();
					return GenericArithmetic<T>.Sub(mult(m_values[0, 0], m_values[1, 1]), mult(m_values[0, 1], m_values[1, 0]));
				} else {
					var c = this.Columns;
					var plusOne = 1.ConvertTo<T>();
					var minusOne = (-1).ConvertTo<T>();
					var det = default(T);
					mult = BuildMultiplier<T>();
					for (byte j = 0; j < c; j++) {
						// Build sub-matrix
						var subSize = checked((byte)(l - 1));
						var numarSubValues = new T[checked(subSize * subSize)];
						var k = 0;
						for (byte ia = 1; ia < l; ia++) {
							for (byte ja = 0; ja < c; ja++) {
								if (ja != j) {
									numarSubValues[k] = m_values[ia, ja];
									k++;
								}
							}
						}
						var subMatrix = new Matrix<T>(subSize, subSize, numarSubValues);

						// Accumulate determinant value at column
						// det += m_values[0, j] * (-1)^(i+j) * det(subMatrix)
						det = GenericArithmetic<T>.Adder(det, mult(mult(m_values[0, j], j % 2 == 0 ? plusOne : minusOne), subMatrix.Determinant()));
					}
					return det;
				}
			}
		}
#endif

		public Matrix<T> Left()
		{
			if (!m_bytAugmentedColumn.HasValue) {
				throw new ArrayTypeMismatchException("The current matrix is not an augmented matrix.");
			}
			var l = this.Lines;
			var c = m_bytAugmentedColumn.Value;
			var ml = new Matrix<T>(l, c, (byte?)null);
			for (byte i = 0; i < l; i++) {
				for (byte j = 0; j < c; j++) {
					ml[i, j] = this[i, j];
				}
			}
			return ml;
		}

		public Matrix<T> Right()
		{
			if (!m_bytAugmentedColumn.HasValue) {
				throw new ArrayTypeMismatchException("The current matrix is not an augmented matrix.");
			}
			var l = this.Lines;
			var a = m_bytAugmentedColumn.Value;
			var c = (byte)(this.Columns - a);
			var mr = new Matrix<T>(l, c, (byte?)null);
			for (byte i = 0; i < l; i++) {
				for (byte j = 0; j < c; j++) {
					mr[i, j] = this[i, (byte)(j + a)];
				}
			}
			return mr;
		}

		public Matrix<T> Minor(byte i, byte j)
		{
			if (m_bytAugmentedColumn.HasValue) {
				throw new ArrayTypeMismatchException("The Minor method cannot be called on augmented matrices.");
			}
			var l = this.Lines;
			var c = this.Columns;
			if (i >= l) {
				throw new ArgumentOutOfRangeException("i");
			}
			if (j >= c) {
				throw new ArgumentOutOfRangeException("j");
			}

			var tarValues = new T[checked((l - 1) * (c - 1))];
			var n = 0;
			for (byte x = 0; x < l; x++) {
				for (byte y = 0; y < c; y++) {
					if ((x != i) && (y != j)) {
						tarValues[n] = this[x, y];
						n++;
					}
				}
			}
			checked {
				return new Matrix<T>((byte)(l - 1), (byte)(c - 1), tarValues);
			}
		}

		public byte[] Find(T value)
		{
			for (byte i = 0; i < this.Lines; i++) {
				for (byte j = 0; j < this.Columns; j++) {
					if (this[i, j].Equals(value)) {
						return new byte[] { i, j };
					}
				}
			}
			return null;
		}

#if !NET20
		/// <summary>
		/// Tries to solve an equation system using the Cramer technique, using this matrix containing the
		/// coefficients of the variable part of the equation system.
		/// </summary>
		/// <param name="constants">A Nx1 matrix having the constant part of the equation system</param>
		/// <param name="variables">Variables names, in order</param>
		/// <returns>
		/// When a solution exists, a set of key-value pairs with the variable name and the solved value for each.
		/// When the Cramer technique cannot find a solution, returns null. It does not necessarily means the
		/// equation system is unsolvable, however.
		/// </returns>
		/// <exception cref="ArgumentNullException">When no variable names are supplied.</exception>
#if NET40 || NET35
		private IDictionary<string, T> SolveWithCramer(Matrix<T> constants, params string[] variables)
#else
		private IReadOnlyDictionary<string, T> SolveWithCramer(Matrix<T> constants, params string[] variables)
#endif
		{
			byte a, b;
			Dictionary<string, T> dicSolved;
			Matrix<T> ma;
			var det = this.Determinant();
			if (det.Equals(default(T))) {
				return null;
			} else {
				if ((variables == null) || (variables.Length < 1)) {
					throw new ArgumentNullException("variables");
				}
				dicSolved = new Dictionary<string, T>();
				var idd = 1.0 / Convert.ToDouble(det);
				for (a = 0; a < variables.Length; a++) {
					ma = this.Clone();
					for (b = 0; b < ma.Lines; b++) {
						ma[b, a] = constants[b, 0];
					}
					dicSolved.Add(variables[a], (Convert.ToDouble(ma.Determinant()) * idd).ConvertTo<T>());
				}
				return dicSolved;
			}
		}

#if false
#if NET40 || NET35
		private IDictionary<string, T> SolveByInversion(Matrix<T> constants, params string[] variables)
#else
		private IReadOnlyDictionary<string, T> SolveByInversion(Matrix<T> constants, params string[] variables)
#endif
		{
			var inverse = ~this;
			if (inverse.HasValue) {
				if ((variables == null) || (variables.Length < 1)) {
					throw new ArgumentNullException("variables");
				}
				var nowKnowns = inverse.Value * constants;
				var dicHere = new Dictionary<string, T>();
				for (byte a = 0; a < variables.Length; a++) {
					dicHere.Add(variables[a], nowKnowns[a, 0]);
				}
				return dicHere;
			} else {
				return null;
			}
		}
#endif

#if NET40 || NET35
		private IDictionary<string, T> SolveDiagonally(Matrix<T> constants, params string[] variables)
#else
		private IReadOnlyDictionary<string, T> SolveDiagonally(Matrix<T> constants, params string[] variables)
#endif
		{
			byte c;
			var m = this.Lines;
			var augmented = this.Augment(constants);
			var zero = default(T);
			// Put zeros in the lower left corner
			PutZeroesInLowerLeft(this, m, ref augmented, (-1).ConvertTo<T>(), zero, BuildMultiplier<T>());

			// Check if we can continue.
			if (augmented[(byte)(m - 1), this.Columns].Equals(zero)) {
				// TODO : Implement linked solutions
				throw new NotSupportedException("An infinity of linked solutions exists.");
			} else {
				int l = 0;
				for (c = 0; c < this.Columns; c++) {
					if (augmented[(byte)(m - 1), c].Equals(zero)) {
						l++;
					}
				}
				if (l == this.Columns) {
					return null; // No solution exists
				} else { // Single solution
					if ((variables == null) || (variables.Length < 1)) {
						throw new ArgumentNullException("variables");
					}
					var dicSolved = new SortedDictionary<string, T>();
					// Compute solution in a loop, starting with the last algebraic variable.
					for (l = 1; l <= m; l++) {
						double newvar = Convert.ToDouble(augmented[(byte)(m - l), this.Columns]);
						for (c = 1; c < l; c++) {
							/* Before code variable inlining
							var coeff = augmented[(byte)(m - l), (byte)(this.Columns - c)];
							var valueOfPreviousVar = dicSolved[variables[variables.Length - c]];
							newvar -= Convert.ToDouble(coeff) * Convert.ToDouble(valueOfPreviousVar);
							// */
							newvar -= Convert.ToDouble(augmented[(byte)(m - l), (byte)(Columns - c)]) *
							 Convert.ToDouble(dicSolved[variables[variables.Length - c]]);
						}
						/* Before code variable inlining
						double isolated = newvar / Convert.ToDouble(augmented[(byte)(m - l), (byte)(variables.Length - l)]);
						dicSolved.Add(variables[variables.Length - l], isolated.ConvertTo<T>());
						// */
						dicSolved.Add(variables[variables.Length - l],
						 (newvar / Convert.ToDouble(augmented[(byte)(m - l), (byte)(variables.Length - l)])).ConvertTo<T>());
					}
#if NET40 || NET35
					return dicSolved;
#else
					return new ReadOnlyDictionary<string, T>(dicSolved);
#endif
				}
			}
		}

		/// <summary>
		/// Tries to solve a linear equation system, using this matrix containing the coefficients of the variable
		/// part of the equation system. For now, it can only find a solution for single unique solution equation systems.
		/// </summary>
		/// <param name="constants">A Nx1 matrix having the constant part of the equation system</param>
		/// <param name="variables">Variables names, in order</param>
		/// <returns>
		/// When a unique solution exists, a set of key-value pairs with the variable name and the solved value for each.
		/// When there is no possible solution, returns null. Otherwise, throws an NotSupportedException.
		/// </returns>
		/// <exception cref="ArgumentNullException">When no variable names are supplied.</exception>
		/// <exception cref="NotSupportedException">When an infinity of linked solutions exists.</exception>
#if NET40 || NET35
		public IDictionary<string, T> Solve(Matrix<T> constants, params string[] variables)
#else
		public IReadOnlyDictionary<string, T> Solve(Matrix<T> constants, params string[] variables)
#endif
		{
#if NET40 || NET35
			IDictionary<string, T> dicSolved = null;
#else
			IReadOnlyDictionary<string, T> dicSolved = null;
#endif
			if (this.IsSquare) {
				dicSolved = SolveWithCramer(constants, variables);
			}
			if (dicSolved == null) {
				dicSolved = SolveDiagonally(constants, variables);
			}
			return dicSolved;
		}
#endif
	}

	public static class MatrixExtensionMethods
	{
#if !NET20
		public static Matrix<T> Multiply<T, TScalar>(this Matrix<T> m, TScalar k)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		where TScalar : struct
		{
			var mult = GenericArithmetic<T>.BuildMultiplier<TScalar>();
			var mm = new Matrix<T>(m.Lines, m.Columns);
			for (byte i = 0; i < m.Lines; i++) {
				for (byte j = 0; j < m.Columns; j++) {
					mm[i, j] = mult(k, m[i, j]);
				}
			}
			return mm;
		}
#endif

#if NET20
		public static Matrix<T> Augment<T>(Matrix<T> coefficients, Matrix<T> values)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
#else
		public static Matrix<T> Augment<T>(this Matrix<T> coefficients, Matrix<T> values)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
#endif
		{
			var cl = coefficients.Lines;
			if (values.Lines == cl) {
				var cc = coefficients.Columns;
				var augmented = new Matrix<T>(cl,
				 checked((byte)(cc + values.Columns)), cc);
				for (byte i = 0; i < cl; i++) {
					byte j;
					for (j = 0; j < cc; j++) {
						augmented[i, j] = coefficients[i, j];
					}
					for (j = 0; j < values.Columns; j++) {
						augmented[i, (byte)(j + cc)] = values[i, j];
					}
				}
				return augmented;
			} else {
				throw new ArrayTypeMismatchException(String.Format(CultureInfo.CurrentCulture,
				 "Cannot augment a {0}-line matrix with a {1}-line matrix.",
				 cl, values.Lines));
			}
		}

#if NET20
		public static Matrix<T> AugmentWithIdentity<T>(Matrix<T> coefficients)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
#else
		public static Matrix<T> AugmentWithIdentity<T>(this Matrix<T> coefficients)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
#endif
		{
			return MatrixExtensionMethods.Augment(coefficients, Matrix<T>.Identity(coefficients.Lines));
		}

		/// <summary>
		/// Rounds off calculation errors of this matrix. This method mutates the matrix.
		/// </summary>
		/// <param name="matrix">Matrix to round</param>
#if NET20
		public static void RoundErrors(Matrix<float> matrix)
#else
		public static void RoundErrors(this Matrix<float> matrix)
#endif
		{
			for (byte i = 0; i < matrix.Lines; i++) {
				for (byte j = 0; j < matrix.Columns; j++) {
					matrix[i, j] = RoundOff.Error(matrix[i, j]);
				}
			}
		}

		/// <summary>
		/// Rounds off calculation errors of this matrix. This method mutates the matrix.
		/// </summary>
		/// <param name="matrix">Matrix to round</param>
#if NET20
		public static void RoundErrors(Matrix<double> matrix)
#else
		public static void RoundErrors(this Matrix<double> matrix)
#endif
		{
			for (byte i = 0; i < matrix.Lines; i++) {
				for (byte j = 0; j < matrix.Columns; j++) {
					matrix[i, j] = RoundOff.Error(matrix[i, j]);
				}
			}
		}

		internal static bool Equals<T>(Nullable<T> a, Nullable<T> b) where T : struct
		{
			return (a.HasValue == b.HasValue) && (!a.HasValue || a.Value.Equals(b.Value));
		}

		public static short Signature(byte i, byte j)
		{
			return (i + j) % 2 == 0 ? (short)1 : (short)-1;
		}
	}
}
