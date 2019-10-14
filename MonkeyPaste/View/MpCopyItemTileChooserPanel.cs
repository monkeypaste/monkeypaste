using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItemTileChooserPanel : ScrollableControl {
        private Color _c1, _c2, _c3;

        public MpCopyItemTileChooserPanel() : base() {
            _c1 = MpColorPallete.Blue;
            _c2 = MpColorPallete.DarkGreen;
            _c3 = MpColorPallete.LightBlue;
            this.DoubleBuffered = true;
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(this.ClientRectangle,_c1,_c2,90);

            ColorBlend cblend = new ColorBlend(3);
            cblend.Colors = new Color[3] { _c1,_c2,_c3 };
            cblend.Positions = new float[3] { 0f,0.5f,1f };

            linearGradientBrush.InterpolationColors = cblend;

            e.Graphics.FillRectangle(linearGradientBrush,this.ClientRectangle);
        }
    }
}
