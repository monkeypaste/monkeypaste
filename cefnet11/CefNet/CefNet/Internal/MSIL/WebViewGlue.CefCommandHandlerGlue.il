.class public auto ansi beforefieldinit CefNet.Internal.WebViewGlue
{
	.method assembly hidebysig instance bool AvoidOnChromeCommand()
	{
		.maxstack 2
		ldarg.0
		ldvirtftn instance bool CefNet.Internal.WebViewGlue::OnChromeCommand(class CefNet.CefBrowser, int32, valuetype CefNet.CefWindowOpenDisposition)
		ldftn instance bool CefNet.Internal.WebViewGlue::OnChromeCommand(class CefNet.CefBrowser, int32, valuetype CefNet.CefWindowOpenDisposition)
		ceq
		ret
	}

}