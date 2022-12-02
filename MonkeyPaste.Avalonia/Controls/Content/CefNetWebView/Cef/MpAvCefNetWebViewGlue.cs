using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CefNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using CefNet.Avalonia;
using CefNet.Internal;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MonkeyPaste.Avalonia {
    public class MpAvCefNetWebViewGlue : AvaloniaWebViewGlue {

        public MpAvCefNetWebViewGlue(WebView view) : base(view) {
        }

        protected override bool OnSetFocus(CefBrowser browser, CefFocusSource source) {
            if (source == CefFocusSource.Navigation) {
                return true;
            }
            
            return false;
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
            if(browser.Host.Client.GetWebView() is MpAvIDragSource ds) {
                Dispatcher.UIThread.Post(async () => {

                    //var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
                    //var kmf = MpAvShortcutCollectionViewModel.Instance.GlobalKeyModifierFlags;
                    //var pe = MpAvPointerInputHelpers.SimulatePointerEventArgs(ds as Control, gmp, kmf);
                    await MpAvDocumentDragHelper.PerformDragAsync(ds, ds.LastPointerPressedEventArgs, allowedOps.ToDragDropEffects());
                    
                });
            }
            return false;            
        }
        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model) {
            // ensure default cefnet context menu is empty
            model.Clear();
        }
    }
}
