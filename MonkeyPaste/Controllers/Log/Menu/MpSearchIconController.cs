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
                Image = Properties.Resources.search,
                SizeMode = PictureBoxSizeMode.AutoSize,
                BackColor = Color.Transparent
            };
            Link(new List<MpIView>() { SearchIconBox });
        }

        public override void Update() {
            //log menu rect
            Rectangle lmr = ((MpLogMenuPanelController)Parent).LogMenuPanel.Bounds;
            SearchIconBox.SetBounds(0,0,lmr.Height,lmr.Height);
        }
    }
}
