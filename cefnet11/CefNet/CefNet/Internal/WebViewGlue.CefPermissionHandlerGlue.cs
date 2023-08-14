using System.Runtime.CompilerServices;

namespace CefNet.Internal
{
	public partial class WebViewGlue
	{
		internal bool AvoidOnRequestMediaAccessPermission()
		{
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool OnRequestMediaAccessPermission(CefBrowser browser, CefFrame frame, string requestingUrl, uint requestedPermissions, CefMediaAccessCallback callback)
		{
			return OnRequestMediaAccessPermission(browser, frame, requestingUrl, (CefMediaAccessPermissionTypes)requestedPermissions, callback);
		}

		/// <summary>
		/// Called when a page requests permission to access media.
		/// </summary>
		/// <param name="requestingUrl">The URL requesting permission</param>
		/// <param name="requestedPermissions">
		/// A combination of values from <see cref="CefMediaAccessPermissionTypes"/>
		/// that represent the requested permissions.
		/// </param>
		/// <returns>
		/// Return True and call <see cref="CefMediaAccessCallback.Continue"/>
		/// either in this function or at a later time to continue or cancel the request.
		/// Return False to cancel the request immediately.
		/// </returns>
		/// <remarks>
		/// This function will not be called if the &quot;--enable-media-stream&quot;
		/// command-line switch is used to grant all permissions.
		/// </remarks>
		protected internal virtual bool OnRequestMediaAccessPermission(CefBrowser browser, CefFrame frame, string requestingUrl, CefMediaAccessPermissionTypes requestedPermissions, CefMediaAccessCallback callback)
		{
			return false;
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal extern bool AvoidOnShowPermissionPrompt();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool OnShowPermissionPrompt(CefBrowser browser, ulong promptId, string requestingOrigin, uint requestedPermissions, CefPermissionPromptCallback callback)
		{
			return OnShowPermissionPrompt(browser, promptId, requestingOrigin, (CefPermissionRequestTypes)requestedPermissions, callback);
		}

		/// <summary>
		/// Called when a page should show a permission prompt.
		/// </summary>
		/// <param name="browser"></param>
		/// <param name="promptId">Uniquely identifies the prompt.</param>
		/// <param name="requestingOrigin">The URL origin requesting permission.</param>
		/// <param name="requestedPermissions">
		/// A combination of values from <see cref="CefPermissionRequestTypes"/>
		/// that represent the requested permissions.</param>
		/// <param name="callback"></param>
		/// <returns>
		/// Return True and call <see cref="CefPermissionPromptCallback.Continue"/> either
		/// in this function or at a later time to continue or cancel the request.
		/// Return False to proceed with default handling.</returns>
		/// <remarks>
		/// With the Chrome runtime, default handling will display the permission prompt UI.
		/// With the Alloy runtime, default handling is <see cref="CefPermissionRequestResult.Ignore"/>.
		/// </remarks>
		protected internal virtual bool OnShowPermissionPrompt(CefBrowser browser, ulong promptId, string requestingOrigin, CefPermissionRequestTypes requestedPermissions, CefPermissionPromptCallback callback)
		{
			return false;
		}

		internal bool AvoidOnDismissPermissionPrompt()
		{
			return false;
		}

		/// <summary>
		/// Called when a permission prompt handled via OnShowPermissionPrompt is
		/// dismissed.
		/// </summary>
		/// <param name="browser"></param>
		/// <param name="promptId">Will match the value that was passed to OnShowPermissionPrompt.</param>
		/// <param name="result">
		/// Will be the value passed to <see cref="CefPermissionPromptCallback.Continue"/>
		/// or <see cref="CefPermissionRequestResult.Ignore"/> if the dialog was dismissed
		/// for other reasons such as navigation, browser closure, etc.
		/// </param>
		/// <remarks>
		/// This function will not be called if OnShowPermissionPrompt
		/// returned False for <paramref name="promptId"/>.
		/// </remarks>
		protected internal virtual void OnDismissPermissionPrompt(CefBrowser browser, ulong promptId, CefPermissionRequestResult result)
		{

		}
	}
}
