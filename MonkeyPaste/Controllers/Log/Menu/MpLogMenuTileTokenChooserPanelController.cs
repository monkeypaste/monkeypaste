using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMenuTileTokenChooserPanelController:MpController {
        public MpLogMenuTileTokenChooserPanel LogMenuTileTokenChooserPanel { get; set; }
        public MpLogMenuTileTokenAddTokenTextBoxController LogMenuTileTokenAddTokenTextBoxController { get; set; }

        public List<MpLogMenuTileTokenPanelController> TileTokenPanelControllerList = new List<MpLogMenuTileTokenPanelController>();

        public MpLogMenuTileTokenChooserPanelController(MpController parentController) : base(parentController) {
            LogMenuTileTokenChooserPanel = new MpLogMenuTileTokenChooserPanel() {
                AutoSize = false,
                BackColor = Properties.Settings.Default.LogMenuTileTokenChooserBgColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderThickness = 0,
                Radius = 10
            };
            LogMenuTileTokenAddTokenTextBoxController = new MpLogMenuTileTokenAddTokenTextBoxController(this);
            LogMenuTileTokenChooserPanel.Controls.Add(LogMenuTileTokenAddTokenTextBoxController.LogMenuTileTokenAddTokenTextBox);

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
            LogMenuTileTokenChooserPanel.Size = new Size(lmpr.Width - lmstr.Width - p * 2,h);
            LogMenuTileTokenChooserPanel.Location = new Point(lmstr.Right + 5,0);

            foreach(MpLogMenuTileTokenPanelController ttpc in TileTokenPanelControllerList) {
                ttpc.Update();
            }
            LogMenuTileTokenAddTokenTextBoxController.Update();
        }
        public int GetTokenId(MpLogMenuTileTokenPanelController ttpc) {
            for(int i = 0;i < TileTokenPanelControllerList.Count;i++) {
                if(TileTokenPanelControllerList[i] == ttpc) {
                    return i;
                }
            }
            return -1;
        }
        public MpLogMenuTileTokenPanelController GetToken(int tokenId) {
            foreach(MpLogMenuTileTokenPanelController ttpc in TileTokenPanelControllerList) {
                if(ttpc.TokenId == tokenId) {
                    return ttpc;
                }
            }
            return null;
        }
    }
}
