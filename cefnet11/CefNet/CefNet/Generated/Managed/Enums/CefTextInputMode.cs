﻿// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------
// Generated by CefGen
// Source: include/internal/cef_types.h
// --------------------------------------------------------------------------------------------﻿
// DO NOT MODIFY! THIS IS AUTOGENERATED FILE!
// --------------------------------------------------------------------------------------------

#pragma warning disable 0169, 1591, 1573

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using CefNet.WinApi;

namespace CefNet
{
	/// <summary>
	/// Input mode of a virtual keyboard. These constants match their equivalents
	/// in Chromium&apos;s text_input_mode.h and should not be renumbered.
	/// See https://html.spec.whatwg.org/#input-modalities:-the-inputmode-attribute
	/// </summary>
	public enum CefTextInputMode
	{
		Default = 0,

		None = 1,

		Text = 2,

		Tel = 3,

		Url = 4,

		Email = 5,

		Numeric = 6,

		Decimal = 7,

		Search = 8,

		Max = Search,
	}
}
