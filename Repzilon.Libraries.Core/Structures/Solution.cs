//
//  Solution.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Measure = System.Collections.Generic.KeyValuePair<string, double>;

namespace Repzilon.Libraries.Core
{
	[StructLayout(LayoutKind.Auto)]
	public struct Solution : IEquatable<Solution>
	{
		public Measure Concentration;
		public Measure? SolutionVolume;
		public double? SolventVolume;
		public double? SoluteVolume;

		#region Equals
		public override bool Equals(object obj)
		{
			return (obj is Solution) && Equals((Solution)obj);
		}

		public bool Equals(Solution other)
		{
			var vent = SolventVolume;
			var ute = SoluteVolume;
			var ovent = other.SolventVolume;
			var oute = other.SoluteVolume;
			return EqualityComparer<Measure>.Default.Equals(Concentration, other.Concentration) &&
				   EqualityComparer<Measure?>.Default.Equals(SolutionVolume, other.SolutionVolume) &&
				   (vent.HasValue == ovent.HasValue) && (vent.Value == ovent.Value) &&
				   (ute.HasValue == oute.HasValue) && (ute.Value == oute.Value);
		}

		public override int GetHashCode()
		{
			unchecked {
				int magic = -1521134295;
				int hashCode = -1047427533 * -1521134295 + Concentration.GetHashCode();
				hashCode = hashCode * magic + SolutionVolume.GetHashCode();
				hashCode = hashCode * magic + SolventVolume.GetHashCode();
				return hashCode * magic + SoluteVolume.GetHashCode();
			}
		}

		public static bool operator ==(Solution left, Solution right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Solution left, Solution right)
		{
			return !(left == right);
		}
		#endregion

		public override string ToString()
		{
			var stbText = new StringBuilder();
			var utionVhv = this.SolutionVolume.HasValue;
			Measure utionVv = default(Measure);
			if (utionVhv) {
				utionVv = this.SolutionVolume.Value;
				AppendMeasure(stbText, utionVv).Append(' ');
			}
			AppendMeasure(stbText.Append("@ "), this.Concentration);
			if (utionVhv) {
				var volumeUnit = utionVv.Key;
				if (!String.IsNullOrEmpty(volumeUnit)) {
					var u = this.SoluteVolume.HasValue;
					var v = this.SolventVolume.HasValue;
					if (u || v) {
						AppendMeasure(stbText.Append(" ("), u, this.SoluteVolume, volumeUnit, " solute");
						if (u && v) {
							stbText.Append(" + ");
						}
						AppendMeasure(stbText, v, this.SolventVolume, volumeUnit, " solvent");
						stbText.Append(')');
					}
				}
			}
			return stbText.ToString();
		}

		private static void AppendMeasure(StringBuilder output, bool hasValue, double? volume, string volumeUnit, string kind)
		{
			if (hasValue) {
				output.Append(volume).Append(' ').Append(volumeUnit).Append(kind);
			}
		}

		private static StringBuilder AppendMeasure(StringBuilder output, Measure measure)
		{
			return output.Append(measure.Value).Append(' ').Append(measure.Key);
		}

		#region Factory methods
		public static Solution Init(double concentration, string unit)
		{
			var sol = new Solution();
			sol.Concentration = InitMeasure(concentration, unit);
			return sol;
		}

		public static Solution Init(double concentration, string concentrationUnit,
		double solutionVolume, string solutionUnit)
		{
			var sol = Init(concentration, concentrationUnit);
			sol.SolutionVolume = InitMeasure(solutionVolume, solutionUnit);
			return sol;
		}

		private static Measure InitMeasure(double value, string unit)
		{
			if (String.IsNullOrEmpty(unit)) {
				throw new ArgumentNullException("unit");
			}
			if (Double.IsNaN(value)) {
				throw new ArgumentNullException("value");
			}
			return new Measure(unit, value);
		}

		public static Solution[] InitMany(double solutionVolume, string solutionUnit, string concentrationUnit,
		params double[] concentrations)
		{
			if ((concentrations == null) || (concentrations.Length < 1)) {
				throw new ArgumentNullException("concentrations");
			}
			var allSolutions = new Solution[concentrations.Length];
			for (int i = 0; i < concentrations.Length; i++) {
				allSolutions[i] = Init(concentrations[i], concentrationUnit, solutionVolume, solutionUnit);
			}
			return allSolutions;
		}
		#endregion
	}
}
