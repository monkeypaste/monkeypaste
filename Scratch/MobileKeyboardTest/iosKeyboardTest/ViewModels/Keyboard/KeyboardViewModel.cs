using Avalonia;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

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

        public static MyKeyboardView CreateKeyboardView(IKeyboardInputConnection inputConn, Size screenSize) {
            var kbvm = new KeyboardViewModel(inputConn, screenSize);
            var kbv = new MyKeyboardView() {
                DataContext = kbvm,
                Width = kbvm.Width,
                Height = kbvm.Height
            };
            return kbv;
        }
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
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
                    _defKeyWidth = Width / Rows.Max(x => x.Count());

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
                    _keyHeight = Height / Rows.Count();
                }
                return _keyHeight;
            }
        }

        double SpecialKeyWidthRatio => 1.5d;
        public double SpecialKeyWidth =>
            DefaultKeyWidth * SpecialKeyWidthRatio;

        public double Width =>
            ScreenSize.Width;
        public double Height =>
            ScreenSize.Height * KEYBOARD_SCREEN_HEIGHT_RATIO;

        #endregion

        #region State
        public CharSetType CharSet { get; set; }
        public ShiftStateType ShiftState { get; set; }
        public IKeyboardInputConnection InputConnection { get; private set; }
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
            foreach (var row in Rows) {
                // center middle row for non-symbol char set
                bool needs_trans = row.Any(x => !x.IsVisible);
                foreach (var key in row) {
                    key.NeedsOuterTranslate = needs_trans;
                }
            }

            foreach (var key in Keys) {
                key.RaisePropertyChanged(nameof(key.DisplayValue));
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
                Console.Write(key.DisplayValue);
            }
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void Init()
        {
            var keys = new List<List<object>>
            {//♠ ♡ ♦ ♤ ♣♧
                (["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"]),
                (["q,+,`", "w,×,~", "e,÷,\\", "r,=,|", "t,/,{", "y,_,}", "u,<,€", "i,>,£", "o,[,¥", "p,],₩"]),
                (["a,!,○", "s,@,•", "d,#,⚪", "f,$,⚫", "g,%,□", "h,^,🔳", "j,&,♤", "k,*,♡", "l,(,♢", "none,),♧"]),
                ([SpecialKeyType.Shift, "z,-,☆", "x,',▪", "c,\",▫", "v,:,≪", "b,;,≫", "n,comma,¡", "m,?,¿", SpecialKeyType.Backspace]),
                ([SpecialKeyType.SymbolToggle, "comma", " ", ".", SpecialKeyType.Enter])
            };
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

        
        #endregion

        #region Commands

        

        #endregion
    }
}
