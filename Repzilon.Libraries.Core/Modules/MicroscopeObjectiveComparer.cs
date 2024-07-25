//
//  MicroscopeObjectiveComparer.cs
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
using System.Collections.Generic;

namespace Repzilon.Libraries.Core
{
	public static class MicroscopeObjectiveComparer
	{
		public static readonly IComparer<MicroscopeObjective> ByLuminosity = new LuminosityComparer();
		public static readonly IComparer<MicroscopeObjective> ByFieldOfView = new FieldOfViewComparer();
		public static readonly IComparer<MicroscopeObjective> ByResolution = new ResolutionComparer();

		class LuminosityComparer : IComparer<MicroscopeObjective>
		{
			public int Compare(MicroscopeObjective x, MicroscopeObjective y)
			{
				return Math.Sign(x.Aperture() - y.Aperture());
			}
		}

		class FieldOfViewComparer : IComparer<MicroscopeObjective>
		{
			public int Compare(MicroscopeObjective x, MicroscopeObjective y)
			{
				return Math.Sign(FieldOfViewRatio(x) - FieldOfViewRatio(y));
			}

			private static float FieldOfViewRatio(MicroscopeObjective lens)
			{
				return (float)(1.0 / (lens.FocalLengthInMeters * lens.Aperture()));
			}
		}

		class ResolutionComparer : IComparer<MicroscopeObjective>
		{
			public int Compare(MicroscopeObjective x, MicroscopeObjective y)
			{
				const float ySquared = 0.0256f; // 16 centimeters is standardized distance
				var halfD1 = x.Aperture() * 0.5f;
				var halfD2 = y.Aperture() * 0.5f;
				var ratio = (halfD1 * Math.Sqrt(ySquared + (halfD2 * halfD2))) /
				 (halfD2 * Math.Sqrt(ySquared + (halfD1 * halfD1)));
				return Math.Sign(Math.Log10(ratio));
			}
		}
	}
}
