using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace MonkeyPaste {
    public partial class MpLogForm : MpResizableBorderlessForm, MpIView {
        public string ViewType {get;set;}
        public string ViewName {get;set;}
        public int ViewId {get;set;}
        public object ViewData {get;set;}

        /*const int AW_SLIDE = 0X00040000;
        const int AW_HOR_POSITIVE = 0x00000001;
        const int AW_HOR_NEGATIVE = 0x00000002;
        const int AW_VER_POSITIVE = 0x00000004;
        const int AW_VER_NEGATIVE = 0x00000008;
        const int AW_BLEND = 0x00080000;
        public bool SlideUp = true;

        [DllImport("user32")]
        static extern bool AnimateWindow(IntPtr hwnd,int time,int flags);*/
        

        /*protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            if(SlideUp) {
                this.Location = new Point(this.Location.X,this.Location.Y - this.Height);
                AnimateWindow(this.Handle,500,AW_SLIDE | AW_VER_NEGATIVE);
                SlideUp = false;
            }
        }*/
        public MpLogForm() : base() {
            this.DoubleBuffered = true;
            ViewType = this.GetType().ToString();
            ViewName = ViewType;
            ViewId = MpSingletonController.Instance.Rand.Next(1,int.MaxValue);
            ViewData = this;
            InitializeComponent();
            WinApi.SetProcessDPIAware();
        }
        
    }
}
