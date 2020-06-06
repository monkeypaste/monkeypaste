using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuPanelController : MpController {
        public Panel LogMainMenuPanel { get; set; }

        public MpLogMainMenuSettingsButtonController LogMainMenuSettingsButtonController { get; set; }
        public MpLogMainMenuAppendModeButtonController LogMainMenuAppendModeButtonController { get; set; }
        public MpLogMainMenuAutoCopyButtonController LogMainMenuAutoCopyButtonController { get; set; }

        public MpLogMainMenuTitlePanelController LogMainMenuTitlePanelController { get; set; }

        public MpLogMainMenuInfoButtonController LogMainMenuInfoButtonController { get; set; }
        public MpLogMainMenuUserButtonController LogMainMenuUserButtonController { get; set; }

        public MpLogMainMenuPanelController(MpController parentController) : base(parentController) {
            LogMainMenuPanel = new Panel() {
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = Color.Yellow
            };
            LogMainMenuPanel.DoubleBuffered(true);
            LogMainMenuSettingsButtonController = new MpLogMainMenuSettingsButtonController(this);
            LogMainMenuPanel.Controls.Add(LogMainMenuSettingsButtonController.LogMainMenuSettingsButton);
            LogMainMenuSettingsButtonController.ButtonClickedEvent += LogMainMenuSettingsButtonController_ButtonClickedEvent;

            LogMainMenuAppendModeButtonController = new MpLogMainMenuAppendModeButtonController(this);
            LogMainMenuPanel.Controls.Add(LogMainMenuAppendModeButtonController.LogMainMenuAppendModeButton);
            LogMainMenuAppendModeButtonController.ButtonClickedEvent += LogMainMenuAppendModeButtonController_ButtonClickedEvent;

            LogMainMenuAutoCopyButtonController = new MpLogMainMenuAutoCopyButtonController(this);
            LogMainMenuPanel.Controls.Add(LogMainMenuAutoCopyButtonController.LogMainMenuAutoCopyButton);
            LogMainMenuAutoCopyButtonController.ButtonClickedEvent += LogMainMenuAutoCopyButtonController_ButtonClickedEvent;

            LogMainMenuTitlePanelController = new MpLogMainMenuTitlePanelController(this);
            LogMainMenuPanel.Controls.Add(LogMainMenuTitlePanelController.LogMainMenuTitlePanel);

            LogMainMenuInfoButtonController = new MpLogMainMenuInfoButtonController(this);
            LogMainMenuPanel.Controls.Add(LogMainMenuInfoButtonController.LogMainMenuInfoButton);
            LogMainMenuInfoButtonController.ButtonClickedEvent += LogMainMenuInfoButtonController_ButtonClickedEvent;

            LogMainMenuUserButtonController = new MpLogMainMenuUserButtonController(this);
            LogMainMenuPanel.Controls.Add(LogMainMenuUserButtonController.LogMainMenuUserButton);
            LogMainMenuUserButtonController.ButtonClickedEvent += LogMainMenuUserButtonController_ButtonClickedEvent;

            ////Link(new List<MpIView>() { LogMainMenuPanel });
        }
        public override Rectangle GetBounds() {
            //logform rect
            Rectangle lfr = ((MpLogFormPanelController)((MpLogMenuPanelController)Parent).Parent).LogFormPanel.Bounds;
            //logform pad
            int lfp = (int)(lfr.Width * Properties.Settings.Default.LogPadRatio);
            //menu height
            int mh = (int)(((float)lfr.Height * Properties.Settings.Default.LogMenuHeightRatio) * 0.5f);

            return new Rectangle(0, 0, lfr.Width, mh);
        }
        public override void Update() {
            LogMainMenuPanel.Bounds = GetBounds();

            LogMainMenuTitlePanelController.Update();

            LogMainMenuInfoButtonController.Update();
            LogMainMenuUserButtonController.Update();

            LogMainMenuSettingsButtonController.Update();
            LogMainMenuAppendModeButtonController.Update();
            LogMainMenuAutoCopyButtonController.Update();

            LogMainMenuPanel.Invalidate();
        }

        private void LogMainMenuAutoCopyButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            Console.WriteLine("AutoCopy button clicked");
        }

        private void LogMainMenuAppendModeButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            Console.WriteLine("AppendMode button clicked");
        }

        private void LogMainMenuSettingsButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            Console.WriteLine("Settings button clicked");
        }

        private void LogMainMenuInfoButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            Console.WriteLine("Info button clicked");
        }
        private void LogMainMenuUserButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            Console.WriteLine("User button clicked");
        }
    }
}
