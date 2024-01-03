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
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	// TODO : Matrix resolution with Cramer method, then fallback to inversion
	[StructLayout(LayoutKind.Auto)]
	public struct Matrix<T> : IEquatable<Matrix<T>>, IFormattable
#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
	, ICloneable
#endif
	where T : struct, IFormattable
	{
		#region Static members
		internal static readonly Func<T, T, T> add = BuildAdder();
		internal static readonly Func<T, T, T> sub = BuildSubtractor();

		private static Func<T, T, T> BuildAdder()
		{
			var type = typeof(T);
			// Declare the parameters
			var paramA = Expression.Parameter(type, "a");
			var paramB = Expression.Parameter(type, "b");

			// Add the parameters together and compile it
			return Expression.Lambda<Func<T, T, T>>(Expression.Add(paramA, paramB), paramA, paramB).Compile();
		}

		private static Func<T, T, T> BuildSubtractor()
		{
			var type = typeof(T);
			// Declare the parameters
			var paramA = Expression.Parameter(type, "a");
			var paramB = Expression.Parameter(type, "b");

			// Add the parameters together and compile it
			return Expression.Lambda<Func<T, T, T>>(Expression.Subtract(paramA, paramB), paramA, paramB).Compile();
		}

		public static T AddScalars(T a, T b)
		{
			return add(a, b);
		}

		public static T SubtractScalars(T a, T b)
		{
			return sub(a, b);
		}

		public static T MultiplyScalars(T a, T b)
		{
			return BuildMultiplier<T>()(a, b);
		}

		public static T MultiplyScalars(T a, T b, T c)
		{
			var mul = BuildMultiplier<T>();
			return mul(mul(a, b), c);
		}
		#endregion

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

		public bool IsSquare {
			get { return Columns == Lines; }
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
		where TIn : struct, IFormattable
		where TOut : struct, IFormattable
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

		public Matrix<TOut> Cast<TOut>() where TOut : struct, IFormattable
		{
			return Cast<T, TOut>(this);
		}

		#region Equals
		public bool Equals(Matrix<T> other)
		{
			if (this.SameSize(other)) {
				var tac = this.m_bytAugmentedColumn;
				var oac = other.m_bytAugmentedColumn;
				if ((tac.HasValue == oac.HasValue) && (tac.Value == oac.Value)) {
					for (byte i = 0; i < this.Lines; i++) {
						for (byte j = 0; j < this.Columns; j++) {
							if (!other[i, j].Equals(this[i, j])) {
								return false;
							}
						}
					}
					return true;
				}
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			// TODO : Support comparing with Matrix using other type of storage
			return (obj is Matrix<T>) && this.Equals((Matrix<T>)obj);
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
			// TODO : Align number output
			StringBuilder stbDesc = new StringBuilder();
			var tl = this.Lines;
			for (byte i = 0; i < tl; i++) {
				if (tl == 1) {
					stbDesc.Append('[');
				} else if (i == 0) {
					stbDesc.Append('/');
				} else if (i == tl - 1) {
					stbDesc.Append('\\');
				} else {
					stbDesc.Append('|');
				}

				for (byte j = 0; j < this.Columns; j++) {
					if (m_bytAugmentedColumn.HasValue && (j == m_bytAugmentedColumn.Value)) {
						stbDesc.Append('|');
					}
					if (j > 0) {
						stbDesc.Append('\t');
					}
					stbDesc.Append(this[i, j].ToString(format, formatProvider));
				}

				if (tl == 1) {
					stbDesc.Append(']');
				} else if (i == 0) {
					stbDesc.Append('\\');
				} else if (i == tl - 1) {
					stbDesc.Append('/');
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
						m[i, j] = add(a[i, j], b[i, j]);
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
						m[i, j] = sub(a[i, j], b[i, j]);
					}
				}
				return m;
			} else {
				throw new ArrayTypeMismatchException(String.Format(CultureInfo.CurrentCulture,
				 "To subtract matrices, their dimensions must be identical. They are {0}x{1} and {2}x{3}.",
				 a.Lines, ac, b.Lines, b.Columns));
			}
		}

		internal static Func<TScalar, T, T> BuildMultiplier<TScalar>() where TScalar : struct
		{
			// Declare the parameters
			var paramA = Expression.Parameter(typeof(TScalar), "a");
			var paramB = Expression.Parameter(typeof(T), "b");

			// Add the parameters together
			BinaryExpression body = Expression.Multiply(paramA, paramB);

			// Compile it
			return Expression.Lambda<Func<TScalar, T, T>>(body, paramA, paramB).Compile();
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
				var mul = BuildMultiplier<T>();
				var c = new Matrix<T>(a.Lines, b.Columns);
				for (byte i = 0; i < c.Lines; i++) {
					for (byte j = 0; j < c.Columns; j++) {
						T sumOfCell = default(T);
						for (byte x = 0; x < b.Lines; x++) {
							sumOfCell = add(sumOfCell, mul(a[i, x], b[x, j]));
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

		// TODO : operator ~ to invert a matrix
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
				var mul = BuildMultiplier<T>();
				byte j;
				for (byte i = 0; i < coefficients.Length; i++) {
					if (coefficients[i].HasValue) {
						for (j = 0; j < this.Columns; j++) {
							accumulator[j] = add(accumulator[j], mul(coefficients[i].Value, this[i, j]));
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
				Func<T, T, T> mul = null;
				if (l == 0) {
					throw new InvalidOperationException("This zero-sized matrix should not exist.");
				} else if (l == 1) {
					return m_values[0, 0];
				} else if (l == 2) { // we already know it is a square matrix
					mul = BuildMultiplier<T>();
					return sub(mul(m_values[0, 0], m_values[1, 1]), mul(m_values[0, 1], m_values[1, 0]));
				} else {
					var c = this.Columns;
					T plusOne = 1.ConvertTo<T>();
					T minusOne = (-1).ConvertTo<T>();
					T det = default(T);
					mul = BuildMultiplier<T>();
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
						det = add(det, mul(mul(m_values[0, j], j % 2 == 0 ? plusOne : minusOne), subMatrix.Determinant()));
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
	}

	public static class MatrixExtensionMethods
	{
		public static Matrix<T> Multiply<T, TScalar>(this Matrix<T> m, TScalar k)
		where T : struct, IFormattable
		where TScalar : struct
		{
			var mul = Matrix<T>.BuildMultiplier<TScalar>();
			var mm = new Matrix<T>(m.Lines, m.Columns);
			for (byte i = 0; i < m.Lines; i++) {
				for (byte j = 0; j < m.Columns; j++) {
					mm[i, j] = mul(k, m[i, j]);
				}
			}
			return mm;
		}

		public static Matrix<T> Augment<T>(this Matrix<T> coefficients, Matrix<T> values)
		where T : struct, IFormattable
		{
			if (values.Lines == coefficients.Lines) {
				var cc = coefficients.Columns;
				var augmented = new Matrix<T>(coefficients.Lines,
				 checked((byte)(cc + values.Columns)), cc);
				for (byte i = 0; i < coefficients.Lines; i++) {
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
				 coefficients.Lines, values.Lines));
			}
		}

		public static Matrix<T> AugmentWithIdentity<T>(this Matrix<T> coefficients)
		where T : struct, IFormattable
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
	}
}
