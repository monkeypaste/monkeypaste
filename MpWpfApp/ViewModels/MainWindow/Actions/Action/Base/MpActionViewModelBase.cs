using Azure;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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

    public abstract class MpActionViewModelBase :
        MpSelectorViewModelBase<MpActionCollectionViewModel, MpActionViewModelBase>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpIUserIconViewModel,
        MpITreeItemViewModel<MpActionViewModelBase>,
        MpIMenuItemViewModel,
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

        public bool IsSelected { get; set; }

        #endregion

        #region MpIMenuItemViewModel Implementation

        public virtual MpMenuItemViewModel MenuItemViewModel {
            get {
                var amivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpActionType).EnumToLabels();
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    switch ((MpActionType)i) {
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
                    amivml.Add(new MpMenuItemViewModel() {
                        IconResourceKey = Application.Current.Resources[resourceKey] as string,
                        Header = triggerLabels[i],
                        Command = AddChildActionCommand,
                        CommandParameter = (MpActionType)i,
                        IsVisible = (MpActionType)i != MpActionType.None && (MpActionType)i != MpActionType.Trigger
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
                if(IsSelected) {
                    return MpSystemColors.IsSelectedBorderColor;
                }
                if(IsHovering) {
                    return MpSystemColors.IsHoveringBorderColor;
                }
                return MpSystemColors.Transparent;
            }
        }
        #endregion

        #region State

        public bool IsRootAction => ParentActionId == 0;

        public bool IsEnabled { get; set; } = false;

        public bool IsEditingDetails { get; set; }

        public bool IsCompareAction => ActionType == MpActionType.Compare;

        public bool IsAnalyzeAction => ActionType == MpActionType.Analyze;

        public bool IsLabelFocused { get; set; } = false;

        public bool IsActionTextBoxFocused { get; set; } = false;

        public bool IsAnyLabelFocused => IsLabelFocused || this.FindAllChildren().Any(x => x.IsLabelFocused);

        public bool IsAnyActionTextBoxFocused => IsActionTextBoxFocused || this.FindAllChildren().Any(x => x.IsActionTextBoxFocused);

        public bool IsAnyTextBoxFocused => IsAnyLabelFocused || IsAnyActionTextBoxFocused;

        public bool IsValid => string.IsNullOrEmpty(ValidationText);

        public string ValidationText { get; set; }

        #endregion

        #region Model

        #region MpIBoxViewModel Implementation

        public double X {
            get {
                if (Box == null) {
                    return 0;
                }
                return Box.X;
            }
            set {
                if (X != value) {
                    Box.X = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public double Y {
            get {
                if (Box == null) {
                    return 0;
                }
                return Box.Y;
            }
            set {
                if (Y != value) {
                    Box.Y = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public double Width {
            get {
                if (Box == null) {
                    return 0;
                }
                return Box.Width;
            }
            set {
                if (Width != value) {
                    Box.Width = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public double Height {
            get {
                if (Box == null) {
                    return 0;
                }
                return Box.Height;
            }
            set {
                if (Height != value) {
                    Box.Width = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public MpBox Box {
            get {
                if (Action == null) {
                    return null;
                }
                return Action.Box;
            }
        }

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

        public int BoxId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.BoxId;
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

        public event EventHandler<MpCopyItem> OnAction;

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

            if (a.Box == null) {
                if (a.BoxId > 0) {
                    a.Box = await MpDb.GetItemAsync<MpBox>(a.BoxId);
                } else {
                    a.Box = await MpBox.Create(
                        boxType: MpBoxType.DesignerItem,
                        boxObjId: a.Id,
                        x: MpMeasurements.Instance.DefaultDesignerItemWidth / 2,
                        y: MpMeasurements.Instance.DefaultDesignerItemHeight / 2,
                        w: MpMeasurements.Instance.DefaultDesignerItemWidth,
                        h: MpMeasurements.Instance.DefaultDesignerItemHeight);
                }
            }
            Action = a;

            OnPropertyChanged(nameof(Box));
            OnPropertyChanged(nameof(Action));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(X));

            var cal = await MpDataModelProvider.GetChildActions(ActionId);

            foreach(var ca in cal.OrderBy(x=>x.SortOrderIdx)) {
                var cavm = await CreateActionViewModel(ca);
                Items.Add(cavm);
            }

            IsBusy = false;
        }

        public async Task<MpActionViewModelBase> CreateActionViewModel(MpAction a) {
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
                    avm = new MpCompareActionViewModel(Parent);
                    break;
                case MpActionType.Macro:
                    avm = new MpMacroActionViewModel(Parent);
                    break;
                case MpActionType.Timer:
                    avm = new MpTimerActionViewModel(Parent);
                    break;
            }

            await avm.InitializeAsync(a);
            avm.ParentActionViewModel = this;

            return avm;
        }

        public virtual void Enable() {
            Children.OrderBy(x => x.SortOrderIdx).ForEach(x => x.Enable());

            //IsEnabled = true;
        }

        public virtual void Disable() {
            // TODO reverse enable
            Children.OrderBy(x => x.SortOrderIdx).ForEach(x => x.Disable());
            //IsEnabled = false;
        }

        public void OnTrigger(object sender, MpCopyItem ci) {
            OnAction?.Invoke(this, ci);
        }
        
        public virtual void Validate() {
            this.FindAllChildren().ForEach(x => x.Validate());
            if(ActionType == MpActionType.None) {
                ValidationText = "Select Action Type";
            }
        }

        #endregion

        #region MpIMatchTrigger Implementation

        public void RegisterTrigger(MpActionViewModelBase mvm) {
            OnAction += mvm.OnAction;
            MpConsole.WriteLine($"Parent Matcher {Label} Registered {mvm.Label} matcher");
        }

        public void UnregisterTrigger(MpActionViewModelBase mvm) {
            OnAction -= mvm.OnAction;
            MpConsole.WriteLine($"Parent Matcher {Label} Unregistered {mvm.Label} from OnCopyItemAdded");
        }

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        #endregion

        #endregion

        #region Protected Methods


        protected virtual async Task PerformAction(MpCopyItem arg) {
            if (arg == null) {
                return;
            }
            await Task.Delay(1);
            OnAction?.Invoke(this, arg);
        }
        #endregion

        #region Private Methods

        private async Task UpdateSortOrder() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        private void MpActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    //if(Parent == null) {
                    //    return;
                    //}
                    //if(IsSelected) {
                    //    Parent.SelectedItem = this;
                    //}
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
                        Task.Run(async () => {
                            await Box.WriteToDatabaseAsync();
                            await Action.WriteToDatabaseAsync();                            
                        });
                    }
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
                 cm.DataContext = MenuItemViewModel;
                 fe.ContextMenu = cm;
                 fe.ContextMenu.PlacementTarget = fe;
                 fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                 fe.ContextMenu.IsOpen = true;
             });

        public ICommand AddChildActionCommand => new RelayCommand<object>(
             async (args) => {
                 MpActionType at = (MpActionType)args;
                 MpAction na = await MpAction.Create(
                                         actionType: at,
                                         parentId: ActionId,
                                         sortOrderIdx: Items.Count);

                 var navm = await CreateActionViewModel(na);

                 Items.Add(navm);

                 SelectedItem = navm;

                 OnPropertyChanged(nameof(Items));

                 Parent.AllSelectedActions.Add(navm);
                 Parent.OnPropertyChanged(nameof(Parent.AllSelectedActions));

                 IsExpanded = true;
             });

        public ICommand DeleteChildActionCommand => new RelayCommand<object>(
            async (args) => {
                var avm = args as MpActionViewModelBase;
                avm.Disable();
                var deleteTasks = avm.FindAllChildren().Select(x => x.Action.DeleteFromDatabaseAsync()).ToList();
                deleteTasks.AddRange(avm.FindAllChildren().Select(x => x.Action.Box?.DeleteFromDatabaseAsync()).ToList());

                deleteTasks.Add(avm.Action.DeleteFromDatabaseAsync());
                deleteTasks.Add(avm.Action.Box?.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                Items.Remove(avm);

                await UpdateSortOrder();
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItem));
                Parent.AllSelectedActions.Remove(avm);
                Parent.OnPropertyChanged(nameof(Parent.AllSelectedActions));
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
