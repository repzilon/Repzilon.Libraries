﻿//
//  Angle.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2023 René Rhéaume
//
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v. 2.0. If a copy of the MPL was 
// not distributed with this file, You can obtain one at 
// https://mozilla.org/MPL/2.0/.
// 
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Repzilon.Libraries.Core
{
	public enum AngleUnit : byte
	{
		Gradian = 0,
		Degree = 1,
		Radian = 57
	}

	public interface IAngle : IComparable, ICloneable, IFormattable, IEquatable<IAngle>, IComparable<IAngle>
	{
		AngleUnit Unit { get; }
		decimal DecimalValue { get; }
		IAngle ConvertTo(AngleUnit unit);
		IAngle Normalize();
		double Sin();
		double Cos();
	}

	[StructLayout(LayoutKind.Auto)]
	public struct Angle<T> : IAngle, IEquatable<Angle<T>>, IComparable<Angle<T>>
	where T : struct, IConvertible, IFormattable, IComparable<T>, IEquatable<T>
	{
		#region Properties
		public T Value { get; private set; }
		public AngleUnit Unit { get; private set; }

		decimal IAngle.DecimalValue
		{
			get { return this.DecimalValue; }
		}

		private decimal DecimalValue
		{
			get { return Convert.ToDecimal(this.Value); }
		}
		#endregion

		#region Constructors
		public Angle(T value, AngleUnit unit)
		{
			Value = value;
			if (unit.IsDefined()) {
				Unit = unit;
			} else {
				throw NewUnknownUnitException(unit);
			}
		}

		private static Angle<T> FromOtherType<TFrom>(TFrom valueInOtherDataType, AngleUnit unit)
		where TFrom : struct, IConvertible, IEquatable<TFrom>
		{
			return new Angle<T>(valueInOtherDataType.ConvertTo<T>(), unit);
		}

		private static Angle<T> FromOtherType<TFrom>(TFrom valueInOtherDataType, double conversionFactor, AngleUnit unit)
		where TFrom : struct, IConvertible, IEquatable<TFrom>
		{
			return FromOtherType(Convert.ToDouble(valueInOtherDataType) * conversionFactor, unit);
		}
		#endregion

		#region ICloneable members
		public Angle<T> Clone()
		{
			return new Angle<T>(Value, Unit);
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
		#endregion

		#region ConvertTo method
		IAngle IAngle.ConvertTo(AngleUnit unit)
		{
			return this.ConvertTo<decimal>(unit, false);
		}

		public Angle<T> ConvertTo(AngleUnit unit)
		{
			return this.ConvertTo<T>(unit, false);
		}

		public Angle<TOut> ConvertTo<TOut>(AngleUnit unit, bool normalize)
		where TOut : struct, IConvertible, IFormattable, IComparable<TOut>, IEquatable<TOut>
		{
			var x = normalize ? this.Normalize() : this;
			if (unit == this.Unit) {
				return Angle<TOut>.FromOtherType(x.Value, unit);
			} else if (unit == AngleUnit.Degree) {
				if (this.Unit == AngleUnit.Gradian) {
					return Angle<TOut>.FromOtherType(x.Value, 0.9, unit);
				} else if (this.Unit == AngleUnit.Radian) {
					const double kRad2Deg = 180 / Math.PI;
					return Angle<TOut>.FromOtherType(x.Value, kRad2Deg, unit);
				} else {
					throw NewConversionException(unit);
				}
			} else if (unit == AngleUnit.Gradian) {
				if (this.Unit == AngleUnit.Degree) {
					const double kDeg2Gon = 1.0 / 0.9;
					return Angle<TOut>.FromOtherType(x.Value, kDeg2Gon, unit);
				} else if (this.Unit == AngleUnit.Radian) {
					const double kRad2Gon = 200 / Math.PI;
					return Angle<TOut>.FromOtherType(x.Value, kRad2Gon, unit);
				} else {
					throw NewConversionException(unit);
				}
			} else if (unit == AngleUnit.Radian) {
				if (this.Unit == AngleUnit.Degree) {
					const double kDeg2Rad = Math.PI / 180;
					return Angle<TOut>.FromOtherType(x.Value, kDeg2Rad, unit);
				} else if (this.Unit == AngleUnit.Gradian) {
					const double kGon2Rad = Math.PI / 200;
					return Angle<TOut>.FromOtherType(x.Value, kGon2Rad, unit);
				} else {
					throw NewConversionException(unit);
				}
			} else {
				throw NewUnknownUnitException(unit);
			}
		}

		private NotSupportedException NewConversionException(AngleUnit destinationUnit)
		{
			throw new NotSupportedException(String.Format(
			 "Cannot convert angle from {0} to {1}.", this.Unit, destinationUnit));
		}

		private static InvalidEnumArgumentException NewUnknownUnitException(AngleUnit unit)
		{
			return new InvalidEnumArgumentException("unit", (int)unit, typeof(AngleUnit));
		}

		public Angle<TOut> Cast<TOut>()
		where TOut : struct, IConvertible, IFormattable, IComparable<TOut>, IEquatable<TOut>
		{
			return this.ConvertTo<TOut>(this.Unit, false);
		}
		#endregion

		#region Normalize method
		IAngle IAngle.Normalize()
		{
			return this.Normalize();
		}

		public Angle<T> Normalize()
		{
			var angle = this.Value;
			var zero = default(T);
			T turn;

			if (this.Unit == AngleUnit.Degree) {
				turn = 360.ConvertTo<T>();
			} else if (this.Unit == AngleUnit.Gradian) {
				turn = 400.ConvertTo<T>();
			} else if (this.Unit == AngleUnit.Radian) {
				turn = (2 * Math.PI).ConvertTo<T>();
			} else {
				throw new InvalidOperationException();
			}

			while (angle.CompareTo(zero) < 0) {
				angle = Matrix<T>.add(angle, turn);
			}
			while (angle.CompareTo(turn) > 0) {
				angle = Matrix<T>.sub(angle, turn);
			}

			return new Angle<T>(angle, this.Unit);
		}
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			if (obj is Angle<T>) {
				return this.Equals((Angle<T>)obj);
			} else {
				var angOther = obj as IAngle;
				return (angOther != null) && this.Equals(angOther);
			}
		}

		public bool Equals(Angle<T> other)
		{
			var u = this.Unit;
			var v = this.Value;
			return (u == other.Unit) ? v.Equals(other.Value) :
			 (Convert.ToDecimal(v) == other.ConvertTo<decimal>(u, false).Value);
		}

		public bool Equals(IAngle other)
		{
			var u = this.Unit;
			decimal v = this.DecimalValue;
			return (u == other.Unit) ? v.Equals(other.DecimalValue) :
			 v.Equals(other.ConvertTo(u).DecimalValue);
		}

		public override int GetHashCode()
		{
			int hashCode = -177567199;
			hashCode = hashCode * -1521134295 + Value.GetHashCode();
			hashCode = hashCode * -1521134295 + Unit.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Angle<T> left, Angle<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Angle<T> left, Angle<T> right)
		{
			return !(left == right);
		}

		public static bool operator ==(Angle<T> left, IAngle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Angle<T> left, IAngle right)
		{
			return !(left == right);
		}
		#endregion

		#region ToString
		/// <summary>
		/// Express the angle as a string.
		/// </summary>
		/// <returns>The value of the angle using the general numeric format in the current culture, followed by full name of the unit in English.</returns>
		public override string ToString()
		{
			return ToString(null, null);
		}

		/// <summary>
		/// Express the angle as a string.
		/// </summary>
		/// <param name="format">A .NET numeric format (standard or custom) specifier. If uppercase, full unit name in English will be used. Otherwise, the symbol will be used.</param>
		/// <param name="formatProvider">Culture under which the number will be formatted. Full unit names are not translated.</param>
		/// <returns>The value of the angle formatted as specified, followed by the symbol or the name of the unit.</returns>
		/// <remarks>
		/// The full unit name can be in plural according to English language grammar rules.
		/// </remarks>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrWhiteSpace(format)) {
				format = "G";
			}
			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}
			bool blnLongUnitName = (format.ToUpperInvariant() == format);
			StringBuilder stbAngle = new StringBuilder();
			stbAngle.Append(this.Value.ToString(format, formatProvider)).Append('\xA0');
			if (blnLongUnitName) {
				stbAngle.Append(this.Unit.ToString().ToLowerInvariant());
				if (Math.Abs(Convert.ToInt32(this.Value)) != 0) {
					stbAngle.Append('s');
				}
			} else if (this.Unit == AngleUnit.Degree) {
				stbAngle.Append('°');
			} else if (this.Unit == AngleUnit.Gradian) {
				stbAngle.Append("gon");
			} else if (this.Unit == AngleUnit.Radian) {
				stbAngle.Append("rad");
			} else {
				stbAngle.AppendFormat(formatProvider, "({0})?", this.Unit);
			}
			return stbAngle.ToString();
		}
		#endregion

		#region CompareTo
		public int CompareTo(Angle<T> other)
		{
			var u = this.Unit;
			var v = this.Value;
			return (u == other.Unit) ? v.CompareTo(other.Value) :
			 Math.Sign(Convert.ToDecimal(v) - other.ConvertTo<decimal>(u, false).Value);
		}

		public int CompareTo(IAngle other)
		{
			var u = this.Unit;
			decimal v = this.DecimalValue;
			return (u == other.Unit) ? v.CompareTo(other.DecimalValue) :
			 Math.Sign(v - other.ConvertTo(u).DecimalValue);
		}

		public int CompareTo(object obj)
		{
			if (obj is Angle<T>) {
				return this.CompareTo((Angle<T>)obj);
			} else {
				var angOther = obj as IAngle;
				return (angOther == null) ? 1 : this.CompareTo(angOther);
			}
		}

		public static bool operator >(Angle<T> operand1, Angle<T> operand2)
		{
			return operand1.CompareTo(operand2) > 0;
		}

		public static bool operator <(Angle<T> operand1, Angle<T> operand2)
		{
			return operand1.CompareTo(operand2) < 0;
		}

		public static bool operator >=(Angle<T> operand1, Angle<T> operand2)
		{
			return operand1.CompareTo(operand2) >= 0;
		}

		public static bool operator <=(Angle<T> operand1, Angle<T> operand2)
		{
			return operand1.CompareTo(operand2) <= 0;
		}

		public static bool operator >(Angle<T> operand1, IAngle operand2)
		{
			return operand1.CompareTo(operand2) > 0;
		}

		public static bool operator <(Angle<T> operand1, IAngle operand2)
		{
			return operand1.CompareTo(operand2) < 0;
		}

		public static bool operator >=(Angle<T> operand1, IAngle operand2)
		{
			return operand1.CompareTo(operand2) >= 0;
		}

		public static bool operator <=(Angle<T> operand1, IAngle operand2)
		{
			return operand1.CompareTo(operand2) <= 0;
		}
		#endregion

		#region Trigonometric functions
		public double Sin()
		{
			return Math.Sin(this.ConvertTo<double>(AngleUnit.Radian, true).Value);
		}

		public double Cos()
		{
			return Math.Cos(this.ConvertTo<double>(AngleUnit.Radian, true).Value);
		}
		#endregion

		#region Addition operator
		public static IAngle operator +(Angle<T> x, Angle<T> y)
		{
			var u = x.Unit;
			if (u == y.Unit) {
				return new Angle<T>(Matrix<T>.add(x.Value, y.Value), u);
			} else {
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian, false);
				var dy = y.ConvertTo<decimal>(AngleUnit.Radian, false);
				return new Angle<decimal>(dx.Value + dy.Value, AngleUnit.Radian);
			}
		}

		public static Angle<decimal> operator +(Angle<T> x, IAngle y)
		{
			var u = x.Unit;
			if (u == y.Unit) {
				return new Angle<decimal>(x.DecimalValue + y.DecimalValue, u);
			} else {
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian, false);
				var dy = y.ConvertTo(AngleUnit.Radian);
				return new Angle<decimal>(dx.DecimalValue + dy.DecimalValue, AngleUnit.Radian);
			}
		}
		#endregion

		#region Subtraction operator
		public static IAngle operator -(Angle<T> x, Angle<T> y)
		{
			var u = x.Unit;
			if (u == y.Unit) {
				return new Angle<T>(Matrix<T>.sub(x.Value, y.Value), u);
			} else {
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian, false);
				var dy = y.ConvertTo<decimal>(AngleUnit.Radian, false);
				return new Angle<decimal>(dx.Value - dy.Value, AngleUnit.Radian);
			}
		}

		public static Angle<decimal> operator -(Angle<T> x, IAngle y)
		{
			var u = x.Unit;
			if (u == y.Unit) {
				return new Angle<decimal>(x.DecimalValue - y.DecimalValue, u);
			} else {
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian, false);
				var dy = y.ConvertTo(AngleUnit.Radian);
				return new Angle<decimal>(dx.DecimalValue - dy.DecimalValue, AngleUnit.Radian);
			}
		}
		#endregion
	}

	public static class AngleExtensions
	{
		public static bool IsDefined(this AngleUnit unit)
		{
			return (unit == AngleUnit.Gradian) || (unit == AngleUnit.Degree) || (unit == AngleUnit.Radian);
		}

		#region Multiplications with possible enlargements
		private static Angle<TAngle> MultiplyInteger<TAngle, TInteger>(Angle<TAngle> angle, TInteger multiplier)
		where TAngle : struct, IConvertible, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		where TInteger : struct, IConvertible, IEquatable<TInteger>
		{
			var mul = Matrix<TAngle>.BuildMultiplier<TInteger>();
			return new Angle<TAngle>(mul(multiplier, angle.Value), angle.Unit);
		}

		public static Angle<TAngle> Multiply<TAngle>(this Angle<TAngle> angle, byte multiplier)
		where TAngle : struct, IConvertible, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return MultiplyInteger(angle, multiplier);
		}

		public static Angle<TAngle> Multiply<TAngle>(this Angle<TAngle> angle, short multiplier)
		where TAngle : struct, IConvertible, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return MultiplyInteger(angle, multiplier);
		}

		public static Angle<TAngle> Multiply<TAngle>(this Angle<TAngle> angle, int multiplier)
		where TAngle : struct, IConvertible, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return MultiplyInteger(angle, multiplier);
		}

		public static Angle<TAngle> Multiply<TAngle>(this Angle<TAngle> angle, TAngle multiplier)
		where TAngle : struct, IConvertible, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return MultiplyInteger(angle, multiplier);
		}

		public static Angle<double> Multiply(this Angle<float> angle, long multiplier)
		{
			return new Angle<double>(angle.Value * multiplier, angle.Unit);
		}

		public static Angle<double> Multiply(this Angle<double> angle, long multiplier)
		{
			return new Angle<double>(angle.Value * multiplier, angle.Unit);
		}

		public static Angle<double> Multiply(this Angle<long> angle, float multiplier)
		{
			return new Angle<double>(angle.Value * multiplier, angle.Unit);
		}

		public static Angle<float> Multiply(this Angle<float> angle, float multiplier)
		{
			return new Angle<float>(angle.Value * multiplier, angle.Unit);
		}

		public static Angle<double> Multiply(this Angle<double> angle, float multiplier)
		{
			return new Angle<double>(angle.Value * multiplier, angle.Unit);
		}

		public static Angle<decimal> Multiply<TMultiplier>(this Angle<decimal> angle, TMultiplier multiplier)
		where TMultiplier : struct, IConvertible, IEquatable<TMultiplier>, IComparable<TMultiplier>
		{
			return new Angle<decimal>(angle.Value * Convert.ToDecimal(multiplier), angle.Unit);
		}

		public static Angle<long> Multiply<TAngle>(this Angle<TAngle> angle, long multiplier)
		where TAngle : struct, IConvertible, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return new Angle<long>(Convert.ToInt64(angle.Value) * multiplier, angle.Unit);
		}

		public static Angle<float> Multiply<TAngle>(this Angle<TAngle> angle, float multiplier)
		where TAngle : struct, IConvertible, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return new Angle<float>(Convert.ToSingle(angle.Value) * multiplier, angle.Unit);
		}

		public static Angle<double> Multiply<TAngle>(this Angle<TAngle> angle, double multiplier)
		where TAngle : struct, IConvertible, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return new Angle<double>(Convert.ToDouble(angle.Value) * multiplier, angle.Unit);
		}

		public static Angle<decimal> Multiply<TAngle>(this Angle<TAngle> angle, decimal multiplier)
		where TAngle : struct, IConvertible, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return new Angle<decimal>(Convert.ToDecimal(angle.Value) * multiplier, angle.Unit);
		}
		#endregion
	}
}
