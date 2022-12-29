
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public abstract class MpAvActionViewModelBase :
        //MpAvTreeSelectorViewModelBase<MpAvTriggerCollectionViewModel, MpAvActionViewModelBase>,
        MpViewModelBase<MpAvTriggerCollectionViewModel>,
        MpIHoverableViewModel,
        //MpISelectableViewModel,
        MpIPopupMenuViewModel,
        MpIUserIconViewModel,
        MpITooltipInfoViewModel,
        MpIBoxViewModel,
        MpIMovableViewModel,
        MpIInvokableAction {
        #region Private Variables

        //private double _maxDeltaLocation = 10;

        private MpPoint _lastLocation = null;


        #endregion

        #region Statics

        public static string GetDefaultActionIconResourceKey(MpActionType at, object subType) {
            switch (at) {
                case MpActionType.Trigger:
                    if(subType is MpTriggerType tt) {
                        switch (tt) {
                            case MpTriggerType.ContentAdded:
                                return "ClipboardImage";
                            case MpTriggerType.ContentTagged:
                                return "PinToCollectionImage";
                            case MpTriggerType.FileSystemChange:
                                return "FolderEventImage";
                            case MpTriggerType.Shortcut:
                                return "HotkeyImage";
                            case MpTriggerType.ParentOutput:
                                return "ChainImage";
                        }
                    }
                    return null;
                case MpActionType.Analyze:
                    return "BrainImage";                    
                case MpActionType.Classify:
                    return "PinToCollectionImage";                    
                case MpActionType.Compare:
                    return "ScalesImage";                    
                case MpActionType.Macro:
                    return "HotkeyImage";                    
                case MpActionType.Timer:
                    return "AlarmClockImage";                    
                case MpActionType.FileWriter:
                    return "FolderEventImage";                    
                case MpActionType.Annotater:
                    return "HighlighterImage";
            }
            // whats params?
            Debugger.Break();
            return null;
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

        #region MpAvTreeSelectorViewModelBase Overrides

        //public override bool IsExpanded => true;



        //bool MpITreeItemViewModel.IsExpanded { get; set; }

        #endregion

        #endregion

        #region MpIMovableViewModel Implementation

        public bool IsMoving { get; set; }

        public bool CanMove { get; set; }

        public int MovableId => ActionId;

        #endregion

        #region MpITooltipInfoViewModel Implementation

        public virtual object Tooltip {
            get {
                string toolTipStr = string.Empty;

                if (this is MpAvAnalyzeActionViewModel) {
                    toolTipStr = "Analyzer - Processes triggered content or previous action output using a selected plugin.";
                } else if (this is MpAvClassifyActionViewModel) {
                    toolTipStr = "Classifier - Automatically adds triggered content to the selected collection.";
                } else if (this is MpAvCompareActionViewModelBase) {
                    toolTipStr = "Comparer - Parses content or previous action output for text. When text is found, the output is ranges where those conditions were met. When comparision fails, no subsequent actions will be evaluated.";
                } else if (this is MpAvFileWriterActionViewModel) {
                    toolTipStr = "File Writer - Saves content to the selected folder.";
                } else if (this is MpAvMacroActionViewModel) {
                    toolTipStr = "Macro - When used after a compare action, embeds a selected local or remote command for onto the text of each match";
                } else if (this is MpAvContentAddTriggerViewModel) {
                    toolTipStr = "Content Added - Triggered when content of the selected type is added";
                } else if (this is MpAvContentTaggedTriggerViewModel) {
                    toolTipStr = "Content Classified - Triggered when content is added to the selected collection";
                } else if (this is MpAvFileSystemTriggerViewModel) {
                    toolTipStr = "Folder Changed - Triggered when a file is added to the selected directory (or subdirectory if checked)";
                } else if (this is MpAvShortcutTriggerViewModel) {
                    toolTipStr = "Shortcut - Triggered when the recorded shortcut is pressed on the currently selected content";
                }

                return toolTipStr;
            }
        }

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

        public bool IsSelected {
            get => Parent.FocusAction == null ? false : Parent.FocusAction.ActionId == ActionId;
            set {
                if(IsSelected != value) {
                    Parent.FocusAction = this;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        #endregion

        #region MpIPopupMenuViewModel Implementation

        public virtual MpMenuItemViewModel PopupMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = "Add",
                            IconResourceKey = "AddImage",
                            SubItems =
                                typeof(MpActionType)
                                .EnumerateEnum<MpActionType>()
                                .Where(x=>x != MpActionType.None && x != MpActionType.Trigger)
                                .Select(x =>
                                    new MpMenuItemViewModel() {
                                        Header = x.EnumToLabel(),
                                        IconResourceKey = GetDefaultActionIconResourceKey(x,null),
                                        Command = AddChildActionCommand,
                                        CommandParameter = x
                                    }).ToList()
                        },
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                            Header = "Remove",
                            IconResourceKey = "DeleteImage",
                            Command = DeleteThisActionCommand
                        }
                    }
                };
            }
        }

        public bool IsPopupMenuOpen { get; set; }

        #endregion

        #region Properties

        #region View Models               
        private MpAvActionViewModelBase _parentTreeItem;
        public MpAvActionViewModelBase ParentTreeItem {
            get {
                if (ParentActionId == 0 || Parent == null) {
                    return null;
                }
                if (_parentTreeItem == null ||
                    _parentTreeItem.ActionId != ParentActionId) {
                    if (_parentTreeItem != null) {
                        // re-parent 
                        MpConsole.WriteLine($"Re-parenting detected for action: {Action}");
                    }
                    // find parent by model
                    _parentTreeItem = Parent.AllActions.FirstOrDefault(x => x.ActionId == ParentActionId);
                }
                return _parentTreeItem;
            }
        }
        public IEnumerable<MpAvActionViewModelBase> Items => 
            Parent.Items
            .Where(x => x.ParentActionId == ActionId).OrderBy(x=>x.SortOrderIdx);

        public IEnumerable<MpAvActionViewModelBase> AllDescendants => 
            Parent.Items
            .Where(x => x.AncestorActionIds.Contains(ActionId))
            .OrderBy(x=>x.TreeLevel)
            .ThenBy(x=>x.SortOrderIdx);
        public IEnumerable<MpAvActionViewModelBase> SelfAndAllDescendants => 
            Parent.Items
            .Where(x => x.AncestorActionIds.Contains(ActionId) || x.ActionId == ActionId)
            .OrderBy(x => x.TreeLevel)
            .ThenBy(x => x.SortOrderIdx);

        public IEnumerable<int> AncestorActionIds {
            get {
                var cur_avm = ParentTreeItem;
                while (cur_avm != null) {
                    yield return cur_avm.ActionId;
                    cur_avm = cur_avm.ParentTreeItem;
                }
            }
        }

        public int TreeLevel => AncestorActionIds.Count();

        public MpAvTriggerActionViewModelBase RootTriggerActionViewModel {
            get {
                var avm = this;
                while (avm.ParentActionId > 0) {
                    avm = avm.ParentTreeItem;
                }
                return avm as MpAvTriggerActionViewModelBase;
            }
        }

        #endregion

        #region Appearance
        public string ActionBackgroundHexColor {
            get {
                //switch (ActionType) {
                //    case MpActionType.Trigger:
                //        return MpSystemColors.maroon;
                //    case MpActionType.Analyze:
                //        return MpSystemColors.magenta;
                //    case MpActionType.Classify:
                //        return MpSystemColors.tomato1;
                //    case MpActionType.Compare:
                //        return MpSystemColors.cyan1;
                //    case MpActionType.Macro:
                //        return MpSystemColors.lightsalmon1;
                //    case MpActionType.Timer:
                //        return MpSystemColors.cornflowerblue;
                //    case MpActionType.FileWriter:
                //        return MpSystemColors.palegoldenrod;
                //}
                //return MpSystemColors.White;
                string keyStr = $"{ActionType}ActionBrush";
                var brush = MpPlatformWrapper.Services.PlatformResource.GetResource(keyStr) as IBrush;
                if(brush == null) {
                    Debugger.Break();
                    return MpSystemColors.Black;
                }
                return brush.ToHex();
            }
        }
        

        public object IconResourceKeyStr {
            get {
                string resourceKey;
                if (IsValid) {
                    if(IconId > 0) {
                        if(ActionType != MpActionType.Trigger) {
                            // triggers should be only type to allow custom icon
                            Debugger.Break();
                        }
                        return IconId;
                    }
                    resourceKey = GetDefaultActionIconResourceKey(ActionType, ActionType == MpActionType.Trigger ? (MpTriggerType)ActionObjId : null);
                } else {
                    resourceKey = "WarningImage";
                }

                return MpPlatformWrapper.Services.PlatformResource.GetResource(resourceKey) as string;
            }
        }

        public string FullName {
            get {
                return this.ToString();
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


        #endregion

        #region State

        public bool IsExpanded { get; set; }
        public bool IsAnyBusy {
            get {
                if(IsBusy) {
                    return true;
                }
                return Items.Any(x => x.IsAnyBusy);
            }
        }

        public bool HasDescription => !string.IsNullOrEmpty(Description);

        public bool IsRootAction => ParentActionId == 0 && this is MpAvTriggerActionViewModelBase;

        public bool LastIsEnabledState { get; set; } = false;

        public bool? IsEnabled { get; set; } = false;


        public bool IsActionTextBoxFocused { get; set; } = false;

        public bool IsValid => string.IsNullOrEmpty(ValidationText);

        private string _validationText;
        public string ValidationText {
            get => _validationText;
            set {
                if(ValidationText != value) {
                    if (!string.IsNullOrEmpty(_validationText)) {
                        // clear invalidations
                        Parent.SetErrorToolTip(ActionId, 0, null);
                    }
                    _validationText = value;
                    OnPropertyChanged(nameof(ValidationText));
                }
            }
        }

        public bool IsHoveringOverLabel { get; set; } = false;

        public bool IsLabelFocused { get; set; } = false;

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

        public double Width => 50;// Parent == null ? 0 : ObservedDesignerItemBounds.Width;

        public double Height => 50;// Parent == null ? 0 : ObservedDesignerItemBounds.Height;

        public MpRect ObservedDesignerItemBounds { get; set; } = MpRect.Empty;

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
                return Action.IsModelReadOnly;
            }
            set {
                if (IsReadOnly != value) {
                    Action.IsModelReadOnly = value;
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

        public MpAvActionViewModelBase(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvActionViewModelBase_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpAction a) {
            IsBusy = true;

            Action = a;

            if (Parent.Items.All(x => x.ActionId != ActionId)) {
                // only add if new
                Parent.Items.Add(this);
            }
            //Items.Clear();

            var cal = await MpDataModelProvider.GetChildActionsAsync(ActionId);
            foreach (var ca in cal.OrderBy(x => x.SortOrderIdx)) {
                await CreateActionViewModel(ca);
            }

            OnPropertyChanged(nameof(Items));

            while (Items.Any(x => x.IsAnyBusy)) {
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
                    avm = new MpAvAnalyzeActionViewModel(Parent);
                    break;
                case MpActionType.Classify:
                    avm = new MpAvClassifyActionViewModel(Parent);
                    break;
                case MpActionType.Compare:
                    avm = new MpAvCompareActionViewModelBase(Parent);
                    break;
                case MpActionType.Macro:
                    avm = new MpAvMacroActionViewModel(Parent);
                    break;
                case MpActionType.Timer:
                    avm = new MpAvTimerActionViewModel(Parent);
                    break;
                case MpActionType.FileWriter:
                    avm = new MpAvFileWriterActionViewModel(Parent);
                    break;
            }
            //avm.ParentTreeItem = this;
            OnActionComplete += avm.OnActionInvoked;

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

        public void OnActionInvoked(object sender, object args) {
            if (!IsEnabled.HasValue) {
                //if action has errors halt 
                return;
            }
            if (!IsEnabled.Value) {
                //if action is disabled pass parent output to child
                OnActionComplete?.Invoke(this, args);
                return;
            }
            Task.Run(() => PerformActionAsync(args).FireAndForgetSafeAsync(this));
        }


        public virtual async Task PerformActionAsync(object arg) {
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

        public MpMenuItemViewModel GetActionMenu(ICommand cmd, IEnumerable<int> selectedActionIds, bool recursive) {
            return new MpMenuItemViewModel() {
                MenuItemId = ActionId,
                Header = Label,
                IconSourceObj = IconResourceKeyStr,
                IsChecked = selectedActionIds.Contains(ActionId),
                Command = cmd,
                CommandParameter = ActionId,
                SubItems = recursive ? Items.Select(x=>x.GetActionMenu(cmd,selectedActionIds, recursive)).ToList() : null
            };
        }

        public override string ToString() {
            if (Action == null) {
                return null;
            }
            if (ParentTreeItem == null) {
                return Label;
            }

            return $"{RootTriggerActionViewModel.Label} - {Label}";
        }

        #endregion

        #region Protected Methods

        protected void ShowValidationNotification(int focusArgNum = 0) {
            Dispatcher.UIThread.Post(async() => {
                var result = await MpNotificationBuilder.ShowNotificationAsync(
                    notificationType: MpNotificationType.InvalidAction,
                    body: ValidationText,
                    retryAction: async (args) => { await ValidateActionAsync(); },
                    fixCommand: Parent.SelectActionCommand,
                    fixCommandArgs: new object[] { ActionId, focusArgNum, ValidationText });
            });
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

        protected virtual async Task<bool> ValidateActionAsync() {
            await Task.Delay(1);
            if (!IsRootAction && ParentTreeItem == null) {
                // this shouldn't happen...
                ValidationText = $"Action '{RootTriggerActionViewModel.Label}/{Label}' must be linked to a trigger";
                ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        protected virtual async Task EnableAsync() {
            await Task.Delay(1);
        }

        protected virtual async Task DisableAsync() { await Task.Delay(1); }

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

            while (RootTriggerActionViewModel.SelfAndAllDescendants.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        prefix.ToLower(),
                                        uniqueIdx);
            }
            return prefix + uniqueIdx;
        }

        private void MpAvActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        //LastSelectedDateTime = DateTime.Now;

                        //Parent.AllSelectedItemActions.ForEach(x => x.IsExpanded = x.ActionId == ActionId);
                        IsExpanded = true;
                        Parent.SelectActionCommand.Execute(this);
                    } else {
                        //IsExpanded = false;
                    }
                    //Parent.OnPropertyChanged(nameof(Parent.FocusAction));
                    //if (this is MpAvTriggerActionViewModelBase && Parent.SelectedItem != this) {
                    //    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    //}
                    //Parent.OnPropertyChanged(nameof(Parent.SelectedAction));
                    //Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
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
                //case nameof(Items):
                //    Parent.NotifyViewportChanged();
                //    break;
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
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    if(ParentTreeItem != null) {
                        ParentTreeItem.OnPropertyChanged(nameof(ParentTreeItem.IsAnyBusy));
                    }
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(SelfAndAllDescendants));
                    break;
                case nameof(IsExpanded):
                    if(!IsSelected) {
                        break;
                    }
                    if(!IsExpanded && IsLabelFocused) {
                        IsExpanded = true;
                    }
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ToggleIsEnabledCommand => new MpAsyncCommand<object>(
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
                    await ValidateActionAsync();
                    if (!IsValid) {
                        IsEnabled = null;
                    } else {
                        await EnableAsync();
                        IsEnabled = true;
                        LastIsEnabledState = true;
                    }
                } else {
                    await DisableAsync();
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
                 IsSelected = true;
                 MpAvMenuExtension.ShowMenu(fe, PopupMenuViewModel);
             },(args) => args is Control);

        public ICommand AddChildActionCommand => new MpCommand<object>(
             async (args) => {
                 IsBusy = true;

                 MpActionType at = (MpActionType)args;
                 MpAction na = await MpAction.CreateAsync(
                                         actionType: at,
                                         label: GetUniqueActionName(at.ToString()),
                                         parentId: ActionId,
                                         sortOrderIdx: Items.Count(),
                                         location: RootTriggerActionViewModel.FindOpenDesignerLocation(Location));

                 var navm = await CreateActionViewModel(na);

                 while(navm.IsBusy) {
                     await Task.Delay(100);
                 }
                 //Items.Add(navm);


                 //AddChildEmptyActionViewModel.IsSelected = false;
                 //navm.IsSelected = true;
                 //Parent.SelectActionCommand.Execute(navm);
                 //Parent.ClearAllOverlaps();

                 //OnPropertyChanged(nameof(Items));

                 RootTriggerActionViewModel.Items.Add(navm);
                 RootTriggerActionViewModel.OnPropertyChanged(nameof(RootTriggerActionViewModel.Items));

                 RootTriggerActionViewModel.OnPropertyChanged(nameof(RootTriggerActionViewModel.SelfAndAllDescendants));
                 Parent.OnPropertyChanged(nameof(Parent.SelectedTriggerAndAllActions));
                 //IsExpanded = true;

                 navm.OnPropertyChanged(nameof(navm.X));
                 navm.OnPropertyChanged(nameof(navm.Y));
                 navm.IsSelected = true;
                 //navm.AddChildEmptyActionViewModel.OnPropertyChanged(nameof(navm.AddChildEmptyActionViewModel.X));
                 //navm.AddChildEmptyActionViewModel.OnPropertyChanged(nameof(navm.AddChildEmptyActionViewModel.Y));

                 IsBusy = false;
             }, (args) => ActionType != MpActionType.None);

        public ICommand DeleteChildActionCommand => new MpCommand<object>(
            async (args) => {
                IsBusy = true;

                var to_delete_avm = args as MpAvActionViewModelBase;
                if(to_delete_avm == null || to_delete_avm.ParentTreeItem != this) {
                    // link error (this cmd is called from arg vm using parentacvm)
                    Debugger.Break();
                    IsBusy = false;
                    return;
                }
                to_delete_avm.ToggleIsEnabledCommand.Execute(false);

                bool remove_descendants = false;
                if(to_delete_avm.Items.Count() > 0) {
                    MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
                    var remove_descendants_result = await MpPlatformWrapper.Services.NativeMessageBox.ShowYesNoCancelMessageBoxAsync(
                        title: $"Remove Options",
                        message: $"Would you like to remove all the sub-actions for '{to_delete_avm.Label}'? (Otherwise they will be re-parented to '{Label}')",
                        iconResourceObj: "ChainImage",
                        anchor: RootTriggerActionViewModel.ObservedDesignerBounds);
                    MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
                    if (remove_descendants_result.IsNull()) {
                        // cancel
                        IsBusy = false;
                        return;
                    }
                    remove_descendants = remove_descendants_result.IsTrue();
                }
                Parent.SelectActionCommand.Execute(this);
                await to_delete_avm.DisableAsync();

                List<Task> delete_tasks = new List<Task>() { to_delete_avm.Action.DeleteFromDatabaseAsync() };
                if (remove_descendants) {
                    delete_tasks.AddRange(to_delete_avm.AllDescendants.Select(x => x.Action.DeleteFromDatabaseAsync()));
                    to_delete_avm.AllDescendants.ForEach(x => x.ToggleIsEnabledCommand.Execute(false));
                    to_delete_avm.AllDescendants.ForEach(x => x.ParentActionId = 0);
                    to_delete_avm.AllDescendants.ForEach(x => RootTriggerActionViewModel.Items.Remove(x));                    
                    to_delete_avm.AllDescendants.ForEach(x => Parent.Items.Remove(x));                    
                } else {
                    // TODO may need to validate new parent child relationship here? not sure
                    foreach(var child in to_delete_avm.Items) {
                        //to_delete_avm.Items.Remove(child);
                        child.ParentActionId = ActionId;
                        //Items.Add(child);
                    }
                }
                await Task.WhenAll(delete_tasks);

                Parent.Items.Remove(to_delete_avm);
                OnPropertyChanged(nameof(Items));
                //to_delete_avm.ParentTreeItem.Items.Remove(to_delete_avm);
                to_delete_avm.ParentActionId = 0;
                RootTriggerActionViewModel.Items.Remove(to_delete_avm);
                RootTriggerActionViewModel.OnPropertyChanged(nameof(RootTriggerActionViewModel.Items));

                IsBusy = false;
            });

        public ICommand DeleteThisActionCommand => new MpCommand(
            () => {
                if (IsRootAction) {
                    Parent.DeleteTriggerCommand.Execute(this);
                    return;
                }

                if (ParentTreeItem == null) {
                    throw new Exception("This shouldn't be null, cannot delete");
                }
                ParentTreeItem.DeleteChildActionCommand.Execute(this);
            });

        public ICommand PerformActionOnSelectedContentCommand => new MpAsyncCommand(
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
                await PerformActionAsync(ao);
                IsPerformingActionFromCommand = false;
            });

        public ICommand FinishMoveCommand => new MpCommand(() => {
            HasModelChanged = true;
        });

        #endregion
    }
}
