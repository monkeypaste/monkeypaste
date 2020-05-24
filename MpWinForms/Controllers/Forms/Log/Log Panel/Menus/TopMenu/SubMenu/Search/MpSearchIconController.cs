using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSearchIconController : MpController {
        public MpSearchIconBox SearchIconBox { get; set; }

        public MpSearchIconController(MpController parentController) : base(parentController) {
            SearchIconBox = new MpSearchIconBox() {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Image = Properties.Resources.search,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None
            };
            //Link(new List<MpIView>() { SearchIconBox });
        }

           public override void Update() {//log menu panel rect
            //log menu rect
            Rectangle lmr = ((MpLogSubMenuPanelController)Parent).LogSubMenuPanel.Bounds;
            SearchIconBox.SetBounds(0,5,lmr.Height,lmr.Height);
            
            SearchIconBox.Invalidate();
        }
    }
}
