using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpMatcherCollectionViewModel : MpSingletonViewModel<MpMatcherCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpMatcherViewModel> Matchers { get; set; } = new ObservableCollection<MpMatcherViewModel>();

        #endregion

        #endregion

        #region Constructors

        public MpMatcherCollectionViewModel() {
            Task.Run(Init);
        }

        public async Task Init() {
            var matchers = await MpDb.Instance.GetItemsAsync<MpMatcher>();

            foreach (var m in matchers) {
                var mvm = await CreateMatcherViewModel(m);
                Matchers.Add(mvm);
            }

        }

        #endregion

        #region Public Methods

        public async Task<MpMatcherViewModel> CreateMatcherViewModel(MpMatcher m) {
            MpMatcherViewModel mvm = new MpMatcherViewModel(this);
            await mvm.InitializeAsync(m);            
            return mvm;
        }

        #endregion

        private bool IsMatch(MpMatcher matcher, string text) {
            switch(matcher.MatcherType) {
                case MpMatcherType.Contains:
                    break;
            }

            return false;
        }

        private void PerformCommand(MpMatchCommand mc, MpMatcher m, object arg) {
            switch(mc.MatcherCommandType) {
                case MpMatchActionType.Analyzer:

                    break;
            }
        }
    }
}
