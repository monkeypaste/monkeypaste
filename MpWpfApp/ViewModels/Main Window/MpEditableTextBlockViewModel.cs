using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpEditableTextBlockViewModel : MpViewModelBase {
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

        public Visibility TextBlockVisibility {
            get {
                if (IsTextBoxFocused) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public Visibility TextBoxVisibility {
            get {
                if (IsTextBoxFocused) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
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

        private Brush _foregroundBrush = Brushes.Black;
        public Brush ForegroundBrush {
            get {
                return _foregroundBrush;
            }
            set {
                //omitting duplicate check to enforce change in ui
                if (_foregroundBrush != value) {
                    _foregroundBrush = value;
                    OnPropertyChanged(nameof(ForegroundBrush));
                }
            }
        }

        private Brush _backgroundBrush = Brushes.Black;
        public Brush BackgroundBrush {
            get {
                return _backgroundBrush;
            }
            set {
                //omitting duplicate check to enforce change in ui
                if (_backgroundBrush != value) {
                    _backgroundBrush = value;
                    OnPropertyChanged(nameof(BackgroundBrush));
                }
            }
        }

        public bool HasText {
            get {
                return Text.Length > 0;
            }
        }
        #endregion

        #region Public Methods
        public MpEditableTextBlockViewModel() : base() { }

        public void EditableTextBlockViewModel_Loaded(object sender, RoutedEventArgs args) {
            
        }
        #endregion

        #region Commands

        #endregion
    }
}
