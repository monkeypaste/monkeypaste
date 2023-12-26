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
                //if (KeyString != value) {
                if (_keyStr != value) {
                    _keyStr = value;
                    if (UnitType == MpParameterValueUnitType.PlainText &&
                        CurrentValue != _keyStr) {
                        // persist new keystring
                        CurrentValue = _keyStr;
                    }
                    OnPropertyChanged(nameof(KeyString));
                    OnPropertyChanged(nameof(KeyGroups));
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
        public bool IsRawInput { get; set; } = false;
        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvShortcutRecorderParameterViewModel() : base(null) { }

        public MpAvShortcutRecorderParameterViewModel(MpAvViewModelBase parent) : base(parent) {
            PropertyChanged += MpAvShortcutRecorderParameterViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            if (Parent is MpAvShortcutTriggerViewModel sctvm) {
                var sc = await MpDataModelProvider.GetItemAsync<MpShortcut>(sctvm.ShortcutId);
                KeyString = sc == null ? string.Empty : sc.KeyString;
            }

            OnPropertyChanged(nameof(KeyGroups));
            IsBusy = false;
        }


        #endregion

        #region Private Methods


        private void MpAvShortcutRecorderParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(KeyString):
                    OnPropertyChanged(nameof(KeyGroups));
                    break;
            }
        }

        #endregion
    }
}
