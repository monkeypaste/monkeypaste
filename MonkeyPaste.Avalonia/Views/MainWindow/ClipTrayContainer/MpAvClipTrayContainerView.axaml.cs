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

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void Gs_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            Dispatcher.UIThread.Post(async () => {
                await Task.Delay(300);
                BindingContext.RefreshLayout();
            });
        }


        private void Gs_DragDelta(object sender, global::Avalonia.Input.VectorEventArgs e) {
            BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow = true;
        }

    }
}
