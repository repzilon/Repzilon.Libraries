//
//  MolecularBiology.cs
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
#if (!NET20)
using System;
using System.Linq;
using System.Collections.Generic;
using Repzilon.Libraries.Core.Regression;

namespace Repzilon.Libraries.Core
{
	public static class MolecularBiology
	{
		private static readonly RegressionModel<double> LowerBound;
		private static readonly RegressionModel<double> UpperBound;

		static MolecularBiology()
		{
			var karAgarose = new float[7] { 0.3f, 0.6f, 0.7f, 0.9f, 1.2f, 1.5f, 2.0f };
			var karMinSize = new short[7] { 5000, 1000, 800, 500, 400, 200, 100 };
			var karMaxSize = new ushort[7] { 60000, 20000, 10000, 7000, 6000, 3000, 2000 };
			var lstMin = new List<PointD>(7);
			var lstMax = new List<PointD>(7);
			for (byte i = 0; i < 7; i++) {
				var dblAgarose = Math.Round(karAgarose[i], 1);
				lstMin.Add(new PointD(dblAgarose, karMinSize[i]));
				lstMax.Add(new PointD(dblAgarose, karMaxSize[i]));
			}
			LowerBound = RegressionModel.Compute(lstMin);
			UpperBound = RegressionModel.Compute(lstMax);
		}

		public static AgaroseRetention[] AgaroseConcentration(params short[] forFragmentLengths)
		{
			var agaroseForLargest = Math.Round(Math.Min(3, UpperBound.Solve(forFragmentLengths.Max())), 1);
			var agaroseForSmallest = Math.Round(Math.Max(0.3, LowerBound.Solve(forFragmentLengths.Min())), 1);
			var cmin = Math.Min(agaroseForLargest, agaroseForSmallest);
			var cmax = Math.Max(agaroseForLargest, agaroseForSmallest);
			var dicMatches = new Dictionary<double, Dictionary<ushort, bool>>();
			for (var ca = cmin; ca <= cmax; ca = RoundOff.Error(ca + 0.1)) {
				var bpmin = Convert.ToUInt16(LowerBound.Evaluate(ca));
				var bpmax = Convert.ToUInt16(UpperBound.Evaluate(ca));
				var dicCheck = new Dictionary<ushort, bool>(forFragmentLengths.Length);
				for (int i = 0; i < forFragmentLengths.Length; i++) {
					var l = forFragmentLengths[i];
					dicCheck.Add((ushort)l, (l >= bpmin) && (l <= bpmax));
				}
				dicMatches.Add(ca, dicCheck);
			}
			var maxMigratable = dicMatches.Max(CountMigratableFragments);
			var bestConcentrations = dicMatches.Where(x => CountMigratableFragments(x) == maxMigratable).Select(SelectKey);

			var lstResults = new List<AgaroseRetention>();
			foreach (var c in bestConcentrations) {
				var lstBasePairs = new List<short>();
				foreach (var kvp in dicMatches[c]) {
					if (kvp.Value) {
						lstBasePairs.Add((short)kvp.Key);
					}
				}
				var agar = new AgaroseRetention();
				agar.MassVolumeConcentration = (float)c;
				agar.FragmentLengths = lstBasePairs.ToArray();
				lstResults.Add(agar);
			}

			return lstResults.ToArray();
		}

		private static double SelectKey(KeyValuePair<double, Dictionary<ushort, bool>> x)
		{
			return x.Key;
		}

		private static int CountMigratableFragments(KeyValuePair<double, Dictionary<ushort, bool>> x)
		{
			int count = 0;
			foreach (var y in x.Value) {
				if (y.Value) {
					count++;
				}
			}
			return count;
		}
	}
}
#endif
