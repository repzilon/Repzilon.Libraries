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
			if ((children == null) || (children.Length < 1)) {
				throw new ArgumentNullException("children");
			}

			var blnInitialMotherVolume = mother.SolutionVolume.HasValue;
			for (int i = 0; i < children.Length; i++) {
				var child = children[i];
				if (child.Concentration.Key != mother.Concentration.Key) {
					throw new ArgumentException("Child solution concentration unit is different from the mother solution.");
				}
				if (blnInitialMotherVolume) {
					if (child.SolutionVolume.Value.Key != mother.SolutionVolume.Value.Key) {
						throw new ArgumentException("Child solution volume unit is different from the mother solution.");
					}
				}

				var childSolutionVolume = child.SolutionVolume.Value.Value;
				child.SoluteVolume = childSolutionVolume * child.Concentration.Value / mother.Concentration.Value;
				child.SolventVolume = childSolutionVolume - child.SoluteVolume;

				if (blnInitialMotherVolume) {
					mother.SolutionVolume = new Measure(mother.SolutionVolume.Value.Key, mother.SolutionVolume.Value.Value - child.SoluteVolume.Value);
				} else if (mother.SolutionVolume.HasValue) {
					mother.SolutionVolume = new Measure(mother.SolutionVolume.Value.Key, mother.SolutionVolume.Value.Value + child.SoluteVolume.Value);
				} else {
					mother.SolutionVolume = new Measure(child.SolutionVolume.Value.Key, child.SoluteVolume.Value);
				}

				children[i] = child;
			}
			return children;
		}

		public static Solution[] Serial(ref Solution mother, params Solution[] children)
		{
			if ((children == null) || (children.Length < 1)) {
				throw new ArgumentNullException("children");
			}

			double volumeFromPrevious = 0;
			for (int i = children.Length - 1; i >= 0; i--) {
				var child = children[i];
				if (child.Concentration.Key != mother.Concentration.Key) {
					throw new ArgumentException("Child solution concentration unit is different from the mother solution.");
				}
				if (i >= 1) {
					if (child.SolutionVolume.Value.Key != children[i - 1].SolutionVolume.Value.Key) {
						throw new ArgumentException("Adjacent child solution volume units are different.");
					}
				}

				var childSolutionVolume = child.SolutionVolume.Value.Value;
				var tempVolume = childSolutionVolume + volumeFromPrevious;
				volumeFromPrevious = child.Concentration.Value * tempVolume /
				 ((i > 0) ? children[i - 1] : mother).Concentration.Value;
				child.SolventVolume = RoundOff.Error(tempVolume - volumeFromPrevious);
				child.SoluteVolume = RoundOff.Error(volumeFromPrevious);

				children[i] = child;
			}
			mother.SolutionVolume = new Measure(children[0].SolutionVolume.Value.Key, children[0].SoluteVolume.Value);
			return children;
		}
	}
}

