//
//  FattyAcid.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
using System.Runtime.InteropServices;

namespace Repzilon.Libraries.Core.Biochemistry
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct FattyAcid : IEquatable<FattyAcid>
#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
	, ICloneable
#endif
	{
		public readonly string Name;
		public readonly string Formula;
		public readonly float MolarMass;

		/// <summary>
		/// Melting point in degrees Celsius
		/// </summary>
		public readonly float MeltingPoint;

		public FattyAcid(string name, float meltingPointInCelsius) :
		this(name, meltingPointInCelsius, Single.NaN, null)
		{
		}

		public FattyAcid(string name, float meltingPointInCelsius, string formula) :
		this(name, meltingPointInCelsius, Chemistry.MolarMass(formula), formula)
		{
		}

		private FattyAcid(string name, float meltingPointInCelsius, float molarMass, string formula)
		{
#if NET35 || NET20
			if (RetroCompat.IsNullOrWhiteSpace(name)) {
#else
			if (String.IsNullOrWhiteSpace(name)) {
#endif
				throw new ArgumentNullException(nameof(name));
			}
			if (meltingPointInCelsius < -273.15f) {
				throw new ArgumentOutOfRangeException(nameof(meltingPointInCelsius));
			}

			Name = name.Trim();
			MeltingPoint = meltingPointInCelsius;
			Formula = formula;
			MolarMass = molarMass;
		}

		#region ICloneable members
		public FattyAcid(FattyAcid source)
		{
			this.Name = source.Name;
			this.Formula = source.Formula;
			this.MolarMass = source.MolarMass;
			this.MeltingPoint = source.MeltingPoint;
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif

		public FattyAcid Clone()
		{
			return new FattyAcid(this);
		}
		#endregion

		#region Equals
		public override bool Equals(object obj)
		{
			return obj is FattyAcid && Equals((FattyAcid)obj);
		}

		public bool Equals(FattyAcid other)
		{
			return Name == other.Name &&
				   Formula == other.Formula &&
				   RoundOff.AreEqual(MolarMass, other.MolarMass) &&
				   RoundOff.AreEqual(MeltingPoint, other.MeltingPoint);
		}

		public override int GetHashCode()
		{
			unchecked {
#pragma warning disable CC0105 // You should use 'var' whenever possible.
#pragma warning disable U2U1000
				// ReSharper disable once SuggestVarOrType_BuiltInTypes
				// ReSharper disable once ConvertToConstant.Local
				/*const*/ int magic = -1521134295;
#pragma warning restore U2U1000
#pragma warning restore CC0105 // You should use 'var' whenever possible.
				var hashCode = 1744539508 * -1521134295 + Name.GetHashCode();
				hashCode = hashCode * magic + Formula.GetHashCode();
				hashCode = hashCode * magic + MolarMass.GetHashCode();
				return hashCode * magic + MeltingPoint.GetHashCode();
			}
		}

		public static bool operator ==(FattyAcid left, FattyAcid right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FattyAcid left, FattyAcid right)
		{
			return !(left == right);
		}
		#endregion
	}
}
