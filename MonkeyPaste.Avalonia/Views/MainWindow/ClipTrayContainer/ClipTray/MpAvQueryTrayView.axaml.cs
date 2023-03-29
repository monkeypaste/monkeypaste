using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;

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
                Debugger.Break();
                return;
            }
            Instance = this;

            AvaloniaXamlLoader.Load(this);

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            //var advSearchSplitter = this.FindControl<GridSplitter>("AdvancedSearchSplitter");
            //advSearchSplitter.DragDelta += AdvSearchSplitter_DragDelta;
            //advSearchSplitter.AddHandler(GridSplitter.DragDeltaEvent, AdvSearchSplitter_DragDelta, RoutingStrategies.Tunnel);
        }

        private void AdvSearchSplitter_DragDelta(object sender, VectorEventArgs e) {
            var gs = sender as GridSplitter;
            var pg = gs.Parent as Grid;
            var pg_r0_def = pg.RowDefinitions[0];
            var sc_sv = this.FindControl<MpAvSearchCriteriaListBoxView>("SearchCriteriaView").FindControl<ScrollViewer>("SearchCriteriaContainerScrollViewer");
            if (pg_r0_def.ActualHeight >= sc_sv.Extent.Height) {
                e.Handled = true;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.MainWindowOrientationChangeBegin:
                case MpMessageType.TrayLayoutChanged:
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
                var lb = this.FindControl<ListBox>("ClipTrayListBox");
                var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
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
