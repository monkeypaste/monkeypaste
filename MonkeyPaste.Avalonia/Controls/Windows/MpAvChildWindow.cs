using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AvKeyGesture = Avalonia.Input.KeyGesture;

namespace MonkeyPaste.Avalonia {
        
    [DoNotNotify]
    public class MpAvChildWindow : MpAvUserControl {
        #region Private Variables
        #endregion

        #region Constants
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
                defaultValue: default);

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
                double scale = this.VisualPixelDensity();
                return new MpPoint(CanvasX, CanvasY).ToAvPixelPoint(scale);
            }
            set {
                if (Position.X != value.X || Position.Y != value.Y) {
                    double scale = this.VisualPixelDensity();
                    var p = value.ToPortablePoint(scale);
                    CanvasX = p.X;
                    CanvasY = p.Y;
                }
            }
        }
        public bool IsActive =>
            this.GetSelfAndVisualDescendants().OfType<Control>().Any(x => x.IsFocused);
        public object Owner { get; set; }

        public object DialogResult { get; set; }
        public MpIPlatformScreenInfoCollection Screens =>
            Mp.Services.ScreenInfoCollection;

        public IWindowImpl PlatformImpl =>
            null;

        #endregion

        #region CanvasX Property
        public double CanvasX {
            get { return GetValue(CanvasXProperty); }
            set { SetValue(CanvasXProperty, value); }
        }

        public static readonly StyledProperty<double> CanvasXProperty =
            AvaloniaProperty.Register<MpAvChildWindow, double>(
                name: nameof(CanvasX),
                defaultValue: default);

        #endregion

        #region CanvasY Property
        public double CanvasY {
            get { return GetValue(CanvasYProperty); }
            set { SetValue(CanvasYProperty, value); }
        }

        public static readonly StyledProperty<double> CanvasYProperty =
            AvaloniaProperty.Register<MpAvChildWindow, double>(
                name: nameof(CanvasY),
                defaultValue: default);

        #endregion

        #region Back Command

        public static readonly AttachedProperty<ICommand> BackCommandProperty =
            AvaloniaProperty.RegisterAttached<MpAvChildWindow, Control, ICommand>(
                nameof(BackCommand));

        public ICommand BackCommand {
            get => GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        //static ICommand DefaultBackCommand => new MpCommand<object>(
        //    (args) => {
        //        MpConsole.WriteLine("Back clicked");
        //        this.Close();
        //        if (ParentWindow == null) {
        //            return;
        //        }
        //        ParentWindow.Activate();
        //    });
        #endregion

        #region Parent Window

        public static readonly AttachedProperty<MpAvChildWindow> ParentWindowProperty =
            AvaloniaProperty.RegisterAttached<MpAvChildWindow, Control, MpAvChildWindow>(
                nameof(ParentWindow), default, false);
        public MpAvChildWindow ParentWindow {
            get => GetValue(ParentWindowProperty);
            set => SetValue(ParentWindowProperty, value);
        }
        #endregion
        
        #region MenuItems

        public static readonly AttachedProperty<IEnumerable<MpAvMenuItemViewModel>> MenuItemsProperty =
            AvaloniaProperty.RegisterAttached<MpAvChildWindow, Control, IEnumerable<MpAvMenuItemViewModel>>(
                nameof(MenuItems), default, false);
        public IEnumerable<MpAvMenuItemViewModel> MenuItems {
            get => GetValue(MenuItemsProperty);
            set => SetValue(MenuItemsProperty, value);
        }
        #endregion

        #endregion

        #region Events
        public event EventHandler Opened;
        public event EventHandler Closed;
        public event EventHandler<WindowClosingEventArgs> Closing;

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
            // TODO attach & position this window to MpAvMainView here
            if(ParentWindow == default) {
#if WINDOWED
                ParentWindow = MpAvWindowManager.ActiveWindow;
#else
                ParentWindow = MpAvWindowManager.ActiveWindow.GetVisualDescendant<MpAvChildWindow>(); 
#endif
            }
            if (MpAvOverlayContainerView.Instance is not { } ocv) {
                if(Parent is Window w) {
                    w.Show();
                }
                return;
            }
            ocv.AddChild(this);
        }
        public void Show(Window owner) {
            Show();
        }
        public void Show(MpAvWindow owner) {
            Show();
        }
        public void Activate() {
            // TODO Focus this child window here
            Activated?.Invoke(this, EventArgs.Empty);
        }
        public void Deactivate() {
            // TODO Focus this child window here
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        public void Close() {
            // TODO detach from MpAvMainView
            MpConsole.WriteLine($"Child Window '{Title}' closed");

            if (MpAvOverlayContainerView.Instance is not { } ocv) {
                return;
            }
            
            if(ocv.RemoveChild(this)) {
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
        protected virtual void OnOpened(EventArgs e) { }

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

        protected virtual void OnClosing(WindowClosingEventArgs e) {
            Closing?.Invoke(this, e);
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }


}
