using Azure;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public interface MpITriggerActionViewModel {
        void RegisterTrigger(MpActionViewModelBase mvm);
        void UnregisterTrigger(MpActionViewModelBase mvm);
    }

    public class MpActionOutput {
        public MpCopyItem CopyItem { get; set; }
        public MpActionOutput Previous { get; set; }
    }

    public abstract class MpActionViewModelBase :
        MpViewModelBase<MpActionCollectionViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpIUserIconViewModel,
        MpITreeItemViewModel<MpActionViewModelBase>,
        MpIBoxViewModel,
        MpIMovableViewModel,
        MpITriggerActionViewModel {
        
        #region Properties

        #region View Models               

        public MpActionViewModelBase ParentActionViewModel { get; set; }

        #endregion

        #region MpIMovableViewModel Implementation

        public bool IsMoving { get; set; }

        public bool CanMove { get; set; }

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; } = false;

        public MpActionViewModelBase ParentTreeItem => ParentActionViewModel;

        public ObservableCollection<MpActionViewModelBase> Items { get; set; } = new ObservableCollection<MpActionViewModelBase>();

        public IList<MpActionViewModelBase> Children => Items;
        #endregion

        #region MpIUserIcon Implementation

        public async Task<MpIcon> GetIcon() {
            var icon = await MpDb.GetItemAsync<MpIcon>(IconId);
            return icon;
        }

        public ICommand SetIconCommand => new RelayCommand<object>(
            async (args) => {
                var icon = args as MpIcon;
                Action.IconId = icon.Id;
                await Action.WriteToDatabaseAsync();
                OnPropertyChanged(nameof(IconId));
            });

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public DateTime LastSelectedDateTime { get; set; }

        public bool IsSelected { get; set; } = false;

        #endregion

        #region MpIMenuItemViewModel Implementation

        public virtual MpMenuItemViewModel MenuItemViewModel {
            get {
                var amivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpActionType).EnumToLabels();
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    MpActionType at = (MpActionType)i;
                    switch (at) {
                        case MpActionType.Analyze:
                            resourceKey = "BrainIcon";
                            break;
                        case MpActionType.Classify:
                            resourceKey = "PinToCollectionIcon";
                            break;
                        case MpActionType.Compare:
                            resourceKey = "ScalesIcon";
                            break;
                        case MpActionType.Macro:
                            resourceKey = "HotkeyIcon";
                            break;
                        case MpActionType.Timer:
                            resourceKey = "AlarmClockIcon";
                            break;
                    }
                    bool isVisible = true;
                    if(at == MpActionType.None || at == MpActionType.Trigger) {
                        isVisible = false;
                    }
                    if(GetType().IsSubclassOf(typeof(MpTriggerActionViewModelBase)) && 
                       GetType() != typeof(MpCompareActionViewModelBase) && 
                       at == MpActionType.Macro) {
                        isVisible = false;
                    }
                    amivml.Add(new MpMenuItemViewModel() {
                        IconResourceKey = Application.Current.Resources[resourceKey] as string,
                        Header = triggerLabels[i],
                        Command = IsPlaceholder ? ParentActionViewModel.AddChildActionCommand : AddChildActionCommand,
                        CommandParameter = (MpActionType)i,
                        IsVisible = isVisible
                    });
                }
                return new MpMenuItemViewModel() {
                    SubItems = amivml
                };
            }
        }

        #endregion

        #region Appearance

        public string BorderBrushHexColor {
            get {
                if(Parent != null && Parent.PrimaryAction != null && Parent.PrimaryAction.ActionId == ActionId) {
                    return MpSystemColors.IsSelectedBorderColor;
                }
                if(IsHovering) {
                    return MpSystemColors.IsHoveringBorderColor;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string IconResourceKeyStr {
            get {
                string resourceKey = string.Empty;
                switch (ActionType) {
                    case MpActionType.Trigger:
                        switch ((MpTriggerType)ActionObjId) {
                            case MpTriggerType.ContentAdded:
                                resourceKey = "ClipboardIcon";
                                break;
                            case MpTriggerType.ContentTagged:
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
                        break;
                    case MpActionType.Analyze:
                        resourceKey = "BrainIcon";
                        break;
                    case MpActionType.Classify:
                        resourceKey = "PinToCollectionIcon";
                        break;
                    case MpActionType.Compare:
                        resourceKey = "ScalesIcon";
                        break;
                    case MpActionType.Macro:
                        resourceKey = "HotkeyIcon";
                        break;
                    case MpActionType.Timer:
                        resourceKey = "AlarmClockIcon";
                        break;
                    case MpActionType.None:
                        resourceKey = "QuestionMarkIcon";
                        break;
                }
                return Application.Current.Resources[resourceKey] as string;
            }
        }

        #endregion

        #region State

        public bool IsPlaceholder => ActionType == MpActionType.None;

        //public bool IsDropDownOpen { get; set; } = false;

        public bool IsRootAction => ParentActionId == 0;

        public bool IsEnabled { get; set; } = false;

        public bool IsEditingDetails { get; set; }

        public bool IsEmptyAction => this is MpEmptyActionViewModel;

        public bool IsCompareAction => ActionType == MpActionType.Compare;

        public bool IsAnalyzeAction => ActionType == MpActionType.Analyze;

        public bool IsActionTextBoxFocused { get; set; } = false;

        public bool IsAnyActionTextBoxFocused => IsActionTextBoxFocused || this.FindAllChildren().Any(x => x.IsActionTextBoxFocused);

        public bool IsValid => string.IsNullOrEmpty(ValidationText);

        public string ValidationText { get; set; }

        public bool IsAnyChildSelected => this.FindAllChildren().Any(x => x.IsSelected);

        #endregion

        #region Model

        #region MpIBoxViewModel Implementation

        public Point Location {
            get {
                return new Point(X, Y);
            }
            set {
                if(Location != value) {
                    X = value.X;
                    Y = value.Y;
                    OnPropertyChanged(nameof(Location));
                }
            }
        }

        public double X {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.X;
            }
            set {
                if (X != value) {
                    Action.X = value;
                    HasModelChanged = !IsMoving;
                    OnPropertyChanged(nameof(X));
                    OnPropertyChanged(nameof(Location));
                }
            }
        }

        public double Y {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.Y;
            }
            set {
                if (Y != value) {
                    Action.Y = value;
                    HasModelChanged = !IsMoving;
                    OnPropertyChanged(nameof(Y));
                    OnPropertyChanged(nameof(Location));
                }
            }
        }

        public double Width => MpMeasurements.Instance.DesignerItemSize;

        public double Height => MpMeasurements.Instance.DesignerItemSize;


        #endregion

        public bool IsReadOnly {
            get {
                if (Action == null) {
                    return false;
                }
                return Action.IsReadOnly;
            }
            set {
                if (IsReadOnly != value) {
                    Action.IsReadOnly = value;
                    OnPropertyChanged(nameof(IsReadOnly));
                }
            }
        }

        public int ActionObjId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.ActionObjId;
            }
            set {
                if (ActionObjId != value) {
                    Action.ActionObjId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ActionObjId));
                }
            }
        }

        public MpActionType ActionType {
            get {
                if (Action == null) {
                    return MpActionType.None;
                }
                return Action.ActionType;
            }
            set {
                if (ActionType != value) {
                    Action.ActionType = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ActionType));
                }
            }
        }

        public virtual string Label {
            get {
                if (Action == null) {
                    return null;
                }
                if(string.IsNullOrEmpty(Action.Label)) {
                    return ActionType.EnumToLabel();
                }
                return Action.Label;
            }
            set {
                if (Label != value) {
                    Action.Label = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public int SortOrderIdx {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.SortOrderIdx;
            }
            set {
                if (SortOrderIdx != value) {
                    Action.SortOrderIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        public int IconId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.IconId;
            }
        }


        public int ParentActionId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.ParentActionId;
            }
            set {
                if(ParentActionId != value) {
                    Action.ParentActionId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ParentActionId));
                }
            }
        }

        public int ActionId {
            get {
                if(Action == null) {
                    return 0;
                }
                return Action.Id;
            }
        }

        public MpAction Action { get; set; }

        #endregion

        #endregion

        #region Events

        public event EventHandler<object> OnActionComplete;

        #endregion

        #region Constructors

        public MpActionViewModelBase() : base(null) { }

        public MpActionViewModelBase(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpActionViewModelBase_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpAction a) {
            IsBusy = true;

            Action = a;

            if(!IsPlaceholder) {
                var cal = await MpDataModelProvider.GetChildActions(ActionId);

                foreach (var ca in cal.OrderBy(x => x.SortOrderIdx)) {
                    var cavm = await CreateActionViewModel(ca);
                    Items.Add(cavm);
                }
            }

            IsBusy = false;
        }

        public async Task<MpActionViewModelBase> CreateActionViewModel(MpAction a) {
            a = a == null ? new MpAction() : a;
            MpActionViewModelBase avm = null;
            switch (a.ActionType) {
                case MpActionType.Trigger:
                    if((MpTriggerType)a.ActionObjId != MpTriggerType.ParentOutput) {
                        throw new Exception("Only parent output triggers can be children of a trigger or action");
                    }
                    avm = new MpParentOutputTriggerViewModel(Parent);
                    break;
                case MpActionType.Analyze:
                    avm = new MpAnalyzeActionViewModel(Parent);
                    break;
                case MpActionType.Classify:
                    avm = new MpClassifyActionViewModel(Parent);
                    break;
                case MpActionType.Compare:
                    avm = new MpCompareActionViewModelBase(Parent);
                    break;
                case MpActionType.Macro:
                    avm = new MpMacroActionViewModel(Parent);
                    break;
                case MpActionType.Timer:
                    avm = new MpTimerActionViewModel(Parent);
                    break;
            }

            avm.ParentActionViewModel = this;
            await avm.InitializeAsync(a);

            var eavm = await avm.CreateEmptyActionViewModel();            
            avm.Items.Add(eavm);

            return avm;
        }

        public async Task<MpEmptyActionViewModel> CreateEmptyActionViewModel() {
            MpEmptyActionViewModel avm = new MpEmptyActionViewModel(Parent);
            avm.ParentActionViewModel = this;
            await avm.InitializeAsync(new MpAction());

            avm.X = X;
            avm.Y = Y - (Height * 1.75);

            return avm;
        }

        public virtual void Enable() {
            Children.OrderBy(x => x.SortOrderIdx).ForEach(x => x.Enable());
            if (IsEnabled) {
                return;
            }
            if (ParentActionViewModel != null) {
                if (ParentActionViewModel.OnActionComplete != null) {
                    ParentActionViewModel.OnActionComplete -= OnActionTriggered;
                }
                ParentActionViewModel.OnActionComplete += OnActionTriggered;
            }

            IsEnabled = true;
        }

        public virtual void Disable() {
            if(!IsEnabled) {
                return;
            }
            if (ParentActionViewModel != null) {
                ParentActionViewModel.OnActionComplete -= OnActionTriggered;
            }
            Children.OrderBy(x => x.SortOrderIdx).ForEach(x => x.Disable());
            IsEnabled = false;
        }

        public void OnActionTriggered(object sender, object args) {
            if(!IsEnabled) {
                OnActionComplete?.Invoke(this, args);
                return;
            }
            //OnAction?.Invoke(this, ci);
            Task.Run(()=>PerformAction(args));
        }
        
        public virtual void Validate() {
            this.FindAllChildren().ForEach(x => x.Validate());
            if(ActionType == MpActionType.None) {
                ValidationText = "Select Action Type";
            } else if(ParentActionViewModel != null && 
                ParentActionViewModel.ActionType != MpActionType.Compare && this is MpCompareActionViewModelBase) {
                ValidationText = "Macro's must be root level action's or a compare output";
            }
        }


        public virtual async Task PerformAction(object arg) {
            if (arg == null) {
                return;
            }
            await Task.Delay(1);
            OnActionComplete?.Invoke(this, arg);
        }

        #endregion

        #region MpIMatchTrigger Implementation

        public void RegisterTrigger(MpActionViewModelBase mvm) {
            OnActionComplete += mvm.OnActionComplete;
            MpConsole.WriteLine($"Parent Matcher {Label} Registered {mvm.Label} matcher");
        }

        public void UnregisterTrigger(MpActionViewModelBase mvm) {
            OnActionComplete -= mvm.OnActionComplete;
            MpConsole.WriteLine($"Parent Matcher {Label} Unregistered {mvm.Label} from OnCopyItemAdded");
        }

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        #endregion

        #endregion

        #region Protected Methods


        #endregion

        #region Private Methods

        private async Task UpdateSortOrder() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Where(x=>x.GetType() != typeof(MpEmptyActionViewModel)).Select(x => x.Action.WriteToDatabaseAsync()));
        }

        private void MpActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    //if(Parent == null) {
                    //    return;
                    //}
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        if(Children.Any(x=>x is MpEmptyActionViewModel)) {
                            Children.Where(x => x is MpEmptyActionViewModel).ForEach(x => (x as MpEmptyActionViewModel).OnPropertyChanged("IsVisible"));
                        }
                    }
                    if(this is MpTriggerActionViewModelBase && Parent.SelectedItem != this) {
                        Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    }
                    Parent.OnPropertyChanged(nameof(Parent.PrimaryAction));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedActions));
                    OnPropertyChanged(nameof(BorderBrushHexColor));
                    break;
                case nameof(IsEnabled):
                    if (IsEnabled) {
                        Enable();
                    } else {
                        Disable();
                    }
                    break;
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        HasModelChanged = false;

                        if(this is MpEmptyActionViewModel) {
                            return;
                        }

                        Task.Run(async () => {
                            await Action.WriteToDatabaseAsync();
                        });
                    }
                    break;
                case nameof(IsMoving):
                    Parent.OnPropertyChanged(nameof(Parent.CanPan));
                    break;
                case nameof(Items):                
                    Parent.NotifyViewportChanged();
                    break;
                case nameof(IsExpanded):
                    if(IsExpanded) {
                        IsSelected = true;
                    }
                    break;
                case nameof(X):
                case nameof(Y):
                    var eavm = Items.FirstOrDefault(x => x is MpEmptyActionViewModel);
                    if(eavm != null) {
                        eavm.X = X;
                        eavm.Y = Y - (Height * 1.75);
                    }
                    //if(IsMoving) {
                    //    Parent.NotifyViewportChanged();
                    //}
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ToggleIsEnabledCommand => new RelayCommand(
             () => {
                 IsEnabled = !IsEnabled;
            });

        public ICommand ShowActionSelectorMenuCommand => new RelayCommand<object>(
             (args) => {
                 var fe = args as FrameworkElement;
                 var cm = new MpContextMenuView();
                 //LastSelectedDateTime = DateTime.Now;
                 IsSelected = true;

                 cm.DataContext = MenuItemViewModel;
                 fe.ContextMenu = cm;
                 fe.ContextMenu.PlacementTarget = fe;
                 fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                 fe.ContextMenu.IsOpen = true;
                 fe.ContextMenu.Closed += ContextMenu_Closed;
             },(args)=>ActionType == MpActionType.None);

        private void ContextMenu_Closed(object sender, RoutedEventArgs e) {
            return;
        }

        public ICommand AddChildActionCommand => new RelayCommand<object>(
             async (args) => {
                 IsBusy = true;

                 //IsDropDownOpen = false;

                 MpActionType at = (MpActionType)args;
                 MpAction na = await MpAction.Create(
                                         actionType: at,
                                         parentId: ActionId,
                                         sortOrderIdx: Items.Count);

                 var navm = await CreateActionViewModel(na);
                 var neavm = navm.Items.FirstOrDefault(x => x is MpEmptyActionViewModel);
                 
                 var eavm = this.Items.FirstOrDefault(x => x is MpEmptyActionViewModel);
                 navm.Location = eavm.Location;
                 neavm.X = navm.X;
                 neavm.Y = navm.Y - (navm.Height * 2);

                 eavm.X = X;
                 eavm.Y = Y + (Height * 2);

                 eavm.IsSelected = false;
                 Items.Add(navm);
                 navm.IsSelected = true;

                 Parent.ClearAllOverlaps();

                 OnPropertyChanged(nameof(Items));

                 Parent.OnPropertyChanged(nameof(Parent.AllSelectedTriggerActions));

                 IsExpanded = true;

                 Parent.NotifyViewportChanged();

                 IsBusy = false;
             }, (args) => ActionType != MpActionType.None);

        public ICommand DeleteChildActionCommand => new RelayCommand<object>(
            async (args) => {
                IsBusy = true;

                var avm = args as MpActionViewModelBase;
                avm.Disable();

                var grandChildren = Parent.AllSelectedTriggerActions.Where(x => x.ParentActionId == avm.ActionId);
                grandChildren.ForEach(x => x.ParentActionId = ActionId);
                grandChildren.ForEach(x => x.ParentActionViewModel = this);
                await Task.WhenAll(grandChildren.Select(x => x.Action.WriteToDatabaseAsync()));
                await avm.Action.DeleteFromDatabaseAsync();

                Items.Remove(avm);

                await UpdateSortOrder();
                OnPropertyChanged(nameof(Items));
                //OnPropertyChanged(nameof(Children));
                //Parent.AllSelectedTriggerActions.Remove(avm);
                Parent.OnPropertyChanged(nameof(Parent.AllSelectedTriggerActions));

                Parent.NotifyViewportChanged();

                IsBusy = false;
            });

        public ICommand DeleteThisActionCommand => new RelayCommand(
            () => {
                if(IsRootAction) {
                    Parent.DeleteTriggerCommand.Execute(this);
                    return;
                }

                if (ParentActionViewModel == null) {
                    throw new Exception("This shouldn't be null, cannot delete");
                }
                ParentActionViewModel.DeleteChildActionCommand.Execute(this);
            });


        #endregion
    }
}
