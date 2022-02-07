using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override async Task PerformAction(MpCopyItem arg) {
            var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Action.ActionObjId);
            object[] args = new object[] { aipvm, arg as MpCopyItem };
            aipvm.Parent.ExecuteAnalysisCommand.Execute(args);

            while (aipvm.Parent.IsBusy) {
                await Task.Delay(100);
            }

            await base.PerformAction(aipvm.Parent.LastResultContentItem);
        }
        #endregion
    }
}
