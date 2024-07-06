using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
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
        
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public KeyboardViewModel Parent { get; set; }
        public KeyViewModel PrevKeyViewModel { get; set; }
        #endregion

        #region Appearance
        public string CurrentChar {
            get
            {
                if(!Characters.Any())
                {
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
            IsShifted ? CurrentChar.ToUpper() : CurrentChar;
        public bool IsVisible =>
            !string.IsNullOrEmpty(PrimaryValue);
        #endregion

        #region Layout

        #region Factors
        double OuterPadX => IsHoldKey ? Parent.DefaultKeyWidth * 0.01 : Parent.DefaultKeyWidth * 0.2;
        double OuterPadY => IsHoldKey ? 0 : Parent.KeyHeight * 0.15;
        double PrimaryFontSizeRatio => 0.5;
        double SecondaryFontSizeRatio => 0.25;
        double SecondaryRatio => 0.25;
        string[] MisAlignedCharacters => [
            //"✖️",
            "♠️",
            "♣️"
            ];

        #endregion
        public double RadiusX => 5;
        public double RadiusY => 5;
        public double PrimaryFontSize =>
            Math.Min(InnerWidth, InnerHeight) * PrimaryFontSizeRatio;
        public double SecondaryFontSize =>
            Math.Min(InnerWidth, InnerHeight) * SecondaryFontSizeRatio;
        private double? _x;
        public double X {
            get {
                if(_x.HasValue) {
                    return _x.Value;
                }
                return PrevKeyViewModel == null ?
                0 :
                PrevKeyViewModel.X + PrevKeyViewModel.Width;
            }
            set {
                if(X != value) {
                    _x = value;
                    this.RaisePropertyChanged(nameof(X));
                }
            }
        }
        private double? _y;
        public double Y {
            get {
                if(_y.HasValue) {
                    return _y.Value;
                }
                return Row * Height;
            }
            set {
                if (Y != value) {
                    _y = value;
                    this.RaisePropertyChanged(nameof(Y));
                }
            }
        }

        public double Width =>
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
        public bool IsHoldFocusKey { get; set; }
        public bool IsHolding { get; set; }
        public bool IsHoldKey { get; set; }
        public bool NeedsOuterTranslate { get; set; }
        public bool NeedsSymbolTranslate =>
            MisAlignedCharacters.Contains(PrimaryValue);
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

        int CharIdx =>
            (int)CharSet >= Characters.Count ? 0 : (int)CharSet;
        #endregion

        #region Model
        public SpecialKeyType SpecialKeyType { get; set; }
        public ObservableCollection<string> SecondaryCharacters { get; set; } = ["A","B","C"];
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
            return $"{PrimaryValue} X:{(int)X} Y:{(int)Y} W:{(int)Width} H:{(int)Height}";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        
        #endregion

        #region Commands
        
        #endregion
    }
}
