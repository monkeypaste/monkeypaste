using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMenuTileTokenPanelController : MpController {
        public int TokenId {
            get {
                return ((MpLogMenuTileTokenChooserPanelController)Parent).GetTokenId(this);
            }
        }

        public MpLogMenuTileTokenPanel LogMenuTileTokenPanel { get; set; }

        public MpLogMenuTileTokenTextBoxController LogMenuTileTokenTextBoxController { get; set; }
        public MpLogMenuTileTokenButtonController LogMenuTileTokenButtonController { get; set; }

        private string _tokenText;

        public MpLogMenuTileTokenPanelController(MpController parentController,string tokenText,Color tokenColor) : base(parentController) {
            _tokenText = tokenText;
            LogMenuTileTokenPanel = new MpLogMenuTileTokenPanel() {
                AutoSize = false,
                Radius = 5,
                BorderThickness = 0,
                BackColor = tokenColor == null ? MpHelperSingleton.Instance.GetRandomColor():tokenColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            LogMenuTileTokenTextBoxController = new MpLogMenuTileTokenTextBoxController(this,tokenText,LogMenuTileTokenPanel.BackColor);
            LogMenuTileTokenPanel.Controls.Add(LogMenuTileTokenTextBoxController.LogMenuTileTokenTextBox);

            LogMenuTileTokenButtonController = new MpLogMenuTileTokenButtonController(this);
            LogMenuTileTokenPanel.Controls.Add(LogMenuTileTokenButtonController.LogMenuTileTokenButton);
            LogMenuTileTokenButtonController.ButtonClickedEvent += LogMenuTileTokenButtonController_ButtonClickedEvent;
        }

        private void LogMenuTileTokenButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            LogMenuTileTokenPanel.Visible = false;
            ((MpLogMenuTileTokenChooserPanelController)Parent).TileTokenPanelControllerList.Remove(this);
            ((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.Controls.Remove(LogMenuTileTokenPanel);            
            LogMenuTileTokenPanel.Dispose();
            ((MpLogMenuTileTokenChooserPanelController)Parent).Update();
        }

        public override void Update() {
            //tile token chooser panel rect
            Rectangle ttcpr = ((MpLogMenuTileTokenChooserPanelController)Parent).LogMenuTileTokenChooserPanel.Bounds;
            //previous token rect
            Rectangle ptr = TokenId == 0 ? Rectangle.Empty:((MpLogMenuTileTokenChooserPanelController)Parent).GetToken(TokenId - 1).LogMenuTileTokenPanel.Bounds;

            //token panel height
            float tph = (float)ttcpr.Height*Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio;
            //token chooser pad
            int tcp = ttcpr.Height - (int)tph;        
            Font f = new Font(Properties.Settings.Default.LogMenuTileTokenFont,(float)ttcpr.Height,GraphicsUnit.Pixel);

            //text size
            Size ts = TextRenderer.MeasureText(LogMenuTileTokenTextBoxController.LogMenuTileTokenTextBox.Text,f);

            LogMenuTileTokenPanel.Size = new Size(ts.Width,(int)tph-tcp);
            LogMenuTileTokenPanel.Location = new Point(ptr.Right+tcp,tcp);
            
            LogMenuTileTokenButtonController.Update();
            LogMenuTileTokenTextBoxController.Update();

            LogMenuTileTokenPanel.Size = new Size(LogMenuTileTokenTextBoxController.LogMenuTileTokenTextBox.Width + (int)tph,LogMenuTileTokenPanel.Height);

            LogMenuTileTokenButtonController.Update(); //LogMenuTileTokenButtonController.LogMenuTileTokenButton.BringToFront();
        }
    }
}
