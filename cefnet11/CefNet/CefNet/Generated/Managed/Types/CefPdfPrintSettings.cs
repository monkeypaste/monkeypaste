﻿// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------
// Generated by CefGen
// Source: Generated/Native/Types/cef_pdf_print_settings_t.cs
// --------------------------------------------------------------------------------------------﻿
// DO NOT MODIFY! THIS IS AUTOGENERATED FILE!
// --------------------------------------------------------------------------------------------

#pragma warning disable 0169, 1591, 1573

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using CefNet.WinApi;
using CefNet.CApi;
using CefNet.Internal;

namespace CefNet
{
	/// <summary>
	/// Structure representing PDF print settings.
	/// </summary>
	/// <remarks>
	/// Role: Proxy
	/// </remarks>
	public unsafe partial struct CefPdfPrintSettings : IDisposable
	{
		private cef_pdf_print_settings_t _instance;

		/// <summary>
		/// Page title to display in the header. Only used if |header_footer_enabled|
		/// is set to true (1).
		/// </summary>
		public string HeaderFooterTitle
		{
			get
			{
				fixed (cef_string_t* s = &_instance.header_footer_title)
				{
					return CefString.Read(s);
				}
			}
			set
			{
				fixed (cef_string_t* s = &_instance.header_footer_title)
				{
					CefString.Replace(s, value);
				}
			}
		}

		/// <summary>
		/// URL to display in the footer. Only used if |header_footer_enabled| is set
		/// to true (1).
		/// </summary>
		public string HeaderFooterUrl
		{
			get
			{
				fixed (cef_string_t* s = &_instance.header_footer_url)
				{
					return CefString.Read(s);
				}
			}
			set
			{
				fixed (cef_string_t* s = &_instance.header_footer_url)
				{
					CefString.Replace(s, value);
				}
			}
		}

		/// <summary>
		/// Output page size in microns. If either of these values is less than or
		/// equal to zero then the default paper size (A4) will be used.
		/// </summary>
		public int PageWidth
		{
			get
			{
				return _instance.page_width;
			}
			set
			{
				_instance.page_width = value;
			}
		}

		public int PageHeight
		{
			get
			{
				return _instance.page_height;
			}
			set
			{
				_instance.page_height = value;
			}
		}

		/// <summary>
		/// The percentage to scale the PDF by before printing (e.g. 50 is 50%).
		/// If this value is less than or equal to zero the default value of 100
		/// will be used.
		/// </summary>
		public int ScaleFactor
		{
			get
			{
				return _instance.scale_factor;
			}
			set
			{
				_instance.scale_factor = value;
			}
		}

		/// <summary>
		/// Margins in points. Only used if |margin_type| is set to
		/// PDF_PRINT_MARGIN_CUSTOM.
		/// </summary>
		public int MarginTop
		{
			get
			{
				return _instance.margin_top;
			}
			set
			{
				_instance.margin_top = value;
			}
		}

		public int MarginRight
		{
			get
			{
				return _instance.margin_right;
			}
			set
			{
				_instance.margin_right = value;
			}
		}

		public int MarginBottom
		{
			get
			{
				return _instance.margin_bottom;
			}
			set
			{
				_instance.margin_bottom = value;
			}
		}

		public int MarginLeft
		{
			get
			{
				return _instance.margin_left;
			}
			set
			{
				_instance.margin_left = value;
			}
		}

		/// <summary>
		/// Margin type.
		/// </summary>
		public CefPdfPrintMarginType MarginType
		{
			get
			{
				return _instance.margin_type;
			}
			set
			{
				_instance.margin_type = value;
			}
		}

		/// <summary>
		/// Set to true (1) to print headers and footers or false (0) to not print
		/// headers and footers.
		/// </summary>
		public bool HeaderFooterEnabled
		{
			get
			{
				return _instance.header_footer_enabled != 0;
			}
			set
			{
				_instance.header_footer_enabled = value ? 1 : 0;
			}
		}

		/// <summary>
		/// Set to true (1) to print the selection only or false (0) to print all.
		/// </summary>
		public bool SelectionOnly
		{
			get
			{
				return _instance.selection_only != 0;
			}
			set
			{
				_instance.selection_only = value ? 1 : 0;
			}
		}

		/// <summary>
		/// Set to true (1) for landscape mode or false (0) for portrait mode.
		/// </summary>
		public bool Landscape
		{
			get
			{
				return _instance.landscape != 0;
			}
			set
			{
				_instance.landscape = value ? 1 : 0;
			}
		}

		/// <summary>
		/// Set to true (1) to print background graphics or false (0) to not print
		/// background graphics.
		/// </summary>
		public bool BackgroundsEnabled
		{
			get
			{
				return _instance.backgrounds_enabled != 0;
			}
			set
			{
				_instance.backgrounds_enabled = value ? 1 : 0;
			}
		}

		public void Dispose()
		{
			HeaderFooterTitle = null;
			HeaderFooterUrl = null;
		}

		public static implicit operator CefPdfPrintSettings(cef_pdf_print_settings_t instance)
		{
			return new CefPdfPrintSettings { _instance = instance };
		}

		public static implicit operator cef_pdf_print_settings_t(CefPdfPrintSettings instance)
		{
			return instance._instance;
		}
	}
}