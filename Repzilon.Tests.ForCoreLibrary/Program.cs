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
using System.Globalization;
using System.Runtime.InteropServices;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal enum TriState : byte
	{
		Unknown,
		False,
		True
	}

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
#if !NET20
			dicTests.Add("Peptides", PeptideGuess.Run);
#endif
			dicTests.Add("Calculus", CalculusTest.Run);
			dicTests.Add("Optics", OpticsTest.Run);

#if NET40 || NET35 || NET20
			TriState enuWorkaroundCygwin = TriState.Unknown;
#else
			TriState enuWorkaroundCygwin = Console.IsInputRedirected ? TriState.True : TriState.Unknown;
#endif
			DisplayMenu(enuWorkaroundCygwin, dicTests);
			char chrPressed = MyReadKey(ref enuWorkaroundCygwin);
			while (char.ToUpperInvariant(chrPressed) != 'Q') {
#if NETCOREAPP1_0
				if (Char.IsDigit(chrPressed)) {
					int intPressed = Int32.Parse(chrPressed.ToString());
#else
				if (Uri.IsHexDigit(chrPressed)) {
					int intPressed = Int32.Parse(chrPressed.ToString(), NumberStyles.HexNumber);
#endif
					if ((intPressed >= 1) && (intPressed <= dicTests.Count)) {
						Console.Write(Environment.NewLine);
						DateTime dtmStart = DateTime.UtcNow;
						dicTests.Values[intPressed - 1](args);
						TimeSpan tsElapsed = DateTime.UtcNow - dtmStart;
						Console.WriteLine("{0} Test took {1:n3}s", dicTests.Keys[intPressed - 1], tsElapsed.TotalSeconds);
						DisplayMenu(enuWorkaroundCygwin, dicTests);
					}
				}
				chrPressed = MyReadKey(ref enuWorkaroundCygwin);
			}
		}

		private static char MyReadKey(ref TriState workaroundCygwin)
		{
			if (workaroundCygwin == TriState.True) {
				return ReadFirstCharOfLine();
			} else if (workaroundCygwin == TriState.False) {
				return ReadKey2();
			} else {
				try {
					return ReadKey2();
				} catch (InvalidOperationException) {
					workaroundCygwin = TriState.True;
					//System.Diagnostics.Debugger.Launch();
					return ReadFirstCharOfLine();
				}
			}
		}

		private static char ReadFirstCharOfLine()
		{
			System.Threading.Thread.Sleep(10000);
			var strLine = Console.ReadLine().Trim();
			return (strLine.Length­ > 0) ? strLine[0] : 'Q';
		}

		private static char ReadKey2()
		{
			return Console.ReadKey().KeyChar;
		}

		private static void DisplayMenu(TriState workaroundCygwin, IDictionary<string, Action<string[]>> allTests)
		{
			Console.WriteLine("====================================");
			Console.WriteLine("Repzilon Libraries Interactive Tests");
			Console.WriteLine("====================================");
			Console.Write(Environment.NewLine);
			var i = 1;
			foreach (var kvp in allTests) {
#if (NETCOREAPP1_0)
				if (i < 10) {
#endif
				Console.WriteLine(@"    ({0:X}) {1}", i, kvp.Key);
				i++;
#if (NETCOREAPP1_0)
				}
#endif
			}

			Console.Write(Environment.NewLine);
			Console.Write(workaroundCygwin == TriState.True ?
			 "In the next 10 seconds, type the number of the test or Q and press Return: " :
			 "Press the number corresponding to the test, or Q to quit: ");
		}

		internal static void OutputSizeOf<T>() where T : struct
		{
			try {
#if NET40 || NET35 || NET20
				Type typT = typeof(T);
				Console.WriteLine("Size of struct {0} is {1} bytes", typT.Name.Replace("`1", "<T>"), Marshal.SizeOf(typT));
#else
				var newT = new T();
				var typT = newT.GetType();
				var strOfT = "<T>";
				if (typT.GenericTypeArguments.Length == 1) {
					strOfT = "<" + typT.GenericTypeArguments[0].Name + ">";
				}
				Console.WriteLine("Size of struct {0} is {1} bytes", typT.Name.Replace("`1", strOfT), Marshal.SizeOf<T>(newT));
#endif
			} catch (ArgumentException) {
				// do nothing
			}
		}
	}
}
