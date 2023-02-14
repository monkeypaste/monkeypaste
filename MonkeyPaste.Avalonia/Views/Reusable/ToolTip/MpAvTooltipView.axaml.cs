using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvToolTipView : UserControl {
        #region Private Variables

        private MpPoint _lastMousePos;

        #endregion

        #region Statics

        static MpAvToolTipView() {
            ToolTipTextProperty.Changed.AddClassHandler<Control>((s, e) => {
                if (s is MpAvToolTipView ttv) {
                    if (e.NewValue is string text && !string.IsNullOrEmpty(text)) {
                        ttv.IsVisible = true;
                        var tb = ttv.FindControl<TextBlock>("ToolTipTextBlock");
                        tb.Text = text;
                    } else {
                        ttv.IsVisible = false;
                    }
                }
            });
        }

        #region ToolTipText Property

        private string _ToolTipText = default;

        public static readonly DirectProperty<MpAvToolTipView, string> ToolTipTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvToolTipView, string>
            (
                nameof(ToolTipText),
                o => o.ToolTipText,
                (o, v) => o.ToolTipText = v
            );

        public string ToolTipText {
            get => _ToolTipText;
            set {
                SetAndRaise(ToolTipTextProperty, ref _ToolTipText, value);
            }
        }

        #endregion 


        #endregion
        public MpAvToolTipView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvTooltipView_AttachedToVisualTree;
            this.DetachedFromVisualTree += MpAvTooltipView_DetachedFromVisualTree;
            this.GetObservable(Control.IsVisibleProperty).Subscribe(value => OnVisibleChanged());
            if (App.Desktop != null &&
                App.Desktop.MainWindow != null) {
                App.Desktop.MainWindow.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnVisibleChanged());
            }
        }

        private void OnVisibleChanged() {
            _lastMousePos = null;
            if (App.Desktop != null &&
                !App.Desktop.MainWindow.IsVisible) {
                // workaround for bug where tooltips don't hide when mw hides
                if (GetPopupRoot() is PopupRoot pur) {
                    pur.Hide();
                }
            }
        }


        private void MpAvTooltipView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (GetHostControl() is Control host_control) {
                host_control.PointerMoved += Host_control_PointerMoved;
            }
            if (GetPopupRoot() is PopupRoot pur) {
                pur.TransparencyLevelHint = WindowTransparencyLevel.Transparent;
                pur.Background = Brushes.Transparent;
            }
        }

        private void Host_control_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {
            var hc = GetHostControl();
            if (hc == null) {
                _lastMousePos = null;
                return;
            }
            if (_lastMousePos == null) {
                _lastMousePos = e.GetScreenMousePoint(hc);
            }
            var mp = e.GetScreenMousePoint(hc);

            SetToolTipOffset(hc, mp);
            _lastMousePos = mp;
        }

        private void MpAvTooltipView_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            this.AttachedToVisualTree -= MpAvTooltipView_AttachedToVisualTree;
            this.DetachedFromVisualTree -= MpAvTooltipView_DetachedFromVisualTree;
            if (GetHostControl() is Control host_control) {
                host_control.PointerMoved -= Host_control_PointerMoved;
            }
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        #region Helpers

        private PopupRoot GetPopupRoot() {
            var tooltip = this.FindAncestorOfType<ToolTip>();
            if (tooltip == null) {
                return null;
            }
            var tooltip_root = tooltip.GetVisualAncestor<PopupRoot>();
            if (tooltip_root != null) {
                return tooltip_root;
            }
            return null;
        }
        private Control GetHostControl() {
            if (GetPopupRoot() is PopupRoot pr &&
                pr.Parent is Control host_control) {
                return host_control;
            }
            return null;
        }

        private MpPoint GetToolTipOffset(Control hc) {
            if (hc == null) {
                return MpPoint.Zero;
            }
            return new MpPoint() {
                X = ToolTip.GetHorizontalOffset(hc),
                Y = ToolTip.GetVerticalOffset(hc)
            };
        }

        private void SetToolTipOffset(Control hc, MpPoint mp) {
            if (hc == null) {
                return;
            }

            var diff = mp - _lastMousePos;
            var w = GetPopupRoot();

            //var w = hc.GetVisualAncestor<Window>();
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
