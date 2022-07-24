
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvActionOutput {
        public MpCopyItem CopyItem { get; set; }
        public MpAvActionOutput Previous { get; set; }
        public abstract object OutputData { get; }
        public abstract string ActionDescription { get; }
    }

    public abstract class MpAvActionViewModelBase :
        MpViewModelBase<MpActionCollectionViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpIUserIconViewModel,
        MpITreeItemViewModel<MpAvActionViewModelBase>,
        MpITooltipInfoViewModel,
        MpIBoxViewModel,
        MpIMovableViewModel,
        MpIActionTrigger {
        #region Private Variables

        private double _maxDeltaLocation = 10;

        private MpPoint _lastLocation;


        #endregion

        #region Properties

        #region View Models               

        public MpAvActionViewModelBase ParentActionViewModel { get; set; }

        //public MpEmptyActionViewModel AddChildEmptyActionViewModel => Items.FirstOrDefault(x => x is MpEmptyActionViewModel) as MpEmptyActionViewModel;

        public MpAvTriggerActionViewModelBase RootTriggerActionViewModel {
            get {
                var rtavm = this;
                while (rtavm.ParentActionViewModel != null) {
                    rtavm = rtavm.ParentActionViewModel;
                }
                return rtavm as MpAvTriggerActionViewModelBase;
            }
        }
        #endregion

        #region MpIMovableViewModel Implementation

        public bool IsMoving { get; set; }

        public bool CanMove { get; set; }

        public int MovableId => ActionId;

        #endregion

        #region MpITooltipInfoViewModel Implementation

        public object Tooltip {
            get {
                string toolTipStr = string.Empty;

                if (this is MpAnalyzeActionViewModel) {
                    toolTipStr = "Analyzer - Processes triggered content or previous action output using a selected plugin.";
                } else if (this is MpClassifyActionViewModel) {
                    toolTipStr = "Classifier - Automatically adds triggered content to the selected collection.";
                } else if (this is MpCompareActionViewModelBase) {
                    toolTipStr = "Comparer - Parses content or previous action output for text. When text is found, the output is ranges where those conditions were met. When comparision fails, no subsequent actions will be evaluated.";
                } else if (this is MpFileWriterActionViewModel) {
                    toolTipStr = "File Writer - Saves content to the selected folder.";
                } else if (this is MpMacroActionViewModel) {
                    toolTipStr = "Macro - When used after a compare action, embeds a selected local or remote command for onto the text of each match";
                } else if (this is MpAvContentAddTriggerViewModel) {
                    toolTipStr = "Content Added - Triggered when content of the selected type is added";
                } else if (this is MpAvContentTaggedTriggerViewModel) {
                    toolTipStr = "Content Classified - Triggered when content is added to the selected collection";
                } else if (this is MpFileSystemTriggerViewModel) {
                    toolTipStr = "Folder Changed - Triggered when a file is added to the selected directory (or subdirectory if checked)";
                } else if (this is MpShortcutTriggerViewModel) {
                    toolTipStr = "Shortcut - Triggered when the recorded shortcut is pressed on the currently selected content";
                }

                return toolTipStr;
            }
        }

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; } = false;

        public MpAvActionViewModelBase ParentTreeItem => ParentActionViewModel;

        public ObservableCollection<MpAvActionViewModelBase> Items { get; set; } = new ObservableCollection<MpAvActionViewModelBase>();

        public ObservableCollection<MpAvActionViewModelBase> Children => Items;


        MpITreeItemViewModel MpITreeItemViewModel.ParentTreeItem { get; }
        ObservableCollection<MpITreeItemViewModel> MpITreeItemViewModel.Children { get; }
        #endregion

        #region MpIUserIcon Implementation

        public int IconId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.IconId;
            }
            set {
                if (IconId != value) {
                    Action.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; } = false;

        #endregion

        #region MpIMenuItemViewModel Implementation

        public virtual MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                var amivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpActionType).EnumToLabels();
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    MpActionType at = (MpActionType)i;
                    switch (at) {
                        case MpActionType.Analyze:
                            resourceKey = "BrainImage";
                            break;
                        case MpActionType.Classify:
                            resourceKey = "PinToCollectionImage";
                            break;
                        case MpActionType.Compare:
                            resourceKey = "ScalesImage";
                            break;
                        case MpActionType.Macro:
                            resourceKey = "HotkeyImage";
                            break;
                        case MpActionType.Timer:
                            resourceKey = "AlarmClockImage";
                            break;
                        case MpActionType.FileWriter:
                            resourceKey = "FolderEventImage";
                            break;
                        case MpActionType.Annotater:
                            resourceKey = "HighlighterImage";
                            break;
                    }
                    bool isVisible = true;
                    if (at == MpActionType.None || at == MpActionType.Trigger) {
                        isVisible = false;
                    }
                    if (GetType().IsSubclassOf(typeof(MpAvTriggerActionViewModelBase)) &&
                       GetType() != typeof(MpCompareActionViewModelBase) &&
                       at == MpActionType.Macro) {
                        isVisible = false;
                    }
                    amivml.Add(new MpMenuItemViewModel() {
                        IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource(resourceKey) as string,
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
                if (Parent != null &&
                   Parent.PrimaryAction != null &&
                   Parent.PrimaryAction.ActionId == ActionId) {
                    if (IsEnabled.HasValue && IsEnabled.Value) {
                        return MpSystemColors.limegreen;
                    }
                    return MpSystemColors.IsSelectedBorderColor;
                }
                if (IsHovering) {
                    return MpSystemColors.IsHoveringBorderColor;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string LabelBorderBrushHexColor {
            get {
                if (IsHoveringOverLabel || IsLabelFocused) {
                    return MpSystemColors.White;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string LabelForegroundBrushHexColor {
            get {
                if (IsLabelFocused) {
                    return MpSystemColors.Black;
                }
                return MpColorHelpers.IsBright(ActionBackgroundHexColor) ? MpSystemColors.black : MpSystemColors.white;
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
                string resourceKey;
                if (IsValid) {
                    switch (ActionType) {
                        case MpActionType.Trigger:
                            resourceKey = new MpEnumToImageResourceKeyConverter().Convert((MpTriggerType)ActionObjId, null, null, null) as string;
                            break;
                        default:
                            resourceKey = new MpEnumToImageResourceKeyConverter().Convert(ActionType, null, null, null) as string;
                            break;
                    }
                } else {
                    resourceKey = "WarningImage";
                }

                return MpPlatformWrapper.Services.PlatformResource.GetResource(resourceKey) as string;
            }
        }

        public string FullName {
            get {
                if (Action == null) {
                    return null;
                }
                if (ParentActionViewModel == null) {
                    return Label;
                }

                return $"{ParentActionViewModel.Label}/{Label}";
            }
        }

        public string EnableToggleButtonShapeHexColor {
            get {
                if (!IsEnabled.HasValue) {
                    return MpSystemColors.Yellow;
                }
                return IsEnabled.Value ? MpSystemColors.limegreen : MpSystemColors.Red;
            }
        }

        public string EnableToggleButtonTooltip {
            get {
                if (!IsEnabled.HasValue) {
                    return ValidationText;
                }
                return IsEnabled.Value ? "Click To Disable" : "Click To Enable";
            }
        }

        public string ActionBackgroundHexColor {
            get {
                switch (ActionType) {
                    case MpActionType.Trigger:
                        return MpSystemColors.maroon;
                    case MpActionType.Analyze:
                        return MpSystemColors.magenta;
                    case MpActionType.Classify:
                        return MpSystemColors.tomato1;
                    case MpActionType.Compare:
                        return MpSystemColors.cyan1;
                    case MpActionType.Macro:
                        return MpSystemColors.lightsalmon1;
                    case MpActionType.Timer:
                        return MpSystemColors.cornflowerblue;
                    case MpActionType.FileWriter:
                        return MpSystemColors.palegoldenrod;
                }
                return MpSystemColors.White;
            }
        }

        #endregion

        #region State

        public bool IsPlaceholder => ActionType == MpActionType.None;

        public bool HasDescription => !string.IsNullOrEmpty(Description);

        public bool IsRootAction => ParentActionId == 0 && this is MpAvTriggerActionViewModelBase;

        public bool LastIsEnabledState { get; set; } = false;

        public bool? IsEnabled { get; set; } = false;

        public bool IsEditingDetails { get; set; }

        public bool IsCompareAction => ActionType == MpActionType.Compare;

        public bool IsAnalyzeAction => ActionType == MpActionType.Analyze;

        public bool IsActionTextBoxFocused { get; set; } = false;

        public bool IsAnyActionTextBoxFocused => IsActionTextBoxFocused || this.FindAllChildren().Any(x => x.IsActionTextBoxFocused);

        public bool IsValid => string.IsNullOrEmpty(ValidationText);

        public string ValidationText { get; set; }

        public bool IsHoveringOverLabel { get; set; } = false;

        public bool IsLabelFocused { get; set; } = false;

        public bool IsAnyChildSelected => this.FindAllChildren().Any(x => x.IsSelected);

        public MpPoint DefaultEmptyActionLocation => new MpPoint(X, Y - (Height * 2));

        public bool IsPerformingActionFromCommand { get; set; } = false;

        #endregion

        #region Model

        #region MpIBoxViewModel Implementation

        public MpPoint Center => new MpPoint(Location.X + (Width / 2), Location.Y + (Height / 2));

        public MpPoint Location {
            get {
                return new MpPoint(X, Y);
            }
            set {
                if (Location != value) {
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

        public double Width => Parent == null ? 0 : Parent.DesignerItemDiameter;

        public double Height => Parent == null ? 0 : Parent.DesignerItemDiameter;


        #endregion

        public string Arg1 {
            get {
                if (Action == null) {
                    return string.Empty;
                }
                return Action.Arg1;
            }
            set {
                if (Arg1 != value) {
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
                    return null;
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

        public string Arg5 {
            get {
                if (Action == null) {
                    return string.Empty;
                }
                return Action.Arg5;
            }
            set {
                if (Arg5 != value) {
                    Action.Arg5 = value; // NOTE no HasModelChanged for abstract fields
                    OnPropertyChanged(nameof(Arg5));
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

        public bool IsEnabledDb {
            get {
                if (Action == null) {
                    return false;
                }
                return Action.IsEnabled;
            }
            set {
                if (IsEnabledDb != value) {
                    Action.IsEnabled = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsEnabledDb));
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
                if (string.IsNullOrEmpty(Action.Label)) {
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




        public int ParentActionId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.ParentActionId;
            }
            set {
                if (ParentActionId != value) {
                    Action.ParentActionId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ParentActionId));
                }
            }
        }

        public int ActionId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.Id;
            }
        }

        public DateTime LastSelectedDateTime {
            get {
                if (Action == null) {
                    return DateTime.MinValue;
                }
                return Action.LastSelectedDateTime;
            }
            set {
                if (LastSelectedDateTime != value) {
                    Action.LastSelectedDateTime = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(LastSelectedDateTime));
                }
            }
        }

        public MpAction Action { get; set; }

        #endregion

        #endregion

        #region Events

        public event EventHandler<object> OnActionComplete;

        #endregion

        #region Constructors

        public MpAvActionViewModelBase() : base(null) { }

        public MpAvActionViewModelBase(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpActionViewModelBase_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpAction a) {
            IsBusy = true;

            Action = a;

            Items.Clear();

            //if(!IsEmptyAction) {
            //    var eavm = await CreateEmptyActionViewModel();
            //    Items.Add(eavm);
            //}

            if (!IsPlaceholder) {
                var cal = await MpDataModelProvider.GetChildActionsAsync(ActionId);

                foreach (var ca in cal.OrderBy(x => x.SortOrderIdx)) {
                    var cavm = await CreateActionViewModel(ca);
                    Items.Add(cavm);
                }
            }

            OnPropertyChanged(nameof(Items));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }

        public async Task<MpAvActionViewModelBase> CreateActionViewModel(MpAction a) {
            a = a == null ? new MpAction() : a;
            MpAvActionViewModelBase avm = null;
            switch (a.ActionType) {
                case MpActionType.Trigger:
                    throw new Exception("Trigger's should be created in the collection forest");
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
                case MpActionType.FileWriter:
                    avm = new MpFileWriterActionViewModel(Parent);
                    break;
            }
            avm.ParentActionViewModel = this;
            OnActionComplete += avm.OnActionTriggered;

            await avm.InitializeAsync(a);

            return avm;
        }

        //public async Task<MpEmptyActionViewModel> CreateEmptyActionViewModel() {
        //    MpEmptyActionViewModel eavm = new MpEmptyActionViewModel(Parent);
        //    eavm.ParentActionViewModel = this;

        //    var emptyAction = await MpAction.Create(
        //        location: DefaultEmptyActionLocation.ToMpPoint(),
        //        suppressWrite: true);

        //    await eavm.InitializeAsync(emptyAction);

        //    return eavm;
        //}

        public void OnActionTriggered(object sender, object args) {
            if (!IsEnabled.HasValue) {
                //if action has errors halt 
                return;
            }
            if (!IsEnabled.Value) {
                //if action is disabled pass parent output to child
                OnActionComplete?.Invoke(this, args);
                return;
            }
            Task.Run(() => PerformAction(args).FireAndForgetSafeAsync(this));
        }


        public virtual async Task PerformAction(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            if (arg is MpAvActionOutput ao) {
                MpConsole.WriteLine("");
                MpConsole.WriteLine($"Action({ActionId}) '{Label}' Completed Successfully");
                MpConsole.WriteLine(ao.ActionDescription);
                MpConsole.WriteLine("");
            }
            await Task.Delay(1);
            OnActionComplete?.Invoke(this, arg);
        }


        #endregion

        #region MpIActionComponentHandler Implementation

        public void RegisterTrigger(MpAvActionViewModelBase mvm) {
            OnActionComplete += mvm.OnActionComplete;
            MpConsole.WriteLine($"Parent Matcher {Label} Registered {mvm.Label} matcher");
        }

        public void UnregisterTrigger(MpAvActionViewModelBase mvm) {
            OnActionComplete -= mvm.OnActionComplete;
            MpConsole.WriteLine($"Parent Matcher {Label} Unregistered {mvm.Label} from OnCopyItemAdded");
        }

        #endregion

        #region Protected Methods

        protected async Task ShowValidationNotification() {
            //bool wasBusy = IsBusy;
            //IsBusy = true;
            //MpDialogResultType userAction = MpDialogResultType.None;

            //await Dispatcher.UIThread.InvokeAsync(async () => {
            //    userAction = await MpNotificationCollectionViewModel.Instance.ShowUserAction(
            //        dialogType: MpNotificationDialogType.InvalidAction,
            //        exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
            //        msg: ValidationText,
            //        retryAction: async(args)=> { await Validate(); },
            //        fixCommand: Parent.SelectActionCommand,
            //        fixCommandArgs: ActionId);
            //});
            await Task.Delay(1);
            Dispatcher.UIThread.Post(() => {
                MpNotificationCollectionViewModel.Instance.ShowNotificationAsync(
                    dialogType: MpNotificationDialogType.InvalidAction,
                    msg: ValidationText,
                    retryAction: async (args) => { await Validate(); },
                    fixCommand: Parent.SelectActionCommand,
                    fixCommandArgs: ActionId).FireAndForgetSafeAsync(this);
            });


            //if (userAction == MpDialogResultType.Retry) {
            //    if(ParentActionViewModel == null) {
            //        // NOTE parent vm is nulled when invalid action was deleted so is no longer an issue
            //        wasBusy = false;
            //    } else {
            //        await Validate();
            //    }
            //} else if (userAction == MpDialogResultType.Ignore) {
            //    wasBusy = false;
            //}
            //IsBusy = wasBusy;
        }

        protected void FinishPerformingAction(object arg) {
            OnActionComplete?.Invoke(this, arg);
        }

        protected virtual MpAvActionOutput GetInput(object arg) {
            if (arg is MpCopyItem ci) {
                // NOTE this should only happen for triggers
                return new MpAvTriggerInput() {
                    CopyItem = ci
                };
            } else if (arg is MpAvActionOutput ao) {
                return ao;
            }
            throw new Exception("Unknown action input: " + arg.ToString());
        }

        protected virtual async Task<bool> Validate() {
            if (!IsRootAction && ParentActionViewModel == null) {
                // this shouldn't happen...
                ValidationText = $"Action '{RootTriggerActionViewModel.Label}/{Label}' must be linked to a trigger";
                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        protected virtual async Task Enable() {
            await Task.Delay(1);
        }

        protected virtual async Task Disable() { await Task.Delay(1); }

        protected virtual bool CanPerformAction(object arg) {
            if (!IsValid ||
               !IsEnabled.HasValue ||
               (IsEnabled.HasValue && !IsEnabled.Value)) {
                return false;
            }
            return true;
        }


        protected void ResetArgs(params object[] argNums) {
            if (argNums == null) {
                Arg1 = Arg2 = Arg3 = Arg4 = Arg5 = null;
                return;
            }
            var argNumVals = argNums.Cast<int>().ToArray();
            if (argNumVals.Contains(1)) {
                Arg1 = null;
            }
            if (argNumVals.Contains(2)) {
                Arg2 = null;
            }
            if (argNumVals.Contains(3)) {
                Arg3 = null;
            }
            if (argNumVals.Contains(4)) {
                Arg4 = null;
            }
            if (argNumVals.Contains(5)) {
                Arg5 = null;
            }
        }

        protected async Task ReEnable() {
            if (IsEnabled.HasValue && IsEnabled.Value) {
                //if is enabled disable
                ToggleIsEnabledCommand.Execute(null);
                while (IsBusy) { await Task.Delay(100); }
            }
            ToggleIsEnabledCommand.Execute(null);

            while (IsBusy) { await Task.Delay(100); }
        }
        #endregion

        #region Private Methods

        private async Task UpdateSortOrder() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        public string GetUniqueActionName(string prefix) {
            int uniqueIdx = 1;
            string testName = string.Format(
                                        @"{0}{1}",
                                        prefix.ToLower(),
                                        uniqueIdx);

            while (Parent.AllSelectedTriggerActions.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        prefix.ToLower(),
                                        uniqueIdx);
            }
            return prefix + uniqueIdx;
        }

        private void MpActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        Parent.AllSelectedTriggerActions.ForEach(x => x.IsExpanded = x.ActionId == ActionId);
                        IsExpanded = true;
                    } else {
                        IsExpanded = false;
                    }

                    if (this is MpAvTriggerActionViewModelBase && Parent.SelectedItem != this) {
                        Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    }
                    Parent.OnPropertyChanged(nameof(Parent.PrimaryAction));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedActions));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    OnPropertyChanged(nameof(BorderBrushHexColor));
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged && IsValid) {
                        HasModelChanged = false;

                        Task.Run(async () => {
                            await Action.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(Items):
                    Parent.NotifyViewportChanged();
                    break;
                case nameof(Location):
                case nameof(X):
                case nameof(Y):
                    //if(!IsBusy) {
                    //    if (_lastLocation.Distance(Location) > _maxDeltaLocation) {
                    //        var delta = Location - _lastLocation;
                    //        delta.Normalize();
                    //        _lastLocation = Location = _lastLocation + (delta * _maxDeltaLocation);
                    //    }
                    //}

                    //if (AddChildEmptyActionViewModel == null) {
                    //    break;
                    //}
                    //AddChildEmptyActionViewModel.Location = DefaultEmptyActionLocation;

                    _lastLocation = Location;
                    break;
                case nameof(IsEnabled):
                    if (!IsEnabled.HasValue) {
                        return;
                    }
                    IsEnabledDb = IsEnabled.Value;
                    OnPropertyChanged(nameof(EnableToggleButtonShapeHexColor));
                    break;
            }
        }
        #endregion

        #region Commands

        public MpIAsyncCommand<object> ToggleIsEnabledCommand => new MpAsyncCommand<object>(
            async (parentToggledState) => {
                bool wasBusy = IsBusy;
                IsBusy = true;

                bool newIsEnabledState = false;
                if (parentToggledState != null) {
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
                        IsEnabled = true;
                        LastIsEnabledState = true;
                    }
                } else {
                    await Disable();
                    IsEnabled = false;
                    LastIsEnabledState = false;
                }
                //if(IsEnabled.HasValue) {
                //    Children.ForEach(x => x.ToggleIsEnabledCommand.Execute(IsEnabled.Value));
                //}

                //while(Children.Any(x=>x.IsBusy)) {
                //    await Task.Delay(100);
                //}

                IsBusy = false;
            });

        public ICommand ShowActionSelectorMenuCommand => new MpCommand<object>(
             (args) => {
                 var fe = args as Control;
                 var cm = MpAvContextMenuView.Instance;
                 //LastSelectedDateTime = DateTime.Now;
                 IsSelected = true;

                 cm.DataContext = ContextMenuItemViewModel;
                 fe.ContextMenu = cm;
                 fe.ContextMenu.PlacementTarget = fe;
                 fe.ContextMenu.PlacementAnchor = PopupAnchor.Right; //System.Windows.Controls.Primitives.PlacementMode.Right;
                 fe.ContextMenu.Open();//IsOpen = true;
             });

        public ICommand AddChildActionCommand => new MpCommand<object>(
             async (args) => {
                 IsBusy = true;

                 MpActionType at = (MpActionType)args;
                 MpAction na = await MpAction.Create(
                                         actionType: at,
                                         label: GetUniqueActionName(at.ToString()),
                                         parentId: ActionId,
                                         sortOrderIdx: Items.Count,
                                         location: Parent.FindOpenDesignerLocation(Location));

                 var navm = await CreateActionViewModel(na);

                 Items.Add(navm);

                 //AddChildEmptyActionViewModel.IsSelected = false;
                 navm.IsSelected = true;

                 //Parent.ClearAllOverlaps();

                 OnPropertyChanged(nameof(Items));

                 Parent.OnPropertyChanged(nameof(Parent.AllSelectedTriggerActions));

                 IsExpanded = true;

                 navm.OnPropertyChanged(nameof(navm.X));
                 navm.OnPropertyChanged(nameof(navm.Y));
                 //navm.AddChildEmptyActionViewModel.OnPropertyChanged(nameof(navm.AddChildEmptyActionViewModel.X));
                 //navm.AddChildEmptyActionViewModel.OnPropertyChanged(nameof(navm.AddChildEmptyActionViewModel.Y));

                 IsBusy = false;
             }, (args) => ActionType != MpActionType.None);

        public ICommand DeleteChildActionCommand => new MpCommand<object>(
            async (args) => {
                IsBusy = true;

                var avm = args as MpAvActionViewModelBase;
                await avm.Disable();

                var grandChildren = Parent.AllSelectedTriggerActions.Where(x => x.ParentActionId == avm.ActionId);
                grandChildren.ForEach(x => x.ParentActionId = ActionId);
                grandChildren.ForEach(x => x.ParentActionViewModel = this);
                await Task.WhenAll(grandChildren.Select(x => x.Action.WriteToDatabaseAsync()));
                await avm.Action.DeleteFromDatabaseAsync();

                Parent.OnPropertyChanged(nameof(Parent.AllSelectedTriggerActions));
                Parent.AllSelectedTriggerActions.Remove(avm);
                await RootTriggerActionViewModel.InitializeAsync(RootTriggerActionViewModel.Action);

                avm.ParentActionViewModel = null;

                Parent.SelectedItem = RootTriggerActionViewModel;
                IsSelected = true;
                //Parent.NotifyViewportChanged();

                IsBusy = false;
            });

        public ICommand DeleteThisActionCommand => new MpCommand(
            () => {
                if (IsRootAction) {
                    Parent.DeleteTriggerCommand.Execute(this);
                    return;
                }

                if (ParentActionViewModel == null) {
                    throw new Exception("This shouldn't be null, cannot delete");
                }
                ParentActionViewModel.DeleteChildActionCommand.Execute(this);
            });

        public MpIAsyncCommand PerformActionOnSelectedContentCommand => new MpAsyncCommand(
            async () => {
                if(MpAvClipTrayViewModel.Instance.SelectedItem == null) {
                    return;
                }
                var ci = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
                if (ci == null) {
                    return;
                }
                IsPerformingActionFromCommand = true;
                var ao = GetInput(ci);
                await PerformAction(ao);
                IsPerformingActionFromCommand = false;
            });


        #endregion
    }
}
