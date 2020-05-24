using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSearchTextBoxController : MpController {
        public MpSearchBox SearchTextBox { get; set; }

        public MpSearchTextBoxController(MpController Parent) : base(Parent) {
            SearchTextBox = new MpSearchBox() {
                ReadOnly = false,
                Multiline = false,
                WordWrap = false,
                BackColor = Color.White,
                ForeColor = Color.Black
            };

            //Link(new List<MpIView>() { SearchTextBox });
        }
           public override void Update() {
            //log menu panel rect
            Rectangle lmpr = ((MpLogSubMenuPanelController)Parent).LogSubMenuPanel.Bounds;
            //logform pad
            int lfp = (int)(lmpr.Width * Properties.Settings.Default.LogPadRatio); 
            SearchTextBox.Font = new Font(
                Properties.Settings.Default.LogMenuSearchFont,
                (int)((lmpr.Height-lfp-lfp)* Properties.Settings.Default.LogMenuSearchFontSizeRatio),
                GraphicsUnit.Pixel);
            SearchTextBox.SetBounds(lmpr.Height,lfp,(int)(lmpr.Width / 7),lmpr.Height - lfp - lfp);
            SearchTextBox.Invalidate();
        }
    }
}
