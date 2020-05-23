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
    public class MpLogFormController : MpController {
        public MpLogFormPanelController LogFormPanelController { get; set; }
        public MpDragTilePanelController DragTilePanelController { get; set; }

        public MpResizableBorderlessForm LogForm { get; set; }

        public MpController FocusedController { get; set; } = null;

        public MpKeyboardHook _escHook,_enterHook;
        public IKeyboardMouseEvents _mouseHook;

        private bool _isFirstLoad = true;
        private bool _canResize = false;
        private bool _isResizing = false;

        private int _yDiff = 0;
        private int _yDiffThreshold = 20;


        public MpLogFormController(MpController Parent) : base(Parent) {
            LogForm = new MpResizableBorderlessForm() {
                AutoSize = false,          
                AutoScaleMode = AutoScaleMode.Dpi,
                Bounds = MpScreenManager.Instance.GetScreenBoundsWithMouse(),
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

            _mouseHook = Hook.GlobalEvents();
            _mouseHook.MouseClick += _clickHook_MouseClick;
            _mouseHook.MouseDown += _mouseDownHook_MouseDown;
            _mouseHook.MouseUp += _upHook_MouseUp;
            _mouseHook.MouseMove += _moveHook_MouseMove;

            Update();
            ShowLogForm();
            HideLogForm();       
        }
        #region HotKeys
        public void ActivateHotKeys() {
            ActivateEscKey();
            ActivateEnterKey();
        }
        public void DeactivateHotKeys() {
            DeactivateEscKey();
            DeactivateEnterKey();
        }
        public void ActivateEscKey() {
            DeactivateEscKey();

            _escHook = new MpKeyboardHook();
            _escHook.RegisterHotKey(ModifierKeys.None, Keys.Escape);
            _escHook.KeyPressed += _escHook_KeyPressed;
        }
        public void DeactivateEscKey() {
            if(_escHook == null) {
                return;
            }
            _escHook.UnregisterHotKey();
            _escHook.Dispose();
            _escHook = null;
        }
        public void ActivateEnterKey() {
            if (_enterHook == null) {
                _enterHook = new MpKeyboardHook();
                _enterHook.RegisterHotKey(ModifierKeys.None, Keys.Enter);
                _enterHook.KeyPressed += _enterHook_KeyPressed;
            }
        }
        public void DeactivateEnterKey() {
            if (_enterHook == null) {
                return;
            }
            _enterHook.UnregisterHotKey();
            _enterHook.Dispose();
            _enterHook = null;
        }
        public override void Update() {
            LogForm.Bounds = MpScreenManager.Instance.GetScreenBoundsWithMouse();
            LogFormPanelController.Update();
            LogForm.Invalidate();
        }
        #endregion

        #region Events
        private void _escHook_KeyPressed(object sender, KeyPressedEventArgs e) {
            HideLogForm();
        }
        private void _enterHook_KeyPressed(object sender, KeyPressedEventArgs e) {
            MpCommandManager.Instance.ClipboardCommander.PasteCopyItem(LogFormPanelController.TileChooserPanelController.SelectedTilePanelController.CopyItem);

        }
        public void _moveHook_MouseMove(object sender,MouseEventArgs e) {
            //check if mouse at screen bounds to reveal log
            if(LogForm == null || !LogForm.Visible) {
                Rectangle sb = MpScreenManager.Instance.GetScreenBoundsWithMouse();
                Rectangle hr = new Rectangle(0,0,sb.Width,10);
                if(e.Location.Y < 5) {
                    ShowLogForm();
                }
            } else {
                Point mp = LogFormPanelController.LogFormPanel.PointToClient(e.Location);
                if(_isResizing) {
                    Rectangle lfpr = LogFormPanelController.LogFormPanel.Bounds;
                    //LogFormPanelController.LogFormPanel.Visible = false;
                    _yDiff = -(e.Location.Y - lfpr.Y);
                    if(Math.Abs(_yDiff) >= _yDiffThreshold) {
                        if(LogFormPanelController.CustomHeight < LogFormPanelController.MinimumHeight) {
                            LogFormPanelController.CustomHeight = lfpr.Height;
                        }
                        LogFormPanelController.CustomHeight += _yDiff;
                        ///Console.WriteLine("Ydiff: " + yDiff);
                        //LogFormPanelController.LogFormPanel.Bounds = new Rectangle(lfpr.X,lfpr.Y + yDiff,lfpr.Width,lfpr.Height + yDiff);
                        Update();
                        _yDiff = 0;
                    }
                }
                //if a tile is being dragged
                else if(DragTilePanelController != null) {
                    DragTilePanelController.ContinueTileDrag(e.Location);
                } else {
                    //LogFormPanelController.LogFormPanel.Visible = true;
                    //Rectangle hr = new Rectangle(0,lfpsr.Y - 5,lfpsr.Width,lfpsr.Y + 5);
                    if(Math.Abs(mp.Y) < 20) {
                        LogForm.Cursor = Cursors.SizeNS;
                        _canResize = true;
                    }
                    else {
                        LogForm.Cursor = Cursors.Default;
                        _canResize = false;
                    }
                }
                //check if mouse hover over tiles
                foreach(MpTilePanelController citc in LogFormPanelController.TileChooserPanelController.TileControllerList) {
                    Rectangle itemControlRect = citc.TilePanel.RectangleToScreen(citc.TileControlController.ItemPanel.ClientRectangle);
                    if(itemControlRect.Contains(e.Location) || citc.TilePanel.ClientRectangle.Contains(e.Location)) {
                        //citc.TileControlController.TraverseItem(citc.TileControlController.ItemPanel.PointToClient(e.Location));
                        citc.SetState(MpTilePanelStateType.Hover);
                    } else if(citc != LogFormPanelController.TileChooserPanelController.SelectedTilePanelController) {
                        citc.SetState(MpTilePanelStateType.Unselected);
                    } else {
                        citc.SetState(MpTilePanelStateType.Selected);
                    }
                }
            }
        }        
        public void _clickHook_MouseClick(object sender,MouseEventArgs e) {
            if(LogForm == null || !LogForm.Visible) {
                return;
            }
            MpTilePanelController clickedTileController = LogFormPanelController.TileChooserPanelController.GetTilePanelControllerAtLocation(e.Location);
            if(clickedTileController != null) {
                LogFormPanelController.TileChooserPanelController.SelectTile(clickedTileController);                
            }
        }
        private void _upHook_MouseUp(object sender,MouseEventArgs e) {
            if(_isResizing) {
                _isResizing = false;
            }
            if(DragTilePanelController != null) {
                DragTilePanelController.EndTileDrag();
                DragTilePanelController = null;
            }
        }
        private void _mouseDownHook_MouseDown(object sender,MouseEventArgs e) {
            if(_canResize) {
                _isResizing = true;
            }
            else {
                _isResizing = false;
            }
            if(LogForm == null || !LogForm.Visible) {
                return;
            }
            
            var dragTileController = LogFormPanelController.TileChooserPanelController.GetTilePanelControllerAtLocation(e.Location);
            if(dragTileController != null) {
                DragTilePanelController = new MpDragTilePanelController(this, dragTileController, e.Location);
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
        public bool IsVisible() {
            return LogForm.Visible;
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
        //private void LogForm_Resize(object sender,EventArgs e) {
        //    //if(_isResizing == false) {
        //    //    _isResizing = true;
        //    //    LogFormPanelController.TileChooserPanelController.HideTiles();
        //    //}
        //    //MpSingletonController.Instance.CustomLogHeight = LogForm.Bounds.Height;

        //    Update();
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
