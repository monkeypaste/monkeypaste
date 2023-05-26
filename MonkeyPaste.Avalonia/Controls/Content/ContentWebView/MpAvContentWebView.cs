using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Org.BouncyCastle.Utilities;
using Avalonia.Layout;
#if DESKTOP

using CefNet;
using CefNet.Avalonia;
using CefNet.Internal;

#endif

namespace MonkeyPaste.Avalonia {

    public enum MpAvEditorBindingFunctionType {
        // two-way *_get async requests
        getDragData,
        getAllSharedTemplatesFromDb,
        getClipboardDataTransferObject,
        getDragDataTransferObject,
        getContactsFromFetcher,

        // one-way *_ntf notifications
        notifyDocSelectionChanged,
        notifyContentChanged,
        notifySubSelectionEnabledChanged,
        notifyException,
        notifyReadOnlyEnabled,
        notifyReadOnlyDisabled,
        notifyInitComplete,
        notifyDomLoaded,
        notifyDropCompleted,
        notifyDragEnter,
        notifyDragLeave,
        notifyDragEnd,
        notifyContentScreenShot,
        notifyUserDeletedTemplate,
        notifyAddOrUpdateTemplate,
        notifyPasteRequest,
        notifyFindReplaceVisibleChange,
        notifyQuerySearchRangesChanged,
        notifyLoadComplete,
        notifyShowCustomColorPicker,
        notifyNavigateUriRequested,
        notifySetClipboardRequested,
        notifyDataTransferCompleted,
        notifySelectionChanged,
        notifyScrollChanged,
        notifyAppendStateChanged,
        notifyInternalContextMenuIsVisibleChanged,
        notifyInternalContextMenuCanBeShownChanged,
        notifyLastTransactionUndone,
        notifyAnnotationSelected,
        notifyShowDebugger,
        notifyDataObjectResponse,
        notifyReadOnlyEnabledFromHost,
        notifyPlainHtmlConverted,
        notifyScrollBarVisibilityChanged
    }
    [DoNotNotify]
    public class MpAvContentWebView :
#if DESKTOP
        WebView,
#else
        MpAvNativeWebViewHost,
#endif
        MpIContentView,
        //MpIAyncJsonMessenger,
        MpIContentViewLocator,
        MpAvIDragSource,
        MpAvIResizableControl,
        MpAvIDomStateAwareWebView,
        MpAvIAsyncJsEvalWebView,
        MpAvIReloadableContentWebView,
        MpAvIWebViewBindingResponseHandler {

        #region Private Variables
        private const int _APPEND_TIMEOUT_MS = 5000;
        private string _contentScreenShotBase64_ntf { get; set; }

        private string _lastLoadedContentHandle = null;

        private DateTime? _locatedDateTime;

        private MpQuillContentDataObjectResponseMessage _lastDataObjectResp = null;
        private MpQuillEditorContentChangedMessage _lastReadOnlyEnabledFromHostResp = null;
        #endregion

        #region Constants

        public const string BLANK_URL = "about:blank";
        public const string APPEND_NOTIFIER_URL_PARAMS = "append_notifier=true";
        #endregion

        #region Statics
        public static string DefaultContentUrl =>
            MpAvClipTrayViewModel.EditorUri;

        private static List<MpAvContentWebView> _AllWebViews = new List<MpAvContentWebView>();

        public static MpAvContentWebView LocateModalWebView() {
            return _AllWebViews.FirstOrDefault(x => x.DataContext is MpAvClipTileViewModel ctvm && ctvm.IsAppendNotifier);
        }
        public static MpAvContentWebView LocateTrayTileWebView(int ciid) {
            if (ciid < 1) {
                return null;
            }
            var result = _AllWebViews
                .Where(x =>
                    x.DataContext is MpAvClipTileViewModel ctvm &&
                    !ctvm.IsAppendNotifier &&
                    ctvm.CopyItemId == ciid)
                .OrderByDescending(x => x._locatedDateTime)
                .ToList();

            if (result.Count == 0) {
                return null;
            }
            if (result.Count > 1) {
                // is this during a pin toggle? was this item pinned?
                //MpDebug.Break();
                // remove old refs
                var stale_wvl = result.Skip(1);
                // TODO? do these need further processing? besides hiding from locator?
                _AllWebViews = _AllWebViews.Where(x => stale_wvl.Contains(x)).ToList();

                MpConsole.WriteLine($"{stale_wvl.Count()} stale webviews removed for item '{result[0].DataContext}'");
            }
            return result[0];
        }

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
            this.ExecuteJavascript(msgJsonBase64Str);
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

        #region MpIContentViewLocator

        MpIContentView MpIContentViewLocator.LocateContentView(int contentId) =>
            LocateTrayTileWebView(contentId);
        MpIContentView MpIContentViewLocator.LocateModalContentView() =>
            LocateModalWebView();

        #endregion

        #region MpIContentView

        void MpIHasDevTools.ShowDevTools() =>
            ShowDevTools();

        bool MpIContentView.IsSubSelectable =>
            IsContentSubSelectable;

        Task MpIContentView.LoadContentAsync() =>
            PerformLoadContentRequestAsync();

        Task<bool> MpIContentView.UpdateContentAsync(MpJsonObject contentJsonObj) =>
            PerformUpdateContentRequestAsync(contentJsonObj);


        #endregion

        #region MpAvIReloadableWebView Implementation

        async Task MpAvIReloadableContentWebView.ReloadContentAsync() {
            await PerformLoadContentRequestAsync();
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

        #region MpAvIDropTarget Implementation 
        public bool IsDropping => BindingContext == null ? false : BindingContext.IsDropOverTile;
        #endregion

        #region MpAvIContentDragSource Implementation
        public PointerPressedEventArgs LastPointerPressedEventArgs { get; private set; }

        public bool WasDragCanceled { get; set; }
        //public void NotifyDropComplete(DragDropEffects dropEffect) {
        //    var dragEndMsg = new MpQuillDragEndMessage() {
        //        dataTransfer = new MpQuillDataTransferMessageFragment() {
        //            dropEffect = dropEffect.ToString().ToLower()
        //        },
        //        fromHost = true,
        //        wasCancel = dropEffect == DragDropEffects.None
        //    };

        //    SendMessage($"dragEnd_ext('{dragEndMsg.SerializeJsonObjectToBase64()}')");

        //    //IsDragging = false;
        //    MpConsole.WriteLine($"Drag complete for '{BindingContext}'. DropEffect: '{dropEffect}'");
        //}
        public void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc, bool meta) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => (this as MpAvIDragSource).NotifyModKeyStateChanged(ctrl, alt, shift, esc, meta));
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
            bool use_placeholders = true,
            bool ignore_selection = false) {
            if (BindingContext == null) {
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
                forOle = !ignore_selection
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
                !formats.Contains(MpPortableDataFormats.AvFileNames)) {
                ignore_pseudo_file = true;
            }

            if (ignore_ss && !provided_formats) {
                contentDataReq.formats.Remove(MpPortableDataFormats.AvPNG);
                contentDataReq.formats.Remove(MpPortableDataFormats.WinBitmap);
                contentDataReq.formats.Remove(MpPortableDataFormats.WinDib);
            }

            _lastDataObjectResp = null;
            SendMessage($"contentDataObjectRequest_ext_ntf('{contentDataReq.SerializeJsonObjectToBase64()}')");
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
                if (contentDataReq.formats.Contains(MpPortableDataFormats.AvFileNames)) {
                    if (avdo.TryGetData(MpPortableDataFormats.AvFileNames, out string fp_str) &&
                        !string.IsNullOrWhiteSpace(fp_str)) {
                        // file data is csv using DefaultCsvProps seperated by rows which should be envNewLine

                        avdo.SetData(MpPortableDataFormats.AvFileNames, fp_str.SplitNoEmpty(MpCopyItem.FileItemSplitter));
                    } else {

                        avdo.SetData(MpPortableDataFormats.AvFileNames, ctvm.CopyItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter));
                    }
                }
            } else if (!ignore_pseudo_file) {
                // NOTE setting dummy file so OLE system sees format on clipboard, actual
                // data is overwritten in core clipboard handler
                if (use_placeholders) {
                    avdo.SetData(MpPortableDataFormats.AvFileNames, new[] { MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT });
                } else {
                    //string ctvm_fp = await ctvm.CopyItemData.ToFileAsync(
                    //            forceNamePrefix: ctvm.CopyItemTitle,
                    //            forceExt: ctvm.CopyItemType == MpCopyItemType.Image ? "png" : "txt",
                    //            isTemporary: true);
                    //avdo.SetData(
                    //    MpPortableDataFormats.AvFileNames,
                    //    new[] { ctvm_fp });

                    // NOTE presumes Text is txt and Image is png
                    // get unique pseudo-file path for whole or partial content
                    bool is_fragment = ctvm.CopyItemType == MpCopyItemType.Text && !contentDataResp.isAllContent ? true : false;
                    string ctvm_fp = ctvm.CopyItem.GetDefaultFilePaths(isFragment: is_fragment).FirstOrDefault();
                    string ctvm_data = is_fragment ? avdo.GetData(MpPortableDataFormats.Text) as string : ctvm.CopyItemData;
                    avdo.SetData(
                        MpPortableDataFormats.AvFileNames,
                        new[] { ctvm_fp });
                    ctvm_data.ToFileAsync(forcePath: ctvm_fp).FireAndForgetSafeAsync();
                }
            }

            bool is_full_content = ctvm.CopyItemType != MpCopyItemType.Text || contentDataResp.isAllContent;
            avdo.AddContentReferences(ctvm.CopyItem, is_full_content);

            if (ctvm.CopyItemType == MpCopyItemType.Image &&
                    ctvm.CopyItemData.ToAvBitmap() is Bitmap bmp) {

                avdo.SetData(MpPortableDataFormats.AvPNG, bmp.ToByteArray());
                avdo.SetData(MpPortableDataFormats.Text, bmp.ToAsciiImage());
                // TODO add colorized ascii maybe as html and rtf!!
            } else if (!ignore_ss) {
                if (use_placeholders) {
                    avdo.SetData(MpPortableDataFormats.AvPNG, MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT.ToBytesFromString());

                    Dispatcher.UIThread.Post(async () => {
                        await SetScreenShotAsync(avdo);
                    });
                } else {
                    await SetScreenShotAsync(avdo);
                }
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

        public virtual void HandleBindingNotification(MpAvEditorBindingFunctionType notificationType, string msgJsonBase64Str) {
            if (!this.IsAttachedToVisualTree()) {
                NeedsEvalJsCleared = true;
                return;
            }
            var ctvm = BindingContext;
            if (ctvm == null &&
                notificationType != MpAvEditorBindingFunctionType.notifyDomLoaded &&
                notificationType != MpAvEditorBindingFunctionType.notifyInitComplete) {
                // converter doesn't have data context but needs to notify dom loaded which doesn't need it
                return;
            }
            MpJsonObject ntf = null;
            switch (notificationType) {

                #region LIFE CYCLE

                case MpAvEditorBindingFunctionType.notifyDomLoaded:
                    IsDomLoaded = true;
                    break;
                case MpAvEditorBindingFunctionType.notifyInitComplete:
                    IsEditorInitialized = true;
                    break;

                case MpAvEditorBindingFunctionType.notifyLoadComplete:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage loadComplete_ntf) {
                        IsEditorLoaded = true;
                        ProcessContentChangedMessage(loadComplete_ntf);
                    }
                    break;

                #endregion

                #region CONTENT CHANGED

                case MpAvEditorBindingFunctionType.notifyReadOnlyDisabled:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillDisableReadOnlyResponseMessage disableReadOnlyMsg) {
                        ctvm.IsContentReadOnly = false;
                        //ctvm.UnconstrainedContentDimensions = new MpSize(disableReadOnlyMsg.editorWidth, disableReadOnlyMsg.editorHeight);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyReadOnlyEnabled:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage enableReadOnlyMsg) {
                        // NOTE only difference from contentChanged is no dimension info and this needs to enable readonly
                        ctvm.IsContentReadOnly = true;
                        ProcessContentChangedMessage(enableReadOnlyMsg);
                    }
                    break;

                case MpAvEditorBindingFunctionType.notifyReadOnlyEnabledFromHost:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage enableReadOnlyFromHostMsg) {
                        _lastReadOnlyEnabledFromHostResp = enableReadOnlyFromHostMsg;
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyContentChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage contentChanged_ntf) {
                        ProcessContentChangedMessage(contentChanged_ntf);
                        //RelayMsg($"contentChanged_ext('{msgJsonBase64Str}')");
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyDataTransferCompleted:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillDataTransferCompletedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
                        ProcessDataTransferCompleteResponse(dataTransferCompleted_ntf).FireAndForgetSafeAsync(BindingContext);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyLastTransactionUndone:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillLastTransactionUndoneNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillLastTransactionUndoneNotification lastTransUndone_ntf) {
                        BindingContext
                            .TransactionCollectionViewModel
                            .RemoveMostRecentTransactionCommand.Execute(null);
                    }
                    break;

                #endregion

                #region LAYOUT

                case MpAvEditorBindingFunctionType.notifyScrollBarVisibilityChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillScrollBarVisibilityChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillScrollBarVisibilityChangedNotification scrollbarVisibleMsg) {
                        BindingContext.IsHorizontalScrollbarVisibile = scrollbarVisibleMsg.isScrollBarXVisible;
                        BindingContext.IsVerticalScrollbarVisibile = scrollbarVisibleMsg.isScrollBarYVisible;
                    }
                    break;
                #endregion

                #region SELECTION

                case MpAvEditorBindingFunctionType.notifySubSelectionEnabledChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillSubSelectionChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillSubSelectionChangedNotification subSelChangedNtf) {
                        ctvm.IsSubSelectionEnabled = subSelChangedNtf.isSubSelectionEnabled;
                    }
                    break;

                case MpAvEditorBindingFunctionType.notifyAnnotationSelected:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillAnnotationSelectedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillAnnotationSelectedMessage annSelectedMsg) {
                        BindingContext
                            .TransactionCollectionViewModel
                            .SelectChildCommand.Execute(annSelectedMsg.annotationGuid);
                    }
                    break;
                #endregion

                #region MODAL SYNC

                //case MpAvEditorBindingFunctionType.notifySelectionChanged:
                //    if(!ctvm.IsAppendNotifier && !ctvm.IsAppendTrayItem) {
                //        break;
                //    }
                //    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillSelectionChangedMessage>(msgJsonBase64Str);
                //    if (ntf is MpQuillSelectionChangedMessage selChangedMsg) {
                //        RelayMsg($"setSelection_ext('{msgJsonBase64Str}')");
                //    }
                //    break;
                //case MpAvEditorBindingFunctionType.notifyScrollChanged:
                //    if (!ctvm.IsAppendNotifier && !ctvm.IsAppendTrayItem) {
                //        break;
                //    }
                //    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillScrollChangedMessage>(msgJsonBase64Str);
                //    if (ntf is MpQuillScrollChangedMessage scrollChangedMsg) {
                //        //RelayMsg($"setScroll_ext('{msgJsonBase64Str}')");
                //    }
                //    break;

                case MpAvEditorBindingFunctionType.notifyAppendStateChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillAppendStateChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillAppendStateChangedMessage appendStateChangedMsg) {
                        ProcessAppendStateChangedMessage(appendStateChangedMsg, "editor");
                    }
                    break;

                #endregion

                #region OLE

                case MpAvEditorBindingFunctionType.notifyDataObjectResponse:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillContentDataObjectResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentDataObjectResponseMessage) {
                        // GetContentDataObject blocks until _lastDataObjectResp is set
                        _lastDataObjectResp = ntf as MpQuillContentDataObjectResponseMessage;
                    }
                    break;

                #region CLIPBOARD

                case MpAvEditorBindingFunctionType.notifySetClipboardRequested:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorSetClipboardRequestNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorSetClipboardRequestNotification setClipboardReq) {
                        ctvm.CopyToClipboardCommand.Execute(null);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyPasteRequest:
                    MpAvClipTrayViewModel.Instance.PasteFromClipTilePasteButtonCommand.Execute(BindingContext);
                    break;

                #endregion

                #region DND

                case MpAvEditorBindingFunctionType.notifyDragEnter:
                    BindingContext.IsDropOverTile = true;
                    break;
                case MpAvEditorBindingFunctionType.notifyDragLeave:
                    BindingContext.IsDropOverTile = false;
                    break;
                case MpAvEditorBindingFunctionType.notifyDragEnd:
                    BindingContext.IsDropOverTile = false;
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillDragEndMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillDragEndMessage dragEndMsg) {
                        WasDragCanceled = dragEndMsg.wasCancel;
                    }
                    break;

                case MpAvEditorBindingFunctionType.notifyDropCompleted:
                    BindingContext.IsDropOverTile = false;
                    MpAvClipTrayViewModel.Instance.SelectedItem = BindingContext;
                    break;

                #endregion

                #endregion

                #region HIGHLIGHTING

                case MpAvEditorBindingFunctionType.notifyFindReplaceVisibleChange:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillContentFindReplaceVisibleChanedNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentFindReplaceVisibleChanedNotificationMessage findReplaceMsgObj) {
                        ctvm.IsFindAndReplaceVisible = findReplaceMsgObj.isFindReplaceVisible;
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyQuerySearchRangesChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillContentQuerySearchRangesChangedNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentQuerySearchRangesChangedNotificationMessage searchRangeCountMsg) {
                        // NOTE content highlight blocks until this is recv'd
                        SearchResponse = searchRangeCountMsg;
                    }
                    break;

                case MpAvEditorBindingFunctionType.notifyContentScreenShot:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillContentScreenShotNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentScreenShotNotificationMessage ssMsg) {
                        _contentScreenShotBase64_ntf = ssMsg.contentScreenShotBase64;
                    }
                    break;

                #endregion

                #region TEMPLATES

                case MpAvEditorBindingFunctionType.notifyAddOrUpdateTemplate:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillTemplateAddOrUpdateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillTemplateAddOrUpdateNotification addOrUpdateTemplateMsg) {
                        var t = MpJsonConverter.DeserializeBase64Object<MpTextTemplate>(addOrUpdateTemplateMsg.addedOrUpdatedTextTemplateBase64JsonStr);
                        MpAvTemplateModelHelper.Instance
                            .AddOrUpdateTemplateAsync(BindingContext.CopyItemId, t)
                            .FireAndForgetSafeAsync();
                    }

                    break;
                case MpAvEditorBindingFunctionType.notifyUserDeletedTemplate:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillUserDeletedTemplateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillUserDeletedTemplateNotification deleteTemplateMsg) {
                        MpAvTemplateModelHelper.Instance
                            .DeleteTemplateAsync(BindingContext.CopyItemId, deleteTemplateMsg.userDeletedTemplateGuid)
                            .FireAndForgetSafeAsync();
                    }
                    break;

                #endregion

                #region WINDOW ACTIONS

                case MpAvEditorBindingFunctionType.notifyShowCustomColorPicker:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillShowCustomColorPickerNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowCustomColorPickerNotification showCustomColorPickerMsg) {
                        Dispatcher.UIThread.Post(async () => {

                            if (string.IsNullOrWhiteSpace(showCustomColorPickerMsg.pickerTitle)) {
                                // editor should provide title for templates but for content set to title here if ya want (may
                                showCustomColorPickerMsg.pickerTitle = $"Pick a color, any color for '{ctvm.CopyItemTitle}'";
                            }
                            string pickerResult = await Mp.Services.CustomColorChooserMenuAsync.ShowCustomColorMenuAsync(
                                showCustomColorPickerMsg.currentHexColor,
                                showCustomColorPickerMsg.pickerTitle,
                                null);

                            var resp = new MpQuillCustomColorResultMessage() {
                                customColorResult = pickerResult
                            };
                            SendMessage($"provideCustomColorPickerResult_ext('{resp.SerializeJsonObjectToBase64()}')");
                        });
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyNavigateUriRequested:
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
                                // re-annotate with new 
                                MpAvAnalyticItemCollectionViewModel.Instance
                                .ApplyCoreAnnotatorCommand.Execute(BindingContext);
                            });
                        }
                        string uri_str = HttpUtility.HtmlDecode(navUriReq.uri);
                        MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(uri_str);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyInternalContextMenuIsVisibleChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillInternalContextIsVisibleChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillInternalContextIsVisibleChangedNotification ctxMenuChangedMsg) {
                        ctvm.CanShowContextMenu = !ctxMenuChangedMsg.isInternalContextMenuVisible;
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyInternalContextMenuCanBeShownChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillInternalContextMenuCanBeShownChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillInternalContextMenuCanBeShownChangedNotification ctxMenuCanShowChangedMsg) {
                        ctvm.CanShowContextMenu = !ctxMenuCanShowChangedMsg.canInternalContextMenuBeShown;
                    }
                    break;
                #endregion

                #region OTHER

                case MpAvEditorBindingFunctionType.notifyShowDebugger:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillShowDebuggerNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowDebuggerNotification showDebugNtf) {
                        MpConsole.WriteLine($"[{ctvm}] {showDebugNtf.reason}");
                        this.ShowDevTools();
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyException:
                    MpDebug.Break($"{ctvm} editor exception");
                    //ntf = MpJsonConverter.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    //if (ntf is MpQuillExceptionMessage exceptionMsgObj) {
                    //    MpConsole.WriteLine($"[{ctvm}] {exceptionMsgObj}");

                    //}
                    break;

                #endregion

                #region GET CALLBACKS

                case MpAvEditorBindingFunctionType.getDragDataTransferObject:
                case MpAvEditorBindingFunctionType.getClipboardDataTransferObject:
                case MpAvEditorBindingFunctionType.getAllSharedTemplatesFromDb:
                    HandleBindingGetRequest(notificationType, msgJsonBase64Str).FireAndForgetSafeAsync(ctvm);
                    break;

                    #endregion
            }

            //MpConsole.WriteLine($"Tile {ctvm} received cef notification type '{notificationType}' w/ msg:",true);
            //MpConsole.WriteLine($"'{(ntf == null ? "NO DATA RECEIVED":ntf.ToPrettyPrintJsonString())}'", false, true);
        }

        private async Task HandleBindingGetRequest(MpAvEditorBindingFunctionType getReqType, string msgJsonBase64) {
            var getReq = MpJsonConverter.DeserializeBase64Object<MpQuillGetRequestNotification>(msgJsonBase64);
            var getResp = new MpQuillGetResponseNotification() { requestGuid = getReq.requestGuid };
            switch (getReqType) {
                case MpAvEditorBindingFunctionType.getAllSharedTemplatesFromDb:
                    var templateReq = MpJsonConverter.DeserializeObject<MpQuillTemplateDbQueryRequestMessage>(getReq.reqMsgFragmentJsonStr);
                    var tl = await MpDataModelProvider.GetTextTemplatesByType(templateReq.templateTypes.Select(x => x.ToEnum<MpTextTemplateType>()));

                    getResp.responseFragmentJsonStr = MpJsonConverter.SerializeObject(tl);
                    break;

                case MpAvEditorBindingFunctionType.getContactsFromFetcher:
                    var cl = await MpAvTemplateModelHelper.Instance.GetContactsAsync();
                    getResp.responseFragmentJsonStr = MpJsonConverter.SerializeObject(cl);
                    break;
                case MpAvEditorBindingFunctionType.getClipboardDataTransferObject:
                    var cb_dtObjReq = MpJsonConverter.DeserializeObject<MpQuillEditorClipboardDataObjectRequestNotification>(getReq.reqMsgFragmentJsonStr);
                    var cb_ido = await Mp.Services.DataObjectHelperAsync.GetPlatformClipboardDataObjectAsync(false) as IDataObject;
                    var cb_dtObjResp = cb_ido.ToQuillDataItemsMessage();
                    getResp.responseFragmentJsonStr = MpJsonConverter.SerializeObject(cb_dtObjResp);
                    break;
                case MpAvEditorBindingFunctionType.getDragDataTransferObject:
                    var drag_dtObjReq = MpJsonConverter.DeserializeObject<MpQuillEditorDragDataObjectRequestNotification>(getReq.reqMsgFragmentJsonStr);
                    var drag_hdo = MpJsonConverter.DeserializeBase64Object<MpQuillHostDataItemsMessage>(drag_dtObjReq.unprocessedDataItemsJsonStr);
                    var unprocessed_drag_avdo = drag_hdo.ToAvDataObject();

                    var processed_drag_avdo = await Mp.Services
                        .DataObjectHelperAsync.ReadDragDropDataObjectAsync(unprocessed_drag_avdo) as IDataObject;

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

        #region View Models
        public MpAvClipTileViewModel BindingContext {
            get {
                if (DataContext is MpAvClipTileViewModel) {
                    return DataContext as MpAvClipTileViewModel;
                }
                if (DataContext is MpNotificationViewModelBase nvmb) {
                    return nvmb.Body as MpAvClipTileViewModel;
                }
                return null;
            }
        }

        #endregion

        #region Bindings & Life Cycle


        #endregion

        #region State
        public MpQuillContentQuerySearchRangesChangedNotificationMessage SearchResponse { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvContentWebView() : base() {
            this.GetObservable(MpAvContentWebView.IsEditorInitializedProperty).Subscribe(value => OnIsEditorInitializedChanged());

            this.GetObservable(MpAvContentWebView.ContentIdProperty).Subscribe(value => OnContentIdChanged());
            this.GetObservable(MpAvContentWebView.IsContentSelectedProperty).Subscribe(value => OnIsContentSelectedChanged());
            this.GetObservable(MpAvContentWebView.IsContentResizingProperty).Subscribe(value => OnIsContentResizingChanged());
            this.GetObservable(MpAvContentWebView.IsContentReadOnlyProperty).Subscribe(value => OnIsContentReadOnlyChanged());
            this.GetObservable(MpAvContentWebView.IsContentSubSelectableProperty).Subscribe(value => OnIsContentSubSelectableChanged());
            this.GetObservable(MpAvContentWebView.IsContentFindAndReplaceVisibleProperty).Subscribe(value => OnIsContentFindOrReplaceVisibleChanged());

            this.PointerPressed += MpAvCefNetWebView_PointerPressed;
            this.AttachedToLogicalTree += MpAvCefNetWebView_AttachedToLogicalTree;

#if DESKTOP
            this.CreateWindow += MpAvCefNetWebView_CreateWindow;
#endif
        }



        #endregion

        #region Public Methods

        public async Task ReloadAsync() {
            await LoadEditorAsync();
            await PerformLoadContentRequestAsync();
        }

        #endregion

        #region Protected Methods
#if DESKTOP
        protected override WebViewGlue CreateWebViewGlue() {
            return new MpAvCefNetWebViewGlue(this);
        }

#endif

        #endregion

        #region Private Methods

#if DESKTOP
        private void MpAvCefNetWebView_CreateWindow(object sender, CreateWindowEventArgs e) {
            if (TopLevel.GetTopLevel(this) is Window w &&
                w.TryGetPlatformHandle() is IPlatformHandle platformHandle) {

                if (platformHandle is IMacOSTopLevelPlatformHandle macOSHandle) {
                    e.WindowInfo.SetAsWindowless(macOSHandle.GetNSWindowRetained());
                } else {
                    e.WindowInfo.SetAsWindowless(platformHandle.Handle);
                }
            }

            e.Client = this.Client;
        }
#endif

        private MpQuillDefaultsRequestMessage GetDefaultsMessage() {
            return new MpQuillDefaultsRequestMessage() {
                defaultFontFamily = MpPrefViewModel.Instance.DefaultEditableFontFamily,
                defaultFontSize = MpPrefViewModel.Instance.DefaultFontSize.ToString() + "px",
                isSpellCheckEnabled = MpPrefViewModel.Instance.IsSpellCheckEnabled,
                currentTheme = MpPrefViewModel.Instance.CurrentThemeName,
                bgOpacity = MpPrefViewModel.Instance.GlobalBgOpacity,
                maxUndo = MpPrefViewModel.Instance.MaxUndoLimit,
                shortcutFragmentStr = MpAvShortcutCollectionViewModel.Instance.EditorShortcutsMsgBase64
            };
        }

        #endregion

        #region Control Life Cycle
        private void MpAvCefNetWebView_AttachedToLogicalTree(object sender, LogicalTreeAttachmentEventArgs e) {
            if (_AllWebViews.Contains(this)) {
                // should only happen once
                MpDebug.Break();
                return;
            }
            // set locatedDateTime to filter out webviews recyling during
            // pin/unpin ops (especially unpinall)
            _locatedDateTime = DateTime.Now;
            _AllWebViews.Add(this);

        }

        private void MpAvCefNetWebView_PointerPressed(object sender, PointerPressedEventArgs e) {
            LastPointerPressedEventArgs = e;
        }
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e) {
            base.OnDetachedFromLogicalTree(e);
            _AllWebViews.Remove(this);
            _locatedDateTime = null;
            _resizerControl = null;

        }

        protected override void OnDataContextEndUpdate() {
            base.OnDataContextEndUpdate();
            //if (_locatedDateTime == null &&
            //    this is not MpAvPlainHtmlConverterWebView &&
            //    (Mp.Services.PlatformInfo.IsDesktop &&
            //    this.GetVisualAncestor<MpAvAppendNotificationWindow>() == null)) {
            //    // is this called before attached to logical tree?
            //    MpDebug.Break();
            //}
            //if(!_AllWebViews.Contains(this))
            // update locate time to match this data context
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

        public static DirectProperty<MpAvContentWebView, bool> IsEditorInitializedProperty =
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

        public static DirectProperty<MpAvContentWebView, bool> IsDomLoadedProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsDomLoaded),
                x => x.IsDomLoaded,
                (x, o) => x.IsDomLoaded = o);

        #endregion

        public virtual string ContentUrl {
            get {
                //if (this.GetVisualRoot() is MpAvAppendNotificationWindow) {
                //    return $"{MpAvClipTrayViewModel.EditorUri}?{APPEND_NOTIFIER_URL_PARAMS}";
                //}

                return MpAvClipTrayViewModel.EditorUri;
            }
        }


#if DESKTOP
        protected override void OnBrowserCreated(EventArgs e) {
            base.OnBrowserCreated(e);
            Navigate(ContentUrl);
        }

        protected override void OnNavigated(NavigatedEventArgs e) {
            base.OnNavigated(e);
            if (e.Url == BLANK_URL) {
                return;
            }
            LoadEditorAsync().FireAndForgetSafeAsync();
        }
#else
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e) {
            base.OnAttachedToLogicalTree(e);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
        }
        public override void OnNavigated(string url) {
            base.OnNavigated(url);
            if (url == BLANK_URL) {
                return;
            }
            LoadEditorAsync().FireAndForgetSafeAsync();
        }
#endif

        private async Task LoadEditorAsync() {
            Dispatcher.UIThread.VerifyAccess();

            while (!IsDomLoaded) {
                // wait for Navigate(EditorPath)
                await Task.Delay(100);
            }

            var req = new MpQuillInitMainRequestMessage() {
                envName = Mp.Services.PlatformInfo.OsType.ToString(),
                defaults = GetDefaultsMessage()
            };
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
        public static DirectProperty<MpAvContentWebView, int> ContentIdProperty =
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

        public static DirectProperty<MpAvContentWebView, bool> IsEditorLoadedProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsEditorLoaded),
                x => x.IsEditorLoaded,
                (x, o) => x.IsEditorLoaded = o,
                false,
                BindingMode.TwoWay);


        #endregion

        private void OnContentIdChanged() {
            if (BindingContext == null ||
                !BindingContext.IsContentReadOnly) {
                return;
            }

            PerformLoadContentRequestAsync().FireAndForgetSafeAsync();
        }

        public async Task<bool> PerformUpdateContentRequestAsync(MpJsonObject jsonObj) {
            Dispatcher.UIThread.VerifyAccess();
            while (!IsEditorInitialized || !IsEditorLoaded) {
                // likely to happen for new content that's been annotated
                await Task.Delay(100);
                if (BindingContext.IsPlaceholder) {
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

        public async Task PerformLoadContentRequestAsync() {
            Dispatcher.UIThread.VerifyAccess();

            IsEditorLoaded = false;

            if (this.PendingEvalCount() > 0 ||
                BindingContext == null) {
                this.NeedsEvalJsCleared = true;
                while (NeedsEvalJsCleared) {
                    await Task.Delay(100);
                }
            }
            if (BindingContext == null) {
                // unloaded
                return;
            }

            if (BindingContext.IsPlaceholder && !BindingContext.IsPinned) {
                return;
            }
            while (!IsEditorInitialized) {
                // wait for initMain - onInitComplete_ntf
                await Task.Delay(100);
            }

            while (BindingContext.FileItemCollectionViewModel.IsAnyBusy) {
                // wait for file icons to populate from ctvm.Init
                await Task.Delay(100);
            }

            var loadContentMsg = new MpQuillLoadContentRequestMessage() {
                contentHandle = BindingContext.PublicHandle,
                contentType = BindingContext.CopyItemType.ToString(),
                itemData = BindingContext.EditorFormattedItemData,
                isReadOnly = BindingContext.IsContentReadOnly,
                isSubSelectionEnabled = BindingContext.IsSubSelectionEnabled
            };

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

            loadContentMsg.appendStateFragment =
                BindingContext.IsAppendNotifier ?
                    MpAvClipTrayViewModel.Instance
                    .GetAppendStateMessage(null)
                    .SerializeJsonObjectToBase64() : null;

            if (BindingContext.IsAppendNotifier) {

            }

            string msgStr = loadContentMsg.SerializeJsonObjectToBase64();

            SendMessage($"loadContent_ext('{msgStr}')");
        }

        private async void ProcessContentChangedMessage(MpQuillEditorContentChangedMessage contentChanged_ntf) {
            if (!IsEditorInitialized) {
                // BUG load stalls on reload while editing waiting for initialzing...
                IsEditorInitialized = true;
            }

            if (BindingContext == null ||
                (BindingContext.IsPlaceholder &&
                !BindingContext.IsPinned)) {
                return;
            }
            bool is_reload = BindingContext.PublicHandle == _lastLoadedContentHandle;
            _lastLoadedContentHandle = BindingContext.PublicHandle;
            IsEditorLoaded = true;

            if (contentChanged_ntf == null) {
                // shouldn't be null
                MpDebug.Break();
                return;
            }
            if (!string.IsNullOrWhiteSpace(contentChanged_ntf.dataTransferCompletedRespFragment) &&
                MpJsonConverter.DeserializeBase64Object<MpQuillDataTransferCompletedNotification>(contentChanged_ntf.dataTransferCompletedRespFragment) is
                MpQuillDataTransferCompletedNotification dtcn) {
                ProcessDataTransferCompleteResponse(dtcn).FireAndForgetSafeAsync(BindingContext);
            }

            if (contentChanged_ntf.itemSize1 >= 0 &&
                contentChanged_ntf.itemSize2 >= 0) {
                BindingContext.CopyItemSize1 = contentChanged_ntf.itemSize1;
                BindingContext.CopyItemSize2 = contentChanged_ntf.itemSize2;
            }

            if (contentChanged_ntf.itemData != null) {
                if (contentChanged_ntf.itemData.IsEmptyRichHtmlString()) {
                    // data's getting reset again
                    MpDebug.Break("data reset caught in content changed");
                }
                BindingContext.CopyItemData = contentChanged_ntf.itemData;
            }
            BindingContext.HasTemplates = contentChanged_ntf.hasTemplates;
            BindingContext.HasEditableTable = contentChanged_ntf.hasEditableTable;
            BindingContext.ActualContentHeight = contentChanged_ntf.contentHeight;

            if (BindingContext.IsAppendNotifier) {
                MpConsole.WriteLine("content changed on append");
                // sync append item to current clipboard
                var append_mpdo = await GetDataObjectAsync(null, false, true);
                await Mp.Services.DataObjectHelperAsync
                    .SetPlatformClipboardAsync(append_mpdo, true);
                MpConsole.WriteLine($"Clipboard updated with append data. Plain Text: ");
                if (append_mpdo.TryGetData(MpPortableDataFormats.Text, out string pt)) {
                    MpConsole.WriteLine(pt);
                } else {
                    MpConsole.WriteLine("NO PLAIN TEXT AVAILABLE");
                }
            }
            //IsEditorLoaded = true;
        }

        private async Task ProcessDataTransferCompleteResponse(MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
            if (BindingContext.IsPlaceholder) {
                // occurs for edit
                return;
            }
            var dtobj = MpJsonConverter.DeserializeBase64Object<MpQuillHostDataItemsMessage>(dataTransferCompleted_ntf.sourceDataItemsJsonStr);
            MpTransactionType transType = dataTransferCompleted_ntf.transferLabel.ToEnum<MpTransactionType>();
            MpPortableDataObject req_mpdo = dtobj.ToAvDataObject();
            if (transType == MpTransactionType.Appended) {
                // NOTE append sources are added before notifying editor since the source of the event
                // is clipboard change not drop or paste events which come from editor so
                // more accurate sources can be obtained checking in build workflow..

                if (!BindingContext.IsAppendNotifier) {
                    MpDebug.Break("Append state mismatch");
                }

                return;
            }

            string resp_json = null;
            if (!string.IsNullOrEmpty(dataTransferCompleted_ntf.changeDeltaJsonStr)) {
                resp_json = dataTransferCompleted_ntf.changeDeltaJsonStr.ToStringFromBase64();
            }

            IEnumerable<string> refs = null;
            if (req_mpdo != null) {
                var other_refs = await Mp.Services.SourceRefTools.GatherSourceRefsAsync(req_mpdo);
                refs = other_refs.Select(x => Mp.Services.SourceRefTools.ConvertToInternalUrl(x));
            }

            if (transType == MpTransactionType.None) {
                // what's the label?
                MpDebug.Break();
                transType = MpTransactionType.Error;
            }

            await Mp.Services.TransactionBuilder.ReportTransactionAsync(
                copyItemId: BindingContext.CopyItemId,
                reqType: MpJsonMessageFormatType.DataObject,
                req: req_mpdo.SerializeData(),
                respType: MpJsonMessageFormatType.Delta,
                resp: resp_json,
                ref_uris: refs,
                transType: transType);
        }

        #endregion

        #region Content State

        #region IsContentSelected

        private bool _isContentSelected;
        public bool IsContentSelected {
            get { return _isContentSelected; }
            set { SetAndRaise(IsContentSelectedProperty, ref _isContentSelected, value); }
        }

        public static DirectProperty<MpAvContentWebView, bool> IsContentSelectedProperty =
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

        public static DirectProperty<MpAvContentWebView, bool> IsContentResizingProperty =
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

        public static DirectProperty<MpAvContentWebView, bool> IsContentReadOnlyProperty =
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
                    MpAvMainWindowViewModel.Instance.IsAnyMainWindowTextBoxFocused = false;

                    if (!BindingContext.IsChildWindowOpen) {
                        MpAvResizeExtension.ResizeAnimated(this, BindingContext.ReadOnlyWidth, BindingContext.ReadOnlyHeight);
                    }

                    //string enableReadOnlyRespStr = await SendMessageAsync("enableReadOnly_ext_ntf()");
                    //var qrm = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(enableReadOnlyRespStr);

                    _lastReadOnlyEnabledFromHostResp = null;
                    SendMessage("enableReadOnly_ext_ntf()");
                    while (_lastReadOnlyEnabledFromHostResp == null) {
                        await Task.Delay(100);
                    }
                    MpQuillEditorContentChangedMessage resp = _lastReadOnlyEnabledFromHostResp;
                    _lastReadOnlyEnabledFromHostResp = null;
                    ProcessContentChangedMessage(resp);
                } else {
                    MpAvResizeExtension.ResizeAnimated(this, BindingContext.EditableWidth, BindingContext.EditableHeight);
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

        public static DirectProperty<MpAvContentWebView, bool> IsContentSubSelectableProperty =
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
                MpAvDocumentDragHelper.DragDataObject != null) {
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

                SendMessage("enableSubSelection_ext()");
                if (BindingContext.HasTemplates && !BindingContext.IsDropOverTile) {
                    MpAvResizeExtension.ResizeAnimated(this, BindingContext.EditableWidth, BindingContext.EditableHeight);
                }
            } else {
                SendMessage("disableSubSelection_ext()");
                MpAvResizeExtension.ResizeAnimated(this, BindingContext.ReadOnlyWidth, BindingContext.ReadOnlyHeight);
            }

        }

        #endregion

        #region IsContentFindAndReplaceVisible Property

        private bool _isContentFindAndReplaceVisible;
        public bool IsContentFindAndReplaceVisible {
            get { return _isContentFindAndReplaceVisible; }
            set { SetAndRaise(IsContentFindAndReplaceVisibleProperty, ref _isContentFindAndReplaceVisible, value); }
        }

        public static DirectProperty<MpAvContentWebView, bool> IsContentFindAndReplaceVisibleProperty =
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

        public void ProcessAppendStateChangedMessage(MpQuillAppendStateChangedMessage appendChangedMsg, string source) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => ProcessAppendStateChangedMessage(appendChangedMsg, source));
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
                        ProcessAppendStateChangedMessage(appendChangedMsg, source);

                    });
                    return;
                }
                //NOTE state changes should only come in one at a time
                // but line vs insert changes are only compared for is true since
                // they are interdependant to avoid double ntf

                if (appendChangedMsg.isAppendPaused != ctrvm.IsAppendPaused) {
                    ctrvm.ToggleAppendPausedCommand.Execute(null);
                } else if (appendChangedMsg.isAppendManualMode != ctrvm.IsAppendManualMode) {
                    ctrvm.ToggleAppendManualModeCommand.Execute(null);
                } else if (appendChangedMsg.isAppendPreMode != ctrvm.IsAppendPreMode) {
                    ctrvm.ToggleAppendPreModeCommand.Execute(null);
                } else if (appendChangedMsg.isAppendLineMode && !ctrvm.IsAppendLineMode) {
                    ctrvm.ToggleAppendLineModeCommand.Execute(null);
                } else if (appendChangedMsg.isAppendInsertMode && !ctrvm.IsAppendInsertMode) {
                    ctrvm.ToggleAppendInsertModeCommand.Execute(null);
                } else if (BindingContext.IsAppendNotifier &&
                    (!appendChangedMsg.isAppendLineMode && !appendChangedMsg.isAppendInsertMode && ctrvm.IsAnyAppendMode)) {
                    ctrvm.DeactivateAppendModeCommand.Execute(null);
                }
            } else {
                SendMessage($"appendStateChanged_ext('{appendChangedMsg.SerializeJsonObjectToBase64()}')");
            }
            //while (ctrvm.IsAnyBusy) {
            //    await Task.Delay(100);
            //}
            //while (ctrvm.IsAddingClipboardItem) {
            //    await Task.Delay(100);
            //}
            //SendMessage($"appendStateChanged_ext('{appendChangedMsg.SerializeJsonObjectToBase64()}')");
        }
        #endregion

    }
}
