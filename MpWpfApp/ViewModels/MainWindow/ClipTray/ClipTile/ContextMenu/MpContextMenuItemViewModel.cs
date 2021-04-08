using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpContextMenuItemViewModel : MpViewModelBase {

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

        private bool _isTagLinkedToClip = false;
        public bool IsChecked {
            get {
                return _isTagLinkedToClip;
            }
            set {
                if (_isTagLinkedToClip != value) {
                    _isTagLinkedToClip = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
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

        private string _iconSource;
        public string IconSource {
            get {
                return _iconSource;
            }
            set {
                if(_iconSource != value) {
                    _iconSource = value;
                    OnPropertyChanged(nameof(IconSource));
                    OnPropertyChanged(nameof(Icon));
                }
            } 
        }

        public Image Icon {
            get {
                if(IsSeparator || string.IsNullOrEmpty(IconSource)) {
                    return null;
                }
                var icon = new Image();
                icon.Source = (BitmapSource)new BitmapImage(new Uri(IconSource));
                return icon;
            }
        }

        private ObservableCollection<MpContextMenuItemViewModel> _subItems = null;
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
        public MpContextMenuItemViewModel() : base() {
            IsSeparator = true;
        }

        public MpContextMenuItemViewModel(
            string header, 
            ICommand command,
            object commandParameter,
            bool isChecked,
            string iconSource = "",
            ObservableCollection<MpContextMenuItemViewModel> subItems = null) : base() {
            Header = header;
            Command = command;
            CommandParameter = commandParameter;
            IsChecked = isChecked;
            IconSource = iconSource;
            SubItems = subItems;
        }
        #endregion
    }
}
