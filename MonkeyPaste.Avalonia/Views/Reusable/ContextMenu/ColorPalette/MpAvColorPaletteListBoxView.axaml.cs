using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Linq;
using Xamarin.Forms.Internals;

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
            //e.Containers
            //    .Select(x => x.ContainerControl)
            //    .Where(x => x != null && x.GetVisualAncestor<ListBoxItem>() != null)
            //    .Select(x => x.GetVisualAncestor<ListBoxItem>())
            //    .ForEach(x => x.AddHandler(PointerPressedEvent, ColorButton_PointerPressed, RoutingStrategies.Tunnel));
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

        private void ColorButton_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (sender is Control control && control.DataContext is MpMenuItemViewModel mivm) {
                e.Handled = mivm.IsCustomColorButton;
                mivm.Command?.Execute(mivm.CommandParameter);
            }
        }
    }
}
