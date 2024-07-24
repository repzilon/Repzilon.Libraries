//
//  Dilution.cs
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
using Coefficient = System.Single;
//using Measure = System.Collections.Generic.KeyValuePair<string, float>;

namespace Repzilon.Libraries.Core
{
	public static class Dilution
	{
		public static Solution[] Direct(ref Solution mother, params Solution[] children)
		{
			ValidateLength(children);

			var blnInitialMotherVolume = mother.SolutionVolume.HasValue;
			for (int i = 0; i < children.Length; i++) {
				var child = children[i];
				ValidateUnits(mother, child, blnInitialMotherVolume, mother,
				 "Child solution volume unit is different from the mother solution.");

				var childSolutionVolume = (Coefficient)child.SolutionVolume.Value.Value;
				var solute = (Coefficient)(childSolutionVolume * child.Concentration.Value / mother.Concentration.Value);
				child = Solution.Init(child, childSolutionVolume - solute, solute);

				Coefficient motherVolume;
				if (blnInitialMotherVolume || mother.SolutionVolume.HasValue) {
					var msvv = mother.SolutionVolume.Value.Value;
					motherVolume = blnInitialMotherVolume ? (Coefficient)(msvv - solute) : (Coefficient)(msvv + solute);
				} else {
					motherVolume = solute;
				}
				mother = Solution.Init(mother, motherVolume, child.SolutionVolume.Value.Key);

				children[i] = child;
			}
			return children;
		}

		public static Solution[] Serial(ref Solution mother, params Solution[] children)
		{
			ValidateLength(children);

			Solution child;
			Coefficient volumeFromPrevious = 0;
			for (int i = children.Length - 1; i >= 0; i--) {
				child = children[i];
				var adjacent = i > 0 ? children[i - 1] : mother;
				ValidateUnits(mother, child, i > 0, adjacent,
				 "Adjacent child solution volume units are different.");

				var tempVolume = (Coefficient)(child.SolutionVolume.Value.Value + volumeFromPrevious);
				volumeFromPrevious = (Coefficient)(child.Concentration.Value * tempVolume / adjacent.Concentration.Value);
				children[i] = Solution.Init(child, RoundOff.Error(tempVolume - volumeFromPrevious), RoundOff.Error(volumeFromPrevious));
			}
			child = children[0];
			mother = Solution.Init(mother, child.SoluteVolume.Value, child.SolutionVolume.Value.Key);
			return children;
		}

		private static void ValidateLength(Solution[] children)
		{
			if ((children == null) || (children.Length < 1)) {
				throw new ArgumentNullException("children");
			}
		}

		private static void ValidateUnits(Solution mother, Solution child, bool checkVolume, Solution other, string volumeUnitDifferentMessage)
		{
			if (child.Concentration.Key != mother.Concentration.Key) {
				throw new ArgumentException("Child solution concentration unit is different from the mother solution.");
			}
			if (checkVolume) {
				if (child.SolutionVolume.Value.Key != other.SolutionVolume.Value.Key) {
					throw new ArgumentException(volumeUnitDifferentMessage);
				}
			}
		}
	}
}
