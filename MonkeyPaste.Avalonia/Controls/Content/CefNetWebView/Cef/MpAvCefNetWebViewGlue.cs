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
        public MpAvCefNetWebViewGlue(IAvaloniaWebViewPrivate view) : base(view) {

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
            bool isDragValid = true;

            // TODO should check dragData here and when invalid return false

            if (!isDragValid) {
                return false;
            }

            Dispatcher.UIThread.Post(async () => {
                var wv = browser.Host.Client.GetWebView() as MpAvCefNetWebView;
                var ctvm = wv.BindingContext;

                EventHandler<string> modKeyUpOrDownHandler = (s, e) => {
                    var modKeyMsg = new MpQuillModifierKeysNotification() {
                        ctrlKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                        altKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown,
                        shiftKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown,
                        escKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown
                    };
                    wv.ExecuteJavascript($"updateModifierKeysFromHost_ext('{modKeyMsg.SerializeJsonObjectToBase64()}')");
                };

                MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += modKeyUpOrDownHandler;
                MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += modKeyUpOrDownHandler;

                var avmpdo = new MpAvDataObject();
                avmpdo.SetData(MpPortableDataFormats.Text, dragData.FragmentText);
                avmpdo.SetData(MpPortableDataFormats.Html, dragData.FragmentHtml);
                avmpdo.MapAllPseudoFormats();
                
                Pointer p = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
                var pe = new PointerEventArgs(Control.PointerPressedEvent,wv,p,
                    MpAvMainWindow.Instance, MpAvMainWindow.Instance.Position.ToAvPoint(),(ulong)DateTime.Now.Ticks,
                    new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed), KeyModifiers.None);

                var result = await DragDrop.DoDragDrop(pe, avmpdo, allowedOps.ToDragDropEffects());
                
                MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= modKeyUpOrDownHandler;
                MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= modKeyUpOrDownHandler;

                MpConsole.WriteLine("Cef Drag Result: " + result);

                var dragEndMsg = new MpQuillDragEndMessage() {
                    dataTransfer = new MpQuillDataTransferMessageFragment() {
                        dropEffect = result.ToCefDragOperationsMask().ToString()
                    }
                };

                await wv.EvaluateJavascriptAsync($"dragEnd_ext('{dragEndMsg.SerializeJsonObjectToBase64()}')");
                if (wv.GetVisualAncestor<MpAvClipTileView>() is MpAvClipTileView ctv) {
                    ctv.ReloadContent();
                }
            });
            return true;
        }
        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model) {
            // ensure default cefnet context menu is empty
            model.Clear();
        }
    }
}
