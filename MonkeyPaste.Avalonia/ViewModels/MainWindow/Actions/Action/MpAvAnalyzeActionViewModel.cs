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
        MpAvActionViewModelBase,
        MpIPopupSelectorMenu 
        {
        #region MpIPopupSelectorMenu Implementation
        public bool IsOpen { get; set; }
        public MpMenuItemViewModel PopupMenu =>
            MpAvAnalyticItemCollectionViewModel.Instance.GetAnalyzerMenu(SelectAnalyzerCommand);
        //public MpMenuItemViewModel SelectedMenuItem =>
        //    SelectedPreset == null ?
        //        null :
        //    (this as MpIPopupSelectorMenu).PopupMenu.SubItems
        //    .SelectMany(x => x.SubItems)
        //    .FirstOrDefault(x => x.MenuItemId == AnalyticItemPresetId);
        //public string EmptyText => "Select Analyzer...";
        //public object EmptyIconResourceObj => MpAvActionViewModelBase.GetDefaultActionIconResourceKey(MpActionType.Analyze, null);


        public object SelectedIconResourceObj =>
            AnalyticItemPresetId == 0 ?
                GetDefaultActionIconResourceKey(ActionType) :
                SelectedPreset == null ?
                    "WarningImage" :
                    SelectedPreset.GetMenu(null).IconSourceObj;
        public string SelectedLabel =>
            AnalyticItemPresetId == 0 ?
                "Select Analyzer..." :
                SelectedPreset == null ?
                    "Not found..." :
                    SelectedPreset.GetMenu(null).Header;

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
                if (Action == null || string.IsNullOrEmpty(Arg1)) {
                    return 0;
                }
                return int.Parse(Arg1);
            }
            set {
                if (AnalyticItemPresetId != value) {
                    Arg1 = value.ToString();
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
        public ICommand SelectAnalyzerCommand => new MpCommand<object>(
            (args) => {
                if (args is int presetId) {
                    if (AnalyticItemPresetId == presetId) {
                        AnalyticItemPresetId = 0;
                    } else {
                        AnalyticItemPresetId = presetId;
                    }
                    OnPropertyChanged(nameof(SelectedPreset));
                    OnPropertyChanged(nameof(SelectedLabel));
                    OnPropertyChanged(nameof(SelectedIconResourceObj));
                }
            });


        #endregion
    }
}
