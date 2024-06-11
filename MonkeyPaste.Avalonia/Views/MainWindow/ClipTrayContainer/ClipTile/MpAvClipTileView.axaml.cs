using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileView() {
            InitializeComponent();

            this.AddHandler(
                    InputElement.PointerWheelChangedEvent,
                    MpAvClipTileView_OnPointerWheelChanged,
                    RoutingStrategies.Tunnel);
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            //InitDnd();
        }
        private void InitDnd() {
            if (!MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                // handled in attached props
                return;
            }
            DragDrop.SetAllowDrop(ClipTileContainerBorder, true);
            //this.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            //this.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            this.AddHandler(DragDrop.DragOverEvent, DragOver);
            this.AddHandler(DragDrop.DropEvent, Drop);

            //this.AddHandler(Control.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        }

        #region Zoom
        private void MpAvClipTileView_OnPointerWheelChanged(object sender, PointerWheelEventArgs e) {
            if (e.KeyModifiers != KeyModifiers.Control) {
                return;
            }
            e.Handled = true;
            bool is_increase = e.Delta.X > 0 || e.Delta.Y > 0;
            if (is_increase) {
                BindingContext.ZoomInCommand.Execute(null);
            } else {
                BindingContext.ZoomOutCommand.Execute(null);
            }
        }
        #endregion



        #region Dnd

        #region Drag Events
        private void OnPointerPressed(object sender, PointerPressedEventArgs e) {
            if (BindingContext == null ||
                !BindingContext.IsEditorLoaded ||
                this.GetVisualDescendant<MpAvContentWebView>() is not { } wv) {
                return;
            }
            e.Handled = true;


            Dispatcher.UIThread.Post(async () => {
                RelayDndMsg("dragstart", e);
                wv.LastPointerPressedEventArgs = e;
                DragDropEffects result = await MpAvContentWebViewDragHelper.StartDragAsync(wv, DragDropEffects.Copy | DragDropEffects.Move);
                RelayDndMsg("dragend", e);
            });

        }
        #endregion

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            RelayDndMsg("dragenter", e);
        }

        private bool _isOver = false;
        private void DragOver(object sender, DragEventArgs e) {
            if (BindingContext == null ||
               !BindingContext.IsEditorLoaded ||
               this.GetVisualDescendant<MpAvContentWebView>() is not { } wv) {
                return;
            }
            string evt = null;
            bool is_over = wv.Bounds.Contains(e.GetPosition(wv));

            if (is_over) {
                if (_isOver) {
                    evt = "dragover";
                } else {
                    _isOver = true;
                    evt = "dragenter";
                }
            } else if (_isOver) {
                _isOver = false;
                evt = "dragleave";
            }
            if (evt == null) {
                return;
            }
            RelayDndMsg(evt, e);
            e.DragEffects = DragDropEffects.Copy;
        }
        private void DragLeave(object sender, DragEventArgs e) {
            //RelayDndMsg("dragleave", e);
        }

        private void Drop(object sender, DragEventArgs e) {
            if (!_isOver) {
                return;
            }
            RelayDndMsg("drop", e);
            _isOver = false;
        }
        #endregion

        private void RelayDndMsg(string eventType, RoutedEventArgs e) {
            string frag = string.Empty;
            Point mp = new Point();

            if (BindingContext == null ||
                !BindingContext.IsEditorLoaded ||
                this.GetVisualDescendant<MpAvContentWebView>() is not { } wv) {
                return;
            }

            if (e is DragEventArgs dea) {
                //e.Handled = true;
                frag = dea.Data.ToQuillDataItemsMessage(dde: DragDropEffects.Copy).SerializeObjectToBase64();
                mp = dea.GetPosition(wv);
            } else if (e is PointerEventArgs pea) {
                mp = pea.GetPosition(wv);
            }
            var dndMsg = new MpQuillDragDropEventMessage() {

                eventType = eventType,
                dataItemsFragment = frag,
                screenX = mp.X,
                screenY = mp.Y,
                shiftKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown,
                ctrlKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                altKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown,
                metaKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsMetaDown,
                escKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown,
            };
            wv.SendMessage($"dragEventFromHost_ext('{dndMsg.SerializeObjectToBase64()}')");
        }
        #endregion

    }
}
