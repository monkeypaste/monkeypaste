using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CefNet;
using CefNet.Avalonia;
using CefNet.CApi;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCefNetWebViewExtension {
        #region Private Variables

        private static readonly double _EDITOR_DEFAULT_WIDTH = 1130;

        #endregion

        static MpAvCefNetWebViewExtension() {
            IsContentReadOnlyProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsContentReadOnlyChanged(x, y));
            IsSubSelectionEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsSubSelectionEnabledChanged(x, y));
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            IsDevToolsVisibleProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsDevToolsVisibleChanged(x, y));
            HtmlDataProperty.Changed.AddClassHandler<Control>((x, y) => HandleHtmlDataChanged(x, y));
        }

        #region Properties

        #region ReadOnlyWidth AvaloniaProperty
        public static double GetReadOnlyWidth(AvaloniaObject obj) {
            return obj.GetValue(ReadOnlyWidthProperty);
        }
        public static void SetReadOnlyWidth(AvaloniaObject obj, double value) {
            obj.SetValue(ReadOnlyWidthProperty, value);
        }

        public static readonly AttachedProperty<double> ReadOnlyWidthProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "ReadOnlyWidth",
                260.0d,
                false);

        #endregion

        #region IsContentReadOnly AvaloniaProperty
        public static bool GetIsContentReadOnly(AvaloniaObject obj) {
            return obj.GetValue(IsContentReadOnlyProperty);
        }
        public static void SetIsContentReadOnly(AvaloniaObject obj, bool value) {
            obj.SetValue(IsContentReadOnlyProperty, value);
        }

        public static readonly AttachedProperty<bool> IsContentReadOnlyProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsContentReadOnly",
                true,
                false);

        private static async void HandleIsContentReadOnlyChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isReadOnly &&
                element is MpAvCefNetWebView wv && 
                wv.DataContext is MpAvClipTileViewModel ctvm &&
                wv.IsEditorInitialized //&&
               // !wv.BindingContext.IsReloading
                ) {
                Control resizeControl = null;
                var ctv = wv.GetVisualAncestor<MpAvClipTileView>();
                if(ctv != null) {
                    resizeControl = ctv.FindControl<Control>("ClipTileResizeBorder");
                }
                // only signal read only change after webview is loaded
                if (isReadOnly) {
                    if(resizeControl != null) {
                        MpAvResizeExtension.ResizeAnimated(resizeControl, ctvm.ReadOnlyWidth, ctvm.ReadOnlyHeight, 3.0d);
                    }
                    string enableReadOnlyRespStr = await wv.EvaluateJavascriptAsync("enableReadOnly_ext()");
                    //ProcessEnableReadOnlyResponse(wv, enableReadOnlyRespStr);
                    var qrm = MpJsonObject.DeserializeBase64Object<MpQuillEnableReadOnlyResponseMessage>(enableReadOnlyRespStr);
                    if (ctvm.CopyItemData != qrm.itemData) {
                        ctvm.CopyItemData = qrm.itemData;
                    }

                } else {
                    if (resizeControl != null) {
                        MpAvResizeExtension.ResizeAnimated(resizeControl, ctvm.EditableWidth, ctvm.EditableHeight, 3.0d);
                    }
                    //var drorMsg = new MpQuillDisableReadOnlyRequestMessage() {
                    //    allAvailableTextTemplates = new List<MpTextTemplate>(),// MpMasterTemplateModelCollectionViewModel.Instance.AllTemplates.ToList(),
                    //    editorHeight = ctvm.EditorHeight
                    //};
                    //string disableReadOnlyResp = await wv.ExecuteJavascript($"disableReadOnly_ext()");
                    //ProcessDisableReadOnlyResponse(wv, disableReadOnlyResp);
                    wv.ExecuteJavascript($"disableReadOnly_ext()");
                }
            }                  
        }

        #endregion

        #region IsSubSelectionEnabled AvaloniaProperty

        public static bool GetIsSubSelectionEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsSubSelectionEnabledProperty);
        }

        public static void SetIsSubSelectionEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsSubSelectionEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsSubSelectionEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsSubSelectionEnabled",
                false,
                false);

        private static void HandleIsSubSelectionEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isSubSelectionEnabled &&
                element is MpAvCefNetWebView wv &&
                GetIsContentReadOnly(wv) &&
                wv.IsEditorInitialized &&
                wv.DataContext is MpAvClipTileViewModel ctvm &&
                !ctvm.IsReloading) {
                if (isSubSelectionEnabled) {
                    // editor handles enabling by double clicking 
                    wv.ExecuteJavascript("enableSubSelection_ext()");
                } else {
                    wv.ExecuteJavascript("disableSubSelection_ext()");
                }
            }
        }

        #endregion

        #region IsDevToolsVisible AvaloniaProperty
        public static bool GetIsDevToolsVisible(AvaloniaObject obj) {
            return obj.GetValue(IsDevToolsVisibleProperty);
        }

        public static void SetIsDevToolsVisible(AvaloniaObject obj, bool value) {
            obj.SetValue(IsDevToolsVisibleProperty, value);
        }

        public static readonly AttachedProperty<bool> IsDevToolsVisibleProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsDevToolsVisible",
                false,
                false,
                BindingMode.TwoWay);

        private static void HandleIsDevToolsVisibleChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if(element is MpAvCefNetWebView wv) {
                if(e.NewValue is bool isVisible) {                    
                    if (isVisible) {                        
                        wv.ShowDevTools();
                    } else {
                        wv.CloseDevTools();
                    }
                }
            }
        }

        private static void Wv_DevToolsProtocolEventAvailable(object sender, DevToolsProtocolEventAvailableEventArgs e) {
            MpConsole.WriteLine($"Dev tools Event: '{e.EventName}' Data: '{e.Data}' ");
        }
        #endregion

        #region HtmlData AvaloniaProperty

        public static string GetHtmlData(AvaloniaObject obj) {
            return obj.GetValue(HtmlDataProperty);
        }

        public static void SetHtmlData(AvaloniaObject obj, string value) {
            obj.SetValue(HtmlDataProperty, value);
        }

        public static readonly AttachedProperty<string> HtmlDataProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "HtmlData",
                string.Empty,
                false);

        private static void HandleHtmlDataChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvCefNetWebView wv &&
                wv.DataContext is MpAvClipTileViewModel ctvm &&
                !ctvm.IsWaitingForDomLoad &&
                !ctvm.IsReloading) {
                Dispatcher.UIThread.Post(async () => {
                    ctvm.IsWaitingForDomLoad = true;
                    while (!wv.IsEditorInitialized) {
                        await Task.Delay(100);
                    }
                    while (ctvm.IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    if (ctvm.IsPlaceholder && !ctvm.IsPinned) {
                        return;
                    }

                    var loadContentMsg = new MpQuillLoadContentRequestMessage() {
                        contentHandle = ctvm.PublicHandle,
                        contentType = ctvm.ItemType.ToString(),
                        itemData = ctvm.EditorFormattedItemData,
                        isPasteRequest = ctvm.IsPasting
                    };

                    var respStr = await wv.EvaluateJavascriptAsync($"loadContent_ext('{loadContentMsg.SerializeJsonObjectToBase64()}')");
                    var resp = MpJsonObject.DeserializeBase64Object<MpQuillLoadContentResponseMessage>(respStr);
                    ctvm.UnformattedContentSize = new MpSize(resp.contentWidth, resp.contentHeight);
                    ctvm.IsWaitingForDomLoad = false;
                });
                // editor will know its loaded by IsLoaded and just set new html
                //LoadContentAsync(wv).FireAndForgetSafeAsync(null);
            }
        }



        //private static async Task LoadContentAsync(Control control) {
        //    if (control is MpAvCefNetWebView wv &&
        //        control.DataContext is MpAvClipTileViewModel ctvm) {
        //        while (ctvm.IsAnyBusy) {
        //            await Task.Delay(100);
        //        }
        //        if (ctvm.IsPlaceholder && !ctvm.IsPinned) {
        //            return;
        //        }
        //        ctvm.IsBusy = true;

        //        var lrm = await CreateLoadRequestMessageAsync(wv);
        //        var loadReqJsonStr = lrm.SerializeJsonObjectToBase64();
        //        string loadResponseMsgStr = await wv.EvaluateJavascriptAsync($"initMain_ext('{loadReqJsonStr}')");
        //        MpQuillLoadContentResponseMessage loadResponseMsg = MpJsonObject.DeserializeBase64Object<MpQuillLoadContentResponseMessage>(loadResponseMsgStr);

        //        ctvm.UnformattedContentSize = new Size(loadResponseMsg.contentWidth, loadResponseMsg.contentHeight);
        //        wv.IsEditorInitialized = true;
        //        wv.Document.ContentEnd.Offset = loadResponseMsg.contentLength - 1;
        //        ctvm.IsBusy = false;

        //        MpConsole.WriteLine($"Tile Content Item '{ctvm.CopyItemTitle}' is loaded");
        //    }
        //}

        //private static async Task<MpQuillLoadContentRequestMessage> CreateLoadRequestMessageAsync(Control control) {
        //    if (control.DataContext is MpAvClipTileViewModel ctvm &&
        //        control is MpAvCefNetWebView wv) {
        //        await Task.Delay(1);
        //        //var tcvm = ctvm.TemplateCollection;
        //        //tcvm.IsBusy = true;

        //        //var templateGuids = ParseTemplateGuids(ctvm.CopyItemData);
        //        //var usedTemplates = await MpDataModelProvider.GetTextTemplatesByGuidsAsync(templateGuids);

        //        //foreach (var cit in usedTemplates) {
        //        //    if (tcvm.Items.Any(x => x.TextTemplateGuid == cit.Guid)) {
        //        //        continue;
        //        //    }
        //        //    var ttvm = await tcvm.CreateTemplateViewModel(cit);
        //        //    tcvm.Items.Add(ttvm);
        //        //}

        //        //tcvm.IsBusy = false;

        //        string itemData = ctvm.CopyItemData;
        //        if (ctvm.ItemType == MpCopyItemType.FileList) {
        //            var fl_frag = new MpQuillFileListDataFragment() {
        //                fileItems = ctvm.FileItems.Select(x => new MpQuillFileListItemDataFragmentMessage() {
        //                    filePath = x.Path,
        //                    fileIconBase64 = x.IconBase64
        //                }).ToList()
        //            };
        //            itemData = fl_frag.SerializeJsonObjectToBase64();
        //        }

        //        return new MpQuillLoadContentRequestMessage() {
        //            contentHandle = ctvm.PublicHandle,
        //            contentType = ctvm.ItemType.ToString(),
        //            envName = "wpf",
        //            itemData = itemData,//ctvm.CopyItemData,
        //            usedTextTemplates = new List<MpTextTemplate>(), //usedTemplates,
        //            isPasteRequest = ctvm.IsPasting,
        //            isReadOnlyEnabled = ctvm.IsContentReadOnly
        //        };
        //    }
        //    return null;
        //}

        //private static async Task LoadTemplatesAsync(Control control) {
        //    if (control.DataContext is MpAvClipTileViewModel ctvm &&
        //        control is MpAvCefNetWebView wv) {
        //        var tcvm = ctvm.TemplateCollection;
        //        tcvm.IsBusy = true;

        //        // get templates present in realtime document
        //        string decodedTemplateGuidsRespStr = await wv.EvaluateJavascriptAsync("getDecodedTemplateGuids_ext()");
        //        Debugger.Break();
        //        var decodedTemplatesResp = MpJsonObject.DeserializeBase64Object<MpQuillActiveTemplateGuidsRequestMessage>(decodedTemplateGuidsRespStr);

        //        // verify template loaded in document exists, if does add to collection if not present on remove from document 
        //        List<MpTextTemplate> loadedTemplateItems = await MpDataModelProvider.GetTextTemplatesByGuidsAsync(decodedTemplatesResp.templateGuids);
        //        var loadedTemplateGuids_toRemove = decodedTemplatesResp.templateGuids.Where(x => loadedTemplateItems.All(y => y.Guid != x));

        //        foreach (var templateGuid_toRemove in loadedTemplateGuids_toRemove) {
        //            wv.ExecuteJavascript($"removeTemplatesByGuid('{templateGuid_toRemove}')");

        //            var templateViewModel_toRemove = tcvm.Items.FirstOrDefault(x => x.TextTemplateGuid == templateGuid_toRemove);
        //            if (templateViewModel_toRemove != null) {
        //                tcvm.Items.Remove(templateViewModel_toRemove);
        //            }
        //        }

        //        string htmlToDecode = string.Empty;
        //        bool isLoaded = await wv.EvaluateJavascriptAsync("checkIsEditorLoaded_ext()") == "true";
        //        if (isLoaded) {
        //            htmlToDecode = await wv.EvaluateJavascriptAsync("getHtml_ext()");
        //        } else {
        //            htmlToDecode = ctvm.CopyItemData;
        //        }
        //    }
        //}

        //private static List<string> ParseTemplateGuids(string text) {
        //    List<string> templateGuids = new List<string>();
        //    int curIdx = 0;
        //    while (curIdx < text.Length) {
        //        string curText = text.Substring(curIdx);
        //        var encodedRangeOpenTagIdx = curText.IndexOf(MpTextTemplate.TextTemplateOpenToken);
        //        if (encodedRangeOpenTagIdx < 0) {
        //            break;
        //        }
        //        var encodedRangeCloseTagIdx = curText.IndexOf(MpTextTemplate.TextTemplateCloseToken);
        //        if (encodedRangeCloseTagIdx < 0) {
        //            MpConsole.WriteLine(@"Corrupt text content, missing ending range tag. Item html: ");
        //            MpConsole.WriteLine(text);
        //            throw new Exception("Corrupt text content see console");
        //        }
        //        string templateGuid = curText.Substring(encodedRangeOpenTagIdx + MpTextTemplate.TextTemplateOpenToken.Length, encodedRangeCloseTagIdx - (encodedRangeOpenTagIdx + MpTextTemplate.TextTemplateOpenToken.Length));

        //        templateGuids.Add(templateGuid);
        //        curIdx = curIdx + encodedRangeCloseTagIdx + 1;
        //    }
        //    return templateGuids.Distinct().ToList();
        //}

        #endregion

        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEnabled",
                false);

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabled &&
                element is MpAvCefNetWebView wv) {
                if(isEnabled) {
                    wv.BrowserCreated += Wv_BrowserCreated;
                    wv.DevToolsProtocolEventAvailable += Wv_DevToolsProtocolEventAvailable;
                    wv.DetachedFromVisualTree += Wv_DetachedFromVisualTree;

                } else {
                    Wv_DetachedFromVisualTree(element, null);
                }
            }
        }

        private static void Wv_BrowserCreated(object sender, EventArgs e) {
            if (sender is MpAvCefNetWebView wv &&
                wv.DataContext is MpAvClipTileViewModel ctvm) {
                wv.IsEditorInitialized = false;

                wv.Navigated += Wv_Navigated;
                wv.Navigate(MpAvClipTrayViewModel.EditorPath);
            }

        }

        private static void Wv_Navigated(object sender, NavigatedEventArgs e) {
            if (e.Url == "about:blank") {
                return;
            }
            if (sender is MpAvCefNetWebView wv && wv.DataContext is MpAvClipTileViewModel ctvm) {
                Dispatcher.UIThread.Post(async () => {
                    while (!wv.IsDomLoaded) {
                        await Task.Delay(100);
                    }

                    //while (ctvm.IsAnyBusy) {
                    //    await Task.Delay(100);
                    //}
                    //if (ctvm.IsPlaceholder && !ctvm.IsPinned) {
                    //    return;
                    //}

                    //ctvm.IsBusy = true;
                    if (string.IsNullOrEmpty(ctvm.CachedState)) {

                        var req = new MpQuillInitMainRequestMessage() {
                            envName = MpPlatformWrapper.Services.OsInfo.OsType.ToString(),
                            isPlainHtmlConverter = false
                        };
                        wv.ExecuteJavascript($"initMain_ext('{req.SerializeJsonObjectToBase64()}')");
                    } else {
                        await wv.EvaluateJavascriptAsync($"setState_ext('{ctvm.CachedState}')");

                        ctvm.CachedState = null;
                    }
                    wv.IsEditorInitialized = true;

                    //var respStr = await wv.EvaluateJavascriptAsync($"initMain_ext('{req.SerializeJsonObjectToBase64()}')");
                    //var resp = MpJsonObject.DeserializeBase64Object<MpQuillInitMainResponseMessage>(respStr);
                    //if (resp.mainStatus == "Success") {
                    //    wv.IsEditorInitialized = true;
                    //    return;
                    //}
                    //// whats the status?
                    //Debugger.Break();
                });
            }
        }

        public static MpQuillEditorStateMessage GetEditorStateFromClipTile(MpAvClipTileViewModel ctvm) {
            return new MpQuillEditorStateMessage() {
                envName = MpPlatformWrapper.Services.OsInfo.OsType.ToString(),
                contentHandle = ctvm.PublicHandle,
                contentItemType = ctvm.ItemType.ToString(),
                contentData = ctvm.CopyItemData,
                isSubSelectionEnabled = ctvm.IsSubSelectionEnabled,
                isReadOnly = ctvm.IsContentReadOnly,
                isPastimgTemplate = ctvm.IsPastingTemplate
            };
        }
        private static void Wv_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var wv = sender as MpAvCefNetWebView;
            if (wv == null) {
                return;
            }
            wv.BrowserCreated -= Wv_BrowserCreated;
            wv.DevToolsProtocolEventAvailable -= Wv_DevToolsProtocolEventAvailable;
            wv.DetachedFromVisualTree -= Wv_DetachedFromVisualTree;
            wv.Navigated -= Wv_Navigated;
        }

        #endregion

        #endregion
        public static async Task SaveTextContentAsync(MpAvCefNetWebView wv) {
            await Task.Delay(1);
            //if (MpAvClipTrayViewModel.Instance.IsRequery ||
            //    MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
            //    return;
            //}

            //if (wv.DataContext is MpAvClipTileViewModel ctvm) {
            //    // flags detail info to reload in ctvm propertychanged
            //    var contentDataReq = new MpQuillContentDataRequestMessage() {
            //        forceFormat = ctvm.ItemType.ToString()
            //    };
            //    var contentDataRespStr = await wv.EvaluateJavascriptAsync($"contentRequest_ext('{contentDataReq.SerializeJsonObjectToBase64()}')");
            //    var contentDataResp = MpJsonObject.DeserializeBase64Object<MpQuillContentDataResponseMessage>(contentDataRespStr);
            //    ctvm.CopyItemData = contentDataResp.contentData;

            //    // this will trigger HtmlDataChanged
            //    ctvm.CopyItemData = await GetEncodedContentAsync(wv);

            //    //await LoadContentAsync(wv);
            //}
        }

        public static async Task<string> GetEncodedContentAsync(
            MpAvCefNetWebView wv,
            bool ignoreSubSelection = true,
            bool asPlainText = false) {
            var ctvm = wv.DataContext as MpAvClipTileViewModel;
            if (ctvm == null) {
                Debugger.Break();
            }

            switch (ctvm.ItemType) {
                case MpCopyItemType.FileList:
                    if (!ignoreSubSelection) {
                        return string.Join(Environment.NewLine, ctvm.FileItems.Where(x => x.IsSelected).Select(x => x.Path));
                    }
                    return string.Join(Environment.NewLine, ctvm.FileItems.Select(x => x.Path));
                case MpCopyItemType.Image:
                    return ctvm.CopyItemData;
                case MpCopyItemType.Text:
                    MpAvITextRange tr = null;
                    if (ignoreSubSelection) {
                        tr = wv.Document.ContentRange();
                    } else {
                        tr = wv.Selection;
                    }
                    //ignoreSubSelection ? rtb.Document.ContentRange() : rtb.Selection;
                    var encRangeReq = new MpQuillGetEncodedRangeDataRequestMessage() {
                        index = tr.Start.Offset,
                        length = tr.End.Offset - tr.Start.Offset,
                        isPlainText = asPlainText
                    };
                    string encRangeRespStr = await wv.EvaluateJavascriptAsync($"getEncodedDataFromRange_ext('{encRangeReq.SerializeJsonObjectToBase64()}')");
                    var encRangeResp = MpJsonObject.DeserializeBase64Object<MpQuillGetEncodedRangeDataResponseMessage>(encRangeRespStr);
                    string contentStr = encRangeResp.encodedRangeData;
                    //string contentStr = asPlainText ? await tr.ToEncodedPlainTextAsync() : await tr.ToEncodedRichTextAsync();
                    return contentStr;
            }
            MpConsole.WriteTraceLine("Unknown item type " + ctvm);
            return null;
        }

        public static async Task FinishContentCutAsync(MpAvClipTileViewModel drag_ctvm) {
            var wv = FindWebViewByViewModel(drag_ctvm);
            if (wv == null) {
                return;
            }
            bool delete_item = false;
            if (drag_ctvm.ItemType == MpCopyItemType.Text) {
                await wv.Selection.SetTextAsync(string.Empty);

                //string dpt = wv.Document.ToPlainText().Trim().Replace(Environment.NewLine, string.Empty);
                string dpt = await new MpAvTextRange(wv.Document.ContentStart, wv.Document.ContentEnd).GetTextAsync();
                dpt = dpt.Trim().Replace(Environment.NewLine, string.Empty);
                if (string.IsNullOrWhiteSpace(dpt)) {
                    delete_item = true;
                }
            } else if (drag_ctvm.ItemType == MpCopyItemType.FileList) {
                if (drag_ctvm.FileItems.Count == 0) {
                    delete_item = true;
                } else {
                    var fileItemsToRemove = drag_ctvm.FileItems.Where(x => x.IsSelected).ToList();
                    for (int i = 0; i < fileItemsToRemove.Count; i++) {
                        drag_ctvm.FileItems.Remove(fileItemsToRemove[i]);
                    }
                    //var paragraphsToRemove = wv.Document.GetAllTextElements()
                    //   .Where(x => x is MpFileItemParagraph).Cast<MpFileItemParagraph>()
                    //       .Where(x => fileItemsToRemove.Any(y => y == x.DataContext));

                    //paragraphsToRemove.ForEach(x => wv.Document.Blocks.Remove(x));
                }
            } else {
                return;
            }

            if (delete_item) {
                await drag_ctvm.CopyItem.DeleteFromDatabaseAsync();
            } else {
                await SaveTextContentAsync(wv);
            }
        }

        public static MpAvCefNetWebView FindWebViewByViewModel(MpAvClipTileViewModel ctvm) {
            var cv = MpAvMainWindow.Instance
                                 .GetVisualDescendants<MpAvCefNetWebView>()
                                 .FirstOrDefault(x => x.DataContext == ctvm);
            if (cv == null) {
               // Debugger.Break();
                return null;
            }
            return cv;
        }
        private static string ProcessEnableReadOnlyResponse(Control control, string enableReadOnlyResponse) {
            if (control.DataContext is MpAvClipTileViewModel ctvm) {
                MpConsole.WriteLine($"Tile content item '{ctvm.CopyItemTitle}' is readonly");

                var qrm = MpJsonObject.DeserializeBase64Object<MpQuillEnableReadOnlyResponseMessage>(enableReadOnlyResponse);

                if (ctvm.CopyItemData != qrm.itemData) {

                    ctvm.CopyItemData = qrm.itemData;
                }

                var ctv = control.GetVisualAncestor<MpAvClipTileView>();
                if (ctv != null) {
                    if (GetReadOnlyWidth(control) < MpAvClipTrayViewModel.Instance.DefaultItemWidth) {
                        SetReadOnlyWidth(control, MpAvClipTrayViewModel.Instance.DefaultItemWidth);
                    }
                    double deltaWidth = GetReadOnlyWidth(control) - ctv.Bounds.Width;

                    var resizeBorder = ctv.FindControl<Border>("ClipTileResizeBorder");
                    MpAvResizeExtension.ResizeByDelta(resizeBorder, deltaWidth, 0, false);
                }

                //MpMasterTemplateModelCollectionViewModel.Instance
                //    .UpdateAsync(qrm.updatedAllAvailableTextTemplates, qrm.userDeletedTemplateGuids)
                //    .FireAndForgetSafeAsync(ctvm);

                return qrm.itemData;
            }
            return null;
        }
        private static void ProcessDisableReadOnlyResponse(Control control, string disableReadOnlyResponse) {
            if (control.DataContext is MpAvClipTileViewModel civm) {
                MpConsole.WriteLine($"Tile content item '{civm.CopyItemTitle}' is editable");

                var qrm = MpJsonObject.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(disableReadOnlyResponse);

                var ctv = control.GetVisualAncestor<MpAvClipTileView>();
                if (ctv != null) {
                    SetReadOnlyWidth(control, ctv.Bounds.Width);

                    double deltaWidth = _EDITOR_DEFAULT_WIDTH - ctv.Bounds.Width;
                    if (deltaWidth > 0) {
                        var resizeBorder = ctv.FindControl<Border>("ClipTileResizeBorder");
                        MpAvResizeExtension.ResizeByDelta(resizeBorder, deltaWidth, 0, false);
                    }
                }
            }
        }


    }
}
