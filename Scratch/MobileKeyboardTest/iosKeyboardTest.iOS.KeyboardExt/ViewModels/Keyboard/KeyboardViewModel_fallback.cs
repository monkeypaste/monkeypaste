using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using Size = Avalonia.Size;

namespace iosKeyboardTest.iOS.KeyboardExt {
    public class KeyboardViewModel_fallback : ViewModelBase_fallback {
        #region Private Variables        
        #endregion

        #region Constants
        const double TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO = 0.3;
        #endregion

        #region Statics
        
        public static Size GetTotalSizeByScreenSize(Size scaledScreenSize) {
            return new Size(scaledScreenSize.Width, scaledScreenSize.Height * TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO);
        }
        public static iosKeyboardTest.KeyboardView CreateKeyboardView(IKeyboardInputConnection_fallback inputConn, Size scaledSize, double scale, out Size unscaledSize) {
            KeyboardViewModel_fallback kbvm = new KeyboardViewModel_fallback(inputConn, scaledSize, scale);
            var kbv = new iosKeyboardTest.KeyboardView() {
                DataContext = kbvm,
                [!Control.WidthProperty] = new Binding { Source = kbvm, Path = nameof(kbvm.TotalWidth)}, 
                [!Control.HeightProperty] = new Binding { Source = kbvm, Path = nameof(kbvm.TotalHeight)}, 
            };
            DateTime press_time = default;
            kbv.PointerPressed += (s, e) => {
                if(kbvm == null) {
                    kbvm = new KeyboardViewModel_fallback(inputConn, scaledSize, scale);
                    kbv.DataContext = kbvm;
                    kbvm.UpdateKeyboardState();                    
                }
                int mb = (int)(GC.GetTotalMemory(false) / (1024*1024));
                Debug.WriteLine($"Mem: {mb}mb");
                press_time = DateTime.Now;
                kbvm.SetPointerLocation(new TouchEventArgs_fallback(e.GetPosition(kbv), TouchEventType_fallback.Press));
            };
            kbv.PointerMoved += (s, e) => {
                if(OperatingSystem.IsWindows() && !e.GetCurrentPoint(kbv).Properties.IsLeftButtonPressed) {
                    return;
                }
                kbvm.SetPointerLocation(new TouchEventArgs_fallback(e.GetPosition(kbv), TouchEventType_fallback.Move));
            };
            kbv.PointerReleased += (s, e) => {
                Debug.WriteLine($"Actual Touch time: {(DateTime.Now - press_time).Milliseconds}ms");
                kbvm.SetPointerLocation(new TouchEventArgs_fallback(e.GetPosition(kbv), TouchEventType_fallback.Release));
            };
                
            
            unscaledSize = kbvm == null ? new(scaledSize.Width * scale, scaledSize.Height*scale) : new Size(kbvm.TotalWidth * scale, kbvm.TotalHeight * scale);
            return kbv;
        }
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        public string Test { get; set; } = "Im a test";

        #region View Models
        public ObservableCollection<KeyViewModel_fallback> Keys { get; set; } = [];
        public KeyViewModel_fallback[] PopupKeys { get; private set; }
        public List<KeyViewModel_fallback> VisiblePopupKeys { get; private set; } = [];
        public List<KeyViewModel_fallback> PressedKeys { get; set; } = [];
        
        public KeyViewModel_fallback SpacebarKey { get; private set; }
        public KeyViewModel_fallback BackspaceKey { get; private set; }
        public KeyViewModel_fallback PeriodKey { get; private set; }

        #endregion

        #region Layout
        public Thickness KeyboardMargin { get; set; } = new Thickness(0, 10);
        public Rect TotalRect { get; private set; } = new();
        public Rect KeyboardRect { get; private set; } = new();
        public int MaxColCount { get; private set; }
        public int RowCount { get; private set; }

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
                if(_keyHeight >= 0) {
                    return _keyHeight;
                }

                if (_keyHeight <= 0) {
                    // avoid div by 0
                    _keyHeight = KeyboardHeight / RowCount;
                }
                return _keyHeight;
            }
        }
        public int MaxPopupColCount =>
            4;
        public int MaxPopupRowCount { get; private set; }
        double SpecialKeyWidthRatio => 1.5d;
        public double SpecialKeyWidth =>
            DefaultKeyWidth * (IsNumbers ? 1 : SpecialKeyWidthRatio);

        double MenuHeightPad => 5;
        public double MenuHeight =>
            KeyHeight + MenuHeightPad;
        public double KeyboardWidth { get; private set; }

        public double KeyboardHeight { get; private set; }
        public double TotalWidth =>
            KeyboardWidth;
        public double TotalHeight =>
            KeyboardHeight +
            (NeedsNextKeyboardButton ? MenuHeight : 0) +
            MenuHeight + KeyboardMargin.Top + KeyboardMargin.Bottom;

        #endregion

        #region Appearance
        public double CursorControlOpacity =>
            IsCursorControlEnabled ? 1 : 0;
        public bool IsSecondaryVisible =>
            IsNumbers || CharSet == CharSetType_fallback.Letters;
        #endregion

        #region State

        string[] eos_chars = ["\n", ".", "!", "?", string.Empty];
        bool IsInitialized { get; set; }

        public KeyboardFeedbackFlags_fallback ActiveChangeFeedback =>
            KeyboardFeedbackFlags_fallback.Vibrate;
        public KeyboardFeedbackFlags_fallback CursorChangeFeedback =>
            KeyboardFeedbackFlags_fallback.Vibrate;
        public KeyboardFeedbackFlags_fallback KeyReleaseFeedback =>
            KeyboardFeedbackFlags_fallback.Vibrate;// | KeyboardFeedbackFlags.Click;
        public KeyboardFeedbackFlags_fallback ReturnReleaseFeedback =>
            KeyboardFeedbackFlags_fallback.Vibrate;// | KeyboardFeedbackFlags.Return;
        string LastInput { get; set; } = string.Empty;
        public bool IsPullEnabled =>
            IsTablet;
        public bool IsSlideEnabled =>
            IsMobile;
        bool IsHeadlessMode =>
            InputConnection is IKeyboardInputConnection_ios_fallback;
        public double ScreenScaling { get; set; }
        public string ErrorText { get; private set; } = "NO ERRORS";
        public bool NeedsNextKeyboardButton =>
            //OperatingSystem.IsWindows() ||
            (OperatingSystem.IsIOS() &&
            InputConnection != null &&
            (InputConnection as IKeyboardInputConnection_ios_fallback).NeedsInputModeSwitchKey);
        double CursorControlFactorX => ScreenScaling * 2;
        double CursorControlFactorY => ScreenScaling * 2;
        bool IsAnyPopupMenuVisible =>
            VisiblePopupKeys.Any();
        bool IsHoldMenuVisible =>
            VisiblePopupKeys.Skip(1).Any();

        int HoldDelayMs { get; set; } = DefHoldDelayMs;
        int MinHoldMs => 500; // NOTE! must be a factor of HoldDelayMs
        static int DefHoldToRepeatBackspaceMs => 200; // NOTE! must be a factor of HoldDelayMs        
        static int DefHoldDelayMs => 5; // NOTE! must be a factor of HoldDelayMs
        double MinCursorControlDragDist => 10;
        int MinBackspaceRepeatMs => 5;
        int BackspaceHoldToRepeatMs { get; set; } = 100;// DefHoldToRepeatBackspaceMs; // NOTE! must be a factor of HoldDelayMs
        int MaxDoubleTapSpaceForPeriodMs => 150;
        private CharSetType_fallback _charSet;
        public CharSetType_fallback CharSet => _charSet;
        public int CharSetIdx {
            get {
                switch(CharSet) {
                    case CharSetType_fallback.Letters:
                        return 0;
                    case CharSetType_fallback.Symbols1:
                        return 1;
                    case CharSetType_fallback.Symbols2:
                        return 2;
                    case CharSetType_fallback.Numbers1:
                        return 0;
                    case CharSetType_fallback.Numbers2:
                        return 1;
                }

                return 0;
            }
        }
        private ShiftStateType_fallback _shiftState;
        public ShiftStateType_fallback ShiftState => _shiftState;
        public IKeyboardInputConnection_fallback InputConnection { get; set; }
        ITriggerTouchEvents_fallback HeadlessRender =>
            InputConnection as ITriggerTouchEvents_fallback;
        public bool IsCursorControlEnabled { get; private set; }
        Point? LastCursorControlUpdateLocation { get; set; }
        
        public bool IsAutoCapitalizationEnabled =>
            IsFreeText;
        #endregion

        #region Model
        public bool IsFreeText { get; private set; }
        public bool IsNumbers { get; private set; }
        public bool IsThemeDark { get; private set; }
        public bool IsTablet { get; private set; }
        public bool IsMobile { get; private set; }       

        KeyboardFlags_fallback KeyboardFlags { get; set; }
        #endregion

        #endregion

        #region Constructors
        public KeyboardViewModel_fallback() : this(null,new Size(360,740),2.75) { }
        public KeyboardViewModel_fallback(IKeyboardInputConnection_fallback inputConn, Size scaledSize, double scale) 
        {
            Debug.WriteLine("kbvm ctor called");
            ScreenScaling = scale;
            SetInputConnection(inputConn);
            if(inputConn is { } ic) {
                Init(ic.Flags);
            } else {
                Init(KeyboardFlags_fallback.Mobile | KeyboardFlags_fallback.FreeText);
            }
            
            SetDesiredSize(scaledSize);
        }

        #endregion

        #region Public Methods
        public void SetInputConnection(IKeyboardInputConnection_fallback conn) {
            void OnPointerChanged(object s, TouchEventArgs_fallback e) {
                SetPointerLocation(e);
            }

            if(InputConnection is { } old_conn){
                old_conn.OnCursorChanged -= Ic_OnCursorChanged;
                old_conn.OnFlagsChanged -= Ic_OnFlagsChanged;
                old_conn.OnDismissed -= InputConnection_OnDismissed;
                if (old_conn is ITriggerTouchEvents_fallback tte) {
                    tte.OnPointerChanged -= OnPointerChanged;
                }
            }
            if(conn is IKeyboardInputConnection_fallback new_conn) {
                InputConnection = new_conn;
                new_conn.OnCursorChanged += Ic_OnCursorChanged;
                new_conn.OnFlagsChanged += Ic_OnFlagsChanged;
                new_conn.OnDismissed += InputConnection_OnDismissed;
                if (new_conn is ITriggerTouchEvents_fallback tte) {
                    tte.OnPointerChanged += OnPointerChanged;
                }
            } else {
                InputConnection = null;
            }

            
        }

        private void InputConnection_OnDismissed(object sender, EventArgs e) {
            ResetState();
        }

        public void ResetLayout() {
            _defKeyWidth = -1;
            _keyHeight = -1;
            this.RaisePropertyChanged(nameof(Keys));
            this.RaisePropertyChanged(nameof(TotalWidth));
            this.RaisePropertyChanged(nameof(TotalHeight));
            this.RaisePropertyChanged(nameof(KeyboardWidth));
            this.RaisePropertyChanged(nameof(KeyboardHeight));

            foreach(var kvm in Keys) {
                kvm.ResetLocation();
                kvm.ResetDisplayValues();
                kvm.ResetSize();
            }
        }
        void ResetState() {
            _lastDebouncedTouchEventArgs = null;
            LastInput = string.Empty;
            IsCursorControlEnabled = false;
            foreach(var pkvm in PressedKeys.ToList()) {
                pkvm.SetPressed(false,null);
            }
            Touches_fallback.Clear();
        }

        private TouchEventArgs_fallback _lastDebouncedTouchEventArgs;

        bool IsTouchBounced(TouchEventArgs_fallback e) {
            if(e == null) {
                return true;
            }
            if(_lastDebouncedTouchEventArgs == null) {
                return false;
            }
            if(_lastDebouncedTouchEventArgs.TouchEventType != e.TouchEventType) {
                return false;
            }
            double dist = Touches_fallback.Dist(e.Location, _lastDebouncedTouchEventArgs.Location);
            return dist < 5;
        }

        public void SetPointerLocation(TouchEventArgs_fallback e) {
            var mp = e.Location;
            var touchType = e.TouchEventType;
            if(Touches_fallback.Update(mp,touchType) is not { } touch) {
                return;
            }

            if (IsTouchBounced(e)) {
                return;
            }
            _lastDebouncedTouchEventArgs = e;


            string key_text = "NONE";
            if (GetKeyUnderPoint(touch.Location) is { } kvm) {
                key_text = kvm.ToString();
            }
            Debug.WriteLine($"Event: '{touchType}' Id: {touch.Id} Key: {key_text}");

            var pressed_kvm = GetPressedKeyForTouch(touch);
            switch(touchType) {
                case TouchEventType_fallback.Press:
                    PressKeyAsync(touch).FireAndForgetSafeAsync();
                    break;
                case TouchEventType_fallback.Move:
                    MoveKey(touch);
                    break;
                case TouchEventType_fallback.Release:
                    ReleaseKey(touch);
                    break;
            }
            //UpdateKeyboardState();
        }
        KeyViewModel_fallback GetPressedKeyForTouch(Touch_fallback touch) {
            return PressedKeys
                        .FirstOrDefault(x => x.TouchId == touch.Id);
        }
        public void SetError(string msg) {
            //Dispatcher.UIThread.Post(() => {
                ErrorText = msg;
                this.RaisePropertyChanged(nameof(ErrorText));
            //});
        }

        public void SetDesiredSize(Size scaledSize) {
            double w = scaledSize.Width;
            double h = scaledSize.Height;
            if(IsHeadlessMode) {
                //w *= ScreenScaling;
                //h *= ScreenScaling;
            }
            KeyboardWidth = w;
            KeyboardHeight = h;
            KeyboardRect = new Rect(0, MenuHeight, KeyboardWidth, KeyboardHeight);
            TotalRect = new Rect(0, 0, TotalWidth, TotalHeight);
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        void Init(KeyboardFlags_fallback flags)
        {
            SetFlags(flags);

            if (IsNumbers) {
                SetCharSet(CharSetType_fallback.Numbers1);
            } else {
                SetCharSet(CharSetType_fallback.Letters);
            }
            SetShiftByLeadingText();

            var keyRows = GetKeyRows(KeyboardFlags);
            Keys.Clear();
            MaxColCount = 0;
            RowCount = keyRows.Count;
            for(int r = 0; r < keyRows.Count; r++)
            {
                MaxColCount = Math.Max(MaxColCount, keyRows[r].Count);
                KeyViewModel_fallback prev_kvm = null;
                for(int c = 0; c < keyRows[r].Count; c++)
                {
                    var keyObj = keyRows[r][c];
                    int cur_col = prev_kvm == null ? 0 : prev_kvm.Column + prev_kvm.ColumnSpan;
                    var kvm = CreateKeyViewModel(keyRows[r][c], r, cur_col,prev_kvm);
                    Keys.Add(kvm);
                    prev_kvm = kvm;
                    if(kvm.IsSpaceBar) {
                        SpacebarKey = kvm;
                    } else if(kvm.IsPeriod) {
                        PeriodKey = kvm;
                    } else if(kvm.IsBackspace) {
                        BackspaceKey = kvm;
                    }
                }
            }
            int max_popup_keys = Keys.Max(x => x.GetSecondaryCharacters().Count());
            MaxPopupRowCount = (int)Math.Floor((double)max_popup_keys / (double)(MaxPopupColCount));

            for (int i = 0; i < 2; i++) {
                // create 2x Max possible popup keys for multi touch
                KeyViewModel_fallback prev_pukvm = null;
                int idx = 0;
                for (int r = 0; r < MaxPopupRowCount; r++) {
                    for (int c = 0; c < MaxPopupColCount; c++) {
                        var pukvm = CreatePopUpKeyViewModel(idx++, r, c, prev_pukvm);
                        Keys.Add(pukvm);
                        prev_pukvm = pukvm;
                    }
                }
            }
            PopupKeys = Keys.Where(x => x.IsPopupKey).ToArray();


            UpdateKeyboardState();
            IsInitialized = true;
        }

        private void Ic_OnFlagsChanged(object sender, EventArgs e) {
            if(!IsInitialized ||
                InputConnection is not { } ic) {
                // ignore flag change before initialized so doesn't double init
                return;
            }
            Init(ic.Flags);
        }

        private void Ic_OnCursorChanged(object sender, EventArgs e) {
            Dispatcher.UIThread.Post(SetShiftByLeadingText);
            //SetShiftByLeadingText();
        }

        void SetShiftByLeadingText() {
            if(!IsAutoCapitalizationEnabled ||
                InputConnection is not { } ic ||
                ShiftState == ShiftStateType_fallback.ShiftLock) {
                return;
            }
            bool needs_shift = false;
            if(ic.GetLeadingText(-1,-1) is { } leading_text) {
                for (int i = 0; i < leading_text.Length; i++) {
                    string cur_char = leading_text[leading_text.Length - i - 1].ToString();
                    if(cur_char == " ") {
                        continue;
                    }
                    int eos_idx = eos_chars.IndexOf(cur_char);
                    // only allow shift if prev char is newline or
                    // space(s) after any end-of-sentence char 
                    needs_shift = 
                        eos_idx >= 0 && 
                        ((i == 0 && eos_idx == 0) || i > 0);
                    break;
                }
            }
            SetShiftState(needs_shift ? ShiftStateType_fallback.Shift : ShiftStateType_fallback.None);
        }
        void SetFlags(KeyboardFlags_fallback kbFlags) {
            KeyboardFlags = kbFlags;
            IsThemeDark = KeyboardFlags.HasFlag(KeyboardFlags_fallback.Dark);
            IsNumbers = KeyboardFlags.HasFlag(KeyboardFlags_fallback.Numbers);
            IsFreeText = KeyboardFlags.HasFlag(KeyboardFlags_fallback.FreeText);
            IsMobile = KeyboardFlags.HasFlag(KeyboardFlags_fallback.Mobile);
            IsTablet = KeyboardFlags.HasFlag(KeyboardFlags_fallback.Tablet);            
        }

        KeyViewModel_fallback GetKeyUnderPoint(Point scaledPoint) {
            var p = scaledPoint;
            var result = Keys
                .Where(x => x != null && !x.IsPopupKey)
                .FirstOrDefault(x => x.TotalRect.Contains(p));
            return result;
        }
        public void UpdateKeyboardState() {
            if(InputConnection is IKeyboardRenderer kr) {
                kr.Render();
                return;
            }
            this.RaisePropertyChanged(nameof(Keys));
            this.RaisePropertyChanged(nameof(KeyboardWidth));
            this.RaisePropertyChanged(nameof(KeyboardHeight));
            this.RaisePropertyChanged(nameof(TotalWidth));
            this.RaisePropertyChanged(nameof(TotalHeight));
            this.RaisePropertyChanged(nameof(MenuHeight));
            this.RaisePropertyChanged(nameof(CursorControlOpacity));
            this.RaisePropertyChanged(nameof(NeedsNextKeyboardButton));


            foreach (var key in Keys) {
                if(key == null) {
                    continue;
                }
                if(IsPullEnabled) {
                    key.RaisePropertyChanged(nameof(key.SecondaryOpacity));
                    key.RaisePropertyChanged(nameof(key.PrimaryOpacity));
                    key.RaisePropertyChanged(nameof(key.SecondaryTranslateOffsetX));
                    key.RaisePropertyChanged(nameof(key.SecondaryTranslateOffsetY));
                    key.RaisePropertyChanged(nameof(key.PullTranslateY));
                }

                key.RaisePropertyChanged(nameof(key.ZIndex));
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
                key.RaisePropertyChanged(nameof(key.KeyOpacity));
                key.RaisePropertyChanged(nameof(key.OuterTranslateX));
                key.RaisePropertyChanged(nameof(key.NeedsSymbolTranslate));
                key.RaisePropertyChanged(nameof(key.IsActiveKey));
                key.RaisePropertyChanged(nameof(key.IsPressed));
                key.RaisePropertyChanged(nameof(key.IsSpecial));
                key.RaisePropertyChanged(nameof(key.CornerRadius));
                key.RaisePropertyChanged(nameof(key.NeedsOuterTranslate));
                if(key.NeedsOuterTranslate) {

                }
                //Debug.WriteLine(key.PrimaryValue);
            }
#if DEBUG
            if(KeyboardGridView.DebugCanvas != null) {
                KeyboardGridView.DebugCanvas.InvalidateVisual();
            }
#endif
        }

        KeyViewModel_fallback CreatePopUpKeyViewModel(int idx, int r, int c, KeyViewModel_fallback prev) {
            var pu_kvm = CreateKeyViewModel(idx, r, c, prev);
            return pu_kvm;
        }
        
        KeyViewModel_fallback CreateKeyViewModel(object keyObj, int r, int c, KeyViewModel_fallback prev)
        {
            var kvm = new KeyViewModel_fallback(this,prev, keyObj)
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
        SpecialKeyType_fallback GetPrimarySpecialKey(KeyboardFlags_fallback kbFlags) {
            if(kbFlags.HasFlag(KeyboardFlags_fallback.FreeText)) {
                return SpecialKeyType_fallback.Enter;
            }
            if(kbFlags.HasFlag(KeyboardFlags_fallback.Numbers) ||
                kbFlags.HasFlag(KeyboardFlags_fallback.Email)) {
                return SpecialKeyType_fallback.Next;
            }
            
            if(kbFlags.HasFlag(KeyboardFlags_fallback.Search)) {
                return SpecialKeyType_fallback.Search;
            }
            if (kbFlags.HasFlag(KeyboardFlags_fallback.Url)) {
                return SpecialKeyType_fallback.Go;
            }

            return SpecialKeyType_fallback.Done;
        }
        List<List<object>> GetKeyRows(KeyboardFlags_fallback kbFlags) {
            List<List<object>> keys = null;
            SpecialKeyType_fallback primarySpecialType = GetPrimarySpecialKey(kbFlags);
            if (kbFlags.HasFlag(KeyboardFlags_fallback.Mobile)) {
                if (kbFlags.HasFlag(KeyboardFlags_fallback.Numbers)) {
                    keys = new List<List<object>>
                {
                    (["1,(", "2,/", "3,)", SpecialKeyType_fallback.Backspace]),
                    (["4,N", "5,comma", "6,.", primarySpecialType]),
                    (["7,*", "8,;", "9,#", SpecialKeyType_fallback.NumberSymbolsToggle]),
                    (["*,-", "0,+", "#,__", "comma"])
                };
                } else {
                    keys = new List<List<object>>
                {
                    (["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"]),
                    (["q,+,`", "w,×,~", "e,÷,\\", "r,=,|", "t,/,{", "y,_,}", "u,<,€", "i,>,£", "o,[,¥", "p,],₩"]),
                    (["a,!,○", "s,@,•", "d,#,⚪", "f,$,⚫", "g,%,□", "h,^,🔳", "j,&,♤", "k,*,♡", "l,(,♢", "none,),♧"]),
                    ([SpecialKeyType_fallback.Shift, "z,-,☆", "x,',▪", "c,\",▫", "v,:,≪", "b,;,≫", "n,comma,¡", "m,?,¿", SpecialKeyType_fallback.Backspace]),
                    ([SpecialKeyType_fallback.SymbolToggle, "comma", " ", ".", primarySpecialType])
                };
                }

            } else {
                keys = new List<List<object>>
                {
                    (["1", "2", "3", "4", "5", "6", "7", "8", "9", "0", SpecialKeyType_fallback.Backspace]),
                    ([SpecialKeyType_fallback.Tab, "q,+,`", "w,×,~", "e,÷,\\", "r,=,|", "t,/,{", "y,_,}", "u,<,€", "i,>,£", "o,[,¥", "p,],₩"]),
                    ([SpecialKeyType_fallback.CapsLock, "a,!,○", "s,@,•", "d,#,⚪", "f,$,⚫", "g,%,□", "h,^,🔳", "j,&,♤", "k,*,♡", "l,(,♢", "none,),♧", primarySpecialType]),
                    ([SpecialKeyType_fallback.Shift, "z,-,☆", "x,',▪", "c,\",▫", "v,:,≪", "b,;,≫", "n,comma,¡", "m,?,¿", "comma",".", SpecialKeyType_fallback.Shift]),
                    ([SpecialKeyType_fallback.SymbolToggle,SpecialKeyType_fallback.Emoji, " ", SpecialKeyType_fallback.ArrowLeft, SpecialKeyType_fallback.ArrowRight, SpecialKeyType_fallback.NextKeyboard])
                };
            }
            return keys;
        }
        
        void ToggleSymbolSet() {
            if(CharSet == CharSetType_fallback.Numbers1) {
                SetCharSet(CharSetType_fallback.Numbers2);
                return;
            } 
            if(CharSet == CharSetType_fallback.Numbers2) {
                SetCharSet(CharSetType_fallback.Numbers1);
                return;
            }

            if (CharSet == CharSetType_fallback.Letters) {
                SetCharSet(CharSetType_fallback.Symbols1);
                return;
            }
            SetCharSet(CharSetType_fallback.Letters);
        }
        void ToggleCapsLock() {
            if(ShiftState == ShiftStateType_fallback.ShiftLock) {
                SetShiftState(ShiftStateType_fallback.None);
            } else {
                SetShiftState(ShiftStateType_fallback.ShiftLock);
            }
        }
        void SetShiftState(ShiftStateType_fallback sst) {
            _shiftState = sst;
            foreach (var kvm in Keys) {
                kvm.UpdateCharacters();
                kvm.ResetDisplayValues();
                kvm.ResetLocation();
                kvm.RaisePropertyChanged(nameof(kvm.KeyOpacity));
            }
        }
        void SetCharSet(CharSetType_fallback cst) {
            _charSet = cst;
            foreach(var kvm in Keys) {
                kvm.UpdateCharacters();
                kvm.ResetDisplayValues();
                kvm.ResetLocation();
                kvm.RaisePropertyChanged(nameof(kvm.KeyOpacity));
            }
        }

        void HandleShift() {
            if (CharSet == CharSetType_fallback.Letters) {
                if (ShiftState == ShiftStateType_fallback.ShiftLock) {
                    SetShiftState(ShiftStateType_fallback.None);
                } else {
                    SetShiftState((ShiftStateType_fallback)((int)ShiftState + 1));
                }
            } else {
                if (CharSet == CharSetType_fallback.Symbols1) {
                    SetCharSet(CharSetType_fallback.Symbols2);
                } else {
                    SetCharSet(CharSetType_fallback.Symbols1);
                }
            }
        }

        void ShowPressPopup(KeyViewModel_fallback kvm, Touch_fallback touch) {
            kvm.ClearPopups();
            kvm.AddPopupAnchor(0, 0, kvm.CurrentChar);
            kvm.PressPopupShowDt = DateTime.Now;
            kvm.FitPopupInFrame(touch);
            //UpdateKeyboardState();
        }
        void ShowHoldPopup(KeyViewModel_fallback kvm, Touch_fallback touch) {
            if(IsHoldMenuVisible && kvm != null && !kvm.HasHoldPopup) {
                return;
            }
            kvm.ClearPopups();

            if(kvm == null ||
                !kvm.HasHoldPopup) {
                return;
            }
            var chars = kvm.SecondaryCharacters.ToList();
            if(kvm.IsRightSideKey) {
                chars.Reverse();
            }
            int idx = 0;
            for (int r = 0; r < MaxPopupRowCount; r++) {
                for (int c = 0; c < MaxPopupColCount; c++) {
                    string pv = string.Empty;
                    if (idx < chars.Count) {
                        // visible popup
                        pv = chars[idx];
                    } else if (r == 0) {
                        break;
                    }
                    kvm.AddPopupAnchor(r, c, pv);
                    idx++;
                }
                if (idx >= chars.Count) {
                    break;
                }
            }
            kvm.FitPopupInFrame(touch);
        }

        #region Cursor Control
        void StartCursorControl(string touchId) {
            if(Touches_fallback.Locate(touchId) is not { } touch) {
                return;
            }
            IsCursorControlEnabled = true;
            LastCursorControlUpdateLocation = touch.Location;
            this.RaisePropertyChanged(nameof(CursorControlOpacity));
        }
        void StopCursorControl(Touch_fallback touch) {
            IsCursorControlEnabled = false;
            LastCursorControlUpdateLocation = null;
            if(SpacebarKey is { } sb_kvm) {
                sb_kvm.SetPressed(false,touch);
            }
            this.RaisePropertyChanged(nameof(CursorControlOpacity));
        }
        void UpdateCursorControl(Touch_fallback touch) {
            double x = touch.Location.X;
            double y = touch.Location.Y;
            var lp = LastCursorControlUpdateLocation ?? touch.Location;
            double lx = lp.X;
            double ly = lp.Y;
            //double lx = LastCursorControlUpdateLocationX ?? x;
            //double ly = LastCursorControlUpdateLocationY ?? y;

            int dx = (int)Math.Floor((x - lx) / (ScreenScaling * 2));
            int dy = (int)Math.Floor((y - ly) / (ScreenScaling * 3));

            if (dx == 0 && dy == 0) {
                return;
            }
            Debug.WriteLine($"DX: {dx} DY: {dy}");

            InputConnection.OnNavigate(dx, dy); 
            InputConnection.OnFeedback(CursorChangeFeedback);

            // ensure when updated last pos is actual position
            LastCursorControlUpdateLocation = touch.Location;
        }
        #endregion

        #region Key Pull
        public void UpdatePull(Touch_fallback touch) {
            var pkvm = GetPressedKeyForTouch(touch);
            if(pkvm == null ||
                !pkvm.CanPullKey) {
                return;
            }
            if(!pkvm.KeyboardRect.Contains(touch.Location)) {
                // reset pull
                //pkvm.PullTranslateY = 0;
                return;
            }
            double y_diff = touch.Location.Y - touch.PressLocation.Y;
            pkvm.PullTranslateY = Math.Clamp(y_diff, 0, pkvm.MaxPullTranslateY);
            //UpdateKeyboardState();
            //Debug.WriteLine($"Pull: {y_diff}");
        }
        #endregion

        void ResetBackspace() {
            //BackspaceHoldToRepeatMs = DefHoldToRepeatBackspaceMs;
        }

        void HandleBackspace() {            
            InputConnection.OnBackspace(1);
            if(!string.IsNullOrEmpty(InputConnection.GetLeadingText(1,1))) {
                // only vibrate if there's text to delete
                InputConnection.OnFeedback(KeyboardFeedbackFlags_fallback.Vibrate);
            }
            
            //BackspaceHoldToRepeatMs = Math.Max(MinBackspaceRepeatMs, BackspaceHoldToRepeatMs - 60);
        }
        async Task PressKeyAsync(Touch_fallback touch) {
            if(GetKeyUnderPoint(touch.Location) is not { } kvm) {
                return;
            }
            InputConnection.OnFeedback(KeyReleaseFeedback);
            kvm.SetPressed(true,touch);
            int t = 0;
            while (true) {
                if (t == 0 && kvm.HasPressPopup) {
                    ShowPressPopup(kvm,touch);

                } else if (t == MinHoldMs) {
                    if (kvm.HasHoldPopup) {
                        ShowHoldPopup(kvm,touch);
                        return;
                    } else if (kvm.IsSpaceBar) {
                        StartCursorControl(touch.Id);
                        return;
                    }
                }
                if(kvm.IsSpaceBar && 
                    Touches_fallback.Dist(touch.Location,touch.PressLocation) >= MinCursorControlDragDist) {
                    StartCursorControl(touch.Id);
                    return;
                }
                if (kvm.IsBackspace && 
                    (t == 0 || (t >= BackspaceHoldToRepeatMs && t % MinBackspaceRepeatMs == 0))) {
                    HandleBackspace();
                    Debug.WriteLine($"Backspace: MinRepeat: {BackspaceHoldToRepeatMs} T: {t}");
                }
                int dt = HoldDelayMs;//Math.Max(1,Math.Min(HoldDelayMs, BackspaceHoldToRepeatMs));
                await Task.Delay(dt);
                t += dt;
                if (!PressedKeys.Contains(kvm)) {
                    return;
                }
            }
        }
        void MoveKey(Touch_fallback touch) {
            if (IsCursorControlEnabled) {
                UpdateCursorControl(touch);
                return;
            } 
            if(IsPullEnabled) {
                UpdatePull(touch);
            }
            var pressed_kvm = GetPressedKeyForTouch(touch);
            if(IsHoldMenuVisible && 
                pressed_kvm != null) {
                pressed_kvm.UpdateActivePopup(touch);
                return;
            }
            var touch_kvm = GetKeyUnderPoint(touch.Location);
            if (IsSlideEnabled &&
                        pressed_kvm != null &&
                        touch_kvm != null &&
                        pressed_kvm != touch_kvm) {
                // when key is pressed and this is its
                // associated touch but the touch isn't over the key

                // soft release it
                SoftReleaseKey(pressed_kvm);

                PressKeyAsync(touch).FireAndForgetSafeAsync();
            }
        } 
        void SoftReleaseKey(KeyViewModel_fallback kvm) {
            if(kvm == null) {
                return;
            }
            kvm.SetPressed(false, null);
        }
        void ReleaseKey(Touch_fallback touch) {
            if (IsCursorControlEnabled) {
                StopCursorControl(touch);
                return;
            }
            if (GetPressedKeyForTouch(touch) is not { } pressed_kvm)  {
                return;
            }

            PerformKeyAction(pressed_kvm);

            bool is_released = Touches_fallback.Locate(touch.Id) == null;
            if(is_released) {
                pressed_kvm.SetPressed(false,null);
                if(pressed_kvm.IsBackspace) {
                    ResetBackspace();
                }
            }
        }

        void PerformKeyAction(KeyViewModel_fallback pressed_kvm) {
            var active_kvm = pressed_kvm.ActivePopupKey;
            if (active_kvm == null) {
                active_kvm = pressed_kvm;
            }
            if (active_kvm == null) {
                return;
            }

            switch (pressed_kvm.SpecialKeyType) {
                case SpecialKeyType_fallback.Shift:
                    HandleShift();
                    break;
                case SpecialKeyType_fallback.SymbolToggle:
                case SpecialKeyType_fallback.NumberSymbolsToggle:
                    ToggleSymbolSet();
                    break;
                case SpecialKeyType_fallback.Backspace:
                    // handled in press
                    break;
                case SpecialKeyType_fallback.Enter:
                case SpecialKeyType_fallback.Next:
                    InputConnection?.OnDone();
                    break;
                case SpecialKeyType_fallback.CapsLock:
                    ToggleCapsLock();
                    break;
                case SpecialKeyType_fallback.NextKeyboard:
                    if (InputConnection is IKeyboardInputConnection_ios_fallback ios_ic) {
                        ios_ic.OnInputModeSwitched();
                    }
                    break;
                default:
                    string pv = active_kvm.PrimaryValue;
                    if (IsPullEnabled &&
                        active_kvm.IsPopupKey && 
                        active_kvm.PopupAnchorKey is { } anchor_kvm &&
                        anchor_kvm.IsPulled) {
                        // release comes from active not pressed
                        // when pulled don't care whats active just use secondary
                        pv = anchor_kvm.SecondaryValue;
                        anchor_kvm.PullTranslateY = 0;
                    }
                    if (!IsNumbers &&
                                active_kvm.IsSpaceBar &&
                                active_kvm.LastReleaseDt is { } lrdt &&
                                DateTime.Now - lrdt <= TimeSpan.FromMilliseconds(MaxDoubleTapSpaceForPeriodMs)) {
                        // double tap space for period
                        // automate backspace, then period press
                        //PerformKeyAction(backspace_kvm);
                        InputConnection.OnBackspace(2);
                        pv = ". ";
                    }
                    InputConnection?.OnText(pv);

                    LastInput = pv;

                    if (ShiftState == ShiftStateType_fallback.Shift) {
                        SetShiftState(ShiftStateType_fallback.None);
                    }
                    if (active_kvm.IsSpaceBar && !IsNumbers) {
                        // after typing space reset to default keyboard                            
                        SetCharSet(CharSetType_fallback.Letters);
                    }
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand NextKeyboardCommand => ReactiveCommand.Create(() => {
            if(InputConnection is not IKeyboardInputConnection_ios_fallback ic_ios
            ) {
                return;
            }
            ic_ios.OnInputModeSwitched();
        });
        
        public ICommand Test1Command => ReactiveCommand.Create<object>(
            (args) => {
        });
        #endregion
    }
}
