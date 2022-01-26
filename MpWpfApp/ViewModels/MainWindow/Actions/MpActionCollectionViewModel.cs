using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpActionCollectionViewModel : 
        MpSelectorViewModelBase<object,MpActionViewModelBase>, 
        MpIMenuItemViewModel,
        MpISingletonViewModel<MpActionCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                var tmivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpTriggerType).EnumToLabels();
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    switch((MpTriggerType)i) {
                        case MpTriggerType.ContentItemAdded:
                            resourceKey = "ClipboardIcon";
                            break;
                        case MpTriggerType.ContentItemAddedToTag:
                            resourceKey = "PinToCollectionIcon";
                            break;
                        case MpTriggerType.FileSystemChange:
                            resourceKey = "FolderEventIcon";
                            break;
                        case MpTriggerType.Shortcut:
                            resourceKey = "HotkeyIcon";
                            break;
                        case MpTriggerType.ParentOutput:
                            resourceKey = "ChainIcon";
                            break;
                    }
                    var tt = (MpTriggerType)i;
                    tmivml.Add(new MpMenuItemViewModel() {
                        IconResourceKey = Application.Current.Resources[resourceKey] as string,
                        Header = triggerLabels[i],
                        Command = AddActionCommand,
                        CommandParameter = tt,
                        IsVisible = tt != MpTriggerType.None && (tt != MpTriggerType.ParentOutput || (SelectedItem != null && !SelectedItem.IsRootAction))
                    });
                }
                return new MpMenuItemViewModel() {
                    SubItems = tmivml
                };
            }
        }

        #endregion

        #region State

        public bool IsVisible { get; set; } = false;

        public bool HasItems => Items.Count > 0;

        public bool IsAnySelected => SelectedItem != null;// SelectedItems.Count > 0;
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

            EnabledAll();
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

        public void EnabledAll() {
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.ParentActionViewModel)));
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.Children)));
            Items.ForEach(x => x.Enable());
        }

        public void DisableAll() {
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
                case nameof(SelectedItem):
                    //SelectedItems.Clear();
                    //if(SelectedItem != null) {
                    //    if(SelectedItem.IsRootAction) {
                    //        SelectedItems.Add(SelectedItem);
                    //    } else {
                    //        var rsavm = SelectedItem.ParentActionViewModel;
                    //        while (rsavm.ParentActionViewModel != null) {
                    //            rsavm = rsavm.ParentActionViewModel;
                    //        }
                    //        if(rsavm == null) {
                    //            throw new Exception("Error unlinked sub item");
                    //        }
                    //        SelectedItems.Add(rsavm);
                    //    }
                        
                        
                    //}
                    //OnPropertyChanged(nameof(SelectedItems));
                    OnPropertyChanged(nameof(IsAnySelected));
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand AddActionCommand => new RelayCommand<object>(
             async (args) => {
                 MpAction na = null;
                 if(args is MpTriggerType tt) {
                     na = await MpAction.Create(
                         label: GetUniqueMatcherName(),
                         actionType: MpActionType.Trigger,
                         actionObjId: (int)tt);


                 } else if (args is MpActionType at) {
                     if(SelectedItem == null) {
                         throw new Exception("Action must have context");
                     }
                     na = await MpAction.Create(
                         label: GetUniqueMatcherName(),
                         actionType: at,
                         0,
                         SelectedItem.ActionId);
                 }

                 var navm = await CreateActionViewModel(na);

                 Items.Add(navm);

                 if(navm.IsRootAction) {
                     //SelectedItems.Clear();
                     SelectedItem = navm;
                 }
                 
                 if(navm.ParentActionViewModel != null) {
                     navm.ParentActionViewModel.OnPropertyChanged(nameof(navm.ParentActionViewModel.Children));
                 }

                 //OnPropertyChanged(nameof(Items));
             });

        public ICommand DeleteActionCommand => new RelayCommand<object>(
            async (args) => {
                if(args is MpActionViewModelBase mvm) {
                    mvm = Items.FirstOrDefault(x => x.ActionId == mvm.ActionId);
                    if(mvm == null) {
                        Debugger.Break();
                    }
                    Items.Remove(mvm);
                    await mvm.Action.DeleteFromDatabaseAsync();
                    if(mvm.IsEnabled) {
                        mvm.Disable();
                    }
                    if(mvm.ParentActionViewModel != null) {
                        await mvm.ParentActionViewModel.InitializeAsync(mvm.ParentActionViewModel.Action);
                    } else {
                        mvm.OnPropertyChanged(nameof(mvm.Children));
                    }
                    OnPropertyChanged(nameof(Items));
                    OnPropertyChanged(nameof(SelectedItem));
                }
            });
        #endregion
    }
}
