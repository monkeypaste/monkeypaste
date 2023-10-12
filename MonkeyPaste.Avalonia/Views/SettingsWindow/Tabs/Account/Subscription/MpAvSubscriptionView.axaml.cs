
using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSubscriptionView : MpAvUserControl<MpAvSubcriptionPurchaseViewModel> {
        public MpAvSubscriptionView() {
            InitializeComponent();
        }

        private void BuyButton_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not Control buyButton) {
                return;
            }
            buyButton.GetObservable(Control.IsVisibleProperty).Subscribe(value => OnBuyButtonIsVisibleChanged(buyButton));
            OnBuyButtonIsVisibleChanged(buyButton);
        }
        private async void OnBuyButtonIsVisibleChanged(Control buyButton) {
            if (!buyButton.IsVisible ||
                this.GetVisualAncestor<ScrollViewer>() is not ScrollViewer sv) {
                return;
            }
            //Dispatcher.UIThread.Post(async () => {
            //    while(true) {
            //        if(sv.Offset)
            //    }

            // BUG calling bringIntoView doesn't do anything on button, so scrolling to end
            // cause its at the bottom

            await Task.Delay(300);
            sv.ScrollToEnd();
            //});
        }
    }
}
