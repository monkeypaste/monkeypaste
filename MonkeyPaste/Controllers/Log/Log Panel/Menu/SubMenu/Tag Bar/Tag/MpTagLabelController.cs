using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagLabelController:MpController {
        public MpTagLinkLabel TagLinkLabel { get; set; }

        public MpTagLabelController(MpController parentController,string tokenText,Color tokenColor,bool _isEdit) : base(parentController) {
            TagLinkLabel = new MpTagLinkLabel(((MpTagChooserPanelController)((MpTagPanelController)parentController).Parent).TagPanelControllerList.IndexOf(((MpTagPanelController)Parent))) {
                TabIndex = 0,
                Visible = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Cursor = Cursors.Arrow,
                BackColor = tokenColor,
                BorderStyle = BorderStyle.None,
                TextAlign = ContentAlignment.MiddleLeft,
                LinkColor = MpHelperSingleton.Instance.IsBright(tokenColor) ? Color.Black:Color.White,
                Text = tokenText,
                LinkBehavior = LinkBehavior.HoverUnderline                
            };

            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = "http://www.dotnetperls.com/";
            TagLinkLabel.Links.Add(link);
            
        }
        public override void Update() {
            //token panel rect
            Rectangle tpr = ((MpTagPanelController)Parent).TagPanel.Bounds;
            //token panel height
            float tph = (float)tpr.Height * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio;
            //token chooser pad
            int tcp = tpr.Height - (int)tph;

            float fontSize = (float)tpr.Height * Properties.Settings.Default.LogMenuTileTokenFontSizeRatio;
            TagLinkLabel.Font = new Font(Properties.Settings.Default.LogMenuTileTokenFont,fontSize,GraphicsUnit.Pixel);
            TagLinkLabel.Size = TextRenderer.MeasureText(TagLinkLabel.Text+"  ",TagLinkLabel.Font);
            TagLinkLabel.Location = new Point((int)(fontSize/4.0f),-(int)(fontSize/6.0f));

            TagLinkLabel.Invalidate();
        }
    }
}
