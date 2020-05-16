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
            TagChooserPanel.Controls.Add(AddTagTextBoxController.AddTagTextBox);
        }
        public override void Update() {
            //log menu panel rect
            Rectangle lmpr = ((MpLogSubMenuPanelController)Parent).LogSubMenuPanel.Bounds;
            //log menu search textbox rect
            Rectangle lmstr = ((MpLogSubMenuPanelController)Parent).LogMenuSearchTextBoxController.SearchTextBox.Bounds;
            //padding
            int lfp = (int)(lmpr.Height * Properties.Settings.Default.LogPadRatio);
            //int h = (int)((float)lmpr.Height * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio);
            int h = lmpr.Height - lfp - lfp;
            int p = 0;
            TagChooserPanel.Size = new Size(lmpr.Width - lmstr.Right - 10,(int)((float)lmstr.Height*Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio));
            TagChooserPanel.Location = new Point(lmstr.Right + 5,lmstr.Y);

            foreach(MpTagPanelController ttpc in TagPanelControllerList) {
                ttpc.Update();
            }
            AddTagTextBoxController.Update();

            TagChooserPanel.Invalidate();
        }
        //ci should always be a newly selected tile
        public void UpdateTagListState(MpCopyItem ci) {
            foreach(MpTagPanelController tpc in TagPanelControllerList) {
                tpc.SetTagState(tpc.Tag.IsLinkedWithCopyItem(ci) ? MpTagPanelState.Selected:MpTagPanelState.Inactive);
            }
            Update();
        }
        public int GetTagId(MpTagPanelController ttpc) {
            for(int i = 0;i < TagPanelControllerList.Count;i++) {
                if(TagPanelControllerList[i] == ttpc) {
                    return i;
                }
            }
            return -1;
        }
        public List<MpTagPanelController> GetFocusedTagList() {
            List<MpTagPanelController> focusedTagList = new List<MpTagPanelController>();

            foreach(MpTagPanelController tpc in TagPanelControllerList) {
                if(tpc.TagPanelState == MpTagPanelState.Focused) {
                    focusedTagList.Add(tpc);
                }
            }

            return focusedTagList;
        }
    }
}
