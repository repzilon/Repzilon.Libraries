//
//  PascalTriangleTest.cs
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
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.ForCoreLibrary
{
	static class PascalTriangleTest
	{
		internal static void Run(string[] args)
		{
			for (byte i = 4; i <= 12; i++) {
				Console.WriteLine(PascalTriangle.Format(PascalTriangle.Make(i)));
			}
		}
	}
}
