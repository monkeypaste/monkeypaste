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

namespace MonkeyPaste.Avalonia {

    public enum MpAvEditorBindingFunctionType {
        // two-way *_get async requests
        getDragData,
        getAllNonInputTemplatesFromDb,

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
        notifyDataTransferCompleted
    }
    [DoNotNotify]
    public class MpAvCefNetWebView : 
        WebView, 
        MpAvIContentView, 
        MpAvIDragSource,
        MpAvIResizableControl {
        #region Private Variables

        private string _pastableContent_ntf { get; set; }
        private string _contentScreenShotBase64_ntf { get; set; }

        #endregion

        #region Statics

        private static List<MpAvCefNetWebView> _AllWebViews = new List<MpAvCefNetWebView>();

        public static MpAvCefNetWebView LocateWebView(int ciid) {
            if(ciid < 1) {
                return null;
            }
            var result = _AllWebViews.Where(x => x.DataContext is MpAvClipTileViewModel ctvm && ctvm.CopyItemId == ciid).ToList();
            if(result.Count == 0) {
                return null;
            }
            if(result.Count > 1) {
                // is this during a pin toggle? was this item pinned?
                Debugger.Break();
            }
            return result[0];
        }

        public static MpAvCefNetWebView LocateWebView(string publicHandle) {
            if(string.IsNullOrWhiteSpace(publicHandle)) {
                return null;
            }
            var result = _AllWebViews.Where(x => x.DataContext is MpAvClipTileViewModel ctvm && ctvm.PublicHandle == publicHandle).ToList();
            if (result.Count == 0) {
                return null;
            }
            if (result.Count > 1) {
                // is this during a pin toggle? was this item pinned?
                Debugger.Break();
            }
            return result[0];
        }

        #endregion

        #region Properties

        #region View Models
        public MpAvClipTileViewModel BindingContext => this.DataContext as MpAvClipTileViewModel;

        #endregion

        #region Bindings & Life Cycle


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

        void MpAvIDragSource.NotifyDropComplete(DragDropEffects dropEffect) {
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
        void MpAvIDragSource.NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc) {
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

        async Task<MpAvDataObject> MpAvIDragSource.GetDataObjectAsync(bool forOle, string[] formats = null) {
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

                bool add_tile_data = ctvm.ItemType != MpCopyItemType.Text ||
                                   (this.IsAllSelected() || this.Selection.Length == 0);
                if (add_tile_data) {
                    avdo.SetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT, ctvm.PublicHandle);
                }

                //MpISourceRef source_ref = ctvm.CopyItem;
                //string source_ref_str = MpJsonObject.SerializeObject(source_ref);
                //avdo.SetData(MpPortableDataFormats.INTERNAL_COPY_ITEM_SOURCE_DATA_FORMAT, source_ref_str);
                avdo.SetData(MpPortableDataFormats.CefAsciiUrl, ctvm.CopyItem.ToSourceRefUrl().ToBytesFromString(Encoding.ASCII));
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

        #region MpAvIContentView Implementation

        //public bool IsViewLoaded => IsDomLoaded;

        //public bool IsContentUnloaded { get; set; } = false;
        public MpAvTextSelection Selection { get; private set; }        

        MpAvIContentDocument MpAvIContentView.Document => Document;
        public bool IsAllSelected() {
            bool is_all_selected;
            if (BindingContext.IsSubSelectionEnabled) {
                // NOTE blindly accounting for quill's extra new line at doc end that can't seem to be gathered
                is_all_selected = Selection.Start.Offset == Document.ContentStart.Offset &&
                                    (Selection.End.Offset == Document.ContentEnd.Offset ||
                                     Selection.End.Offset == Document.ContentEnd.Offset - 1);
            } else {
                is_all_selected = true;
            }
            return is_all_selected;
        }


        #region Document Property

        public MpAvHtmlDocument Document { get; set; }

        public static readonly DirectProperty<MpAvCefNetWebView, MpAvHtmlDocument> DocumentProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, MpAvHtmlDocument>(
                nameof(Document),
                x => x.Document,
                enableDataValidation: true);

        #endregion

        #endregion

        #endregion

        #region Constructors

        public MpAvCefNetWebView() : base() {
            //this.CreateWindow += MpAvCefNetWebView_CreateWindow;
            Document = new MpAvHtmlDocument(this);
            Selection = new MpAvTextSelection(Document);
            MpMessenger.RegisterGlobal(ReceivedGlobalMessega);

            this.GetObservable(MpAvCefNetWebView.ContentHandleProperty).Subscribe(value => OnContentHandleChanged());
            this.GetObservable(MpAvCefNetWebView.IsContentSelectedProperty).Subscribe(value => OnIsContentSelectedChanged());
            this.GetObservable(MpAvCefNetWebView.IsContentReadOnlyProperty).Subscribe(value => OnIsContentReadOnlyChanged());
            this.GetObservable(MpAvCefNetWebView.IsContentSubSelectableProperty).Subscribe(value => OnIsContentSubSelectableChanged());
            this.GetObservable(MpAvCefNetWebView.IsContentFindAndReplaceVisibleProperty).Subscribe(value => OnIsContentFindOrReplaceVisibleChanged());
            
        }

        #endregion

        #region Public Methods

        public MpQuillEditorStateMessage GetEditorStateFromClipTile() {
            return new MpQuillEditorStateMessage() {
                envName = MpPlatformWrapper.Services.OsInfo.OsType.ToString(),
                contentHandle = BindingContext.PublicHandle,
                contentItemType = BindingContext.ItemType.ToString(),
                contentData = BindingContext.CopyItemData,
                isSubSelectionEnabled = BindingContext.IsSubSelectionEnabled,
                isReadOnly = BindingContext.IsContentReadOnly,
                isPastimgTemplate = BindingContext.IsPasting
            };
        }
        #endregion

        #region Protected Methods

        protected override WebViewGlue CreateWebViewGlue() {
            return new MpAvCefNetWebViewGlue(this);
        }


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

        #region Content Lifecycle
        #endregion

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

        #region WebView Binding Methods
        public async void HandleBindingNotification(MpAvEditorBindingFunctionType notificationType, string msgJsonBase64Str) {
            var ctvm = DataContext as MpAvClipTileViewModel;
            if (ctvm == null && notificationType != MpAvEditorBindingFunctionType.notifyDomLoaded) {
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
                        ProcessLoadCompleteResponse(loadComplete_ntf);
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
                        Document.ProcessContentChangedMessage(enableReadOnlyMsg);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyContentChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage contentChanged_ntf) {
                        Document.ProcessContentChangedMessage(contentChanged_ntf);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyDataTransferCompleted:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillDataTransferCompletedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
                        MpISourceRef sourceRef = null;
                        if (!string.IsNullOrEmpty(dataTransferCompleted_ntf.dataTransferSourceUrl)) {
                            var sr = MpSourceRef.ParseFromInternalUrl(dataTransferCompleted_ntf.dataTransferSourceUrl);
                            if (sr != null) {
                                if (!string.IsNullOrEmpty(sr.SourcePublicHandle)) {
                                    // get db id from handle
                                    int ciid = 0;
                                    var sctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == sr.SourcePublicHandle) as MpAvClipTileViewModel;
                                    if (sctvm == null) {
                                        // check for recycled tile
                                        if (MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count > 0 &&
                                            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels[0].PublicHandle == sr.SourcePublicHandle) {
                                            ciid = MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels[0].Id;
                                        }
                                    } else {
                                        ciid = sctvm.CopyItemId;
                                    }
                                    if (ciid > 0) {
                                        sr.SourceObjId = ciid;
                                        // internal source
                                        sourceRef = sr;
                                    }
                                }
                            }
                        }
                        if (sourceRef == null) {
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

                case MpAvEditorBindingFunctionType.notifyDocSelectionChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillContentSelectionChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentSelectionChangedMessage selChange_ntf) {
                        UpdateSelection(selChange_ntf.index, selChange_ntf.length, selChange_ntf.selText, true, selChange_ntf.isChangeBegin);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifySubSelectionEnabledChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillSubSelectionChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillSubSelectionChangedNotification subSelChangedNtf) {
                        ctvm.IsSubSelectionEnabled = subSelChangedNtf.isSubSelectionEnabled;
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
                    MpAvClipTrayViewModel.Instance.PasteSelectedClipsCommand.Execute(true);
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

                // REUSABLE

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
                case MpAvEditorBindingFunctionType.notifyException:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillExceptionMessage exceptionMsgObj) {
                        MpConsole.WriteLine($"[{ctvm}] {exceptionMsgObj}");
                        //Debugger.Break();
                    }
                    break;

                // GET CALLBACKS

                case MpAvEditorBindingFunctionType.getAllNonInputTemplatesFromDb:
                    HandleBindingGetRequest(notificationType, msgJsonBase64Str).FireAndForgetSafeAsync(ctvm);
                    break;
            }

            //MpConsole.WriteLine($"Tile {ctvm} received cef notification type '{notificationType}' w/ msg:",true);
            //MpConsole.WriteLine($"'{(ntf == null ? "NO DATA RECEIVED":ntf.ToPrettyPrintJsonString())}'", false, true);
        }

        private async Task HandleBindingGetRequest(MpAvEditorBindingFunctionType getReqType, string msgJsonBase64) {
            var getReq = MpJsonObject.DeserializeBase64Object<MpQuillGetRequestNotification>(msgJsonBase64);
            switch (getReqType) {
                case MpAvEditorBindingFunctionType.getAllNonInputTemplatesFromDb:
                    var templateReq = MpJsonObject.DeserializeObject<MpQuillTemplateDbQueryRequestMessage>(getReq.reqMsgFragmentJsonStr);
                    var tl = await MpDataModelProvider.GetTextTemplatesByType(templateReq.templateTypes.Select(x => x.ToEnum<MpTextTemplateType>()));

                    var getResp = new MpQuillGetResponseNotification() {
                        requestGuid = getReq.requestGuid,
                        responseFragmentJsonStr = MpJsonObject.SerializeObject(tl)
                    };

                    this.ExecuteJavascript($"getRequestResponse_ext('{getResp.SerializeJsonObjectToBase64()}')");
                    break;
            }
        }

        public void UpdateSelection(int index, int length, string text, bool isFromEditor, bool isChangeBegin) {
            var newStart = new MpAvTextPointer(Document, index);
            var newEnd = new MpAvTextPointer(Document, index + length);
            if (isFromEditor) {
                Selection.Start = newStart;
                Selection.End = newEnd;
                Selection.UpdateSelectedTextFromEditor(text);
            } else {
                Selection.Select(newStart, newEnd);
            }
            MpMessageType selChangeMsg = isChangeBegin ? MpMessageType.ContentSelectionChangeBegin : MpMessageType.ContentSelectionChangeEnd;
            MpMessenger.Send(selChangeMsg, DataContext);

            //MpConsole.WriteLine($"Tile: '{(DataContext as MpAvClipTileViewModel).CopyItemTitle}' Selection Changed: '{Selection}'");
        }
        public void SelectAll() {
            this.ExecuteJavascript("selectAll_ext()");
        }
        public void DeselectAll() {
            this.ExecuteJavascript($"deselectAll_ext()");
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

        protected override void OnBrowserCreated(EventArgs e) {
            base.OnBrowserCreated(e);
            Navigate(MpAvClipTrayViewModel.EditorPath);
        }
        protected override void OnNavigated(NavigatedEventArgs e) {
            base.OnNavigated(e);
            if (e.Url == "about:blank") {
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
            if (BindingContext == null || string.IsNullOrEmpty(BindingContext.CachedState)) {
                var req = new MpQuillInitMainRequestMessage() {
                    envName = MpPlatformWrapper.Services.OsInfo.OsType.ToString(),
                    isPlainHtmlConverter = false
                };
                this.ExecuteJavascript($"initMain_ext('{req.SerializeJsonObjectToBase64()}')");
            } else {
                await this.EvaluateJavascriptAsync($"setState_ext('{BindingContext.CachedState}')");

                BindingContext.CachedState = null;
            }
        }

        #endregion

        #region Content Life Cycle
        public bool IsContentLoaded { get; set; }

        public bool NeedsEvalJsCleared { get; set; }

        #region ContentHandle Property

        private string _contentHandle;
        public string ContentHandle {
            get { return _contentHandle; }
            set { SetAndRaise(ContentHandleProperty, ref _contentHandle, value); }
        }
        public static DirectProperty<MpAvCefNetWebView, string> ContentHandleProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, string>(
                nameof(ContentHandle),
                x => x.ContentHandle,
                (x, o) => x.ContentHandle = o);

        #endregion ContentHandle Property

        protected override void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);

            PerformLoadContentRequestAsync().FireAndForgetSafeAsync();
        }
        private void OnContentHandleChanged() {
            if(BindingContext == null || BindingContext.IsReloading) {
                return;
            }

            PerformLoadContentRequestAsync().FireAndForgetSafeAsync();
        }

        private async Task PerformLoadContentRequestAsync() {
            Dispatcher.UIThread.VerifyAccess();

            // NOTE!! we want to bind IsContentLoaded to IsViewLoaded but keeps giving stackoverflow
            BindingContext.IsViewLoaded = false;
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
                contentType = BindingContext.ItemType.ToString(),
                itemData = BindingContext.EditorFormattedItemData,
                isPasteRequest = BindingContext.IsPasting
            };

            if (!string.IsNullOrEmpty(MpAvSearchBoxViewModel.Instance.SearchText)) {
                loadContentMsg.searchText = MpAvSearchBoxViewModel.Instance.SearchText;
                var sfcvm = MpAvSearchBoxViewModel.Instance.SearchFilterCollectionViewModel;
                loadContentMsg.isCaseSensitive = sfcvm.Filters.FirstOrDefault(x => x.FilterType == MpContentFilterType.CaseSensitive).IsChecked.IsTrue();
                loadContentMsg.isWholeWord = sfcvm.Filters.FirstOrDefault(x => x.FilterType == MpContentFilterType.WholeWord).IsChecked.IsTrue();
                loadContentMsg.useRegex = sfcvm.Filters.FirstOrDefault(x => x.FilterType == MpContentFilterType.Regex).IsChecked.IsTrue();
            }
            string msgStr = loadContentMsg.SerializeJsonObjectToBase64();

            this.ExecuteJavascript($"loadContent_ext('{msgStr}')");
        }

        private void ProcessLoadCompleteResponse(MpQuillEditorContentChangedMessage contentChangeResp) {
            Document.ProcessContentChangedMessage(contentChangeResp);
            IsContentLoaded = true;
            BindingContext.IsViewLoaded = true;
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

        #region IsContentReadOnly 

        private bool _isContentReadOnly;
        public bool IsContentReadOnly {
            get { return _isContentReadOnly; }
            set { SetAndRaise(IsContentReadOnlyProperty, ref _isContentReadOnly, value); }
        }

        public static DirectProperty<MpAvCefNetWebView, bool> IsContentReadOnlyProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, bool>(
                nameof(IsContentReadOnly),
                x => x.IsContentReadOnly,
                (x, o) => x.IsContentReadOnly = o);

        private void OnIsContentReadOnlyChanged() {
            if (BindingContext == null || !IsContentLoaded) {
                return;
            }
            if (IsContentReadOnly) {
                MpAvResizeExtension.ResizeAnimated(this, BindingContext.ReadOnlyWidth, BindingContext.ReadOnlyHeight);
                Dispatcher.UIThread.Post(async () => {
                    string enableReadOnlyRespStr = await this.EvaluateJavascriptAsync("enableReadOnly_ext()");
                    var qrm = MpJsonObject.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(enableReadOnlyRespStr);
                    this.Document.ProcessContentChangedMessage(qrm);
                });
            } else {
                MpAvResizeExtension.ResizeAnimated(this, BindingContext.EditableWidth, BindingContext.EditableHeight);
                this.ExecuteJavascript($"disableReadOnly_ext()");
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
                (x, o) => x.IsContentSubSelectable = o);

        private void OnIsContentSubSelectableChanged() {
            if (BindingContext == null ||
                !IsContentLoaded ||
                !IsContentReadOnly ||
                BindingContext.IsReloading) {
                return;
            }
            if (IsContentSubSelectable) {
                this.ExecuteJavascript("enableSubSelection_ext()");
                if (BindingContext.HasTemplates && !BindingContext.IsDropOverTile) {
                    MpAvResizeExtension.ResizeAnimated(this, BindingContext.EditableWidth, BindingContext.EditableHeight);
                }
            } else {
                MpAvResizeExtension.ResizeAnimated(this, BindingContext.ReadOnlyWidth, BindingContext.ReadOnlyHeight);
                this.ExecuteJavascript("disableSubSelection_ext()");
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
                (x, o) => x.IsContentFindAndReplaceVisible = o);

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
    }
}
