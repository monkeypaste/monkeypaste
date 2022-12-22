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
using Avalonia.Platform;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvAppendNotificationWindow : Window {
        #region Private Variables
        #endregion

        #region Statics

        public static MpAvAppendNotificationWindow Instance { get; private set; }

        public static async Task InitAsync() {
            if (Instance != null) {
                return;
            }

            Instance = new MpAvAppendNotificationWindow();

            var empty_msg_nf = new MpNotificationFormat() {
                NotificationType = MpNotificationType.AppendChanged,
                MaxShowTimeMs = -1,
                //BodyFormat = MpTextContentFormat.RichHtml,
                Body = MpAvClipTrayViewModel.Instance.ModalClipTileViewModel,
                IconSourceObj = "AppImage"
            };

            Instance.DataContext = await MpAppendNotificationViewModel.InitAsync(empty_msg_nf);

            if (Instance.Content is Control rootControl) {
                rootControl.AttachedToVisualTree += (s, e) => {
                    Instance.Hide();

                    MpPlatformWrapper.Services.ProcessWatcher.AddOtherThisAppHandle(Instance.PlatformImpl.Handle.Handle);
                };
            }
            Instance.Show();
        }
        #endregion

        public MpAppendNotificationViewModel BindingContext => DataContext as MpAppendNotificationViewModel;

        #region Constructors
        public MpAvAppendNotificationWindow() : base() {
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
