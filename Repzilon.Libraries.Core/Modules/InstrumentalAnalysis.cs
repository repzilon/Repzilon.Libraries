//
//  InstrumentalAnalysis.cs
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
using Repzilon.Libraries.Core.Regression;

namespace Repzilon.Libraries.Core
{
	public enum PeakWidthFrame : byte
	{
		HalfWidthAt606ThousandthsOfHeight = 2,
		Sigma = 2,
		WidthAtHalfHeight = 3,
		Delta = 3,
		WidthOnBaseline = 8,
		W = 8
	}

	public static class InstrumentalAnalysis
	{
		public static float TheoricalPlates(float retentionTime, float width, PeakWidthFrame reference)
		{
			if ((reference == PeakWidthFrame.Delta) || (reference == PeakWidthFrame.W)) {
				return ((reference == PeakWidthFrame.Delta) ? 5.54f : 16f) * retentionTime * retentionTime / (width * width);
			} else if (reference == PeakWidthFrame.Sigma) {
				throw new NotSupportedException(
				 "Counting number of theorical plates using the half-width at 60.6% of height is not supported.");
			} else {
				throw RetroCompat.NewUndefinedEnumException("reference", reference);
			}
		}

		public static float ResolutionBetweenPeaks(float retentionTime1, float retentionTime2,
		float width1, float width2, PeakWidthFrame reference)
		{
			var r2 = reference;
			if ((r2 == PeakWidthFrame.Delta) || (r2 == PeakWidthFrame.W)) {
				return ((r2 == PeakWidthFrame.Delta) ? 1.18f : 2) * (retentionTime2 - retentionTime1) / (width2 + width1);
			} else if (r2 == PeakWidthFrame.Sigma) {
				throw new NotSupportedException(
				 "Computing resolution between peaks using their half-width at 60.6% of height is not supported.");
			} else {
				throw RetroCompat.NewUndefinedEnumException("reference", r2);
			}
		}

		public static RegressionModel<double> ThreePointsDropLine(double absorbance1, double absorbance2,
		double waveLength1, double waveLength2)
		{
			var m = (absorbance2 - absorbance1) / (waveLength2 - waveLength1);
			return new RegressionModel<double>(absorbance2 - (m * waveLength2), m, Math.Sign(m),
			 MathematicalModel.Affine, waveLength1, waveLength2);
		}

		public static RegressionModel<double> ThreePointsDropLine(float absorbance1, float absorbance2,
		short waveLength1, short waveLength2)
		{
			var a2 = RoundOff.UpsizeError(absorbance2);
			double m = (a2 - RoundOff.UpsizeError(absorbance1)) / (waveLength2 - waveLength1);
			return new RegressionModel<double>(a2 - (m * waveLength2), m, Math.Sign(m),
			 MathematicalModel.Affine, waveLength1, waveLength2);
		}
	}
}
