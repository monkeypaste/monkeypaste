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
        #region Drag

        private void MpAvClipTileView_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (BindingContext.IsHitTestEnabled) {
                // let webview handle when hit testable (ie subselection is enabled)
                return;
            }
            this.DragCheckAndStart(e, StartDragAsync, EndDragAsync, true);
        }

        private void StartDragAsync(PointerPressedEventArgs e) {
            Dispatcher.UIThread.Post(async () => {
                MpAvIContentView cv = BindingContext.GetContentView();
                cv.SelectAll();
                BindingContext.IsTileDragging = true;

                IDataObject avmpdo = await BindingContext.ConvertToDataObject(false);
                var result = await DragDrop.DoDragDrop(e, avmpdo, DragDropEffects.Copy | DragDropEffects.Move);
            });

        }
        private void EndDragAsync() {
            MpAvIContentView cv = BindingContext.GetContentView();
            cv.DeselectAll();

            BindingContext.IsTileDragging = false;
            MpAvMainWindowViewModel.Instance.IsDropOverMainWindow = false;
        }

        #endregion

        public MpAvClipTileView() {
            InitializeComponent();
            this.PointerPressed += MpAvClipTileView_PointerPressed;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
