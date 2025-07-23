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
#if !NET20
using System;
using System.Linq;
using System.Collections.Generic;
using Repzilon.Libraries.Core.Regression;
// ReSharper disable RedundantExplicitArraySize

namespace Repzilon.Libraries.Core
{
	public static class MolecularBiology
	{
		private static readonly RegressionModel<double> LowerBound;
		private static readonly RegressionModel<double> UpperBound;

#pragma warning disable S3963 // "static" fields should be initialized inline
		static MolecularBiology()
		{
			const int kPoints = 7;
			var karAgarose = new float[kPoints] { 0.3f, 0.6f, 0.7f, 0.9f, 1.2f, 1.5f, 2.0f };
			var karMinSize = new short[kPoints] { 5000, 1000, 800, 500, 400, 200, 100 };
			var karMaxSize = new ushort[kPoints] { 60000, 20000, 10000, 7000, 6000, 3000, 2000 };
			var lstMin = new List<PointD>(kPoints);
			var lstMax = new List<PointD>(kPoints);
			for (byte i = 0; i < kPoints; i++) {
				var dblAgarose = Math.Round(karAgarose[i], 1);
				lstMin.Add(new PointD(dblAgarose, karMinSize[i]));
				lstMax.Add(new PointD(dblAgarose, karMaxSize[i]));
			}
			LowerBound = RegressionModel.Compute(lstMin);
			UpperBound = RegressionModel.Compute(lstMax);
		}
#pragma warning restore S3963 // "static" fields should be initialized inline

		public static AgaroseRetention[] AgaroseConcentration(params short[] forFragmentLengths)
		{
			var rmdUpper = UpperBound;
			var rmdLower = LowerBound;
			var agaroseForLargest = Math.Round(Math.Min(3, rmdUpper.Solve(forFragmentLengths.Max())), 1);
			var agaroseForSmallest = Math.Round(Math.Max(0.3, rmdLower.Solve(forFragmentLengths.Min())), 1);
			var cmin = Math.Min(agaroseForLargest, agaroseForSmallest);
			var cmax = Math.Max(agaroseForLargest, agaroseForSmallest);
			var dicMatches = new Dictionary<double, Dictionary<ushort, bool>>(checked((byte)(10 * (cmax - cmin))));
			for (var ca = cmin; ca <= cmax; ca = RoundOff.Error(ca + 0.1)) {
				var bpmin = Convert.ToUInt16(rmdLower.Evaluate(ca));
				var bpmax = Convert.ToUInt16(rmdUpper.Evaluate(ca));
				var dicCheck = new Dictionary<ushort, bool>(forFragmentLengths.Length);
#pragma warning disable CC0006 // Use foreach
				// ReSharper disable once ForCanBeConvertedToForeach
				for (var i = 0; i < forFragmentLengths.Length; i++) {
#pragma warning restore CC0006 // Use foreach
					var l = forFragmentLengths[i];
					dicCheck.Add((ushort)l, (l >= bpmin) && (l <= bpmax));
				}
				dicMatches.Add(ca, dicCheck);
			}
			var maxMigratable = dicMatches.Max(CountMigratableFragments);
			var bestConcentrations = dicMatches.Where(x => CountMigratableFragments(x) == maxMigratable).Select(SelectKey);

			var lstResults = new List<AgaroseRetention>();
			foreach (var c in bestConcentrations) {
				var lstBasePairs = new List<short>(dicMatches.Count);
				foreach (var kvp in dicMatches[c]) {
					if (kvp.Value) {
						lstBasePairs.Add((short)kvp.Key);
					}
				}
				bool blnSame;
				var agarLast = default(AgaroseRetention);
				if (lstResults.Count >= 1) {
					agarLast = lstResults[lstResults.Count - 1];
					var f = agarLast.FragmentLengths.Length;
					blnSame = lstBasePairs.Count == f;
					if (blnSame) {
						for (var i = 0; blnSame && i < f; i++) {
							blnSame &= lstBasePairs.Contains(agarLast.FragmentLengths[i]);
						}
					}
				} else {
					blnSame = false;
				}
				if (blnSame) {
					agarLast.UpperMassVolumeConcentration = (float)c;
					lstResults[lstResults.Count - 1] = agarLast;
				} else {
					lstResults.Add(new AgaroseRetention((float)c, lstBasePairs.ToArray()));
				}
			}

			return lstResults.ToArray();
		}

		private static double SelectKey(KeyValuePair<double, Dictionary<ushort, bool>> x)
		{
			return x.Key;
		}

		private static int CountMigratableFragments(KeyValuePair<double, Dictionary<ushort, bool>> x)
		{
			var count = 0;
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
