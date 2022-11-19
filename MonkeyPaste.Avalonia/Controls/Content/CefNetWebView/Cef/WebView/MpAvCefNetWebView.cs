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
        MpAvIResizableControl {
        #region Private Variables
        #endregion

        #region Statics
        #endregion

        #region Properties
        public MpAvClipTileViewModel BindingContext => this.DataContext as MpAvClipTileViewModel;


        private bool _isContentLoaded = false;
        public bool IsContentLoaded {
            get => _isContentLoaded;
            set {
                if(IsContentLoaded != value) {
                    _isContentLoaded = value;
                    if(BindingContext != null) {
                        BindingContext.OnPropertyChanged(nameof(BindingContext.IsAnyBusy));
                    }
                }
            }
        }

        public bool IsEditorInitialized { get; private set; } = false;

        public bool IsDomLoaded { get; set; } = false;

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

        #region MpAvIContentView Implementation

        public bool IsViewLoaded => IsDomLoaded;

        public bool IsContentUnloaded { get; set; } = false;
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

        #endregion

        #region Document Property

        public MpAvHtmlDocument Document { get; set; }

        public static readonly DirectProperty<MpAvCefNetWebView, MpAvHtmlDocument> DocumentProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetWebView, MpAvHtmlDocument>(
                nameof(Document),
                x => x.Document,
                enableDataValidation: true);

        #endregion

        #endregion

        #region Constructors

        public MpAvCefNetWebView() : base() {
            //this.CreateWindow += MpAvCefNetWebView_CreateWindow;
            Document = new MpAvHtmlDocument(this);
            Selection = new MpAvTextSelection(Document);
            MpMessenger.RegisterGlobal(ReceivedGlobalMessega);
            
        }

        #endregion

        #region Public Methods

        #region WebView Binding Methods
        public async void HandleBindingNotification(MpAvEditorBindingFunctionType notificationType, string msgJsonBase64Str) {
            var ctvm =DataContext as MpAvClipTileViewModel;
            if(ctvm == null && notificationType != MpAvEditorBindingFunctionType.notifyDomLoaded) {
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
                case MpAvEditorBindingFunctionType.notifyReadOnlyDisabled:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillDisableReadOnlyResponseMessage disableReadOnlyMsg) {
                        ctvm.IsContentReadOnly = false;
                        ctvm.UnformattedContentSize = new MpSize(disableReadOnlyMsg.editorWidth, disableReadOnlyMsg.editorHeight);
                    }
                    break;

                // CONTENT CHANGED

                case MpAvEditorBindingFunctionType.notifyLoadComplete:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage loadComplete_ntf) {
                        Document.ProcessContentChangedMessage(loadComplete_ntf);
                        IsContentLoaded = true;
                        IsContentUnloaded = false;
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyContentChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillEditorContentChangedMessage contentChanged_ntf) {
                        Document.ProcessContentChangedMessage(contentChanged_ntf);
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
                case MpAvEditorBindingFunctionType.notifyDataTransferCompleted:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillDataTransferCompletedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillDataTransferCompletedNotification dataTransferCompleted_ntf) {
                        MpISourceRef sourceRef = null;
                        if(!string.IsNullOrEmpty(dataTransferCompleted_ntf.dataTransferSourceUrl)) {
                            var sr = MpSourceRef.ParseFromInternalUrl(dataTransferCompleted_ntf.dataTransferSourceUrl);
                            if(sr != null) {
                                if(!string.IsNullOrEmpty(sr.SourcePublicHandle)) {
                                    // get db id from handle
                                    if (MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == sr.SourcePublicHandle) is MpAvClipTileViewModel sctvm) {
                                        sr.SourceObjId = sctvm.CopyItemId;
                                        // internal source
                                        sourceRef = sr;
                                    }
                                }                                                  
                            }            
                        }
                        if(sourceRef == null) {
                            var url = await MpPlatformWrapper.Services.UrlBuilder.CreateAsync(dataTransferCompleted_ntf.dataTransferSourceUrl);
                            if (url != null) {
                                // remote source
                                sourceRef = url;
                            }
                        }
                        if(sourceRef == null) {
                            var app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo);
                            if(app != null) {
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
                    Document.PastableContentResponse = msgJsonBase64Str;
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
                        Document.ContentScreenShotBase64 = ssMsg.contentScreenShotBase64;
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
                    if(ntf is MpQuillShowCustomColorPickerNotification showCustomColorPickerMsg) {
                        Dispatcher.UIThread.Post(async () => {

                            if(string.IsNullOrWhiteSpace(showCustomColorPickerMsg.pickerTitle)) {
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
                        MpConsole.WriteLine(exceptionMsgObj.ToString());
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
            switch(getReqType) {
                case MpAvEditorBindingFunctionType.getAllNonInputTemplatesFromDb:
                    var templateReq = MpJsonObject.DeserializeObject<MpQuillTemplateDbQueryRequestMessage>(getReq.reqMsgFragmentJsonStr);
                    var tl = await MpDataModelProvider.GetTextTemplatesByType(templateReq.templateTypes.Select(x=>x.ToEnum<MpTextTemplateType>()));

                    var getResp = new MpQuillGetResponseNotification() {
                        requestGuid = getReq.requestGuid,
                        responseFragmentJsonStr = MpJsonObject.SerializeObject(tl)
                    };

                    this.ExecuteJavascript($"getRequestResponse_ext('{getResp.SerializeJsonObjectToBase64()}')");
                    break;
            }
        }

        public void UpdateSelection(int index, int length,string text, bool isFromEditor, bool isChangeBegin) {
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

        #region Drag Handler

        public bool HandleStartDragging(CefBrowser browser, CefDragData dragData, CefDragOperationsMask allowedOps, int x, int y) {
            PerformDragAsync(dragData, allowedOps).FireAndForgetSafeAsync();
            return false;
        }

        #endregion

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

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            _resizerControl = null;
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

        #region Drag Helpers

        private PointerEventArgs _capturedPointerEventArgs;
        private bool _wasEscPressed = false;
        private MpPoint _lastGlobalMousePoint; // debouncer
        private DragDropEffects _dragEffects;
        private async Task PerformDragAsync(CefDragData dragData, CefDragOperationsMask allowedOps) {
            if(!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(() => PerformDragAsync(dragData, allowedOps));
                return;
            }

            ResetDragState();
            BindingContext.IsTileDragging = true;
            HookDragEvents();

            _dragEffects = allowedOps.ToDragDropEffects();

            if (BindingContext.ItemType == MpCopyItemType.FileList) {
                _dragEffects = DragDropEffects.Copy;
            } else if (BindingContext.ItemType == MpCopyItemType.Image) {
                _dragEffects = DragDropEffects.Move;
            } else {
                // no changes for text
            }

            var source_data_object = await Document.GetDataObjectAsync(false, false, false);

            if(source_data_object == null) {
                // this seems to happen due to data conversion errors somewhere
                Debugger.Break();
                FinishDrag(true);
                return;
            }
            MpAvDataObject.SetSourceDragDataObject(source_data_object);

            // seems excessive...but ultimately all ole pref's come from plugins so pass everthing through cb plugin system just like writing to clipboard
            var processed_data_object = await MpPlatformWrapper.Services.DataObjectHelperAsync.WriteDragDropDataObject(source_data_object) as MpAvDataObject;
            MpAvDataObject.UpdateDragDataObject(processed_data_object);
            MpAvExternalDropWindowViewModel.Instance.ShowDropWindowCommand.Execute(null);

            while (_capturedPointerEventArgs == null) {
                await Task.Delay(100);
            }

            var result = await DragDrop.DoDragDrop(_capturedPointerEventArgs, processed_data_object, _dragEffects);

            
            FinishDrag(false);

            MpConsole.WriteLine("Cef Drag Result: " + result);
        }

        private void CapturePointerEventHandler(object sender, PointerEventArgs e) {
            _capturedPointerEventArgs = e;
            if (!e.IsLeftDown(this)) {
                // NOTE not sure if these events are received since dnd is progress 
                // but probably good to keep since drag end is so annoying to handle...
                MpConsole.WriteLine("CefGlue pointer move event detached ITSELF");
                this.PointerMoved -= CapturePointerEventHandler;

                MpAvExternalDropWindowViewModel.Instance.DoNotRememberDropInfoCommand.Execute(null);
                return;
            }
        }

        private void OnGlobalKeyPrssedOrReleasedHandler(object sender, string key) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => OnGlobalKeyPrssedOrReleasedHandler(sender, key));
                return;
            }

            if (MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown) {
                _wasEscPressed = true;
            }
            var modKeyMsg = new MpQuillModifierKeysNotification() {
                ctrlKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                altKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown,
                shiftKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown,
                escKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown
            };
            this.ExecuteJavascript($"updateModifierKeysFromHost_ext('{modKeyMsg.SerializeJsonObjectToBase64()}')");
        }

        private void OnGlobalMouseMove(object s, MpPoint e) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => OnGlobalMouseMove(s, e));
                return;
            }
            var scvm = MpAvShortcutCollectionViewModel.Instance;

            if(_lastGlobalMousePoint == null) {
                _lastGlobalMousePoint = e;
            } else if(e.Distance(_lastGlobalMousePoint) < MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST) {
                // debounce (window handle from point is expensive)
                return;
            }
            MpAvExternalDropWindowViewModel.Instance.UpdateDropAppViewModelCommand.Execute(e);
            _lastGlobalMousePoint = e;
        }

        private void OnGlobalMouseReleased(object s, bool e) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => OnGlobalMouseReleased(s, e));
                return;
            }
            MpAvExternalDropWindowViewModel.Instance.ShowFinishDropMenuCommand.Execute(null);

        }

        private void HookDragEvents() {
            this.PointerMoved += CapturePointerEventHandler;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += OnGlobalKeyPrssedOrReleasedHandler;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += OnGlobalKeyPrssedOrReleasedHandler;

            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove += OnGlobalMouseMove;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += OnGlobalMouseReleased;
        }

        private void UnhookDragEvents() {
            this.PointerMoved -= CapturePointerEventHandler;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= OnGlobalKeyPrssedOrReleasedHandler;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= OnGlobalKeyPrssedOrReleasedHandler;

            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove -= OnGlobalMouseMove;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased -= OnGlobalMouseReleased;
        }

        private void FinishDrag(bool wasErrorOrCancel) {
            MpAvDataObject.SetSourceDragDataObject(null);

            bool wasCopy = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown;
            bool wasSuccess = !_wasEscPressed && !wasErrorOrCancel;
            bool wasSelfDrop = wasSuccess && BindingContext.IsDropOverTile;

            string dropEffect = wasSelfDrop && !wasCopy ? "move" : "copy";

            if (!wasSuccess) {
                dropEffect = "none";
            }

            var dragEndMsg = new MpQuillDragEndMessage() {
                dataTransfer = new MpQuillDataTransferMessageFragment() {
                    dropEffect = dropEffect
                },
                fromHost = true,
                wasCancel = !wasSuccess
            };

            this.ExecuteJavascript($"dragEnd_ext('{dragEndMsg.SerializeJsonObjectToBase64()}')");

            ResetDragState();
            MpConsole.WriteLine("Was Self drop: " + wasSelfDrop);
            MpConsole.WriteLine("ACTUAL drag result: " + dropEffect);
        }
        private void ResetDragState() {
            UnhookDragEvents();
            if (BindingContext != null) {
                BindingContext.IsTileDragging = false;
            }
            _capturedPointerEventArgs = null;
            _wasEscPressed = false;
            _lastGlobalMousePoint = null;
        }
        #endregion

        #endregion
    }
}
