using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyzeActionViewModel :
        MpAvActionViewModelBase,
        MpIContentTypeDependant {
        #region Constants

        public const string SELECTED_ANALYZER_PARAM_ID = "SelectedAnalyzerId";

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessComponent _actionComponentFormat;
        public override MpHeadlessComponent ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessComponent() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = UiStrings.ActionAnalyzeLabel,
                                controlType = MpParameterControlType.ComponentPicker,
                                unitType = MpParameterValueUnitType.AnalyzerComponentId,
                                isRequired = true,
                                paramId = SELECTED_ANALYZER_PARAM_ID
                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Interfaces

        #region MpIContentTypeDependant Implementation

        bool MpIContentTypeDependant.IsContentTypeValid(MpCopyItemType cit) {
            if (SelectedPreset is MpIContentTypeDependant ctd) {
                return ctd.IsContentTypeValid(cit);
            }
            return false;
        }

        #endregion

        #endregion

        #region Properties

        #region View Models

        public MpAvAnalyticItemPresetViewModel SelectedPreset =>
            MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == AnalyticItemPresetId);

        #endregion

        #region Appearance

        public override object IconResourceObj =>
            SelectedPreset == null ? base.IconResourceObj : SelectedPreset.IconId;
        public override string ActionHintText =>
            UiStrings.ActionAnalyzerHint;

        #endregion

        #region State

        public bool IsShowingParameters { get; set; } = false;

        #endregion

        #region Model

        public int AnalyticItemPresetId {
            get {
                if (ArgLookup.TryGetValue(SELECTED_ANALYZER_PARAM_ID, out var param_vm) &&
                    param_vm.IntValue is int curVal) {
                    return curVal;
                }
                return 0;
            }
            set {
                if (AnalyticItemPresetId != value) {
                    ArgLookup[SELECTED_ANALYZER_PARAM_ID].IntValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AnalyticItemPresetId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvAnalyzeActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvAnalyzeActionViewModel_PropertyChanged;
        }


        #endregion

        #region Public Overrides

        protected override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                await FinishActionAsync(arg);
                return;
            }

            var actionInput = GetInput(arg);

            var aipvm =
                MpAvAnalyticItemCollectionViewModel.Instance
                .AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == AnalyticItemPresetId);

            MpAvAnalyzeOutput output = new MpAvAnalyzeOutput() {
                Previous = arg as MpAvActionOutput,
                CopyItem = actionInput.CopyItem
            };
            object[] args = new object[] { aipvm, actionInput.CopyItem };
            if (aipvm != null &&
               aipvm.Parent != null &&
               aipvm.Parent.PerformAnalysisCommand.CanExecute(args)) {
                MpAnalyzerTransaction ltr = await aipvm.Parent.PerformAnalysisAsync(args);
                if (ltr == null) {
                    MpConsole.WriteLine("");
                    MpConsole.WriteLine($"Analyzer '{aipvm.FullName}' returned null to Action({ActionId}) '{FullName}', so {RootTriggerActionViewModel.Label} will stop.");
                    MpConsole.WriteLine("");
                } else {
                    output.PluginResponse = ltr.Response;

                    if (output.CopyItem != null && ltr.ResponseContent != null &&
                       output.CopyItem.Id != ltr.ResponseContent.Id &&
                       ltr.RequestContent is MpCopyItem req_ci) {
                        // analyzer created NEW content
                        output.NewCopyItem = ltr.ResponseContent;
                        output.CopyItem = req_ci;
                    } else if (ltr.ResponseContent != null) {
                        // use (possibly) updated item from analysis result
                        output.CopyItem = ltr.ResponseContent;
                    }
                }
            } else {
                MpConsole.WriteLine("");
                MpConsole.WriteLine($"Action({ActionId}) '{FullName}' Failed to execute, analyzer w/ presetId({AnalyticItemPresetId}) not found");
                MpConsole.WriteLine("");
            }
            await FinishActionAsync(output);
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpPreset aip && aip.Id == AnalyticItemPresetId) {
                Task.Run(ValidateActionAndDescendantsAsync);
            }
        }

        protected override async Task ValidateActionAndDescendantsAsync() {
            await base.ValidateActionAndDescendantsAsync();
            if (!IsValid) {
                return;
            }

            if (AnalyticItemPresetId == 0) {
                //ValidationText = $"No analyzer selection for action '{FullName}'";
                ValidationText = string.Format(UiStrings.ActionAnalyzeValidation1, FullName);
            } else if (SelectedPreset == null) {
                //ValidationText = $"Analyzer for Action '{FullName}' not found";
                ValidationText = string.Format(UiStrings.ActionAnalyzeValidation2, FullName);
            } else {
                ValidationText = string.Empty;
                // check ancestors to ensure analyzer supports content type as input

                MpAvActionViewModelBase closest_type_dep_ancestor = ParentActionViewModel;
                while (closest_type_dep_ancestor != null) {
                    // walk up action chain, to last analyzer or content add trigger to validate
                    if (closest_type_dep_ancestor is MpAvContentAddTriggerViewModel cavm &&
                        cavm.AddedContentType != MpCopyItemType.None &&
                         !SelectedPreset.Parent.IsContentTypeValid(cavm.AddedContentType)) {
                        //ValidationText = $"Parent '{closest_type_dep_ancestor.Label}' filters only for '{cavm.AddedContentType}' type content and analyzer '{SelectedPreset.FullName}' will never execute because it does not support '{cavm.AddedContentType}' type of input ";
                        ValidationText = string.Format(UiStrings.ActionAnalyzeValidation3, closest_type_dep_ancestor.Label, SelectedPreset.FullName, cavm.AddedContentType.ToString());
                        break;
                    }
                    if (closest_type_dep_ancestor is MpAvAnalyzeActionViewModel aavm &&
                        aavm.SelectedPreset != null) {
                        // TODO need to have an analyzer.IsInputValid using analyzerOutputformat flags here but 
                        // still not clear how handling previous output is it just LastOutput?
                        // if any previous how would user specify or query?

                    }
                    closest_type_dep_ancestor = closest_type_dep_ancestor.ParentActionViewModel;
                }
            }
            if (!IsValid) {
                ShowValidationNotification();
            }
        }

        protected override void Param_vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.Param_vm_PropertyChanged(sender, e);
            if (sender is not MpAvComponentPickerParameterViewModel cppvm) {
                return;
            }
            switch (e.PropertyName) {
                case nameof(cppvm.ComponentId):
                    AnalyticItemPresetId = cppvm.ComponentId;
                    break;
            }
        }
        #endregion

        #region Private Methods

        private void MpAvAnalyzeActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(AnalyticItemPresetId):
                case nameof(ActionArgs):
                case nameof(ArgLookup):
                    OnPropertyChanged(nameof(SelectedPreset));
                    OnPropertyChanged(nameof(IconResourceObj));
                    break;
                case nameof(IsPerformingAction):
                    //MpConsole.WriteLine($"Analyzer '{this}' IsPerformingAction: {IsPerformingAction}");
                    break;
            }
        }

        #endregion

        #region Commands


        public ICommand ToggleShowParametersCommand => new MpCommand(
            () => {
                IsShowingParameters = !IsShowingParameters;
            });


        #endregion
    }
}
