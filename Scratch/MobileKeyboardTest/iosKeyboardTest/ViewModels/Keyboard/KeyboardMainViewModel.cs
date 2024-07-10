using Avalonia;
using ReactiveUI;

namespace iosKeyboardTest;

public class KeyboardMainViewModel : ViewModelBase
{
    #region Private Variables
    #endregion

    #region Constants
    public const double TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO = 0.3;

    #endregion

    #region Statics

    public static KeyboardView CreateKeyboardView(IKeyboardInputConnection inputConn, Size scaledScreenSize, double scale, out Size unscaledSize) {
        var kbmvm = new KeyboardMainViewModel(inputConn, scaledScreenSize);
        var kbvm = kbmvm.KeyboardViewModel;
        var kbv = new KeyboardView() {
            DataContext = kbmvm,
            Width = kbmvm.TotalWidth,
            Height = kbmvm.TotalHeight 
        };
        unscaledSize = new Size(kbmvm.TotalWidth * scale, kbmvm.TotalHeight * scale);
        return kbv;
    }
    #endregion

    #region Interfaces
    #endregion

    #region Properties

    #region View Models
    public KeyboardViewModel KeyboardViewModel { get; private set; }
    #endregion

    #region Appearance
    #endregion

    #region Layout

    private Size _screenSize;
    public Size ScreenSize {
        get {
            if (_screenSize == null) {
                return new Size(500, 500);
            }
            return _screenSize;
        }
    }
    double KeyHeightPad => 5;
    public double TotalWidth =>
        KeyboardViewModel.KeyboardWidth;
    public double TotalHeight =>
        KeyboardViewModel.KeyboardHeight + 
        (KeyboardViewModel.NeedsNextKeyboardButton ? MenuHeight : 0) +
        MenuHeight;
    public double MenuHeight =>
        KeyboardViewModel.KeyHeight + KeyHeightPad;
    #endregion

    #region State
    #endregion

    #region Models
    #endregion
    #endregion

    #region Constructors
    public KeyboardMainViewModel() : this(null,new Size(360,740)) { }
    public KeyboardMainViewModel(IKeyboardInputConnection inputConn, Size scaledScreenSize) {
        KeyboardViewModel = new KeyboardViewModel(this, inputConn);
        SetScreenSize(scaledScreenSize);
        _screenSize = scaledScreenSize;
    }
    #endregion

    #region Public Methods
    public void SetScreenSize(Size scaledScreenSize) {
        _screenSize = scaledScreenSize;
        RefreshLayout();
    }
    public void ForceSize(Size size) {

        KeyboardViewModel.KeyboardWidth = size.Width;
        KeyboardViewModel.KeyboardHeight = size.Height;
        RefreshLayout();
    }
    #endregion

    #region Protected Methods
    #endregion

    #region Private Methods
    void RefreshLayout() {
        this.RaisePropertyChanged(nameof(TotalWidth));
        this.RaisePropertyChanged(nameof(TotalHeight));
        this.RaisePropertyChanged(nameof(MenuHeight));

        KeyboardViewModel.RefreshLayout();


    }
    #endregion

    #region Commands
    #endregion
}
