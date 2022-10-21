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
        notifyEditorSelectionChanged,
        notifyContentLengthChanged,
        notifySubSelectionEnabledChanged,
        notifyDropEffectChanged,
        notifyException,
        notifyDragStartOrEnd,
        notifyReadOnlyEnabled,
        notifyReadOnlyDisabled,
        notifyDomLoaded,
        notifyDropCompleted,
        notifyDragEnter,
        notifyDragLeave,
        notifyContentScreenShot,
        notifyUserDeletedTemplate,
        notifyAddOrUpdateTemplate,
        notifyPasteTemplateRequest
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
        public bool IsEditorInitialized { get; set; } = false;
        public bool IsDomLoaded { get; set; } = false;


        public MpAvTextSelection Selection { get; private set; }

        public MpAvHtmlDocument Document { get; set; }

        MpAvIContentDocument MpAvIContentView.Document => Document;


        #endregion

        #region Constructors

        public MpAvCefNetWebView() : base() {
            //this.CreateWindow += MpAvCefNetWebView_CreateWindow;
            Document = new MpAvHtmlDocument(this);
            Selection = new MpAvTextSelection(Document);

            //CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Copy, OnCopy, OnCanExecuteClipboardCommand));
            //CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Cut, OnCut, OnCanExecuteClipboardCommand));
            //CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Paste, OnPaste, OnCanExecuteClipboardCommand));

        }

        private void MpAvCefNetWebView_CreateWindow(object sender, CreateWindowEventArgs e) {
            IPlatformHandle platformHandle = MpAvMainWindow.Instance.PlatformImpl.Handle;
            if (platformHandle is IMacOSTopLevelPlatformHandle macOSHandle)
                e.WindowInfo.SetAsWindowless(macOSHandle.GetNSWindowRetained());
            else
                e.WindowInfo.SetAsWindowless(platformHandle.Handle);

            e.Client = this.Client;
        }

        #endregion

        #region WebView Binding Methods
        public void HandleBindingNotification(MpAvEditorBindingFunctionType notificationTYpe, string msgJsonBase64Str) {
            var ctvm =DataContext as MpAvClipTileViewModel;
            if(ctvm == null && notificationTYpe != MpAvEditorBindingFunctionType.notifyDomLoaded) {
                // converter doesn't have data context but needs to notify dom loaded which doesn't need it
                return;
            }
            switch (notificationTYpe) {
                case MpAvEditorBindingFunctionType.notifyEditorSelectionChanged:
                    var selChangedJsonMsgObj = MpJsonObject.DeserializeBase64Object<MpQuillContentSelectionChangedMessage>(msgJsonBase64Str);
                   UpdateSelection(selChangedJsonMsgObj.index, selChangedJsonMsgObj.length, selChangedJsonMsgObj.selText, true, selChangedJsonMsgObj.isChangeBegin);
                    break;
                case MpAvEditorBindingFunctionType.notifyContentLengthChanged:
                    var contentLengthMsgObj = MpJsonObject.DeserializeBase64Object<MpQuillContentLengthChangedMessage>(msgJsonBase64Str);
                   Document.ContentEnd.Offset = contentLengthMsgObj.length;
                    break;
                case MpAvEditorBindingFunctionType.notifyReadOnlyEnabled:
                    var enableReadOnlyMsg = MpJsonObject.DeserializeBase64Object<MpQuillEnableReadOnlyResponseMessage>(msgJsonBase64Str);
                    ctvm.IsContentReadOnly = true;
                    ctvm.CopyItemData = enableReadOnlyMsg.itemData;
                    break;
                case MpAvEditorBindingFunctionType.notifyReadOnlyDisabled:
                    var disableReadOnlyMsg = MpJsonObject.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonBase64Str);
                    ctvm.IsContentReadOnly = false;
                    ctvm.UnformattedContentSize = new MpSize(disableReadOnlyMsg.editorWidth, disableReadOnlyMsg.editorHeight);
                    break;

                case MpAvEditorBindingFunctionType.notifySubSelectionEnabledChanged:
                    var subSelChangedNtf = MpJsonObject.DeserializeBase64Object<MpQuillSubSelectionChangedMessageOrNotification>(msgJsonBase64Str);
                    ctvm.IsSubSelectionEnabled = subSelChangedNtf.isSubSelectionEnabled;
                    break;
                case MpAvEditorBindingFunctionType.notifyDomLoaded:
                   IsDomLoaded = true;
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
                    var ssMsg = MpJsonObject.DeserializeBase64Object<MpQuillContentScreenShotNotificationMessage>(msgJsonBase64Str);
                    Document.ContentScreenShotBase64 = ssMsg.contentScreenShotBase64;
                    break;
                case MpAvEditorBindingFunctionType.notifyPasteTemplateRequest:
                    MpAvClipTrayViewModel.Instance.PasteSelectedClipsCommand.Execute(true);
                    break;
                case MpAvEditorBindingFunctionType.notifyException:
                    var exceptionMsgObj = MpJsonObject.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    MpConsole.WriteLine(exceptionMsgObj);
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
    }
}
