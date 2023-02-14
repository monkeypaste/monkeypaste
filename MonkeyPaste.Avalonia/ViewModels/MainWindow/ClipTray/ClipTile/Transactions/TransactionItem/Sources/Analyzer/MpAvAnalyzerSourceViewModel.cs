using MonkeyPaste.Common.Plugin;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyzerSourceViewModel : MpAvTransactionSourceViewModelBase {

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        public MpAvAnalyticItemPresetViewModel PresetViewModel =>
            MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == SourceObjId);

        #endregion

        #region State
        #endregion

        #region Model

        public MpPluginRequestFormatBase ParameterReqFormat { get; private set; }
        public int PresetId { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvAnalyzerSourceViewModel(MpAvTransactionItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpTransactionSource ts) {
            IsBusy = true;
            await base.InitializeAsync(ts);

            if (PresetViewModel == null) {
                ParameterReqFormat = null;
            } else {
                ParameterReqFormat = MpPluginRequestFormatBase.Parse(SourceArg);
            }

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(PresetViewModel));
            OnPropertyChanged(nameof(Body));

            IsBusy = false;
        }

        #endregion

        #region Commands
        #endregion
    }
}
