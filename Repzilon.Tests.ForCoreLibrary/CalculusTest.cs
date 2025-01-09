//
//  CalculusTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024-2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal enum MathFunction : byte
	{
		Other,
		Trigonometric,
		InverseTrigo,
		Hyperbolic
	}

	internal static class CalculusTest
	{
		private const int testCount = 100000;
		private static readonly Random Random = new Random();

		internal static void Run(string[] args)
		{
			Console.WriteLine("Calcul intégral travail 1 #2");
			SummationTest(10000, 729, "Math.Pow", CalculusWork1No2Fp);
			SummationTest(10000, 729, "Pow(i32, u16)", CalculusWork1No2Int64);
			SummationTest(10000, 729, "IIf", CalculusWork1No2IIf);
			SummationTest(10000, 729, "IIfn", CalculusWork1No2IIfn);
			SummationTest(10000, 729, "IIfd", CalculusWork1No2IIfd);

			Console.WriteLine("Factorielles");
			Console.WriteLine("20! vaut {0}", ExtraMath.Factorial(20));
			for (byte i = 21; i <= 27; i++) {
				Console.WriteLine("{0}! vaut {1}", i, ExtraMath.BigFactorial(i));
			}

			Console.WriteLine("Test de méthodes mathématiques avec Decimal");
			TestMathAnalog("Sqrt", MathFunction.Other, Math.Sqrt, ExtraMath.Sqrt);
#if false
			TestMathAnalog("Exp", MathFunction.Other, Math.Exp, ExtraMath.Exp);
			TestMathAnalog("Ln", MathFunction.Other, Math.Log, ExtraMath.Log);
			TestMathAnalog("Log10", MathFunction.Other, Math.Log10, ExtraMath.Log10);
			TestMathAnalog("Sin", MathFunction.Trigonometric, Math.Sin, ExtraMath.Sin);
			TestMathAnalog("Cos", MathFunction.Trigonometric, Math.Cos, ExtraMath.Cos);
			TestMathAnalog("Tan", MathFunction.Trigonometric, Math.Tan, ExtraMath.Tan);
			TestMathAnalog("Asin", MathFunction.InverseTrigo, Math.Asin, ExtraMath.Asin);
			TestMathAnalog("Acos", MathFunction.InverseTrigo, Math.Acos, ExtraMath.Acos);
			TestMathAnalog("Atan", MathFunction.InverseTrigo, Math.Atan, ExtraMath.Atan);
#endif
			TestMathAnalog("Sinh", MathFunction.Hyperbolic, Math.Sinh, ExtraMath.Sinh);
			TestMathAnalog("Cosh", MathFunction.Hyperbolic, Math.Cosh, ExtraMath.Cosh);
			TestMathAnalog("Tanh", MathFunction.Hyperbolic, Math.Tanh, ExtraMath.Tanh);
		}

		#region Summation test
#if NETFRAMEWORK
		private static void SummationTest(int benchLoops, int summationUpper, string legend,
		Converter<int, long> forEach)
#else
		private static void SummationTest(int benchLoops, int summationUpper, string legend,
		Func<int, long> forEach)
#endif
		{
			var dtmStart = DateTime.UtcNow;
			for (int i = 0; i < benchLoops; i++) {
				Integral.Summation(1, summationUpper, forEach);
			}
			var tsDuration = DateTime.UtcNow - dtmStart;
			var result = Integral.Summation(1, summationUpper, forEach);
			Console.WriteLine("={0}\t{2,-16} {1,7:n0} Hz", result, benchLoops / tsDuration.TotalSeconds, legend);
		}

		private static long CalculusWork1No2Fp(int k)
		{
			return 3 * k * (long)Math.Pow(-1, k - 1);
		}

		// The checked part here has almost no performance penalty
		private static long CalculusWork1No2Int64(int k)
		{
			return 3 * k * Pow(-1, checked((ushort)(k - 1)));
		}

		private static long CalculusWork1No2IIf(int k)
		{
			return 3 * k * (((k - 1) % 2 == 0) ? 1 : -1);
		}

		private static long CalculusWork1No2IIfn(int k)
		{
			return 3 * k * ((k % 2 == 0) ? -1 : 1);
		}

		private static long CalculusWork1No2IIfd(int k)
		{
			return 3 * k * ((k % 2 != 0) ? 1 : -1);
		}

		[Obsolete("15 times slower than Math.Pow.")]
		private static long Pow(int b, ushort e)
		{
			long r = 1;
			for (var i = 1; i <= e; i++) {
				r *= b;
			}
			return r;
		}
		#endregion

		#region Decimal math
		private static void TestMathAnalog(string name, MathFunction kind,
#if NET20
		Converter<double, double> math, Converter<decimal, decimal> extraMath)
#else
		Func<double, double> math, Func<decimal, decimal> extraMath)
#endif
		{
			if (math == null) {
				throw new ArgumentNullException("math");
			}
			if (extraMath == null) {
				throw new ArgumentNullException("extraMath");
			}
			Console.Write(name);
			Console.Write("\t: ");
			try {
				DateTime dtmStart = DateTime.UtcNow;
				for (int i = 0; i < testCount; i++) {
					double x = Random.NextDouble();
					if (kind == MathFunction.InverseTrigo) {
						x = x * 2 - 1;
					} else {
						x = Math.Exp(Random.NextDouble()) * Math.Pow(Random.NextDouble(), Math.E);
					}
					double fr8 = math(x);
					decimal fD = extraMath((decimal)x);

					double der8 = (double)fD - fr8;
					decimal deD = fD - (decimal)fr8;
					if (!RoundOff.AreEqual(der8, 0)) {
						throw new ArithmeticException(String.Format(
						 "Too big difference: x={1}{0}\tf(x[r8])={2,-29} Δ[r8]={4:e16}{0}\t f(x[D])={3} Δ[D]={5:e25}",
						 Environment.NewLine, x, fr8, fD, der8, deD));
					}
				}
				var hertz = testCount / (DateTime.UtcNow - dtmStart).TotalSeconds;
				Console.WriteLine("success at {0:n0} Hz", hertz);
			} catch (Exception exc) {
				Console.WriteLine("FAIL");
				Console.Error.WriteLine(exc.Message);
			}
		}

		/*
		private static void TestMethodAtan2()
		{
			for (int i = 0; i < testCount; i++) {
				double x = Random.NextDouble();
				double y = Random.NextDouble();
				decimal dx = (decimal)x;
				decimal dy = (decimal)y;
				var d = Math.Atan2(y, x);
				var z = ExtraMath.Atan2(dy, dx);
				Debug.Assert(Math.Abs((decimal)d - z) < epsilon);
			}
		}

		private static void TestMethodPow001()
		{
			double x = 10;
			double y = -5;
			double result = Math.Pow(x, y);

			Assert.AreEqual(result, 1E-05);

			decimal dx = 10;
			decimal dy = -5;
			decimal dResult = ExtraMath.Pow(dx, dy);

			Assert.AreEqual(dResult, 0.00001m);
		}

		private static void TestMethodPow002()
		{
			double x = 10;
			double y = 5;
			double result = Math.Pow(x, y);

			Assert.AreEqual(result, 100000);

			decimal dx = 10;
			decimal dy = 5;
			decimal dResult = ExtraMath.Pow(dx, dy);

			Assert.AreEqual(dResult, 100000m);
		}// */
		#endregion
	}
}
