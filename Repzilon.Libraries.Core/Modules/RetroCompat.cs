//
//  RetroCompat.cs
//
//  Author:
//       René Rhéaume <repzilon@users.noreply.github.com>
//
// Copyright (C) 2024-2025 René Rhéaume
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL was
// not distributed with this file, You can obtain one at
// https://mozilla.org/MPL/2.0/.
//
using System;
#if NET20 || NET35 || NET40 || NETSTANDARD1_1
using System.Collections;
using System.Collections.Generic;
#endif
#if !(NETSTANDARD1_1 || NETCOREAPP1_0 || NETSTANDARD1_3 || NETSTANDARD1_6)
using System.ComponentModel;
#else
using System.Globalization;
#endif

namespace Repzilon.Libraries.Core
{
	internal static class RetroCompat
	{
#if NET35 || NET20
		internal static bool IsNullOrWhiteSpace(string text)
		{
			return (text == null) || (text.Length < 1) || (text.Trim().Length < 1);
		}
#endif

#if NETSTANDARD1_1
		internal static ArgumentOutOfRangeException NewUndefinedEnumException<T>(string name, T value)
		where T : struct
		{
			return new ArgumentOutOfRangeException(name,
			 "The value " + value.ToString() + " of enumeration named " + name + " of type " + typeof(T) + " is not valid.");
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

#if NETCOREAPP1_0 || NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6
		internal static string ToLower(this string text, CultureInfo culture)
		{
			return CultureInfo.InvariantCulture.Equals(culture) ? text.ToLowerInvariant() : text.ToLower();
		}
#endif

#if NET5_0 || NET6_0
		internal const double Tau = Math.Tau;
#else
		internal const double Tau = 2 * Math.PI;
#endif
	}

#if NET20
	public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

	internal delegate TResult Func<out TResult>();
#endif

#if NET40 || NET35 || NET20 || NETSTANDARD1_1
	public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
	{
		TValue this[TKey key] { get; }

		IEnumerable<TKey> Keys { get; }

		IEnumerable<TValue> Values { get; }

		bool ContainsKey(TKey key);

		bool TryGetValue(TKey key, out TValue value);
	}

	public interface IReadOnlyCollection<T> : IEnumerable<T>
	{
		int Count { get; }
	}

	internal class ReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> m_dicInner;

		public ReadOnlyDictionary(IDictionary<TKey, TValue> toWrap)
		{
			m_dicInner = toWrap ?? throw new ArgumentNullException(nameof(toWrap));
		}

		public TValue this[TKey key] => m_dicInner[key];

		public IEnumerable<TKey> Keys => m_dicInner.Keys;

		public IEnumerable<TValue> Values => m_dicInner.Values;

		public int Count => m_dicInner.Count;

		public bool ContainsKey(TKey key) => m_dicInner.ContainsKey(key);

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => m_dicInner.GetEnumerator();

		public bool TryGetValue(TKey key, out TValue value) => m_dicInner.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
#endif
}
