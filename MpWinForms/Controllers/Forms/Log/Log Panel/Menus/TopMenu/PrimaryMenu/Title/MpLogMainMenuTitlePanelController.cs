using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuTitlePanelController:MpController {
        public MpLogMainMenuTitlePanel LogMainMenuTitlePanel { get; set; }

        public MpLogMainMenuTitleLinkLabelController LogMainMenuTitleLinkLabelController { get; set; }
        public MpLogMainMenuTitleIconBoxController LogMainMenuTitleIconBoxController { get; set; }

        public MpLogMainMenuTitlePanelController(MpController parentController) : base(parentController) {
            LogMainMenuTitlePanel = new MpLogMainMenuTitlePanel() {
                AutoSize = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None
            };

            LogMainMenuTitleIconBoxController = new MpLogMainMenuTitleIconBoxController(this);
            LogMainMenuTitlePanel.Controls.Add(LogMainMenuTitleIconBoxController.LogMainMenuTitleIconBox);

            LogMainMenuTitleLinkLabelController = new MpLogMainMenuTitleLinkLabelController(this);
            LogMainMenuTitlePanel.Controls.Add(LogMainMenuTitleLinkLabelController.LogMainMenuTitleLinkLabel);
        }

           public override void Update() {
            //log main menu panel rect
            Rectangle lmmpr = ((MpLogMainMenuPanelController)Parent).LogMainMenuPanel.Bounds;

            //log main menu title panel height
            int lmmtph = (int)(Properties.Settings.Default.LogMainMenuTitleHeightRatio * lmmpr.Height);
            Font titleFont = new Font(Properties.Settings.Default.LogMainMenuTitleFont,(float)lmmtph,FontStyle.Bold,GraphicsUnit.Pixel);
            Size lmmtps = TextRenderer.MeasureText("   Monkey Paste",titleFont);

            //center main menu panel
            Point cmmp = new Point((int)((float)lmmpr.Width / 2.0f)-(int)((float)lmmtps.Width/2.0f),0);

            LogMainMenuTitlePanel.SetBounds(cmmp.X,cmmp.Y,lmmtps.Width,lmmtps.Height);

            LogMainMenuTitleIconBoxController.Update();
            LogMainMenuTitleLinkLabelController.Update();

            lmmtps = TextRenderer.MeasureText("Monkey Paste",titleFont);
            lmmtps = new Size(lmmtps.Width + LogMainMenuTitleIconBoxController.LogMainMenuTitleIconBox.Width + 5,lmmtps.Height);

            cmmp = new Point((int)((float)lmmpr.Width / 2.0f) - (int)((float)lmmtps.Width / 2.0f),0);

            LogMainMenuTitlePanel.SetBounds(cmmp.X,cmmp.Y,lmmtps.Width,lmmtps.Height);

            LogMainMenuTitlePanel.Invalidate();
        }
        
    }
}
