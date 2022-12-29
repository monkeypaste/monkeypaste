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
        MpIPopupSelectorMenu {
        #region MpIPopupSelectorMenu Implementation
        public bool IsOpen { get; set; }
        public MpMenuItemViewModel PopupMenu =>
            MpAvAnalyticItemCollectionViewModel.Instance.GetAnalyzerMenu(SelectAnalyzerCommand);
        public MpMenuItemViewModel SelectedMenuItem =>
            SelectedPreset == null ?
                null :
            (this as MpIPopupSelectorMenu).PopupMenu.SubItems
            .SelectMany(x => x.SubItems)
            .FirstOrDefault(x => x.MenuItemId == AnalyticItemPresetId);
        public string EmptyText => "Select Analyzer...";
        public object EmptyIconResourceObj => MpAvActionViewModelBase.GetDefaultActionIconResourceKey(MpActionType.Analyze, null);

        #endregion

        #region Properties

        #region View Models

        public MpAvAnalyticItemPresetViewModel SelectedPreset {
            get {
                if(MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return null;
                }
                return MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == AnalyticItemPresetId);
            }
            set {
                if(SelectedPreset != value) {
                    AnalyticItemPresetId = value.AnalyticItemPresetId;
                    // NOTE dont understand why but HasModelChanged property change not firing from this
                    // so forcing db write for now..
                    Task.Run(async () => { await Action.WriteToDatabaseAsync(); });
                    OnPropertyChanged(nameof(SelectedPreset));
                }
            }
        }

        #endregion

        #region State

        public bool IsShowingParameters { get; set; } = false;

        #endregion

        #region Model

        public int AnalyticItemPresetId {
            get {
                if (Action == null) {
                    return 0;
                }
                return ActionObjId;
            }
            set {
                if (AnalyticItemPresetId != value) {
                    ActionObjId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AnalyticItemPresetId));
                    OnPropertyChanged(nameof(SelectedPreset));
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

        protected override async Task<bool> ValidateActionAsync() {
            await base.ValidateActionAsync();
            if (!IsValid) {
                return false;
            }
            if(AnalyticItemPresetId == 0) {
                return true;
            }

            if (SelectedPreset == null) {
                ValidationText = $"Analyzer for Action '{FullName}' not found";
                ShowValidationNotification();
            } else {
                var pavm = ParentTreeItem;
                while(pavm != null) {
                    if(pavm is MpAvCompareActionViewModelBase cavm) {
                        if(cavm.IsItemTypeCompare) {
                            if(!SelectedPreset.Parent.IsContentTypeValid(cavm.ContentItemType)) {
                                ValidationText = $"Parent Comparer '{pavm.Label}' filters only for '{cavm.ContentItemType.ToString()}' type content and analyzer '{SelectedPreset.FullName}' will never execute because it does not support '{cavm.ContentItemType.ToString()}' type of input ";
                                ShowValidationNotification();
                                return IsValid;
                            }
                        }
                    }
                    pavm = pavm.ParentTreeItem;
                }
                ValidationText = string.Empty;
            }
            return IsValid;
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
                    AnalyticItemPresetId = presetId;
                    OnPropertyChanged(nameof(SelectedPreset));
                    OnPropertyChanged(nameof(SelectedMenuItem));
                }
            });

        #endregion
    }
}
