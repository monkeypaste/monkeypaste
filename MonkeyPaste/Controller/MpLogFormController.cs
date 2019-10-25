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
        [DllImport("user32.dll")]
        static extern bool SetActiveWindow(IntPtr hWnd);

        public MpClipboardHelper ClipboardController { get; set; }

        public MpLogForm LogForm { get; set; }

        public MpTileChooserPanelController TileChooserPanelController { get; set; }

        private MpKeyboardHook _toggleLogHook,_escHook,_enterHook, _spaceHook;
        private IKeyboardMouseEvents _clickHook;

        private int _customHeight = 0;

        public MpLogFormController(MpController parentController) : base(parentController) {
            LogForm = new MpLogForm() {
                AutoSize = false,                
                AutoScaleMode = AutoScaleMode.None,
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
            _clickHook.MouseClick += _clickHook_MouseClick;

            _toggleLogHook = new MpKeyboardHook();
            _toggleLogHook.KeyPressed += _toggleLogHook_KeyPressed;
            _toggleLogHook.RegisterHotKey(ModifierKeys.Control,Keys.D);
            _toggleLogHook.RegisterHotKey(ModifierKeys.None,Keys.CapsLock);

            UpdateBounds();

            TileChooserPanelController = new MpTileChooserPanelController(this);
            LogForm.Controls.Add(TileChooserPanelController.TileChooserPanel);
            
            LogForm.Show();
            LogForm.Hide();
        }

        public override void UpdateBounds() {
            //current screen rect
            Rectangle sr = MpHelperSingleton.Instance.GetScreenBoundsWithMouse();
            int h = _customHeight > 0 ? _customHeight : (int)((float)sr.Height * (float)MpSingletonController.Instance.GetSetting("LogScreenHeightRatio"));
            _customHeight = h;

            LogForm.SetBounds(0,sr.Height - h,sr.Width,h);

            if(TileChooserPanelController != null) {
                TileChooserPanelController.UpdateBounds();
            }
        }

        private void _clickHook_MouseClick(object sender,MouseEventArgs e) {
            if(!LogForm.Visible) {
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
                TileChooserPanelController.SelectedTileController = clickedTileController;
            }
        }

        #region Events
        private void LogForm_Load(object sender,EventArgs e) {
            UpdateBounds();

            ShowLogForm();
            ClipboardController = new MpClipboardHelper();
            ClipboardController.Init();
            HideLogForm();
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
            MpSingletonController.Instance.SetKeyboardHook(MpInputCommand.HideLog,_escHook);

            _enterHook = new MpKeyboardHook();
            _enterHook.KeyPressed += _enterHook_KeyPressed;
            _enterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
            //_enterHook.RegisterHotKey(ModifierKeys.None,Keys.Space);
            MpSingletonController.Instance.SetKeyboardHook(MpInputCommand.PasteTile,_enterHook);

            _spaceHook = new MpKeyboardHook();
            _spaceHook.KeyPressed += _spaceHook_KeyPressed;
            TileChooserPanelController.ActivateHotKeys();
        }
        public void DeactivateHotKeys() {
            if(_escHook != null) {
                _escHook.UnregisterHotKey();
                _escHook = null;
            }
            if(_enterHook != null) {
                _enterHook.UnregisterHotKey();
                _enterHook = null;
            }
            if(_spaceHook != null) {
                _spaceHook.UnregisterHotKey();
                _spaceHook = null;
            }
            TileChooserPanelController.DeactivateHotKeys();
        }
        private void _escHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(LogForm.Visible) {
                HideLogForm();
                return;
            }
        }
        private void _toggleLogHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(LogForm.Visible) {
                HideLogForm();
                return;
            }
            ShowLogForm();
        }
        private void _enterHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            _toggleLogHook_KeyPressed(null,null);
            PasteCopyItem();
        }
        private void _spaceHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            _toggleLogHook_KeyPressed(null,null);
            PasteCopyItem();
        }
        private void LogForm_Resize(object sender,EventArgs e) {            
            _customHeight = LogForm.Bounds.Height;
            
            if(TileChooserPanelController != null) {
                TileChooserPanelController.OnFormResize(LogForm.Bounds);
            }
            
        }       
        public MpTileChooserPanelController GetCopyItemPanelController() {
            return TileChooserPanelController;
        }
        public MpLogForm GetLogForm() {
            return LogForm;
        }
        public void ShowLogForm() {
            LogForm.Show();
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
            LogForm.Close();
            LogForm = null;
        }
        
        public void PasteCopyItem() {
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
            MpCopyItem copyItem = TileChooserPanelController.GetSelectedCopyItem();

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
            SetActiveWindow(ClipboardController.GetLastWindowWatcher().LastHandle);
            SendKeys.Send("^v");

            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);

            copyItem.CopyCount++;
            MpSingletonController.Instance.GetMpData().UpdateMpCopyItem(copyItem);
        }

        

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            
        }
    }
}
