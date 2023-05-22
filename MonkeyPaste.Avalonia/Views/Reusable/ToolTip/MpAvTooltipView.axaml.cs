using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
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
                    ttv.Init();
                }
            });
            ToolTipHtmlProperty.Changed.AddClassHandler<Control>((s, e) => {
                if (s is MpAvToolTipView ttv) {
                    ttv.Init();
                }
            });
        }

        public string ToolTipContent =>
            string.IsNullOrEmpty(ToolTipText) ?
                ToolTipHtml :
                ToolTipText;

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
            this.AttachedToVisualTree += MpAvTooltipView_AttachedToVisualTree;
            this.DetachedFromVisualTree += MpAvTooltipView_DetachedFromVisualTree;
            this.AttachedToLogicalTree += MpAvToolTipView_AttachedToLogicalTree;
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
                //var hl = this.FindControl<Control>("ToolTipHtmlPanel");
                //hl.IsVisible = !string.IsNullOrEmpty(ToolTipHtml);
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
                pur.Hide();
                pur.IsVisible = false;
            }
        }


        private void MpAvToolTipView_AttachedToLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            if (GetHostControl() is Control host_control) {
                host_control.PointerMoved += Host_control_PointerMoved;
            }
            if (GetPopupRoot() is PopupRoot pur) {
                pur.TransparencyLevelHint = WindowTransparencyLevel.Transparent;
                pur.Background = Brushes.Transparent;
                foreach (var elm in pur.GetSelfAndLogicalDescendants()) {
                    if (elm is Control c) {
                        c.IsHitTestVisible = false;
                        c.Focusable = false;
                    }
                }
            }
        }
        private void MpAvTooltipView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {

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
            this.AttachedToLogicalTree -= MpAvToolTipView_AttachedToLogicalTree;
            if (GetHostControl() is Control host_control) {
                host_control.PointerMoved -= Host_control_PointerMoved;
            }
        }

        #region Helpers

        private PopupRoot GetPopupRoot() {
            //var tooltip = this.FindAncestorOfType<ToolTip>();
            //if (tooltip == null) {
            //    return null;
            //}
            //var tooltip_root = tooltip.GetVisualAncestor<PopupRoot>();
            //if (tooltip_root != null) {
            //    return tooltip_root;
            //}
            //return null;
            if (TopLevel.GetTopLevel(this) is PopupRoot pur) {
                return pur;
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
