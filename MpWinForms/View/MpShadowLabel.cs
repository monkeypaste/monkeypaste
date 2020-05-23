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
        public Color TileColor { get; set; }

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
            e.Graphics.FillRectangle(new SolidBrush(TileColor), t);
            e.Graphics.DrawString(Text, Font, new SolidBrush(BackColor), BackOffset);
            e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), PointF.Empty);
            //base.OnPaint(e);
        }
        public MpShadowLabel(Color tileColor) : base() {          
            this.DoubleBuffered = true;
            TileColor = tileColor;
            ForeColor = MpHelperSingleton.Instance.IsBright(TileColor) ? Color.Black : Color.White;
            BackColor = ForeColor == Color.Black ? Color.White : Color.Black;
            BackOffset = new PointF(3.0f, 5.0f);
        }
    }
}
