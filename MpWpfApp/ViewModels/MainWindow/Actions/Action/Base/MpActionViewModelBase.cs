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
        public object OutputData { get; set; }
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
        #region Private Variables

        private Point _lastLocation;
        

        #endregion

        #region Properties

        #region View Models               

        public MpActionViewModelBase ParentActionViewModel { get; set; }

        public MpEmptyActionViewModel AddChildEmptyActionViewModel => Items.FirstOrDefault(x => x is MpEmptyActionViewModel) as MpEmptyActionViewModel;
        
        public MpTriggerActionViewModelBase RootTriggerActionViewModel {
            get {
                var rtavm = this;
                while(rtavm.ParentActionViewModel != null) {
                    rtavm = rtavm.ParentActionViewModel;
                }
                return rtavm as MpTriggerActionViewModelBase;
            }
        }
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

        public string LabelBorderBrushHexColor {
            get {
                if(IsHoveringOverLabel || IsLabelFocused) {
                    return MpSystemColors.White;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string LabelBackgroundBrushHexColor {
            get {
                if (IsLabelFocused) {
                    return MpSystemColors.White;
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
                    case MpActionType.Transform:
                        resourceKey = "WandIcon";
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

        public bool IsLabelVisible {
            get {
                if(Parent == null) {
                    return false;
                }
                return Parent.PrimaryAction.ActionId == ActionId && !IsEmptyAction;
            }
        }

        public bool IsPlaceholder => ActionType == MpActionType.None;

        public bool HasDescription => !string.IsNullOrEmpty(Description);

        public bool IsRootAction => ParentActionId == 0 && this is MpTriggerActionViewModelBase;
        
        public bool LastIsEnabledState { get; set; } = false;
       
        public bool? IsEnabled { get; set; } = false;


        public bool IsEditingDetails { get; set; }

        public bool IsEmptyAction => this is MpEmptyActionViewModel;

        public bool IsCompareAction => ActionType == MpActionType.Compare;

        public bool IsAnalyzeAction => ActionType == MpActionType.Analyze;

        public bool IsActionTextBoxFocused { get; set; } = false;

        public bool IsAnyActionTextBoxFocused => IsActionTextBoxFocused || this.FindAllChildren().Any(x => x.IsActionTextBoxFocused);

        public bool IsValid => string.IsNullOrEmpty(ValidationText);

        public string ValidationText { get; set; }

        public bool IsHoveringOverLabel { get; set; } = false;

        public bool IsLabelFocused { get; set; } = false;

        public bool IsAnyChildSelected => this.FindAllChildren().Any(x => x.IsSelected);

        public bool IsPropertyListItemVisible {
            get {
                if(Parent == null || this is MpEmptyActionViewModel) {
                    return false;
                }
                if(IsRootAction) {
                    return true;
                }
                if(IsSelected || ParentActionViewModel.IsExpanded || IsExpanded || ParentActionViewModel.IsSelected) {
                    return true;
                }
                return false;
            }
        }
        public Point DefaultEmptyActionLocation => new Point(X, Y - (Height * 2));

        #endregion

        #region Model

        #region MpIBoxViewModel Implementation

        public Point Center => new Point(Location.X + (Width / 2), Location.Y + (Height / 2));

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

        public double Width => MpMeasurements.Instance.DesignerItemDiameter;

        public double Height => MpMeasurements.Instance.DesignerItemDiameter;


        #endregion

        public string Arg1 {
            get {
                if(Action == null) {
                    return string.Empty;
                }
                return Action.Arg1;
            }
            set {
                if(Arg1 != value) {
                    Action.Arg1 = value; // NOTE no HasModelChanged for abstract fields
                    OnPropertyChanged(nameof(Arg1));
                }
            }
        }

        public string Arg2 {
            get {
                if (Action == null) {
                    return string.Empty;
                }
                return Action.Arg2;
            }
            set {
                if (Arg2 != value) {
                    Action.Arg2 = value; // NOTE no HasModelChanged for abstract fields
                    OnPropertyChanged(nameof(Arg2));
                }
            }
        }

        public string Arg3 {
            get {
                if (Action == null) {
                    return string.Empty;
                }
                return Action.Arg3;
            }
            set {
                if (Arg3 != value) {
                    Action.Arg3 = value; // NOTE no HasModelChanged for abstract fields
                    OnPropertyChanged(nameof(Arg3));
                }
            }
        }

        public string Arg4 {
            get {
                if (Action == null) {
                    return string.Empty;
                }
                return Action.Arg4;
            }
            set {
                if (Arg4 != value) {
                    Action.Arg4 = value; // NOTE no HasModelChanged for abstract fields
                    OnPropertyChanged(nameof(Arg4));
                }
            }
        }

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
        public virtual string Description {
            get {
                if (Action == null) {
                    return null;
                }
                return Action.Description;
            }
            set {
                if (Description != value) {
                    Action.Description = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Description));
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

            Items.Clear();

            if(!IsEmptyAction) {
                var eavm = await CreateEmptyActionViewModel();
                Items.Add(eavm);
            }

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
                case MpActionType.Transform:
                    avm = new MpTransformActionViewModel(Parent);
                    break;
            }

            avm.ParentActionViewModel = this;
            await avm.InitializeAsync(a);

            return avm;
        }

        public async Task<MpEmptyActionViewModel> CreateEmptyActionViewModel() {
            MpEmptyActionViewModel eavm = new MpEmptyActionViewModel(Parent);
            eavm.ParentActionViewModel = this;

            var emptyAction = await MpAction.Create(
                location: DefaultEmptyActionLocation.ToMpPoint(),
                suppressWrite: true);

            await eavm.InitializeAsync(emptyAction);

            return eavm;
        }

        public void OnActionTriggered(object sender, object args) {
            if(!IsEnabled.HasValue) {
                //if action has errors halt 
                return;
            }
            if(!IsEnabled.Value) {
                //if action is disabled pass parent output to child
                OnActionComplete?.Invoke(this, args);
                return;
            }
            //OnAction?.Invoke(this, ci);
            Task.Run(()=>PerformAction(args));
        }


        public virtual async Task PerformAction(object arg) {
            if (!CanPerformAction(arg)) {
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

        protected async Task ShowValidationNotification() {
            var userAction = await MpNotificationBalloonViewModel.Instance.ShowUserActions(
                    notificationType: MpNotificationType.InvalidAction,
                    exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
                    msg: ValidationText);


            if (userAction == MpNotificationUserActionType.Retry) {
                await Validate();
            }
        }


        protected virtual async Task<bool> Validate() {
            if(!IsRootAction && ParentActionViewModel == null) {
                // this shouldn't happen...
                ValidationText = $"Action '{RootTriggerActionViewModel.Label}/{Label}' must be linked to a trigger";
                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }

            if(ActionType == MpActionType.None && !IsEmptyAction) {
                ValidationText = $"Action '{RootTriggerActionViewModel.Label}/{Label}' must have a type to be enabled";
                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }

            return IsValid;
        }

        protected virtual async Task Enable() { await Task.Delay(1); }

        protected virtual async Task Disable() { await Task.Delay(1); }

        protected virtual bool CanPerformAction(object arg) {
            if (!IsValid ||
               !IsEnabled.HasValue ||
               (IsEnabled.HasValue && !IsEnabled.Value)) {
                return false;
            }
            return true;
        }

        #endregion

        #region Private Methods

        private async Task UpdateSortOrder() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Where(x=>x.GetType() != typeof(MpEmptyActionViewModel)).Select(x => x.Action.WriteToDatabaseAsync()));
        }

        private void MpActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        if(Children.Any(x=>x is MpEmptyActionViewModel)) {
                            Children.Where(x => x is MpEmptyActionViewModel).ForEach(x => (x as MpEmptyActionViewModel).OnPropertyChanged("IsVisible"));
                        }

                        IsExpanded = true;
                    }
                    if(this is MpTriggerActionViewModelBase && Parent.SelectedItem != this) {
                        Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    }
                    Parent.OnPropertyChanged(nameof(Parent.PrimaryAction));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedActions));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    OnPropertyChanged(nameof(IsPropertyListItemVisible));
                    this.FindAllChildren().ForEach(x => x.OnPropertyChanged(nameof(IsPropertyListItemVisible)));
                    OnPropertyChanged(nameof(BorderBrushHexColor));
                    break;
                case nameof(IsEnabled):
                    if(!IsEnabled.HasValue) {
                        break;
                    }
                    Task.Run(async () => {
                        if (IsEnabled.HasValue && IsEnabled.Value) {
                            await Enable();
                        } else {
                            await Disable();
                        }
                    });
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
                case nameof(Items):                
                    Parent.NotifyViewportChanged();
                    break;
                case nameof(IsExpanded):
                    if(IsExpanded && !IsSelected) {
                        IsSelected = true;
                    }
                    OnPropertyChanged(nameof(IsPropertyListItemVisible));
                    this.FindAllChildren().ForEach(x => x.OnPropertyChanged(nameof(IsPropertyListItemVisible)));
                    break;
                case nameof(Location):
                case nameof(X):
                case nameof(Y):
                    
                    if(AddChildEmptyActionViewModel == null) {
                        break;
                    }

                    AddChildEmptyActionViewModel.Location = DefaultEmptyActionLocation;

                    _lastLocation = Location;
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ToggleIsEnabledCommand => new MpRelayCommand<bool>(
            async (parentToggledState) => {
                bool newIsEnabledState = false;
                if(parentToggledState != null) {
                    newIsEnabledState = (bool)parentToggledState;                    
                } else {
                    newIsEnabledState = !LastIsEnabledState;
                }
                
                if (newIsEnabledState) {
                    await Validate();
                    if (!IsValid) {
                        IsEnabled = null;
                    } else {
                        await Enable();
                        if(ParentActionViewModel != null) {
                            ParentActionViewModel.OnActionComplete += OnActionTriggered;
                        }
                        IsEnabled = true;
                        LastIsEnabledState = true;
                    }
                } else {
                    await Disable();
                    if(ParentActionViewModel != null) {
                        ParentActionViewModel.OnActionComplete -= OnActionTriggered;
                    }
                    IsEnabled = false;
                    LastIsEnabledState = false;
                }
                if(IsEnabled.HasValue) {
                    Children.ForEach(x => x.ToggleIsEnabledCommand.Execute(IsEnabled.Value));
                }
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

                 MpActionType at = (MpActionType)args;
                 MpAction na = await MpAction.Create(
                                         actionType: at,
                                         parentId: ActionId,
                                         sortOrderIdx: Items.Count,
                                         location: Parent.FindOpenDesignerLocation(Location).ToMpPoint());

                 var navm = await CreateActionViewModel(na);
                 
                 Items.Add(navm);

                 AddChildEmptyActionViewModel.IsSelected = false;
                 navm.IsSelected = true;

                 //Parent.ClearAllOverlaps();

                 OnPropertyChanged(nameof(Items));

                 Parent.OnPropertyChanged(nameof(Parent.AllSelectedTriggerActions));

                 IsExpanded = true;

                 Parent.NotifyViewportChanged();

                 navm.OnPropertyChanged(nameof(navm.X));
                 navm.OnPropertyChanged(nameof(navm.Y));
                 navm.AddChildEmptyActionViewModel.OnPropertyChanged(nameof(navm.AddChildEmptyActionViewModel.X));
                 navm.AddChildEmptyActionViewModel.OnPropertyChanged(nameof(navm.AddChildEmptyActionViewModel.Y));

                 IsBusy = false;
             }, (args) => ActionType != MpActionType.None);

        public ICommand DeleteChildActionCommand => new RelayCommand<object>(
            async (args) => {
                IsBusy = true;

                var avm = args as MpActionViewModelBase;
                await avm.Disable();

                var grandChildren = Parent.AllSelectedTriggerActions.Where(x => x.ParentActionId == avm.ActionId && !x.IsEmptyAction);
                grandChildren.ForEach(x => x.ParentActionId = ActionId);
                grandChildren.ForEach(x => x.ParentActionViewModel = this);
                await Task.WhenAll(grandChildren.Select(x => x.Action.WriteToDatabaseAsync()));
                await avm.Action.DeleteFromDatabaseAsync();

                Parent.OnPropertyChanged(nameof(Parent.AllSelectedTriggerActions));
                Parent.AllSelectedTriggerActions.Remove(avm);
                await RootTriggerActionViewModel.InitializeAsync(RootTriggerActionViewModel.Action);

                Parent.SelectedItem = RootTriggerActionViewModel;
                IsSelected = true;
                //Parent.NotifyViewportChanged();

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
