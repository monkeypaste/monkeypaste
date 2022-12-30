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
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
using Avalonia.Controls.Selection;
using Gtk;

namespace MonkeyPaste.Avalonia {
    public class MpAvTriggerCollectionViewModel :
        MpViewModelBase,
        MpIPopupMenuViewModel,
        //MpIAsyncComboBoxViewModel,
        MpIAsyncSingletonViewModel<MpAvTriggerCollectionViewModel>,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpISidebarItemViewModel,
        MpIDesignerSettingsViewModel {
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
            }
        }

        #endregion

        #region MpIDesignerSettingsViewModel Implementation

        public double MinScale => SelectedTrigger == null ? MpAvTriggerActionViewModelBase.DEFAULT_MIN_SCALE : SelectedTrigger.MinScale;
        public double MaxScale => SelectedTrigger == null ? MpAvTriggerActionViewModelBase.DEFAULT_MAX_SCALE : SelectedTrigger.MaxScale;
        public double Scale {
            get => SelectedTrigger == null ? 1 : SelectedTrigger.Scale;
            set {
                if(SelectedTrigger == null) {
                    return;
                }
                SelectedTrigger.Scale = value;
                OnPropertyChanged(nameof(Scale));
            }
        }
        public double TranslateOffsetX {
            get => SelectedTrigger == null ? 0 : SelectedTrigger.TranslateOffsetX;
            set {
                if (SelectedTrigger == null) {
                    return;
                }
                SelectedTrigger.TranslateOffsetX = value;
                OnPropertyChanged(nameof(TranslateOffsetX));
            }
        }
        public double TranslateOffsetY {
            get => SelectedTrigger == null ? 0 : SelectedTrigger.TranslateOffsetY;
            set {
                if (SelectedTrigger == null) {
                    return;
                }
                SelectedTrigger.TranslateOffsetY = value;
                OnPropertyChanged(nameof(TranslateOffsetY));
            }
        }

        public bool IsGridVisible {
            get => SelectedTrigger == null ? false : SelectedTrigger.IsGridVisible;
            set {
                if (SelectedTrigger == null) {
                    return;
                }
                SelectedTrigger.IsGridVisible = value;
                OnPropertyChanged(nameof(IsGridVisible));
            }
        }

        public ICommand ResetDesignerViewCommand => SelectedTrigger == null ? null : SelectedTrigger.ResetDesignerViewCommand;

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
                if (SelectedTrigger != null) {
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

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Properties

        public static CancellationTokenSource CTS = new CancellationTokenSource();


        #region View Models       

        public ObservableCollection<MpAvActionViewModelBase> Items { get; private set; } = new ObservableCollection<MpAvActionViewModelBase>();
        public ObservableCollection<MpAvTriggerActionViewModelBase> FocusTriggers { get; private set; } = new ObservableCollection<MpAvTriggerActionViewModelBase>();
        //public SelectionModel<MpAvActionViewModelBase> ActionSelection { get; private set; }
        public IEnumerable<MpAvTriggerActionViewModelBase> Triggers => Items.Where(x => x is MpAvTriggerActionViewModelBase).Cast<MpAvTriggerActionViewModelBase>();

        private MpAvTriggerActionViewModelBase _selectedTrigger;
        public MpAvTriggerActionViewModelBase SelectedTrigger {
            get => _selectedTrigger;
            set {
                if(SelectedTrigger != value) {
                    FocusTriggers.Clear();
                    _selectedTrigger = value;
                    if(SelectedTrigger != null) {
                        FocusTriggers.Add(SelectedTrigger);
                    }
                    OnPropertyChanged(nameof(SelectedTrigger));
                    OnPropertyChanged(nameof(FocusTriggers));
                }
            }
        }
        public MpAvActionViewModelBase FocusAction { get; set; }

        public IEnumerable<MpAvActionViewModelBase> SelectedTriggerAndAllActions =>
            Items.Where(x => x.RootTriggerActionViewModel == SelectedTrigger);

        public IEnumerable<MpAvActionViewModelBase> AllActions => Items;


        #endregion

        #region Appearance
        #endregion

        #region Layout

        public MpRect ObservedDesignerBounds { get; set; } = MpRect.Empty;
        public double DesignerItemDiameter => 50;
        //public MpPoint DesignerCenterLocation => new MpPoint(
        //    (ObservedDesignerBounds.Width / 2) - (DesignerItemDiameter / 2),
        //    (ObservedDesignerBounds.Height / 2) - (DesignerItemDiameter / 2));

        public MpPoint DesignerCenterLocation => MpPoint.Zero;
        public MpPoint DefaultTriggerLocation => new MpPoint(-DesignerItemDiameter/2, -DesignerItemDiameter/2);

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
            PropertyChanged += MpAvTriggerCollectionViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
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
            }

            while (Items.Any(x => x.IsAnyBusy)) {
                // wait for all action trees to initialize before enabling
                await Task.Delay(100);
            }

            Items.ForEach(x => x.OnPropertyChanged(nameof(x.ParentActionViewModel)));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Triggers));

            await RestoreAllEnabled();

            if(AllActions.Count() > 0) {
                // select most recent action
                MpAvActionViewModelBase actionToSelect = AllActions
                                .Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);

                if (actionToSelect != null) {
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


        public void SetErrorToolTip(int actionId, int argNum, string text) {
            Dispatcher.UIThread.Post(async() => {
                if (FocusAction != null && FocusAction.ActionId != actionId) {
                    // ignore non-visible tooltip validation changes
                    return;
                }
                // wait for content control to bind to primary action...
                await Task.Delay(300);
                var apv = MpAvMainWindow.Instance.GetVisualDescendant<MpAvActionPropertyView>();
                if (apv != null) {
                    var rapcc = apv.FindControl<ContentControl>("RootActionPropertyContentControl");
                    if (rapcc != null) {

                        var allArgControls =
                            rapcc.GetVisualDescendants<Control>()
                            .Where(x => x.Classes.Any(x => x.StartsWith("arg")));

                        var argToolTip = new MpAvToolTipView() {
                            ToolTipText = text,
                            Classes = Classes.Parse("error")
                        };
                        foreach (var arg_control in allArgControls) {
                            if (arg_control.Classes.Any(x => x == $"arg{argNum}")) {
                                ToolTip.SetTip(arg_control, argToolTip);
                                if (!arg_control.Classes.Contains("invalid")) {
                                    arg_control.Classes.Add("invalid");
                                }
                            } else {
                                ToolTip.SetTip(arg_control, null);
                                arg_control.Classes.Remove("invalid");
                            }
                        }
                    }
                }
            });
        }
        #endregion

        #region Protected Methods



        #endregion

        #region Private Methods


        private async Task UpdateSortOrderAsync() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        private void MpAvTriggerCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(FocusAction):
                    if (FocusAction == null) {
                        FocusAction = SelectedTrigger;
                    } else if (SelectedTrigger == null ||
                                SelectedTrigger.ActionId != FocusAction.RootTriggerActionViewModel.ActionId) {
                        SelectedTrigger = FocusAction.RootTriggerActionViewModel;
                    }
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    break;
                case nameof(SelectedTrigger):
                    if(SelectedTrigger == null) {
                        FocusAction = null;
                    } else if (FocusAction == null || FocusAction.RootTriggerActionViewModel.ActionId != SelectedTrigger.ActionId) {
                        FocusAction = SelectedTrigger;
                    }
                    if (SelectedTrigger != null) {
                        SelectedTrigger.OnPropertyChanged(nameof(SelectedTrigger.SelfAndAllDescendants));
                    }
                    OnPropertyChanged(nameof(SelectedTriggerAndAllActions));
                    OnPropertyChanged(nameof(MinScale));
                    OnPropertyChanged(nameof(MaxScale));
                    OnPropertyChanged(nameof(Scale));
                    OnPropertyChanged(nameof(TranslateOffsetX));
                    OnPropertyChanged(nameof(TranslateOffsetY));
                    OnPropertyChanged(nameof(IsGridVisible));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsActionDesignerVisible)));
                    OnPropertyChanged(nameof(Items));
                    break;
            }
        }


        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Triggers));
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
                         sortOrderIdx: Items.Count,
                         location: DefaultTriggerLocation);

                 var new_trigger_vm = await CreateTriggerViewModel(na);

                 while(new_trigger_vm.IsBusy) {
                     await Task.Delay(100);
                 }
                 //Items.Add(new_trigger_vm);
                 
                 await Task.Delay(300);

                 //SelectActionCommand.Execute(new_trigger_vm);
                 FocusAction = new_trigger_vm;
                 OnPropertyChanged(nameof(SelectedTrigger));

                 OnPropertyChanged(nameof(Items));
                 OnPropertyChanged(nameof(Triggers));
                 new_trigger_vm.ResetDesignerViewCommand.Execute(null);

                 IsBusy = false;
             });

        public ICommand DeleteActionCommand => new MpCommand<object>(
            async (args) => {
                IsBusy = true;

                var child_to_delete_avm = args as MpAvActionViewModelBase;
                if (child_to_delete_avm == null) {
                    // link error (this cmd is called from arg vm using parentacvm)
                    Debugger.Break();
                    IsBusy = false;
                    return;
                }

                List<MpAvActionViewModelBase> to_remove_list = new List<MpAvActionViewModelBase>() {
                    child_to_delete_avm
                };

                bool remove_descendants = false;
                if(child_to_delete_avm.ParentActionId == 0) {
                    // trigger deletes children by default
                    remove_descendants = true;
                } else if (child_to_delete_avm.Children.Count() > 0) {
                    MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
                    var remove_descendants_result = await MpPlatformWrapper.Services.NativeMessageBox.ShowYesNoCancelMessageBoxAsync(
                        title: $"Remove Options",
                        message: $"Would you like to remove all the sub-actions for '{child_to_delete_avm.Label}'? (Otherwise they will be re-parented to '{child_to_delete_avm.ParentActionViewModel.Label}')",
                        iconResourceObj: "ChainImage",
                        anchor: ObservedDesignerBounds);
                    MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
                    if (remove_descendants_result.IsNull()) {
                        // cancel
                        IsBusy = false;
                        return;
                    }
                    remove_descendants = remove_descendants_result.IsTrue();
                }

                FocusAction = child_to_delete_avm.ParentActionViewModel;
                
                if (remove_descendants) {
                    to_remove_list.AddRange(child_to_delete_avm.AllDescendants);
                } else {
                    child_to_delete_avm.Children.ForEach(x => x.ParentActionId = child_to_delete_avm.ParentActionId);
                }

                foreach (var to_remove_avm in to_remove_list.OrderByDescending(x => x.TreeLevel).ThenBy(x => x.SortOrderIdx)) {
                    to_remove_avm.ToggleIsEnabledCommand.Execute(false);
                    while(to_remove_avm.IsBusy) {
                        await Task.Delay(100);
                    }
                    await to_remove_avm.Action.DeleteFromDatabaseAsync();
                    //to_remove_avm.RootTriggerActionViewModel.Children.Remove(to_remove_avm);
                    Items.Remove(to_remove_avm);
                }
                if(child_to_delete_avm.ParentActionId == 0) {
                    await UpdateSortOrderAsync();
                    SelectedTrigger = null;
                    OnPropertyChanged(nameof(Triggers));
                } else {
                    await child_to_delete_avm.ParentActionViewModel.UpdateSortOrderAsync();
                }
                IsBusy = false;
            });

        public ICommand SelectActionCommand => new MpCommand<object>(
            (args) => {
                MpAvActionViewModelBase toSelect_avmb = null;
                int focusArgNum = 0;
                string error_text = null;
                if (args is MpAvActionViewModelBase) {
                    toSelect_avmb = args as MpAvActionViewModelBase;
                } else if (args is int actionId) {
                    toSelect_avmb = AllActions.FirstOrDefault(x => x.ActionId == actionId);
                } else if (args is object[] argParts) {
                    if (argParts[0] is int actionIdPart) {
                        toSelect_avmb = AllActions.FirstOrDefault(x => x.ActionId == actionIdPart);
                    }
                    if (argParts[1] is int) {
                        focusArgNum = (int)argParts[1];
                    }
                    if (argParts[2] is string) {
                        error_text = argParts[2] as string;
                    }
                }
                FocusAction = toSelect_avmb;

                OnPropertyChanged(nameof(FocusAction));

                if(focusArgNum > 0) {

                    MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(this);
                }

                if (toSelect_avmb != null) {
                    SetErrorToolTip(toSelect_avmb.ActionId, focusArgNum, error_text);
                }
            }, (args) => {
                MpAvActionViewModelBase toSelect_avmb = null;
                int focusArgNum = 0;
                if(args is MpAvActionViewModelBase) {
                    toSelect_avmb = args as MpAvActionViewModelBase;
                } else if(args is int actionId) {
                    toSelect_avmb = AllActions.FirstOrDefault(x => x.ActionId == actionId);
                } else if(args is object[] argParts) {
                    if (argParts[0] is int actionIdPart) {
                        toSelect_avmb = AllActions.FirstOrDefault(x => x.ActionId == actionIdPart);
                    }
                    if (argParts[1] is int) {
                        focusArgNum = (int)argParts[1];
                    }
                }
                if(toSelect_avmb != null && focusArgNum > 0) {
                    // always allow execute when focus is passed
                    return true;
                }
                if(toSelect_avmb == null && FocusAction != null) {
                    return true;
                }
                if(toSelect_avmb != null && FocusAction == null) {
                    return true;
                }
                if(toSelect_avmb == null && FocusAction == null) {
                    return true;
                }
                return FocusAction.ActionId != toSelect_avmb.ActionId;
            });




        #endregion
    }
}
