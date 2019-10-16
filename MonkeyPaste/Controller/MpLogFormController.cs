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

        public Rectangle sb;
        private MpClipboardHelper _clipboardController;
        private MpLogForm _logForm;

        private MpCopyItemTileChooserPanelController _copyItemTileChooserController;

        private MpKeyboardHook _toggleLogHook;
        private MpKeyboardHook _escHook;
        private MpKeyboardHook _enterHook, _spaceHook;

        private int _customHeight = 0;
        private bool _isVisible = false;
        private bool _isInit = true;
        private IKeyboardMouseEvents _clickHook;

        public MpLogFormController(MpController parentController) : base(parentController) {
            _clickHook = Hook.GlobalEvents();
            _clickHook.MouseClick += _clickHook_MouseClick;

            _logForm = new MpLogForm() {
                AutoSize = false,
                AutoScaleMode = AutoScaleMode.None
                //Visible = false,
                //Opacity = 0
            };
            
            _logForm.Load += LogForm_Load;
            _logForm.FormClosing += logForm_Closing;
            _logForm.FormClosed += logForm_Closed;
            _logForm.Leave += LogForm_Leave;
            _logForm.Deactivate += LogForm_Leave;
            _logForm.Resize += _logForm_Resize;
            _logForm.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            _toggleLogHook = new MpKeyboardHook();
            _toggleLogHook.KeyPressed += _toggleLogHook_KeyPressed;
            _toggleLogHook.RegisterHotKey(ModifierKeys.Control,Keys.D);
            _toggleLogHook.RegisterHotKey(ModifierKeys.None,Keys.CapsLock);
            MpSingletonController.Instance.SetKeyboardHook(MpInputCommand.ToggleLog,_toggleLogHook);
            
            _copyItemTileChooserController = new MpCopyItemTileChooserPanelController(this);
            _logForm.Controls.Add(_copyItemTileChooserController.GetCopyItemPanel());
            UpdateLogFormBounds();
            _logForm.Show();
            _logForm.Hide();
        }

        private void _clickHook_MouseClick(object sender,MouseEventArgs e) {
            if(!_isVisible) {
                return;
            }
            MpCopyItemTileController clickedTileController = null;
            foreach(MpCopyItemTileController citc in _copyItemTileChooserController.CopyItemTileControllerList) {
                Rectangle tileRect = citc.CopyItemTilePanel.RectangleToScreen(citc.CopyItemTilePanel.ClientRectangle);
                if(tileRect.Contains(e.Location) || citc.CopyItemTilePanel.ClientRectangle.Contains(e.Location)) {
                    clickedTileController = citc;
                }
            }
            if(clickedTileController != null) {
                _copyItemTileChooserController.SelectedCopyItemTileController = clickedTileController;
            }
        }

        #region Events
        private void LogForm_Load(object sender,EventArgs e) {
            UpdateLogFormBounds();

            ShowLogForm();
            _clipboardController = new MpClipboardHelper();
            _clipboardController.Init();
            HideLogForm();
            _isInit = false;
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
           // _spaceHook.RegisterHotKey(ModifierKeys.None,Keys.Space);
            //MpSingletonController.Instance.SetKeyboardHook(MpInputCommand.EditTile,_spaceHook);

            _copyItemTileChooserController.ActivateHotKeys();
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
            _copyItemTileChooserController.DeactivateHotKeys();
        }
        private void _escHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(_logForm.Visible) {
                HideLogForm();
                return;
            }
        }
        private void _toggleLogHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(_logForm.Visible) {
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
        private void _logForm_Resize(object sender,EventArgs e) {
            _customHeight = _logForm.Bounds.Height;
            if(_copyItemTileChooserController != null) {
                _copyItemTileChooserController.OnFormResize(_logForm.Bounds);
            }
            
        }       
        public MpCopyItemTileChooserPanelController GetCopyItemPanelController() {
            return _copyItemTileChooserController;
        }
        public MpLogForm GetLogForm() {
            return _logForm;
        }
        public void UpdateLogFormBounds() {
            //Process.GetCurrentProcess().Refresh();
            sb = MpHelperSingleton.Instance.GetScreenBoundsWithMouse();
            int h = _customHeight > 0 ? _customHeight : (int)((float)sb.Height * (float)MpSingletonController.Instance.GetSetting("LogScreenHeightRatio"));

            _customHeight = h;

            _logForm.Bounds = new Rectangle(0,sb.Height - h,sb.Width,h);
            _logForm.ClientSize = _logForm.Bounds.Size;
            _copyItemTileChooserController.UpdatePanelBounds(_logForm.Bounds);
        }
        public void ShowLogForm() {
            _logForm.Show();
            _logForm.Activate();
            if(!_isInit) {
                _logForm.Opacity = 100;
                _logForm.Visible = true;
            }
            ActivateHotKeys();
            _isVisible = true;
        }
        public void HideLogForm() {
            _logForm.Hide();
            _logForm.Visible = false;
            DeactivateHotKeys();
            _isVisible = false;
        }
        public void ToggleLogForm() {
            if(_isVisible) {
                HideLogForm();
            } else {
                ShowLogForm();
            }
        }
        public void CloseLogForm() {
            _logForm.Close();
            _logForm = null;
        }
        
        public void PasteCopyItem() {
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
            MpCopyItem copyItem = _copyItemTileChooserController.GetSelectedCopyItem();

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
            SetActiveWindow(_clipboardController.GetLastWindowWatcher().LastHandle);
            SendKeys.Send("^v");

            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);
        }

        public override void UpdateBounds() {
            throw new NotImplementedException();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            
        }
    }
}
