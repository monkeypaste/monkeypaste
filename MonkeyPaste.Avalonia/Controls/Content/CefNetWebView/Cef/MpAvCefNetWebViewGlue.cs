#if DESKTOP

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CefNet;
using CefNet.Avalonia;
using CefNet.Internal;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvCefNetWebViewGlue : AvaloniaWebViewGlue {
        public MpAvCefNetWebViewGlue(WebView view) : base(view) { }
        //
        // Summary:
        //     Called when the browser component is requesting focus. Return false to allow
        //     the focus to be set or true to cancel setting the focus.
        //
        // Parameters:
        //   browser:
        //
        //   source:
        //     Indicates where the focus request is originating from.
        //
        // Returns:
        //     Return false to allow the focus to be set or true to cancel setting the focus.
        protected override bool OnSetFocus(CefBrowser browser, CefFocusSource source) {
            //if (source == CefFocusSource.Navigation)
            //    return true;
            //return false;
            return false;

            //if (source == CefFocusSource.Navigation) {
            //    return false;
            //}

            //return true;
        }

        /// <summary>
        /// Called when the user starts dragging content in the web view. OS APIs that run a system message
        /// loop may be used within the StartDragging call. Don't call any of CefBrowserHost::DragSource*Ended*
        /// methods after returning false. Call CefBrowserHost::DragSourceEndedAt and DragSourceSystemDragEnded
        /// either synchronously or asynchronously to inform the web view that the drag operation has ended.
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="dragData">The contextual information about the dragged content.</param>
        /// <param name="allowedOps"></param>
        /// <param name="x">The X-location in screen coordinates.</param>
        /// <param name="y">The Y-location in screen coordinates.</param>
        /// <returns>Return false to abort the drag operation or true to handle the drag operation.</returns>
        /// 
        protected override bool StartDragging(CefBrowser browser, CefDragData dragData, CefDragOperationsMask allowedOps, int x, int y) {
            if (browser.Host.Client.GetWebView() is not MpAvIContentWebViewDragSource ds ||
                ds is not Control c ||
                ds is not MpIContentView cv ||
                !cv.IsContentLoaded) {

                // not a real drag, not sure why this happens (occured attempting resize) 
                // maybe coming from transparent placeholder?
                return false;
            }
            Dispatcher.UIThread.Post(async () => {
                allowedOps = CefDragOperationsMask.Copy;
                var de = DragDropEffects.Copy;// | DragDropEffects.Move;
                await MpAvContentWebViewDragHelper.StartDragAsync(ds, de);

                browser.Host.DragSourceEndedAt(0, 0, CefDragOperationsMask.None);
                browser.Host.DragSourceSystemDragEnded();
            });

            return true;
        }
        protected override void UpdateDragCursor(CefBrowser browser, CefDragOperationsMask operation) {
            if (operation == CefDragOperationsMask.None) {
                MpConsole.WriteLine($"Drag cursor set to '{operation}'");
            }
            base.UpdateDragCursor(browser, operation);
        }
        protected override bool OnCursorChange(CefBrowser browser, nint cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo) {
            MpConsole.WriteLine($"Cursor: {type}");
            if (type == CefCursorType.Northsouthresize) {

            }
            return base.OnCursorChange(browser, cursorHandle, type, customCursorInfo);
        }
        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model) {
            // ensure default cefnet context menu is empty
            model.Clear();
        }
    }
}

#endif