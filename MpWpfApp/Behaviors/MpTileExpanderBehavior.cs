using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTileExpanderBehavior : Behavior<MpClipTileContainerView> {
        private double _unexpandedScrollOfset = 0;
        private double _originalMainWindowTop = 0;
        private double _initialExpandedMainWindowTop = 0;
        private Point _lastMousePosition;
        private FrameworkElement _mainWindowTitlePanel;

        private object _dataContext;

        public static bool IsAnyExpandingOrUnexpanding { get; set; } = false;

        protected override void OnAttached() {
            base.OnAttached();
            if(!AssociatedObject.IsLoaded) {
                AssociatedObject.Loaded += AssociatedObject_Loaded;
                AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            } else {
                OnLoaded();
            }
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            if (AssociatedObject != null) {
                AssociatedObject.Loaded -= AssociatedObject_Loaded;
                AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            }
            MpMainWindowViewModel.Instance.OnMainWindowHide -= MainWindowViewModel_OnMainWindowHide;

            MpMessenger.Instance.Unregister<MpMessageType>(_dataContext, ReceiveClipTileMessage, _dataContext);
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e) {
            Detach();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            OnLoaded();
        }

        private void OnLoaded() {
            _dataContext = AssociatedObject.DataContext;
            MpMessenger.Instance.Register<MpMessageType>(_dataContext, ReceiveClipTileMessage, _dataContext);

            MpMainWindowViewModel.Instance.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;

            _mainWindowTitlePanel = (Application.Current.MainWindow as MpMainWindow).TitleMenu;
        }

        #region Manual Resize Event Handlers

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            if (!MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }

            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e) {
            if (!MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }

            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.ResizeNS;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            _mainWindowTitlePanel.ReleaseMouseCapture();
            MpMainWindowViewModel.Instance.IsResizing = false;

            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (!MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }
            _lastMousePosition = e.GetPosition(Application.Current.MainWindow);
            _mainWindowTitlePanel.CaptureMouse();
            MpMainWindowViewModel.Instance.IsResizing = true;
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (!_mainWindowTitlePanel.IsMouseCaptured) {
                e.Handled = false;
                return;
            }

            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.ResizeNS;

            var mp = e.GetPosition(Application.Current.MainWindow);
            double deltaY = mp.Y - _lastMousePosition.Y;
            _lastMousePosition = mp;

            Resize(-deltaY);
            e.Handled = true;
        }

        #endregion

        private void ReceiveClipTileMessage(MpMessageType msg) {
            switch(msg) {

                case MpMessageType.Expand:
                    Expand();
                    _mainWindowTitlePanel.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
                    _mainWindowTitlePanel.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
                    _mainWindowTitlePanel.PreviewMouseMove += AssociatedObject_MouseMove;
                    _mainWindowTitlePanel.MouseEnter += AssociatedObject_MouseEnter;
                    _mainWindowTitlePanel.MouseLeave += AssociatedObject_MouseLeave;                    
                    break;
                case MpMessageType.Unexpand:
                    Unexpand();

                    _mainWindowTitlePanel.PreviewMouseLeftButtonDown -= AssociatedObject_MouseDown;
                    _mainWindowTitlePanel.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
                    _mainWindowTitlePanel.PreviewMouseMove -= AssociatedObject_MouseMove;
                    _mainWindowTitlePanel.MouseEnter -= AssociatedObject_MouseEnter;
                    _mainWindowTitlePanel.MouseLeave -= AssociatedObject_MouseLeave;
                    break;
            }
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            if(ctvm.IsAnyPastingTemplate) {
                return;
            }
            ctvm.IsExpanded = false;
        }

        private void Ctvm_OnUnExpandRequest(object sender, EventArgs e) {
            MpHelpers.Instance.RunOnMainThread(Unexpand);
        }

        private void Ctvm_OnExpandRequest(object sender, EventArgs e) {
            MpHelpers.Instance.RunOnMainThread(Expand);
        }

        public void Resize(double deltaHeight) {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            if (!ctvm.IsExpanded || MpDragDropManager.Instance.IsDragAndDrop) {
                return;
            }

            var mwvm = MpMainWindowViewModel.Instance;
            mwvm.IsResizing = true;

            mwvm.MainWindowTop -= deltaHeight;
            mwvm.ClipTrayHeight += deltaHeight;
            ctvm.TileBorderHeight += deltaHeight;
            ctvm.TileContentHeight += deltaHeight;

            double boundAdjust = 0;
            if (mwvm.MainWindowTop < MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding) {
                boundAdjust = mwvm.MainWindowTop - MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding;
            } else if (mwvm.MainWindowTop > mwvm.MainWindowHeight - MpMeasurements.Instance.MainWindowMinHeight) {
                boundAdjust = mwvm.MainWindowTop - (mwvm.MainWindowHeight - MpMeasurements.Instance.MainWindowMinHeight);
            }

            mwvm.MainWindowTop -= boundAdjust;
            mwvm.ClipTrayHeight += boundAdjust;
            ctvm.TileBorderHeight += boundAdjust;
            ctvm.TileContentHeight += boundAdjust;

            MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayScreenWidth));
            mwvm.OnPropertyChanged(nameof(mwvm.ClipTrayAndCriteriaListHeight));
            mwvm.IsResizing = false;

            Application.Current.MainWindow.GetVisualDescendents<MpUserControl>().ForEach(x => x.UpdateLayout());
        }

        private void Expand() {
            MpConsole.WriteLine("Expanding...");
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            var mwvm = MpMainWindowViewModel.Instance;
            //need to do this so listboxitem matches w/ datacontext or it will expand to another tiles size
            IsAnyExpandingOrUnexpanding = true;
            mwvm.IsResizing = true;

            MpClipTrayViewModel.Instance.Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsPlaceholder)));

            var _deltaSize = new Point();

            _unexpandedScrollOfset = MpClipTrayViewModel.Instance.ScrollOffset;
            MpClipTrayViewModel.Instance.ScrollOffset = MpClipTrayViewModel.Instance.LastScrollOfset = 0;

            _originalMainWindowTop = mwvm.MainWindowTop;

            ctvm.ResetSubSelection(false);
            //if (ctvm.ItemViewModels.Count > 1 && ctvm.SelectedItems.Count == ctvm.ItemViewModels.Count) {
            //    ctvm.ResetSubSelection(false);
            //} else {
            //    ctvm.IsSelected = true;
            //}

            //trigger app mode column to hide
            ctvm.OnPropertyChanged(nameof(ctvm.FlipButtonVisibility));
            ctvm.Parent.OnPropertyChanged(nameof(ctvm.Parent.IsAnyTileExpanded));
            mwvm.OnPropertyChanged(nameof(mwvm.AppModeButtonGridWidth));

            //

            //find max change in y so main window doesn't go past top of screen
            double maxDeltaHeight = SystemParameters.PrimaryScreenHeight - MpMeasurements.Instance.MainWindowDefaultHeight;
            //add padding so user can click away from window
            maxDeltaHeight -= MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding;

            //only make tile larger otherwise leave standard height
            if(ctvm.TotalExpandedContentSize.Height > ctvm.TileContentHeight) {
                double actualHeightDiff = ctvm.TotalExpandedContentSize.Height - ctvm.TileContentHeight;

                //adjust the difference for the toolbars shown after expansion
                if (ctvm.IsAnyPastingTemplate) {
                    _deltaSize.Y += MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                }
                _deltaSize.Y += MpMeasurements.Instance.ClipTileEditToolbarHeight;

                _deltaSize.Y = Math.Min(maxDeltaHeight, actualHeightDiff);
            }            

            //make change in height so window doesn't get smaller but also doesn't extend past top of screen
            //_deltaSize.Height = Math.Min(maxDeltaHeight, deltaContentHeight);
            //sanity check so heights are the same after all that
            _deltaSize.X =  mwvm.ClipTrayWidth - MpMeasurements.Instance.ClipTileMinSize - (MpMeasurements.Instance.ClipTileExpandedMargin*2);

            mwvm.MainWindowTop -= _deltaSize.Y;
            _initialExpandedMainWindowTop = mwvm.MainWindowTop;

            mwvm.ClipTrayHeight += _deltaSize.Y;

            ctvm.TileBorderWidth += _deltaSize.X;
            ctvm.TileBorderHeight += _deltaSize.Y;

            ctvm.TileContentWidth += _deltaSize.X;
            ctvm.TileContentHeight += _deltaSize.Y;

            ctvm.OnPropertyChanged(nameof(ctvm.TrayX));
            MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayScreenWidth));
            mwvm.OnPropertyChanged(nameof(mwvm.ClipTrayAndCriteriaListHeight));

            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            if(clv != null) {
                clv.EditToolbarView.Visibility = Visibility.Visible;
                clv.UpdateLayout();
            } else {
                Debugger.Break();
            }

            var civl = AssociatedObject.GetVisualDescendents<MpContentItemView>().ToList();
            foreach(var civ in civl) {
                var civm = civ.DataContext as MpContentItemViewModel;
                civm.OnPropertyChanged(nameof(civm.EditorHeight));
                civm.OnPropertyChanged(nameof(civm.IsEditingContent));
                if(ctvm.IsTextItem) {
                    var rtbv = civ.GetVisualDescendent<MpRtbView>();
                    rtbv.Rtb.FitDocToRtb();
                    if (civm.IsSelected) {
                        rtbv.Focus();
                        rtbv.Rtb.CaretPosition = rtbv.Rtb.Document.ContentStart;
                    }
                }
            }

            if(clv != null) {
                var sv = clv.ContentListBox.GetScrollViewer();
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            AssociatedObject.UpdateLayout();

            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel += ApplicationHook_MouseWheel;

            mwvm.IsResizing = false;
            IsAnyExpandingOrUnexpanding = false;
        }

        public void Unexpand() {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            var mwvm = MpMainWindowViewModel.Instance;

            MpConsole.WriteLine("Unexpanding...");
            mwvm.IsResizing = true;
            IsAnyExpandingOrUnexpanding = true;


            //trigger app mode column to hide
            ctvm.OnPropertyChanged(nameof(ctvm.FlipButtonVisibility));
            ctvm.Parent.OnPropertyChanged(nameof(ctvm.Parent.IsAnyTileExpanded));
            mwvm.OnPropertyChanged(nameof(mwvm.AppModeButtonGridWidth));

            double temp = mwvm.MainWindowTop;
            //this resets window top to standard 
            MpMainWindowViewModel.Instance.SetupMainWindowRect();
            double deltaHeight = _originalMainWindowTop - temp;

            mwvm.MainWindowTop = _originalMainWindowTop;
            mwvm.ClipTrayHeight = MpMeasurements.Instance.ClipTrayMinHeight;

            var ctrvm = MpClipTrayViewModel.Instance;

            ctvm.TileBorderWidth = MpMeasurements.Instance.ClipTileBorderMinSize;
            ctvm.TileBorderHeight = MpMeasurements.Instance.ClipTileMinSize;

            ctvm.TileContentWidth = MpMeasurements.Instance.ClipTileContentMinWidth;
            ctvm.TileContentHeight = MpMeasurements.Instance.ClipTileContentHeight;

            ctvm.OnPropertyChanged(nameof(ctvm.TrayX));
            MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayScreenWidth));
            mwvm.OnPropertyChanged(nameof(mwvm.ClipTrayAndCriteriaListHeight));

            ctrvm.Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsPlaceholder)));

            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            if(clv != null) {
                clv.EditToolbarView.Visibility = Visibility.Collapsed;
                clv.UpdateLayout();

                var sv = clv.ContentListBox.GetScrollViewer();
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

                ctvm.ItemViewModels.ForEach(x => x.UnexpandItemCommand.Execute(null));
            }

            AssociatedObject.UpdateLayout();
            AssociatedObject.GetVisualAncestor<ListBoxItem>().UpdateLayout();

            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel -= ApplicationHook_MouseWheel;

            MpClipTrayViewModel.Instance.ScrollOffset = MpClipTrayViewModel.Instance.LastScrollOfset = _unexpandedScrollOfset;

            mwvm.IsResizing = false;
            IsAnyExpandingOrUnexpanding = false;
        }

        private void ApplicationHook_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            clv.Civm_OnScrollWheelRequest(this, -e.Delta);
        }
    }
}
