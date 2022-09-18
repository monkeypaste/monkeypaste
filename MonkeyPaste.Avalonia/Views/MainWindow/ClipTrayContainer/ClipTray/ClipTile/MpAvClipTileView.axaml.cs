using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Avalonia.Utils.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        #region Drag

        private void MpAvClipTileView_PointerPressed(object sender, PointerPressedEventArgs e) {
            //if (BindingContext.IsHitTestEnabled) {
            //    // let webview handle when hit testable (ie subselection is enabled)
            //    return;
            //}
            this.DragCheckAndStart(e, StartDrag, ContinueDrag, EndDrag);
        }

        private void StartDrag(PointerPressedEventArgs e) {
            MpAvIContentView cv = BindingContext.GetContentView();
            cv.SelectAll();

            // add temp key down listener for notifying editor for visual feedback
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += Global_DragKeyUpOrDown;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += Global_DragKeyUpOrDown;

            MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();
            MpAvMainWindowViewModel.Instance.IsDropOverMainWindow = true;
            BindingContext.IsTileDragging = true;

            if(cv is MpAvCefNetWebView wv) {
                // notify editor that its dragging and not just in a drop state
                var dragStartMsg = new MpQuillIsDraggingNotification() { isDragging = true };
                wv.ExecuteJavascript($"updateIsDraggingFromHost_ext('{dragStartMsg.SerializeJsonObjectToBase64()}')");
            }

            Dispatcher.UIThread.Post(async () => {
                var avmpdo = await BindingContext.ConvertToDataObject(false);
                // for tile drag all is always selected so add tile to formats
                avmpdo.SetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT, BindingContext);
                var result = await DragDrop.DoDragDrop(e, avmpdo, DragDropEffects.Copy | DragDropEffects.Move);
                EndDrag();
            });

        }


        private void ContinueDrag(PointerEventArgs e) {
            MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();

        }
        private void EndDrag() {
            if(BindingContext.IsTileDragging == false) {
                // can be called twice when esc-canceled (first from StartDrag handler then from the checker pointer up so ignore 2nd
                return;
            }
            MpAvIContentView cv = BindingContext.GetContentView();
            cv.DeselectAll();
            if (cv is MpAvCefNetWebView wv) {
                // notify editor that its dragging and not just in a drop state
                var dragEndMsg = new MpQuillIsDraggingNotification() { isDragging = false };
                wv.ExecuteJavascript($"updateIsDraggingFromHost_ext('{dragEndMsg.SerializeJsonObjectToBase64()}')");
            }
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= Global_DragKeyUpOrDown;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= Global_DragKeyUpOrDown;

            MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = null;
            MpAvMainWindowViewModel.Instance.IsDropOverMainWindow = false;
            BindingContext.IsTileDragging = false;
        }

        private void Global_DragKeyUpOrDown(object sender, string e) {
            if(BindingContext.GetContentView() is MpAvCefNetWebView wv) {
                var modKeyMsg = new MpQuillModifierKeysNotification() {
                    ctrlKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                    altKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown,
                    shiftKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown,
                    escKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown
                };
                wv.ExecuteJavascript($"updateModifierKeysFromHost_ext('{modKeyMsg.SerializeJsonObjectToBase64()}')");
            }            
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
