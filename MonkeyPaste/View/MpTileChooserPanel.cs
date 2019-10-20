using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileChooserPanel : ScrollableControl {
        public Color C1 { get; set; }
        public Color C2 { get; set; }
        public Color C3 { get; set; }  

        public MpTileChooserPanel() : base() {
            C1 = (Color)MpSingletonController.Instance.GetSetting("TileChooserBgColor1");
            C2 = (Color)MpSingletonController.Instance.GetSetting("TileChooserBgColor2");
            C3 = (Color)MpSingletonController.Instance.GetSetting("TileChooserBgColor3");
            this.DoubleBuffered = true;
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(this.ClientRectangle,C1,C2,90);

            ColorBlend cblend = new ColorBlend(3);
            cblend.Colors = new Color[3] { C1,C2,C3 };
            cblend.Positions = new float[3] { 0f,0.5f,1f };

            linearGradientBrush.InterpolationColors = cblend;

            e.Graphics.FillRectangle(linearGradientBrush,this.ClientRectangle);
        }
    }
}
