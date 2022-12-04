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
    public class MpAvAnalyzeActionViewModel : MpAvActionViewModelBase {
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

        public MpAvAnalyzeActionViewModel(MpAvActionCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Overrides

        public override async Task PerformActionAsync(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            var aipvm = MpAvAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(AnalyticItemPresetId);
            
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
                Task.Run(Validate);
            }
        }

        protected override async Task<bool> Validate() {
            await base.Validate();
            if (!IsValid) {
                return IsValid;
            }

            var aipvm = MpAvAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Action.ActionObjId);
            if (aipvm == null) {
                ValidationText = $"Analyzer for Action '{FullName}' not found";
                await ShowValidationNotification();
            } else {
                var pavm = ParentActionViewModel;
                while(pavm != null) {
                    if(pavm is MpAvCompareActionViewModelBase cavm) {
                        if(cavm.IsItemTypeCompare) {
                            if(!aipvm.Parent.IsContentTypeValid(cavm.ContentItemType)) {
                                ValidationText = $"Parent Comparer '{pavm.Label}' filters only for '{cavm.ContentItemType.ToString()}' type content and analyzer '{aipvm.FullName}' will never execute because it does not support '{cavm.ContentItemType.ToString()}' type of input ";
                                await ShowValidationNotification();
                                return IsValid;
                            }
                        }
                    }
                    pavm = pavm.ParentActionViewModel;
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

        #endregion
    }
}
