using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutRecorderParameterViewModel :
        MpAvParameterViewModelBase, MpAvIShortcutCommandViewModel {
        #region Private Variables
        #endregion

        #region MpAvIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType { get; }
        public string KeyString =>
            MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutKeyString(this);
        public ICommand ShortcutCommand { get; }
        public object ShortcutCommandParameter { get; }
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

        public MpAvShortcutRecorderParameterViewModel(MpViewModelBase parent) : base(parent) {
            if (parent is MpAvShortcutTriggerViewModel stvm) {

                ShortcutType = MpShortcutType.InvokeAction;
                ShortcutCommand = MpAvTriggerCollectionViewModel.Instance.InvokeActionCommand;
                ShortcutCommandParameter = stvm.ActionId;
            }

        }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            OnPropertyChanged(nameof(CurrentValue));
            if (this is MpISliderViewModel svm) {
                svm.OnPropertyChanged(nameof(svm.MinValue));
                svm.OnPropertyChanged(nameof(svm.MaxValue));
                svm.OnPropertyChanged(nameof(svm.SliderValue));

            }

            await Task.Delay(1);

            IsBusy = false;
        }






        #endregion
    }
}
