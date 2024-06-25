using Avalonia.Controls;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;

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

            ClipTraySplitter.DragStarted += (s, e) => MpMessenger.SendGlobal(MpMessageType.PinTrayResizeBegin);
            ClipTraySplitter.DragCompleted += (s, e) => MpMessenger.SendGlobal(MpMessageType.PinTrayResizeEnd);
            ClipTraySplitter.DragDelta += (s, e) => MpMessenger.SendGlobal(MpMessageType.PinTraySizeChanged);

            //PinTrayView.PinTrayEmptyContainer.Loaded += (s, e) => SetupEmptyMsgFix(PinTrayView.PinTrayEmptyContainer);
            //QueryTrayView.QueryTrayEmptyContainer.Loaded += (s, e) => SetupEmptyMsgFix(QueryTrayView.QueryTrayEmptyContainer);
        }


        private void SetupEmptyMsgFix(Control empty_cntr) {
            if (empty_cntr.GetVisualAncestors().OfType<Control>() is not { } empty_cntr_al) {
                return;
            }
            empty_cntr_al.ForEach(x => x.EffectiveViewportChanged += (s, e) => {
                empty_cntr.InvalidateAll();
            });
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
                int ctvm_pin_idx = BindingContext.PinnedItems.IndexOf(ctvm);
                var ptr_ctvm_lbi = PinTrayView.PinTrayListBox.ContainerFromIndex(ctvm_pin_idx);
                ptr_ctvm_lbi?.BringIntoView();
                return;
            }

            //var ctr_lb = this.GetVisualDescendant<MpAvPinTrayView>().GetVisualDescendant<ListBox>();
            //var ctr_ctvm_lbi = ctr_lb.ItemContainerGenerator.ContainerFromIndex(ctvm.ItemIdx);
            //ctr_ctvm_lbi?.BringIntoView();
            return;
        }

        public double GetQueryTrayRatio() {
            return MpAvClipTrayContainerView.Instance.QueryTrayView.Bounds.Width /
                MpAvClipTrayContainerView.Instance.ClipTrayContainerGrid.Bounds.Width;
        }

    }
}
