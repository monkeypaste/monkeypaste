
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMenuPanelController : MpController {
        public MpLogMenuPanel LogMenuPanel { get; set; }
        public PictureBox SearchIconBox { get; set; }
        public MpLogMenuSearchTextBoxController LogMenuSearchTextBoxController { get; set; }
        public MpLogMenuTileTokenChooserPanelController LogMenuTileTokenChooserPanelController { get; set; }

        public MpLogMenuPanelController(MpController Parent) : base(Parent) {
            LogMenuPanel = new MpLogMenuPanel() {
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            SearchIconBox = new PictureBox() {
                Image = Properties.Resources.search,
                SizeMode = PictureBoxSizeMode.AutoSize,
                BackColor = Color.Transparent
            };
            //LogMenuPanel.Controls.Add(SearchIconBox);

            LogMenuSearchTextBoxController = new MpLogMenuSearchTextBoxController(this);
            LogMenuPanel.Controls.Add(LogMenuSearchTextBoxController.SearchTextBox);

            LogMenuTileTokenChooserPanelController = new MpLogMenuTileTokenChooserPanelController(this);
            LogMenuPanel.Controls.Add(LogMenuTileTokenChooserPanelController.LogMenuTileTokenChooserPanel);

            Link(new List<MpIView> { LogMenuPanel });
        }

        public override void Update() {
            //logform rect
            Rectangle lfr = ((MpLogFormController)Parent).LogForm.Bounds;
            //logform drag handle height
            int lfdhh = Properties.Settings.Default.LogResizeHandleHeight;
            //logform pad
            int lfp = (int)(lfr.Width * Properties.Settings.Default.LogPadRatio);
            //menu height
            int mh = (int)((float)lfr.Height * Properties.Settings.Default.LogMenuHeightRatio);

            LogMenuPanel.SetBounds(0, lfdhh,lfr.Width,mh-lfdhh);
            SearchIconBox.SetBounds(0,0,mh,mh);

            LogMenuSearchTextBoxController.Update();
            LogMenuTileTokenChooserPanelController.Update();
        }
    }
}
