
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

        public MpTagChooserPanelController TileTagChooserPanelController { get; set; }

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

            TileTagChooserPanelController = new MpTagChooserPanelController(this,MpApplication.Instance.DataModel.Db.GetTags());
            LogSubMenuPanel.Controls.Add(TileTagChooserPanelController.TagChooserPanel);
        }
        public override Rectangle GetBounds() {
            //logform rect
            Rectangle lfr = ((MpLogFormPanelController)((MpLogMenuPanelController)Parent).Parent).LogFormPanel.Bounds;
            //logform drag handle height
            int lfdhh = Properties.Settings.Default.LogResizeHandleHeight;
            //logform pad
            int lfp = (int)(lfr.Width * Properties.Settings.Default.LogPadRatio);
            //menu height
            int mh = (int)((float)lfr.Height * Properties.Settings.Default.LogMenuHeightRatio);

            return new Rectangle(0, (int)((float)mh * 0.5f), lfr.Width, mh);
        }
        public override void Update() {
            LogSubMenuPanel.Bounds = GetBounds();

            SearchIconController.Update();
            LogMenuSearchTextBoxController.Update();
            TileTagChooserPanelController.Update();

            LogSubMenuPanel.Invalidate();
        }
    }
}
