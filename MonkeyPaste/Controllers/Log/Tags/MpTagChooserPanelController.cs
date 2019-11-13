using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagChooserPanelController:MpController {
        public MpTokenChooserPanel TagChooserPanel { get; set; }
        public MpAddTagTextBoxController AddTagTextBoxController { get; set; }

        public List<MpTagPanelController> TagPanelControllerList = new List<MpTagPanelController>();

        public MpTagChooserPanelController(MpController parentController,List<MpTag> tagList) : base(parentController) {
            TagChooserPanel = new MpTokenChooserPanel() {
                AutoSize = false,
                BackColor = Properties.Settings.Default.LogMenuTileTokenChooserBgColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderThickness = 0,
                Radius = 10
            };
            foreach(MpTag tag in tagList) {
                MpTagPanelController tpc = new MpTagPanelController(this,tag);
                TagPanelControllerList.Add(tpc);
                TagChooserPanel.Controls.Add(tpc.TagPanel);                
            }
            AddTagTextBoxController = new MpAddTagTextBoxController(this);
            TagChooserPanel.Controls.Add(AddTagTextBoxController.LogMenuTileTokenAddTokenTextBox);

            //TileTokenPanelControllerList.Add(new MpLogMenuTileTokenPanelController(this,0,"Hi how are you are you ok?",Color.Orange));
            //LogMenuTileTokenChooserPanel.Controls.Add(TileTokenPanelControllerList[0].LogMenuTileTokenPanel);
            //TileTokenPanelControllerList.Add(new MpLogMenuTileTokenPanelController(this,1,"I'm fat",Color.Orange));
            //LogMenuTileTokenChooserPanel.Controls.Add(TileTokenPanelControllerList[1].LogMenuTileTokenPanel);
        }
        public override void Update() {
            //log menu panel rect
            Rectangle lmpr = ((MpLogMenuPanelController)Parent).LogMenuPanel.Bounds;
            //log menu search textbox rect
            Rectangle lmstr = ((MpLogMenuPanelController)Parent).LogMenuSearchTextBoxController.SearchTextBox.Bounds;
            //padding
            //int h = (int)((float)lmpr.Height * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio);
            int h = lmpr.Height;
            int p = 0;
            TagChooserPanel.Size = new Size(lmpr.Width - lmstr.Width - p * 2,h);
            TagChooserPanel.Location = new Point(lmstr.Right + 5,0);

            foreach(MpTagPanelController ttpc in TagPanelControllerList) {
                ttpc.Update();
            }
            AddTagTextBoxController.Update();
        }
        public int GetTokenId(MpTagPanelController ttpc) {
            for(int i = 0;i < TagPanelControllerList.Count;i++) {
                if(TagPanelControllerList[i] == ttpc) {
                    return i;
                }
            }
            return -1;
        }
        public MpTagPanelController GetToken(int tokenId) {
            foreach(MpTagPanelController ttpc in TagPanelControllerList) {
                if(ttpc.TokenId == tokenId) {
                    return ttpc;
                }
            }
            return null;
        }
    }
}
