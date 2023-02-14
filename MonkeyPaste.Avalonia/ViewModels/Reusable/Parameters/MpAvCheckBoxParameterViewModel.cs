using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvCheckBoxParameterViewModel : MpAvParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvCheckBoxParameterViewModel() : base(null) { }

        public MpAvCheckBoxParameterViewModel(MpIParameterHostViewModel parent) : base(parent) { }

        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            IsBusy = false;
        }

        #endregion
    }
}
