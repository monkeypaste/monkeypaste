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
	/// Enumerates the various representations of the ordering of audio channels.
	/// Must be kept synchronized with media::ChannelLayout from Chromium.
	/// See media@base @channel _layout.h
	/// </summary>
	public enum CefChannelLayout
	{
		CEF_CHANNEL_LAYOUT_NONE = 0,

		CEF_CHANNEL_LAYOUT_UNSUPPORTED = 1,

		/// <summary>
		/// Front C
		/// </summary>
		CEF_CHANNEL_LAYOUT_MONO = 2,

		/// <summary>
		/// Front L, Front R
		/// </summary>
		CEF_CHANNEL_LAYOUT_STEREO = 3,

		/// <summary>
		/// Front L, Front R, Back C
		/// </summary>
		CEF_CHANNEL_LAYOUT_2_1 = 4,

		/// <summary>
		/// Front L, Front R, Front C
		/// </summary>
		CEF_CHANNEL_LAYOUT_SURROUND = 5,

		/// <summary>
		/// Front L, Front R, Front C, Back C
		/// </summary>
		CEF_CHANNEL_LAYOUT_4_0 = 6,

		/// <summary>
		/// Front L, Front R, Side L, Side R
		/// </summary>
		CEF_CHANNEL_LAYOUT_2_2 = 7,

		/// <summary>
		/// Front L, Front R, Back L, Back R
		/// </summary>
		CEF_CHANNEL_LAYOUT_QUAD = 8,

		/// <summary>
		/// Front L, Front R, Front C, Side L, Side R
		/// </summary>
		CEF_CHANNEL_LAYOUT_5_0 = 9,

		/// <summary>
		/// Front L, Front R, Front C, LFE, Side L, Side R
		/// </summary>
		CEF_CHANNEL_LAYOUT_5_1 = 10,

		/// <summary>
		/// Front L, Front R, Front C, Back L, Back R
		/// </summary>
		CEF_CHANNEL_LAYOUT_5_0_BACK = 11,

		/// <summary>
		/// Front L, Front R, Front C, LFE, Back L, Back R
		/// </summary>
		CEF_CHANNEL_LAYOUT_5_1_BACK = 12,

		/// <summary>
		/// Front L, Front R, Front C, Side L, Side R, Back L, Back R
		/// </summary>
		CEF_CHANNEL_LAYOUT_7_0 = 13,

		/// <summary>
		/// Front L, Front R, Front C, LFE, Side L, Side R, Back L, Back R
		/// </summary>
		CEF_CHANNEL_LAYOUT_7_1 = 14,

		/// <summary>
		/// Front L, Front R, Front C, LFE, Side L, Side R, Front LofC, Front RofC
		/// </summary>
		CEF_CHANNEL_LAYOUT_7_1_WIDE = 15,

		/// <summary>
		/// Stereo L, Stereo R
		/// </summary>
		CEF_CHANNEL_LAYOUT_STEREO_DOWNMIX = 16,

		/// <summary>
		/// Stereo L, Stereo R, LFE
		/// </summary>
		CEF_CHANNEL_LAYOUT_2POINT1 = 17,

		/// <summary>
		/// Stereo L, Stereo R, Front C, LFE
		/// </summary>
		CEF_CHANNEL_LAYOUT_3_1 = 18,

		/// <summary>
		/// Stereo L, Stereo R, Front C, Rear C, LFE
		/// </summary>
		CEF_CHANNEL_LAYOUT_4_1 = 19,

		/// <summary>
		/// Stereo L, Stereo R, Front C, Side L, Side R, Back C
		/// </summary>
		CEF_CHANNEL_LAYOUT_6_0 = 20,

		/// <summary>
		/// Stereo L, Stereo R, Side L, Side R, Front LofC, Front RofC
		/// </summary>
		CEF_CHANNEL_LAYOUT_6_0_FRONT = 21,

		/// <summary>
		/// Stereo L, Stereo R, Front C, Rear L, Rear R, Rear C
		/// </summary>
		CEF_CHANNEL_LAYOUT_HEXAGONAL = 22,

		/// <summary>
		/// Stereo L, Stereo R, Front C, LFE, Side L, Side R, Rear Center
		/// </summary>
		CEF_CHANNEL_LAYOUT_6_1 = 23,

		/// <summary>
		/// Stereo L, Stereo R, Front C, LFE, Back L, Back R, Rear Center
		/// </summary>
		CEF_CHANNEL_LAYOUT_6_1_BACK = 24,

		/// <summary>
		/// Stereo L, Stereo R, Side L, Side R, Front LofC, Front RofC, LFE
		/// </summary>
		CEF_CHANNEL_LAYOUT_6_1_FRONT = 25,

		/// <summary>
		/// Front L, Front R, Front C, Side L, Side R, Front LofC, Front RofC
		/// </summary>
		CEF_CHANNEL_LAYOUT_7_0_FRONT = 26,

		/// <summary>
		/// Front L, Front R, Front C, LFE, Back L, Back R, Front LofC, Front RofC
		/// </summary>
		CEF_CHANNEL_LAYOUT_7_1_WIDE_BACK = 27,

		/// <summary>
		/// Front L, Front R, Front C, Side L, Side R, Rear L, Back R, Back C.
		/// </summary>
		CEF_CHANNEL_LAYOUT_OCTAGONAL = 28,

		/// <summary>
		/// Channels are not explicitly mapped to speakers.
		/// </summary>
		CEF_CHANNEL_LAYOUT_DISCRETE = 29,

		/// <summary>
		/// Front L, Front R, Front C. Front C contains the keyboard mic audio. This
		/// layout is only intended for input for WebRTC. The Front C channel
		/// is stripped away in the WebRTC audio input pipeline and never seen outside
		/// of that.
		/// </summary>
		CEF_CHANNEL_LAYOUT_STEREO_AND_KEYBOARD_MIC = 30,

		/// <summary>
		/// Front L, Front R, Side L, Side R, LFE
		/// </summary>
		CEF_CHANNEL_LAYOUT_4_1_QUAD_SIDE = 31,

		/// <summary>
		/// Actual channel layout is specified in the bitstream and the actual channel
		/// count is unknown at Chromium media pipeline level (useful for audio
		/// pass-through mode).
		/// </summary>
		CEF_CHANNEL_LAYOUT_BITSTREAM = 32,

		/// <summary>
		/// Front L, Front R, Front C, LFE, Side L, Side R,
		/// Front Height L, Front Height R, Rear Height L, Rear Height R
		/// Will be represented as six channels (5.1) due to eight channel limit
		/// kMaxConcurrentChannels
		/// </summary>
		CEF_CHANNEL_LAYOUT_5_1_4_DOWNMIX = 33,

		/// <summary>
		/// Max value, must always equal the largest entry ever logged.
		/// </summary>
		CEF_CHANNEL_LAYOUT_MAX = CEF_CHANNEL_LAYOUT_5_1_4_DOWNMIX,
	}
}
