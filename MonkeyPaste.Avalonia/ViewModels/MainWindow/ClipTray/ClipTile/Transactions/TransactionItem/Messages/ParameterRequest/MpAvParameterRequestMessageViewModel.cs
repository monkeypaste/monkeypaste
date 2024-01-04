using AngleSharp.Common;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public override MpAvMenuItemViewModel ContextMenuItemViewModel {
            get {
                if (PresetViewModel == null) {
                    return null;
                }
                return new MpAvMenuItemViewModel() {
                    ParentObj = this,
                    Header =
                        MpAvDateTimeToStringConverter.Instance.Convert(Parent.TransactionDateTime, null, UiStrings.CommonDateTimeFormat, null) as string,
                    IconSourceObj = PresetViewModel.IconId,
                    SubItems = new List<MpAvMenuItemViewModel>() {
                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.ClipTileTransactionViewSourceHeader,
                            IconSourceObj = "GraphImage",
                            IsVisible = Parent.Response is MpAvAnnotationMessageViewModel amvm && amvm.RootAnnotationViewModel != null,
                            Command = MpAvClipTrayViewModel.Instance.SelectClipTileTransactionNodeCommand,
                            CommandParameter =
                                Parent.Response is MpAvAnnotationMessageViewModel amvm2 && amvm2.RootAnnotationViewModel != null ?
                                new object[]{HostClipTileViewModel.CopyItemId, amvm2.RootAnnotationViewModel.AnnotationGuid } : null
                        }
                    }
                };
            }
        }
        #endregion

        #region State

        public bool CanRestore {
            get {
                if (PresetViewModel == null ||
                    ParameterReqFormat == null ||
                    ParameterReqFormat.items == null) {
                    return false;
                }
                return PresetViewModel.ParamLookup.Difference(ParameterReqFormat.items.ToDictionary(x => x.paramId, x => x.value)).Any();

            }
        }
        bool WasRestored { get; set; }

        #endregion

        #region Model

        public MpPluginParameterRequestFormat ParameterReqFormat { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvParameterRequestMessageViewModel(MpAvTransactionItemViewModel parent) : base(parent) {
            PropertyChanged += MpAvParameterRequestMessageViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        public override async Task InitializeAsync(object jsonOrParsedFragment, MpAvITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;
            Json = jsonOrParsedFragment is string ? jsonOrParsedFragment.ToString() : string.Empty;
            if (!string.IsNullOrEmpty(Json)) {
                ParameterReqFormat = MpJsonExtensions.ParseParamRequest(Json);
            }

            var tsl = await MpDataModelProvider.GetCopyItemTransactionSourcesAsync(TransactionId);
            if (tsl.FirstOrDefault(x => x.CopyItemSourceType == MpTransactionSourceType.AnalyzerPreset) is MpTransactionSource ts) {
                if (tsl.Where(x => x.CopyItemSourceType == MpTransactionSourceType.AnalyzerPreset).Count() > 1) {
                    // this would be a big problem, pretty sure only analyzer can be involved in a transaction at time..
                    MpDebug.Break();
                }
                PresetViewModel = MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == ts.SourceObjId);

            }
            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(CanRestore));
            OnPropertyChanged(nameof(PresetViewModel));
            OnPropertyChanged(nameof(Body));
            IsBusy = false;
        }


        #endregion

        #region Private Methods
        private void MpAvParameterRequestMessageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        if (PresetViewModel != null) {
                            // NOTE attaching this lazy to prevent memory leaks and excessive updates
                            // cause there will be a lot of this guys
                            PresetViewModel.OnParameterValuesChanged += PresetViewModel_OnParameterValuesChanged;
                        }
                        OnPropertyChanged(nameof(CanRestore));
                        // set preset to this requests values
                        RestoreRequestPresetValuesCommand.Execute(null);
                    } else {
                        if (PresetViewModel != null) {
                            PresetViewModel.OnParameterValuesChanged -= PresetViewModel_OnParameterValuesChanged;
                        }
                        // put saved values back
                        ResetPresetValuesCommand.Execute(null);
                    }
                    break;
            }
        }
        private void PresetViewModel_OnParameterValuesChanged(object sender, MpAvParameterViewModelBase e) {
            OnPropertyChanged(nameof(CanRestore));
        }
        #endregion

        #region Commands

        public MpIAsyncCommand ResetPresetValuesCommand => new MpAsyncCommand(
            async () => {
                var preset = await MpDataModelProvider.GetItemAsync<MpPluginPreset>(PresetViewModel.AnalyticItemPresetId);

                await PresetViewModel.InitializeAsync(preset);

                //PresetViewModel.ResetOrDeleteThisPresetCommand.Execute(null);
                WasRestored = false;
                MpConsole.WriteLine($"Preset '{PresetViewModel}' values were reset to db by '{this}'");
            }, () => {
                return PresetViewModel != null && WasRestored;
            });

        public ICommand RestoreRequestPresetValuesCommand => new MpCommand(
            () => {
                foreach (var req_param in ParameterReqFormat.items) {
                    if (PresetViewModel.Items.FirstOrDefault(x => x.ParamId.Equals(req_param.paramId))
                            is not { } pvm) {
                        // should probably not happen but may if this transaction is from an older version 
                        // of the analyzer there could be missing params, maybe better to disable in this 
                        // case, not sure
                        continue;
                    }
                    pvm.CurrentValue = req_param.value;
                }
                //PresetViewModel.ResetOrDeleteThisPresetCommand.Execute(ParameterReqFormat);
                OnPropertyChanged(nameof(CanRestore));
                WasRestored = true;
            });

        #endregion
    }
}
