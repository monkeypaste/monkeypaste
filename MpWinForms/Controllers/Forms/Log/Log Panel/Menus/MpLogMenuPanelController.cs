using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMenuPanelController : MpController {
        public Panel LogMenuPanel { get; set; }

        public MpLogMainMenuPanelController LogMainMenuPanelController { get; set; }
        public MpLogSubMenuPanelController LogSubMenuPanelController { get; set; }
        
        public MpLogMenuPanelController(MpController parentController) : base(parentController) {
            LogMenuPanel = new Panel() {
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            LogMenuPanel.DoubleBuffered(true);

            LogMainMenuPanelController = new MpLogMainMenuPanelController(this);
            LogMenuPanel.Controls.Add(LogMainMenuPanelController.LogMainMenuPanel);

            LogSubMenuPanelController = new MpLogSubMenuPanelController(this);
            LogMenuPanel.Controls.Add(LogSubMenuPanelController.LogSubMenuPanel);
        }

        public override void Update() {
            //logform rect
            Rectangle lfr = ((MpLogFormPanelController)Parent).LogFormPanel.Bounds;
            //logform pad
            int lfp = (int)(lfr.Width * Properties.Settings.Default.LogPadRatio);
            //menu height
            int mh = (int)((float)lfr.Height * Properties.Settings.Default.LogMenuHeightRatio);

            LogMenuPanel.SetBounds(0,0,lfr.Width,(int)((float)mh*1.5f));

            LogMainMenuPanelController.Update();
            LogSubMenuPanelController.Update();

            LogMenuPanel.Invalidate();
        }

        public override Rectangle GetBounds() {
            throw new NotImplementedException();
        }
    }
}
