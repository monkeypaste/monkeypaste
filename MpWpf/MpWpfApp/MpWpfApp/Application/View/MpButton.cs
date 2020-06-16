using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpWpfApp {
    public class MpButton : PictureBox {
        public Image DefaultImage { get; set; }
        public Image OverImage { get; set; }
        public Image DownImage { get; set; }        
    }
}
