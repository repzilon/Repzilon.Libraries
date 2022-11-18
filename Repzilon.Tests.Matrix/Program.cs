//
//  Program.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2022 René Rhéaume
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.Matrix
{
	public static class Program
	{
		static void Main()
		{
			var ex80_a = new Matrix<short>(3, 3, 2, -1, 5, -3, 4, 7, 1, -1, 0);
			var ex80_b = new Matrix<short>(3, 3, 1, 4, 0, -3, 6, -5, 1, 0, -1);
			var ex80_result = (3 * ex80_a) + ex80_b;
			Console.WriteLine(ex80_a);
			Console.WriteLine(ex80_b);
			Console.WriteLine(ex80_result);

			var ex81_a = new Matrix<short>(2, 2, 1, 3, -2, 1);
			var ex81_b = new Matrix<short>(2, 3, 1, 2, 1, -3, 4, -5);
			try {
				var ex81_ab = ex81_a * ex81_b;
				Console.WriteLine(ex81_ab);
				var ex81_ba = ex81_b * ex81_a;
				Console.WriteLine(ex81_ba);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.ToString());
			}

			var pap_m = new Matrix<short>(3, 3, 4, 3, 5, -3, -7, 1, 8, 0, 0);
			var pap_n = new Matrix<short>(3, 3, 4, 5, 1, 0, 1, -4, 6, 2, -1);
			var pap_a = pap_m + pap_n;
			var pap_b = (2 * pap_n) - (5 * pap_m);
			var pap_c = pap_m * pap_n;
			var pap_d = pap_n * pap_m;

			Console.WriteLine(pap_a);
			Console.WriteLine(pap_b);
			Console.WriteLine(pap_c);
			Console.WriteLine(pap_d);

			Console.WriteLine("Press Enter to exit...");
			Console.ReadLine();
		}
	}
}

