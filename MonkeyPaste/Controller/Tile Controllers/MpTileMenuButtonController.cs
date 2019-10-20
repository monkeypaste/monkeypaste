using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpTileMenuButtonController : MpController {
        private int buttonId = 0;
        public MpButton TileMenuButton { get; set; }
        public MpTileMenuButtonController(int buttonId,string title,MpController parentController) : base(parentController) {
            this.buttonId = buttonId;
            TileMenuButton = new MpButton() {
                Text = title,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = (Color)MpSingletonController.Instance.GetSetting("TileMenuColor")
            };
        }

        public override void UpdateBounds() {
            int totalButtons = ((MpTileMenuPanelController)ParentController).TileMenuButtonControllerList.Count;
            Rectangle mr = ((MpTileMenuPanelController)ParentController).TileMenuPanel.Bounds;
            TileMenuButton.SetBounds((int)(mr.Width / totalButtons)*buttonId,0,(int)(mr.Width / totalButtons),mr.Height);
            Font buttonFont = new Font((string)MpSingletonController.Instance.GetSetting("TileMenuFont"),(float)mr.Height * (float)MpSingletonController.Instance.GetSetting("TileMenuFontRatio"),GraphicsUnit.Pixel);
            TileMenuButton.Font = buttonFont;
        }
    }
}
