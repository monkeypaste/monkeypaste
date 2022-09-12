using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebViewControl;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCefWebViewExtension {
        static MpAvCefWebViewExtension() {
            IsContentReadOnlyProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsContentReadOnlyChanged(x, y));
            ContentHtmlProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsContentHtmlChanged(x, y));
        }

        #region Properties

        #region ContentHtml AvaloniaProperty

        public static string GetContentHtml(AvaloniaObject obj) {
            return obj.GetValue(ContentHtmlProperty);
        }

        public static void SetContentHtml(AvaloniaObject obj, string value) {
            obj.SetValue(ContentHtmlProperty, value);
        }

        public static readonly AttachedProperty<string> ContentHtmlProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "ContentHtml",
                string.Empty);

        private static void HandleIsContentHtmlChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is string contentHtmlStr &&
                element is WebViewControl.WebView wv) {                
                wv.Navigated += (url, frameName) => {
                    LoadContentAsync(wv).FireAndForgetSafeAsync(MpAvClipTrayViewModel.Instance);
                };
            }

            async Task LoadContentAsync(Control control) {
                if(control.DataContext is MpAvClipTileViewModel ctvm) {
                    while(ctvm.IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    if(ctvm.IsPlaceholder && !ctvm.IsPinned) {
                        return;
                    }

                    var lrm = await CreateLoadRequestMessageAsync(wv);
                    var loadReqJsonStr = lrm.SerializeJsonObject();

                    string loadResponseMsgStr = await wv.EvaluateScript<string>($"init_ext('{loadReqJsonStr}')");
                    MpQuillLoadResponseMessage loadResponseMsg = MpJsonObject.DeserializeObject<MpQuillLoadResponseMessage>(loadResponseMsgStr);
                    ctvm.UnformattedContentSize = new Size(loadResponseMsg.contentWidth, loadResponseMsg.contentHeight);

                    MpConsole.WriteLine($"Tile Content Item '{ctvm.CopyItemTitle}' is loaded");
                }
            }

            async Task<MpQuillLoadRequestMessage> CreateLoadRequestMessageAsync(Control control) {
                if (control.DataContext is MpAvClipTileViewModel ctvm &&
                    control is WebViewControl.WebView wv) {
                    var tcvm = ctvm.TemplateCollection;
                    tcvm.IsBusy = true;

                    var templateGuids = ParseTemplateGuids(ctvm.CopyItemData);
                    var usedTemplates = await MpDataModelProvider.GetTextTemplatesByGuidsAsync(templateGuids);

                    foreach (var cit in usedTemplates) {
                        if(tcvm.Items.Any(x=>x.TextTemplateGuid == cit.Guid)) {
                            continue;
                        }
                        var ttvm = await tcvm.CreateTemplateViewModel(cit);
                        tcvm.Items.Add(ttvm);
                    }

                    tcvm.IsBusy = false;

                    return new MpQuillLoadRequestMessage() {
                        envName = "wpf",
                        itemData = ctvm.CopyItemData,
                        usedTextTemplates = usedTemplates,
                        isPasteRequest = ctvm.IsPasting,
                        isReadOnlyEnabled = ctvm.IsContentReadOnly
                    };
                }
                return null;
            }

            async Task LoadTemplatesAsync(Control control) {
                if (control.DataContext is MpAvClipTileViewModel ctvm &&
                    control is WebViewControl.WebView wv) {
                    var tcvm = ctvm.TemplateCollection;
                    tcvm.IsBusy = true;

                    // get templates present in realtime document
                    var decodedTemplateGuidsObj = await wv.EvaluateScript<object>("getDecodedTemplateGuids_ext()");
                    Debugger.Break();
                    List<string> loadedTemplateGuids = MpJsonObject.DeserializeObject<List<string>>(decodedTemplateGuidsObj);

                    // verify template loaded in document exists, if does add to collection if not present on remove from document 
                    List<MpTextTemplate> loadedTemplateItems = await MpDataModelProvider.GetTextTemplatesByGuidsAsync(loadedTemplateGuids);
                    var loadedTemplateGuids_toRemove = loadedTemplateGuids.Where(x => loadedTemplateItems.All(y => y.Guid != x));
                    
                    foreach (var templateGuid_toRemove in loadedTemplateGuids_toRemove) {
                        wv.ExecuteScript($"removeTemplatesByGuid('{templateGuid_toRemove}')");

                        var templateViewModel_toRemove = tcvm.Items.FirstOrDefault(x => x.TextTemplateGuid == templateGuid_toRemove);
                        if (templateViewModel_toRemove != null) {
                            tcvm.Items.Remove(templateViewModel_toRemove);
                        }
                    }

                    string htmlToDecode = string.Empty;
                    bool isLoaded = await wv.EvaluateScript<bool>("checkIsLoaded_ext()");
                    if(isLoaded) {
                        htmlToDecode = await wv.EvaluateScript<string>("getHtml_ext()");
                    } else {
                        htmlToDecode = ctvm.CopyItemData;
                    }
                }
            }

            List<string> ParseTemplateGuids(string text) {
                List<string> templateGuids = new List<string>();
                int curIdx = 0;
                while(curIdx < text.Length) {
                    var encodedRangeOpenTagIdx = text.Substring(curIdx).IndexOf(MpTextTemplate.TextTemplateOpenToken);
                    if (encodedRangeOpenTagIdx < 0) {
                        break;
                    }
                    var encodedRangeCloseTagIdx = text.Substring(encodedRangeOpenTagIdx).IndexOf(MpTextTemplate.TextTemplateCloseToken);
                    if (encodedRangeCloseTagIdx < 0) {
                        MpConsole.WriteLine(@"Corrupt text content, missing ending range tag. Item html: ");
                        MpConsole.WriteLine(text);
                        throw new Exception("Corrupt text content see console");
                    }
                    string templateGuid = text.Substring(encodedRangeOpenTagIdx + MpTextTemplate.TextTemplateOpenToken.Length, encodedRangeCloseTagIdx);

                    templateGuids.Add(templateGuid);
                    curIdx = encodedRangeCloseTagIdx + 1;
                }
                return templateGuids.Distinct().ToList();
            }
        }

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
                element is WebViewControl.WebView wv) {
                if(wv.IsJavascriptEngineInitialized) {
                    bool isInitialized = await wv.EvaluateScript<bool>("checkIsLoaded()");
                    if(isInitialized) {
                        // only signal read only change after webview is loaded
                        if (isReadOnly) {
                            var enableReadOnlyResp = await wv.EvaluateScript<string>("enableReadOnly()");
                            ProcessEnableReadOnlyResponse(wv, enableReadOnlyResp);
                        } else {
                            MpQuillDisableReadOnlyRequestMessage drorMsg = CreateDisableReadOnlyMessage(wv);
                            var disableReadOnlyResp = await wv.EvaluateScript<string>($"disableReadOnly('{drorMsg.SerializeJsonObject()}')");
                            ProcessDisableReadOnlyResponse(wv, disableReadOnlyResp);
                        }
                    }
                }
            } 

            string ProcessEnableReadOnlyResponse(Control control, string enableReadOnlyResponse) {
                if (control.DataContext is MpAvClipTileViewModel ctvm) {
                    MpConsole.WriteLine($"Tile content item '{ctvm.CopyItemTitle}' is readonly");
                    
                    var qrm = MpJsonObject.DeserializeObject<MpQuillEnableReadOnlyResponseMessage>(enableReadOnlyResponse);

                    ctvm.CopyItemData = qrm.itemData;
                    MpConsole.WriteLine("Skipping writing updated item data: ");
                    MpConsole.WriteLine(qrm.itemData);

                    //var ctcv = fe.GetVisualAncestor<MpAvClipTileView>();
                    //if (ctcv != null) {
                    //    ctcv.TileResizeBehavior.ResizeWidth(GetReadOnlyWidth(fe));
                    //}

                    MpMasterTemplateModelCollectionViewModel.Instance.UpdateAsync(qrm.updatedAllAvailableTextTemplates, qrm.userDeletedTemplateGuids).FireAndForgetSafeAsync(ctvm);

                    return qrm.itemData;
                }
                return null;
            }
            void ProcessDisableReadOnlyResponse(Control fe, string disableReadOnlyResponse) {
                if (fe.DataContext is MpAvClipTileViewModel civm) {
                    MpConsole.WriteLine($"Tile content item '{civm.CopyItemTitle}' is editable");

                    var qrm = MpJsonObject.DeserializeObject<MpQuillDisableReadOnlyResponseMessage>(disableReadOnlyResponse);

                    //var ctcv = fe.GetVisualAncestor<MpAvClipTileView>();
                    //if (ctcv != null) {
                    //    SetReadOnlyWidth(fe, ctcv.ActualWidth);

                    //    if (ctcv.ActualWidth < 900) {
                    //        ctcv.TileResizeBehavior.ResizeWidth(900);// qrm.editorWidth - ctcv.ActualWidth, 0);
                    //    }
                    //} else {
                    //    SetReadOnlyWidth(fe, MpClipTileViewModel.DefaultBorderWidth);
                    //}
                }
            }

            MpQuillDisableReadOnlyRequestMessage CreateDisableReadOnlyMessage(Control fe) {
                var ctvm = fe.DataContext as MpAvClipTileViewModel;
                MpConsole.WriteLine($"Tile content item '{ctvm.CopyItemTitle}' is editable");

                MpQuillDisableReadOnlyRequestMessage drorMsg = new MpQuillDisableReadOnlyRequestMessage() {
                    allAvailableTextTemplates = MpMasterTemplateModelCollectionViewModel.Instance.AllTemplates.ToList(),
                    editorHeight = ctvm.EditorHeight//fe.GetVisualAncestor<MpRtbContentView>().ActualHeight
                };
                return drorMsg;
            }            
        }

        #endregion

        #endregion

        public static void InitCef() {
            string cefLogPath = Path.Combine(Environment.CurrentDirectory, "ceflog.txt");
            if (File.Exists(cefLogPath)) {
                File.Delete(cefLogPath);
            }

            if (!OperatingSystem.IsLinux()) {
                WebView.Settings.OsrEnabled = true;
                WebView.Settings.LogFile = "ceflog.txt";
                //WebView.Settings.EnableErrorLogOnly = true;

                string cefCacheDir = Path.Combine(Environment.CurrentDirectory, "cefcache");
                if (Directory.Exists(cefCacheDir)) {
                    Directory.Delete(cefCacheDir, true);
                }
                Directory.CreateDirectory(cefCacheDir);
                WebView.Settings.CachePath = cefCacheDir;
            }
        }
    }
}
