using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace MonkeyPaste {
    public class MpCopyItemTileController:MpController {
        [DllImport("user32.dll")]
        static extern bool SetActiveWindow(IntPtr hWnd);

        public static int TotalTileCount = 0;

        private int _tileId { get; set; }
        public int TileId { get { return _tileId; } set { _tileId = value; } }

        private MpCopyItemTilePanel _copyItemTilePanel = null;
        public MpCopyItemTilePanel CopyItemTilePanel { get { return _copyItemTilePanel; } set { _copyItemTilePanel = value; } }

        private MpCopyItemTileTitlePanelController _copyItemTileTilePanelController { get; set; }
        public MpCopyItemTileTitlePanelController CopyItemTileTitlePanelController { get { return _copyItemTileTilePanelController; } set { _copyItemTileTilePanelController = value; } }

        private MpCopyItemControlController _copyItemControlController { get; set; }
        public MpCopyItemControlController CopyItemControlController { get { return _copyItemControlController; } set { _copyItemControlController = value; } }

        private Panel _overlayPanel { get; set; }
        public Panel OverlayPanel { get { return _overlayPanel; } set { _overlayPanel = value; } }

        private bool _hasFocus  {get;set;}
        public bool HasFocus { get { return _hasFocus; } set { _hasFocus = value; } }

        private bool _isEditable { get; set; }
        public bool IsEditable { get { return _isEditable; } set { _isEditable = value; } }

        private Color _tileColor;

        private MpCopyItem _copyItem { get; set; }
        public MpCopyItem CopyItem { get { return _copyItem; } set { _copyItem = value; } }

        private MpKeyboardHook _escKeyHook;
        private MpKeyboardHook _spaceKeyHook;

        public MpCopyItemTileController(int tileSize,MpCopyItem ci,Color tileColor,MpController parentController) : base(parentController) {
            TileId = ++TotalTileCount;
            IsEditable = false;
            HasFocus = false;
            IsEditable = ci.copyItemTypeId == MpCopyItemType.RichText || ci.copyItemTypeId == MpCopyItemType.Text;
            _copyItem = ci;

            _tileColor = tileColor;

            _escKeyHook = new MpKeyboardHook();
            _escKeyHook.KeyPressed += _escKeyHook_KeyPressed;
            _spaceKeyHook = new MpKeyboardHook();
            _spaceKeyHook.KeyPressed += _spaceKeyHook_KeyPressed;

            CopyItemTilePanel = new MpCopyItemTilePanel() {
                AutoScroll = true,
                AutoSize = true,
                //FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
                BackColor = _tileColor,
                Size = new System.Drawing.Size(tileSize,tileSize),
                //Padding = new Padding((int)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePad")),
                //Anchor = AnchorStyles.Left
            };
            CopyItemTilePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            CopyItemTileTitlePanelController = new MpCopyItemTileTitlePanelController(tileSize,ci,tileColor,this);
            CopyItemTilePanel.Controls.Add(CopyItemTileTitlePanelController.CopyItemTileTitlePanel);
            
            CopyItemControlController = new MpCopyItemControlController(tileSize,ci,this);
            CopyItemTilePanel.Controls.Add(CopyItemControlController.ItemControl);
            CopyItemControlController.ItemControl.BringToFront();

            UpdateTileSize(tileSize);
        }

        private void _spaceKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            ((RichTextBox)CopyItemControlController.ItemControl).ReadOnly = false;
        }

        private void _escKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            ((RichTextBox)CopyItemControlController.ItemControl).ReadOnly = true;
        }

        public void UpdateTileSize(int tileSize) {
            int tp = (int)((float)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePadRatio") * (float)tileSize);
            int ts = tileSize - (tp * 2);
            int tth = ts - tp;// (int)((float)ts * (float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleRatio"));
            int x = ((TotalTileCount - _tileId) * ts) + ((TotalTileCount - _tileId + 1) * (tp*2));
            int y = tp;
            CopyItemTilePanel.SetBounds(x,y,ts,ts);
            CopyItemTileTitlePanelController.UpdateTileSize(tth);
            CopyItemControlController.UpdateTileSize(tth);
            //_titlePanel.SetBounds(x-tp,y-tp,tileSize,tth);
            //_contentPanel.SetBounds(tp,tth,ts - (tp * 2),ts - tp * 2 - tth);
            

            //_titleTextBox.Font = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileTitleFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleFontSize"));

           /* Font f = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize"));
            //_titleTextBox.Scale(new SizeF(1.0f,1.0f));
            if(_itemControl.GetType() == typeof(RichTextBox)) {
                ((RichTextBox)_itemControl).Font = f;
                ((RichTextBox)_itemControl).Scale(new SizeF(1.0f,1.0f));
            }
            else if(_itemControl.GetType() == typeof(TextBox)) {
                ((TextBox)_itemControl).Font = f;
                ((TextBox)_itemControl).Scale(new SizeF(1.0f,1.0f));
            }
            else if(_itemControl.GetType() == typeof(WebBrowser)) {
                ((WebBrowser)_itemControl).Font = f;9
                ((WebBrowser)_itemControl).Scale(new SizeF(1.0f,1.0f));
            }*/
        }
        public void SetFocus(bool newFocus) {
            this._hasFocus = newFocus;
            _copyItemTilePanel.BackColor = (this._hasFocus) ? (Color)MpSingletonController.Instance.GetSetting("LogPanelTileActiveColor") : _tileColor;
            if(newFocus) {
                //ActivateHotKeys();
            } else {
                //DeactivateHotKeys();
                //CopyItemControlController.IsReadOnly = true;
            }
        }
        
        public void ActivateHotKeys() {
            if(IsEditable) {
                _escKeyHook.RegisterHotKey(ModifierKeys.None,Keys.Escape);
                _spaceKeyHook.RegisterHotKey(ModifierKeys.None,Keys.Space);
            }            
        }      

        public void DeactivateHotKeys() {
            if(IsEditable) {
                _escKeyHook.UnregisterHotKey();
                _spaceKeyHook.UnregisterHotKey();
            }            
        }
    }
}
