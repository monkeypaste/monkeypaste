using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvToolTipView : MpAvUserControl<object> {
        #region Private Variables

        private MpPoint _lastMousePos;
        private DateTime? _lastEnterDt = null;

        const int WAIT_TO_HIDE_INPUT_TOOLTIP_MS = 1_000;

        #endregion

        #region Statics

        private static string[] _InputMatchers = new string[] {
            @"</a>"
        };
        static MpAvToolTipView() {
            //ToolTipTextProperty.Changed.AddClassHandler<Control>((s, e) => {
            //    if (s is MpAvToolTipView ttv) {
            //        ttv.Init();
            //    }
            //});
            //ToolTipHtmlProperty.Changed.AddClassHandler<Control>((s, e) => {
            //    if (s is MpAvToolTipView ttv) {
            //        ttv.Init();
            //    }
            //});
        }

        #endregion

        #region Properties

        #region InputGestureText Property

        private string _InputGestureText = string.Empty;

        public static readonly DirectProperty<MpAvToolTipView, string> InputGestureTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvToolTipView, string>
            (
                nameof(InputGestureText),
                o => o.InputGestureText,
                (o, v) => o.InputGestureText = v,
                string.Empty
            );

        public string InputGestureText {
            get => _InputGestureText;
            set {
                SetAndRaise(InputGestureTextProperty, ref _InputGestureText, value);
            }
        }

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

        public bool HasText =>
            !string.IsNullOrEmpty(ToolTipText) ||
                !string.IsNullOrEmpty(ToolTipHtml);

        #endregion

        public MpAvToolTipView() {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            IsHitTestVisible = false;
            if (TopLevel.GetTopLevel(this) is TopLevel tt_tl) {
                tt_tl.Classes.Add("transparent");
                tt_tl.IsHitTestVisible = false;
            }
            if (e.Root is not TopLevel tl) {
                return;
            }
            tl.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
            tl.Background = Brushes.Transparent;

            if (tl.Parent is not Control hc) {
                return;
            }
            hc.GetObservable(Control.IsVisibleProperty).Subscribe(value => OnHostOrHostTopLevelVisibleChanged(hc));
            if (TopLevel.GetTopLevel(hc) is TopLevel host_tl) {
                host_tl.GetObservable(Control.IsVisibleProperty).Subscribe(value => OnHostOrHostTopLevelVisibleChanged(host_tl));
            }
            // workaround to pass tooltip type from hint to tooltip
            if (hc.Classes.Contains("warning")) {
                this.Classes.Add("warning");
            } else if (hc.Classes.Contains("error")) {
                this.Classes.Add("error");
            }

            //hc.PointerMoved += HostControl_PointerMoved;
            hc.PointerEntered += HostOrThis_PointerEntered;
            hc.PointerExited += HostOrThis_PointerExited;
        }
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            _lastMousePos = null;
            _lastEnterDt = null;
        }

        protected override void OnPointerEntered(PointerEventArgs e) {
            base.OnPointerEntered(e);
            HostOrThis_PointerEntered(this, e);
        }
        protected override void OnPointerExited(PointerEventArgs e) {
            base.OnPointerExited(e);
            HostOrThis_PointerExited(this, e);
        }

        private void OnHostOrHostTopLevelVisibleChanged(Control changedControl) {
            // only used to hide tooltip if it or its toplevel are hidden/detached
            if (GetHostControl() is not Control hc ||
                hc.IsEffectivelyVisible) {
                return;
            }

            ToolTip.SetIsOpen(GetHostControl(), false);
        }

        private async void HostOrThis_PointerEntered(object sender, global::Avalonia.Input.PointerEventArgs e) {
            Control hc = null;
            if (sender == this) {
                hc = GetHostControl();
            } else {
                hc = sender as Control;
            }
            if (hc == null) {
                return;
            }
            if (!HasText) {
                ToolTip.SetIsOpen(hc, false);
                return;
            }
            if (!HasInputControl()) {
                // only freeze input tooltips
                if (sender == this) {
                    ToolTip.SetIsOpen(hc, true);
                }
                return;
            }
            _lastEnterDt = DateTime.Now;
            if (sender == this) {
                // not host control enter
                return;
            }
            int delay_ms = ToolTip.GetShowDelay(hc);
            await Task.Delay(delay_ms);
            if (_lastEnterDt == null) {
                return;
            }
            ToolTip.SetIsOpen(hc, true);
        }
        private async void HostOrThis_PointerExited(object sender, global::Avalonia.Input.PointerEventArgs e) {
            if (!HasInputControl()) {
                // only freeze input tooltips
                return;
            }
            Control hc = null;
            if (sender == this) {
                hc = GetHostControl();
            } else {
                hc = sender as Control;
            }
            if (hc == null) {
                return;
            }
            // override default tooltip behavior and keep visible
            ToolTip.SetIsOpen(hc, true);
            _lastEnterDt = null;
            await Task.Delay(WAIT_TO_HIDE_INPUT_TOOLTIP_MS);
            if (_lastEnterDt == null) {
                // no new pointer enter after delay so hide
                ToolTip.SetIsOpen(hc, false);
            }
        }


        private void HostControl_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {
            if (HasInputControl()) {
                // only do move animation if non-input tooltip
                return;
            }

            if (sender is not Control hc) {
                _lastMousePos = null;
                return;
            }

            if (_lastMousePos == null) {
                _lastMousePos = e.GetScreenMousePoint(hc);
            }
            var mp = e.GetScreenMousePoint(hc);
            SetToolTipOffset(hc, mp - _lastMousePos, mp);
            _lastMousePos = mp;
        }

        #region Helpers

        private Control GetHostControl() {
            if (TopLevel.GetTopLevel(this) is not TopLevel tl ||
                tl.Parent is not Control host_control) {
                return null;
            }
            return host_control;
        }

        private void SetToolTipOffset(Control hc, MpPoint diff, MpPoint scr_mp) {
            if (hc == null ||
                TopLevel.GetTopLevel(this) is not PopupRoot pur) {
                return;
            }

            double pd = 1;// w.PlatformImpl.DesktopScaling;
            MpRect mw_screen_rect = pur.Screens.ScreenFromBounds(pur.Bounds.ToPortableRect().ToAvPixelRect(pd)).Bounds.ToPortableRect(pd);

            var screen_centroid = mw_screen_rect.Centroid();
            var hc_centroid = hc.PointToScreen(hc.Bounds.ToPortableRect().Centroid().ToAvPoint()).ToPortablePoint(pd);

            var hc_vector = hc_centroid - screen_centroid;
            double hc_dist = hc_vector.Length;

            double scale = 10;// (this.Bounds.Width + this.Bounds.Height) / 2.0d;
            double tt_dist = hc_dist - scale;

            var new_vector = (hc_vector.Normalized * tt_dist) + diff;
            var newOffset = new_vector - hc_vector;

            var pur_scr_bounds = pur.Bounds.ToPortableRect();
            pur_scr_bounds.TranslateOrigin(null, true);
            if (pur_scr_bounds.Contains(scr_mp)) {

            } else {
                ToolTip.SetHorizontalOffset(hc, newOffset.X);
                ToolTip.SetVerticalOffset(hc, newOffset.Y);
            }

        }

        private bool HasInputControl() {
            if (ToolTipHtml == null) {
                return false;
            }
            return _InputMatchers.Any(x => ToolTipHtml.ToLower().Contains(x));
        }

        #endregion
    }
}
