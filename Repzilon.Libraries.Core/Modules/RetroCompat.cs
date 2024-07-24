//
//  RetroCompat.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
#if !(NETSTANDARD1_1 || NETCOREAPP1_0 || NETSTANDARD1_3 || NETSTANDARD1_6)
using System.ComponentModel;
#endif

namespace Repzilon.Libraries.Core
{
	public static class RetroCompat
	{
#if NET35 || NET20
		public static bool IsNullOrWhiteSpace(string text)
		{
			return (text == null) || (text.Length < 1) || (text.Trim().Length < 1);
		}
#endif

#if NETSTANDARD1_1
		internal static ArgumentOutOfRangeException NewUndefinedEnumException<T>(string name, T value)
		where T : struct
		{
			return new ArgumentOutOfRangeException(name,
			 "The value of enumeration named " + name + " of type " + typeof(T) + " is not valid.");
		}
#elif NETCOREAPP1_0 || NETSTANDARD1_3 || NETSTANDARD1_6
		internal static ArgumentOutOfRangeException NewUndefinedEnumException<T>(string name, T value)
		where T : struct, IConvertible
		{
			return new ArgumentOutOfRangeException(name, Convert.ToInt32(value),
			 "The value of enumeration named " + name + " of type " + typeof(T) + " is not valid.");
		}
#else
		internal static InvalidEnumArgumentException NewUndefinedEnumException<T>(string name, T value)
		where T : struct, IConvertible
		{
			return new InvalidEnumArgumentException(name, Convert.ToInt32(value), typeof(T));
		}
#endif
	}

#if NET20
	public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);

	public delegate TResult Func<TResult>();
#endif
}
