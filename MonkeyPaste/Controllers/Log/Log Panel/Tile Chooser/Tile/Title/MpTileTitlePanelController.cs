using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitlePanelController : MpController {
        public static int ShadowOffset { get; set; } = 1;

        //title panel
        public MpTileTitlePanel TileTitlePanel { get; set; }
        public MpTileTitleTextBox TileTitleTextBox { get; set; }
        public MpTileTitleLabel TileTitleLabel { get; set; }

        public MpTileTitleIconPanelController TileTitleIconPanelController { get; set; }
        public MpTileTitleTextBoxController TileTitleTextBoxController { get; set; }
        
        private int _copyItemId;

        public MpTileTitlePanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {            
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

            TileTitleLabel = new MpTileTitleLabel(tileId,panelId) {
                Text = ci.Title,
                //Angle = 0,
                //XOffset = MpTileTitlePanelController.ShadowOffset,
                //YOffset = MpTileTitlePanelController.ShadowOffset,
                //ShadowColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.White : Color.Black,
                BackColor = Color.Transparent,
                //Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None,
                //ForeColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.Black : Color.White
            };
            TileTitleLabel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleLabel.Click += _titleLabel_Click;

            //TileTitleLabelShadow = new MpTileTitleLabel(tileId,panelId) {
            //    Text = ci.Title,
            //    BackColor = Color.Transparent,
            //    Margin = Padding.Empty,
            //    Padding = Padding.Empty,
            //    BorderStyle = BorderStyle.None,
            //    ForeColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.White : Color.Black
            //};

            TileTitleTextBox = new MpTileTitleTextBox(tileId,panelId) {
                Text = ci.Title,
                ReadOnly = false,
                Visible = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None,
                MaxLength = Properties.Settings.Default.MaxTitleLength
            };
            TileTitleTextBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleTextBox.KeyUp += TileTitleTextBox_KeyUp;
            TileTitleTextBox.LostFocus += _titleTextBox_LostFocus;

            TileTitleTextBoxController = new MpTileTitleTextBoxController(tileId,panelId,ci,this);
            //TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleLabelShadow);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleLabel);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleTextBox);

            TileTitlePanel.BackColor = ci.ItemColor.Color;// MpHelperSingleton.Instance.GetRandomColor();
            TileTitleTextBoxController.TileTitleTextBox.BackColor = TileTitlePanel.BackColor;
            TileTitleTextBoxController.TileTitleTextBox.ForeColor = MpHelperSingleton.Instance.IsBright(TileTitlePanel.BackColor) ? Color.Black : Color.White;
            TileTitleTextBoxController.TileTitleLabel.BackColor = TileTitleTextBoxController.TileTitleTextBox.BackColor;
            TileTitleTextBoxController.TileTitleLabel.ForeColor = TileTitleTextBoxController.TileTitleTextBox.ForeColor;

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
            TileTitleTextBoxController.Update();

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
            //TileTitleLabelShadow.Font = TileTitleLabel.Font;

            //tile title textbox size
            Size tttbs = TextRenderer.MeasureText(TileTitleTextBox.Text,TileTitleTextBox.Font);

            Rectangle titleBounds = new Rectangle(tp * 2,10,tttbs.Width,tttbs.Height);
            TileTitleTextBox.Bounds = titleBounds;
            TileTitleLabel.Bounds = TileTitleTextBox.Bounds;
            //TileTitleLabelShadow.Bounds = titleBounds;
            //TileTitleLabelShadow.Location = new Point(titleBounds.X + MpTileTitlePanelController.ShadowOffset,titleBounds.Y + MpTileTitlePanelController.ShadowOffset);

            if(TileTitleLabel.Visible) {
                TileTitleLabel.BringToFront();
                TileTitleTextBox.Visible = false;
                //TileTitleLabelShadow.SendToBack();
            }
            else {
                TileTitleTextBox.BringToFront();
            }
            //TileTitleLabelShadow.Invalidate();
            //TileTitleTextBox.Invalidate();
            TileTitleLabel.Invalidate();
        }
        private void TileTitleTextBox_KeyUp(object sender,KeyEventArgs e) {
            ((MpTileTitlePanelController)Parent).Update();
            TileTitleTextBox.Focus();
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
            TileTitleLabel.Visible = false;
            TileTitleTextBox.Visible = true;

            //TileTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
            //((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor = Color.White;
            TileTitleTextBox.BackColor = Color.White;
            TileTitleTextBox.ForeColor = Color.Black;
            TileTitleTextBox.Focus();
            ((MpTileTitlePanelController)Parent).Update();
        }
        private void ReadMode() {
            TileTitleLabel.Visible = true;
            TileTitleTextBox.Visible = false;

            //TileTitleTextBox.BorderStyle = BorderStyle.None;
            TileTitleLabel.BackColor = ((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor;
            TileTitleLabel.ForeColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.Black : Color.White;
            if(TileTitleTextBox.Text == string.Empty) {
                TileTitleTextBox.Text = "     ";
            }
            TileTitleTextBox.Invalidate();
            TileTitleLabel.Invalidate();
            Update();
        }
    }
}