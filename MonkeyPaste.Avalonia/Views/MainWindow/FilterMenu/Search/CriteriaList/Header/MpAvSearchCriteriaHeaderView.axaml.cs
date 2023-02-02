using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpSearchDetailView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaHeaderView : MpAvUserControl<MpAvSearchCriteriaItemCollectionViewModel> {
        public MpAvSearchCriteriaHeaderView() {
            InitializeComponent();

            var attc = this.FindControl<Control>("AllTagTileContainer");
            var sttc = this.FindControl<Control>("SelectedTagTileContainer");
            if(attc != null) {
                attc.AddHandler(PointerPressedEvent, Attc_PointerPressed, RoutingStrategies.Tunnel);
            }
            if(sttc != null) {
                sttc.AddHandler(PointerPressedEvent, Attc_PointerPressed, RoutingStrategies.Tunnel);
            }
            
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void Attc_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            e.Handled = true;

            var tag_container = sender as Control;
            if(tag_container == null) {
                return;
            }

            bool is_all_tag = tag_container.Name == "AllTagTileContainer";
            int tag_id_to_select = is_all_tag ? BindingContext.AllTagId : BindingContext.SelectedSearchTagId;
            BindingContext.SelectSearchTagCommand.Execute(tag_id_to_select);
        }
    }
}
