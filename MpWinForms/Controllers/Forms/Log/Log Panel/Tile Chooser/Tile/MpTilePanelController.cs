using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VisualEffects;
using VisualEffects.Animations.Effects;
using VisualEffects.Easing;

namespace MonkeyPaste
{
    public class MpTilePanelController : MpPanelController, IComparable {
        private static int _OffsetX = 0;
        public static int OffsetX {
            get {
                return _OffsetX;
            }
            set {
                _OffsetX = value;
                Console.WriteLine("New offset: " + _OffsetX);
            }
        }

        public static Size TilePanelSize { get; set; } = Size.Empty;
        
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

        private MpTilePanelStateType _tilePanelState = MpTilePanelStateType.None;
        public MpTilePanelStateType TilePanelState {
            get {
                return _tilePanelState;
            }
        }

        public MpTilePanelController(MpCopyItem ci,MpController Parent) : base(Parent) {
            CopyItem = ci;

            TilePanel = new MpTilePanel() {
                AutoScroll = false,
                AutoSize = false,
                BackColor = Properties.Settings.Default.LogPanelBgColor, //MpHelperSingleton.Instance.GetRandomColor()
                RectRadius = Properties.Settings.Default.TileBorderRadius,
                //BackgroundGradientMode = BevelPanel.AdvancedPanel.PanelGradientMode.ForwardDiagonal,
                FlatBorderColor = Color.Transparent,
                Style = BeveledPanel.AdvancedPanel.BevelStyle.Flat,
                
                EdgeWidth = 5,
                StartColor = ci.ItemColor.Color,
                EndColor = ci.ItemColor.Color,
                ShadowColor = Color.Black,
                ShadowShift = 3,
                ShadowStyle = BeveledPanel.AdvancedPanel.ShadowMode.ForwardDiagonal,
                TabIndex = 1
            };
            TilePanel.DoubleBuffered(true);
            //TilePanel.BorderColor = TilePanel.BackColor;
            TilePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileHeaderPanelController = new MpTileHeaderPanelController(this);
            TilePanel.Controls.Add(TileHeaderPanelController.TileHeaderPanel);
            //TileHeaderPanelController.TileHeaderExpandButtonController.ButtonClickedEvent += TileHeaderExpandButtonController_ButtonClickedEvent;
            TileHeaderPanelController.TileHeaderCloseButtonController.ButtonClickedEvent += TileHeaderCloseButtonController_ButtonClickedEvent;

            TileTitlePanelController = new MpTileTitlePanelController(ci,this);
            TilePanel.Controls.Add(TileTitlePanelController.TileTitlePanel);

            TileDetailsPanelController = new MpTileDetailsPanelController(ci,this);
            TilePanel.Controls.Add(TileDetailsPanelController.TileDetailsPanel);

            //always call this last since it fills remaining space
            TileControlController = new MpTileControlController(ci,this);
            TilePanel.Controls.Add(TileControlController.ItemPanel);

            SetState(MpTilePanelStateType.Unselected);
            
            //TileMenuPanelController = new MpTileMenuPanelController(tileId,panelId,this);

            //MouseOverItemControlHook = new MpMouseHook();
            //MouseOverItemControlHook.MouseEvent += _mouseOverItemControlHook_MouseEvent;
        }
        public override Rectangle GetBounds() {
            //tile chooser panel controller
            var tcpc = (MpTileChooserPanelController)Find(typeof(MpTileChooserPanelController));
            
            int listIdx = tcpc.GetVisibleTilePanelControllerList().IndexOf(this);
            if (listIdx < 0) {
                //TilePanel.Visible = false;
                return Rectangle.Empty;
            }
            //tile chooser panel rect
            Rectangle tcpr = tcpc.TileChooserPanel.Bounds;
            
            //tile padding
            int tp = (int)(Properties.Settings.Default.TileChooserPadHeightRatio * tcpr.Height);
            //tile size
            int ts = tcpr.Height - (int)(tp * 2);
            int x = (listIdx * tcpr.Height + tp) + MpTilePanelController.OffsetX;
            return new Rectangle(x, tp, ts, ts);
        }
        public override void Update() {
            TilePanel.Bounds = GetBounds();

            TileHeaderPanelController.Update();
            TileTitlePanelController.Update();
            TileDetailsPanelController.Update();
            TileControlController.Update();
            //TileMenuPanelController.Update();

            //if(BorderPanelController == null) {
            //    BorderPanelController = new MpTileBorderPanelController(tcpc,this);
            //    tcpc.TileChooserPanel.Controls.Add(BorderPanelController.TileBorderPanel);
            //}

            //BorderPanelController.Update();
            TilePanel.Invalidate();

            TilePanelSize = TilePanel.Size;
        }
        public void AnimateTileY(int finalY, int duration, int delay, EasingDelegate easing) {
            TilePanel.Animate(
                new YLocationEffect(),
                easing,
                finalY,
                duration,
                delay
            );
        }
        private void TileHeaderCloseButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            CloseButtonClickedEvent(this,e);
        }
        private void TileHeaderExpandButtonController_ButtonClickedEvent(object sender,EventArgs e) {
            ExpandButtonClickedEvent(this,e);
        }
        private void _mouseOverItemControlHook_MouseEvent(object sender,Gma.System.MouseKeyHook.MouseEventExtArgs e) {
            //Console.WriteLine("Chuck chatted too much");
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
            return _tilePanelState == MpTilePanelStateType.Selected;
        }
        
        public void SetState(MpTilePanelStateType newState) {
            if(newState == MpTilePanelStateType.Hidden) {
                TilePanel.Visible = false;
            } else {
                TilePanel.Visible = true;
            }
            if (newState == MpTilePanelStateType.Hover) {
                //ignore hover if selected
                if (_tilePanelState == MpTilePanelStateType.Selected) {
                    return;
                }
                TilePanel.FlatBorderColor = Properties.Settings.Default.TileHoverBorderColor;
            }
            else if(newState == MpTilePanelStateType.Unselected) {
                TilePanel.FlatBorderColor = Color.Transparent;
            } else if(newState == MpTilePanelStateType.Selected) {
                TilePanel.FlatBorderColor = Properties.Settings.Default.TileBorderSelectedColor;
            }
            TilePanel.Invalidate();
            _tilePanelState = newState;
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

    public enum MpTilePanelStateType {
        None = 0,
        Hidden,
        Disabled,
        Unselected,
        Hover,
        Selected,
    }
}
