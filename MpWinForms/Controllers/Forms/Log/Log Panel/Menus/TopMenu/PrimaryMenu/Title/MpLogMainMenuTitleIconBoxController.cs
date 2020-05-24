using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuTitleIconBoxController : MpController {
        public MpLogMainMenuTitleIconBox LogMainMenuTitleIconBox { get; set; }
        public MpLogMainMenuTitleIconBoxController(MpController parentController) : base(parentController) {
            LogMainMenuTitleIconBox = new MpLogMainMenuTitleIconBox() {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Image = Properties.Resources.monkey3,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None
            };

            //Link(new List<MpIView>() { LogMainMenuTitleIconBox });
        }

           public override void Update() {
            //log main menu title panel rect
            Rectangle lmmtpr = ((MpLogMainMenuTitlePanelController)Parent).LogMainMenuTitlePanel.Bounds;
            //icon pad
            int ip = (int)((float)lmmtpr.Height * Properties.Settings.Default.LogMainMenuIconSizeRatio);

            LogMainMenuTitleIconBox.SetBounds(0,0,ip,ip);

            LogMainMenuTitleIconBox.Invalidate();
        }
    }
}
