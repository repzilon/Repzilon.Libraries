//
//  Exp.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023 René Rhéaume
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
	[StructLayout(LayoutKind.Auto), CLSCompliant(false)]
	public struct Exp : IFormattable, IEquatable<Exp>
	{
		private readonly short mantissaThousandths;
		public readonly byte Base;
		public readonly SByte Exponent;

		public float Mantissa {
			get { return (float)(mantissaThousandths * 0.001); }
		}

		public Exp(float mantissa, byte numericBase, SByte exponent)
		{
			if ((mantissa <= -10) || (mantissa >= 10)) {
				throw new ArgumentOutOfRangeException("mantissa", mantissa, "Absolute value of the mantissa must be under 10.");
			}
			mantissaThousandths = Convert.ToInt16(mantissa * 1000);
			Base = numericBase;
			Exponent = exponent;
		}

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
			StringBuilder stbExp = new StringBuilder();
			stbExp.Append(this.Mantissa.ToString(format, formatProvider)).Append(" x ");
			stbExp.Append(this.Base).Append('^').Append(this.Exponent);
			return stbExp.ToString();
		}
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is Exp && Equals((Exp)obj);
		}

		public bool Equals(Exp other)
		{
			return (mantissaThousandths == other.mantissaThousandths) &&
			 (Base == other.Base) && (Exponent == other.Exponent);
		}

		public override int GetHashCode()
		{
			int hashCode = 1362180524;
			hashCode = hashCode * -1521134295 + mantissaThousandths.GetHashCode();
			hashCode = hashCode * -1521134295 + Base.GetHashCode();
			hashCode = hashCode * -1521134295 + Exponent.GetHashCode();
			return hashCode;
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
				var e2 = (SByte)(x.Exponent + y.Exponent);
				var m2 = x.Mantissa * y.Mantissa;
				return AdjustMantissaExponent(m2, b, e2);
			}
		}

		public static Exp operator /(Exp x, Exp y)
		{
			var b = x.Base;
			if (y.Base != b) {
				throw new ArgumentException("Base must be identical");
			} else {
				var e2 = (SByte)(x.Exponent - y.Exponent);
				var m2 = x.Mantissa / y.Mantissa;
				return AdjustMantissaExponent(m2, b, e2);
			}
		}

		private static Exp AdjustMantissaExponent(float m2, byte b, sbyte e2)
		{
			if ((m2 <= -b) || (m2 >= b) || ((m2 > -1) && (m2 < 1))) {
				var magnitude = Convert.ToSByte(Math.Floor(Math.Log(Math.Abs(m2), b)));
				e2 += magnitude;
				m2 = (float)(m2 * ExtraMath.Pow(b, (SByte)(-magnitude)));
			}
			return new Exp(m2, b, e2);
		}
	}
}
