using System.Windows.Input;

namespace MpWpfApp {
    public class MpSortTypeComboBoxItemViewModel : MpViewModelBase<object> {
        #region Properties
        private string _header;
        public string Header {
            get {
                return _header;
            }
            set {
                if (_header != value) {
                    _header = value;
                    OnPropertyChanged_old(nameof(Header));
                }
            }
        }

        public bool IsVisible { get; set; } = true;
        #endregion

        #region Public Methods
        public MpSortTypeComboBoxItemViewModel(string header, ICommand command) : base(null) {
            Header = header;
            Command = command;
            if(Header == "Manual") {
                IsVisible = false;
            }
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
                    OnPropertyChanged_old(nameof(Command));
                }
            }
        }
        #endregion

    }
}
