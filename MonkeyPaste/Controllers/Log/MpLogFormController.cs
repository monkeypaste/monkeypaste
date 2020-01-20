using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using VisualEffects;
using VisualEffects.Animations.Effects;
using VisualEffects.Easing;

namespace MonkeyPaste {
    public class MpLogFormController : MpController {
        public static MpDb Db { get; set; }

        public MpClipboardHelper ClipboardController { get; set; }

        public MpLogFormPanelController LogFormPanelController { get; set; }

        public MpLogForm LogForm { get; set; }        

        private MpKeyboardHook _toggleLogHook,_escHook, _enterHook,_leftHook,_rightHook;
        private IKeyboardMouseEvents _clickHook,_moveHook,_mouseDownHook,_mouseUpHook;

        private bool _isFirstLoad = true;
        private bool _canResize = false;
        private bool _isResizing = false;
       // private int _customHeight = 0;

        public MpLogFormController(MpController Parent,string dbPath,string dbPassword) : base(Parent) {
            Db = new MpDb(dbPath,dbPassword,null,null);

            LogForm = new MpLogForm() {
                AutoSize = false,                
                AutoScaleMode = AutoScaleMode.Dpi,
                MinimumSize = new Size(15,200),
                TransparencyKey = Color.Fuchsia,
                BackColor = Color.Fuchsia
            };
            //Update(true);
            LogForm.Load += LogForm_Load;
            //LogForm.Activated += LogForm_Activated;
            LogForm.FormClosing += logForm_Closing;
            LogForm.FormClosed += logForm_Closed;
            LogForm.Leave += LogForm_Leave;
            LogForm.Deactivate += LogForm_Leave;
            LogForm.Resize += LogForm_Resize;
            LogForm.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            
            LogFormPanelController = new MpLogFormPanelController(this);
            LogForm.Controls.Add(LogFormPanelController.LogFormPanel);


            //these events do not get deactivated            
            _clickHook = Hook.GlobalEvents();
            _clickHook.MouseClick += _clickHook_MouseClick;

            _moveHook = Hook.GlobalEvents();
            _moveHook.MouseMove += _moveHook_MouseMove;

            _toggleLogHook = new MpKeyboardHook();
            _toggleLogHook.RegisterHotKey(ModifierKeys.Control,Keys.D);
            _toggleLogHook.KeyPressed += _toggleLogHook_KeyPressed;
                       
            Update();
            //LogForm.Show();
            //LogForm.Hide();
            Link(new List<MpIView> { LogForm });            
        }


        private void LogForm_Activated(object sender,EventArgs e) {
            int t = 500;
            int d = 50;

            if(LogFormPanelController.TileChooserPanelController.TileControllerList.Count > 0) {
                int fy = LogFormPanelController.TileChooserPanelController.SelectedTileBorderPanelController.TileBorderPanel.Location.Y;
                LogFormPanelController.TileChooserPanelController.SelectedTileBorderPanelController.TileBorderPanel.Location = new Point(LogFormPanelController.TileChooserPanelController.SelectedTileBorderPanelController.TileBorderPanel.Location.X,LogFormPanelController.TileChooserPanelController.TileChooserPanel.Bottom);
                LogFormPanelController.TileChooserPanelController.SelectedTileBorderPanelController.TileBorderPanel.Invalidate();
                int idx = LogFormPanelController.TileChooserPanelController.TileControllerList.IndexOf(LogFormPanelController.TileChooserPanelController.SelectedTilePanelController);
                LogFormPanelController.TileChooserPanelController.SelectedTileBorderPanelController.TileBorderPanel.Animate(new YLocationEffect(),EasingFunctions.SineEaseOut,fy,t,idx*d);
            }
            foreach(MpTilePanelController tpc in LogFormPanelController.TileChooserPanelController.GetVisibleTilePanelControllerList()) {
                int fy = tpc.TilePanel.Location.Y;
                tpc.TilePanel.Location = new Point(tpc.TilePanel.Location.X,LogFormPanelController.TileChooserPanelController.TileChooserPanel.Bottom);
                tpc.TilePanel.Invalidate();
                int idx = LogFormPanelController.TileChooserPanelController.TileControllerList.IndexOf(tpc);
                tpc.AnimateTileY(fy,t,idx * d,EasingFunctions.SineEaseOut);
            }
        }

        public override void Update() {
            LogForm.Bounds = MpHelperSingleton.Instance.GetScreenBoundsWithMouse();

            LogFormPanelController.Update();
            LogForm.Invalidate();
        }

        #region Events
        public void _moveHook_MouseMove(object sender,MouseEventArgs e) {
            if(LogForm == null || !LogForm.Visible) {
                Rectangle sb = MpHelperSingleton.Instance.GetScreenBoundsWithMouse();
                Rectangle hr = new Rectangle(0,0,sb.Width,10);
                if(e.Location.Y < 5) {
                    ToggleLogVisibility();
                }
            } else {
                Rectangle lfpr = LogFormPanelController.LogFormPanel.Bounds;
                Rectangle lfpsr = LogFormPanelController.LogFormPanel.RectangleToScreen(lfpr);
                Point mp = LogFormPanelController.LogFormPanel.PointToClient(e.Location);
                if(_isResizing) {
                    int yDiff = -(e.Location.Y - lfpr.Y);
                    //Console.WriteLine("Ydiff: " + yDiff);
                    LogFormPanelController.LogFormPanel.Bounds = new Rectangle(lfpr.X,lfpr.Y + yDiff,lfpr.Width,lfpr.Height + yDiff);
                    Update();
                } else {
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

                foreach(MpTilePanelController citc in LogFormPanelController.TileChooserPanelController.TileControllerList) {
                    Rectangle itemControlRect = citc.TilePanel.RectangleToScreen(citc.TileControlController.ItemPanel.ClientRectangle);
                    if(itemControlRect.Contains(e.Location) || citc.TilePanel.ClientRectangle.Contains(e.Location)) {
                        citc.TileControlController.TraverseItem(citc.TileControlController.ItemPanel.PointToClient(e.Location));
                    }
                }
            }      
        }
        
        public void _clickHook_MouseClick(object sender,MouseEventArgs e) {
            if(LogForm == null || !LogForm.Visible) {
                return;
            }
            MpTilePanelController clickedTileController = null;
            foreach(MpTilePanelController citc in LogFormPanelController.TileChooserPanelController.TileControllerList) {
                Rectangle tileRect = citc.TilePanel.RectangleToScreen(citc.TilePanel.ClientRectangle);
                if(tileRect.Contains(e.Location) || citc.TilePanel.ClientRectangle.Contains(e.Location)) {
                    clickedTileController = citc;
                }
            }
            if(clickedTileController != null) {
                LogFormPanelController.TileChooserPanelController.SelectTile(clickedTileController);                
            }
        }

        private void LogForm_Load(object sender,EventArgs e) {
            Update();
            ClipboardController = new MpClipboardHelper();
            ClipboardController.ClipboardChangedEvent += ClipboardController_ClipboardChangedEvent;
            ClipboardController.Init();
            ShowLogForm();
        }

        private void ClipboardController_ClipboardChangedEvent(object sender,MpCopyItem copyItem) {
            LogFormPanelController.TileChooserPanelController.AddNewCopyItemPanel(copyItem);
        }

        private void logForm_Closing(object sender,FormClosingEventArgs e) {
            HideLogForm();
            e.Cancel = true;
        }
        private void logForm_Closed(object sender,EventArgs e) {
            HideLogForm();
        }
        private void LogForm_Leave(object sender,EventArgs e) {
            HideLogForm();
        }
        private void LogForm_Enter(object sender,EventArgs e) {

        }
        #endregion
        public void ActivateHotKeys() {
            if(_enterHook == null) {
                _enterHook = new MpKeyboardHook();
                _enterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
                _enterHook.KeyPressed += EnterHook_KeyPressed;
            }
            if(_escHook == null) {
                _escHook = new MpKeyboardHook();
                _escHook.RegisterHotKey(ModifierKeys.None,Keys.Escape);
                _escHook.KeyPressed += EscHook_KeyPressed;
            }
            if(_rightHook == null) {
                _rightHook = new MpKeyboardHook();
                _rightHook.RegisterHotKey(ModifierKeys.None,Keys.Right);
                _rightHook.KeyPressed += _rightHook_KeyPressed;
            }
            if(_leftHook == null) {
                _leftHook = new MpKeyboardHook();
                _leftHook.RegisterHotKey(ModifierKeys.None,Keys.Left);
                _leftHook.KeyPressed += _leftHook_KeyPressed;
            }
            if(_mouseDownHook == null) {
                _mouseDownHook = Hook.GlobalEvents();
                _mouseDownHook.MouseDown += _mouseDownHook_MouseDown;
            }
            if(_mouseUpHook == null) {
                _mouseUpHook = Hook.GlobalEvents();
                _mouseUpHook.MouseUp += _upHook_MouseUp;
            }
        }      

        public void DeactivateHotKeys() {
            if(_enterHook != null) {
                _enterHook.UnregisterHotKey();
                _enterHook.Dispose();
                _enterHook = null;
            }
            if(_escHook != null) {
                _escHook.UnregisterHotKey();
                _escHook.Dispose();
                _escHook = null;
            }
            if(_rightHook != null) {
                _rightHook.UnregisterHotKey();
                _rightHook.Dispose();
                _rightHook = null;
            }
            if(_leftHook != null) {
                _leftHook.UnregisterHotKey();
                _leftHook.Dispose();
                _leftHook = null;
            }
            if(_mouseDownHook != null) {
                _mouseDownHook.Dispose();
                _mouseDownHook = null;
            }
            if(_mouseUpHook != null) {
                _mouseUpHook.Dispose();
                _mouseUpHook = null;
            }
        }
        private void _upHook_MouseUp(object sender,MouseEventArgs e) {
            if(_isResizing) {
                _isResizing = false;
            }
        }
        private void _mouseDownHook_MouseDown(object sender,MouseEventArgs e) {
            if(_canResize) {
                _isResizing = true;
            }
            else {
                _isResizing = false;
            }
        }
        private void _rightHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            LogFormPanelController.TileChooserPanelController.SelectNextTile();
        }
        private void _leftHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            LogFormPanelController.TileChooserPanelController.SelectPreviousTile();
        }
        
        private void ToggleLogVisibility() {
            if(LogForm.Visible) {
                HideLogForm();
                return;
            }
            ShowLogForm();
        }
        private void EscHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(LogForm.Visible) {
                HideLogForm();
                return;
            }
        }
        private void _toggleLogHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            ToggleLogVisibility();
        }
        private void EnterHook_KeyPressed(object sender,KeyPressedEventArgs e) {

            HideLogForm();
            PasteCopyItem();
        }
        private void LogForm_Resize(object sender,EventArgs e) {
            //if(_isResizing == false) {
            //    _isResizing = true;
            //    LogFormPanelController.TileChooserPanelController.HideTiles();
            //}
            //MpSingletonController.Instance.CustomLogHeight = LogForm.Bounds.Height;
            
            Update();
        }       
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
        public void ToggleLogForm() {
            if(LogForm.Visible) {
                HideLogForm();
            } else {
                ShowLogForm();
            }
        }
        public void CloseLogForm() {
            DeactivateHotKeys();
            //_clickHook.Dispose();
            //_moveHook.Dispose();
            LogForm.Close();
            LogForm = null;
        }
        
        public void PasteCopyItem() {
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
            MpCopyItem copyItem = LogFormPanelController.TileChooserPanelController.SelectedTilePanelController.CopyItem;

            if(copyItem.copyItemTypeId == MpCopyItemType.Text) {
                System.Windows.Clipboard.SetData(DataFormats.Text,(string)copyItem.GetData());
            } else if(copyItem.copyItemTypeId == MpCopyItemType.RichText) {
                System.Windows.Clipboard.SetData(DataFormats.Text,(string)copyItem.GetData());
            }
            else if(copyItem.copyItemTypeId == MpCopyItemType.HTMLText) {
                System.Windows.Clipboard.SetData(DataFormats.Text,(string)copyItem.GetData());
            }
            else if(copyItem.copyItemTypeId == MpCopyItemType.Image) {
                System.Windows.Clipboard.SetImage((BitmapSource)copyItem.GetData());
            }
            else if(copyItem.copyItemTypeId == MpCopyItemType.FileList) {
                System.Windows.Clipboard.SetFileDropList((StringCollection)copyItem.GetData());
            }
            //WinApi.SetActiveWindow(ClipboardController.GetLastWindowWatcher().LastHandle);
            SendKeys.Send("^v");

            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);

            //only create to write to db

            MpPasteHistory pasteHistory = new MpPasteHistory(copyItem,ClipboardController.GetLastWindowWatcher().LastHandle);

            MpSingletonController.Instance.AppendItem = null;
        }        
    }
}
