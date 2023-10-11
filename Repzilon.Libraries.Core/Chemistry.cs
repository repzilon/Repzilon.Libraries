//
//  Chemistry.cs
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
	public static class Chemistry
	{
		public static float AminoAcidIsoelectric(float pKa1, float pKa2,
		byte cationCount_ph1andhalf, float pKaR)
		{
			if (cationCount_ph1andhalf > 2) {
				throw new ArgumentOutOfRangeException("cationCount_ph1andhalf", cationCount_ph1andhalf,
				 "The lateral chain of a amino acid can only form a cation, a dication or no cation at all under very acidic conditions.");
			} else {
				float sum;
				if (cationCount_ph1andhalf == 2) {
					sum = pKa2 + pKaR;
				} else if ((cationCount_ph1andhalf == 1) && (pKaR < pKa2)) {
					sum = pKa1 + pKaR;
				} else {
					sum = pKa1 + pKa2;
				}
				return RoundOff.Error(0.5f * sum);
			}
		}

		public static float AminoAcidIsoelectric(float pKa1, float pKa2)
		{
			return RoundOff.Error(0.5f * (pKa1 + pKa2));
		}
	}
}
