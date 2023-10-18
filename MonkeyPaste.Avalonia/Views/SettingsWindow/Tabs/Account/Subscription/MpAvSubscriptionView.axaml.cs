
using Avalonia;
using Avalonia.Controls;
using System;

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
            //if (!buyButton.IsVisible ||
            //    this.GetVisualAncestor<ScrollViewer>() is not ScrollViewer sv) {
            //    return;
            //}
            //await Task.Delay(300);
            //sv.ScrollToEnd();
        }
    }
}
