
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogSubMenuPanelController : MpController {
        public MpLogSubMenuPanel LogSubMenuPanel { get; set; }
        
        public MpSearchTextBoxController LogMenuSearchTextBoxController { get; set; }

        public MpTagChooserPanelController LogMenuTileTokenChooserPanelController { get; set; }

        public MpSearchIconController SearchIconController { get; set; }

        public MpLogSubMenuPanelController(MpController Parent) : base(Parent) {
            LogSubMenuPanel = new MpLogSubMenuPanel() {
                BackColor = Color.Brown,
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            SearchIconController = new MpSearchIconController(this);
            LogSubMenuPanel.Controls.Add(SearchIconController.SearchIconBox);

            LogMenuSearchTextBoxController = new MpSearchTextBoxController(this);
            LogSubMenuPanel.Controls.Add(LogMenuSearchTextBoxController.SearchTextBox);

            LogMenuTileTokenChooserPanelController = new MpTagChooserPanelController(this,MpAppManager.Instance.DataModel.Db.GetTags());
            LogSubMenuPanel.Controls.Add(LogMenuTileTokenChooserPanelController.TagChooserPanel);
        }
        public override void Update() {
            //logform rect
            Rectangle lfr = ((MpLogFormPanelController)((MpLogMenuPanelController)Parent).Parent).LogFormPanel.Bounds;
            //logform drag handle height
            int lfdhh = Properties.Settings.Default.LogResizeHandleHeight;
            //logform pad
            int lfp = (int)(lfr.Width * Properties.Settings.Default.LogPadRatio);
            //menu height
            int mh = (int)((float)lfr.Height * Properties.Settings.Default.LogMenuHeightRatio);

            LogSubMenuPanel.SetBounds(0,(int)((float)mh*0.5f),lfr.Width,mh);

            SearchIconController.Update();
            LogMenuSearchTextBoxController.Update();
            LogMenuTileTokenChooserPanelController.Update();

            LogSubMenuPanel.Invalidate();
        }
    }
}
