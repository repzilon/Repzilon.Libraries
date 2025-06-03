//
//  MultipleHeadspaceExtraction.cs
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
using System.Collections.Generic;
using Repzilon.Libraries.Core.Regression;

namespace Repzilon.Libraries.Core
{
	public class MultipleHeadspaceExtraction
	{
		private List<PointD> m_lstAreaByVolume = new List<PointD>();
		private bool m_blnRanCalibrate;
		private LinearRegressionResult m_lrdCalibration;

		public MultipleHeadspaceExtraction()
		{
		}

		public void AddLevel(double volume, params float[] areas)
		{
			if (m_blnRanCalibrate) {
				throw new InvalidOperationException("You cannot add calibration levels after calling Calibrate().");
			}
			m_lstAreaByVolume.Add(new PointD(volume, TotalArea(areas)));
		}

		public static double TotalArea(params float[] areas)
		{
			const float kMinCorrelation = -0.9949874f; //-0.99498743710662f; // the slope is normally downwards => negative r
			if ((areas == null) || (areas.Length < 1)) {
				throw new ArgumentNullException("areas");
			}
			int c = areas.Length;
			if (c < 4) {
				throw new ArgumentException("A multiple headspace extraction needs at least four extractions.");
			}
			var lstAreas = new List<PointD>(c);
			int i;
			for (i = 0; i < c; i++) {
				lstAreas.Add(new PointD(i + 1, Math.Log(areas[i])));
			}
			var lrdArea = LinearRegression.Compute(lstAreas);
			if ((float)lrdArea.Correlation > kMinCorrelation) {
				lstAreas.RemoveAt(0);
				lrdArea = LinearRegression.Compute(lstAreas);
				i = 1;
			} else {
				i = 0;
			}
			return (i > 0 ? areas[0] : 0) + (areas[i] / (1.0 - Math.Exp(lrdArea.Slope)));
		}

		public void Calibrate()
		{
			var lstAreasByVolume = m_lstAreaByVolume;
			if ((!m_blnRanCalibrate) && (lstAreasByVolume.Count >= 3)) {
				lstAreasByVolume.Add(new PointD(0, 0));
				m_lrdCalibration = LinearRegression.Compute(lstAreasByVolume);
				m_blnRanCalibrate = true;
			}
		}

		public double InterpolateVolume(params float[] areas)
		{
			if (!m_blnRanCalibrate) {
				this.Calibrate();
			}
			if (!m_blnRanCalibrate) {
				throw new InvalidOperationException("At least three calibration levels are needed before interpolating.");
			}
			return m_lrdCalibration.InterpolateX(TotalArea(areas));
		}

		public override string ToString()
		{
			var lrdCalibration = m_lrdCalibration;
			return m_blnRanCalibrate ?
			 lrdCalibration.ToString() + "\tr = " + lrdCalibration.Correlation :
			 "n = " + m_lstAreaByVolume.Count;
		}
	}
}
