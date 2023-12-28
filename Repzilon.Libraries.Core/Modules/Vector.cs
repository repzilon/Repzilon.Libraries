//
//  Vector.cs
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

namespace Repzilon.Libraries.Core
{
	public static class Vector
	{
		public static TwoDVector<T> New<T>(T x, T y)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>
		{
			return new TwoDVector<T>(x, y);
		}

		public static ThreeDVector<T> New<T>(T x, T y, T z)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>
		{
			return new ThreeDVector<T>(x, y, z);
		}

		public static PolarVector<T> New<T>(T norm, T angle, AngleUnit unit)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>
		{
			return new PolarVector<T>(norm, angle, unit);
		}

		public static PolarVector<T> New<T>(T norm, Angle<T> angle)
		where T : struct, IFormattable, IEquatable<T>, IComparable<T>
		{
			return new PolarVector<T>(norm, angle);
		}
	}
}

