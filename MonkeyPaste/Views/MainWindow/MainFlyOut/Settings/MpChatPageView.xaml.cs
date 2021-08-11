using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using System.Reflection;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpChatPageView : ContentPage {
        private MpChatPageViewModel _chatViewModel;

        public MpChatPageView(MpChatPageViewModel viewModel) {
            _chatViewModel = viewModel;
            
            InitializeComponent();

            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            
            _chatViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
            
            BindingContext = _chatViewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            var safeArea = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
            MainGrid.HeightRequest = Height - safeArea.Top - safeArea.Bottom;
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
           // MessageList.ScrollTo(_chatViewModel.Messages.Last(), null, ScrollToPosition.End, true);
        }
    }
}