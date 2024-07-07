using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        KeyViewModel PopupKeyParent =>
            IsPopupKey ? Parent.PressedKeyViewModel : null;
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
            IsShifted && IsInput ? CurrentChar.ToUpper() : CurrentChar;
        public bool IsVisible =>
            !string.IsNullOrEmpty(PrimaryValue);
        #endregion

        #region Layout

        #region Factors
        double DefaultOuterPadX =>
            Math.Min(5, Parent.DefaultKeyWidth / Parent.MaxColCount);
        double OuterPadX =>
            IsPopupKey ? 0 : DefaultOuterPadX;
        double OuterPadY => 
            IsPopupKey ? 
                0 : 
                Parent.KeyHeight * 0.15;
        double PopupOffsetX => 0;
        double PopupOffsetY => 0;
        double PrimaryFontSizeRatio => 0.5;
        double SecondaryFontSizeRatio => 0.25;
        double SecondaryRatio => 0.25;
        string[] MisAlignedCharacters => [
            //"✖️",
            "♠️",
            "♣️"
            ];

        public double RadiusX => 5;
        public double RadiusY => 5;
        #endregion
        public double PrimaryFontSize =>
            Math.Min(InnerWidth, InnerHeight) * PrimaryFontSizeRatio;
        public double SecondaryFontSize =>
            Math.Min(InnerWidth, InnerHeight) * SecondaryFontSizeRatio;
        public double X {
            get {
                if(PrevKeyViewModel == null) {
                    if(IsPopupKey && PopupKeyParent is { } pressed_kvm) {
                        return pressed_kvm.X + PopupOffsetX;
                    }
                    if(NeedsOuterTranslate) {
                        return OuterTranslateX;
                    }
                    return 0;
                }
                return PrevKeyViewModel.X + PrevKeyViewModel.Width;
            }
        }

        public double Y {
            get {
                if(IsPopupKey && PopupKeyParent is { } pressed_kvm) {
                    return pressed_kvm.Y - pressed_kvm.Height + PopupOffsetY;
                }
                if(!Row.HasValue) {
                    return double.MinValue;
                }
                return Row.Value * Height;
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
        public int? Row { get; set; }
        public int? Column { get; set; }
        public int ColumnSpan { get; set; } = 1;
        #endregion

        #region State
        public bool CanRepeat =>
            SpecialKeyType == SpecialKeyType.Backspace;
        bool IsSpaceKey =>
            PrimaryValue == " ";
        public bool HasAnyPopup =>
            HasPressPopup || HasHoldPopup;
        public bool HasPressPopup =>
            IsInput && !IsSpaceKey;
        public bool HasHoldPopup =>
            SecondaryCharacters.Any();
        public bool IsActiveKey { 
            get {
                if(IsPressed) {
                    if(Parent != null && Parent.PopupKeys.Any()) {
                        // popupkeys take active for input
                        return false;
                    }
                    return true;
                }
                if(IsPopupKey &&
                    Parent.KeyboardPointerLocation is { } p &&
                    Parent.PopupKeys is { } pu_kvml) {
                    // NOTE this presumes theres only ONE line of popup chars

                    if(pu_kvml.FirstOrDefault() == this) {
                        // first popup
                        return p.X <= X + Width;
                    }
                    if(pu_kvml.LastOrDefault() == this) {
                        // last popup
                        return p.X >= X;
                    }
                    //some other
                    return p.X >= X && p.X <= X + Width;
                }
                return false;
            } 
        }
        public DateTime? PressedDt { get; private set; }
        public bool IsPressed { get; set; }
        public bool IsPopupKey =>
            PopupKeyIdx >= 0;
        public int PopupKeyIdx { get; set; } = -1;
        public bool NeedsOuterTranslate { get; set; }
        public bool NeedsSymbolTranslate =>
            MisAlignedCharacters.Contains(PrimaryValue);
        public bool IsSpecial =>
            SpecialKeyType != SpecialKeyType.None;
        public bool IsInput =>
            !IsSpecial;
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
            PropertyChanged += KeyViewModel_PropertyChanged;
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

        private void KeyViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsPressed):
                    PressedDt = IsPressed ? DateTime.Now : null;
                    if(!IsPressed) {

                    }
                    break;
            }
        }
        
        #endregion

        #region Commands

        #endregion
    }
}
