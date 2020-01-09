using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuPanelController : MpController {
        public MpLogMainMenuPanel LogMainMenuPanel { get; set; }

        public MpLogMainMenuTitlePanelController LogMainMenuTitlePanelController { get; set; }

        public MpLogMainMenuInfoButtonController LogMainMenuInfoButtonController { get; set; }
        public MpLogMainMenuUserButtonController LogMainMenuUserButtonController { get; set; }

        public MpLogMainMenuPanelController(MpController parentController) : base(parentController) {
            LogMainMenuPanel = new MpLogMainMenuPanel() {
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = Color.Yellow
            };
            LogMainMenuTitlePanelController = new MpLogMainMenuTitlePanelController(this);
            LogMainMenuPanel.Controls.Add(LogMainMenuTitlePanelController.LogMainMenuTitlePanel);

            LogMainMenuInfoButtonController = new MpLogMainMenuInfoButtonController(this);
            LogMainMenuPanel.Controls.Add(LogMainMenuInfoButtonController.LogMainMenuInfoButton);
            LogMainMenuInfoButtonController.ButtonClickedEvent += LogMainMenuInfoButtonController_ButtonClickedEvent;

            LogMainMenuUserButtonController = new MpLogMainMenuUserButtonController(this);
            LogMainMenuPanel.Controls.Add(LogMainMenuUserButtonController.LogMainMenuUserButton);
            LogMainMenuUserButtonController.ButtonClickedEvent += LogMainMenuUserButtonController_ButtonClickedEvent;

            Link(new List<MpIView>() { LogMainMenuPanel });
        }

        private void LogMainMenuInfoButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            Console.WriteLine("Info button clicked");
        }
        private void LogMainMenuUserButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            Console.WriteLine("User button clicked");
        }
        public override void Update() {
            //logform rect
            Rectangle lfr = ((MpLogFormPanelController)((MpLogMenuPanelController)Parent).Parent).LogFormPanel.Bounds;
            //logform pad
            int lfp = (int)(lfr.Width * Properties.Settings.Default.LogPadRatio);
            //menu height
            int mh = (int)(((float)lfr.Height * Properties.Settings.Default.LogMenuHeightRatio)*0.5f);

            LogMainMenuPanel.SetBounds(0,0,lfr.Width,mh);

            LogMainMenuTitlePanelController.Update();
            LogMainMenuInfoButtonController.Update();
            LogMainMenuUserButtonController.Update();

            LogMainMenuPanel.Invalidate();
        }
    }
}
