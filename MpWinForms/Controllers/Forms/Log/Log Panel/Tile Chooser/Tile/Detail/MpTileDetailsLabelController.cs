using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileDetailsLabelController : MpController {
        public Label DetailsLabel { get; set; }

        private int _currentDetailId = 0;
        private MpCopyItem _copyItem;
        
        public MpTileDetailsLabelController(MpController parentController) : base(parentController) {
            DetailsLabel = new Label() {
                BackColor = Color.Transparent,
                ForeColor = Color.Black,//MpHelperSingleton.Instance.IsBright((((MpTileDetailsPanelController)Parent).TileDetailsPanel).BackColor) ? Color.Black : Color.White,
                BorderStyle = BorderStyle.None,
                TextAlign = ContentAlignment.MiddleCenter
            };
            DetailsLabel.DoubleBuffered(true);
            DetailsLabel.MouseEnter += DetailsTextBox_MouseEnter;
            //Link(new List<MpIView> { DetailsLabel });
        }
           public override void Update() {
            //tile details panel  rect
            Rectangle tpr = ((MpTilePanelController)Find("MpTilePanelController")).TilePanel.Bounds;
            //tile padding
            int tp = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tpr.Width);
            //tile details height
            int tdh = (int)(Properties.Settings.Default.TileDetailHeightRatio * tpr.Height);
            DetailsLabel.SetBounds(tp,tpr.Height - tdh - tp - tp,tpr.Width - (tp * 2),tdh);
            
            float fontSize = Properties.Settings.Default.TileDetailFontSizeRatio * (float)tpr.Height;
            fontSize = fontSize < 1.0f ? 10.0f : fontSize;
            DetailsLabel.Font = new Font(Properties.Settings.Default.TileDetailFont,fontSize,GraphicsUnit.Pixel);
           //DetailsTextBox.Location = new Point();
            DetailsLabel.Text = GetCurrentDetail(_currentDetailId);

            DetailsLabel.Invalidate();
        }
        private void DetailsTextBox_MouseEnter(object sender,EventArgs e) {
            if(++_currentDetailId > 2) {
                _currentDetailId = 0;
            }
            Update();
        }
        protected string GetCurrentDetail(int detailId) {
            MpCopyItem ci = ((MpTilePanelController)Find("MpTilePanelController")).CopyItem;
            string info = "I dunno";// string.Empty;
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
                    DataTable dt = MpAppManager.Instance.DataModel.Db.Execute("select * from MpPasteHistory where fk_MpCopyItemId=" + ci.CopyItemId);
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
