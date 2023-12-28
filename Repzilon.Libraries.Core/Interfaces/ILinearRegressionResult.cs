//
//  ILinearRegressionResult.cs
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
	public interface ILinearRegressionResult<T> : IFormattable
#if (!NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6)
	, ICloneable
#endif
	where T : struct
#if (!NETSTANDARD1_1)
	, IConvertible
#endif
	{
		T Slope { get; }
		T Intercept { get; }
		T Correlation { get; }

		T Determination();
		T ExplainedVariation();
		T ExtrapolateX(T y);
		T ExtrapolateY(T x);
		T InterceptStdDev();
		T RelativeBias(T x);
		T ResidualStdDev();
		T SlopeStdDev();
		T StdDevForYc(T yc, int k);
		T TotalError(T x);
		T TotalVariation();
		T UnexplainedVariation();
		T YExtrapolationConfidenceFactor(T x0, bool repeated);
	}
}