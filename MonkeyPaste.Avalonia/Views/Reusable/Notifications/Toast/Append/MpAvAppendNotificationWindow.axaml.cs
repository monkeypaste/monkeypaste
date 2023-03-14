using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvAppendNotificationWindow : MpAvWindow {
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
                    Instance.WindowState = WindowState.Minimized;
                    Instance.Hide();
                    Mp.Services.ProcessWatcher.AddOtherThisAppHandle(Instance.PlatformImpl.Handle.Handle);
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
                if (BindingContext != null &&
                    BindingContext.Body is MpAvClipTileViewModel ctvm &&
                    ctvm.GetContentView() is MpIContentView cv) {
                    cv.ShowDevTools();
                }
            });
    }

}
