//
//  Program.cs
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
using System.Collections.Generic;
#if !NETCOREAPP1_0
using System.Globalization;
#endif
#if NETCOREAPP3_1 || NET5_0 || NET6_0
using System.Runtime.CompilerServices;
#else
using System.Runtime.InteropServices;
#endif

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
			dicTests.Add("Normal law", NormalLawTest.Run);
			dicTests.Add("Student distribution", StudentTest.Run);

#if NET40 || NET35 || NET20
			var enuWorkaroundCygwin = TriState.Unknown;
#else
			var enuWorkaroundCygwin = Console.IsInputRedirected ? TriState.True : TriState.Unknown;
#endif
			DisplayMenu(enuWorkaroundCygwin, dicTests);
			var chrPressed = MyReadKey(ref enuWorkaroundCygwin);
			while (char.ToUpperInvariant(chrPressed) != 'Q') {
#if NETCOREAPP1_0
				if (Char.IsDigit(chrPressed)) {
					var intPressed = Int32.Parse(chrPressed.ToString());
#else
				if (Uri.IsHexDigit(chrPressed)) {
					var intPressed = Int32.Parse(chrPressed.ToString(), NumberStyles.HexNumber);
#endif
					if ((intPressed >= 1) && (intPressed <= dicTests.Count)) {
						Console.Write(Environment.NewLine);
						var dtmStart = DateTime.UtcNow;
						dicTests.Values[intPressed - 1](args);
						var tsElapsed = DateTime.UtcNow - dtmStart;
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
			var strLine = Console.ReadLine();
			if (strLine != null) {
				strLine = strLine.Trim();
			}
			return !String.IsNullOrEmpty(strLine) ? strLine[0] : 'Q';
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
#if NETCOREAPP1_0
				if (i < 10) {
#endif
				Console.WriteLine(@"    ({0:X}) {1}", i, kvp.Key);
				i++;
#if NETCOREAPP1_0
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
			var typT = typeof(T);
#if DEBUG
			var strOfT = TypeNameWithGeneric(typT);
#endif

			try {
#if DEBUG
				Console.WriteLine("Size of struct {0,-30} is {1,3} bytes", strOfT,
#else
				Console.WriteLine("Size of struct {0,-30} is {1,3} bytes", TypeNameWithGeneric(typT),
#endif
#if NETCOREAPP3_1 || NET5_0 || NET6_0
				 Unsafe.SizeOf<T>());
#elif NETCOREAPP1_0
				 Marshal.SizeOf<T>());
#else
				 Marshal.SizeOf(typT));
#endif
#pragma warning disable CC0004 // Catch block cannot be empty
#if DEBUG
			} catch (ArgumentException excArg) {
				Console.Error.WriteLine("Size of struct {0} is unknown because {1}", strOfT, excArg.Message);
#else
			} catch (ArgumentException) {
				// do nothing
#endif
			}
#pragma warning restore CC0004 // Catch block cannot be empty
		}

		private static string TypeNameWithGeneric(Type dotnetType)
		{
#if !NETCOREAPP1_0
			var typarGTA = dotnetType.GetGenericArguments();
#else
			var typarGTA = dotnetType.GenericTypeArguments;
#endif
			return dotnetType.Name.Replace("`1", (typarGTA.Length == 1) ? "<" + typarGTA[0].Name + ">" : "<T>");
		}
	}
}
