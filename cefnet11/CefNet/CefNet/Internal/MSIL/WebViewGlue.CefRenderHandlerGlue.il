.class public auto ansi beforefieldinit CefNet.Internal.WebViewGlue
{
	.method assembly hidebysig instance bool AvoidStartDragging()
	{
		.maxstack 2
		ldarg.0
		ldvirtftn instance bool CefNet.Internal.WebViewGlue::StartDragging(class CefNet.CefBrowser, class CefNet.CefDragData, valuetype CefNet.CefDragOperationsMask allowedOps, int32, int32)
		ldftn instance bool CefNet.Internal.WebViewGlue::StartDragging(class CefNet.CefBrowser, class CefNet.CefDragData, valuetype CefNet.CefDragOperationsMask allowedOps, int32, int32)
		ceq
		ret
	}

	.method assembly hidebysig instance bool AvoidUpdateDragCursor()
	{
		.maxstack 2
		ldarg.0
		ldvirtftn instance void CefNet.Internal.WebViewGlue::UpdateDragCursor(class CefNet.CefBrowser, valuetype CefNet.CefDragOperationsMask allowedOps)
		ldftn instance void CefNet.Internal.WebViewGlue::UpdateDragCursor(class CefNet.CefBrowser, valuetype CefNet.CefDragOperationsMask allowedOps)
		ceq
		ret
	}

	.method assembly hidebysig instance bool AvoidOnTouchHandleStateChanged()
	{
		.maxstack 2
		ldarg.0
		ldvirtftn instance void CefNet.Internal.WebViewGlue::OnTouchHandleStateChanged(class CefNet.CefBrowser, valuetype CefNet.CefTouchHandleState state)
		ldftn instance void CefNet.Internal.WebViewGlue::OnTouchHandleStateChanged(class CefNet.CefBrowser, valuetype CefNet.CefTouchHandleState state)
		ceq
		ret
	}

}