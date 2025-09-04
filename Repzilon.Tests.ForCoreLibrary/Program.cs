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
using System.Diagnostics;
using System.Globalization;
#if NETCOREAPP3_1 || NET5_0 || NET6_0
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal enum TriState : byte
	{
		Unknown,
		False,
		True
	}

	internal enum SupportLevel : byte
	{
		None,
		Partial,
		Complete
	}

	internal static class Program
	{
		internal static readonly SupportLevel UnicodeTerminal = SupportsUnicodeTerminal();

		private static void Main(string[] args)
		{
			var dicTests = new SortedList<string, Action<string[]>>();
			dicTests.Add("Significant Digits", DigitTest.Run);
			dicTests.Add("Matrix", MatrixTest.Run);
			dicTests.Add("Pascal Triangle", PascalTriangleTest.Run);
			dicTests.Add("Vectors", VectorTest.Run);
			dicTests.Add("Linear Regression", LinearRegressionTest.Run);
			dicTests.Add("Chemistry", ChemistryTest.Run);
			dicTests.Add("Dilution", DilutionTest.Run);
#if !NET20
			dicTests.Add("Peptides", PeptideGuess.Run);
#endif
			dicTests.Add("Calculus", CalculusTest.Run);
			dicTests.Add("Optics", OpticsTest.Run);
			dicTests.Add("Normal Distribution", NormalLawTest.Run);
			dicTests.Add("Student T-distribution", StudentTest.Run);
			dicTests.Add("Instrumental Analysis", InstrumentalAnalysisTest.Run);
			dicTests.Add("Specialized Regressions", SpecializedRegressionTest.Run);

			// Needed on Windows to enable Unicode support
#if NETFRAMEWORK
			var os = Environment.OSVersion;
			if ((os.Platform == PlatformID.Win32NT) && (os.Version.Major >= 6)) {
#else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
#endif
				Console.OutputEncoding = Encoding.UTF8;
			}
#pragma warning disable U2U1104
			Console.WriteLine("CurrentCulture: {0}\tCurrentUICulture: {1}\tOutputEncoding: {2} •₀₁₂₃₄₅₆₇₈₉ₘₐₓµ∫∞ŷΔ²",
			 CultureInfo.CurrentCulture.Name, CultureInfo.CurrentUICulture.Name,
			 Console.OutputEncoding.WebName);
#pragma warning restore U2U1104
			if (args == null || args.Length < 1) {
				RunInteractively(dicTests, args);
			} else if ((args[0] == "--help") || (args[0] == "-h") || (args[0] == "/?")) {
				OutputUsage();
			} else if (args[0] == "--demos") {
				var lstSequence = new List<int>();
				int i;
				for (i = 1; i < args.Length; i++) {
					var strarTests = args[i].Split(',');
					for (var j = 0; j < strarTests.Length; j++) {
						int numero;
						if (Int32.TryParse(strarTests[j], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out numero) &&
						(numero >= 1) && (numero <= dicTests.Count)) {
							lstSequence.Add(numero);
						}
					}
				}

				if (lstSequence.Count > 0) {
					for (i = 0; i < lstSequence.Count; i++) {
						RunSingleDemo(lstSequence[i], dicTests, args);
					}
				} else {
					RunInteractively(dicTests, args);
				}
			} else {
				RunInteractively(dicTests, args);
			}
		}

		private static void RunInteractively(SortedList<string, Action<string[]>> allDemos, string[] args)
		{
#if NET40 || NET35 || NET20
			var enuWorkaroundCygwin = TriState.Unknown;
#else
			var enuWorkaroundCygwin = Console.IsInputRedirected ? TriState.True : TriState.Unknown;
#endif
			DisplayMenu(enuWorkaroundCygwin, allDemos);
			var chrPressed = MyReadKey(ref enuWorkaroundCygwin);
			while (char.ToUpperInvariant(chrPressed) != 'Q') {
#if NETCOREAPP1_0
				if (Char.IsDigit(chrPressed)) {
					var intPressed = Int32.Parse(chrPressed.ToString());
#else
				if (Uri.IsHexDigit(chrPressed)) {
					var intPressed = Int32.Parse(chrPressed.ToString(), NumberStyles.HexNumber);
#endif
					if ((intPressed >= 1) && (intPressed <= allDemos.Count)) {
						RunSingleDemo(intPressed, allDemos, args);
						DisplayMenu(enuWorkaroundCygwin, allDemos);
					}
				}
				chrPressed = MyReadKey(ref enuWorkaroundCygwin);
			}
		}

		private static void RunSingleDemo(int numero, SortedList<string, Action<string[]>> allDemos, string[] args)
		{
			const float kToKiB = 1.0f / 1024;
			Console.WriteLine();
			var lngRamBefore = Math.Ceiling(CurrentMemoryUsage() * kToKiB);
			var dtmStart = DateTime.UtcNow;
			allDemos.Values[numero - 1](args);
			var tsElapsed = DateTime.UtcNow - dtmStart;
			// ReSharper disable once InconsistentNaming
			var lngRamAfterNoGC = Math.Ceiling(CurrentMemoryUsage() * kToKiB);
			Console.Write("{0} Demo took {1:n3}s\t{2}: ",
			 allDemos.Keys[numero - 1], tsElapsed.TotalSeconds, IsMacOsX() ? "GC memory" : "RAM");
			Console.Write("{0} kiB -> {1} kiB", lngRamBefore, lngRamAfterNoGC);
			// Call GC.Collect only when memory usage blows up, otherwise it makes the process consume more RAM
			if (lngRamAfterNoGC > 50 * 1024) {
#pragma warning disable S1215 // "GC.Collect" should not be called
				GC.Collect();
#pragma warning restore S1215 // "GC.Collect" should not be called
				Console.WriteLine(" -> {0} kiB", Math.Ceiling(CurrentMemoryUsage() * kToKiB));
			}
			Console.WriteLine();
		}

		private static long CurrentMemoryUsage()
		{
			if (IsMacOsX()) {
				// Microsoft is too lazy to provide a libproc wrapper. But libproc is poorly documented, so I will not
				// dwell into it right now. Working set is also a poor metric: it is only reliable when swapping
				// and memory compression are disabled. I will use the GC memory, while underestimating memory usage
				// by a large measure, it does not have the reliability problem.
				return GC.GetTotalMemory(false);
			} else {
				// A Process instance is more like a snapshot
				using (var prcSelf = Process.GetCurrentProcess()) {
					return prcSelf.PrivateMemorySize64;
				}
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
			Console.WriteLine();
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

			Console.WriteLine();
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
			var typarGta = dotnetType.GetGenericArguments();
#else
			var typarGta = dotnetType.GenericTypeArguments;
#endif
			return dotnetType.Name.Replace("`1", (typarGta.Length == 1) ? "<" + typarGta[0].Name + ">" : "<T>");
		}

		internal static void OutputHeading(string text)
		{
			Console.WriteLine();
			Console.WriteLine(text);
			Console.WriteLine(new String('-', text.Length));
		}

		private static bool IsMacOsX()
		{
#if NETFRAMEWORK
			return Environment.OSVersion.Platform == PlatformID.MacOSX;
#else
			return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif
		}

		private static SupportLevel SupportsUnicodeTerminal()
		{
			if (IsMacOsX()) {
				return SupportLevel.Complete;
			} else {
#if NETFRAMEWORK
				var os = Environment.OSVersion;
				if (os.Platform == PlatformID.Win32NT) {
					return os.Version.CompareTo(new Version(6, 2)) >= 0 ? SupportLevel.Partial : SupportLevel.None;
				}
#else
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
					return SupportLevel.Partial;
				}
#endif
				else {
					return (Console.OutputEncoding is UTF8Encoding) || (Console.OutputEncoding is UnicodeEncoding) ?
					 SupportLevel.Complete : SupportLevel.None;
				}
			}
		}

		private static void OutputUsage()
		{
			Console.WriteLine(
#pragma warning disable UD0011 // The composite format string is not valid
@"NAME
	Repzilon.Tests.ForCoreLibrary - Demo program for Repzilon.Libraries

SYNOPSIS
	Repzilon.Tests.ForCoreLibrary {--help|-h|/?} - displays this message
	Repzilon.Tests.ForCoreLibrary --demos <comma separated test numbers>

DESCRIPTION
	When run without arguments, will display a menu listing demos, which
	can be selected by pressing a key associated with the demo, then
	return to the menu. You can exit the program by pressing Q in the menu.

	When the --demos argument is used, the demos identified with the same
	key as inside the menu will be run in sequence without displaying the
	menu, without asking for a key press and leave immediately. However,
	if no sequence is supplied, it will revert to the interactive mode,
	just like when no arguments are passed.

CONTACT INFO
	(C) 2022-2025 René Rhéaume <repzilon@users.noreply.github.com>
	Licensed under the MPL 2.0, available at https://mozilla.org/MPL/2.0/
	There is NO WARRANTY, to the extent of the law.
	Project page: https://github.com/repzilon/Repzilon.Libraries");
#pragma warning restore UD0011 // The composite format string is not valid
		}
	}
}
