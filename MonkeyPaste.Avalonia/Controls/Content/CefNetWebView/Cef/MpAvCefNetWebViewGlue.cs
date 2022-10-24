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
        public MpAvCefNetWebViewGlue(MpAvCefNetWebView view) : base(view) {

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
            bool isDragValid = true;

            // TODO should check dragData here and when invalid return false

            if (!isDragValid) {
                return false;
            }

            Dispatcher.UIThread.Post(async () => {
                var wv = browser.Host.Client.GetWebView() as MpAvCefNetWebView;
                var ctvm = wv.BindingContext;
                object drag_lock = System.Guid.NewGuid().ToString();

                // NOTE only setting this true (maynot be if not all selected) for js msg to know its for ole
                ctvm.IsTileDragging = true; 

                PointerEventArgs pe = null;
                EventHandler<PointerEventArgs> pointer_move_handler = null;
                pointer_move_handler = (s, e) => {
                    pe = e;
                    if(e.IsLeftDown(wv)) {
                        MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();
                    } else {
                        // NOTE not sure if these events are received since dnd is progress 
                        // but probably good to keep since drag end is so annoying to handle...
                        MpConsole.WriteLine("CefGlue pointer move event detached ITSELF");
                        MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = null;
                        wv.PointerMoved -= pointer_move_handler;
                    }
                    
                };

                wv.PointerMoved += pointer_move_handler;
                bool wasEscapePressed = false;

                EventHandler<string> modKeyUpOrDownHandler = (s, e) => {
                    if(MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown) {
                        wasEscapePressed = true;
                    }
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

                //bool is_tile_drag = false;
                DragDropEffects allowedEffects = allowedOps.ToDragDropEffects();

                if (ctvm.ItemType == MpCopyItemType.FileList) {
                    allowedEffects = DragDropEffects.Copy;
                } else if (ctvm.ItemType == MpCopyItemType.Image) {
                    allowedEffects = DragDropEffects.Move;
                }
                MpAvDataObject avmpdo = await wv.Document.GetDataObjectAsync(false, false);
                ctvm.IsTileDragging = await avmpdo.ContainsInternalContentItem_safe(drag_lock);

                // seems excessive...but ultimately all ole pref's come from plugins so pass everthing through cb plugin system just like writing to clipboard
                avmpdo = await MpPlatformWrapper.Services.DataObjectHelperAsync.WriteDragDropDataObject(avmpdo) as MpAvDataObject;

                while (pe == null) {
                    await Task.Delay(100);
                }

                var result = await DragDrop.DoDragDrop(pe, avmpdo, allowedEffects);
                bool wasCopy = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown;
                bool wasSelfDrop = !wasEscapePressed && ctvm.IsHovering;
                ctvm.IsTileDragging = false;
                wv.PointerMoved -= pointer_move_handler;
                MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = null;

                MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= modKeyUpOrDownHandler;
                MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= modKeyUpOrDownHandler;

                MpConsole.WriteLine("Cef Drag Result: " + result);


                MpConsole.WriteLine("Was Self drop: " + wasSelfDrop);

                string dropEffect = wasSelfDrop && !wasCopy ? "move":"copy";

                if(wasEscapePressed) {
                    dropEffect = "none";
                }

                MpConsole.WriteLine("ACTUAL drag result: " + dropEffect);
                //if(!wasSelfDrop) {
                var dragEndMsg = new MpQuillDragEndMessage() {
                    dataTransfer = new MpQuillDataTransferMessageFragment() {
                        dropEffect = dropEffect
                    },
                    fromHost = true,
                    wasCancel = wasEscapePressed
                };

                // wait before singaling drag end, 
                //await Task.Delay(500);
                await wv.EvaluateJavascriptAsync($"dragEnd_ext('{dragEndMsg.SerializeJsonObjectToBase64()}')");
                //}

            });
            return false;
        }
        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model) {
            // ensure default cefnet context menu is empty
            model.Clear();
        }
    }
}
