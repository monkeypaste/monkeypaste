using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvParameterRequestMessageViewModel  : MpAvTransactionMessageViewModelBase, MpITransactionNodeViewModel {

        #region Interfaces
        #endregion

        #region Properties
        public override object IconResourceObj => "QuillDeltaImage";
        //public override object Body { get; }
        public override string LabelText => "Delta";
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

        public MpAvParameterRequestMessageViewModel(MpAvTransactionItemViewModelBase parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(object jsonOrParsedFragment, MpITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;

            Json = jsonOrParsedFragment is string ? jsonOrParsedFragment.ToString() : string.Empty;
            if(!string.IsNullOrEmpty(Json)) {
                ParameterReqFormat = MpPluginRequestFormatBase.Parse(Json);
            }

            var tsl = await MpDataModelProvider.GetCopyItemTransactionSourcesAsync(TransactionId);
            if(tsl.FirstOrDefault(x=>x.CopyItemSourceType == MpTransactionSourceType.AnalyzerPreset) is MpTransactionSource ts) {
                if(tsl.Where(x=>x.CopyItemSourceType == MpTransactionSourceType.AnalyzerPreset).Count() > 1) {
                    // this would be a big problem, pretty sure only analyzer can be involved in a transaction at time..
                    Debugger.Break();
                }
                PresetViewModel = MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == ts.SourceObjId);
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
