using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvColorPaletteListBoxView : MpAvUserControl<MpMenuItemViewModel> {
        //Type IStyleable.StyleKey => typeof(MenuItem);
        public MpAvColorPaletteListBoxView() {
            InitializeComponent();
            var cplb = this.FindControl<ListBox>("ColorPaletteListBox");
            this.DataContextChanged += MpAvColorPaletteListBoxView_DataContextChanged;
            cplb.DataContextChanged += Cplb_DataContextChanged;
            cplb.ItemContainerGenerator.Materialized += ItemContainerGenerator_Materialized;
        }

        private void ItemContainerGenerator_Materialized(object sender, global::Avalonia.Controls.Generators.ItemContainerEventArgs e) {
            return;
        }

        private void MpAvColorPaletteListBoxView_DataContextChanged(object sender, EventArgs e) {
            if(BindingContext != null) {

            }
        }

        private void Cplb_DataContextChanged(object sender, EventArgs e) {
            if (BindingContext != null) {

            }
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
