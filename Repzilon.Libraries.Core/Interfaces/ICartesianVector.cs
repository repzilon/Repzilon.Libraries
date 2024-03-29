﻿//
//  ICartesianVector.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2023-2024 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;

namespace Repzilon.Libraries.Core.Vectors
{
	public interface ICartesianVector<T> : IPoint<T>
	where T : struct, IFormattable, IEquatable<T>, IComparable<T>, IComparable
	{
		ICartesianVector<TOut> Cast<TOut>() where TOut : struct, IFormattable, IEquatable<TOut>, IComparable<TOut>, IComparable;
		double Norm();
		ICartesianVector<T> ToUnitary();
	}
}
