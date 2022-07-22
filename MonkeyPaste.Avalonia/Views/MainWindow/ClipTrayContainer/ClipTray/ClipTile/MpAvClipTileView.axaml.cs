using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        //public MpAvContentView ContentView { get; private set; }
        //private WebView wv;

        public MpAvClipTileView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvClipTileView_AttachedToVisualTree;
            this.PointerPressed += MpAvClipTileView_PointerPressed;
            this.DataContextChanged += MpAvClipTileView_DataContextChanged;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }




        private void CtvBorder_PointerMoved(object sender, PointerEventArgs e) {
            if(BindingContext == null) {
                return;
            }
            e.Handled = BindingContext.IsContentReadOnly || BindingContext.IsSubSelectionEnabled;
        }

        private void CtvBorder_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            if(BindingContext is MpISelectorItemViewModel<MpAvClipTileViewModel> sivm) {
                sivm.Selector.SelectedItem = BindingContext;
            }
            e.Handled = BindingContext.IsContentReadOnly || BindingContext.IsSubSelectionEnabled;
        }

        private void MpAvClipTileView_DataContextChanged(object sender, System.EventArgs e) {
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                //case nameof(BindingContext.CopyItemData):
                //    ContentView.Default.ContentData = BindingContext.CopyItemData;
                //    break;
            }
        }

        private void MpAvClipTileView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
                if (BindingContext is MpISelectorItemViewModel<MpAvClipTileViewModel> sivm) {
                    sivm.Selector.SelectedItem = BindingContext;
                }
            }
        }

        private void MpAvClipTileView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayLayoutChanged);
        }


        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TrayLayoutChanged:
                    break;
            }
        }
    }
}
