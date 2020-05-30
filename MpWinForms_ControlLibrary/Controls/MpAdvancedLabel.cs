using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste_WindowsControlLibrary {
    public partial class MpAdvancedLabel : UserControl {
        [Browsable(true), Category("ShadowOffset"), Description("The corner round radius.")]
        public PointF ShadowOffset { get; set; }
        [Browsable(true), Category("ShadowColor"), Description("The corner round radius.")]
        public Color ShadowColor { get; set; }

        private string _text = string.Empty;
        [Browsable(true), Category("Text"), Description("The corner round radius.")]
        public override string Text {
            get {
                return _text;
            }
            set {
                _text = value;
                Invalidate();
            }
        }
        [Browsable(true), Category("Font"), Description("The corner round radius.")]
        public override Font Font { get; set; }

        public MpAdvancedLabel() {
            Size = new Size(512, 512);
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            ShadowColor = Color.Black;
            ShadowOffset = new PointF(3.0f, 3.0f);
            Font = new Font("Bahnschrift", 40.0f, GraphicsUnit.Pixel);
            // Paint += this.MpCopyItemCard_Paint;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e) {
            //base.OnPaint(e);
            Rectangle t = ClientRectangle;
            t.Inflate(new Size((int)ShadowOffset.X, (int)ShadowOffset.Y));
            e.Graphics.FillRectangle(new SolidBrush(BackColor), t);
            e.Graphics.DrawString(Text, Font, new SolidBrush(ShadowColor), ShadowOffset);
            e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), PointF.Empty);
            
        }
        public MpAdvancedLabel(Color tileColor) : base() {        
            this.DoubleBuffered = true;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            ShadowColor = Color.Black;
            ShadowOffset = new PointF(3.0f, 3.0f);
        }
        #region Designer Autocode
        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // MpTilePanel
            // 
            this.Name = "MpAdvancedLabelPanel";
            this.Size = new System.Drawing.Size(300,50);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
