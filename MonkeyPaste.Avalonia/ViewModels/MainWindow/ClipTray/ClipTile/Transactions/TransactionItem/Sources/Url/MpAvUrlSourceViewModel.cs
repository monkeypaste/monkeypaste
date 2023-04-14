using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvUrlSourceViewModel : MpAvTransactionSourceViewModel {

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        public MpAvAnalyticItemPresetViewModel PresetViewModel { get; private set; }

        #endregion

        #region State
        #endregion

        #region Model

        public MpPluginRequestFormatBase ParameterReqFormat { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvUrlSourceViewModel(MpAvTransactionItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpTransactionSource ts) {
            IsBusy = true;
            await base.InitializeAsync(ts);

            if (string.IsNullOrEmpty(SourceArg)) {
                ParameterReqFormat = null;
            } else {
                ParameterReqFormat = MpJsonConverter.DeserializeObject<MpPluginRequestFormatBase>(SourceArg);
            }
            PresetViewModel =
                MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == SourceObjId);

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
