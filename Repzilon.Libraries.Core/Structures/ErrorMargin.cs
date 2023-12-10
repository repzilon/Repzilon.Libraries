//
//  ErrorMargin.cs
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
using System.Text;

namespace Repzilon.Libraries.Core
{
	public struct ErrorMargin<T> where T: struct, IFormattable
#if (!NETSTANDARD1_1)
	, IConvertible
#endif
	{
		public T Middle;
		public T Margin;

		public ErrorMargin(T middle, T margin)
		{
			Middle = middle;
			Margin = margin;
		}

		public T Min()
		{
			return Matrix<T>.sub(Middle, Margin);
		}

		public T Max()
		{
			return Matrix<T>.add(Middle, Margin);
		}

		public override string ToString()
		{
			var stbInterval = new StringBuilder();
			stbInterval.Append(this.Middle).Append(" ± ").Append(this.Margin).Append(" -> [").Append(this.Min()).Append("; ").Append(this.Max()).Append(']');
			return stbInterval.ToString();
		}
	}
}
