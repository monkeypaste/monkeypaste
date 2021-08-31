using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace MonkeyPaste {
    public partial class MpMainShell : Shell {
        public static bool IsLoaded { get; set; } = false;

        public MpSettingsPageView SettingsPageView { get; set; }

        public MpIKeyboardInteractionService LayoutService { get; set; }
        public MpILocalStorage StorageService { get; set; }
        public MpIGlobalTouch GlobalTouchService { get; set; }
        public static MpINativeInterfaceWrapper NativeWrapper { get; set; }
        public MpIDbInfo DbInfo { get; set; }

        public event EventHandler<object> OnShellDisappearing;

        public MpMainShell() {
            IsLoaded = true;
            InitializeComponent();
        }


        public MpMainShell(MpINativeInterfaceWrapper niw) {
            IsLoaded = true;

            NativeWrapper = niw;
            GlobalTouchService = niw.GetGlobalTouch();
            LayoutService = niw.GetKeyboardInteractionService();
            StorageService = niw.GetLocalStorageManager();
            DbInfo = niw.GetDbInfo();

            InitializeComponent();
        }

        public ICommand OpenSettingsPageCommand => new Command(async () => {
            SettingsPageView = new MpSettingsPageView() ?? SettingsPageView;
            await Navigation.PushModalAsync(SettingsPageView);
        });        

        private void Button_Clicked(object sender, EventArgs e) {
            OpenSettingsPageCommand.Execute(null);
        }

        private void Shell_Disappearing(object sender, EventArgs e) {
            return;
        }

        private void StackLayout_LayoutChanged(object sender, EventArgs e) {
            return;
        }
    }
}
