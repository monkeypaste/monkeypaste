using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
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
using System.Threading.Tasks;
using System.Windows.Input;
using Size = Avalonia.Size;

namespace iosKeyboardTest {
    public class KeyboardViewModel : ViewModelBase {
        #region Private Variables        
        #endregion

        #region Constants
        const double TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO = 0.3;
        #endregion

        #region Statics
        public static Size GetTotalSizeByScreenSize(Size scaledScreenSize) {
            return new Size(scaledScreenSize.Width, scaledScreenSize.Height * TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO);
        }
        public static KeyboardView CreateKeyboardView(IKeyboardInputConnection inputConn, Size scaledSize, double scale, out Size unscaledSize) {
            var kbvm = new KeyboardViewModel(inputConn, scaledSize, scale);
            var kbv = new KeyboardView() {
                DataContext = kbvm,
                Width = kbvm.TotalWidth,
                Height = kbvm.TotalHeight
            };
            unscaledSize = new Size(kbvm.TotalWidth * scale, kbvm.TotalHeight * scale);
            return kbv;
        }
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
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
                if(!Rows.Any()) {
                    return 0;
                }
                if (_keyHeight <= 0) {
                    // avoid div by 0
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
            MenuHeight;


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
        bool IsHeadlessMode =>
            InputConnection is IHeadlessRender;
        public double ScreenScaling { get; set; }
        public string ErrorText { get; private set; } = "NO ERRORS";
        public bool NeedsNextKeyboardButton =>
            //OperatingSystem.IsWindows() ||
            (OperatingSystem.IsIOS() &&
            InputConnection != null &&
            (InputConnection as IKeyboardInputConnection_ios).NeedsInputModeSwitchKey);
        double CursorControlFactorX => 4;
        double CursorControlFactorY => 4;
        public bool IsNumbers =>
            KeyboardFlags.HasFlag(KeyboardFlags.Numbers);
        KeyboardFlags KeyboardFlags { get; set; }
        bool IsPopupVisible =>
            //PopupKeys.Any() &&
            //PressedKeyViewModel != null &&
            //ActiveKeyViewModel != null &&
            //PressedKeyViewModel.PrimaryValue != ActiveKeyViewModel.PrimaryValue;
            PopupKeys.Count() > 1;
        uint RepeatCount { get; set; }

        TimeSpan MinHoldDur => TimeSpan.FromMilliseconds(300);
        TimeSpan MinRepeatDur => TimeSpan.FromMilliseconds(300);
        public Point? KeyboardPointerLocation { get; private set; }
        public Point? KeyboardPointerDownLocation { get; private set; }
        public CharSetType CharSet { get; set; }
        public ShiftStateType ShiftState { get; set; }
        IKeyboardInputConnection InputConnection { get; set; }
        IHeadlessRender HeadlessRender =>
            InputConnection as IHeadlessRender;
        public bool IsCursorControlEnabled => LastCursorControlUpdateLocation.HasValue;
        Point? LastCursorControlUpdateLocation { get; set; }
        #endregion

        #endregion

        #region Constructors
        public KeyboardViewModel() : this(null,new Size(360,740),2.75) { }
        public KeyboardViewModel(IKeyboardInputConnection inputConn, Size scaledSize, double scale) 
        {
            Debug.WriteLine("kbvm ctor called");
            ScreenScaling = scale;
            Keys.CollectionChanged += Keys_CollectionChanged;
            SetInputConnection(inputConn);

            Init(KeyboardFlags.Phone);
            SetDesiredSize(scaledSize);
        }

        #endregion

        #region Public Methods
        public void SetInputConnection(IKeyboardInputConnection conn) {
            InputConnection = conn;
        }
        public void RefreshLayout() {
            _defKeyWidth = -1;
            _keyHeight = -1;
            UpdateKeyboardState();
        }
        public void SetPointerLocation(Point? mp) {
            var last_pressed_kvm = PressedKeyViewModel;
            var last_active_kvm = ActiveKeyViewModel;
            var new_pressed_kvm = mp.HasValue ? GetKeyUnderPoint(mp.Value) : null;
            if(IsCursorControlEnabled) {
                new_pressed_kvm = last_pressed_kvm;
            }
            var last_mp = KeyboardPointerLocation;
            KeyboardPointerLocation = mp;
            if(KeyboardPointerDownLocation == null || KeyboardPointerLocation == null) {
                // set down on press and clear on release
                KeyboardPointerDownLocation = KeyboardPointerLocation;
            }

            if (KeyboardPointerLocation.HasValue &&
               (last_pressed_kvm == new_pressed_kvm || IsPopupVisible)) {
                // still over same key


                UpdatePull();
            } else {
                if (last_pressed_kvm == null) {
                    // new press
                    PressKey(new_pressed_kvm);
                    StartPressTimer();
                } else if (new_pressed_kvm == null) {
                    // release
                    ReleaseKey(last_active_kvm);
                    ClearHoldKeys(true);
                } else if(last_pressed_kvm != new_pressed_kvm) {
                    // drag enter
                    if(last_pressed_kvm != null && last_pressed_kvm.IsPulling) {
                        //
                    } else {
                        ClearHoldKeys(true);
                        PressKey(new_pressed_kvm);
                        StartPressTimer();
                    }
                }
            }
            if(!KeyboardPointerLocation.HasValue) {
                // this shouldn't be needed but maybe due to desktop
                ClearHoldKeys(true);
                if(IsCursorControlEnabled) {
                    StopCursorControl();
                }
            }
            UpdateKeyboardState();
        }

        public void SetError(string msg) {
            Dispatcher.UIThread.Post(() => {
                ErrorText = msg;
                this.RaisePropertyChanged(nameof(ErrorText));
            });
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
            //while(true) {
            //    // hack to update layout until Total Size equals param size
            //    // since layout depends on KeyboardWidth/Height NOT total
            //    //RefreshLayout();
            //    // initially TotalHeight is bigger than param size by top/bottom menu heights
            //    double diff = TotalHeight - size.Height;
            //    if(Math.Abs(diff) < 0.01) {
            //        break;
            //    }
            //    KeyboardHeight -= 0.01;
            //}
            //double final_diff = TotalHeight - size.Height;
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
            UpdateKeyboardState();
        }


        private void Keys_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if(e.OldItems == null ||
                e.OldItems.OfType<KeyViewModel>() is not { } removed_kvml) {
                return;
            }
            foreach(var kvm in removed_kvml) {
                kvm.Cleanup();
            }
        }
        KeyViewModel GetKeyUnderPoint(Point scaledPoint) {
            var p = scaledPoint;
            if(IsHeadlessMode) {
                // translate for menu strip height
                //p = new Point(scaledPoint.X * ScreenScaling, (scaledPoint.Y * ScreenScaling) - MenuHeight);
                p = new Point(scaledPoint.X, scaledPoint.Y - MenuHeight);
            }
            var result = Keys
                .Where(x => x != null)
                .Select(x => (x, new Rect(x.X, x.Y, x.Width, x.Height)))
                .FirstOrDefault(x => x.Item2.Contains(p));
            return result.x;
        }
        public void UpdateKeyboardState() {
            this.RaisePropertyChanged(nameof(Keys));
            this.RaisePropertyChanged(nameof(KeyboardWidth));
            this.RaisePropertyChanged(nameof(KeyboardHeight));
            this.RaisePropertyChanged(nameof(IsCursorControlEnabled));
            this.RaisePropertyChanged(nameof(TotalWidth));
            this.RaisePropertyChanged(nameof(TotalHeight));
            this.RaisePropertyChanged(nameof(MenuHeight));

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
                key.RaisePropertyChanged(nameof(key.SecondaryOpacity));
                key.RaisePropertyChanged(nameof(key.PrimaryOpacity));
                key.RaisePropertyChanged(nameof(key.SecondaryTranslateOffsetX));
                key.RaisePropertyChanged(nameof(key.SecondaryTranslateOffsetY));
                key.RaisePropertyChanged(nameof(key.PullTranslateY));
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
                key.RaisePropertyChanged(nameof(key.IsSpecial));
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
        private void StartPressTimer() {
            hold_timer_count++;
            if(PressedKeyViewModel == null) {
                FinishHoldTimer();
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                int hold_delay = 30;
                var cur_pressed_kvm = PressedKeyViewModel;
                var hold_sw = Stopwatch.StartNew();
                while(true) {
                    if(IsCursorControlEnabled) {
                        UpdateCursorControl();
                        await Task.Delay(hold_delay);
                        continue;
                    }
                    if (cur_pressed_kvm != PressedKeyViewModel) {
                        // no longer holding
                        FinishHoldTimer();
                        return;
                    }
                    //if(cur_pressed_kvm == null) {
                    //    await Task.Delay(hold_delay);
                    //    continue;
                    //}
                    if(cur_pressed_kvm != null && cur_pressed_kvm.CanRepeat) {
                        // only backspace key
                        if(hold_sw.Elapsed >= MinRepeatDur) {
                            RepeatCount++;
                            hold_sw.Restart();
                            Debug.WriteLine($"Repeat: {RepeatCount}");
                            for (int i = 0; i < RepeatCount; i++) {
                                ReleaseKey(cur_pressed_kvm,true);
                            }
                        }
                        await Task.Delay(hold_delay);
                        continue;
                    }
                    if (hold_sw.Elapsed >= MinHoldDur) {
                        // hold
                        if (cur_pressed_kvm != null && cur_pressed_kvm.IsSpaceBar) {
                            StartCursorControl();
                        } else {
                            if (cur_pressed_kvm != null && cur_pressed_kvm.HasHoldPopup) {
                                ShowHoldPopup(cur_pressed_kvm);
                            }
                            FinishHoldTimer();
                            return;
                        }

                    }

                    await Task.Delay(hold_delay);
                }
            });
        }
        void FinishHoldTimer() {
            hold_timer_count--;
            //Debug.WriteLine($"Hold timer done. Remaining: {hold_timer_count}");
        }

        void ShowPressPopup(KeyViewModel kvm) {
            ClearHoldKeys();

            if (kvm != null && kvm.HasPressPopup) {
                kvm.IsPressed = true;
                var sec_kvm = CreatePopUpKeyViewModel(kvm, 0,1, kvm.CurrentChar);
                Keys.Add(sec_kvm);
            }
            UpdateKeyboardState();
        }
        void ShowHoldPopup(KeyViewModel kvm) {
            if(IsPopupVisible && kvm != null && !kvm.HasHoldPopup) {
                return;
            }
            ClearHoldKeys();
            if(kvm != null) {
                if(kvm.HasHoldPopup) {
                    var chars = kvm.SecondaryCharacters.ToList();
                    int count = chars.Count;
                    if (count > MaxPopupColCount) {
                        // add fake ones to keep popup square
                        int col_diff = count % MaxPopupColCount;
                        while (col_diff != 0) {
                            chars.Add(string.Empty);
                            count++;
                            col_diff = count % MaxPopupColCount;
                        }
                    }
                    var sec_kvml =
                        chars
                        .Select((x, idx) => CreatePopUpKeyViewModel(kvm, idx, chars.Count, x))
                        .Where(x => x != null);
                    Keys.AddRange(sec_kvml);
                }
                
            }
            UpdateKeyboardState();
        }
        void ShowPullKey(KeyViewModel kvm) {

        }
        #region Cursor Control
        void StartCursorControl() {
            if(KeyboardPointerLocation == null) {
                // shouldn't happen
                Debugger.Break();
            }
            LastCursorControlUpdateLocation = KeyboardPointerLocation.Value;
            UpdateKeyboardState();
        }
        void StopCursorControl() {
            LastCursorControlUpdateLocation = null;
            UpdateKeyboardState();
        }
        void UpdateCursorControl() {
            var mp = KeyboardPointerLocation.Value;
            var lump = LastCursorControlUpdateLocation.Value;

            int dx = (int)Math.Floor((mp.X - lump.X)/CursorControlFactorX);
            int dy = (int)Math.Floor((mp.Y - lump.Y)/CursorControlFactorY);
            if(dx == 0 && dy == 0) {
                return;
            }
            LastCursorControlUpdateLocation = mp;
            InputConnection.OnNavigate(dx,dy);
        }
        #endregion

        #region Key Pull
        public void UpdatePull() {
            if(KeyboardPointerLocation is not { } mp ||
                KeyboardPointerDownLocation is not { } dmp ||
                PressedKeyViewModel is not { } pkvm ||
                !pkvm.CanPullKey) {
                return;
            }
            double y_diff = mp.Y - dmp.Y;
            pkvm.PullTranslateY = Math.Clamp(y_diff, 0, pkvm.MaxPullTranslateY);
            UpdateKeyboardState();
            Debug.WriteLine($"Pull: {y_diff}");
        }
        #endregion

        void PressKey(KeyViewModel kvm) {
            if(kvm == null) {
                return;
            }
            kvm.IsPressed = true;
            ShowPressPopup(kvm);
            Debug.WriteLine($"Released '{kvm.CurrentChar}'");
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
                    if(IsCursorControlEnabled) {
                        StopCursorControl();
                        break;
                    }
                    string pv = kvm.PrimaryValue;
                    if(kvm.IsPopupKey && PressedKeyViewModel.IsPulled) {
                        // release comes from active not pressed
                        // when pulled don't care whats active just use secondary
                        pv = PressedKeyViewModel.SecondaryValue;
                        PressedKeyViewModel.PullTranslateY = 0;
                    }
                    InputConnection?.OnText(pv);

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
        public ICommand NextKeyboardCommand => ReactiveCommand.Create(() => {
            if(InputConnection is not IKeyboardInputConnection_ios ic_ios
            ) {
                return;
            }
            ic_ios.OnInputModeSwitched();
        });
        
        public ICommand Test1Command => ReactiveCommand.Create<object>(
            (args) => {

                //var test = new Border() {
                //    Background = Brushes.Purple,
                //    Width = 1000,
                //    Height = 1000,
                //    //Child = new Ellipse() {
                //    //    Width = 100,
                //    //    Height = 100,
                //    //    Fill = Brushes.Orange
                //    //}
                //    Child = new TestView() {
                //        Width = 100,
                //        Height = 100
                //    }
                //};
                //var rect = new Rect(0, 0, 500, 500);
                //var test = new TestView() {
                //    Width = rect.Width,
                //    Height = rect.Height
                //};
                //test.InitializeComponent();
                //test.Measure(rect.Size);
                //test.Arrange(rect);
                //test.UpdateLayout();
                //test.InvalidateVisual();

                var test = KeyboardBuilder.Build(InputConnection,new Size(KeyboardWidth,KeyboardHeight),ScreenScaling, out _);

                RenderHelpers.RenderToFile(test, @"C:\Users\tkefauver\Desktop\test1.png");


        });
        #endregion
    }
}
