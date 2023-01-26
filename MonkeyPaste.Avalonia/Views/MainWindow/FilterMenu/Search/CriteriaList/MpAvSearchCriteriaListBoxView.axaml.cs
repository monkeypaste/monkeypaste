using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpSearchDetailView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaListBoxView : MpAvUserControl<MpAvSearchCriteriaItemCollectionViewModel> {
        public MpAvSearchCriteriaListBoxView() {
            InitializeComponent();
            var sclb = this.FindControl<ListBox>("SearchCriteriaListBox");
            sclb.PointerWheelChanged += Sclb_PointerWheelChanged;
            sclb.AddHandler(PointerWheelChangedEvent, Sclb_PointerWheelChanged, RoutingStrategies.Tunnel);
        }

        private void Sclb_PointerWheelChanged(object sender, global::Avalonia.Input.PointerWheelEventArgs e) {
            e.Handled = true;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
