using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;


namespace MpWpfApp {
    public class MpSelectionBehavior : Behavior<FrameworkElement> {
        private const double MINIMUM_DRAG_DISTANCE = 10;

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
            //if (AssociatedObject is MpClipTileView) {
            //    AssociatedObject.Loaded += AssociatedObject_Loaded;

            //    AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;

            //    AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;

            //    AssociatedObject.MouseMove += AssociatedObject_MouseMove;

            //    AssociatedObject.KeyDown += AssociatedObject_KeyDown;

            //    AssociatedObject.KeyUp += AssociatedObject_KeyUp;
            //}
            AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseButtonDown;
        }

        private void AssociatedObject_PreviewMouseButtonDown(object sender, MouseButtonEventArgs e) {
            bool isRightClick = e.ChangedButton == MouseButton.Right;
            
            if(AssociatedObject is MpClipTileView) {
                var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
                if (ctvm.IsSelected && ctvm.IsAnyOverDragButton) {
                    e.Handled = false;
                    return;
                }
                SelectTile(isRightClick);
            } else if (AssociatedObject is MpContentItemView ||
                        AssociatedObject is MpRtbView) {
                var civm = AssociatedObject.DataContext as MpContentItemViewModel;
                if (civm.IsSelected && civm.IsOverDragButton) {
                    e.Handled = false;
                    return;
                }
                SelectContentItem(isRightClick);

                if (isRightClick &&
                   (AssociatedObject.DataContext as MpContentItemViewModel).IsEditingContent) {
                    e.Handled = false;
                    return;
                } else {
                    e.Handled = true;
                } 
            }
            if (isRightClick) {
                ShowContextMenu();
            }
        }

        #region Selection

        public void SelectTile(bool isRightClick) {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            ctvm.IsSelected = true;
            ctvm.LastSelectedDateTime = DateTime.Now;
            //if (ctvm.SelectedItems.Count == 0 && ctvm.HeadItem != null) {
            //    ctvm.HeadItem.IsSelected = true;
            //    ctvm.HeadItem.LastSubSelectedDateTime = DateTime.Now;
            //}
            if (ctvm.Parent.SelectedItems.Count > 1 &&
               !MpHelpers.Instance.IsMultiSelectKeyDown() &&
               !isRightClick) {
                foreach (var sctvm in ctvm.Parent.SelectedItems) {
                    if (sctvm != ctvm) {
                        if (sctvm.IsFlipped) {
                            ctvm.Parent.FlipTileCommand.Execute(sctvm);
                        }
                        sctvm.IsSelected = false;
                        foreach (var scivm in sctvm.SelectedItems) {
                            scivm.IsSelected = false;
                        }
                    }
                }
            }
        }

        public void SelectContentItem(bool isRightClick) {
            var civm = AssociatedObject.DataContext as MpContentItemViewModel;
            bool wasSelected = civm.IsSelected;
            civm.IsSelected = true;
            civm.LastSubSelectedDateTime = DateTime.Now;
            if (!civm.Parent.IsSelected) {
                civm.Parent.IsSelected = true;
                civm.Parent.LastSelectedDateTime = DateTime.Now;
            }
            if (!MpHelpers.Instance.IsMultiSelectKeyDown() && (!isRightClick || !wasSelected)) {
                if (civm.Parent.Parent.SelectedItems.Count > 1) {
                    foreach (var sctvm in civm.Parent.Parent.SelectedItems) {
                        if (sctvm != civm.Parent) {
                            sctvm.IsSelected = false;
                            foreach (var scivm in sctvm.SelectedItems) {
                                scivm.IsSelected = false;
                            }
                        }
                    }
                }
                if (civm.Parent.SelectedItems.Count > 1) {
                    foreach (var scivm in civm.Parent.SelectedItems) {
                        if (scivm != civm) {
                            scivm.IsSelected = false;
                        }
                    }
                }
            }
        }

        private void ShowContextMenu() {
            AssociatedObject.ContextMenu = new MpContentContextMenuView();
            AssociatedObject.ContextMenu.IsOpen = true;
        }

        #endregion

        #region Drag Drop

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            (AssociatedObject.DataContext as MpClipTileViewModel).MainWindowViewModel.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            if (!isDragging) {
                return;
            }
            InvalidateDrop();
        }

        #region Key Up/Down Events
        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e) {
            if (isDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    isDragCopy = false;
                }
            }
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e) {
            if (isDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    isDragCopy = true;
                }

                if (e.Key == Key.Escape) {
                    CancelDrag();
                }
            }
        }
        #endregion


        #region Mouse Events

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            //var ctv = sender as MpClipTileView;
            //var ctvm = ctv.DataContext as MpClipTileViewModel;
            //if(!ctvm.IsSelected) {
            //    ctv.SelectionBehavior.SelectTile(false);
            //} else {
            //    e.Handled = true;
            //}
            mouseStartPosition = e.GetPosition(Application.Current.MainWindow);
            AssociatedObject.CaptureMouse();
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            AssociatedObject.ReleaseMouseCapture();
            EndDrop();
            ResetCursor();
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            ctvm.ItemViewModels.ForEach(x => x.IsSubDragging = false);
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            Vector diff = e.GetPosition(Application.Current.MainWindow) - mouseStartPosition;
            if (AssociatedObject.IsMouseCaptured &&
                (diff.Length >= MINIMUM_DRAG_DISTANCE || isDragging)) {
                isDragging = true;
                var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
                ctvm.ItemViewModels.ForEach(x => x.IsSubDragging = x.IsSelected);
                Drag(e);
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
                //drag is over clip tray
                if (ctvIdx < ctrvlb.Items.Count) {
                    Rect dropTileRect = ctrvlb.GetListBoxItemRect(ctvIdx);
                    double dropTileMidX = dropTileRect.X + (dropTileRect.Width / 2);
                    Point trayMp = e.GetPosition(ctrvlb);
                    double mpXDistFromDropTileMidX = Math.Abs(dropTileMidX - trayMp.X);
                    if (mpXDistFromDropTileMidX <= dropTileRect.Width * 0.25) {
                        MpContentListView clv = ctrvlb.GetListBoxItem(ctvIdx).GetVisualDescendent<MpContentListView>();
                        var clvlb = clv.ContentListBox;
                        dropBehavior = clv.DropBehavior2;
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
                        dropBehavior = ClipTrayView.DropBehavior2;
                    }
                } else {
                    //dragging to the right of last item so assume its a tray drop
                    dropBehavior = ClipTrayView.DropBehavior2;
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
            if (dropBehavior != null && ctvIdx != dropBehavior.dropIdx) {
                StartDrop(ctvIdx);
            }
        }

        private void InvalidateDrop() {
            dropBehavior?.CancelDrop();
            dropBehavior = null;
            isDropValid = false;

            UpdateCursor();
        }

        private void CancelDrag() {
            if (!isDragging) {
                return;
            }
            AssociatedObject.ReleaseMouseCapture();
            isDragging = false;

            InvalidateDrop();
        }

        private void StartDrop(int dropIdx) {
            isDropValid = dropBehavior.StartDrop(MpClipTrayViewModel.Instance.SelectedItems, dropIdx);

            UpdateCursor();
        }

        private void EndDrop() {
            if (!isDragging) {
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
            Cursor currentCursor = DefaultCursor;

            if (!isDragging) {
                currentCursor = DefaultCursor;
            } else if (!isDropValid) {
                currentCursor = InvalidCursor;
            } else if (isDragCopy) {
                currentCursor = CopyCursor;
            } else if (isDragging) {
                currentCursor = MoveCursor;
            }

            SetCursor(currentCursor);
        }

        private void SetCursor(Cursor c) {
            Application.Current.MainWindow.ForceCursor = true;
            Application.Current.MainWindow.Cursor = c;
        }

        private void ResetCursor() {
            Application.Current.MainWindow.ForceCursor = true;
            Application.Current.MainWindow.Cursor = DefaultCursor;
        }

        #endregion

        #endregion
    }
}
