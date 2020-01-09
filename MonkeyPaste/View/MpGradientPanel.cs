using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpGradientPanel : Panel {
        public Color C1 { get; set; } = Color.Red;
        public Color C2 { get; set; } = Color.Green;

        public override Color BackColor {
            get {
                return base.BackColor;
            }
            set {
                C1 = value;
                base.BackColor = value;
            }
        }
        public MpGradientPanel() : base() {
            DoubleBuffered = true;
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(this.ClientRectangle,C1,C2,0.0f);

            ColorBlend cblend = new ColorBlend(3);
            cblend.Colors = new Color[2] { C1,C2 };
            cblend.Positions = new float[2] { 0f,1f };

            linearGradientBrush.InterpolationColors = cblend;

            e.Graphics.FillRectangle(linearGradientBrush,this.ClientRectangle);
        }
    }
}