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
        public MpCopyItemTileChooserPanel() : base() {
            this.DoubleBuffered = true;
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(this.ClientRectangle,Color.Red,Color.Yellow,45);

            ColorBlend cblend = new ColorBlend(3);
            cblend.Colors = new Color[3] { Color.Red,Color.Yellow,Color.Green };
            cblend.Positions = new float[3] { 0f,0.5f,1f };

            linearGradientBrush.InterpolationColors = cblend;

            e.Graphics.FillRectangle(linearGradientBrush,this.ClientRectangle);
        }
    }
}
