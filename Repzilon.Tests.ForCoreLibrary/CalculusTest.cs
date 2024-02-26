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
			DateTime dtmStart;
			TimeSpan tsDuration;
			IConvertible result;

			dtmStart = DateTime.UtcNow;
			for (int i = 0; i < 10000; i++) {
				result = ExtraMath.Summation(1, 729, CalculusWork1No2FP);
			}
			tsDuration = DateTime.UtcNow - dtmStart;
			result = ExtraMath.Summation(1, 729, CalculusWork1No2FP);
			Console.WriteLine("={0}\tMath.Pow\t{1,6:n0} Hz", result, 10000 / tsDuration.TotalSeconds);

			dtmStart = DateTime.UtcNow;
			for (int i = 0; i < 10000; i++) {
				result = ExtraMath.Summation(1, 729, CalculusWork1No2Int64);
			}
			tsDuration = DateTime.UtcNow - dtmStart;
			Console.WriteLine("={0}\tExtraMath.Pow\t{1,6:n0} Hz", result, 10000 / tsDuration.TotalSeconds);
		}

		private static double CalculusWork1No2FP(int k)
		{
			return 3 * k * Math.Pow(-1, k - 1);
		}

		private static long CalculusWork1No2Int64(int k)
		{
			return 3 * k * Pow(-1, checked((ushort)(k - 1)));
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
