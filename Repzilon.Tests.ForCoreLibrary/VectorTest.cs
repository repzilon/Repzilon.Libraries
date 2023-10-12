//
//  Program.cs
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
using Repzilon.Libraries.Core;

namespace Repzilon.Tests.Vector
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			var exa55b_i2 = TwoDVector<short>.Sum(3, 4, 45, AngleUnit.Degree);
			var exa55b_f4 = TwoDVector<float>.Sum(3, 4, 45, AngleUnit.Degree);
			var exa55b_f8 = TwoDVector<double>.Sum(3, 4, 45, AngleUnit.Degree);
			var exa55b_de = TwoDVector<decimal>.Sum(3, 4, 45, AngleUnit.Degree);
			Console.WriteLine("Exemple 55b : Int16={0} Single={1} Double={2} Decimal={3}", exa55b_i2, exa55b_f4, exa55b_f8, exa55b_de);
			var exa55c_f4 = new Angle<float>((float)Math.Asin(4 * new Angle<float>(135, AngleUnit.Degree).Sin() / exa55b_f4), AngleUnit.Radian);
			Console.WriteLine("Exemple 55c : {0:g} or {1:g}", exa55c_f4, exa55c_f4.ConvertTo(AngleUnit.Degree));

			var exa57_u = new TwoDVector<short>(3, 2);
			var exa57_v = new TwoDVector<short>(4, 5);
			var exa57_a = exa57_u + exa57_v;
			var exa57_b = exa57_u - exa57_v;
			var exa57_c = 2 * exa57_u;
			Console.WriteLine("Exemple 57  : u+v={0} u-v={1} 2u={2}", exa57_a, exa57_b, exa57_c);

			var exa58_u = new TwoDVector<short>(1, -2);
			var exa58_v = new TwoDVector<short>(5, 3);
			var exa58_n = ((2 * exa58_u) + exa58_v).Norm();
			Console.WriteLine("Exemple 58  : ||2u+v||={0}", exa58_n);

			var exa60_b = new TwoDVector<short>(-3 - 1, 7 - 1).ToPolar().ConvertTo(AngleUnit.Degree);
			Console.WriteLine("Exemple 60  : DC={0:g}", exa60_b);

			var exa61 = new TwoDVector<short>(2, -2).ToPolar().ConvertTo(AngleUnit.Degree).Cast<float>();
			Console.WriteLine("Exemple 61  : v={0:g}", exa61);

			var exa62 = new PolarVector<float>(3, 210, AngleUnit.Degree).ToCartesian();
			Console.WriteLine("Exemple 62  : v={0}", exa62);

			var exa63_u = new PolarVector<float>(2, 45, AngleUnit.Degree);
			var exa63_v = new PolarVector<float>(4, -30, AngleUnit.Degree);
			var exa63_a = ((Angle<float>)(exa63_u.Angle - exa63_v.Angle));
			var exa63_ng = TwoDVector<float>.Sum(exa63_u.Norm, exa63_v.Norm, exa63_a);
			var exa63_s = exa63_u + exa63_v;
			Console.WriteLine("Exemple 63  : ||R||={0} u+v={1} ||u+v||={2}", exa63_ng, exa63_s, exa63_s.Norm());

			var exa68_u = new PolarVector<float>(9, 35, AngleUnit.Degree);
			var exa68_v = new PolarVector<float>(5, 90 + 20, AngleUnit.Degree);
			var exa68_w = new PolarVector<float>(3, 180 + 50, AngleUnit.Degree);
			var exa68_r = ((exa68_u + exa68_v) + exa68_w).ToPolar().ConvertTo(AngleUnit.Degree);
			Console.WriteLine("Exemple 68  : R={0:g}", exa68_r);

			var exa69_ref = Example69WithDecimal(false, false);
			ShowcaseExample69(exa69_ref, Example69WithSingle);
			ShowcaseExample69(exa69_ref, Example69WithDouble);
			ShowcaseExample69(exa69_ref, Example69WithDecimal);
			ShowcaseExample69(exa69_ref, Example69WithExp);

			Console.ReadLine();
		}

		#region Example 69 implementations
		private static float Example69WithSingle(bool consoleOutput, bool roundErrors)
		{
			const float exa69_q1 = 0.000003f;
			const float exa69_q2 = -0.000002f;
			const float exa69_q3 = 0.000001f;
			var exa69_f13 = ExtraMath.CoulombLab(exa69_q1, exa69_q3, 0.03f);
			var exa69_f23 = ExtraMath.CoulombLab(exa69_q2, exa69_q3, 0.05f);
			if (roundErrors) {
				exa69_f13 = RoundOff.Error(exa69_f13);
				exa69_f23 = RoundOff.Error(exa69_f23);
			}
			var exa69_v13 = new PolarVector<float>(exa69_f13, 90, AngleUnit.Degree).ToCartesian();
			var angle = new Angle<float>(270, AngleUnit.Degree) + new Angle<float>((float)Math.Atan2(4, 3), AngleUnit.Radian);
			var exa69_v23 = new PolarVector<float>(exa69_f23, ((Angle<decimal>)angle).Cast<float>()).ToCartesian();
			if (roundErrors) {
				exa69_v13 = exa69_v13.RoundError();
				exa69_v23 = exa69_v23.RoundError();
			}
			var exa69_r = Convert.ToSingle((exa69_v13 + exa69_v23).Norm());
			Example69Console(consoleOutput, exa69_f13, exa69_f23, exa69_v13, exa69_v23, exa69_r);
			return exa69_r;
		}

		private static double Example69WithDouble(bool consoleOutput, bool roundErrors)
		{
			const double exa69_q1 = 0.000003;
			const double exa69_q2 = -0.000002;
			const double exa69_q3 = 0.000001;
			var exa69_f13 = ExtraMath.CoulombLab(exa69_q1, exa69_q3, 0.03);
			var exa69_f23 = ExtraMath.CoulombLab(exa69_q2, exa69_q3, 0.05);
			if (roundErrors) {
				exa69_f13 = RoundOff.Error(exa69_f13);
				exa69_f23 = RoundOff.Error(exa69_f23);
			}
			TwoDVector<double> exa69_v13, exa69_v23;
			double exa69_r;
			Example69PartCWithDouble(roundErrors, exa69_f13, exa69_f23, out exa69_v13, out exa69_v23, out exa69_r);
			Example69Console(consoleOutput, exa69_f13, exa69_f23, exa69_v13, exa69_v23, exa69_r);
			return exa69_r;
		}

		private static void Example69PartCWithDouble(bool roundErrors, double exa69_f13, double exa69_f23, out TwoDVector<double> exa69_v13, out TwoDVector<double> exa69_v23, out double exa69_r)
		{
			exa69_v13 = new PolarVector<double>(exa69_f13, 90, AngleUnit.Degree).ToCartesian();
			var angle = new Angle<double>(270, AngleUnit.Degree) + new Angle<double>(Math.Atan2(4, 3), AngleUnit.Radian);
			exa69_v23 = new PolarVector<double>(exa69_f23, ((Angle<decimal>)angle).Cast<double>()).ToCartesian();
			if (roundErrors) {
				exa69_v13 = exa69_v13.RoundError();
				exa69_v23 = exa69_v23.RoundError();
			}
			exa69_r = (exa69_v13 + exa69_v23).Norm();
		}

		private static decimal Example69WithDecimal(bool consoleOutput, bool roundErrors)
		{
			const decimal exa69_q1 = 0.000003m;
			const decimal exa69_q2 = -0.000002m;
			const decimal exa69_q3 = 0.000001m;
			var exa69_f13 = ExtraMath.CoulombLab(exa69_q1, exa69_q3, 0.03m);
			var exa69_f23 = ExtraMath.CoulombLab(exa69_q2, exa69_q3, 0.05m);
			var exa69_v13 = new PolarVector<decimal>(exa69_f13, 90, AngleUnit.Degree).ToCartesian();
			var angle = new Angle<decimal>(270, AngleUnit.Degree) + new Angle<decimal>((decimal)Math.Atan2(4, 3), AngleUnit.Radian);
			var exa69_v23 = new PolarVector<decimal>(exa69_f23, (Angle<decimal>)angle).ToCartesian();
			var exa69_vr = exa69_v13 + exa69_v23;
			var exa69_r = ExtraMath.Hypoth(exa69_vr.X, exa69_vr.Y);
			Example69Console(consoleOutput, exa69_f13, exa69_f23, exa69_v13, exa69_v23, (double)exa69_r);
			return exa69_r;
		}

		private static Exp Example69WithExp(bool consoleOutput, bool roundErrors)
		{
			var exa69_q1 = new Exp(3, 10, -6);
			var exa69_q2 = new Exp(-2, 10, -6);
			var exa69_q3 = new Exp(1, 10, -6);
			var exa69_f13 = ExtraMath.CoulombLab(exa69_q1, exa69_q3, new Exp(3, 10, -2));
			var exa69_f23 = ExtraMath.CoulombLab(exa69_q2, exa69_q3, new Exp(5, 10, -2));
			TwoDVector<double> exa69_v13, exa69_v23;
			double exa69_r;
			Example69PartCWithDouble(roundErrors,
			 roundErrors ? RoundOff.Error(exa69_f13.ToDouble()) : exa69_f13.ToDouble(),
			 roundErrors ? RoundOff.Error(exa69_f23.ToDouble()) : exa69_f23.ToDouble(),
			 out exa69_v13, out exa69_v23, out exa69_r);
			Example69Console(consoleOutput, exa69_f13, exa69_f23, exa69_v13, exa69_v23, exa69_r);

			var exponent = Convert.ToSByte(Math.Floor(Math.Log10(exa69_r)));
			var mantissa = (float)(exa69_r * ExtraMath.Pow(10, (sbyte)(-1 * exponent)));
			var result = new Exp(mantissa, 10, exponent);
			return result;
		}
		#endregion

		#region Example 69 showcasing
		private static void ShowcaseExample69<T>(decimal referenceResult, Func<bool, bool, T> implementation)
		{
			Console.Write(Environment.NewLine);
			var ru = implementation(true, false);
			var rr = implementation(true, true);
			var tsUnrounded = BenchExample69(false, implementation);
			var tsRounded = BenchExample69(true, implementation);
			Console.WriteLine("Exemple 69 {0,-7} : {1:f3}s non arrondi Δ {3:e}; {2:f3}s arrondi Δ {4:e}",
			 typeof(T), tsUnrounded.TotalSeconds, tsRounded.TotalSeconds,
			 (InDecimal(ru) - referenceResult), (InDecimal(rr) - referenceResult));
		}

		private static decimal InDecimal(object value)
		{
			return (value is Exp) ? ((Exp)value).ToDecimal() : Convert.ToDecimal(value);
		}

		private static TimeSpan BenchExample69<T>(bool rounding, Func<bool, bool, T> implementation)
		{
			DateTime dtmStart = DateTime.UtcNow;
			for (int i = 0; i < 1000000; i++) {
				implementation(false, rounding);
			}
			return DateTime.UtcNow - dtmStart;
		}

		private static void Example69Console<TC, TV>(bool consoleOutput, TC exa69_f13, TC exa69_f23, TwoDVector<TV> exa69_v13, TwoDVector<TV> exa69_v23, double exa69_r)
		where TV : struct, IConvertible, IFormattable, IEquatable<TV>, IComparable<TV>
		{
			if (consoleOutput) {
				Console.WriteLine("Exemple 69a : ||F13||={0}", exa69_f13);
				Console.WriteLine("Exemple 69b : ||F23||={0}", exa69_f23);
				Console.WriteLine("Exemple 69c : F13={0} F23={1} ||R||={2}", exa69_v13, exa69_v23, exa69_r);
			}
		}
		#endregion
	}
}
