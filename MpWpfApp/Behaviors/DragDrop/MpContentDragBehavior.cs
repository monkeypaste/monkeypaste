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
    public class MpContentDragBehavior : Behavior<FrameworkElement> {
        private const double MINIMUM_DRAG_DISTANCE = 10;
        private static MpContentContextMenuView _ContentContextMenu;

        private bool isDropValid = false;
        private bool isDragging = false;
        private bool isDragCopy = false;

        public Cursor DefaultCursor = Cursors.Arrow;
        private Cursor MoveCursor = Cursors.Hand;
        private Cursor CopyCursor = Cursors.Cross;
        private Cursor InvalidCursor = Cursors.No;

        private Point mouseStartPosition;

        private MpDropBehavior dropBehavior;

        protected override void OnAttached() {            
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseRightButtonDown += AssociatedObject_PreviewMouseRightButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;

           // MpMessenger.Instance.Register<MpMessageType>(AssociatedObject.DataContext, ReceiveClipTileMessage, AssociatedObject.DataContext);
        }

        private void ReceiveClipTileMessage(MpMessageType msg) {
            switch (msg) {
                //case MpMessageType.Expand:
                //    AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
                //    AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
                //    AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
                //    AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                //    AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
                //    break;
                //case MpMessageType.Unexpand:
                //    AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
                //    AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
                //    AssociatedObject.MouseMove += AssociatedObject_MouseMove;
                //    AssociatedObject.KeyDown += AssociatedObject_KeyDown;
                //    AssociatedObject.KeyUp += AssociatedObject_KeyUp;
                //    break;
            }
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
           MpMainWindowViewModel.Instance.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            if(!isDragging) {
                return;
            }
            InvalidateDrop();
        }

        #region Mouse Events

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }
            mouseStartPosition = e.GetPosition(Application.Current.MainWindow);

            //
            if (AssociatedObject.DataContext is MpContentItemViewModel civm) {                
                if (civm.IsSelected) {
                } else {
                    civm.IsSelected = true;
                }
                //if(civm.IsOverHyperlink) {
                //    e.Handled = false;
                //    return;
                //}
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
            if (AssociatedObject.DataContext is MpContentItemViewModel civm) {
                if (civm.IsSelected) {
                } else {
                    civm.IsSelected = true;
                }
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
            Vector diff = e.GetPosition(Application.Current.MainWindow) - mouseStartPosition;
            if (AssociatedObject.IsMouseCaptured &&
                (diff.Length >= MINIMUM_DRAG_DISTANCE || isDragging)) {
                isDragging = true;
                MpClipTrayViewModel.Instance.SelectedContentItemViewModels.ForEach(x => x.IsItemDragging = true);
                Drag(e);
            }
        }

        #endregion

        #region Key Up/Down Events
        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e) {
            if(isDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    isDragCopy = false;
                }
            }
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e) {
            if(isDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    isDragCopy = true;
                }

                if (e.Key == Key.Escape) {
                    CancelDrag();
                }
            }
        }
        #endregion

        #region State Changes
        private void Drag(MouseEventArgs e) {
            var parent = Application.Current.MainWindow;

            UpdateCursor();

            var ClipTrayView = parent.GetVisualDescendent<MpClipTrayView>();
            MpDropBehavior lastDropBehavior = dropBehavior;
            ListBox ctrvlb = ClipTrayView.ClipTray;
            int ctvIdx = ClipTrayView.ClipTray.GetItemIndexAtPoint(e.GetPosition(ctrvlb));
            if (ctvIdx >= 0) {
                //drag is within clip tray bounds
                if (ctvIdx < ctrvlb.Items.Count) {
                    Rect dropTileRect = ctrvlb.GetListBoxItemRect(ctvIdx);
                    double dropTileMidX = dropTileRect.X + (dropTileRect.Width / 2);
                    Point trayMp = e.GetPosition(ctrvlb.GetScrollViewer());
                    double mpXDistFromDropTileMidX = Math.Abs(dropTileMidX - trayMp.X);
                    if (mpXDistFromDropTileMidX <= dropTileRect.Width * 0.25) {
                        MpContentListView clv = ctrvlb.GetListBoxItem(ctvIdx).GetVisualDescendent<MpContentListView>();
                        var clvlb = clv.ContentListBox;
                        dropBehavior = clv.DropBehavior;
                        ctvIdx = clv.ContentListBox.GetItemIndexAtPoint(e.GetPosition(clv.ContentListBox));
                        if (ctvIdx < clvlb.Items.Count) {
                            Rect dropItemRect = clvlb.GetListBoxItemRect(ctvIdx);
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
                        dropBehavior = ClipTrayView.DropBehavior;
                    }
                } else {
                    //dragging to the right of last item so assume its a tray drop
                    dropBehavior = ClipTrayView.DropBehavior;
                }
            } else {
                //outside of tray
                if (dropBehavior != null) {
                    InvalidateDrop();
                }
            }
            if (lastDropBehavior != dropBehavior && lastDropBehavior != null) {
                lastDropBehavior.CancelDrop();
            }
            if (dropBehavior != null) {
                if (ctvIdx != dropBehavior.dropIdx) {
                    StartDrop(ctvIdx); 
                }
                dropBehavior.AutoScrollByMouse();
            }
        }

        private void InvalidateDrop() {
            dropBehavior?.CancelDrop();
            dropBehavior = null;
            isDropValid = false;

            UpdateCursor();
        }

        private void CancelDrag() {
            if(!isDragging) {
                return;
            }
            AssociatedObject.ReleaseMouseCapture();
            isDragging = false;
            MpClipTrayViewModel.Instance.SelectedContentItemViewModels.ForEach(x => x.IsItemDragging = false);

            InvalidateDrop();
        }

        private void StartDrop(int dropIdx) {
            isDropValid = dropBehavior.StartDrop(MpClipTrayViewModel.Instance.SelectedItems, dropIdx);

            UpdateCursor();
        }

        private void EndDrop() {
            if(!isDragging) {
                return;
            }
            if (dropBehavior != null) {
                dropBehavior.Drop(isDragCopy);
                dropBehavior = null;
            }
            isDropValid = false;
            isDragging = false;
            AssociatedObject.ReleaseMouseCapture();

            UpdateCursor();
        }

        #endregion

        #region Adorner Updates

        #endregion

        #region Cursor Updates

        private void UpdateCursor() {
            MpCursorType currentCursor = MpCursorType.Default;

            if (!isDragging) {
                currentCursor = MpCursorType.Default;
            } else if(!isDropValid) {
                currentCursor = MpCursorType.Invalid;
            } else if(isDragCopy) {
                currentCursor = MpCursorType.Copy;
            } else if(isDragging) {
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
