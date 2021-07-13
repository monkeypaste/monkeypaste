using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace MonkeyPaste {
    public partial class MpMainShell : Shell {
        public MpSettingsPageView SettingsPageView { get; set; }
        public MpIKeyboardInteractionService LayoutService { get; set; }
        public MpILocalStorage StorageService { get; set; }

        public MpMainShell() {
            InitializeComponent();
        }

        public MpMainShell(MpINativeInterfaceWrapper niw) : this() {
            LayoutService = niw.GetKeyboardInteractionService();
            StorageService = niw.GetLocalStorageManager();
        }

        public ICommand OpenSettingsPageCommand => new Command(async () => {
            SettingsPageView = new MpSettingsPageView() ?? SettingsPageView;
            await Navigation.PushModalAsync(SettingsPageView);
        });

        private void Button_Clicked(object sender, EventArgs e) {
            OpenSettingsPageCommand.Execute(null);
        }
    }
}
