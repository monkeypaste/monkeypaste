using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using PropertyChanged;
using System.Diagnostics;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;
using Avalonia.Input;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvToolTipLinkView : UserControl {
        #region Private Variables
        
        private MpPoint _lastMousePos;

        #endregion

        #region Statics

        static MpAvToolTipLinkView() {
            ToolTipTextProperty.Changed.AddClassHandler<Control>((s, e) => {
                if (s is MpAvToolTipLinkView ttv) {
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

        #region ToolTipText Direct Avalonia Property

        private string _ToolTipText = default;

        public static readonly DirectProperty<MpAvToolTipLinkView, string> ToolTipTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvToolTipLinkView, string>
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

        #region ToolTipUri Property

        private string _ToolTipUri = default;

        public static readonly DirectProperty<MpAvToolTipLinkView, string> ToolTipUriProperty =
            AvaloniaProperty.RegisterDirect<MpAvToolTipLinkView, string>
            (
                nameof(ToolTipUri),
                o => o.ToolTipUri,
                (o, v) => o.ToolTipUri = v
            );

        public string ToolTipUri {
            get => _ToolTipUri;
            set {
                SetAndRaise(ToolTipUriProperty, ref _ToolTipUri, value);

            }
        }

        #endregion

        #endregion
        public MpAvToolTipLinkView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvTooltipView_AttachedToVisualTree;
            this.DetachedFromVisualTree += MpAvTooltipView_DetachedFromVisualTree;
            this.GetObservable(Control.IsVisibleProperty).Subscribe(value => OnVisibleChanged());
            this.GetObservable(MpAvToolTipLinkView.ToolTipUriProperty).Subscribe(value => OnUriChanged());
            var tb = this.FindControl<TextBlock>("ToolTipTextBlock");
            tb.PointerPressed += MpAvClipTileDetailView_PointerPressed;

        }

        private void OnVisibleChanged() {
            _lastMousePos = null;
        }

        private void OnUriChanged() {
            var tb = this.FindControl<TextBlock>("ToolTipTextBlock");
            if(tb == null) {
                Debugger.Break();
                return;
            }
            if(string.IsNullOrEmpty(ToolTipUri)) {
                tb.Classes.Add("IsLink");
            } else {
                tb.Classes.Remove("IsLink");
            }
            
        }


        private void MpAvTooltipView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (GetHostControl() is Control host_control) {
                host_control.PointerMoved += Host_control_PointerMoved;
            }
        }

        private void Host_control_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {
            var hc = GetHostControl();
            if(hc == null) {
                _lastMousePos = null;
                return;
            }
            if (_lastMousePos == null) {
                _lastMousePos = e.GetClientMousePoint(hc);
            }
            var mp = e.GetClientMousePoint(hc);
            var diff = mp - _lastMousePos;

            var offset = GetToolTipOffset(hc);
            SetToolTipOffset(hc, offset + diff);
            _lastMousePos = mp;
        }

        private void MpAvTooltipView_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            this.AttachedToVisualTree -= MpAvTooltipView_AttachedToVisualTree;
            this.DetachedFromVisualTree -= MpAvTooltipView_DetachedFromVisualTree;
            if (GetHostControl() is Control host_control) {
                host_control.PointerMoved -= Host_control_PointerMoved;
            }
        }

        private void MpAvClipTileDetailView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (string.IsNullOrEmpty(ToolTipUri)) {
                return;
            }

            bool can_goto = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            if (can_goto) {
                if (OperatingSystem.IsWindows()) {
                    Process.Start("explorer", ToolTipUri);
                } else {
                    using (var myProcess = new Process()) {
                        myProcess.StartInfo.UseShellExecute = true;
                        myProcess.StartInfo.FileName = ToolTipUri;
                        myProcess.Start();
                    }
                }
                return;
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
            if(hc == null) {
                return MpPoint.Zero;
            }
            return new MpPoint() {
                X = ToolTip.GetHorizontalOffset(hc),
                Y = ToolTip.GetVerticalOffset(hc)
            };
        }

        private void SetToolTipOffset(Control hc, MpPoint newOffset) {
            if(hc == null) {
                return;
            }
            ToolTip.SetHorizontalOffset(hc, newOffset.X);
            ToolTip.SetVerticalOffset(hc, newOffset.Y);
        }

        #endregion
    }
}