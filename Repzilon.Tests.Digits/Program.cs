//
//  Program.cs
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
using Repzilon.Libraries.Core;
using System;

namespace Repzilon.Tests.Digits
{
	class Program
	{
		static void Main(string[] args)
		{
			Action<string> toConsole = Console.WriteLine;
			try {
				TestDigitCount(new short[] { -2, -1, 0, 1, 2, 123, 2005, 2000 }, new byte[] { 1, 1, 1, 1, 1, 3, 4, 1 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(new int[] { -2, -1, 0, 1, 2, 123, 2005, 2000 }, new byte[] { 1, 1, 1, 1, 1, 3, 4, 1 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(new long[] { -2, -1, 0, 1, 2, 123, 2005, 2000 }, new byte[] { 1, 1, 1, 1, 1, 3, 4, 1 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(new ushort[] { 0, 1, 2, 123, 2005, 2000 }, new byte[] { 1, 1, 1, 3, 4, 1 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(
				 new float[] { -2, -1, 0, 1, 2, 123f, 43.567f, 0.0054f, 0.124f, 25.02f, 2005f, 3.00f, 300.0f, 2000f },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 1, 1, 1 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(
				 new double[] { -2, -1, 0, 1, 2, 123, 43.567, 0.0054, 0.124, 25.02, 2005, 3.00, 300.0, 2000 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 1, 1, 1 }, toConsole);
				Console.Write(Environment.NewLine);
				TestDigitCount(
				 new decimal[] { -2, -1, 0, 1, 2, 123, 43.567m, 0.0054m, 0.124m, 25.02m, 2005, 3.00m, 300.0m, 2000 },
				 new byte[] { 1, 1, 1, 1, 1, 3, 5, 2, 3, 4, 4, 1, 1, 1 }, toConsole);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
			}

			Console.WriteLine("Press Enter to exit...");
			Console.ReadLine();
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

				bool[] blnarOK = new bool[c];
				for (int i = 0; i < c; i++) {
					var d = SignificantDigits.Count(values[i]);
					blnarOK[i] = d == expectedCounts[i];
					if (messageWriter != null) {
						messageWriter(String.Format(
						 blnarOK[i] ? "{0}\t-> {1} correct" : "{0}\t-> {1} WRONG (should be {2})",
						 values[i], d, expectedCounts[i]));
					}
				}
				return blnarOK;
			}
			return Array.Empty<bool>();
		}
	}
}
