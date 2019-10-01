using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace MonkeyPaste {
    public partial class MpLogForm : MpResizableBorderlessForm {

        [DllImport("User32.dll")]
        public static extern int SetProcessDPIAware();

        public MpLogForm() : base() {
            //SetProcessDPIAware();
            InitializeComponent();
        }
    }
}
