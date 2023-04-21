using MonkeyPaste.Common.Plugin;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutRecorderParameterViewModel :
        MpAvParameterViewModelBase,
        MpAvIKeyGestureViewModel,
        MpIShortcutCommandViewModel {
        #region Private Variables
        #endregion

        #region MpAvIKeyGestureViewModel Implementation

        public ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups =>
            new ObservableCollection<MpAvShortcutKeyGroupViewModel>(KeyString.ToKeyItems());

        #endregion
        #region MpIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType { get; set; }

        private string _keyStr;
        public string KeyString {
            get {
                if (!string.IsNullOrEmpty(_keyStr)) {
                    return _keyStr;
                }
                return MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutKeyString(this);
            }
            set {
                if (KeyString != value) {
                    _keyStr = value;
                    if (UnitType == MpParameterValueUnitType.PlainText &&
                        CurrentValue != _keyStr) {
                        // persist new keystring
                        CurrentValue = _keyStr;
                    }
                    OnPropertyChanged(nameof(KeyString));
                }
            }
        }
        public ICommand ShortcutCommand { get; set; }
        public object ShortcutCommandParameter { get; set; }
        #endregion

        #region Properties

        #region View Models
        #endregion

        #region Appearance
        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvShortcutRecorderParameterViewModel() : base(null) { }

        public MpAvShortcutRecorderParameterViewModel(MpViewModelBase parent) : base(parent) { }


        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            if (Parent is MpAvKeySimulatorActionViewModel) {
                KeyString = CurrentValue;
            }

            IsBusy = false;
        }


        #endregion
    }
}
