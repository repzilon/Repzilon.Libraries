//
//  EnzymeKinematic.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024-2025 René Rhéaume
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
using Repzilon.Libraries.Core.Regression;

namespace Repzilon.Libraries.Core.Biochemistry
{
	public enum EnzymeSpeedRepresentation : byte
	{
		MichaelisMenten,
		LineweaverBurk,
		EadieHofstee,
		HanesWoolf,
		DirectLinearMedian
	}

#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct EnzymeKinematic<T> : IEquatable<EnzymeKinematic<T>>, IFormattable,
	IComparableEnzymeKinematic, IEquatable<IComparableEnzymeKinematic>
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	where T : struct, IFormattable, IComparable<T>, IEquatable<T>, IComparable
	{
		public readonly KeyValuePair<T, string> Vmax;
		public readonly KeyValuePair<T, string> Km;
		public readonly T Correlation;

		#region Properties
		public EnzymeSpeedRepresentation Representation { get; private set; }

		IComparable IComparableEnzymeKinematic.VmaxNumber
		{
			get { return Vmax.Key; }
		}

		string IComparableEnzymeKinematic.VmaxUnit
		{
			get { return Vmax.Value; }
		}

		IComparable IComparableEnzymeKinematic.KmNumber
		{
			get { return Km.Key; }
		}

		string IComparableEnzymeKinematic.KmUnit
		{
			get { return Km.Value; }
		}

		private IComparable VmaxNumber
		{
			get { return Vmax.Key; }
		}

		private string VmaxUnit
		{
			get { return Vmax.Value; }
		}

		private IComparable KmNumber
		{
			get { return Km.Key; }
		}

		private string KmUnit
		{
			get { return Km.Value; }
		}

		IComparable IComparableEnzymeKinematic.Correlation
		{
			get { return Correlation; }
		}
		#endregion

		public EnzymeKinematic(KeyValuePair<T, string> vmax, KeyValuePair<T, string> km, T correlation,
		EnzymeSpeedRepresentation representation) : this()
		{
			Vmax = vmax;
			Km = km;
			Correlation = correlation;
			Representation = representation;
		}

		public EnzymeKinematic(T vmaxValue, string vmaxUnit, T kmValue, string kmUnit, T correlation,
		EnzymeSpeedRepresentation representation) : this()
		{
			if (String.IsNullOrEmpty(vmaxUnit)) {
				throw new ArgumentNullException("vmaxUnit");
			}
			if (String.IsNullOrEmpty(kmUnit)) {
				throw new ArgumentNullException("kmUnit");
			}
			Vmax = new KeyValuePair<T, string>(vmaxValue, vmaxUnit);
			Km = new KeyValuePair<T, string>(kmValue, kmUnit);
			Correlation = correlation;
			Representation = representation;
		}

		#region ICloneable members
		public EnzymeKinematic(EnzymeKinematic<T> source) : this(source.Vmax, source.Km, source.Correlation,
		source.Representation)
		{
		}

		public EnzymeKinematic<T> Clone()
		{
			return new EnzymeKinematic<T>(this);
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif
		#endregion

		#region Equals
		public bool Equals(EnzymeKinematic<T> other)
		{
			var kvpVmax = this.Vmax;
			var kvpKm   = this.Km;
			return Equals(kvpVmax.Key, other.VmaxNumber) && kvpVmax.Value == other.VmaxUnit &&
			 Equals(kvpKm.Key, other.KmNumber) && kvpKm.Value == other.KmUnit &&
			 EqualityComparer<T>.Default.Equals(Correlation, other.Correlation) &&
			 (other.Representation == Representation);
		}

		private bool Equals(IComparableEnzymeKinematic other)
		{
			if (other == null) {
				return false;
			} else {
				var kvpVmax = this.Vmax;
				var kvpKm = this.Km;
				return Equals(kvpVmax.Key, other.VmaxNumber) && kvpVmax.Value == other.VmaxUnit &&
				 Equals(kvpKm.Key, other.KmNumber) && kvpKm.Value == other.KmUnit &&
				 Equals(this.Correlation, other.Correlation) &&
				 other.Representation == this.Representation;
			}
		}

		bool IEquatable<IComparableEnzymeKinematic>.Equals(IComparableEnzymeKinematic other)
		{
			return this.Equals(other);
		}

		public override bool Equals(object obj)
		{
			return obj is EnzymeKinematic<T> ? Equals((EnzymeKinematic<T>)obj) : Equals(obj as IComparableEnzymeKinematic);
		}

		public override int GetHashCode()
		{
			unchecked {
#pragma warning disable U2U1000
				// ReSharper disable once SuggestVarOrType_BuiltInTypes
				// ReSharper disable once ConvertToConstant.Local
				/*const*/ int magic = -1521134295;
#pragma warning restore U2U1000
				var measure = Vmax;
				var hashCode = (667060969 * -1521134295) + measure.Key.GetHashCode();
				hashCode = (hashCode * magic) + measure.Value.GetHashCode();
				measure = Km;
				hashCode = (hashCode * magic) + measure.Key.GetHashCode();
				hashCode = (hashCode * magic) + measure.Value.GetHashCode();
				hashCode = (hashCode * magic) + Correlation.GetHashCode();
				return (hashCode * magic) + (int)Representation;
			}
		}

		public static bool operator ==(EnzymeKinematic<T> left, EnzymeKinematic<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(EnzymeKinematic<T> left, EnzymeKinematic<T> right)
		{
			return !left.Equals(right);
		}
		#endregion

		public override string ToString()
		{
			var kvpVmax = this.Vmax;
			var kvpKm = this.Km;
			return new StringBuilder(120)
			 .AppendFormat("v<sub>max</sub>: {0} {1}; k<sub>m</sub>: {2} ", kvpVmax.Key, kvpVmax.Value, kvpKm.Key)
			 .AppendFormat("{0} (r={1:f6} with {2})", kvpKm.Value, Correlation, Representation).ToString();
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			var stbAsString = new StringBuilder();
			AppendMeasure(stbAsString.Append("v<sub>max</sub>: "), Vmax, format, formatProvider);
			AppendMeasure(stbAsString.Append("; k<sub>m</sub>: "), Km, format, formatProvider);
			return stbAsString.Append(" (r=").Append(Correlation.ToString(format, formatProvider))
			 .Append(" with ").Append(Representation).Append(')').ToString();
		}

		private static void AppendMeasure(StringBuilder buffer, KeyValuePair<T, string> measure, string format,
		IFormatProvider formatProvider)
		{
			buffer.Append(measure.Key.ToString(format, formatProvider)).Append(' ').Append(measure.Value);
		}
	}

	public static class EnzymeKinematicExtension
	{
		#region Round to precision
#if NET20
		public static EnzymeKinematic<float> RoundedToPrecision(EnzymeKinematic<float> self, byte significantDigits)
#else
		public static EnzymeKinematic<float> RoundedToPrecision(this EnzymeKinematic<float> self, byte significantDigits)
#endif
		{
			return RoundedToPrecision(self, significantDigits, significantDigits);
		}

#if NET20
		public static EnzymeKinematic<float> RoundedToPrecision(EnzymeKinematic<float> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<float> RoundedToPrecision(this EnzymeKinematic<float> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<float>(
			 SignificantDigits.Round(kvpVmax.Key, forSpeed), kvpVmax.Value,
			 SignificantDigits.Round(kvpKm.Key, forConcentration), kvpKm.Value, self.Correlation,
			 self.Representation);
		}

#if NET20
		public static EnzymeKinematic<double> RoundedToPrecision(EnzymeKinematic<double> self, byte significantDigits)
#else
		public static EnzymeKinematic<double> RoundedToPrecision(this EnzymeKinematic<double> self, byte significantDigits)
#endif
		{
			return RoundedToPrecision(self, significantDigits, significantDigits);
		}

#if NET20
		public static EnzymeKinematic<double> RoundedToPrecision(EnzymeKinematic<double> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<double> RoundedToPrecision(this EnzymeKinematic<double> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<double>(
			 SignificantDigits.Round(kvpVmax.Key, forSpeed), kvpVmax.Value,
			 SignificantDigits.Round(kvpKm.Key, forConcentration), kvpKm.Value, self.Correlation,
			 self.Representation);
		}

#if NET20
		public static EnzymeKinematic<decimal> RoundedToPrecision(EnzymeKinematic<decimal> self, byte significantDigits)
#else
		public static EnzymeKinematic<decimal> RoundedToPrecision(this EnzymeKinematic<decimal> self, byte significantDigits)
#endif
		{
			return RoundedToPrecision(self, significantDigits, significantDigits);
		}

#if NET20
		public static EnzymeKinematic<decimal> RoundedToPrecision(EnzymeKinematic<decimal> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<decimal> RoundedToPrecision(this EnzymeKinematic<decimal> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<decimal>(
			 SignificantDigits.Round(kvpVmax.Key, forSpeed), kvpVmax.Value,
			 SignificantDigits.Round(kvpKm.Key, forConcentration), kvpKm.Value, self.Correlation,
			 self.Representation);
		}
		#endregion

		#region Round to decimals
#if NET20
		public static EnzymeKinematic<float> RoundedToDecimals(EnzymeKinematic<float> self, byte decimals)
#else
		public static EnzymeKinematic<float> RoundedToDecimals(this EnzymeKinematic<float> self, byte decimals)
#endif
		{
			return RoundedToDecimals(self, decimals, decimals);
		}

#if NET20
		public static EnzymeKinematic<float> RoundedToDecimals(EnzymeKinematic<float> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<float> RoundedToDecimals(this EnzymeKinematic<float> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<float>(
			 (float)Math.Round(kvpVmax.Key, forSpeed, MidpointRounding.ToEven), kvpVmax.Value,
			 (float)Math.Round(kvpKm.Key, forConcentration, MidpointRounding.ToEven), kvpKm.Value, self.Correlation,
			 self.Representation);
		}

#if NET20
		public static EnzymeKinematic<double> RoundedToDecimals(EnzymeKinematic<double> self, byte decimals)
#else
		public static EnzymeKinematic<double> RoundedToDecimals(this EnzymeKinematic<double> self, byte decimals)
#endif
		{
			return RoundedToDecimals(self, decimals, decimals);
		}

#if NET20
		public static EnzymeKinematic<double> RoundedToDecimals(EnzymeKinematic<double> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<double> RoundedToDecimals(this EnzymeKinematic<double> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<double>(
			 Math.Round(kvpVmax.Key, forSpeed, MidpointRounding.ToEven), kvpVmax.Value,
			 Math.Round(kvpKm.Key, forConcentration, MidpointRounding.ToEven), kvpKm.Value, self.Correlation,
			 self.Representation);
		}

#if NET20
		public static EnzymeKinematic<decimal> RoundedToDecimals(EnzymeKinematic<decimal> self, byte decimals)
#else
		public static EnzymeKinematic<decimal> RoundedToDecimals(this EnzymeKinematic<decimal> self, byte decimals)
#endif
		{
			return RoundedToDecimals(self, decimals, decimals);
		}

#if NET20
		public static EnzymeKinematic<decimal> RoundedToDecimals(EnzymeKinematic<decimal> self, byte forConcentration, byte forSpeed)
#else
		public static EnzymeKinematic<decimal> RoundedToDecimals(this EnzymeKinematic<decimal> self, byte forConcentration, byte forSpeed)
#endif
		{
			var kvpVmax = self.Vmax;
			var kvpKm = self.Km;
			return new EnzymeKinematic<decimal>(
			 Math.Round(kvpVmax.Key, forSpeed, MidpointRounding.ToEven), kvpVmax.Value,
			 Math.Round(kvpKm.Key, forConcentration, MidpointRounding.ToEven), kvpKm.Value, self.Correlation,
			 self.Representation);
		}
		#endregion

		#region Area between curves
		private const double TargetDelta = 1.4e-17;

#if NET20
		public static double AreaBetween(EnzymeKinematic<double> kinematic, RegressionModel<double> michaelisMenten, bool proleptic)
#else
		public static double AreaBetween(this EnzymeKinematic<double> kinematic, RegressionModel<double> michaelisMenten, bool proleptic)
#endif
		{
			var dblLower = proleptic ? 0 : michaelisMenten.MinX;
			var dblUpper = michaelisMenten.MaxX;
			var dblarCrossings = FindKinematicCrossings(kinematic, michaelisMenten, dblLower, dblUpper);
			if (proleptic) {
				dblLower = michaelisMenten.Solve(0);
			}
			return AreaBetween(kinematic, michaelisMenten, dblLower, dblUpper, dblarCrossings);
		}

		private static double AreaBetween(EnzymeKinematic<double> kinematic, RegressionModel<double> michaelisMenten,
		double globalLower, double globalUpper, double[] intersections)
		{
			if (intersections.Length < 1) { // no crossing, should be pretty rare, but I cannot exclude it
				return AreaBetween(kinematic, michaelisMenten, globalLower, globalUpper);
			} else {
				var cm1 = intersections.Length - 1;
				var dblTotalArea = AreaBetween(kinematic, michaelisMenten, globalLower, intersections[0]) +
				 AreaBetween(kinematic, michaelisMenten, intersections[cm1], globalUpper);
				for (var i = 0; i < cm1; i++) {
					dblTotalArea += AreaBetween(kinematic, michaelisMenten, intersections[i], intersections[i + 1]);
				}
				return dblTotalArea;
			}
		}

		private static double AreaBetween(EnzymeKinematic<double> kinematic, RegressionModel<double> michaelisMenten,
		double localLower, double localUpper)
		{
			var dblAreaExp = michaelisMenten.EvaluatePrimitive(localUpper) - michaelisMenten.EvaluatePrimitive(localLower);
			var dblAreaMdl = TheoricalPrimitive(kinematic, localUpper) - TheoricalPrimitive(kinematic, localLower);
			return Math.Abs(dblAreaExp - dblAreaMdl);
		}

		private static double[] FindKinematicCrossings(EnzymeKinematic<double> kinematic,
		RegressionModel<double> michaelisMenten, double lowerBound, double upperBound)
		{
#pragma warning disable CC0105 // You should use 'var' whenever possible.
#pragma warning disable U2U1000
			// ReSharper disable SuggestVarOrType_BuiltInTypes
			// ReSharper disable ConvertToConstant.Local
			/*const*/ double kZero = 0;
			/*const*/ double kHalf = 0.5;
			// ReSharper restore ConvertToConstant.Local
			// ReSharper restore SuggestVarOrType_BuiltInTypes
#pragma warning restore U2U1000
#pragma warning restore CC0105 // You should use 'var' whenever possible.

			if (lowerBound < kZero) {
				throw new ArgumentOutOfRangeException("lowerBound", lowerBound,
				 "A substrate concentration cannot be negative.");
			} else if (lowerBound >= upperBound) {
				throw new ArgumentException("Make sure the lower concentration bound is lower than the upper one.");
			}

			var lstCrossings = new List<double>(3);
			var km = kinematic.Km.Key;
			var kmo7 = 1.07 * km;
			AddKinematicCrossing(lstCrossings, kinematic, michaelisMenten, km, lowerBound, kmo7, lowerBound, upperBound);
			var m2 = RoundOff.AreEqual(lowerBound, 0) ? km : lowerBound;
			AddKinematicCrossing(lstCrossings, kinematic, michaelisMenten, m2 * kHalf, kZero, m2, lowerBound, upperBound);
			AddKinematicCrossing(lstCrossings, kinematic, michaelisMenten, (km + upperBound) * kHalf, kmo7, upperBound, lowerBound, upperBound);
			lstCrossings.Sort();
			return lstCrossings.ToArray();
		}

		private static void AddKinematicCrossing(List<double> destination,
		EnzymeKinematic<double> kinematic, RegressionModel<double> michaelisMenten,
		double candidate, double localMin, double localMax, double globalMin, double globalMax)
		{
			var x = FindNewtonCrossing(kinematic, michaelisMenten, candidate, localMin, localMax);
			if ((!Double.IsNaN(x)) && (x >= globalMin) && (x <= globalMax)) {
				destination.Add(x);
			}
		}

		private static double FindNewtonCrossing(EnzymeKinematic<double> kinematic,
		RegressionModel<double> michaelisMenten, double candidate, double min, double max)
		{
			double fx;
			var k = 1;
#if DEBUG
			double dx;
#endif
			var dblTargetDelta = TargetDelta;
			do {
				fx = ExperimentalMinusTheorical(michaelisMenten, kinematic, candidate);
#if DEBUG
				dx = ExperimentalMinusTheoricalDerivative(michaelisMenten, kinematic, candidate);
				candidate -= fx / dx;
#else
				candidate -= fx / ExperimentalMinusTheoricalDerivative(michaelisMenten, kinematic, candidate);
#endif
				k++;
			} while ((k <= 100) && (candidate > 0) && (Math.Abs(fx) > dblTargetDelta));
			//        ^ candidate is a concentration in practise, and can only be positive
#if DEBUG && !NETSTANDARD1_1
			Console.WriteLine("Newton: différence de {0} après {1} itérations", fx, k);
#endif
			return (candidate >= min && candidate <= max) ? candidate : Double.NaN;
		}

		private static double ExperimentalMinusTheorical(RegressionModel<double> experimental,
		EnzymeKinematic<double> theorical, double x)
		{
			return experimental.Evaluate(x) - Theorical(theorical, x);
		}

		private static double Theorical(EnzymeKinematic<double> theorical, double x)
		{
			return theorical.Vmax.Key * x / (x + theorical.Km.Key);
		}

		private static double ExperimentalMinusTheoricalDerivative(RegressionModel<double> experimental,
		EnzymeKinematic<double> theorical, double x)
		{
			var km = theorical.Km.Key;
			return experimental.EvaluateDerivative(x) - (theorical.Vmax.Key * km / ((x + km) * (x + km)));
		}

		private static double TheoricalPrimitive(EnzymeKinematic<double> theorical, double x)
		{
			var km = theorical.Km.Key;
			return theorical.Vmax.Key * (x - km * Math.Log(Math.Abs(x + km)));
		}
		#endregion
	}
}
