#if DESKTOP

using Avalonia.Input;
using Avalonia.Threading;
using CefNet;
using CefNet.Avalonia;
using CefNet.Internal;
using System.Windows.Controls;

namespace MonkeyPaste.Avalonia {
    public class MpAvCefNetWebViewGlue : AvaloniaWebViewGlue {

        public MpAvCefNetWebViewGlue(WebView view) : base(view) {
        }
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
            if (browser.Host.Client.GetWebView() is MpAvIDragSource ds) {
                Dispatcher.UIThread.Post(async () => {

                    //var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
                    //var kmf = MpAvShortcutCollectionViewModel.Instance.GlobalKeyModifierFlags;
                    //var pe = MpAvPointerInputHelpers.SimulatePointerEventArgs(ds as Control, gmp, kmf);

                    //var de = allowedOps.ToDragDropEffects();
                    var de = DragDropEffects.Move | DragDropEffects.Copy;
                    await MpAvContentDragHelper.StartDragAsync(ds, ds.LastPointerPressedEventArgs, de);

                    browser.Host.DragSourceEndedAt(0, 0, CefDragOperationsMask.None);
                    browser.Host.DragSourceSystemDragEnded();
                });
            }
            return true;
        }
        protected override void UpdateDragCursor(CefBrowser browser, CefDragOperationsMask operation) {
            base.UpdateDragCursor(browser, operation);
        }

        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model) {
            // ensure default cefnet context menu is empty
            model.Clear();
        }
    }
}

#endif