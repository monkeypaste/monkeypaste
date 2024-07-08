using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Size = Avalonia.Size;

namespace iosKeyboardTest
{
    public class KeyboardViewModel : ViewModelBase {
        #region Private Variables        
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public KeyboardMainViewModel Parent { get; private set; }
        public ObservableCollection<KeyViewModel> Keys { get; set; } = [];
        public KeyViewModel PressedKeyViewModel =>
            Keys
            .FirstOrDefault(x => x != null && x.IsPressed);
        public KeyViewModel ActiveKeyViewModel =>
            Keys
            .FirstOrDefault(x => x != null && x.IsActiveKey);

        public IEnumerable<KeyViewModel> PopupKeys =>
            Keys
            .Where(x => x != null && x.IsPopupKey)
            .OrderBy(x => x.PopupKeyIdx);

        public KeyViewModel DefaultPopupKey =>
            PressedKeyViewModel == null || !PopupKeys.Any() ?
                null :
                PopupKeys.FirstOrDefault(x => x.CurrentChar == PressedKeyViewModel.CurrentChar);

        IEnumerable<IEnumerable<KeyViewModel>> Rows =>
            Keys.Where(x => x != null && !x.IsPopupKey)
            .OrderBy(x => x.Column)
            .GroupBy(x => x.Row);
        #endregion

        #region Layout

        public double PopupWidth =>
            PopupKeys.Any() ?
                (PopupKeys.Max(x => x.Column) + 1) * DefaultKeyWidth :
                0;
        public double PopupHeight =>
            PopupKeys.Any() ?
                (PopupKeys.Max(x => x.Row) + 1) * KeyHeight :
                0;
        public int MaxColCount =>
            Rows.Max(x => x.Count());
        public Size ScreenSize =>
            Parent.ScreenSize;

        private double _defKeyWidth = -1;
        public double DefaultKeyWidth {
            get {
                if (_defKeyWidth <= 0) {
                    _defKeyWidth = KeyboardWidth / MaxColCount;

                }
                return _defKeyWidth;
            }
        }

        private double _keyHeight = -1;
        public double KeyHeight {
            get {
                if (_keyHeight <= 0) {
                    _keyHeight = KeyboardHeight / Rows.Count();
                }
                return _keyHeight;
            }
        }
        public int MaxPopupColCount =>
            4;

        double SpecialKeyWidthRatio => 1.5d;
        public double SpecialKeyWidth =>
            DefaultKeyWidth * (IsNumbers ? 1 : SpecialKeyWidthRatio);

        public double KeyboardWidth =>
            ScreenSize.Width;
        public double KeyboardHeight =>
            ScreenSize.Height * KeyboardMainViewModel.TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO;

        public double PopupOverflowTranslateX {
            get {
                if (PressedKeyViewModel == null ||
                    !PopupKeys.Any()) {
                    return 0;
                }

                // NOTE this calculation needs to match how X is decided in kvm
                // the only difference is it doesn't account for PopupOffset
                double pukl_left = PressedKeyViewModel.X;
                double pukl_right = pukl_left + PopupKeys.Sum(x => x.Width);
                double offsetX = 0;
                if (pukl_right > KeyboardWidth) {
                    // popup overflows right side, shift left
                    offsetX = KeyboardWidth - pukl_right;
                }
                double new_left = pukl_left + offsetX;
                if (new_left < 0) {
                    // TODO? if this happens then popup keys need to wrap...

                }
                return offsetX;
            }
        }
        #endregion

        #region Appearance
        public bool IsSecondaryVisible =>
            IsNumbers || CharSet == CharSetType.Letters;
        #endregion

        #region State
        bool IsActiveOverriden { get; set; }
        public bool IsNumbers =>
            KeyboardFlags.HasFlag(KeyboardFlags.Numbers);
        KeyboardFlags KeyboardFlags { get; set; }
        bool IsHoldPopupVisible =>
            //PopupKeys.Any() &&
            //PressedKeyViewModel != null &&
            //ActiveKeyViewModel != null &&
            //PressedKeyViewModel.PrimaryValue != ActiveKeyViewModel.PrimaryValue;
            PopupKeys.Count() > 1;
        uint RepeatCount { get; set; }

        TimeSpan MinHoldDur => TimeSpan.FromMilliseconds(300);
        //TimeSpan MinRepeatDur => TimeSpan.FromMilliseconds((int)(300/(RepeatCount == 0 ? 1:RepeatCount)));
        TimeSpan MinRepeatDur => TimeSpan.FromMilliseconds(300);
        public Point? KeyboardPointerLocation { get; private set; }
        public Point? KeyboardPointerDownLocation { get; private set; }
        public CharSetType CharSet { get; set; }
        public ShiftStateType ShiftState { get; set; }
        IKeyboardInputConnection InputConnection { get; set; }
        #endregion

        #endregion

        #region Constructors
        public KeyboardViewModel() : this(null,null) { }
        public KeyboardViewModel(KeyboardMainViewModel parent, IKeyboardInputConnection inputConn) 
        {
            Debug.WriteLine("kbvm ctor called");
            Parent = parent ?? new();
            SetInputConnection(inputConn); 
            RefreshLayout();
            Init(KeyboardFlags.Phone);
        }
        #endregion

        #region Public Methods
        public void SetInputConnection(IKeyboardInputConnection conn) {
            InputConnection = conn;
        }
        public void RefreshLayout() {
            _defKeyWidth = -1;
            _keyHeight = -1;
            RefreshKeyboardState();
        }
        public void SetPointerLocation(Point? mp) {
            var last_pressed_kvm = PressedKeyViewModel;
            var new_pressed_kvm = mp.HasValue ? GetKeyUnderPoint(mp.Value) : null;
            var last_mp = KeyboardPointerLocation;
            KeyboardPointerLocation = mp;
            if(KeyboardPointerDownLocation == null || KeyboardPointerLocation == null) {
                // set down on press and clear on release
                KeyboardPointerDownLocation = KeyboardPointerLocation;
                IsActiveOverriden = false;
            }

            if (KeyboardPointerLocation.HasValue &&
                (last_pressed_kvm == new_pressed_kvm || IsHoldPopupVisible)) {
                // still over same key

                if((ActiveKeyViewModel == null || IsActiveOverriden) &&
                    PopupKeys.OrderByDescending(x=>x.LastActiveDt).FirstOrDefault() is { } lapu_kvm) {
                    // weird arrangement, would need to inflate perimeter cells to keyboard edges to do this hack..
                    // clear overrides to see if one found...

                    foreach(var pkvm in PopupKeys) {
                        pkvm.ClearActive();
                    }
                    if(ActiveKeyViewModel == null) {
                        // still weird, use last
                        foreach (var pkvm in PopupKeys) {
                            pkvm.IsActiveKey = pkvm == lapu_kvm;
                        }
                        IsActiveOverriden = true;
                    } else {
                        IsActiveOverriden = false;
                    }
                }
            } else {
                if (last_pressed_kvm == null) {
                    // new press
                    PressKey(new_pressed_kvm);
                    StartHoldTimer();
                } else if (new_pressed_kvm == null) {
                    // release
                    ReleaseKey(ActiveKeyViewModel);
                    ClearHoldKeys(true);
                } else if(last_pressed_kvm != new_pressed_kvm) {
                    // drag enter
                    ClearHoldKeys(true);
                    PressKey(new_pressed_kvm);
                    StartHoldTimer();
                }
            }
            if(!KeyboardPointerLocation.HasValue) {
                // this shouldn't be needed but maybe due to desktop
                ClearHoldKeys(true);
            }
            RefreshKeyboardState();
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        void Init(KeyboardFlags flags)
        {
            KeyboardFlags = flags;
            var keys = GetKeys(KeyboardFlags);
            for(int r = 0; r < keys.Count; r++)
            {
                KeyViewModel prev_kvm = null;
                for(int c = 0; c < keys[r].Count; c++)
                {
                    var keyObj = keys[r][c];
                    int cur_col = prev_kvm == null ? 0 : prev_kvm.Column + prev_kvm.ColumnSpan;
                    var kvm = CreateKeyViewModel(keys[r][c], r, cur_col,prev_kvm);
                    Keys.Add(kvm);
                    prev_kvm = kvm;
                }
            }
            RefreshKeyboardState();
        }
        KeyViewModel GetActiveKey() {
            if(KeyboardPointerLocation is not { } mp) {
                return null;
            }
            var px = mp.X;
            var py = mp.Y;
            double dist((double,double) p1, (double,double) p2) {
                return Math.Sqrt(Math.Pow(p2.Item1 - p1.Item1,2) + Math.Pow(p2.Item2 - p1.Item2,2));
            }
            if(IsHoldPopupVisible &&
                KeyboardPointerDownLocation is { } pd &&
                DefaultPopupKey is { } def_pu_kvm) {
                // anchor touch offset to initial location
                double multiplier = 3.5d;
                double initial_x = def_pu_kvm.X;
                double initial_y = def_pu_kvm.Y;
                px = initial_x + ((px - pd.X) * multiplier);
                py = initial_y + ((px - pd.Y) * multiplier);

                var closest = 
                    PopupKeys.Where(x=>!x.IsFakePopupKey).Aggregate((a, b) => dist((px, py), (a.X + a.Width / 2, a.Y + a.Height / 2)) < dist((px, py), (b.X + b.Width / 2, b.Y + b.Height / 2)) ? a : b);
                return closest;
            }
            return PressedKeyViewModel;
        }
        KeyViewModel GetKeyUnderPoint(Point p) {
            var result = Keys
                .Where(x => x != null)
                .Select(x => (x, new Rect(x.X, x.Y, x.Width, x.Height)))
                .FirstOrDefault(x => x.Item2.Contains(p));
            return result.x;
        }
        void RefreshKeyboardState() {
            this.RaisePropertyChanged(nameof(Keys));
            this.RaisePropertyChanged(nameof(KeyboardWidth));
            this.RaisePropertyChanged(nameof(KeyboardHeight));

            foreach (var row in Rows.ToList()) {
                // center middle row for non-symbol char set
                bool needs_trans = row.Any(x => !x.IsVisible);
                foreach (var key in row) {
                    key.NeedsOuterTranslate = needs_trans;
                }
            }

            foreach (var key in Keys) {
                if(key == null) {
                    continue;
                }
                key.RaisePropertyChanged(nameof(key.IsSecondaryVisible));
                key.RaisePropertyChanged(nameof(key.PrimaryValue));
                key.RaisePropertyChanged(nameof(key.SecondaryValue));
                key.RaisePropertyChanged(nameof(key.PrimaryFontSize));
                key.RaisePropertyChanged(nameof(key.SecondaryFontSize));
                key.RaisePropertyChanged(nameof(key.IsShiftOn));
                key.RaisePropertyChanged(nameof(key.IsShiftLock));
                key.RaisePropertyChanged(nameof(key.X));
                key.RaisePropertyChanged(nameof(key.Y));
                key.RaisePropertyChanged(nameof(key.Width));
                key.RaisePropertyChanged(nameof(key.Height));
                key.RaisePropertyChanged(nameof(key.InnerWidth));
                key.RaisePropertyChanged(nameof(key.InnerHeight));
                key.RaisePropertyChanged(nameof(key.IsVisible));
                key.RaisePropertyChanged(nameof(key.OuterTranslateX));
                key.RaisePropertyChanged(nameof(key.NeedsSymbolTranslate));
                key.RaisePropertyChanged(nameof(key.IsActiveKey));
                key.RaisePropertyChanged(nameof(key.IsPressed));
                //Debug.WriteLine(key.PrimaryValue);
            }
        }

        KeyViewModel CreatePopUpKeyViewModel(KeyViewModel source_kvm, int popup_idx, int total_count, string disp_val) {
            if(source_kvm.IsSpecial) {
                return null;
            }
            int r = (int)Math.Floor((double)popup_idx / (double)(MaxPopupColCount));
            int c = popup_idx % MaxPopupColCount;
            var pu_kvm = CreateKeyViewModel(disp_val, r, c, PopupKeys.LastOrDefault());
            pu_kvm.PopupKeyIdx = popup_idx;
            return pu_kvm;
        }
        
        KeyViewModel CreateKeyViewModel(object keyObj, int r, int c, KeyViewModel prev)
        {
            var kvm = new KeyViewModel(this,prev, keyObj)
            {
                Row = r,
                Column = c
            };            
            return kvm;
        }
        string GetAlphasForNumeric(string num) {
            switch(num) {
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
        List<List<object>> GetKeys(KeyboardFlags kbFlags) {
            List<List<object>> keys = null;
            if (kbFlags.HasFlag(KeyboardFlags.Phone)) {
                if (kbFlags.HasFlag(KeyboardFlags.Numbers)) {
                    keys = new List<List<object>>
                {
                    (["1,(", "2,/", "3,)", SpecialKeyType.Backspace]),
                    (["4,N", "5,comma", "6,.", SpecialKeyType.Next]),
                    (["7,*", "8,;", "9,#", SpecialKeyType.NumberSymbolsToggle]),
                    (["*,-", "0,+", "#,__", "comma"])
                };
                } else {
                    keys = new List<List<object>>
                {
                    (["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"]),
                    (["q,+,`", "w,×,~", "e,÷,\\", "r,=,|", "t,/,{", "y,_,}", "u,<,€", "i,>,£", "o,[,¥", "p,],₩"]),
                    (["a,!,○", "s,@,•", "d,#,⚪", "f,$,⚫", "g,%,□", "h,^,🔳", "j,&,♤", "k,*,♡", "l,(,♢", "none,),♧"]),
                    ([SpecialKeyType.Shift, "z,-,☆", "x,',▪", "c,\",▫", "v,:,≪", "b,;,≫", "n,comma,¡", "m,?,¿", SpecialKeyType.Backspace]),
                    ([SpecialKeyType.SymbolToggle, "comma", " ", ".", SpecialKeyType.Enter])
                };
                }

            } else {
                keys = new List<List<object>>
                {
                    (["1", "2", "3", "4", "5", "6", "7", "8", "9", "0", SpecialKeyType.Backspace]),
                    ([SpecialKeyType.Tab, "q,+,`", "w,×,~", "e,÷,\\", "r,=,|", "t,/,{", "y,_,}", "u,<,€", "i,>,£", "o,[,¥", "p,],₩"]),
                    ([SpecialKeyType.CapsLock, "a,!,○", "s,@,•", "d,#,⚪", "f,$,⚫", "g,%,□", "h,^,🔳", "j,&,♤", "k,*,♡", "l,(,♢", "none,),♧", SpecialKeyType.Enter]),
                    ([SpecialKeyType.Shift, "z,-,☆", "x,',▪", "c,\",▫", "v,:,≪", "b,;,≫", "n,comma,¡", "m,?,¿", "comma",".", SpecialKeyType.Shift]),
                    ([SpecialKeyType.SymbolToggle,SpecialKeyType.Emoji, " ", SpecialKeyType.ArrowLeft, SpecialKeyType.ArrowRight, SpecialKeyType.NextKeyboard])
                };
            }
            return keys;
        }
        
        void ToggleSymbolSet() {
            if(CharSet == CharSetType.Numbers1) {
                CharSet = CharSetType.Numbers2;
            } else if(CharSet == CharSetType.Numbers2) {
                CharSet = CharSetType.Numbers1;
            }

            if (CharSet == CharSetType.Letters) {
                CharSet = CharSetType.Symbols1;
            } else {
                CharSet = CharSetType.Letters;
            }
        }
        void ToggleCapsLock() {
            if(ShiftState == ShiftStateType.ShiftLock) {
                ShiftState = ShiftStateType.None;
            } else {
                ShiftState = ShiftStateType.ShiftLock;
            }
        }
        void HandleShift() {
            if (CharSet == CharSetType.Letters) {
                if (ShiftState == ShiftStateType.ShiftLock) {
                    ShiftState = ShiftStateType.None;
                } else {
                    ShiftState = (ShiftStateType)((int)ShiftState + 1);
                }
            } else {
                if (CharSet == CharSetType.Symbols1) {
                    CharSet = CharSetType.Symbols2;
                } else {
                    CharSet = CharSetType.Symbols1;
                }
            }
        }

        void ClearHoldKeys(bool isRelease = false) {
            //return;
            var to_remove = Keys.Where(x => x != null && x.IsPopupKey).ToList();
            foreach (var rmv in to_remove) {
                Keys.Remove(rmv);
            }
            
            if(isRelease) {
                var to_clear = Keys.Where(x => x != null && x.IsPressed).ToList();
                foreach (var rmv in to_clear) {
                    rmv.IsPressed = false;
                }
            }
        }
        static int hold_timer_count = 0;
        private void StartHoldTimer() {
            hold_timer_count++;
            if(PressedKeyViewModel == null) {
                FinishHoldTimer();
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var cur_pressed_kvm = PressedKeyViewModel;
                var hold_sw = Stopwatch.StartNew();
                while(true) {
                    if(cur_pressed_kvm != PressedKeyViewModel) {
                        // no longer holding
                        FinishHoldTimer();
                        return;
                    }
                    if(cur_pressed_kvm.CanRepeat) {
                        // only backspace key
                        if(hold_sw.Elapsed >= MinRepeatDur) {
                            RepeatCount++;
                            hold_sw.Restart();
                            Console.WriteLine($"Repeat: {RepeatCount}");
                            for (int i = 0; i < RepeatCount; i++) {
                                ReleaseKey(cur_pressed_kvm,true);
                            }
                        }
                    } else {
                        if (hold_sw.Elapsed >= MinHoldDur) {
                            // hold
                            if(cur_pressed_kvm == null ||
                                (cur_pressed_kvm != null && cur_pressed_kvm.HasHoldPopup)) {
                                ShowHoldPopup(cur_pressed_kvm);
                            }
                            FinishHoldTimer();
                            return;
                        }
                    }
                    
                    await Task.Delay(30);
                }
            });
        }
        void FinishHoldTimer() {
            hold_timer_count--;
            Debug.WriteLine($"Hold timer done. Remaining: {hold_timer_count}");
        }

        void ShowPressPopup(KeyViewModel kvm) {
            ClearHoldKeys();

            if (kvm != null && kvm.HasPressPopup) {
                kvm.IsPressed = true;
                var sec_kvm = CreatePopUpKeyViewModel(kvm, 0,1, kvm.CurrentChar);
                Keys.Add(sec_kvm);
            }
            RefreshKeyboardState();
        }
        void ShowHoldPopup(KeyViewModel kvm) {
            ClearHoldKeys();
            if(kvm != null && kvm.HasHoldPopup) {
                var chars = kvm.SecondaryCharacters.ToList();
                int count = chars.Count;
                if (count > MaxPopupColCount) {
                    // add fake ones to keep popup square
                    int col_diff = count % MaxPopupColCount;
                    while(col_diff != 0) {
                        chars.Add(string.Empty);
                        count++; 
                        col_diff = count % MaxPopupColCount;
                    }
                }
                var sec_kvml =
                    chars
                    .Select((x, idx) => CreatePopUpKeyViewModel(kvm, idx, chars.Count, x))
                    .Where(x=>x != null);
                Keys.AddRange(sec_kvml);
            }
            RefreshKeyboardState();
        }
        void ShowPullKey(KeyViewModel kvm) {

        }
        void PressKey(KeyViewModel kvm) {
            if(kvm == null) {
                return;
            }
            kvm.IsPressed = true;
            ShowPressPopup(kvm);
            Debug.WriteLine($"Hold {kvm.CurrentChar}");
        }
        void ReleaseKey(KeyViewModel kvm, bool isRepeat = false) {
            if (kvm == null) {
                return;
            }
            switch (kvm.SpecialKeyType) {
                case SpecialKeyType.Shift:
                    HandleShift();
                    break;
                case SpecialKeyType.SymbolToggle:
                case SpecialKeyType.NumberSymbolsToggle:
                    ToggleSymbolSet();
                    break;
                case SpecialKeyType.Backspace:
                    InputConnection?.OnDelete();
                    break;
                case SpecialKeyType.Enter:
                case SpecialKeyType.Next:
                    InputConnection?.OnDone();
                    break;
                case SpecialKeyType.CapsLock:
                    ToggleCapsLock();
                    break;
                case SpecialKeyType.NextKeyboard:
                    if (InputConnection is IKeyboardInputConnection_ios ios_ic) {
                        ios_ic.OnInputModeSwitched();
                    }
                    break;

                default:
                    InputConnection?.OnText(kvm.PrimaryValue);

                    if (ShiftState == ShiftStateType.Shift) {
                        ShiftState = ShiftStateType.None;
                    }
                    if(kvm.IsSpaceBar && !IsNumbers) {
                        // after typing space reset to default keyboard
                        CharSet = CharSetType.Letters;
                    }
                    break;
            }
            if(kvm.CanRepeat && !isRepeat) {
                RepeatCount = 0;
            }
            //Debug.WriteLine($"Tapped {kvm.CurrentChar}");
        }
        #endregion

        #region Commands
        #endregion
    }
}
