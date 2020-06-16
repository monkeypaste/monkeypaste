using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpWinFormsApp {
    public class MpEditableLabelController : MpController {
        public Label Label { get; set; }
        public TextBox TextBox { get; set; }

        private bool _isEditable = false;
        public bool IsEditable {
            get {
                return _isEditable;
            }
            set {
                _isEditable = value;
                if (_isEditable) {
                    Label.Visible = false;
                    TextBox.Visible = true;
                } else {
                    Label.Visible = true;
                    TextBox.Visible = false;
                }
            }
        }

        private bool _sizeFromParentHeight = false;
        private float _sizeRatio = 1.0f;
        private string _fontName, _text;
        
        public MpEditableLabelController(Color textColor,Color backColor,bool isEditable,string text,MpController parent,bool sizeFromParentHeight = true,float sizeRatio = 1.0f,string fontName = "Caldera") : base(parent) {
            _sizeFromParentHeight = sizeFromParentHeight;
            _sizeRatio = sizeRatio;
            _fontName = fontName;
            _text = text;
            Label = new Label() {
                Visible = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Cursor = Cursors.Arrow,
                BackColor = backColor,
                ForeColor = textColor,
                BorderStyle = BorderStyle.None,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = text,
                Font = GetFont(),
                Bounds = GetBounds()
            };
            Label.DoubleBuffered(true);
            TextBox = new TextBox() {
                ReadOnly = false,
                Multiline = false,
                WordWrap = false,
                Margin = Padding.Empty,
                SelectionLength = 0,
                Padding = Padding.Empty,
                Cursor = Cursors.Arrow,
                BackColor = backColor,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Left,
                ForeColor = textColor,
                Text = text,
                Font = GetFont(),
                Bounds = GetBounds()
            };
            TextBox.DoubleBuffered(true);
            IsEditable = isEditable;
        }
        public Font GetFont() {
            //parent rect
            Rectangle pr = ((MpController)Parent).GetBounds();

            float fontSize = _sizeFromParentHeight ? (float)pr.Height * _sizeRatio: (float)pr.Width * _sizeRatio;
            fontSize = fontSize < 1.0f ? 10.0f : fontSize;
            return new Font(_fontName, fontSize, GraphicsUnit.Pixel);
        }
        public override Rectangle GetBounds() {
            Size size = TextRenderer.MeasureText(_text, GetFont());
            return new Rectangle(0, 0, size.Width, size.Height);
        }
        public override void Update() {
            Label.Font = GetFont();
            Label.Bounds = GetBounds();
            TextBox.Font = GetFont();
            TextBox.Bounds = GetBounds();

            Label.Invalidate();
            TextBox.Invalidate();
        }
    }
}
