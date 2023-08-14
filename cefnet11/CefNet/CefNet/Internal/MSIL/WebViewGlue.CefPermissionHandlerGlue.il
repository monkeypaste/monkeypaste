.class public auto ansi beforefieldinit CefNet.Internal.WebViewGlue
{
	.method assembly hidebysig instance bool AvoidOnShowPermissionPrompt()
	{
		.maxstack 2
		ldarg.0
		ldvirtftn instance bool CefNet.Internal.WebViewGlue::OnShowPermissionPrompt(class CefNet.CefBrowser, uint64, string, valuetype CefNet.CefPermissionRequestTypes, class CefNet.CefPermissionPromptCallback)
		ldftn instance bool CefNet.Internal.WebViewGlue::OnShowPermissionPrompt(class CefNet.CefBrowser, uint64, string, valuetype CefNet.CefPermissionRequestTypes, class CefNet.CefPermissionPromptCallback)
		ceq
		ret
	}
}