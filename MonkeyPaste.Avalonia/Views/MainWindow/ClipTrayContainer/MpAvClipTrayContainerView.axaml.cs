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
using Avalonia.Input;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTrayContainerView : MpAvUserControl<MpAvClipTrayViewModel> {
        public static MpAvClipTrayContainerView Instance { get; private set; }

        //private ListBox _pinTrayListBox;
        //public ListBox PinTrayListBox {
        //    get {
        //        if(_pinTrayListBox == null) {
        //            var ptrv = this.GetVisualDescendant<MpAvPinTrayView>();
        //            if(ptrv == null) {
        //                return null;
        //            }
        //            var ptrv_lb = ptrv.GetVisualDescendant<ListBox>();
        //            if(ptrv_lb == null) {
        //                return null;
        //            }
        //            _pinTrayListBox = ptrv_lb;
        //        }
        //        return _pinTrayListBox;
        //    }
        //}

        //private ListBox _clipTrayListBox;
        //public ListBox ClipTrayListBoxRef {
        //    get {
        //        if (_clipTrayListBox == null) {
        //            var ctrv = this.GetVisualDescendant<MpAvClipTrayView>();
        //            if (ctrv == null) {
        //                return null;
        //            }
        //            var ctrv_lb = ctrv.GetVisualDescendant<ListBox>();
        //            if (ctrv_lb == null) {
        //                return null;
        //            }
        //            _clipTrayListBox = ctrv_lb;
        //        }
        //        return _clipTrayListBox;
        //    }
        //}

        public MpAvClipTrayContainerView() {
            if (Instance != null) {
                // ensure singleton
                Debugger.Break();
                return;
            }
            Instance = this;

            InitializeComponent();

            if(BindingContext == null) {
                this.DataContextChanged += MpAvClipTrayContainerView_DataContextChanged;
            } else {
                MpAvClipTrayContainerView_DataContextChanged(null, null);
            }
            
            var gs = this.FindControl<GridSplitter>("ClipTraySplitter");
            //gs.GetObservable(GridSplitter.IsEnabledProperty).Subscribe(value => GridSplitter_IsEnabledChanged(gs, value));
            //gs.AddHandler(GridSplitter.PointerPressedEvent, Gs_PointerPressed, RoutingStrategies.Tunnel);
            //gs.AddHandler(GridSplitter.PointerReleasedEvent, Gs_PointerReleased, RoutingStrategies.Tunnel);

            gs.DragDelta += Gs_DragDelta;
            
        }

        private void MpAvClipTrayContainerView_DataContextChanged(object sender, EventArgs e) {
            if(BindingContext == null) {
                return;
            }
            //BindingContext.PropertyChanged += BindingContext_PropertyChanged;
            BindingContext.OnScrollIntoPinTrayViewRequest += BindingContext_OnScrollIntoPinTrayViewRequest;
        }

        private void BindingContext_OnScrollIntoPinTrayViewRequest(object sender, object e) {
            var ctvm = e as MpAvClipTileViewModel;
            if(ctvm == null) {
                return;
            }
            if(ctvm.IsPinned) {
                var ptr_lb = this.GetVisualDescendant<MpAvPinTrayView>().GetVisualDescendant<ListBox>();
                int ctvm_pin_idx = BindingContext.PinnedItems.IndexOf(ctvm);
                var ptr_ctvm_lbi = ptr_lb.ItemContainerGenerator.ContainerFromIndex(ctvm_pin_idx);
                ptr_ctvm_lbi?.BringIntoView();
                return;
            }
            
            //var ctr_lb = this.GetVisualDescendant<MpAvPinTrayView>().GetVisualDescendant<ListBox>();
            //var ctr_ctvm_lbi = ctr_lb.ItemContainerGenerator.ContainerFromIndex(ctvm.ItemIdx);
            //ctr_ctvm_lbi?.BringIntoView();
            return;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(BindingContext.IsPinTrayVisible)) {
                var ptrv = this.FindControl<MpAvPinTrayView>("PinTrayView");
                if(BindingContext.IsPinTrayVisible) {
                    ptrv.IsVisible = true;
                    Dispatcher.UIThread.Post(async () => {
                        while (ptrv.Width < BindingContext.DefaultPinTrayWidth) {
                            ptrv.Width += 5.0d;
                            await Task.Delay(30);
                        }
                        ptrv.Width = BindingContext.DefaultPinTrayWidth;
                    });
                } else {
                    Dispatcher.UIThread.Post(async () => {
                        while (ptrv.Width > 0) {
                            double nw = Math.Max(0, ptrv.Width - 5.0d);
                            ptrv.Width = nw;
                            await Task.Delay(30);
                        }
                        ptrv.Width = 0;
                        ptrv.IsVisible = false;
                    });
                }
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void GridSplitter_IsEnabledChanged(GridSplitter gs, bool isEnabled) {
            if (!gs.IsEnabled) {
                // this ensures when gs is disabled the pin tray column is hidden inlcuding splitter
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
            BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow = true;
            //var ptr = this.FindControl<MpAvPinTrayView>("PinTrayView");

            //var ptrlb = ptr.FindControl<ListBox>("PinTrayListBox");
            //if(ptrlb == null) {
            //    return;
            //}
            //var ptrsv = ptrlb.GetVisualDescendant<ScrollViewer>();
            //if(ptrsv == null) {
            //    return;
            //}
            //BindingContext.PinTrayTotalWidth = ptrsv.Extent.Width;
            //BindingContext.OnPropertyChanged(nameof(BindingContext.ClipTrayScreenWidth));
            //ptrlb.InvalidateMeasure();
        }

    }
}
