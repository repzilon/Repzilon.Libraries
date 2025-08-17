//
//  ILogisticRegressionResult.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;

namespace Repzilon.Libraries.Core.Regression
{
	public interface ILogisticRegressionResult<T> : IFormattable
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct
	{
		int Count { get; }
		T Amplitude { get; }
		T Intercept { get; }
		T Location { get; }
		T Scale { get; }
		T Correlation { get; }

		T Determination();
		T InterpolateY(T x);
	}
}
