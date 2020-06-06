using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagTextBoxController:MpController {
        public TextBox TagTextBox { get; set; }

        public MpTagTextBoxController(MpController parentController,string tokenText,Color tokenColor,bool _isEdit) : base(parentController) {
            TagTextBox = new TextBox() {
                ReadOnly = !_isEdit,
                Multiline = false,
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
        public override Rectangle GetBounds() {
            return base.GetBounds();
        }
        public override void Update() {
            //token panel rect
            Rectangle tpr = ((MpTagPanelController)Parent).GetBounds();

            float fontSize = (float)tpr.Height * Properties.Settings.Default.TagFontSizeRatio;
            fontSize = fontSize < 1.0f ? 10.0f : fontSize;
            TagTextBox.Font = new Font(Properties.Settings.Default.TagFont,fontSize,GraphicsUnit.Pixel);
            TagTextBox.Size = TextRenderer.MeasureText(TagTextBox.Text+"  ",TagTextBox.Font);
            TagTextBox.Location = new Point((int)(fontSize*Properties.Settings.Default.TagTextPadX),-(int)(fontSize*Properties.Settings.Default.TagTextPadY));

            TagTextBox.Invalidate();
        }
    }
}
