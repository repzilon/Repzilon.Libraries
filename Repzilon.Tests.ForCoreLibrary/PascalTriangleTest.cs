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

namespace Repzilon.Tests.Pascal
{
	internal static class Program
	{
		static void Main()
		{
			for (byte i = 4; i <= 12; i++) {
				Console.WriteLine(PascalTriangle.Format(PascalTriangle.Make(i)));
			}

			Console.WriteLine("Press Enter to exit...");
			Console.ReadLine();
		}
	}
}
