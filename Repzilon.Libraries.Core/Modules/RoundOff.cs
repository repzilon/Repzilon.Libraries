//
//  RoundOff.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2024 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Globalization;

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

		public static double UpsizeError(double formerSingle)
		{
			return Math.Round(formerSingle, 15 - 2 - 5, MidpointRounding.ToEven);
		}

		public static decimal Error(decimal value)
		{
			var x = Math.Round(value, 25, MidpointRounding.ToEven);
			var ciC = CultureInfo.InvariantCulture;
			var s = x.ToString(ciC);
#if NETFRAMEWORK || NETSTANDARD
			return s.Contains(".") ? Decimal.Parse(s.TrimEnd('0'), ciC) : x;
#else
			return s.Contains('.') ? Decimal.Parse(s.TrimEnd('0'), ciC) : x;
#endif
		}

		[CLSCompliant(false)]
		public static Exp Error(Exp value)
		{
			var b = value.Base;
			return new Exp((float)Math.Round(value.Mantissa, 3 - 1, MidpointRounding.ToEven),
			 b == 0 ? (byte)10 : b, value.Exponent);
		}

		[CLSCompliant(false)]
		public static Exp18 Error(Exp18 value)
		{
			return new Exp18((float)Math.Round(value.Mantissa, 4 - 1, MidpointRounding.ToEven),
			 value.Base, value.Exponent);
		}

		public static bool AreEqual(float value, int k)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return Error(value) == k;
		}

		public static bool AreEqual(double value, int k)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return Error(value) == k;
		}

		public static bool AreEqual(float a, float b)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return Error(a) == Error(b);
		}
	}
}
