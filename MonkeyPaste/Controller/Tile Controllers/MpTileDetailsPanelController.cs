using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileDetailsPanelController : MpController {
        public TextBox DetailsTextBox { get; set; }
        public Panel TileDetailsPanel { get; set; }
        public MpTileDetailsPanelController(MpController parentController) : base(parentController) {
            TileDetailsPanel = new Panel() {
                AutoSize = false,
                BackColor = Color.Green//((MpTileTitlePanelController)ParentController).TileTitlePanel.BackColor
            };

            DetailsTextBox = new TextBox() {                
                BackColor = ((MpTileTitlePanelController)ParentController).TileTitlePanel.BackColor,
                BorderStyle = BorderStyle.None
            };
            TileDetailsPanel.Controls.Add(DetailsTextBox);
        }

        public override void UpdateBounds() {
            //tile title panel rect
            Rectangle ttpr = ((MpTileTitlePanelController)ParentController).TileTitlePanel.Bounds;
            //title title textbox recrt
            Rectangle tttbr = ((MpTileTitlePanelController)ParentController).TileTitleTextBoxController.TileTitleTextBox.Bounds;
            //tile title icon panel rect
            Rectangle ttipr = ((MpTileTitlePanelController)ParentController).TileTitleIconPanelController.TileTitleIconPanel.Bounds;
            //header icon pad
            int hip = (int)((float)ttpr.Width * (float)MpSingletonController.Instance.Settings.GetSetting("TilePadWidthRatio"));

            TileDetailsPanel.SetBounds(ttipr.Right + hip,tttbr.Bottom+hip,ttpr.Width - ttipr.Width - (hip * 3),ttpr.Height - hip - hip);

            MpCopyItem ci = ((MpTilePanelController)((MpTileTitlePanelController)ParentController).ParentController).CopyItem;
            string info = "UNKNOWN";
            if(ci.copyItemTypeId == MpCopyItemType.Image) {
                Image ciimg = MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])ci.GetData());
                info = "("+ciimg.Width + ") x (" + ciimg.Height+")";
            } else if(ci.copyItemTypeId == MpCopyItemType.Text) {
                info = ((string)ci.GetData()).Length + " chars | " + MpHelperSingleton.Instance.GetLineCount((string)ci.GetData()) + " lines";
            } else if(ci.copyItemTypeId == MpCopyItemType.FileList) {
                info = ((string[])ci.GetData()).Length + " files | "+MpHelperSingleton.Instance.FileListSize((string[])ci.GetData())+ " bytes";
            }
            DetailsTextBox.Text = info;

            float fontSize = 25.0f;// (float)MpSingletonController.Instance.GetSetting("TileDetailFontSizeRatio") * (float)(ttpr.Height - hip * 2);
            DetailsTextBox.Font = new Font((string)MpSingletonController.Instance.GetSetting("TileDetailFont"),fontSize,GraphicsUnit.Pixel);
            DetailsTextBox.ForeColor = (Color)MpSingletonController.Instance.GetSetting("TileDetailFontColor");
            DetailsTextBox.TextAlign = HorizontalAlignment.Left;
            DetailsTextBox.Location = new Point();
            
            DetailsTextBox.BringToFront();
            TileDetailsPanel.BringToFront();
        }
    }
}
