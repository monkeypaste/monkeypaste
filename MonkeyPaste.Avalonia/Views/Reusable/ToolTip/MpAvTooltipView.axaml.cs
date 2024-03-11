using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvToolTipView : MpAvUserControl<object> {
        #region Private Variables

        #endregion

        #region Statics

        private static string[] _InputMatchers = new string[] {
            @"</a>"
        };

        #endregion

        #region Properties
        public bool IsDevToolsOpen { get; set; }

        #region Anchor Property
        public object Anchor {
            get { return GetValue(AnchorProperty); }
            set { SetValue(AnchorProperty, value); }
        }

        public static readonly StyledProperty<object> AnchorProperty =
            AvaloniaProperty.Register<MpAvToolTipView, object>(
                name: nameof(Anchor),
                defaultValue: default);

        #endregion

        #region Anchor Property
        public PlacementMode PlacementMode {
            get { return GetValue(PlacementModeProperty); }
            set { SetValue(PlacementModeProperty, value); }
        }

        public static readonly StyledProperty<PlacementMode> PlacementModeProperty =
            AvaloniaProperty.Register<MpAvToolTipView, PlacementMode>(
                name: nameof(PlacementMode),
                defaultValue: PlacementMode.Center);

        #endregion

        #region InputGestureText Property
        public string InputGestureText {
            get { return GetValue(InputGestureTextProperty); }
            set { SetValue(InputGestureTextProperty, value); }
        }

        public static readonly StyledProperty<string> InputGestureTextProperty =
            AvaloniaProperty.Register<MpAvToolTipView, string>(
                name: nameof(InputGestureText),
                defaultValue: string.Empty);

        #endregion

        #region ToolTipText Property
        public string ToolTipText {
            get { return GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        public static readonly StyledProperty<string> ToolTipTextProperty =
            AvaloniaProperty.Register<MpAvToolTipView, string>(
                name: nameof(ToolTipText),
                defaultValue: string.Empty);

        #endregion

        #region IsHtml Property
        public bool IsHtml {
            get { return GetValue(IsHtmlProperty); }
            set { SetValue(IsHtmlProperty, value); }
        }

        public static readonly StyledProperty<bool> IsHtmlProperty =
            AvaloniaProperty.Register<MpAvToolTipView, bool>(
                name: nameof(IsHtml),
                defaultValue: false);

        #endregion 


        #endregion

        public MpAvToolTipView() {
            InitializeComponent();
        }
        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            if (TopLevel.GetTopLevel(this) is not PopupRoot pur) {
                return;
            }
            pur.Classes.Add("tooltip");

            if (pur.DataContext is MpAvNotificationViewModelBase nvmb &&
                !nvmb.IsModal &&
                GetHostControl() is Control host_control) {
                // BUG avalonia won't show tooltips if they open under pointer so scooching
                host_control.Classes.Add("tt_near_right");
#if MAC
                host_control.Classes.Add("tt_near_top");
#else
                host_control.Classes.Add("tt_near_bottom");
#endif
            }
            MoveToolTip(MpAvShortcutCollectionViewModel.Instance.GlobalScaledMouseLocation);
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);

            if (!MpAvPrefViewModel.Instance.ShowTooltips) {
                IsVisible = false;
                return;
            }
#if DEBUG
            if (TopLevel.GetTopLevel(this) is TopLevel tl) {
                tl.AttachDevTools(MpAvWindow.DefaultDevToolOptions);
            }
#endif
            this.EffectiveViewportChanged += MpAvToolTipView_EffectiveViewportChanged;

            if (MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown && !IsDevToolsOpen) {
                //SetupDevTools();
            }
        }

        private void MpAvToolTipView_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            MoveToolTip(MpAvShortcutCollectionViewModel.Instance.GlobalScaledMouseLocation);

        }

        private void SetupDevTools() {
            Dispatcher.UIThread.Post(async () => {
                IsDevToolsOpen = true;

                await Task.Delay(1000);

                async Task ShowDevToolsAsync() {
                    PopupRoot root;
                    Control hc;
                    while (true) {
                        if (!IsDevToolsOpen) {
                            return;
                        }
                        if (TopLevel.GetTopLevel(this) is not PopupRoot pr ||
                            GetHostControl() is not Control host) {
                            await Task.Delay(50);
                            continue;
                        }
                        root = pr;
                        hc = host;
                        break;
                    }
                    while (MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown) {
                        await Task.Delay(100);
                    }
                    this.Focusable = true;
                    this.Focus();
                    Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence("F12");

                    while (true) {
                        if (!IsDevToolsOpen) {
                            return;
                        }
                        if (ToolTip.GetIsOpen(hc)) {
                            await Task.Delay(50);
                            if (MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown) {
                                IsDevToolsOpen = false;
                            }
                            continue;
                        }
                        ToolTip.SetIsOpen(hc, true);
                        ShowDevToolsAsync().FireAndForgetSafeAsync();
                        return;
                    }
                }
                await ShowDevToolsAsync();

            });
        }
        private Control GetHostControl() {
            if (TopLevel.GetTopLevel(this) is not TopLevel tl ||
                tl.Parent is not Control host_control) {
                return null;
            }
            return host_control;
        }

        private void MoveToolTip(MpPoint scr_mp) {
            if (TopLevel.GetTopLevel(this) is not PopupRoot pr ||
                pr.Screens.ScreenFromWindow(pr) is not Screen pr_screen ||
                GetHostControl() is not Control host) {
                return;
            }
            if (Anchor is Control anchor &&
                anchor.TranslatePoint(new(), host) is { } p) {
                var anchor_origin = p.ToPortablePoint();
                double hw = this.Bounds.Center.X;
                double hh = this.Bounds.Center.Y;
                MpConsole.WriteLine($"Anchor Origin: {anchor_origin}", true);
                MpPoint new_offset = new();
                switch (PlacementMode) {
                    case PlacementMode.TopEdgeAlignedLeft:
                        new_offset = anchor_origin;
                        break;
                    case PlacementMode.Top:
                        new_offset = anchor_origin + new MpPoint(anchor.Bounds.Center.X - this.Bounds.Center.X, 0);
                        break;
                    case PlacementMode.Center:
                        //new_offset = anchor_origin + anchor.Bounds.Center.ToPortablePoint() - this.Bounds.Center.ToPortablePoint();
                        new_offset = anchor_origin;
                        break;
                    default:
                        MpDebug.Break($"Unhandled tooltip placement '{PlacementMode}'");
                        break;
                }
                MpConsole.WriteLine($"ToolTip Offset: {new_offset}", false, true);
                ToolTip.SetHorizontalOffset(host, new_offset.X);
                ToolTip.SetVerticalOffset(host, new_offset.Y);
                return;
            }
            var scr_center = pr_screen.Bounds.Center.ToPortablePoint(pr_screen.Scaling);
            var delta = (scr_center - scr_mp).Normalized;
            while (true) {
                Point pr_mp = pr.PointToClient(scr_mp.ToAvPixelPoint(pr_screen.Scaling));
                if (pr.Bounds.Size.Width > 0 && pr.Bounds.Size.Height > 0 && !pr.Bounds.Contains(pr_mp)) {
                    // will show
                    break;
                }
                double new_offset_x = ToolTip.GetHorizontalOffset(host) + delta.X;
                double new_offset_y = ToolTip.GetVerticalOffset(host) + delta.Y;
                ToolTip.SetHorizontalOffset(host, new_offset_x);
                ToolTip.SetVerticalOffset(host, new_offset_y);
            }
        }
        //private void SetToolTipOffset(Control hc, MpPoint diff, MpPoint scr_mp) {
        //    if (hc == null ||
        //        TopLevel.GetTopLevel(this) is not PopupRoot pur) {
        //        return;
        //    }

        //    double pd = 1;// w.PlatformImpl.DesktopScaling;
        //    MpRect mw_screen_rect = pur.Screens.ScreenFromBounds(pur.Bounds.ToPortableRect().ToAvPixelRect(pd)).Bounds.ToPortableRect(pd);

        //    var screen_centroid = mw_screen_rect.Centroid();
        //    var hc_centroid = hc.PointToScreen(hc.Bounds.ToPortableRect().Centroid().ToAvPoint()).ToPortablePoint(pd);

        //    var hc_vector = hc_centroid - screen_centroid;
        //    double hc_dist = hc_vector.Length;

        //    double scale = 10;// (this.Bounds.Width + this.Bounds.Height) / 2.0d;
        //    double tt_dist = hc_dist - scale;

        //    var new_vector = (hc_vector.Normalized * tt_dist) + diff;
        //    var newOffset = new_vector - hc_vector;

        //    var pur_scr_bounds = pur.Bounds.ToPortableRect();
        //    pur_scr_bounds.TranslateOrigin(null, true);
        //    if (pur_scr_bounds.Contains(scr_mp)) {

        //    } else {
        //        ToolTip.SetHorizontalOffset(hc, newOffset.X);
        //        ToolTip.SetVerticalOffset(hc, newOffset.Y);
        //    }

        //}
        //protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        //    base.OnAttachedToVisualTree(e);
        //    IsHitTestVisible = false;
        //    if (TopLevel.GetTopLevel(this) is TopLevel tt_tl) {
        //        tt_tl.Classes.Add("transparent");
        //        tt_tl.IsHitTestVisible = false;
        //    }
        //    if (e.Root is not TopLevel tl) {
        //        return;
        //    }
        //    tl.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        //    tl.Background = Brushes.Transparent;

        //    if (tl.Parent is not Control hc) {
        //        return;
        //    }
        //    hc.GetObservable(Control.IsVisibleProperty).Subscribe(paramValue => OnHostOrHostTopLevelVisibleChanged(hc));
        //    if (TopLevel.GetTopLevel(hc) is TopLevel host_tl) {
        //        host_tl.GetObservable(Control.IsVisibleProperty).Subscribe(paramValue => OnHostOrHostTopLevelVisibleChanged(host_tl));
        //    }
        //    // workaround to pass tooltip type from hint to tooltip
        //    if (hc.Classes.Contains("warning")) {
        //        this.Classes.Add("warning");
        //    } else if (hc.Classes.Contains("error")) {
        //        this.Classes.Add("error");
        //    }

        //    //hc.PointerMoved += HostControl_PointerMoved;
        //    hc.PointerEntered += HostOrThis_PointerEntered;
        //    hc.PointerExited += HostOrThis_PointerExited;
        //}
        //protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        //    base.OnDetachedFromVisualTree(e);
        //    _lastMousePos = null;
        //    _lastEnterDt = null;
        //}

        //protected override void OnPointerEntered(PointerEventArgs e) {
        //    base.OnPointerEntered(e);
        //    HostOrThis_PointerEntered(this, e);
        //}
        //protected override void OnPointerExited(PointerEventArgs e) {
        //    base.OnPointerExited(e);
        //    HostOrThis_PointerExited(this, e);
        //}

        //private void OnHostOrHostTopLevelVisibleChanged(Control changedControl) {
        //    // only used to hide tooltip if it or its toplevel are hidden/detached
        //    if (GetHostControl() is not Control hc ||
        //        hc.IsEffectivelyVisible) {
        //        return;
        //    }

        //    ToolTip.SetIsOpen(GetHostControl(), false);
        //}

        //private async void HostOrThis_PointerEntered(object sender, global::Avalonia.Input.PointerEventArgs e) {
        //    Control hc = null;
        //    if (sender == this) {
        //        hc = GetHostControl();
        //    } else {
        //        hc = sender as Control;
        //    }
        //    if (hc == null) {
        //        return;
        //    }
        //    if (!HasText) {
        //        ToolTip.SetIsOpen(hc, false);
        //        return;
        //    }
        //    if (!HasInputControl()) {
        //        // only freeze input tooltips
        //        if (sender == this) {
        //            ToolTip.SetIsOpen(hc, true);
        //        }
        //        return;
        //    }
        //    _lastEnterDt = DateTime.Now;
        //    if (sender == this) {
        //        // not host control enter
        //        return;
        //    }
        //    int delay_ms = ToolTip.GetShowDelay(hc);
        //    await Task.Delay(delay_ms);
        //    if (_lastEnterDt == null) {
        //        return;
        //    }
        //    ToolTip.SetIsOpen(hc, true);
        //}
        //private async void HostOrThis_PointerExited(object sender, global::Avalonia.Input.PointerEventArgs e) {
        //    if (!HasInputControl()) {
        //        // only freeze input tooltips
        //        return;
        //    }
        //    Control hc = null;
        //    if (sender == this) {
        //        hc = GetHostControl();
        //    } else {
        //        hc = sender as Control;
        //    }
        //    if (hc == null) {
        //        return;
        //    }
        //    // override default tooltip behavior and keep visible
        //    ToolTip.SetIsOpen(hc, true);
        //    _lastEnterDt = null;
        //    await Task.Delay(WAIT_TO_HIDE_INPUT_TOOLTIP_MS);
        //    if (_lastEnterDt == null) {
        //        // no new pointer enter after delay so hide
        //        ToolTip.SetIsOpen(hc, false);
        //    }
        //}


        //private void HostControl_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {
        //    if (HasInputControl()) {
        //        // only do move animation if non-input tooltip
        //        return;
        //    }

        //    if (sender is not Control hc) {
        //        _lastMousePos = null;
        //        return;
        //    }

        //    if (_lastMousePos == null) {
        //        _lastMousePos = e.GetScreenMousePoint();
        //    }
        //    var mp = e.GetScreenMousePoint();
        //    SetToolTipOffset(hc, mp - _lastMousePos, mp);
        //    _lastMousePos = mp;
        //}

        //#region Helpers





        //private bool HasInputControl() {
        //    if (ToolTipHtml == null) {
        //        return false;
        //    }
        //    return _InputMatchers.Any(x => ToolTipHtml.ToLower().Contains(x));
        //}

        //#endregion
    }
}
