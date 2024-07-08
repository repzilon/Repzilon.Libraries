//
//  Exp.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2024 René Rhéaume
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
	// TODO : Implement addition and subtraction operators
	// TODO : Add method Parse
	// TODO : Add method TryParse
	// TODO : Create a new version with a signed 18-bit mantissa (yes 18),
	//		  a 6-bit adjusted base ([2; 65] stored as [0; 63]) and the
	//        same signed 8-bit exponent, for more precision.
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	[CLSCompliant(false)]
	public struct Exp : IComparable, IFormattable, IEquatable<Exp>, IComparable<Exp>,
	IEquatable<double>, IEquatable<decimal>,
	IComparable<double>, IComparable<decimal>
#if !NETSTANDARD1_1
	, IEquatable<IConvertible>, IComparable<IConvertible>
#endif
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	{
		private readonly short mantissaThousandths;
		public readonly byte Base;
		public readonly SByte Exponent;

		public float Mantissa
		{
			get { return (float)(mantissaThousandths * 0.001); }
		}

		public Exp(float mantissa, byte numericBase, SByte exponent)
		{
			if (numericBase < 2) {
				throw new ArgumentOutOfRangeException("numericBase", numericBase, "A numeric base of 0 or 1 does not make sense.");
			}
			if ((mantissa <= -10) || (mantissa >= 10)) {
				throw new ArgumentOutOfRangeException("mantissa", mantissa, "Absolute value of the mantissa must be under 10.");
			}
			mantissaThousandths = Convert.ToInt16(mantissa * 1000);
			Base = numericBase;
			Exponent = exponent;
		}

		private Exp(short mantissa, byte numericBase, SByte exponent)
		{
			mantissaThousandths = mantissa;
			Base = numericBase;
			Exponent = exponent;
		}

		#region ICloneable members
		public Exp(Exp source) : this(source.mantissaThousandths, source.Base, source.Exponent) { }

		public Exp Clone()
		{
			return new Exp(this);
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
			if (obj is Exp) {
				return Equals((Exp)obj);
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

		public bool Equals(Exp other)
		{
			return (mantissaThousandths == other.mantissaThousandths) &&
			 (Base == other.Base) && (Exponent == other.Exponent);
		}

		public bool Equals(double other)
		{
			return this.ToDouble() == other;
		}

		public bool Equals(decimal other)
		{
			return this.ToDecimal() == other;
		}

#if !NETSTANDARD1_1
		public bool Equals(IConvertible other)
		{
			return (other != null) && (this.ToDouble() == Convert.ToDouble(other));
		}
#endif

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (1362180524 * -1521134295) + mantissaThousandths;
				hashCode = (hashCode * -1521134295) + Base;
				return (hashCode * -1521134295) + Exponent;
			}
		}

		public static bool operator ==(Exp left, Exp right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Exp left, Exp right)
		{
			return !(left == right);
		}
		#endregion

		public double ToDouble()
		{
			return 0.001 * mantissaThousandths * ExtraMath.Pow(this.Base, this.Exponent);
		}

		public decimal ToDecimal()
		{
			return 0.001m * mantissaThousandths * (decimal)ExtraMath.Pow(this.Base, this.Exponent);
		}

		public static Exp operator *(Exp x, Exp y)
		{
			var b = x.Base;
			if (y.Base != b) {
				throw new ArgumentException("Base must be identical");
			} else {
				return AdjustMantissaExponent(x.Mantissa * y.Mantissa, b, x.Exponent + y.Exponent);
			}
		}

		public static Exp operator /(Exp x, Exp y)
		{
			var b = x.Base;
			if (y.Base != b) {
				throw new ArgumentException("Base must be identical");
			} else {
				return AdjustMantissaExponent(x.Mantissa / y.Mantissa, b, x.Exponent - y.Exponent);
			}
		}

		private static Exp AdjustMantissaExponent(float m2, byte b, int e2)
		{
			if ((m2 <= -b) || (m2 >= b) || ((m2 > -1) && (m2 < 1))) {
				var magnitude = (int)Math.Floor(Math.Log(Math.Abs(m2), b));
				e2 += magnitude;
				m2 = (float)(m2 * ExtraMath.Pow(b, (SByte)(-magnitude)));
			}
			return new Exp(m2, b, (SByte)e2);
		}

		#region IComparable members
		public int CompareTo(object obj)
		{
			if (obj is Exp) {
				return CompareTo((Exp)obj);
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

		public int CompareTo(Exp other)
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

		public static bool operator <(Exp left, Exp right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(Exp left, Exp right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >(Exp left, Exp right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(Exp left, Exp right)
		{
			return left.CompareTo(right) >= 0;
		}

		public static bool operator <(Exp left, double right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(Exp left, double right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >(Exp left, double right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(Exp left, double right)
		{
			return left.CompareTo(right) >= 0;
		}

		public static bool operator <(Exp left, decimal right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(Exp left, decimal right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >(Exp left, decimal right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(Exp left, decimal right)
		{
			return left.CompareTo(right) >= 0;
		}

#if !NETSTANDARD1_1
		public static bool operator <(Exp left, IConvertible right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(Exp left, IConvertible right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >(Exp left, IConvertible right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(Exp left, IConvertible right)
		{
			return left.CompareTo(right) >= 0;
		}
#endif
		#endregion
	}
}
