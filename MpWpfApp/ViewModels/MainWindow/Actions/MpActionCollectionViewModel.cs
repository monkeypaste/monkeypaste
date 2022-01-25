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
    public class MpActionCollectionViewModel : 
        MpSelectorViewModelBase<object,MpActionViewModelBase>, 
        MpISingletonViewModel<MpActionCollectionViewModel>,
        MpISelectorViewModel<MpActionViewModelBase> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        
        #endregion

        #region State

        public bool IsVisible { get; set; } = false;

        public bool HasItems => Items.Count > 0;
        #endregion

        #endregion

        #region Constructors

        private static MpActionCollectionViewModel _instance;
        public static MpActionCollectionViewModel Instance => _instance ?? (_instance = new MpActionCollectionViewModel());


        public MpActionCollectionViewModel() : base(null) {
            PropertyChanged += MpMatcherCollectionViewModel_PropertyChanged;
        }

        public async Task Init() {
            var al = await MpDb.GetItemsAsync<MpAction>();

            foreach (var m in al) {
                var mvm = await CreateActionViewModel(m);
                Items.Add(mvm);
            }

            LinkAllTriggers();
        }

        public IList<MpActionViewModelBase> FindChildren(MpActionViewModelBase avm) {
            return Items.Where(x => x.ParentActionId == avm.ActionId).ToList();
        }

        #endregion

        #region Public Methods

        public async Task<MpActionViewModelBase> CreateActionViewModel(MpAction a) {
            MpActionViewModelBase mvm = null;
            switch(a.ActionType) {
                case MpActionType.Trigger:
                    switch ((MpTriggerType)a.ActionObjId) {
                        case MpTriggerType.ContentItemAdded:
                            mvm = new MpContentAddTriggerViewModel(this);
                            break;
                        case MpTriggerType.ContentItemAddedToTag:
                            mvm = new MpContentTaggedTriggerViewModel(this);
                            break;
                        case MpTriggerType.FileSystemChange:
                            mvm = new MpFileSystemTriggerViewModel(this);
                            break;
                        case MpTriggerType.Shortcut:
                            mvm = new MpShortcutTriggerViewModel(this);
                            break;
                        case MpTriggerType.ParentOutput:
                            mvm = new MpParentOutputTriggerViewModel(this);
                            break;

                    }
                    break;
                case MpActionType.Analyze:
                    mvm = new MpAnalyzeActionViewModel(this);
                    break;
                case MpActionType.Classify:
                    mvm = new MpClassifyActionViewModel(this);
                    break;
                case MpActionType.Compare:
                    mvm = new MpCompareActionViewModel(this);
                    break;
            }

            await mvm.InitializeAsync(a);            
            return mvm;
        }

        public void LinkAllTriggers() {
            Items.ForEach(x => x.Enable());
        }

        public void UnlinkAllTriggers() {
            Items.ForEach(x => x.Disable());
        }

        public string GetUniqueMatcherName(string prefix = "Action") {
            int uniqueIdx = 1;
            string testName = string.Format(
                                        @"{0}{1}",
                                        prefix.ToLower(),
                                        uniqueIdx);

            while (Items.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        prefix.ToLower(),
                                        uniqueIdx);
            }
            return prefix + uniqueIdx;
        }

        #endregion

        #region Private Methods

        private void MpMatcherCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsVisible):
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.IsGridSplitterEnabled));
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.AppModeButtonGridMinWidth));

                    if (IsVisible) {
                        MpAnalyticItemCollectionViewModel.Instance.IsVisible = false;
                        MpTagTrayViewModel.Instance.IsVisible = false;

                    }
                    break;
            }
        }


        #endregion

        #region Commands

        public ICommand AddActionCommand => new RelayCommand<object>(
             async (args) => {
                 MpAction na = null;
                 if (args is MpAnalyticItemPresetViewModel aipvm) {
                     na = await MpAction.Create(
                         GetUniqueMatcherName($"{aipvm.Label} Action"),
                         MpActionType.Analyze,
                         aipvm.AnalyticItemPresetId);
                     
                } else if (args is MpActionViewModelBase mvm) {
                     na = await MpAction.Create(
                         GetUniqueMatcherName($"{mvm.Label} Action"),
                         MpActionType.Trigger,
                         (int)MpTriggerType.ParentOutput);
                 } else {
                     na = await MpAction.Create(
                         GetUniqueMatcherName(),
                         MpActionType.Trigger,
                         (int)MpTriggerType.ContentItemAdded);
                 }
                 var navm = await CreateActionViewModel(na);

                 Items.Add(navm);

                 SelectedItem = navm;
             });

        public ICommand DeleteActionCommand => new RelayCommand<object>(
            async (args) => {
                if(args is MpActionViewModelBase mvm) {
                    Items.Remove(mvm);
                    await mvm.Action.DeleteFromDatabaseAsync();
                    if(mvm.IsEnabled) {
                        mvm.Disable();
                    }
                    if(mvm.ParentActionViewModel != null) {
                        await mvm.ParentActionViewModel.InitializeAsync(mvm.ParentActionViewModel.Action);
                    }
                }
            }, (args) => args != null);
        #endregion
    }
}
