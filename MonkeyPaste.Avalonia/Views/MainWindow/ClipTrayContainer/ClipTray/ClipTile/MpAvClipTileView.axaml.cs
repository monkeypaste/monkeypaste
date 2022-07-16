using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;
using WebViewControl;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvClipTileView_AttachedToVisualTree;
            this.PointerPressed += MpAvClipTileView_PointerPressed;
            this.DataContextChanged += MpAvClipTileView_DataContextChanged;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            var dbgButton = this.FindControl<Button>("DebugButton");
            dbgButton.Click += DbgButton_Click;
        }

        private void DbgButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            var webView = this.FindControl<WebView>("WebView");
            webView.ShowDeveloperTools();
            
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TrayLayoutChanged:
                    break;
            }
        }
        private void MpAvClipTileView_DataContextChanged(object sender, System.EventArgs e) {
            //InvalidateVisual();
            //if (e.OldValue != null && e.OldValue is MpClipTileViewModel octvm) {
            //    octvm.OnUiUpdateRequest -= Rtbcvm_OnUiUpdateRequest;
            //    octvm.OnScrollToHomeRequest -= Rtbcvm_OnScrollToHomeRequest;
            //    octvm.PropertyChanged -= Rtbcvm_PropertyChanged;
            //}
            //if (e.NewValue != null && e.NewValue is MpClipTileViewModel nctvm) {
            //    nctvm.OnUiUpdateRequest += Rtbcvm_OnUiUpdateRequest;
            //    nctvm.OnScrollToHomeRequest += Rtbcvm_OnScrollToHomeRequest;
            //}
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
            }
        }

        private void MpAvClipTileView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
                //if(BindingContext is MpISelectableViewModel svm) {
                //    svm.IsSelected = true;
                //}
                if (BindingContext is MpISelectorItemViewModel<MpAvClipTileViewModel> sivm) {
                    sivm.Selector.SelectedItem = BindingContext;
                }
            }
        }

        private void MpAvClipTileView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayLayoutChanged);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
