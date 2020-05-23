using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpTileMenuButtonController : MpController {
        private int buttonId = 0;
        private int rowId = 0, colId = 0;
        private bool _isFocused = false;
        private bool _isDown = false;
        private bool _isDisabled = false;
        private Color _downColor = Color.Yellow, _upColor = Color.White, _focusColor = Color.White, _defaultColor = Color.Black;

        public MpTileMenuButton TileMenuButton { get; set; }

        public MpTileMenuButtonController(int rowId,int colId,int buttonId,int tileId,int panelId,string title,MpController Parent) : base(Parent) {
            this.rowId = rowId;
            this.colId = colId;
            this.buttonId = buttonId;
            _defaultColor = Properties.Settings.Default.TileMenuColor;
            TileMenuButton = new MpTileMenuButton(buttonId,tileId,panelId) {
                Text = title,    
                UseVisualStyleBackColor = false,
                TabIndex = buttonId,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = Color.FromArgb(50,_defaultColor),
                ForeColor = Color.White
            };
            TileMenuButton.GotFocus += TileMenuButton_GotFocus;
            TileMenuButton.LostFocus += TileMenuButton_LostFocus;
            TileMenuButton.MouseHover += TileMenuButton_MouseHover;
            TileMenuButton.MouseLeave += TileMenuButton_MouseLeave;
            TileMenuButton.MouseDown += TileMenuButton_MouseDown;
            TileMenuButton.MouseUp += TileMenuButton_MouseUp;
            TileMenuButton.Click += TileMenuButton_Click;
            TileMenuButton.KeyDown += TileMenuButton_KeyDown;
            TileMenuButton.KeyUp += TileMenuButton_KeyUp;

            //Link(new List<MpIView> { TileMenuButton});
        }
           public override void Update() {
            int totalButtons = ((MpTileMenuPanelController)Parent).TileMenuButtonControllerList.Count;
            int rc = ((MpTileMenuPanelController)Parent).RowCount;
            int cc = ((MpTileMenuPanelController)Parent).ColCount;
            Rectangle mr = ((MpTileMenuPanelController)Parent).TileMenuPanel.Bounds;
            //button size
            TileMenuButton.Size = new Size((int)(mr.Width / cc),(int)(mr.Height / rc));
            TileMenuButton.Location = new Point(TileMenuButton.Size.Width*colId,TileMenuButton.Height*this.rowId);
            //TileMenuButton.SetBounds((int)(mr.Width / totalButtons)*buttonId,0,(int)(mr.Width / totalButtons),mr.Height);
            Font buttonFont = new Font(Properties.Settings.Default.TileMenuFont,(float)mr.Height * Properties.Settings.Default.TileMenuFontRatio,GraphicsUnit.Pixel);
            TileMenuButton.Font = buttonFont;
        }

        private void TileMenuButton_Click(object sender,EventArgs e) {
            SetFocus(true);
        }

        private void TileMenuButton_MouseUp(object sender,System.Windows.Forms.MouseEventArgs e) {
            TileMenuButton.BackColor = _upColor;
        }

        private void TileMenuButton_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e) {
            TileMenuButton.BackColor = _downColor;
        }

        private void TileMenuButton_KeyUp(object sender,System.Windows.Forms.KeyEventArgs e) {
            if(e.KeyCode == System.Windows.Forms.Keys.Space || e.KeyCode == System.Windows.Forms.Keys.Enter) {
                TileMenuButton.BackColor = _defaultColor;
            }
        }
        private void TileMenuButton_KeyDown(object sender,System.Windows.Forms.KeyEventArgs e) {
            if(e.KeyCode == System.Windows.Forms.Keys.Space || e.KeyCode == System.Windows.Forms.Keys.Enter) {
                TileMenuButton.BackColor = _downColor;
            }
            else if(e.KeyCode == System.Windows.Forms.Keys.Left || (e.KeyCode == System.Windows.Forms.Keys.Tab && e.Modifiers == System.Windows.Forms.Keys.Shift)) {
                TileMenuButton.SelectNextControl(TileMenuButton,false,true,false,true);
            }
            else if(e.KeyCode == System.Windows.Forms.Keys.Right || e.KeyCode == System.Windows.Forms.Keys.Tab) {
                TileMenuButton.SelectNextControl(TileMenuButton,true,true,false,true);
            }
        }

        private void TileMenuButton_MouseLeave(object sender,EventArgs e) {
            SetFocus(false);
        }

        private void TileMenuButton_MouseHover(object sender,EventArgs e) {
            SetFocus(true);
        }

        private void TileMenuButton_LostFocus(object sender,EventArgs e) {
            SetFocus(false);
        }

        private void TileMenuButton_GotFocus(object sender,EventArgs e) {
            SetFocus(true);
        }
        public void SetFocus(bool newFocus) {
            _isFocused = newFocus;
            if(_isFocused) {
                TileMenuButton.Focus();
                TileMenuButton.BackColor = _focusColor;
            } else {
                TileMenuButton.BackColor = _defaultColor;
            }
        }
    }
}
