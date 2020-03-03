using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAdvancedPanel : BevelPanel.AdvancedPanel,MpIView {
        private System.Windows.Forms.Panel panel1;

        public MpAdvancedPanel() : base() {
            this.DoubleBuffered = true;

            ViewType = this.GetType().ToString();
            ViewId = MpSingletonController.Instance.Rand.Next(1,int.MaxValue);
            ViewName = ViewType+"_"+ViewId;
            ViewData = this;
            RectRadius = 50;
            EdgeWidth = 38;
        }
        public string ViewType { get; set; }
        public string ViewName { get; set; }
        public int ViewId { get; set; }
        public object ViewData { get; set; }

        private void InitializeComponent() {
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 100);
            this.panel1.TabIndex = 0;
            this.ResumeLayout(false);

        }
    }
}
