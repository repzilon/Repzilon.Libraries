//
//  IAngle.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022-2023 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;

namespace Repzilon.Libraries.Core
{
	public enum AngleUnit : byte
	{
		Gradian = 0,
		Degree = 1,
		Radian = 57
	}

	public interface IAngle : IComparable, IFormattable, IEquatable<IAngle>, IComparable<IAngle>
#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
	, ICloneable
#endif
	{
		AngleUnit Unit { get; }
		decimal DecimalValue { get; }
		IAngle ConvertTo(AngleUnit unit);
		IAngle Normalize();
		double Sin();
		double Cos();
	}
}
