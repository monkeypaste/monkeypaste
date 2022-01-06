using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpMatcherCollectionViewModel : MpSingletonViewModel<MpMatcherCollectionViewModel> {
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

        public MpMatcherCollectionViewModel() {
            PropertyChanged += MpMatcherCollectionViewModel_PropertyChanged;
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
                case MpMatchActionType.Analyzer:

                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand AddMatcherCommand => new RelayCommand<object>(
            async (args) => {
                if(args is MpAnalyticItemPresetViewModel aipvm) {

                }
            }, (args) => args != null);

        public ICommand DeleteMatcherCommand => new RelayCommand<object>(
            async (args) => {
                if(args is MpMatcherViewModel mvm) {
                    Matchers.Remove(mvm);
                    await mvm.Matcher.DeleteFromDatabaseAsync();
                }
            }, (args) => args != null);
        #endregion
    }
}
