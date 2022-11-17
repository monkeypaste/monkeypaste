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
        #endregion

        static MpAvCefNetWebViewExtension() {
            IsContentReadOnlyProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsContentReadOnlyChanged(x, y));
            IsSubSelectionEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsSubSelectionEnabledChanged(x, y));
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            IsDevToolsVisibleProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsDevToolsVisibleChanged(x, y));
            IsHostSelectedProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsHostSelectedChanged(x, y));
            IsFindAndReplaceVisibleProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsFindAndReplaceVisibleChanged(x, y));
            CopyItemIdProperty.Changed.AddClassHandler<Control>((x, y) => HandleCopyItemIdChanged(x, y));
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


        #region IsFindAndReplaceVisible AvaloniaProperty
        public static bool GetIsFindAndReplaceVisible(AvaloniaObject obj) {
            return obj.GetValue(IsFindAndReplaceVisibleProperty);
        }
        public static void SetIsFindAndReplaceVisible(AvaloniaObject obj, bool value) {
            obj.SetValue(IsFindAndReplaceVisibleProperty, value);
        }

        public static readonly AttachedProperty<bool> IsFindAndReplaceVisibleProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsFindAndReplaceVisible",
                false,
                false);
        private static void HandleIsFindAndReplaceVisibleChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isFindReplaceVisible &&
               element is MpAvCefNetWebView wv &&
               wv.DataContext is MpAvClipTileViewModel ctvm &&
               wv.IsContentLoaded) {
                if(isFindReplaceVisible) {
                    wv.ExecuteJavascript($"showFindAndReplace_ext()");
                } else {
                    wv.ExecuteJavascript($"hideFindAndReplace_ext()");
                }                
            }
        }
        #endregion

        #region IsHostSelected AvaloniaProperty
        public static bool GetIsHostSelected(AvaloniaObject obj) {
            return obj.GetValue(IsHostSelectedProperty);
        }
        public static void SetIsHostSelected(AvaloniaObject obj, bool value) {
            obj.SetValue(IsHostSelectedProperty, value);
        }

        public static readonly AttachedProperty<bool> IsHostSelectedProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsHostSelected",
                false,
                false);
        private static void HandleIsHostSelectedChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isHostSelected &&
               element is MpAvCefNetWebView wv &&
               wv.DataContext is MpAvClipTileViewModel ctvm &&
               wv.IsContentLoaded) {
                var msg = new MpQuillIsHostSelectedChangedMessage() {
                    isHostSelected = isHostSelected
                };
                if(isHostSelected) {
                    wv.Focus();
                }
                wv.ExecuteJavascript($"hostIsSelectedChanged_ext('{msg.SerializeJsonObjectToBase64()}')");
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
                element is MpAvCefNetWebView wv && 
                wv.DataContext is MpAvClipTileViewModel ctvm &&
                wv.IsContentLoaded //&&
               // !wv.BindingContext.IsReloading
                ) {
                //Control resizeControl = null;
                //var ctv = wv.GetVisualAncestor<MpAvClipTileView>();
                //if(ctv != null) {
                //    resizeControl = ctv.FindControl<Control>("ClipTileResizeBorder");
                //}
                // only signal read only change after webview is loaded
                if (isReadOnly) {
                    MpAvResizeExtension.ResizeAnimated(wv, ctvm.ReadOnlyWidth, ctvm.ReadOnlyHeight);
                    string enableReadOnlyRespStr = await wv.EvaluateJavascriptAsync("enableReadOnly_ext()");
                    var qrm = MpJsonObject.DeserializeBase64Object<MpQuillEditorContentChangedMessage>(enableReadOnlyRespStr);
                    wv.Document.ProcessContentChangedMessage(qrm);
                } else {
                    MpAvResizeExtension.ResizeAnimated(wv, ctvm.EditableWidth, ctvm.EditableHeight);
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
                wv.IsContentLoaded &&
                wv.DataContext is MpAvClipTileViewModel ctvm &&
                !ctvm.IsReloading) {
                if (isSubSelectionEnabled) {
                    wv.ExecuteJavascript("enableSubSelection_ext()");
                    if(ctvm.HasTemplates && !ctvm.IsDropOverTile) {
                        MpAvResizeExtension.ResizeAnimated(wv, ctvm.EditableWidth, ctvm.EditableHeight);
                    }
                } else {
                    MpAvResizeExtension.ResizeAnimated(wv, ctvm.ReadOnlyWidth, ctvm.ReadOnlyHeight);
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

        #region CopyItemId AvaloniaProperty

        public static int GetCopyItemId(AvaloniaObject obj) {
            return obj.GetValue(CopyItemIdProperty);
        }

        public static void SetCopyItemId(AvaloniaObject obj, int value) {
            obj.SetValue(CopyItemIdProperty, value);
        }
        public static readonly AttachedProperty<int> CopyItemIdProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "CopyItemId",
                0,
                false);

        private static void HandleCopyItemIdChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvCefNetWebView wv &&
                wv.DataContext is MpAvClipTileViewModel ctvm &&
                !ctvm.IsReloading) {
                Dispatcher.UIThread.Post(async () => {
                    // NOTE IsContentLoaded is set true in binding response (notifyLoadComplete) after editor loadContent()
                    wv.IsContentLoaded = false;
                    
                    if (ctvm.IsPlaceholder && !ctvm.IsPinned) {
                        return;
                    }
                    while (!wv.IsEditorInitialized) {
                        await Task.Delay(100);
                    }
                    while(ctvm.FileItemCollectionViewModel.IsAnyBusy) {
                        // wait for file icons to populate from ctvm.Init
                        await Task.Delay(100);
                    }

                    var loadContentMsg = new MpQuillLoadContentRequestMessage() {
                        contentHandle = ctvm.PublicHandle,
                        contentType = ctvm.ItemType.ToString(),
                        itemData = ctvm.EditorFormattedItemData,
                        isPasteRequest = ctvm.IsPasting
                    };

                    if(!string.IsNullOrEmpty(MpAvSearchBoxViewModel.Instance.SearchText)) {
                        var sbvm = MpAvSearchBoxViewModel.Instance;
                        loadContentMsg.searchText = sbvm.SearchText;
                        loadContentMsg.isCaseSensitive = sbvm.Filters.FirstOrDefault(x => x.FilterType == MpContentFilterType.CaseSensitive).IsChecked.IsTrue();
                        loadContentMsg.isWholeWord = sbvm.Filters.FirstOrDefault(x => x.FilterType == MpContentFilterType.WholeWord).IsChecked.IsTrue();
                        loadContentMsg.useRegex = sbvm.Filters.FirstOrDefault(x => x.FilterType == MpContentFilterType.Regex).IsChecked.IsTrue();
                    }
                    string msgStr = loadContentMsg.SerializeJsonObjectToBase64();
                    
                    
                    wv.ExecuteJavascript($"loadContent_ext('{msgStr}')");
                });
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
                    wv.DetachedFromVisualTree += Wv_DetachedFromVisualTree;

                } else {
                    Wv_DetachedFromVisualTree(element, null);
                }
            }
        }

        private static void Wv_BrowserCreated(object sender, EventArgs e) {
            if (sender is MpAvCefNetWebView wv &&
                wv.DataContext is MpAvClipTileViewModel ctvm) {
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
                isPastimgTemplate = ctvm.IsPasting
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
        
    }
}
