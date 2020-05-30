using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VisualEffects;
using VisualEffects.Animations.Effects;
using VisualEffects.Easing;
using wdi.ui;

namespace MonkeyPaste
{
    public class MpTilePanelController : MpController {
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
        
        public FlipPanel FlipPanel { get; set; } 

        private MpTilePanelStateType _tilePanelState = MpTilePanelStateType.None;
        public MpTilePanelStateType TilePanelState {
            get {
                return _tilePanelState;
            }
        }
        
        public MpTilePanelController(MpCopyItem ci,MpController Parent) : base(Parent) {
            CopyItem = ci;
            FlipPanel = new FlipPanel() {
                AutoSize = false,
                AutoScroll = false,
                Bounds = GetBounds()
            };
            FlipPanel.DoubleBuffered(true);

            TilePanel = new MpTilePanel() {                
                AutoScroll = false,
                AutoSize = false,
                Size = GetBounds().Size,
                BackColor = Properties.Settings.Default.LogPanelBgColor, //MpHelperSingleton.Instance.GetRandomColor()
                //RectRadius = Properties.Settings.Default.TileBorderRadius,
                //BackgroundGradientMode = BevelPanel.AdvancedPanel.PanelGradientMode.ForwardDiagonal,
                FlatBorderColor = Color.Transparent,
                Style = BeveledPanel.AdvancedPanel.BevelStyle.Flat,
                EdgeWidth = 5,
                StartColor = Color.Transparent,//Properties.Settings.Default.LogPanelBgColor, ,//ci.ItemColor.Color,
                EndColor = Color.Transparent,//Properties.Settings.Default.LogPanelBgColor,,//ci.ItemColor.Color,
                ShadowColor = Color.DimGray,
                ShadowShift = 3,
                ShadowStyle = BeveledPanel.AdvancedPanel.ShadowMode.ForwardDiagonal
            };
            TilePanel.DoubleBuffered(true);

            Panel backPanel = new Panel() {
                AutoScroll = false,
                AutoSize = false,
                BackColor = Color.Pink,
                Size = GetBounds().Size
            };
            backPanel.DoubleBuffered(true);
            FlipPanel.Front = TilePanel;
            FlipPanel.Back = backPanel;
            FlipPanel.TimerInterval = 5;
            FlipPanel.DoShading = true;

            TileTitlePanelController = new MpTileTitlePanelController(ci,this);
            TilePanel.Controls.Add(TileTitlePanelController.TileTitlePanel);

            //always call this last since it fills remaining space
            TileContentController = new MpTileContentPanelController(ci,this);
            TilePanel.Controls.Add(TileContentController.TileContentPanel);            
            
            SetState(MpTilePanelStateType.Unfocused);

            DefineEvents();
        }

        public override void DefineEvents() {
            TilePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileContentController.ScrollPanelController.VScrollbarPanelController.ScrollContinueEvent += (s, e) => {
                SetState(MpTilePanelStateType.Scrolling);
            };
            TileContentController.ScrollPanelController.HScrollbarPanelController.ScrollContinueEvent += (s, e) => {
                SetState(MpTilePanelStateType.Scrolling);
            };            

        }
        public override Rectangle GetBounds() {
            //tile chooser panel controller
            var tcpc = (MpTileChooserPanelController)Parent;
            int listIdx = tcpc.GetVisibleTilePanelControllerIdx(this);
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
            FlipPanel.Bounds = GetBounds();
            TilePanelSize = TilePanel.Size;

            TileTitlePanelController.Update();
            TileContentController.Update();

            FlipPanel.Refresh();
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
        public void Focus(bool isFocused) {
            if(isFocused) {
                TilePanel.Focus();
                //FlipPanel.BackColor = Properties.Settings.Default.TileBorderSelectedColor;
                TilePanel.FlatBorderColor = Properties.Settings.Default.TileBorderSelectedColor;
                TileTitlePanelController.TileTitlePanel.Style = BeveledPanel.AdvancedPanel.BevelStyle.Flat;
                TileTitlePanelController.TileTitlePanel.FlatBorderColor = Properties.Settings.Default.TileBorderSelectedColor;
                //TileTitlePanelController.TileTitleTextBoxController.TileTitleLabel.ForeColor = Properties.Settings.Default.TileBorderSelectedColor;
                TileContentController.ScrollPanelController.ShowScrollbars();
                SetState(MpTilePanelStateType.Focused);
            } else {
                TilePanel.FlatBorderColor = Color.Transparent;
                TileTitlePanelController.TileTitlePanel.Style = BeveledPanel.AdvancedPanel.BevelStyle.Raised;
                TileTitlePanelController.TileTitlePanel.FlatBorderColor = Color.Transparent;
                //TileTitlePanelController.TileTitleTextBoxController.TileTitleLabel.ForeColor = Color.White;
                //TileContentController.ScrollPanelController.HideScrollbars();
                SetState(MpTilePanelStateType.Unfocused);
            }
        }
        public void Hover(bool isOver) {
            if(_tilePanelState == MpTilePanelStateType.Focused) {
                return;
            }
            if(isOver) {
                TilePanel.FlatBorderColor = Properties.Settings.Default.TileBorderHoverColor;
                //FlipPanel.BackColor = Properties.Settings.Default.TileBorderHoverColor;
                TileTitlePanelController.TileTitlePanel.Style = BeveledPanel.AdvancedPanel.BevelStyle.Flat;
                TileTitlePanelController.TileTitlePanel.FlatBorderColor = Properties.Settings.Default.TileBorderHoverColor;
                //TileTitlePanelController.TileTitleTextBoxController.TileTitleLabel.ForeColor = Properties.Settings.Default.TileBorderHoverColor;
                TileContentController.ScrollPanelController.ShowScrollbars();
                SetState(MpTilePanelStateType.Hover);
            } else {
                TilePanel.FlatBorderColor = Color.Transparent;
                TileTitlePanelController.TileTitlePanel.Style = BeveledPanel.AdvancedPanel.BevelStyle.Raised;
                TileTitlePanelController.TileTitlePanel.FlatBorderColor = Color.Transparent;
                //TileTitlePanelController.TileTitleTextBoxController.TileTitleLabel.ForeColor = Color.White;
                //TileContentController.ScrollPanelController.HideScrollbars();
                SetState(MpTilePanelStateType.Unfocused);
            }
        }
        public void Hide() {
            TilePanel.Visible = false;

            SetState(MpTilePanelStateType.Hidden);
        }
        public void Show() {
            TilePanel.Visible = true;

            SetState(MpTilePanelStateType.Unfocused);
        }
        public void Drag() {
            SetState(MpTilePanelStateType.Dragging);
        }
        private void SetState(MpTilePanelStateType newState) {
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
                    MpApplication.Instance.DataModel.ClipboardManager.PasteCopyItem(CopyItem);
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
        Unfocused,
        Scrolling,
        Dragging,
        Hover,
        Focused,
    }
}
