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
using System.Runtime.InteropServices;
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
        public IEnumerable<KeyViewModel> PopupKeys =>
            Keys.Where(x => x.IsPopupKey);
        public IEnumerable<KeyViewModel> VisiblePopupKeys =>
            PopupKeys.Where(x => x.IsVisible);
        public IEnumerable<KeyViewModel> PressedKeys =>
            Keys
            .Where(x => x != null && x.IsPressed);
        public IEnumerable<KeyViewModel> PopupAnchorKeys =>
            PopupKeys.Select(x => x.PopupAnchorKey).Distinct();
        public KeyViewModel LastPressedKey =>
            PressedKeys.Where(x => x.LastPressDt.HasValue).OrderByDescending(x => x.LastPressDt).FirstOrDefault();
        public KeyViewModel ActiveKeyViewModel =>
            Keys
            .FirstOrDefault(x => x != null && x.IsActiveKey);
        public KeyViewModel SpacebarKey { get; private set; }
        
        IEnumerable<IEnumerable<KeyViewModel>> Rows =>
            Keys.Where(x => x != null && !x.IsPopupKey)
            .OrderBy(x => x.Column)
            .GroupBy(x => x.Row);
        #endregion

        #region Layout
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
            MenuHeight;


        public double PopupOverflowTranslateX {
            get {
                if (LastPressedKey == null ||
                    !PopupKeys.Any()) {
                    return 0;
                }

                // NOTE this calculation needs to match how X is decided in kvm
                // the only difference is it doesn't account for PopupOffset
                double pukl_left = LastPressedKey.X;
                double pukl_right = pukl_left + VisiblePopupKeys.Sum(x => x.Width);
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
            InputConnection is IKeyboardInputConnection_ios;
        public double ScreenScaling { get; set; }
        public string ErrorText { get; private set; } = "NO ERRORS";
        public bool NeedsNextKeyboardButton =>
            //OperatingSystem.IsWindows() ||
            (OperatingSystem.IsIOS() &&
            InputConnection != null &&
            (InputConnection as IKeyboardInputConnection_ios).NeedsInputModeSwitchKey);
        double CursorControlFactorX => 4;
        double CursorControlFactorY => 4;
        public bool IsNumbers { get; private set; }
        KeyboardFlags KeyboardFlags { get; set; }
        bool IsHoldMenuVisible =>
            VisiblePopupKeys.Skip(1).Any();
        int RepeatCount { get; set; }

        int MinHoldMs => 750;
        int MinRepeatMs => 300;
        TimeSpan MinHoldDur => TimeSpan.FromMilliseconds(MinHoldMs);
        TimeSpan MinRepeatDur => TimeSpan.FromMilliseconds(MinRepeatMs);
        public Point? KeyboardPointerLocation { get; private set; }
        public Point? KeyboardPointerDownLocation { get; private set; }
        public CharSetType CharSet { get; set; }
        public ShiftStateType ShiftState { get; set; }
        public IKeyboardInputConnection InputConnection { get; set; }
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
        public void ResetLayout() {
            _defKeyWidth = -1;
            _keyHeight = -1;
            foreach(var kvm in Keys) {
                kvm.ResetLocation();
            }
            UpdateKeyboardState();
        }

        private TouchEventArgs _lastDebouncedTouchEventArgs;

        bool IsTouchBounced(TouchEventArgs e) {
            if(e == null) {
                return true;
            }
            if(_lastDebouncedTouchEventArgs == null) {
                return false;
            }
            if(_lastDebouncedTouchEventArgs.TouchEventType != e.TouchEventType) {
                return false;
            }
            double dist = Touches.Dist(e.Location, _lastDebouncedTouchEventArgs.Location);
            return dist < 5;
        }

        public void SetPointerLocation(TouchEventArgs e) {
            var mp = e.Location;
            var touchType = e.TouchEventType;
            if(Touches.Update(mp,touchType) is not { } touch) {
                return;
            }

            //if (IsTouchBounced(e)) {
            //    return;
            //}
            //_lastDebouncedTouchEventArgs = e;

            KeyViewModel touch_kvm = default;

            if(IsCursorControlEnabled) {
                touch_kvm = SpacebarKey;
            } else {
                touch_kvm = GetKeyUnderPoint(mp);
            }
            Debug.WriteLine($"Event: '{touchType}' Id: {touch.Id} Key: '{touch_kvm}'");

            if (Touches.Primary is { } pt) {
                KeyboardPointerLocation = pt.Location;
            } else {
                KeyboardPointerLocation = null;
            }
            if(KeyboardPointerDownLocation == null || KeyboardPointerLocation == null) {
                // set down on press and clear on release
                KeyboardPointerDownLocation = KeyboardPointerLocation;
            }
            var pressed_kvm = GetPressedKeyForTouch(touch);
            switch(touchType) {
                case TouchEventType.Press:
                    PressKey(touch_kvm).ConfigureAwait(false);
                    MoveKey(touch_kvm, touch);
                    break;
                case TouchEventType.Move:
                    UpdatePull(touch);
                    if (IsHoldMenuVisible) {
                        MoveKey(pressed_kvm,touch);
                        break;
                    } 
                    if(IsCursorControlEnabled) {
                        UpdateCursorControl();
                        break;
                    }

                    if(pressed_kvm != touch_kvm &&
                        touch_kvm != null && 
                        pressed_kvm.Row + 1 != touch_kvm.Row) {
                        // when key is pressed and this is its
                        // associated touch but the touch isn't over the key
                        
                        // soft release it
                        ReleaseKey(pressed_kvm,false, false);

                        PressKey(touch_kvm).ConfigureAwait(false);
                    }
                    break;
                case TouchEventType.Release:
                    var to_release = touch_kvm;
                    if(to_release == null || 
                        !to_release.IsPressed) {
                        to_release = pressed_kvm;
                    }
                    if(to_release == null) {
                        break;
                    }
                    ReleaseKey(to_release,false,true);
                    break;
            }
            UpdateKeyboardState();
        }
        KeyViewModel GetPressedKeyForTouch(Touch touch) {
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
            IsNumbers = KeyboardFlags.HasFlag(KeyboardFlags.Numbers);
            MaxColCount = 0;

            var keyRows = GetKeyRows(KeyboardFlags);
            RowCount = keyRows.Count;
            for(int r = 0; r < keyRows.Count; r++)
            {
                MaxColCount = Math.Max(MaxColCount, keyRows[r].Count);
                KeyViewModel prev_kvm = null;
                for(int c = 0; c < keyRows[r].Count; c++)
                {
                    var keyObj = keyRows[r][c];
                    int cur_col = prev_kvm == null ? 0 : prev_kvm.Column + prev_kvm.ColumnSpan;
                    var kvm = CreateKeyViewModel(keyRows[r][c], r, cur_col,prev_kvm);
                    Keys.Add(kvm);
                    prev_kvm = kvm;
                    if(kvm.IsSpaceBar) {
                        SpacebarKey = kvm;
                    }
                }
            }
            int max_popup_keys = Keys.Max(x => x.SecondaryCharacters.Count());
            MaxPopupRowCount = (int)Math.Floor((double)max_popup_keys / (double)(MaxPopupColCount));

            KeyViewModel prev_pukvm = null;
            int idx = 0;
            for (int r = 0; r < MaxPopupRowCount; r++) {
                for (int c = 0; c < MaxPopupColCount; c++) {
                    var pukvm = CreatePopUpKeyViewModel(idx++, r,c, prev_pukvm);
                    Keys.Add(pukvm);
                    prev_pukvm = pukvm;
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
                key.RaisePropertyChanged(nameof(key.CornerRadius));
                key.RaisePropertyChanged(nameof(key.NeedsOuterTranslate));
                if(key.NeedsOuterTranslate) {

                }
                //Debug.WriteLine(key.PrimaryValue);
            }
        }


        void ShowPopUpKeyViewModel(KeyViewModel source_kvm, int r, int c, string disp_val) {
            if (source_kvm.IsSpecial ||
                PopupKeys.FirstOrDefault(x=>x.Row == r && x.Column == c) is not { } pukvm) {
                return;
            }

            pukvm.SetPopupAnchor(source_kvm, disp_val);
            if(pukvm.IsVisible) {
                source_kvm.VisiblePopupColCount = Math.Max(c + 1, source_kvm.VisiblePopupColCount);
                source_kvm.VisiblePopupRowCount = Math.Max(r + 1, source_kvm.VisiblePopupRowCount);
            }
        }
        KeyViewModel CreatePopUpKeyViewModel(int idx, int r, int c, KeyViewModel prev) {
            var pu_kvm = CreateKeyViewModel(null, r, c, prev);
            pu_kvm.PopupKeyIdx = idx;
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
        List<List<object>> GetKeyRows(KeyboardFlags kbFlags) {
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
            ResetLayout();
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

        void ClearHoldKeys(KeyViewModel kvm) {
            kvm.VisiblePopupColCount = 0;
            kvm.VisiblePopupRowCount = 0;
            var to_remove = PopupKeys.Where(x => x.PopupAnchorKey == kvm).ToList();
            foreach (var rmv in to_remove) {
                rmv.RemovePopupAnchor();
            }
        }
        void ShowPressPopup(KeyViewModel kvm) {
            ClearHoldKeys(kvm);
            ShowPopUpKeyViewModel(kvm, 0, 0, kvm.CurrentChar);
            UpdateKeyboardState();
        }
        void ShowHoldPopup(KeyViewModel kvm) {
            if(IsHoldMenuVisible && kvm != null && !kvm.HasHoldPopup) {
                return;
            }
            ClearHoldKeys(kvm);
            if(kvm != null) {
                if(kvm.HasHoldPopup) {
                    var chars = kvm.SecondaryCharacters.ToList();
                    int idx = 0;
                    for (int r = 0; r < MaxPopupRowCount; r++) {
                        for (int c = 0; c < MaxPopupColCount; c++) {
                            string pv = string.Empty;
                            if(idx < chars.Count) {
                                // visible popup
                                pv = chars[idx];
                            } else if(r == 0) {
                                break;
                            }
                            ShowPopUpKeyViewModel(kvm, r, c, pv);
                            idx++;
                        }
                        if(idx >= chars.Count) {
                            break;
                        }
                    }
                }                
            }
            KeyboardGridView.DebugCanvas.InvalidateVisual();
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
            //UpdateKeyboardState();
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
        public void UpdatePull(Touch touch) {
            var pkvm = GetPressedKeyForTouch(touch);
            if(pkvm == null ||
                !pkvm.CanPullKey) {
                return;
            }
            if(!pkvm.Rect.Contains(touch.Location)) {
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

        async Task PressKey(KeyViewModel kvm) {
            if(kvm == null) {
                return;
            }
            kvm.SetPressed(true);
            int delay_ms = 20;
            int t = 0;
            var touch_center = kvm.Rect.Center;
            while (true) {
                if (t == 0 && kvm.HasPressPopup) {
                    ShowPressPopup(kvm);
                } else if (t == MinHoldMs) {
                    if (kvm.HasHoldPopup) {
                        ShowHoldPopup(kvm);
                    } else if(kvm.IsSpaceBar) {
                        StartCursorControl();
                    }
                }
                if (kvm.CanRepeat && t % MinRepeatMs == 0) {
                    int del_count = RepeatCount + RepeatCount + 1;
                    Debug.WriteLine($"Repeat Count: {RepeatCount} Del Count: {del_count}");
                    for (int i = 0; i < del_count; i++) {
                        ReleaseKey(kvm, true,true);
                    }
                    RepeatCount++;
                }
                await Task.Delay(delay_ms);
                t += delay_ms;
                if (!PressedKeys.Contains(kvm)) {
                    return;
                }
            }
        }
        void MoveKey(KeyViewModel kvm, Touch touch) {
            if(kvm == null) {
                return;
            }
            kvm.UpdateActive(touch);
        }
        void ReleaseKey(KeyViewModel kvm, bool isRepeat, bool performAction) {
            if(kvm == null) {
                return;
            }
            if(!performAction) {
                kvm.SetPressed(false);
                UpdateKeyboardState();
                return;
            }
            var active_kvm = kvm.ActivePopupKey;
            if (active_kvm == null) {
                active_kvm = kvm;
            }
            if (active_kvm == null) {                      
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
                    string pv = active_kvm.PrimaryValue;
                    if(active_kvm.IsPopupKey && active_kvm.PopupAnchorKey is { } anchor_kvm &&
                        anchor_kvm.IsPulled) {
                        // release comes from active not pressed
                        // when pulled don't care whats active just use secondary
                        pv = anchor_kvm.SecondaryValue;
                        anchor_kvm.PullTranslateY = 0;
                    }
                    InputConnection?.OnText(pv);

                    if (ShiftState == ShiftStateType.Shift) {
                        ShiftState = ShiftStateType.None;
                    }
                    if(active_kvm.IsSpaceBar && !IsNumbers) {
                        // after typing space reset to default keyboard
                        CharSet = CharSetType.Letters;
                    }
                    break;
            }
            if(active_kvm.CanRepeat && !isRepeat) {
                RepeatCount = 0;
            }

            if (!isRepeat) {
                kvm.SetPressed(false);
                ClearHoldKeys(kvm);
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
