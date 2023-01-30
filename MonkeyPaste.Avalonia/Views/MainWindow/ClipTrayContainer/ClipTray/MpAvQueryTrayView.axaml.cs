using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvQueryTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        #region Private Variables
        private DispatcherTimer _autoScrollTimer;
        private double[] _autoScrollAccumulators;

        private ScrollViewer _sv;
        private ListBox _lb;
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

            _sv = this.FindControl<ScrollViewer>("ClipTrayScrollViewer");            
            _lb = this.FindControl<ListBox>("ClipTrayListBox");
            _lb.GotFocus += Lb_GotFocus;
            var lb_sv = _lb.GetVisualDescendant<ScrollViewer>();
            Dispatcher.UIThread.Post(async () => {
                while(lb_sv == null) {
                    lb_sv = _lb.GetVisualDescendant<ScrollViewer>();

                    if(lb_sv == null) {
                        await Task.Delay(100);
                    } else {
                        lb_sv.EffectiveViewportChanged += Lb_EffectiveViewportChanged;
                        return;
                    }
                }
            });

            var advSearchSplitter = this.FindControl<GridSplitter>("AdvancedSearchSplitter");
            advSearchSplitter.DragCompleted += AdvSearchSplitter_DragCompleted;
        }
        private void AdvSearchSplitter_DragCompleted(object sender, VectorEventArgs e) {
            MpAvSearchCriteriaItemCollectionViewModel.Instance
                .BoundCriteriaListBoxScreenHeight += e.Vector.ToPortablePoint().Y;
        }

        private void Lb_GotFocus(object sender, global::Avalonia.Input.GotFocusEventArgs e) {
            if (BindingContext.IsTrayEmpty) {
                return;
            }
            if (e.NavigationMethod == NavigationMethod.Tab) {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
                    // shift tab from clip tray select last
                    BindingContext.SelectedItem = BindingContext.Items.Last();
                } else {
                    BindingContext.SelectedItem = BindingContext.Items.First();
                }
            }
        }

        private void Lb_EffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e) {
            // NOTE don't delete below its related to orientation/layout change logic where grid loadMore locks up in vertical mode 
            if(sender is Control control && control.GetVisualDescendant<Canvas>() is Canvas items_panel_canvas) {
                double max_diff = 1.0;
                double w_diff = Math.Abs(items_panel_canvas.Bounds.Width - BindingContext.QueryTrayTotalWidth);
                double h_diff = Math.Abs(items_panel_canvas.Bounds.Height - BindingContext.QueryTrayTotalHeight);
                if (w_diff <= max_diff && h_diff <= max_diff) {
                    // NOTE the lb w/h is bound to total dimensions but only reports screen dimensions
                    // the items panel is CLOSE to actual total dimensions but off by some kind of pixel snapping small value

                    // this is intended to retain scroll position when orientation changes
                    // and this event is triggered twice,
                    // first when the container changes (which the if is trying is set to ignore)
                    // second when the list is transformed                    
                    // the *Begin() is called right before mw rect is changed in mwvm.CycleOrientation

                    //BindingContext.DropScrollAnchor();
                }
            }
            
            return;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void MpAvClipTrayView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            if (Design.IsDesignMode) {
                return;
            }
            //simulate change to initialize layout
            ReceivedGlobalMessage(MpMessageType.TrayLayoutChanged);
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
            if(_autoScrollTimer == null) {
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
            if(!MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                // wait till end of dnd to stop timer
                // otherwise it goes on/off a lot
                BindingContext.NotifyDragOverTrays(false);
                return;
            }
            if(BindingContext.IsBusy) {
                return;
            }
            Dispatcher.UIThread.Post(() => {
                var lbmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
                if(MpAvPagingListBoxExtension.CheckAndDoAutoScrollJump(_sv,_lb,lbmp)) {
                    // drag is over a tray track and is thumb dragging
                    // until outside track, then busy for load more
                    return;
                }
                var scroll_delta = _sv.AutoScroll(
                    lbmp,
                    _sv,
                    ref _autoScrollAccumulators,
                    false);

                BindingContext.ScrollOffset += scroll_delta;
            });            
        }

        #endregion
    }
}
