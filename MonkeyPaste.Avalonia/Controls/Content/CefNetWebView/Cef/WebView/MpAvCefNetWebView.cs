using Avalonia.Threading;
using CefNet;
using CefNet.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using Avalonia;
using MonkeyPaste.Common;
using Avalonia.Interactivity;
using CefNet.Internal;
using Avalonia.Controls;
using Avalonia.Platform;
using System.Web;
using Xamarin.Essentials;
using Avalonia.LogicalTree;
using Pango;
using System.Collections;
using Avalonia.Data;

namespace MonkeyPaste.Avalonia {

    public enum MpAvEditorBindingFunctionType {
        // two-way *_get async requests
        getDragData,
        getAllNonInputTemplatesFromDb, 
        getClipboardDataTransferObject,

        // one-way *_ntf notifications
        notifyDocSelectionChanged,
        notifyContentChanged,
        notifySubSelectionEnabledChanged,
        notifyDropEffectChanged,
        notifyException,
        notifyDragStartOrEnd,
        notifyReadOnlyEnabled,
        notifyReadOnlyDisabled, 
        notifyInitComplete,
        notifyDomLoaded,
        notifyDropCompleted,
        notifyDragEnter,
        notifyDragLeave,
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
        notifyPasteIsReady,
        notifyDataTransferCompleted,
        notifySelectionChanged,
        notifyScrollChanged,
        notifyAppendModeChanged,
        notifyInternalContextMenuIsVisibleChanged
    }
    [DoNotNotify]
    public class MpAvCefNetWebView : 
        WebView, 
        //MpAvIContentView, 
        MpAvIDragSource,
        MpAvIResizableControl,
        MpAvIDomStateAwareWebView,
        MpAvIAsyncJsEvalWebView,
        MpAvIReloadableContentWebView,
        MpAvIWebViewBindingResponseHandler {
        #region Private Variables
        private const int _APPEND_TIMEOUT_MS = 5000;
        private string _pastableContent_ntf { get; set; }
        private string _contentScreenShotBase64_ntf { get; set; }

        #endregion

        #region Constants

        public const string BLANK_URL = "about:blank";

        #endregion

        #region Statics

        private static List<MpAvCefNetWebView> _AllWebViews = new List<MpAvCefNetWebView>();

        public static string DefaultContentUrl => MpAvClipTrayViewModel.EditorPath;
        public static MpAvCefNetWebView LocateModalWebView() {
            return _AllWebViews.FirstOrDefault(x => x.DataContext is MpAvClipTileViewModel ctvm && ctvm.IsAppendNotifier);
        }
        public static MpAvCefNetWebView LocateTrayTileWebView(int ciid) {
            if(ciid < 1) {
                return null;
            }
            var result = _AllWebViews
                .Where(x => 
                    x.DataContext is MpAvClipTileViewModel ctvm && 
                    !ctvm.IsAppendNotifier &&
                    ctvm.CopyItemId == ciid).ToList();

            if(result.Count == 0) {
                return null;
            }
            if(result.Count > 1) {
                // is this during a pin toggle? was this item pinned?
                Debugger.Break();
            }
            return result[0];
        }

        #endregion

        #region Properties

        #region View Models
        public MpAvClipTileViewModel BindingContext {
            get {
                if(DataContext is MpAvClipTileViewModel) {
                    return DataContext as MpAvClipTileViewModel;
                }
                if(DataContext is MpNotificationViewModelBase nvmb) {
                    return nvmb.Body as MpAvClipTileViewModel;
                }
                return null;
            }
        }

        #endregion

        #region Bindings & Life Cycle


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
                if(_resizerControl == null) {
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

        public void NotifyDropComplete(DragDropEffects dropEffect) {
            var dragEndMsg = new MpQuillDragEndMessage() {
                dataTransfer = new MpQuillDataTransferMessageFragment() {
                    dropEffect = dropEffect.ToString().ToLower()
                },
                fromHost = true,
                wasCancel = dropEffect == DragDropEffects.None
            };

            this.ExecuteJavascript($"dragEnd_ext('{dragEndMsg.SerializeJsonObjectToBase64()}')");

            //IsDragging = false;
            MpConsole.WriteLine($"Drag complete for '{BindingContext}'. DropEffect: '{dropEffect}'");
        }
        public void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => (this as MpAvIDragSource).NotifyModKeyStateChanged(ctrl,alt,shift,esc));
                return;
            }
            var modKeyMsg = new MpQuillModifierKeysNotification() {
                ctrlKey = ctrl,
                altKey = alt,
                shiftKey = shift,
                escKey = esc
            };
            this.ExecuteJavascript($"updateModifierKeysFromHost_ext('{modKeyMsg.SerializeJsonObjectToBase64()}')");
        }

        public async Task<MpAvDataObject> GetDataObjectAsync(bool forOle, string[] formats = null) {
            if(BindingContext == null) {
                Debugger.Break();
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
            if (ctvm.ItemType != MpCopyItemType.Image && ignore_ss) {
                contentDataReq.formats.Remove(MpPortableDataFormats.AvPNG);
                contentDataReq.formats.Remove(MpPortableDataFormats.WinBitmap);
                contentDataReq.formats.Remove(MpPortableDataFormats.WinDib);
            }
            MpQuillContentDataResponseMessage contentDataResp = null;

            if (false) {//contentDataReq.forPaste && ctvm.HasTemplates) {
                if (ctvm.IsContentReadOnly) {
                    //var ctv = this.GetVisualAncestor<MpAvClipTileView>();
                    //if (ctv != null) {
                    //    var resizeControl = ctv.FindControl<Control>("ClipTileResizeBorder");
                    //    MpAvResizeExtension.ResizeAnimated(resizeControl, ctvm.EditableWidth, ctvm.EditableHeight);
                    //}
                    MpAvResizeExtension.ResizeAnimated(this, ctvm.EditableWidth, ctvm.EditableHeight);
                }
                _pastableContent_ntf = null;
                this.ExecuteJavascript($"contentRequest_ext('{contentDataReq.SerializeJsonObjectToBase64()}')");
                while (_pastableContent_ntf == null) {
                    await Task.Delay(100);
                }
                contentDataResp = MpJsonObject.DeserializeBase64Object<MpQuillContentDataResponseMessage>(_pastableContent_ntf);
                _pastableContent_ntf = null;
            } else {
                var contentDataRespStr = await this.EvaluateJavascriptAsync($"contentRequest_ext('{contentDataReq.SerializeJsonObjectToBase64()}')");
                contentDataResp = MpJsonObject.DeserializeBase64Object<MpQuillContentDataResponseMessage>(contentDataRespStr);
            }


            if (contentDataResp.dataItems == null) {
                return null;
            }
            var avdo = new MpAvDataObject();
            foreach (var di in contentDataResp.dataItems) {
                avdo.SetData(di.format, di.data);
            }

            if (forOle) {
                if (ctvm.ItemType == MpCopyItemType.Image) {
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

                if (ctvm.ItemType == MpCopyItemType.FileList) {
                    avdo.SetData(MpPortableDataFormats.AvFileNames, ctvm.CopyItemData.SplitNoEmpty(Environment.NewLine));
                } else if (!ignore_pseudo_file) {
                    // js doesn't set file stuff for non-files
                    avdo.SetData(
                        MpPortableDataFormats.AvFileNames,
                        ctvm.CopyItemData.ToFile(
                            forceNamePrefix: ctvm.CopyItemTitle,
                            forceExt: ctvm.ItemType == MpCopyItemType.Image ? "png" : "txt",
                            isTemporary: true));
                }

                bool add_tile_data = ctvm.ItemType != MpCopyItemType.Text || contentDataResp.isAllContent;
                if (add_tile_data) {
                    avdo.SetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT, ctvm.PublicHandle);
                }
                string ctvm_source_url = ctvm.CopyItem.ToSourceRefUrl();
                avdo.SetData(MpPortableDataFormats.CefAsciiUrl, ctvm_source_url.ToBytesFromString(Encoding.ASCII));
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

        #region State
        #endregion

        #endregion

        #region Constructors

        public MpAvCefNetWebView() : base() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessega);
            this.GetObservable(MpAvCefNetWebView.AppendDataProperty).Subscribe(value => OnAppendDataChanged());
            this.GetObservable(MpAvCefNetWebView.AppendModeStateProperty).Subscribe(value => OnAppendModeStateChanged());

            this.GetObservable(MpAvCefNetWebView.ContentDataProperty).Subscribe(value => OnContentDataChanged());
            this.GetObservable(MpAvCefNetWebView.IsContentSelectedProperty).Subscribe(value => OnIsContentSelectedChanged());
            this.GetObservable(MpAvCefNetWebView.IsContentResizingProperty).Subscribe(value => OnIsContentResizingChanged());
            this.GetObservable(MpAvCefNetWebView.IsContentReadOnlyProperty).Subscribe(value => OnIsContentReadOnlyChanged());
            this.GetObservable(MpAvCefNetWebView.IsContentSubSelectableProperty).Subscribe(value => OnIsContentSubSelectableChanged());
            this.GetObservable(MpAvCefNetWebView.IsContentFindAndReplaceVisibleProperty).Subscribe(value => OnIsContentFindOrReplaceVisibleChanged());   
        }


        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        protected override WebViewGlue CreateWebViewGlue() {
            return new MpAvCefNetWebViewGlue(this);
        }

        protected override void OnGotFocus(GotFocusEventArgs e) {
            base.OnGotFocus(e);
            if(BindingContext == null) {
                return;
            }
            if(!BindingContext.IsContentReadOnly) {
                MpAvMainWindowViewModel.Instance.IsAnyTextBoxFocused = true;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e) {
            base.OnLostFocus(e);
            if (BindingContext == null) {
                return;
            }
            if (!BindingContext.IsContentReadOnly) {
                MpAvMainWindowViewModel.Instance.IsAnyTextBoxFocused = false;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            LastPointerPressedEventArgs = e;
        }


        #endregion

        #region Private Methods

        private void MpAvCefNetWebView_CreateWindow(object sender, CreateWindowEventArgs e) {
            IPlatformHandle platformHandle = MpAvMainWindow.Instance.PlatformImpl.Handle;
            if (platformHandle is IMacOSTopLevelPlatformHandle macOSHandle)
                e.WindowInfo.SetAsWindowless(macOSHandle.GetNSWindowRetained());
            else
                e.WindowInfo.SetAsWindowless(platformHandle.Handle);

            e.Client = this.Client;
        }

        private void ReceivedGlobalMessega(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SelectNextMatch:
                    var navNextMsg = new MpQuillContentSearchRangeNavigationMessage() { curIdxOffset = 1 };
                    this.ExecuteJavascript($"searchNavOffsetChanged_ext('{navNextMsg.SerializeJsonObjectToBase64()}')");
                    break;
                case MpMessageType.SelectPreviousMatch:
                    var navPrevMsg = new MpQuillContentSearchRangeNavigationMessage() { curIdxOffset = -1 };
                    this.ExecuteJavascript($"searchNavOffsetChanged_ext('{navPrevMsg.SerializeJsonObjectToBase64()}')");
                    break;

            }
        }

        #endregion

        #region MpAvIWebViewBindingResponseHandler Implementation

        async Task MpAvIWebViewBindingResponseHandler.HandleBindingNotificationAsync(MpAvEditorBindingFunctionType notificationType, string msgJsonBase64Str) {
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
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage loadComplete_ntf) {
                        IsContentLoaded = true;
                        ProcessContentChangedMessage(loadComplete_ntf);
                    }
                    break;


                // CONTENT CHANGED

                case MpAvEditorBindingFunctionType.notifyReadOnlyDisabled:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillDisableReadOnlyResponseMessage disableReadOnlyMsg) {
                        ctvm.IsContentReadOnly = false;
                        ctvm.UnformattedContentSize = new MpSize(disableReadOnlyMsg.editorWidth, disableReadOnlyMsg.editorHeight);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyReadOnlyEnabled:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage enableReadOnlyMsg) {
                        // NOTE only difference from contentChanged is no dimension info and this needs to enable readonly
                        ctvm.IsContentReadOnly = true;
                        ProcessContentChangedMessage(enableReadOnlyMsg);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyContentChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage contentChanged_ntf) {
                        ProcessContentChangedMessage(contentChanged_ntf);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyDataTransferCompleted:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillDataTransferCompletedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
                        MpISourceRef sourceRef = null;
                        bool had_internal_handle = false;
                        if (!string.IsNullOrEmpty(dataTransferCompleted_ntf.dataTransferSourceUrl)) {
                            var sr = MpSourceRef.ParseFromInternalUrl(dataTransferCompleted_ntf.dataTransferSourceUrl);
                            if (sr != null) {
                                if (!string.IsNullOrEmpty(sr.SourcePublicHandle)) {
                                    // get db id from handle
                                    had_internal_handle = true;
                                    int ciid = 0;
                                    // check all tiles
                                    var sctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == sr.SourcePublicHandle) as MpAvClipTileViewModel;
                                    if (sctvm == null) {
                                        // check for recycled tile
                                        if (MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count > 0 &&
                                            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels[0].PublicHandle == sr.SourcePublicHandle) {
                                            // recycled source (internal)
                                            ciid = MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels[0].Id;
                                        }
                                        if(ciid == 0) {
                                            // check pending new models
                                            var pending_ci = MpAvClipTrayViewModel.Instance.PendingNewModels.FirstOrDefault(x => x.PublicHandle.ToLower() == sr.SourcePublicHandle.ToLower());
                                            if(pending_ci != null) {
                                                // pending source (internal)
                                                ciid = pending_ci.Id;
                                            }
                                        }
                                    } else {
                                        // tile source (internal)
                                        ciid = sctvm.CopyItemId;
                                    }
                                    if (ciid > 0) {
                                        // internal source
                                        sr.SourceObjId = ciid;
                                        sourceRef = sr;
                                    }
                                }
                            }
                        }
                        if (sourceRef == null && !had_internal_handle) {
                            var url = await MpPlatformWrapper.Services.UrlBuilder.CreateAsync(dataTransferCompleted_ntf.dataTransferSourceUrl);
                            if (url != null) {
                                // remote source
                                sourceRef = url;
                            }
                        }
                        if (sourceRef == null) {
                            var app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo);
                            if (app != null) {
                                // local source
                                sourceRef = app;
                            }
                        }
                        ctvm.AddSourceRefAsync(sourceRef).FireAndForgetSafeAsync(ctvm);
                    }
                    break;

                // SELECTION

                case MpAvEditorBindingFunctionType.notifySubSelectionEnabledChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillSubSelectionChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillSubSelectionChangedNotification subSelChangedNtf) {
                        ctvm.IsSubSelectionEnabled = subSelChangedNtf.isSubSelectionEnabled;
                    }
                    break;

                // MODAL SYNC

                case MpAvEditorBindingFunctionType.notifySelectionChanged:
                    if(!ctvm.IsAppendNotifier && !ctvm.IsAppendTrayItem) {
                        break;
                    }
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillSelectionChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillSelectionChangedMessage selChangedMsg) {
                        MpAvCefNetWebView dest_wv = null;
                        if(ctvm.IsAppendNotifier) {
                            // relay sel to tray
                            dest_wv = LocateTrayTileWebView(ctvm.CopyItemId);
                        } else {
                            // relay to modal
                            dest_wv = LocateModalWebView();
                        }
                        if (dest_wv == null) {
                            break;
                        }
                        dest_wv.ExecuteJavascript($"setSelection_ext('{msgJsonBase64Str}')");
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyScrollChanged:
                    if (!ctvm.IsAppendNotifier && !ctvm.IsAppendTrayItem) {
                        break;
                    }
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillScrollChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillScrollChangedMessage scrollChangedMsg) {
                        MpAvCefNetWebView dest_wv = null;
                        if (ctvm.IsAppendNotifier) {
                            // relay sel to tray
                            dest_wv = LocateTrayTileWebView(ctvm.CopyItemId);
                        } else {
                            // relay to modal
                            dest_wv = LocateModalWebView();
                        }
                        if (dest_wv == null) {
                            break;
                        }
                        dest_wv.ExecuteJavascript($"setScroll_ext('{msgJsonBase64Str}')");
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyAppendModeChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillAppendModeChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillAppendModeChangedMessage appendModeChangedMsg) {
                        ProcessAppendModeChangedAsync(appendModeChangedMsg).FireAndForgetSafeAsync();
                    }
                    break;


                // CLIPBOARD

                case MpAvEditorBindingFunctionType.notifySetClipboardRequested:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillEditorSetClipboardRequestNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorSetClipboardRequestNotification setClipboardReq) {
                        ctvm.CopyToClipboardCommand.Execute(null);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyPasteRequest:
                    MpAvClipTrayViewModel.Instance.PasteFromClipTilePasteButtonCommand.Execute(BindingContext);
                    break;
                case MpAvEditorBindingFunctionType.notifyPasteIsReady:
                    // NOTE picked up in Document.GetDataObject
                    _pastableContent_ntf = msgJsonBase64Str;
                    break;

                // DND

                case MpAvEditorBindingFunctionType.notifyDragEnter:
                    BindingContext.IsDropOverTile = true;
                    break;
                case MpAvEditorBindingFunctionType.notifyDragLeave:
                    BindingContext.IsDropOverTile = false;
                    break;

                case MpAvEditorBindingFunctionType.notifyDropCompleted:
                    BindingContext.IsDropOverTile = false;
                    MpAvClipTrayViewModel.Instance.SelectedItem = BindingContext;
                    break;

                // HIGHLIGHTING

                case MpAvEditorBindingFunctionType.notifyFindReplaceVisibleChange:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillContentFindReplaceVisibleChanedNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentFindReplaceVisibleChanedNotificationMessage findReplaceMsgObj) {
                        ctvm.IsFindAndReplaceVisible = findReplaceMsgObj.isFindReplaceVisible;
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyQuerySearchRangesChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillContentQuerySearchRangesChangedNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentQuerySearchRangesChangedNotificationMessage searchRangeCountMsg) {
                        if (searchRangeCountMsg.rangeCount > 1) {
                            MpAvSearchBoxViewModel.Instance.NotifyHasMultipleMatches();
                        }
                    }
                    break;

                case MpAvEditorBindingFunctionType.notifyContentScreenShot:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillContentScreenShotNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentScreenShotNotificationMessage ssMsg) {
                        _contentScreenShotBase64_ntf = ssMsg.contentScreenShotBase64;
                    }
                    break;

                // TEMPLATES

                case MpAvEditorBindingFunctionType.notifyAddOrUpdateTemplate:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillTemplateAddOrUpdateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillTemplateAddOrUpdateNotification addOrUpdateTemplateMsg) {
                        var t = MpJsonObject.DeserializeBase64Object<MpTextTemplate>(addOrUpdateTemplateMsg.addedOrUpdatedTextTemplateBase64JsonStr);
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
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillUserDeletedTemplateNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillUserDeletedTemplateNotification deleteTemplateMsg) {
                        Task.Run(async () => {
                            var t = await MpDataModelProvider.GetTextTemplateByGuidAsync(deleteTemplateMsg.userDeletedTemplateGuid);
                            if (t == null) {
                                MpConsole.WriteLine($"Template not found to delete. Guid '{deleteTemplateMsg.userDeletedTemplateGuid}' Tile: '{ctvm}'");
                                return;
                            }
                            if (t.IsInputTypeTemplate()) {
                                // shouldn't exist
                                Debugger.Break();
                            }
                            await t.DeleteFromDatabaseAsync();
                            MpConsole.WriteLine($"Template '{t}': DELETED");
                        }).FireAndForgetSafeAsync(ctvm);
                    }
                    break;

                // WINDOW ACTIONS

                case MpAvEditorBindingFunctionType.notifyShowCustomColorPicker:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillShowCustomColorPickerNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillShowCustomColorPickerNotification showCustomColorPickerMsg) {
                        Dispatcher.UIThread.Post(async () => {

                            if (string.IsNullOrWhiteSpace(showCustomColorPickerMsg.pickerTitle)) {
                                // editor should provide title for templates but for content set to title here if ya want (may
                                showCustomColorPickerMsg.pickerTitle = $"Pick a color, any color for '{ctvm.CopyItemTitle}'";
                            }
                            string pickerResult = await MpPlatformWrapper.Services.CustomColorChooserMenuAsync.ShowCustomColorMenuAsync(
                                showCustomColorPickerMsg.currentHexColor,
                                showCustomColorPickerMsg.pickerTitle,
                                null);

                            var resp = new MpQuillCustomColorResultMessage() {
                                customColorResult = pickerResult
                            };
                            this.ExecuteJavascript($"provideCustomColorPickerResult_ext('{resp.SerializeJsonObjectToBase64()}')");
                        });
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyNavigateUriRequested:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillNavigateUriRequestNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillNavigateUriRequestNotification navUriReq) {
                        var uri = new Uri(HttpUtility.HtmlDecode(navUriReq.uri), UriKind.Absolute);
                        MpAvUriNavigator.NavigateToUri(uri);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyInternalContextMenuIsVisibleChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillInternalContextIsVisibleChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillInternalContextIsVisibleChangedNotification ctxMenuChangedMsg) {
                        
                    }
                    break;

                // OTHER

                case MpAvEditorBindingFunctionType.notifyException:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillExceptionMessage exceptionMsgObj) {
                        MpConsole.WriteLine($"[{ctvm}] {exceptionMsgObj}");
                        //Debugger.Break();
                    }
                    break;

                // GET CALLBACKS

                case MpAvEditorBindingFunctionType.getClipboardDataTransferObject:
                case MpAvEditorBindingFunctionType.getAllNonInputTemplatesFromDb:
                    HandleBindingGetRequest(notificationType, msgJsonBase64Str).FireAndForgetSafeAsync(ctvm);
                    break;
            }

            //MpConsole.WriteLine($"Tile {ctvm} received cef notification type '{notificationType}' w/ msg:",true);
            //MpConsole.WriteLine($"'{(ntf == null ? "NO DATA RECEIVED":ntf.ToPrettyPrintJsonString())}'", false, true);
        }

        private async Task HandleBindingGetRequest(MpAvEditorBindingFunctionType getReqType, string msgJsonBase64) {
            var getReq = MpJsonObject.DeserializeBase64Object<MpQuillGetRequestNotification>(msgJsonBase64);
            var getResp = new MpQuillGetResponseNotification() { requestGuid = getReq.requestGuid };
            switch (getReqType) {
                case MpAvEditorBindingFunctionType.getAllNonInputTemplatesFromDb:
                    var templateReq = MpJsonObject.DeserializeObject<MpQuillTemplateDbQueryRequestMessage>(getReq.reqMsgFragmentJsonStr);
                    var tl = await MpDataModelProvider.GetTextTemplatesByType(templateReq.templateTypes.Select(x => x.ToEnum<MpTextTemplateType>()));

                    getResp.responseFragmentJsonStr = MpJsonObject.SerializeObject(tl);
                    break;
                case MpAvEditorBindingFunctionType.getClipboardDataTransferObject:
                    var dtObjReq = MpJsonObject.DeserializeObject<MpQuillEditorDataTransferObjectRequestNotification>(getReq.reqMsgFragmentJsonStr);
                    var avdo = await MpPlatformWrapper.Services.DataObjectHelperAsync.GetPlatformClipboardDataObjectAsync(false) as IDataObject;
                    if(avdo == null) {
                        avdo = new MpAvDataObject();
                    }
                    var dil = new List<MpQuillContentDataResponseFormattedDataItemFragment>();
                    foreach (var format in avdo.GetAllDataFormats()) {
                        string data = null;
                        if(avdo.Get(format) is byte[] bytes &&
                            bytes.ToBase64String() is string bytesStr) {
                            data = bytesStr;
                        } else if(avdo.Get(format) is IEnumerable<string> strs &&
                            string.Join(Environment.NewLine,strs) is string strSet) {
                            data = strSet;
                        } else {
                            data = avdo.Get(format).ToString();
                        }
                        if(data == null) {
                            continue;
                        }
                        dil.Add(new MpQuillContentDataResponseFormattedDataItemFragment() { format = format, data = data });
                    }

                    var dtObjResp = new MpQuillEditorDataTransferObjectResponseNotification() {
                        dataItems = dil
                    };
                    getResp.responseFragmentJsonStr = MpJsonObject.SerializeObject(dtObjResp);
                    break;
            }

            if(string.IsNullOrEmpty(getResp.responseFragmentJsonStr)) {
                // no data to return
                return;
            }

            this.ExecuteJavascript($"getRequestResponse_ext('{getResp.SerializeJsonObjectToBase64()}')");
        }

        #endregion

        #region Control Life Cycle


        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e) {
            base.OnAttachedToLogicalTree(e);

            if (_AllWebViews.Contains(this)) {
                // should only happen once
                Debugger.Break();
                return;
            }
            _AllWebViews.Add(this);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e) {
            base.OnDetachedFromLogicalTree(e);
            _AllWebViews.Remove(this);
            _resizerControl = null;

        }
        #endregion

        #region Dom Init

        public bool IsEditorInitialized { get; private set; } = false;

        #region IsDomLoaded Property

        private bool _isDomLoaded = false;
        public bool IsDomLoaded {
            get { return _isDomLoaded; }
            set { SetAndRaise(IsDomLoadedProperty, ref _isDomLoaded, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, bool> IsDomLoadedProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, bool>(
                nameof(IsDomLoaded),
                x => x.IsDomLoaded,
                (x, o) => x.IsDomLoaded = o);

        #endregion IsDomLoaded Property

        public virtual string ContentUrl => MpAvClipTrayViewModel.EditorPath;


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

        private async Task LoadEditorAsync() {
            Dispatcher.UIThread.VerifyAccess();

            while (!IsDomLoaded) {
                // wait for Navigate(EditorPath)
                await Task.Delay(100);
            }

            var req = new MpQuillInitMainRequestMessage() {
                envName = MpPlatformWrapper.Services.OsInfo.OsType.ToString()
            };
            this.ExecuteJavascript($"initMain_ext('{req.SerializeJsonObjectToBase64()}')");
        }

        #endregion

        #region Content Life Cycle

        public bool NeedsEvalJsCleared { get; set; }

        #region ContentData Property

        private string _contentData;
        public string ContentData {
            get { return _contentData; }
            set { SetAndRaise(ContentDataProperty, ref _contentData, value); }
        }
        public static DirectProperty<MpAvCefNetWebView, string> ContentDataProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, string>(
                nameof(ContentData),
                x => x.ContentData,
                (x, o) => x.ContentData = o);

        #endregion 

        #region IsContentLoaded Property

        private bool _isContentLoaded;
        public bool IsContentLoaded {
            get { return _isContentLoaded; }
            set { SetAndRaise(IsContentLoadedProperty, ref _isContentLoaded, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, bool> IsContentLoadedProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, bool>(
                nameof(IsContentLoaded),
                x => x.IsContentLoaded,
                (x, o) => x.IsContentLoaded = o,
                false,
                BindingMode.TwoWay);

        #endregion 

        private void OnContentDataChanged() {
            if(BindingContext == null) {
                return;
            }

            PerformLoadContentRequestAsync().FireAndForgetSafeAsync();
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

            MpQuillLoadContentRequestMessage loadContentMsg = CreateLoadContentRequestMessage();
            string msgStr = loadContentMsg.SerializeJsonObjectToBase64();

            this.ExecuteJavascript($"loadContent_ext('{msgStr}')");
        }
        private static int test = 0;
        protected virtual MpQuillLoadContentRequestMessage CreateLoadContentRequestMessage() {
            //if(BindingContext.CopyItemTitle == "Untitled1579") {
            //    test++;
            //    if(test > 1) {
            //        Debugger.Break();
            //    }
            //}
            var loadContentMsg = new MpQuillLoadContentRequestMessage() {
                contentHandle = BindingContext.PublicHandle,
                contentType = BindingContext.ItemType.ToString(),
                itemData = ContentData,//BindingContext.EditorFormattedItemData,
                isPasteRequest = BindingContext.IsPasting
            };

            if (!string.IsNullOrEmpty(MpAvSearchBoxViewModel.Instance.SearchText)) {
                loadContentMsg.searchText = MpAvSearchBoxViewModel.Instance.SearchText;
                var sfcvm = MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel;
                loadContentMsg.isCaseSensitive = sfcvm.Filters.FirstOrDefault(x => x.FilterType == MpContentFilterType.CaseSensitive).IsChecked.IsTrue();
                loadContentMsg.isWholeWord = sfcvm.Filters.FirstOrDefault(x => x.FilterType == MpContentFilterType.WholeWord).IsChecked.IsTrue();
                loadContentMsg.useRegex = sfcvm.Filters.FirstOrDefault(x => x.FilterType == MpContentFilterType.Regex).IsChecked.IsTrue();
            }
            return loadContentMsg;
        }

        private void ProcessContentChangedMessage(MpQuillEditorContentChangedMessage contentChanged_ntf) {
            if(!IsContentLoaded) {
                //Debugger.Break();
            }

            if (contentChanged_ntf == null) {
                // shouldn't be null
                Debugger.Break();
                return;
            }
            if (contentChanged_ntf.length > 0) {
                BindingContext.CharCount = contentChanged_ntf.length;
            }
            if (contentChanged_ntf.lines > 0) {
                BindingContext.LineCount = contentChanged_ntf.lines;
            }

            if (contentChanged_ntf.itemData != null) {
                BindingContext.CopyItemData = contentChanged_ntf.itemData;
            }
            if (contentChanged_ntf.editorHeight > 0 && contentChanged_ntf.editorHeight > 0) {
                BindingContext.UnformattedContentSize = new MpSize(contentChanged_ntf.editorWidth, contentChanged_ntf.editorHeight);
            }
            BindingContext.HasTemplates = contentChanged_ntf.hasTemplates;

            BindingContext.DetailCollectionViewModel.InitializeAsync().FireAndForgetSafeAsync(BindingContext);

            IsContentLoaded = true;

        }

        #endregion

        #region Content State

        #region IsContentSelected

        private bool _isContentSelected;
        public bool IsContentSelected {
            get { return _isContentSelected; }
            set { SetAndRaise(IsContentSelectedProperty, ref _isContentSelected, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, bool> IsContentSelectedProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, bool>(
                nameof(IsContentSelected),
                x => x.IsContentSelected,
                (x, o) => x.IsContentSelected = o);


        private void OnIsContentSelectedChanged() {
            if (BindingContext == null || !IsContentLoaded) {
                return;
            }
            var msg = new MpQuillIsHostFocusedChangedMessage() {
                isHostFocused = IsContentSelected
            };
            if (IsContentSelected) {
                this.Focus();
            }
            this.ExecuteJavascript($"hostIsFocusedChanged_ext('{msg.SerializeJsonObjectToBase64()}')");
        }

        #endregion IsContentSelected

        #region IsContentResizing 

        private bool _isContentResizing;
        public bool IsContentResizing {
            get { return _isContentResizing; }
            set { SetAndRaise(IsContentResizingProperty, ref _isContentResizing, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, bool> IsContentResizingProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, bool>(
                nameof(IsContentResizing),
                x => x.IsContentResizing,
                (x, o) => x.IsContentResizing = o);

        private void OnIsContentResizingChanged() {
            if (BindingContext == null || !IsContentLoaded) {
                return;
            }
            if (IsContentResizing) {
                this.ExecuteJavascript($"disableWindowResizeUpdate_ext()");
            } else {
                this.ExecuteJavascript($"enableWindowResizeUpdate_ext()"); ;
            }
        }
        #endregion IsContentResizing 

        #region IsContentReadOnly 

        private bool _isContentReadOnly = true;
        public bool IsContentReadOnly {
            get { return _isContentReadOnly; }
            set { SetAndRaise(IsContentReadOnlyProperty, ref _isContentReadOnly, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, bool> IsContentReadOnlyProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, bool>(
                nameof(IsContentReadOnly),
                x => x.IsContentReadOnly,
                (x, o) => x.IsContentReadOnly = o,
                true,
                BindingMode.TwoWay);

        private void OnIsContentReadOnlyChanged() {
            if (BindingContext == null || !IsContentLoaded) {
                return;
            }
            if (IsContentReadOnly) {
                MpAvResizeExtension.ResizeAnimated(this, BindingContext.ReadOnlyWidth, BindingContext.ReadOnlyHeight);
                Dispatcher.UIThread.Post(async () => {
                    string enableReadOnlyRespStr = await this.EvaluateJavascriptAsync("enableReadOnly_ext()");
                    var qrm = MpJsonObject.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(enableReadOnlyRespStr);
                    ProcessContentChangedMessage(qrm);
                });
            } else {
                this.ExecuteJavascript($"disableReadOnly_ext()");
                MpAvResizeExtension.ResizeAnimated(this,BindingContext.EditableWidth, BindingContext.EditableHeight);
            }
        }
        #endregion IsContentReadOnly 

        #region IsContentSubSelectable 

        private bool _isContentSubSelectable;
        public bool IsContentSubSelectable {
            get { return _isContentSubSelectable; }
            set { SetAndRaise(IsContentSubSelectableProperty, ref _isContentSubSelectable, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, bool> IsContentSubSelectableProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, bool>(
                nameof(IsContentSubSelectable),
                x => x.IsContentSubSelectable,
                (x, o) => x.IsContentSubSelectable = o,
                false,
                BindingMode.TwoWay);

        private void OnIsContentSubSelectableChanged() {
            if (BindingContext == null ||
                !IsContentLoaded ||
                !IsContentReadOnly) {
                return;
            }
            if (IsContentSubSelectable) {
                this.ExecuteJavascript("enableSubSelection_ext()");
                if (BindingContext.HasTemplates && !BindingContext.IsDropOverTile) {
                    MpAvResizeExtension.ResizeAnimated(this, BindingContext.EditableWidth, BindingContext.EditableHeight);
                }
            } else {
                this.ExecuteJavascript("disableSubSelection_ext()");
                MpAvResizeExtension.ResizeAnimated(this,BindingContext.ReadOnlyWidth, BindingContext.ReadOnlyHeight);
            }

        }

        #endregion IsContentSubSelectable 

        #region IsContentFindAndReplaceVisible Property

        private bool _isContentFindAndReplaceVisible;
        public bool IsContentFindAndReplaceVisible {
            get { return _isContentFindAndReplaceVisible; }
            set { SetAndRaise(IsContentFindAndReplaceVisibleProperty, ref _isContentFindAndReplaceVisible, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, bool> IsContentFindAndReplaceVisibleProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, bool>(
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
                this.ExecuteJavascript($"showFindAndReplace_ext()");
            } else {
                this.ExecuteJavascript($"hideFindAndReplace_ext()");
            }
        }


        #endregion IsContentFindAndReplaceVisible Property       


        #endregion

        #region Append

        #region AppendModeState Property

        private bool? _appendModeState;
        public bool? AppendModeState {
            get { return _appendModeState; }
            set { SetAndRaise(AppendModeStateProperty, ref _appendModeState, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, bool?> AppendModeStateProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, bool?>(
                nameof(AppendModeState),
                x => x.AppendModeState,
                (x, o) => x.AppendModeState = o,
                null,
                BindingMode.TwoWay);
        private void OnAppendModeStateChanged() {
            if (BindingContext == null ||
                (!BindingContext.IsAppendNotifier &&
                !BindingContext.IsAppendTrayItem)) {
                return;
            }

            MpConsole.WriteLine($"AppendModeState changed. AppendModeState: {AppendModeState}");
            Dispatcher.UIThread.Post(async () => {
                //var sw = Stopwatch.StartNew();
                //while (!IsContentLoaded) {
                //    await Task.Delay(100);
                //    if (sw.ElapsedMilliseconds > _APPEND_TIMEOUT_MS) {
                //        //timeout, content changed never notified back
                //        Debugger.Break();
                //        break;
                //    }
                //}
                var reqMsg = new MpQuillAppendModeChangedMessage() {
                    isAppendLineMode = AppendModeState.IsTrue(),
                    isAppendMode = AppendModeState.IsFalse()
                };
                this.ExecuteJavascript($"appendModeChanged_ext('{reqMsg.SerializeJsonObjectToBase64()}')");

                if (BindingContext.IsAppendNotifier && AppendModeState.IsNull()) {
                    //MpAppendNotificationViewModel.Instance.IsClosing = true;
                    MpAvNotificationWindowManager.Instance.HideNotification(MpAppendNotificationViewModel.Instance);
                }
            });
        }
        #endregion HasAppendModel Property

        #region AppendData Property

        private string _appendData;
        public string AppendData {
            get { return _appendData; }
            set { SetAndRaise(AppendDataProperty, ref _appendData, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, string> AppendDataProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, string>(
                nameof(AppendData),
                x => x.AppendData,
                (x, o) => x.AppendData = o,
                null,
                BindingMode.TwoWay);

        private async void OnAppendDataChanged() {
            if (BindingContext == null ||
                AppendData == null) {
                return;
            }

            MpConsole.WriteLine($"AppendData changed. AppendData: {AppendData}");
            var req = new MpQuillAppendDataRequestMessage() { appendData = AppendData };
            this.ExecuteJavascript($"appendData_ext('{req.SerializeJsonObjectToBase64()}')");

            AppendData = null;
            IsContentLoaded = false;
            if (this.GetVisualAncestor<Window>() is Window w &&
                w.IsVisible) {
                return;
            }
            while (!IsContentLoaded) {
                await Task.Delay(100);
            }
            //show updated append buffer if not already visible
            MpNotificationBuilder.ShowNotificationAsync(MpNotificationType.AppendChanged).FireAndForgetSafeAsync();

        }
        #endregion AppendData Property

        private async Task ProcessAppendModeChangedAsync(MpQuillAppendModeChangedMessage appendChangedMsg) {
            await MpAvClipTrayViewModel.Instance.UpdateAppendModeStateFromContentAsync(BindingContext, appendChangedMsg);
            MpAvCefNetWebView dest_wv;
            if (BindingContext.IsAppendNotifier) {
                // relay sel to tray
                dest_wv = LocateTrayTileWebView(BindingContext.CopyItemId);
            } else {
                // relay to modal
                dest_wv = LocateModalWebView();
            }
            if (dest_wv == null) {
                return;
            }
            dest_wv.ExecuteJavascript($"appendModeChanged_ext('{appendChangedMsg.SerializeJsonObjectToBase64()}')");
        }
        #endregion
    }
}
