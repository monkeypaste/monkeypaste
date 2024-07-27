using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Size = Avalonia.Size;

namespace iosKeyboardTest.iOS {
    public class KeyboardViewModel : ViewModelBase, IKeyboardViewRenderer, IKeyboardRenderSource {
        #region Private Variables        
        #endregion

        #region Constants
        const double TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO_PORTRAIT = 0.3;
        const double TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO_LANDSCAPE = 0.5;

        public const double KEYBOARD_CNTR_SCREEN_HEIGHT_RATIO = 0.75;
        #endregion

        #region Statics
        static Size? ScaledScreenSize { get; set; }
        public static Size GetTotalSizeByScreenSize(Size scaledScreenSize, bool isPortrait) {
            if (ScaledScreenSize == null) {
                ScaledScreenSize = scaledScreenSize;
            }
            double ratio = isPortrait ? TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO_PORTRAIT : TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO_LANDSCAPE;

            return new Size(scaledScreenSize.Width, scaledScreenSize.Height * ratio);
        }

        #endregion

        #region Interfaces

        #region IKeyboardViewRenderer Implementation
        void IKeyboardViewRenderer.Layout(bool invalidate) {
            Keys.ForEach(x => x.Renderer.Layout(invalidate));
            this.RaisePropertyChanged(nameof(KeyboardWidth));
            this.RaisePropertyChanged(nameof(KeyboardHeight));
            this.RaisePropertyChanged(nameof(TotalWidth));
            this.RaisePropertyChanged(nameof(TotalHeight));
            this.RaisePropertyChanged(nameof(MenuHeight));
        }

        void IKeyboardViewRenderer.Measure(bool invalidate) {
            Keys.ForEach(x => x.Renderer.Measure(invalidate));
            this.RaisePropertyChanged(nameof(NeedsNextKeyboardButton));
        }

        void IKeyboardViewRenderer.Paint(bool invalidate) {
            Keys.ForEach(x => x.Renderer.Paint(invalidate));

            this.RaisePropertyChanged(nameof(CursorControlOpacity));
            this.RaisePropertyChanged(nameof(ErrorText));
        }
        void IKeyboardViewRenderer.Render(bool invalidate) {
            Keys.ForEach(x => x.Renderer.Render(invalidate));
            Renderer.Layout(false);
            Renderer.Measure(false);
            Renderer.Paint(invalidate);
        }
        #endregion

        #endregion

        #region Properties
        public string Test { get; set; } = "Im a test";

        #region Members
        public KeyboardFlags KeyboardFlags { get; set; }
        KeyboardFlags LastInitializedFlags { get; set; }

        public IKeyboardInputConnection InputConnection { get; set; }

        IKeyboardViewRenderer _renderer;
        public IKeyboardViewRenderer Renderer =>
            _renderer ?? this;
        #endregion

        #region View Models
        public ObservableCollection<KeyViewModel> Keys { get; set; } = [];
        public KeyViewModel[] PopupKeys { get; private set; }
        public List<KeyViewModel> VisiblePopupKeys { get; private set; } = [];
        public List<KeyViewModel> PressedKeys { get; set; } = [];
        public KeyViewModel SpacebarKey { get; private set; }
        public KeyViewModel BackspaceKey { get; private set; }
        public KeyViewModel PeriodKey { get; private set; }

        #endregion

        #region Layout
        Size DesiredSize { get; set; }
        public Thickness KeyboardMargin { get; set; } = new Thickness(0, 3, 0, 5);
        public double TotalTopPad { get; private set; }
        public Rect CursorControlRect { get; private set; }
        public Rect CursorControlTextRect { get; private set; }
        public Rect InnerRect { get; private set; } = new();
        public Rect TotalRect { get; private set; } = new();
        public Rect MenuRect { get; private set; } = new();
        public Rect KeyboardRect { get; private set; } = new();
        public int MaxColCount { get; private set; } = 1;
        public int RowCount { get; private set; }
        public double CursorControlFontSize => 12;
        public double NumberRowHeightRatio => IsNumPadLayout ? 1 : 0.75;

        private double _defKeyWidth = -1;
        public double DefaultKeyWidth {
            get {
                if (_defKeyWidth <= 0) {
                    _defKeyWidth = KeyboardInnerWidth / MaxColCount;

                }
                return _defKeyWidth;
            }
        }

        void SetRowHeights() {
            if(IsNumberRowVisible) {
                _keyHeight = -1;
                var def_height = KeyboardInnerHeight / RowCount;
                _numKeyHeight = def_height * NumberRowHeightRatio;
                double num_diff = def_height - _numKeyHeight;
                double diff_per_row = num_diff / (RowCount - 1);
                _keyHeight = def_height + diff_per_row;
            } else {
                _keyHeight = KeyboardInnerHeight / RowCount;
                _numKeyHeight = _keyHeight;
            }
        }
        private double _numKeyHeight = -1;
        public double NumberKeyHeight {
            get {
                if (_numKeyHeight >= 0) {
                    return _numKeyHeight;
                }

                if (_numKeyHeight <= 0) {
                    SetRowHeights();
                }
                return _numKeyHeight;
            }
        }
        private double _keyHeight = -1;
        public double DefaultKeyHeight {
            get {
                if (_keyHeight >= 0) {
                    return _keyHeight;
                }

                if (_keyHeight <= 0) {
                    SetRowHeights();
                }
                return _keyHeight;
            }
        }
        public int MaxPopupColCount =>
            4;
        public int MaxPopupRowCount { get; private set; }
        double SpecialKeyWidthRatio => 1.5d;
        public double SpecialKeyWidth =>
            DefaultKeyWidth * (IsNumPadLayout ? 1 : SpecialKeyWidthRatio);

        double MenuHeightPad => 5;
        public double MenuHeight =>
            DefaultKeyHeight + MenuHeightPad;
        public double FooterHeight =>
            NeedsNextKeyboardButton ? MenuHeight * 0.5 : 0;

        public double KeyboardWidth { get; private set; }

        public double KeyboardHeight { get; private set; }
        public double KeyboardInnerWidth =>
            KeyboardWidth - KeyboardMargin.Left - KeyboardMargin.Right;

        public double KeyboardInnerHeight =>
            KeyboardHeight - KeyboardMargin.Top - KeyboardMargin.Bottom;
        public double TotalWidth =>
            KeyboardWidth;
        public double TotalHeight =>
            TotalTopPad +
            MenuHeight +
            KeyboardHeight +
            FooterHeight;

        #endregion

        #region Appearance
        public string CursorControlText => "👆Cursor Control";
        public double CursorControlOpacity =>
            IsCursorControlEnabled ? 1 : 0;
        public bool IsSecondaryVisible =>
            IsNumPadLayout || IsLettersCharSet;
        #endregion

        #region State
        public bool IsLettersCharSet =>
            CharSet == CharSetType.Letters;
        public bool IsSymbols1CharSet =>
            CharSet == CharSetType.Symbols1;
        public bool IsSymbols2CharSet =>
            CharSet == CharSetType.Symbols2;
        public bool IsAnySymbolsCharSet =>
            IsSymbols1CharSet || IsSymbols2CharSet;
        public bool IsNumbers1CharSet =>
            CharSet == CharSetType.Numbers1;
        public bool IsNumbers2CharSet =>
            CharSet == CharSetType.Numbers2;
        public bool IsAnyNumbersCharSet =>
            IsNumbers1CharSet || IsNumbers2CharSet;
        public bool IsBusy { get; set; }

        string[] eos_chars = ["\n", ".", "!", "?", string.Empty];
        bool IsInitialized =>
            InputConnection != null && InputConnection.Flags == LastInitializedFlags;

        public KeyboardFeedbackFlags ActiveChangeFeedback =>
            KeyboardFeedbackFlags.Vibrate;
        public KeyboardFeedbackFlags CursorChangeFeedback =>
            KeyboardFeedbackFlags.Vibrate;
        public KeyboardFeedbackFlags KeyReleaseFeedback =>
            KeyboardFeedbackFlags.Vibrate;// | KeyboardFeedbackFlags.Click;
        public KeyboardFeedbackFlags ReturnReleaseFeedback =>
            KeyboardFeedbackFlags.Vibrate;// | KeyboardFeedbackFlags.Return;
        string LastInput { get; set; } = string.Empty;
        public bool IsAnyShiftState =>
            ShiftState != ShiftStateType.None;
        public bool IsShiftOnTemp =>
            ShiftState == ShiftStateType.Shift;
        public bool IsShiftOnLock =>
            ShiftState == ShiftStateType.ShiftLock;
        public bool IsPullEnabled =>
            IsTablet;
        public bool IsSlideEnabled =>
            IsMobile;
        bool IsHeadlessMode =>
            InputConnection is IKeyboardInputConnection_ios;
        public double ScreenScaling { get; private set; }
        public double ActualScaling { get; private set; }
        public string ErrorText { get; private set; } = "NO ERRORS";
        public bool NeedsNextKeyboardButton =>
            //OperatingSystem.IsWindows() ||
            (OperatingSystem.IsIOS() &&
            InputConnection != null &&
            (InputConnection as IKeyboardInputConnection_ios).NeedsInputModeSwitchKey);
        double CursorControlFactorX => ScreenScaling * 2;
        double CursorControlFactorY => ScreenScaling * 2;
        bool IsAnyPopupMenuVisible =>
            VisiblePopupKeys.Any();
        bool IsHoldMenuVisible =>
            VisiblePopupKeys.Skip(1).Any();

        private CharSetType _charSet;
        public CharSetType CharSet => _charSet;
        public int CharSetIdx {
            get {
                switch (CharSet) {
                    case CharSetType.Letters:
                        return 0;
                    case CharSetType.Symbols1:
                        return 1;
                    case CharSetType.Symbols2:
                        return 2;
                    case CharSetType.Numbers1:
                        return 0;
                    case CharSetType.Numbers2:
                        return 1;
                }

                return 0;
            }
        }
        private ShiftStateType _shiftState;
        public ShiftStateType ShiftState => _shiftState;
        ITriggerTouchEvents HeadlessRender =>
            InputConnection as ITriggerTouchEvents;

        #region Hold/Tap Stuff
        static int DefHoldDelayMs => 5; // NOTE! must be a factor of HoldDelayMs
        int HoldDelayMs { get; set; } = DefHoldDelayMs;
        int MinHoldMs => 500; // NOTE! must be a factor of HoldDelayMs

        int MaxDoubleTapSpaceForPeriodMs => 150;
        int MaxCursorChangeFromEventDelayForFeedbackMs => 50;

        #region Backspace
        int MinBackspaceRepeatMs => 25;
        int BackspaceHoldToRepeatMs { get; set; } = 100;// DefHoldToRepeatBackspaceMs; // NOTE! must be a factor of HoldDelayMs
        DateTime? LastBackspaceUpdateDt { get; set; }
        #endregion

        #region Cursor Control
        double MinCursorControlDragDist => 10;
        Point? LastCursorControlUpdateLocation { get; set; }
        DateTime? LastCursorControlUpdateDt { get; set; }
        public bool IsCursorControlEnabled { get; private set; }
        #endregion
        #endregion
        #endregion

        #region Model
        public bool IsTextLayout =>
            !IsNumPadLayout;
        public bool IsPin { get; private set; }
        public bool IsDigits { get; private set; }
        public bool IsNumbers { get; private set; }
        public bool IsNumPadLayout { get; private set; }
        public bool IsThemeDark { get; private set; }
        public bool IsTablet { get; private set; }
        public bool IsMobile { get; private set; }
        public bool IsNumberRowVisible { get; private set; }
        public bool IsNumberRowHidden =>
            !IsNumberRowVisible;
        public bool IsKeyBordersVisible { get; private set; }
        public bool IsEmojiButtonVisible { get; private set; }
        public bool IsExtendedPopupsEnabled { get; private set; }
        public bool IsAutoCapitalizationEnabled { get; private set; }
        public bool IsDoubleTapSpaceEnabled { get; private set; }
        public bool CanCursorControlBeEnabled { get; private set; }
        #endregion

        #endregion

        #region Constructors
        public KeyboardViewModel() : this(null, new Size(360, 740), 2.75) { }
        public KeyboardViewModel(IKeyboardInputConnection inputConn, Size scaledSize, double scale) : this(inputConn, scaledSize, scale, scale) { }

        public KeyboardViewModel(IKeyboardInputConnection inputConn, Size scaledSize, double scale, double actualScale) {
            Debug.WriteLine("kbvm ctor called");
            ScreenScaling = scale;
            ActualScaling = actualScale;
            Keys.CollectionChanged += Keys_CollectionChanged;
            SetInputConnection(inputConn);
            SetDesiredSize(scaledSize);
            if (inputConn is { } ic) {
                Init(ic.Flags);
            } else {
                Init(KeyboardFlags.Mobile | KeyboardFlags.Normal);
            }
            SetDesiredSize(scaledSize);

        }


        #endregion

        #region Public Methods
        public void SetRenderer(IKeyboardViewRenderer renderer) {
            _renderer = renderer;
        }
        public void SetInputConnection(IKeyboardInputConnection conn) {
            void OnPointerChanged(object s, TouchEventArgs e) {
                SetPointerLocation(e);
            }

            if (InputConnection is IKeyboardInputConnection old_conn) {
                old_conn.OnFlagsChanged -= Ic_OnFlagsChanged;
                old_conn.OnDismissed -= InputConnection_OnDismissed;
                if (old_conn is ITriggerTouchEvents tte) {
                    tte.OnPointerChanged -= OnPointerChanged;
                }

                if (old_conn is IKeyboardInputConnection_android old_ann_conn) {
                    old_ann_conn.OnCursorChanged -= Ic_OnCursorChanged_android;
                } else {
                    old_conn.OnCursorChanged -= Ic_OnCursorChanged;
                }
            }
            if (conn is IKeyboardInputConnection new_conn) {
                InputConnection = new_conn;
                new_conn.OnFlagsChanged += Ic_OnFlagsChanged;
                new_conn.OnDismissed += InputConnection_OnDismissed;
                if (new_conn is ITriggerTouchEvents tte) {
                    tte.OnPointerChanged += OnPointerChanged;
                }
                if(new_conn is IKeyboardInputConnection_android new_ann_conn) {
                    new_ann_conn.OnCursorChanged += Ic_OnCursorChanged_android;
                } else {
                    new_conn.OnCursorChanged += Ic_OnCursorChanged;
                }
            } else {
                InputConnection = null;
            }


        }


        private void InputConnection_OnDismissed(object sender, EventArgs e) {
            ResetState();
        }

        public void ResetLayout() {
            _numKeyHeight = -1;
            _defKeyWidth = -1;
            _keyHeight = -1;
        }
        void ResetState() {
            _lastDebouncedTouchEventArgs = null;
            LastInput = string.Empty;
            IsCursorControlEnabled = false;
            foreach (var pkvm in PressedKeys.ToList()) {
                pkvm.SetPressed(false, null);
            }
            Touches.Clear();
        }

        private TouchEventArgs _lastDebouncedTouchEventArgs;

        bool IsTouchBounced(TouchEventArgs e) {
            if(IsCursorControlEnabled) {
                return false;
            }
            if (e == null) {
                return true;
            }
            if (_lastDebouncedTouchEventArgs == null) {
                return false;
            }
            if (_lastDebouncedTouchEventArgs.TouchEventType != e.TouchEventType) {
                return false;
            }
            double dist = Touches.Dist(e.Location, _lastDebouncedTouchEventArgs.Location);
            return dist < 5;
        }

        public void SetPointerLocation(TouchEventArgs e) {
            var mp = e.Location;
            var touchType = e.TouchEventType;

            if (Touches.Update(mp, touchType) is not { } touch) {
                return;
            }

            if (IsTouchBounced(e)) {
                return;
            }
            _lastDebouncedTouchEventArgs = e;

            switch (touchType) {
                case TouchEventType.Press:
                    PressKeyAsync(touch).FireAndForgetSafeAsync();
                    break;
                case TouchEventType.Move:
                    MoveKey(touch);
                    break;
                case TouchEventType.Release:
                    ReleaseKey(touch);
                    break;
            }
        }
        KeyViewModel GetPressedKeyForTouch(Touch touch) {
            return PressedKeys
                        .FirstOrDefault(x => x.TouchId == touch.Id);
        }
        public void SetError(string msg) {
            ErrorText = msg;
            Renderer.Paint(true);
        }

        public void SetDesiredSize(Size scaledSize) {
            DesiredSize = scaledSize;
            double w = scaledSize.Width;
            double h = scaledSize.Height;
            if (IsHeadlessMode) {
                //w *= ScreenScaling;
                //h *= ScreenScaling;
            }
            KeyboardWidth = w;
            KeyboardHeight = h;
            MenuRect = new Rect(0, TotalTopPad, KeyboardWidth, MenuHeight);
            KeyboardRect = new Rect(0, MenuRect.Bottom, KeyboardWidth, KeyboardHeight);
            InnerRect = new Rect(0, MenuRect.Top, KeyboardWidth, MenuHeight + KeyboardHeight);
            TotalRect = new Rect(0, 0, TotalWidth, TotalHeight);

            CursorControlRect = new Rect(0, 0, InnerRect.Width, InnerRect.Height);
            var cct_size = new Size(CursorControlText.Length * CursorControlFontSize, CursorControlFontSize);
            double cct_l = CursorControlRect.Center.X - (cct_size.Width / 2);
            double cct_t = CursorControlRect.Center.Y - (cct_size.Height / 2);
            CursorControlTextRect = new Rect(cct_l, cct_t, cct_size.Width, cct_size.Height);

            ResetLayout();
            Keys.ForEach(x => x.SetSize());
            UpdateKeyboardState();
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        public void Init(KeyboardFlags flags) {
            IsBusy = true;
            Debug.WriteLine($"Busy called. Flags: '{flags}'");

            SetFlags(flags);

            if(IsKeyBordersVisible) {
                KeyboardPalette.SetTheme(IsThemeDark);
            } else {
                // when bg off ensure popups have bg  and special keys
                KeyboardPalette.SetTheme(
                    IsThemeDark,
                    spa: KeyboardPalette.BG_ALPHA,
                    pua: 255);
            }
            


            if (IsNumPadLayout) {
                SetCharSet(CharSetType.Numbers1);
            } else {
                SetCharSet(CharSetType.Letters);
            }
            CheckAutoCap();

            var keyRows = GetKeyRows(KeyboardFlags);
            Keys.Clear();
            MaxColCount = 0;
            RowCount = keyRows.Count;
            for (int r = 0; r < keyRows.Count; r++) {
                MaxColCount = Math.Max(MaxColCount, keyRows[r].Count);
                KeyViewModel prev_kvm = null;
                for (int c = 0; c < keyRows[r].Count; c++) {
                    var keyObj = keyRows[r][c];
                    int cur_col = prev_kvm == null ? 0 : prev_kvm.Column + (int)Math.Ceiling(prev_kvm.ColumnSpan);
                    var kvm = CreateKeyViewModel(keyRows[r][c], r, cur_col, prev_kvm);
                    Keys.Add(kvm);
                    prev_kvm = kvm;
                    if (kvm.IsSpaceBar) {
                        SpacebarKey = kvm;
                    } else if (kvm.IsPeriod) {
                        PeriodKey = kvm;
                    } else if (kvm.IsBackspace) {
                        BackspaceKey = kvm;
                    }
                }
            }
            int max_popup_keys = Keys.Max(x => x.GetSecondaryCharacters().Count());
            MaxPopupRowCount = (int)Math.Floor((double)max_popup_keys / (double)(MaxPopupColCount));
            //TotalTopPad = MaxPopupRowCount * DefaultKeyHeight;

            for (int i = 0; i < 2; i++) {
                // create 2x Max possible popup keys for multi touch
                KeyViewModel prev_pukvm = null;
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

            SetDesiredSize(DesiredSize);
            ResetLayout();
            UpdateKeyboardState();
            LastInitializedFlags = KeyboardFlags;
            IsBusy = false;
        }

        private void Keys_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            this.RaisePropertyChanged(nameof(Keys));
        }
        private void Ic_OnFlagsChanged(object sender, EventArgs e) {
            //if (IsInitialized ||
            //    InputConnection is not { } ic) {
            //    // ignore flag change before initialized so doesn't double init
            //    return;
            //}
            var new_flags = InputConnection.Flags;
            if (IsInitialized &&
                ((new_flags.HasFlag(KeyboardFlags.Portrait) && LastInitializedFlags.HasFlag(KeyboardFlags.Landscape)) ||
                (new_flags.HasFlag(KeyboardFlags.Landscape) && LastInitializedFlags.HasFlag(KeyboardFlags.Portrait)))) {
                // orientation change
                ScaledScreenSize = new Size(ScaledScreenSize.Value.Height, ScaledScreenSize.Value.Width);
                var desired_size = GetTotalSizeByScreenSize(ScaledScreenSize.Value, new_flags.HasFlag(KeyboardFlags.Portrait));
                SetDesiredSize(desired_size);
                InputConnection.OnText(Environment.NewLine + "VM " + (new_flags.HasFlag(KeyboardFlags.Portrait) ? "PORTRAIT" : "LANDSCAPE"));
            }
            if (new_flags.HasFlag(KeyboardFlags.PlatformView)) {
                Init(new_flags);
                return;
            }
            Dispatcher.UIThread.Post(() => Init(new_flags));
        }

        private void Ic_OnCursorChanged(object sender, EventArgs e) {
            HandleCursorChange(null);
        }

        private void Ic_OnCursorChanged_android(object sender, (string text, (int sidx, int len)) e) {
            HandleCursorChange(e);
        }
        void HandleCursorChange((string text, (int sidx, int len))? textInfo) {

            DateTime this_cursor_change_dt = DateTime.Now;

            bool do_vibrate = false;
            if (LastCursorControlUpdateDt is { } lccudt &&
                this_cursor_change_dt - lccudt <= TimeSpan.FromMilliseconds(MaxCursorChangeFromEventDelayForFeedbackMs)) {
                do_vibrate = true;
            } else if (LastBackspaceUpdateDt is { } lbsudt &&
                this_cursor_change_dt - lbsudt <= TimeSpan.FromMilliseconds(MaxCursorChangeFromEventDelayForFeedbackMs)) {
                do_vibrate = true;
            }
            if (do_vibrate) {
                InputConnection.OnFeedback(KeyboardFeedbackFlags.Vibrate);
            }
            if (KeyboardFlags.HasFlag(KeyboardFlags.PlatformView)) {
                CheckAutoCap(textInfo);
                return;
            }
            Dispatcher.UIThread.Post(()=>CheckAutoCap(textInfo));
        }
        void CheckAutoCap((string text, (int sidx, int len))? textInfo) {
            if (!IsAutoCapitalizationEnabled ||
                InputConnection is not { } ic ||
                ShiftState == ShiftStateType.ShiftLock) {
                return;
            }
            bool needs_shift = false;
            string leading_text = null;
            if(textInfo.HasValue) {
                // BUG android OnUpdateSelection is called right BEFORE its available to the CurrentInputConnection
                // so have to use the params from OnUpdateSelection
                if (string.IsNullOrEmpty(textInfo.Value.text) || (textInfo.Value.Item2.sidx == 0 && textInfo.Value.Item2.len == 0)) {
                    leading_text = null;
                } else {
                    leading_text = textInfo.Value.text.Substring(0, textInfo.Value.Item2.sidx + 1);
                }                
            } else {
                leading_text = ic.GetLeadingText(-1, -1);
            }
            if (string.IsNullOrEmpty(leading_text)) {
                // auto cap if insert is leading
                needs_shift = true;
            } else {

                for (int i = 0; i < leading_text.Length; i++) {
                    string cur_char = leading_text[leading_text.Length - i - 1].ToString();
                    if (cur_char == " ") {
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
            SetShiftState(needs_shift ? ShiftStateType.Shift : ShiftStateType.None);
        }
        void SetFlags(KeyboardFlags kbFlags) {
            if (kbFlags.HasFlag(KeyboardFlags.EmojiKey) && (kbFlags.HasFlag(KeyboardFlags.Email) || kbFlags.HasFlag(KeyboardFlags.Url))) {
                // remove emoji key for url/email. It takes up too much space and kb overflows plus it doesn't make sense for those keyboards
                kbFlags &= ~KeyboardFlags.EmojiKey;
            }
            KeyboardFlags = kbFlags;
            IsThemeDark = KeyboardFlags.HasFlag(KeyboardFlags.Dark);
            IsNumbers = KeyboardFlags.HasFlag(KeyboardFlags.Numbers);
            IsDigits = KeyboardFlags.HasFlag(KeyboardFlags.Digits);
            IsPin = KeyboardFlags.HasFlag(KeyboardFlags.Pin);
            IsNumPadLayout = IsNumbers || IsDigits || IsPin;
            IsMobile = KeyboardFlags.HasFlag(KeyboardFlags.Mobile);
            IsTablet = KeyboardFlags.HasFlag(KeyboardFlags.Tablet);
            IsNumberRowVisible = KeyboardFlags.HasFlag(KeyboardFlags.NumberRow);
            IsKeyBordersVisible = KeyboardFlags.HasFlag(KeyboardFlags.KeyBorders);
            IsEmojiButtonVisible = KeyboardFlags.HasFlag(KeyboardFlags.EmojiKey);
            IsExtendedPopupsEnabled = KeyboardFlags.HasFlag(KeyboardFlags.ShowPopups);

            IsAutoCapitalizationEnabled = KeyboardFlags.HasFlag(KeyboardFlags.AutoCap);
            IsDoubleTapSpaceEnabled = KeyboardFlags.HasFlag(KeyboardFlags.DoubleTapSpace);
            CanCursorControlBeEnabled = KeyboardFlags.HasFlag(KeyboardFlags.CursorControl);

        }

        KeyViewModel GetKeyUnderPoint(Point scaledPoint) {
            var p = scaledPoint;
            var result = Keys
                .Where(x => x != null && !x.IsPopupKey)
                .FirstOrDefault(x => x.TotalRect.Contains(p));
            return result;
        }
        public void UpdateKeyboardState() {

            foreach (var kvm in Keys) {
                if (kvm == null) {
                    continue;
                }
                kvm.Renderer.Render(true);
            }
            Renderer.Render(true);
#if DEBUG
            if (KeyboardGridView.DebugCanvas != null) {
                KeyboardGridView.DebugCanvas.InvalidateVisual();
            }
#endif
        }

        KeyViewModel CreatePopUpKeyViewModel(int idx, int r, int c, KeyViewModel prev) {
            var pu_kvm = CreateKeyViewModel(idx, r, c, prev);
            return pu_kvm;
        }

        KeyViewModel CreateKeyViewModel(object keyObj, int r, int c, KeyViewModel prev) {
            var kvm = new KeyViewModel(this, prev, keyObj) {
                Row = r,
                Column = c
            };
            return kvm;
        }


        SpecialKeyType GetPrimarySpecialKey(KeyboardFlags kbFlags) {
            if (kbFlags.HasFlag(KeyboardFlags.Next)) {
                return SpecialKeyType.Next;
            }
            if (kbFlags.HasFlag(KeyboardFlags.Done)) {
                return SpecialKeyType.Done;
            }
            if (kbFlags.HasFlag(KeyboardFlags.Normal)) {
                return SpecialKeyType.Enter;
            }
            if (kbFlags.HasFlag(KeyboardFlags.Numbers) ||
                kbFlags.HasFlag(KeyboardFlags.Email)) {
                return SpecialKeyType.Next;
            }
            if (kbFlags.HasFlag(KeyboardFlags.Search)) {
                return SpecialKeyType.Search;
            }
            if (kbFlags.HasFlag(KeyboardFlags.Url)) {
                return SpecialKeyType.Go;
            }

            return SpecialKeyType.Done;
        }
        List<List<object>> GetKeyRows(KeyboardFlags kbFlags) {
            List<List<object>> keys = null;
            SpecialKeyType primarySpecialType = GetPrimarySpecialKey(kbFlags);
            if (kbFlags.HasFlag(KeyboardFlags.Mobile)) {
                if (kbFlags.HasFlag(KeyboardFlags.Numbers) || kbFlags.HasFlag(KeyboardFlags.Digits)) {
                    keys = new List<List<object>> {
                        (["1,(", "2,/", "3,)", SpecialKeyType.Backspace]),
                        (["4,N", "5,comma", "6,.", primarySpecialType]),
                        (["7,*", "8,;", "9,#", SpecialKeyType.NumberSymbolsToggle]),
                        (["*,-", "0,+", "#,__", "comma"])
                    };
                } else if (kbFlags.HasFlag(KeyboardFlags.Pin)) {
                    keys = new List<List<object>> {
                        (["1", "2", "3", SpecialKeyType.Backspace]),
                        (["4", "5", "6", primarySpecialType]),
                        (["7", "8", "9", ""]),
                        (["", "0", "", ""])
                    };
                } else {                    
                    keys = [
                            (["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"]),
                            (["q,+,`", "w,×,~", "e,÷,\\", "r,=,|", "t,/,{", "y,_,}", "u,<,€", "i,>,£", "o,[,¥", "p,],₩"]),
                            (["a,!,○", "s,@,•", "d,#,⚪", "f,$,⚫", "g,%,□", "h,^,🔳", "j,&,♤", "k,*,♡", "l,(,♢", "none,),♧"]),
                            ([SpecialKeyType.Shift, "z,-,☆", "x,',▪", "c,\",▫", "v,:,≪", "b,;,≫", "n,comma,¡", "m,?,¿", SpecialKeyType.Backspace]),
                            ([SpecialKeyType.SymbolToggle, "comma", " ", ".", primarySpecialType])
                        ];

                    if (kbFlags.HasFlag(KeyboardFlags.Email) || kbFlags.HasFlag(KeyboardFlags.Url)) {
                        // add input type char after symbol toggle
                        string ins_char1 = kbFlags.HasFlag(KeyboardFlags.Email) ? "@" : "/";
                        int ins_idx1 = keys.Last().IndexOf(SpecialKeyType.SymbolToggle) + 1;
                        keys.Last().Insert(ins_idx1, ins_char1);

                        // insert .com after period
                        string ins_char2 = ".com";
                        int ins_idx2 = keys.Last().IndexOf(".") + 1;
                        keys.Last().Insert(ins_idx2, ins_char2);
                    } 
                    if (kbFlags.HasFlag(KeyboardFlags.EmojiKey)) {
                        // insert emoji key before space bar
                        keys.Last().Insert(1, SpecialKeyType.Emoji);
                    }

                    if (!kbFlags.HasFlag(KeyboardFlags.NumberRow)) {
                        // numbers become 2nd row secondary 
                        var num_row = keys[0];
                        keys.Remove(num_row);
                        List<object> new_top_row = [];
                        for (int i = 0; i < keys[0].Count; i++) {
                            if (keys[0][i] is not string top_set_val) {
                                continue;
                            }
                            var val_parts = top_set_val.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            val_parts.Insert(1, num_row[i] as string);
                            new_top_row.Add(string.Join(",", val_parts));
                        }
                        keys[0] = new_top_row;
                    }
                }

            } else {
                // tablet
                keys = new List<List<object>> {
                    (["1", "2", "3", "4", "5", "6", "7", "8", "9", "0", SpecialKeyType.Backspace]),
                    ([SpecialKeyType.Tab, "q,+,`", "w,×,~", "e,÷,\\", "r,=,|", "t,/,{", "y,_,}", "u,<,€", "i,>,£", "o,[,¥", "p,],₩"]),
                    ([SpecialKeyType.CapsLock, "a,!,○", "s,@,•", "d,#,⚪", "f,$,⚫", "g,%,□", "h,^,🔳", "j,&,♤", "k,*,♡", "l,(,♢", "none,),♧", primarySpecialType]),
                    ([SpecialKeyType.Shift, "z,-,☆", "x,',▪", "c,\",▫", "v,:,≪", "b,;,≫", "n,comma,¡", "m,?,¿", "comma",".", SpecialKeyType.Shift]),
                    ([SpecialKeyType.SymbolToggle, SpecialKeyType.Emoji, " ", SpecialKeyType.ArrowLeft, SpecialKeyType.ArrowRight, SpecialKeyType.NextKeyboard])
                };
            }
            return keys;
        }

        void ToggleSymbolSet() {
            CharSetType to_select = CharSet;
            switch(CharSet) {
                case CharSetType.Numbers1:
                    to_select = CharSetType.Numbers2;
                    break;
                case CharSetType.Numbers2:
                    to_select = CharSetType.Numbers1;
                    break;
                case CharSetType.Letters:
                    to_select = CharSetType.Symbols1;
                    break;
                default:
                    to_select = CharSetType.Letters;
                    break;

            }
            if(to_select == CharSet) {
                // missing state?
                Debugger.Break();
            }
            SetCharSet(to_select);
        }
        void ToggleCapsLock() {
            if (ShiftState == ShiftStateType.ShiftLock) {
                SetShiftState(ShiftStateType.None);
            } else {
                SetShiftState(ShiftStateType.ShiftLock);
            }
        }
        void SetShiftState(ShiftStateType sst) {
            if(_shiftState == sst) {
                return;
            }
            _shiftState = sst;
            foreach (var kvm in Keys) {
                kvm.UpdateCharacters();
                kvm.Renderer.Measure(false);
                kvm.Renderer.Render(true);
            }
        }
        void SetCharSet(CharSetType cst) {
            if(_charSet == cst) {
                return;
            }
            _charSet = cst;
            foreach (var kvm in Keys) {
                kvm.UpdateCharacters();
                kvm.Renderer.Measure(false);
            }
            Renderer.Render(true);
        }

        void HandleShift() {
            if (CharSet == CharSetType.Letters) {
                if (ShiftState == ShiftStateType.ShiftLock) {
                    SetShiftState(ShiftStateType.None);
                } else {
                    SetShiftState((ShiftStateType)((int)ShiftState + 1));
                }
            } else {
                if (CharSet == CharSetType.Symbols1) {
                    SetCharSet(CharSetType.Symbols2);
                } else {
                    SetCharSet(CharSetType.Symbols1);
                }
            }
        }

        void ShowPressPopup(KeyViewModel kvm, Touch touch) {
            if (!kvm.CanShowPopup) {
                return;
            }
            kvm.ClearPopups();
            kvm.AddPopupAnchor(0, 0, kvm.CurrentChar);
            kvm.PressPopupShowDt = DateTime.Now;
            kvm.FitPopupInFrame(touch);
        }
        void ShowHoldPopup(KeyViewModel kvm, Touch touch) {
            if(!kvm.CanShowPopup) {
                return;
            }
            if (IsHoldMenuVisible && kvm != null && !kvm.HasHoldPopup) {
                return;
            }
            kvm.ClearPopups();

            if (kvm == null ||
                !kvm.HasHoldPopup) {
                return;
            }
            var chars = kvm.SecondaryCharacters.ToList();
            if (kvm.IsRightSideKey) {
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
            if (Touches.Locate(touchId) is not { } touch) {
                return;
            }
            IsCursorControlEnabled = true;
            LastCursorControlUpdateLocation = touch.Location;
            Renderer.Render(true);
        }
        void StopCursorControl(Touch touch) {
            IsCursorControlEnabled = false;
            LastCursorControlUpdateLocation = null;
            if (SpacebarKey is { } sb_kvm) {
                sb_kvm.SetPressed(false, touch);
            }
            Renderer.Render(true);
        }
        void UpdateCursorControl(Touch touch) {
            double x = touch.Location.X;
            double y = touch.Location.Y;
            var lp = LastCursorControlUpdateLocation ?? touch.Location;
            double lx = lp.X;
            double ly = lp.Y;

            double x_factor = ActualScaling * 7;
            double y_factor = ActualScaling * 10;

            int max_x = 5;
            int max_y = 5;

            int dx = (int)Math.Floor((x - lx) / x_factor);
            int dy = (int)Math.Floor((y - ly) / y_factor);

            dy = 0; // too volatile for now

            if (dx == 0 && dy == 0) {
                return;
            }
            dx = Math.Clamp(dx, -max_x, max_x);
            dy = Math.Clamp(dy, -max_y, max_y);

            Debug.WriteLine($"DX: {dx} DY: {dy}");

            LastCursorControlUpdateDt = DateTime.Now;
            InputConnection.OnNavigate(dx, dy);

            // ensure when updated last pos is actual position
            LastCursorControlUpdateLocation = touch.Location;
        }
        #endregion

        #region Key Pull
        public void UpdatePull(Touch touch) {
            var pkvm = GetPressedKeyForTouch(touch);
            if (pkvm == null ||
                !pkvm.CanPullKey) {
                return;
            }
            if (!pkvm.KeyboardRect.Contains(touch.Location)) {
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
        bool CheckCanCursorControlBeEnabled(Touch touch, KeyViewModel kvm, bool isHold) {
            if(!kvm.IsSpaceBar ||
                kvm.IsSoftPressed ||
                !CanCursorControlBeEnabled ||
                IsCursorControlEnabled) {
                return false;
            }
            if(isHold) {
                return true;
            }
            return Touches.Dist(touch.Location, touch.PressLocation) >= MinCursorControlDragDist;
        }
        void HandleBackspace() {
            LastBackspaceUpdateDt = DateTime.Now;
            InputConnection.OnBackspace(1);
        }
        async Task PressKeyAsync(Touch touch, bool isSoft = false) {
            if (GetKeyUnderPoint(touch.Location) is not { } kvm) {
                return;
            }
            InputConnection.OnFeedback(KeyReleaseFeedback);
            kvm.SetPressed(true, touch, isSoft);
            int t = 0;
            while (true) {
                if (t == 0 && kvm.HasPressPopup) {
                    ShowPressPopup(kvm, touch);

                } else if (t == MinHoldMs) {
                    if (kvm.HasHoldPopup) {
                        ShowHoldPopup(kvm, touch);
                        return;
                    } else if (CheckCanCursorControlBeEnabled(touch,kvm,true)) {
                        StartCursorControl(touch.Id);
                        return;
                    }
                }
                if (CheckCanCursorControlBeEnabled(touch,kvm,false)) {
                    StartCursorControl(touch.Id);
                    return;
                }
                if (kvm.IsBackspace &&
                    (t == 0 || (t >= BackspaceHoldToRepeatMs && t % MinBackspaceRepeatMs == 0))) {
                    HandleBackspace();
                    //Debug.WriteLine($"Backspace: MinRepeat: {BackspaceHoldToRepeatMs} T: {t}");
                }
                int dt = HoldDelayMs;//Math.Max(1,Math.Min(HoldDelayMs, BackspaceHoldToRepeatMs));
                await Task.Delay(dt);
                t += dt;
                if (!PressedKeys.Contains(kvm)) {
                    return;
                }
            }
        }
        void MoveKey(Touch touch) {
            if (IsCursorControlEnabled) {
                UpdateCursorControl(touch);
                return;
            }
            if (IsPullEnabled) {
                UpdatePull(touch);
            }
            var pressed_kvm = GetPressedKeyForTouch(touch);
            if (IsHoldMenuVisible &&
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

                PressKeyAsync(touch,true).FireAndForgetSafeAsync();
            }
        }
        void SoftReleaseKey(KeyViewModel kvm) {
            if (kvm == null) {
                return;
            }
            kvm.SetPressed(false, null);
        }
        void ReleaseKey(Touch touch) {
            if (IsCursorControlEnabled) {
                StopCursorControl(touch);
                return;
            }
            if (GetPressedKeyForTouch(touch) is not { } pressed_kvm) {
                return;
            }

            PerformKeyAction(pressed_kvm);

            bool is_released = Touches.Locate(touch.Id) == null;
            if (is_released) {
                pressed_kvm.SetPressed(false, null);
                if (pressed_kvm.IsBackspace) {
                    ResetBackspace();
                }
            }
        }

        void PerformKeyAction(KeyViewModel pressed_kvm) {
            var active_kvm = pressed_kvm.ActivePopupKey;
            if (active_kvm == null) {
                active_kvm = pressed_kvm;
            }
            if (active_kvm == null) {
                return;
            }

            switch (pressed_kvm.SpecialKeyType) {
                case SpecialKeyType.Shift:
                    HandleShift();
                    break;
                case SpecialKeyType.SymbolToggle:
                case SpecialKeyType.NumberSymbolsToggle:
                    ToggleSymbolSet();
                    break;
                case SpecialKeyType.Backspace:
                    // handled in press
                    break;
                case SpecialKeyType.Done:
                case SpecialKeyType.Go:
                case SpecialKeyType.Search:
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
                    if (!IsNumPadLayout &&
                        IsDoubleTapSpaceEnabled &&
                        active_kvm.IsSpaceBar &&
                        active_kvm.LastReleaseDt is { } lrdt &&
                        DateTime.Now - lrdt <= TimeSpan.FromMilliseconds(MaxDoubleTapSpaceForPeriodMs)) {
                        InputConnection.OnBackspace(1);
                        pv = ". ";
                    }
                    InputConnection?.OnText(pv);

                    LastInput = pv;

                    if (ShiftState == ShiftStateType.Shift) {
                        SetShiftState(ShiftStateType.None);
                    }
                    if (active_kvm.IsSpaceBar && !IsNumPadLayout) {
                        // after typing space reset to default keyboard                            
                        SetCharSet(CharSetType.Letters);
                    }
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand NextKeyboardCommand => ReactiveCommand.Create(() => {
            if (InputConnection is not IKeyboardInputConnection_ios ic_ios
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

                //var test = KeyboardBuilder.Build(InputConnection, new Size(KeyboardWidth, KeyboardHeight), ScreenScaling, out _);

                //RenderHelpers.RenderToFile(test, @"C:\Users\tkefauver\Desktop\test1.png");


            });
        #endregion
    }
    public class ViewModelBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null, [CallerFilePath] string path = null, [CallerMemberName] string memName = null, [CallerLineNumber] int line = 0) {
            if (PropertyChanged == null ||
                propertyName == null) {
                return;
            }
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
