//
//  Matrix.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022 René Rhéaume
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
using System.Text;

namespace Repzilon.Libraries.Core
{
	public struct Matrix<T> : ICloneable, IEquatable<Matrix<T>> where T : struct, IConvertible, IEquatable<T>
	{
		#region Static members
		private static readonly Func<T, T, T> add = BuildAdder();
		private static readonly Func<T, T, T> sub = BuildSubtractor();

		private static Func<T, T, T> BuildAdder()
		{
			// Declare the parameters
			var paramA = Expression.Parameter(typeof(T), "a");
			var paramB = Expression.Parameter(typeof(T), "b");

			// Add the parameters together
			BinaryExpression body = Expression.Add(paramA, paramB);

			// Compile it
			return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
		}

		private static Func<T, T, T> BuildSubtractor()
		{
			// Declare the parameters
			var paramA = Expression.Parameter(typeof(T), "a");
			var paramB = Expression.Parameter(typeof(T), "b");

			// Add the parameters together
			BinaryExpression body = Expression.Subtract(paramA, paramB);

			// Compile it
			return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
		}
		#endregion

		public readonly byte Lines;
		public readonly byte Columns;
		private readonly T[,] m_values;

		#region Constructors
		public Matrix(byte lines, byte columns)
		{
			Lines = lines;
			Columns = columns;
			m_values = new T[lines, columns];
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
			for (byte i = 0; i < size; i++) {
				m[i, i] = (T)Convert.ChangeType(1, typeof(T));
			}
			return m;
		}
		#endregion

		public T this[byte l, byte c] {
			get { return m_values[l, c]; }
			set { m_values[l, c] = value; }
		}

		#region ICloneable members
		public Matrix<T> Clone()
		{
			var other = new Matrix<T>(this.Lines, this.Columns);
			for (byte i = 0; i < this.Lines; i++) {
				for (byte j = 0; j < this.Columns; j++) {
					other[i, j] = this[i, j];
				}
			}
			return other;
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
		#endregion

		public static Matrix<TOut> Cast<TIn, TOut>(Matrix<TIn> source)
		where TIn : struct, IConvertible, IEquatable<TIn>
		where TOut : struct, IConvertible, IEquatable<TOut>
		{
			var other = new Matrix<TOut>(source.Lines, source.Columns);
			for (byte i = 0; i < source.Lines; i++) {
				for (byte j = 0; j < source.Columns; j++) {
					other[i, j] = (TOut)Convert.ChangeType(source[i, j], typeof(TOut));
				}
			}
			return other;
		}

		public Matrix<TOut> Cast<TOut>() where TOut : struct, IConvertible, IEquatable<TOut>
		{
			return Cast<T, TOut>(this);
		}

		#region Equals
		public bool Equals(Matrix<T> other)
		{
			if (this.SameSize(other)) {
				for (byte i = 0; i < this.Lines; i++) {
					for (byte j = 0; j < this.Columns; j++) {
						if (!other[i, j].Equals(this[i, j])) {
							return false;
						}
					}
				}
				return true;
			} else {
				return false;
			}
		}

		public override bool Equals(object obj)
		{
			return (obj != null) && (obj is Matrix<T>) && this.Equals((Matrix<T>)obj);
		}

		public override int GetHashCode()
		{
			int hashCode = 1317424563;
			hashCode = hashCode * -1521134295 + Lines.GetHashCode();
			hashCode = hashCode * -1521134295 + Columns.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<T[,]>.Default.GetHashCode(m_values);
			return hashCode;
		}

		public bool SameSize(Matrix<T> other)
		{
			return (other.Lines == this.Lines) && (other.Columns == this.Columns);
		}
		#endregion

		public override string ToString()
		{
			StringBuilder stbDesc = new StringBuilder();
			for (byte i = 0; i < this.Lines; i++) {
				if (this.Lines == 1) {
					stbDesc.Append('[');
				} else if (i == 0) {
					stbDesc.Append('/');
				} else if (i == this.Lines - 1) {
					stbDesc.Append('\\');
				} else {
					stbDesc.Append('|');
				}

				for (byte j = 0; j < this.Columns; j++) {
					if (j > 0) {
						stbDesc.Append('\t');
					}
					stbDesc.Append(this[i, j]);
				}

				if (this.Lines == 1) {
					stbDesc.Append(']');
				} else if (i == 0) {
					stbDesc.Append('\\');
				} else if (i == this.Lines - 1) {
					stbDesc.Append('/');
				} else {
					stbDesc.Append('|');
				}

				if (i < this.Lines - 1) {
					stbDesc.Append(Environment.NewLine);
				}
			}
			stbDesc.AppendFormat(" {0}x{1} of {2}", this.Lines, this.Columns, typeof(T));
			return stbDesc.ToString();
		}

		#region Operators
		public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
		{
			if (b.SameSize(a)) {
				var m = new Matrix<T>(a.Lines, a.Columns);
				for (byte i = 0; i < a.Lines; i++) {
					for (byte j = 0; j < a.Columns; j++) {
						m[i, j] = add(a[i, j], b[i, j]);
					}
				}
				return m;
			} else {
				throw new ArrayTypeMismatchException(String.Format(CultureInfo.CurrentCulture,
				 "To add matrices, their dimensions must be identical. They are {0}x{1} and {2}x{3}.",
				 a.Lines, a.Columns, b.Lines, b.Columns));
			}
		}		

		public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
		{
			if (b.SameSize(a)) {
				var m = new Matrix<T>(a.Lines, a.Columns);
				for (byte i = 0; i < a.Lines; i++) {
					for (byte j = 0; j < a.Columns; j++) {
						m[i, j] = sub(a[i, j], b[i, j]);
					}
				}
				return m;
			} else {
				throw new ArrayTypeMismatchException(String.Format(CultureInfo.CurrentCulture,
				 "To subtract matrices, their dimensions must be identical.",
				 a.Lines, a.Columns, b.Lines, b.Columns));
			}
		}

		internal static Func<TScalar, T, T> BuildMultiplier<TScalar>() where TScalar : struct, IConvertible, IEquatable<T>
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

		public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
		{
			if (a.Columns == b.Lines) {
				var mul = BuildMultiplier<T>();
				var c = new Matrix<T>(a.Lines, b.Columns);
				for (byte i = 0; i < c.Lines; i++) {
					for (byte j = 0; j < c.Columns; j++) {
						T sumOfCell = default(T);
						for (byte x = 0; x < b.Lines; x++) {
							sumOfCell = add(sumOfCell, mul(a[i,x], b[x,j]));
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
		#endregion
	}

	public static class MatrixExtensionMethods
	{
		public static Matrix<T> Multiply<T, TScalar>(this Matrix<T> m, TScalar k)
		where T : struct, IConvertible, IEquatable<T>
		where TScalar : struct, IConvertible, IEquatable<T>
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
	}
}
