//
//  IComparableEnzymeKinematic.cs
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

namespace Repzilon.Libraries.Core.Biochemistry
{
	internal interface IComparableEnzymeKinematic
	{
		IComparable VmaxNumber { get; }
		string VmaxUnit { get; }
		IComparable KmNumber { get; }
		string KmUnit { get; }
		IComparable Correlation { get; }
		EnzymeSpeedRepresentation Representation { get; }
	}
}
