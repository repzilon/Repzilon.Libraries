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
	// TODO : Use the decimal math functions everywhere decimal type is involved
	partial class ExtraMath
	{
		#region Code from raminrahimzada
		// Based from https://github.com/raminrahimzada/CSharp-Helper-Classes/blob/master/Math/DecimalMath/DecimalMath.cs
		// with some simplification

		// License of original code: "You can freely use DecimalMath or any part of it in your project without blaming me about future bugs that may break your production ;)"
		// Which I interpret as dedication to public domain because it says nothing about license terms distribution
		// nor attribution. There is only a discharge of liability.
		// Therefore, I can say I am the author of the modified version and license the modified version under the
		// MPL version 2.

		// Unfortunately too inaccurate when fuzz testing. Only hyperbolic functions work as intended.

		private static readonly decimal HalfPi = 1.570796326794896619231321691639751442098584699687552910487M;
		private static readonly decimal QuarterPi = 0.785398163397448309615660845819875721049292349843776455243M;

		private static readonly decimal Half = 0.5M;

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
			var y = Exp(x);
			var yy = Decimal.One / y;
			return (y - yy) / (y + yy);
		}
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
		public static readonly decimal E = 2.7182818284590452353602874714m;

		/// <summary>
		/// Represents the ratio of the circumference of a circle to its
		/// diameter, specified by the constant, π.
		/// </summary>
		/// <remarks>
		/// According to <see href="https://oeis.org/A000796">The On-Line Encyclopedia of Integer Sequences</see>,
		/// the value of π (also called Archimedes's constant) to 100 decimal places is
		/// 3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679.
		/// </remarks>
		public static readonly decimal Pi = 3.1415926535897932384626433833m;

		/// <summary>
		/// Represents the ratio of the circumference of a circle to its radius, specified by the constant, τ.
		/// It is twice the value of π an is present in recent versions of the Math static class in BCL.
		/// </summary>
		/// <remarks>
		/// According to <see href="https://tauday.com/tau-manifesto">The Tau Manifesto</see>,
		/// the value of τ to 100 decimal places is
		/// 6.2831853071795864769252867665590057683943387987502116419498891846156328125724179972560696506842341359.
		/// </remarks>
		public static readonly decimal Tau = 6.2831853071795864769252867666m;

		/// <summary>
		/// Smallest non-zero decimal value.
		/// </summary>
		/// <remarks>
		/// <code>new decimal(1, 0, 0, false, 28);</code> or 1e-28m.
		/// </remarks>
		private static readonly decimal SmallestNonZeroDec = 0.0000000000000000000000000001m;

		// This table is required for the Round function which can specify the number of digits to round to
		private static readonly decimal[] RoundPower10Decimal = new decimal[]
		{
			1E0m,  1E1m,  1E2m,  1E3m,  1E4m,  1E5m,  1E6m,  1E7m,  1E8m,  1E9m,
			1E10m, 1E11m, 1E12m, 1E13m, 1E14m, 1E15m, 1E16m, 1E17m, 1E18m, 1E19m,
			1E20m, 1E21m, 1E22m, 1E23m, 1E24m, 1E25m, 1E26m, 1E27m, 1E28m,
		};

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
			/*const*/ decimal kOne = 1;
			/*const*/ decimal kZero = 0;

			var doubleIteration = 0; // current iteration * 2
			var nextAdd = kZero;

			// Special cases
			if (m == -1) {
				return -QuarterPi;
			} else if (m == kZero) {
				return kZero;
			} else if (m == kOne) {
				return QuarterPi;
			}

			if (m < -1) {
				// Force down to -1 to 1 interval for faster convergence
				return -HalfPi - Atan(kOne / m);
			} else if (m > kOne) {
				// Force down to -1 to 1 interval for faster convergence
				return HalfPi - Atan(kOne / m);
			}

			var result = kZero;		
			var y = (m * m) / (kOne + (m * m));
			while (true) {
				if (doubleIteration == 0) {
					nextAdd = m / (kOne + (m * m));  // is = y / x  but this is better for very small numbers where y = 9
				} else {
					// We multiply by -1 each time so that the sign of the component
					// changes each time. The first item is positive and it
					// alternates back and forth after that.
					// Following is equivalent to: nextAdd *= y * (iteration * 2) / (iteration * 2 + 1);
					nextAdd *= y * doubleIteration / (doubleIteration + kOne);
				}

				if (nextAdd == kZero) {
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
			/*const*/ decimal kZero = 0;
			/*const*/ decimal kPi = Pi;

			if (x == kZero && y == kZero) {
				return kZero;
			} else if (x == kZero) {
				return y > kZero ? HalfPi : -HalfPi;
			} else if (y == kZero) {
				return x > kZero ? kZero : kPi;
			}

			var aTan = Atan(y / x);
			if (x > kZero) {
				return aTan;
			}
			return kZero > 0 ? aTan + kPi : aTan - kPi;
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
			/*const*/ decimal kZero = 0;

			var doubleIteration = 0; // current iteration * 2
			var nextAdd = kZero;
			var result = kZero;

			/*const*/ decimal kTau = Tau;

			// Normalize to between -2Pi <= m <= 2Pi
			m = Remainder(m, kTau);

			if (m == kZero || m == kTau) {
				return 1m;
			} else if (m == Pi) {
				return -1m;
			} else if (m == HalfPi || m == Pi + HalfPi) {
				return kZero;
			}

			var xSquared = m * m;
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

				if (nextAdd == kZero) {
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
			/*const*/ decimal kZero = 0;
			/*const*/ decimal kOne = 1;

			decimal result;
			decimal nextAdd;
			bool reciprocal;
			decimal t;

			/*const*/ decimal kNapier = E;

			reciprocal = m < kZero;
			m = Math.Abs(m);
			t = Math.Truncate(m);

			if (m == kZero) {
				result = kOne;
			} else if (m == kOne) {
				result = kNapier;
			} else if (m > kOne && t != m) {
				// Split up into integer and fractional
				result = Exp(t) * Exp(m - t);
			} else if (m == t) {
				// Integer power
				result = ExpBySquaring(kNapier, (int)m);
			} else {
				// Fractional power < 1
				// See http://mathworld.wolfram.com/ExponentialFunction.html
				int iteration = 0;
				nextAdd = kZero;
				result = kZero;

				while (true) {
					if (iteration == 0) {
						nextAdd = kOne;            // == Pow(d, 0) / Factorial(0) == 1 / 1 == 1
					} else {
						nextAdd *= m / iteration;  // == Pow(d, iteration) / Factorial(iteration)
					}

					if (nextAdd == kZero) {
						break;
					}

					result += nextAdd;

					iteration += 1;
				}
			}

			// Take reciprocal if this was a negative power
			// Note that result will never be zero at this point.
			if (reciprocal) {
				result = kOne / result;
			}
			return result;
		}

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
			/*const*/ decimal kZero = 0;

			var doubleIteration = 0; // current iteration * 2
			var nextAdd = kZero;
			var result = kZero;

			/*const*/ decimal kTau = Tau;

			// Normalize to between -2Pi <= m <= 2Pi
			m = Remainder(m, kTau);

			if (m == kZero || m == Pi || m == kTau) {
				return kZero;
			} else if (m == HalfPi) {
				return 1;
			} else if (m == Pi + HalfPi) {
				return -1;
			}

			var mSquared = m * m;
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
				if (nextAdd == kZero) {
					break;
				}

				result += nextAdd;

				doubleIteration += 2;
			}

			return result;
		}

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

			var halfS = m / 2m;
			var lastX = -1m;
			decimal nextX;

			// Begin with an estimate for the square root.
			// Use hardware to get us there quickly.
			decimal x = (decimal)Math.Sqrt(decimal.ToDouble(m));

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

		/// <summary>
		/// Raises one number to an integral power.
		/// </summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Exponentiation_by_squaring
		/// </remarks>
		private static decimal ExpBySquaring(decimal x, int y)
		{
			if (y < 0) {
				throw new ArgumentOutOfRangeException("y", y, "Negative exponents are not supported");
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
			bits[3] = bits[3] & ~(signMask | scaleMask); // clear out exponent and negative bit
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

			int i;
			var shiftingNumber = m2;
			var sign = Math.Sign(m1);
			decimal digit;
			var timesInto = Math.Truncate(m1 / m2);
			var totalPlaces = GetDecimalPlaces(m2, true);

			for (i = 0; i <= totalPlaces; i++) {
				// Note that first "digit" will be the integer portion of d2
				digit = Math.Truncate(shiftingNumber);

				m1 -= timesInto * (digit / RoundPower10Decimal[i]);

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

		private static readonly decimal LnOf2 = 0.6931471805599453094172321214581765680755001343602552541206800094933936219696947m;
		private static readonly decimal LnOf10 = 2.302585092994045684017991454684364207601101488628772976033327900967572609677352m;
		private static readonly decimal OneOfLn10 = 0.4342944819032518276511289189166050822943970058036665661144537831658646492088708m;

		public static decimal Ln(decimal a)
		{
			/*const*/ decimal kOne = Decimal.One;
			decimal x;
			byte k, n;
			if (a <= 0) {
				throw new ArgumentOutOfRangeException("a", "The logarithm of 0 or a negative number does not exist.");
			} else if (a < kOne) {
				x = a;	// mantissa
				k = 0;	// magnitude
				while (x < kOne) {
					x *= 10;
					k++;
				}
				return Ln(x) - (k * LnOf10);
			} else if (a == kOne) {
				return 0;
			} else if (a == 2) {
				return LnOf2;
			} else if (a == E) {
				return kOne;
			} else if (a == 10) {
				return LnOf10;
			} else {
				// https://math.stackexchange.com/questions/1585952/is-there-a-way-to-calculate-decimal-powers-using-only-addition-subtraction-mul
				// ln(x+1) developped as a series
				// A. Find the biggest k where a/(2^k) > 1
				x = a;
				for (k = 1; (k < 32) && (x > kOne); k++) {
					x = a / (1 << k);
				}
				if (x <= kOne) {
					k--;
				}
				// B. Compute x for ln(x+1) which is a/(2^k) - 1
				x = (a / (1 << k)) - kOne;
				// C. Develop the series of ln(x+1) with a loop
				var lnxp1 = x;
				for (n = 2; n <= 44; n++) {
					lnxp1 += Minus1Pow(n - 1) * Pow(x, n) / n;
				}
				// D. Return the result
				return lnxp1 + (k * LnOf2);
			}
		}

		public static decimal Log10(decimal a)
		{
			/*const*/ decimal kZero = Decimal.Zero;
			/*const*/ decimal kOne = Decimal.One;
			if (a <= kZero) {
				throw new ArgumentOutOfRangeException("a", "The logarithm of 0 or a negative number does not exist.");
			} else if (a == kOne) {
				return kZero;
			} else if (a == 10) {
				return kOne;
			} else {
				return Ln(a) * OneOfLn10;
			}
		}
	}
}
