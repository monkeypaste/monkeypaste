using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpShadowLabel : UserControl {
        public PointF BackOffset { get; set; }
        public Color ShadowColor { get; set; }

        private string _text = string.Empty;
        public override string Text {
            get {
                return _text;
            }
            set {
                _text = value;
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            //do nonthing
        }
        protected override void OnPaint(PaintEventArgs e) {
            Rectangle t = ClientRectangle;
            t.Inflate(new Size((int)BackOffset.X, (int)BackOffset.Y));
            e.Graphics.FillRectangle(new SolidBrush(BackColor), t);
            e.Graphics.DrawString(Text, Font, new SolidBrush(ShadowColor), BackOffset);
            e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), PointF.Empty);
            //Size = TextRenderer.MeasureText(Text, Font);
        }
        public MpShadowLabel(Color backColor) : base() {          
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            ForeColor = Color.White;// MpHelperSingleton.Instance.IsBright(TileColor) ? Color.Black : Color.White;
            BackColor = backColor;
            ShadowColor = Color.Black;// ForeColor == Color.Black ? Color.White : Color.Black;
            BackOffset = new PointF(3.0f, 3.0f);
        }
    }
}
