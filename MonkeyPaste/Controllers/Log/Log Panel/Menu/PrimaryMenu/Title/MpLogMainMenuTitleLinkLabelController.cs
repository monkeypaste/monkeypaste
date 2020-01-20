
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuTitleLinkLabelController:MpController {
        public MpLogMainMenuTitleLinkLabel LogMainMenuTitleLinkLabel { get; set; }

        public MpLogMainMenuTitleLinkLabelController(MpController parentController) : base(parentController) {
            LogMainMenuTitleLinkLabel = new MpLogMainMenuTitleLinkLabel() {
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Text = "MonkeyPaste",
                LinkBehavior = LinkBehavior.HoverUnderline
            };

            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = "http://www.dotnetperls.com/";
            LogMainMenuTitleLinkLabel.Links.Add(link);
        }

        public override void Update() {
            //log main menu title panel rect
            Rectangle lmmtpr = ((MpLogMainMenuTitlePanelController)Parent).LogMainMenuTitlePanel.Bounds;
            //log main menu title icon box rect
            Rectangle lmmtibr = ((MpLogMainMenuTitlePanelController)Parent).LogMainMenuTitleIconBoxController.LogMainMenuTitleIconBox.Bounds;

            LogMainMenuTitleLinkLabel.Font = new Font(Properties.Settings.Default.LogMainMenuTitleFont,(float)((float)lmmtpr.Height*Properties.Settings.Default.LogMainMenuTitleFontSizeRatio),FontStyle.Bold,GraphicsUnit.Pixel);
            Size labelSize = TextRenderer.MeasureText(LogMainMenuTitleLinkLabel.Text,LogMainMenuTitleLinkLabel.Font);

            LogMainMenuTitleLinkLabel.SetBounds(lmmtibr.Right + 5,0,labelSize.Width,labelSize.Height);

            LogMainMenuTitleLinkLabel.Invalidate();
        }
    }
}
