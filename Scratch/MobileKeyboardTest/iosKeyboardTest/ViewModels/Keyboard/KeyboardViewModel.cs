using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Size = Avalonia.Size;

namespace iosKeyboardTest
{
    public class KeyboardViewModel : ViewModelBase
    {
        #region Private Variables        
        #endregion

        #region Constants
        const double KEYBOARD_SCREEN_HEIGHT_RATIO = 0.3;
        #endregion

        #region Statics

        public static MyKeyboardView CreateKeyboardView(IKeyboardInputConnection inputConn, Size screenSize, double scale, out Size unscaledSize) {
            var kbvm = new KeyboardViewModel(inputConn, screenSize);
            var kbv = new MyKeyboardView() {
                DataContext = kbvm,
                Width = kbvm.KeyboardWidth,
                Height = kbvm.KeyboardHeight
            };
            unscaledSize = new Size(kbvm.KeyboardWidth * scale, kbvm.KeyboardHeight * scale);
            return kbv;
        }
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public KeyViewModel HoldingKeyViewModel =>
            Keys.FirstOrDefault(x => x.IsHolding);
        public KeyViewModel HoldingFocusKeyViewModel =>
            Keys.FirstOrDefault(x => x.IsHoldFocusKey);
        public ObservableCollection<KeyViewModel> Keys { get; set; } = [];
        IEnumerable<IEnumerable<KeyViewModel>> Rows =>
            Keys.OrderBy(x => x.Column).GroupBy(x => x.Row);
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

        private double _defKeyWidth = -1;
        public double DefaultKeyWidth { 
            get
            {
                if(_defKeyWidth <= 0)
                {
                    _defKeyWidth = KeyboardWidth / Rows.Max(x => x.Count());

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
            DefaultKeyWidth * SpecialKeyWidthRatio;

        public double KeyboardWidth =>
            ScreenSize.Width;
        public double KeyboardHeight =>
            ScreenSize.Height * KEYBOARD_SCREEN_HEIGHT_RATIO;

        #endregion

        #region State
        public CharSetType CharSet { get; set; }
        public ShiftStateType ShiftState { get; set; }
        IKeyboardInputConnection InputConnection { get; set; }
        #endregion
        #endregion

        #region Constructors
        public KeyboardViewModel() : this(null,default) { }
        private KeyboardViewModel(IKeyboardInputConnection inputConn, Size screenSize) 
        {
            Console.WriteLine("kbvm ctor called");
            _screenSize = screenSize;
            SetInputConnection(inputConn);
            Init();
        }
        #endregion

        #region Public Methods
        public void SetInputConnection(IKeyboardInputConnection conn) {
            InputConnection = conn;
        }
        public void RefreshKeyboardState() {
            this.RaisePropertyChanged(nameof(Keys));

            foreach (var row in Rows) {
                // center middle row for non-symbol char set
                bool needs_trans = row.Any(x => !x.IsVisible);
                foreach (var key in row) {
                    key.NeedsOuterTranslate = needs_trans;
                }
            }

            foreach (var key in Keys) {
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
                key.RaisePropertyChanged(nameof(key.IsHoldFocusKey));
                Console.Write(key.PrimaryValue);
            }
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void Init()
        {
            var keys = GetKeys(KeyboardFlags.Phone);
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

        private List<List<object>> GetKeys(KeyboardFlags kbFlags) {
            List<List<object>> keys = null;
            if(kbFlags.HasFlag(KeyboardFlags.Phone)) {
                keys = new List<List<object>>
                {
                    (["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"]),
                    (["q,+,`", "w,×,~", "e,÷,\\", "r,=,|", "t,/,{", "y,_,}", "u,<,€", "i,>,£", "o,[,¥", "p,],₩"]),
                    (["a,!,○", "s,@,•", "d,#,⚪", "f,$,⚫", "g,%,□", "h,^,🔳", "j,&,♤", "k,*,♡", "l,(,♢", "none,),♧"]),
                    ([SpecialKeyType.Shift, "z,-,☆", "x,',▪", "c,\",▫", "v,:,≪", "b,;,≫", "n,comma,¡", "m,?,¿", SpecialKeyType.Backspace]),
                    ([SpecialKeyType.SymbolToggle, "comma", " ", ".", SpecialKeyType.Enter])
                };
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

        private KeyViewModel CreateKeyViewModel(object keyObj, int r, int c, KeyViewModel prev)
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

        private IEnumerable<string> GetSpecialKeyChars(SpecialKeyType skt)
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
                case SpecialKeyType.Backspace:
                    yield return "⌫";
                    break;
                case SpecialKeyType.SymbolToggle:
                    yield return "!#1";
                    yield return "ABC";
                    break;
                case SpecialKeyType.Enter:
                    yield return "⏎";
                    break;

            }
        }

        void ToggleSymbolSet() {
            if (CharSet == CharSetType.Letters) {
                CharSet = CharSetType.Symbols1;
            } else {
                CharSet = CharSetType.Letters;
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

        public void ClearHoldKeys() {
            var to_remove = Keys.Where(x => x.IsHoldKey).ToList();
            foreach (var rmv in to_remove) {
                Keys.Remove(rmv);
            }
            
            var to_clear = Keys.Where(x => x.IsHolding).ToList();
            foreach (var rmv in to_clear) {
                rmv.IsHolding = false;
            }
            RefreshKeyboardState();
        }
        #endregion

        #region Commands
        public ICommand KeyTapCommand => ReactiveCommand.Create<object>((args) => {
            bool was_hold = false;
            if (args is not MyKeyView c ||
                c.DataContext is not KeyViewModel kvm) {
                if(args is KeyViewModel hold_kvm) {
                    // triggered hold key tap
                    kvm = hold_kvm;
                    was_hold = true;
                } else {
                    return;
                }
            }
            switch (kvm.SpecialKeyType) {
                case SpecialKeyType.Shift:
                    HandleShift();
                    break;
                case SpecialKeyType.SymbolToggle:
                    ToggleSymbolSet();
                    break;
                case SpecialKeyType.Backspace:
                    InputConnection?.OnDelete();
                    break;
                case SpecialKeyType.Enter:
                    InputConnection?.OnDone();
                    break;
                case SpecialKeyType.NextKeyboard:
                    if (InputConnection is iosIKeyboardInputConnection ios_ic) {
                        ios_ic.OnInputModeSwitched();
                    }
                    break;
                default:
                    InputConnection?.OnText(kvm.PrimaryValue);
                    break;
            }
            try {
                if(was_hold) {
                    ClearHoldKeys();
                } else {
                    RefreshKeyboardState();
                }
                
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine($"Tapped {kvm.CurrentChar}");
        });
        public ICommand KeyHoldCommand => ReactiveCommand.Create<object>((args) => {

            if (args is not Control b || 
                b.GetVisualAncestors().OfType<MyKeyboardView>().FirstOrDefault() is not { } kbv ||
                b.DataContext is not KeyViewModel kvm ||
                !kvm.SecondaryCharacters.Any()) {
                return;
            }
            void OnRelease(object seder, EventArgs e) {
                ClearHoldKeys();
                kbv.RemoveHandler(Control.PointerReleasedEvent, OnRelease);
            }
            ClearHoldKeys();
            kbv.AddHandler(Control.PointerReleasedEvent, OnRelease, RoutingStrategies.Tunnel);
            kvm.IsHolding = true;
            double cur_x = kvm.X;
            double cur_y = kvm.Y - kvm.Height;
            foreach(var sec_char in kvm.SecondaryCharacters) {
                var sec_kvm = CreateKeyViewModel(sec_char, 0, 0, null);
                sec_kvm.IsHoldKey = true;
                sec_kvm.NeedsOuterTranslate = kvm.NeedsOuterTranslate;
                sec_kvm.X = cur_x;
                sec_kvm.Y = cur_y;
                Keys.Add(sec_kvm);
                cur_x += sec_kvm.Width;
                if(HoldingFocusKeyViewModel == null) {
                    sec_kvm.IsHoldFocusKey = true;
                }
            }
            RefreshKeyboardState();
            this.RaisePropertyChanged(nameof(Keys));
            
            Console.WriteLine($"Hold {kvm.CurrentChar}");
        });


        #endregion
    }
}
