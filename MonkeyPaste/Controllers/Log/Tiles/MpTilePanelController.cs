using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using System.Windows.Forms.Integration;

namespace MonkeyPaste {
    public class MpTilePanelController : MpController, IComparable {
        public static int OffsetX { get; set; } = 0;

        public static int SelectedTileId { get; set; }

        public int TileId { get; set; } = -1;
        //public int SortOrder { get; set; } = 0;

        public MpTilePanel TilePanel { get; set; }

        public MpTileControlController TileControlController { get; set; }
        public MpTileTitlePanelController TileTitlePanelController { get; set; }
        //public MpTileMenuPanelController TileMenuPanelController { get; set; }
        public MpTileDetailsPanelController TileDetailsPanelController { get; set; }

        public MpMouseHook MouseOverItemControlHook { get; set; }

        public MpCopyItem CopyItem { get; set; }

        public static MpKeyboardHook EscKeyHook, SpaceKeyHook;        

        //public delegate void SelectionToggled(object sender,bool isActive);
        //public event SelectionToggled SelectionToggledEvent;

        private bool _isSelected = false;

        public MpTilePanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            TileId = tileId;
            CopyItem = ci;

            TilePanel = new MpTilePanel(tileId,panelId) {
                AutoScroll = false,
                AutoSize = false,
                BackColor = MpHelperSingleton.Instance.GetRandomColor(),
                Radius = Properties.Settings.Default.TileBorderRadius
            };
            TilePanel.BorderColor = TilePanel.BackColor;
            TilePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileTitlePanelController = new MpTileTitlePanelController(tileId,panelId,ci,this);
            TilePanel.Controls.Add(TileTitlePanelController.TileTitlePanel);            

            TileDetailsPanelController = new MpTileDetailsPanelController(tileId,panelId,this);
            TilePanel.Controls.Add(TileDetailsPanelController.TileDetailsPanel);
            
            TileControlController = new MpTileControlController(tileId,panelId,ci,this);
            TilePanel.Controls.Add(TileControlController.ItemPanel);

            //TileMenuPanelController = new MpTileMenuPanelController(tileId,panelId,this);

            MouseOverItemControlHook = new MpMouseHook();
            MouseOverItemControlHook.MouseEvent += _mouseOverItemControlHook_MouseEvent;

            Link(new List<MpIView> { TilePanel });
        }


        private void _mouseOverItemControlHook_MouseEvent(object sender,Gma.System.MouseKeyHook.MouseEventExtArgs e) {
            //Console.WriteLine("Chuck chatted too much");

        }
        public override void Update() {
            Update(((MpTileChooserPanelController)Parent).TileChooserPanel.Bounds,new List<float>() { Properties.Settings.Default.TileChooserPadHeightRatio },new List<bool>() { true },new List<bool> { true });
            //if(TilePanel.Visible == false) {
            //    return;
            //}
            //tile chooser panel rect
            Rectangle tcpr = ((MpTileChooserPanelController)Parent).TileChooserPanel.Bounds;
            int listIdx = ((MpTileChooserPanelController)Parent).TileControllerList.IndexOf(this);
            //tile padding
            int tp = (int)(Properties.Settings.Default.TileChooserPadHeightRatio * tcpr.Height);
           //tile size
            int ts = tcpr.Height - (int)(tp*2);
            TilePanel.Location = new System.Drawing.Point((listIdx * tcpr.Height + tp)+OffsetX,tp);
            TilePanel.Size = new System.Drawing.Size(ts,ts);

            TileTitlePanelController.Update();
            TileControlController.Update();
            TileDetailsPanelController.Update();
            //TileMenuPanelController.Update();
        }
        public void ShowMenu() {            
            //TileMenuPanelController.TileMenuPanel.Visible = true;
            //TileMenuPanelController.Update();
                //TileMenuPanelController.TileMenuButtonControllerList[0].TileMenuButton.ButtonOver();
                //TileMenuPanelController.TileMenuButtonControllerList[0].TileMenuButton.Refresh();
                //TileMenuPanelController.TileMenuPanel.BringToFront();
        }
        public void HideMenu() {
            //TileMenuPanelController.TileMenuPanel.Visible = false;
           // TileMenuPanelController.Update();
        }
        public bool IsSelected() {
            return _isSelected;
        }
        public void SetOver(bool isOver) {
            if(isOver) {

            }
        }
        public void SetFocus(bool isFocused) {
            if(isFocused) {
                TilePanel.BorderColor = Properties.Settings.Default.TileSelectedColor;
               // TileControlController.ItemControl.Enabled = true;

                MouseOverItemControlHook.RegisterMouseEvent(MpMouseEvent.HitBox,TileControlController.ItemControl.RectangleToScreen(TileControlController.ItemControl.Bounds));

                //ShowMenu();
            } else {
                TilePanel.BorderColor = TilePanel.BackColor;//(Color)MpSingletonController.Instance.GetSetting("TileUnfocusColor");
                //TileControlController.ItemControl.Enabled = false;
                MouseOverItemControlHook.UnregisterMouseEvent();
                //HideMenu();
            }
            _isSelected = isFocused;
            //TileTitlePanelController.TileTitlePanel.BorderColor = TilePanel.BorderColor;
        }

        public void ActivateHotKeys() {}      

        public void DeactivateHotKeys() {}

        public int CompareTo(object obj) {
            if(obj == null || obj.GetType() != typeof(MpTilePanelController)) {
                return 1;
            }
            MpTilePanelController otherTilePanelController = (MpTilePanelController)obj;

            if(otherTilePanelController.CopyItem.CopyDateTime < CopyItem.CopyDateTime) {
                return -1;
            } else if(otherTilePanelController.CopyItem.CopyDateTime > CopyItem.CopyDateTime) {
                return 1;
            }
            return 0;
        }
    }   
}
