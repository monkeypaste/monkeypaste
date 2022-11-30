using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;
using MonkeyPaste;
using MonkeyPaste.Common;
using System.Linq;
using Avalonia.Threading;
using System;
using PropertyChanged;
using System.Collections.Generic;
using System.Diagnostics;
using MonkeyPaste.Common.Avalonia;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMessageNotificationWindow : Window {
        #region Private Variables
        #endregion

        #region Statics

        public static MpAvMessageNotificationWindow WebViewInstance { get; private set; }

        public static async Task<MpAvMessageNotificationWindow> CreateWebViewInstanceAsync() {
            if(WebViewInstance != null) {
                return WebViewInstance;
            }
            
            WebViewInstance = new MpAvMessageNotificationWindow();

            //var empty_ctvm = await MpAvClipTrayViewModel.Instance.CreateClipTileViewModel(null);

            var empty_msg_nf = new MpNotificationFormat() {
                NotificationType = MpNotificationType.Message,
                MaxShowTimeMs = -1,
                BodyFormat = MpTextContentFormat.RichHtml,
                Body = MpAvClipTrayViewModel.Instance.AppendNotifierViewModel,
                Title = "Empty Title",
                IconSourceStr = "AppImage"
            };
            var cc = WebViewInstance.FindControl<ContentControl>("NotificationBodyContentControl");
            cc.DataContextChanged += (s, e) => {
                WebViewInstance.InvalidateAll();
            };

            WebViewInstance.GetObservable(Window.IsVisibleProperty).Subscribe(value => {
                //if(!WebViewInstance.IsVisible) {
                //    return;
                //}
                //int x = WebViewInstance.Position.X;
                //int y = WebViewInstance.Position.Y;
                //int cx = (int)WebViewInstance.Width;
                //int cy = (int)WebViewInstance.Height;
                //MpAvTopMostWindow.InitTopmostWindow(WebViewInstance.PlatformImpl.Handle.Handle, x, y, cx, cy);
            });
            WebViewInstance.DataContext = await MpNotificationBuilder.CreateNotifcationViewModelAsync(empty_msg_nf);

            if(WebViewInstance.Content is Control rootControl) {
                rootControl.AttachedToVisualTree += async(s, e) => {
                    if (OperatingSystem.IsWindows()) {
                        // hide converter window from windows alt-tab menu

                        //MpAvToolWindow_Win32.InitToolWindow(WebViewInstance.PlatformImpl.Handle.Handle);

                    }
                    WebViewInstance.Hide();
                    var wv = WebViewInstance.GetVisualDescendant<MpAvCefNetWebView>();
                    while(wv == null) {
                        await Task.Delay(10);
                        wv = WebViewInstance.GetVisualDescendant<MpAvCefNetWebView>();
                    }
                    wv.ContentUrl = $"{wv.DefaultContentUrl}?{MpAvCefNetWebView.APPEND_NOTIFIER_PARAMS}";

                };
            }
           
            return WebViewInstance;
        }
        #endregion

        public MpMessageNotificationViewModel BindingContext => DataContext as MpMessageNotificationViewModel;
        public MpAvMessageNotificationWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);
            var cc = this.FindControl<ContentControl>("NotificationBodyContentControl");
            cc.ApplyTemplate();
        }
        public ICommand ShowNotifierDevToolsCommand => new MpCommand(
            () => {
                var wv = this.GetVisualDescendant<MpAvCefNetWebView>();
                if(wv == null) {
                    return;
                }
                wv.ShowDevTools();
            });
    }

}
