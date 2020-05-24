using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpButtonController : MpControlController {
        private bool _isFocused = false;
        private bool _isDown = false;
        private bool _isDisabled = false;

        public Color DownColor = Color.Yellow;
        public Color UpColor { get; set; } = Color.White;
        public Color OverColor { get; set; } = Color.White;
        public Color ActiveColor { get; set; } = Color.Red;
        public Color DefaultColor { get; set; } = Color.Blue;

        public delegate void ButtonClick(object sender, EventArgs e);
        public event ButtonClick ButtonClickEvent;
       

        public MpButton TileMenuButton { get; set; }

        private int _rId, _cId;

        public MpButtonController(int rId, int cId,string title,MpController Parent) : base(Parent) {
            _rId = rId;
            _cId = cId;
            DefaultColor = Properties.Settings.Default.LogMenuLeftInactiveColor;
            TileMenuButton = new MpButton() {
                Text = title,    
                //UseVisualStyleBackColor = false,
                //TabIndex = buttonId,
                //FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = Color.FromArgb(50,DefaultColor),
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
            TileMenuButton.Location = new Point(TileMenuButton.Size.Width*_cId,TileMenuButton.Height*_rId);
            //TileMenuButton.SetBounds((int)(mr.Width / totalButtons)*buttonId,0,(int)(mr.Width / totalButtons),mr.Height);
            //Font buttonFont = new Font(Properties.Settings.Default.TileMenuFont,(float)mr.Height * Properties.Settings.Default.TileMenuFontRatio,GraphicsUnit.Pixel);
            //TileMenuButton.Font = buttonFont;
        }

        private void TileMenuButton_Click(object sender,EventArgs e) {
            SetFocus(true);
            ButtonClickEvent(sender, e);
        }

        private void TileMenuButton_MouseUp(object sender,System.Windows.Forms.MouseEventArgs e) {
            TileMenuButton.BackColor = UpColor;            
        }

        private void TileMenuButton_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e) {
            TileMenuButton.BackColor = DownColor;
        }

        private void TileMenuButton_KeyUp(object sender,System.Windows.Forms.KeyEventArgs e) {
            if(e.KeyCode == System.Windows.Forms.Keys.Space || e.KeyCode == System.Windows.Forms.Keys.Enter) {
                TileMenuButton.BackColor = DefaultColor;
            }
        }
        private void TileMenuButton_KeyDown(object sender,System.Windows.Forms.KeyEventArgs e) {
            if(e.KeyCode == System.Windows.Forms.Keys.Space || e.KeyCode == System.Windows.Forms.Keys.Enter) {
                TileMenuButton.BackColor = DownColor;
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
                TileMenuButton.BackColor = ActiveColor;
            } else {
                TileMenuButton.BackColor = DefaultColor;
            }
        }

        public override Rectangle GetBounds() {
            throw new NotImplementedException();
        }
    }
}
