using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AvKeyGesture = Avalonia.Input.KeyGesture;

namespace MonkeyPaste.Avalonia {
    [Flags]
    public enum MpChildWindowTransition : long {
        None = 0,
        SlideInFromLeft = 1L << 0,
        SlideOutToLeft = 1L << 1,
        SlideInFromTop = 1L << 2,
        SlideOutToTop = 1L << 3,
        FadeIn = 1L << 4,
        FadeOut = 1L << 5,
    }

    public enum MpAvHeaderBackButtonType {
        None = 0,
        Arrow,
        Close
    }
    public interface MpAvIIsVisibleViewModel : MpIViewModel {
        bool IsVisible { get; }
    }
    public interface MpAvIHeaderMenuViewModel : MpIViewModel {
        IEnumerable<MpAvIMenuItemViewModel> HeaderMenuItems { get; }
        string HeaderTitle { get; }
        IBrush HeaderBackground { get; }
        ICommand BackCommand { get; }
        object BackCommandParameter { get; }
        MpAvHeaderBackButtonType BackButtonType { get; }
    }

    public interface MpAvIFocusHeaderMenuViewModel : MpAvIHeaderMenuViewModel {
    }

    public interface MpILoadableViewModel : MpIViewModel {
        bool IsLoadable { get; }
        bool IsLoaded { get; set; }
    }
    [DoNotNotify] 
    public class MpAvChildWindow : MpAvUserControl {
        #region Private Variables
        #endregion

        #region Constants
        const double HEADER_HEIGHT_RATIO = 0.125;
        public const double TRANSITION_TIME_S = 0.25d;
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Window Properties

        #region Title Property
        public MpAvWindowIcon Icon {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly StyledProperty<MpAvWindowIcon> IconProperty =
            AvaloniaProperty.Register<MpAvChildWindow, MpAvWindowIcon>(
                name: nameof(Icon),
                defaultValue: default);

        #endregion

        #region Title Property
        public string Title {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<MpAvChildWindow, string>(
                name: nameof(Title),
                defaultValue: default);

        #endregion

        #region ShowActivated Property
        public bool ShowActivated {
            get { return GetValue(ShowActivatedProperty); }
            set { SetValue(ShowActivatedProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowActivatedProperty =
            AvaloniaProperty.Register<MpAvChildWindow, bool>(
                name: nameof(ShowActivated),
                defaultValue: true);

        #endregion

        #region ShowInTaskbar Property
        public bool ShowInTaskbar {
            get { return GetValue(ShowInTaskbarProperty); }
            set { SetValue(ShowInTaskbarProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowInTaskbarProperty =
            AvaloniaProperty.Register<MpAvChildWindow, bool>(
                name: nameof(ShowInTaskbar),
                defaultValue: default);

        #endregion

        #region WindowStartupLocation Property
        public WindowStartupLocation WindowStartupLocation {
            get { return GetValue(WindowStartupLocationProperty); }
            set { SetValue(WindowStartupLocationProperty, value); }
        }

        public static readonly StyledProperty<WindowStartupLocation> WindowStartupLocationProperty =
            AvaloniaProperty.Register<MpAvChildWindow, WindowStartupLocation>(
                name: nameof(WindowStartupLocation),
                defaultValue: default);

        #endregion

        #region SizeToContent Property
        public SizeToContent SizeToContent {
            get { return GetValue(SizeToContentProperty); }
            set { SetValue(SizeToContentProperty, value); }
        }

        public static readonly StyledProperty<SizeToContent> SizeToContentProperty =
            AvaloniaProperty.Register<MpAvChildWindow, SizeToContent>(
                name: nameof(SizeToContent),
                defaultValue: default);

        #endregion

        #region Topmost Property
        public bool Topmost {
            get { return GetValue(TopmostProperty); }
            set { SetValue(TopmostProperty, value); }
        }

        public static readonly StyledProperty<bool> TopmostProperty =
            AvaloniaProperty.Register<MpAvChildWindow, bool>(
                name: nameof(Topmost),
                defaultValue: default);

        #endregion

        #region CanResize Property
        public bool CanResize {
            get { return GetValue(CanResizeProperty); }
            set { SetValue(CanResizeProperty, value); }
        }

        public static readonly StyledProperty<bool> CanResizeProperty =
            AvaloniaProperty.Register<MpAvChildWindow, bool>(
                name: nameof(CanResize),
                defaultValue: default);

        #endregion

        #region WindowState Property
        public WindowState WindowState {
            get { return GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }

        public static readonly StyledProperty<WindowState> WindowStateProperty =
            AvaloniaProperty.Register<MpAvChildWindow, WindowState>(
                name: nameof(WindowState),
                defaultValue: default);

        #endregion

        #region TransparencyLevelHint Property
        public IReadOnlyList<WindowTransparencyLevel> TransparencyLevelHint {
            get { return GetValue(TransparencyLevelHintProperty); }
            set { SetValue(TransparencyLevelHintProperty, value); }
        }

        public static readonly StyledProperty<IReadOnlyList<WindowTransparencyLevel>> TransparencyLevelHintProperty =
            AvaloniaProperty.Register<MpAvChildWindow, IReadOnlyList<WindowTransparencyLevel>>(
                name: nameof(WindowTransparencyLevel),
                defaultValue: default);

        #endregion

        #region ExtendClientAreaChromeHints Property
        public ExtendClientAreaChromeHints ExtendClientAreaChromeHints {
            get { return GetValue(ExtendClientAreaChromeHintsProperty); }
            set { SetValue(ExtendClientAreaChromeHintsProperty, value); }
        }

        public static readonly StyledProperty<ExtendClientAreaChromeHints> ExtendClientAreaChromeHintsProperty =
            AvaloniaProperty.Register<MpAvChildWindow, ExtendClientAreaChromeHints>(
                name: nameof(ExtendClientAreaChromeHints),
                defaultValue: default);

        #endregion

        #region SystemDecorations Property
        public SystemDecorations SystemDecorations {
            get { return GetValue(SystemDecorationsProperty); }
            set { SetValue(SystemDecorationsProperty, value); }
        }

        public static readonly StyledProperty<SystemDecorations> SystemDecorationsProperty =
            AvaloniaProperty.Register<MpAvChildWindow, SystemDecorations>(
                name: nameof(SystemDecorations),
                defaultValue: default);

        #endregion

        #region ExtendClientAreaToDecorationsHint Property
        public bool ExtendClientAreaToDecorationsHint {
            get { return GetValue(ExtendClientAreaToDecorationsHintProperty); }
            set { SetValue(ExtendClientAreaToDecorationsHintProperty, value); }
        }

        public static readonly StyledProperty<bool> ExtendClientAreaToDecorationsHintProperty =
            AvaloniaProperty.Register<MpAvChildWindow, bool>(
                name: nameof(ExtendClientAreaToDecorationsHint),
                defaultValue: default);

        #endregion

        public Size? FrameSize {
            get {
                return Bounds.Size;
            }
        }
        public Size ClientSize {
            get {
                return Bounds.Size;
            }
        }

        public PixelPoint Position {
            get {
                //if(MpAvMainView.Instance == null) {
                //    return default;
                //}
                if(TopLevel.GetTopLevel(this) is not { } tl ||
                    this.TranslatePoint(new(),tl) is not { } scaled_pos) {
                    return default;
                }
                double scale = 1d;// this.VisualPixelDensity();
                return scaled_pos.ToPortablePoint().ToAvPixelPoint(scale);
            }
            set {
                if (Position.X != value.X || Position.Y != value.Y) {
                    if (this.RenderTransform is not TransformGroup tg ||
                        tg.Children.OfType<TranslateTransform>().FirstOrDefault() is not { } tt) {
                        return;
                    }
                    double scale = 1d;//this.VisualPixelDensity();
                    var p = value.ToPortablePoint(scale);
                    tt.X = p.X;
                    tt.Y = p.Y;
                }
            }
        }

        private bool _isActive;
        public bool IsActive { 
            get {
                if(TopLevel.GetTopLevel(this) is Window real_win &&
                    !real_win.IsActive) {
                    // only for windowed mode so clip tray doesn't think clipboard changes are internal
                    return false;
                }
                return _isActive;
            }
            private set {
                if(_isActive != value) {
                    _isActive = value;
                }
            }
        }
        public object Owner { get; set; }

        public object DialogResult { get; set; }
        public MpIPlatformScreenInfoCollection Screens =>
            Mp.Services.ScreenInfoCollection;

        public IWindowImpl PlatformImpl =>
            null;

        #endregion

        #region HeaderViewModel

        public static readonly AttachedProperty<MpAvIHeaderMenuViewModel> HeaderViewModelProperty =
            AvaloniaProperty.RegisterAttached<MpAvChildWindow, Control, MpAvIHeaderMenuViewModel>(
                nameof(HeaderViewModel));

        public MpAvIHeaderMenuViewModel HeaderViewModel {
            get => GetValue(HeaderViewModelProperty);
            set => SetValue(HeaderViewModelProperty, value);
        }

        #endregion


        #region Appearance

        #endregion

        #region State

        public bool IsClosing { get; set; }



        #region IsVisible Property

        private bool _isVisible;
        public new bool IsVisible {
            get => IsClosing ? true : _isVisible;
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    if (IsClosing) {
                        return;
                    }
                    SetValue(IsVisibleProperty, _isVisible);
                }
            }
        }

        public static new readonly StyledProperty<bool> IsVisibleProperty =
            AvaloniaProperty.Register<MpAvChildWindow, bool>(nameof(IsVisible), true);

        #endregion

        #endregion

        #region Layout        

        #region OpenTransition

        public static readonly AttachedProperty<MpChildWindowTransition> OpenTransitionProperty =
            AvaloniaProperty.RegisterAttached<MpAvChildWindow, Control, MpChildWindowTransition>(
                nameof(OpenTransition), 
                MpChildWindowTransition.SlideInFromLeft | MpChildWindowTransition.FadeIn);
        public MpChildWindowTransition OpenTransition {
            get => GetValue(OpenTransitionProperty);
            set => SetValue(OpenTransitionProperty, value);
        }
        #endregion
        
        #region CloseTransition

        public static readonly AttachedProperty<MpChildWindowTransition> CloseTransitionProperty =
            AvaloniaProperty.RegisterAttached<MpAvChildWindow, Control, MpChildWindowTransition>(
                nameof(CloseTransition), 
                MpChildWindowTransition.SlideOutToLeft | MpChildWindowTransition.FadeOut);
        public MpChildWindowTransition CloseTransition {
            get => GetValue(CloseTransitionProperty);
            set => SetValue(CloseTransitionProperty, value);
        }
        #endregion

        #endregion

        public double WidthRatio { get; set; } = 1.0d;
        public double HeightRatio { get; set; } = 1.0d;

        public bool IsBaseLevelWindow =>
#if MOBILE_OR_WINDOWED
            this is MpAvMainWindow ||
            this is MpAvMainView;
#else
            false;
#endif

        #endregion

        #region Events
        public event EventHandler Opened;
        public event EventHandler Closed;
        public event EventHandler<CancelEventArgs> Closing;

        public event EventHandler Activated;
        public event EventHandler Deactivated;
        #endregion

        #region Constructors
        public MpAvChildWindow() {
        }
        #endregion

        #region Public Methods
        public IPlatformHandle? TryGetPlatformHandle() {
            if (TopLevel.GetTopLevel(App.MainView) is not { } tl ||
                tl.TryGetPlatformHandle() is not { } ph) {
                return null;
            }
            return ph;
        }
        public async Task<T> ShowDialog<T>(MpAvWindow owner) {
            Show(owner);
            while (DialogResult == null) {
                await Task.Delay(100);
            }
            return (T)(object)DialogResult;
        }
        public void Show() {
            SetupWindow();
            if (MpAvOverlayContainerView.Instance is not { } ocv ||
                IsBaseLevelWindow) {
                Activate();
                if (Parent is Window w) {
                    // loader or main view
                    w.Show();
                } else {
                    // a ntf during startup
                    Dispatcher.UIThread.Post(async () => {
                        while(MpAvOverlayContainerView.Instance == null) {
                            await Task.Delay(100);
                        }
                        Show();
                        return;
                    });
                }
                return;
            }

            ocv.ShowWindow(this);
        }
        public void Show(Window owner) {
            Show();
        }
        public void Show(MpAvWindow owner) {
            Show();
        }
        public void Activate() {
            // TODO Focus this child window here
#if MOBILE_OR_WINDOWED
            MpAvWindowManager.AllWindows.ForEach(x => x.IsActive = x == this);
#endif
            Activated?.Invoke(this, EventArgs.Empty); 
        }
        public void Deactivate() {
            // TODO Focus this child window here
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        public void Close() {
            var closing_args = new CancelEventArgs();
            OnClosing(closing_args);
            if(closing_args.Cancel) {
                MpConsole.WriteLine($"Close canceled");
                return;
            }

            Deactivate();
            IsClosing = true;
            
            if (MpAvOverlayContainerView.Instance is not { } ocv) {
                return;
            }
            
            void OnChildRemoved(object sender, MpAvChildWindow removed_cw) {
                if(removed_cw != this) {
                    return;
                }
                ocv.OnChildRemoved -= OnChildRemoved;
                IsClosing = false;
                IsVisible = false;
            }

            ocv.OnChildRemoved += OnChildRemoved;
            if(ocv.RemoveWindow(this)) {
                MpConsole.WriteLine($"Child Window '{Title}' closed");
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }
        public void Close(object dialogResult) {
            DialogResult = dialogResult;
            Close();
        }
        public void Hide() {
            IsVisible = false;
        }


        public void AttachDevTools() { }
        public void AttachDevTools(DevToolsOptions dto) { }
        public void AttachDevTools(AvKeyGesture kg) { }
        #endregion

        #region Protected Methods
        protected virtual void OnOpened(EventArgs e) {
            if(ShowActivated) {
                Activate();
            }
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            OnOpened(EventArgs.Empty);
            Opened?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnClosed(EventArgs e) { }
        protected override void OnUnloaded(RoutedEventArgs e) {
            base.OnUnloaded(e);
            OnClosed(EventArgs.Empty);
            Closed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnClosing(CancelEventArgs e) {
            Closing?.Invoke(this, e);
        }


        protected override void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);
            if(HeaderViewModel == null) {
                HeaderViewModel = DataContext as MpAvIHeaderMenuViewModel;
            }

        }
        #endregion

        #region Private Methods
        private void SetupWindow() {
            if(WidthRatio >= 0) {
                Width = Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Width * WidthRatio;
            }
            if(HeightRatio >= 0) {
                Height = Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Height * HeightRatio;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
