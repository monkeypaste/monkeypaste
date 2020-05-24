using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpControlViewFormat {
        public MpBoxModel Margin, Padding, Border;
        public Color BackColor, ForeColor, BorderColor,ShadowColor;
        public int CornerRadius, ShadowOffsetX, ShadowOffsetY;
    }
    public abstract class MpControlController : MpController {
        public MpControlViewFormat ControlViewFormat { get; set; }

        public MpControlController(MpController p) : base(p) {}

        public abstract void Update();

        public abstract Rectangle GetBounds();
    }
}
