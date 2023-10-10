﻿//
//  Round.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022 René Rhéaume
//
// This Source Code Form is subject to the terms of the 
// Mozilla Public License, v. 2.0. If a copy of the MPL was 
// not distributed with this file, You can obtain one at 
// https://mozilla.org/MPL/2.0/.
//
using System;

namespace Repzilon.Libraries.Core
{
	public static class RoundOff
	{
		public static float Error(float value)
		{
			return (float)Math.Round(value, 7 - 2, MidpointRounding.ToEven);
		}

		public static double Error(double value)
		{
			return Math.Round(value, 15 - 2, MidpointRounding.ToEven);
		}
	}
}
