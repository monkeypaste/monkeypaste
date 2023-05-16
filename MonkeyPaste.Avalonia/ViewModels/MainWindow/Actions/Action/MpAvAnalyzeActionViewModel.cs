using Avalonia.Controls.Shapes;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
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

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Analyzer",
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

        public override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            var actionInput = GetInputWithCallback(arg, string.Empty, out var lastOutputCallback);

            var aipvm =
                MpAvAnalyticItemCollectionViewModel.Instance
                .AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == AnalyticItemPresetId);

            MpAvAnalyzeOutput output = new MpAvAnalyzeOutput() {
                Previous = arg as MpAvActionOutput,
                CopyItem = actionInput.CopyItem
            };
            object[] args = new object[] { aipvm, actionInput.CopyItem, lastOutputCallback };
            if (aipvm != null &&
               aipvm.Parent != null &&
               aipvm.Parent.ExecuteAnalysisCommand.CanExecute(args)) {
                await aipvm.Parent.ExecuteAnalysisCommand.ExecuteAsync(args);

                //while (aipvm.Parent.IsBusy) {
                //    await Task.Delay(100);
                //}

                if (aipvm.Parent.LastTransaction != null) {
                    if (output.CopyItem != null && aipvm.Parent.LastTransaction.ResponseContent != null &&
                        output.CopyItem.Id != aipvm.Parent.LastTransaction.ResponseContent.Id) {
                        // analyzer created NEW content
                        // TODO how should new content be handled?
                    } else if (aipvm.Parent.LastTransaction.ResponseContent != null) {
                        // use (possibly) updated item from analysis result
                        output.CopyItem = aipvm.Parent.LastTransaction.ResponseContent;
                    }

                    output.TransactionResult = aipvm.Parent.LastTransaction.Response;
                } else {
                    MpConsole.WriteLine("");
                    MpConsole.WriteLine($"Analyzer '{aipvm.FullName}' returned null to Action({ActionId}) '{FullName}', so {RootTriggerActionViewModel.Label} will stop.");
                    MpConsole.WriteLine("");
                }
            } else {
                MpConsole.WriteLine("");
                MpConsole.WriteLine($"Action({ActionId}) '{FullName}' Failed to execute, analyzer w/ presetId({AnalyticItemPresetId}) not found");
                MpConsole.WriteLine("");
            }
            await base.PerformActionAsync(output);
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpPluginPreset aip && aip.Id == AnalyticItemPresetId) {
                Task.Run(ValidateActionAsync);
            }
        }

        protected override async Task ValidateActionAsync() {
            await base.ValidateActionAsync();
            if (!IsValid) {
                return;
            }

            if (AnalyticItemPresetId == 0) {
                ValidationText = $"No analyzer selection for action '{FullName}'";
            } else if (SelectedPreset == null) {
                ValidationText = $"Analyzer for Action '{FullName}' not found";
            } else {
                ValidationText = string.Empty;
                // check ancestors to ensure analyzer supports content type as input

                MpAvActionViewModelBase closest_type_dep_ancestor = ParentActionViewModel;
                while (closest_type_dep_ancestor != null) {
                    // walk up action chain, to last analyzer or content add trigger to validate
                    if (closest_type_dep_ancestor is MpAvContentAddTriggerViewModel cavm &&
                        cavm.AddedContentType != MpCopyItemType.None &&
                         !SelectedPreset.Parent.IsContentTypeValid(cavm.AddedContentType)) {
                        ValidationText = $"Parent '{closest_type_dep_ancestor.Label}' filters only for '{cavm.AddedContentType}' type content and analyzer '{SelectedPreset.FullName}' will never execute because it does not support '{cavm.AddedContentType}' type of input ";
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

        #endregion

        #region Private Methods

        private void MpAvAnalyzeActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ActionArgs):
                    OnPropertyChanged(nameof(SelectedPreset));
                    break;
                case nameof(SelectedPreset):
                    OnPropertyChanged(nameof(IconResourceObj));
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
