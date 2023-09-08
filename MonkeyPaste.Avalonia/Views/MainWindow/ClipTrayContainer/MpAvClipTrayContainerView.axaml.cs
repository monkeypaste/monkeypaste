using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTrayContainerView : MpAvUserControl<MpAvClipTrayViewModel> {
        public static MpAvClipTrayContainerView Instance { get; private set; }

        public MpAvClipTrayContainerView() {
            if (Instance != null) {
                // ensure singleton
                MpDebug.Break();
                return;
            }
            Instance = this;

            InitializeComponent();

            if (BindingContext == null) {
                this.DataContextChanged += MpAvClipTrayContainerView_DataContextChanged;
            } else {
                MpAvClipTrayContainerView_DataContextChanged(null, null);
            }

            var gs = this.FindControl<GridSplitter>("ClipTraySplitter");
            gs.AddHandler(Control.PointerPressedEvent, Gs_PointerPressed, RoutingStrategies.Tunnel);
            gs.AddHandler(Control.PointerReleasedEvent, Gs_PointerReleased, RoutingStrategies.Tunnel);
        }

        private void Gs_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            MpMessenger.SendGlobal(MpMessageType.PinTrayResizeEnd);
        }

        private void Gs_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            MpMessenger.SendGlobal(MpMessageType.PinTrayResizeBegin);
        }

        public void UpdatePinTrayVarDimension(GridLength gl) {
            var ctrcv_container_grid = this.FindControl<Grid>("ClipTrayContainerGrid");

            if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                if (ctrcv_container_grid.ColumnDefinitions.Count == 0) {
                    return;
                }
                ctrcv_container_grid.ColumnDefinitions[0].Width = gl;
            } else {
                if (ctrcv_container_grid.RowDefinitions.Count == 0) {
                    return;
                }
                ctrcv_container_grid.RowDefinitions[0].Height = gl;
            }
        }

        private void MpAvClipTrayContainerView_DataContextChanged(object sender, EventArgs e) {
            if (BindingContext == null) {
                return;
            }
            //BindingContext.PropertyChanged += BindingContext_PropertyChanged;
            BindingContext.OnScrollIntoPinTrayViewRequest += BindingContext_OnScrollIntoPinTrayViewRequest;
        }

        private void BindingContext_OnScrollIntoPinTrayViewRequest(object sender, object e) {
            var ctvm = e as MpAvClipTileViewModel;
            if (ctvm == null) {
                return;
            }
            if (ctvm.IsPinned) {
                var ptr_lb = this.GetVisualDescendant<MpAvPinTrayView>().GetVisualDescendant<ListBox>();
                int ctvm_pin_idx = BindingContext.PinnedItems.IndexOf(ctvm);
                var ptr_ctvm_lbi = ptr_lb.ContainerFromIndex(ctvm_pin_idx);
                ptr_ctvm_lbi?.BringIntoView();
                return;
            }

            //var ctr_lb = this.GetVisualDescendant<MpAvPinTrayView>().GetVisualDescendant<ListBox>();
            //var ctr_ctvm_lbi = ctr_lb.ItemContainerGenerator.ContainerFromIndex(ctvm.ItemIdx);
            //ctr_ctvm_lbi?.BringIntoView();
            return;
        }
    }
}
