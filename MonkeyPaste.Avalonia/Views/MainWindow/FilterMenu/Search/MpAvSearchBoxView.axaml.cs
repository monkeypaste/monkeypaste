using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSearchBoxView : MpAvUserControl<MpAvSearchBoxViewModel> {
        public MpAvSearchBoxView() {
            AvaloniaXamlLoader.Load(this);
        }

        protected override async void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);
            if (BindingContext == null) {
                return;
            }
            await Task.Delay(500);
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsExpanded));
            this?.InvalidateAll();
        }

        private void SearchBox_KeyUp(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (sender is Control control &&
                    control.GetVisualDescendant<TextBox>() is TextBox tb) {
                    tb.SelectAll();
                }
                e.Handled = true;
                BindingContext.PerformSearchCommand.Execute(null);

            }
        }

    }
}
