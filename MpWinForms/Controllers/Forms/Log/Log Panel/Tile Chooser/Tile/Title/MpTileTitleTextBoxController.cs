using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleTextBoxController:MpController {
        public TextBox TileTitleTextBox { get; set; }
        public MpShadowLabel TileTitleLabel { get; set; }

        private MpKeyboardHook _enterHook, _escHook;

        private string _orgTitle = string.Empty;
        private string _emptyTitle = "                  "; //18 spaces

        private int _copyItemId;

        public MpTileTitleTextBoxController(MpCopyItem ci,MpController Parent) : base(Parent) {
            _orgTitle = ci.Title;
            _copyItemId = ci.CopyItemId;
            if(ci.Title.Trim() == string.Empty) {
                ci.Title = _emptyTitle;
                ci.WriteToDatabase();
            }

            TileTitleLabel = new MpShadowLabel(ci.ItemColor.Color) {
                Text = ci.Title,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None,
            };
            TileTitleLabel.DoubleBuffered(true);
            TileTitleLabel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleLabel.Click += _titleLabel_Click;

            TileTitleTextBox = new TextBox() {
                Text = ci.Title,
                ReadOnly = false,
                Visible = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = Color.Black,
                MaxLength = Properties.Settings.Default.MaxTitleLength
            };
            TileTitleTextBox.DoubleBuffered(true);
            TileTitleTextBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleTextBox.KeyUp += TileTitleTextBox_KeyUp;
            TileTitleTextBox.LostFocus += _titleTextBox_LostFocus;
        }

        public override void Update() {
            //tile padding
            int tp = Properties.Settings.Default.TileItemPadding;
            //tile item padding
            int tip = Properties.Settings.Default.TileItemPadding;
            //tile title panel rect
            Rectangle ttpr = ((MpTileTitlePanelController)Find("MpTileTitlePanelController")).TileTitlePanel.Bounds;

            float fontSize = (Properties.Settings.Default.TileTitleHeightFontRatio * (float)(ttpr.Height)) - (float)tp;
            fontSize = fontSize < 1.0f ? 10.0f : fontSize;
            TileTitleTextBox.Font = new Font(Properties.Settings.Default.TileTitleFont, fontSize, GraphicsUnit.Pixel);
            TileTitleLabel.Font = TileTitleTextBox.Font;

            //tile title textbox size
            Size tttbs = TextRenderer.MeasureText(TileTitleTextBox.Text, TileTitleTextBox.Font);

            Rectangle titleBounds = new Rectangle(tp * 2, 10, tttbs.Width, tttbs.Height);
            TileTitleTextBox.Bounds = titleBounds;
            TileTitleLabel.Bounds = TileTitleTextBox.Bounds;

            if (TileTitleLabel.Visible) {
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
            ((MpTileTitlePanelController)Parent).Update();
        }
        private void _titleTextBox_LostFocus(object sender,EventArgs e) {            
            ReadMode();
        }

        private void _titleLabel_Click(object sender,EventArgs e) {
            EditMode();
        }

        private void _enterHook_KeyPressed(object sender, KeyPressedEventArgs e) {
            ReadMode();
        }
        private void _escHook_KeyPressed(object sender, KeyPressedEventArgs e) {
            ReadMode(true);
        }
        private void EditMode() {
            ((MpLogFormController)Find(typeof(MpLogFormController))).DeactivateHotKeys();
            ActivateHotKeys();

            TileTitleLabel.Visible = false;
            TileTitleTextBox.Visible = true;
            TileTitleTextBox.Focus();
            TileTitleTextBox.SelectAll();

            _orgTitle = TileTitleLabel.Text;
            ((MpTileTitlePanelController)Parent).Update();
        }
        private void ReadMode(bool revertTitle = false) {
            DeactivateHotKeys();

            ((MpLogFormController)Find(typeof(MpLogFormController))).ActivateHotKeys();

            TileTitleLabel.Visible = true;
            TileTitleTextBox.Visible = false;

            TileTitleLabel.Visible = true;
            TileTitleTextBox.Visible = false;

            TileTitleTextBox.Text = revertTitle ? _orgTitle : TileTitleTextBox.Text;

            MpCopyItem ci = new MpCopyItem(_copyItemId);
            if (ci.Title != TileTitleTextBox.Text) {
                ci.Title = TileTitleTextBox.Text.Trim();
                ci.WriteToDatabase();
            }
            if (TileTitleTextBox.Text.Trim() == string.Empty) {
                TileTitleTextBox.Text = _emptyTitle;
            }
            TileTitleLabel.Text = TileTitleTextBox.Text;

            ((MpTileTitlePanelController)Parent).Update();
        }
        public void ActivateHotKeys() {
            ActivateEnterKey();
            ActivateEscKey();
        }
        public void DeactivateHotKeys() {
            DeactivateEnterKey();
            DeactivateEscKey();
        }
        public void ActivateEnterKey() {
            if (_enterHook == null) {
                _enterHook = new MpKeyboardHook();
                _enterHook.RegisterHotKey(ModifierKeys.None, Keys.Enter);
                _enterHook.KeyPressed += _enterHook_KeyPressed;
            }
        }

        public void DeactivateEnterKey() {
            if (_enterHook != null) {
                _enterHook.UnregisterHotKey();
                _enterHook.Dispose();
                _enterHook = null;
            }
        }

        public void ActivateEscKey() {
            if (_escHook == null) {
                _escHook = new MpKeyboardHook();
                _escHook.RegisterHotKey(ModifierKeys.None, Keys.Escape);
                _escHook.KeyPressed += _escHook_KeyPressed;
            }
        }

        public void DeactivateEscKey() {
            if (_escHook != null) {
                _escHook.UnregisterHotKey();
                _escHook.Dispose();
                _escHook = null;
            }
        }
    }
}
