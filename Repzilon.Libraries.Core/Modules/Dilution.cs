//
//  Dilution.cs
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
using Measure = System.Collections.Generic.KeyValuePair<string, double>;

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

				var childSolutionVolume = child.SolutionVolume.Value.Value;
				var solute = childSolutionVolume * child.Concentration.Value / mother.Concentration.Value;
				child.SoluteVolume = solute;
				child.SolventVolume = childSolutionVolume - solute;

				double motherVolume;
				if (blnInitialMotherVolume || mother.SolutionVolume.HasValue) {
					var msvv = mother.SolutionVolume.Value.Value;
					motherVolume = blnInitialMotherVolume ? msvv - solute : msvv + solute;
				} else {
					motherVolume = solute;
				}
				mother.SolutionVolume = new Measure(child.SolutionVolume.Value.Key, motherVolume);

				children[i] = child;
			}
			return children;
		}

		public static Solution[] Serial(ref Solution mother, params Solution[] children)
		{
			ValidateLength(children);

			Solution child;
			double volumeFromPrevious = 0;
			for (int i = children.Length - 1; i >= 0; i--) {
				child = children[i];
				var adjacent = i > 0 ? children[i - 1] : mother;
				ValidateUnits(mother, child, i > 0, adjacent,
				 "Adjacent child solution volume units are different.");

				var tempVolume = child.SolutionVolume.Value.Value + volumeFromPrevious;
				volumeFromPrevious = child.Concentration.Value * tempVolume / adjacent.Concentration.Value;
				child.SolventVolume = RoundOff.Error(tempVolume - volumeFromPrevious);
				child.SoluteVolume = RoundOff.Error(volumeFromPrevious);

				children[i] = child;
			}
			child = children[0];
			mother.SolutionVolume = new Measure(child.SolutionVolume.Value.Key, child.SoluteVolume.Value);
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
