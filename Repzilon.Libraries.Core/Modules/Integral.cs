//
//  Integral.cs
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

namespace Repzilon.Libraries.Core
{
	/// <summary>
	/// Methods for evaluating definite integrals
	/// </summary>
	public static class Integral
	{
#if !NET20
		[Obsolete]
		public static T Summation<T>(int m, int n, Func<int, T> forEach)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			if (forEach == null) {
				throw new ArgumentNullException("forEach");
			}

			T sum = default(T);
			for (var k = m; k <= n; k++) {
#if DEBUG
				T value = forEach(k);
				sum = GenericArithmetic<T>.Adder(sum, value);
#else
				sum = GenericArithmetic<T>.Adder(sum, forEach(k));
#endif
			}
			return sum;
		}

		[Obsolete]
		public static T DifferenceOfPrimitives<T>(T a, T b, Func<T, T> expression)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			return GenericArithmetic<T>.Sub(expression(b), expression(a));
		}
#endif

#if NETFRAMEWORK
		public static long Summation(int m, int n, Converter<int, long> forEach)
#else
		public static long Summation(int m, int n, Func<int, long> forEach)
#endif
		{
			if (forEach == null) {
				throw new ArgumentNullException("forEach");
			}

			long sum = 0;
			for (var k = m; k <= n; k++) {
				sum += forEach(k);
			}
			return sum;
		}

#if NETFRAMEWORK
		public static float Summation(int m, int n, Converter<int, float> forEach)
#else
		public static float Summation(int m, int n, Func<int, float> forEach)
#endif
		{
			if (forEach == null) {
				throw new ArgumentNullException("forEach");
			}

			float sum = 0;
			for (var k = m; k <= n; k++) {
				sum += forEach(k);
			}
			return sum;
		}

#if NETFRAMEWORK
		public static double Summation(int m, int n, Converter<int, double> forEach)
#else
		public static double Summation(int m, int n, Func<int, double> forEach)
#endif
		{
			if (forEach == null) {
				throw new ArgumentNullException("forEach");
			}

			double sum = 0;
			for (var k = m; k <= n; k++) {
				sum += forEach(k);
			}
			return sum;
		}

#if NETFRAMEWORK
		public static decimal Summation(int m, int n, Converter<int, decimal> forEach)
#else
		public static decimal Summation(int m, int n, Func<int, decimal> forEach)
#endif
		{
			if (forEach == null) {
				throw new ArgumentNullException("forEach");
			}

			decimal sum = 0;
			for (var k = m; k <= n; k++) {
				sum += forEach(k);
			}
			return sum;
		}

#if NETFRAMEWORK
		public static double Riemann(double a, double b, int n, Converter<double, double> expression)
#else
		public static double Riemann(double a, double b, int n, Func<double, double> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			double sum = 0;
			var deltaXk = b / n;
			for (var k = 1; k <= n; k++) {
				sum += expression(a + k * deltaXk) * deltaXk;
			}
			return sum;
		}

#if NETFRAMEWORK
		public static decimal Riemann(decimal a, decimal b, int n, Converter<decimal, decimal> expression)
#else
		public static decimal Riemann(decimal a, decimal b, int n, Func<decimal, decimal> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			decimal sum = 0;
			var deltaXk = b / n;
			for (var k = 1; k <= n; k++) {
				sum += expression(a + k * deltaXk) * deltaXk;
			}
			return sum;
		}

#if NETFRAMEWORK
		public static float DifferenceOfPrimitives(float a, float b, Converter<float, float> expression)
#else
		public static float DifferenceOfPrimitives(float a, float b, Func<float, float> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			return expression(b) - expression(a);
		}

#if NETFRAMEWORK
		public static double DifferenceOfPrimitives(double a, double b, Converter<double, double> expression)
#else
		public static double DifferenceOfPrimitives(double a, double b, Func<double, double> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			return expression(b) - expression(a);
		}

#if NETFRAMEWORK
		public static decimal DifferenceOfPrimitives(decimal a, decimal b, Converter<decimal, decimal> expression)
#else
		public static decimal DifferenceOfPrimitives(decimal a, decimal b, Func<decimal, decimal> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			return expression(b) - expression(a);
		}

#if NETFRAMEWORK
		public static double Simpson(double a, double b, Converter<double, double> expression)
#else
		public static double Simpson(double a, double b, Func<double, double> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			const double kOneSixth = 1.0 / 6;
			return (b - a) * kOneSixth * (expression(a) + (4 * expression(0.5 * (a + b))) + expression(b));
		}

#if NETFRAMEWORK
		public static double SimpsonThreeEights(double a, double b, Converter<double, double> expression)
#else
		public static double SimpsonThreeEights(double a, double b, Func<double, double> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			const double kOneThird = 1.0 / 3;
			return (b - a) * 0.125 * (expression(a) + (3 * expression(kOneThird * (2 * a + b))) + (3 * expression(kOneThird * (a + 2 * b))) + expression(b));
		}

#if NETFRAMEWORK
		public static double Simpson(double a, double b, int n, Converter<double, double> expression)
#else
		public static double Simpson(double a, double b, int n, Func<double, double> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			const double kOneThird = 1.0 / 3;
			var h = (b - a) / n;
			var sum = expression(a) + expression(b);
			for (int i = 1; i < n; i++) {
#if DEBUG
				var xi = a + (i * h);
				var y = expression(xi);
				sum += y * ((i % 2 == 1) ? 4 : 2);
#else
				sum += expression(a + (i * h)) * ((i % 2 == 1) ? 4 : 2);
#endif
			}
			return kOneThird * h * sum;
		}

#if NETFRAMEWORK
		public static decimal Simpson(decimal a, decimal b, int n, Converter<decimal, decimal> expression)
#else
		public static decimal Simpson(decimal a, decimal b, int n, Func<decimal, decimal> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			const decimal kOneThird = 1.0m / 3;
			var h = (b - a) / n;
			var sum = expression(a) + expression(b);
			for (int i = 1; i < n; i++) {
#if DEBUG
				var xi = a + (i * h);
				var y = expression(xi);
				sum += y * ((i % 2 == 1) ? 4 : 2);
#else
				sum += expression(a + (i * h)) * ((i % 2 == 1) ? 4 : 2);
#endif
			}
			return kOneThird * h * sum;
		}

#if NETFRAMEWORK
		public static double SimpsonThreeEights(double a, double b, int n, Converter<double, double> expression)
#else
		public static double SimpsonThreeEights(double a, double b, int n, Func<double, double> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			if (n % 3 != 0) {
				throw new ArgumentOutOfRangeException("n", n, "The number of iterations must be a multiple of 3.");
			}
			var h = (b - a) / n;
			var sum = expression(a) + expression(b);
			for (int i = 1; i < n; i += 3) {
#if DEBUG
				var xi = a + (i * h);
				var y = expression(xi);
				sum += 3 * y;

				xi = a + ((i + 1) * h);
				y = expression(xi);
				sum += 3 * y;

				if (i + 2 < n) {
					xi = a + ((i + 2) * h);
					y = expression(xi);
					sum += 2 * y;
				}
#else
				var xi = a + (i * h);
				sum += 3 * (expression(xi) + expression(xi + h));
				if (i + 2 < n) {
					sum += 2 * expression(xi + h + h);
				}
#endif
			}
			return 0.375 * h * sum;
		}

#if NETFRAMEWORK
		public static decimal SimpsonThreeEights(decimal a, decimal b, int n, Converter<decimal, decimal> expression)
#else
		public static decimal SimpsonThreeEights(decimal a, decimal b, int n, Func<decimal, decimal> expression)
#endif
		{
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			if (n % 3 != 0) {
				throw new ArgumentOutOfRangeException("n", n, "The number of iterations must be a multiple of 3.");
			}
			var h = (b - a) / n;
			var sum = expression(a) + expression(b);
			for (int i = 1; i < n; i += 3) {
#if DEBUG
				var xi = a + (i * h);
				var y = expression(xi);
				sum += 3 * y;

				xi = a + ((i + 1) * h);
				y = expression(xi);
				sum += 3 * y;

				if (i + 2 < n) {
					xi = a + ((i + 2) * h);
					y = expression(xi);
					sum += 2 * y;
				}
#else
				var xi = a + (i * h);
				sum += 3 * (expression(xi) + expression(xi + h));
				if (i + 2 < n) {
					sum += 2 * expression(xi + h + h);
				}
#endif
			}
			return 0.375m * h * sum;
		}
	}
}
