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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Repzilon.Tests.ForCoreLibrary
{
	static class Program
	{
		static void Main(string[] args)
		{
			var dicTests = new SortedList<string, Action<string[]>>();
			dicTests.Add("Significant Digits", DigitTest.Run);
			dicTests.Add("Matrix", MatrixTest.Run);
			dicTests.Add("Pascal Triangle", PascalTriangleTest.Run);
			dicTests.Add("Vectors", VectorTest.Run);
			dicTests.Add("Linear Regression", LinearRegressionTest.Run);
			dicTests.Add("Chemistry", MolarMassTest.Run);
			dicTests.Add("Dilution", DilutionTest.Run);
			dicTests.Add("Peptides", PeptideGuess.Run);

			DisplayMenu(dicTests);
			var cki = Console.ReadKey();
			while (Char.ToUpperInvariant(cki.KeyChar) != 'Q') {
				if (Char.IsDigit(cki.KeyChar)) {
					int intPressed = Convert.ToInt32(cki.KeyChar.ToString());
					int i = 1;
					bool blnRan = false;
					DateTime dtmStart;
					foreach (var kvp in dicTests) {
						if (i == intPressed) {
							Console.Write(Environment.NewLine);
							dtmStart = DateTime.UtcNow;
							kvp.Value(args);
							TimeSpan tsElapsed = DateTime.UtcNow - dtmStart;
							blnRan = true;
							Console.WriteLine("{0} Test took {1:n3}s", kvp.Key, tsElapsed.TotalSeconds);
						}
						i++;
					}
					if (blnRan) {
						DisplayMenu(dicTests);
					}
				}
				cki = Console.ReadKey();
			}
		}

		private static void DisplayMenu(IDictionary<string, Action<string[]>> allTests)
		{
			Console.WriteLine("====================================");
			Console.WriteLine("Repzilon Libraries Interactive Tests");
			Console.WriteLine("====================================");
			Console.Write(Environment.NewLine);
			int i = 1;
			foreach (var kvp in allTests) {
				Console.WriteLine(@"    ({0}) {1}", i, kvp.Key);
				i++;
			}

			Console.Write(Environment.NewLine);
			Console.Write("Press the number corresponding to the test, or Q to quit: ");
		}

		internal static void OutputSizeOf<T>() where T : struct
		{
			try {
				Console.WriteLine("Size of struct {0} is {1} bytes", typeof(T).Name, Marshal.SizeOf<T>());
			} catch (ArgumentException) {
				// do nothing
			}
		}
	}
}
