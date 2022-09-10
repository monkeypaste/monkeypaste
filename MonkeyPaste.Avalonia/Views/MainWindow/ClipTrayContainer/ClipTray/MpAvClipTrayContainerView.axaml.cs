using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTrayContainerView : MpAvUserControl<MpAvClipTrayViewModel> {

        public MpAvClipTrayContainerView() {
            InitializeComponent();
            var gs = this.FindControl<GridSplitter>("ClipTraySplitter");
            gs.GetObservable(GridSplitter.IsEnabledProperty).Subscribe(value => GridSplitter_IsEnabledChanged(gs, value));
            gs.AddHandler(GridSplitter.PointerPressedEvent, Gs_PointerPressed, RoutingStrategies.Tunnel);
            gs.AddHandler(GridSplitter.PointerReleasedEvent, Gs_PointerReleased, RoutingStrategies.Tunnel);
            gs.DragDelta += Gs_DragDelta;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void GridSplitter_IsEnabledChanged(GridSplitter gs, bool isEnabled) {
            if (!gs.IsEnabled) {
                var ctrcg = this.FindControl<Grid>("ClipTrayContainerGrid");
                if(MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    ctrcg.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
                } else {
                    ctrcg.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Auto);
                }                
            }
        }

        private void Gs_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            var gs = sender as GridSplitter;
            if (gs.IsEnabled) {
                //pin tray has items

                //MpSize pinTraySize = this.FindControl<MpAvPinTrayView>("PinTrayView").Bounds.Size.ToPortableSize();
                var ctrcg = this.FindControl<Grid>("ClipTrayContainerGrid");
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    GridLength pinColWidth = ctrcg.ColumnDefinitions[0].Width;
                    if (pinColWidth.IsAuto) {
                        //is default, collapsed so pop it out to show one item
                        pinColWidth = new GridLength(BindingContext.DefaultItemWidth, GridUnitType.Pixel);
                    } else {
                        //pinColWidth = new GridLength(pinTraySize.Width, GridUnitType.Pixel);
                        pinColWidth = new GridLength(gs.Width, GridUnitType.Pixel);
                    }
                    ctrcg.ColumnDefinitions[0].Width = pinColWidth;
                } else {
                    GridLength pinRowHeight = ctrcg.RowDefinitions[0].Height;
                    if (pinRowHeight.IsAuto) {
                        //is default, collapsed so pop it out to show one item
                        pinRowHeight = new GridLength(BindingContext.DefaultItemHeight, GridUnitType.Pixel);
                    } else {
                        //pinRowHeight = new GridLength(pinTraySize.Height, GridUnitType.Pixel);
                        pinRowHeight = new GridLength(gs.Height, GridUnitType.Pixel);
                    }
                    ctrcg.RowDefinitions[0].Height = pinRowHeight;
                }                
            }
        }

        private void Gs_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            Dispatcher.UIThread.Post(async () => {
                await Task.Delay(300);
                BindingContext.RefreshLayout();
            });
        }


        private void Gs_DragDelta(object sender, global::Avalonia.Input.VectorEventArgs e) {
            BindingContext.HasUserAlteredPinTrayWidth = true;
            var ptr = this.FindControl<MpAvPinTrayView>("PinTrayView");

            var ptrlb = ptr.FindControl<ListBox>("PinTrayListBox");
            if(ptrlb == null) {
                return;
            }
            //var ptrsv = ptrlb.GetVisualDescendant<ScrollViewer>();
            //if(ptrsv == null) {
            //    return;
            //}
            //BindingContext.PinTrayTotalWidth = ptrsv.Extent.Width;
            //BindingContext.OnPropertyChanged(nameof(BindingContext.ClipTrayScreenWidth));
            ptrlb.InvalidateMeasure();
        }
    }
}
