using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpExcludedAppLabelController:MpController {
        public Label ExcludedAppLabel { get; set; }

        public MpExcludedAppLabelController(MpController parentController,string appName,Color tokenColor) : base(parentController) {
            ExcludedAppLabel = new Label() {
                TabIndex = 0,
                Visible = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Cursor = Cursors.Arrow,
                BackColor = tokenColor,
                BorderStyle = BorderStyle.None,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = appName               
            };
        }
           public override void Update() {
            //token panel rect
            Rectangle tpr = ((MpExcludedAppPanelController)Parent).ExcludedAppPanel.Bounds;
            //token panel height
            float tph = (float)tpr.Height * Properties.Settings.Default.TagPanelHeightRatio;
            //token chooser pad
            int tcp = tpr.Height - (int)tph;

            float fontSize = (float)tpr.Height * Properties.Settings.Default.TagFontSizeRatio;
            ExcludedAppLabel.Font = new Font(Properties.Settings.Default.TagFont,fontSize,GraphicsUnit.Pixel);
            ExcludedAppLabel.Size = TextRenderer.MeasureText(ExcludedAppLabel.Text+"  ",ExcludedAppLabel.Font);
            ExcludedAppLabel.Location = new Point((int)(fontSize/4.0f),-(int)(fontSize/6.0f));

            ExcludedAppLabel.Invalidate();
        }
    }
}
