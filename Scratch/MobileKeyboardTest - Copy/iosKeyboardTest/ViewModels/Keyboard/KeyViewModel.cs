using Avalonia;

using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        static double Pad => 20;
        static string[] MisAlignedCharacters => [
            //"✖️",
            "♠️",
            "♣️"
            ];

        
        
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        KeyboardViewModel Parent { get; set; }
        KeyViewModel PrevKeyViewModel { get; set; }
        #endregion

        #region Appearance
        public string CurrentChar {
            get
            {
                if(!Characters.Any())
                {
                    return string.Empty;
                }
                int idx = (int)CharSet >= Characters.Count ? 0 : (int)CharSet;
                return Characters[idx] ?? string.Empty;
            }
        }

        public string DisplayValue =>
            IsShifted ? CurrentChar.ToUpper() : CurrentChar;
        public bool IsVisible =>
            !string.IsNullOrEmpty(DisplayValue);
        #endregion

        #region Layout
        public double X =>
            PrevKeyViewModel == null ?
                0 :
                PrevKeyViewModel.X + PrevKeyViewModel.Width;

        public double Y =>
            Row * Height;

        public double Width =>
            ColumnSpan * (SpecialKeyType == SpecialKeyType.None ?
                Parent.DefaultKeyWidth :
                Parent.SpecialKeyWidth);
        public double Height =>
            Parent.KeyHeight;
        public double InnerWidth =>
            Width - Pad;
        public double InnerHeight =>
            Height - Pad;
        //public Rect Bounds => new Rect(X, Y, Width, Height);
        public double OuterTranslateX =>
            NeedsOuterTranslate && IsVisible ?
                Parent.DefaultKeyWidth / 2 : 0;
        public int Row { get; set; }
        public int Column { get; set; }
        public int ColumnSpan { get; set; } = 1;
        #endregion

        #region State
        public bool NeedsOuterTranslate { get; set; }
        public bool NeedsSymbolTranslate =>
            MisAlignedCharacters.Contains(DisplayValue);
        public bool IsSpecial =>
            SpecialKeyType != SpecialKeyType.None;
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
        #endregion

        #region Model
        public SpecialKeyType SpecialKeyType { get; set; }
        public ObservableCollection<string> Characters { get; set; } = [];
        #endregion

        #endregion

        #region Constructors
        public KeyViewModel(KeyboardViewModel parent, KeyViewModel prev)
        {
            Parent = parent;
            PrevKeyViewModel = prev;
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            return $"{DisplayValue} X:{(int)X} Y:{(int)Y} W:{(int)Width} H:{(int)Height}";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
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
        #endregion

        #region Commands
        public ICommand KeyTapCommand => ReactiveCommand.Create<object>((args) => {
            if (Parent == null ||
                args is not MyKeyView c) {
                return;
            }
            switch (SpecialKeyType) {
                case SpecialKeyType.Shift:
                    HandleShift();
                    break;
                case SpecialKeyType.SymbolToggle:
                    ToggleSymbolSet();
                    break;
                case SpecialKeyType.Backspace:
                    Parent.InputConnection?.OnDelete();
                    break;
                case SpecialKeyType.Enter:
                    Parent.InputConnection?.OnDone();
                    break;
                default:
                    Parent.InputConnection?.OnText(DisplayValue);
                    break;
            }
            try {
                Parent.RefreshKeyboardState();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine($"Tapped {CurrentChar}");
        });
        public ICommand KeyHoldCommand => ReactiveCommand.Create<object>((args) => {
            if (Parent == null ||
                args is not MyKeyView b || 
                b.DataContext is not KeyViewModel kvm) {
                return;
            }
            Parent.RefreshKeyboardState();
            Console.WriteLine($"Hold {kvm.CurrentChar}");
        });
        #endregion
    }
}
