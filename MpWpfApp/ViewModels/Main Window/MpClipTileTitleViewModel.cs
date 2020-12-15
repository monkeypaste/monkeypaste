using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpClipTileTitleViewModel : MpViewModelBase {
        #region Properties
        //public BitmapSource TitleSwirl {
        //    get {
        //        return CopyItem.ItemTitleSwirl;
        //    }
        //    set {
        //        if (CopyItem.ItemTitleSwirl != value) {
        //            CopyItem.ItemTitleSwirl = value;
        //            OnPropertyChanged(nameof(TitleSwirl));
        //            OnPropertyChanged(nameof(CopyItem));
        //        }
        //    }
        //}

        //public BitmapSource CopyItemAppIcon {
        //    get {
        //        return CopyItem.App.Icon.IconImage;
        //    }
        //}

        //public string CopyItemAppName {
        //    get {
        //        return CopyItem.App.AppName;
        //    }
        //}

        //public Brush TitleColor {
        //    get {
        //        return new SolidColorBrush(CopyItem.ItemColor.Color);
        //    }
        //    set {
        //        if (CopyItem.ItemColor.Color != ((SolidColorBrush)value).Color) {
        //            CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
        //            //CopyItem.ItemTitleSwirl = CopyItem.InitSwirl();
        //            OnPropertyChanged(nameof(TitleColor));
        //            OnPropertyChanged(nameof(CopyItem));
        //        }
        //    }
        //}

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

        #endregion

        #region Commands

        #endregion
    }
}
