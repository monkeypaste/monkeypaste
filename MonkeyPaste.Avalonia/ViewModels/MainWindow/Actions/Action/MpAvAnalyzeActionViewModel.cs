using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyzeActionViewModel : 
        MpAvActionViewModelBase {
        #region Constants

        public const string SELECTED_ANALYZER_PARAM_ID = "SelectedAnalyzerId";

        #endregion

        #region MpIParameterHost Overrides

        private MpTriggerPluginFormat _actionComponentFormat;
        public override MpTriggerPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpTriggerPluginFormat() {
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
        #region Properties

        #region View Models

        public MpAvAnalyticItemPresetViewModel SelectedPreset =>
            MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == AnalyticItemPresetId);

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

        public MpAvAnalyzeActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Overrides

        public override async Task PerformActionAsync(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            var aipvm = MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x=>x.AnalyticItemPresetId == AnalyticItemPresetId);
            
            object[] args = new object[] { aipvm, actionInput.CopyItem };
            if(aipvm != null && 
               aipvm.Parent != null &&
               aipvm.Parent.ExecuteAnalysisCommand.CanExecute(args)) {
                aipvm.Parent.ExecuteAnalysisCommand.Execute(args);

                while (aipvm.Parent.IsBusy) {
                    await Task.Delay(100);
                }

                if(aipvm.Parent.LastTransaction != null && aipvm.Parent.LastTransaction.Response != null) {
                    await base.PerformActionAsync(
                        new MpAvAnalyzeOutput() {
                            Previous = arg as MpAvActionOutput,
                            CopyItem = actionInput.CopyItem,
                            TransactionResult = aipvm.Parent.LastTransaction.Response
                        });
                    return;
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
            
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpPluginPreset aip && aip.Id == AnalyticItemPresetId) {
                Task.Run(ValidateActionAsync);
            }
        }

        protected override async Task ValidateActionAsync() {
            await Task.Delay(1);

            if(AnalyticItemPresetId == 0) {
                ValidationText = $"No analyzer selection for action '{FullName}'";
            } else if (SelectedPreset == null) {
                ValidationText = $"Analyzer for Action '{FullName}' not found";
            } else {
                // check ancestors to ensure analyzer supports content type as input
                var pavm = ParentActionViewModel;
                while(pavm != null) {
                    if(pavm is MpAvCompareActionViewModelBase cavm) {
                        if(cavm.IsItemTypeCompare) {
                            if(!SelectedPreset.Parent.IsContentTypeValid(cavm.ContentItemType)) {
                                ValidationText = $"Parent Comparer '{pavm.Label}' filters only for '{cavm.ContentItemType.ToString()}' type content and analyzer '{SelectedPreset.FullName}' will never execute because it does not support '{cavm.ContentItemType.ToString()}' type of input ";
                                break;
                            }
                        }
                    }
                    pavm = pavm.ParentActionViewModel;
                }
                ValidationText = string.Empty;
            }
            if(!IsValid) {
                ShowValidationNotification();
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
