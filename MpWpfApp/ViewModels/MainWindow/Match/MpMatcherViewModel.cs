using MonkeyPaste;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpMatcherViewModel : MpViewModelBase<MpMatcherCollectionViewModel> {

        #region Properties

        #region View Models

        
        #endregion

        #region Model

        public MpMatcher Matcher { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpMatcherViewModel() : base(null) { }

        public MpMatcherViewModel(MpMatcherCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpMatcher m) {
            IsBusy = true;

            Matcher = m;

            switch(Matcher.TriggerType) {
                case MpMatchTriggerType.Content:
                    MpClipTrayViewModel.Instance.OnCopyItemItemAdd += Instance_OnCopyItemItemAdd;
                    break;
            }

            await Task.Delay(1);

            IsBusy = false;
        }


        private void CheckForMatch(object arg) {
            Task.Run(async () => {
                MpCopyItem nci = arg as MpCopyItem;

                string compareStr = string.Empty;

                switch (Matcher.TriggerActionType) {
                    case MpMatchActionType.Analyzer:

                        var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Matcher.TriggerActionObjId);
                        object[] args = new object[] { aipvm, nci };
                        aipvm.Parent.ExecuteAnalysisCommand.Execute(args);

                        while (aipvm.Parent.IsBusy) {
                            await Task.Delay(100);
                        }

                        if (aipvm.Parent.LastTransaction == null) {
                            return;
                        }

                        compareStr = aipvm.Parent.LastTransaction.Response.ToString();
                        break;
                }

                switch(Matcher.MatcherType) {
                    case MpMatcherType.Contains:
                        if(compareStr.ToLower().Contains(Matcher.MatchData.ToLower())) {
                            await PerformIsMatchAction(arg);
                        }
                        break;
                }
            });            
        }

        private async Task PerformIsMatchAction(object arg) {
            switch(Matcher.IsMatchActionType) {
                case MpMatchActionType.Classifier:
                    var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagId == Matcher.IsMatchTargetObjectId);
                    await ttvm.AddContentItem((arg as MpCopyItem).Id);
                    break;
            }
        }

        private void Instance_OnCopyItemItemAdd(object sender, object e) {
            CheckForMatch(e);
        }


        #endregion

    }
}
