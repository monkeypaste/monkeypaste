using Avalonia.Controls;
using Avalonia.Input;
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

            InitializeComponent();

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            var advSearchSplitter = this.FindControl<GridSplitter>("AdvancedSearchSplitter");
            advSearchSplitter.DragCompleted += AdvSearchSplitter_DragCompleted;

            var scv = this.FindControl<Control>("SearchDetailView");
            scv.EffectiveViewportChanged += Scv_EffectiveViewportChanged;
        }


        private void Scv_EffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e) {
            //Dispatcher.UIThread.Post(async () => {
            //    // BUG not sure why but when adv query row height changes
            //    // all tiles location goes to 0, maybe a x/y distance property
            //    // thats changing, i really don't know but this waits a second then updates
            //    await Task.Delay(300);

            //    MpAvClipTrayViewModel.Instance.RefreshQueryTrayLayout();
            //});
        }
        private void AdvSearchSplitter_DragCompleted(object sender, VectorEventArgs e) {
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;
            double nh = scicvm.BoundCriteriaListBoxScreenHeight + e.Vector.ToPortablePoint().Y;
            scicvm.BoundCriteriaListBoxScreenHeight = Math.Min(nh, scicvm.MaxSearchCriteriaListBoxHeight);
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
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
                var lbmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
                if (MpAvPagingListBoxExtension.CheckAndDoAutoScrollJump(sv, lb, lbmp)) {
                    // drag is over a tray track and is thumb dragging
                    // until outside track, then busy for load more
                    return;
                }
                var scroll_delta = sv.AutoScroll(
                    lbmp,
                    sv,
                    ref _autoScrollAccumulators,
                    false);

                BindingContext.ScrollOffset += scroll_delta;
            });
        }

        #endregion
    }
}
