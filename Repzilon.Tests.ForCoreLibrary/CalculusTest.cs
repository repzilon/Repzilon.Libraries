//
//  CalculusTest.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024 René Rhéaume
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
	internal static class CalculusTest
	{
		internal static void Run(string[] args)
		{
			Console.WriteLine("Calcul intégral travail 1 #2");
			SummationTest(10000, 729, "Math.Pow", CalculusWork1No2FP);
			SummationTest(10000, 729, "Pow(i32, u16)", CalculusWork1No2Int64);
			SummationTest(10000, 729, "IIf", CalculusWork1No2IIf);
		}

		private static void SummationTest<T>(int benchLoops, int summationUpper, string legend, Func<int,T> forEach)
		where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
		{
			var dtmStart = DateTime.UtcNow;
			T result;
			for (int i = 0; i < benchLoops; i++) {
				result = ExtraMath.Summation(1, summationUpper, forEach);
			}
			var tsDuration = DateTime.UtcNow - dtmStart;
			result = ExtraMath.Summation(1, summationUpper, forEach);
			Console.WriteLine("={0}\t{2,-16} {1,7:n0} Hz", result, benchLoops / tsDuration.TotalSeconds, legend);
		}

		private static double CalculusWork1No2FP(int k)
		{
			return 3 * k * Math.Pow(-1, k - 1);
		}

		private static long CalculusWork1No2Int64(int k)
		{
			return 3 * k * Pow(-1, checked((ushort)(k - 1)));
		}

		private static long CalculusWork1No2IIf(int k)
		{
			return 3 * k * (((k - 1) % 2 == 0) ? 1 : -1);
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
	}
}
