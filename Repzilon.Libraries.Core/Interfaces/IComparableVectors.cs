//
//  IComparableVectors.cs
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
	internal interface IComparableTwoDVector
	{
		IComparable X { get; }
		IComparable Y { get; }
	}

	// Note : Do not make IComparableThreeDVector "inherit" from IComparableTwoDVector.
	// A 3D vector is not considered as an extended 2D vector, but as a different notion.
	internal interface IComparableThreeDVector
	{
		IComparable X { get; }
		IComparable Y { get; }
		IComparable Z { get; }
	}

	internal interface IComparablePolarVector
	{
		IComparable Norm { get; }
		IAngle Angle { get; }
	}
}
