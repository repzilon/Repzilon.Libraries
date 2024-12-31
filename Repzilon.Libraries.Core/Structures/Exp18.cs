//
//  Exp18.cs
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
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core
{
	// TODO : Add method Parse
	// TODO : Add method TryParse
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	[CLSCompliant(false)]
	public struct Exp18 : IComparable, IFormattable, IEquatable<Exp18>, IComparable<Exp18>,
	IEquatable<double>, IEquatable<decimal>,
	IComparable<double>, IComparable<decimal>
#if !NETSTANDARD1_1
	, IEquatable<IConvertible>, IComparable<IConvertible>, IConvertible
#endif
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	{
		/// <summary>
		/// Layout : Highest 8 bits for exponent (thus the Int32 sign bit is the sign bit of the exponent), acts like SByte,
		/// middle 6 bits for an unsigned windowed radix ([2; 65] stored as [0; 63])
		/// and lower 18 bits for mantissa (top bit is sign bit, stores mantissa as non-complemented 1/100000ths,
		/// whereas old Exp stores mantissa as 1/1000ths in two's complement Int16)
		/// </summary>
		/// <remarks>
		/// Little endianness is assumed (all x86, x86_64, most ARM and AArch64). Would probably do not so funny stuff
		/// with old big endian PowerPC Macs on Mono (and Xbox 360 on Compact Framework or Mono?).
		/// </remarks>
		private readonly Int32 data;

		public int MantissaTenThousandths
		{
			get {
				var mut = data & 0x1ffff;
				return ((data & 0x20000) != 0) ? mut * -1 : mut;
			}
		}

		public byte Base
		{
			get { return (byte)(((data & 0xfc0000) >> 18) + 2); }
		}

		public SByte Exponent
		{
			get { return (SByte)((data & 0xff000000) >> 24); }
		}

		public float Mantissa
		{
			get { return RoundOff.Error(MantissaTenThousandths * 0.0001f); }
		}

		private bool IsZero()
		{
			return MantissaTenThousandths == 0;
		}

		public Exp18(float mantissa, byte numericBase, SByte exponent)
		{
			Exp.CheckForInit(mantissa, numericBase);
			if (numericBase > 65) {
				throw new ArgumentOutOfRangeException("numericBase", numericBase,
				 "A base bigger than 65 cannot be used with Exp18.");
			}
			data = Pack(Convert.ToInt32(mantissa * 10000), numericBase, exponent);
		}

		public Exp18(double floatingNumber)
		{
			var exponent = Convert.ToSByte(Math.Floor(Math.Log10(Math.Abs(floatingNumber))));
			var mantissa = floatingNumber * Math.Pow(10, -1 * exponent);
			data = Pack(Convert.ToInt32(mantissa * 10000), 10, exponent);
		}

		private Exp18(int mantissa, byte numericBase, SByte exponent)
		{
			data = Pack(mantissa, numericBase, exponent);
		}

		private static int Pack(int mantissa, byte numericBase, SByte exponent)
		{
#if DEBUG
			int newData = (exponent << 24);
			newData |= ((numericBase - 2) << 18);
			newData |= (Math.Abs(mantissa) & 0x1ffff);
			newData |= (mantissa < 0 ? 0x20000 : 0);
			return newData;
#else
			return (exponent << 24) | ((numericBase - 2) << 18) | (Math.Abs(mantissa) & 0x1ffff) |
				   (mantissa < 0 ? 0x20000 : 0);
#endif
		}

		#region ICloneable members
		public Exp18(Exp18 source) : this(source.MantissaTenThousandths, source.Base, source.Exponent) { }

		public Exp18 Clone()
		{
			return new Exp18(this);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return Clone();
		}
#endif
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
			// Special case for [Ee][0-9]* format strings
			if ((format[0] == 'e') || (format[0] == 'E')) {
				return this.ToDecimal().ToString(format, formatProvider);
			} else {
				var stbExp = new StringBuilder();
				stbExp.Append(this.Mantissa.ToString(format, formatProvider)).Append(" x ");
				stbExp.Append(this.Base).Append('^').Append(this.Exponent);
				return stbExp.ToString();
			}
		}
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			if (obj is Exp18) {
				return Equals((Exp18)obj);
			} else if (obj is double) {
				return Equals((double)obj);
			} else if (obj is decimal) {
				return Equals((decimal)obj);
			} else {
#if NETSTANDARD1_1
				return false;
#else
				return Equals(obj as IConvertible);
#endif
			}
		}

		public bool Equals(Exp18 other)
		{
			return (data == other.data);
		}

		public bool Equals(double other)
		{
			return RoundOff.Equals(this.ToDouble(), other);
		}

		public bool Equals(decimal other)
		{
			return this.ToDecimal() == other;
		}

#if !NETSTANDARD1_1
		public bool Equals(IConvertible other)
		{
			return (other != null) && RoundOff.Equals(this.ToDouble(), Convert.ToDouble(other));
		}
#endif

		public override int GetHashCode()
		{
			return data;
		}

		public static bool operator ==(Exp18 left, Exp18 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Exp18 left, Exp18 right)
		{
			return !(left == right);
		}
		#endregion

		public double ToDouble()
		{
			return 0.0001 * MantissaTenThousandths * ExtraMath.Pow(this.Base, this.Exponent);
		}

		public decimal ToDecimal()
		{
			return 0.0001m * MantissaTenThousandths * (decimal)ExtraMath.Pow(this.Base, this.Exponent);
		}

		public static Exp18 operator +(Exp18 x, Exp18 y)
		{
			if (x.IsZero()) {
				return y;
			} else if (y.IsZero()) {
				return x;
			} else {
				var b = x.Base;
				var p = x.Exponent;
				if ((y.Base == b) && (y.Exponent == p)) {
					return AdjustMantissaExponent(x.Mantissa + y.Mantissa, b, p);
				} else if (y.Base == b) {
					if (p > y.Exponent) {
						return AdjustMantissaExponent(x.Mantissa * (float)Math.Pow(b, p - y.Exponent) + y.Mantissa, b, y.Exponent);
					} else {
						return AdjustMantissaExponent(x.Mantissa + y.Mantissa * (float)Math.Pow(b, y.Exponent - p), b, p);
					}
				} else {
					throw new ArgumentException("Base and exponent must be identical");
				}
			}
		}

		public static Exp18 operator -(Exp18 x, Exp18 y)
		{
			if (x.IsZero()) {
				throw new NotSupportedException();
			} else if (y.IsZero()) {
				return x;
			} else {
				var b = x.Base;
				var p = x.Exponent;
				if ((y.Base != b) || (y.Exponent != p)) {
					throw new ArgumentException("Base and exponent must be identical");
				} else {
					return AdjustMantissaExponent(x.Mantissa - y.Mantissa, b, p);
				}
			}
		}

		public static Exp18 operator *(Exp18 x, Exp18 y)
		{
			if (x.IsZero()) {
				return x;
			} else if (y.IsZero()) {
				return y;
			} else {
				var b = x.Base;
				if (y.Base != b) {
					throw new ArgumentException("Base must be identical");
				} else {
					return AdjustMantissaExponent(x.Mantissa * y.Mantissa, b, x.Exponent + y.Exponent);
				}
			}
		}

		public static Exp18 operator /(Exp18 x, Exp18 y)
		{
			if (y.IsZero()) {
				throw new DivideByZeroException();
			} else if (x.IsZero()) {
				return x;
			} else {
				var b = x.Base;
				if (y.Base != b) {
					throw new ArgumentException("Base must be identical");
				} else {
					return AdjustMantissaExponent(x.Mantissa / y.Mantissa, b, x.Exponent - y.Exponent);
				}
			}
		}

		private static Exp18 AdjustMantissaExponent(float m2, byte b, int e2)
		{
			if ((m2 <= -b) || (m2 >= b) || ((m2 > -1) && (m2 < 1))) {
				var magnitude = (int)Math.Floor(Math.Log(Math.Abs(m2), b));
				e2 += magnitude;
				m2 = (float)(m2 * ExtraMath.Pow(b, (SByte)(-magnitude)));
			}
			return new Exp18(m2, b, (SByte)e2);
		}

		#region IComparable members
		public int CompareTo(object obj)
		{
			if (obj is Exp18) {
				return CompareTo((Exp18)obj);
			} else if (obj is double) {
				return CompareTo((double)obj);
			} else if (obj is decimal) {
				return CompareTo((decimal)obj);
			} else {
#if NETSTANDARD1_1
				return 1;
#else
				return CompareTo(obj as IConvertible);
#endif
			}
		}

		public int CompareTo(Exp18 other)
		{
			return this.ToDouble().CompareTo(other.ToDouble());
		}

		public int CompareTo(double other)
		{
			return this.ToDouble().CompareTo(other);
		}

		public int CompareTo(decimal other)
		{
			return this.ToDecimal().CompareTo(other);
		}

#if !NETSTANDARD1_1
		public int CompareTo(IConvertible other)
		{
			return this.ToDouble().CompareTo(Convert.ToDouble(other));
		}
#endif

		public static bool operator <(Exp18 left, Exp18 right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(Exp18 left, Exp18 right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >(Exp18 left, Exp18 right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(Exp18 left, Exp18 right)
		{
			return left.CompareTo(right) >= 0;
		}

		public static bool operator <(Exp18 left, double right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(Exp18 left, double right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >(Exp18 left, double right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(Exp18 left, double right)
		{
			return left.CompareTo(right) >= 0;
		}

		public static bool operator <(Exp18 left, decimal right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(Exp18 left, decimal right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >(Exp18 left, decimal right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(Exp18 left, decimal right)
		{
			return left.CompareTo(right) >= 0;
		}

#if !NETSTANDARD1_1
		public static bool operator <(Exp18 left, IConvertible right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(Exp18 left, IConvertible right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >(Exp18 left, IConvertible right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(Exp18 left, IConvertible right)
		{
			return left.CompareTo(right) >= 0;
		}
#endif
		#endregion

		#region IConvertible members
#if !NETSTANDARD1_1
		TypeCode IConvertible.GetTypeCode()
		{
			return TypeCode.Object;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this.ToDecimal());
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this.ToDecimal());
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return Convert.ToChar(this.ToDecimal());
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return Convert.ToDateTime(this.ToDecimal());
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return this.ToDecimal();
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return this.ToDouble();
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this.ToDecimal());
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this.ToDecimal());
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this.ToDecimal());
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this.ToDecimal());
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return (float)this.ToDouble();
		}

		string IConvertible.ToString(IFormatProvider provider)
		{
			return this.ToString("g", provider);
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this.ToDecimal());
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this.ToDecimal());
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this.ToDecimal());
		}
#endif
		#endregion
	}
}
