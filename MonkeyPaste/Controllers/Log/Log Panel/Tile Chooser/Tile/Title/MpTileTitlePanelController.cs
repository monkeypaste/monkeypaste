using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitlePanelController : MpController {
        //title panel
        public MpTileTitlePanel TileTitlePanel { get; set; }
        public MpTileTitleTextBox TileTitleTextBox { get; set; }
        public MpTileTitleLabel TileTitleLabel { get; set; }

        public MpTileTitleIconPanelController TileTitleIconPanelController { get; set; }
        
        private int _copyItemId;
        private MpKeyboardHook _enterHook,_escHook;
        private string _orgTitle = string.Empty;

        public MpTileTitlePanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            _orgTitle = ci.Title;
            //parent panel
            TileTitlePanel = new MpTileTitlePanel(tileId,panelId) {
                BorderStyle = BorderStyle.None,
                BackColor = ci.ItemColor.Color,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            TileTitlePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;       
           
            TileTitleIconPanelController = new MpTileTitleIconPanelController(tileId,panelId,ci,this);
            TileTitlePanel.Controls.Add(TileTitleIconPanelController.TileTitleIconBox);

            //title label/textbox
            _copyItemId = ci.CopyItemId;
            if(ci.Title.Trim() == string.Empty) {
                ci.Title = "Empty";
                ci.WriteToDatabase();
            }

            TileTitleLabel = new MpTileTitleLabel(tileId,panelId,ci.ItemColor.Color) {
                Text = ci.Title,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None,
                ForeColor = MpHelperSingleton.Instance.IsBright(ci.ItemColor.Color) ? Color.Black : Color.White,
            };
            TileTitleLabel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleLabel.Click += _titleLabel_Click;
            
            TileTitleTextBox = new MpTileTitleTextBox(tileId,panelId) {
                Text = ci.Title,
                ReadOnly = false,
                Visible = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None,
                //SelectionLength = 0,
                MaxLength = Properties.Settings.Default.MaxTitleLength
            };
            TileTitleTextBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleTextBox.KeyUp += TileTitleTextBox_KeyUp;
            TileTitleTextBox.LostFocus += _titleTextBox_LostFocus;
            
            TileTitlePanel.Controls.Add(TileTitleLabel);
            TileTitlePanel.Controls.Add(TileTitleTextBox);

            TileTitlePanel.BackColor = ci.ItemColor.Color;
            TileTitleTextBox.BackColor = Color.White;
            TileTitleTextBox.ForeColor = Color.Black;
            TileTitleLabel.BackColor = TileTitleTextBox.BackColor;
            TileTitleLabel.ForeColor = TileTitleTextBox.ForeColor;

           // TileTitleTextBoxController.Read
            Link(new List<MpIView> { TileTitlePanel, TileTitleTextBox,TileTitleLabel});
        }
        
        public override void Update() {
            UpdatePanel();
            UpdateTitle();
        }
        private void UpdatePanel() {
            //tile panel
            var tp = ((MpTilePanelController)Find("MpTilePanelController")).TilePanel;
            //tile rect
            Rectangle tr = tp.Bounds;
            //tile header rect
            Rectangle thr = ((MpTilePanelController)Find("MpTilePanelController")).TileHeaderPanelController.TileHeaderPanel.Bounds;

            //tile title height
            int tth = (int)((float)tr.Height * Properties.Settings.Default.TileTitleHeightRatio);
            //tile padding
            int tpd = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width) + ((MpTilePanelController)Find("MpTilePanelController")).TilePanel.EdgeWidth;
            TileTitlePanel.SetBounds(tpd,thr.Bottom,tr.Width - tpd - tp.ShadowShift - tp.EdgeWidth,tth);

            TileTitleIconPanelController.Update();

            TileTitlePanel.Invalidate();
        }
        private void UpdateTitle() {
            //tile padding
            int tp = Properties.Settings.Default.TileItemPadding;
            //tile item padding
            int tip = Properties.Settings.Default.TileItemPadding;
            //tile title panel rect
            Rectangle ttpr = ((MpTileTitlePanelController)Find("MpTileTitlePanelController")).TileTitlePanel.Bounds;

            float fontSize = (Properties.Settings.Default.TileTitleHeightFontRatio * (float)(ttpr.Height)) - (float)tp;
            fontSize = fontSize < 1.0f ? 10.0f : fontSize;
            TileTitleTextBox.Font = new Font(Properties.Settings.Default.TileTitleFont,fontSize,GraphicsUnit.Pixel);
            TileTitleLabel.Font = TileTitleTextBox.Font;

            //tile title textbox size
            Size tttbs = TextRenderer.MeasureText(TileTitleTextBox.Text,TileTitleTextBox.Font);

            Rectangle titleBounds = new Rectangle(tp * 2,10,tttbs.Width,tttbs.Height);
            TileTitleTextBox.Bounds = titleBounds;
            TileTitleLabel.Bounds = TileTitleTextBox.Bounds;
            
            if(TileTitleLabel.Visible) {
                TileTitleLabel.BringToFront();
                TileTitleTextBox.Visible = false;
            }
            else {
                TileTitleTextBox.BringToFront();
            }
            TileTitleTextBox.Invalidate();
            TileTitleLabel.Invalidate();
        }
        private void TileTitleTextBox_KeyUp(object sender,KeyEventArgs e) {
            TileTitleTextBox.Focus();
            Update();
        }
        private void _titleTextBox_LostFocus(object sender,EventArgs e) {
            MpCopyItem ci = new MpCopyItem(_copyItemId);
            if(ci.Title != TileTitleTextBox.Text) {
                ci.Title = TileTitleTextBox.Text;
                ci.WriteToDatabase();
            }
            ReadMode();
        }

        private void _titleLabel_Click(object sender,EventArgs e) {
            EditMode();
        }
        private void EditMode() {
            ((MpLogFormController)Find("MpLogFormController")).DeactivateArrowKeys();
            ((MpLogFormController)Find("MpLogFormController")).DeactivateEnterKey();
            ((MpLogFormController)Find("MpLogFormController")).DeactivateEscKey();

            ActivateEnterKey();
            ActivateEscKey();

            TileTitleLabel.Visible = false;
            TileTitleTextBox.Visible = true;
            TileTitleTextBox.Focus();
            TileTitleTextBox.DeselectAll();
            TileTitleTextBox.SelectionStart = TileTitleTextBox.TextLength;
            //TileTitleTextBox.SelectionLength = 0;
            _orgTitle = TileTitleLabel.Text;
            Update();
        }
        private void ReadMode(bool revertTitle = false) {
            DeactivateEnterKey();
            DeactivateEscKey();

            ((MpLogFormController)Find("MpLogFormController")).ActivateArrowKeys();
            ((MpLogFormController)Find("MpLogFormController")).ActivateEnterKey();
            ((MpLogFormController)Find("MpLogFormController")).ActivateEscKey();

            TileTitleLabel.Visible = true;
            TileTitleTextBox.Visible = false;

            TileTitleLabel.Text = revertTitle ? _orgTitle : TileTitleTextBox.Text;
            TileTitleTextBox.Text = TileTitleLabel.Text;

            Update();
        }
        public void ActivateEnterKey() {
            if(_enterHook == null) {
                _enterHook = new MpKeyboardHook();
                _enterHook.RegisterHotKey(ModifierKeys.None,Keys.Enter);
                _enterHook.KeyPressed += _enterHook_KeyPressed;
            }
        }       

        public void DeactivateEnterKey() {
            if(_enterHook != null) {
                _enterHook.UnregisterHotKey();
                _enterHook.Dispose();
                _enterHook = null;
            }
        }
        
        public void ActivateEscKey() {
            if(_escHook == null) {
                _escHook = new MpKeyboardHook();
                _escHook.RegisterHotKey(ModifierKeys.None,Keys.Escape);
                _escHook.KeyPressed += _escHook_KeyPressed;
            }
        }     

        public void DeactivateEscKey() {
            if(_escHook != null) {
                _escHook.UnregisterHotKey();
                _escHook.Dispose();
                _escHook = null;
            }
        }
        private void _enterHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            ReadMode();
        }
        private void _escHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            ReadMode(true);
        }
    }
}