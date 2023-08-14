using System;
using System.ComponentModel;
using System.IO;

namespace CefNet
{
	/// <summary>
	/// Provides data for the <see cref="IChromiumWebView.Download"/> event.
	/// </summary>
	public sealed class DownloadEventArgs : CancelEventArgs
	{
		private readonly CefBeforeDownloadCallback _callback;

		/// <summary>
		/// Initializes a new instance of the <see cref="DownloadEventArgs"/> class.
		/// </summary>
		/// <param name="downloadOperation">The download operation.</param>
		/// <param name="suggestedName">The suggested name for the download file.</param>
		internal unsafe DownloadEventArgs(CefNetDownloadOperation downloadOperation, string suggestedName, CefBeforeDownloadCallback callback)
		{
			_callback = callback;
			this.ShowDialog = true;
			this.DownloadPath = suggestedName;
			this.DownloadOperation = downloadOperation;
		}

		public CefNetDownloadOperation DownloadOperation { get; }

		public string DownloadPath { get; private set; }

		internal bool? ShowDialog { get; private set; }

		public void Save(string path)
		{
			if (path is null)
				throw new ArgumentNullException(nameof(path));
			if (!Path.IsPathRooted(path) || Directory.Exists(path))
				throw new ArgumentException("Expected the full file path for the download including the file name.", nameof(path));
			this.ShowDialog = false;
			this.DownloadPath = path;
			this.Cancel = false;
		}

		public CefBeforeDownloadCallback GetDeferral()
		{
			this.ShowDialog = null;
			this.Cancel = false;
			return _callback;
		}
	}
}
