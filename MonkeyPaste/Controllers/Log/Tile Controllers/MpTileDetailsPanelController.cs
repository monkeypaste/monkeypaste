using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileDetailsPanelController : MpController {
        public MpTileDetailsTextBox DetailsTextBox { get; set; }
        public MpTileDetailPanel TileDetailsPanel { get; set; }

        public MpTileDetailsPanelController(int tileId,int panelId,MpController Parent) : base(Parent) {
            TileDetailsPanel = new MpTileDetailPanel(tileId,panelId) {
                AutoSize = false,
                BackColor = Properties.Settings.Default.TileItemBgColor,
                BorderStyle = BorderStyle.None
            };

            DetailsTextBox = new MpTileDetailsTextBox(tileId,panelId) {
                BackColor = Properties.Settings.Default.TileItemBgColor,
                ForeColor = MpHelperSingleton.Instance.IsBright(TileDetailsPanel.BackColor) ? Color.Black:Color.White,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Center
            };

            MpCopyItem ci = ((MpTilePanelController)Parent).CopyItem;
            string info = "UNKNOWN";
            if(ci.copyItemTypeId == MpCopyItemType.Image) {
                Image ciimg = MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])ci.GetData());
                info = "(" + ciimg.Width + ") x (" + ciimg.Height + ")";
            }
            else if(ci.copyItemTypeId == MpCopyItemType.Text) {
                info = ((string)ci.GetData()).Length + " chars | " + MpHelperSingleton.Instance.GetLineCount((string)ci.GetData()) + " lines";
            }
            else if(ci.copyItemTypeId == MpCopyItemType.FileList) {
                info = ((string[])ci.GetData()).Length + " files | " + MpHelperSingleton.Instance.FileListSize((string[])ci.GetData()) + " bytes";
            }
            DetailsTextBox.Text = info;
            TileDetailsPanel.Controls.Add(DetailsTextBox);

            Link(new List<MpIView> { DetailsTextBox,TileDetailsPanel });
        }

        public override void Update() {
            //tile  rect
            Rectangle tr = ((MpTilePanelController)Parent).TilePanel.Bounds;
            //tile padding
            int tp = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width);
            //tile details height
            int tdh = (int)(Properties.Settings.Default.TileDetailHeightRatio * tr.Height);
            TileDetailsPanel.SetBounds(tp,tr.Height-tdh-tp,tr.Width-(tp*2),tdh);
            DetailsTextBox.SetBounds(0,0,TileDetailsPanel.Width,TileDetailsPanel.Height);
                     
            float fontSize = Properties.Settings.Default.TileDetailFontSizeRatio * (float)tdh;
            DetailsTextBox.Font = new Font(Properties.Settings.Default.TileDetailFont,fontSize,GraphicsUnit.Pixel);           
            DetailsTextBox.Location = new Point();
        }
    }
}
