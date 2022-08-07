using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvColorPaletteListBoxView : MenuItem , IStyleable {
        Type IStyleable.StyleKey => typeof(MenuItem);
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
