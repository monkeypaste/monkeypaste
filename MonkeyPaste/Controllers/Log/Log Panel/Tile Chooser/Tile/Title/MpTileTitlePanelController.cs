using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitlePanelController : MpController {
        //title panel
        public MpTileTitlePanel TileTitlePanel { get; set; }

        public MpTileTitleIconPanelController TileTitleIconPanelController { get; set; }
        public MpTileTitleTextBoxController TileTitleTextBoxController { get; set; }       
        

        public MpTileTitlePanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {            
            //parent panel
            TileTitlePanel = new MpTileTitlePanel(tileId,panelId) {
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            TileTitlePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;            
            
            TileTitleIconPanelController = new MpTileTitleIconPanelController(tileId,panelId,ci,this);
            TileTitlePanel.Controls.Add(TileTitleIconPanelController.TileTitleIconBox);

            TileTitleTextBoxController = new MpTileTitleTextBoxController(tileId,panelId,ci,this);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleLabel);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleTextBox);
            
            TileTitlePanel.BackColor = MpHelperSingleton.Instance.GetRandomColor();
            TileTitleTextBoxController.TileTitleTextBox.BackColor = TileTitlePanel.BackColor;
            TileTitleTextBoxController.TileTitleTextBox.ForeColor = MpHelperSingleton.Instance.IsBright(TileTitlePanel.BackColor) ? Color.Black : Color.White;

            Link(new List<MpIView> { TileTitlePanel});
        }
        public override void Update() {
            //tile rect
            Rectangle tr = ((MpTilePanelController)Find("MpTilePanelController")).TilePanel.Bounds;
            //tile header rect
            Rectangle thr = ((MpTilePanelController)Find("MpTilePanelController")).TileHeaderPanelController.TileHeaderPanel.Bounds;

            //tile title height
            int tth = (int)((float)tr.Height * Properties.Settings.Default.TileTitleHeightRatio);
            //tile padding
            int tp = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width);            
            TileTitlePanel.SetBounds(tp+tr.X,tp + thr.Bottom+tr.Y,tr.Width - tp,tth);

            TileTitleIconPanelController.Update();
            TileTitleTextBoxController.Update();
            TileTitleTextBoxController.TileTitleTextBox.BringToFront();

            TileTitlePanel.Invalidate();
        }
    }
}