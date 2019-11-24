using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogFormPanelController : MpController {
        public MpLogFormPanel LogFormPanel { get; set; }
        private bool _isFirstLoad = true;

        public MpLogFormPanelController(MpController parentController) : base(parentController) {
            LogFormPanel = new MpLogFormPanel() {
                AutoSize = false,
                BorderStyle = BorderStyle.None,
                MinimumSize = new Size(15,200)
            };
        }

        public override void Update() {
            //log form rect
            Rectangle lfr = ((MpLogFormController)Parent).LogForm.Bounds;

            int h = _isFirstLoad ? (int)((float)lfr.Height * Properties.Settings.Default.LogScreenHeightRatio) : LogFormPanel.Height;

            LogFormPanel.SetBounds(0,lfr.Height - h,lfr.Width,h);
        }
    }
}
