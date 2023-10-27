using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
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
using Avalonia.Layout;
using Avalonia.Platform;

#if DESKTOP

#if PLAT_WV
        using AvaloniaWebView; 
#elif CEF_WV

using CefNet.Avalonia;
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
        MpAvIReloadableContentWebView,
        MpAvIWebViewBindingResponseHandler {


        #region Private Variables
        private object _sendMessageLock = new object();
        private string _contentScreenShotBase64_ntf { get; set; }

        //private string _lastLoadedContentHandle = null;

        private DateTime? _locatedDateTime;

        private MpQuillContentDataObjectResponseMessage _lastDataObjectResp = null;
        private MpQuillEditorContentChangedMessage _lastReadOnlyEnabledFromHostResp = null;
        private MpQuillEditorSelectionStateMessage _lastEditorSelectionStateMessage = null;
        private DateTime? _lastAppendStateChangeCompleteDt = null;

        #endregion

        #region Constants
        #endregion

        #region Statics
        public static bool BreakOnNextLoad = false;

        #endregion

        #region Interfaces

        #region MpIWebView Implementation
#if !DESKTOP
        public override MpAvIWebViewBindingResponseHandler BindingHandler =>
            this;
#endif

        #endregion

        #region MpIJsonMessenger Implementation
        public void SendMessage(string msgJsonBase64Str) {
#if DESKTOP
#if PLAT_WV
            Dispatcher.UIThread.Post(async () => {
                var result = await WebView.ExecuteScriptAsync(msgJsonBase64Str);
            }); 
#elif CEF_WV
            this.ExecuteJavascript(msgJsonBase64Str);
#endif
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
        //#if DESKTOP
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
        void MpIHasDevTools.ShowDevTools() =>
#if PLAT_WV
        WebView.OpenDevToolsWindow(); 
#else
            ShowDevTools();
#endif

        bool MpIContentView.IsSubSelectable =>
            IsContentSubSelectable;


        Task<bool> MpIContentView.UpdateContentAsync(MpJsonObject contentJsonObj) =>
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

        public PointerEventArgs LastPointerPressedEventArgs { get; private set; }

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
            SendMessage($"updateModifierKeysFromHost_ext('{modKeyMsg.SerializeJsonObjectToBase64()}')");
        }

        public async Task<MpAvDataObject> GetDataObjectAsync(
            string[] formats = null,
            bool use_placeholders = false,
            bool ignore_selection = false) {
            if (BindingContext == null ||
                BindingContext.IsAnyPlaceholder) {
                MpDebug.Break();
                return new MpAvDataObject();
            }
            if (use_placeholders && !MpAvExternalDropWindowViewModel.Instance.IsDropWidgetEnabled) {
                use_placeholders = false;
            }

            var ctvm = BindingContext;
            // clear screenshot
            _contentScreenShotBase64_ntf = null;

            var contentDataReq = new MpQuillContentDataObjectRequestMessage() {
                // 'forOle' not the best name 
                selectionOnly = !ignore_selection
            };

            // NOTE when file is on clipboard pasting into tile removes all other formats besides file
            // and pseudo files are only needed for dnd comptaibility so its gewd
            if (formats == null) {
                // NOTE important that ALL data formats are on clipboard for drag source obj to process 
                contentDataReq.formats = MpPortableDataFormats.RegisteredFormats.ToList();
            } else {
                contentDataReq.formats = formats.ToList();
            }

            bool provided_formats = formats != null;
            bool ignore_pseudo_file = false;
            bool ignore_ss = true;

            if (ctvm.CopyItemType != MpCopyItemType.Image &&
                contentDataReq.formats.Contains(MpPortableDataFormats.AvPNG) /*&& use_placeholders*/) {
                ignore_ss = false;
            }
            if (ctvm.CopyItemType != MpCopyItemType.FileList &&
                provided_formats &&
                !formats.Contains(MpPortableDataFormats.AvFiles)) {
                ignore_pseudo_file = true;
            }

            if (ignore_ss && !provided_formats) {
                contentDataReq.formats.Remove(MpPortableDataFormats.AvPNG);
                contentDataReq.formats.Remove(MpPortableDataFormats.WinBitmap);
                contentDataReq.formats.Remove(MpPortableDataFormats.WinDib);
            }

            _lastDataObjectResp = null;
            SendMessage($"contentDataObjectRequestAsync_ext_ntf('{contentDataReq.SerializeJsonObjectToBase64()}')");
            while (_lastDataObjectResp == null) {
                // wait for binding handler to receive 
                await Task.Delay(100);
            }

            // store resp and immediabtly clear msg obj
            MpQuillContentDataObjectResponseMessage contentDataResp = _lastDataObjectResp;
            _lastDataObjectResp = null;

            if (contentDataResp.dataItems == null) {
                return null;
            }
            var avdo = new MpAvDataObject();
            foreach (var di in contentDataResp.dataItems) {
                avdo.SetData(di.format, di.data);
            }

            if (ctvm.CopyItemType == MpCopyItemType.FileList) {
                if (contentDataReq.formats.Contains(MpPortableDataFormats.AvFiles)) {
                    if (avdo.TryGetData(MpPortableDataFormats.AvFiles, out string fp_str) &&
                        !string.IsNullOrWhiteSpace(fp_str)) {
                        // file data is csv using DefaultCsvProps seperated by rows which should be envNewLine

                        avdo.SetData(MpPortableDataFormats.AvFiles, fp_str.SplitNoEmpty(MpCopyItem.FileItemSplitter));
                    } else {

                        avdo.SetData(MpPortableDataFormats.AvFiles, ctvm.CopyItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter));
                    }
                }
            } else if (!ignore_pseudo_file) {
                // NOTE setting dummy file so OLE system sees format on clipboard, actual
                // data is overwritten in core clipboard handler
                if (use_placeholders) {
                    avdo.SetData(MpPortableDataFormats.AvFiles, new[] { MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT });
                } else {
                    // NOTE presumes Text is txt and Image is png
                    // get unique pseudo-file path for whole or partial content
                    bool is_fragment = ctvm.CopyItemType == MpCopyItemType.Text && !contentDataResp.isAllContent ? true : false;
                    string ctvm_fp = ctvm.CopyItem.GetDefaultFilePaths(isFragment: is_fragment).FirstOrDefault();
                    string ctvm_data = is_fragment ? avdo.GetData(MpPortableDataFormats.Text) as string : ctvm.CopyItemData;
                    avdo.SetData(
                        MpPortableDataFormats.AvFiles,
                        new[] { ctvm_fp });
                    ctvm_data.ToFile(forcePath: ctvm_fp);
                }
            }

            bool is_full_content = ctvm.CopyItemType == MpCopyItemType.Image || contentDataResp.isAllContent;
            avdo.AddContentReferences(ctvm.CopyItem, is_full_content);

            if (ctvm.CopyItemType == MpCopyItemType.Image &&
                    ctvm.CopyItemData.ToAvBitmap() is Bitmap bmp) {

                avdo.SetData(MpPortableDataFormats.AvPNG, bmp.ToByteArray());
                avdo.SetData(MpPortableDataFormats.Text, bmp.ToAsciiImage());
                // TODO add colorized ascii maybe as html and rtf!!
            } else if (!ignore_ss) {
                //if (use_placeholders) {
                //    avdo.SetData(MpPortableDataFormats.AvPNG, MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT.ToBytesFromString());

                //    Dispatcher.UIThread.Post(async () => {
                //        await SetScreenShotAsync(avdo);
                //    });
                //} else {
                //    await SetScreenShotAsync(avdo);
                //}
            }

            await avdo.MapAllPseudoFormatsAsync();
            // remove all empty formats (workaround for cefnet bug w/ empty asciiUrl
            avdo.DataFormatLookup.Where(x => x.Value == null)
                .ForEach(x => avdo.DataFormatLookup.Remove(x.Key));
            return avdo;
        }

        private async Task SetScreenShotAsync(MpAvDataObject avdo) {
            bool timed_out = false;
            int ss_timeout = 15_000;
            var ss_sw = Stopwatch.StartNew();
            // screen shot is async and js notifies w/ base64 property here
            while (_contentScreenShotBase64_ntf == null) {
                await Task.Delay(100);
                if (ss_sw.ElapsedMilliseconds >= ss_timeout) {
                    MpConsole.WriteLine("screen shot timed out :(");
                    timed_out = true;
                    break;
                }
            }
            if (!timed_out) {
                if (_contentScreenShotBase64_ntf.ToBytesFromBase64String() is byte[] ss_bytes) {

                    MpConsole.WriteLine("screen shot set. byte count: " + ss_bytes.Length);
                    avdo.Set(MpPortableDataFormats.AvPNG, ss_bytes);
                }
            }
        }
        public bool IsCurrentDropTarget => BindingContext == null ? false : BindingContext.IsDropOverTile;

        #endregion

        #region MpAvIWebViewBindingResponseHandler Implementation

        private string _lastContentHandle { get; set; } = null;
        public virtual void HandleBindingNotification(
            MpEditorBindingFunctionType notificationType,
            string msgJsonBase64Str,
            string contentHandle) {
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
            MpJsonObject ntf = null;
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
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage loadComplete_ntf) {
                        //_lastLoadedContentHandle = contentHandle
                        IsEditorLoaded = true;
                        ProcessContentChangedMessage(loadComplete_ntf, is_new_content);
                    }
                    break;

                #endregion

                #region CONTENT CHANGED

                case MpEditorBindingFunctionType.notifyReadOnlyDisabled:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillDisableReadOnlyResponseMessage disableReadOnlyMsg) {
                        ctvm.IsContentReadOnly = false;
                        //ctvm.UnconstrainedContentDimensions = new MpSize(disableReadOnlyMsg.editorWidth, disableReadOnlyMsg.editorHeight);
                    }
                    break;
                case MpEditorBindingFunctionType.notifyReadOnlyEnabled:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage enableReadOnlyMsg) {
                        // NOTE only difference from contentChanged is no dimension info and this needs to enable readonly
                        ctvm.IsContentReadOnly = true;
                        ProcessContentChangedMessage(enableReadOnlyMsg);
                    }
                    break;

                case MpEditorBindingFunctionType.notifyReadOnlyEnabledFromHost:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage enableReadOnlyFromHostMsg) {
                        _lastReadOnlyEnabledFromHostResp = enableReadOnlyFromHostMsg;
                    }
                    break;
                case MpEditorBindingFunctionType.notifyContentChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage contentChanged_ntf) {
                        ProcessContentChangedMessage(contentChanged_ntf);
                        //RelayMsg($"contentChanged_ext('{msgJsonBase64Str}')");
                    }
                    break;
                case MpEditorBindingFunctionType.notifyDataTransferCompleted:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillDataTransferCompletedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
                        ProcessDataTransferCompleteResponse(dataTransferCompleted_ntf);
                    }
                    break;
                case MpEditorBindingFunctionType.notifyLastTransactionUndone:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillLastTransactionUndoneNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillLastTransactionUndoneNotification lastTransUndone_ntf) {
                        BindingContext
                            .TransactionCollectionViewModel
                            .RemoveMostRecentTransactionCommand.Execute(null);
                    }
                    break;

                #endregion

                #region LAYOUT

                case MpEditorBindingFunctionType.notifyScrollBarVisibilityChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillOverrideScrollNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillOverrideScrollNotification scrollbarVisibleMsg) {
                        MpAvClipTrayViewModel.Instance.IsScrollDisabled = scrollbarVisibleMsg.canScrollY;
                    }
                    break;

                case MpEditorBindingFunctionType.notifyShowToolTip:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillShowToolTipNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowToolTipNotification showToolTipNtf) {
                        if (showToolTipNtf.isVisible && !IsDragging) {
                            AvToolTip.SetTip(this, null);
                            var tt = new MpAvToolTipView();
                            tt.ToolTipText = showToolTipNtf.tooltipText;
                            tt.ToolTipHtml = showToolTipNtf.tooltipHtml;
                            tt.InputGestureText = showToolTipNtf.gestureText;
                            if (!tt.HasText) {
                                return;
                            }

                            void tt_attachedToView(object sender, EffectiveViewportChangedEventArgs e) {
                                // anchor is some editor elm's center
                                // move tooltip towards editor center along line from center to anchor
                                // to avoid tooltip overlapping mouse which creates a stutter from
                                // leave/exit calls

                                var tl = TopLevel.GetTopLevel(tt);
                                var tt_size = tl.Bounds.Size.ToPortableSize();
                                if (tt_size.IsEmpty()) {
                                    return;
                                }
                                MpPoint center_p = this.Bounds.Center.ToPortablePoint();
                                //MpPoint anchor_p = new MpPoint(showToolTipNtf.anchorX, showToolTipNtf.anchorY);
                                //// adj tooltip to be its height + pad away from anchor
                                //double adj_pad = 10;
                                //double adj_dist = (tt_size.Height / 2) + adj_pad;
                                //MpPoint center_to_anchor = anchor_p - center_p;
                                //double anchor_dist = center_to_anchor.Length;
                                //if (anchor_dist <= Math.Abs(adj_dist)) {
                                //    // anchor is IN the center so flip adj_dist out along line
                                //    adj_dist *= -1;
                                //}
                                //double offset_dist = anchor_dist - adj_dist;
                                //MpPoint adj_offset = center_to_anchor.Normalized * offset_dist;
                                //MpPoint adj_p = anchor_p - adj_offset;
                                //adj_p -= (tt_size.ToPortablePoint() * 0.5);
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
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorSelectionStateMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorSelectionStateMessage selStateMsg) {
                        _lastEditorSelectionStateMessage = selStateMsg;
                    }
                    break;

                case MpEditorBindingFunctionType.notifySubSelectionEnabledChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillSubSelectionChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillSubSelectionChangedNotification subSelChangedNtf) {
                        ctvm.IsSubSelectionEnabled = subSelChangedNtf.isSubSelectionEnabled;
                    }
                    break;

                case MpEditorBindingFunctionType.notifyAnnotationSelected:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillAnnotationSelectedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillAnnotationSelectedMessage annSelectedMsg) {
                        BindingContext
                            .TransactionCollectionViewModel
                            .SelectChildCommand.Execute(annSelectedMsg.annotationGuid);
                    }
                    break;
                #endregion

                #region APPEND STATE

                case MpEditorBindingFunctionType.notifyAppendStateChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillAppendStateChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillAppendStateChangedMessage appendStateChangedMsg) {
                        ProcessAppendStateChangedMessageAsync(appendStateChangedMsg, "editor").FireAndForgetSafeAsync();
                    }
                    break;

                case MpEditorBindingFunctionType.notifyAppendStateChangeComplete:
                    _lastAppendStateChangeCompleteDt = DateTime.Now;
                    break;

                #endregion

                #region OLE

                case MpEditorBindingFunctionType.notifyDataObjectResponse:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillContentDataObjectResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentDataObjectResponseMessage) {
                        // GetContentDataObject blocks until _lastDataObjectResp is set
                        _lastDataObjectResp = ntf as MpQuillContentDataObjectResponseMessage;
                    }
                    break;

                #region CLIPBOARD

                case MpEditorBindingFunctionType.notifySetClipboardRequested:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorSetClipboardRequestNotification>(msgJsonBase64Str);
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
                    BindingContext.IsDropOverTile = true;
                    break;
                case MpEditorBindingFunctionType.notifyDragLeave:
                    BindingContext.IsDropOverTile = false;
                    break;
                case MpEditorBindingFunctionType.notifyDragEnd:
                    BindingContext.IsDropOverTile = false;
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillDragEndMessage>(msgJsonBase64Str);
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
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillContentFindReplaceVisibleChanedNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentFindReplaceVisibleChanedNotificationMessage findReplaceMsgObj) {
                        ctvm.IsFindAndReplaceVisible = findReplaceMsgObj.isFindReplaceVisible;
                    }
                    break;
                case MpEditorBindingFunctionType.notifyQuerySearchRangesChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillContentQuerySearchRangesChangedNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentQuerySearchRangesChangedNotificationMessage searchRangeCountMsg) {
                        // NOTE content highlight blocks until this is recv'd
                        SearchResponse = searchRangeCountMsg;
                    }
                    break;

                case MpEditorBindingFunctionType.notifyContentScreenShot:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillContentScreenShotNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentScreenShotNotificationMessage ssMsg) {
                        _contentScreenShotBase64_ntf = ssMsg.contentScreenShotBase64;
                    }
                    break;

                #endregion

                #region TEMPLATES

                case MpEditorBindingFunctionType.notifyAddOrUpdateTemplate:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillTemplateAddOrUpdateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillTemplateAddOrUpdateNotification addOrUpdateTemplateMsg) {
                        var t = MpJsonConverter.DeserializeBase64Object<MpTextTemplate>(addOrUpdateTemplateMsg.addedOrUpdatedTextTemplateBase64JsonStr);
                        MpAvTemplateModelHelper.Instance
                            .AddUpdateOrDeleteTemplateAsync(BindingContext.CopyItemId, t, false)
                            .FireAndForgetSafeAsync();
                    }

                    break;
                case MpEditorBindingFunctionType.notifyUserDeletedTemplate:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillUserDeletedTemplateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillUserDeletedTemplateNotification deleteTemplateMsg) {
                        MpAvTemplateModelHelper.Instance
                            .AddUpdateOrDeleteTemplateAsync(BindingContext.CopyItemId, new MpTextTemplate() { Guid = deleteTemplateMsg.userDeletedTemplateGuid }, true)
                            .FireAndForgetSafeAsync();
                    }
                    break;

                #endregion

                #region PASTE INFO
                case MpEditorBindingFunctionType.notifyPasteInfoFormatsClicked:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillPasteInfoFormatsClickedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillPasteInfoFormatsClickedNotification pasteInfoFormatsClickedMsg) {
                        MpAvAppCollectionViewModel.Instance
                        .ShowAppPresetsContextMenuCommand.Execute(
                            new object[] {
                                this,
                                MpPortableProcessInfo.Create(pasteInfoFormatsClickedMsg.infoId),
                                new MpPoint(pasteInfoFormatsClickedMsg.offsetX,pasteInfoFormatsClickedMsg.offsetY),
                                "full"
                            });
                    }
                    break;
                #endregion

                #region WINDOW ACTIONS

                case MpEditorBindingFunctionType.notifyShowCustomColorPicker:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillShowCustomColorPickerNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowCustomColorPickerNotification showCustomColorPickerMsg) {
                        Dispatcher.UIThread.Post(async () => {

                            if (string.IsNullOrWhiteSpace(showCustomColorPickerMsg.pickerTitle)) {
                                // editor should provide title for templates but for content set to title here if ya want (may
                                showCustomColorPickerMsg.pickerTitle = $"Pick a color, any color for '{ctvm.CopyItemTitle}'";
                            }
                            string pickerResult = await Mp.Services.CustomColorChooserMenuAsync.ShowCustomColorMenuAsync(
                                selectedColor: showCustomColorPickerMsg.currentHexColor,
                                title: showCustomColorPickerMsg.pickerTitle,
                                owner: null,
                                allowAlpha: true);

                            var resp = new MpQuillCustomColorResultMessage() {
                                customColorResult = pickerResult
                            };
                            SendMessage($"provideCustomColorPickerResult_ext('{resp.SerializeJsonObjectToBase64()}')");
                        });
                    }
                    break;
                case MpEditorBindingFunctionType.notifyNavigateUriRequested:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillNavigateUriRequestNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillNavigateUriRequestNotification navUriReq) {
                        if (navUriReq.linkType == "hexcolor") {
                            Dispatcher.UIThread.Post(async () => {
                                string hex = navUriReq.linkText;
                                string result_hex = await Mp.Services.CustomColorChooserMenuAsync
                                    .ShowCustomColorMenuAsync(
                                        selectedColor: hex,
                                        title: "Editor color");

                                if (string.IsNullOrEmpty(result_hex)) {
                                    return;
                                }

                                var hex_delta = new MpQuillDelta() {
                                    ops = new List<Op>() {
                                        new Op() {
                                            retain = navUriReq.linkDocIdx
                                        },
                                        new Op() {
                                            delete = navUriReq.linkText.Length
                                        },
                                        new Op() {
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
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillInternalContextIsVisibleChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillInternalContextIsVisibleChangedNotification ctxMenuChangedMsg) {
                        ctvm.CanShowContextMenu = !ctxMenuChangedMsg.isInternalContextMenuVisible;
                    }
                    break;
                case MpEditorBindingFunctionType.notifyInternalContextMenuCanBeShownChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillInternalContextMenuCanBeShownChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillInternalContextMenuCanBeShownChangedNotification ctxMenuCanShowChangedMsg) {
                        ctvm.CanShowContextMenu = !ctxMenuCanShowChangedMsg.canInternalContextMenuBeShown;
                    }
                    break;
                #endregion

                #region OTHER

                case MpEditorBindingFunctionType.notifyShowDebugger:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillShowDebuggerNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowDebuggerNotification showDebugNtf) {
                        MpConsole.WriteLine($"[{ctvm}] {showDebugNtf.reason}");
                        this.ShowDevTools();
                    }
                    break;
                case MpEditorBindingFunctionType.notifyException:
#if DEBUG
                    ShowDevTools();
#endif
                    MpDebug.Break($"{ctvm} editor exception");
                    //ntf = MpJsonConverter.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    //if (ntf is MpQuillExceptionMessage exceptionMsgObj) {
                    //    MpConsole.WriteLine($"[{ctvm}] {exceptionMsgObj}");

                    //}
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
            var getReq = MpJsonConverter.DeserializeBase64Object<MpQuillGetRequestNotification>(msgJsonBase64);
            var getResp = new MpQuillGetResponseNotification() { requestGuid = getReq.requestGuid };
            switch (getReqType) {
                case MpEditorBindingFunctionType.getAllSharedTemplatesFromDb:
                    var templateReq = MpJsonConverter.DeserializeObject<MpQuillTemplateDbQueryRequestMessage>(getReq.reqMsgFragmentJsonStr);
                    var tl = await MpDataModelProvider.GetTextTemplatesByType(templateReq.templateTypes.Select(x => x.ToEnum<MpTextTemplateType>()));

                    getResp.responseFragmentJsonStr = MpJsonConverter.SerializeObject(tl);
                    break;

                case MpEditorBindingFunctionType.getContactsFromFetcher:
                    var cl = await MpAvTemplateModelHelper.Instance.GetContactsAsync();
                    getResp.responseFragmentJsonStr = MpJsonConverter.SerializeObject(cl);
                    break;

                case MpEditorBindingFunctionType.getMessageBoxResult:
                    var msgBoxReq = MpJsonConverter.DeserializeObject<MpQuillShowDialogRequestMessage>(getReq.reqMsgFragmentJsonStr);

                    object result = null;

                    switch (msgBoxReq.dialogType) {
                        case "okcancel":

                            result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                                title: msgBoxReq.title,
                                message: msgBoxReq.msg,
                                iconResourceObj: msgBoxReq.iconResourceObj,
                                owner: TopLevel.GetTopLevel(this));
                            break;
                    }

                    var msgBoxResp = new MpQuillShowDialogResponseMessage() {
                        dialogResponse = result.ToStringOrDefault()
                    };
                    getResp.responseFragmentJsonStr = MpJsonConverter.SerializeObject(msgBoxResp);
                    break;
                case MpEditorBindingFunctionType.getClipboardDataTransferObject:
                    var cb_dtObjReq = MpJsonConverter.DeserializeObject<MpQuillEditorClipboardDataObjectRequestNotification>(getReq.reqMsgFragmentJsonStr);
                    var cb_ido = await Mp.Services.DataObjectTools.ReadClipboardAsync(false) as IDataObject;
                    var cb_dtObjResp = cb_ido.ToQuillDataItemsMessage();
                    getResp.responseFragmentJsonStr = MpJsonConverter.SerializeObject(cb_dtObjResp);
                    break;
                case MpEditorBindingFunctionType.getDragDataTransferObject:
                    // NOTE to avoid data object limitations of letting cef handle dnd when internal drag, 
                    // use full object to perform actual drop. Otherwise its only used for drag effect.
                    // If dnd was passive (simulating all drop events via messages) it would be asynchronous 
                    // which has lots of thread access issues

                    IDataObject processed_drag_avdo = MpAvDoDragDropWrapper.DragDataObject;
                    if (processed_drag_avdo == null) {
                        // external drag, use data from webview
                        var drag_dtObjReq = MpJsonConverter.DeserializeObject<MpQuillEditorDragDataObjectRequestNotification>(getReq.reqMsgFragmentJsonStr);
                        var drag_hdo = MpJsonConverter.DeserializeBase64Object<MpQuillHostDataItemsMessage>(drag_dtObjReq.unprocessedDataItemsJsonStr);

                        var unprocessed_drag_avdo = drag_hdo.ToAvDataObject();

                        processed_drag_avdo = await Mp.Services
                            .DataObjectTools.ReadDragDropDataObjectAsync(unprocessed_drag_avdo) as IDataObject;

                        if (BindingContext.CopyItemType == MpCopyItemType.FileList &&
                            processed_drag_avdo.TryGetData(MpPortableDataFormats.AvFiles, out object fn_obj)) {
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
                                processed_drag_avdo.Set(MpPortableDataFormats.INTERNAL_FILE_LIST_FRAGMENT_FORMAT, fl_frag.SerializeJsonObjectToBase64());
                            } else {
                                MpDebug.Break($"unhandled file obj type {fn_obj.GetType()}");
                            }
                        }
                    }


                    var processed_drag_hdo = processed_drag_avdo.ToQuillDataItemsMessage();
                    getResp.responseFragmentJsonStr = MpJsonConverter.SerializeObject(processed_drag_hdo);
                    break;
            }

            if (string.IsNullOrEmpty(getResp.responseFragmentJsonStr)) {
                // no data to return
                return;
            }

            SendMessage($"getRequestResponse_ext('{getResp.SerializeJsonObjectToBase64()}')");
        }

        #endregion

        #endregion

        #region Properties

        #region Web View

#if PLAT_WV
        protected WebView _webView
        public virtual WebView WebView { get; set; } 
#endif
        #endregion

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

        #region Constructors

        public MpAvContentWebView() : base() {
            InitialUrl = Mp.Services.PlatformInfo.EditorPath.ToFileSystemUriFromPath();
            this.GetObservable(MpAvContentWebView.IsEditorInitializedProperty).Subscribe(value => OnIsEditorInitializedChanged());

            this.GetObservable(MpAvContentWebView.ContentIdProperty).Subscribe(value => OnContentIdChanged());
            this.GetObservable(MpAvContentWebView.IsContentSelectedProperty).Subscribe(value => OnIsContentSelectedChanged());
            this.GetObservable(MpAvContentWebView.IsContentResizingProperty).Subscribe(value => OnIsContentResizingChanged());
            this.GetObservable(MpAvContentWebView.IsContentReadOnlyProperty).Subscribe(value => OnIsContentReadOnlyChanged());
            this.GetObservable(MpAvContentWebView.IsContentSubSelectableProperty).Subscribe(value => OnIsContentSubSelectableChanged());
            this.GetObservable(MpAvContentWebView.IsContentFindAndReplaceVisibleProperty).Subscribe(value => OnIsContentFindOrReplaceVisibleChanged());

        }

#if PLAT_WV
        private void WebView_WebMessageReceived(object sender, WebViewCore.Events.WebViewMessageReceivedEventArgs e) {
            if (MpJsonConverter.DeserializeObject<MpQuillPostMessageResponse>(e.Message) is MpQuillPostMessageResponse resp) {
                HandleBindingNotification(resp.msgType, resp.msgData, resp.handle);
            }
        } 
        
        private void WebView_NavigationCompleted(object sender, WebViewCore.Events.WebViewUrlLoadedEventArg e) {
            LoadEditorAsync().FireAndForgetSafeAsync();
        }
#elif CEF_WV

        protected override void Dispose(bool disposing) {
            if (disposing &&
                BindingContext != null &&
                BindingContext.IsFinalClosingState) {
                // disposal handled in pop out closed handler after IsFinalClosingState reset
                return;
            }
            base.Dispose(disposing);
        }


        //protected override async void OnNavigated(NavigatedEventArgs e) {
        //    base.OnNavigated(e);
        //    if (MpUrlHelpers.IsBlankUrl(e.Url)) {
        //        return;
        //    }
        //    //await LoadEditorAsync();
        //}
#else
        //public override void OnNavigated(string url) {
        //    base.OnNavigated(url);
        //    if (MpUrlHelpers.IsBlankUrl(url)) {
        //        return;
        //    }
        //    LoadEditorAsync().FireAndForgetSafeAsync();
        //}
#endif

        #endregion

        #region Public Methods
        public async Task ReloadAsync() {
            await LoadEditorAsync();
            await LoadContentAsync();
        }

        public async Task<MpQuillEditorSelectionStateMessage> GetSelectionStateAsync() {
            // NOTE resetting sel state is lazy upon request
            _lastEditorSelectionStateMessage = null;
            SendMessage($"selectionStateRequest_ext_ntf()");
            while (_lastEditorSelectionStateMessage == null) {
                await Task.Delay(100);
            }
            return _lastEditorSelectionStateMessage;
        }

        public void FinishDisposal() {
#if CEF_WV
            this.Dispose(true);
#endif
        }
        #endregion

        #region Protected Methods
        protected override void OnDragLeave(RoutedEventArgs e) {
            base.OnDragLeave(e);

            var dndMsg = new MpQuillDragDropEventMessage() {
                eventType = "dragleave"
            };
            SendMessage($"dragEventFromHost_ext('{dndMsg.SerializeJsonObjectToBase64()}')");
        }

        protected override void OnDrop(DragEventArgs e) {
            base.OnDrop(e);
            this.Cursor = null;// new Cursor(StandardCursorType.Arrow);
        }

        protected override void OnCursorChange(CursorChangeEventArgs e) {
            base.OnCursorChange(e);
        }
#if DESKTOP

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e) {
            if (!IsScrollWheelEnabled) {
                // disabled in no-select mode, otherwise cefnet swallows event and pin tray won't scroll
                return;
            }
            base.OnPointerWheelChanged(e);
        }

#endif
        protected virtual MpQuillInitMainRequestMessage GetInitMessage() {
            return new MpQuillInitMainRequestMessage() {
                envName = Mp.Services.PlatformInfo.OsType.ToString(),
                defaults = GetDefaultsMessage()
            };
        }

        #endregion

        #region Private Methods
        private void InitDnd() {

        }
        private MpQuillDefaultsRequestMessage GetDefaultsMessage() {
            return new MpQuillDefaultsRequestMessage() {
                minLogLevel = (int)MpConsole.MinLogLevel,
                isDebug = MpDebug.IsDebug,
                defaultFontFamily = MpAvPrefViewModel.Instance.DefaultEditableFontFamily,
                defaultFontSize = MpAvPrefViewModel.Instance.DefaultFontSize.ToString() + "px",
                isSpellCheckEnabled = MpAvPrefViewModel.Instance.IsSpellCheckEnabled,
                currentTheme = MpAvPrefViewModel.Instance.ThemeType != MpThemeType.Dark ? MpThemeType.Light.ToString().ToLower() : MpThemeType.Dark.ToString().ToLower(),
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
                contentId = BindingContext.CopyItemId,
                contentHandle = BindingContext.PublicHandle,
                contentType = BindingContext.CopyItemType.ToString(),
                itemData = BindingContext.EditorFormattedItemData,
                isReadOnly = BindingContext.IsContentReadOnly,
                isSubSelectionEnabled = BindingContext.IsSubSelectionEnabled,
                breakBeforeLoad = BreakOnNextLoad
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
                    }.SerializeJsonObjectToBase64() : null;
            }

            loadContentMsg.appendStateFragment =
                BindingContext.IsAppendNotifier ?
                    MpAvClipTrayViewModel.Instance
                    .GetAppendStateMessage(null)
                    .SerializeJsonObjectToBase64() : null;

            if (MpAvPersistentClipTilePropertiesHelper
                .TryGetPersistentSubSelectionState(
                    BindingContext.CopyItemId,
                    BindingContext.QueryOffsetIdx,
                    out var sel_state)) {
                loadContentMsg.selectionFragment = sel_state.SerializeJsonObjectToBase64();
            }

            if (MpAvPersistentClipTilePropertiesHelper
                .IsPersistentIsSubSelectable_ById(BindingContext.CopyItemId, BindingContext.QueryOffsetIdx)) {
                loadContentMsg.pasteButtonInfoFragment = MpAvClipTrayViewModel.Instance.CurPasteInfoMessage.SerializeJsonObjectToBase64();
            }

            return loadContentMsg;
        }
        #endregion

        #region Control Life Cycle
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e) {
            base.OnAttachedToLogicalTree(e);
            Mp.Services.ContentViewLocator.AddView(this);

#if PLAT_WV
            WebView.WebMessageReceived += WebView_WebMessageReceived;
            WebView.NavigationCompleted += WebView_NavigationCompleted
            //Content = WebView;
#endif
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            LastPointerPressedEventArgs = e;
        }
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e) {
            base.OnDetachedFromLogicalTree(e);
            Mp.Services.ContentViewLocator.RemoveView(this);

        }

        protected override void OnDataContextEndUpdate() {
            base.OnDataContextEndUpdate();
            _locatedDateTime = DateTime.Now;
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

#if CEF_WV
            var sw = Stopwatch.StartNew();
            while (!IsDomLoaded) {
                // wait for Navigate(EditorPath)
                await Task.Delay(100);
                if (sw.ElapsedMilliseconds > 15_000) {
                    // BUG found this stuck here, i think it makes the ObjectDisposedException
                    // would be nice to infer state  but just letting it load to see if its ok
                    //MpDebug.Break($"editor timeout, should open its dev tools");
                    //IsDomLoaded = true;
                    //ShowDevTools();
                    MpDebug.BreakAll();
                    break;
                }
            }
#else
            await Task.Delay(1);
#endif
            MpConsole.WriteLine($"waited for domload: {sw.ElapsedMilliseconds}ms");
            var req = GetInitMessage();
            SendMessage($"initMain_ext('{req.SerializeJsonObjectToBase64()}')");
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

        public async Task<bool> PerformUpdateContentRequestAsync(MpJsonObject jsonObj) {
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
                req.deltaFragmentStr = jsonObj.SerializeJsonObjectToBase64();
            } else if (jsonObj is MpAnnotationNodeFormat) {
                req.annotationFragmentStr = jsonObj.SerializeJsonObjectToBase64();
            } else {
                // since there is nothing known to apply consider it successful
                return true;
            }
            SendMessage($"updateContents_ext('{req.SerializeJsonObjectToBase64()}')");
            return true;
        }

        public async Task LoadContentAsync(bool isSearchEnabled = true) {
            Dispatcher.UIThread.VerifyAccess();
            if (this is MpAvPlainHtmlConverterWebView) {

            }
            IsEditorLoaded = false;

#if CEF_WV
            if (this is WebView wv && wv.PendingEvalCount() > 0 ||
                BindingContext == null) {
                this.NeedsEvalJsCleared = true;
                while (NeedsEvalJsCleared) {
                    await Task.Delay(100);
                }
            }
#endif
            if (BindingContext == null) {
                // unloaded
                return;
            }

            if (BindingContext.IsAnyPlaceholder) {
                // no content
                return;
            }
            while (!IsEditorInitialized) {
                // wait for initMain - onInitComplete_ntf
                await Task.Delay(100);
            }
            if (BindingContext == null) {
                // unloaded
                return;
            }
            while (BindingContext.FileItemCollectionViewModel.IsAnyBusy) {
                // wait for file icons to populate from ctvm.Init
                await Task.Delay(100);
            }

            var loadContentMsg = GetLoadContentMessage(isSearchEnabled);
            if (loadContentMsg.breakBeforeLoad) {
                ShowDevTools();
                await Task.Delay(5000);
            }
            string msgStr = loadContentMsg.SerializeJsonObjectToBase64();

            SendMessage($"loadContentAsync_ext('{msgStr}')");
        }

        private void ProcessContentChangedMessage(MpQuillEditorContentChangedMessage contentChanged_ntf, bool isInitialLoad = false) {
            if (!IsEditorInitialized) {
                // BUG load stalls on reload while editing waiting for initialzing...
                IsEditorInitialized = true;
            }

            if (BindingContext == null ||
                BindingContext.IsAnyPlaceholder) {
                return;
            }

            IsEditorLoaded = true;

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
            BindingContext.CopyItemSize1 = contentChanged_ntf.itemSize1;
            BindingContext.CopyItemSize2 = contentChanged_ntf.itemSize2;

            //Dispatcher.UIThread.Post(() => {
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
                MpJsonConverter.DeserializeBase64Object<MpQuillDataTransferCompletedNotification>(contentChanged_ntf.dataTransferCompletedRespFragment) is
                MpQuillDataTransferCompletedNotification dtcn) {
                ProcessDataTransferCompleteResponse(dtcn);
            }


            BindingContext.IgnoreHasModelChanged = false;

            if (BindingContext.IsAppendNotifier) {
                MpConsole.WriteLine("content changed on append");
                Dispatcher.UIThread.Post(async () => {
                    // NOTE have to call this as
                    // a workaround since db update trigger made editing too slow
                    // and this is a case its needed or it doesn't update (the view at least)
                    //await BindingContext.RefreshModelAsync();
                    // sync append item to current clipboard
                    var append_mpdo = await GetDataObjectAsync(null, false, true);
                    await Mp.Services.DataObjectTools
                        .WriteToClipboardAsync(append_mpdo, true);
                    MpConsole.WriteLine($"Clipboard updated with append data. Plain Text: ");
                    if (append_mpdo.TryGetData(MpPortableDataFormats.Text, out string pt)) {
                        MpConsole.WriteLine(pt);
                    } else {
                        MpConsole.WriteLine("NO PLAIN TEXT AVAILABLE");
                    }
                });
            }
            // });

        }

        private void ProcessDataTransferCompleteResponse(MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
            if (BindingContext.IsAnyPlaceholder) {
                // occurs for edit
                return;
            }
            ProcessContentChangedMessage(
                MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(
                    dataTransferCompleted_ntf.contentChangedMessageFragment));

            var dtobj = MpJsonConverter.DeserializeBase64Object<MpQuillHostDataItemsMessage>(dataTransferCompleted_ntf.sourceDataItemsJsonStr);
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
            var msg = new MpQuillIsHostFocusedChangedMessage() {
                isHostFocused = IsContentSelected
            };
            if (IsContentSelected) {
                if (BindingContext.IsSubSelectionEnabled) {
                    this.Focus();
                    SendMessage($"hostIsFocusedChanged_ext('{msg.SerializeJsonObjectToBase64()}')");
                }
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
                }
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
                SendMessage($"enableSubSelection_ext('{MpAvClipTrayViewModel.Instance.CurPasteInfoMessage.SerializeJsonObjectToBase64()}')");
                if (//BindingContext.HasTemplates && 
                    !BindingContext.IsDropOverTile) {
                    GrowView();
                }
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
                SendMessage($"appendStateChanged_ext('{appendChangedMsg.SerializeJsonObjectToBase64()}')");
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
            if (BindingContext.IsAppendNotifier &&
                    (!appendChangedMsg.isAppendLineMode && !appendChangedMsg.isAppendInsertMode && ctrvm.IsAnyAppendMode)) {
                yield return ctrvm.DeactivateAppendModeCommand;
            }
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
                TopLevel.GetTopLevel(this) is MpAvWindow w &&
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
}
