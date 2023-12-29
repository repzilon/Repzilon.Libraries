//
//  DilutionTest.cs
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
using Repzilon.Libraries.Core;
using Measure = System.Collections.Generic.KeyValuePair<string, double>;

namespace Repzilon.Tests.ForCoreLibrary
{
	[StructLayout(LayoutKind.Auto)]
	struct Solution : IEquatable<Solution>
	{
		public Measure Concentration;
		public Measure? SolutionVolume;
		public double? SolventVolume;
		public double? SoluteVolume;

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is Solution solution && Equals(solution);
		}

		public bool Equals(Solution other)
		{
			return EqualityComparer<Measure>.Default.Equals(Concentration, other.Concentration) &&
				   EqualityComparer<Measure?>.Default.Equals(SolutionVolume, other.SolutionVolume) &&
				   EqualityComparer<double?>.Default.Equals(SolventVolume, other.SolventVolume) &&
				   EqualityComparer<double?>.Default.Equals(SoluteVolume, other.SoluteVolume);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Concentration, SolutionVolume, SolventVolume, SoluteVolume);
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
			if (this.SolutionVolume.HasValue) {
				AppendMeasure(stbText, this.SolutionVolume.Value).Append(' ');
			}
			AppendMeasure(stbText.Append("@ "), this.Concentration);
			if (this.SolutionVolume.HasValue) {
				var volumeUnit = this.SolutionVolume.Value.Key;
				if (!String.IsNullOrEmpty(volumeUnit)) {
					if (this.SoluteVolume.HasValue || this.SolventVolume.HasValue) {
						stbText.Append(" (");
						if (this.SoluteVolume.HasValue) {
							stbText.Append(this.SoluteVolume.Value).Append(' ').Append(volumeUnit).Append(" solute");
						}
						if (this.SoluteVolume.HasValue && this.SolventVolume.HasValue) {
							stbText.Append(" + ");
						}
						if (this.SolventVolume.HasValue) {
							stbText.Append(this.SolventVolume.Value).Append(' ').Append(volumeUnit).Append(" solvent");
						}
						stbText.Append(')');
					}
				}
			}
			return stbText.ToString();
		}

		private static StringBuilder AppendMeasure(StringBuilder output, Measure measure)
		{
			return output.Append(measure.Value).Append(' ').Append(measure.Key);
		}

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
	}

	static class DilutionTest
	{
		private static Solution[] DirectDilution(ref Solution mother, params Solution[] children)
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

		private static Solution[] SerialDilution(ref Solution mother, params Solution[] children)
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

		internal static void Run(string[] args)
		{
			var exa17_m = Solution.Init(20, "%", 50, "mL");
			var exa17_f = Solution.InitMany(200, "mL", "%", 2, 1);
			var exa17_out = DirectDilution(ref exa17_m, exa17_f);
			OutputDirectDilution("Exemple 17 :", exa17_out);

			var exa18_m = Solution.Init(5, "g/L");
			var exa18_s1 = Solution.Init(2, "g/L", 0.15, "L");
			var exa18_s2 = Solution.Init(0.5, "g/L", 0.10, "L");
			var exa18_out = SerialDilution(ref exa18_m, exa18_s1, exa18_s2);
			OutputSerialDilution("Exemple 18 :", exa18_out);

			var exer1_12_m = Solution.Init(20, "mol/L");
			var exer1_12_f = Solution.InitMany(15, "mL", "mol/L", 8, 5);
			var exer1_12a_out = DirectDilution(ref exer1_12_m, exer1_12_f);
			OutputDirectDilution("Exercices 1 #12 a) :", exer1_12a_out);
			var exer1_12b_out = SerialDilution(ref exer1_12_m, exer1_12_f);
			OutputSerialDilution("Exercices 1 #12 b) :", exer1_12b_out);

			var exer1_13_m = Solution.Init(30, "%");
			var exer1_13_f = Solution.InitMany(0.5, "L", "%", 7, 5, 2);
			var exer1_13_out = SerialDilution(ref exer1_13_m, exer1_13_f);
			OutputSerialDilution("Exercices 1 #13 :", exer1_13_out);

			var exer1_14_m = Solution.Init(1, "Cm");
			var exer1_14_f = Solution.InitMany(10, "µL", "Cm", 0.25, 0.2, 0.1);
			var exer1_14_out = DirectDilution(ref exer1_14_m, exer1_14_f);
			OutputDirectDilution("Exercices 1 #14 ", exer1_14_out);

			var exer1_15_m = Solution.Init(1, "Cm");
			var exer1_15_f = Solution.InitMany(150, "mL", "Cm", 0.5, 0.2, 0.05, 0.02);
			var exer1_15_out = SerialDilution(ref exer1_15_m, exer1_15_f);
			OutputSerialDilution("Exercices 1 #15 ", exer1_15_out);
		}

		private static void OutputDirectDilution(string title, params Solution[] children)
		{
			Console.WriteLine(title);
			for (int i = 0; i < children.Length; i++) {
				var child = children[i];
				Console.WriteLine("Pour la solution fille {0}, diluer {1} {2} de solution mère dans {3} {2} de solvant.",
				 i + 1, child.SoluteVolume.Value, child.SolutionVolume.Value.Key, child.SolventVolume.Value);
			}
		}

		private static void OutputSerialDilution(string title, params Solution[] children)
		{
			Console.WriteLine(title);
			for (int i = 0; i < children.Length; i++) {
				var child = children[i];
				Console.WriteLine("Pour la solution fille {0}, prélever {1} {2} de la solution {3} pour la diluer dans {4} {2} de solvant.",
				 i + 1, child.SoluteVolume.Value, child.SolutionVolume.Value.Key,
				 (i < 1) ? "mère" : "fille " + i, child.SolventVolume.Value);
			}
		}
	}
}
