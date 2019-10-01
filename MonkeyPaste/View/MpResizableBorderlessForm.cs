using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Media;

namespace MonkeyPaste {
    
    public enum MpFormHT {
        Left = 10,
        Rig = 11,
        Top = 12,
        TopLeft = 13,
        TopRig = 14,
        Bottom = 15,
        BottomLeft = 16,
        BottomRight = 17
    }
    public partial class MpResizableBorderlessForm : Form {
        private const int _dragPad = 10;

        public MpResizableBorderlessForm() {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw,true);            
        }

        private void MpResizableBorderlessForm_Load(object sender,EventArgs e) {
            
        }

        protected override void WndProc(ref Message m) {
            if(m.Msg == 0x84) {  // Trap WM_NCHITTEST
                System.Drawing.Point pos = new System.Drawing.Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if(pos.Y >= -_dragPad && pos.Y <= _dragPad && pos.X >= -_dragPad && pos.X <= this.ClientSize.Width + _dragPad) {
                    m.Result = (IntPtr)MpFormHT.Top;//17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}
