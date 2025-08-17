//
//  LogisticRegressionResultM.cs
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Repzilon.Libraries.Core.Regression
{
#if DEBUG
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Auto)]
#endif
	public struct DecimalLogisticRegressionResult : ILogisticRegressionResult<decimal>,
	IEquatable<DecimalLogisticRegressionResult>
	{
		public int Count { get; private set; }
		public decimal Amplitude { get; private set; }
		public decimal Intercept { get; private set; }
		public decimal Location { get; private set; }
		public decimal Scale { get; private set; }
		public decimal Correlation { get; private set; }
		public readonly decimal MinX;
		public readonly decimal MaxX;
		public readonly decimal MinY;
		public readonly decimal MaxY;

		public DecimalLogisticRegressionResult(int count, decimal intercept, decimal amplitude, decimal location,
		decimal scale, decimal correlation, decimal minX, decimal maxX, decimal minY, decimal maxY)
		{
			MinX             = minX;
			MaxX             = maxX;
			MinY             = minY;
			MaxY             = maxY;
			this.Count       = count;
			this.Amplitude   = amplitude;
			this.Intercept   = intercept;
			this.Location    = location;
			this.Scale       = scale;
			this.Correlation = correlation;
		}

		internal DecimalLogisticRegressionResult(DecimalLogisticRegressionResult unstretched,
		DecimalLinearRegressionResult secant) : this(unstretched)
		{
			this.Intercept = secant.Intercept + secant.Slope * unstretched.Intercept;
			this.Amplitude = unstretched.Amplitude * secant.Slope;
		}

		#region ICloneable members
		public DecimalLogisticRegressionResult(DecimalLogisticRegressionResult other)
		{
			MinX             = other.MinX;
			MaxX             = other.MaxX;
			MinY             = other.MinY;
			MaxY             = other.MaxY;
			this.Count       = other.Count;
			this.Amplitude   = other.Amplitude;
			this.Intercept   = other.Intercept;
			this.Location    = other.Location;
			this.Scale       = other.Scale;
			this.Correlation = other.Correlation;
		}

#if !NETCOREAPP1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
		object ICloneable.Clone()
		{
			return this.Clone();
		}
#endif

		public DecimalLogisticRegressionResult Clone()
		{
			return new DecimalLogisticRegressionResult(this);
		}
		#endregion

		#region ToString
		public override string ToString()
		{
			return this.ToString(null, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
#if NET35 || NET20
			if (RetroCompat.IsNullOrWhiteSpace(format)) {
#else
			if (String.IsNullOrWhiteSpace(format)) {
#endif
				format = "G";
			}
			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}
			var stbFormula = new StringBuilder();
			stbFormula.Append("y = ").Append(this.Intercept.ToString(format, formatProvider)).Append(" + ")
			 .Append(this.Amplitude.ToString(format, formatProvider)).Append("*[1/(1+e^((x-")
			 .Append(this.Location.ToString(format, formatProvider)).Append(")/-")
			 .Append(this.Scale.ToString(format, formatProvider)).Append("))]");
			return stbFormula.ToString();
		}
		#endregion

		public decimal Determination()
		{
			var r = this.Correlation;
			return r * r;
		}

		public decimal InterpolateY(decimal x)
		{
			var min = this.MinX;
			var max = this.MaxX;
			if ((x < min) || (x > max)) {
				throw new ArgumentOutOfRangeException(nameof(x),
				 String.Format("x is outside the range [{0}; {1}]", min, max));
			}
			var exponent = (x - this.Location) / -this.Scale;
			var unscaled = 1 / (1 + ExtraMath.Exp(exponent));
			return RoundOff.Error(this.Intercept + (this.Amplitude * unscaled));
		}

		#region Equals
		public bool Equals(DecimalLogisticRegressionResult other)
		{
			return MinX == other.MinX && MaxX == other.MaxX && MinY == other.MinY && MaxY == other.MaxY &&
				   this.Count == other.Count && this.Amplitude == other.Amplitude &&
				   this.Intercept == other.Intercept && this.Location == other.Location && this.Scale == other.Scale &&
				   this.Correlation == other.Correlation;
		}

		public override bool Equals(object obj)
		{
			return obj is DecimalLogisticRegressionResult other && this.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked {
#pragma warning disable U2U1000 // Local variable can be inlined or declared const
#pragma warning disable CC0105  // You should use 'var' whenever possible.
				// ReSharper disable once ConvertToConstant.Local
				// ReSharper disable once SuggestVarOrType_BuiltInTypes
				/*const*/ int magic = 397;
#pragma warning restore CC0105  // You should use 'var' whenever possible.
#pragma warning restore U2U1000 // Local variable can be inlined or declared const
				var hashCode = MinX.GetHashCode();
				hashCode = (hashCode * magic) ^ MaxX.GetHashCode();
				hashCode = (hashCode * magic) ^ MinY.GetHashCode();
				hashCode = (hashCode * magic) ^ MaxY.GetHashCode();
				hashCode = (hashCode * magic) ^ this.Count;
				hashCode = (hashCode * magic) ^ this.Amplitude.GetHashCode();
				hashCode = (hashCode * magic) ^ this.Intercept.GetHashCode();
				hashCode = (hashCode * magic) ^ this.Location.GetHashCode();
				hashCode = (hashCode * magic) ^ this.Scale.GetHashCode();
				hashCode = (hashCode * magic) ^ this.Correlation.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(DecimalLogisticRegressionResult left, DecimalLogisticRegressionResult right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DecimalLogisticRegressionResult left, DecimalLogisticRegressionResult right)
		{
			return !left.Equals(right);
		}
		#endregion
	}
}
