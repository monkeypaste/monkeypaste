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

namespace MonkeyPaste.Avalonia {

    public enum MpAvEditorBindingFunctionType {
        getDragData,
        getAllTemplatesFromDb,
        notifyDocSelectionChanged,
        notifyContentLengthChanged,
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
        notifyPasteTemplateRequest,
        notifyFindReplaceVisibleChange,
        notifyQuerySearchRangesChanged,
        notifyLoadComplete,
        notifyShowCustomColorPicker
    }
    [DoNotNotify]
    public class MpAvCefNetWebView : 
        WebView, 
        MpAvIContentView {
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

        private void MpAvCefNetWebView_CreateWindow(object sender, CreateWindowEventArgs e) {
            IPlatformHandle platformHandle = MpAvMainWindow.Instance.PlatformImpl.Handle;
            if (platformHandle is IMacOSTopLevelPlatformHandle macOSHandle)
                e.WindowInfo.SetAsWindowless(macOSHandle.GetNSWindowRetained());
            else
                e.WindowInfo.SetAsWindowless(platformHandle.Handle);

            e.Client = this.Client;
        }

        private void ReceivedGlobalMessega(MpMessageType msg) {
            switch(msg) {
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
        public void HandleBindingNotification(MpAvEditorBindingFunctionType notificationType, string msgJsonBase64Str) {
            var ctvm =DataContext as MpAvClipTileViewModel;
            if(ctvm == null && notificationType != MpAvEditorBindingFunctionType.notifyDomLoaded) {
                // converter doesn't have data context but needs to notify dom loaded which doesn't need it
                return;
            }
            MpJsonObject ntf = null;
            switch (notificationType) {
                case MpAvEditorBindingFunctionType.notifyDomLoaded:
                    IsDomLoaded = true;
                    break;
                case MpAvEditorBindingFunctionType.notifyInitComplete:
                    IsEditorInitialized = true;
                    break;

                case MpAvEditorBindingFunctionType.notifyLoadComplete:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillLoadContentResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillLoadContentResponseMessage resp) {
                        ctvm.UnformattedContentSize = new MpSize(resp.contentWidth, resp.contentHeight);
                        ctvm.LineCount = resp.lineCount;
                        ctvm.CharCount = resp.charCount;
                        Document.ContentEnd.Offset = resp.charCount;
                        IsContentLoaded = true;
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyDocSelectionChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillContentSelectionChangedMessage>(msgJsonBase64Str);
                    if(ntf is MpQuillContentSelectionChangedMessage selChange_ntf) {
                        UpdateSelection(selChange_ntf.index, selChange_ntf.length, selChange_ntf.selText, true, selChange_ntf.isChangeBegin);
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyContentLengthChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillContentLengthChangedMessage>(msgJsonBase64Str);
                    if(ntf is MpQuillContentLengthChangedMessage cntLength_ntf) {
                        Document.ContentEnd.Offset = cntLength_ntf.length;
                        ctvm.CharCount = cntLength_ntf.length;
                        ctvm.LineCount = cntLength_ntf.lines;
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyReadOnlyEnabled:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillEnableReadOnlyResponseMessage>(msgJsonBase64Str);
                    if(ntf is MpQuillEnableReadOnlyResponseMessage enableReadOnlyMsg) {
                        ctvm.IsContentReadOnly = true;
                        ctvm.CopyItemData = enableReadOnlyMsg.itemData;
                    }                    
                    break;
                case MpAvEditorBindingFunctionType.notifyReadOnlyDisabled:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonBase64Str);
                    if(ntf is MpQuillDisableReadOnlyResponseMessage disableReadOnlyMsg) {
                        ctvm.IsContentReadOnly = false;
                        ctvm.UnformattedContentSize = new MpSize(disableReadOnlyMsg.editorWidth, disableReadOnlyMsg.editorHeight);
                    }
                    break;

                case MpAvEditorBindingFunctionType.notifySubSelectionEnabledChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillSubSelectionChangedNotification>(msgJsonBase64Str);
                    if (ntf is MpQuillSubSelectionChangedNotification subSelChangedNtf) {
                        ctvm.IsSubSelectionEnabled = subSelChangedNtf.isSubSelectionEnabled;
                    }
                    break;
               
                case MpAvEditorBindingFunctionType.notifyDropCompleted:
                    Dispatcher.UIThread.Post(async () => {
                        // when  drop completes wait for drag tile (if internal) to reload before updating selection to drop tile
                        await Task.Delay(300);
                        while (MpAvClipTrayViewModel.Instance.AllItems.Any(x => x.IsReloading)) {
                            await Task.Delay(100);
                        }
                        await Task.Delay(300);
                        MpAvMainWindow.Instance.Activate();
                        MpAvClipTrayViewModel.Instance.ClearAllSelection();
                        MpAvClipTrayViewModel.Instance.SelectedItem = BindingContext;
                        MpAvClipTrayViewModel.Instance.AllItems.ForEach(x => x.IsHovering = x.CopyItemId == BindingContext.CopyItemId);

                        //if (!BindingContext.IsPinned) {
                        //    var lb = this.GetVisualAncestor<ListBox>();

                        //    var lbi = this.GetVisualAncestor<ListBoxItem>();
                        //    lb.SelectedItem = BindingContext;
                        //}
                       
                        ////BindingContext.IsSelected = true;
                        //if (!this.IsKeyboardFocusWithin) {
                        //    this.Focus();
                        //}
                    });
                    break;
                case MpAvEditorBindingFunctionType.notifyDragEnter:
                    BindingContext.IsHovering = true;
                    break;
                case MpAvEditorBindingFunctionType.notifyAddOrUpdateTemplate:
                    var aoumsg = MpJsonObject.DeserializeBase64Object<MpQuillTemplateAddOrUpdateNotification>(msgJsonBase64Str);
                    //Debugger.Break();
                    break;
                case MpAvEditorBindingFunctionType.notifyUserDeletedTemplate:
                    var udmsg = MpJsonObject.DeserializeBase64Object<MpQuillUserDeletedTemplateNotification>(msgJsonBase64Str);
                    //Debugger.Break();
                    break;
                case MpAvEditorBindingFunctionType.notifyDragLeave:
                    BindingContext.IsHovering = false;
                    break;
                case MpAvEditorBindingFunctionType.notifyContentScreenShot:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillContentScreenShotNotificationMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillContentScreenShotNotificationMessage ssMsg) {
                        Document.ContentScreenShotBase64 = ssMsg.contentScreenShotBase64;
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyPasteTemplateRequest:
                    MpAvClipTrayViewModel.Instance.PasteSelectedClipsCommand.Execute(true);
                    break;
                case MpAvEditorBindingFunctionType.notifyException:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    if(ntf is MpQuillExceptionMessage exceptionMsgObj) {
                        // handled post-case
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyFindReplaceVisibleChange:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillContentFindReplaceVisibleChanedNotificationMessage>(msgJsonBase64Str);
                    if(ntf is MpQuillContentFindReplaceVisibleChanedNotificationMessage findReplaceMsgObj) {
                        ctvm.IsFindAndReplaceVisible = findReplaceMsgObj.isFindReplaceVisible;
                    }
                    break;
                case MpAvEditorBindingFunctionType.notifyQuerySearchRangesChanged:
                    ntf = MpJsonObject.DeserializeBase64Object<MpQuillContentQuerySearchRangesChangedNotificationMessage>(msgJsonBase64Str);
                    if(ntf is MpQuillContentQuerySearchRangesChangedNotificationMessage searchRangeCountMsg) {
                        if (searchRangeCountMsg.rangeCount > 1) {
                            MpAvSearchBoxViewModel.Instance.NotifyHasMultipleMatches();
                        }
                    }
                   
                    break;
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
            }

            //MpConsole.WriteLine($"Tile {ctvm} received cef notification type '{notificationType}' w/ msg:",true);
            //MpConsole.WriteLine($"'{(ntf == null ? "NO DATA RECEIVED":ntf.ToPrettyPrintJsonString())}'", false, true);
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

            MpConsole.WriteLine($"Tile: '{(DataContext as MpAvClipTileViewModel).CopyItemTitle}' Selection Changed: '{Selection}'");
        }
        public void SelectAll() {
            this.ExecuteJavascript("selectAll_ext()");
        }
        public void DeselectAll() {
            this.ExecuteJavascript($"deselectAll_ext()");
        }


        #endregion

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
    }
}
