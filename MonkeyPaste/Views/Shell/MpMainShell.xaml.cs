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

        public MpMainShell() {
            InitializeComponent();
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
