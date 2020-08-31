using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpClipTileContextMenuItemViewModel : MpViewModelBase {
        private object _commandParameter = null;
        public object CommandParameter {
            get {
                return _commandParameter;
            }
            set {
                if(_commandParameter != value) {
                    _commandParameter = value;
                    OnPropertyChanged(nameof(CommandParameter));
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

        private Visibility _isVisible = Visibility.Visible;
        public Visibility IsVisible {
            get {
                return _isVisible;
            }
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
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

        public MpClipTileContextMenuItemViewModel(string header, ICommand command,object commandParameter, bool isChecked) {
            Header = header;
            Command = command;
            CommandParameter = commandParameter;
            IsChecked = isChecked;
        }
    }
}
