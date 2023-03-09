using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
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
#if DESKTOP

using CefNet;
using CefNet.Avalonia;
using CefNet.Internal;

#endif

namespace MonkeyPaste.Avalonia {

    public enum MpAvEditorBindingFunctionType {
        // two-way *_get async requests
        getDragData,
        getAllNonInputTemplatesFromDb,
        getClipboardDataTransferObject,
        getDragDataTransferObject,

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
        notifyLastTransactionUndone,
        notifyAnnotationSelected,
        notifyShowDebugger
    }
    [DoNotNotify]
    public class MpAvContentWebView :
#if DESKTOP
        WebView,
#else
        MpAvNativeWebViewHost,
#endif
        MpIContentView,
        MpIAyncJsonMessenger,
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

        #region MpIAyncJsonMessenger Implementation
        public async Task<string> SendMessageAsync(string msgJsonBase64Str) {
            string result;
#if DESKTOP
            result = await this.EvaluateJavascriptAsync(msgJsonBase64Str);
#else
            if (Interop == null) {
                MpDebug.Break("lifecycle bug");
                return string.Empty;
            }
            result = await Interop.SendMessageAsync(this, msgJsonBase64Str);
#endif
            return result;
        }
        #endregion

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

        Task MpIContentView.UpdateContentAsync(MpJsonObject contentJsonObj) =>
            PerformUpdateContentRequestAsync(contentJsonObj);


        #endregion

        #region MpAvIReloadableWebView Implementation

        async Task MpAvIReloadableContentWebView.ReloadContentAsync() {
            await PerformLoadContentRequestAsync();
        }

        #endregion

        #region MpAvIResizableControl Implementation
        private Control _resizerControl;
        Control MpAvIResizableControl.ResizerControl {
            get {
                if (_resizerControl == null) {
                    var ctv = this.GetVisualAncestor<MpAvClipTileView>();
                    if (ctv != null) {
                        _resizerControl = ctv.FindControl<Control>("ClipTileResizeBorder");
                    }
                }
                return _resizerControl;
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

        public async Task<MpAvDataObject> GetDataObjectAsync(bool forOle, string[] formats = null) {
            if (BindingContext == null) {
                MpDebug.Break();
                return new MpAvDataObject();
            }
            var ctvm = BindingContext;
            // clear screenshot
            _contentScreenShotBase64_ntf = null;

            var contentDataReq = new MpQuillContentDataRequestMessage() {
                forOle = forOle
            };

            bool ignore_ss = true;
            // NOTE when file is on clipboard pasting into tile removes all other formats besides file
            // and pseudo files are only needed for dnd comptaibility so its gewd
            bool ignore_pseudo_file = false;//contentDataReq.forCutOrCopy;
            if (formats == null) {
                // TODO need to implement disable preset stuff once clipboard ui is in use 
                // for realtime RegisterFormats data
                contentDataReq.formats = MpPortableDataFormats.RegisteredFormats.ToList();
            } else {
                contentDataReq.formats = formats.ToList();
            }
            if (ctvm.CopyItemType != MpCopyItemType.Image && ignore_ss) {
                contentDataReq.formats.Remove(MpPortableDataFormats.AvPNG);
                contentDataReq.formats.Remove(MpPortableDataFormats.WinBitmap);
                contentDataReq.formats.Remove(MpPortableDataFormats.WinDib);
            }

            var contentDataRespStr =
                await SendMessageAsync($"contentRequest_ext('{contentDataReq.SerializeJsonObjectToBase64()}')");
            MpQuillContentDataResponseMessage contentDataResp =
                MpJsonConverter.DeserializeBase64Object<MpQuillContentDataResponseMessage>(contentDataRespStr);


            if (contentDataResp.dataItems == null) {
                return null;
            }
            var avdo = new MpAvDataObject();
            foreach (var di in contentDataResp.dataItems) {
                avdo.SetData(di.format, di.data);
            }

            if (forOle) {
                if (ctvm.CopyItemType == MpCopyItemType.Image) {
                    avdo.SetData(MpPortableDataFormats.AvPNG, ctvm.CopyItemData.ToAvBitmap().ToByteArray());
                    //var bmp = ctvm.CopyItemData.ToAvBitmap();
                    //avdo.SetData(MpPortableDataFormats.Text, bmp.ToAsciiImage());
                    //avdo.SetData(MpPortableDataFormats.AvHtml_bytes, bmp.ToRichHtmlImage());
                    // TODO add colorized ascii maybe as html and rtf!!
                } else if (!ignore_ss) {
                    // screen shot is async and js notifies w/ base64 property here
                    while (_contentScreenShotBase64_ntf == null) {

                        await Task.Delay(100);
                    }
                    avdo.SetData(MpPortableDataFormats.AvPNG, _contentScreenShotBase64_ntf);
                }

                if (ctvm.CopyItemType == MpCopyItemType.FileList) {
                    avdo.SetData(MpPortableDataFormats.AvFileNames, ctvm.CopyItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter));
                } else if (!ignore_pseudo_file) {
                    // js doesn't set file stuff for non-files
                    string ctvm_fp = await ctvm.CopyItemData.ToFileAsync(
                                forceNamePrefix: ctvm.CopyItemTitle,
                                forceExt: ctvm.CopyItemType == MpCopyItemType.Image ? "png" : "txt",
                                isTemporary: true);
                    avdo.SetData(
                        MpPortableDataFormats.AvFileNames,
                        new List<string>() { ctvm_fp });
                }

                bool add_tile_data = ctvm.CopyItemType != MpCopyItemType.Text || contentDataResp.isAllContent;
                if (add_tile_data) {
                    avdo.SetData(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT, ctvm.PublicHandle);
                }
                //avdo.SetData(MpPortableDataFormats.CefAsciiUrl, MpPlatformWrapper.Services.SourceRefBuilder.ToUrlAsciiBytes(ctvm.CopyItem));

                List<string> uri_list = new List<string>();
                if (avdo.TryGetData<IEnumerable<string>>(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out var uris)) {
                    uri_list = uris.ToList();
                }
                uri_list.Add(Mp.Services.SourceRefBuilder.ConvertToRefUrl(ctvm.CopyItem));
                avdo.SetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, uri_list);
            }


            avdo.MapAllPseudoFormats();

            //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvPNG));
            //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT));
            //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvFileNames));
            //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvHtml_bytes));
            //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefHtml));
            //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefText));

            return avdo;
        }
        public bool IsCurrentDropTarget => BindingContext == null ? false : BindingContext.IsDropOverTile;

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
            MpMessenger.RegisterGlobal(ReceivedGlobalMessega);
            this.GetObservable(MpAvContentWebView.AppendDataProperty).Subscribe(value => OnAppendDataChanged());
            this.GetObservable(MpAvContentWebView.AppendModeStateProperty).Subscribe(value => OnAppendModeStateChanged("command"));

            this.GetObservable(MpAvContentWebView.IsEditorInitializedProperty).Subscribe(value => OnIsEditorInitializedChanged());

            this.GetObservable(MpAvContentWebView.ContentIdProperty).Subscribe(value => OnContentIdChanged());
            this.GetObservable(MpAvContentWebView.IsContentSelectedProperty).Subscribe(value => OnIsContentSelectedChanged());
            this.GetObservable(MpAvContentWebView.IsContentResizingProperty).Subscribe(value => OnIsContentResizingChanged());
            this.GetObservable(MpAvContentWebView.IsContentReadOnlyProperty).Subscribe(value => OnIsContentReadOnlyChanged());
            this.GetObservable(MpAvContentWebView.IsContentSubSelectableProperty).Subscribe(value => OnIsContentSubSelectableChanged());
            this.GetObservable(MpAvContentWebView.IsContentFindAndReplaceVisibleProperty).Subscribe(value => OnIsContentFindOrReplaceVisibleChanged());

            this.PointerPressed += MpAvCefNetWebView_PointerPressed;
            this.AttachedToLogicalTree += MpAvCefNetWebView_AttachedToLogicalTree;
        }



        #endregion

        #region Public Methods

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
            if (App.MainWindow == null) {
                return;
            }
            IPlatformHandle platformHandle = App.MainWindow.PlatformImpl.Handle;
            if (platformHandle is IMacOSTopLevelPlatformHandle macOSHandle) {
                e.WindowInfo.SetAsWindowless(macOSHandle.GetNSWindowRetained());
            } else {
                e.WindowInfo.SetAsWindowless(platformHandle.Handle);
            }

            e.Client = this.Client;
        }
#endif

        private void ReceivedGlobalMessega(MpMessageType msg) {
            //switch (msg) {
            //case MpMessageType.SelectNextMatch:
            //    var navNextMsg = new MpQuillContentSearchRangeNavigationMessage() { curIdxOffset = 1 };
            //    SendMessage($"searchNavOffsetChanged_ext('{navNextMsg.SerializeJsonObjectToBase64()}')");
            //    break;
            //case MpMessageType.SelectPreviousMatch:
            //    var navPrevMsg = new MpQuillContentSearchRangeNavigationMessage() { curIdxOffset = -1 };
            //    SendMessage($"searchNavOffsetChanged_ext('{navPrevMsg.SerializeJsonObjectToBase64()}')");
            //    break;

            //}
        }

        #endregion

        #region MpAvIWebViewBindingResponseHandler Implementation

        void MpAvIWebViewBindingResponseHandler.HandleBindingNotification(MpAvEditorBindingFunctionType notificationType, string msgJsonBase64Str) {
            var ctvm = BindingContext;
            if (ctvm == null &&
                notificationType != MpAvEditorBindingFunctionType.notifyDomLoaded &&
                notificationType != MpAvEditorBindingFunctionType.notifyInitComplete) {
                // converter doesn't have data context but needs to notify dom loaded which doesn't need it
                return;
            }
            MpJsonObject ntf = null;
            switch (notificationType) {

                // LIFE CYCLE

                case MpAvEditorBindingFunctionType.notifyDomLoaded:
                    IsDomLoaded = true;
                    break;
                case MpAvEditorBindingFunctionType.notifyInitComplete:
                    IsEditorInitialized = true;
                    break;

                case MpAvEditorBindingFunctionType.notifyLoadComplete:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage loadComplete_ntf) {
                        IsContentLoaded = true;
                        ProcessContentChangedMessage(loadComplete_ntf);
                        //OnAppendModeStateChanged("editor");
                    }
                    break;


                // CONTENT CHANGED

                case MpAvEditorBindingFunctionType.notifyReadOnlyDisabled:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillDisableReadOnlyResponseMessage disableReadOnlyMsg) {
                        ctvm.IsContentReadOnly = false;
                        ctvm.UnconstrainedContentDimensions = new MpSize(disableReadOnlyMsg.editorWidth, disableReadOnlyMsg.editorHeight);
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

                // SELECTION

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
                            .SelectedTransaction
                            .SelectChildCommand.Execute(annSelectedMsg.annotationGuid);
                    }
                    break;

                // MODAL SYNC

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


                // CLIPBOARD

                case MpAvEditorBindingFunctionType.notifySetClipboardRequested:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillEditorSetClipboardRequestNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorSetClipboardRequestNotification setClipboardReq) {
                        ctvm.CopyToClipboardCommand.Execute(null);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyPasteRequest:
                    MpAvClipTrayViewModel.Instance.PasteFromClipTilePasteButtonCommand.Execute(BindingContext);
                    break;

                // DND

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

                // HIGHLIGHTING

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

                // TEMPLATES

                case MpAvEditorBindingFunctionType.notifyAddOrUpdateTemplate:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillTemplateAddOrUpdateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillTemplateAddOrUpdateNotification addOrUpdateTemplateMsg) {
                        var t = MpJsonConverter.DeserializeBase64Object<MpTextTemplate>(addOrUpdateTemplateMsg.addedOrUpdatedTextTemplateBase64JsonStr);
                        if (t.IsInputTypeTemplate()) {
                            // ignore, no point persisting input templates since they're only relevant during a paste 
                            MpConsole.WriteLine($"Ignoring addOrUpdate INPUT template: '{t}'");
                            return;
                        }
                        Task.Run(async () => {
                            int tid = await MpDataModelProvider.GetTextTemplateIdByGuidAsync(t.Guid);
                            t.Id = tid;
                            await t.WriteToDatabaseAsync();
                            MpConsole.WriteLine($"Template '{t}': {(tid == 0 ? "Added" : "Updated")}");
                        }).FireAndForgetSafeAsync(ctvm);
                    }

                    break;
                case MpAvEditorBindingFunctionType.notifyUserDeletedTemplate:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillUserDeletedTemplateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillUserDeletedTemplateNotification deleteTemplateMsg) {
                        Task.Run(async () => {
                            var t = await MpDataModelProvider.GetTextTemplateByGuidAsync(deleteTemplateMsg.userDeletedTemplateGuid);
                            if (t == null) {
                                MpConsole.WriteLine($"Template not found to delete. Guid '{deleteTemplateMsg.userDeletedTemplateGuid}' Tile: '{ctvm}'");
                                return;
                            }
                            if (t.IsInputTypeTemplate()) {
                                // shouldn't exist
                                MpDebug.Break();
                            }
                            await t.DeleteFromDatabaseAsync();
                            MpConsole.WriteLine($"Template '{t}': DELETED");
                        }).FireAndForgetSafeAsync(ctvm);
                    }
                    break;

                // WINDOW ACTIONS

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
                        var uri = new Uri(HttpUtility.HtmlDecode(navUriReq.uri), UriKind.Absolute);
                        MpAvUriNavigator.NavigateToUri(uri);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyInternalContextMenuIsVisibleChanged:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillInternalContextIsVisibleChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillInternalContextIsVisibleChangedNotification ctxMenuChangedMsg) {
                        ctvm.CanShowContextMenu = !ctxMenuChangedMsg.isInternalContextMenuVisible;
                    }
                    break;

                // OTHER

                case MpAvEditorBindingFunctionType.notifyShowDebugger:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillShowDebuggerNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowDebuggerNotification showDebugNtf) {
                        MpConsole.WriteLine($"[{ctvm}] {showDebugNtf.reason}");
                        this.ShowDevTools();
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyException:
                    ntf = MpJsonConverter.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillExceptionMessage exceptionMsgObj) {
                        MpConsole.WriteLine($"[{ctvm}] {exceptionMsgObj}");
                        //MpDebug.Break();
                    }
                    break;

                // GET CALLBACKS

                case MpAvEditorBindingFunctionType.getDragDataTransferObject:
                case MpAvEditorBindingFunctionType.getClipboardDataTransferObject:
                case MpAvEditorBindingFunctionType.getAllNonInputTemplatesFromDb:
                    HandleBindingGetRequest(notificationType, msgJsonBase64Str).FireAndForgetSafeAsync(ctvm);
                    break;
            }

            //MpConsole.WriteLine($"Tile {ctvm} received cef notification type '{notificationType}' w/ msg:",true);
            //MpConsole.WriteLine($"'{(ntf == null ? "NO DATA RECEIVED":ntf.ToPrettyPrintJsonString())}'", false, true);
        }

        private async Task HandleBindingGetRequest(MpAvEditorBindingFunctionType getReqType, string msgJsonBase64) {
            var getReq = MpJsonConverter.DeserializeBase64Object<MpQuillGetRequestNotification>(msgJsonBase64);
            var getResp = new MpQuillGetResponseNotification() { requestGuid = getReq.requestGuid };
            switch (getReqType) {
                case MpAvEditorBindingFunctionType.getAllNonInputTemplatesFromDb:
                    var templateReq = MpJsonConverter.DeserializeObject<MpQuillTemplateDbQueryRequestMessage>(getReq.reqMsgFragmentJsonStr);
                    var tl = await MpDataModelProvider.GetTextTemplatesByType(templateReq.templateTypes.Select(x => x.ToEnum<MpTextTemplateType>()));

                    getResp.responseFragmentJsonStr = MpJsonConverter.SerializeObject(tl);
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
            if (_locatedDateTime == null && this is not MpAvPlainHtmlConverterWebView) {
                // is this called before attached to logical tree?
                MpDebug.Break();
            }
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

        protected virtual void OnIsEditorInitializedChanged() {

        }
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
                if (this.GetVisualRoot() == App.MainView) {
                    return MpAvClipTrayViewModel.EditorUri;
                }
                return $"{MpAvClipTrayViewModel.EditorUri}?{APPEND_NOTIFIER_URL_PARAMS}";
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
                envName = Mp.Services.PlatformInfo.OsType.ToString()
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

        private bool _isContentLoaded;
        public bool IsContentLoaded {
            get { return _isContentLoaded; }
            set { SetAndRaise(IsContentLoadedProperty, ref _isContentLoaded, value); }
        }

        public static DirectProperty<MpAvContentWebView, bool> IsContentLoadedProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, bool>(
                nameof(IsContentLoaded),
                x => x.IsContentLoaded,
                (x, o) => x.IsContentLoaded = o,
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

        public async Task PerformUpdateContentRequestAsync(MpJsonObject jsonObj) {
            Dispatcher.UIThread.VerifyAccess();
            await Task.Delay(1);
            if (!IsEditorInitialized || !IsContentLoaded) {
                // which is it? what's the state of tile?
                MpDebug.Break();
                return;
            }
            var req = new MpQuillUpdateContentRequestMessage();
            if (jsonObj is MpQuillDelta) {
                req.deltaFragmentStr = jsonObj.SerializeJsonObjectToBase64();
            } else if (jsonObj is MpAnnotationNodeFormat) {
                req.annotationFragmentStr = jsonObj.SerializeJsonObjectToBase64();
            } else {
                return;
            }
            SendMessage($"updateContents_ext('{req.SerializeJsonObjectToBase64()}')");
        }

        public async Task PerformLoadContentRequestAsync() {
            Dispatcher.UIThread.VerifyAccess();

            IsContentLoaded = false;

            if (this.PendingEvalCount() > 0) {
                this.NeedsEvalJsCleared = true;
                while (NeedsEvalJsCleared) {
                    await Task.Delay(100);
                }
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
                //annotationsJsonStr = BindingContext.AnnotationsJsonStr
            };

            var searches =
                Mp.Services.Query.Infos
                .Where(x => !string.IsNullOrEmpty(x.MatchValue))
                .Select(x => new MpQuillContentSearchRequestMessage() {
                    searchText = x.MatchValue,
                    isCaseSensitive = x.QueryFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive),
                    isWholeWordMatch = x.QueryFlags.HasFlag(MpContentQueryBitFlags.WholeWord),
                    useRegEx = x.QueryFlags.HasFlag(MpContentQueryBitFlags.Regex)
                });
            loadContentMsg.searchesFragment =
                searches.Any() ?
                new MpQuillContentSearchesFragment() {
                    searches = searches.ToList()
                }.SerializeJsonObjectToBase64() : null;

            string msgStr = loadContentMsg.SerializeJsonObjectToBase64();

            SendMessage($"loadContent_ext('{msgStr}')");
        }

        private void ProcessContentChangedMessage(MpQuillEditorContentChangedMessage contentChanged_ntf) {
            bool is_reload = BindingContext.PublicHandle == _lastLoadedContentHandle;
            _lastLoadedContentHandle = BindingContext.PublicHandle;

            if (!IsContentLoaded) {
                //MpDebug.Break();
            }

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

            bool hasSizeChanged = false;
            if (contentChanged_ntf.length > 0 &&
                BindingContext.CharCount != contentChanged_ntf.length) {
                hasSizeChanged = true;
                BindingContext.CharCount = contentChanged_ntf.length;
            }
            if (contentChanged_ntf.lines > 0 &&
                BindingContext.LineCount != contentChanged_ntf.lines) {
                hasSizeChanged = true;
                BindingContext.LineCount = contentChanged_ntf.lines;
            }
            if (hasSizeChanged) {
                BindingContext.CopyItemSize = new MpSize(BindingContext.CharCount, BindingContext.LineCount);
            }

            if (contentChanged_ntf.itemData != null) {
                if (contentChanged_ntf.itemData.IsEmptyRichHtmlString()) {
                    // data's getting reset again
                    MpDebug.Break();
                }
                BindingContext.CopyItemData = contentChanged_ntf.itemData;
            }
            if (contentChanged_ntf.editorHeight > 0 &&
                contentChanged_ntf.editorHeight > 0) {
                var new_size = new MpSize(contentChanged_ntf.editorWidth, contentChanged_ntf.editorHeight);
                if (!new_size.IsValueEqual(BindingContext.UnconstrainedContentDimensions)) {
                    BindingContext.UnconstrainedContentDimensions = new_size;
                }
            }
            BindingContext.HasTemplates = contentChanged_ntf.hasTemplates;

            IsContentLoaded = true;
        }

        private async Task ProcessDataTransferCompleteResponse(MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
            var dtobj = MpJsonConverter.DeserializeBase64Object<MpQuillHostDataItemsMessage>(dataTransferCompleted_ntf.sourceDataItemsJsonStr);
            MpPortableDataObject req_mpdo = dtobj.ToAvDataObject();

            string resp_json = null;
            if (!string.IsNullOrEmpty(dataTransferCompleted_ntf.changeDeltaJsonStr)) {
                resp_json = dataTransferCompleted_ntf.changeDeltaJsonStr.ToStringFromBase64();
            }

            IEnumerable<string> refs = null;
            if (req_mpdo != null) {
                var other_refs = await Mp.Services.SourceRefBuilder.GatherSourceRefsAsync(req_mpdo);
                refs = other_refs.Select(x => Mp.Services.SourceRefBuilder.ConvertToRefUrl(x));
            }
            MpTransactionType transType = dataTransferCompleted_ntf.transferLabel.ToEnum<MpTransactionType>();

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
                ref_urls: refs,
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
                !IsContentLoaded ||
                MpAvFocusManager.Instance.IsInputControlFocused) {
                return;
            }
            var msg = new MpQuillIsHostFocusedChangedMessage() {
                isHostFocused = IsContentSelected
            };
            if (IsContentSelected) {
                this.Focus();
            }
            SendMessage($"hostIsFocusedChanged_ext('{msg.SerializeJsonObjectToBase64()}')");
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
            if (BindingContext == null || !IsContentLoaded) {
                return;
            }
            if (IsContentResizing) {
                SendMessage($"disableWindowResizeUpdate_ext()");
            } else {
                SendMessage($"enableWindowResizeUpdate_ext()"); ;
            }
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
            if (BindingContext == null || !IsContentLoaded) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                if (IsContentReadOnly) {
                    MpAvMainWindowViewModel.Instance.IsAnyMainWindowTextBoxFocused = false;

                    MpAvResizeExtension.ResizeAnimated(this, BindingContext.ReadOnlyWidth, BindingContext.ReadOnlyHeight);
                    string enableReadOnlyRespStr = await SendMessageAsync("enableReadOnly_ext()");
                    var qrm = MpJsonConverter.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(enableReadOnlyRespStr);
                    ProcessContentChangedMessage(qrm);
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
                !IsContentLoaded ||
                !IsContentReadOnly) {
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
                !IsContentLoaded) {
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

        #region AppendModeState Property

        private MpAppendModeFlags _appendModeState;
        public MpAppendModeFlags AppendModeState {
            get { return _appendModeState; }
            set { SetAndRaise(AppendModeStateProperty, ref _appendModeState, value); }
        }

        public static DirectProperty<MpAvContentWebView, MpAppendModeFlags> AppendModeStateProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, MpAppendModeFlags>(
                nameof(AppendModeState),
                x => x.AppendModeState,
                (x, o) => x.AppendModeState = o,
                MpAppendModeFlags.None,
                BindingMode.TwoWay);

        #endregion

        #region AppendData Property

        private string _appendData;
        public string AppendData {
            get { return _appendData; }
            set { SetAndRaise(AppendDataProperty, ref _appendData, value); }
        }

        public static DirectProperty<MpAvContentWebView, string> AppendDataProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentWebView, string>(
                nameof(AppendData),
                x => x.AppendData,
                (x, o) => x.AppendData = o,
                null,
                BindingMode.TwoWay);


        #endregion

        private void OnAppendModeStateChanged(string source) {
            if (BindingContext == null ||
                !BindingContext.IsAppendNotifier) {
                return;
            }

            // only called when mode changed in tray so the processState() ignores it (source is not 'editor')
            // except relaying it to tray tile (if present)
            var ctrvm = MpAvClipTrayViewModel.Instance;
            MpConsole.WriteLine($"AppendModeState changed. AppendModeState: {AppendModeState}");
            var reqMsg = new MpQuillAppendStateChangedMessage() {
                isAppendLineMode = AppendModeState.HasFlag(MpAppendModeFlags.AppendLine),
                isAppendMode = AppendModeState.HasFlag(MpAppendModeFlags.Append),
                isAppendManualMode = AppendModeState.HasFlag(MpAppendModeFlags.Manual),
            };
            ProcessAppendStateChangedMessage(reqMsg, source);
        }

        private void OnAppendDataChanged() {
            if (BindingContext == null ||
                string.IsNullOrEmpty(AppendData) ||
                !BindingContext.IsAppendNotifier) {
                return;
            }

            MpConsole.WriteLine($"AppendData changed. AppendData: {AppendData}");
            var req = new MpQuillAppendStateChangedMessage() {

                isAppendLineMode = AppendModeState.HasFlag(MpAppendModeFlags.AppendLine),
                isAppendMode = AppendModeState.HasFlag(MpAppendModeFlags.Append),
                isAppendManualMode = AppendModeState.HasFlag(MpAppendModeFlags.Manual),
                appendData = AppendData
            };
            SendMessage($"appendDataToNotifier_ext('{req.SerializeJsonObjectToBase64()}')");

            AppendData = null;

        }

        private void ProcessAppendStateChangedMessage(MpQuillAppendStateChangedMessage appendChangedMsg, string source) {
            Dispatcher.UIThread.Post(async () => {
                var ctrvm = MpAvClipTrayViewModel.Instance;
                var cur_append_tile = ctrvm.AppendClipTileViewModel;

                if (source == "editor") {
                    // no matter source if mode has a true and doesn't match tray update tray
                    // only disable once message from notifier has no true modes 
                    // only show ntf if it was an appendData msg, the rest delegated to tray
                    if (appendChangedMsg.isAppendLineMode && !ctrvm.IsAppendLineMode) {
                        ctrvm.ToggleAppendLineModeCommand.Execute(null);
                    } else if (appendChangedMsg.isAppendMode && !ctrvm.IsAppendMode) {
                        ctrvm.ToggleAppendModeCommand.Execute(null);
                    } else if (BindingContext.IsAppendNotifier &&
                        (!appendChangedMsg.isAppendLineMode && !appendChangedMsg.isAppendMode && ctrvm.IsAnyAppendMode)) {
                        // only let notifier deactivate
                        await ctrvm.DeactivateAppendModeAsync();
                    } else if (BindingContext.IsAppendNotifier &&
                                !string.IsNullOrEmpty(appendChangedMsg.appendData)) {
                        MpNotificationBuilder.ShowNotificationAsync(MpNotificationType.AppendChanged).FireAndForgetSafeAsync();
                    }
                }
                while (ctrvm.IsAnyBusy) {
                    await Task.Delay(100);
                }
                while (ctrvm.IsAddingClipboardItem) {
                    await Task.Delay(100);
                }
                RelayMsg($"appendStateChanged_ext('{appendChangedMsg.SerializeJsonObjectToBase64()}')").FireAndForgetSafeAsync();
            });
        }

        private async Task RelayMsg(string msg) {
            await Task.Delay(1);
            MpAvContentWebView dest_wv = null;
            if (BindingContext.IsAppendNotifier) {
                // relay to tray tile
                dest_wv = LocateTrayTileWebView(BindingContext.CopyItemId);

            } else if (BindingContext.IsAppendTrayItem) {
                // relay to notifier
                dest_wv = LocateModalWebView();
            }
            if (dest_wv == null) {
                return;
            }
            dest_wv.SendMessage(msg);
        }
        #endregion

    }
}
