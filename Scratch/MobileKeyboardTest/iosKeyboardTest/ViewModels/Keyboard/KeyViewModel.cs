using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Input;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;

namespace iosKeyboardTest {
    public class KeyViewModel : ViewModelBase, IKeyboardViewRenderer, IKeyboardRenderSource {
        #region Private Variables
        #endregion

        #region Constants

        public const string BACKSPACE_IMG_FILE_NAME = "backspace.png";//"⌫";
        public const string EMOJI_SELECT_BTN_IMG_FILE_NAME = "emoji.png";//"☺";
        //public const string ENTER_IMG_FILE_NAME = "enter.png";//"⏎";
        public const string NEXT_KEYBOARD_IMG_FILE_NAME = "globe.png";
        public const string SEARCH_IMG_FILE_NAME = "search.png";//"🔍";
        public const string SHIFT_IMG_FILE_NAME = "shift.png";//"⇧";
        public const string SHIFT_LOCK_IMG_FILE_NAME = "shift_lock.png";
        public const string SHIFT_ON_IMG_FILE_NAME = "shift_on.png";


        #endregion

        #region Statics 
        public static string[] IMG_FILE_NAMES => [
            SHIFT_IMG_FILE_NAME,
            SHIFT_ON_IMG_FILE_NAME,
            SHIFT_LOCK_IMG_FILE_NAME,
            SEARCH_IMG_FILE_NAME,
            //ENTER_IMG_FILE_NAME,
            BACKSPACE_IMG_FILE_NAME,
            EMOJI_SELECT_BTN_IMG_FILE_NAME,
            EMOJI_SELECT_BTN_IMG_FILE_NAME,
        ];

        static string SHIFT_TEXT_2 = "1/2";
        static string SHIFT_TEXT_3 = "2/2";
        static string TAB_TEXT = "Tab";
        static string CAPS_LOCK_TEXT = "Caps Lock";
        static string GO_TEXT = "Go";
        static string ENTER_TEXT = "Enter";
        static string NEXT_TEXT = "Next";
        static string DONE_TEXT = "Done";
        static string SYMBOLS1_TEXT = "!#1";
        static string SYMBOLS2_TEXT = "ABC";        
        static string NUM_SYMBOLS1_TEXT = "*+#";
        static string NUM_SYMBOLS2_TEXT = "123";

        static string[] _SPECIAL_KEY_TEXTS;
        static string[] SPECIAL_KEY_TEXTS {
            get {
                if(_SPECIAL_KEY_TEXTS == null) {
                    _SPECIAL_KEY_TEXTS = [
                        SHIFT_TEXT_2,
                        SHIFT_TEXT_3,
                        TAB_TEXT,
                        CAPS_LOCK_TEXT,
                        GO_TEXT,
                        NEXT_TEXT,
                        DONE_TEXT,
                        SYMBOLS1_TEXT,
                        SYMBOLS2_TEXT,
                        NUM_SYMBOLS1_TEXT,
                        NUM_SYMBOLS2_TEXT,
                    ];
                }
                return _SPECIAL_KEY_TEXTS;
            }
        }

        static List<List<string>> _letterGroups;
        static List<List<string>> LetterGroups {
            get {
                // from https://ios.gadgethacks.com/how-to/every-hidden-special-character-your-iphones-keyboard-you-can-unlock-right-now-0384666/
                if (_letterGroups == null) {
                    _letterGroups = new List<List<string>>() {
                        // letters
                        (["a","à","á","â","ä","æ","ã","å","ā","ǎ","ă","ą"]),
                        (["c","ç","ć","č","ċ"]),
                        (["d","ď","ð"]),
                        (["e","è","é","ê","ë","ē","ė","ę","ě","ẽ"]),
                        (["g","ğ","ġ"]),
                        (["h","ħ"]),
                        (["i","ì","į","ī","í","ï","î","ı","ĩ","ǐ"]),
                        (["k","ķ"]),
                        (["l","ł","ľ","ļ"]),
                        (["n","ń","ñ","ň","ņ"]),
                        (["o","õ","ō","ø","œ","ó","ò","ö","ô","ő","ǒ"]),
                        (["r","ř"]),
                        (["s","ß","ś","š","ş","ș"]),
                        (["t","ț","ť","þ"]),
                        (["u","ū","ú","ù","ü","û","ų","ů","ű","ũ","ǔ"]),
                        (["w","ŵ"]),
                        (["y","ÿ","ŷ","ý"]),
                        (["z","ž","ź","ż"]),
                        (["b"]),
                        (["f"]),
                        (["j"]),
                        (["m"]),
                        (["p"]),
                        (["q"]),
                        (["v"]),
                        (["x"]),

                        // symbols/numbers
                        (["0","º"]),
                        (["-","–","—","•"]),
                        (["/","\\"]),
                        (["$","₽","¥","€","¢","£","₩"]),
                        (["&","§"]),
                        (["\"","«","»","„","“","”"]),
                        ([".","…"]),
                        (["?","¿"]),
                        (["!","¡"]),
                        (["'","`","‘","’"]),
                        (["%","‰"]),
                        (["=","≈","≠"]),
                        ([" "]),
                        ([","]),
                        (["1"]),
                        (["2"]),
                        (["3"]),
                        (["4"]),
                        (["5"]),
                        (["6"]),
                        (["7"]),
                        (["8"]),
                        (["9"]),
                        ([":"]),
                        ([";"]),
                        (["("]),
                        ([")"]),
                        (["@"]),
                        ([""]),
                        (["["]),
                        (["]"]),
                        (["{"]),
                        (["}"]),
                        (["#"]),
                        (["^"]),
                        (["*"]),
                        (["+"]),
                        (["_"]),
                        (["|"]),
                        (["~"]),
                        (["<"]),
                        ([">"]),
                        (["€"]),
                        (["£"]),
                        (["¥"]),
                        (["•"])
                    };
                }
                return _letterGroups;
            }
        }
        #endregion

        #region Interfaces

        #region IKeyboardViewRenderer Implementation
        void IKeyboardViewRenderer.Measure(bool invalidate) {
            if (this is KeyViewModel kvm) {
                ClearRects();

                kvm.RaisePropertyChanged(nameof(NeedsOuterTranslate));
                kvm.RaisePropertyChanged(nameof(IsSpecial));
                kvm.RaisePropertyChanged(nameof(PrimaryFontSize));
                kvm.RaisePropertyChanged(nameof(SecondaryFontSize));
                kvm.RaisePropertyChanged(nameof(OuterTranslateX));
                kvm.RaisePropertyChanged(nameof(X));
                kvm.RaisePropertyChanged(nameof(Y));
                kvm.RaisePropertyChanged(nameof(PrimaryTranslateOffsetX));
                kvm.RaisePropertyChanged(nameof(PrimaryTranslateOffsetY));
                kvm.RaisePropertyChanged(nameof(SecondaryTranslateOffsetX));
                kvm.RaisePropertyChanged(nameof(SecondaryTranslateOffsetY));
                kvm.RaisePropertyChanged(nameof(PullTranslateY));
            }

        }

        void IKeyboardViewRenderer.Paint(bool invalidate) {
            if (this is KeyViewModel kvm) {
                kvm.RaisePropertyChanged(nameof(IsSecondaryVisible));
                kvm.RaisePropertyChanged(nameof(IsPressed));
                kvm.RaisePropertyChanged(nameof(KeyOpacity));
                kvm.RaisePropertyChanged(nameof(PrimaryValue));
                kvm.RaisePropertyChanged(nameof(SecondaryValue));
                kvm.RaisePropertyChanged(nameof(PrimaryOpacity));
                kvm.RaisePropertyChanged(nameof(SecondaryOpacity));
                kvm.RaisePropertyChanged(nameof(IsShiftKeyAndOnTemp));
                kvm.RaisePropertyChanged(nameof(IsShiftKeyAndOnLock));
            }
        }

        void IKeyboardViewRenderer.Layout(bool invalidate) {
            if (this is KeyViewModel kvm) {
                kvm.RaisePropertyChanged(nameof(ZIndex));
                kvm.RaisePropertyChanged(nameof(IsActiveKey));

                kvm.RaisePropertyChanged(nameof(Width));
                kvm.RaisePropertyChanged(nameof(Height));
                kvm.RaisePropertyChanged(nameof(InnerWidth));
                kvm.RaisePropertyChanged(nameof(InnerHeight));
                kvm.RaisePropertyChanged(nameof(CornerRadius));
            }
        }
        void IKeyboardViewRenderer.Render(bool invalidate) {
            Renderer.Measure(false);
            Renderer.Layout(false);
            Renderer.Paint(invalidate);
        }
        #endregion

        #endregion

        #region Properties

        #region Members
        IKeyboardViewRenderer _renderer;
        public IKeyboardViewRenderer Renderer =>
            _renderer ?? this;
        #endregion

        #region View Models
        public KeyboardViewModel Parent { get; set; }
        public KeyViewModel PrevKeyViewModel { get; set; }
        public KeyViewModel PopupAnchorKey { get; private set; }

        public IEnumerable<KeyViewModel> PopupKeys =>
            Parent.VisiblePopupKeys
            .Where(x => x.PopupAnchorKey == this);
        public KeyViewModel DefaultPopupKey =>
            PopupKeys
            .FirstOrDefault(x => x.IsDefaultPopupKey);
        private KeyViewModel _activePopupKey;
        public KeyViewModel ActivePopupKey {
            get => _activePopupKey ?? DefaultPopupKey;
            private set {
                if (_activePopupKey != value) {
                    _activePopupKey = value;
                }
            }
        }

        #endregion

        #region Appearance
        public string BgHex { get; private set; }
        public string PrimaryHex { get; private set; }
        public string SecondaryHex { get; private set; }
        public double PrimaryFontSize =>
            Math.Max(1, Math.Min(InnerWidth, InnerHeight) * PrimaryFontSizeRatio);
        public double SecondaryFontSize {
            get {
                double fs = Math.Max(1, Math.Min(InnerWidth, InnerHeight) * SecondaryFontSizeRatio);
                if (!IsPulling) {
                    return fs;
                }
                double pull_percent = PullTranslateY / MaxPullTranslateY;
                fs = PrimaryFontSize * pull_percent;
                return fs;
            }
        }
        public double SecondaryOpacity {
            get {
                double op = IsSecondaryVisible ? 1 : 0;
                if (!IsPulling) {
                    return op;
                }
                double pull_percent = PullTranslateY / MaxPullTranslateY;
                op = pull_percent;
                return op;
            }
        }
        public double PrimaryOpacity {
            get {
                double op = 1;
                if (!IsPulling) {
                    return op;
                }
                double pull_percent = PullTranslateY / MaxPullTranslateY;
                op = 1 - pull_percent;
                return op;
            }
        }

        public double PrimaryTranslateOffsetX { get; set; } = 0;
        public double PrimaryTranslateOffsetY { get; set; } = 0;
        public bool IsSecondaryVisible {
            get {
                if(string.IsNullOrEmpty(SecondaryValue)) {
                    return false;
                }
                if (Parent.IsNumPadLayout) {
                    if (SpecialKeyType == SpecialKeyType.NumberSymbolsToggle) {
                        return false;
                    }
                    return true;
                }
                if (Parent.IsTextLayout && IsInput) {
                    if (!Parent.IsNumberRowVisible) {
                        if(Row > 0 || !Parent.IsLettersCharSet) {
                            return false;
                        }
                    } else {
                        if (!Parent.IsLettersCharSet) {
                            return false;
                        }
                    }
                    
                    return true;
                }
                return false;
            }
        }
        public double KeyOpacity =>
            IsVisible ? 1 : 0;
        public bool IsVisible {
            get {
                if (IsPopupKey) {
                    if (IsFakePopupKey) {
                        return true;
                    }
                }
                return !string.IsNullOrEmpty(CurrentChar);
            }
        }

        bool IsBgAlwaysVisible {
            get {
                // TODO should probably make some keys always have bg, like space etc like gboard
                return IsSpaceBar || IsPrimarySpecial || IsShiftKeyAndOnTemp;
            }
        }
        #endregion

        #region Layout

        #region Factors
        public double PopupScale => 2;
        public double PopupKeyWidthRatio =>
            1.07;
        public double DefaultOuterPadX =>
            Math.Min(10, Width / Parent.MaxColCount);
        public double OuterPadX =>
            IsPopupKey ? 0 : DefaultOuterPadX;
        public double OuterPadY =>
            IsPopupKey ?
                0 :
                Parent.DefaultKeyHeight * 0.15;
        public double PrimaryFontSizeRatio {
            get {
                if (IsDotCom) {
                    return 0.3;
                }
                if (IsShiftKey && Parent.IsLettersCharSet) {
                    return 1;
                }
                if (IsBackspace) {
                    return 0.5;
                }
                if (IsPrimarySpecialKey) {
                    return 0.33;
                }
                if (IsSpecialDisplayText) {
                    return 0.5;
                }
                if (IsShiftKey && !Parent.IsLettersCharSet) {
                    return 0.5;
                }
                if(Parent.IsNumPadLayout && IsInput) {
                    return 0.65;
                }
                return 0.65;
            }
        }           

        public CornerRadius CornerRadius {
            get {
                double cr = 7;
                if (IsPopupKey && PopupAnchorKey is { } anchor_kvm) {
                    int max_row = anchor_kvm.VisiblePopupRowCount - 1;
                    int max_col = anchor_kvm.VisiblePopupColCount - 1;
                    double tl = 0;
                    double tr = 0;
                    double bl = 0;
                    double br = 0;
                    if (Row == 0 && Column == 0) {
                        tl = cr;
                    }
                    if (Row == 0 && Column == max_col) {
                        tr = cr;
                    }
                    if (Row == max_row && Column == max_col) {
                        br = cr;
                    }
                    if (Row == max_row && Column == 0) {
                        bl = cr;
                    }
                    return new CornerRadius(tl, tr, br, bl);

                }
                return new CornerRadius(cr);
            }
        }

        #endregion

        public int ZIndex =>
            IsPopupKey ? 1 : 0;
        public double InnerX =>
            X + (OuterPadX / 2);
        public double InnerY =>
            Y + (OuterPadY / 2);
        public double X {
            get {
                if (_keyboardRect is not { } rect) {
                    rect = FindRect();
                    _keyboardRect = rect;
                }
                return rect.X;
            }
        }
        public double Y {
            get {
                if (_keyboardRect is not { } rect) {
                    rect = FindRect();
                    _keyboardRect = rect;
                }
                return rect.Y;
            }
        }
        public void TranslatePopupLocation(double ox, double oy) {
            PopupOffsetX = ox;
            PopupOffsetY = oy;
            ClearRects();
            Renderer.Render(true);
        }
        public Rect PopupRect { get; private set; }

        private Rect? _secondaryTextRect;
        public Rect SecondaryTextRect {
            get {
                if(_secondaryTextRect is not { } ptr) {
                    if (_keyboardRect is not { } kbRect) {
                        kbRect = FindRect();
                        _keyboardRect = kbRect;
                    }
                    var str = InnerRect.Deflate(new Thickness(InnerWidth * 0.3, InnerHeight * 0.3));
                    double pad = 3;
                    _secondaryTextRect = new Rect(InnerRect.Right - str.Width - pad, InnerRect.Top + pad, str.Width, str.Height);
                }               
                
                return _secondaryTextRect.Value;
            }
        }
        private Rect? _primaryTextRect;
        public Rect PrimaryTextRect {
            get {
                if(_primaryTextRect is not { } ptr) {
                    if (_keyboardRect is not { } kbRect) {
                        kbRect = FindRect();
                        _keyboardRect = kbRect;
                    }                    
                    _primaryTextRect = InnerRect.Deflate(new Thickness(InnerWidth * 0.5, InnerHeight * 0.5));
                }               
                
                return _primaryTextRect.Value;
            }
        }
        
        private Rect? _primaryImageRect;
        public Rect PrimaryImageRect {
            get {
                if(_primaryImageRect is not { } ptr) {
                    if (_keyboardRect is not { } kbRect) {
                        kbRect = FindRect();
                        _keyboardRect = kbRect;
                    }
                    
                    double w = Math.Min(InnerWidth,InnerHeight) - 5;
                    double h = w;
                    double x = (InnerRect.Width - w) / 2;
                    double y = (InnerRect.Height - h) / 2;
                    _primaryImageRect = new Rect(x, y, w, h);
                }               
                
                return _primaryImageRect.Value;
            }
        }
        
        private Rect? _innerRect;
        public Rect InnerRect {
            get {
                if(_innerRect is not { } ir) {
                    if (_keyboardRect is not { } kbRect) {
                        kbRect = FindRect();
                        _keyboardRect = kbRect;
                    }
                    ir = new Rect(InnerX,InnerY, InnerWidth, InnerHeight);
                    _innerRect = ir;
                }
                
                return _innerRect.Value;
            }
        }

        private Rect? _keyboardRect;
        public Rect KeyboardRect {
            get {
                if (_keyboardRect is not { } kbRect) {
                    kbRect = FindRect();
                    _keyboardRect = kbRect;
                }
                return _keyboardRect.Value;
            }
        }

        private Rect? _totalRect;
        public Rect TotalRect {
            get {
                if (_totalRect is not { } tRect) {
                    if (_keyboardRect is not { } kbRect) {
                        kbRect = FindRect();
                        _keyboardRect = kbRect;
                    }

                    tRect = new Rect(kbRect.X, kbRect.Y + Parent.KeyboardRect.Top, kbRect.Width, kbRect.Height);
                    _totalRect = tRect;
                }
                return tRect;
            }
        }

        public Point SecondaryOffsetRatio {
            get {
                double x = 0.7;
                double y = 0.65;
                if(IsAlphaNumericNumber) {
                    x = 0.7;
                    y = 0.2;
                }
                return new Point(x, y);
            }
        }

        public double SecondaryTranslateOffsetX {
            get {
                double x = -3;
                if (!IsPulling) {
                    return x;
                }
                double max_x = (-Width / 4);// - (SecondaryFontSize/2);
                double pull_percent = PullTranslateY / MaxPullTranslateY;
                x = max_x * pull_percent;
                return x;
            }
        }
        public double SecondaryTranslateOffsetY {
            get {
                double y = 3;
                if (!IsPulling) {
                    return y;
                }
                double max_top = (Height / 4) - 5;// - (SecondaryFontSize*1);
                double pull_percent = PullTranslateY / MaxPullTranslateY;
                y = max_top * pull_percent;
                return y;
            }
        }
        public double SecondaryFontSizeRatio => 
            IsAlphaNumericNumber ? 0.2 : 0.25;
        public int VisiblePopupColCount { get; set; }
        public int VisiblePopupRowCount { get; set; }

        public bool IsRightSideKey { get; private set; }
            

        public double Width { get; private set; }
        public double Height { get; private set; }
        public double InnerWidth =>
            Width - OuterPadX;
        public double InnerHeight =>
            Height - OuterPadY;

        public double OuterTranslateX {
            get {
                return NeedsOuterTranslate && IsVisible ?
                Parent.DefaultKeyWidth / 2 : 0;
            }
        }
        public int Row { get; set; }
        public int Column { get; set; }
        public double ColumnSpan { get; set; } = 1;
        public double PopupOffsetX { get; private set; }
        public double PopupOffsetY { get; private set; }
        #endregion

        #region State
        public bool IsHoldMenuOpen =>
            PopupKeys.Any() && PopupKeys.Skip(1).Any();

        bool IsSpecialDisplayText =>
            IsSpecial && SPECIAL_KEY_TEXTS.Contains(CurrentChar);
        public bool IsPrimaryImage =>
            CurrentChar != null && CurrentChar.EndsWith(".png");
        public bool CanShowPressPopup {
            get {
                if(!Parent.IsNumberRowVisible && Parent.IsTextLayout && Row == 0) {
                    return true;
                }
                return HasPressPopup;
            }
        }
        public bool CanShowHoldPopup {
            get {
                if(!Parent.IsNumberRowVisible && Parent.IsTextLayout && Row == 0) {
                    return true;
                }
                return HasHoldPopup;
            }
        }
        public DateTime? PressPopupShowDt { get; set; }
        public bool IsDefaultPopupKey =>
            PopupAnchorKey != null &&
            PopupAnchorKey.SecondaryCharacters.FirstOrDefault() == CurrentChar;
        public bool IsPrimarySpecial =>
            SpecialKeyType == SpecialKeyType.Enter ||
            SpecialKeyType == SpecialKeyType.Done ||
            SpecialKeyType == SpecialKeyType.Go ||
            SpecialKeyType == SpecialKeyType.Next;
        public string TouchId { get; set; }
        public DateTime? LastPressDt { get; set; }
        public DateTime? LastReleaseDt { get; set; }
        public bool CanPullKey =>
            !IsPopupKey && !string.IsNullOrEmpty(SecondaryValue);
        public bool IsPulling =>
            PullTranslateY > 0;// MaxPullTranslateY * 0.25;
        public bool IsPulled =>
            PullTranslateY >= MaxPullTranslateY; //* 0.75;
        public double PullTranslateY { get; set; } = 0;
        public double MaxPullTranslateY =>
            InnerHeight / 2;

        public bool HasAnyPopup =>
            HasPressPopup || HasHoldPopup;
        public bool HasPressPopup =>
            IsInput && !IsSpaceBar;
        public bool HasHoldPopup =>
            IsInput && SecondaryCharacters.Any(x => x != CurrentChar);
        //public bool IsActiveKey =>
        //    Parent.ActiveKeyViewModel == this;

        public bool IsActiveKey =>
            PopupAnchorKey != null && PopupAnchorKey.ActivePopupKey == this;

        public bool IsPressed { get; private set; }
        public bool IsSoftPressed { get; private set; }
        public bool IsPopupKey =>
            PopupKeyIdx >= 0;
        public bool IsFakePopupKey { get; set; }

        public bool IsLastPopupKey =>
            PopupAnchorKey != null &&
            PopupAnchorKey.PopupKeys
            .Where(x => !x.IsFakePopupKey)
            .OrderBy(x => x.PopupKeyIdx)
            .LastOrDefault() == this;
        public int PopupKeyIdx { get; set; } = -1;
        public bool NeedsOuterTranslate {
            get {
                if (IsPopupKey) {
                    if (PopupAnchorKey == null) {
                        return false;
                    }
                    return PopupAnchorKey.NeedsOuterTranslate;
                }
                var result = Row == TranslatedRow && Parent.IsLettersCharSet;
                if(result) {

                }
                return result;
            }
        }
        int TranslatedRow =>
            Parent.IsNumberRowVisible ? 2 : 1;
        public bool IsSpecial =>
            SpecialKeyType != SpecialKeyType.None;
        public bool IsPeriod =>
            CurrentChar == ".";
        public bool IsBackspace =>
            SpecialKeyType == SpecialKeyType.Backspace;
        public bool IsShiftKey =>
            SpecialKeyType == SpecialKeyType.Shift;
        public bool IsDotCom =>
            CurrentChar == ".com";

        public bool IsSymbolToggle =>
            SpecialKeyType == SpecialKeyType.SymbolToggle;
        public bool IsPrimarySpecialKey =>
            SpecialKeyType == SpecialKeyType.Done ||
            SpecialKeyType == SpecialKeyType.Search ||
            SpecialKeyType == SpecialKeyType.Go ||
            SpecialKeyType == SpecialKeyType.Enter ||
            SpecialKeyType == SpecialKeyType.Next;
        public bool IsSpaceBar =>
            CurrentChar == " ";
        public bool IsEmojiKey =>
            CurrentChar == EMOJI_SELECT_BTN_IMG_FILE_NAME;
        bool IsAlphaNumericNumber =>
            Parent.IsNumPadLayout &&
                        (Parent.IsDigits || Parent.IsPin) &&
                        IsNumber;
        public bool IsNumber =>
            "0123456789".Contains(CurrentChar);
        public bool IsInput =>
            !IsSpecial;
        public bool IsShiftKeyAndOnTemp =>
            IsShiftKey &&
            Parent.IsLettersCharSet &&
            Parent.IsShiftOnTemp;
        public bool IsShiftKeyAndOnLock =>
            IsShiftKey &&
            Parent.IsLettersCharSet &&
            Parent.IsShiftOnLock;


        public string CurrentChar { get; private set; }

        public string SecondaryValue { get; private set; }
        public string PrimaryValue =>
            Parent.IsAnyShiftState && IsInput ? CurrentChar.ToUpper() : CurrentChar;


        public IEnumerable<string> SecondaryCharacters { get; private set; }

        #endregion

        #region Model
        public SpecialKeyType SpecialKeyType { get; set; }
        public ObservableCollection<string> Characters { get; set; } = [];
        #endregion

        #endregion

        #region Events
        public event EventHandler OnCleanup;
        public event EventHandler OnHidePopup;
        #endregion

        #region Constructors
        public KeyViewModel(KeyboardViewModel parent, KeyViewModel prev, object keyObj) {
            Parent = parent;
            PrevKeyViewModel = prev;
            Init(keyObj);
        }

        #endregion

        #region Public Methods
        public void SetRenderer(IKeyboardViewRenderer renderer) {
            _renderer = renderer;
        }
        public void Cleanup() {
            OnCleanup?.Invoke(this, EventArgs.Empty);
        }

        public void SetPressed(bool isPressed, Touch t, bool isSoft = false) {
            if (isPressed) {
                IsPressed = true;
                IsSoftPressed = isSoft;
                TouchId = t.Id;
                LastPressDt = DateTime.Now;
                if (!Parent.PressedKeys.Contains(this)) {
                    Parent.PressedKeys.Add(this);
                }
            } else {
                ActivePopupKey = null;
                IsPressed = false;
                IsSoftPressed = false;
                PullTranslateY = 0;
                LastPressDt = null;
                TouchId = null;
                LastReleaseDt = DateTime.Now;
                Parent.PressedKeys.Remove(this);
                ClearPopups();

            }
            Renderer.Paint(true);
        }

        #region Popups
        public void UpdateActivePopup(Touch touch) {
            KeyViewModel last_active = ActivePopupKey;
            bool show_debug = false;
#if DEBUG 
            if (show_debug) {
                KeyboardGridView.DebugCanvas.Children.Clear();
                KeyboardGridView.DebugCanvas.InvalidateVisual();
                foreach (var pukvm in PopupKeys) {
                    if (pukvm.CheckIsActive(touch, true)) {
                        ActivePopupKey = pukvm;
                    }
                }
            } else {
                ActivePopupKey = PopupKeys.FirstOrDefault(x => x.CheckIsActive(touch, false));
            }
#else
            ActivePopupKey = PopupKeys.FirstOrDefault(x => x.CheckIsActive(touch,false));
#endif
            foreach (var pukvm in PopupKeys) {
                pukvm.Renderer.Render(true);
            }

            if (last_active != ActivePopupKey &&
                ActivePopupKey != null) {
                Parent.InputConnection.OnFeedback(Parent.ActiveChangeFeedback);
            }
        }
        public void FitPopupInFrame(Touch touch) {
            if (!PopupKeys.Any()) {
                return;
            }

            double l = PopupKeys.Min(x => x.TotalRect.Left);
            double t = PopupKeys.Min(x => x.TotalRect.Top);
            double r = PopupKeys.Max(x => x.TotalRect.Right);
            double b = PopupKeys.Max(x => x.TotalRect.Bottom);

            if (Parent.CanShowPopupWindows) {
                if(PopupKeys.Skip(1).Any()) {

                }
                // don't need to fit
                double w = r - l;
                double h = b - t;

                l = TotalRect.Center.X - (w / 2);
                t = Parent.KeyboardRect.Top + KeyboardRect.Top - h;
                r = l + w;
                b = t + h;

                double edge_pad = 5;
                double r_diff = r - Parent.KeyboardRect.Right;
                if (r_diff > 0) {
                    l = l - r_diff - edge_pad;
                    r = r - r_diff - edge_pad;
                }
                double l_diff = Parent.KeyboardRect.Left - l;
                if (l_diff > 0) {
                    l = l + l_diff + edge_pad;
                    r = r + l_diff + edge_pad;
                }
                PopupRect = new Rect(l, t, w, h);
            } else {
                double x_diff = 0;
                double y_diff = t - Parent.TotalRect.Top;

                bool contain_frame = true;

                if (contain_frame) {
                    if (y_diff < 0) {
                        t -= y_diff;
                        b -= y_diff;
                    } else {
                        y_diff = 0;
                    }
                    var this_key_rect = this.TotalRect;
                    var y_adj_rect = new Rect(l, t, r - l, b - t);
                    if (y_adj_rect.Intersects(this_key_rect)) {
                        if (IsRightSideKey) {
                            x_diff = this_key_rect.Left - r - Width;
                        } else {
                            x_diff = l - this_key_rect.Right + (Width * 2);
                        }
                    }
                } else {
                    //if (y_diff < 0) {
                    //    t += y_diff;
                    //    b += y_diff;
                    //} else {
                    //    y_diff = 0;
                    //}
                }
                foreach (var pukvm in PopupKeys) {
                    pukvm.TranslatePopupLocation(x_diff, -y_diff);
                }
                double tl = PopupKeys.Min(x => x.TotalRect.Left);
                double tt = PopupKeys.Min(x => x.TotalRect.Top);
                double tr = PopupKeys.Max(x => x.TotalRect.Right);
                double tb = PopupKeys.Max(x => x.TotalRect.Bottom);
                PopupRect = new Rect(tl, tt, tr, tb);
            }
            UpdateActivePopup(touch);
            this.Renderer.Render(true);
        }
        public void AddPopupAnchor(int r, int c, string disp_val) {
            if (Parent.PopupKeys.FirstOrDefault(x => x.Row == r && x.Column == c && (x.PopupAnchorKey == null || x.PopupAnchorKey == this)) is not { } pukvm) {
                return;
            }

            pukvm.SetPopupAnchor(this, disp_val);
            if (pukvm.IsVisible) {
                VisiblePopupColCount = Math.Max(c + 1, VisiblePopupColCount);
                VisiblePopupRowCount = Math.Max(r + 1, VisiblePopupRowCount);
            }
        }
        void ClearRects() {
            _primaryTextRect = null;
            _primaryImageRect = null;
            _secondaryTextRect = null;
            _innerRect = null;
            _keyboardRect = null;
            _totalRect = null;
        }

        public void SetPopupAnchor(KeyViewModel anchor_kvm, string disp_val) {
            ClearRects();
            PopupAnchorKey = anchor_kvm;
            SetCharacters([disp_val]);
            IsFakePopupKey = string.IsNullOrEmpty(disp_val);
            if (!Parent.VisiblePopupKeys.Contains(this)) {
                Parent.VisiblePopupKeys.Add(this);
            }
        }
        public void RemovePopupAnchor() {
            ClearRects();
            PopupOffsetX = 0;
            PopupOffsetY = 0;
            PopupAnchorKey = null;
            IsFakePopupKey = false;
            Parent.VisiblePopupKeys.Remove(this);
            Renderer.Layout(false);
            SetCharacters([]);
            Renderer.Paint(true);
        }

        public void ClearPopups() {
            bool had_popups = VisiblePopupColCount > 0 || VisiblePopupRowCount > 0;
            VisiblePopupColCount = 0;
            VisiblePopupRowCount = 0;
            ActivePopupKey = null;
            var to_rmv = PopupKeys.ToArray();
            foreach (var rmv in to_rmv) {
                rmv.RemovePopupAnchor();
            }
            if(had_popups) {
                OnHidePopup?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Display Values

        public IEnumerable<string> GetSecondaryCharacters() {
            if (Parent.IsSlideEnabled &&
                    !string.IsNullOrEmpty(SecondaryValue)) {
                // insert secondary for mobile
                yield return SecondaryValue;
            }
            if (LetterGroups.FirstOrDefault(x => x.Any(y => y == CurrentChar)) is not { } lgl) {
                yield break;
            }
            foreach (var lg in lgl) {
                yield return lg;
            }
        }
        void SetCharacters(IEnumerable<string> chars) {
            Characters.Clear();
            Characters.AddRange(chars);
            SecondaryCharacters = GetSecondaryCharacters();
            UpdateCharacters();
        }
        public void UpdateCharacters() {
            if (IsShiftKey && Parent.IsLettersCharSet) {
                if (IsShiftKeyAndOnLock) {
                    CurrentChar = SHIFT_LOCK_IMG_FILE_NAME;
                } else if (IsShiftKeyAndOnTemp) {
                    CurrentChar = SHIFT_ON_IMG_FILE_NAME;
                } else {
                    CurrentChar = SHIFT_IMG_FILE_NAME;
                }
            } else if (Characters.Any()) {
                int char_idx = Parent.CharSetIdx >= Characters.Count ? 0 : Parent.CharSetIdx;
                CurrentChar = Characters[char_idx] ?? string.Empty;
                int next_idx = char_idx + 1;
                if (next_idx >= Characters.Count) {
                    SecondaryValue = string.Empty;
                } else {
                    if (IsAlphaNumericNumber) {
                        SecondaryValue = GetAlphasForNumeric(CurrentChar);
                    } else {
                        SecondaryValue = Characters[next_idx] ?? string.Empty;
                    }
                    
                }
            } else {
                CurrentChar = string.Empty;
            }
            ClearRects();
        }

        #endregion


        public override string ToString() {
            return $"'{PrimaryValue}' X:{(int)X} Y:{(int)Y} W:{(int)Width} H:{(int)Height}";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void Init(object keyObj) {
            List<string> chars = [];
            
            if (keyObj is string keyStr && keyStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) is { } keyParts) {
                // special strings:
                // none
                // comma
                if (!keyParts.Any() && keyStr == ",") {
                    // encode comma for comma popup from its decoded display value
                    keyParts = ["comma"];
                }
                foreach (string kp in keyParts) {
                    string text = kp;
                    if (text == "none") {
                        text = null;
                    } else if (text == "comma") {
                        text = ",";
                    }
                    if (text == " ") {
                        
                    }
                    chars.Add(text);
                }
            } else if (keyObj is SpecialKeyType skt) {
                SpecialKeyType = skt;
                chars.AddRange(GetSpecialKeyCharsOrResourceKeys(skt));
            } else if (keyObj is int popupIdx) {
                PopupKeyIdx = popupIdx;
            }
            SetCharacters(chars);
            /*width
            IsPopupKey ? Parent.DefaultKeyWidth * PopupKeyWidthRatio :
             ColumnSpan * ((SpecialKeyType == SpecialKeyType.None || SpecialKeyType == SpecialKeyType.Emoji) ?
                Parent.DefaultKeyWidth :
                Parent.SpecialKeyWidth);
            */
            /*height
            !IsPopupKey && !Parent.IsNumPadLayout && IsNumber ? Parent.NumberKeyHeight : Parent.DefaultKeyHeight;
            */


            Renderer.Render(true);
        }
        public void SetSize() {
            if (IsSpaceBar) {
                int emoji_diff = Parent.IsEmojiButtonVisible ? 1 : 0;
                ColumnSpan =
                    Parent.KeyboardFlags.HasFlag(KeyboardFlags.Email) ||
                    Parent.KeyboardFlags.HasFlag(KeyboardFlags.Url) ? 3 : 5 - emoji_diff;
            } else if (IsSpecial && !IsEmojiKey) {
                ColumnSpan = Parent.SpecialKeyWidth / Parent.DefaultKeyWidth;
            } else {
                ColumnSpan = 1;
            }

            Width = Parent.DefaultKeyWidth * ColumnSpan;
            Height = Parent.DefaultKeyHeight;
            if (IsPopupKey) {
                Width *= PopupKeyWidthRatio;
            }
            if (!IsPopupKey && !Parent.IsNumPadLayout && IsNumber) {
                Height = Parent.NumberKeyHeight;
            }
        }
        public void SetBrushes() {
            string bg = KeyboardPalette.DefaultKeyBgHex;
            string fg = KeyboardPalette.FgHex;
            if (IsSpecial) {
                bg = IsPressed ? KeyboardPalette.SpecialKeyPressedBgHex : KeyboardPalette.SpecialKeyBgHex;
                if (IsShiftKeyAndOnTemp || IsShiftKeyAndOnLock) {
                    fg = KeyboardPalette.ShiftFgHex;
                } else if (IsPrimarySpecial) {
                    bg = IsPressed ? KeyboardPalette.PrimarySpecialKeyPressedBgHex : KeyboardPalette.PrimarySpecialKeyBgHex;
                }
            } else if (IsPopupKey) {
                bg = IsActiveKey ? KeyboardPalette.HoldFocusBgHex : KeyboardPalette.HoldBgHex;
                fg = KeyboardPalette.HoldFgHex;
            } else {
                bg = IsPressed ? KeyboardPalette.PressedBgHex : KeyboardPalette.DefaultKeyBgHex;
            }
            BgHex = bg;
            PrimaryHex = fg;

            if (IsSecondaryVisible) {
                SecondaryHex = KeyboardPalette.FgHex2;
            } else {
                SecondaryHex = KeyboardPalette.Transparent;
            }

            if(!IsPopupKey &&
                !IsPressed && 
                !Parent.IsKeyBordersVisible && 
                !IsBgAlwaysVisible) {
                BgHex = KeyboardPalette.Transparent;
            }
        }
        string GetAlphasForNumeric(string num) {
            switch (num) {
                default:
                    return string.Empty;
                case "2":
                    return "ABC";
                case "3":
                    return "DEF";
                case "4":
                    return "GHI";
                case "5":
                    return "JKL";
                case "6":
                    return "MNO";
                case "7":
                    return "PQRS";
                case "8":
                    return "TUV";
                case "9":
                    return "WXYZ";
                case "0":
                    return "+";
            }
        }
        IEnumerable<string> GetSpecialKeyCharsOrResourceKeys(SpecialKeyType skt) {
            switch (skt) {
                case SpecialKeyType.None:
                    yield break;
                case SpecialKeyType.Emoji:
                    yield return EMOJI_SELECT_BTN_IMG_FILE_NAME;
                    break;
                case SpecialKeyType.Shift:
                    yield return SHIFT_IMG_FILE_NAME;
                    yield return SHIFT_TEXT_2;
                    yield return SHIFT_TEXT_3;
                    break;
                case SpecialKeyType.Tab:
                    yield return TAB_TEXT;
                    break;
                case SpecialKeyType.CapsLock:
                    yield return CAPS_LOCK_TEXT;
                    break;
                case SpecialKeyType.Go:
                    yield return GO_TEXT;
                    break;
                case SpecialKeyType.Next:
                    yield return NEXT_TEXT;
                    break;
                case SpecialKeyType.Done:
                    yield return DONE_TEXT;
                    break;
                case SpecialKeyType.Backspace:
                    yield return BACKSPACE_IMG_FILE_NAME;
                    break;
                case SpecialKeyType.SymbolToggle:
                    yield return SYMBOLS1_TEXT;
                    yield return SYMBOLS2_TEXT;
                    break;
                case SpecialKeyType.NumberSymbolsToggle:
                    yield return NUM_SYMBOLS1_TEXT;
                    yield return NUM_SYMBOLS2_TEXT;
                    break;
                case SpecialKeyType.Enter:
                    yield return ENTER_TEXT;
                    break;
                case SpecialKeyType.Search:
                    yield return SEARCH_IMG_FILE_NAME;
                    break;

            }
        }
        bool CheckIsActive(Touch touch, bool debug) {
            if (IsPressed) {
                // popupkeys take active for input
                return !HasAnyPopup;
            }
            if (!IsPopupKey ||
                IsFakePopupKey ||
                PopupAnchorKey is not { } anchor_kvm ||
                anchor_kvm.DefaultPopupKey is not { } def_kvm) {
                return false;
            }

            // anchor touch offset to initial location
            var p = touch.Location;
            var pd = touch.PressLocation;
            double multiplier = 1d;// 3.5d;
            double offset_x = def_kvm.KeyboardRect.Center.X;
            double offset_y = def_kvm.KeyboardRect.Center.Y;
            double px = offset_x + ((p.X - pd.X) * multiplier);
            double py = offset_y + ((p.Y - pd.Y) * multiplier);

            // snap perimeter cells to outer bounds
            double ol = Math.Min(0, px);
            double ot = Math.Min(0, py);
            double or = Math.Max(Parent.KeyboardWidth, px);
            double ob = Math.Max(Parent.KeyboardHeight, py);
            double l = X;
            double t = Y;
            double r = X + Width;
            double b = Y + Height;

            if (Row == 0) {
                t = Math.Min(t, ot);
            }
            if (Row == anchor_kvm.VisiblePopupRowCount - 1) {
                b = Math.Max(b, ob);
            }
            if (Column == 0) {
                l = Math.Min(l, ol);
            }
            if (Column == anchor_kvm.VisiblePopupColCount - 1) {
                r = Math.Max(r, or);
            }
            if (IsLastPopupKey) {
                r = Math.Max(r, or);
                b = Math.Max(b, ob);
            }

            var hit_rect = new Rect(l, t, Math.Max(0, r - l), Math.Max(0, b - t));
            var adj_p = new Point(px, py);
            if (debug) {
                DrawActiveDebug(hit_rect, adj_p);
            }

            bool is_hit = hit_rect.Contains(adj_p);
            return is_hit;
        }

        Rect FindRect() {
            double x = 0;
            double y = 0;
            if (IsPopupKey) {
                if (PopupAnchorKey is not { } anchor_kvm) {
                    return new();
                }
                if(Parent.CanShowPopupWindows) {
                    x = Column * Width;
                    y = Row * Height;
                } else {
                    double origin_offset_x = 0;
                    if (anchor_kvm.IsRightSideKey && !Parent.CanShowPopupWindows) {
                        origin_offset_x = PopupAnchorKey.PopupKeys.Max(x => x.Column) * -Width;
                    }
                    x = anchor_kvm.X + (Column * Width) + origin_offset_x;

                    double origin_offset_y = PopupAnchorKey.PopupKeys.Max(x => x.Row) * Height;
                    y = anchor_kvm.Y - anchor_kvm.Height - origin_offset_y + (Row * Height);
                }
                return new Rect(x + PopupOffsetX, y + PopupOffsetY, Width, Height);
            }
            if (PrevKeyViewModel == null) {
                x = NeedsOuterTranslate ? OuterTranslateX : Parent.KeyboardMargin.Left;
            } else {
                x = PrevKeyViewModel.X + PrevKeyViewModel.Width;
            }
            y = Parent.KeyboardMargin.Top;
            if(Row > 0) {
                y = Parent.Keys.Where(x => !x.IsPopupKey && x.Row == Row - 1).Max(x => x.KeyboardRect.Bottom);
            }
            IsRightSideKey = x + (Width / 2) > (Parent.KeyboardWidth / 2);
            return new Rect(x, y, Width, Height);
        }
        void DrawActiveDebug(Rect hitRect, Point p) {
#if DEBUG
            var colors = new IBrush[]{
                Brushes.Red,
                Brushes.Orange,
                Brushes.Yellow,
                Brushes.Green,
                Brushes.Blue,
                Brushes.Indigo,
                Brushes.Violet,
                Brushes.Purple,
                Brushes.Red,
                Brushes.Orange,
                Brushes.Yellow,
                Brushes.Green,
                Brushes.Blue,
                Brushes.Indigo,
                Brushes.Violet,
                Brushes.Purple, };
            Color color = Colors.Pink;
            if (PopupKeyIdx < colors.Length && colors[PopupKeyIdx] is ImmutableSolidColorBrush scb) {
                color = scb.Color;
            }

            var cnvs = KeyboardGridView.DebugCanvas;

            var rect = new Avalonia.Controls.Shapes.Rectangle() {
                Tag = PopupKeyIdx,
                Opacity = 0.5,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = new SolidColorBrush(color, 0.5),
                Width = hitRect.Width,
                Height = hitRect.Height
            };

            cnvs.Children.Add(rect);
            Canvas.SetLeft(rect, hitRect.X);
            Canvas.SetTop(rect, hitRect.Y);

            double r = 2.5;
            var ellipse = new Avalonia.Controls.Shapes.Ellipse() {
                Opacity = 0.5,
                Fill = Brushes.Red,
                Width = r * 2,
                Height = r * 2
            };

            cnvs.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, p.X - r);
            Canvas.SetTop(ellipse, p.Y - r);
#endif
        }
        #endregion

        #region Commands
        public ICommand PerformTapActionCommand => ReactiveCommand.Create(
            () => {
                Parent.PerformKeyAction(this);
            });
        
        public ICommand PerformDoubleTapActionCommand => ReactiveCommand.Create(
            () => {
                if(!IsSpaceBar ||
                    Parent.IsNumPadLayout ||
                    !Parent.IsDoubleTapSpaceEnabled) {
                    return;
                }
                Parent.InputConnection.OnBackspace(1);
                Parent.InputConnection.OnText(". ");
            });
        #endregion
    }
}
