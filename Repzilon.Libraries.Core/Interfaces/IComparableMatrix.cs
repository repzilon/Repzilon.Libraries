//
//  IComparableMatrix.cs
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

namespace Repzilon.Libraries.Core
{
	internal interface IComparableMatrix
	{
		byte Lines { get; }
		byte Columns { get; }
		byte? AugmentedColumn { get; }

		IComparable ValueAt(byte l, byte c);

		bool SameSize(IComparableMatrix other);
	}
}
