using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Media;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileView() {
            AvaloniaXamlLoader.Load(this);

            InitPinPlaceholder();
        }

        #region Pin Placeholder
        private void InitPinPlaceholder() {
            this.FindControl<Control>("PinPlaceholderOverlay")
                .GetObservable(IsVisibleProperty)
                .Subscribe(value => OnPinPlaceholderOverlayIsVisibleChanged(value));
        }
        private void OnPinPlaceholderOverlayIsVisibleChanged(bool isVisible) {
            if (MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening ||
                !isVisible) {
                return;
            }

            Dispatcher.UIThread.Post(async () => {
                ListBoxItem pin_placeholder_lbi = await this.GetVisualAncestorAsync<ListBoxItem>();
                if (pin_placeholder_lbi == null) {
                    return;
                }
                BindingContext.OnPropertyChanged(nameof(BindingContext.PinPlaceholderLabel));
                if (string.IsNullOrEmpty(BindingContext.PinPlaceholderLabel)) {

                }
                if (isVisible) {
                    pin_placeholder_lbi.PointerPressed += Pin_placeholder_lbi_PointerPressed;
                    pin_placeholder_lbi.PointerReleased += Pin_placeholder_lbi_PointerReleased;
                } else {
                    pin_placeholder_lbi.PointerPressed -= Pin_placeholder_lbi_PointerPressed;
                    pin_placeholder_lbi.PointerReleased -= Pin_placeholder_lbi_PointerReleased;
                }
            });
        }

        private void Pin_placeholder_lbi_PointerPressed(object sender, PointerPressedEventArgs e) {
            e.Handled = true;
            if (e.ClickCount == 2) {
                // attemp to unpin pin placeholder tile using click location
                MpAvClipTrayViewModel.Instance.UnpinTileCommand.Execute(BindingContext);
            }
        }
        private void Pin_placeholder_lbi_PointerReleased(object sender, PointerReleasedEventArgs e) {
            e.Handled = true;
        }

        #endregion

    }
}
