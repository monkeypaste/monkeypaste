using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpSettingsPageView : ContentPage {
        
        public MpSettingsPageView() {
            InitializeComponent();
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
        }

        public ICommand Start => new Command(async () => {
            //var spvm = BindingContext as MpSettingsPageViewModel;
            //MpViewModelBase.User = spvm.Username;

            //if(ChatPageView == null) {
            //    var cpvm = new MpChatPageViewModel(new MpSyncService());
            //    ChatPageView = new MpChatPageView(cpvm);
            //}
            //await Navigation.PushModalAsync(ChatPageView);
        });

        private void Button_Clicked(object sender, EventArgs e) {
            Start.Execute(null);
        }
    }
}