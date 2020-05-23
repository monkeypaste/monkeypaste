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
    public partial class MpClipRow : UserControl {
        public MpClipRow() {
            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.DrawEllipse(Pens.Red, ClientRectangle);
            Font stringFont = new Font("Arial", 12);
            e.Graphics.DrawString(
                "Second custom control in the Windows Control Library", 
                stringFont, 
                Brushes.LightPink, 
                ClientRectangle
            );
            base.OnPaint(e);
        }
    }
}
