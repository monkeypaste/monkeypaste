using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpSearchDetailView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaListBoxView : MpAvUserControl<MpAvSearchCriteriaItemCollectionViewModel> {
        public MpAvSearchCriteriaListBoxView() {
            InitializeComponent();
            var sclb = this.FindControl<ListBox>("SearchCriteriaListBox");
            //sclb.AddHandler(PointerWheelChangedEvent, Sclb_PointerWheelChanged, RoutingStrategies.Tunnel);
        }

        private void Sclb_PointerWheelChanged(object sender, global::Avalonia.Input.PointerWheelEventArgs e) {
            var sclb = this.FindControl<ListBox>("SearchCriteriaListBox");
            var sv = sclb.GetVisualDescendant<ScrollViewer>();
            double dir = e.Delta.Y > 0 ? 1 : -1;
            double amt = 30;
            sv.ScrollByPointDelta(new MpPoint(0, amt * dir));
            e.Handled = true;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
