using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VisualEffects;
using VisualEffects.Animations.Effects;
using VisualEffects.Easing;

namespace MonkeyPaste
{
    public class MpTilePanelController : MpControlController {
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

        public MpTileContentPanelController TileContentController { get; set; }
        public MpTileTitlePanelController TileTitlePanelController { get; set; }

        // public MpMouseHook MouseOverItemControlHook { get; set; }

        public delegate void ExpandButtonClicked(object sender,EventArgs e);
        public event ExpandButtonClicked ExpandButtonClickedEvent;

        public delegate void CloseButtonClicked(object sender,EventArgs e);
        public event CloseButtonClicked CloseButtonClickedEvent;
        
        public MpKeyboardHook _enterHook;

        public MpCopyItem CopyItem { get; set; }      

        private bool _isSelected = false;

        private Panel _fillGapPanel { get; set; }
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
                Bounds = GetBounds(),
                BackColor = Properties.Settings.Default.LogPanelBgColor, //MpHelperSingleton.Instance.GetRandomColor()
                //RectRadius = Properties.Settings.Default.TileBorderRadius,
                //BackgroundGradientMode = BevelPanel.AdvancedPanel.PanelGradientMode.ForwardDiagonal,
                FlatBorderColor = Color.Transparent,
                Style = BeveledPanel.AdvancedPanel.BevelStyle.Flat,
                EdgeWidth = 5,
                StartColor = Color.White,//Properties.Settings.Default.LogPanelBgColor, ,//ci.ItemColor.Color,
                EndColor = Color.White,//Properties.Settings.Default.LogPanelBgColor,,//ci.ItemColor.Color,
                ShadowColor = Color.Black,
                ShadowShift = 3,
                ShadowStyle = BeveledPanel.AdvancedPanel.ShadowMode.ForwardDiagonal,
                TabIndex = 1
            };
            TilePanel.DoubleBuffered(true);
            TilePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitlePanelController = new MpTileTitlePanelController(ci,this);
            TilePanel.Controls.Add(TileTitlePanelController.TileTitlePanel);

            //always call this last since it fills remaining space
            TileContentController = new MpTileContentPanelController(ci,this);
            TilePanel.Controls.Add(TileContentController.TileContentPanel);
            TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollContinueEvent += (s, e) => {
                SetState(MpTilePanelStateType.Scrolling);
            };
            TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollContinueEvent += (s, e) => {
                SetState(MpTilePanelStateType.Scrolling);
            };
            //fill gap rect
            Rectangle fgr = new Rectangle(TileContentController.GetBounds().X, (int)(TileTitlePanelController.GetBounds().Height / 2), TileContentController.GetBounds().Width, TileContentController.GetBounds().Height);
            _fillGapPanel = new Panel() {
                BackColor = Color.White,
                AutoSize = false,
                AutoScroll = false,
                Bounds = fgr,
                BorderStyle = BorderStyle.None
            };
            TilePanel.Controls.Add(_fillGapPanel);

            TileTitlePanelController.TileTitlePanel.BringToFront();
            _fillGapPanel.SendToBack();
            SetState(MpTilePanelStateType.Unselected);
        }
        
        
        public override Rectangle GetBounds() {
            //tile chooser panel controller
            var tcpc = (MpTileChooserPanelController)Parent;
            int listIdx = tcpc.GetVisibleTilePanelControllerIdx(this);
            listIdx = listIdx < 0 ? 0 : listIdx;
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
            TilePanelSize = TilePanel.Size;
            _fillGapPanel.Bounds = new Rectangle(TileContentController.GetBounds().X, (int)(TileTitlePanelController.GetBounds().Height / 2), TileContentController.GetBounds().Width, TileContentController.GetBounds().Height);
            
            TileTitlePanelController.Update();
            TileContentController.Update();

            TilePanel.Invalidate();
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
        
        public void SetState(MpTilePanelStateType newState) {
            if(newState == MpTilePanelStateType.Hidden) {
                TilePanel.Visible = false;
            } else {
                TilePanel.Visible = true;
            }
            if (newState == MpTilePanelStateType.HoverSelected) {
                TilePanel.FlatBorderColor = Properties.Settings.Default.TileBorderSelectedColor;
                TileContentController.ScrollPanelController.ShowScrollbars();
            }
            else if (newState == MpTilePanelStateType.HoverUnselected) {
                TilePanel.FlatBorderColor = Properties.Settings.Default.TileBorderHoverColor;
                TileContentController.ScrollPanelController.ShowScrollbars();
            }
            else if(newState == MpTilePanelStateType.Unselected) {
                TilePanel.FlatBorderColor = Color.Transparent;
                TileContentController.ScrollPanelController.HideScrollbars();
            } else if(newState == MpTilePanelStateType.Selected) {
                TilePanel.FlatBorderColor = Properties.Settings.Default.TileBorderSelectedColor;
                TileContentController.ScrollPanelController.HideScrollbars();
            } else if(newState == MpTilePanelStateType.Scrolling) {
                TileContentController.ScrollPanelController.ShowScrollbars();
            }
            _tilePanelState = newState;

            Update();
        }
        #region Hotkeys
        public void ActivateHotKeys() {}      
        public void DeactivateHotKeys() {}
        public void ActivateEnterKey() {
            if (_enterHook == null) {
                _enterHook = new MpKeyboardHook();
                _enterHook.RegisterHotKey(ModifierKeys.None, Keys.Enter);
                _enterHook.KeyPressed += (s,e) => {
                    MpCommandManager.Instance.ClipboardCommander.PasteCopyItem(CopyItem);
                };
            }
        }
        public void DeactivateEnterKey() {
            if (_enterHook == null) {
                return;
            }
            _enterHook.UnregisterHotKey();
            _enterHook.Dispose();
            _enterHook = null;
        }
        #endregion
    }

    public enum MpTilePanelStateType {
        None = 0,
        Hidden,
        Disabled,
        Unselected,
        Scrolling,
        Dragging,
        HoverSelected,
        HoverUnselected,
        Selected,
    }
}
