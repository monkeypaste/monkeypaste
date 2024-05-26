using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;

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
            return MpAvClipTrayContainerView.Instance.ClipTrayView.Bounds.Width /
                MpAvClipTrayContainerView.Instance.ClipTrayContainerGrid.Bounds.Width;
        }

    }
}
