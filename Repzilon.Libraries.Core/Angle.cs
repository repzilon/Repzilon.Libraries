//
//  Angle.cs
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
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	public enum AngleUnit : byte
	{
		Gradian = 0,
		Degree = 1,
		Radian = 57
	}

	// TODO : Implement IComparable<Angle<T>> to Angle<T>
	// TODO : Implement IComparable to Angle<T>
	[StructLayout(LayoutKind.Auto)]
	public struct Angle<T> : ICloneable, IEquatable<Angle<T>>, IFormattable
	where T : struct, IConvertible, IComparable<T>, IEquatable<T>
	{
		public readonly T Value;
		public readonly AngleUnit Unit;

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

		public Angle<TOut> ConvertTo<TOut>(AngleUnit unit, bool normalize)
		where TOut : struct, IConvertible, IComparable<TOut>, IEquatable<TOut>
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

		#region Equals
		public override bool Equals(object obj)
		{
			// TODO : Support comparing angles using other type of storage
			return (obj != null) && (obj is Angle<T>) && this.Equals((Angle<T>)obj);
		}

		public bool Equals(Angle<T> other)
		{
			if (this.Unit == other.Unit) {
				return this.Value.Equals(other.Value);
			} else { // TODO : Check what defines mathematical equality
				return false;
			}
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
		/// <param name="formatProvider">Culture under which the number will be formatted</param>
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
			stbAngle.AppendFormat(formatProvider, format, this.Value).Append('\xA0');
			if (blnLongUnitName) {
				stbAngle.Append(this.Unit);
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

		// TODO : implement + operator with angle
		// TODO : implement - operator with angle
		// TODO : implement * operator with scalar
	}

	public static class AngleExtensions
	{
		public static bool IsDefined(this AngleUnit unit)
		{
			return (unit == AngleUnit.Gradian) || (unit == AngleUnit.Degree) || (unit == AngleUnit.Radian);
		}
	}
}

