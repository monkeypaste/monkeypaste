using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileDetailsTextBoxController : MpController {
        public MpTileDetailsTextBox DetailsTextBox { get; set; }

        private int _currentDetailId = 0;
        private MpCopyItem _copyItem;

        protected MpTileDetailsTextBoxController(MpController parentController) : base(parentController) { }

        public MpTileDetailsTextBoxController(int tileId,int panelId,MpController parentController) : base(parentController) {
            DetailsTextBox = new MpTileDetailsTextBox(tileId,panelId) {
                BackColor = Properties.Settings.Default.TileItemBgColor,
                ForeColor = MpHelperSingleton.Instance.IsBright((((MpTileDetailsPanelController)Parent).TileDetailsPanel).BackColor) ? Color.Black : Color.White,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Center
            };
            DetailsTextBox.MouseEnter += DetailsTextBox_MouseEnter;
            Link(new List<MpIView> { DetailsTextBox });
        }
        public override void Update() {
            //tile details panel  rect
            Rectangle tdpr = ((MpTileDetailsPanelController)Parent).TileDetailsPanel.Bounds;
            DetailsTextBox.SetBounds(0,0,tdpr.Width,tdpr.Height);

            float fontSize = Properties.Settings.Default.TileDetailFontSizeRatio * (float)tdpr.Height;
            fontSize = fontSize < 1.0f ? 10.0f : fontSize;
            DetailsTextBox.Font = new Font(Properties.Settings.Default.TileDetailFont,fontSize,GraphicsUnit.Pixel);
            DetailsTextBox.Location = new Point();
            DetailsTextBox.Text = GetCurrentDetail(_currentDetailId);

            DetailsTextBox.Invalidate();
        }
        private void DetailsTextBox_MouseEnter(object sender,EventArgs e) {
            if(++_currentDetailId > 2) {
                _currentDetailId = 0;
            }
            Update();
        }
        protected string GetCurrentDetail(int detailId) {
            MpCopyItem ci = ((MpTilePanelController)((MpTileDetailsPanelController)Parent).Parent).CopyItem;
            string info = string.Empty;
            switch(detailId) {
                //created
                case 0:
                    info = ci.CopyDateTime.ToString();
                    break;
                //chars/lines
                case 1:
                    if(ci.CopyItemType == MpCopyItemType.Image) {
                        Image ciimg = MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])ci.GetData());
                        info = "(" + ciimg.Width + ") x (" + ciimg.Height + ")";
                    }
                    else if(ci.CopyItemType == MpCopyItemType.Text) {
                        info = ((string)ci.GetData()).Length + " chars | " + MpHelperSingleton.Instance.GetLineCount((string)ci.GetData()) + " lines";
                    }
                    else if(ci.CopyItemType == MpCopyItemType.FileList) {
                        info = ((string[])ci.GetData()).Length + " files | " + MpHelperSingleton.Instance.FileListSize((string[])ci.GetData()) + " bytes";
                    }
                    break;
                //# copies/# pastes
                case 2:
                    DataTable dt = MpLogFormController.Db.Execute("select * from MpPasteHistory where fk_MpCopyItemId=" + ci.CopyItemId);
                    info = ci.CopyCount + " copies | " + dt.Rows.Count + " pastes";
                    break;                
                default:
                    info = "Unknown detailId: "+detailId;
                    break;
            }
            
            return info;
        }
    }
}
