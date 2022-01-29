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
    public class MpTileResizeBehavior : MpBehavior<MpClipTileContainerView> {
        private bool _canResize;

        private bool _isResizing {
            get {
                if(AssociatedObject == null || AssociatedObject.BindingContext == null) {
                    return false;
                }
                return AssociatedObject.BindingContext.IsResizing;
            }
            set {
                if(AssociatedObject != null && 
                   AssociatedObject.BindingContext != null) {
                    AssociatedObject.BindingContext.IsResizing = value;
                    AssociatedObject.BindingContext.OnPropertyChanged(nameof(AssociatedObject.BindingContext.TileBorderBrush));
                }
            }
        }

        private double _maxResizeDist = MpMeasurements.Instance.ClipTileBorderThickness * 2;
        private Point _lastMousePosition;
        private bool _isMouseDown = false;

        protected override void OnLoad() {
            base.OnLoad();

            _dataContext = AssociatedObject.DataContext;
            MpMessenger.Register<MpMessageType>(
                _dataContext, 
                ReceiveClipTileMessage, 
                _dataContext);

            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            AssociatedObject.MouseDoubleClick += AssociatedObject_MouseDoubleClick;
        }


        protected override void OnUnload() {
            base.OnUnload();

            AssociatedObject.PreviewMouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;

            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;

            MpMessenger.Unregister<MpMessageType>(
                _dataContext, 
                ReceiveClipTileMessage, 
                _dataContext);
        }

        protected override void OnMainWindowHide(object sender, EventArgs e) {
            base.OnMainWindowHide(sender, e);
            if (AssociatedObject.BindingContext.IsAnyPastingTemplate) {
                return;
            }
            AssociatedObject.BindingContext.IsExpanded = false;
        }

        private void ReceiveClipTileMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.Expand:
                    break;
                case MpMessageType.Unexpand:
                    break;
            }
        }

        #region Manual Resize Event Handlers

        private void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            e.Handled = false;
            _isMouseDown = false;
            _isResizing = false;
            _lastMousePosition = new Point();
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _isMouseDown = true;
            e.Handled = false;
        }

        private void AssociatedObject_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            double deltaWidth = AssociatedObject.BindingContext.TileBorderWidth - MpClipTileViewModel.DefaultBorderWidth;
            Resize(-deltaWidth);

            if(MpClipTrayViewModel.Instance.PersistentUniqueWidthTileLookup.ContainsKey(AssociatedObject.BindingContext.HeadItem.CopyItemId)) {
                MpClipTrayViewModel.Instance.PersistentUniqueWidthTileLookup.Remove(AssociatedObject.BindingContext.HeadItem.CopyItemId);
            }
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            if (!_isResizing) {
                _canResize = false;

                AssociatedObject.BindingContext.CanResize = false;
                AssociatedObject.BindingContext.OnPropertyChanged(nameof(AssociatedObject.BindingContext.TileBorderBrush));
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if(MpDragDropManager.Instance.IsDragAndDrop) {
                return;
            }
            if(_isResizing && !_isMouseDown) {
                //resize complete so reset

                if (AssociatedObject.IsMouseCaptured) {
                    AssociatedObject.ReleaseMouseCapture();
                }

                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
                
                _isResizing = _canResize = false;
                _lastMousePosition = new Point();

                //send resize as mainwindow resize so all drop handlers update rects
                MpMessenger.Send<MpMessageType>(
                    MpMessageType.ResizeCompleted,
                    (Application.Current.MainWindow as MpMainWindow).TitleBarView.MainWindowResizeBehvior);
            }
            var mp = e.GetPosition(AssociatedObject);
            
            if(!_isResizing) {
                if (AssociatedObject.ActualWidth - mp.X <= _maxResizeDist) {
                    //can Resize right
                    _canResize = true;
                } else {
                    _canResize = false;
                }
            }

            if(_canResize) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.ResizeWE;

                if(_isMouseDown && !_isResizing) {
                    _isResizing = true;
                    if (!AssociatedObject.IsMouseCaptured) {
                        _isResizing = AssociatedObject.CaptureMouse();
                    }
                    _lastMousePosition = e.GetPosition(AssociatedObject);
                }
                if (_isResizing) {
                    double deltaX = mp.X - _lastMousePosition.X;

                    Resize(deltaX);
                } 
            } else {
                //MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
            }

            AssociatedObject.BindingContext.IsResizing = _isResizing;
            AssociatedObject.BindingContext.CanResize = _canResize;
            AssociatedObject.BindingContext.OnPropertyChanged(nameof(AssociatedObject.BindingContext.TileBorderBrush));


            _lastMousePosition = mp;
        }

        #endregion

        public void Resize(double deltaWidth) {
            if (Math.Abs(deltaWidth) == 0) {
                return;
            }

            var ctvm = AssociatedObject.BindingContext;
            var ctrvm = MpClipTrayViewModel.Instance;
            var msrmvm = MpMeasurements.Instance;

            double oldHeadTrayX = ctrvm.HeadItem.TrayX;
            double oldScrollOffset = ctrvm.ScrollOffset;

            double origWidth = ctvm.TileBorderWidth;
            ctvm.TileBorderWidth = Math.Max(msrmvm.ClipTileBorderMinWidth,ctvm.TileBorderWidth + deltaWidth);
            
            ctrvm.PersistentUniqueWidthTileLookup
                .AddOrReplace(ctvm.HeadItem.CopyItemId, ctvm.TileBorderWidth);

            double widthDiff = ctvm.TileBorderWidth - origWidth;

            if(_canResize) {
                ctrvm.Items.
                    Where(x => x.QueryOffsetIdx >= ctvm.QueryOffsetIdx).
                    ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
            }

            //double oldScrollOffsetDiffWithHead = ctrvm.ScrollOffset - oldHeadTrayX;

            //ctrvm.OnPropertyChanged(nameof(ctrvm.ClipTrayTotalTileWidth));
            //ctrvm.OnPropertyChanged(nameof(ctrvm.ClipTrayScreenWidth));
            //ctrvm.OnPropertyChanged(nameof(ctrvm.ClipTrayTotalWidth));
            //ctrvm.OnPropertyChanged(nameof(ctrvm.MaximumScrollOfset));
            ////ctrvm.Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));

            //double newHeadTrayX = ctrvm.HeadItem.TrayX;
            //double headOffsetRatio = newHeadTrayX / oldHeadTrayX;

            //headOffsetRatio = double.IsNaN(headOffsetRatio) ? 0 : headOffsetRatio;
            //double newScrollOfsetDiffWithHead = headOffsetRatio * oldScrollOffsetDiffWithHead;
            //double newScrollOfset = (ctrvm.HeadQueryIdx * msrmvm.ClipTileMinSize) + newScrollOfsetDiffWithHead;
            
            MpClipTrayViewModel.Instance.AdjustScrollOffsetToResize(oldHeadTrayX, oldScrollOffset);
            //ctrvm.ScrollOffset = ctrvm.LastScrollOfset = newScrollOfset;

            MpMessenger.Send<MpMessageType>(MpMessageType.Resizing);
        }
    }
}
