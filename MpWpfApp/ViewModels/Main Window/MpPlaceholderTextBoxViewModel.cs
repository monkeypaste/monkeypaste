using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpPlaceholderTextBoxViewModel : MpViewModelBase {
        #region Private Variables

        #endregion

        #region Properties     

        private double _width = 125;
        public double Width {
            get {
                return _width;
            }
            set {
                if (_width != value) {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        private double _height = 25;
        public double Height {
            get {
                return _height;
            }
            set {
                if (_height != value) {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        private double _fontSize = 14;
        public double FontSize {
            get {
                return _fontSize;
            }
            set {
                if (_fontSize != value) {
                    _fontSize = value;
                    OnPropertyChanged(nameof(FontSize));
                }
            }
        }

        private double _cornerRadius = 10;
        public double CornerRadius {
            get {
                return _cornerRadius;
            }
            set {
                if (_cornerRadius != value) {
                    _cornerRadius = value;
                    OnPropertyChanged(nameof(CornerRadius));
                }
            }
        }

        private string _placeholderText = "Placeholder Text";
        public string PlaceholderText {
            get {
                return _placeholderText;
            }
            set {
                if (_placeholderText != value) {
                    _placeholderText = value;
                    OnPropertyChanged(nameof(PlaceholderText));
                }
            }
        }

        private string _text = string.Empty;
        public string Text {
            get {
                return _text;
            }
            set {
                if (_text != value) {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                    OnPropertyChanged(nameof(HasText));
                    OnPropertyChanged(nameof(ClearTextButtonVisibility));
                    OnPropertyChanged(nameof(TextBoxFontStyle));
                    OnPropertyChanged(nameof(TextBoxBorderBrush));
                }
            }
        }

        private bool _isTextBoxFocused = false;
        public bool IsTextBoxFocused {
            get {
                return _isTextBoxFocused;
            }
            set {
                //omitting duplicate check to enforce change in ui
                if (_isTextBoxFocused != value) {
                    _isTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsTextBoxFocused));
                }
            }
        }

        private bool _isTextValid = true;
        public bool IsTextValid {
            get {
                return _isTextValid;
            }
            set {
                //omitting duplicate check to enforce change in ui
                if (_isTextValid != value) {
                    _isTextValid = value;
                    OnPropertyChanged(nameof(IsTextValid));
                    OnPropertyChanged(nameof(TextBoxBorderBrush));
                }
            }
        }

        public bool HasText {
            get {
                return Text.Length > 0 && Text != PlaceholderText;
            }
        }

        public Brush TextBoxBorderBrush {
            get {
                if (IsTextValid) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        }

        public SolidColorBrush TextBoxTextBrush {
            get {
                if (HasText) {
                    return Brushes.Black;
                }
                return Brushes.DimGray;
            }
        }

        public FontStyle TextBoxFontStyle {
            get {
                if (HasText) {
                    return FontStyles.Normal;
                }
                return FontStyles.Italic;
            }
        }

        public Visibility ClearTextButtonVisibility {
            get {
                if (HasText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Public Methods
        public MpPlaceholderTextBoxViewModel() : base() { }

        public void PlaceholderTextBoxBorder_Loaded(object sender, RoutedEventArgs args) {
            var tb = (TextBox)((MpClipBorder)sender).FindName("PlaceholderTextBox");
            tb.GotFocus += (s, e4) => {
                if (!HasText) {
                    Text = string.Empty;
                }

                IsTextBoxFocused = true;
                MainWindowViewModel.ClipTrayViewModel.ResetClipSelection();
                OnPropertyChanged(nameof(TextBoxFontStyle));
                OnPropertyChanged(nameof(TextBoxTextBrush));
            };
            tb.LostFocus += (s, e5) => {
                IsTextBoxFocused = false;
                if (!HasText) {
                    Text = Properties.Settings.Default.SearchPlaceHolderText;
                }
            };
            if(string.IsNullOrEmpty(Text)) {
                Text = Properties.Settings.Default.SearchPlaceHolderText;
            }
        }
        #endregion

        #region Commands
        private RelayCommand _clearTextCommand;
        public ICommand ClearTextCommand {
            get {
                if (_clearTextCommand == null) {
                    _clearTextCommand = new RelayCommand(ClearText, CanClearText);
                }
                return _clearTextCommand;
            }
        }
        private bool CanClearText() {
            return Text.Length > 0;
        }
        private void ClearText() {
            Text = string.Empty;
            IsTextBoxFocused = true;
        }
        #endregion
    }
}
