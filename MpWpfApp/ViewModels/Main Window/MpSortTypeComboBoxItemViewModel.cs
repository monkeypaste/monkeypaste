using System.Windows.Input;

namespace MpWpfApp {
    public class MpSortTypeComboBoxItemViewModel : MpViewModelBase {
        #region Properties
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
        #endregion

        #region Public Methods
        public MpSortTypeComboBoxItemViewModel(string header, ICommand command) : base() {
            Header = header;
            Command = command;
        }
        public override string ToString() {
            return Header;
        }
        #endregion

        #region Commands

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
        #endregion

    }
}
