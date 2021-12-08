using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpContextMenuItemViewModel : MpViewModelBase<object> {
        #region Properties
        private bool _isSeparator = false;
        public bool IsSeparator {
            get {
                return _isSeparator;
            }
            set {
                if (_isSeparator != value) {
                    _isSeparator = value;
                    OnPropertyChanged(nameof(IsSeparator));
                }
            }
        }

        private bool? _isChecked = null;
        public bool? IsChecked {
            get {
                return _isChecked;
            }
            set {
                if (_isChecked != value) {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        public bool IsCheckable {
            get {
                return IsChecked.HasValue;
            }
        }

        private Visibility _menuItemVisibility = Visibility.Visible;
        public Visibility MenuItemVisibility {
            get {
                return _menuItemVisibility;
            }
            set {
                if (_menuItemVisibility != value) {
                    _menuItemVisibility = value;
                    OnPropertyChanged(nameof(MenuItemVisibility));
                }
            }
        }

        private string _header;
        public string Header {
            get {
                return _header;
            }
            set {
                if (_header != value) {
                    _header = value;
                    OnPropertyChanged(nameof(Header));
                }
            }
        }

        private ICommand _command;
        public ICommand Command {
            get {
                return _command;
            }
            set {
                if (_command != value) {
                    _command = value;
                    OnPropertyChanged(nameof(Command));
                }
            }
        }

        private object _commandParameter = null;
        public object CommandParameter {
            get {
                return _commandParameter;
            }
            set {
                if (_commandParameter != value) {
                    _commandParameter = value;
                    OnPropertyChanged(nameof(CommandParameter));
                }
            }
        }

        private string _inputGestureText;
        public string InputGestureText {
            get {
                return _inputGestureText;
            }
            set {
                if (_inputGestureText != value) {
                    _inputGestureText = value;
                    OnPropertyChanged(nameof(InputGestureText));
                }
            }
        }

        private string _iconSource;
        public string IconSource {
            get {
                return _iconSource;
            }
            set {
                if(_iconSource != value) {
                    _iconSource = value;
                    OnPropertyChanged(nameof(IconSource));
                }
            } 
        }

        private Brush _iconBackgroundBrush = Brushes.Transparent;
        public Brush IconBackgroundBrush {
            get {
                return _iconBackgroundBrush;
            }
            set {
                if(_iconBackgroundBrush != value) {
                    _iconBackgroundBrush = value;
                    OnPropertyChanged(nameof(IconBackgroundBrush));
                }
            }
        }

        private Image _icon = null;
        public Image Icon {
            get {
                return _icon;
            }
            set {
                if(_icon != value) {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        private ObservableCollection<MpContextMenuItemViewModel> _subItems = new ObservableCollection<MpContextMenuItemViewModel>();
        public ObservableCollection<MpContextMenuItemViewModel> SubItems {
            get {
                return _subItems;
            }
            set {
                if(_subItems != value) {
                    _subItems = value;
                    OnPropertyChanged(nameof(SubItems));
                }
            }
        }
        #endregion

        #region Public Methods

        public MpContextMenuItemViewModel() : base(null)  {
            PropertyChanged += MpContextMenuItemViewModel_PropertyChanged;
            IsSeparator = true;
        }

        
        public MpContextMenuItemViewModel(
            string header, 
            ICommand command,
            object commandParameter,
            bool? isChecked,
            string iconSource = "",
            ObservableCollection<MpContextMenuItemViewModel> subItems = null,
            string inputGestureText = "",
            Brush bgBrush = null) : this() {
            IsSeparator = false;

            Header = header;
            Command = command;
            CommandParameter = commandParameter;
            IsChecked = isChecked;
            IconSource = iconSource;
            SubItems = subItems ?? new ObservableCollection<MpContextMenuItemViewModel>();
            InputGestureText = inputGestureText;
            IconBackgroundBrush = bgBrush == null ? Brushes.Transparent : bgBrush;
        }
        #endregion

        #region Private Methods

        private void MpContextMenuItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IconSource):
                    if (!string.IsNullOrEmpty(IconSource)) {
                        var icon = new Image();
                        if (IconSource.Length <= MpPreferences.Instance.MaxFilePathCharCount && File.Exists(IconSource)) {
                            icon.Source = (BitmapSource)new BitmapImage(new Uri(IconSource));

                        } else {
                            icon.Source = IconSource.ToBitmapSource();
                            //icon.Height = icon.Width = 20;
                        }
                        Icon = icon;
                        Icon.Stretch = Stretch.Fill;
                    }
                    break;
                case nameof(IconBackgroundBrush):
                    if (IconBackgroundBrush != null) {
                        var bgBmp = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/texture.png"));
                        bgBmp = MpHelpers.Instance.TintBitmapSource(bgBmp, ((SolidColorBrush)IconBackgroundBrush).Color, false);
                        var borderBmp = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/textureborder.png"));
                        if (!MpHelpers.Instance.IsBright((IconBackgroundBrush as SolidColorBrush).Color)) {
                            borderBmp = MpHelpers.Instance.TintBitmapSource(borderBmp, Colors.White, false);
                        }
                        var icon = new Image();
                        icon.Source = MpHelpers.Instance.MergeImages(new List<BitmapSource> { bgBmp, borderBmp });
                        if (!IsChecked.HasValue || IsChecked.Value) {
                            string checkPath = !IsChecked.HasValue ? @"/Images/check_partial.png" : @"/Images/check.png";
                            var checkBmp = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + checkPath));
                            if (!MpHelpers.Instance.IsBright((IconBackgroundBrush as SolidColorBrush).Color)) {
                                checkBmp = MpHelpers.Instance.TintBitmapSource(checkBmp, Colors.White, false);
                            }
                            icon.Source = MpHelpers.Instance.MergeImages(new List<BitmapSource> { (BitmapSource)icon.Source, checkBmp });
                        }
                        Icon = icon;
                    }
                    break;
            }
        }


        #endregion
    }
}
