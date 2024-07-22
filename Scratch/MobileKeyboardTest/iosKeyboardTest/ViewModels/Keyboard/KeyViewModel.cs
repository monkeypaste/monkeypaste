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
    public class KeyViewModel : ViewModelBase, IKeyboardViewRenderer {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics 
        static IEnumerable<string> GetSpecialKeyChars(SpecialKeyType skt) {
            switch (skt) {
                case SpecialKeyType.None:
                    yield break;
                case SpecialKeyType.Shift:
                    yield return "⇧";
                    yield return "1/2";
                    yield return "2/2";
                    break;
                case SpecialKeyType.Tab:
                    yield return "Tab";
                    break;
                case SpecialKeyType.CapsLock:
                    yield return "Caps Lock";
                    break;
                case SpecialKeyType.Go:
                    yield return "Go";
                    break;
                case SpecialKeyType.Next:
                    yield return "Next";
                    break;
                case SpecialKeyType.Done:
                    yield return "Done";
                    break;
                case SpecialKeyType.Backspace:
                    yield return "⌫";
                    break;
                case SpecialKeyType.SymbolToggle:
                    yield return "!#1";
                    yield return "ABC";
                    break;
                case SpecialKeyType.NumberSymbolsToggle:
                    yield return "*+#";
                    yield return "123";
                    break;
                case SpecialKeyType.Enter:
                    yield return "⏎";
                    break;
                case SpecialKeyType.Search:
                    yield return "🔍";
                    break;

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
        public void Measure(bool invalidate) {
            _keyboardRect = FindRect();
            _totalRect = null;

            RaisePropertyChanged(nameof(NeedsSymbolTranslate));
            RaisePropertyChanged(nameof(NeedsOuterTranslate));
            RaisePropertyChanged(nameof(IsSpecial));
            RaisePropertyChanged(nameof(PrimaryFontSize));
            RaisePropertyChanged(nameof(SecondaryFontSize));
            RaisePropertyChanged(nameof(OuterTranslateX));
            RaisePropertyChanged(nameof(X));
            RaisePropertyChanged(nameof(Y));
            RaisePropertyChanged(nameof(PrimaryTranslateOffsetX));
            RaisePropertyChanged(nameof(PrimaryTranslateOffsetY));
            RaisePropertyChanged(nameof(SecondaryTranslateOffsetX));
            RaisePropertyChanged(nameof(SecondaryTranslateOffsetY));
            RaisePropertyChanged(nameof(PullTranslateY));
        }

        public void Paint(bool invalidate) {
            RaisePropertyChanged(nameof(IsSecondaryVisible));
            RaisePropertyChanged(nameof(IsPressed));
            RaisePropertyChanged(nameof(KeyOpacity));
            RaisePropertyChanged(nameof(PrimaryValue));
            RaisePropertyChanged(nameof(SecondaryValue));
            RaisePropertyChanged(nameof(PrimaryOpacity));
            RaisePropertyChanged(nameof(SecondaryOpacity));
            RaisePropertyChanged(nameof(IsShiftOn));
            RaisePropertyChanged(nameof(IsShiftLock));
        }

        public void Layout(bool invalidate) {
            RaisePropertyChanged(nameof(ZIndex));
            RaisePropertyChanged(nameof(IsActiveKey));

            RaisePropertyChanged(nameof(Width));
            RaisePropertyChanged(nameof(Height));
            RaisePropertyChanged(nameof(InnerWidth));
            RaisePropertyChanged(nameof(InnerHeight));
            RaisePropertyChanged(nameof(CornerRadius));
        }
        public void Render(bool invalidate) {
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
        public double PrimaryFontSize =>
            Math.Max(1, Math.Min(MaxFontSize, Math.Min(InnerWidth, InnerHeight) * PrimaryFontSizeRatio));
        public double SecondaryFontSize {
            get {
                double fs = Math.Max(1, Math.Min(MaxFontSize, Math.Min(InnerWidth, InnerHeight) * SecondaryFontSizeRatio));
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
        public bool IsSecondaryVisible {
            get {
                if (Parent.IsNumbers) {
                    if (SpecialKeyType == SpecialKeyType.NumberSymbolsToggle) {
                        return false;
                    }
                    return true;
                }
                if (Parent.CharSet == CharSetType.Letters && IsInput) {
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
        #endregion

        #region Layout

        #region Factors
        public double PopupScale => 2;
        public double PopupKeyWidthRatio =>
            1.07;
        public double DefaultOuterPadX =>
            7;// Math.Min(10, Width / Parent.MaxColCount);
        public double OuterPadX =>
            IsPopupKey ? 0 : DefaultOuterPadX;
        public double OuterPadY =>
            IsPopupKey ?
                0 :
                Parent.DefaultKeyHeight * 0.15;
        public double PrimaryFontSizeRatio => 
            Parent.IsNumbers && IsInput ? 
                1.25 :
                IsDotCom ? 0.3 : 0.75;
        public double MaxFontSize => 16;
        public double SecondaryFontSizeRatio => 0.33;
        public double SecondaryRatio => 0.25;
        string[] MisAlignedCharacters => [
            //"✖️",
            "♠️",
            "♣️"
            ];

        public CornerRadius CornerRadius {
            get {
                double cr = 3;
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
            Renderer.Measure(true);
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
                    tRect = new Rect(kbRect.X, kbRect.Y + Parent.MenuHeight, kbRect.Width, kbRect.Height);
                    _totalRect = tRect;
                }
                return tRect;
            }
        }


        public int VisiblePopupColCount { get; set; }
        public int VisiblePopupRowCount { get; set; }

        public bool IsRightSideKey =>
            X + (Width / 2) > (Parent.KeyboardWidth / 2);

        public double Width =>
            IsPopupKey ? Parent.DefaultKeyWidth * PopupKeyWidthRatio :
             ColumnSpan * (SpecialKeyType == SpecialKeyType.None ?
                Parent.DefaultKeyWidth :
                Parent.SpecialKeyWidth);
        public double Height =>
            !IsPopupKey && !Parent.IsNumbers && IsNumber ? Parent.NumberKeyHeight : Parent.DefaultKeyHeight;
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
        public int ColumnSpan { get; set; } = 1;

        public double PopupOffsetX { get; private set; }
        public double PopupOffsetY { get; private set; }
        #endregion

        #region State

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
        bool IsSpaceKey =>
            PrimaryValue == " ";
        public bool HasAnyPopup =>
            HasPressPopup || HasHoldPopup;
        public bool HasPressPopup =>
            IsInput && !IsSpaceKey;
        public bool HasHoldPopup =>
            IsInput && SecondaryCharacters.Any(x => x != CurrentChar);
        //public bool IsActiveKey =>
        //    Parent.ActiveKeyViewModel == this;

        public bool IsActiveKey =>
            PopupAnchorKey != null && PopupAnchorKey.ActivePopupKey == this;

        public bool IsPressed { get; private set; }
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
                return Row == 2 && Parent.CharSet == CharSetType.Letters;
            }
        }
        public bool NeedsSymbolTranslate =>
            MisAlignedCharacters.Contains(PrimaryValue);
        public bool IsSpecial =>
            SpecialKeyType != SpecialKeyType.None;
        public bool IsSpaceBar =>
            CurrentChar == " ";
        public bool IsPeriod =>
            CurrentChar == ".";
        public bool IsBackspace =>
            SpecialKeyType == SpecialKeyType.Backspace;
        public bool IsShift =>
            SpecialKeyType == SpecialKeyType.Shift;
        public bool IsDotCom =>
            CurrentChar == ".com";
        public bool IsNumber =>
            "0123456789".Contains(CurrentChar);
        public bool IsInput =>
            !IsSpecial;
        public bool IsShiftOn =>
            SpecialKeyType == SpecialKeyType.Shift &&
            Parent.CharSet == CharSetType.Letters &&
            Parent.ShiftState == ShiftStateType.Shift;
        public bool IsShiftLock =>
            SpecialKeyType == SpecialKeyType.Shift &&
            Parent.CharSet == CharSetType.Letters &&
            Parent.ShiftState == ShiftStateType.ShiftLock;
        bool IsShifted =>
            Parent.ShiftState != ShiftStateType.None;


        public string CurrentChar { get; private set; }

        public string SecondaryValue { get; private set; }
        public string PrimaryValue =>
            IsShifted && IsInput ? CurrentChar.ToUpper() : CurrentChar;


        public IEnumerable<string> SecondaryCharacters { get; private set; }

        #endregion

        #region Model
        public SpecialKeyType SpecialKeyType { get; set; }
        public ObservableCollection<string> Characters { get; set; } = [];
        #endregion

        #endregion

        #region Events
        public event EventHandler OnCleanup;
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

        public void SetPressed(bool isPressed, Touch t) {
            if (isPressed) {
                IsPressed = true;
                TouchId = t.Id;
                LastPressDt = DateTime.Now;
                if (!Parent.PressedKeys.Contains(this)) {
                    Parent.PressedKeys.Add(this);
                }
            } else {
                ActivePopupKey = null;
                IsPressed = false;
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
                pukvm.Render(true);
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
            if(PopupKeys.Count() > 1) {

            }
            double l = PopupKeys.Min(x => x.TotalRect.Left);
            double t = PopupKeys.Min(x => x.TotalRect.Top);
            double r = PopupKeys.Max(x => x.TotalRect.Right);
            double b = PopupKeys.Max(x => x.TotalRect.Bottom);

            double x_diff = 0;
            double y_diff = t - Parent.TotalRect.Top;

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
            foreach (var pukvm in PopupKeys) {
                pukvm.TranslatePopupLocation(x_diff, -y_diff);
            }
            UpdateActivePopup(touch);
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

        public void SetPopupAnchor(KeyViewModel anchor_kvm, string disp_val) {
            _keyboardRect = null;
            _totalRect = null;
            PopupAnchorKey = anchor_kvm;
            SetCharacters([disp_val]);
            IsFakePopupKey = string.IsNullOrEmpty(disp_val);
            if (!Parent.VisiblePopupKeys.Contains(this)) {
                Parent.VisiblePopupKeys.Add(this);
            }

            Renderer.Layout(false);
            Renderer.Paint(true);
        }
        public void RemovePopupAnchor() {
            _keyboardRect = null;
            _totalRect = null;
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
            VisiblePopupColCount = 0;
            VisiblePopupRowCount = 0;
            ActivePopupKey = null;
            var to_rmv = PopupKeys.ToArray();
            foreach (var rmv in to_rmv) {
                rmv.RemovePopupAnchor();
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
            if (Characters.Any()) {
                int char_idx = Parent.CharSetIdx >= Characters.Count ? 0 : Parent.CharSetIdx;
                CurrentChar = Characters[char_idx] ?? string.Empty;
                int next_idx = char_idx + 1;
                if (next_idx >= Characters.Count) {
                    SecondaryValue = string.Empty;
                } else {
                    SecondaryValue = Characters[next_idx] ?? string.Empty;
                }
            } else {
                CurrentChar = string.Empty;
            }
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
                        ColumnSpan = Parent.KeyboardFlags.HasFlag(KeyboardFlags.Email) || Parent.KeyboardFlags.HasFlag(KeyboardFlags.Url) ? 3: 5;
                    }
                    chars.Add(text);
                }
            } else if (keyObj is SpecialKeyType skt) {
                SpecialKeyType = skt;
                chars.AddRange(GetSpecialKeyChars(skt));
            } else if (keyObj is int popupIdx) {
                PopupKeyIdx = popupIdx;
            }
            SetCharacters(chars);
            Renderer.Render(true);
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
                double origin_offset = 0;
                if (anchor_kvm.IsRightSideKey) {
                    origin_offset = PopupAnchorKey.PopupKeys.Max(x => x.Column) * -Width;
                }
                x = anchor_kvm.X + (Column * Width) + origin_offset;

                double offset = PopupAnchorKey.PopupKeys.Max(x => x.Row) * Height;
                y = anchor_kvm.Y - anchor_kvm.Height - offset + (Row * Height);
                if (y < 0) {

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

        #endregion
    }
}
