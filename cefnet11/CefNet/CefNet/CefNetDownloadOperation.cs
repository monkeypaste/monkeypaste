using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefNet
{
	public sealed class CefNetDownloadOperation
	{
		private static readonly Dictionary<int, WeakReference<CefNetDownloadOperation>> _Downloads = new Dictionary<int, WeakReference<CefNetDownloadOperation>>(); 
		private CefDownloadItemCallback _callback;
		private TaskCompletionSource<bool> _completion;

		internal CefNetDownloadOperation(CefDownloadItem downloadItem)
		{
			_completion = new TaskCompletionSource<bool>((TaskCreationOptions)64); // TaskCreationOptions.RunContinuationsAsynchronously
			this.Id = (int)downloadItem.Id;
			this.StartTime = downloadItem.StartTime;
			//this.FullPath = downloadItem.FullPath;
			this.Url = downloadItem.Url;
			this.OriginalUrl = downloadItem.OriginalUrl;
			this.SuggestedFileName = downloadItem.SuggestedFileName;
			this.ContentDisposition = downloadItem.ContentDisposition;
			this.MimeType = downloadItem.MimeType;

			this.UpdateInternal(downloadItem, null);
			lock (_Downloads)
			{
				_Downloads[this.Id] = new WeakReference<CefNetDownloadOperation>(this);
			}
		}

		~CefNetDownloadOperation()
		{
			lock (_Downloads)
			{
				_Downloads.Remove(this.Id);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the download is in progress.
		/// </summary>
		public bool IsInProgress { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the download is complete.
		/// </summary>
		public bool IsComplete { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the download has been canceled or interrupted.
		/// </summary>
		public bool IsCanceled { get; private set; }

		/// <summary>
		/// Gets a simple speed estimate in bytes/s.
		/// </summary>
		public long CurrentSpeed { get; private set; }

		/// <summary>
		/// Gets the rough percent complete or -1 if the receive total size is
		/// unknown.
		/// </summary>
		public int PercentComplete { get; private set; }

		/// <summary>
		/// Gets the total number of bytes.
		/// </summary>
		public long TotalBytes { get; private set; }

		/// <summary>
		/// Gets the number of received bytes.
		/// </summary>
		public long ReceivedBytes { get; private set; }

		/// <summary>
		/// Gets the time that the download started.
		/// </summary>
		public DateTime StartTime { get; private set; }

		/// <summary>
		/// Gets the time that the download ended.
		/// </summary>
		public DateTime? EndTime { get; private set; }

		/// <summary>
		/// Gets the full path to the downloaded or downloading file.
		/// </summary>
		public string FullPath { get; private set; }

		/// <summary>
		/// Gets the unique identifier for this download.
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// Gets the URL.
		/// </summary>
		public string Url { get; private set; }

		/// <summary>
		/// Gets the original URL before any redirections.
		/// </summary>
		public string OriginalUrl { get; private set; }

		/// <summary>
		/// Gets the suggested file name.
		/// </summary>
		public string SuggestedFileName { get; internal set; }

		/// <summary>
		/// Gets the content disposition.
		/// </summary>
		public string ContentDisposition { get; private set; }

		/// <summary>
		/// Gets the mime type.
		/// </summary>
		public string MimeType { get; private set; }

		/// <summary>
		/// Gets a value that indicates that an interrupted download can be resumed.
		/// </summary>
		public bool CanResume
		{
			get { return IsInProgress && _callback is not null; }
		}

		internal static bool TryUpdate(CefDownloadItem downloadItem, CefDownloadItemCallback callback)
		{
			WeakReference<CefNetDownloadOperation> weakRef;
			int id = (int)downloadItem.Id;
			lock (_Downloads)
			{
				_Downloads.TryGetValue(id, out weakRef);
			}
			if (weakRef is not null && weakRef.TryGetTarget(out CefNetDownloadOperation dl))
			{
				dl.UpdateInternal(downloadItem, callback);
				return true;
			}
			return false;
		}

		private void UpdateInternal(CefDownloadItem downloadItem, CefDownloadItemCallback callback)
		{
			if (!downloadItem.IsValid)
				return;

			this.IsInProgress = downloadItem.IsInProgress;
			this.IsComplete = downloadItem.IsComplete;
			this.IsCanceled = downloadItem.IsCanceled;
			this.CurrentSpeed = downloadItem.CurrentSpeed;
			this.PercentComplete = downloadItem.PercentComplete;
			this.TotalBytes = downloadItem.TotalBytes;
			this.ReceivedBytes = downloadItem.ReceivedBytes;
			this.EndTime = this.IsComplete ? downloadItem.EndTime : null;
			this.FullPath = downloadItem.FullPath;

			if (this.SuggestedFileName is null)
				this.SuggestedFileName = downloadItem.SuggestedFileName;
			//this.StartTime = downloadItem.StartTime.ToDateTime();
			//this.Url = downloadItem.Url;
			//this.OriginalUrl = downloadItem.OriginalUrl;
			//this.ContentDisposition = downloadItem.ContentDisposition;
			//this.MimeType = downloadItem.MimeType;

			if (_completion.Task.IsCanceled)
			{
				callback.Cancel();
				_callback = null;
			}
			else
			{
				_callback = callback;
				if (this.IsComplete)
					_completion.TrySetResult(true);
			}
		}

		public void Cancel()
		{
			Interlocked.Exchange(ref _callback, null)?.Cancel();
			_completion.TrySetCanceled();
		}

		/// <summary>
		/// Resumes a paused download. May also resume a download that was interrupted
		/// for another reason if <see cref="CanResume"/> returns true.
		/// </summary>
		public void Resume()
		{
			_callback?.Resume();
		}

		/// <summary>
		/// Pauses the download.
		/// </summary>
		/// <remarks>
		/// No effect if download is already paused.
		/// </remarks>
		public void Pause()
		{
			_callback?.Pause();
		}

		/// <summary>
		/// Returns a task that will complete when the download is completed.
		/// </summary>
		/// <returns>
		/// A task that represents the completion of the download.
		/// </returns>
		public Task WhenComplete()
		{
			return _completion.Task;
		}
	}
}
