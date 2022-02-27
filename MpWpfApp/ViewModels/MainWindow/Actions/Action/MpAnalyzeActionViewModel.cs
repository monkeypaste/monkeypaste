using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpAnalyzeOutput : MpActionOutput {
        public MpCopyItem NewContentItem { get; set; }
    }

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
            if(!CanPerformAction(arg)) {
                return;
            }

            MpCopyItem ci = null;
            if(arg is MpCopyItem) {
                ci = arg as MpCopyItem;
            } else if(arg is MpCompareOutput co) {
                ci = co.CopyItem;
            } else if (arg is MpAnalyzeOutput ao) {
                ci = ao.CopyItem;
            } else if (arg is MpClassifyOutput clo) {
                ci = clo.CopyItem;
            }

            var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Action.ActionObjId);
            
            object[] args = new object[] { aipvm, ci };
            if(aipvm != null && 
               aipvm.Parent != null &&
               aipvm.Parent.ExecuteAnalysisCommand.CanExecute(args)) {
                aipvm.Parent.ExecuteAnalysisCommand.Execute(args);

                while (aipvm.Parent.IsBusy) {
                    await Task.Delay(100);
                }

                MpCopyItem nci = null;
                if (aipvm.Parent.LastResultContentItem != null &&
                   (aipvm.Parent.LastResultContentItem.Id != ci.Id ||
                   aipvm.Parent.LastResultContentItem.Id == 0)) {
                    nci = aipvm.Parent.LastResultContentItem;
                }
                await base.PerformAction(new MpAnalyzeOutput() {
                    Previous = arg as MpActionOutput,
                    CopyItem = ci,
                    NewContentItem = nci,
                    OutputData = aipvm.Parent.LastTransaction.Response
                });
            }
        }

        #endregion

        #region Protected Methods


        protected override async Task<bool> Validate() {
            await base.Validate();
            if (!IsValid) {
                return IsValid;
            }

            var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Action.ActionObjId);
            if (aipvm == null) {
                ValidationText = $"Analyzer for Action '{RootTriggerActionViewModel.Label}/{Label}' not found";
                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        #endregion
    }
}
