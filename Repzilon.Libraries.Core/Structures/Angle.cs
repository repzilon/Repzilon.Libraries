//
//  Angle.cs
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
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core.Vectors
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct Angle<T> : IAngle, IEquatable<Angle<T>>, IComparable<Angle<T>>
	where T : struct, IFormattable, IComparable<T>, IEquatable<T>
	{
		#region Properties
		public T Value { get; private set; }
		public AngleUnit Unit { get; private set; }

		decimal IAngle.DecimalValue
		{
			get { return DecimalValue; }
		}

		private decimal DecimalValue
		{
			get { return Convert.ToDecimal(Value); }
		}
		#endregion

		#region Constructors
		public Angle(T value, AngleUnit unit) : this()
		{
			Value = value;
			if (AngleExtensions.IsDefined(unit)) {
				Unit = unit;
			} else {
				throw NewUnknownUnitException(unit);
			}
		}

		private static Angle<T> FromOtherType<TFrom>(TFrom valueInOtherDataType, AngleUnit unit)
		where TFrom : struct
		{
			return new Angle<T>(MatrixExtensionMethods.ConvertTo<T>(valueInOtherDataType), unit);
		}

		private static Angle<T> FromOtherType<TFrom>(TFrom valueInOtherDataType, double conversionFactor,
		AngleUnit unit)
		where TFrom : struct
		{
			return FromOtherType(Convert.ToDouble(valueInOtherDataType) * conversionFactor, unit);
		}
		#endregion

		#region ICloneable members
		public Angle(Angle<T> source) : this(source.Value, source.Unit) { }

		public Angle<T> Clone()
		{
			return new Angle<T>(Value, Unit);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		#region ConvertTo method
		IAngle IAngle.ConvertTo(AngleUnit unit)
		{
#if NET20
			return ConvertTo<decimal>(unit);
#else
			return ConvertTo<decimal>(unit, false);
#endif
		}

		public Angle<T> ConvertTo(AngleUnit unit)
		{
#if NET20
			return ConvertTo<T>(unit);
#else
			return ConvertTo<T>(unit, false);
#endif
		}

#if NET20
		public Angle<TOut> ConvertTo<TOut>(AngleUnit unit)
		where TOut : struct, IFormattable, IComparable<TOut>, IEquatable<TOut>
#else
		public Angle<TOut> ConvertTo<TOut>(AngleUnit unit, bool normalize)
		where TOut : struct, IFormattable, IComparable<TOut>, IEquatable<TOut>
#endif
		{
#if NET20
			var x = this;
#else
			var x = normalize ? Normalize() : this;
#endif
			var v = x.Value;
			var tu = this.Unit;
			if (unit == tu) {
				return Angle<TOut>.FromOtherType(v, unit);
			} else if (unit == AngleUnit.Degree) {
				if (tu == AngleUnit.Gradian) {
					return Angle<TOut>.FromOtherType(v, 0.9, unit);
				} else if (tu == AngleUnit.Radian) {
					const double kRad2Deg = 180 / Math.PI;
					return Angle<TOut>.FromOtherType(v, kRad2Deg, unit);
				} else {
					throw NewConversionException(unit);
				}
			} else if (unit == AngleUnit.Gradian) {
				if (tu == AngleUnit.Degree) {
					const double kDeg2Gon = 1.0 / 0.9;
					return Angle<TOut>.FromOtherType(v, kDeg2Gon, unit);
				} else if (tu == AngleUnit.Radian) {
					const double kRad2Gon = 200 / Math.PI;
					return Angle<TOut>.FromOtherType(v, kRad2Gon, unit);
				} else {
					throw NewConversionException(unit);
				}
			} else if (unit == AngleUnit.Radian) {
				if (tu == AngleUnit.Degree) {
					const double kDeg2Rad = Math.PI / 180;
					return Angle<TOut>.FromOtherType(v, kDeg2Rad, unit);
				} else if (tu == AngleUnit.Gradian) {
					const double kGon2Rad = Math.PI / 200;
					return Angle<TOut>.FromOtherType(v, kGon2Rad, unit);
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

#if NETCOREAPP1_0 || NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6
		private static ArgumentOutOfRangeException NewUnknownUnitException(AngleUnit unit)
		{
			return new ArgumentOutOfRangeException("unit", (int)unit, "Unknown angle unit.");
		}
#else
		private static InvalidEnumArgumentException NewUnknownUnitException(AngleUnit unit)
		{
			return new InvalidEnumArgumentException("unit", (int)unit, typeof(AngleUnit));
		}
#endif

		public Angle<TOut> Cast<TOut>()
		where TOut : struct, IFormattable, IComparable<TOut>, IEquatable<TOut>
		{
#if NET20
			return ConvertTo<TOut>(Unit);
#else
			return ConvertTo<TOut>(Unit, false);
#endif
		}
		#endregion

		#region Normalize method
#if !NET20
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
				angle = GenericArithmetic<T>.adder(angle, turn);
			}
			while (angle.CompareTo(turn) > 0) {
				angle = GenericArithmetic<T>.sub(angle, turn);
			}

			return new Angle<T>(angle, this.Unit);
		}
#endif
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
#if NET20
			 (Convert.ToDecimal(v) == other.ConvertTo<decimal>(u).Value);
#else
			 (Convert.ToDecimal(v) == other.ConvertTo<decimal>(u, false).Value);
#endif
		}

		public bool Equals(IAngle other)
		{
			var u = this.Unit;
			var v = this.DecimalValue;
			return (u == other.Unit) ? v.Equals(other.DecimalValue) : v.Equals(other.ConvertTo(u).DecimalValue);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (-177567199 * -1521134295) + Value.GetHashCode();
				return (hashCode * -1521134295) + (int)Unit;
			}
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
			var stbAngle = new StringBuilder();
			var tu = this.Unit;
			IFormattable tv = this.Value;
			stbAngle.Append(tv.ToString(format, formatProvider)).Append('\xA0');
			if (format.ToUpperInvariant() == format) { // is long unit name requested?
				stbAngle.Append(tu.ToString().ToLowerInvariant());
				if (Math.Abs(Convert.ToInt32(tv)) != 0) {
					stbAngle.Append('s');
				}
			} else if (tu == AngleUnit.Degree) {
				stbAngle.Append('°');
			} else if (tu == AngleUnit.Gradian) {
				stbAngle.Append("gon");
			} else if (tu == AngleUnit.Radian) {
				stbAngle.Append("rad");
			} else {
				stbAngle.AppendFormat(formatProvider, "({0})?", tu);
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
#if NET20
			 Math.Sign(Convert.ToDecimal(v) - other.ConvertTo<decimal>(u).Value);
#else
			 Math.Sign(Convert.ToDecimal(v) - other.ConvertTo<decimal>(u, false).Value);
#endif
		}

		public int CompareTo(IAngle other)
		{
			var u = this.Unit;
			var v = this.DecimalValue;
			return (u == other.Unit) ? v.CompareTo(other.DecimalValue) : Math.Sign(v - other.ConvertTo(u).DecimalValue);
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
#if NET20
			return Math.Sin(ConvertTo<double>(AngleUnit.Radian).Value);
#else
			return Math.Sin(ConvertTo<double>(AngleUnit.Radian, true).Value);
#endif
		}

		public double Cos()
		{
#if NET20
			return Math.Cos(ConvertTo<double>(AngleUnit.Radian).Value);
#else
			return Math.Cos(ConvertTo<double>(AngleUnit.Radian, true).Value);
#endif
		}
		#endregion

		#region Addition operator
#if !NET20
		public static IAngle operator +(Angle<T> x, Angle<T> y)
		{
			var u = x.Unit;
			if (u == y.Unit) {
				return new Angle<T>(GenericArithmetic<T>.adder(x.Value, y.Value), u);
			} else {
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian, false);
				var dy = y.ConvertTo<decimal>(AngleUnit.Radian, false);
				return new Angle<decimal>(dx.Value + dy.Value, AngleUnit.Radian);
			}
		}
#endif

		public static Angle<decimal> operator +(Angle<T> x, IAngle y)
		{
			var u = x.Unit;
			if (u == y.Unit) {
				return new Angle<decimal>(x.DecimalValue + y.DecimalValue, u);
			} else {
#if NET20
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian);
#else
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian, false);
#endif
				var dy = y.ConvertTo(AngleUnit.Radian);
				return new Angle<decimal>(dx.DecimalValue + dy.DecimalValue, AngleUnit.Radian);
			}
		}
		#endregion

		#region Subtraction operator
#if !NET20
		public static IAngle operator -(Angle<T> x, Angle<T> y)
		{
			var u = x.Unit;
			if (u == y.Unit) {
				return new Angle<T>(GenericArithmetic<T>.sub(x.Value, y.Value), u);
			} else {
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian, false);
				var dy = y.ConvertTo<decimal>(AngleUnit.Radian, false);
				return new Angle<decimal>(dx.Value - dy.Value, AngleUnit.Radian);
			}
		}
#endif

		public static Angle<decimal> operator -(Angle<T> x, IAngle y)
		{
			var u = x.Unit;
			if (u == y.Unit) {
				return new Angle<decimal>(x.DecimalValue - y.DecimalValue, u);
			} else {
#if NET20
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian);
#else
				var dx = x.ConvertTo<decimal>(AngleUnit.Radian, false);
#endif
				var dy = y.ConvertTo(AngleUnit.Radian);
				return new Angle<decimal>(dx.DecimalValue - dy.DecimalValue, AngleUnit.Radian);
			}
		}
		#endregion
	}

	public static class AngleExtensions
	{
#if NET20
		public static bool IsDefined(AngleUnit unit)
#else
		public static bool IsDefined(this AngleUnit unit)
#endif
		{
			return (unit == AngleUnit.Gradian) || (unit == AngleUnit.Degree) || (unit == AngleUnit.Radian);
		}

		#region Multiplications with possible enlargements
#if !NET20
		private static Angle<TAngle> MultiplyInteger<TAngle, TInteger>(Angle<TAngle> angle, TInteger multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		where TInteger : struct, IEquatable<TInteger>
		{
			var mult = GenericArithmetic<TAngle>.BuildMultiplier<TInteger>();
			return new Angle<TAngle>(mult(multiplier, angle.Value), angle.Unit);
		}

		public static Angle<TAngle> Multiply<TAngle>(this Angle<TAngle> angle, byte multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return MultiplyInteger(angle, multiplier);
		}

		public static Angle<TAngle> Multiply<TAngle>(this Angle<TAngle> angle, short multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return MultiplyInteger(angle, multiplier);
		}

		public static Angle<TAngle> Multiply<TAngle>(this Angle<TAngle> angle, int multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return MultiplyInteger(angle, multiplier);
		}

		public static Angle<TAngle> Multiply<TAngle>(this Angle<TAngle> angle, TAngle multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
		{
			return MultiplyInteger(angle, multiplier);
		}
#endif

#if NET20
		public static Angle<double> Multiply(Angle<float> angle, long multiplier)
#else
		public static Angle<double> Multiply(this Angle<float> angle, long multiplier)
#endif
		{
			return new Angle<double>(angle.Value * multiplier, angle.Unit);
		}

#if NET20
		public static Angle<double> Multiply(Angle<double> angle, long multiplier)
#else
		public static Angle<double> Multiply(this Angle<double> angle, long multiplier)
#endif
		{
			return new Angle<double>(angle.Value * multiplier, angle.Unit);
		}

#if NET20
		public static Angle<double> Multiply(Angle<long> angle, float multiplier)
#else
		public static Angle<double> Multiply(this Angle<long> angle, float multiplier)
#endif
		{
			return new Angle<double>(angle.Value * multiplier, angle.Unit);
		}

#if NET20
		public static Angle<float> Multiply(Angle<float> angle, float multiplier)
#else
		public static Angle<float> Multiply(this Angle<float> angle, float multiplier)
#endif
		{
			return new Angle<float>(angle.Value * multiplier, angle.Unit);
		}

#if NET20
		public static Angle<double> Multiply(Angle<double> angle, float multiplier)
#else
		public static Angle<double> Multiply(this Angle<double> angle, float multiplier)
#endif

		{
			return new Angle<double>(angle.Value * multiplier, angle.Unit);
		}

#if NET20
		public static Angle<decimal> Multiply<TMultiplier>(Angle<decimal> angle, TMultiplier multiplier)
		where TMultiplier : struct, IEquatable<TMultiplier>, IComparable<TMultiplier>
#else
		public static Angle<decimal> Multiply<TMultiplier>(this Angle<decimal> angle, TMultiplier multiplier)
		where TMultiplier : struct, IEquatable<TMultiplier>, IComparable<TMultiplier>
#endif
		{
			return new Angle<decimal>(angle.Value * Convert.ToDecimal(multiplier), angle.Unit);
		}

#if NET20
		public static Angle<long> Multiply<TAngle>(Angle<TAngle> angle, long multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
#else
		public static Angle<long> Multiply<TAngle>(this Angle<TAngle> angle, long multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
#endif
		{
			return new Angle<long>(Convert.ToInt64(angle.Value) * multiplier, angle.Unit);
		}

#if NET20
		public static Angle<float> Multiply<TAngle>(Angle<TAngle> angle, float multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
#else
		public static Angle<float> Multiply<TAngle>(this Angle<TAngle> angle, float multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
#endif
		{
			return new Angle<float>(Convert.ToSingle(angle.Value) * multiplier, angle.Unit);
		}

#if NET20
		public static Angle<double> Multiply<TAngle>(Angle<TAngle> angle, double multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
#else
		public static Angle<double> Multiply<TAngle>(this Angle<TAngle> angle, double multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
#endif
		{
			return new Angle<double>(Convert.ToDouble(angle.Value) * multiplier, angle.Unit);
		}

#if NET20
		public static Angle<decimal> Multiply<TAngle>(Angle<TAngle> angle, decimal multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
#else
		public static Angle<decimal> Multiply<TAngle>(this Angle<TAngle> angle, decimal multiplier)
		where TAngle : struct, IFormattable, IEquatable<TAngle>, IComparable<TAngle>
#endif
		{
			return new Angle<decimal>(Convert.ToDecimal(angle.Value) * multiplier, angle.Unit);
		}
		#endregion
	}
}
