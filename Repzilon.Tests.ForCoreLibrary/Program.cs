//
//  Program.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2024 René Rhéaume
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
	internal static class Program
	{
		private static void Main(string[] args)
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
			dicTests.Add("Calculus", CalculusTest.Run);

			DisplayMenu(dicTests);
			bool blnWordaroundCygwin = false;
			char chrPressed;
			try {
				chrPressed = Console.ReadKey().KeyChar;
			} catch (InvalidOperationException) {
				blnWordaroundCygwin = true;
				chrPressed = (char)Console.Read();
			}
			while (Char.ToUpperInvariant(chrPressed) != 'Q') {
				if (Char.IsDigit(chrPressed)) {
					int intPressed = Int32.Parse(chrPressed.ToString());				
					if ((intPressed >= 1) && (intPressed <= dicTests.Count)) {
						Console.Write(Environment.NewLine);
						DateTime dtmStart = DateTime.UtcNow;
						dicTests.Values[intPressed - 1](args);
						TimeSpan tsElapsed = DateTime.UtcNow - dtmStart;
						Console.WriteLine("{0} Test took {1:n3}s", dicTests.Keys[intPressed - 1], tsElapsed.TotalSeconds);
						DisplayMenu(dicTests);
					}
				}
				chrPressed = blnWordaroundCygwin ? (char)Console.Read() : Console.ReadKey().KeyChar;
			}
		}

		private static void DisplayMenu(IDictionary<string, Action<string[]>> allTests)
		{
			Console.WriteLine("====================================");
			Console.WriteLine("Repzilon Libraries Interactive Tests");
			Console.WriteLine("====================================");
			Console.Write(Environment.NewLine);
			var i = 1;
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
