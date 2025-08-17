//
//  LogisticModelTest.cs
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
using Repzilon.Libraries.Core;
using Repzilon.Libraries.Core.Regression;

namespace Repzilon.Tests.ForCoreLibrary
{
	internal static class LogisticModelTest
	{
		internal static void Run(string[] args)
		{
			Program.OutputHeading("Biofermentation semaine 3 exercice");
			LogisticModelSix(new PointM(0, 1.5m), new PointM(5, 2), new PointM(9, 3.5m), new PointM(13, 6.2m),
			 new PointM(16, 8.2m), new PointM(20, 9.4m), new PointM(24, 9.8m), new PointM(28, 9.9m));

			Program.OutputHeading("Biofermentation laboratoire 5 saturation en oxygène");
			LogisticModelSix(new PointM(0, 105.1m), new PointM(0.5m, 103.7m), new PointM(1, 98.4m),
			 new PointM(1.5m, 85.5m), new PointM(2, 51.6m), new PointM(2.5m, 1), new PointM(3, 0.8m),
			 new PointM(3.5m, 0.3m));
		}

		private static void LogisticModelSix(params PointM[] points)
		{
			var karValidCombos = new byte[] { 1, 2, 3, 5, 6, 7 };
			var lorarAll       = new DecimalLogisticRegressionResult[6];
			int i;
			for (i = 0; i < 6; i++) {
				lorarAll[i] = LogisticRegression.Compute((LogisticRegressionOptions)karValidCombos[i], points);
			}
			Console.WriteLine("X\tY\ty m\ty l\ty a\ty ms\ty ls\ty as");
			for (i = 0; i < points.Length; i++) {
				Console.Write("{0}\t{1,5:f1}", points[i].X, points[i].Y);
				for (var j = 0; j < 6; j++) {
					Console.Write("\t{0,6:f2}", lorarAll[j].InterpolateY(points[i].X));
				}
				Console.WriteLine();
			}
			for (i = 0; i < 6; i++) {
				Console.WriteLine("{0,-30}  r={1}",
				 (LogisticRegressionOptions)karValidCombos[i], lorarAll[i].Correlation);
				Console.WriteLine(lorarAll[i]);
			}
		}
	}
}
