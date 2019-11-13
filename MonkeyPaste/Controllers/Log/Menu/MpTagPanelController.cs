using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagPanelController : MpController {
        public int TokenId {
            get {
                return ((MpTagChooserPanelController)Parent).GetTokenId(this);
            }
        }

        public MpTagPanel TagPanel { get; set; }

        public MpTagTextBoxController TagTextBoxController { get; set; }
        public MpTagButtonController TagButtonController { get; set; }

        private string _tokenText;

        public MpTagPanelController(MpController parentController,string tokenText,Color tokenColor) : base(parentController) {
            _tokenText = tokenText;
            TagPanel = new MpTagPanel() {
                AutoSize = false,
                Radius = 5,
                BorderThickness = 0,
                BackColor = tokenColor == null ? MpHelperSingleton.Instance.GetRandomColor():tokenColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            TagTextBoxController = new MpTagTextBoxController(this,tokenText,TagPanel.BackColor);
            TagPanel.Controls.Add(TagTextBoxController.TagTextBox);

            TagButtonController = new MpTagButtonController(this);
            TagPanel.Controls.Add(TagButtonController.LogMenuTagButton);
            TagButtonController.ButtonClickedEvent += LogMenuTileTokenButtonController_ButtonClickedEvent;
        }

        private void LogMenuTileTokenButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            TagPanel.Visible = false;
            ((MpTagChooserPanelController)Parent).TagPanelControllerList.Remove(this);
            ((MpTagChooserPanelController)Parent).TagChooserPanel.Controls.Remove(TagPanel);            
            TagPanel.Dispose();
            ((MpTagChooserPanelController)Parent).Update();
        }

        public override void Update() {
            //tile token chooser panel rect
            Rectangle ttcpr = ((MpTagChooserPanelController)Parent).TagChooserPanel.Bounds;
            //previous token rect
            Rectangle ptr = TokenId == 0 ? Rectangle.Empty:((MpTagChooserPanelController)Parent).GetToken(TokenId - 1).TagPanel.Bounds;

            //token panel height
            float tph = (float)ttcpr.Height*Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio;
            //token chooser pad
            int tcp = ttcpr.Height - (int)tph;        
            Font f = new Font(Properties.Settings.Default.LogMenuTileTokenFont,(float)ttcpr.Height,GraphicsUnit.Pixel);

            //text size
            Size ts = TextRenderer.MeasureText(TagTextBoxController.TagTextBox.Text,f);

            TagPanel.Size = new Size(ts.Width,(int)tph-tcp);
            TagPanel.Location = new Point(ptr.Right+tcp,tcp);
            
            TagButtonController.Update();
            TagTextBoxController.Update();

            TagPanel.Size = new Size(TagTextBoxController.TagTextBox.Width + (int)tph,TagPanel.Height);

            TagButtonController.Update(); //LogMenuTileTokenButtonController.LogMenuTileTokenButton.BringToFront();
        }
    }
}
