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
    public class KeyboardViewModel : ViewModelBase
    {
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
            .FirstOrDefault(x =>x != null && x.IsPressed);
        public KeyViewModel ActiveKeyViewModel =>
            Keys
            .FirstOrDefault(x => x != null && x.IsActiveKey);

        public IEnumerable<KeyViewModel> PopupKeys =>
            Keys
            .Where(x => x != null && x.IsPopupKey)
            .OrderBy(x => x.PopupKeyIdx);
        IEnumerable<IEnumerable<KeyViewModel>> Rows =>
            Keys.Where(x=>x != null && x.Column.HasValue && x.Row.HasValue)
            .OrderBy(x => x.Column)
            .GroupBy(x => x.Row.Value);
        #endregion

        #region Layout

        public int MaxColCount =>
            Rows.Max(x => x.Count());
        public Size ScreenSize =>
            Parent.ScreenSize;

        private double _defKeyWidth = -1;
        public double DefaultKeyWidth { 
            get
            {
                if(_defKeyWidth <= 0)
                {
                    _defKeyWidth = KeyboardWidth / MaxColCount;

                }
                return _defKeyWidth;
            }
        }
        
        private double _keyHeight = -1;
        public double KeyHeight { 
            get
            {
                if(_keyHeight <= 0)
                {
                    _keyHeight = KeyboardHeight / Rows.Count();
                }
                return _keyHeight;
            }
        }

        double SpecialKeyWidthRatio => 1.5d;
        public double SpecialKeyWidth =>
            DefaultKeyWidth * (IsNumbers ? 1:SpecialKeyWidthRatio);

        public double KeyboardWidth =>
            ScreenSize.Width;
        public double KeyboardHeight =>
            ScreenSize.Height * KeyboardMainViewModel.TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO;

        #endregion

        #region State
        public bool IsNumbers =>
            KeyboardFlags.HasFlag(KeyboardFlags.Numbers);
        KeyboardFlags KeyboardFlags { get; set; }
        bool IsHoldPopupVisible =>
            PopupKeys.Any() &&
            PressedKeyViewModel != null &&
            ActiveKeyViewModel != null &&
            PressedKeyViewModel.PrimaryValue != ActiveKeyViewModel.PrimaryValue;
        uint RepeatCount { get; set; }

        TimeSpan MinHoldDur => TimeSpan.FromMilliseconds(300);
        //TimeSpan MinRepeatDur => TimeSpan.FromMilliseconds((int)(300/(RepeatCount == 0 ? 1:RepeatCount)));
        TimeSpan MinRepeatDur => TimeSpan.FromMilliseconds(300);
        public Point? KeyboardPointerLocation { get; private set; }
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
            Init(KeyboardFlags.Tablet);
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
            var last_active_kvm = ActiveKeyViewModel;
            var new_pressed_kvm = mp.HasValue ? GetKeyUnderPoint(mp.Value) : null;
            KeyboardPointerLocation = mp;

            if (KeyboardPointerLocation.HasValue &&
                (last_pressed_kvm == new_pressed_kvm || IsHoldPopupVisible)) {
                // still over same key

            } else {
                if (last_pressed_kvm == null) {
                    // new press
                    PressKey(new_pressed_kvm);
                    StartHoldTimer();
                } else if (new_pressed_kvm == null) {
                    // release
                    ReleaseKey(last_active_kvm);
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
                    int cur_col = prev_kvm == null ? 0 : prev_kvm.Column.Value + prev_kvm.ColumnSpan;
                    var kvm = CreateKeyViewModel(keys[r][c], r, cur_col,prev_kvm);
                    Keys.Add(kvm);
                    prev_kvm = kvm;
                }
            }
            RefreshKeyboardState();
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
                key.RaisePropertyChanged(nameof(key.PrimaryValue));
                key.RaisePropertyChanged(nameof(key.SecondaryValue));
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

        KeyViewModel CreatePopUpKeyViewModel(KeyViewModel source_kvm, int popup_idx, bool show_self) {
            if(source_kvm == null ||  
                source_kvm.IsSpecial ||
                popup_idx < 0 || 
                popup_idx >= 
                source_kvm.SecondaryCharacters.Count) {
                if(show_self && source_kvm != null && !source_kvm.IsSpecial) {
                    // only show self on input keys
                } else {

                    return null;
                }
            }
            string disp_val = show_self ? source_kvm.PrimaryValue : source_kvm.SecondaryCharacters[popup_idx];
            var pu_kvm = CreateKeyViewModel(disp_val, null, null, PopupKeys.LastOrDefault());
            pu_kvm.PopupKeyIdx = popup_idx;
            return pu_kvm;
        }
        
        KeyViewModel CreateKeyViewModel(object keyObj, int? r, int? c, KeyViewModel prev)
        {
            var kvm = new KeyViewModel(this,prev)
            {
                Row = r,
                Column = c
            };
            if(keyObj is string keyStr && keyStr.Split(',') is { } keyParts)
            {
                // special strings:
                // none
                // comma
                foreach(string kp in keyParts)
                {
                    string text = kp;
                    if(text == "none")
                    {
                        text = null;
                    } else if(text == "comma")
                    {
                        text = ",";
                    }
                    if(text == " ")
                    {
                        kvm.ColumnSpan = 5;
                    }
                    kvm.Characters.Add(text);
                }
            } else if(keyObj is SpecialKeyType skt)
            {
                kvm.SpecialKeyType = skt;
                kvm.Characters.AddRange(GetSpecialKeyChars(skt));

            }
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
        IEnumerable<string> GetSpecialKeyChars(SpecialKeyType skt)
        {
            switch(skt)
            {
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
                            ShowHoldPopup(cur_pressed_kvm);
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
                var sec_kvm = CreatePopUpKeyViewModel(kvm, 0, true);
                Keys.Add(sec_kvm);
            }
            RefreshKeyboardState();
        }
        void ShowHoldPopup(KeyViewModel kvm) {
            ClearHoldKeys();
            if(kvm != null) {
                for (int i = 0; i < kvm.SecondaryCharacters.Count; i++) {
                    var sec_kvm = CreatePopUpKeyViewModel(kvm, i, false);
                    Keys.Add(sec_kvm);
                }
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
