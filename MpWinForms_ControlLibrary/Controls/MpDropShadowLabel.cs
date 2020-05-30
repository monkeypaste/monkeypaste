using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste_WindowsControlLibrary {
    public partial class MpDropShadowLabel : UserControl {
        [Browsable(true), Category("ShadowOffset")]
        public PointF ShadowOffset { get; set; } = new PointF(3, 3);

        [Browsable(true), Category("ShadowColor")]
        public Color ShadowColor { get; set; } = Color.Black;

        private string _text = "ShadowLabel";
        [Browsable(true), Category("Text")]
        public override string Text {
            get {
                return _text;
            }
            set {
                _text = value;
                Invalidate();
            }
        }
        [Browsable(true), Category("Font"), Description("Style of the shadow.")]
        public override Font Font { get; set; } = new Font("Bahnschrift", 37.0f, GraphicsUnit.Pixel);

        protected override void OnPaint(PaintEventArgs e) {
            //base.OnPaint(e);
            Rectangle t = ClientRectangle;
            t.Inflate(new Size((int)ShadowOffset.X, (int)ShadowOffset.Y));
            e.Graphics.FillRectangle(new SolidBrush(BackColor), t);
            e.Graphics.DrawString(Text, Font, new SolidBrush(ShadowColor), ShadowOffset);
            e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), PointF.Empty);

        }
        //public MpShadowLabel(Color tileColor) : base() {
        //    this.DoubleBuffered = true;
        //    TileColor = tileColor;
        //    BackColor = Color.Transparent;
        //    ForeColor = Color.White;// MpHelperSingleton.Instance.IsBright(TileColor) ? Color.Black : Color.White;
        //    ShadowColor = Color.Black;// ForeColor == Color.Black ? Color.White : Color.Black;
        //    BackOffset = new PointF(3.0f, 3.0f);
        //}

        public MpDropShadowLabel() {
            this.DoubleBuffered = true;
            BackColor = Color.Transparent;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();
        }
    }
}
