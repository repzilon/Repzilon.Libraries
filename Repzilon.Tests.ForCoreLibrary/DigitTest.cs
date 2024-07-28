//
//  DigitTest.cs
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
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class DigitTest
	{
		internal static void Run(string[] args)
		{
			Action<string> toConsole = WriteCompact;
			try {
				//* Testing number types
				TestDigitCount(new short[] { -2, -1, 0, 1, 2, 123, 2005, 2000, 325, 3002 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 4, 1, 3, 4 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(new int[] { -2, -1, 0, 1, 2, 123, 2005, 2000, 325, 3002, 34000 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 4, 1, 3, 4, 2 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(new long[] { -2, -1, 0, 1, 2, 123, 2005, 2000, 325, 3002, 34000 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 4, 1, 3, 4, 2 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(new ushort[] { 0, 1, 2, 123, 2005, 2000, 325, 3002, 34000 },
				 new byte[] { 1, 1, 1, 3, 4, 1, 3, 4, 2 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(
				 new float[] { -2, -1, 0, 1, 2, 123f, 43.567f, 0.0054f, 0.124f, 25.02f, 2005f, 3.00f, 300.0f, 2000f, 325f, 3002f, 34000f, 40.40f, 0.00010300f },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 1, 1, 1, 3, 4, 2, 3, 3 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(
				 new double[] { -2, -1, 0, 1, 2, 123, 43.567, 0.0054, 0.124, 25.02, 2005, 3.00, 300.0, 2000, 325, 3002, 34000, 40.40, 0.00010300 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 1, 1, 1, 3, 4, 2, 3, 3 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(
				 new decimal[] { -2, -1, 0, 1, 2, 123, 43.567m, 0.0054m, 0.124m, 25.02m, 2005, 3.00m, 300.0m, 2000, 325, 3002, 34000, 40.40m, 0.00010300m },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 1, 1, 1, 3, 4, 2, 3, 3 }, toConsole);
				Console.Write(Environment.NewLine);
				// */
#if NET40 || NET35 || NET20
				System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-CA");
#else
				CultureInfo.CurrentCulture = new CultureInfo("fr-CA");
#endif
				TestDigitCount(
				 new string[] { "-2", "-1", "0", "1", "2", "123", "43,567", "0,0054", "0,124", "25,02", "2005", "3,00", "300,0", "2000", "325", "3002", "34 000", "40,40", "0,000 103 00", "2,005" + "\xA0" + "700e14", "5,4e-3", "3,000E2" },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 3, 4, 1, 3, 4, 2, 4, 5, 7, 2, 4 }, toConsole);

				Console.Write(Environment.NewLine);
				Console.WriteLine("Testing Round method");
				for (byte f = 2; f <= 4; f++) {
					var sngRounded = SignificantDigits.Round(43.50872f, f, RoundingMode.ToEven);
					var dblRounded = SignificantDigits.Round(43.50872, f, RoundingMode.ToEven);
					var dcmRounded = SignificantDigits.Round(43.50872m, f, RoundingMode.ToEven);
					Console.WriteLine("{0}\t{1}\t{2}\t{3}", 43.50872m, sngRounded, dblRounded, dcmRounded);
				}
				var x = (0.02015 * 0.25) - 0.001;
				var xr = SignificantDigits.Round(x, 1, RoundingMode.ToEven);
				Console.WriteLine("{0} -> {1}", x, xr);

				var karFiveFiguresInput = new double[] { 42.08651, 42.08615, 4286099, 4200800, 0.0000986013333, 1.00457e-14, 2.04445, 1.0406899e7 };
				var karFiveFiguresExpected = new double[] { 42.087, 42.086, 4286100, 4200800, 0.000098601, 1.0046e-14, 2.0445, 1.0407e7 };
				for (int i = 0; i < karFiveFiguresInput.Length; i++) {
					var dblComputed = SignificantDigits.Round(karFiveFiguresInput[i], 5, RoundingMode.AwayFromZero);
					Console.WriteLine(RoundOff.Equals(dblComputed, karFiveFiguresExpected[i]) ? "{0,14} -> {1,10} correct" : "{0,14} -> {1,10} WRONG (should be {2})",
					 karFiveFiguresInput[i], dblComputed, karFiveFiguresExpected[i]);
				}
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
			}

			Console.Write(Environment.NewLine);

			const decimal b = -1.55859375m;
			const decimal c = -4.8828125m;
			var pr = ExtraMath.SolveQuadratic(1, b, c);
			Console.WriteLine("Zeros for 1x² + {0}x + {1} : {2}", b, c, pr);
		}

		private static bool[] TestDigitCount<T>(
		T[] values, byte[] expectedCounts, Action<string> messageWriter) where T : IConvertible
		{
			if ((values != null) && (expectedCounts != null)) {
				int c = values.Length;
				if (expectedCounts.Length != c) {
					throw new ArgumentException("The count of elements in each of the passed arrays must be identical.");
				}
				if (messageWriter != null) {
					messageWriter(String.Format("Data type: {0}", typeof(T).Name));
				}

				var blnarOk = new bool[c];
				for (int i = 0; i < c; i++) {
					var d = SignificantDigits.Count(values[i]);
					blnarOk[i] = d == expectedCounts[i];
					if (messageWriter != null) {
						messageWriter(String.Format(
						 blnarOk[i] ? "{0,13} -> {1} correct" : "{0,13} -> {1} WRONG (should be {2})",
						 values[i], d, expectedCounts[i]));
					}
				}
				return blnarOk;
			}
#if NET40 || NET35 || NET20
			return new bool[0];
#else
			return Array.Empty<bool>();
#endif
		}

		private static void WriteCompact(string text)
		{
			if (text.Contains(" correct")) {
				Console.Write(text.Substring(0, text.IndexOf("->")).Trim());
				Console.Write(" ; ");
			} else {
				Console.Write(Environment.NewLine);
				Console.WriteLine(text);
			}
		}
	}
}
