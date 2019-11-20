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

namespace MonkeyPaste {
    public class MpLogFormController : MpController {
        public static MpDb Db { get; set; }

        public MpClipboardHelper ClipboardController { get; set; }

        public MpLogForm LogForm { get; set; }

        public MpTileChooserPanelController TileChooserPanelController { get; set; }

        public MpLogMenuPanelController LogMenuPanelController { get; set; }

        public static MpKeyboardHook EnterHook { get; set; }
        private MpKeyboardHook _toggleLogHook,_escHook, _spaceHook;
        private IKeyboardMouseEvents _clickHook,_moveHook;

        private bool _isFirstLoad = true;
        private bool _isResizing = false;
       // private int _customHeight = 0;

        public MpLogFormController(MpController Parent,string dbPath,string dbPassword) : base(Parent) {
            Db = new MpDb(dbPath,dbPassword,null,null);

            LogForm = new MpLogForm() {
                AutoSize = false,                
                AutoScaleMode = AutoScaleMode.Dpi,
                MinimumSize = new Size(15,200)                
            };
            LogForm.Load += LogForm_Load;
            LogForm.FormClosing += logForm_Closing;
            LogForm.FormClosed += logForm_Closed;
            LogForm.Leave += LogForm_Leave;
            LogForm.Deactivate += LogForm_Leave;
            LogForm.Resize += LogForm_Resize;
            LogForm.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            //these events do not get deactivated
            _clickHook = Hook.GlobalEvents();
            _clickHook.MouseUp += _clickHook_MouseClick;
            _moveHook = Hook.GlobalEvents();
            _moveHook.MouseMove += _moveHook_MouseMove;

            _toggleLogHook = new MpKeyboardHook();
            _toggleLogHook.KeyPressed += _toggleLogHook_KeyPressed;
            _toggleLogHook.RegisterHotKey(ModifierKeys.Control,Keys.D);
            //_toggleLogHook.RegisterHotKey(ModifierKeys.None,Keys.CapsLock);

            LogMenuPanelController = new MpLogMenuPanelController(this);
            LogMenuPanelController.LogMenuSearchTextBoxController.SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            LogForm.Controls.Add(LogMenuPanelController.LogMenuPanel);

            TileChooserPanelController = new MpTileChooserPanelController(this,Db.GetCopyItems());
            LogForm.Controls.Add(TileChooserPanelController.TileChooserPanel);
                       
            //Update();
            //LogForm.Show();
            //LogForm.Hide();
            Link(new List<MpIView> { LogForm });            
        }

        public override void Update() {
            //current screen rect
            Rectangle sr = MpHelperSingleton.Instance.GetScreenBoundsWithMouse();
            
            int h = _isFirstLoad ? (int)((float)sr.Height * Properties.Settings.Default.LogScreenHeightRatio) : LogForm.Height;
            //MpSingletonController.Instance.CustomLogHeight = h;

            LogForm.SetBounds(0,sr.Height - h,sr.Width,h);

            if(TileChooserPanelController != null) {
                TileChooserPanelController.Update();
                
            }
            if(LogMenuPanelController != null) {
                LogMenuPanelController.Update();
            }

            LogForm.Invalidate();
        }

        #region Events
        private void _moveHook_MouseMove(object sender,MouseEventArgs e) {
            if(LogForm == null || !LogForm.Visible) {
                return;
            }
            foreach(MpTilePanelController citc in TileChooserPanelController.TileControllerList) {
                Rectangle itemControlRect = citc.TilePanel.RectangleToScreen(citc.TileControlController.ItemPanel.ClientRectangle);
                if(itemControlRect.Contains(e.Location) || citc.TilePanel.ClientRectangle.Contains(e.Location)) {
                    citc.TileControlController.TraverseItem(citc.TileControlController.ItemPanel.PointToClient(e.Location));
                }
            }

        }
        private void SearchTextBox_TextChanged(object sender,EventArgs e) {
            string searchText = LogMenuPanelController.LogMenuSearchTextBoxController.SearchTextBox.Text;
            TileChooserPanelController.FilterTiles(searchText);
        }
        private void _clickHook_MouseClick(object sender,MouseEventArgs e) {
            if(LogForm == null || !LogForm.Visible) {
                return;
            }
            MpTilePanelController clickedTileController = null;
            foreach(MpTilePanelController citc in TileChooserPanelController.TileControllerList) {
                Rectangle tileRect = citc.TilePanel.RectangleToScreen(citc.TilePanel.ClientRectangle);
                if(tileRect.Contains(e.Location) || citc.TilePanel.ClientRectangle.Contains(e.Location)) {
                    clickedTileController = citc;
                }
            }
            if(clickedTileController != null) {
                TileChooserPanelController.SelectedTilePanelController = clickedTileController;
                if(TileChooserPanelController.SelectedTilePanelController.TileTitlePanelController.TileTitleTextBoxController.TileTitleTextBox.ReadOnly) {
                    TileChooserPanelController.SelectedTilePanelController.TilePanel.Focus();
                }                
                TileChooserPanelController.SelectedTileBorderPanelController.TileBorderPanel.Visible = true;
                TileChooserPanelController.Update();
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
            TileChooserPanelController.AddNewCopyItemPanel(copyItem);
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
            if(_escHook != null) {
                DeactivateHotKeys();
            }
            _escHook = new MpKeyboardHook();
            _escHook.KeyPressed += _escHook_KeyPressed;
            _escHook.RegisterHotKey(ModifierKeys.None,Keys.Escape);

            if(EnterHook == null) {
                EnterHook = new MpKeyboardHook();
                EnterHook.KeyPressed += EnterHook_KeyPressed;
            }
            if(!EnterHook.IsRegistered()) {
                EnterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
            }

            _spaceHook = new MpKeyboardHook();
            _spaceHook.KeyPressed += _spaceHook_KeyPressed;
            TileChooserPanelController.ActivateHotKeys();
        }
        public void DeactivateHotKeys() {
            if(_escHook != null) {
                _escHook.UnregisterHotKey();
                _escHook = null;
            }
            if(EnterHook != null) {
                EnterHook.UnregisterHotKey();
                EnterHook = null;
            }
            if(_spaceHook != null) {
                _spaceHook.UnregisterHotKey();
                _spaceHook = null;
            }
            TileChooserPanelController.DeactivateHotKeys();
        }
        public void ToggleLogVisibility() {
            if(LogForm.Visible) {
                HideLogForm();
                return;
            }
            ShowLogForm();
        }
        private void _escHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(LogForm.Visible) {
                HideLogForm();
                return;
            }
        }
        private void _toggleLogHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            ToggleLogVisibility();
        }
        public void EnterHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            HideLogForm();
            PasteCopyItem();
        }
        private void _spaceHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            _toggleLogHook_KeyPressed(null,null);
            PasteCopyItem();
        }
        private void LogForm_Resize(object sender,EventArgs e) {
            //if(_isResizing == false) {
            //    _isResizing = true;
            //    TileChooserPanelController.HideTiles();
            //}
            //MpSingletonController.Instance.CustomLogHeight = LogForm.Bounds.Height;
            
            Update();
        }       
        public void ShowLogForm() {
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
            _clickHook.Dispose();
            _moveHook.Dispose();
            LogForm.Close();
            LogForm = null;
        }
        
        public void PasteCopyItem() {
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
            MpCopyItem copyItem = TileChooserPanelController.SelectedTilePanelController.CopyItem;

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
            WinApi.SetActiveWindow(ClipboardController.GetLastWindowWatcher().LastHandle);
            SendKeys.Send("^v");

            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);

            //only create to write to db
            MpPasteHistory pasteHistory = new MpPasteHistory(copyItem,ClipboardController.GetLastWindowWatcher().LastHandle);

            MpSingletonController.Instance.AppendItem = null;
        }        
    }
}
