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

namespace MpWpfApp {    //MpContentDragBehavior
    public class MpContentListBoxSelectionBehavior : Behavior<MpContentItemView> {
        private static MpContentContextMenuView _ContentContextMenu;

        #region Private Variables
        private const double MINIMUM_DRAG_DISTANCE = 10;

        private bool IsDropValid => _curDropTarget != null;

        private bool IsDragging => MpDragDropManager.Instance.IsDragAndDrop;
        private bool IsDragCopy = false;

        private Point _mouseStartPosition;

        private MpIContentDropTarget _curDropTarget;

        private bool _wasUnloaded = false;

        private MpContentItemViewModel _persistentContentItemViewModel;

        #endregion

        #region Properties

        #endregion

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
            if(IsDragging) {
                _wasUnloaded = true;
            } else if(!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                //Detach();
            }
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            MpMainWindowViewModel.Instance.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {            
            Reset();
        }

        #region Mouse Events

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (AssociatedObject.BindingContext.IsSelected &&
                AssociatedObject.BindingContext.Parent.IsExpanded) {
                e.Handled = false;
                return;
            }
            AssociatedObject.BindingContext.IsSelected = true;

            MpDragDropManager.Instance.StartDragCheck(e.GetPosition(Application.Current.MainWindow));


            //Mouse.AddMouseMoveHandler(Application.Current.MainWindow, MainWindow_MouseMove);
            //Mouse.AddMouseUpHandler(Application.Current.MainWindow, MainWindow_MouseUp);

            //Keyboard.AddKeyDownHandler(Application.Current.MainWindow, MainWindow_KeyDown);
            //Keyboard.AddKeyUpHandler(Application.Current.MainWindow, MainWindow_KeyUp);
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
            bool handleMouseUp = !(_curDropTarget is MpExternalDropBehavior);

            if (IsDragging) {
                EndDrop();
                ResetCursor();
            } else if(MpDragDropManager.Instance.IsDragAndDrop) {
                //this maynot be necessary but this is to fix if drop wasn't handled
                MpDragDropManager.Instance.StopDrag();
            }
            if(e.RoutedEvent != null && handleMouseUp) {
                e.Handled = true;
            } else if(e.RoutedEvent != null) {
                e.Handled = false;
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e) {
            if(Mouse.LeftButton  == MouseButtonState.Released) {
                MainWindow_MouseUp(this, new RoutedEventArgs());
            }
            Vector diff = e.GetPosition(Application.Current.MainWindow) - _mouseStartPosition;
            
            if (diff.Length >= MINIMUM_DRAG_DISTANCE || IsDragging) {
                if(!IsDragging) {                    
                    //_isDragging = true; 
                    Mouse.Capture(Application.Current.MainWindow);
                    CheckKeys();
                    MpDragDropManager.Instance.StartDrag();
                }
                Drag();
            }
        }

        #endregion

        #region Key Up/Down Events
        private void MainWindow_KeyUp(object sender, KeyEventArgs e) {
            if (IsDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    IsDragCopy = false;
                }
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e) {
            if (IsDragging) {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                    IsDragCopy = true;
                }

                if (e.Key == Key.Escape) {
                    Reset();
                }
            }
        }

        private void CheckKeys() {
            if (IsDragging) {
                IsDragCopy = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            }
        }
        #endregion

        #region State Changes

        private void Drag() {
            var dropTarget = MpDragDropManager.Instance.SelectDropTarget(MpClipTrayViewModel.Instance.PersistentSelectedModels);

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
            //MpContentDropManager.Instance.StopDrag();

            UpdateCursor();
        }

        private void Reset() {
            MpDragDropManager.Instance.StopDrag();

            Mouse.RemoveMouseMoveHandler(Application.Current.MainWindow, MainWindow_MouseMove);
            Mouse.RemoveMouseUpHandler(Application.Current.MainWindow, MainWindow_MouseUp);

            Keyboard.RemoveKeyDownHandler(Application.Current.MainWindow, MainWindow_KeyDown);
            Keyboard.RemoveKeyUpHandler(Application.Current.MainWindow, MainWindow_KeyUp);

            _curDropTarget = null;
            IsDragCopy = false;

            UpdateCursor();
        }

        private void EndDrop() {
            MpHelpers.Instance.RunOnMainThread(async () => {                
                if (!IsDragging) {
                    return;
                }
                if (_curDropTarget != null) {
                    await _curDropTarget.Drop(IsDragCopy,MpClipTrayViewModel.Instance.PersistentSelectedModels);
                } 
                
                //_isDragging = false;
                
                Reset();

                if (_wasUnloaded) {
                    Detach();
                }
            });
        }

        #endregion

        #region Cursor Updates

        private void UpdateCursor() {
            MpCursorType currentCursor = MpCursorType.Default;

            if (!IsDragging) {
                currentCursor = MpCursorType.Default;
            } else if (!IsDropValid) {
                currentCursor = MpCursorType.Invalid;
            } else if (IsDragCopy) {
                currentCursor = _curDropTarget.CopyCursor;
            } else if (IsDragging) {
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
