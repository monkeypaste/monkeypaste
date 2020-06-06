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
        public MpTileContentPanelController TileContentController { get; set; }
        public MpTileTitlePanelController TileTitlePanelController { get; set; }

        public MpRoundedPanel TilePanel { get; set; }

        public MpKeyboardHook _enterHook;

        public MpCopyItem CopyItem { get; set; }

        private bool _isFocused = false;
        public bool IsFocused {
            get {
                return _isFocused;
            }
        }
        
        private MpTilePanelStateType _tilePanelState = MpTilePanelStateType.None;
        public MpTilePanelStateType TilePanelState {
            get {
                return _tilePanelState;
            }
        }

        private Point _lastMouseLoc = Point.Empty;

        public MpTilePanelController(MpCopyItem ci,MpController Parent) : base(Parent) {
            CopyItem = ci;

            TilePanel = new MpRoundedPanel() {
                AutoScroll = false,
                AutoSize = false,
                Bounds = GetBounds(),
                BackColor = Properties.Settings.Default.TileBgColor,
                BorderColor = Properties.Settings.Default.TileBorderUnfocusedColor,
                BorderThickness = 15,//(int)((float)GetBounds().Width * Properties.Settings.Default.TileBorderThicknessRatio),
                Radius = Properties.Settings.Default.TileBorderRadius
            };
            TilePanel.DoubleBuffered(true);
            
            TileTitlePanelController = new MpTileTitlePanelController(ci,this);
            TilePanel.Controls.Add(TileTitlePanelController.TileTitlePanel);

            //always call this last since it fills remaining space
            TileContentController = new MpTileContentPanelController(ci,this);
            TilePanel.Controls.Add(TileContentController.TileContentPanel);

            Focus(false);

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
            TilePanel.VisibleChanged += (s, e) => {
                Console.WriteLine("Visibility chaged");
            };
        }
        public override Rectangle GetBounds() {
            //tile chooser panel controller
            var tcpc = (MpTileChooserPanelController)Parent;
            int listIdx = 0;
            if(MpLogFormController.IsFirstLoad) {
                listIdx = tcpc.TileControllerList.IndexOf(this);
            } else {
                listIdx = tcpc.GetVisibleTilePanelControllerIdx(this);
            }
            listIdx = listIdx < 0 ? 0 : listIdx;
            //tile chooser panel rect
            Rectangle tcpr = tcpc.TileChooserPanel.Bounds;

            //tile padding
            int tp = (int)(Properties.Settings.Default.TilePaddingRatio * tcpr.Height);
            //tile size
            int ts = tcpr.Height - (int)(tp * 2);
            int x = (listIdx * (tcpr.Height + tp));
            return new Rectangle(x, tp, ts, ts);
        }
        public override void Update() {
            TilePanel.Bounds = GetBounds();

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

        #region Actions
        public void Focus(bool isFocused) {
            if(isFocused) {
                _isFocused = true;
                TileContentController.ScrollPanelController.TileContentControlController.TileContentControl.Focus();
                TilePanel.BorderColor = Properties.Settings.Default.TileBorderFocusedColor;
                SetState(MpTilePanelStateType.Focused);
            } else {
                _isFocused = false;
                TilePanel.BorderColor = Color.White;
                SetState(MpTilePanelStateType.Unfocused);
            }
            TilePanel.Invalidate();
        }
        public void Hover() {
            if (_isFocused) {
                TilePanel.BorderColor = Properties.Settings.Default.TileBorderFocusedColor;
                SetState(MpTilePanelStateType.HoverFocused);
            }
            else {
                TilePanel.BorderColor = Properties.Settings.Default.TileBorderHoverColor;
                SetState(MpTilePanelStateType.HoverUnfocused);
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
        HoverUnfocused,
        HoverFocused,
        Focused,
    }
}
