using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagTextBoxController:MpController {
        public MpTagTextBox TagTextBox { get; set; }

        public MpTagTextBoxController(MpController parentController,string tokenText,Color tokenColor,bool _isEdit) : base(parentController) {
            TagTextBox = new MpTagTextBox(((MpTagChooserPanelController)((MpTagPanelController)parentController).Parent).TagPanelControllerList.IndexOf(((MpTagPanelController)Parent))) {
                ReadOnly = !_isEdit,
                Multiline = false,
                TabIndex = 0,
                WordWrap = false,
                Margin = Padding.Empty,
                SelectionLength = 0,
                Padding = Padding.Empty,
                Cursor = Cursors.Arrow,
                BackColor = tokenColor,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Left,
                ForeColor = MpHelperSingleton.Instance.IsBright(tokenColor) ? Color.Black:Color.White,
                Text = tokenText
            };
            TagTextBox.DoubleBuffered(true);
        }
           public override void Update() {
            //token panel rect
            Rectangle tpr = ((MpTagPanelController)Parent).TagPanel.Bounds;
            //token panel height
            float tph = (float)tpr.Height * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio;
            //token chooser pad
            int tcp = tpr.Height - (int)tph;

            float fontSize = (float)tpr.Height * Properties.Settings.Default.LogMenuTileTokenFontSizeRatio;
            TagTextBox.Font = new Font(Properties.Settings.Default.LogMenuTileTokenFont,fontSize,GraphicsUnit.Pixel);
            TagTextBox.Size = TextRenderer.MeasureText(TagTextBox.Text+"  ",TagTextBox.Font);
            TagTextBox.Location = new Point((int)(fontSize/4.0f),-(int)(fontSize/6.0f));

            TagTextBox.Invalidate();
        }
    }
}
