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
			Console.WriteLine(ExtraMath.Summation(1, 729, CalculusWork1No2));
		}

		private static double CalculusWork1No2(int k)
		{
			return 3 * k * Math.Pow(-1, k - 1);
		}
	}
}
