using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvToolTip : ToolTip {
        protected override Type StyleKeyOverride => typeof(ToolTip);

        public static readonly AttachedProperty<object?> TipProperty =
            AvaloniaProperty.RegisterAttached<MpAvToolTip, Control, object?>("Tip", inherits: true);
        public MpAvToolTip() : base() {
            Focusable = false;
            IsEnabled = false;
        }

    }

    [DoNotNotify]
    public partial class MpAvToolTipView : UserControl {
        #region Private Variables

        private MpPoint _lastMousePos;
        private bool _isMoveAttached = false;
        private bool _isDisableAttached = false;

        #endregion

        #region Statics

        static MpAvToolTipView() {
            ToolTipTextProperty.Changed.AddClassHandler<Control>((s, e) => {
                if (s is MpAvToolTipView ttv) {
                    ttv.Init();
                }
            });
            ToolTipHtmlProperty.Changed.AddClassHandler<Control>((s, e) => {
                if (s is MpAvToolTipView ttv) {
                    ttv.Init();
                }
            });
        }

        #endregion

        #region Properties
        public string ToolTipContent =>
            string.IsNullOrEmpty(ToolTipText) ?
                ToolTipHtml :
                ToolTipText;

        #region IsTooltipFollowEnabled AvaloniaProperty

        public bool IsTooltipFollowEnabled {
            get { return GetValue(IsTooltipFollowEnabledProperty); }
            set { SetValue(IsTooltipFollowEnabledProperty, value); }
        }

        public static readonly StyledProperty<bool> IsTooltipFollowEnabledProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(
                name: nameof(IsTooltipFollowEnabled),
                defaultValue: true);

        #endregion

        #region ToolTipText Property

        private string _ToolTipText = string.Empty;

        public static readonly DirectProperty<MpAvToolTipView, string> ToolTipTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvToolTipView, string>
            (
                nameof(ToolTipText),
                o => o.ToolTipText,
                (o, v) => o.ToolTipText = v,
                string.Empty
            );

        public string ToolTipText {
            get => _ToolTipText;
            set {
                SetAndRaise(ToolTipTextProperty, ref _ToolTipText, value);
            }
        }

        #endregion

        #region ToolTipHtml Property

        private string _ToolTipHtml = string.Empty;

        public static readonly DirectProperty<MpAvToolTipView, string> ToolTipHtmlProperty =
            AvaloniaProperty.RegisterDirect<MpAvToolTipView, string>
            (
                nameof(ToolTipHtml),
                o => o.ToolTipHtml,
                (o, v) => o.ToolTipHtml = v,
                string.Empty
            );

        public string ToolTipHtml {
            get => _ToolTipHtml;
            set {
                SetAndRaise(ToolTipHtmlProperty, ref _ToolTipHtml, value);
            }
        }

        #endregion 


        #endregion
        public MpAvToolTipView() {
            AvaloniaXamlLoader.Load(this);

            //if (!IsTooltipFollowEnabled) {
            //    IsEnabled = false;
            //    return;
            //}

            this.AttachedToVisualTree += MpAvTooltipView_AttachedToVisualTree;
            this.GetObservable(Control.IsVisibleProperty).Subscribe(value => OnVisibleChanged());
            if (MpAvWindowManager.MainWindow != null) {
                MpAvWindowManager.MainWindow.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnVisibleChanged());
            }
        }

        public void Init() {
            IsVisible = !string.IsNullOrEmpty(ToolTipContent);
            if (IsVisible) {
                var tb = this.FindControl<Control>("ToolTipTextBlock");
                tb.IsVisible = !string.IsNullOrEmpty(ToolTipText);
                var hl = this.FindControl<Control>("ToolTipHtmlPanel");
                hl.IsVisible = !string.IsNullOrEmpty(ToolTipHtml);
            }
        }

        private void OnVisibleChanged() {
            _lastMousePos = null;
            if (!IsVisible) {
                return;
            }
            Init();
            if (MpAvWindowManager.MainWindow != null &&
                !MpAvWindowManager.MainWindow.IsVisible) {
                // workaround for bug where tooltips don't hide when mw hides
                IsVisible = false;
            }
            if (!IsVisible && GetPopupRoot() is PopupRoot pur) {
                pur.IsVisible = false;
                pur.Hide();
            }
        }

        private void MpAvTooltipView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            Init();
        }

        private void Host_control_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {
            var hc = GetHostControl();

            if (hc == null) {
                _lastMousePos = null;
                return;
            }

            if (!IsVisible) {
                hc.PointerMoved -= Host_control_PointerMoved;
                return;
            }

            if (!_isMoveAttached) {
                hc.PointerMoved += Host_control_PointerMoved;
                _isMoveAttached = true;
            }
            if (!_isDisableAttached &&
                GetPopupRoot() is PopupRoot pur) {
                pur.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
                pur.Background = Brushes.Transparent;
                foreach (var elm in pur.GetSelfAndLogicalDescendants()) {
                    if (elm is Control c) {
                        //c.IsHitTestVisible = false;
                        //c.Focusable = false;
                        //c.IsEnabled = false;
                    }
                }
                _isDisableAttached = true;
            }

            if (_lastMousePos == null) {
                _lastMousePos = e.GetScreenMousePoint(hc);
            }
            var mp = e.GetScreenMousePoint(hc);

            SetToolTipOffset(hc, mp);
            _lastMousePos = mp;
        }

        #region Helpers

        private PopupRoot GetPopupRoot() {
            var tooltip = this.GetLogicalAncestors().FirstOrDefault(x => x is ToolTip);
            if (tooltip == null) {
                return null;
            }
            var tooltip_root = tooltip.GetLogicalAncestors().FirstOrDefault(x => x is PopupRoot);
            if (tooltip_root is PopupRoot pr) {
                return pr;
            }
            return null;

            //if (TopLevel.GetTopLevel(this) is PopupRoot pur) {
            //    return pur;
            //}
            //return null;
        }
        private Control GetHostControl() {
            if (GetPopupRoot() is PopupRoot pr &&
                pr.Parent is Control host_control) {
                return host_control;
            }
            return null;
        }

        private void SetToolTipOffset(Control hc, MpPoint mp) {
            if (hc == null) {
                return;
            }

            var diff = mp - _lastMousePos;
            var w = GetPopupRoot();

            if (w == null) {
                // occuring in plugin preset icon popup menu (when window)
                var test = hc.GetVisualAncestors();
                Debugger.Break();
                return;
            }

            double pd = 1;// w.PlatformImpl.DesktopScaling;
            MpRect mw_screen_rect = w.Screens.ScreenFromBounds(w.Bounds.ToPortableRect().ToAvPixelRect(pd)).Bounds.ToPortableRect(pd);

            var screen_centroid = mw_screen_rect.Centroid();
            var hc_centroid = hc.PointToScreen(hc.Bounds.ToPortableRect().Centroid().ToAvPoint()).ToPortablePoint(pd);

            var hc_vector = hc_centroid - screen_centroid;
            double hc_dist = hc_vector.Length;

            double scale = 10;// (this.Bounds.Width + this.Bounds.Height) / 2.0d;
            double tt_dist = hc_dist - scale;

            var new_vector = (hc_vector.Normalized * tt_dist) + diff;
            var newOffset = new_vector - hc_vector;
            ToolTip.SetHorizontalOffset(hc, newOffset.X);
            ToolTip.SetVerticalOffset(hc, newOffset.Y);
        }

        #endregion
    }
}
