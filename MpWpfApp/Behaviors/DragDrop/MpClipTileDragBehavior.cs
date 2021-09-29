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
    public class MpClipTileDragBehavior : Behavior<MpClipTileView> {
        private const double MINIMUM_DRAG_DISTANCE = 20;
        private static MpClipTrayView ClipTrayView;

        private Cursor originalCursor;
        private Cursor MoveCursor = Cursors.Hand;
        private Cursor CopyCursor = Cursors.Cross;
        private Cursor InvalidCursor = Cursors.No;
        private bool isDropValid = false;

        private double maxTileDropDist;
        private Point elementStartPosition;
        private Point mouseStartPosition;
        private TranslateTransform transform = new TranslateTransform();

        private MpDragRectListAdorner dragRectListAdorner;
        private AdornerLayer adornerLayer;


        private MpDropBehavior dropBehavior;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            if(AssociatedObject.Visibility != Visibility.Visible) {
                EndDragOps();
            }
        }

        protected override void OnAttached() {            
            AssociatedObject.Loaded += AssociatedObject_Loaded;

            Window parent = Application.Current.MainWindow;
            if (ClipTrayView == null) {
                ClipTrayView = parent.GetVisualDescendent<MpClipTrayView>();
            }
            
            AssociatedObject.RenderTransform = transform;

            AssociatedObject.MouseLeftButtonDown += (sender, e) => {
                elementStartPosition = AssociatedObject.TranslatePoint(new Point(), parent);
                mouseStartPosition = e.GetPosition(parent);
                AssociatedObject.CaptureMouse();
            };

            AssociatedObject.MouseLeftButtonUp += (s, e) => {
                EndDragOps();
                AssociatedObject.ReleaseMouseCapture();                
            };

            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            (AssociatedObject.DataContext as MpClipTileViewModel).MainWindowViewModel.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
            maxTileDropDist = AssociatedObject.ActualWidth * 0.4;

            dragRectListAdorner = new MpDragRectListAdorner(AssociatedObject);
            this.adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            adornerLayer.Add(dragRectListAdorner);

            adornerLayer.Update();
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            CancelDragOver();
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            Vector diff = e.GetPosition(Application.Current.MainWindow) - mouseStartPosition;
            if (AssociatedObject.IsMouseCaptured &&
                diff.Length >= MINIMUM_DRAG_DISTANCE) {
                if(MpHelpers.Instance.IsEscapeKeyDown()) {
                    CancelDragOver();
                    AssociatedObject.ReleaseMouseCapture();
                    return;
                }
                var mkl = MpHelpers.Instance.GetModKeyDownList();
                if (mkl.Contains(Key.LeftCtrl) || mkl.Contains(Key.RightCtrl)) {
                    SetCursor(CopyCursor);
                } else if(isDropValid) {
                    SetCursor(MoveCursor);
                } else {
                    SetCursor(InvalidCursor);
                }
                //if mouse down was on tile
                if (dropBehavior == null) {
                    //flag content items for drag selection
                    ShowDragAdorners();
                }
                MpDropBehavior lastDropBehavior = dropBehavior;
                ListBox ctrvlb = ClipTrayView.ClipTray;
                int ctvIdx = ClipTrayView.ClipTray.GetItemIndexAtPoint(e.GetPosition(ctrvlb));
                if(ctvIdx >= 0) {
                    //drag is over clip tray
                    if(ctvIdx < ctrvlb.Items.Count) {
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
                            if(trayMp.X > dropTileMidX) {
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
                    if(dropBehavior != null) {
                        CancelDragOver();                        
                    }
                }
                if(lastDropBehavior != dropBehavior && lastDropBehavior != null) {
                    lastDropBehavior.CancelDrop();                  
                }
                if (dropBehavior != null && ctvIdx != dropBehavior.dropIdx) {
                    StartDragOver(ctvIdx);                    
                }
            }
        }

        private void StartDragOver(int idx) {            
            ShowDragAdorners();
            isDropValid = dropBehavior.StartDrop(MpClipTrayViewModel.Instance.SelectedModels, idx);
            if (!isDropValid) {
                SetCursor(InvalidCursor);
            } else {
                ResetCursor();
            }
        }

        private void CancelDragOver() {
            dropBehavior?.CancelDrop();
            dropBehavior = null;
            SetCursor(InvalidCursor);
            isDropValid = false;
        }
        private void EndDragOps() {
            if (dropBehavior != null) {
                dropBehavior.Drop();// GetCursor() == CopyCursor);
                dropBehavior = null;
            }
            isDropValid = false;
            HideDragAdorners();
            ResetCursor();
        }

        private void ShowDragAdorners() {
            dragRectListAdorner.RectList = ClipTrayView.GetSelectedContentItemViewRects(AssociatedObject);
            dragRectListAdorner.IsShowing = true;
            adornerLayer.Update();
        }

        private void HideDragAdorners() {
            dragRectListAdorner.RectList.Clear();
            dragRectListAdorner.IsShowing = false;
            adornerLayer.Update();
        }

        private void SetCursor(Cursor c) {
            return;
            if(originalCursor == null) {
                originalCursor = GetCursor();
            }
            Application.Current.MainWindow.ForceCursor = true;
            Application.Current.MainWindow.Cursor = c;
        }

        private Cursor GetCursor() {
            return Application.Current.MainWindow.Cursor;
        }
        private void ResetCursor() {
            if(originalCursor != null) {
                SetCursor(originalCursor);
            }
            originalCursor = null;
        }
    }
}
