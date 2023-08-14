using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using CefNet.CApi;

namespace CefNet
{
	partial class CefDownloadItem
	{
		/// <summary>
		/// Returns a wrapper for the specified pointer to <see cref="cef_download_item_t"/> instance.
		/// </summary>
		/// <param name="create">Represents a method that create a new wrapper.</param>
		/// <param name="instance">The pointer to <see cref="cef_download_item_t"/> object.</param>
		/// <returns>Returns new wrapper of type <see cref="CefDownloadItem"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static CefDownloadItem Wrap(Func<IntPtr, CefDownloadItem> create, cef_download_item_t* instance)
		{
			if (instance == null)
				return null;
			return create(new IntPtr(instance));
		}
	}
}
