using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpPopupMessagePage : PopupPage {
        public static void ShowPopupMessage(string message) {
            _ = new MpPopupMessagePage(message);
        }

        public MpPopupMessagePage() : this(string.Empty) { }
        
        public MpPopupMessagePage(string message) : base() {
            InitializeComponent();
            MessageLabel.Text = message;

            Device.BeginInvokeOnMainThread(async () => {
                await PopupNavigation.Instance.PushAsync(this);
            });
        }
        protected override async void OnAppearing() {
            base.OnAppearing();

            await HidePopup();
        }

        private async Task HidePopup() {
            await Task.Delay(4000);

            if (PopupNavigation.Instance.PopupStack.Contains(this))
                await PopupNavigation.Instance.RemovePageAsync(this);
        }
    }
}