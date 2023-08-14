using System.IO;
using System.Threading;

namespace CefNet.Internal
{
	public partial class WebViewGlue
	{
		internal bool AvoidCanDownload() => false;

		/// <summary>
		/// Called before a download begins in response to a user-initiated action
		/// (e.g. alt + link click or link click that returns a `Content-Disposition:
		/// attachment` response from the server).
		/// </summary>
		/// <param name="url">The target download URL.</param>
		/// <param name="requestMethod">The target function (GET, POST, etc)</param>
		/// <returns>Return True to proceed with the download or False to cancel the download.</returns>
		internal protected virtual bool CanDownload(CefBrowser browser, string url, string requestMethod)
		{
			return true;
		}

		internal bool AvoidOnBeforeDownload() => false;

		/// <summary>
		/// Called before a download begins.
		/// </summary>
		/// <param name="browser">The browser.</param>
		/// <param name="suggestedName">The suggested name for the download file.</param>
		/// <param name="downloadItem">Do not keep a reference to <paramref name="downloadItem"/> outside of this function.</param>
		/// <param name="callback">
		/// Execute <paramref name="callback"/> either asynchronously or in this function
		/// to continue the download if desired.
		/// </param>
		internal protected virtual void OnBeforeDownload(CefBrowser browser, CefDownloadItem downloadItem, string suggestedName, CefBeforeDownloadCallback callback)
		{
			var e = new DownloadEventArgs(new CefNetDownloadOperation(downloadItem), suggestedName, callback);
			e.DownloadOperation.SuggestedFileName = suggestedName;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				WebView.RaiseDownload(e);
				if (e.Cancel)
					e.DownloadOperation.Cancel();
				else if (e.ShowDialog.HasValue)
					callback.Continue(e.DownloadPath != suggestedName && Path.IsPathRooted(e.DownloadPath) ? e.DownloadPath : null, e.ShowDialog.Value);
			});
		}

		internal bool AvoidOnDownloadUpdated() => false;

		/// <summary>
		/// Called when a download&apos;s status or progress information has been updated.
		/// </summary>
		/// <param name="downloadItem">
		/// Do not keep a reference to <paramref name="downloadItem"/> outside of this function.
		/// </param>
		/// <param name="callback">
		/// Execute <paramref name="callback"/> either asynchronously or in this function to cancel the
		/// download if desired.
		/// </param>
		/// <remarks>This may be called multiple times before and after <see cref="OnBeforeDownload"/>.</remarks>
		internal protected virtual void OnDownloadUpdated(CefBrowser browser, CefDownloadItem downloadItem, CefDownloadItemCallback callback)
		{
			CefNetDownloadOperation.TryUpdate(downloadItem, callback);
		}
	}
}
