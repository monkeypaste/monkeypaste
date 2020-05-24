using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using NonInvasiveKeyboardHookLibrary;

namespace MonkeyPaste
{
    public class MpLogFormController : MpControlController {
        public MpLogFormPanelController LogFormPanelController { get; set; }
        public MpDragTilePanelController DragTilePanelController { get; set; }

        public MpResizableBorderlessForm LogForm { get; set; }

        public MpController FocusedController { get; set; } = null;

        private bool _isFirstLoad = true;
        private bool _canResize = false;
        private bool _isResizing = false;
        private bool _isScrolling = false;
        private bool _isLMouseDown = false;

        private int _yDiff = 0;
        private int _yDiffThreshold = 20;
        
        public MpKeyboardHook _escHook;
        public IKeyboardMouseEvents _mouseHook;

        public MpLogFormController(MpController Parent) : base(Parent) {
            LogForm = new MpResizableBorderlessForm() {
                AutoSize = false,          
                AutoScaleMode = AutoScaleMode.Dpi,
                Bounds = GetBounds(),
                TransparencyKey = Color.Fuchsia,
                BackColor = Color.Fuchsia
            };
            LogForm.FormClosing += (sender, e) => {
                HideLogForm();
                e.Cancel = true;
            };
            LogForm.FormClosed += (sender, e) => {
                HideLogForm();
            };
            LogForm.Leave += (sender, e) => {
                HideLogForm();
            };
            LogForm.Deactivate += (sender, e) => {
                HideLogForm();
            };
            LogForm.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;           

            LogFormPanelController = new MpLogFormPanelController(this);
            LogForm.Controls.Add(LogFormPanelController.LogFormPanel);
            LogFormPanelController.TileChooserPanelController.StateChangedEvent += (s, e) => {
                if(e.NewState == MpTileChooserPanelStateType.Scrolling) {
                    _isScrolling = true;
                    DeactivateHotKeys();
                } else {
                    _isScrolling = false;
                    _isLMouseDown = false;
                    ActivateHotKeys();
                }
            };
            Update();
            ShowLogForm();
            HideLogForm();       
        }
        
        public override void Update() {
            LogForm.Bounds = GetBounds();

            LogFormPanelController.Update();
            LogForm.Invalidate();
        }
        #region HotKeys
        public void ActivateHotKeys() {
            ActivateEscKey();
            ActivateMouseListener();
        }
        public void DeactivateHotKeys() {
            DeactivateEscKey();
            DeactivateMouseListener();
        }
        public void ActivateMouseListener() {
            _mouseHook = Hook.GlobalEvents();
            _mouseHook.MouseMove += _mouseHook_MouseMove;
            _mouseHook.MouseDown += _mouseHook_MouseDown;
            _mouseHook.MouseUp += _mouseHook_MouseUp;
            _mouseHook.MouseClick += _mouseHook_MouseClick;
        }      

        public void DeactivateMouseListener() {
            if(_mouseHook == null) {
                return;
            }
            _mouseHook.Dispose();
            _mouseHook = null;
        }
        public void ActivateEscKey() {
            DeactivateEscKey();

            _escHook = new MpKeyboardHook();
            _escHook.RegisterHotKey(ModifierKeys.None, Keys.Escape);
            _escHook.KeyPressed += (s,e) => HideLogForm();
        }
        public void DeactivateEscKey() {
            if (_escHook == null) {
                return;
            }
            _escHook.UnregisterHotKey();
            _escHook.Dispose();
            _escHook = null;
        }
        #endregion
        #region events
        private void _mouseHook_MouseClick(object sender, MouseEventArgs e) {
            foreach (MpTilePanelController citc in LogFormPanelController.TileChooserPanelController.TileControllerList) {
                Rectangle tileRect = citc.TilePanel.RectangleToScreen(citc.TilePanel.ClientRectangle);
                if (tileRect.Contains(e.Location) || citc.TilePanel.ClientRectangle.Contains(e.Location)) {
                    LogFormPanelController.TileChooserPanelController.SelectedTilePanelController = citc;
                }
            }
            DragTilePanelController = null;
        }
        private void _mouseHook_MouseUp(object sender, MouseEventArgs e) {
            _isLMouseDown = false;
            if (_isResizing) {
                _isResizing = false;
            }
            if (DragTilePanelController != null) {
                DragTilePanelController.EndTileDrag();
                DragTilePanelController = null;
            }
        }
        private void _mouseHook_MouseDown(object sender, MouseEventArgs e) {
            _isLMouseDown = true;
            if (_canResize) {
                _isResizing = true;
            }
            else {
                _isResizing = false;
            }
            if (LogForm == null || !LogForm.Visible) {
                return;
            }

            var dragTileController = LogFormPanelController.TileChooserPanelController.GetTilePanelControllerFromTitleAtLocation(e.Location);
            if (dragTileController != null) {
                DragTilePanelController = new MpDragTilePanelController(this, dragTileController, e.Location);

            }
        }
        public void _mouseHook_MouseMove(object sender, MouseEventArgs e) {
            if (_isResizing) {
                //log form panel rect
                Rectangle lfpr = LogFormPanelController.LogFormPanel.Bounds;
                _yDiff = -(e.Location.Y - lfpr.Y);
                if (Math.Abs(_yDiff) >= _yDiffThreshold) {
                    if (LogFormPanelController.CustomHeight < LogFormPanelController.MinimumHeight) {
                        LogFormPanelController.CustomHeight = lfpr.Height;
                    }
                    LogFormPanelController.CustomHeight += _yDiff;
                    Update();
                    _yDiff = 0;
                }
            }
            //if a tile is being dragged
            else if (DragTilePanelController != null) {
                DragTilePanelController.ContinueTileDrag(e.Location);
            }
            else if(_isScrolling) {

            }
            //otherwise if no buttons down check if can resize logform panel
            else if(!_isLMouseDown) {
                Point mp = LogFormPanelController.LogFormPanel.PointToClient(e.Location);
                if (Math.Abs(mp.Y) < 20) {
                    LogForm.Cursor = Cursors.SizeNS;
                    _canResize = true;
                }
                else {
                    LogForm.Cursor = Cursors.Default;
                    _canResize = false;
                }
                //check if mouse hover over tiles
                foreach (MpTilePanelController citc in LogFormPanelController.TileChooserPanelController.TileControllerList) {
                    Rectangle tileRect = citc.TilePanel.RectangleToScreen(citc.TilePanel.ClientRectangle);
                    if (tileRect.Contains(e.Location) || citc.TilePanel.ClientRectangle.Contains(e.Location)) {
                        if (citc == LogFormPanelController.TileChooserPanelController.SelectedTilePanelController) {
                            citc.SetState(MpTilePanelStateType.HoverSelected);
                        }
                        else {
                            citc.SetState(MpTilePanelStateType.HoverUnselected);
                        }
                    }
                    else if (citc == LogFormPanelController.TileChooserPanelController.SelectedTilePanelController) {
                        citc.SetState(MpTilePanelStateType.Selected);
                    }
                    else {
                        citc.SetState(MpTilePanelStateType.Unselected);
                    }
                }
            }
        }
        #endregion
        public void ShowLogForm() {
            if(LogForm.Visible) {
                return;
            }
            if(_isFirstLoad) {
                Update();
                _isFirstLoad = false;
            }
            LogForm.Show();
            LogForm.Visible = true;
            LogForm.Activate();
            ActivateHotKeys();
        }
        public void HideLogForm() {
            LogForm.Hide();
            LogForm.Visible = false;
            DeactivateHotKeys();
        }
        public void CloseLogForm() {
            //DeactivateHotKeys();
            //_clickHook.Dispose();
            //_moveHook.Dispose();
            LogForm.Close();
            LogForm = null;
        }
        public override Rectangle GetBounds() {
           return MpSingleton.Instance.ScreenManager.GetScreenWorkingAreaWithMouse();
        }
        
        private void LogForm_Resize(object sender, EventArgs e) {
            Update();
        }
        //private void _rightHook_KeyPressed(object sender, KeyPressedEventArgs e) {
        //    LogFormPanelController.TileChooserPanelController.SelectNextTile();
        //}
        //private void _leftHook_KeyPressed(object sender, KeyPressedEventArgs e) {
        //    LogFormPanelController.TileChooserPanelController.SelectPreviousTile();
        //}

        //private void ClipboardController_ClipboardChangedEvent(object sender,MpCopyItem copyItem) {
        //    LogFormPanelController.TileChooserPanelController.AddNewCopyItemPanel(copyItem);
        //    LogFormPanelController.TileChooserPanelController.SelectTile(LogFormPanelController.TileChooserPanelController.TileControllerList[LogFormPanelController.TileChooserPanelController.TileControllerList.Count - 1]);
        //}


        //public void ActivateArrowKeys() {
        //    if(_rightHook == null) {
        //        _rightHook = new MpKeyboardHook();
        //        _rightHook.RegisterHotKey(ModifierKeys.None,Keys.Right);
        //        _rightHook.KeyPressed += _rightHook_KeyPressed;
        //    }
        //    if(_leftHook == null) {
        //        _leftHook = new MpKeyboardHook();
        //        _leftHook.RegisterHotKey(ModifierKeys.None,Keys.Left);
        //        _leftHook.KeyPressed += _leftHook_KeyPressed;
        //    }
        //}
        //public void DeactivateArrowKeys() {
        //    if(_rightHook != null) {
        //        _rightHook.UnregisterHotKey();
        //        _rightHook.Dispose();
        //        _rightHook = null;
        //    }
        //    if(_leftHook != null) {
        //        _leftHook.UnregisterHotKey();
        //        _leftHook.Dispose();
        //        _leftHook = null;
        //    }
        //}
        //public void ActivateEnterKey() {
        //    if(_enterHook == null) {
        //        _enterHook = new MpKeyboardHook();
        //        _enterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
        //        _enterHook.KeyPressed += EnterHook_KeyPressed;
        //    }
        //}
        //public void DeactivateEnterKey() {
        //    if(_enterHook != null) {
        //        _enterHook.UnregisterHotKey();
        //        _enterHook.Dispose();
        //        _enterHook = null;
        //    }
        //}
        //public void DeactivateEscKey() {
        //    if(_escHook != null) {
        //        _escHook.UnregisterHotKey();
        //        _escHook.Dispose();
        //        _escHook = null;
        //    }
        //}
        //public void ActivateHotKeys() {
        //    ActivateEnterKey();
        //    ActivateEscKey();
        //    ActivateArrowKeys();
        //    if(_mouseDownHook == null) {
        //        _mouseDownHook = Hook.GlobalEvents();
        //        _mouseDownHook.MouseDown += _mouseDownHook_MouseDown;
        //    }
        //    if(_mouseUpHook == null) {
        //        _mouseUpHook = Hook.GlobalEvents();
        //        _mouseUpHook.MouseUp += _upHook_MouseUp;
        //    }
        //}      

        //public void DeactivateHotKeys() {
        //    DeactivateEscKey();
        //    DeactivateArrowKeys();
        //    if(_mouseDownHook != null) {
        //        _mouseDownHook.Dispose();
        //        _mouseDownHook = null;
        //    }
        //    if(_mouseUpHook != null) {
        //        _mouseUpHook.Dispose();
        //        _mouseUpHook = null;
        //    }
        //}


        //private void EscHook_KeyPressed(object sender,KeyPressedEventArgs e) {
        //    if(LogForm.Visible) {
        //        HideLogForm();
        //        return;
        //    }
        //}
        //private void _toggleLogHook_KeyPressed(object sender,KeyPressedEventArgs e) {
        //    ToggleLogVisibility();
        //}
        //private void EnterHook_KeyPressed(object sender,)KeyPressedEventArgs e) {
        //    HideLogForm();
        //    MpSingletonController.Instance.ClipboardController.PasteCopyItem(LogFormPanelController.TileChooserPanelController.SelectedTilePanelController.CopyItem);
        //}


        //public void ToggleLogForm() {
        //    if(LogForm.Visible) {
        //        HideLogForm();
        //    } else {
        //        ShowLogForm();
        //    }
        //}
    }
}
