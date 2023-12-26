//
//  Electricity.cs
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

namespace Repzilon.Libraries.Core
{
	public static class Electricity
	{
		/// <summary>
		/// Computes the force in newtons between two charges in an electric field
		/// </summary>
		/// <param name="qi">first electric charge (in coulombs)</param>
		/// <param name="qj">second electric charge (in coulombs)</param>
		/// <param name="rij">distance between 2 charges (in metres)</param>
		/// <returns>The force between the two charges in newtons.</returns>
		public static double CoulombLab(double qi, double qj, double rij)
		{
			return 9000000000.0 * Math.Abs(qi * qj) / (rij * rij);
		}

		public static decimal CoulombLab(decimal qi, decimal qj, decimal rij)
		{
			return 9000000000 * Math.Abs(qi * qj) / (rij * rij);
		}

		public static float CoulombLab(float qi, float qj, float rij)
		{
			return 9000000000.0f * Math.Abs(qi * qj) / (rij * rij);
		}

		public static Exp CoulombLab(Exp qi, Exp qj, Exp rij)
		{
			return new Exp(9, 10, 9) * ExtraMath.Abs(qi * qj) / (rij * rij);
		}
	}
}
