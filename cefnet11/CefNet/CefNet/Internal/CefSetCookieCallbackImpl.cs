using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefNet.Internal
{
	internal sealed class CefSetCookieCallbackImpl : CefSetCookieCallback
	{
		private readonly TaskCompletionSource<bool> _completion;

		public CefSetCookieCallbackImpl()
		{
			_completion = new TaskCompletionSource<bool>();
		}

		protected internal override void OnComplete(bool success)
		{
			_completion.TrySetResult(success);
		}

		public void Cancel()
		{
			_completion.TrySetCanceled();
		}

		public Task<bool> WaitTask
		{
			get { return _completion.Task; }
		}
	}
}
