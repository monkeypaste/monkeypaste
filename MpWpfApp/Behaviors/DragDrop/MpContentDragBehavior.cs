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

        private bool _wasUnloaded = false;

        private bool _isDropValid => _curDropTarget != null;

        private bool _isDragging = false;
        private bool _isDragCopy = false;

        private Point _mouseStartPosition;

        private MpIContentDropTarget _curDropTarget;

        protected override void OnAttached() {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseRightButtonDown += AssociatedObject_PreviewMouseRightButtonDown;
            AssociatedObject.KeyDown += MainWindow_KeyDown;
            AssociatedObject.KeyUp += MainWindow_KeyUp;
        }

        protected override void OnDetaching() {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseRightButtonDown -= AssociatedObject_PreviewMouseRightButtonDown;
            AssociatedObject.KeyDown -= MainWindow_KeyDown;
            AssociatedObject.KeyUp -= MainWindow_KeyUp;

            MpMainWindowViewModel.Instance.OnMainWindowHide -= MainWindowViewModel_OnMainWindowHide;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e) {
            if(_isDragging) {
                _wasUnloaded = true;
            } else if(!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                //Detach();
            }
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
            if (AssociatedObject.BindingContext.IsSelected &&
                AssociatedObject.BindingContext.Parent.IsExpanded) {
                e.Handled = false;
                return;
            }
            AssociatedObject.BindingContext.IsSelected = true;

            _mouseStartPosition = e.GetPosition(Application.Current.MainWindow);

            Mouse.AddMouseMoveHandler(Application.Current.MainWindow, MainWindow_MouseMove);
            Mouse.AddMouseUpHandler(Application.Current.MainWindow, MainWindow_MouseUp);

            Keyboard.AddKeyDownHandler(Application.Current.MainWindow, MainWindow_KeyDown);
            Keyboard.AddKeyUpHandler(Application.Current.MainWindow, MainWindow_KeyUp);
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

        private void MainWindow_MouseUp(object sender, RoutedEventArgs e) {
            Mouse.RemoveMouseMoveHandler(Application.Current.MainWindow, MainWindow_MouseMove);
            Mouse.RemoveMouseUpHandler(Application.Current.MainWindow, MainWindow_MouseUp);

            Keyboard.RemoveKeyDownHandler(Application.Current.MainWindow, MainWindow_KeyDown);
            Keyboard.RemoveKeyUpHandler(Application.Current.MainWindow, MainWindow_KeyUp);

            if (_isDragging) {
                EndDrop();
                ResetCursor();
            } else if(MpContentDropManager.Instance.IsDragAndDrop) {
                //this maynot be necessary but this is to fix if drop wasn't handled
                MpContentDropManager.Instance.StopDrag();
            }
            if(e.RoutedEvent != null) {
                e.Handled = true;
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e) {
            if(Mouse.LeftButton  == MouseButtonState.Released) {
                MainWindow_MouseUp(this, new RoutedEventArgs());
            }
            Vector diff = e.GetPosition(Application.Current.MainWindow) - _mouseStartPosition;
            
            if (diff.Length >= MINIMUM_DRAG_DISTANCE || _isDragging) {
                if(!_isDragging) {                    
                    _isDragging = true;
                    CheckKeys();
                    MpContentDropManager.Instance.StartDrag();
                }
                Drag();
            }
        }

        #endregion

        #region Key Up/Down Events
        private void MainWindow_KeyUp(object sender, KeyEventArgs e) {
            if (_isDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    _isDragCopy = false;
                }
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e) {
            if (_isDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    _isDragCopy = true;
                }

                if (e.Key == Key.Escape) {
                    CancelDrag();
                }
            }
        }

        private void CheckKeys() {
            if (_isDragging) {
                _isDragCopy = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            }
        }
        #endregion

        #region State Changes

        private void Drag() {
            //UpdateCursor();

            var dropTarget = MpContentDropManager.Instance.Select(MpClipTrayViewModel.Instance.PersistentSelectedModels);

            if (dropTarget != _curDropTarget) {
                _curDropTarget?.CancelDrop();
                _curDropTarget = dropTarget;
                _curDropTarget?.StartDrop();
            }
            if (_curDropTarget == null) {
                InvalidateDrop();
            } else {
                _curDropTarget.ContinueDragOverTarget();
            }

            UpdateCursor();
        }

        private void InvalidateDrop() {
            _curDropTarget?.CancelDrop();
            _curDropTarget = null;

            UpdateCursor();
        }

        private void CancelDrag() {
            MpConsole.WriteLine("Drag Canceled");
            if (!_isDragging) {
                return;
            }
            _isDragging = false;

            InvalidateDrop();
        }

        private void EndDrop() {
            MpHelpers.Instance.RunOnMainThread(async () => {                
                if (!_isDragging) {
                    return;
                }
                if (_curDropTarget != null) {
                    await _curDropTarget.Drop(_isDragCopy,MpClipTrayViewModel.Instance.PersistentSelectedModels);
                    _curDropTarget = null;
                } 
                MpContentDropManager.Instance.StopDrag();
                _isDragging = false;
                
                UpdateCursor();

                if (_wasUnloaded) {
                    Detach();
                }
            });
        }

        #endregion

        #region Cursor Updates

        private void UpdateCursor() {
            MpCursorType currentCursor = MpCursorType.Default;

            if (!_isDragging) {
                currentCursor = MpCursorType.Default;
            } else if (!_isDropValid) {
                currentCursor = MpCursorType.Invalid;
            } else if (_isDragCopy) {
                currentCursor = _curDropTarget.CopyCursor;
            } else if (_isDragging) {
                currentCursor = _curDropTarget.MoveCursor;
            }

            MpMouseViewModel.Instance.CurrentCursor = currentCursor;
        }

        private void ResetCursor() {
            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        #endregion
    }
}
