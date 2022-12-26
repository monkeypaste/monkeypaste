using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Selection;

namespace MonkeyPaste.Avalonia {
    public class MpAvTriggerCollectionViewModel :
        MpAvSelectorViewModelBase<object, MpAvTriggerActionViewModelBase>,
        MpIPopupMenuViewModel,
        MpIAsyncComboBoxViewModel,
        MpIAsyncSingletonViewModel<MpAvTriggerCollectionViewModel>,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpISidebarItemViewModel {
        #region Private Variables

        #endregion

        #region MpIPopupMenuViewModel Implementation
        bool MpIPopupMenuViewModel.IsPopupMenuOpen { get; set; }
        public MpMenuItemViewModel PopupMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems =
                                typeof(MpTriggerType)
                                .EnumerateEnum<MpTriggerType>()
                                .Where(x => x != MpTriggerType.None)
                                .Select(x =>
                                    new MpMenuItemViewModel() {
                                        Header = x.EnumToLabel(),
                                        IconResourceKey = MpAvActionViewModelBase.GetDefaultActionIconResourceKey(MpActionType.Trigger, x),
                                        Command = AddTriggerCommand,
                                        CommandParameter = x
                                    }).ToList()
                };

                var tmivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpTriggerType).EnumToLabels("Select Trigger");
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    switch ((MpTriggerType)i) {
                        case MpTriggerType.ContentAdded:
                            resourceKey = "ClipboardImage";
                            break;
                        case MpTriggerType.ContentTagged:
                            resourceKey = "PinToCollectionImage";
                            break;
                        case MpTriggerType.FileSystemChange:
                            resourceKey = "FolderEventImage";
                            break;
                        case MpTriggerType.Shortcut:
                            resourceKey = "HotkeyImage";
                            break;
                        case MpTriggerType.ParentOutput:
                            resourceKey = "ChainImage";
                            break;
                    }
                    var tt = (MpTriggerType)i;
                    tmivml.Add(new MpMenuItemViewModel() {
                        IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource(resourceKey) as string, //MpPlatformWrapper.Services.PlatformResource.GetResource(resourceKey) as string,
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

        #region MpIAsyncComboBoxViewModel Implementation

        IEnumerable<MpIComboBoxItemViewModel> MpIAsyncComboBoxViewModel.Items => Items;
        MpIComboBoxItemViewModel MpIAsyncComboBoxViewModel.SelectedItem {
            get => SelectedItem;
            set => SelectedItem = (MpAvTriggerActionViewModelBase)value;
        }
        private bool _isTriggerDropDownOpen = false;
        bool MpIAsyncComboBoxViewModel.IsDropDownOpen {
            get => _isTriggerDropDownOpen;
            set => _isTriggerDropDownOpen = value;
        }

        #endregion


        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISidebarItemViewModel Implementation

        private double _defaultSelectorColumnVarDimLength = 350;
        private double _defaultParameterColumnVarDimLength = 625;
        public double DefaultSidebarWidth {
            get {
                if (MpAvMainWindowViewModel.Instance.IsVerticalOrientation) {
                    return MpAvMainWindowViewModel.Instance.MainWindowWidth;
                }
                double w = _defaultSelectorColumnVarDimLength;
                if (SelectedItem != null) {
                    w += _defaultParameterColumnVarDimLength;
                }
                return w;
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvClipTrayViewModel.Instance.ClipTrayScreenHeight;
                }
                double h = _defaultSelectorColumnVarDimLength;
                //if (SelectedItem != null) {
                //    //h += _defaultParameterColumnVarDimLength;
                //}
                return h;
            }
        }
        public double SidebarWidth { get; set; } = 0;
        public double SidebarHeight { get; set; } = 0;
        public bool IsSidebarVisible { get; set; } = false;

        #endregion
        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region Properties
        public static CancellationTokenSource CTS = new CancellationTokenSource();

        #region View Models       

        public MpAvActionViewModelBase PrimaryAction =>
            SelectedItem == null ? null : SelectedItem.SelectedItem;

        public IEnumerable<MpAvActionViewModelBase> AllActions => 
            Items.SelectMany(x => x.SelfAndAllDescendants).Cast<MpAvActionViewModelBase>();

        public IEnumerable<MpAvActionViewModelBase> AllSelectedItemActions =>
            SelectedItem == null ? null : SelectedItem.SelfAndAllDescendants.Cast<MpAvActionViewModelBase>();

        #endregion


        

        #region Appearance

        public string AddNewButtonBorderBrushHexColor {
            get {
                if (IsHovering) {
                    return MpSystemColors.LightGray;
                }
                return MpSystemColors.DarkGray;
            }
        }

        #endregion

        #region State

        public bool IsAnyBusy {
            get {
                if(IsBusy) {
                    return true;
                }
                return Items.Any(x => x.IsAnyBusy);
            }
        }
        #endregion

        #endregion

        #region Constructors

        private static MpAvTriggerCollectionViewModel _instance;
        public static MpAvTriggerCollectionViewModel Instance => _instance ?? (_instance = new MpAvTriggerCollectionViewModel());


        public MpAvTriggerCollectionViewModel() : base(null) {
            PropertyChanged += MpMatcherCollectionViewModel_PropertyChanged;
        }



        #endregion

        #region Public Methods

        public async Task InitAsync() {
            IsBusy = true;

            MpConsole.WriteLine("Action Collectoin Init!");

            Items.Clear();
            var tal = await MpDataModelProvider.GetAllTriggerActionsAsync();

            foreach (var ta in tal) {
                var tavm = await CreateTriggerViewModel(ta);
                Items.Add(tavm);
            }

            while (Items.Any(x => x.IsAnyBusy)) {
                // wait for all action trees to initialize before enabling
                await Task.Delay(100);
            }

            Items.ForEach(x => x.OnPropertyChanged(nameof(x.ParentTreeItem)));
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.Children)));
            OnPropertyChanged(nameof(Items));

            await RestoreAllEnabled();//.FireAndForgetSafeAsync(this);

            if(AllActions.Count() > 0) {
                // select most recent action
                MpAvActionViewModelBase actionToSelect = AllActions
                                .Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);

                if (actionToSelect != null) {
                    //SelectedItem = actionToSelect.RootTriggerActionViewModel;
                    //actionToSelect.IsSelected = true;
                    //OnPropertyChanged(nameof(SelectedItem));
                    //SelectedItem.OnPropertyChanged(nameof(SelectedActions));
                    SelectActionCommand.Execute(actionToSelect);
                }
            }
            

            IsBusy = false;
        }
        public async Task<MpAvTriggerActionViewModelBase> CreateTriggerViewModel(MpAction a) {
            
            if(a.ActionType != MpActionType.Trigger || 
               (MpTriggerType)a.ActionObjId == MpTriggerType.ParentOutput) {
                throw new Exception("This is only supposed to load root level triggers");
            }
            MpAvTriggerActionViewModelBase tavm = null;
            switch ((MpTriggerType)a.ActionObjId) {
                case MpTriggerType.None:
                    tavm = new MpAvTriggerActionViewModelBase(this);
                    break;
                case MpTriggerType.ContentAdded:
                    tavm = new MpAvContentAddTriggerViewModel(this);
                    break;
                case MpTriggerType.ContentTagged:
                    tavm = new MpAvContentTaggedTriggerViewModel(this);
                    break;
                case MpTriggerType.FileSystemChange:
                    tavm = new MpAvFileSystemTriggerViewModel(this);
                    break;
                case MpTriggerType.Shortcut:
                    tavm = new MpAvShortcutTriggerViewModel(this);
                    break;
            }

            await tavm.InitializeAsync(a);

            return tavm;
        }

        public async Task RestoreAllEnabled() {
            // NOTE this is only called on init and needs to wait for dependant vm's to load so wait here

            //while(MpAvTagTrayViewModel.Instance.IsBusy)

            foreach(var avm in AllActions) {
                // TODO this could be optimized by toggling enabled in parallel
                if(avm.IsEnabledDb) {
                    avm.ToggleIsEnabledCommand.Execute(true);
                    while(avm.IsBusy) {
                        await Task.Delay(100);
                    }
                }
            }

            //while(AllActions.Any(x=>x.IsBusy)) {
            //    await Task.Delay(100);
            //}
        }

        public async Task DisableAll() {
            await Task.Delay(1);
            Items.ForEach(x => x.ToggleIsEnabledCommand.Execute(false));
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

        #region Protected Methods



        #endregion

        #region Private Methods

        private void Selection_SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs<MpAvActionViewModelBase> e) {
            foreach(var avmb in AllActions) {
                avmb.IsSelected = e.SelectedItems.Contains(avmb);
            }
        }

        private async Task UpdateSortOrder() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        private void MpMatcherCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                //case nameof(IsSidebarVisible):
                //    if (IsSidebarVisible) {
                //        MpAvAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                //        MpAvTagTrayViewModel.Instance.IsSidebarVisible = false;
                //        MpAvClipboardHandlerCollectionViewModel.Instance.IsSidebarVisible = false;
                //    }
                //    MpAvMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel));
                //    break;
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(IsAnySelected));
                    OnPropertyChanged(nameof(AllSelectedItemActions));
                    OnPropertyChanged(nameof(PrimaryAction));
                    break;
                case nameof(PrimaryAction):
                    OnPropertyChanged(nameof(IsAnySelected));
                    break;
                case nameof(HasModelChanged):
                    if(SelectedItem == null) {
                        return;
                    }
                    if(HasModelChanged) {
                        Task.Run(async () => {
                            await SelectedItem.Action.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ResetDesignerCommand => new MpCommand(
            () => {

            });

        public ICommand ShowTriggerSelectorMenuCommand => new MpCommand<object>(
             (args) => {
                 var control = args as Control;
                 MpAvMenuExtension.ShowMenu(control, PopupMenuViewModel, MpPoint.Zero);
             }, (args) => args is Control);

        public ICommand AddTriggerCommand => new MpCommand<object>(
             async (args) => {
                 IsBusy = true;
                                  
                 MpTriggerType tt = args == null ? MpTriggerType.None : (MpTriggerType)args;
                 
                 MpAction na = await MpAction.CreateAsync(
                         label: GetUniqueTriggerName(tt.ToString()),
                         actionType: MpActionType.Trigger,
                         actionObjId: (int)tt,
                         sortOrderIdx: Items.Count);

                 var new_trigger_vm = await CreateTriggerViewModel(na);

                 while(new_trigger_vm.IsBusy) {
                     await Task.Delay(100);
                 }

                 Items.Add(new_trigger_vm);
                 await Task.Delay(300);
                 SelectActionCommand.Execute(new_trigger_vm);
                 await Task.Delay(300);
                 SelectedItem.ToggleIsEnabledCommand.Execute(null);

                 IsBusy = false;
             });

        public ICommand DeleteTriggerCommand => new MpCommand<object>(
            async (args) => {
                IsBusy = true;

                var tavm = args as MpAvTriggerActionViewModelBase;
                tavm.ToggleIsEnabledCommand.Execute(false);

                var deleteTasks = tavm.FindAllChildren().Cast<MpAvActionViewModelBase>().Select(x => x.Action.DeleteFromDatabaseAsync()).ToList();
                deleteTasks.Add(tavm.Action.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                Items.Remove(tavm);

                OnPropertyChanged(nameof(AllSelectedItemActions));

                await UpdateSortOrder();
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItem));

                IsBusy = false;
            });

        public ICommand SelectActionCommand => new MpCommand<object>(
            (args) => {
                MpAvActionViewModelBase toSelect_avmb = args as MpAvActionViewModelBase;
                if(toSelect_avmb == null && args is int actionId) {
                    toSelect_avmb = AllActions.FirstOrDefault(x => x.ActionId == actionId);
                }

                if (!IsSidebarVisible) {
                    IsSidebarVisible = true;
                }

                if(toSelect_avmb != null) {
                    SelectedItem = toSelect_avmb.RootTriggerActionViewModel;
                    SelectedItem.SelectedItem = toSelect_avmb;

                    SelectedItem.SelfAndAllDescendants.Cast<MpAvActionViewModelBase>()
                        .ForEach(x => x.OnPropertyChanged(nameof(x.IsSelectedAction)));


                    if (this is MpIAsyncComboBoxViewModel cmbivm) {
                        cmbivm.OnPropertyChanged(nameof(cmbivm.SelectedItem));
                    }
                }
            });



        #endregion
    }
}
