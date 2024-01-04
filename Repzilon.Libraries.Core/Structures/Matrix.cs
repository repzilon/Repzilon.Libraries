//
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	[StructLayout(LayoutKind.Auto)]
	public struct Matrix<T> : IEquatable<Matrix<T>>, IFormattable,
	IComparableMatrix, IEquatable<IComparableMatrix>
#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
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
			T one = 1.ConvertTo<T>();
			for (byte i = 0; i < size; i++) {
				m[i, i] = one;
			}
			return m;
		}

		public static Matrix<T> Signature(byte size)
		{
			var m = new Matrix<T>(size, size);
			T plusOne = 1.ConvertTo<T>();
			T minusOne = (-1).ConvertTo<T>();
			for (byte i = 0; i < size; i++) {
				for (byte j = 0; j < size; j++) {
					m[i, j] = (i + j) % 2 == 0 ? plusOne : minusOne;
				}
			}
			return m;
		}
		#endregion

		public T this[byte l, byte c] {
			get { return m_values[l, c]; }
			set { m_values[l, c] = value; }
		}

		IComparable IComparableMatrix.ValueAt(byte l, byte c)
		{
			return m_values[l, c];
		}

		public bool IsSquare {
			get { return Columns == Lines; }
		}

		byte IComparableMatrix.Lines {
			get { return this.Lines; }
		}

		byte IComparableMatrix.Columns {
			get { return this.Columns; }
		}

		byte? IComparableMatrix.AugmentedColumn {
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

#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
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
					other[i, j] = source[i, j].ConvertTo<TOut>();
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

		public bool Equals(IComparableMatrix other)
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

		public override bool Equals(object obj)
		{
			if (obj is Matrix<T>) {
				return this.Equals((Matrix<T>)obj);
			} else {
				return this.Equals(obj as IComparableMatrix);
			}
		}

		public override int GetHashCode()
		{
			unchecked {
				int magic = -1521134295;
				int hashCode = (1832363379 * -1521134295) + Lines;
				hashCode = hashCode * magic + Columns;
				hashCode = hashCode * magic + m_bytAugmentedColumn.GetValueOrDefault();
				return hashCode * magic + EqualityComparer<T[,]>.Default.GetHashCode(m_values);
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
			if (String.IsNullOrWhiteSpace(format)) {
				format = "G";
			}
			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}
			StringBuilder stbDesc = new StringBuilder();
			byte i, j;
			var tl = this.Lines;

			bool blnCI = (formatProvider == CultureInfo.InvariantCulture);
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
					stbDesc.Append(blnCI ? '/' : '⎡');
				} else if (i == tl - 1) {
					stbDesc.Append(blnCI ? '\\' : '⎣');
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
					stbDesc.Append(blnCI ? '\\' : '⎤');
				} else if (i == tl - 1) {
					stbDesc.Append(blnCI ? '/' : '⎦');
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
		public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
		{
			var ac = a.Columns;
			if (b.SameSize(a)) {
				var m = new Matrix<T>(a.Lines, ac);
				for (byte i = 0; i < a.Lines; i++) {
					for (byte j = 0; j < ac; j++) {
						m[i, j] = GenericArithmetic<T>.adder(a[i, j], b[i, j]);
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
						m[i, j] = GenericArithmetic<T>.sub(a[i, j], b[i, j]);
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
						T sumOfCell = default(T);
						for (byte x = 0; x < b.Lines; x++) {
							sumOfCell = GenericArithmetic<T>.adder(sumOfCell, mult(a[i, x], b[x, j]));
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

		public static Matrix<T> operator |(Matrix<T> coefficients, Matrix<T> values)
		{
			return MatrixExtensionMethods.Augment(coefficients, values);
		}

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
			T minusOne = (-1).ConvertTo<T>();
			T zero = default(T);
			var mult = BuildMultiplier<T>();
			// Put zeroes in the lower left corner
			for (c = 0; c < self.Columns - 1; c++) {
				for (l = (byte)(c + 1); l < m; l++) {
					if (!augmented[l, c].Equals(zero)) {
						AutoRun(augmented, l, c, minusOne, mult);

#if (DEBUG)
						// Reduce the number of negative signs by multiplying by -1
						int np = 0, nn = 0;
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
				double?[] coeffs = new double?[m];
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

		private static void AutoRun(Matrix<T> augmented, byte l, byte c, T minusOne, Func<T, T, T> mult)
		{
			T?[] coeffs = new T?[augmented.Lines];
			coeffs[c] = mult(augmented[l, c], minusOne);
			coeffs[l] = augmented[c, c];
			augmented.RunCommand(l, coeffs);
		}
		#endregion

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
							accumulator[j] = GenericArithmetic<T>.adder(accumulator[j], mult(coefficients[i].Value, this[i, j]));
						}
					}
				}
				for (j = 0; j < this.Columns; j++) {
					this[destinationLine, j] = accumulator[j];
				}
			}
		}

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
					T temp = this[first, j];
					this[first, j] = this[second, j];
					this[second, j] = temp;
				}
			}
		}

		public T Determinant()
		{
			if (!this.IsSquare) {
				throw new ArrayTypeMismatchException("A determinant is possible for square matrices only.");
			} else {
				var l = this.Lines;
				Func<T, T, T> mult = null;
				if (l == 0) {
					throw new InvalidOperationException("This zero-sized matrix should not exist.");
				} else if (l == 1) {
					return m_values[0, 0];
				} else if (l == 2) { // we already know it is a square matrix
					mult = BuildMultiplier<T>();
					return GenericArithmetic<T>.sub(mult(m_values[0, 0], m_values[1, 1]), mult(m_values[0, 1], m_values[1, 0]));
				} else {
					var c = this.Columns;
					T plusOne = 1.ConvertTo<T>();
					T minusOne = (-1).ConvertTo<T>();
					T det = default(T);
					mult = BuildMultiplier<T>();
					for (byte j = 0; j < c; j++) {
						// Build sub-matrix
						byte subSize = checked((byte)(l - 1));
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
						det = GenericArithmetic<T>.adder(det, mult(mult(m_values[0, j], j % 2 == 0 ? plusOne : minusOne), subMatrix.Determinant()));
					}
					return det;
				}
			}
		}

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

		public static short Signature(byte i, byte j)
		{
			return (i + j) % 2 == 0 ? (short)1 : (short)-1;
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
		private IReadOnlyDictionary<string, T> SolveWithCramer(Matrix<T> constants, params string[] variables)
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

		/// <summary>
		/// Tries to solves a linear equation system, using this matrix containing the coefficients of the variable
		/// part of the equation system. For now, only square matrices are supported and may only find a solution for
		/// single unique solution equation systems.
		/// </summary>
		/// <param name="constants">A Nx1 matrix having the constant part of the equation system</param>
		/// <param name="variables">Variables names, in order</param>
		/// <returns>
		/// When a unique solution exists, a set of key-value pairs with the variable name and the solved value for each.
		/// Otherwise, throws an NotSupportedException.
		/// </returns>
		/// <exception cref="ArgumentNullException">When no variable names are supplied.</exception>
		/// <exception cref="NotSupportedException">
		/// When this matrix is not square or for square matrices, when neither Cramer nor matrix inversion techniques
		/// yields a single solution. Support will improve in the future.
		/// </exception>
		public IReadOnlyDictionary<string, T> Solve(Matrix<T> constants, params string[] variables)
		{
			if (this.IsSquare) {
				var dicSolved = SolveWithCramer(constants, variables);
				if (dicSolved == null) {
					// Try to solve by matrix inversion
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
						// FIXME : Gauss matrix solving technique is not yet implemented
						throw new NotSupportedException("Gauss matrix solving technique is not yet supported.");
					}
				} else {
					return dicSolved;
				}
			} else {
				// FIXME : Solving non square matrices is not yet supported
				throw new NotSupportedException("Solving non square matrices is not yet supported.");
			}
		}
	}

	public static class MatrixExtensionMethods
	{
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

		public static Matrix<T> Augment<T>(this Matrix<T> coefficients, Matrix<T> values)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
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

		public static Matrix<T> AugmentWithIdentity<T>(this Matrix<T> coefficients)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			return coefficients.Augment(Matrix<T>.Identity(coefficients.Lines));
		}

		/// <summary>
		/// Rounds off calculation errors of this matrix. This method mutates the matrix.
		/// </summary>
		/// <param name="matrix">Matrix to round</param>
		public static void RoundErrors(this Matrix<float> matrix)
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
		public static void RoundErrors(this Matrix<double> matrix)
		{
			for (byte i = 0; i < matrix.Lines; i++) {
				for (byte j = 0; j < matrix.Columns; j++) {
					matrix[i, j] = RoundOff.Error(matrix[i, j]);
				}
			}
		}

		public static TOut ConvertTo<TOut>(this ValueType value)
		where TOut : struct
		{
			return (TOut)Convert.ChangeType(value, typeof(TOut));
		}

		internal static bool Equals<T>(Nullable<T> a, Nullable<T> b) where T : struct
		{
			return (a.HasValue == b.HasValue) && (!a.HasValue || a.Value.Equals(b.Value));
		}
	}
}
