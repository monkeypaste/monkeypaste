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
    public partial class MpAvAppendNotificationWindow : Window {
        #region Private Variables
        #endregion

        #region Statics

        public static MpAvAppendNotificationWindow Instance { get; private set; }

        public static async Task<MpAvAppendNotificationWindow> CreateWebViewInstanceAsync() {
            if(Instance != null) {
                return Instance;
            }
            
            Instance = new MpAvAppendNotificationWindow();

            //var empty_ctvm = await MpAvClipTrayViewModel.Instance.CreateClipTileViewModel(null);

            var empty_msg_nf = new MpNotificationFormat() {
                NotificationType = MpNotificationType.AppendChanged,
                MaxShowTimeMs = -1,
                //BodyFormat = MpTextContentFormat.RichHtml,
                Body = MpAvClipTrayViewModel.Instance.AppendNotifierViewModel,
                IconSourceStr = "AppImage"
            };

            Instance.DataContext = await MpAppendNotificationViewModel.InitAsync(empty_msg_nf);

            if(Instance.Content is Control rootControl) {
                rootControl.AttachedToVisualTree += async(s, e) => {
                    if (OperatingSystem.IsWindows()) {
                        // hide converter window from windows alt-tab menu

                        //MpAvToolWindow_Win32.InitToolWindow(WebViewInstance.PlatformImpl.Handle.Handle);

                    }
                    Instance.Hide();
                    var wv = Instance.GetVisualDescendant<MpAvCefNetWebView>();
                    while(wv == null) {
                        await Task.Delay(10);
                        wv = Instance.GetVisualDescendant<MpAvCefNetWebView>();
                    }
                    wv.ContentUrl = $"{wv.DefaultContentUrl}?{MpAvCefNetWebView.APPEND_NOTIFIER_PARAMS}";

                };
            }
           
            return Instance;
        }
        public static async Task InitAsync() {
            await CreateWebViewInstanceAsync();
            if (Instance != null) {
                Instance.Show();
            }
        }
        #endregion

        public MpMessageNotificationViewModel BindingContext => DataContext as MpMessageNotificationViewModel;

        #region Constructors
        public MpAvAppendNotificationWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        #endregion

        #region Public Methods



        #endregion
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
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
