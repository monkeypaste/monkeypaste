using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvQueryTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        #region Private Variables
        private DispatcherTimer _autoScrollTimer;
        private double[] _autoScrollAccumulators;
        #endregion

        #region Statics

        public static MpAvQueryTrayView Instance { get; private set; }

        #endregion

        public MpAvQueryTrayView() {
            if (Instance != null) {
                // ensure singleton
                MpDebug.Break();
                return;
            }
            Instance = this;

            InitializeComponent();
            this.AttachedToVisualTree += MpAvQueryTrayView_AttachedToVisualTree;


            if (this.FindControl<ScrollViewer>("QueryRepeaterScrollViewer") is ScrollViewer sv) {
                sv.AddHandler(PointerWheelChangedEvent, QueryRepeaterScrollViewer_PointerWheelChanged, RoutingStrategies.Tunnel);
            }

        }

        private void QueryRepeaterScrollViewer_PointerWheelChanged(object sender, PointerWheelEventArgs e) {
            if (sender is not ScrollViewer sv //||
                                              //!BindingContext.CanScroll
                ) {
                return;
            }
            e.Handled = true;
            double multiplier = 120;
            var s = BindingContext.ScrollVector.ToPortablePoint();
            if (BindingContext.LayoutType == MpClipTrayLayoutType.Stack) {
                if (BindingContext.ListOrientation == Orientation.Horizontal) {
                    s.X += e.Delta.Y * multiplier;
                } else {
                    s.Y += e.Delta.Y * multiplier;
                }
            }
            BindingContext.ScrollVector = s.ToAvVector();
        }

        private void MpAvQueryTrayView_AttachedToVisualTree(object sender, global::Avalonia.VisualTreeAttachmentEventArgs e) {
            InitDnd();
        }

        #region Drop

        private void InitDnd() {
            if (this.FindControl<ListBox>("QueryTrayListBox") is not ListBox ctrlb) {
                return;
            }

            DragDrop.SetAllowDrop(ctrlb, true);
            ctrlb.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            ctrlb.AddHandler(DragDrop.DragOverEvent, DragOver);
            ctrlb.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            ctrlb.AddHandler(DragDrop.DropEvent, Drop);
            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
        }

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragEnter] PinTrayListBox: ");
            Dispatcher.UIThread.Post(() => {
                BindingContext.IsDragOverQueryTray = true;
            });
            StartAutoScroll();
        }

        private void DragOver(object sender, DragEventArgs e) {
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, is_copy);
            e.DragEffects = GetDndEffects(is_drop_valid, is_copy);
        }
        private void DragLeave(object sender, DragEventArgs e) {
            // MpConsole.WriteLine("[DragLeave] PinTrayListBox: ");
            ResetDrop();
        }

        private async void Drop(object sender, DragEventArgs e) {
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, is_copy);
            // MpConsole.WriteLine("[Drop] PinTrayListBox DropIdx: " + drop_idx + " IsCopy: " + is_copy + " IsValid: " + is_drop_valid);

            e.DragEffects = GetDndEffects(is_drop_valid, is_copy);
            if (!is_drop_valid) {
                ResetDrop();
                return;
            }

            var cur_ttvm = MpAvTagTrayViewModel.Instance.LastSelectedActiveItem;
            // NOTE need to use processed/output data object, avdo becomes disposed
            var mpdo = await
                Mp.Services.DataObjectTools
                .ReadDataObjectAsync(e.Data, MpDataObjectSourceType.QueryTrayDrop) as MpAvDataObject;

            Dispatcher.UIThread.Post(async () => {
                var drop_ci = await Mp.Services.ContentBuilder.BuildFromDataObjectAsync(mpdo, is_copy);
                if (drop_ci == null || drop_ci.Id == 0) {
                    ResetDrop();
                    return;
                }

                cur_ttvm.LinkCopyItemCommand.Execute(drop_ci.Id);
                if (cur_ttvm.IsSelected) {
                    // when selected do in-place requery
                    while (!BindingContext.QueryCommand.CanExecute(string.Empty)) {
                        await Task.Delay(100);
                    }
                    BindingContext.QueryCommand.CanExecute(string.Empty);
                }

                ResetDrop();
            });
        }

        #endregion

        #region Drop Helpers (supposed to link drop to tag but disabled cause bugz)

        private DragDropEffects GetDndEffects(bool is_drop_valid, bool is_copy) {
            DragDropEffects dde = DragDropEffects.None;

            if (!is_drop_valid) {
                return dde;
            }
            dde |= DragDropEffects.Link;
            if (is_copy) {
                dde |= DragDropEffects.Copy;
            }

            return dde;
        }
        private bool IsDropValid(IDataObject avdo, bool is_copy) {
            if (avdo.ContainsSearchCriteria()) {
                return false;
            }
            if (avdo.ContainsTagItem()) {
                return false;
            }
            var cur_ttvm = MpAvTagTrayViewModel.Instance.LastSelectedActiveItem;
            if (cur_ttvm == null || !cur_ttvm.CanLinkContent) {
                // only allow drop onto link tags
                return false;
            }
            if (is_copy) {
                return true;
            }
            if (avdo.TryGetSourceRefIdBySourceType(MpTransactionSourceType.CopyItem, out int ciid)) {
                bool is_already_linked = cur_ttvm.IsCopyItemLinked(ciid);
                if (is_already_linked) {
                    // invalidate tile drag if tag is already linked to copy item and its not a copy operation
                    return false;
                }
            }
            return true;
        }

        private void ResetDrop() {
            BindingContext.IsDragOverQueryTray = false;
            StopAutoScroll();
        }
        #endregion

        #endregion


        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.MainWindowOrientationChangeBegin:
                case MpMessageType.PreTrayLayoutChange:
                    //this.InvalidateMeasure();
                    break;
                case MpMessageType.DropOverTraysBegin:
                    StartAutoScroll();
                    break;
                case MpMessageType.DropOverTraysEnd:
                    StopAutoScroll();
                    break;
            }
        }

        #region Pin Placeholder
        public void Pin_placeholder_lbi_PointerPressed(object sender, PointerPressedEventArgs e) {
            e.Handled = true;
            if (e.ClickCount == 2) {
                // attemp to unpin pin placeholder tile using click location
                MpAvClipTrayViewModel.Instance.UnpinTileCommand.Execute(BindingContext);
            }
        }
        public void Pin_placeholder_lbi_PointerReleased(object sender, PointerReleasedEventArgs e) {
            e.Handled = true;
        }

        #endregion

        #region Auto Scroll

        private void StartAutoScroll() {
            if (_autoScrollTimer == null) {
                _autoScrollTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
                _autoScrollTimer.Tick += _autoScrollTimer_Tick;
            }
            _autoScrollTimer.Start();
        }

        private void StopAutoScroll() {
            if (_autoScrollTimer == null) {
                return;
            }
            _autoScrollTimer.Stop();
            _autoScrollAccumulators.ForEach(x => x = 0);

        }

        private void _autoScrollTimer_Tick(object sender, EventArgs e) {
            if (!MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                // wait till end of dnd to stop timer
                // otherwise it goes on/off a lot
                BindingContext.NotifyDragOverTrays(false);
                return;
            }
            if (BindingContext.IsBusy) {
                return;
            }
            Dispatcher.UIThread.Post(() => {
                var sv = this.FindControl<ScrollViewer>("ClipTrayScrollViewer");
                var lb = this.FindControl<ListBox>("QueryTrayListBox");
                var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalScaledMouseLocation;
                if (MpAvPagingListBoxExtension.CheckAndDoAutoScrollJump(sv, lb, gmp)) {
                    // drag is over a tray track and is thumb dragging
                    // until outside track, then busy for load more
                    return;
                }
                var scroll_delta = sv.AutoScroll(
                    gmp,
                    sv,
                    ref _autoScrollAccumulators,
                    false);

                BindingContext.ScrollOffset += scroll_delta;
            });
        }

        #endregion
    }
}
