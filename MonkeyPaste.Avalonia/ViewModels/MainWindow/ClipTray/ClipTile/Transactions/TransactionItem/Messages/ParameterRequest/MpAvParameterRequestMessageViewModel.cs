using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvParameterRequestMessageViewModel :
        MpAvTransactionMessageViewModelBase,
        MpAvITransactionNodeViewModel {

        #region Interfaces
        #endregion

        #region Properties
        public override object IconResourceObj => "QuillDeltaImage";
        //public override object Body { get; }
        public override string LabelText => "Delta";
        #region View Models

        public MpAvAnalyticItemPresetViewModel PresetViewModel { get; private set; }

        public override MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                if (PresetViewModel == null) {
                    return null;
                }
                return new MpMenuItemViewModel() {
                    Header = PresetViewModel.Label,
                    IconSourceObj = PresetViewModel.IconId,
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = "Select",
                            IconSourceObj = "SlidersImage",
                            Command = PresetViewModel.Parent.SelectPresetCommand,
                            CommandParameter = new object[] { PresetViewModel, HostClipTileViewModel.CopyItem }
                        },
                        new MpMenuItemViewModel() {
                            Header = "View",
                            IconSourceObj = "GraphImage",
                            IsVisible = Parent.Response is MpAvAnnotationMessageViewModel amvm && amvm.RootAnnotationViewModel != null,
                            Command = HostClipTileViewModel.TransactionCollectionViewModel.SelectChildCommand,
                            CommandParameter =
                                Parent.Response is MpAvAnnotationMessageViewModel amvm2 && amvm2.RootAnnotationViewModel != null ?
                                amvm2.RootAnnotationViewModel : null
                        },
                    }
                };
            }
        }
        #endregion

        #region State

        #endregion

        #region Model

        public MpPluginParameterRequestFormat ParameterReqFormat { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvParameterRequestMessageViewModel(MpAvTransactionItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(object jsonOrParsedFragment, MpAvITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;

            Json = jsonOrParsedFragment is string ? jsonOrParsedFragment.ToString() : string.Empty;
            if (!string.IsNullOrEmpty(Json)) {
                ParameterReqFormat = MpPluginParameterRequestFormat.Parse(Json);
            }

            var tsl = await MpDataModelProvider.GetCopyItemTransactionSourcesAsync(TransactionId);
            if (tsl.FirstOrDefault(x => x.CopyItemSourceType == MpTransactionSourceType.AnalyzerPreset) is MpTransactionSource ts) {
                if (tsl.Where(x => x.CopyItemSourceType == MpTransactionSourceType.AnalyzerPreset).Count() > 1) {
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
