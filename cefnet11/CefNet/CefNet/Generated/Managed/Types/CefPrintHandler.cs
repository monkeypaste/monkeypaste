﻿// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------
// Generated by CefGen
// Source: Generated/Native/Types/cef_print_handler_t.cs
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
	/// Implement this structure to handle printing on Linux. Each browser will have
	/// only one print job in progress at a time. The functions of this structure
	/// will be called on the browser process UI thread.
	/// </summary>
	/// <remarks>
	/// Role: Handler
	/// </remarks>
	public unsafe partial class CefPrintHandler : CefBaseRefCounted<cef_print_handler_t>, ICefPrintHandlerPrivate
	{
#if NET_LESS_5_0
		private static readonly OnPrintStartDelegate fnOnPrintStart = OnPrintStartImpl;

		private static readonly OnPrintSettingsDelegate fnOnPrintSettings = OnPrintSettingsImpl;

		private static readonly OnPrintDialogDelegate fnOnPrintDialog = OnPrintDialogImpl;

		private static readonly OnPrintJobDelegate fnOnPrintJob = OnPrintJobImpl;

		private static readonly OnPrintResetDelegate fnOnPrintReset = OnPrintResetImpl;

		private static readonly GetPdfPaperSizeDelegate fnGetPdfPaperSize = GetPdfPaperSizeImpl;

#endif // NET_LESS_5_0
		internal static unsafe CefPrintHandler Create(IntPtr instance)
		{
			return new CefPrintHandler((cef_print_handler_t*)instance);
		}

		public CefPrintHandler()
		{
			cef_print_handler_t* self = this.NativeInstance;
			#if NET_LESS_5_0
			self->on_print_start = (void*)Marshal.GetFunctionPointerForDelegate(fnOnPrintStart);
			self->on_print_settings = (void*)Marshal.GetFunctionPointerForDelegate(fnOnPrintSettings);
			self->on_print_dialog = (void*)Marshal.GetFunctionPointerForDelegate(fnOnPrintDialog);
			self->on_print_job = (void*)Marshal.GetFunctionPointerForDelegate(fnOnPrintJob);
			self->on_print_reset = (void*)Marshal.GetFunctionPointerForDelegate(fnOnPrintReset);
			self->get_pdf_paper_size = (void*)Marshal.GetFunctionPointerForDelegate(fnGetPdfPaperSize);
			#else
			self->on_print_start = (delegate* unmanaged[Stdcall]<cef_print_handler_t*, cef_browser_t*, void>)&OnPrintStartImpl;
			self->on_print_settings = (delegate* unmanaged[Stdcall]<cef_print_handler_t*, cef_browser_t*, cef_print_settings_t*, int, void>)&OnPrintSettingsImpl;
			self->on_print_dialog = (delegate* unmanaged[Stdcall]<cef_print_handler_t*, cef_browser_t*, int, cef_print_dialog_callback_t*, int>)&OnPrintDialogImpl;
			self->on_print_job = (delegate* unmanaged[Stdcall]<cef_print_handler_t*, cef_browser_t*, cef_string_t*, cef_string_t*, cef_print_job_callback_t*, int>)&OnPrintJobImpl;
			self->on_print_reset = (delegate* unmanaged[Stdcall]<cef_print_handler_t*, cef_browser_t*, void>)&OnPrintResetImpl;
			self->get_pdf_paper_size = (delegate* unmanaged[Stdcall]<cef_print_handler_t*, cef_browser_t*, int, cef_size_t>)&GetPdfPaperSizeImpl;
			#endif
		}

		public CefPrintHandler(cef_print_handler_t* instance)
			: base((cef_base_ref_counted_t*)instance)
		{
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		extern bool ICefPrintHandlerPrivate.AvoidOnPrintStart();

		/// <summary>
		/// Called when printing has started for the specified |browser|. This
		/// function will be called before the other OnPrint*() functions and
		/// irrespective of how printing was initiated (e.g.
		/// cef_browser_host_t::print(), JavaScript window.print() or PDF extension
		/// print button).
		/// </summary>
		protected internal unsafe virtual void OnPrintStart(CefBrowser browser)
		{
		}

#if NET_LESS_5_0
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private unsafe delegate void OnPrintStartDelegate(cef_print_handler_t* self, cef_browser_t* browser);

#endif // NET_LESS_5_0
		// void (*)(_cef_print_handler_t* self, _cef_browser_t* browser)*
#if !NET_LESS_5_0
		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
#endif
		private static unsafe void OnPrintStartImpl(cef_print_handler_t* self, cef_browser_t* browser)
		{
			var instance = GetInstance((IntPtr)self) as CefPrintHandler;
			if (instance == null || ((ICefPrintHandlerPrivate)instance).AvoidOnPrintStart())
			{
				ReleaseIfNonNull((cef_base_ref_counted_t*)browser);
				return;
			}
			instance.OnPrintStart(CefBrowser.Wrap(CefBrowser.Create, browser));
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		extern bool ICefPrintHandlerPrivate.AvoidOnPrintSettings();

		/// <summary>
		/// Synchronize |settings| with client state. If |get_defaults| is true (1)
		/// then populate |settings| with the default print settings. Do not keep a
		/// reference to |settings| outside of this callback.
		/// </summary>
		protected internal unsafe virtual void OnPrintSettings(CefBrowser browser, CefPrintSettings settings, bool getDefaults)
		{
		}

#if NET_LESS_5_0
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private unsafe delegate void OnPrintSettingsDelegate(cef_print_handler_t* self, cef_browser_t* browser, cef_print_settings_t* settings, int get_defaults);

#endif // NET_LESS_5_0
		// void (*)(_cef_print_handler_t* self, _cef_browser_t* browser, _cef_print_settings_t* settings, int get_defaults)*
#if !NET_LESS_5_0
		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
#endif
		private static unsafe void OnPrintSettingsImpl(cef_print_handler_t* self, cef_browser_t* browser, cef_print_settings_t* settings, int get_defaults)
		{
			var instance = GetInstance((IntPtr)self) as CefPrintHandler;
			if (instance == null || ((ICefPrintHandlerPrivate)instance).AvoidOnPrintSettings())
			{
				ReleaseIfNonNull((cef_base_ref_counted_t*)browser);
				ReleaseIfNonNull((cef_base_ref_counted_t*)settings);
				return;
			}
			instance.OnPrintSettings(CefBrowser.Wrap(CefBrowser.Create, browser), CefPrintSettings.Wrap(CefPrintSettings.Create, settings), get_defaults != 0);
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		extern bool ICefPrintHandlerPrivate.AvoidOnPrintDialog();

		/// <summary>
		/// Show the print dialog. Execute |callback| once the dialog is dismissed.
		/// Return true (1) if the dialog will be displayed or false (0) to cancel the
		/// printing immediately.
		/// </summary>
		protected internal unsafe virtual bool OnPrintDialog(CefBrowser browser, bool hasSelection, CefPrintDialogCallback callback)
		{
			return default;
		}

#if NET_LESS_5_0
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private unsafe delegate int OnPrintDialogDelegate(cef_print_handler_t* self, cef_browser_t* browser, int has_selection, cef_print_dialog_callback_t* callback);

#endif // NET_LESS_5_0
		// int (*)(_cef_print_handler_t* self, _cef_browser_t* browser, int has_selection, _cef_print_dialog_callback_t* callback)*
#if !NET_LESS_5_0
		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
#endif
		private static unsafe int OnPrintDialogImpl(cef_print_handler_t* self, cef_browser_t* browser, int has_selection, cef_print_dialog_callback_t* callback)
		{
			var instance = GetInstance((IntPtr)self) as CefPrintHandler;
			if (instance == null || ((ICefPrintHandlerPrivate)instance).AvoidOnPrintDialog())
			{
				ReleaseIfNonNull((cef_base_ref_counted_t*)browser);
				ReleaseIfNonNull((cef_base_ref_counted_t*)callback);
				return default;
			}
			return instance.OnPrintDialog(CefBrowser.Wrap(CefBrowser.Create, browser), has_selection != 0, CefPrintDialogCallback.Wrap(CefPrintDialogCallback.Create, callback)) ? 1 : 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		extern bool ICefPrintHandlerPrivate.AvoidOnPrintJob();

		/// <summary>
		/// Send the print job to the printer. Execute |callback| once the job is
		/// completed. Return true (1) if the job will proceed or false (0) to cancel
		/// the job immediately.
		/// </summary>
		protected internal unsafe virtual bool OnPrintJob(CefBrowser browser, string documentName, string pdfFilePath, CefPrintJobCallback callback)
		{
			return default;
		}

#if NET_LESS_5_0
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private unsafe delegate int OnPrintJobDelegate(cef_print_handler_t* self, cef_browser_t* browser, cef_string_t* document_name, cef_string_t* pdf_file_path, cef_print_job_callback_t* callback);

#endif // NET_LESS_5_0
		// int (*)(_cef_print_handler_t* self, _cef_browser_t* browser, const cef_string_t* document_name, const cef_string_t* pdf_file_path, _cef_print_job_callback_t* callback)*
#if !NET_LESS_5_0
		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
#endif
		private static unsafe int OnPrintJobImpl(cef_print_handler_t* self, cef_browser_t* browser, cef_string_t* document_name, cef_string_t* pdf_file_path, cef_print_job_callback_t* callback)
		{
			var instance = GetInstance((IntPtr)self) as CefPrintHandler;
			if (instance == null || ((ICefPrintHandlerPrivate)instance).AvoidOnPrintJob())
			{
				ReleaseIfNonNull((cef_base_ref_counted_t*)browser);
				ReleaseIfNonNull((cef_base_ref_counted_t*)callback);
				return default;
			}
			return instance.OnPrintJob(CefBrowser.Wrap(CefBrowser.Create, browser), CefString.Read(document_name), CefString.Read(pdf_file_path), CefPrintJobCallback.Wrap(CefPrintJobCallback.Create, callback)) ? 1 : 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		extern bool ICefPrintHandlerPrivate.AvoidOnPrintReset();

		/// <summary>
		/// Reset client state related to printing.
		/// </summary>
		protected internal unsafe virtual void OnPrintReset(CefBrowser browser)
		{
		}

#if NET_LESS_5_0
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private unsafe delegate void OnPrintResetDelegate(cef_print_handler_t* self, cef_browser_t* browser);

#endif // NET_LESS_5_0
		// void (*)(_cef_print_handler_t* self, _cef_browser_t* browser)*
#if !NET_LESS_5_0
		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
#endif
		private static unsafe void OnPrintResetImpl(cef_print_handler_t* self, cef_browser_t* browser)
		{
			var instance = GetInstance((IntPtr)self) as CefPrintHandler;
			if (instance == null || ((ICefPrintHandlerPrivate)instance).AvoidOnPrintReset())
			{
				ReleaseIfNonNull((cef_base_ref_counted_t*)browser);
				return;
			}
			instance.OnPrintReset(CefBrowser.Wrap(CefBrowser.Create, browser));
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		extern bool ICefPrintHandlerPrivate.AvoidGetPdfPaperSize();

		/// <summary>
		/// Return the PDF paper size in device units. Used in combination with
		/// cef_browser_host_t::print_to_pdf().
		/// </summary>
		protected internal unsafe virtual CefSize GetPdfPaperSize(CefBrowser browser, int deviceUnitsPerInch)
		{
			return default;
		}

#if NET_LESS_5_0
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private unsafe delegate cef_size_t GetPdfPaperSizeDelegate(cef_print_handler_t* self, cef_browser_t* browser, int device_units_per_inch);

#endif // NET_LESS_5_0
		// cef_size_t (*)(_cef_print_handler_t* self, _cef_browser_t* browser, int device_units_per_inch)*
#if !NET_LESS_5_0
		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
#endif
		private static unsafe cef_size_t GetPdfPaperSizeImpl(cef_print_handler_t* self, cef_browser_t* browser, int device_units_per_inch)
		{
			var instance = GetInstance((IntPtr)self) as CefPrintHandler;
			if (instance == null || ((ICefPrintHandlerPrivate)instance).AvoidGetPdfPaperSize())
			{
				ReleaseIfNonNull((cef_base_ref_counted_t*)browser);
				return default;
			}
			return instance.GetPdfPaperSize(CefBrowser.Wrap(CefBrowser.Create, browser), device_units_per_inch);
		}
	}
}