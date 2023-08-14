using System;
using System.Collections.Generic;
using System.Text;

namespace CefNet.Internal
{
	public partial class WebViewGlue
	{

		internal bool AvoidOnBeforeContextMenu()
		{
			return false;
		}

		internal protected virtual void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model)
		{

		}

		internal bool AvoidRunContextMenu()
		{
			return false;
		}

		internal protected virtual bool RunContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model, CefRunContextMenuCallback callback)
		{
			return WebView.RaiseRunContextMenu(frame, menuParams, model, callback);
		}

		internal bool AvoidOnContextMenuCommand()
		{
			return false;
		}

		internal protected virtual bool OnContextMenuCommand(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, int commandId, CefEventFlags eventFlags)
		{
			return false;
		}

		internal bool AvoidOnContextMenuDismissed()
		{
			return false;
		}

		/// <summary>
		/// Called when the context menu is dismissed irregardless of whether the menu
		/// was canceled or a command was selected.
		/// </summary>
		internal protected virtual void OnContextMenuDismissed(CefBrowser browser, CefFrame frame)
		{

		}

		internal bool AvoidRunQuickMenu()
		{
			return false;
		}

		/// <summary>
		/// Called to allow custom display of the quick menu for a windowless browser.
		/// </summary>
		/// <param name="browser"></param>
		/// <param name="frame"></param>
		/// <param name="location">The top left corner of the selected region</param>
		/// <param name="size">The size of the selected region</param>
		/// <param name="editStateFlags">A combination of flags that represent the state of the quick menu.</param>
		/// <param name="callback"></param>
		/// <returns>
		/// Return True if the menu will be handled and execute <paramref name="callback"/> either synchronously or
		/// asynchronously with the selected command ID. Return false to cancel the menu.
		/// </returns>
		internal protected virtual bool RunQuickMenu(CefBrowser browser, CefFrame frame, CefPoint location, CefSize size, CefQuickMenuEditStateFlags editStateFlags, CefRunQuickMenuCallback callback)
		{
			return false;
		}

		internal bool AvoidOnQuickMenuCommand()
		{
			return false;
		}

		/// <summary>
		/// Called to execute a command selected from the quick menu for a windowless
		/// browser.
		/// </summary>
		/// <param name="browser"></param>
		/// <param name="frame"></param>
		/// <param name="commandId">A command identifier.</param>
		/// <param name="eventFlags"></param>
		/// <returns>
		/// Return True if the command was handled or False for the
		/// default implementation.
		/// </returns>
		/// <remarks>
		/// See <see cref="CefMenuId"/> for command IDs that have default implementations.
		/// </remarks>
		internal protected virtual bool OnQuickMenuCommand(CefBrowser browser, CefFrame frame, int commandId, CefEventFlags eventFlags)
		{
			return false;
		}

		internal bool AvoidOnQuickMenuDismissed()
		{
			return false;
		}

		/// <summary>
		/// Called when the quick menu for a windowless browser is dismissed
		/// irregardless of whether the menu was canceled or a command was selected.
		/// </summary>
		internal protected virtual void OnQuickMenuDismissed(CefBrowser browser, CefFrame frame)
		{

		}
	}
}
