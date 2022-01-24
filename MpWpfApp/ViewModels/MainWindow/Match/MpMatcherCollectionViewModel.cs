using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpMatcherCollectionViewModel : 
        MpViewModelBase, 
        MpISingletonViewModel<MpMatcherCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpMatcherViewModel> Matchers { get; set; } = new ObservableCollection<MpMatcherViewModel>();

        #endregion

        #region State

        public bool IsVisible { get; set; } = false;

        #endregion

        #endregion

        #region Constructors

        private static MpMatcherCollectionViewModel _instance;
        public static MpMatcherCollectionViewModel Instance => _instance ?? (_instance = new MpMatcherCollectionViewModel());


        public MpMatcherCollectionViewModel() : base(null) {
            PropertyChanged += MpMatcherCollectionViewModel_PropertyChanged;
        }

        public async Task Init() {
            var matchers = await MpDb.GetItemsAsync<MpMatcher>();

            foreach (var m in matchers.Where(x=>x.ParentMatcherId == 0).OrderBy(x=>x.SortOrderIdx)) {
                var mvm = await CreateMatcherViewModel(m);
                Matchers.Add(mvm);
            }

            LinkAllTriggers();
        }

        #endregion

        #region Public Methods

        public async Task<MpMatcherViewModel> CreateMatcherViewModel(MpMatcher m) {
            MpMatcherViewModel mvm = new MpMatcherViewModel(this);
            await mvm.InitializeAsync(m);            
            return mvm;
        }

        public void LinkAllTriggers() {
            Matchers.ForEach(x => x.Enable());
        }

        public void UnlinkAllTriggers() {
            Matchers.ForEach(x => x.Disable());
        }

        public string GetUniqueMatcherName(string prefix = "Matcher") {
            int uniqueIdx = 1;
            string uniqueName = string.IsNullOrEmpty(prefix) ? $"Matcher": prefix;
            string testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);

            while (Matchers.Any(x => x.Title.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
            }
            return uniqueName + uniqueIdx;
        }

        #endregion

        #region Private Methods
        private void MpMatcherCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsVisible):
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.IsGridSplitterEnabled));
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.AppModeButtonGridMinWidth));

                    //if (IsVisible) {
                    //    MpAnalyticItemCollectionViewModel.Instance.IsVisible = false;
                    //    MpTagTrayViewModel.Instance.IsVisible = false;
                    //}
                    break;
            }
        }

        private bool IsMatch(MpMatcher matcher, string text) {
            switch(matcher.MatcherType) {
                case MpMatcherType.Contains:
                    break;
            }

            return false;
        }

        private void PerformCommand(MpMatchCommand mc, MpMatcher m, object arg) {
            switch(mc.MatcherCommandType) {
                case MpMatcherActionType.Analyze:

                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand AddMatcherCommand => new RelayCommand<object>(
             async (args) => {
                 if (args is MpAnalyticItemPresetViewModel aipvm) {
                     var nm = await MpMatcher.Create(
                         MpMatcherType.None,
                         GetUniqueMatcherName(),
                         string.Empty,
                         string.Empty,
                         MpMatcherTriggerType.None,
                         MpMatcherActionType.Analyze,
                         aipvm.AnalyticItemPresetId,
                         MpMatcherActionType.None,
                         0);

                     var nmvm = await CreateMatcherViewModel(nm);

                     Matchers.Add(nmvm);
                     aipvm.OnPropertyChanged(nameof(aipvm.MatcherViewModels));
                } else if (args is MpMatcherViewModel mvm) {
                     var nm = await MpMatcher.Create(
                         MpMatcherType.None,
                         GetUniqueMatcherName(),
                         string.Empty,
                         string.Empty,
                         MpMatcherTriggerType.ParentMatchOutput,
                         MpMatcherActionType.None,
                         mvm.MatcherId,
                         MpMatcherActionType.None,
                         0,
                         mvm.MatcherId,
                         mvm.MatcherViewModels.Count);

                     await mvm.InitializeAsync(mvm.Matcher);
                     mvm.OnPropertyChanged(nameof(mvm.MatcherViewModels));
                 }
             }, (args) => args != null);

        public ICommand DeleteMatcherCommand => new RelayCommand<object>(
            async (args) => {
                if(args is MpMatcherViewModel mvm) {
                    Matchers.Remove(mvm);
                    await mvm.Matcher.DeleteFromDatabaseAsync();
                    if(mvm.IsEnabled) {
                        mvm.Disable();
                    }
                    if(mvm.ParentMatcherViewModel != null) {
                        await mvm.ParentMatcherViewModel.InitializeAsync(mvm.ParentMatcherViewModel.Matcher);
                    }
                }
            }, (args) => args != null);
        #endregion
    }
}
