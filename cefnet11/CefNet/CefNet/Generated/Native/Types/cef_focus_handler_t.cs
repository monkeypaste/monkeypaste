﻿// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------
// Generated by CefGen
// Source: include/capi/cef_focus_handler_capi.h
// --------------------------------------------------------------------------------------------﻿
// DO NOT MODIFY! THIS IS AUTOGENERATED FILE!
// --------------------------------------------------------------------------------------------

#pragma warning disable 0169, 1591, 1573

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using CefNet.WinApi;

namespace CefNet.CApi
{
	/// <summary>
	/// Implement this structure to handle events related to focus. The functions of
	/// this structure will be called on the UI thread.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe partial struct cef_focus_handler_t
	{
		/// <summary>
		/// Base structure.
		/// </summary>
		public cef_base_ref_counted_t @base;

		/// <summary>
		/// void (*)(_cef_focus_handler_t* self, _cef_browser_t* browser, int next)*
		/// </summary>
		public void* on_take_focus;

		/// <summary>
		/// Called when the browser component is about to loose focus. For instance,
		/// if focus was on the last HTML element and the user pressed the TAB key.
		/// |next| will be true (1) if the browser is giving focus to the next
		/// component and false (0) if the browser is giving focus to the previous
		/// component.
		/// </summary>
		[NativeName("on_take_focus")]
		public unsafe void OnTakeFocus(cef_browser_t* browser, int next)
		{
			fixed (cef_focus_handler_t* self = &this)
			{
				((delegate* unmanaged[Stdcall]<cef_focus_handler_t*, cef_browser_t*, int, void>)on_take_focus)(self, browser, next);
			}
		}

		/// <summary>
		/// int (*)(_cef_focus_handler_t* self, _cef_browser_t* browser, cef_focus_source_t source)*
		/// </summary>
		public void* on_set_focus;

		/// <summary>
		/// Called when the browser component is requesting focus. |source| indicates
		/// where the focus request is originating from. Return false (0) to allow the
		/// focus to be set or true (1) to cancel setting the focus.
		/// </summary>
		[NativeName("on_set_focus")]
		public unsafe int OnSetFocus(cef_browser_t* browser, CefFocusSource source)
		{
			fixed (cef_focus_handler_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_focus_handler_t*, cef_browser_t*, CefFocusSource, int>)on_set_focus)(self, browser, source);
			}
		}

		/// <summary>
		/// void (*)(_cef_focus_handler_t* self, _cef_browser_t* browser)*
		/// </summary>
		public void* on_got_focus;

		/// <summary>
		/// Called when the browser component has received focus.
		/// </summary>
		[NativeName("on_got_focus")]
		public unsafe void OnGotFocus(cef_browser_t* browser)
		{
			fixed (cef_focus_handler_t* self = &this)
			{
				((delegate* unmanaged[Stdcall]<cef_focus_handler_t*, cef_browser_t*, void>)on_got_focus)(self, browser);
			}
		}
	}
}
