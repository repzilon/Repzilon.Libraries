//
//  ExtraMath_decimal.cs
//
//  Authors:
//       Nathan P. Jones
//       Neil McNeight
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
	partial class ExtraMath
	{
		#region Code from raminrahimzada
		// Based from https://github.com/raminrahimzada/CSharp-Helper-Classes/blob/master/Math/DecimalMath/DecimalMath.cs
		// with duplicate removal, simplification and IL tuning.

		// License of original code: "You can freely use DecimalMath or any part of it in your project without blaming me about future bugs that may break your production ;)"
		// Which I interpret as dedication to public domain because it says nothing about license terms distribution
		// nor attribution. There is only a discharge of liability.
		// Therefore, I can say I am the author of the modified version and license the modified version under the
		// MPL version 2.

		// Unfortunately too inaccurate when fuzz testing. Only hyperbolic functions work as intended.

		private const decimal HalfPi = 1.570796326794896619231321691639751442098584699687552910487M;
		private const decimal QuarterPi = 0.785398163397448309615660845819875721049292349843776455243M;

#if false
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
#endif

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
#endif

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
		#endregion

		#region Code by Nathan P. Jones and Neil McNeight
		// Based from https://github.com/McNeight/MathM
		#region MIT License
		// Copyright(c) 2015-2019 Nathan P Jones
		// Copyright(c) 2019 Neil McNeight

		// Permission is hereby granted, free of charge, to any person obtaining a copy
		// of this software and associated documentation files(the "Software"), to deal
		// in the Software without restriction, including without limitation the rights
		// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
		// copies of the Software, and to permit persons to whom the Software is
		// furnished to do so, subject to the following conditions:

		// The above copyright notice and this permission notice shall be included in all
		// copies or substantial portions of the Software.

		// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
		// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
		// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
		// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
		// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
		// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
		// SOFTWARE.
		#endregion

		// This version here is a modification as part of a larger work and I am allowed to relicense the modified
		// version under the MPL version 2.

		// This code provides a satisfying implementation of Sin, Cos, Atan and Sqrt (better than the one I had).

		/// <summary>
		/// Represents the natural logarithmic base, specified by the constant, e.
		/// </summary>
		/// <remarks>
		/// According to <see href="https://oeis.org/A001113">The On-Line Encyclopedia of Integer Sequences</see>,
		/// the value of e (also called Euler's number or Napier's constant) to 100 decimal places is
		/// 2.7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274.
		/// </remarks>
		public const decimal E = 2.7182818284590452353602874714m;

		/// <summary>
		/// Represents the ratio of the circumference of a circle to its
		/// diameter, specified by the constant, π.
		/// </summary>
		/// <remarks>
		/// According to <see href="https://oeis.org/A000796">The On-Line Encyclopedia of Integer Sequences</see>,
		/// the value of π (also called Archimedes's constant) to 100 decimal places is
		/// 3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679.
		/// </remarks>
		public const decimal Pi = 3.1415926535897932384626433833m;

		/// <summary>
		/// Represents the ratio of the circumference of a circle to its radius, specified by the constant, τ.
		/// It is twice the value of π an is present in recent versions of the Math static class in BCL.
		/// </summary>
		/// <remarks>
		/// According to <see href="https://tauday.com/tau-manifesto">The Tau Manifesto</see>,
		/// the value of τ to 100 decimal places is
		/// 6.2831853071795864769252867665590057683943387987502116419498891846156328125724179972560696506842341359.
		/// </remarks>
		public const decimal Tau = 6.2831853071795864769252867666m;

#if false
		/// <summary>
		/// The value of the natural logarithm of 10.
		/// </summary>
		/// <remarks>
		/// According to <see href="https://oeis.org/A002392">The On-Line Encyclopedia of Integer Sequences</see>,
		/// the value of Ln10 to 86 decimal places is
		/// 2.30258509299404568401799145468436420760110148862877297603332790096757260967735248023599.
		/// </remarks>
		private const decimal Ln10 = 2.3025850929940456840179914547m;
#endif

		/// <summary>
		/// Smallest non-zero decimal value.
		/// </summary>
		/// <remarks>
		/// <code>new decimal(1, 0, 0, false, 28);</code> or 1e-28m.
		/// </remarks>
		private const decimal SmallestNonZeroDec = 0.0000000000000000000000000001m;

		// This table is required for the Round function which can specify the number of digits to round to
		private static readonly decimal[] roundPower10Decimal = new decimal[]
		{
			1E0m,  1E1m,  1E2m,  1E3m,  1E4m,  1E5m,  1E6m,  1E7m,  1E8m,  1E9m,
			1E10m, 1E11m, 1E12m, 1E13m, 1E14m, 1E15m, 1E16m, 1E17m, 1E18m, 1E19m,
			1E20m, 1E21m, 1E22m, 1E23m, 1E24m, 1E25m, 1E26m, 1E27m, 1E28m,
		};

#if false
		/// <summary>
		/// Returns the angle whose cosine is the specified number.
		/// </summary>
		/// <param name="m">A number representing a cosine, where -1 ≤ <paramref name="m"/> ≤ 1.</param>
		/// <returns>An angle, θ, measured in radians, such that 0 ≤ θ ≤ π.</returns>
		/// <remarks>
		/// Multiply the return value by (180 / <see cref="PI"/>) to convert from radians to degrees.
		/// </remarks>
		public static decimal Acos(decimal m)
		{
			// See http://en.wikipedia.org/wiki/Inverse_trigonometric_function
			// and http://mathworld.wolfram.com/InverseCosine.html
			if (m < -1m || m > 1m) {
				throw new ArgumentOutOfRangeException("m", m, "Valid values are between -1 and 1, inclusive.");
			}

			// Special cases
			if (m == -1m) {
				return Pi;
			} else if (m == 0m) {
				return HalfPi;
			} else if (m == 1m) {
				return 0m;
			} else {
				return 2m * Atan(Sqrt(1m - (m * m)) / (1m + m));
			}
		}

		/// <summary>
		/// Returns the angle whose sine is the specified number.
		/// </summary>
		/// <param name="m">A number representing a sine, where -1 ≤ m ≤ 1.</param>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Inverse_trigonometric_function
		/// and http://mathworld.wolfram.com/InverseSine.html
		/// I originally used the Taylor series for ASin, but it was extremely slow
		/// around -1 and 1 (millions of iterations) and still ends up being less
		/// accurate than deriving from the ATan function.
		/// </remarks>
		/// <returns></returns>
		public static decimal Asin(decimal m)
		{
			if (m < -1m || m > 1m) {
				throw new ArgumentOutOfRangeException("m", m, "Valid values are between -1 and 1, inclusive.");
			}

			// Special cases
			if (m == -1m) {
				return -HalfPi;
			} else if (m == 0m) {
				return 0m;
			} else if (m == 1m) {
				return HalfPi;
			} else {
				return 2m * Atan(m / (1m + Sqrt(1m - (m * m))));
			}
		}
#endif

		/// <summary>
		/// Returns the angle whose tangent is the specified number.
		/// </summary>
		/// <param name="m">A number representing a tangent.</param>
		/// <remarks>
		/// See http://mathworld.wolfram.com/InverseTangent.html for faster converging
		/// series from Euler that was used here.
		/// </remarks>
		/// <returns></returns>
		public static decimal Atan(decimal m)
		{
			// Special cases
			if (m == -1) {
				return -QuarterPi;
			} else if (m == 0) {
				return 0;
			} else if (m == 1) {
				return QuarterPi;
			}

			if (m < -1) {
				// Force down to -1 to 1 interval for faster convergence
				return -HalfPi - Atan(1 / m);
			} else if (m > 1) {
				// Force down to -1 to 1 interval for faster convergence
				return HalfPi - Atan(1 / m);
			}

			var result = 0m;
			var doubleIteration = 0; // current iteration * 2
			var y = (m * m) / (1 + (m * m));
			var nextAdd = 0m;

			while (true) {
				if (doubleIteration == 0) {
					nextAdd = m / (1 + (m * m));  // is = y / x  but this is better for very small numbers where y = 9
				} else {
					// We multiply by -1 each time so that the sign of the component
					// changes each time. The first item is positive and it
					// alternates back and forth after that.
					// Following is equivalent to: nextAdd *= y * (iteration * 2) / (iteration * 2 + 1);
					nextAdd *= y * doubleIteration / (doubleIteration + 1);
				}

				if (nextAdd == 0) {
					break;
				}

				result += nextAdd;

				doubleIteration += 2;
			}

			return result;
		}

		/// <summary>
		/// Returns the angle whose tangent is the quotient of two specified numbers.
		/// </summary>
		/// <param name="y">The y coordinate of a point.</param>
		/// <param name="x">The x coordinate of a point.</param>
		/// <returns>
		/// An angle, θ, measured in radians, such that -π ≤ θ ≤ π, and tan(θ) = y / x,
		/// where (x, y) is a point in the Cartesian plane. Observe the following:
		/// For (x, y) in quadrant 1, 0 &lt; θ &lt; π/2.
		/// For (x, y) in quadrant 2, π/2 &lt; θ ≤ π.
		/// For (x, y) in quadrant 3, -π &lt; θ &lt; -π/2.
		/// For (x, y) in quadrant 4, -π/2 &lt; θ &lt; 0.
		/// </returns>
		public static decimal Atan2(decimal y, decimal x)
		{
			if (x == 0 && y == 0) {
				return 0;
			}
			if (x == 0) {
				return y > 0 ? HalfPi : -HalfPi;
			}
			if (y == 0) {
				return x > 0 ? 0 : Pi;
			}

			var aTan = Atan(y / x);
			if (x > 0) {
				return aTan;
			}
			return y > 0 ? aTan + Pi : aTan - Pi;
		}

		/// <summary>
		/// Returns the cosine of the specified angle.
		/// </summary>
		/// <param name="m">An angle, measured in radians.</param>
		/// <remarks>
		/// Uses a Taylor series to calculate sine. See
		/// http://en.wikipedia.org/wiki/Trigonometric_functions for details.
		/// </remarks>
		/// <returns></returns>
		public static decimal Cos(decimal m)
		{
			// Normalize to between -2Pi <= m <= 2Pi
			m = Remainder(m, Tau);

			if (m == 0 || m == Tau) {
				return 1m;
			} else if (m == Pi) {
				return -1m;
			} else if (m == HalfPi || m == Pi + HalfPi) {
				return 0m;
			}

			var result = 0m;
			var doubleIteration = 0; // current iteration * 2
			var xSquared = m * m;
			var nextAdd = 0m;

			while (true) {
				if (doubleIteration == 0) {
					nextAdd = 1m;
				} else {
					// We multiply by -1 each time so that the sign of the component
					// changes each time. The first item is positive and it
					// alternates back and forth after that.
					// Following is equivalent to: nextAdd *= -1 * x * x / ((2 * iteration - 1) * (2 * iteration));
					nextAdd *= -1 * xSquared / ((doubleIteration * doubleIteration) - doubleIteration);
				}

				if (nextAdd == 0m) {
					break;
				}

				result += nextAdd;

				doubleIteration += 2;
			}

			return result;
		}


		/// <summary>
		/// Returns <see cref="E">e</see> raised to the specified power.
		/// </summary>
		/// <param name="m">A number specifying a power.</param>
		/// <returns></returns>
#if false
		public static decimal Exp(decimal m)
#else
		private static decimal Exp(decimal m)
#endif
		{
			decimal result;
			decimal nextAdd;
			int iteration;
			bool reciprocal;
			decimal t;

			reciprocal = m < 0;
			m = Math.Abs(m);

			t = Math.Truncate(m);

			if (m == 0) {
				result = 1;
			} else if (m == 1) {
				result = E;
			} else if (Math.Abs(m) > 1 && t != m) {
				// Split up into integer and fractional
				result = Exp(t) * Exp(m - t);
			} else if (m == t) {
				// Integer power
				result = ExpBySquaring(E, m);
			} else {
				// Fractional power < 1
				// See http://mathworld.wolfram.com/ExponentialFunction.html
				iteration = 0;
				nextAdd = 0;
				result = 0;

				while (true) {
					if (iteration == 0) {
						nextAdd = 1;               // == Pow(d, 0) / Factorial(0) == 1 / 1 == 1
					} else {
						nextAdd *= m / iteration;  // == Pow(d, iteration) / Factorial(iteration)
					}

					if (nextAdd == 0) {
						break;
					}

					result += nextAdd;

					iteration += 1;
				}
			}

			// Take reciprocal if this was a negative power
			// Note that result will never be zero at this point.
			if (reciprocal) {
				result = 1 / result;
			}
			return result;
		}

#if false
		/// <summary>
		/// Returns the natural (base e) logarithm of a specified number.
		/// </summary>
		/// <param name="m">A number whose logarithm is to be found.</param>
		/// <remarks>
		/// I'm still not satisfied with the speed. I tried several different
		/// algorithms that you can find in a historical version of this
		/// source file. The one I settled on was the best of mediocrity.
		/// </remarks>
		/// <returns></returns>
		public static decimal Log(decimal m)
		{
			if (m < 0) {
				throw new ArgumentException("Natural logarithm is a complex number for values less than zero!", "m");
			} else if (m == 0) {
				throw new OverflowException("Natural logarithm is defined as negative infinity at zero which the Decimal data type can't represent!");
			} else if (m == 1) {
				return 0;
			}

			if (m >= 1) {
				decimal power = 0m;

				decimal x = m;
				while (x > 1) {
					x /= 10;
					power += 1;
				}

				return Log(x) + (power * Ln10);
			}

			// See http://en.wikipedia.org/wiki/Natural_logarithm#Numerical_value
			// for more information on this faster-converging series.

			decimal y;
			decimal ySquared;

			decimal iteration = 0;
			decimal exponent = 0m;
			decimal nextAdd;
			decimal result = 0m;

			y = (m - 1) / (m + 1);
			ySquared = y * y;

			while (true) {
				if (iteration == 0) {
					exponent = 2 * y;
				} else {
					exponent *= ySquared;
				}

				nextAdd = exponent / ((2 * iteration) + 1);

				if (nextAdd == 0) {
					break;
				}

				result += nextAdd;

				iteration += 1;
			}

			return result;
		}

		/// <summary>
		/// Returns the logarithm of a specified number in a specified base.
		/// </summary>
		/// <param name="m">A number whose logarithm is to be found.</param>
		/// <param name="newBase">The base of the logarithm.</param>
		/// <remarks>
		/// This is a relatively naive implementation that simply divides the
		/// natural log of <paramref name="m"/> by the natural log of the base.
		/// </remarks>
		/// <returns></returns>
		public static decimal Log(decimal m, decimal newBase)
		{
			if (m < 0) {
				throw new ArgumentException("Logarithm is a complex number for values less than zero!", "m");
			} else if (m == 0) {
				throw new OverflowException("Logarithm is defined as negative infinity at zero which the Decimal data type can't represent!");
			} else if (newBase < 0) {
				throw new ArgumentException("Logarithm base would be a complex number for values less than zero!", "m");
			} else if (newBase == 0) {
				throw new OverflowException("Logarithm base would be negative infinity at zero which the Decimal data type can't represent!");
			}

			// Short circuit the checks below if m is 1 because
			// that will yield 0 in the numerator below and give us
			// 0 for any base, even ones that would yield infinity.
			return m == 1 ? 0m : Log(m) / Log(newBase);
		}

		/// <summary>
		/// Returns the base 10 logarithm of a specified number.
		/// </summary>
		/// <param name="m">A number whose logarithm is to be found.</param>
		/// <returns></returns>
		public static decimal Log10(decimal m)
		{
			if (m < 0) {
				throw new ArgumentException("Logarithm is a complex number for values less than zero!", "m");
			} else if (m == 0) {
				throw new OverflowException("Logarithm is defined as negative infinity at zero which the Decimal data type can't represent!");
			}
			return Log(m) / Ln10;
		}

		/// <summary>
		/// Returns a specified number <paramref name="x"/> raised to the specified power <paramref name="y"/>.
		/// </summary>
		/// <param name="x">A number to be raised to a power.</param>
		/// <param name="y">A number that specifies a power.</param>
		/// <returns></returns>
		public static decimal Pow(decimal x, decimal y)
		{
			decimal result;
			var isNegativeExponent = false;

			// Handle negative exponents
			if (y < 0) {
				isNegativeExponent = true;
				y = Math.Abs(y);
			}

			if (y == 0) {
				result = 1;
			} else if (y == 1) {
				result = x;
			} else {
				var t = decimal.Truncate(y);

				if (y == t) {
					// Integer powers
					result = ExpBySquaring(x, y);
				} else {
					// Fractional power < 1
					// See http://en.wikipedia.org/wiki/Exponent#Real_powers
					// The next line is an optimization of Exp(y * Log(x)) for better precision
					result = ExpBySquaring(x, t) * Exp((y - t) * Log(x));
				}
			}

			if (isNegativeExponent) {
				// Note, for IEEE floats this would be Infinity and not an exception...
				if (result == 0) {
					throw new OverflowException("Negative power of 0 is undefined!");
				}

				result = 1 / result;
			}

			return result;
		}
#endif

		/// <summary>
		/// Returns the sine of the specified angle.
		/// </summary>
		/// <param name="m">An angle, measured in radians.</param>
		/// <remarks>
		/// Uses a Taylor series to calculate sine. See
		/// http://en.wikipedia.org/wiki/Trigonometric_functions for details.
		/// </remarks>
		/// <returns></returns>
		public static decimal Sin(decimal m)
		{
			// Normalize to between -2Pi <= m <= 2Pi
			m = Remainder(m, Tau);

			if (m == 0 || m == Pi || m == Tau) {
				return 0;
			} else if (m == HalfPi) {
				return 1;
			} else if (m == Pi + HalfPi) {
				return -1;
			}

			var result = 0m;
			var doubleIteration = 0; // current iteration * 2
			var mSquared = m * m;
			var nextAdd = 0m;

			while (true) {
				if (doubleIteration == 0) {
					nextAdd = m;
				} else {
					// We multiply by -1 each time so that the sign of the component
					// changes each time. The first item is positive and it
					// alternates back and forth after that.
					// Following is equivalent to: nextAdd *= -1 * m * m / ((2 * iteration) * (2 * iteration + 1));
					nextAdd *= -1 * mSquared / ((doubleIteration * doubleIteration) + doubleIteration);
				}

				// Debug.WriteLine("{0:000}:{1,33:+0.0000000000000000000000000000;-0.0000000000000000000000000000} ->{2,33:+0.0000000000000000000000000000;-0.0000000000000000000000000000}",
				//    doubleIteration / 2, nextAdd, result + nextAdd);
				if (nextAdd == 0) {
					break;
				}

				result += nextAdd;

				doubleIteration += 2;
			}

			return result;
		}

#if true
		/// <summary>
		/// Returns the square root of a given number.
		/// </summary>
		/// <param name="m">A non-negative number.</param>
		/// <remarks>
		/// Uses an implementation of the "Babylonian Method".
		/// See http://en.wikipedia.org/wiki/Methods_of_computing_square_roots#Babylonian_method
		/// </remarks>
		/// <returns></returns>
		public static decimal Sqrt(decimal m)
		{
			if (m < 0) {
				throw new ArgumentOutOfRangeException("m", m, "Cannot extract the square root of a negative number.");
			}

			// Prevent divide-by-zero errors below. Dividing either
			// of the numbers below will yield a recurring 0 value
			// for halfS eventually converging on zero.
			if (m == 0m || m == SmallestNonZeroDec) {
				return 0m;
			}

			decimal x;
			var halfS = m / 2m;
			var lastX = -1m;
			decimal nextX;

			// Begin with an estimate for the square root.
			// Use hardware to get us there quickly.
			x = (decimal)Math.Sqrt(decimal.ToDouble(m));

			while (true) {
				nextX = (x / 2m) + (halfS / x);

				// The next check effectively sees if we've ran out of
				// precision for our data type.
				if (nextX == x || nextX == lastX) {
					break;
				}

				lastX = x;
				x = nextX;
			}

			return nextX;
		}
#endif

#if false
		/// <summary>
		/// Returns the tangent of the specified angle.
		/// </summary>
		/// <param name="m">An angle, measured in radians.</param>
		/// <remarks>
		/// Uses a Taylor series to calculate sine. See
		/// http://en.wikipedia.org/wiki/Trigonometric_functions for details.
		/// </remarks>
		/// <returns></returns>
		public static decimal Tan(decimal m)
		{
			return Sin(m) / Cos(m);
		}
#endif

		/// <summary>
		/// Raises one number to an integral power.
		/// </summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Exponentiation_by_squaring
		/// </remarks>
		private static decimal ExpBySquaring(decimal x, decimal y)
		{
			if (y < 0) {
				throw new ArgumentOutOfRangeException("y", "Negative exponents not supported!");
			}
			if (decimal.Truncate(y) != y) {
				throw new ArgumentException("Exponent must be an integer!", "y");
			}

			var result = 1m;
			var multiplier = x;

			while (y > 0) {
				if ((y % 2) == 1) {
					result *= multiplier;
					y -= 1;
					if (y == 0) {
						break;
					}
				}

				multiplier *= multiplier;
				y /= 2;
			}

			return result;
		}

		/// <summary>
		/// Gets the number of decimal places in a decimal value.
		/// </summary>
		/// <remarks>
		/// Started with something found here: http://stackoverflow.com/a/6092298/856595
		/// </remarks>
		private static int GetDecimalPlaces(decimal m, bool countTrailingZeros)
		{
			const int signMask = unchecked((int)0x80000000);
			const int scaleMask = 0x00FF0000;
			const int scaleShift = 16;

			var bits = decimal.GetBits(m);
			var result = (bits[3] & scaleMask) >> scaleShift;  // extract exponent

			// Return immediately for values without a fractional portion or if we're counting trailing zeros
			if (countTrailingZeros || (result == 0)) {
				return result;
			}

			// Get a raw version of the decimal's integer
			bits[3] = bits[3] & ~unchecked(signMask | scaleMask); // clear out exponent and negative bit
			var rawValue = new decimal(bits);

			// Account for trailing zeros
			while ((result > 0) && ((rawValue % 10) == 0)) {
				result--;
				rawValue /= 10;
			}

			return result;
		}

		/// <summary>
		/// Gets the remainder of one number divided by another number in such a way as to retain maximum precision.
		/// </summary>
		private static decimal Remainder(decimal m1, decimal m2)
		{
			if (Math.Abs(m1) < Math.Abs(m2)) {
				return m1;
			}

			var timesInto = Math.Truncate(m1 / m2);
			var shiftingNumber = m2;
			var sign = Math.Sign(m1);

			for (var i = 0; i <= GetDecimalPlaces(m2, true); i++) {
				// Note that first "digit" will be the integer portion of d2
				var digit = Math.Truncate(shiftingNumber);

				m1 -= timesInto * (digit / roundPower10Decimal[i]);

				shiftingNumber = (shiftingNumber - digit) * 10m; // remove used digit and shift for next iteration
				if (shiftingNumber == 0m) {
					break;
				}
			}

			// If we've crossed zero because of the precision mismatch,
			// we need to add a whole d2 to get a correct result.
			if (m1 != 0 && Math.Sign(m1) != sign) {
				m1 = Math.Sign(m2) == sign ? m1 + m2 : m1 - m2;
			}

			return m1;
		}
#endregion
	}
}
