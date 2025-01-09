//
//  ExtraMath_decimal.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;

namespace Repzilon.Libraries.Core
{
	// Based from https://github.com/raminrahimzada/CSharp-Helper-Classes/blob/master/Math/DecimalMath/DecimalMath.cs
	// with duplicate removal, simplification and IL tuning.
	// Unfortunately too inaccurate when fuzz testing. Only hyperbolic functions work as intended.
	partial class ExtraMath
	{
		/// <summary>
		/// Twice the value of π. Present in recent versions of the Math static class in BCL.
		/// </summary>
		public const decimal Tau = 6.28318530717958647692528676655900576839433879875021M;

#if false
		private const decimal HalfPi = 1.570796326794896619231321691639751442098584699687552910487M;
		private const decimal QuarterPi = 0.785398163397448309615660845819875721049292349843776455243M;

		/// <summary>
		/// The reciprocal of e
		/// </summary>
		private const decimal Einv = 0.3678794411714423215955237701614608674458111310317678M;

		/// <summary>
		/// The reciprocal of ln(10)
		/// </summary>
		private const decimal Log10Inv = 0.434294481903251827651128918916605082294397005803666566114M;
#endif

		private const decimal Half = 0.5M;

#if false
		/// <summary>
		/// Maximum iterations in Taylor series
		/// </summary>
		private const int MaxIteration = 100;
#endif

#if false
		public static decimal Exp(decimal x)
#else
		/// <remarks>This function is only accurate enough for running hyperbolic functions.</remarks>
		private static decimal Exp(decimal x)
#endif
		{
			/*const*/ decimal kOne = 1;

			var count = 0;
			if (x > kOne) {
				count  = Decimal.ToInt32(Decimal.Truncate(x));
				x     -= Decimal.Truncate(x);
			}
			if (x < 0) {
				count = Decimal.ToInt32(Decimal.Truncate(x) - kOne);
				x     = kOne + (x - Decimal.Truncate(x));
			}

			var     result    = kOne;
			var     factorial = kOne;
			var     iteration = 1;
			decimal cachedResult;
			do {
				cachedResult  = result;
				factorial    *= x / iteration++;
				result       += factorial;
			} while (cachedResult != result);

			return count == 0 ? result : result * Pow(E, count);
		}

#if false
		public static decimal Pow(decimal radix, decimal exponent)
		{
			/*const*/ decimal kZero = 0;
			/*const*/ decimal kOne = 1;

			if ((exponent == kZero) || (radix == kOne)) {
				return kOne;
			} else if (exponent == kOne) {
				return radix;
			} else if (radix == kZero) {
				if (exponent > kZero) {
					return kZero;
				} else {
					throw new ArgumentOutOfRangeException("exponent", exponent,
					 "With a base of 0, a negative power leads to a division by 0.");
				}
			} else if (exponent == Decimal.MinusOne) {
				return kOne / radix;
			} else if (RoundOff.AreEqual(Math.Abs(exponent - (long)exponent), 0)) { // is exponent an integer?
				var powerInt = (int)exponent;
				if (radix > kZero) {
					return Pow(radix, powerInt);
				} else if (radix < kZero) {
					return powerInt % 2 == 0 ? Exp(exponent * Log(-radix)) : -Exp(exponent * Log(-radix));
				}
			} else if (radix < kZero) { // already !isPowerInteger
				throw new NotSupportedException(
				 "A negative base and a non-integer exponent lead to a complex number.");
			}
			return Exp(exponent * Log(radix));
		}
#endif

#if false
		public static decimal Pow(decimal value, int power)
#else
		private static decimal Pow(decimal value, int power)
#endif
		{
			/*const*/ decimal kOne = 1;

			while (true) {
				if (power == Decimal.Zero) {
					return kOne;
				}
				if (power < Decimal.Zero) {
					value = kOne / value;
					power = -power;
					continue;
				}

				var q       = power;
				var prod    = kOne;
				var current = value;
				while (q > 0) {
					if (q % 2 == 1) {
						// detects the 1s in the binary expression of power
						prod = current * prod; // picks up the relevant power
						q--;
					}

					current  *= current; // value^i -> value^(2*i)
					q       >>= 1;
				}

				return prod;
			}
		}

#if false
		public static decimal Log10(decimal x)
		{
			return Log(x) * Log10Inv;
		}

		/// <summary>
		/// Natural logarithm of a number
		/// </summary>
		/// <param name="x">A real number</param>
		public static decimal Log(decimal x)
		{
			var count = 0;
			decimal result;
			int iteration;
			decimal y;

			/*const*/ decimal kZero = 0;
			/*const*/ decimal kOne = 1;
			/*const*/ decimal kEinv = Einv;

			if (x <= kZero) {
				throw new ArgumentOutOfRangeException("x", x, "Must be greater than zero.");
			}

			while (x >= kOne) {
				x *= kEinv;
				count++;
			}
			while (x <= kEinv) {
				x *= E;
				count--;
			}

			x--;
			if (x == kZero) {
				return count;
			}

			result      = kZero;
			iteration   = 0;
			y           = kOne;
			var cacheResult = result - kOne;
			while (cacheResult != result && iteration < MaxIteration) {
				iteration++;
				cacheResult  = result;
				y           *= -x;
				result      += y / iteration;
			}

			return count - result;
		}

		public static decimal Cos(decimal x)
		{
			decimal y, xx;
			int i;

			/*const*/ decimal kPi = Pi;

			//truncating to  [-2*PI;2*PI]
			TruncateToPeriodicInterval(ref x);

			// now x in (-2pi,2pi)
			if (x >= kPi && x <= Tau) {
				return -Cos(x - kPi);
			}
			if (x >= -Tau && x <= -Pi) {
				return -Cos(x + kPi);
			}

			x *= x;
			//y=1-x/2!+x^2/4!-x^3/6!...
			xx      = -x * Half;
			y       = Decimal.One + xx;
			var cachedY = y - Decimal.One; //init cache  with different value
			for (i = 1; cachedY != y && i < MaxIteration; i++) {
				cachedY = y;
				decimal factor = -Half / (i * ((i << 1) + 3) + 1); //2i^2+2i+i+1=2i^2+3i+1
				xx     *= x * factor;
				y      += xx;
			}

			return y;
		}

		public static decimal Tan(decimal x)
		{
			var cos = Cos(x);
			if (cos == Decimal.Zero) {
				throw new ArgumentOutOfRangeException("x", x, "Undefined when value is an odd multiple of pi/2.");
			}

			//calculate sin using cos
			return CalculateSinFromCos(x, cos) / cos;
		}

		/// <summary>
		/// Helper function for calculating sin(x) from cos(x)
		/// </summary>
		/// <param name="x">Value to calculate sine from</param>
		/// <param name="cos">Cosine of x</param>
		/// <returns>Sine of x</returns>
		private static decimal CalculateSinFromCos(decimal x, decimal cos)
		{
			var moduleOfSin = Sqrt(Decimal.One - cos * cos);

			// Is sign of sine positive ?
			// 1. Clamp to [-2*PI;2*PI]
			TruncateToPeriodicInterval(ref x);
			// 2. now x in [-2*PI;2*PI]
			return (x >= -Tau && x <= -Pi) || (x >= Decimal.Zero && x <= Pi) ? moduleOfSin : -moduleOfSin;
		}

		public static decimal Sin(decimal x)
		{
			return CalculateSinFromCos(x, Cos(x));
		}

		/// <summary>
		/// Truncates to [-2*PI; 2*PI]
		/// </summary>
		/// <param name="x">Value to truncate</param>
		private static void TruncateToPeriodicInterval(ref decimal x)
		{
			/*const*/ decimal kTau = Tau;
			int divide;
			while (x >= kTau) {
				divide = Math.Abs(Decimal.ToInt32(x / kTau));
				x -= divide * kTau;
			}
			while (x <= -Tau) {
				divide = Math.Abs(Decimal.ToInt32(x / kTau));
				x += divide * kTau;
			}
		}
#endif

		public static decimal Sinh(decimal x)
		{
			var y = Exp(x);
			return (y - (Decimal.One / y)) * Half;
		}

		public static decimal Cosh(decimal x)
		{
			var y = Exp(x);
			return (y + (Decimal.One / y)) * Half;
		}

		public static decimal Tanh(decimal x)
		{
			var y  = Exp(x);
			var yy = Decimal.One / y;
			return (y - yy) / (y + yy);
		}

#if false
		public static decimal Asin(decimal x)
		{
			decimal y, result;
			int i;

			/*const*/ decimal kOne = 1;
			/*const*/ decimal kZero = 0;
			/*const*/ decimal kHalf = Half;
			/*const*/ decimal kHalfPi = HalfPi;

			if (x > kOne || x < -Decimal.One) {
				throw new ArgumentOutOfRangeException("x", x, "Must be between -1 and 1");
			}

			//known values
			if (x == kZero) {
				return kZero;
			} else if (x == kOne) {
				return kHalfPi;
			}

			//asin function is odd function
			if (x < kZero) {
				return -Asin(-x);
			}

			//my optimize trick here

			// used a math formula to speed up :
			// asin(x)=0.5*(pi/2-asin(1-2*x*x))
			// if x>=0 is true

			var newX = kOne - 2 * x * x;

			//for calculating new value near to zero than current
			//because we gain more speed with values near to zero
			if (Math.Abs(x) > Math.Abs(newX)) {
				return kHalf * (kHalfPi - Asin(newX));
			}

			y      = kZero;
			result = x;
			decimal cachedResult;
			i = 1;
			y += result;
			var xx = x * x;
			do {
				cachedResult =  result;
				result       *= xx * (kOne - kHalf / i);
				y            += result / ((i << 1) + 1);
				i++;
			} while (cachedResult != result);

			return y;
		}

		public static decimal Atan(decimal x)
		{
			/*const*/ decimal kOne = 1;
			/*const*/ decimal kZero = 0;
			if (x == kZero) {
				return kZero;
			} else if (x == kOne) {
				return QuarterPi;
			} else {
				return Asin(x / Sqrt(kOne + x * x));
			}		
		}

		public static decimal Acos(decimal x)
		{
			/*const*/ decimal kOne = 1;
			/*const*/ decimal kZero = 0;
			/*const*/ decimal kHalfPi = HalfPi;
			if (x == kZero) {
				return kHalfPi;
			} else if (x == kOne) {
				return kZero;
			} else if (x < kZero) {
				return Pi - Acos(-x);
			} else {
				return kHalfPi - Asin(x);
			}	
		}

		/// <seealso cref="http://i.imgur.com/TRLjs8R.png"/>
		public static decimal Atan2(decimal y, decimal x)
		{
			/*const*/ decimal kZero = 0;
			/*const*/ decimal kPi = Pi;
			if (x == kZero) {
				if (y > kZero) {
					return HalfPi;
				} else if (y < kZero) {
					return -HalfPi;
				} else {
					throw new ArgumentException("Arguments for Atan2 function cannot be both 0.");
				}
			} else {
				var arctan = Atan(y / x);
				if (y >= kZero) {
					arctan += kPi;
				} else if (y < kZero) {
					arctan -= kPi;
				}
				return arctan;
			}
		}
#endif
	}
}
