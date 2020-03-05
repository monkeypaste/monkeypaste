using System;
using System.Collections.Generic;
using System.Drawing;

namespace MonkeyPaste
{
    public class MpTilePanelController : MpController, IComparable {
        public static int OffsetX { get; set; } = 0;

        public static Size TilePanelSize { get; set; } = Size.Empty;

        public int TileId { get; set; } = -1;
        //public int SortOrder { get; set; } = 0;

        public MpTilePanel TilePanel { get; set; }
        public MpTileBorderPanelController BorderPanelController { get; set; }

        public MpTileControlController TileControlController { get; set; }
        public MpTileTitlePanelController TileTitlePanelController { get; set; }
        //public MpTileMenuPanelController TileMenuPanelController { get; set; }
        //public MpTileDetailsPanelController TileDetailsPanelController { get; set; }
        public MpTileDetailsLabelController TileDetailsLabelController { get; set; }

        public MpTileHeaderPanelController TileHeaderPanelController { get; set; }

        // public MpMouseHook MouseOverItemControlHook { get; set; }

        public delegate void ExpandButtonClicked(object sender,EventArgs e);
        public event ExpandButtonClicked ExpandButtonClickedEvent;

        public delegate void CloseButtonClicked(object sender,EventArgs e);
        public event CloseButtonClicked CloseButtonClickedEvent;

        public MpCopyItem CopyItem { get; set; }      
        //public DateTime CopyDateTime { get; set; }

        private bool _isSelected = false;

        private MpTilePanelState _tilePanelState = MpTilePanelState.None;
        public MpTilePanelState TilePanelState {
            get {
                return _tilePanelState;
            }
        }

        public MpTilePanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            TileId = tileId;
            CopyItem = ci;
            //CopyDateTime = CopyItem.CopyDateTime;

            TilePanel = new MpTilePanel(tileId,panelId) {
                AutoScroll = false,
                AutoSize = false,
                BackColor = Properties.Settings.Default.LogPanelBgColor, //MpHelperSingleton.Instance.GetRandomColor()
                RectRadius = Properties.Settings.Default.TileBorderRadius,
                //BackgroundGradientMode = BevelPanel.AdvancedPanel.PanelGradientMode.ForwardDiagonal,
                FlatBorderColor = Color.Transparent,
                Style = BevelPanel.AdvancedPanel.BevelStyle.Flat,
                EdgeWidth = 5,                
                StartColor = ci.ItemColor.Color,
                EndColor = ci.ItemColor.Color,
                ShadowColor = Color.Black,
                ShadowShift = 3,
                ShadowStyle = BevelPanel.AdvancedPanel.ShadowMode.ForwardDiagonal,           
                TabIndex = 1
            };
            //TilePanel.BorderColor = TilePanel.BackColor;
            TilePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileHeaderPanelController = new MpTileHeaderPanelController(tileId,panelId,this);
            TilePanel.Controls.Add(TileHeaderPanelController.TileHeaderPanel);
            //TileHeaderPanelController.TileHeaderExpandButtonController.ButtonClickedEvent += TileHeaderExpandButtonController_ButtonClickedEvent;
            TileHeaderPanelController.TileHeaderCloseButtonController.ButtonClickedEvent += TileHeaderCloseButtonController_ButtonClickedEvent;

            TileTitlePanelController = new MpTileTitlePanelController(tileId,panelId,ci,this);
            TilePanel.Controls.Add(TileTitlePanelController.TileTitlePanel);

            //TileDetailsPanelController = new MpTileDetailsPanelController(ci,tileId,panelId,this);
            //TilePanel.Controls.Add(TileDetailsPanelController.TileDetailsPanel);
            TileDetailsLabelController = new MpTileDetailsLabelController(tileId,panelId,this);
            TilePanel.Controls.Add(TileDetailsLabelController.DetailsLabel);

            //always call this last since it fills remaining space
            TileControlController = new MpTileControlController(tileId,panelId,ci,this);
            TilePanel.Controls.Add(TileControlController.ItemPanel);

            SetState(MpTilePanelState.Unselected);
            //TileMenuPanelController = new MpTileMenuPanelController(tileId,panelId,this);

            //MouseOverItemControlHook = new MpMouseHook();
            //MouseOverItemControlHook.MouseEvent += _mouseOverItemControlHook_MouseEvent;
            
            Link(new List<MpIView> { TilePanel });
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
            //tile chooser panel controller
            var tcpc = ((MpTileChooserPanelController)Find("MpTileChooserPanelController"));
            //tile chooser panel rect
            Rectangle tcpr = tcpc.TileChooserPanel.Bounds;
            int listIdx = ((MpTileChooserPanelController)Parent).GetVisibleTilePanelControllerList().IndexOf(this);
            //tile padding
            int tp = (int)(Properties.Settings.Default.TileChooserPadHeightRatio * tcpr.Height);
           //tile size
            int ts = tcpr.Height - (int)(tp*2);
            TilePanel.SetBounds((listIdx * tcpr.Height + tp)+OffsetX,tp,ts,ts);

            TileHeaderPanelController.Update();
            TileTitlePanelController.Update();
            TileDetailsLabelController.Update();
            TileControlController.Update();
            //TileMenuPanelController.Update();

            if(BorderPanelController == null) {
                BorderPanelController = new MpTileBorderPanelController(tcpc,this);
                tcpc.TileChooserPanel.Controls.Add(BorderPanelController.TileBorderPanel);
            }

            BorderPanelController.Update();
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
        public void SetState(MpTilePanelState newState) {
            _tilePanelState = newState;
            if(_tilePanelState == MpTilePanelState.Hidden) {
                TilePanel.Visible = false;
            } else {
                TilePanel.Visible = true;
            }        
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

    public enum MpTilePanelState {
        None = 0,
        Hidden,
        Disabled,
        Unselected,
        Hover,
        Selected,
    }
}
