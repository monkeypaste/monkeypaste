using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.VisualTree;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace iosKeyboardTest
{
    public class KeyViewModel : ViewModelBase {
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
                    yield return "⬆️";
                    yield return "1/2";
                    yield return "2/2";
                    break;
                case SpecialKeyType.Tab:
                    yield return "Tab";
                    break;
                case SpecialKeyType.CapsLock:
                    yield return "Caps Lock";
                    break;
                case SpecialKeyType.Next:
                    yield return "Next";
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
        #endregion

        #region Properties

        #region View Models
        public KeyboardViewModel Parent { get; set; }
        public KeyViewModel PrevKeyViewModel { get; set; }
        KeyViewModel PopupKeyParent =>
            IsPopupKey ? Parent.PressedKeyViewModel : null;
        #endregion

        #region Appearance
        public double PrimaryFontSize =>
            Math.Min(MaxFontSize, Math.Min(InnerWidth, InnerHeight) * PrimaryFontSizeRatio);
        public double SecondaryFontSize {
            get {
                double fs = Math.Min(MaxFontSize, Math.Min(InnerWidth, InnerHeight) * SecondaryFontSizeRatio);
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
                double op = IsSecondaryVisible ? 1:0;
                if(!IsPulling) {
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
                if(!IsPulling) {
                    return op;
                }
                double pull_percent = PullTranslateY / MaxPullTranslateY;
                op = 1 - pull_percent;
                return op;
            }
        }

        public double SecondaryTranslateOffsetX {
            get {
                double x = -3;
                if(!IsPulling) {
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
        public bool IsSecondaryVisible =>
            Parent.IsNumbers ||
            (Parent.CharSet == CharSetType.Letters && IsInput);
        public bool IsVisible =>
            IsPopupKey || !string.IsNullOrEmpty(PrimaryValue);
        #endregion

        #region Layout

        #region Factors
        double PopupKeyWidthRatio =>
            1.07;
        double DefaultOuterPadX =>
            Math.Min(5, Parent.DefaultKeyWidth / Parent.MaxColCount);
        double OuterPadX =>
            IsPopupKey ? 0 : DefaultOuterPadX;
        double OuterPadY => 
            IsPopupKey ? 
                0 : 
                Parent.KeyHeight * 0.15;
        public double PopupOffsetX =>
            Parent.PopupOverflowTranslateX;
        double PopupOffsetY => 0;
        double PrimaryFontSizeRatio => 0.5;
        double MaxFontSize => 16;
        double SecondaryFontSizeRatio => 0.25;
        double SecondaryRatio => 0.25;
        string[] MisAlignedCharacters => [
            //"✖️",
            "♠️",
            "♣️"
            ];

        public CornerRadius CornerRadius {
            get {
                double cr = 5;
                if (IsPopupKey) {
                    int max_row = Parent.PopupKeys.Max(x => x.Row);
                    int max_col = Parent.PopupKeys.Max(x => x.Column);
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
        
        int MaxPopupColCount => Parent.MaxPopupColCount;
        public double X {
            get {
                double x = 0;
                if(IsPopupKey && 
                    PopupKeyParent is { } pressed_kvm) {
                    double origin_offset = 0;
                    if(pressed_kvm.IsRightSideKey) {
                       origin_offset = Parent.PopupKeys.Max(x => x.Column) * -Width;
                    }
                    x = pressed_kvm.X + (Column * Width) + origin_offset; 
                } else if(PrevKeyViewModel == null) {
                    if(NeedsOuterTranslate) {
                        x = OuterTranslateX;
                    }
                 } else {
                    x = PrevKeyViewModel.X + PrevKeyViewModel.Width;
                }
                return x;
            }
        }

        public double Y {
            get {
                if(IsPopupKey && PopupKeyParent is { } pressed_kvm) {
                    if(Row == 1) {

                    }
                    double offset = Parent.PopupKeys.Max(x => x.Row) * Height;
                    return pressed_kvm.Y - pressed_kvm.Height - offset + (Row*Height);
                }
                return Row * Height;
            }
        }

        
        public bool IsRightSideKey =>
            X + (Width / 2) > (Parent.KeyboardWidth / 2);

        public double Width =>
            IsPopupKey ? Parent.DefaultKeyWidth*PopupKeyWidthRatio :
             ColumnSpan * (SpecialKeyType == SpecialKeyType.None ?
                Parent.DefaultKeyWidth :
                Parent.SpecialKeyWidth);
        public double Height =>
            Parent.KeyHeight;
        public double InnerWidth =>
            Width - OuterPadX;
        public double InnerHeight =>
            Height - OuterPadY;

        public double OuterTranslateX =>
            NeedsOuterTranslate && IsVisible ?
                Parent.DefaultKeyWidth / 2 : 0;
        public int Row { get; set; }
        public int Column { get; set; }
        public int ColumnSpan { get; set; } = 1;
        #endregion

        #region State
        public bool CanPullKey =>
            !IsPopupKey && !string.IsNullOrEmpty(SecondaryValue);
        public bool IsPulling =>
            PullTranslateY >= MaxPullTranslateY/2;
        public bool IsPulled =>
            PullTranslateY >= MaxPullTranslateY * 0.75;
        public double PullTranslateY { get; set; } = 0;
        public double MaxPullTranslateY =>
            InnerHeight/2;
        public bool CanRepeat =>
            SpecialKeyType == SpecialKeyType.Backspace;
        bool IsSpaceKey =>
            PrimaryValue == " ";
        public bool HasAnyPopup =>
            HasPressPopup || HasHoldPopup;
        public bool HasPressPopup =>
            IsInput && !IsSpaceKey;
        public bool HasHoldPopup =>
            SecondaryCharacters.Any(x=>x != CurrentChar);
        //public bool IsActiveKey =>
        //    Parent.ActiveKeyViewModel == this;
        public DateTime? LastActiveDt { get; set; }
        public bool IsActiveKey {
            get {
                if (IsPressed) {
                    if (Parent != null && Parent.PopupKeys.Any()) {
                        // popupkeys take active for input
                        return false;
                    }
                    return true;
                }
                if (!IsPopupKey ||
                    //IsFakePopupKey ||
                    Parent.KeyboardPointerDownLocation is not { } pd ||
                    Parent.KeyboardPointerLocation is not { } p ||
                    Parent.DefaultPopupKey is not { } def_pu_kvm ||
                    Parent.PopupKeys is not { } pu_kvml) {
                    return false;
                }

                // anchor touch offset to initial location
                double multiplier = 3.5d;
                double initial_x = def_pu_kvm.X;
                double initial_y = def_pu_kvm.Y;
                double px = initial_x + ((p.X - pd.X) * multiplier);
                double py = initial_y + ((p.Y - pd.Y) * multiplier);

                bool in_x = false;
                bool in_y = false;

                bool is_rightest = Parent.PopupKeys/*.Where(x=>!x.IsFakePopupKey)*/.Max(x => x.X) == X;
                if (Column == 0) {
                    // first col
                    in_x = px <= X + Width;
                }
                if (!in_x && is_rightest) {
                    // last col
                    in_x = px >= X;
                }
                if (!in_x) {
                    // inner col
                    in_x = px >= X && px <= X + Width;
                }

                bool is_bottomest = Parent.PopupKeys/*.Where(x => !x.IsFakePopupKey)*/.Max(x => x.Y) == Y;
                if (Row == 0) {
                    // top row 
                    in_y = py <= Y + Height;
                }
                if (!in_y && is_bottomest) {
                    // bottom row
                    in_y = py >= Y;
                }
                if (!in_y) {
                    // inner row
                    in_y = py >= Y && py <= Y + Height;
                }
                if(in_x && in_y) {

                }
                return in_x && in_y;
            }
        }
        public DateTime? PressedDt { get; private set; }
        public bool IsPressed { get; set; }
        public bool IsPopupKey =>
            PopupKeyIdx >= 0;
        public bool IsFakePopupKey =>
            IsPopupKey && PrimaryValue == string.Empty;
        public int PopupKeyIdx { get; set; } = -1;
        public int PopupRowIdx { get; set; } = 0;
        public int PopupRowCount =>
            Parent.PopupKeys.Any() ?
                Parent.PopupKeys.Max(x => x.PopupRowIdx) + 1 : 0;
        public bool NeedsOuterTranslate { get; set; }
        public bool NeedsSymbolTranslate =>
            MisAlignedCharacters.Contains(PrimaryValue);
        public bool IsSpecial =>
            SpecialKeyType != SpecialKeyType.None;
        public bool IsSpaceBar =>
            CurrentChar == " ";
        public bool IsInput =>
            !IsSpecial;
        public bool IsShiftOn =>
            SpecialKeyType == SpecialKeyType.Shift &&
            CharSet == CharSetType.Letters &&
            ShiftState == ShiftStateType.Shift;
        public bool IsShiftLock =>
            SpecialKeyType == SpecialKeyType.Shift &&
            CharSet == CharSetType.Letters &&
            ShiftState == ShiftStateType.ShiftLock;
        bool IsShifted =>
            ShiftState != ShiftStateType.None;

        CharSetType CharSet {
            get => Parent.CharSet;
            set => Parent.CharSet = value;
        }
        ShiftStateType ShiftState {
            get => Parent.ShiftState;
            set => Parent.ShiftState = value;
        }

        int CharIdx =>
            (int)CharSet >= Characters.Count ? 0 : (int)CharSet;

        public string CurrentChar {
            get {
                if (!Characters.Any()) {
                    return string.Empty;
                }

                return Characters[CharIdx] ?? string.Empty;
            }
        }

        public string SecondaryValue {
            get {
                int next_idx = CharIdx + 1;
                if (next_idx >= Characters.Count) {
                    return string.Empty;
                }
                return Characters[next_idx] ?? string.Empty;
            }
        }
        public string PrimaryValue =>
            IsShifted && IsInput ? CurrentChar.ToUpper() : CurrentChar;

        public IEnumerable<string> SecondaryCharacters {
            get {
                if(LetterGroups.FirstOrDefault(x => x.Any(y => y == CurrentChar)) is not { } lgl) {
                    yield break;
                }
                foreach (var lg in lgl) {
                    yield return lg;
                }
            }
        }

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
        public KeyViewModel(KeyboardViewModel parent, KeyViewModel prev, object keyObj)
        {
            Parent = parent;
            PrevKeyViewModel = prev;
            Init(keyObj);
            PropertyChanged += KeyViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public void Cleanup() {
            PropertyChanged -= KeyViewModel_PropertyChanged;
            OnCleanup?.Invoke(this, EventArgs.Empty);
        }
        public override string ToString()
        {
            return $"{PrimaryValue} X:{(int)X} Y:{(int)Y} W:{(int)Width} H:{(int)Height}";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void Init(object keyObj) {
            if (keyObj is string keyStr && keyStr.Split(new string[] {","},StringSplitOptions.RemoveEmptyEntries) is { } keyParts) {
                // special strings:
                // none
                // comma
                if(!keyParts.Any() && keyStr == ",") {
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
                        ColumnSpan = 5;
                    }
                    Characters.Add(text);
                }
            } else if (keyObj is SpecialKeyType skt) {
                SpecialKeyType = skt;
                Characters.AddRange(GetSpecialKeyChars(skt));
            }
        }

        private void KeyViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsPressed):
                    //PullTranslateY = IsPressed ? 50 : 0;
                    //this.RaisePropertyChanged(nameof(PullTranslateY));
                    PressedDt = IsPressed ? DateTime.Now : null;
                    if(!IsPressed) {
                        PullTranslateY = 0;
                    }
                    break;
                case nameof(IsActiveKey):
                    if(IsActiveKey) {
                        LastActiveDt = DateTime.Now;
                    }
                    break;
            }
        }
        
        #endregion

        #region Commands

        #endregion
    }
}
