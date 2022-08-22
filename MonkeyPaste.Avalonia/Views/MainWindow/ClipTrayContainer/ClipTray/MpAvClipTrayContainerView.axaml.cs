using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTrayContainerView : MpAvUserControl<MpAvClipTrayViewModel> {

        public MpAvClipTrayContainerView() {
            InitializeComponent();
            var gs = this.FindControl<GridSplitter>("ClipTraySplitter");
            gs.GetObservable(GridSplitter.IsEnabledProperty).Subscribe(value => GridSplitter_IsEnabledChanged(gs, value));
            gs.PointerPressed += Gs_PointerPressed;
            gs.DragDelta += Gs_DragDelta;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void GridSplitter_IsEnabledChanged(GridSplitter gs, bool isEnabled) {
            if (!gs.IsEnabled) {
                var ctrcg = this.FindControl<Grid>("ClipTrayContainerGrid");
                ctrcg.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
            }
        }

        private void Gs_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            var gs = sender as GridSplitter;
            if (gs.IsEnabled) {
                var ctrcg = this.FindControl<Grid>("ClipTrayContainerGrid");
                //pin tray has items
                GridLength pinColWidth = ctrcg.ColumnDefinitions[0].Width;
                if (pinColWidth.IsAuto) {
                    //is default, collapsed so pop it out to show one item
                    pinColWidth = new GridLength(BindingContext.DefaultItemWidth, GridUnitType.Pixel);
                } else {
                    pinColWidth = new GridLength(gs.Bounds.Width, GridUnitType.Pixel);
                }
                ctrcg.ColumnDefinitions[0].Width = pinColWidth;
            }
        }


        private void Gs_DragDelta(object sender, global::Avalonia.Input.VectorEventArgs e) {
            BindingContext.HasUserAlteredPinTrayWidth = true;
            var ptr = this.FindControl<MpAvPinTrayView>("PinTrayView");

            var ptrlb = ptr.FindControl<ListBox>("PinTrayListBox");
            //BindingContext.PinTrayTotalWidth = ptrlb.GetVisualDescendant<ScrollViewer>().Extent.Width;
            //BindingContext.OnPropertyChanged(nameof(BindingContext.ClipTrayScreenWidth));
        }
    }
}
