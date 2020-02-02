using System;
using System.Collections.Generic;
using System.Drawing;

namespace MonkeyPaste
{
    public class MpTilePanelController : MpController, IComparable {
        public static int OffsetX { get; set; } = 0;

        public static int SelectedTileId { get; set; }

        public static Size TilePanelSize { get; set; } = Size.Empty;

        public int TileId { get; set; } = -1;
        //public int SortOrder { get; set; } = 0;

        public MpTilePanel TilePanel { get; set; }

        public MpTileControlController TileControlController { get; set; }
        public MpTileTitlePanelController TileTitlePanelController { get; set; }
        //public MpTileMenuPanelController TileMenuPanelController { get; set; }
        public MpTileDetailsPanelController TileDetailsPanelController { get; set; }
        public MpTileHeaderPanelController TileHeaderPanelController { get; set; }

        // public MpMouseHook MouseOverItemControlHook { get; set; }

        public delegate void ExpandButtonClicked(object sender,EventArgs e);
        public event ExpandButtonClicked ExpandButtonClickedEvent;

        public delegate void CloseButtonClicked(object sender,EventArgs e);
        public event CloseButtonClicked CloseButtonClickedEvent;

        public MpCopyItem CopyItem { get; set; }      
        
        private bool _isSelected = false;
        
        public MpTilePanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            TileId = tileId;
            CopyItem = ci;            

            TilePanel = new MpTilePanel(tileId,panelId) {
                AutoScroll = false,
                AutoSize = false,
                BackColor = MpHelperSingleton.Instance.GetRandomColor()
                //Radius = Properties.Settings.Default.TileBorderRadius
                /*BackgroundGradientMode = BevelPanel.AdvancedPanel.PanelGradientMode.Vertical,
                EdgeWidth = 2,
                EndColor = System.Drawing.Color.LightGreen,
                FlatBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))),((int)(((byte)(102)))),((int)(((byte)(102))))),
                Location = new System.Drawing.Point(258,43),
                Name = "advancedPanel2",
                RectRadius = 20,
                ShadowColor = System.Drawing.Color.DimGray,
                ShadowShift = 10,
                ShadowStyle = BevelPanel.AdvancedPanel.ShadowMode.ForwardDiagonal,
                Size = new System.Drawing.Size(322,177),
                StartColor = System.Drawing.Color.ForestGreen,
                Style = BevelPanel.AdvancedPanel.BevelStyle.Raised,
                TabIndex = 1*/
            };
            //TilePanel.BorderColor = TilePanel.BackColor;
            TilePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileHeaderPanelController = new MpTileHeaderPanelController(tileId,panelId,this);
            TilePanel.Controls.Add(TileHeaderPanelController.TileHeaderPanel);
            TileHeaderPanelController.TileHeaderExpandButtonController.ButtonClickedEvent += TileHeaderExpandButtonController_ButtonClickedEvent;
            TileHeaderPanelController.TileHeaderCloseButtonController.ButtonClickedEvent += TileHeaderCloseButtonController_ButtonClickedEvent;

            TileTitlePanelController = new MpTileTitlePanelController(tileId,panelId,ci,this);
            TilePanel.Controls.Add(TileTitlePanelController.TileTitlePanel);            

            TileDetailsPanelController = new MpTileDetailsPanelController(ci,tileId,panelId,this);
            TilePanel.Controls.Add(TileDetailsPanelController.TileDetailsPanel);

            //always call this last since it fills remaining space
            TileControlController = new MpTileControlController(tileId,panelId,ci,this);
            TilePanel.Controls.Add(TileControlController.ItemPanel);

            //TileMenuPanelController = new MpTileMenuPanelController(tileId,panelId,this);

            //MouseOverItemControlHook = new MpMouseHook();
            //MouseOverItemControlHook.MouseEvent += _mouseOverItemControlHook_MouseEvent;
        
            Link(new List<MpIView> { /*TilePanel*/ });
        }
        /*public void AnimateTileY(int finalY,int duration,int delay,EasingDelegate easing) {
            TilePanel.Animate(
                new YLocationEffect(),
                easing,
                finalY,
                duration,
                delay
            );
        }*/
        private void TileHeaderCloseButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            CloseButtonClickedEvent(this,e);
        }

        private void TileHeaderExpandButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            ExpandButtonClickedEvent(this,e);
        }

        private void _mouseOverItemControlHook_MouseEvent(object sender,Gma.System.MouseKeyHook.MouseEventExtArgs e) {
            //Console.WriteLine("Chuck chatted too much");

        }
        public override void Update() {
            //Update(((MpTileChooserPanelController)Parent).TileChooserPanel.Bounds,new List<float>() { Properties.Settings.Default.TileChooserPadHeightRatio },new List<bool>() { true },new List<bool> { true });
            //if(TilePanel.Visible == false) {
            //    return;
            //}
            //tile chooser panel rect
            Rectangle tcpr = ((MpTileChooserPanelController)Parent).TileChooserPanel.Bounds;
            int listIdx = ((MpTileChooserPanelController)Parent).GetVisibleTilePanelControllerList().IndexOf(this);
            //tile padding
            int tp = (int)(Properties.Settings.Default.TileChooserPadHeightRatio * tcpr.Height);
           //tile size
            int ts = tcpr.Height - (int)(tp*2);
            TilePanel.SetBounds((listIdx * tcpr.Height + tp)+OffsetX,tp,ts,ts);


            TileHeaderPanelController.Update();
            TileTitlePanelController.Update();
            TileDetailsPanelController.Update();
            TileControlController.Update();
            //TileMenuPanelController.Update();

            TilePanel.Invalidate();

            TilePanelSize = TilePanel.Size;
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
            TilePanel.BringToFront();
            if(isFocused) {
                //TilePanel.BorderColor = Properties.Settings.Default.TileSelectedColor;
               // TileControlController.ItemControl.Enabled = true;

                //MouseOverItemControlHook.RegisterMouseEvent(MpMouseEvent.HitBox,TileControlController.ItemControl.RectangleToScreen(TileControlController.ItemControl.Bounds));

                //ShowMenu();
            } else {
                //TilePanel.BorderColor = TilePanel.BackColor;//(Color)MpSingletonController.Instance.GetSetting("TileUnfocusColor");
                //TileControlController.ItemControl.Enabled = false;
                //MouseOverItemControlHook.UnregisterMouseEvent();
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
