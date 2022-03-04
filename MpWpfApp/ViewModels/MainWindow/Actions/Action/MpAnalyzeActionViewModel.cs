using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpAnalyzeActionViewModel : MpActionViewModelBase {
        #region Properties

        #region View Models

        public MpAnalyticItemPresetViewModel SelectedPreset {
            get {
                if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return null;
                }
                return MpAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == AnalyticItemPresetId);
            }
            set {
                if(SelectedPreset != value) {
                    AnalyticItemPresetId = value.AnalyticItemPresetId;
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

        public MpAnalyzeActionViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Public Overrides

        public override async Task PerformAction(object arg) {
            if (ActionId == 597) {
                MpConsole.WriteLine("Classifier Perform Action Called");
            }
            if (!CanPerformAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Action.ActionObjId);
            
            object[] args = new object[] { aipvm, actionInput.CopyItem };
            if(aipvm != null && 
               aipvm.Parent != null &&
               aipvm.Parent.ExecuteAnalysisCommand.CanExecute(args)) {
                aipvm.Parent.ExecuteAnalysisCommand.Execute(args);

                while (aipvm.Parent.IsBusy) {
                    await Task.Delay(100);
                }

                await base.PerformAction(
                    new MpActionOutput() {
                        Previous = arg as MpActionOutput,
                        CopyItem = actionInput.CopyItem,
                        OutputData = aipvm.Parent.LastTransaction.Response
                    });
            }
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpAnalyticItemPreset aip && aip.Id == AnalyticItemPresetId) {
                Task.Run(Validate);
            }
        }

        protected override async Task<bool> Validate() {
            IsBusy = true;

            await base.Validate();
            if (!IsValid) {
                return IsValid;
            }

            var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Action.ActionObjId);
            if (aipvm == null) {
                ValidationText = $"Analyzer for Action '{RootTriggerActionViewModel.Label}/{Label}' not found";
                await ShowValidationNotification();
            } else {
                var pavm = ParentActionViewModel;
                while(pavm != null) {
                    if(pavm is MpCompareActionViewModelBase cavm) {
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
