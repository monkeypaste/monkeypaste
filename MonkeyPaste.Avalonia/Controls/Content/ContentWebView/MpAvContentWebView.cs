using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;

using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using AvToolTip = Avalonia.Controls.ToolTip;

#if WINDOWS
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media.Media3D;
#endif

#if CEFNET_WV
using CefNet.Avalonia;
#endif

#if OUTSYS_WV
using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using Xilium.CefGlue.Common.Handlers;
using Xilium.CefGlue.Common.Helpers.Logger;
using Xilium.CefGlue.Common.InternalHandlers;
#endif

#if SUGAR_WV
using TheArtOfDev.HtmlRenderer.Avalonia;
#if MAC
using MonoMac.Foundation;
#endif
#endif

namespace MonkeyPaste.Avalonia {
    public class MpAvContentWebView :
        MpAvWebView,
        MpIContentView,
        MpAvIContentWebViewDragSource,
        MpAvIResizableControl,
        MpAvIDomStateAwareWebView,
        MpAvIAsyncJsEvalWebView,
        MpAvIReloadableContentWebView {

        #region Private Variables

        private MpQuillContentDataObjectResponseMessage _lastDataObjectResp = null;
        private MpQuillEditorContentChangedMessage _lastReadOnlyEnabledFromHostResp = null;
        private MpQuillEditorSelectionStateMessage _lastEditorSelectionStateMessage = null;
        private DateTime? _lastAppendStateChangeCompleteDt = null;

        #endregion

        #region Constants
        #endregion

        #region Statics
        public static bool BreakOnNextLoad = false;

#if OUTSYS_WV
        static MpAvContentWebView() {
            //CommonCefRenderHandler.StartDragFunc = StartDragging;
        }
        }

        private static bool StartDragging(CefBrowser browser, CefDragData dragData, CefDragOperationsMask allowedOps, int x, int y) {

            Dispatcher.UIThread.Post(async () => {
                allowedOps = CefDragOperationsMask.Copy;
                if (MpAvClipTrayViewModel.Instance.SelectedItem == null ||
                       MpAvClipTrayViewModel.Instance.SelectedItem.GetContentView() is not MpAvContentWebView wv ||
                       !wv.IsEditorLoaded) {
                    return;
                }
                DragDropEffects result = await MpAvContentWebViewDragHelper.StartDragAsync(wv, DragDropEffects.Copy);

                //browser.GetHost().DragSourceEndedAt(0, 0, CefDragOperationsMask.None);
                //browser.GetHost().DragSourceSystemDragEnded();
            });

            return true;
        }
#endif

        #endregion

        #region Interfaces
        #region MpIJsonMessenger Implementation
        public void SendMessage(string msgJsonBase64Str) {
#if CEFNET_WV
            this.ExecuteJavascript(msgJsonBase64Str);
#elif OUTSYS_WV
            this.ExecuteScript(msgJsonBase64Str);
#elif SUGAR_WV
            if (this is MpICanExecuteJavascript icej) {
                icej.ExecuteJavascript(msgJsonBase64Str);
            }
#else
            if (Interop == null) {
                MpDebug.Break("lifecycle bug");
                return;
            }
            Interop.SendMessage(this, msgJsonBase64Str);
#endif
        }


        #endregion

        //        #region MpIAyncJsonMessenger Implementation
        //        public async Task<string> SendMessageAsync(string msgJsonBase64Str) {
        //            string result;
        //#if CEFNET_WV
        //            result = await this.EvaluateJavascriptAsync(msgJsonBase64Str);
        //#else
        //            if (Interop == null) {
        //                MpDebug.Break("lifecycle bug");
        //                return string.Empty;
        //            }
        //            result = await Interop.SendMessageAsync(this, msgJsonBase64Str);
        //#endif
        //            return result;
        //        }
        //        #endregion


        #region MpIContentView

        #region MpIRecyclableLocatorItem Implementation
        int MpILocatorItem.LocationId =>
            BindingContext is MpILocatorItem ? (BindingContext as MpILocatorItem).LocationId : 0;
        DateTime? MpIRecyclableLocatorItem.LocatedDateTime { get; set; }
        #endregion

        bool MpIContentView.IsViewInitialized =>
            IsEditorInitialized;

        bool MpIContentView.IsContentLoaded =>
            IsEditorLoaded;

        bool MpIContentView.IsSubSelectable =>
            IsContentSubSelectable;


        Task<bool> MpIContentView.UpdateContentAsync(object contentJsonObj) =>
            PerformUpdateContentRequestAsync(contentJsonObj);


        #endregion

        #region MpAvIReloadableWebView Implementation

        async Task MpAvIReloadableContentWebView.ReloadContentAsync() {
            await LoadContentAsync();
        }

        #endregion

        #region MpAvIResizableControl Implementation
        private Control _resizerControl;
        public Control ResizerControl {
            get {
                if (_resizerControl == null) {
                    var ctv = this.GetVisualAncestor<MpAvClipTileView>();
                    if (ctv != null) {
                        string resizer_name =
                            MpAvMainWindowViewModel.Instance.IsVerticalOrientation ?
                                "ClipTileHeightResizeBorder" : "ClipTileWidthResizeBorder";
                        _resizerControl = ctv.FindControl<Control>(resizer_name);
                    }
                }
                return _resizerControl;
            }
            set {
                _resizerControl = value;
            }
        }
        #endregion

        #region MpAvIContentDragSource Implementation
        public string[] GetDragFormats() {
            if (BindingContext == null) {
                return new string[] { };
            }
            return BindingContext.GetOleFormats(true);
        }

        public PointerEventArgs LastPointerPressedEventArgs { get; set; }

        public bool IsDragging {
            get {
                if (BindingContext is MpIDraggable dr) {
                    return dr.IsDragging;
                }
                return false;
            }
            set {
                if (BindingContext is MpIDraggable dr) {
                    dr.IsDragging = value;
                }
            }
        }
        public bool WasDragCanceled { get; set; }

        public void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc, bool meta) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => NotifyModKeyStateChanged(ctrl, alt, shift, esc, meta));
                return;
            }
            var modKeyMsg = new MpQuillModifierKeysNotification() {
                ctrlKey = ctrl,
                altKey = alt,
                shiftKey = shift,
                escKey = esc,
                metaKey = meta
            };
            SendMessage($"updateModifierKeysFromHost_ext('{modKeyMsg.SerializeObjectToBase64()}')");
        }


        public async Task<MpAvDataObject> GetDataObjectAsync(
            string[] formats = null,
            bool ignore_selection = false) {
            if (BindingContext == null ||
                BindingContext.IsAnyPlaceholder) {
                MpDebug.Break();
                return new MpAvDataObject();
            }

            var ctvm = BindingContext;
            var contentDataReq = new MpQuillContentDataObjectRequestMessage() {
                // 'forOle' not the best name 
                selectionOnly = !ignore_selection
            };

            // NOTE when file is on clipboard pasting into tile removes all other formats besides file
            // and pseudo files are only needed for dnd comptaibility so its gewd
            if (formats == null) {
                // NOTE important that ALL data formats are on clipboard for drag source obj to process 
                contentDataReq.formats = MpDataFormatRegistrar.RegisteredFormats.ToList();
            } else {
                contentDataReq.formats = formats.ToList();
            }

            bool provided_formats = formats != null;

            MpAvDataObject GetFallbackDataObject() {
                var fb_avdo = BindingContext.CopyItem.ToAvDataObject(true, true, contentDataReq.formats.ToArray());
                //await fb_avdo.MapAllPseudoFormatsAsync();
                fb_avdo.DataFormatLookup.Where(x => x.Value == null).ForEach(x => fb_avdo.Remove(x.Key));
                return fb_avdo;
            }

            var avdo = new MpAvDataObject();
            bool is_all_content = true;
            bool use_wv = IsEditorLoaded &&
#if SUGAR_WV
                (ReadOnlyWebView == null || !ReadOnlyWebView.IsVisible);
#else
                true;
#endif
            MpConsole.WriteLine($"GetDataObject using {(use_wv ? "rwwv" : "rowv")}");
            if (use_wv) {
                var sw = Stopwatch.StartNew();
                _lastDataObjectResp = null;
                SendMessage($"contentDataObjectRequestAsync_ext_ntf('{contentDataReq.SerializeObjectToBase64()}')");
                while (_lastDataObjectResp == null) {
                    // wait for binding handler to receive 
                    await Task.Delay(100);
                    if (sw.Elapsed > TimeSpan.FromSeconds(10)) {
                        MpConsole.WriteLine($"GetDataObject timeout waiting for resp for '{BindingContext}'");
                        return GetFallbackDataObject();
                    }
                }

                // store resp and immediabtly clear msg obj
                MpQuillContentDataObjectResponseMessage contentDataResp = _lastDataObjectResp;
                _lastDataObjectResp = null;
                is_all_content = contentDataResp.isAllContent;

                if (contentDataResp.dataItems == null) {
                    return null;
                }
                foreach (var di in contentDataResp.dataItems) {
                    avdo.SetData(di.format, di.data);
                }
            } else {
#if SUGAR_WV
                string pt = ignore_selection || ReadOnlyWebView.SelectedText.IsNullOrEmpty() ?
                    BindingContext.SearchableText : ReadOnlyWebView.SelectedText;
                string html = ignore_selection || ReadOnlyWebView.SelectedHtml.IsNullOrEmpty() ?
                    BindingContext.CopyItemData : ReadOnlyWebView.SelectedHtml;
                // NOTE always providing text and html (should be stripping later)

                avdo.SetData(MpPortableDataFormats.Text, pt);
                avdo.SetData(MpPortableDataFormats.Html, html);
#else
                return GetFallbackDataObject();
#endif
            }

            if (ctvm.CopyItemType == MpCopyItemType.FileList) {
                if (contentDataReq.formats.Contains(MpPortableDataFormats.Files)) {
                    if (avdo.TryGetData(MpPortableDataFormats.Files, out string fp_str) &&
                        !string.IsNullOrWhiteSpace(fp_str)) {
                        // file data is csv using DefaultCsvProps seperated by rows which should be envNewLine

                        avdo.SetData(MpPortableDataFormats.Files, fp_str.SplitNoEmpty(MpCopyItem.FileItemSplitter));
                    } else {

                        avdo.SetData(MpPortableDataFormats.Files, ctvm.CopyItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter));
                    }
                }
            } 

            bool is_full_content = ctvm.CopyItemType == MpCopyItemType.Image || is_all_content;
            avdo.AddContentReferences(ctvm.CopyItem, is_full_content);

            if (ctvm.CopyItemType == MpCopyItemType.Image &&
                    ctvm.CopyItemData.ToAvBitmap() is Bitmap bmp) {

                avdo.SetData(MpPortableDataFormats.Image, bmp.ToByteArray());
                //avdo.SetData(MpPortableDataFormats.Text, bmp.ToAsciiImage());
                // TODO add colorized ascii maybe as html and rtf!!
            } 
            // remove all empty formats (workaround for cefnet bug w/ empty asciiUrl
            avdo.DataFormatLookup.Where(x => x.Value == null)
                .ForEach(x => avdo.Remove(x.Key));
            return avdo;
        }

        public bool IsCurrentDropTarget => BindingContext == null ? false : BindingContext.IsDropOverTile;

        #endregion

        #region MpAvIWebViewBindingResponseHandler Implementation

        private string _lastContentHandle { get; set; } = null;
        public override void HandleBindingNotification(
            MpEditorBindingFunctionType notificationType,
            string msgJsonBase64Str,
            string contentHandle) {
            base.HandleBindingNotification(notificationType, msgJsonBase64Str, contentHandle);

            bool is_new_content = _lastContentHandle != contentHandle;
            _lastContentHandle = contentHandle;
            if (!this.IsAttachedToVisualTree()) {
                NeedsEvalJsCleared = true;
                return;
            }

            var ctvm = BindingContext;
            if (ctvm == null &&
                notificationType != MpEditorBindingFunctionType.notifyDomLoaded &&
                notificationType != MpEditorBindingFunctionType.notifyInitComplete) {
                // converter doesn't have data context but needs to notify dom loaded which doesn't need it
                MpDebug.Assert(this is MpAvPlainHtmlConverterWebView, "Shouldn't happen, editor never loads. Maybe need to block until data context set?");
                return;
            }
            if (BindingContext != null && BindingContext.CopyItemTitle == "Text12" && BindingContext.IsWindowOpen) {

            }
            object ntf = null;
            switch (notificationType) {

                #region LIFE CYCLE

                case MpEditorBindingFunctionType.notifyDomLoaded:
                    IsDomLoaded = true;
                    LoadEditorAsync().FireAndForgetSafeAsync();
                    break;
                case MpEditorBindingFunctionType.notifyInitComplete:
                    IsEditorInitialized = true;
                    break;

                case MpEditorBindingFunctionType.notifyLoadComplete:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage loadComplete_ntf) {
                        //_lastLoadedContentHandle = contentHandle
                        IsEditorLoaded = true;
                        ProcessContentChangedMessage(loadComplete_ntf, is_new_content);
                    }
                    break;

                #endregion

                #region CONTENT CHANGED

                case MpEditorBindingFunctionType.notifyContentImageLoaded:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillContentImageLoadedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillContentImageLoadedNotification imgLoadedNtf) {
                        Dispatcher.UIThread.Post(async () => {
                            var sw = Stopwatch.StartNew();
                            while (BindingContext.IsPlaceholder) {
                                await Task.Delay(100);
                                if (sw.Elapsed > TimeSpan.FromSeconds(5)) {
                                    MpConsole.WriteLine($"Image load timeout, item is placeholder..");
                                    return;
                                }
                            }
                            BindingContext.CopyItemSize1 = (int)imgLoadedNtf.width;
                            BindingContext.CopyItemSize2 = (int)imgLoadedNtf.height;
                        });
                    }
                    break;
                case MpEditorBindingFunctionType.notifyReadOnlyDisabled:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillDisableReadOnlyResponseMessage disableReadOnlyMsg) {
                        ctvm.IsContentReadOnly = false;
                        //ctvm.UnconstrainedContentDimensions = new MpSize(disableReadOnlyMsg.editorWidth, disableReadOnlyMsg.editorHeight);
                    }
                    break;
                case MpEditorBindingFunctionType.notifyReadOnlyEnabled:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage enableReadOnlyMsg) {
                        // NOTE only difference from contentChanged is no dimension info and this needs to enable readonly
                        ctvm.IsContentReadOnly = true;
                        ProcessContentChangedMessage(enableReadOnlyMsg);
                    }
                    break;

                case MpEditorBindingFunctionType.notifyReadOnlyEnabledFromHost:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage enableReadOnlyFromHostMsg) {
                        _lastReadOnlyEnabledFromHostResp = enableReadOnlyFromHostMsg;
                    }
                    break;
                case MpEditorBindingFunctionType.notifyContentChanged:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage contentChanged_ntf) {
                        ProcessContentChangedMessage(contentChanged_ntf);
                        //RelayMsg($"contentChanged_ext('{msgJsonBase64Str}')");
                    }
                    break;
                case MpEditorBindingFunctionType.notifyDataTransferCompleted:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillDataTransferCompletedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
                        ProcessDataTransferCompleteResponse(dataTransferCompleted_ntf);
                    }
                    break;
                case MpEditorBindingFunctionType.notifyLastTransactionUndone:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillLastTransactionUndoneNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillLastTransactionUndoneNotification lastTransUndone_ntf) {
                        BindingContext
                            .TransactionCollectionViewModel
                            .RemoveMostRecentTransactionCommand.Execute(null);
                    }
                    break;

                #endregion

                #region LAYOUT

                case MpEditorBindingFunctionType.notifyScrollBarVisibilityChanged:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillOverrideScrollNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillOverrideScrollNotification scrollbarVisibleMsg) {
                        MpAvClipTrayViewModel.Instance.IsScrollDisabled = scrollbarVisibleMsg.canScrollY;
                    }
                    break;

                case MpEditorBindingFunctionType.notifyShowToolTip:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillShowToolTipNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowToolTipNotification showToolTipNtf) {
                        if (showToolTipNtf.isVisible && !IsDragging) {
                            AvToolTip.SetTip(this, null);
                            var tt = new MpAvToolTipView();
                            tt.IsHtml = !string.IsNullOrEmpty(showToolTipNtf.tooltipHtml);
                            tt.ToolTipText = tt.IsHtml ? showToolTipNtf.tooltipHtml : showToolTipNtf.tooltipText;
                            tt.InputGestureText = showToolTipNtf.gestureText;
                            if (string.IsNullOrEmpty(tt.ToolTipText)) {
                                return;
                            }

                            void tt_attachedToView(object sender, EffectiveViewportChangedEventArgs e) {
                                // anchor is some editor elm's center
                                // move tooltip towards editor center along line from center to anchor
                                // to avoid tooltip overlapping mouse which creates a stutter from
                                // leave/exit calls

                                var tl = MpAvWindowManager.GetTopLevel(tt);
                                var tt_size = tl.Bounds.Size.ToPortableSize();
                                if (tt_size.IsEmpty()) {
                                    return;
                                }
                                MpPoint center_p = this.Bounds.Center.ToPortablePoint();
                                MpPoint adj_p = center_p - (tt_size.ToPortablePoint() * 0.5);
                                if (Math.Abs(center_p.Y - showToolTipNtf.anchorY) < tt_size.Height) {
                                    // if its in the middle just shove to the top
                                    adj_p.Y = this.Bounds.Top;
                                } else if (showToolTipNtf.anchorY < tt_size.Height) {
                                    // if its at the top shove it to the bottom
                                    adj_p.Y = this.Bounds.Bottom - tt_size.Height;
                                }

                                AvToolTip.SetHorizontalOffset(this, adj_p.X);
                                AvToolTip.SetVerticalOffset(this, adj_p.Y);

                                tt.EffectiveViewportChanged -= tt_attachedToView;
                            }
                            // wait till tt is attached to know its height
                            tt.EffectiveViewportChanged += tt_attachedToView;

                            AvToolTip.SetTip(this, tt);
                            AvToolTip.SetPlacement(this, PlacementMode.TopEdgeAlignedLeft);
                            AvToolTip.SetIsOpen(this, true);
                        } else {
                            AvToolTip.SetIsOpen(this, false);
                            AvToolTip.SetTip(this, null);
                        }

                    }
                    break;
                #endregion

                #region SELECTION

                case MpEditorBindingFunctionType.notifySelectionState:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillEditorSelectionStateMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorSelectionStateMessage selStateMsg) {
                        _lastEditorSelectionStateMessage = selStateMsg;
                    }
                    break;

                case MpEditorBindingFunctionType.notifySubSelectionEnabledChanged:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillSubSelectionChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillSubSelectionChangedNotification subSelChangedNtf) {
#if !SUGAR_WV
                        ctvm.IsSubSelectionEnabled = subSelChangedNtf.isSubSelectionEnabled;
#endif
                    }
                    break;

                case MpEditorBindingFunctionType.notifyAnnotationSelected:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillAnnotationSelectedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillAnnotationSelectedMessage annSelectedMsg) {
                        BindingContext
                            .TransactionCollectionViewModel
                            .SelectChildCommand.Execute(new object[] { annSelectedMsg.annotationGuid, annSelectedMsg.isDblClick });
                    }
                    break;
                #endregion

                #region APPEND STATE

                case MpEditorBindingFunctionType.notifyAppendStateChanged:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillAppendStateChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillAppendStateChangedMessage appendStateChangedMsg) {
                        ProcessAppendStateChangedMessageAsync(appendStateChangedMsg, "editor").FireAndForgetSafeAsync();
                    }
                    break;

                case MpEditorBindingFunctionType.notifyAppendStateChangeComplete:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillAppendStateChangeCompletedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillAppendStateChangeCompletedMessage append_resp &&
                        !append_resp.appendContentHandle.IsNullOrEmpty() &&
                        append_resp.appendDocLength >= 0) {
                        // update appended range

                        MpAvClipTrayViewModel.Instance.AddOrUpdateRecentAppendInfo(append_resp.appendContentHandle, (append_resp.appendDocIdx, append_resp.appendDocLength));
                    }
                    _lastAppendStateChangeCompleteDt = DateTime.Now;
                    break;

                #endregion

                #region OLE

                case MpEditorBindingFunctionType.notifyDataObjectResponse:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillContentDataObjectResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentDataObjectResponseMessage) {
                        // GetContentDataObject blocks until _lastDataObjectResp is set
                        _lastDataObjectResp = ntf as MpQuillContentDataObjectResponseMessage;
                    }
                    break;

                #region CLIPBOARD

                case MpEditorBindingFunctionType.notifySetClipboardRequested:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillEditorSetClipboardRequestNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorSetClipboardRequestNotification setClipboardReq) {
                        ctvm.CopyToClipboardCommand.Execute(null);
                    }
                    break;
                case MpEditorBindingFunctionType.notifyPasteRequest:
                    MpAvClipTrayViewModel.Instance.PasteFromClipTilePasteButtonCommand.Execute(BindingContext);
                    break;

                #endregion

                #region DND

                case MpEditorBindingFunctionType.notifyDragEnter:
                    //if (MpAvClipTrayViewModel.Instance.AllActiveItems.FirstOrDefault(x => x.IsDropOverTile) is { } old_drop_ctvm &&
                    //    old_drop_ctvm != BindingContext &&
                    //    old_drop_ctvm.GetContentView() is MpAvContentWebView old_drop_cwv) {
                    //    var dndMsg = new MpQuillDragDropEventMessage() {
                    //        eventType = "dragleave"
                    //    };
                    //    old_drop_cwv.SendMessage($"dragEventFromHost_ext('{dndMsg.SerializeObjectToBase64()}')");
                    //}
                    BindingContext.IsDropOverTile = true;
                    break;
                case MpEditorBindingFunctionType.notifyDragLeave:
                    BindingContext.IsDropOverTile = false;
                    break;
                case MpEditorBindingFunctionType.notifyDragEnd:
                    BindingContext.IsDropOverTile = false;
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillDragEndMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillDragEndMessage dragEndMsg) {
                        WasDragCanceled = dragEndMsg.wasCancel;
                    }
                    break;

                case MpEditorBindingFunctionType.notifyDropCompleted:

                    BindingContext.IsDropOverTile = false;
                    BindingContext.IsSelected = true;
                    //MpAvClipTrayViewModel.Instance.SelectedItem = BindingContext;
                    break;

                #endregion

                #endregion

                #region HIGHLIGHTING

                case MpEditorBindingFunctionType.notifyFindReplaceVisibleChange:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillContentFindReplaceVisibleChanedNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentFindReplaceVisibleChanedNotificationMessage findReplaceMsgObj) {
                        ctvm.IsFindAndReplaceVisible = findReplaceMsgObj.isFindReplaceVisible;
                    }
                    break;
                case MpEditorBindingFunctionType.notifyQuerySearchRangesChanged:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillContentQuerySearchRangesChangedNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentQuerySearchRangesChangedNotificationMessage searchRangeCountMsg) {
                        // NOTE content highlight blocks until this is recv'd
                        SearchResponse = searchRangeCountMsg;
                    }
                    break;

                #endregion

                #region TEMPLATES

                case MpEditorBindingFunctionType.notifyAddOrUpdateTemplate:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillTemplateAddOrUpdateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillTemplateAddOrUpdateNotification addOrUpdateTemplateMsg) {
                        var t = MpJsonExtensions.DeserializeBase64Object<MpTextTemplate>(addOrUpdateTemplateMsg.addedOrUpdatedTextTemplateBase64JsonStr);
                        MpAvTemplateModelHelper.Instance
                            .AddUpdateOrDeleteTemplateAsync(BindingContext.CopyItemId, t, false)
                            .FireAndForgetSafeAsync();
                    }

                    break;
                case MpEditorBindingFunctionType.notifyUserDeletedTemplate:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillUserDeletedTemplateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillUserDeletedTemplateNotification deleteTemplateMsg) {
                        MpAvTemplateModelHelper.Instance
                            .AddUpdateOrDeleteTemplateAsync(BindingContext.CopyItemId, new MpTextTemplate() { Guid = deleteTemplateMsg.userDeletedTemplateGuid }, true)
                            .FireAndForgetSafeAsync();
                    }
                    break;

                #endregion

                #region PASTE INFO
                case MpEditorBindingFunctionType.notifyPasteInfoFormatsClicked:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillPasteInfoFormatsClickedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillPasteInfoFormatsClickedNotification pasteInfoFormatsClickedMsg) {
                        MpAvAppCollectionViewModel.Instance
                        .ShowAppPresetsContextMenuCommand.Execute(
                            new object[] {
                                this,
                                MpPortableProcessInfo.FromPath(pasteInfoFormatsClickedMsg.infoId),
                                new MpPoint(pasteInfoFormatsClickedMsg.offsetX,pasteInfoFormatsClickedMsg.offsetY),
                                "full"
                            });
                    }
                    break;
                #endregion

                #region WINDOW ACTIONS

                case MpEditorBindingFunctionType.notifyShowCustomColorPicker:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillShowCustomColorPickerNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowCustomColorPickerNotification showCustomColorPickerMsg) {
                        Dispatcher.UIThread.Post(async () => {
                            string pickerResult = await Mp.Services.CustomColorChooserMenuAsync.ShowCustomColorMenuAsync(
                                selectedColor: showCustomColorPickerMsg.currentHexColor,
                                title: showCustomColorPickerMsg.pickerTitle,
                                owner: null,
                                allowAlpha: true);

                            var resp = new MpQuillCustomColorResultMessage() {
                                customColorResult = pickerResult
                            };
                            SendMessage($"provideCustomColorPickerResult_ext('{resp.SerializeObjectToBase64()}')");
                        });
                    }
                    break;
                case MpEditorBindingFunctionType.notifyNavigateUriRequested:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillNavigateUriRequestNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillNavigateUriRequestNotification navUriReq) {
                        if (navUriReq.linkType == "hexcolor") {
                            Dispatcher.UIThread.Post(async () => {
                                string hex = navUriReq.linkText;
                                string result_hex = await Mp.Services.CustomColorChooserMenuAsync
                                    .ShowCustomColorMenuAsync(
                                        selectedColor: hex,
                                        title: UiStrings.EditorColorChooserTitle);

                                if (string.IsNullOrEmpty(result_hex)) {
                                    return;
                                }

                                var hex_delta = new MpQuillDelta() {
                                    ops = new List<MpQuillOp>() {
                                        new MpQuillOp() {
                                            retain = navUriReq.linkDocIdx
                                        },
                                        new MpQuillOp() {
                                            delete = navUriReq.linkText.Length
                                        },
                                        new MpQuillOp() {
                                            insert = result_hex
                                        }
                                    }
                                };
                                if (this is MpIContentView cv) {
                                    await cv.UpdateContentAsync(hex_delta);

                                }
                                // wait for delta to updated in editor
                                await Task.Delay(500);
                                // re-annotate with new hex

                                // TODO this is a good case for triggers to 
                                // be a collection of triggers and not 1 so
                                // core annotate trigger is applied on 
                                // new AND updated content
                                MpAvAnalyticItemCollectionViewModel.Instance
                                .ApplyCoreAnnotatorCommand.Execute(BindingContext);
                            });
                            return;
                        }
                        string uri_str = HttpUtility.HtmlDecode(navUriReq.uri);

                        MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(uri_str);
                    }
                    break;
                case MpEditorBindingFunctionType.notifyInternalContextMenuIsVisibleChanged:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillInternalContextIsVisibleChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillInternalContextIsVisibleChangedNotification ctxMenuChangedMsg) {
                        ctvm.CanShowContextMenu = !ctxMenuChangedMsg.isInternalContextMenuVisible;
                    }
                    break;
                case MpEditorBindingFunctionType.notifyInternalContextMenuCanBeShownChanged:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillInternalContextMenuCanBeShownChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillInternalContextMenuCanBeShownChangedNotification ctxMenuCanShowChangedMsg) {
                        ctvm.CanShowContextMenu = !ctxMenuCanShowChangedMsg.canInternalContextMenuBeShown;
                    }
                    break;
                case MpEditorBindingFunctionType.notifyPointerEvent:
                    ntf = MpJsonExtensions.DeserializeBase64Object<MpQuillPointerEventMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillPointerEventMessage pointerMsg) {

                        if (pointerMsg.eventType != "up" && pointerMsg.eventType != "down") {
                            var pe = MpAvPointerInputHelpers.CreatePointerEventArgs(
                                eventType: pointerMsg.eventType.ToEnum<MpPointerEventType>().ToRoutedEvent(),
                                interactive: this,
                                mp: new MpPoint(pointerMsg.clientX, pointerMsg.clientY),
                                KeyModifiers.None,
                                isLocalMp: true);
                            this.RaiseEvent(pe);
                            return;
                        }
                        if (this.GetVisualAncestor<MpAvClipTileView>() is not { } ctv ||
                            ctv.ClipTileContainerBorder == null) {
                            return;
                        }
                        var gmp = this.PointToScreen(new Point(pointerMsg.clientX, pointerMsg.clientY));
                        var mp = ctv.ClipTileContainerBorder.PointToClient(gmp).ToPortablePoint();
                        var pe2 = MpAvPointerInputHelpers.CreatePointerEventArgs(
                                eventType: pointerMsg.eventType.ToEnum<MpPointerEventType>().ToRoutedEvent(),
                                interactive: ctv.ClipTileContainerBorder,
                                mp: mp,
                                kmf: KeyModifiers.None,
                                isLocalMp: true,
                                isLeftButton: pointerMsg.isLeft);
                        ctv.ClipTileContainerBorder.RaiseEvent(pe2);

                    }
                    break;

                #endregion

                #region OTHER
                case MpEditorBindingFunctionType.notifyDevToolsRequested:
                    if (MpAvWindowManager.GetTopLevel(this) is MpAvWindow tl) {
                        tl.ShowDevTools();
                    }
                    break;
                #endregion

                #region GET CALLBACKS

                case MpEditorBindingFunctionType.getDragDataTransferObject:
                case MpEditorBindingFunctionType.getClipboardDataTransferObject:
                case MpEditorBindingFunctionType.getAllSharedTemplatesFromDb:
                case MpEditorBindingFunctionType.getContactsFromFetcher:
                case MpEditorBindingFunctionType.getMessageBoxResult:
                    HandleBindingGetRequest(notificationType, msgJsonBase64Str).FireAndForgetSafeAsync(ctvm);
                    break;

                    #endregion
            }

            //MpConsole.WriteLine($"Tile {ctvm} received cef notification type '{notificationType}' w/ msg:",true);
            //MpConsole.WriteLine($"'{(ntf == null ? "NO DATA RECEIVED":ntf.ToPrettyPrintJsonString())}'", false, true);
        }

        private async Task HandleBindingGetRequest(MpEditorBindingFunctionType getReqType, string msgJsonBase64) {
            var getReq = MpJsonExtensions.DeserializeBase64Object<MpQuillGetRequestNotification>(msgJsonBase64);
            var getResp = new MpQuillGetResponseNotification() { requestGuid = getReq.requestGuid };
            switch (getReqType) {
                case MpEditorBindingFunctionType.getAllSharedTemplatesFromDb:
                    var templateReq = MpJsonExtensions.DeserializeObject<MpQuillTemplateDbQueryRequestMessage>(getReq.reqMsgFragmentJsonStr);
                    var tl = await MpDataModelProvider.GetTextTemplatesByType(templateReq.templateTypes.Select(x => x.ToEnum<MpTextTemplateType>()));

                    getResp.responseFragmentJsonStr = MpJsonExtensions.SerializeObject(tl);
                    break;

                case MpEditorBindingFunctionType.getContactsFromFetcher:
                    var cl = await MpFetchPluginsDataProvider.GetContactsAsync();
                    getResp.responseFragmentJsonStr = MpJsonExtensions.SerializeObject(cl);
                    break;

                case MpEditorBindingFunctionType.getMessageBoxResult:
                    var msgBoxReq = MpJsonExtensions.DeserializeObject<MpQuillShowDialogRequestMessage>(getReq.reqMsgFragmentJsonStr);

                    object result = null;

                    switch (msgBoxReq.dialogType) {
                        case "okcancel":

                            result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                                title: msgBoxReq.title,
                                message: msgBoxReq.msg,
                                iconResourceObj: msgBoxReq.iconResourceObj);
                            break;
                    }

                    var msgBoxResp = new MpQuillShowDialogResponseMessage() {
                        dialogResponse = result.ToStringOrDefault()
                    };
                    getResp.responseFragmentJsonStr = MpJsonExtensions.SerializeObject(msgBoxResp);
                    break;
                case MpEditorBindingFunctionType.getClipboardDataTransferObject:
                    var cb_dtObjReq = MpJsonExtensions.DeserializeObject<MpQuillEditorClipboardDataObjectRequestNotification>(getReq.reqMsgFragmentJsonStr);
                    var cb_ido = await Mp.Services.DataObjectTools.ReadClipboardAsync(false) as IDataObject;
                    var cb_dtObjResp = cb_ido.ToQuillDataItemsMessage();
                    getResp.responseFragmentJsonStr = MpJsonExtensions.SerializeObject(cb_dtObjResp);
                    break;
                case MpEditorBindingFunctionType.getDragDataTransferObject:
                    // NOTE to avoid data object limitations of letting cef handle dnd when internal drag, 
                    // use full object to perform actual drop. Otherwise its only used for drag effect.
                    // If dnd was passive (simulating all drop events via messages) it would be asynchronous 
                    // which has lots of thread access issues

                    IDataObject processed_drag_avdo = MpAvDoDragDropWrapper.DragDataObject;
                    if (processed_drag_avdo == null) {
                        // external drag, use data from webview
                        var drag_dtObjReq = MpJsonExtensions.DeserializeObject<MpQuillEditorDragDataObjectRequestNotification>(getReq.reqMsgFragmentJsonStr);
                        var drag_hdo = MpJsonExtensions.DeserializeBase64Object<MpQuillHostDataItemsMessage>(drag_dtObjReq.unprocessedDataItemsJsonStr);

                        var unprocessed_drag_avdo = drag_hdo.ToAvDataObject();

                        MpPortableProcessInfo drag_pi = Mp.Services.DragProcessWatcher.DragProcess;

                        processed_drag_avdo = await Mp.Services
                            .DataObjectTools.ReadDataObjectAsync(unprocessed_drag_avdo, MpDataObjectSourceType.ClipTileDrop) as IDataObject;

                        if (drag_pi == null &&
                            processed_drag_avdo.TryGetData<MpPortableProcessInfo>(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, out var proc_drag_pi)) {
                            drag_pi = proc_drag_pi;
                        }
                        if (drag_pi != null) {
                            // convert source app to uri 
                            string drag_app_url = await Mp.Services.SourceRefTools.FetchOrCreateAppRefUrlAsync(drag_pi);
                            processed_drag_avdo.AddOrUpdateUri(drag_app_url);
                        }
                        if (BindingContext.CopyItemType == MpCopyItemType.FileList &&
                            processed_drag_avdo.TryGetData(MpPortableDataFormats.Files, out object fn_obj)) {
                            // NOTE for file drops files are converted to fragment and dropped like append handling
                            if (fn_obj is IEnumerable<string> fnl) {
                                var fl_frag = new MpQuillFileListDataFragment() {
                                    fileItems = fnl.Select(x => Uri.UnescapeDataString(x)).Select(x => new MpQuillFileListItemDataFragmentMessage() {
                                        filePath = x,
                                        fileIconBase64 =
                                            x.IsFileOrDirectory() ?
                                                ((Bitmap)MpAvStringFileOrFolderPathToBitmapConverter.Instance.Convert(x, null, null, null)).ToBase64String() :
                                                MpAvPrefViewModel.Instance.ThemeType == MpThemeType.Dark ?
                                                    MpBase64Images.MissingFile_white :
                                                    MpBase64Images.MissingFile
                                    }).ToList()
                                };
                                processed_drag_avdo.Set(MpPortableDataFormats.INTERNAL_FILE_LIST_FRAGMENT_FORMAT, fl_frag.SerializeObjectToBase64());
                            } else {
                                MpDebug.Break($"unhandled file obj type {fn_obj.GetType()}");
                            }
                        }
                    }


                    var processed_drag_hdo = processed_drag_avdo.ToQuillDataItemsMessage();
                    getResp.responseFragmentJsonStr = MpJsonExtensions.SerializeObject(processed_drag_hdo);
                    break;
            }

            if (string.IsNullOrEmpty(getResp.responseFragmentJsonStr)) {
                // no data to return
                return;
            }

            SendMessage($"getRequestResponse_ext('{getResp.SerializeObjectToBase64()}')");
        }

        #endregion

        #endregion

        #region Properties

        #region Web View

        #endregion


#if SUGAR_WV
        #region ReadOnlyWebView

        MpAvHtmlPanel _readOnlyWebView;
        public MpAvHtmlPanel ReadOnlyWebView {
            get {
                if (_readOnlyWebView == null &&
                    this.GetVisualAncestor<MpAvCompositeContentView>() is { } ccv) {
                    _readOnlyWebView = ccv.ReadOnlyWebView;
                }
                return _readOnlyWebView;
            }
        }


        #endregion
#endif

        #region View Models
        public MpAvClipTileViewModel BindingContext {
            get {
                if (DataContext is MpAvClipTileViewModel) {
                    return DataContext as MpAvClipTileViewModel;
                }
                if (DataContext is MpAvNotificationViewModelBase nvmb) {
                    return nvmb.Body as MpAvClipTileViewModel;
                }
                return null;
            }
        }

        #endregion

        #region State
        public MpQuillContentQuerySearchRangesChangedNotificationMessage SearchResponse { get; set; }

        public bool CanSendContentMessage =>
            IsEditorInitialized &&
            IsEditorLoaded;

        #region IsScrollWheelEnabled

        public static readonly StyledProperty<bool> IsScrollWheelEnabledProperty =
            AvaloniaProperty.Register<MpAvContentWebView, bool>(
                nameof(IsScrollWheelEnabled),
                defaultValue: true);

        public bool IsScrollWheelEnabled {
            get { return GetValue(IsScrollWheelEnabledProperty); }
            set { SetValue(IsScrollWheelEnabledProperty, value); }
        }
        #endregion

        #endregion

        #endregion

        #region Events
        public event EventHandler EditorInitialized;
        public event EventHandler ContentLoaded;
        #endregion

        #region Constructors

        public MpAvContentWebView() : base() {
            //this.SetDragHandler(new MpAvOutSysDragHandler(this));

            Address = Mp.Services.PlatformInfo.EditorPath.ToFileSystemUriFromPath();

            this.GetObservable(MpAvContentWebView.ContentScaleProperty).Subscribe(value => OnContentScaleChanged()).AddDisposable(this);
            this.GetObservable(MpAvContentWebView.IsEditorInitializedProperty).Subscribe(value => OnIsEditorInitializedChanged()).AddDisposable(this);
            this.GetObservable(MpAvContentWebView.ContentIdProperty).Subscribe(value => OnContentIdChanged()).AddDisposable(this);
            this.GetObservable(MpAvContentWebView.IsContentSelectedProperty).Subscribe(value => OnIsContentSelectedChanged()).AddDisposable(this);
            this.GetObservable(MpAvContentWebView.IsContentResizingProperty).Subscribe(value => OnIsContentResizingChanged()).AddDisposable(this);
            this.GetObservable(MpAvContentWebView.IsContentReadOnlyProperty).Subscribe(value => OnIsContentReadOnlyChanged()).AddDisposable(this);
            this.GetObservable(MpAvContentWebView.IsContentSubSelectableProperty).Subscribe(value => OnIsContentSubSelectableChanged()).AddDisposable(this);
            this.GetObservable(MpAvContentWebView.IsContentFindAndReplaceVisibleProperty).Subscribe(value => OnIsContentFindOrReplaceVisibleChanged()).AddDisposable(this);

#if SUGAR_WV
            //Background = Brushes.Transparent;

#endif
        }

#if CEFNET_WV

        //protected override void Dispose(bool disposing) {
        //    if (disposing &&
        //        BindingContext != null &&
        //        BindingContext.IsFinalClosingState) {
        //        // disposal handled in pop out closed handler after IsFinalClosingState reset
        //        return;
        //    }
        //    base.Dispose(disposing);
        //}
#else
        public override void OnNavigated(string url) {
            base.OnNavigated(url);
            if (MpUrlHelpers.IsBlankUrl(url)) {
                return;
            }
            LoadEditorAsync().FireAndForgetSafeAsync();
        }
#endif

        #endregion

        #region Public Methods
        public override string ToString() {
            return $"Address: '{Address.ToStringOrEmpty().SplitNoEmpty($"/").LastOrDefault()}' DataContext: '{DataContext}'";
        }

        private void ClearState() {
            DataContext = null;
            ContentId = 0;
            _lastContentHandle = null;
            IsDomLoaded = false;
            IsEditorInitialized = false;
            IsEditorLoaded = false;

        }
        public async Task ReloadAsync() {
            if (IsEditorLoaded) {
                SendMessage($"prepareForReload_ext()");
            }
#if CEFNET_WV
            await Task.Delay(1);
            try {
                ReloadIgnoreCache();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error reloading {this}.", ex);
            }
#else
            await LoadEditorAsync();
            await LoadContentAsync();
#endif

        }

        public async Task<MpQuillEditorSelectionStateMessage> GetSelectionStateAsync() {
            if (!this.IsAttachedToVisualTree() ||
                (BindingContext != null && BindingContext.CopyItemType == MpCopyItemType.Image)) {
                return new();
            }
            // NOTE resetting sel state is lazy upon request
            _lastEditorSelectionStateMessage = null;
            SendMessage($"selectionStateRequest_ext_ntf()");
            while (_lastEditorSelectionStateMessage == null) {
                await Task.Delay(100);
            }
            return _lastEditorSelectionStateMessage;
        }

        public void FinishDisposal() {
#if CEFNET_WV
            //this.Dispose(true);
#endif
        }
        #endregion

        #region Protected Methods
#if CEFNET_WV
        protected override void OnDragLeave(RoutedEventArgs e) {
            base.OnDragLeave(e);

            var dndMsg = new MpQuillDragDropEventMessage() {
                eventType = "dragleave"
            };
            SendMessage($"dragEventFromHost_ext('{dndMsg.SerializeObjectToBase64()}')");
        }
#endif


#if SUGAR_WV
        protected override void OnPointerEntered(PointerEventArgs e) {
            base.OnPointerEntered(e);
            //MpConsole.WriteLine($"Wv Pointer entered");
            if (BindingContext == null) {
                return;
            }
            BindingContext.IsHovering = true;
            BindingContext.IsContentHovering = true;
        }
        protected override void OnPointerMoved(PointerEventArgs e) {
            base.OnPointerMoved(e);

            if (BindingContext == null) {
                return;
            }
            BindingContext.IsHovering = true;
            BindingContext.IsContentHovering = true;
            //MpConsole.WriteLine($"Wv Pointer moved");
        }
        protected override void OnPointerExited(PointerEventArgs e) {
            base.OnPointerExited(e);
            //MpConsole.WriteLine($"Wv Pointer exited");

            if (BindingContext == null) {
                return;
            }
            BindingContext.IsDropOverTile = false;
            BindingContext.IsContentHovering = false;
            // BUG when this is triggered mp is still in the tile but tile won't
            // receive actual exit so only allow if exited to title panel
            //if (this.GetVisualAncestor<MpAvClipTileView>() is not { } ctv) {
            //    return;
            //}
            //BindingContext.IsHovering = ctv.IsPointerOver;
        }
#endif
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e) {
            if (!IsScrollWheelEnabled) {
                // disabled in no-select mode, otherwise cefnet swallows event and pin tray won't scroll
                return;
            }
            base.OnPointerWheelChanged(e);
        }
        protected virtual MpQuillInitMainRequestMessage GetInitMessage() {
            return new MpQuillInitMainRequestMessage() {
                envName = Mp.Services.PlatformInfo.OsType.ToString(),
                defaults = GetDefaultsMessage()
            };
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e) {
            base.OnAttachedToLogicalTree(e);
            Mp.Services.ContentViewLocator.AddView(this);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            LastPointerPressedEventArgs = e;
        }
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e) {
            base.OnDetachedFromLogicalTree(e);
            Mp.Services.ContentViewLocator.RemoveView(this);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e) {

            base.OnPropertyChanged(e);

            if (e.Property == IsEditorInitializedProperty &&
                IsEditorInitialized) {
                EditorInitialized?.Invoke(this, EventArgs.Empty);
                return;
            }
            if (e.Property == IsEditorLoadedProperty &&
                IsEditorLoaded) {
                ContentLoaded?.Invoke(this, EventArgs.Empty);
                return;
            }
        }
        #endregion

        #region Private Methods
        private void InitDnd() {

        }
        private MpQuillDefaultsRequestMessage GetDefaultsMessage() {
            return new MpQuillDefaultsRequestMessage() {
                internalSourceBaseUri = Mp.Services.SourceRefTools.InternalSourceBaseUri,
                minLogLevel = (int)MpConsole.MinLogLevel,
                isDebug = MpDebug.IsDebug,
                cultureCode = MpAvCurrentCultureViewModel.Instance.CurrentCulture.Name,
                isRightToLeft = MpAvPrefViewModel.Instance.IsTextRightToLeft,
                defaultFontFamily = MpAvPrefViewModel.Instance.DefaultEditableFontFamily,
                defaultFontSize = MpAvPrefViewModel.Instance.DefaultFontSize.ToString() + "px",
                isSpellCheckEnabled = MpAvPrefViewModel.Instance.IsSpellCheckEnabled,
                currentTheme =
                    MpAvPrefViewModel.Instance.ThemeType != MpThemeType.Dark ?
                        MpThemeType.Light.ToString().ToLowerInvariant() : MpThemeType.Dark.ToString().ToLowerInvariant(),
                bgOpacity = MpAvPrefViewModel.Instance.GlobalBgOpacity,
                maxUndo = MpAvPrefViewModel.Instance.MaxUndoLimit,
                shortcutFragmentStr = MpAvShortcutCollectionViewModel.Instance.EditorShortcutsMsgBase64,
                isDataTransferDestFormattingEnabled = MpAvPrefViewModel.Instance.IsDataTransferDestinationFormattingEnabled
            };
        }

        private MpQuillLoadContentRequestMessage GetLoadContentMessage(bool isSearchEnabled = true) {
            if (BindingContext == null) {
                return new MpQuillLoadContentRequestMessage() {
                    contentHandle = "<EMPTY CONTENT HANDLE>",
                    contentType = "Text",
                    itemData = string.Empty,
                    isReadOnly = true,
                    isSubSelectionEnabled = false
                };

            }

            var loadContentMsg = new MpQuillLoadContentRequestMessage() {
                isPopOut = BindingContext.IsWindowOpen,
                editorScale = ContentScale,
                contentId = BindingContext.CopyItemId,
                contentHandle = BindingContext.PublicHandle,
                contentType = BindingContext.CopyItemType.ToString(),
                itemData = BindingContext.EditorFormattedItemData,
                isReadOnly = BindingContext.IsContentReadOnly,
                isSubSelectionEnabled = BindingContext.IsSubSelectionEnabled,
                breakBeforeLoad = BreakOnNextLoad,
            };
            BreakOnNextLoad = false;


            if (isSearchEnabled) {
                var searches =
                    Mp.Services.Query.Infos
                        .Where(x => !string.IsNullOrEmpty(x.MatchValue) && x.QueryFlags.HasStringMatchFilterFlag())
                        .Select(x => new MpQuillContentSearchRequestMessage() {
                            searchText = x.MatchValue,
                            isCaseSensitive = x.QueryFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive),
                            isWholeWordMatch = x.QueryFlags.HasFlag(MpContentQueryBitFlags.WholeWord),
                            useRegEx = x.QueryFlags.HasFlag(MpContentQueryBitFlags.Regex),
                            matchType = x.QueryFlags.GetStringMatchType().ToString()
                        });

                loadContentMsg.searchesFragment =
                    searches.Any() ?
                    new MpQuillContentSearchesFragment() {
                        searches = searches.ToList()
                    }.SerializeObjectToBase64() : null;
            }

            loadContentMsg.appendStateFragment =
                BindingContext.IsAppendNotifier ?
                    MpAvClipTrayViewModel.Instance
                    .GetAppendStateMessage(null,null)
                    .SerializeObjectToBase64() : null;

            if (MpAvPersistentClipTilePropertiesHelper
                .TryGetPersistentSubSelectionState(
                    BindingContext.CopyItemId,
                    BindingContext.QueryOffsetIdx,
                    out var sel_state)) {
                loadContentMsg.selectionFragment = sel_state.SerializeObjectToBase64();
            }

            if (MpAvPersistentClipTilePropertiesHelper
                .IsPersistentIsSubSelectable_ById(BindingContext.CopyItemId, BindingContext.QueryOffsetIdx)) {
                loadContentMsg.pasteButtonInfoFragment = MpAvClipTrayViewModel.Instance.CurPasteInfoMessage.SerializeObjectToBase64();
            }

            return loadContentMsg;
        }
        #endregion

        #region Dom Init

        #region IsEditorInitialized Property

        private bool _isEditorInitialized = false;
        public bool IsEditorInitialized {
            get { return _isEditorInitialized; }
            protected set { SetAndRaise(IsEditorInitializedProperty, ref _isEditorInitialized, value); }
        }

        public static readonly DirectProperty<MpAvContentWebView, bool> IsEditorInitializedProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsEditorInitialized),
                x => x.IsEditorInitialized,
                (x, o) => x.IsEditorInitialized = o);

        protected virtual void OnIsEditorInitializedChanged() { }
        #endregion

        #region IsDomLoaded Property

        private bool _isDomLoaded = false;
        public bool IsDomLoaded {
            get { return _isDomLoaded; }
            set { SetAndRaise(IsDomLoadedProperty, ref _isDomLoaded, value); }
        }

        public static readonly DirectProperty<MpAvContentWebView, bool> IsDomLoadedProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsDomLoaded),
                x => x.IsDomLoaded,
                (x, o) => x.IsDomLoaded = o);

        #endregion
        public async Task LoadEditorAsync() {
            Dispatcher.UIThread.VerifyAccess();
#if CEFNET_WV || SUGAR_WV
            var sw = Stopwatch.StartNew();
            while (!IsDomLoaded) {
                // wait for Navigate(EditorPath)
                await Task.Delay(100);
                if (sw.ElapsedMilliseconds > 60_000) {
                    // BUG found this stuck here, i think it makes the ObjectDisposedException
                    // would be nice to infer state  but just letting it load to see if its ok
                    //MpDebug.Break($"editor timeout, should open its dev tools");
                    //IsDomLoaded = true;
                    //ShowDevTools();
                    MpDebug.BreakAll();
                    break;
                }
            }
            //MpConsole.WriteLine($"waited for domload: {sw.ElapsedMilliseconds}ms");
#else
            await Task.Delay(1);
#endif
            var req = GetInitMessage();
            SendMessage($"initMain_ext('{req.SerializeObjectToBase64()}')");
        }

        #endregion

        #region Content Life Cycle

        public bool NeedsEvalJsCleared { get; set; }

        #region ContentId Property

        private int _contentId;
        public int ContentId {
            get { return _contentId; }
            set { SetAndRaise(ContentIdProperty, ref _contentId, value); }
        }
        public static readonly DirectProperty<MpAvContentWebView, int> ContentIdProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, int>(
                nameof(ContentId),
                x => x.ContentId,
                (x, o) => x.ContentId = o,
                0);

        #endregion

        #region ContentScale Property

        private double _ContentScale;
        public double ContentScale {
            get { return _ContentScale; }
            set { SetAndRaise(ContentScaleProperty, ref _ContentScale, value); }
        }
        public static readonly DirectProperty<MpAvContentWebView, double> ContentScaleProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, double>(
                nameof(ContentScale),
                x => x.ContentScale,
                (x, o) => x.ContentScale = o,
                MpCopyItem.ZOOM_FACTOR_DEFAULT);

        private void OnContentScaleChanged() {
            if (!IsEditorLoaded) {
                return;
            }
            var req = new MpQuillEditorScaleChangedMessage() { editorScale = ContentScale };
            SendMessage($"setEditorZoom_ext('{req.SerializeObjectToBase64()}')");
        }
        #endregion

        #region IsContentLoaded Property

        private bool _isEditorLoaded;
        public bool IsEditorLoaded {
            get { return _isEditorLoaded; }
            set { SetAndRaise(IsEditorLoadedProperty, ref _isEditorLoaded, value); }
        }

        public static readonly DirectProperty<MpAvContentWebView, bool> IsEditorLoadedProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsEditorLoaded),
                x => x.IsEditorLoaded,
                (x, o) => x.IsEditorLoaded = o,
                false,
                BindingMode.TwoWay);


        #endregion

        private void OnContentIdChanged() {
            _lastContentHandle = null;
            if (BindingContext == null ||
                !BindingContext.IsContentReadOnly) {
                return;
            }
            LoadContentAsync().FireAndForgetSafeAsync();
        }

        public async Task<bool> PerformUpdateContentRequestAsync(object jsonObj) {
            Dispatcher.UIThread.VerifyAccess();
            while (!IsEditorInitialized || !IsEditorLoaded) {
                // likely to happen for new content that's been annotated
                await Task.Delay(100);
                if (BindingContext.IsAnyPlaceholder) {
                    // content was recycled before editor could load
                    // not sure this will happen but it will mean the 
                    // annotation won't ever be applied 
                    // SO if this does happen a way to fix would be either
                    // a) not setting transaction time until its applied and 
                    // applying null trans time transactions on init
                    // b) adding a WasApplied bit field to MpCopyItemTransaction 
                    // checking that on init

                    // either one isn't simple but maybe it won't happen
                    MpDebug.Break("apply transaction failure");
                    return false;
                }
            }
            var req = new MpQuillUpdateContentRequestMessage();
            if (jsonObj is MpQuillDelta) {
                req.deltaFragmentStr = jsonObj.SerializeObjectToBase64(true);
            } else if (jsonObj is MpAnnotationNodeFormat) {
                req.annotationFragmentStr = jsonObj.SerializeObjectToBase64(true);
            } else {
                // since there is nothing known to apply consider it successful
                return true;
            }
            SendMessage($"updateContents_ext('{req.SerializeObjectToBase64()}')");
            return true;
        }

        public async Task LoadContentAsync(bool isSearchEnabled = true) {
            Dispatcher.UIThread.VerifyAccess();
            IsEditorLoaded = false;

#if CEFNET_WV
            if (this is WebView wv && wv.PendingEvalCount() > 0 ||
                BindingContext == null) {
                this.NeedsEvalJsCleared = true;
                var sw = Stopwatch.StartNew();
                while (NeedsEvalJsCleared) {
                    await Task.Delay(100);
                    if (sw.ElapsedMilliseconds > 5_000) {
                        MpConsole.WriteLine($"Content web view '{this}' load content NeedEvalJsCleared timeout, unsetting and conitnuing ");
                        NeedsEvalJsCleared = false;
                        break;
                    }
                }
            }
#endif
            if (MpAvWindowManager.GetTopLevel(this) is MpAvWindow w &&
                w.Classes.Contains("content-window")) {

            }
            if (BindingContext == null) {
                // unloaded
                return;
            }

            if (BindingContext.IsAnyPlaceholder) {
                // no content
                return;
            }

            var sw2 = Stopwatch.StartNew();
            while (!IsDomLoaded) {
                await Task.Delay(100);
                if (sw2.Elapsed > TimeSpan.FromMilliseconds(5_000) && this is not MpAvPlainHtmlConverterWebView) {
                    // timeout
                    return;
                }
            }
            while (!IsEditorInitialized) {
                // wait for initMain - onInitComplete_ntf
                await Task.Delay(100);
                if (sw2.Elapsed > TimeSpan.FromMilliseconds(5_000) && this is not MpAvPlainHtmlConverterWebView) {
                    // timeout
                    return;
                }
            }
            if (BindingContext == null) {
                // unloaded
                return;
            }
            while (BindingContext.FileItemCollectionViewModel.IsAnyBusy) {
                // wait for file icons to populate from ctvm.CheckEnumUiStrings
                await Task.Delay(100);
            }

            var loadContentMsg = GetLoadContentMessage(isSearchEnabled);
            if (loadContentMsg.breakBeforeLoad) {
                OpenDevTools();
                await Task.Delay(5000);
            }
            string msgStr = loadContentMsg.SerializeObjectToBase64();

            SendMessage($"loadContentAsync_ext('{msgStr}')");
        }

        private void ProcessContentChangedMessage(MpQuillEditorContentChangedMessage contentChanged_ntf, bool isInitialLoad = false) {
            if (!IsEditorInitialized) {
                // BUG load stalls on reload while editing waiting for initialzing...
                IsEditorInitialized = true;
            }


            IsEditorLoaded = true;

            if (BindingContext == null ||
                BindingContext.IsAnyPlaceholder) {
                return;
            }
            if (BindingContext.IsAppendNotifier &&
                BindingContext.AppendCount == 0) {
                // don't set clipboard to append buffer until somethings actually appended
                // so it loads quicker
                return;
            }

            if (contentChanged_ntf == null) {
                // shouldn't be null
                MpDebug.Break($"Content changed resp was null");
                return;
            }
            if (BindingContext.PublicHandle != contentChanged_ntf.contentHandle) {
                // BUG pinning item from query tray right after closing a pop out
                // window used old window data and replaced new item's data with it
                var actual_contexts = MpAvClipTrayViewModel.Instance.AllItems.Where(x => x.PublicHandle == contentChanged_ntf.contentHandle);

                MpDebug.Break($"Content Handle mismatch for tile '{BindingContext}'. Ignoring model update.", true);
                return;
            }

            BindingContext.HasEditableTable = contentChanged_ntf.hasEditableTable;
            BindingContext.ActualContentHeight = contentChanged_ntf.contentHeight;
            BindingContext.CopyItemSize1 = contentChanged_ntf.itemSize1;
            BindingContext.CopyItemSize2 = contentChanged_ntf.itemSize2;

            if (BindingContext.HasTemplates != contentChanged_ntf.hasTemplates) {
                // find out if tile has templates lazy cause some are not in db
                BindingContext.HasTemplates = contentChanged_ntf.hasTemplates;
                if (BindingContext.HasTemplates && BindingContext.IsSubSelectionEnabled) {
                    // expand (if needed) for template toolbar stuff (meant to happen while editing really..)
                    GrowView();
                }
            }
            if (isInitialLoad) {
                // avoid initial load re-writes, only on subsequent content changes
                return;
            }

            BindingContext.IgnoreHasModelChanged = true;

            BindingContext.SearchableText = contentChanged_ntf.itemPlainText;

            if (contentChanged_ntf.itemData != null) {
                bool is_empty = contentChanged_ntf.itemData.IsNullOrWhitespaceHtmlString();
                if (is_empty &&
                    BindingContext.IsContentReadOnly &&
                    MpCopyItem.IS_EMPTY_HTML_CHECK_ENABLED) {
                    // data's getting reset again
                    MpDebug.Break("data reset caught in webview process content changed. ignoring update");
                } else {
                    BindingContext.CopyItemData = contentChanged_ntf.itemData;
                }
            }
            if (!string.IsNullOrWhiteSpace(contentChanged_ntf.dataTransferCompletedRespFragment) &&
                MpJsonExtensions.DeserializeBase64Object<MpQuillDataTransferCompletedNotification>(contentChanged_ntf.dataTransferCompletedRespFragment) is
                MpQuillDataTransferCompletedNotification dtcn) {
                ProcessDataTransferCompleteResponse(dtcn);
            }


            BindingContext.IgnoreHasModelChanged = false;

            if (BindingContext.IsAppendNotifier) {
                MpConsole.WriteLine("content changed on append");
                Dispatcher.UIThread.Post(async () => {
                    // sync append item to current clipboard
                    var append_mpdo = await GetDataObjectAsync(null, true);
                    await Mp.Services.DataObjectTools
                        .WriteToClipboardAsync(append_mpdo, true);
                });
            }
        }

        private void ProcessDataTransferCompleteResponse(MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
            if (BindingContext.IsAnyPlaceholder) {
                // occurs for edit
                return;
            }
            ProcessContentChangedMessage(
                MpJsonExtensions.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(
                    dataTransferCompleted_ntf.contentChangedMessageFragment));

            var dtobj = MpJsonExtensions.DeserializeBase64Object<MpQuillHostDataItemsMessage>(dataTransferCompleted_ntf.sourceDataItemsJsonStr);
            MpTransactionType transType = dataTransferCompleted_ntf.transferLabel.ToEnum<MpTransactionType>();
            MpPortableDataObject req_mpdo = dtobj.ToAvDataObject();
            BindingContext
                .TransactionCollectionViewModel
                .CreateTransactionFromOleOpCommand.Execute(new object[] { transType, req_mpdo });
        }

        #endregion

        #region Content State

        #region IsContentSelected

        private bool _isContentSelected;
        public bool IsContentSelected {
            get { return _isContentSelected; }
            set { SetAndRaise(IsContentSelectedProperty, ref _isContentSelected, value); }
        }

        public static readonly DirectProperty<MpAvContentWebView, bool> IsContentSelectedProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsContentSelected),
                x => x.IsContentSelected,
                (x, o) => x.IsContentSelected = o);


        private void OnIsContentSelectedChanged() {
            if (BindingContext == null ||
                !IsEditorLoaded //||
                                // MpAvFocusManager.Instance.IsInputControlFocused
                ) {
                return;
            }
            if (IsContentSelected && BindingContext.IsSubSelectionEnabled) {
                FocusEditor();
            }
        }


        #endregion

        #region IsContentResizing 

        private bool _isContentResizing;
        public bool IsContentResizing {
            get { return _isContentResizing; }
            set { SetAndRaise(IsContentResizingProperty, ref _isContentResizing, value); }
        }

        public static readonly DirectProperty<MpAvContentWebView, bool> IsContentResizingProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsContentResizing),
                x => x.IsContentResizing,
                (x, o) => x.IsContentResizing = o);

        private void OnIsContentResizingChanged() {
            if (BindingContext == null || !IsEditorLoaded) {
                return;
            }
            //if (IsContentResizing) {
            //    SendMessage($"disableWindowResizeUpdate_ext()");
            //} else {
            //    SendMessage($"enableWindowResizeUpdate_ext()"); ;
            //}
        }
        #endregion

        #region IsContentReadOnly 

        private bool _isContentReadOnly = true;
        public bool IsContentReadOnly {
            get { return _isContentReadOnly; }
            set { SetAndRaise(IsContentReadOnlyProperty, ref _isContentReadOnly, value); }
        }

        public static readonly DirectProperty<MpAvContentWebView, bool> IsContentReadOnlyProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsContentReadOnly),
                x => x.IsContentReadOnly,
                (x, o) => x.IsContentReadOnly = o,
                true,
                BindingMode.TwoWay);

        private void OnIsContentReadOnlyChanged() {
            if (BindingContext == null || !IsEditorLoaded) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                if (IsContentReadOnly) {
                    if (!BindingContext.IsWindowOpen) {
                        ShrinkView();
                    }
                    _lastReadOnlyEnabledFromHostResp = null;
                    SendMessage("enableReadOnly_ext_ntf()");
                    while (_lastReadOnlyEnabledFromHostResp == null) {
                        await Task.Delay(100);
                    }
                    MpQuillEditorContentChangedMessage resp = _lastReadOnlyEnabledFromHostResp;
                    _lastReadOnlyEnabledFromHostResp = null;
                    ProcessContentChangedMessage(resp);
                } else {
                    GrowView();
                    SendMessage($"disableReadOnly_ext()");
                    await Task.Delay(1000);
                    FocusEditor();
                }
            });
        }

        public void FocusEditor() {
            Dispatcher.UIThread.Post(async () => {
                var sw = Stopwatch.StartNew();
                while (!IsEditorLoaded) {
                    await Task.Delay(100);
                    if (sw.Elapsed > TimeSpan.FromSeconds(5)) {
                        MpConsole.WriteLine($"FocusEditor timeout reach, avoiding sending message");
                        return;
                    }
                }
                this.Focus();
                SendMessage($"hostIsFocusedChanged_ext('{new MpQuillIsHostFocusedChangedMessage() {
                    isHostFocused = true
                }.SerializeObjectToBase64()}')");
            });
        }
        #endregion

        #region IsContentSubSelectable 

        private bool _isContentSubSelectable;
        public bool IsContentSubSelectable {
            get { return _isContentSubSelectable; }
            set { SetAndRaise(IsContentSubSelectableProperty, ref _isContentSubSelectable, value); }
        }

        public static readonly DirectProperty<MpAvContentWebView, bool> IsContentSubSelectableProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsContentSubSelectable),
                x => x.IsContentSubSelectable,
                (x, o) => x.IsContentSubSelectable = o,
                false,
                BindingMode.TwoWay);

        private async void OnIsContentSubSelectableChanged() {
            if (BindingContext == null ||
                !IsEditorLoaded ||
                !IsContentReadOnly ||
                MpAvContentWebViewDragHelper.DragDataObject != null) {
                return;
            }

            if (!IsEditorInitialized && BindingContext.IsPlaceholder) {
                // it does happen (i think this is the other i case for pin/unpin thing)
                MpDebug.Break();
            }
            while (!IsEditorInitialized) {
                // when tile ispinned is toggled and was subselectable, need to let the new view catch up
                await Task.Delay(100);
            }

            if (IsContentSubSelectable) {
                SendMessage($"enableSubSelection_ext('{MpAvClipTrayViewModel.Instance.CurPasteInfoMessage.SerializeObjectToBase64()}')");
                if (//BindingContext.HasTemplates && 
                    !BindingContext.IsDropOverTile) {
                    GrowView();
                }
                await Task.Delay(300);
                FocusEditor();
            } else {
                SendMessage("disableSubSelection_ext()");
                ShrinkView();
            }
        }

        #endregion

        #region IsContentFindAndReplaceVisible Property

        private bool _isContentFindAndReplaceVisible;
        public bool IsContentFindAndReplaceVisible {
            get { return _isContentFindAndReplaceVisible; }
            set { SetAndRaise(IsContentFindAndReplaceVisibleProperty, ref _isContentFindAndReplaceVisible, value); }
        }

        public static readonly DirectProperty<MpAvContentWebView, bool> IsContentFindAndReplaceVisibleProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsContentFindAndReplaceVisible),
                x => x.IsContentFindAndReplaceVisible,
                (x, o) => x.IsContentFindAndReplaceVisible = o,
                false,
                BindingMode.TwoWay);

        private void OnIsContentFindOrReplaceVisibleChanged() {
            if (BindingContext == null ||
                !IsEditorLoaded) {
                return;
            }
            if (IsContentFindAndReplaceVisible) {
                SendMessage($"showFindAndReplace_ext()");
            } else {
                SendMessage($"hideFindAndReplace_ext()");
            }
        }


        #endregion

        #endregion

        #region Append

        public async Task ProcessAppendStateChangedMessageAsync(MpQuillAppendStateChangedMessage appendChangedMsg, string source) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(() => ProcessAppendStateChangedMessageAsync(appendChangedMsg, source));
                return;
            }
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var cur_append_tile = ctrvm.AppendClipTileViewModel;

            if (source == "editor") {
                if (cur_append_tile != null &&
                    cur_append_tile.CopyItemId != BindingContext.CopyItemId &&
                    (appendChangedMsg.isAppendInsertMode || appendChangedMsg.isAppendLineMode)) {
                    // new tile attempting to activate
                    Dispatcher.UIThread.Post(async () => {
                        int deactivated_ciid = ctrvm.AppendClipTileViewModel.CopyItemId;
                        // go through current tile deactivation and recall this to process
                        await ctrvm.DeactivateAppendModeCommand.ExecuteAsync();
                        await ProcessAppendStateChangedMessageAsync(appendChangedMsg, source);

                    });
                    return;
                }
                var append_cmdl = GetAppendChangeCommands(appendChangedMsg).ToList();
                if (append_cmdl.Count > 1) {
                    // check doubles, should be whats throwing toggling off
                }
                foreach (var cmd in append_cmdl) {
                    await cmd.ExecuteAsync();
                }

            } else {
                if (!CanSendContentMessage) {
                    // won't need to update state, its passed in init so ignore this case
                    return;
                }
                DateTime? lastStateComplete = _lastAppendStateChangeCompleteDt;
                SendMessage($"appendStateChanged_ext('{appendChangedMsg.SerializeObjectToBase64()}')");
                while (lastStateComplete == _lastAppendStateChangeCompleteDt) {
                    // wait for change to complete
                    await Task.Delay(100);
                }
            }
        }

        private IEnumerable<MpIAsyncCommand> GetAppendChangeCommands(MpQuillAppendStateChangedMessage appendChangedMsg) {
            //NOTE state changes should only come in one at a time
            // but line vs insert changes are only compared for is true since
            // they are interdependant to avoid double ntf

            var ctrvm = MpAvClipTrayViewModel.Instance;
            if (appendChangedMsg.isAppendPaused != ctrvm.IsAppendPaused) {
                yield return ctrvm.ToggleAppendPausedCommand;
            }
            if (appendChangedMsg.isAppendManualMode != ctrvm.IsAppendManualMode) {
                yield return ctrvm.ToggleAppendManualModeCommand;
            }
            if (appendChangedMsg.isAppendPreMode != ctrvm.IsAppendPreMode) {
                yield return ctrvm.ToggleAppendPreModeCommand;
            }
            if (appendChangedMsg.isAppendLineMode && !ctrvm.IsAppendLineMode) {
                yield return ctrvm.ToggleAppendLineModeCommand;
            }
            if (appendChangedMsg.isAppendInsertMode && !ctrvm.IsAppendInsertMode) {
                yield return ctrvm.ToggleAppendInsertModeCommand;
            }
            // NOTE make sure deactivate is returned last or it maybe be overriden 
            if (BindingContext.IsAppendNotifier &&
                    (!appendChangedMsg.isAppendLineMode && !appendChangedMsg.isAppendInsertMode && ctrvm.IsAnyAppendMode)) {
                yield return ctrvm.DeactivateAppendModeCommand;
            }
        }
        #endregion

        #region Dnd
        public void RelayDndMsg(MpDragDropOpType eventType, RoutedEventArgs e, IDataObject ido = default, PixelPoint gmp = default) {
            if (eventType == MpDragDropOpType.reset) {
                SendMessage($"resetDragAndDrop_ext()");
                return;
            }
            string frag = string.Empty;
            Point mp = new Point();

            if (BindingContext == null ||
                !BindingContext.IsEditorLoaded ||
                this.GetVisualDescendant<MpAvContentWebView>() is not { } wv) {
                return;
            }

            if (e is DragEventArgs dea) {
                //e.Handled = true;
                frag = dea.Data.ToQuillDataItemsMessage(dde: DragDropEffects.Copy).SerializeObjectToBase64();
                mp = dea.GetPosition(wv);
            } else if (e is PointerEventArgs pea) {
                mp = pea.GetPosition(wv);
            }
            if (ido != default && frag.IsNullOrEmpty()) {
                frag = ido.ToQuillDataItemsMessage(dde: DragDropEffects.Copy).SerializeObjectToBase64();
            }
            if (gmp != default) {
                mp = this.PointToClient(gmp);
            }
            var dndMsg = new MpQuillDragDropEventMessage() {

                eventType = eventType.ToString(),
                dataItemsFragment = frag,
                screenX = mp.X,
                screenY = mp.Y,
                shiftKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown,
                ctrlKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                altKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown,
                metaKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsMetaDown,
                escKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown,
            };
            wv.SendMessage($"dragEventFromHost_ext('{dndMsg.SerializeObjectToBase64()}')");
        }
        #endregion
        private void GrowView() {
            
            double nw = Math.Max(BindingContext.DesiredWidth, BindingContext.BoundWidth);
            double nh = Math.Max(BindingContext.BoundHeight, BindingContext.BoundHeight); //no change
            Resize(nw, nh);
        }
        private void ShrinkView() {
            double nw = Math.Min(BindingContext.DesiredWidth, BindingContext.BoundWidth);
            double nh = Math.Min(BindingContext.MinHeight, BindingContext.BoundHeight);
            Resize(nw, nh);
        }
        private void Resize(double nw, double nh) {
            // NOTE trying to isolate this cause persistent size gets lost
            // keeping animation smooth so waiting till end to make sure its set

            if (BindingContext.IsWindowOpen &&
                MpAvWindowManager.GetTopLevel(this) is MpAvWindow w &&
                w.Screens.ScreenFromPoint(w.Position) is Screen ws) {
                // for pop out keep it on its screen
                double pd = this.VisualPixelDensity();
                var nss = new MpSize(nw, nh).ToAvPixelSize(pd);
                var nsr = new PixelRect(w.Position, nss);
                var wr = ws.WorkingArea;
                int nx = w.Position.X;
                int ny = w.Position.Y;
                if (!wr.Contains(nsr)) {
                    if (nsr.X < wr.X) {
                        nx = wr.X;
                    }
                    if (nsr.Right > wr.Right) {
                        nx = wr.Right - nsr.Width;
                    }
                    if (nsr.Y < wr.Y) {
                        ny = wr.Y;
                    }
                    if (nsr.Bottom > wr.Bottom) {
                        ny = wr.Bottom - nsr.Height;
                    }
                    w.Position = new PixelPoint(nx, ny);
                }
            }
            MpAvResizeExtension.ResizeAnimated(this, nw, nh, () => {
                if (BindingContext == null) {
                    return;
                }
                BindingContext.StoreSelectionStateCommand.Execute(null);
            });
        }

    }

#if OUTSYS_WV
    //public class MpAvOutSysDragHandler : DragHandler {
    //    private MpAvContentWebView _wv;
    //    public MpAvOutSysDragHandler(MpAvContentWebView wv) {
    //        _wv = wv;
    //    }
    //    protected override bool OnDragEnter(CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask) {
    //        if (_wv == null ||
    //            !_wv.IsEditorLoaded) {
    //            return false;
    //        }

    //        var dndMsg = new MpQuillDragDropEventMessage() {
    //            eventType = "dragenter",
    //            dataItemsFragment = new MpAvDataObject(MpPortableDataFormats.Text, "TEST").ToQuillDataItemsMessage().SerializeObjectToBase64()
    //        };
    //        _wv.SendMessage($"dragEventFromHost_ext('{dndMsg.SerializeObjectToBase64()}')");
    //        return false;
    //    }

    //    protected override void OnDraggableRegionsChanged(CefBrowser browser, CefFrame frame, CefDraggableRegion[] regions) {

    //    }
    //}
#endif
}
