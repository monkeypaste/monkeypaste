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

            Link(new List<MpIView>() { SearchTextBox });
        }
        public override void Update() {
            //log menu panel rect
            Rectangle lmpr = ((MpLogMenuPanelController)Parent).LogMenuPanel.Bounds;
            SearchTextBox.SetBounds(lmpr.Height,0,(int)(lmpr.Width / 7),lmpr.Height);
            SearchTextBox.Font = new Font(Properties.Settings.Default.LogMenuSearchFont,(int)(lmpr.Height* Properties.Settings.Default.LogMenuSearchFontSizeRatio),GraphicsUnit.Pixel);
        }
    }
}
