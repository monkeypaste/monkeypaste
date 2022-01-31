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
        MpISingletonViewModel<MpActionCollectionViewModel>,
        MpIResizableViewModel,
        MpISidebarItemViewModel {
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
                        Command = AddTriggerCommand,
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

        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion

        #region MpISidebarItemViewModel Implementation

        public double DefaultSidebarWidth { get; } = 150;
        public bool IsSidebarVisible { get; set; } = false;

        public MpISidebarItemViewModel NextSidebarItem { get; set; }
        public MpISidebarItemViewModel PreviousSidebarItem { get; set; }

        #endregion

        #region Appearance

        //public double ActionTreeHeight 
        #endregion

        #region State


        public bool IsAnyTextBoxFocused => SelectedItem != null && SelectedItem.IsAnyTextBoxFocused;

        #endregion

        #endregion

        #region Constructors

        private static MpActionCollectionViewModel _instance;
        public static MpActionCollectionViewModel Instance => _instance ?? (_instance = new MpActionCollectionViewModel());


        public MpActionCollectionViewModel() : base(null) {
            PropertyChanged += MpMatcherCollectionViewModel_PropertyChanged;
        }

        public async Task Init() {
            IsBusy = true;

            var tal = await MpDataModelProvider.GetAllTriggerActions();

            foreach (var ta in tal) {
                var tavm = await CreateTriggerViewModel(ta);
                Items.Add(tavm);
            }

            EnabledAll();

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public async Task<MpActionViewModelBase> CreateTriggerViewModel(MpAction a) {
            
            if(a.ActionType != MpActionType.Trigger || 
               (MpTriggerType) a.ActionObjId == MpTriggerType.None || 
               (MpTriggerType)a.ActionObjId == MpTriggerType.ParentOutput) {
                throw new Exception("This is only supposed to load root level triggers");
            }
            MpTriggerActionViewModelBase tavm = null;
            switch ((MpTriggerType)a.ActionObjId) {
                case MpTriggerType.ContentItemAdded:
                    tavm = new MpContentAddTriggerViewModel(this);
                    break;
                case MpTriggerType.ContentItemAddedToTag:
                    tavm = new MpContentTaggedTriggerViewModel(this);
                    break;
                case MpTriggerType.FileSystemChange:
                    tavm = new MpFileSystemTriggerViewModel(this);
                    break;
                case MpTriggerType.Shortcut:
                    tavm = new MpShortcutTriggerViewModel(this);
                    break;
                //case MpTriggerType.ParentOutput:
                //    tavm = new MpParentOutputTriggerViewModel(this);
                //    break;
            }

            await tavm.InitializeAsync(a);            
            return tavm;
        }

        public void EnabledAll() {
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.ParentActionViewModel)));
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.Children)));
            Items.ForEach(x => x.Enable());
        }

        public void DisableAll() {
            Items.ForEach(x => x.Disable());
        }

        public string GetUniqueTriggerName(string prefix) {
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

        private async Task UpdateSortOrder() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        private void MpMatcherCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSidebarVisible):
                    MpSidebarViewModel.Instance.OnPropertyChanged(nameof(MpSidebarViewModel.Instance.IsAnySidebarOpen));
                    

                    if (IsSidebarVisible) {
                        MpAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpTagTrayViewModel.Instance.IsSidebarVisible = false;

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

        public ICommand ShowActionSelectorMenuCommand => new RelayCommand<object>(
             (args) => {
                 var fe = args as FrameworkElement;
                 var cm = new MpContextMenuView();
                 cm.DataContext = MenuItemViewModel;
                 fe.ContextMenu = cm;
                 fe.ContextMenu.PlacementTarget = fe;
                 fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
                 fe.ContextMenu.IsOpen = true;
             });

        public ICommand AddTriggerCommand => new RelayCommand<object>(
             async (args) => {
                 IsBusy = true;

                 MpTriggerType tt = args == null ? MpTriggerType.None : (MpTriggerType)args;
                 
                 MpAction na = await MpAction.Create(
                         label: GetUniqueTriggerName("Trigger"),
                         actionType: MpActionType.Trigger,
                         actionObjId: (int)tt,
                         sortOrderIdx: Items.Count);

                 var navm = await CreateTriggerViewModel(na);

                 Items.Add(navm);

                 SelectedItem = navm;

                 OnPropertyChanged(nameof(Items));

                 IsBusy = false;
             });

        public ICommand DeleteTriggerCommand => new RelayCommand<object>(
            async (args) => {
                var tavm = args as MpTriggerActionViewModelBase;
                tavm.Disable();
                var deleteTasks = tavm.FindAllChildren().Select(x => x.Action.DeleteFromDatabaseAsync()).ToList();
                deleteTasks.Add(tavm.Action.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                Items.Remove(tavm);

                await UpdateSortOrder();
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItem));
            });

        #endregion
    }
}
