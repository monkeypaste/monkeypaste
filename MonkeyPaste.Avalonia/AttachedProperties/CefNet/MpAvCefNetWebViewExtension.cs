using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Threading;
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

        private static readonly double _EDITOR_DEFAULT_WIDTH = 1200;

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
                wv.IsEditorInitialized) {
                // only signal read only change after webview is loaded
                if (isReadOnly) {
                    var enableReadOnlyResp = await wv.EvaluateJavascriptAsync("enableReadOnly()");
                    ProcessEnableReadOnlyResponse(wv, enableReadOnlyResp);
                } else {
                    MpQuillDisableReadOnlyRequestMessage drorMsg = CreateDisableReadOnlyMessage(wv);
                    string drorMsgStr = drorMsg.Serialize();
                    MpConsole.WriteLine("Disable ReadOnly request:");
                    MpConsole.WriteLine(drorMsgStr);
                    string disableReadOnlyResp = await wv.EvaluateJavascriptAsync($"disableReadOnly('{drorMsgStr}')");
                    ProcessDisableReadOnlyResponse(wv, disableReadOnlyResp);
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
                wv.IsEditorInitialized) {
                  if (isSubSelectionEnabled) {
                    wv.ExecuteJavascript("enableSubSelection()");
                } else {
                    wv.ExecuteJavascript("disableSubSelection()");
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
            if (e.NewValue is string htmlDataStr &&
                element is MpAvCefNetWebView wv &&
                wv.IsEditorInitialized) {
                wv.ExecuteJavascript($"setHtml('{htmlDataStr}')");
            }
        }

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
                }
            }
        }
        #endregion

        #endregion


        private static void Wv_BrowserCreated(object sender, EventArgs e) {
            if (sender is MpAvCefNetWebView wv &&
                wv.DataContext is MpAvClipTileViewModel ctvm) {
                wv.IsEditorInitialized = false;

                wv.Navigated += Wv_Navigated;
                wv.Navigate(ctvm.EditorPath);
            }

        }

        private static void Wv_Navigated(object sender, NavigatedEventArgs e) {
            if (sender is MpAvCefNetWebView wv) {
                Dispatcher.UIThread.Post(() => {
                    //await Task.Delay(3000);
                    LoadContentAsync(wv).FireAndForgetSafeAsync(MpAvClipTrayViewModel.Instance);
                    return;
                });
            }
        }
        private static async Task LoadContentAsync(Control control) {
            if (control is MpAvCefNetWebView wv &&
                control.DataContext is MpAvClipTileViewModel ctvm) {
                while (ctvm.IsAnyBusy) {
                    await Task.Delay(100);
                }
                if (ctvm.IsPlaceholder && !ctvm.IsPinned) {
                    return;
                }

                ctvm.IsBusy = true;

                var lrm = await CreateLoadRequestMessageAsync(wv);
                var loadReqJsonStr = lrm.Serialize();
                string loadResponseMsgStr = null;
                while (loadResponseMsgStr == null) {
                    string resp = await wv.EvaluateJavascriptAsync($"init('{loadReqJsonStr}')");
                    if (resp == MpCefNetApplication.JS_REF_ERROR || resp == null) {
                        await Task.Delay(100);
                        continue;
                    }
                    loadResponseMsgStr = resp;
                }

                MpQuillLoadResponseMessage loadResponseMsg = MpJsonObject.DeserializeObject<MpQuillLoadResponseMessage>(loadResponseMsgStr);

                ctvm.UnformattedContentSize = new Size(loadResponseMsg.contentWidth, loadResponseMsg.contentHeight);

                wv.IsEditorInitialized = true;
                ctvm.IsBusy = false;

                MpConsole.WriteLine($"Tile Content Item '{ctvm.CopyItemTitle}' is loaded");
            }
        }

        private static async Task<MpQuillLoadRequestMessage> CreateLoadRequestMessageAsync(Control control) {
            if (control.DataContext is MpAvClipTileViewModel ctvm &&
                control is MpAvCefNetWebView wv) {
                var tcvm = ctvm.TemplateCollection;
                tcvm.IsBusy = true;

                var templateGuids = ParseTemplateGuids(ctvm.CopyItemData);
                var usedTemplates = await MpDataModelProvider.GetTextTemplatesByGuidsAsync(templateGuids);

                foreach (var cit in usedTemplates) {
                    if (tcvm.Items.Any(x => x.TextTemplateGuid == cit.Guid)) {
                        continue;
                    }
                    var ttvm = await tcvm.CreateTemplateViewModel(cit);
                    tcvm.Items.Add(ttvm);
                }

                tcvm.IsBusy = false;

                return new MpQuillLoadRequestMessage() {
                    envName = "wpf",
                    itemEncodedHtmlData = ctvm.CopyItemData,
                    usedTextTemplates = usedTemplates,
                    isPasteRequest = ctvm.IsPasting,
                    isReadOnlyEnabled = ctvm.IsContentReadOnly
                };
            }
            return null;
        }

        private static async Task LoadTemplatesAsync(Control control) {
            if (control.DataContext is MpAvClipTileViewModel ctvm &&
                control is MpAvCefNetWebView wv) {
                var tcvm = ctvm.TemplateCollection;
                tcvm.IsBusy = true;

                // get templates present in realtime document
                var decodedTemplateGuidsObj = await wv.EvaluateJavascriptAsync("getDecodedTemplateGuids()");
                Debugger.Break();
                List<string> loadedTemplateGuids = MpJsonObject.DeserializeObject<List<string>>(decodedTemplateGuidsObj);

                // verify template loaded in document exists, if does add to collection if not present on remove from document 
                List<MpTextTemplate> loadedTemplateItems = await MpDataModelProvider.GetTextTemplatesByGuidsAsync(loadedTemplateGuids);
                var loadedTemplateGuids_toRemove = loadedTemplateGuids.Where(x => loadedTemplateItems.All(y => y.Guid != x));

                foreach (var templateGuid_toRemove in loadedTemplateGuids_toRemove) {
                    wv.ExecuteJavascript($"removeTemplatesByGuid('{templateGuid_toRemove}')");

                    var templateViewModel_toRemove = tcvm.Items.FirstOrDefault(x => x.TextTemplateGuid == templateGuid_toRemove);
                    if (templateViewModel_toRemove != null) {
                        tcvm.Items.Remove(templateViewModel_toRemove);
                    }
                }

                string htmlToDecode = string.Empty;
                bool isLoaded = await wv.EvaluateJavascriptAsync("checkIsEditorLoaded()") == "true";
                if (isLoaded) {
                    htmlToDecode = await wv.EvaluateJavascriptAsync("getHtml()");
                } else {
                    htmlToDecode = ctvm.CopyItemData;
                }
            }
        }

        private static List<string> ParseTemplateGuids(string text) {
            List<string> templateGuids = new List<string>();
            int curIdx = 0;
            while (curIdx < text.Length) {
                string curText = text.Substring(curIdx);
                var encodedRangeOpenTagIdx = curText.IndexOf(MpTextTemplate.TextTemplateOpenToken);
                if (encodedRangeOpenTagIdx < 0) {
                    break;
                }
                var encodedRangeCloseTagIdx = curText.IndexOf(MpTextTemplate.TextTemplateCloseToken);
                if (encodedRangeCloseTagIdx < 0) {
                    MpConsole.WriteLine(@"Corrupt text content, missing ending range tag. Item html: ");
                    MpConsole.WriteLine(text);
                    throw new Exception("Corrupt text content see console");
                }
                string templateGuid = curText.Substring(encodedRangeOpenTagIdx + MpTextTemplate.TextTemplateOpenToken.Length, encodedRangeCloseTagIdx - (encodedRangeOpenTagIdx + MpTextTemplate.TextTemplateOpenToken.Length));

                templateGuids.Add(templateGuid);
                curIdx = curIdx + encodedRangeCloseTagIdx + 1;
            }
            return templateGuids.Distinct().ToList();
        }
        private static string ProcessEnableReadOnlyResponse(Control control, string enableReadOnlyResponse) {
            if (control.DataContext is MpAvClipTileViewModel ctvm) {
                MpConsole.WriteLine($"Tile content item '{ctvm.CopyItemTitle}' is readonly");

                var qrm = MpJsonObject.DeserializeObject<MpQuillEnableReadOnlyResponseMessage>(enableReadOnlyResponse);

                if (ctvm.CopyItemData != qrm.itemEncodedHtmlData) {

                    ctvm.CopyItemData = qrm.itemEncodedHtmlData;
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

                MpMasterTemplateModelCollectionViewModel.Instance
                    .UpdateAsync(qrm.updatedAllAvailableTextTemplates, qrm.userDeletedTemplateGuids)
                    .FireAndForgetSafeAsync(ctvm);

                return qrm.itemEncodedHtmlData;
            }
            return null;
        }
        private static void ProcessDisableReadOnlyResponse(Control control, string disableReadOnlyResponse) {
            if (control.DataContext is MpAvClipTileViewModel civm) {
                MpConsole.WriteLine($"Tile content item '{civm.CopyItemTitle}' is editable");

                //var qrm = MpJsonObject.DeserializeObject<MpQuillDisableReadOnlyResponseMessage>(disableReadOnlyResponse);

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

        private static MpQuillDisableReadOnlyRequestMessage CreateDisableReadOnlyMessage(Control fe) {
            var ctvm = fe.DataContext as MpAvClipTileViewModel;
            MpConsole.WriteLine($"Tile content item '{ctvm.CopyItemTitle}' is editable");

            MpQuillDisableReadOnlyRequestMessage drorMsg = new MpQuillDisableReadOnlyRequestMessage() {
                allAvailableTextTemplates = new List<MpTextTemplate>(),// MpMasterTemplateModelCollectionViewModel.Instance.AllTemplates.ToList(),
                editorHeight = ctvm.EditorHeight
            };
            return drorMsg;
        }

    }
}
