
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
        
        public MpSearchTextBoxController LogMenuSearchTextBoxController { get; set; }

        public MpTagChooserPanelController LogMenuTileTokenChooserPanelController { get; set; }

        public MpSearchIconController SearchIconController { get; set; }

        public MpLogMenuPanelController(MpController Parent) : base(Parent) {
            LogMenuPanel = new MpLogMenuPanel() {
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            SearchIconController = new MpSearchIconController(this);
            LogMenuPanel.Controls.Add(SearchIconController.SearchIconBox);

            LogMenuSearchTextBoxController = new MpSearchTextBoxController(this);
            LogMenuPanel.Controls.Add(LogMenuSearchTextBoxController.SearchTextBox);

            LogMenuTileTokenChooserPanelController = new MpTagChooserPanelController(this,MpLogFormController.Db.GetTags());
            LogMenuPanel.Controls.Add(LogMenuTileTokenChooserPanelController.TagChooserPanel);

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

            SearchIconController.Update();
            LogMenuSearchTextBoxController.Update();
            LogMenuTileTokenChooserPanelController.Update();

            LogMenuPanel.Invalidate();
        }
    }
}
