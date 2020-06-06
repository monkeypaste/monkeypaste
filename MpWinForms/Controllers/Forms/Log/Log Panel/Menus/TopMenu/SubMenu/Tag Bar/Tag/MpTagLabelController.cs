using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagLabelController:MpController {
        public LinkLabel TagLinkLabel { get; set; }

        public MpTagLabelController(MpController parentController,string tagText,Color tagColor,bool _isEdit) : base(parentController) {
            TagLinkLabel = new LinkLabel() {
                Visible = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Cursor = Cursors.Arrow,
                BackColor = tagColor,
                BorderStyle = BorderStyle.None,
                TextAlign = ContentAlignment.MiddleLeft,
                LinkColor = MpHelperSingleton.Instance.IsBright(tagColor) ? Color.Black:Color.White,
                Text = tagText,
                LinkBehavior = LinkBehavior.HoverUnderline                
            };
            TagLinkLabel.DoubleBuffered(true);

            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = "http://www.dotnetperls.com/";
            TagLinkLabel.Links.Add(link);
            
        }
        public Font GetFont() {
            //tag panel rect
            Rectangle tpr = ((MpTagPanelController)Parent).GetBounds();

            float fontSize = (float)tpr.Height * Properties.Settings.Default.TagFontSizeRatio;
            fontSize = fontSize < 1.0f ? 10.0f : fontSize;
            return new Font(Properties.Settings.Default.TagFont, fontSize, GraphicsUnit.Pixel);
        }
        public override Rectangle GetBounds() {
            Size labelSize = TextRenderer.MeasureText(TagLinkLabel.Text , GetFont());
            return new Rectangle(0,0, labelSize.Width, labelSize.Height);
        }
        public override void Update() {
            TagLinkLabel.Bounds = GetBounds();
            TagLinkLabel.Font = GetFont();

            TagLinkLabel.Invalidate();
        }
    }
}
