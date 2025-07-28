//
//  DigitTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Globalization;
using Repzilon.Libraries.Core;
// ReSharper disable RedundantExplicitArrayCreation

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class DigitTest
	{
		internal static void Run(string[] args)
		{
			Action<string> toConsole = WriteCompact;
			// ReSharper disable TooWideLocalVariableScope
			byte f;
			double dblComputed;
			// ReSharper restore TooWideLocalVariableScope
			var ciOriginal = CultureInfo.CurrentCulture;
			try {
				//* Testing number types
				TestDigitCount(new short[] { -2, -1, 0, 1, 2, 123, 2005, 2000, 325, 3002 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 4, 1, 3, 4 }, toConsole);
				Console.WriteLine();
				TestDigitCount(new int[] { -2, -1, 0, 1, 2, 123, 2005, 2000, 325, 3002, 34000 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 4, 1, 3, 4, 2 }, toConsole);
				Console.WriteLine();
				TestDigitCount(new long[] { -2, -1, 0, 1, 2, 123, 2005, 2000, 325, 3002, 34000 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 4, 1, 3, 4, 2 }, toConsole);
				Console.WriteLine();
				TestDigitCount(new ushort[] { 0, 1, 2, 123, 2005, 2000, 325, 3002, 34000 },
				 new byte[] { 1, 1, 1, 3, 4, 1, 3, 4, 2 }, toConsole);
				Console.WriteLine();
				TestDigitCount(
				 new float[] { -2, -1, 0, 1, 2, 123f, 43.567f, 0.0054f, 0.124f, 25.02f, 2005f, 3.00f, 300.0f, 2000f, 325f, 3002f, 34000f, 40.40f, 0.00010300f },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 1, 1, 1, 3, 4, 2, 3, 3 }, toConsole);
				Console.WriteLine();
				TestDigitCount(
				 new double[] { -2, -1, 0, 1, 2, 123, 43.567, 0.0054, 0.124, 25.02, 2005, 3.00, 300.0, 2000, 325, 3002, 34000, 40.40, 0.00010300 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 1, 1, 1, 3, 4, 2, 3, 3 }, toConsole);
				Console.WriteLine();
				TestDigitCount(
				 new decimal[] { -2, -1, 0, 1, 2, 123, 43.567m, 0.0054m, 0.124m, 25.02m, 2005, 3.00m, 300.0m, 2000, 325, 3002, 34000, 40.40m, 0.00010300m },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 1, 1, 1, 3, 4, 2, 3, 3 }, toConsole);
				Console.WriteLine();
				// */
#if NET40 || NET35 || NET20
				System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-CA");
#else
				CultureInfo.CurrentCulture = new CultureInfo("fr-CA");
#endif
				TestDigitCount(
				 new string[] { "-2", "-1", "0", "1", "2", "123", "43,567", "0,0054", "0,124", "25,02", "2005", "3,00", "300,0", "2000", "325", "3002", "34 000", "40,40", "0,000 103 00", "2,005" + "\xA0" + "700e14", "5,4e-3", "3,000E2" },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 3, 4, 1, 3, 4, 2, 4, 5, 7, 2, 4 }, toConsole);

				Console.WriteLine();
				Console.WriteLine("Testing Round method");
				for (f = 2; f <= 4; f++) {
					Console.Write("{0}\t{1}\t{2}\t", 43.50872m,
					 SignificantDigits.Round(43.50872f, f), SignificantDigits.Round(43.50872, f));
					Console.WriteLine(SignificantDigits.Round(43.50872m, f));
				}
				dblComputed = (0.02015 * 0.25) - 0.001;
				Console.WriteLine("{0} -> {1}", dblComputed, SignificantDigits.Round(dblComputed, 1));

				var karFiveFiguresInput = new double[8] { 42.08651, 42.08615, 4286099, 4200800, 0.0000986013333, 1.00457e-14, 2.04445, 1.0406899e7 };
				var karFiveFiguresExpected = new double[8] { 42.087, 42.086, 4286100, 4200800, 0.000098601, 1.0046e-14, 2.0445, 1.0407e7 };
				for (f = 0; f < 8; f++) {
					var input = karFiveFiguresInput[f];
					var expected = karFiveFiguresExpected[f];
					dblComputed = SignificantDigits.Round(input, 5, RoundingMode.AwayFromZero);
					// This is the test of RoundOff.Equals itself
#pragma warning disable RECS0030 // Suggests using the class declaring a static function when calling it
					Console.WriteLine(RoundOff.Equals(dblComputed, expected) ? "{0,14} -> {1,10} correct" : "{0,14} -> {1,10} WRONG (should be {2})",
#pragma warning restore RECS0030 // Suggests using the class declaring a static function when calling it
					 input, dblComputed, expected);
				}
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
			} finally {
#if NET40 || NET35 || NET20
				System.Threading.Thread.CurrentThread.CurrentCulture = ciOriginal;
#else
				CultureInfo.CurrentCulture = ciOriginal;
#endif
			}

			Console.WriteLine();

			const decimal b = -1.55859375m;
			const decimal c = -4.8828125m;
			Console.WriteLine("Zeros for 1x² + {0}x + {1} : {2}", b, c, ExtraMath.SolveQuadratic(1, b, c));
		}

		private static void TestDigitCount<T>(
		T[] values, byte[] expectedCounts, Action<string> messageWriter) where T : IConvertible
		{
			if ((values != null) && (expectedCounts != null)) {
				var c = values.Length;
				if (expectedCounts.Length != c) {
					throw new ArgumentException("The count of elements in each of the passed arrays must be identical.");
				}
				if (messageWriter != null) {
					messageWriter("Data type: " + typeof(T).Name);
				}

				for (var i = 0; i < c; i++) {
					var value = values[i];
					var expectedCount = expectedCounts[i];
					var d = SignificantDigits.Count(value);
					var ok = d == expectedCount;
					if (messageWriter != null) {
						messageWriter(String.Format(
						 ok ? "{0,13} -> {1} correct" : "{0,13} -> {1} WRONG (should be {2})",
						 value, d, expectedCount));
					}
				}
			}
		}

		private static void WriteCompact(string text)
		{
			if (text.Contains(" correct")) {
				Console.Write(text.Substring(0, text.IndexOf("->", StringComparison.Ordinal)).Trim());
				Console.Write(" ; ");
			} else {
				Console.WriteLine();
				Console.WriteLine(text);
			}
		}
	}
}
