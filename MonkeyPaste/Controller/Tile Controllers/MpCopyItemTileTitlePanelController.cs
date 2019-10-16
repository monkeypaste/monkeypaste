using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItemTileTitlePanelController : MpController {
        //title panel
        private MpCopyItemTileTitlePanel _copyItemTileTitlePanel { get; set; }
        public MpCopyItemTileTitlePanel CopyItemTileTitlePanel { get { return _copyItemTileTitlePanel; } set { _copyItemTileTitlePanel = value; } }
        
        //icon controller
        private MpCopyItemTileTitleIconPanelController _copyItemTileTitleIconPanelController { get; set; }
        public MpCopyItemTileTitleIconPanelController CopyItemTileTitleIconPanelController { get { return _copyItemTileTitleIconPanelController; } set { _copyItemTileTitleIconPanelController = value; } }

        //title menu controller 
        private MpCopyItemTileTitleMenuPanelController _copyItemTileTitleMenuPanelController { get; set; }
        public MpCopyItemTileTitleMenuPanelController CopyItemTileTitleMenuPanelController { get { return _copyItemTileTitleMenuPanelController; } set { _copyItemTileTitleMenuPanelController = value; } }
        //private TextBox _copyItemTitleTextBox { get; set; }
        // public TextBox CopyItemTitleTextBox { get { return _copyItemTitleTextBox; } set { _copyItemTitleTextBox = value; } }

        private string _orgTitle;
        private int _copyItemId;
        private MpKeyboardHook _escKeyHook;

        public MpCopyItemTileTitlePanelController(int tileSize,MpCopyItem ci,Color tileColor,MpController parentController) : base(parentController) {
            _orgTitle = ci.Title;
            _copyItemId = ci.copyItemId;
            _escKeyHook = new MpKeyboardHook();
            _escKeyHook.KeyPressed += _escKeyHook_KeyPressed;

            //parent panel
            CopyItemTileTitlePanel = new MpCopyItemTileTitlePanel() {
                BackColor = tileColor,
                BorderStyle = BorderStyle.None
                
            };
            CopyItemTileTitlePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            //tile height
            int th = (int)((float)tileSize * (float)MpSingletonController.Instance.GetSetting("TileMenuHeightRatio"));
            CopyItemTileTitleIconPanelController = new MpCopyItemTileTitleIconPanelController(th,ci,this);
           
            CopyItemTileTitleMenuPanelController = new MpCopyItemTileTitleMenuPanelController(this);
            /*
            CopyItemTitleTextBox = new TextBint w x() {
                Text = ci.Title,
                //Anchor = AnchorStyles.Right,                
                ReadOnly = true,
                BackColor = tileColor,
                Font = titleFont,
                BorderStyle = BorderStyle.None
            };
            CopyItemTitleTextBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            CopyItemTitleTextBox.MouseEnter += _titleTextBox_MouseEnter;
            //CopyItemTitleTextBox.Enter += _titleTextBox_LostFocus;
            CopyItemTitleTextBox.MouseLeave += _titleTextBox_LostFocus;
            CopyItemTitleTextBox.Click += _titleTextBox_Click;
            CopyItemTitleTextBox.Leave += _titleTextBox_MouseLeave;

            CopyItemTileTitlePanel.Controls.Add(CopyItemTitleTextBox);            
            CopyItemTitleTextBox.BringToFront();
            */

            CopyItemTileTitlePanel.Controls.Add(CopyItemTileTitleIconPanelController.CopyItemTileTitleIconPanel);           

            UpdateBounds();
        }

        private void _escKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            throw new NotImplementedException();
        }
        private void ActivateHotKeys() {
            _escKeyHook.RegisterHotKey(ModifierKeys.None,Keys.Escape);
        }
        private void DeactivateHotKeys() {
            _escKeyHook.UnregisterHotKey();
        }
       /* public void UpdateTileSize(int tileSize) {
            int tp = (int)((float)MpSingletonController.Instance.GetSetting("TileOuterPadScreenWidthRatio") * (float)tileSize);
            int ts = tileSize;
            int tth = (int)((float)ts * (float)MpSingletonController.Instance.GetSetting("TileMenuHeightRatio"));

            CopyItemTileTitlePanel.Location = new Point(tp,tp);
            CopyItemTileTitlePanel.Size = new Size(ts-tp,tth);

            int tfs = (int)((float)tth * (float)MpSingletonController.Instance.GetSetting("TileMenuFontRatio"));
            CopyItemTitleTextBox.Location = new Point(tth+tp,(int)((CopyItemTileTitlePanel.Height/2)-(CopyItemTitleTextBox.Height/2)));
            CopyItemTitleTextBox.Size = new Size(ts-tth-tp-tp-tp,tth);
            CopyItemTitleTextBox.Font = new Font((string)MpSingletonController.Instance.GetSetting("TileMenuFont"),tfs,GraphicsUnit.Pixel);

            CopyItemTileTitleIconPanelController.UpdatePanelSize(tth-tp);
        }
        private void _titleTextBox_LostFocus(object sender,EventArgs e) {
            if(_orgTitle != _copyItemTitleTextBox.Text) {
                MpCopyItem ci = MpSingletonController.Instance.GetMpData().GetMpCopyItem(_copyItemId);
                ci.Title = CopyItemTitleTextBox.Text;                
                ci.WriteToDatabase();
            }
            if(!CopyItemTitleTextBox.Focused) {
                CopyItemTitleTextBox.Cursor = Cursors.Arrow;
                CopyItemTitleTextBox.BorderStyle = BorderStyle.None;
                CopyItemTitleTextBox.BackColor = CopyItemTileTitlePanel.BackColor;
                CopyItemTitleTextBox.ReadOnly = true;
                CopyItemTileTitlePanel.Focus();
                DeactivateHotKeys();
            }            
        }

        private void _titleTextBox_Click(object sender,EventArgs e) {
            _orgTitle = CopyItemTitleTextBox.Text;
            CopyItemTitleTextBox.ReadOnly = false;
            CopyItemTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
            CopyItemTitleTextBox.BackColor = (Color)MpSingletonController.Instance.GetSetting("TileMenuColor");
            ActivateHotKeys();
        }

        private void _titleTextBox_MouseLeave(object sender,EventArgs e) {
            CopyItemTitleTextBox.Cursor = Cursors.Arrow;
            CopyItemTitleTextBox.BorderStyle = BorderStyle.None;
            CopyItemTitleTextBox.ReadOnly = true;
            CopyItemTitleTextBox.BackColor = CopyItemTileTitlePanel.BackColor;
            CopyItemTileTitlePanel.Focus();
            DeactivateHotKeys();
        }

        private void _titleTextBox_MouseEnter(object sender,EventArgs e) {
            CopyItemTitleTextBox.Cursor = Cursors.IBeam;
            CopyItemTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
        }**/

        public override void UpdateBounds() {
            int tileSize = ((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)ParentController).ParentController).CopyItemTileChooserPanel.Bounds.Height;
            int tp = (int)((float)MpSingletonController.Instance.GetSetting("TileOuterPadScreenWidthRatio") * (float)tileSize);
            int ts = tileSize;
            int tth = (int)((float)ts * (float)MpSingletonController.Instance.GetSetting("TileMenuHeightRatio"));

            CopyItemTileTitlePanel.Location = new Point(tp,tp);
            CopyItemTileTitlePanel.Size = new Size(ts - tp,tth);          
        }
    }
}