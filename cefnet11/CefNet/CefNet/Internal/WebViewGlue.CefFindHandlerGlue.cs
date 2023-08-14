namespace CefNet.Internal
{
	public partial class WebViewGlue
	{
		internal bool AvoidOnFindResult()
		{
			return false;
		}

		/// <summary>
		/// Called to report find results returned by <see cref="CefBrowserHost.Find"/>.
		/// </summary>
		/// <param name="browser">The browser.</param>
		/// <param name="identifier">A unique incremental identifier for the currently active search.</param>
		/// <param name="count">The number of matches currently identified.</param>
		/// <param name="selectionRect">The location of where the match was found (in window coordinates).</param>
		/// <param name="activeMatchOrdinal">The current position in the search results.</param>
		/// <param name="finalUpdate"> True if this is the last find notification.</param>
		internal protected virtual void OnFindResult(CefBrowser browser, int identifier, int count, CefRect selectionRect, int activeMatchOrdinal, bool finalUpdate)
		{
			WebView.RaiseTextFound(new TextFoundEventArgs(identifier, count, selectionRect, activeMatchOrdinal, finalUpdate));
		}
	}
}
