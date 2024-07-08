//
//  Solution.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Coefficient = System.Single;
using Measure = System.Collections.Generic.KeyValuePair<string, float>;

namespace Repzilon.Libraries.Core
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct Solution : IEquatable<Solution>
	{
		public readonly Measure Concentration;
		public readonly Measure? SolutionVolume;
		public readonly Coefficient? SolventVolume;
		public readonly Coefficient? SoluteVolume;

		private Solution(Measure concentration, Measure? solutionVolume, Coefficient? solventVolume, Coefficient? soluteVolume)
		{
			Concentration = concentration;
			SolutionVolume = solutionVolume;
			SolventVolume = solventVolume;
			SoluteVolume = soluteVolume;
		}

		#region Equals
		public override bool Equals(object obj)
		{
			return (obj is Solution) && Equals((Solution)obj);
		}

		public bool Equals(Solution other)
		{
			return EqualityComparer<Measure>.Default.Equals(Concentration, other.Concentration) &&
				   EqualityComparer<Measure?>.Default.Equals(SolutionVolume, other.SolutionVolume) &&
				   MatrixExtensionMethods.Equals(SolventVolume, other.SolventVolume) &&
				   MatrixExtensionMethods.Equals(SoluteVolume, other.SoluteVolume);
		}

		public override int GetHashCode()
		{
			unchecked {
				var magic = -1521134295;
				var hashCode = (-1047427533 * -1521134295) + Concentration.GetHashCode();
				hashCode = (hashCode * magic) + SolutionVolume.GetHashCode();
				hashCode = (hashCode * magic) + SolventVolume.GetHashCode();
				return (hashCode * magic) + SoluteVolume.GetHashCode();
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
			var utionVv = default(Measure);
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

		private static void AppendMeasure(StringBuilder output, bool hasValue, Coefficient? volume, string volumeUnit, string kind)
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
		public static Solution Init(Coefficient concentration, string unit)
		{
			return new Solution(InitMeasure(concentration, unit), null, null, null);
		}

		public static Solution Init(Coefficient concentration, string concentrationUnit,
		Coefficient solutionVolume, string solutionUnit)
		{
			return new Solution(InitMeasure(concentration, concentrationUnit),
			 InitMeasure(solutionVolume, solutionUnit), null, null);
		}

		private static Measure InitMeasure(Coefficient value, string unit)
		{
			if (String.IsNullOrEmpty(unit)) {
				throw new ArgumentNullException("unit");
			}
			if (Coefficient.IsNaN(value)) {
				throw new ArgumentNullException("value");
			}
			return new Measure(unit, value);
		}

		public static Solution[] InitMany(Coefficient solutionVolume, string solutionUnit, string concentrationUnit,
		params Coefficient[] concentrations)
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

		public static Solution Init(Solution original, Coefficient solventVolume, Coefficient soluteVolume)
		{
			return new Solution(original.Concentration, original.SolutionVolume, solventVolume, soluteVolume);
		}

		public static Solution Init(Solution original, Coefficient solutionVolume, string solutionUnit)
		{
			return new Solution(original.Concentration, InitMeasure(solutionVolume, solutionUnit),
			 original.SolventVolume, original.SoluteVolume);
		}
		#endregion
	}
}
