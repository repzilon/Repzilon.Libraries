//
//  OpticsTest.cs
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
using System.Collections.Generic;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class OpticsTest
	{
		internal static void Run(string[] args)
		{
			var moarExample3Orig = new MicroscopeObjective[] {
				new MicroscopeObjective(10 / 100.0f, 3.2f),
				new MicroscopeObjective(6.0f / 100.0f, 1.5f)
			};
			var moarExample3Mutable = new MicroscopeObjective[] {
				new MicroscopeObjective(10 / 100.0f, 3.2f),
				new MicroscopeObjective(6.0f / 100.0f, 1.5f)
			};
			OutputBestObjective("luminosity", MicroscopeObjectiveComparer.ByLuminosity, moarExample3Mutable, moarExample3Orig);
			OutputBestObjective("field of view", MicroscopeObjectiveComparer.ByFieldOfView, moarExample3Mutable, moarExample3Orig);
			OutputBestObjective("resolution", MicroscopeObjectiveComparer.ByResolution, moarExample3Mutable, moarExample3Orig);
		}

		private static void OutputBestObjective(string criterion, IComparer<MicroscopeObjective> comparer,
		MicroscopeObjective[] mutableArray, MicroscopeObjective[] originalArray)
		{
			Array.Sort(mutableArray, comparer);
			var best = mutableArray[mutableArray.Length - 1];
			Console.WriteLine("Best objective by {0}\tis at index {2}, which is {1}", criterion, best,
			 Array.IndexOf(originalArray, best));
		}
	}
}
