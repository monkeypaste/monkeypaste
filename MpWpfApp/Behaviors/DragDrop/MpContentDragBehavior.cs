using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {    
    public class MpContentDragBehavior : Behavior<MpContentItemView> {
        private const double MINIMUM_DRAG_DISTANCE = 10;
        private static MpContentContextMenuView _ContentContextMenu;

        private bool _isDropValid = false;
        private bool _isDragging = false;
        private bool _isDragCopy = false;

        private Point _mouseStartPosition;

        private MpDropBehavior _currentDropBehavior;

        private MpIContentDropTarget _curDropTarget;

        protected override void OnAttached() {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseRightButtonDown += AssociatedObject_PreviewMouseRightButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            MpMainWindowViewModel.Instance.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            if (!_isDragging) {
                return;
            }
            InvalidateDrop();
        }

        #region Mouse Events

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }
            _mouseStartPosition = e.GetPosition(Application.Current.MainWindow);

            if(!AssociatedObject.BindingContext.IsSelected) {
                AssociatedObject.BindingContext.IsSelected = true;
            }

            AssociatedObject.CaptureMouse();

            e.Handled = true;
        }


        private void AssociatedObject_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            if (MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }
            if (_ContentContextMenu == null) {
                _ContentContextMenu = new MpContentContextMenuView();
            }

            if (!AssociatedObject.BindingContext.IsSelected) {
                AssociatedObject.BindingContext.IsSelected = true;
            }

            e.Handled = true;

            AssociatedObject.ContextMenu = _ContentContextMenu;
            AssociatedObject.ContextMenu.PlacementTarget = AssociatedObject;
            AssociatedObject.ContextMenu.IsOpen = true;
        }

        private void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }
            AssociatedObject.ReleaseMouseCapture();
            EndDrop();
            ResetCursor();

            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            Vector diff = e.GetPosition(Application.Current.MainWindow) - _mouseStartPosition;
            if (AssociatedObject.IsMouseCaptured &&
                (diff.Length >= MINIMUM_DRAG_DISTANCE || _isDragging)) {
                if(!_isDragging) {
                    MpClipTrayViewModel.Instance.StartDrag();
                    _isDragging = true;
                }
                
                //MpClipTrayViewModel.Instance.SelectedContentItemViewModels.ForEach(x => x.IsItemDragging = true);
                Drag(e);
            }
        }

        #endregion

        #region Key Up/Down Events
        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e) {
            if (_isDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    _isDragCopy = false;
                }
            }
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e) {
            if (_isDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    _isDragCopy = true;
                }

                if (e.Key == Key.Escape) {
                    CancelDrag();
                }
            }
        }
        #endregion

        #region State Changes

        private void Drag(MouseEventArgs e) {
            UpdateCursor();

            var dropTarget = MpContentDropManager.Instance.Select(MpClipTrayViewModel.Instance.GetDragData(), e);
            if(dropTarget != _curDropTarget) {
                _curDropTarget?.CancelDrop();
                _curDropTarget = dropTarget;
            }
            if(_curDropTarget == null) {
                InvalidateDrop();
            } else {
                _isDropValid = true;
                UpdateCursor();
                _curDropTarget.AutoScrollByMouse(e);
                _curDropTarget.ContinueDragOverTarget(e);
            }
            

            return;

            var parent = Application.Current.MainWindow;
            var ClipTrayView = parent.GetVisualDescendent<MpClipTrayView>();
            MpDropBehavior lastDropBehavior = _currentDropBehavior;
            ListBox ctrvlb = ClipTrayView.ClipTray;
            int ctvIdx = ClipTrayView.ClipTray.GetItemIndexAtPoint(e.GetPosition(ctrvlb));
            if (ctvIdx >= 0) {
                //drag is within clip tray bounds
                if (ctvIdx < ctrvlb.Items.Count) {
                    Rect dropTileRect = ctrvlb.GetListBoxItemRect(ctvIdx);
                    double dropTileMidX = dropTileRect.X + (dropTileRect.Width / 2);
                    Point trayMp = e.GetPosition(ctrvlb.GetScrollViewer());
                    double mpXDistFromDropTileMidX = Math.Abs(dropTileMidX - trayMp.X);

                    MpContentListView clv = ctrvlb.GetListBoxItem(ctvIdx).GetVisualDescendent<MpContentListView>();
                    int tempCtvIdx = clv.ContentListBox.GetItemIndexAtPoint(e.GetPosition(clv.ContentListBox));
                    tempCtvIdx = tempCtvIdx < 0 ? 0 : tempCtvIdx >= clv.ContentListBox.Items.Count ? clv.ContentListBox.Items.Count - 1 : tempCtvIdx;
                    var dropItem = clv.ContentListBox.GetListBoxItem(tempCtvIdx);
                    Rect dropItemRect = dropItem.GetRect();
                    Point itemMp = e.GetPosition(dropItem);
                    var itemRtbView = dropItem.GetVisualDescendent<MpRtbView>();
                    bool isMerge = false;
                    if (itemMp.Y > dropItemRect.Height / 2) {
                        itemRtbView.ScrollToEnd();                        
                    } else {
                        itemRtbView.ScrollToHome();                        
                    }
                    double homeDist = itemRtbView.HomeCaretLine[0].Distance(e.GetPosition(itemRtbView.Rtb.Document));
                    double endDist = itemRtbView.EndCaretLine[0].Distance(e.GetPosition(itemRtbView.Rtb.Document));
                    if (endDist < homeDist && endDist < 20) {
                        isMerge = true;
                        MpRtbView.ShowEndCaretAdorner(itemRtbView);
                    } else if (homeDist < endDist && homeDist < 20) {
                        isMerge = true;
                        MpRtbView.ShowHomeCaretAdorner(itemRtbView);
                    }
                    if (isMerge) {
                        _currentDropBehavior = itemRtbView.DropBehavior;
                    } else {
                        if (mpXDistFromDropTileMidX <= dropTileRect.Width * 0.25) {
                            clv = ctrvlb.GetListBoxItem(ctvIdx).GetVisualDescendent<MpContentListView>();
                            var clvlb = clv.ContentListBox;
                            _currentDropBehavior = clv.DropBehavior;
                            ctvIdx = clv.ContentListBox.GetItemIndexAtPoint(e.GetPosition(clv.ContentListBox));
                            if (ctvIdx < clvlb.Items.Count) {
                                dropItemRect = clvlb.GetListBoxItemRect(ctvIdx);
                                double dropItemMidY = dropItemRect.Y + (dropItemRect.Height / 2);
                                Point itemListMp = e.GetPosition(clvlb);
                                if (itemListMp.Y > dropItemMidY) {
                                    ctvIdx = ctvIdx + 1;
                                }
                            }
                        } else {
                            if (trayMp.X > dropTileMidX) {
                                ctvIdx = ctvIdx + 1;
                            }
                            _currentDropBehavior = ClipTrayView.DropBehavior;
                        }
                    }
                } else {
                    //dragging to the right of last item so assume its a tray drop
                    _currentDropBehavior = ClipTrayView.DropBehavior;
                }
            } else {
                //outside of tray
                if (_currentDropBehavior != null) {
                    InvalidateDrop();
                }
            }
            if (lastDropBehavior != _currentDropBehavior && lastDropBehavior != null) {
                lastDropBehavior.CancelDrop();
            }
            if (_currentDropBehavior != null) {
                if (ctvIdx != _currentDropBehavior.dropIdx) {
                    StartDrop(ctvIdx);
                }
                _currentDropBehavior.AutoScrollByMouse();
            }
        }

        private void InvalidateDrop() {
            _currentDropBehavior?.CancelDrop();
            _currentDropBehavior = null;
            _isDropValid = false;

            UpdateCursor();
        }

        private void CancelDrag() {
            if (!_isDragging) {
                return;
            }
            AssociatedObject.ReleaseMouseCapture();
            _isDragging = false;
            MpClipTrayViewModel.Instance.SelectedContentItemViewModels.ForEach(x => x.IsItemDragging = false);

            InvalidateDrop();
        }

        private void StartDrop(int dropIdx) {
            _isDropValid = _currentDropBehavior.StartDrop(MpClipTrayViewModel.Instance.SelectedItems, dropIdx);

            UpdateCursor();
        }

        private void EndDrop() {
            if (!_isDragging) {
                return;
            }
            if (_currentDropBehavior != null) {
                _currentDropBehavior.Drop(_isDragCopy);
                _currentDropBehavior = null;
            } else {
                MpClipTrayViewModel.Instance.CancelDrag();
            }
            _isDropValid = false;
            _isDragging = false;
            AssociatedObject.ReleaseMouseCapture();

            UpdateCursor();
        }

        #endregion

        #region Adorner Updates

        #endregion

        #region Cursor Updates

        private void UpdateCursor() {
            MpCursorType currentCursor = MpCursorType.Default;

            if (!_isDragging) {
                currentCursor = MpCursorType.Default;
            } else if (!_isDropValid) {
                currentCursor = MpCursorType.Invalid;
            } else if (_isDragCopy) {
                currentCursor = MpCursorType.Copy;
            } else if (_isDragging) {
                currentCursor = MpCursorType.Move;
            }

            MpMouseViewModel.Instance.CurrentCursor = currentCursor;
        }

        private void ResetCursor() {
            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        #endregion
    }
}
