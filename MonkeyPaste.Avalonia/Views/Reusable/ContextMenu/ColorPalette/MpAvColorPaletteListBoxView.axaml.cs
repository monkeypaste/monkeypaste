using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvColorPaletteListBoxView : MpAvUserControl<MpMenuItemViewModel> {
        public MpAvColorPaletteListBoxView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void ColorButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is Control control && control.DataContext is MpMenuItemViewModel mivm) {
                if (!mivm.IsCustomColorButton) {
                    MpPlatformWrapper.Services.ContextMenuCloser.CloseMenu();
                }
            }
        }
    }
}
