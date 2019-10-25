using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace MonkeyPaste {
    public class MpTilePanelController : MpController {     
        //public static int TotalTileCount = 0;

        public int TileId { get; set; } = -1;
        public MpTilePanel TilePanel { get; set; }
        public MpTileControlController TileControlController { get; set; }
        public MpTileTitlePanelController TileTitlePanelController { get; set; }
        public MpTileMenuPanelController TileMenuPanelController { get; set; }

        public Color TileColor { get; set; }
               
        public MpCopyItem CopyItem { get; set; }

        private MpKeyboardHook _escKeyHook,_spaceKeyHook;

        public MpTilePanelController(int tileId,MpCopyItem ci,MpController parentController) : base(parentController) {
            TileId = tileId;
            CopyItem = ci;
            TileColor = (Color)MpSingletonController.Instance.GetSetting("TileColor1");

            TilePanel = new MpTilePanel() {
                AutoScroll = true,
                AutoSize = false,
                BackColor = TileColor
            };
            TilePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileTitlePanelController = new MpTileTitlePanelController(ci,this);
            TilePanel.Controls.Add(TileTitlePanelController.TileTitlePanel);

            TileControlController = new MpTileControlController(ci,this);
            TilePanel.Controls.Add(TileControlController.ItemControl);
            TileControlController.ItemControl.BringToFront();

            TileMenuPanelController = new MpTileMenuPanelController(this);
            TilePanel.Controls.Add(TileMenuPanelController.TileMenuPanel);

            UpdateBounds();
        }
        public override void UpdateBounds() {
            //tile chooser panel rect
            Rectangle tcpr = ((MpTileChooserPanelController)ParentController).TileChooserPanel.Bounds;
            //this tile's idx
            int idx = TileId;
            //total tile's
            int tidx = ((MpTileChooserPanelController)ParentController).TileControllerList.Count;
            //tile padding
            int tp = (int)((float)MpSingletonController.Instance.Settings.GetSetting("TileChooserPadHeightRatio") * tcpr.Height);
           //tile size
            int ts = tcpr.Height - (int)(tp*2);

            TilePanel.SetBounds(((tidx-idx-1)*tcpr.Height+tp),tp,ts,ts);
            TilePanel.Thickness = (int)MpSingletonController.Instance.GetSetting("TileBorderThickness");
            //TileTitlePanelController.TileTitlePanel.Thickness = (int)MpSingletonController.Instance.GetSetting("TileBorderThickness");
            TileTitlePanelController.UpdateBounds();
            TileControlController.UpdateBounds();
            TileMenuPanelController.UpdateBounds();

            TilePanel.Refresh();
        }
        public void ShowMenu() {            
                TileMenuPanelController.TileMenuPanel.Visible = true;
                TileMenuPanelController.TileMenuButtonControllerList[0].TileMenuButton.ButtonOver();
                TileMenuPanelController.TileMenuButtonControllerList[0].TileMenuButton.Refresh();
                TileMenuPanelController.TileMenuPanel.BringToFront();
        }
        public void HideMenu() {
            TileMenuPanelController.TileMenuPanel.Visible = false;
        }

        public void SetFocus(bool isFocused) {
            if(isFocused) {
                TilePanel.BorderColor = (Color)MpSingletonController.Instance.GetSetting("TileFocusColor");
                TileControlController.ItemControl.Enabled = true;
                //ShowMenu();
            } else {
                TilePanel.BorderColor = (Color)MpSingletonController.Instance.GetSetting("TileUnfocusColor");
                TileControlController.ItemControl.Enabled = false;
                //HideMenu();
            }
            //TileTitlePanelController.TileTitlePanel.BorderColor = TilePanel.BorderColor;
        }
        public void ActivateHotKeys() {
        }      

        public void DeactivateHotKeys() {
        }
    }
}
