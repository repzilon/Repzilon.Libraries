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
#if !NET20 && !NET35
using System.Numerics;
#endif
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal enum MathFunction : byte
	{
		Other,
		Exp,
		Trigonometric,
		InverseTrigo,
		Hyperbolic
	}

	internal static class CalculusTest
	{
		private const int TestCount = 100000;
		private static readonly Random Random = new Random();

		internal static void Run(string[] args)
		{
			Program.OutputHeading("Calcul intégral travail 1 #2");
			SummationTest(10000, 729, "Math.Pow", CalculusWork1No2Fp);
			SummationTest(10000, 729, "Pow(i32, u16)", CalculusWork1No2Int64);
			SummationTest(10000, 729, "IIf", CalculusWork1No2IIf);
			SummationTest(10000, 729, "IIfn", CalculusWork1No2IIfn);
			SummationTest(10000, 729, "IIfd", CalculusWork1No2IIfd);

			Program.OutputHeading("Factorielles");
			Console.WriteLine("20! vaut {0}", ExtraMath.Factorial(20));
			byte i;
			for (i = 21; i <= 27; i++) {
				Console.WriteLine("{0}! vaut {1}", i, ExtraMath.BigFactorial(i));
			}

			Program.OutputHeading("Test de méthodes mathématiques avec Decimal");
			// Force conversion from a stored decimal on disk to a double in memory by making it a variable
#pragma warning disable CC0001 // You should use 'var' whenever possible.
#pragma warning disable U2U1000
			// ReSharper disable once SuggestVarOrType_BuiltInTypes
			// ReSharper disable once ConvertToConstant.Local
			/*const*/ decimal kVerySmallSquare = 6.681844869362281E-18m;
#pragma warning restore U2U1000
#pragma warning restore CC0001 // You should use 'var' whenever possible.
			TestMathAnalog((double)kVerySmallSquare, MathFunction.Other, Math.Sqrt, ExtraMath.Sqrt);
			var ln3d = Math.Log(3);
			var ln3m = ExtraMath.Ln(3);
			var strFormat = "ln(3)\t{0}   {1}m   Δ FPU: {2:g13}   Δ MATH: {3:g13}";
			if (Program.UnicodeTerminal == SupportLevel.None) {
				strFormat = strFormat.Replace("Δ", "Delta");
			}
			Console.WriteLine(strFormat, ln3d, ln3m, ln3m - (decimal)ln3d,
			 ln3m - 1.098612288668109691395245236922525704647490557822749451734694333637494293218609m);
			TestMathAnalog((double)kVerySmallSquare, MathFunction.Other, Math.Log, ExtraMath.Ln);

			const decimal exponent     = 2.51059145358269m;
			const decimal referenceExp = 12.3122100081427838867620839168142458108126104545775059648361387825733687m;
			//const decimal exponent = 2.3988421091824654m;
			//const decimal referenceExp = 11.0104201325268752089128775065992851571285119260399503911266699103786971m;

			var expd  = Math.Exp((double)exponent);
			var expm0 = ExtraMath.Exp(exponent);
			var expm2 = ExtraMath.ExpRepzi2(exponent);
			strFormat = "e^x ({4})\t{0}   {1}m   Δ FPU: {2:g13}   Δ MATH: {3:g13}";
			if (Program.UnicodeTerminal == SupportLevel.None) {
				strFormat = strFormat.Replace("Δ", "Delta");
			}
			Console.WriteLine(strFormat, expd, expm0, expm0 - (decimal)expd, expm0 - referenceExp, 'N');
			Console.WriteLine(strFormat, expd, expm2, expm2 - (decimal)expd, expm2 - referenceExp, 'R');

			TestMathAnalog("Exp (N)", MathFunction.Exp, Math.Exp, ExtraMath.Exp);
			TestMathAnalog("Exp (R)", MathFunction.Exp, Math.Exp, ExtraMath.ExpRepzi2);

			TestMathAnalog("Sqrt", MathFunction.Other, Math.Sqrt, ExtraMath.Sqrt);
			TestMathAnalog("Ln", MathFunction.Other, Math.Log, ExtraMath.Ln);
			TestMathAnalog("Log10", MathFunction.Other, Math.Log10, ExtraMath.Log10);

			TestMathAnalog("Sin", MathFunction.Trigonometric, Math.Sin, ExtraMath.Sin);
			TestMathAnalog("Cos", MathFunction.Trigonometric, Math.Cos, ExtraMath.Cos);
#if false
			TestMathAnalog("Tan", MathFunction.Trigonometric, Math.Tan, ExtraMath.Tan);
			TestMathAnalog("Asin", MathFunction.InverseTrigo, Math.Asin, ExtraMath.Asin);
			TestMathAnalog("Acos", MathFunction.InverseTrigo, Math.Acos, ExtraMath.Acos);
#endif
			TestMathAnalog("Atan", MathFunction.InverseTrigo, Math.Atan, ExtraMath.Atan);
			TestMathAnalog("Sinh", MathFunction.Hyperbolic, Math.Sinh, ExtraMath.Sinh);
			TestMathAnalog("Cosh", MathFunction.Hyperbolic, Math.Cosh, ExtraMath.Cosh);
			TestMathAnalog("Tanh", MathFunction.Hyperbolic, Math.Tanh, ExtraMath.Tanh);

			Program.OutputHeading("Fibonnaci itératif");
			try {
				for (i = 0; i <= 254; i++) {
					Fibonacci(i);
				}
			} catch (OverflowException) {
				Console.Error.WriteLine("Fibonacci failed for n=" + i);
			} finally {
				Console.WriteLine("F({0})={1}", i - 1, Fibonacci((byte)(i - 1)));
			}

			try {
				for (i = 0; i <= 254; i++) {
					FibonacciDec(i);
				}
			} catch (OverflowException) {
				Console.Error.WriteLine("Fibonacci failed for n=" + i);
			} finally {
				Console.WriteLine("F({0})={1}", i - 1, FibonacciDec((byte)(i - 1)));
			}

#if !NET20 && !NET35
			Console.WriteLine("F({0})={1}", 255, FibonacciBig(255));
			Console.WriteLine("F({0})={1}", 65535, FibonacciBig(65535));
			var dtmStart = DateTime.UtcNow;
			var x = FibonacciBig(240000);
			var tsElapsed = DateTime.UtcNow - dtmStart;
			Console.WriteLine("F({0})={1} in {2}", 240000, x, tsElapsed);
#endif
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
			for (var i = 0; i < benchLoops; i++) {
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
				throw new ArgumentNullException(nameof(math));
			}
			if (extraMath == null) {
				throw new ArgumentNullException(nameof(extraMath));
			}
			Console.Write(name);
			Console.Write("\t: ");
			try {
				var dtmStart = DateTime.UtcNow;
				for (var i = 0; i < TestCount; i++) {
					var x = Random.NextDouble();
					if (kind == MathFunction.InverseTrigo) {
						x = x * 2 - 1;
					} else {
						x = Math.Exp(Random.NextDouble()) * Math.Pow(Random.NextDouble(), Math.E);
					}
					TestMathAnalog(x, kind, math, extraMath);
				}
				var hertz = TestCount / (DateTime.UtcNow - dtmStart).TotalSeconds;
				Console.WriteLine("success at {0:n0} Hz", hertz);
			} catch (Exception exc) {
				Console.WriteLine("FAIL");
				Console.Error.WriteLine(exc.Message);
			}
		}

		private static void TestMathAnalog(double x, MathFunction kind,
#if NET20
		Converter<double, double> math, Converter<decimal, decimal> extraMath)
#else
		Func<double, double> math, Func<decimal, decimal> extraMath)
#endif
		{
			// This is a private method for which I won't pass null
#pragma warning disable CC0031 // Check for null before calling a delegate
			var fr8 = math(x);
			var fD = extraMath((decimal)x);
#pragma warning restore CC0031 // Check for null before calling a delegate
			var der8 = (double)fD - fr8;

			if (kind == MathFunction.Exp) {
				if (Math.Abs(der8) > 7e-14) {
					ThrowUnacceptableDeviation(x, fr8, fD, der8);
				}
			} else if (!RoundOff.AreEqual(der8, 0)) {
				ThrowUnacceptableDeviation(x, fr8, fD, der8);
			}
		}

		private static void ThrowUnacceptableDeviation(double x, double fr8, decimal fD, double der8)
		{
			throw new ArithmeticException(String.Format(
			 "Too big difference: x={1}{0}\tf(x[r8])={2,-29} Δ[r8]={4:e16}{0}\t f(x[D])={3} Δ[D]={5:e25}",
			 Environment.NewLine, x, fr8, fD, der8, fD - (decimal)fr8));
		}

#if false
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
		}
#endif
		#endregion

		private static long Fibonacci(byte n)
		{
			long a = 0;
			long b = 1;
			long tmp;

			while (n-- > 0) {
				checked { tmp = a + b; }
				a = b;
				b = tmp;
			}

			return a;
		}

		private static decimal FibonacciDec(byte n)
		{
			decimal a = 0;
			decimal b = 1;
			decimal tmp;

			while (n-- > 0) {
				tmp = a + b;
				a = b;
				b = tmp;
			}

			return a;
		}

#if !NET20 && !NET35
		private static BigInteger FibonacciBig(uint n)
		{
			BigInteger a = 0;
			BigInteger b = 1;
			BigInteger tmp;

			while (n-- > 0) {
				tmp = a + b;
				a = b;
				b = tmp;
			}

			return a;
		}
#endif
	}
}
