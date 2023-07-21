using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvButtonParameterViewModel :
        MpAvParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Appearance

        private string _title;
        public string Title {
            get {
                return _title != null ?
                        _title :
                        ParameterFormat == null ||
                        ParameterFormat.values == null ||
                        ParameterFormat.values.Count == 0 ||
                        string.IsNullOrEmpty(ParameterFormat.values[0].label) ?
                            ParameterFormat.label :
                            ParameterFormat.values[0].label;
            }
            set {
                if (_title != value) {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        #endregion
        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvButtonParameterViewModel() : base(null) { }

        public MpAvButtonParameterViewModel(MpViewModelBase parent) : base(parent) { }


        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpParameterValue aipv) {
            await base.InitializeAsync(aipv);
            OnPropertyChanged(nameof(Title));
        }
        #endregion

        #region Commands

        public ICommand ClickCommand { get; set; } // must be set at runtime
        #endregion
    }
}
