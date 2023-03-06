
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public abstract class MpAvActionViewModelBase :
        MpViewModelBase<MpAvTriggerCollectionViewModel>,
        MpIActionPluginComponent,
        MpITreeItemViewModel,
        MpIHoverableViewModel,
        MpIPopupMenuViewModel,
        MpIContextMenuViewModel,
        MpITooltipInfoViewModel,
        MpILabelText,
        MpIBoxViewModel,
        MpIMovableViewModel,
        MpIInvokableAction,
        MpIParameterHostViewModel,
        MpAvIParameterCollectionViewModel,
        MpIPopupMenuPicker {

        #region Private Variables

        private bool _isShowingValidationMsg = false;
        #endregion

        #region Statics

        public static string GetDefaultActionIconResourceKey(object actionOrTriggerType) {
            if (actionOrTriggerType is MpAvActionViewModelBase avmb) {
                if (avmb is MpAvTriggerActionViewModelBase tvmb) {
                    actionOrTriggerType = tvmb.TriggerType;
                } else {
                    actionOrTriggerType = avmb.ActionType;
                }
            }
            if (actionOrTriggerType is MpTriggerType tt) {
                switch (tt) {
                    case MpTriggerType.ContentAdded:
                        return "ClipboardImage";
                    case MpTriggerType.ContentTagged:
                        return "PinToCollectionImage";
                    case MpTriggerType.FileSystemChange:
                        return "FolderEventImage";
                    case MpTriggerType.Shortcut:
                        return "HotkeyImage";
                }
            }
            if (actionOrTriggerType is MpActionType at) {
                switch (at) {
                    case MpActionType.Analyze:
                        return "BrainImage";
                    case MpActionType.Classify:
                        return "PinToCollectionImage";
                    case MpActionType.Compare:
                        return "ScalesImage";
                    case MpActionType.Repeater:
                        return "AlarmClockImage";
                    case MpActionType.FileWriter:
                        return "FolderEventImage";
                }
            }
            // whats params?
            Debugger.Break();
            return "QuestiongMarkImage";
        }
        #endregion

        #region Interfaces


        #region MpITreeItemViewModel Implementation

        IEnumerable<MpITreeItemViewModel> MpITreeItemViewModel.Children =>
            RootTriggerActionViewModel == null ? null :
            RootTriggerActionViewModel.SelfAndAllDescendants.Where(x => x.ParentActionId == ActionId);

        MpITreeItemViewModel MpITreeItemViewModel.ParentTreeItem =>
            RootTriggerActionViewModel == null ? null :
            RootTriggerActionViewModel.SelfAndAllDescendants.FirstOrDefault(x => x.ActionId == ParentActionId);

        #region MpITreeItemViewModel Implementation
        bool MpIExpandableViewModel.IsExpanded { get => false; set => _ = value; }

        #endregion

        #endregion

        #region MpIActionPluginComponent Implementation
        Task MpIActionPluginComponent.PerformActionAsync(object arg) => PerformActionAsync(arg);

        bool MpIActionPluginComponent.CanPerformAction(object arg) => CanPerformAction(arg);

        Task MpIActionPluginComponent.ValidateActionAsync() => ValidateActionAsync();

        string MpIActionPluginComponent.ValidationText => ValidationText;
        #endregion

        #region MpILabelText Implementation

        public MpMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedActionIds, bool recursive) {
            return new MpMenuItemViewModel() {
                MenuItemId = ActionId,
                Header = Label,
                IconSourceObj = IconResourceObj,
                IsChecked = selectedActionIds.Contains(ActionId),
                Command = cmd,
                CommandParameter = ActionId,
                SubItems = recursive ? Children.Cast<MpIPopupMenuPicker>().Select(x => x.GetMenu(cmd, cmdArg, selectedActionIds, recursive)).ToList() : null
            };
        }
        #endregion

        #region MpILabelText Implementation

        string MpILabelText.LabelText => Label;

        #endregion

        #region MpAvIParameterCollectionViewModel Implementation

        IEnumerable<MpAvParameterViewModelBase> MpAvIParameterCollectionViewModel.Items => ActionArgs;
        MpAvParameterViewModelBase MpAvIParameterCollectionViewModel.SelectedItem { get; set; }

        #region MpISaveOrCancelableViewModel Implementation

        public ICommand SaveCommand => new MpCommand(
            () => {
                ActionArgs.ForEach(x => x.SaveCurrentValueCommand.Execute(null));
            },
            () => {
                return CanSaveOrCancel;
            }, new[] { this });
        public ICommand CancelCommand => new MpCommand(
            () => {
                ActionArgs.ForEach(x => x.RestoreLastValueCommand.Execute(null));
            },
            () => {
                return CanSaveOrCancel;
            }, new[] { this });

        //private bool _canSaveOrCancel = false;
        //public bool CanSaveOrCancel {
        //    get {
        //        bool hasChanged = ActionArgs.Any(x => x.HasModelChanged);
        //        if (hasChanged) {
        //            SaveCommand.Execute(null);
        //            //Task.Run(async () => {

        //            //});
        //            // this should notify action parameter property changes
        //            //OnPropertyChanged(nameof(ArgLookup));
        //            // save args on any change
        //        }
        //        // NOTE disabling cancelable on actions, only used for analyzers
        //        return false;
        //    }
        //}
        private bool _canSaveOrCancel = false;
        public bool CanSaveOrCancel {
            get {
                bool result = ActionArgs.Any(x => x.HasModelChanged);
                if (result != _canSaveOrCancel) {
                    _canSaveOrCancel = result;
                    OnPropertyChanged(nameof(CanSaveOrCancel));
                    if (!_canSaveOrCancel) {
                        // leaf actions need to use ActionArgs property change to update parameter properties
                        OnPropertyChanged(nameof(ActionArgs));
                    }
                }
                return _canSaveOrCancel;
            }
        }

        #endregion

        #endregion

        #region MpIParameterHost Implementation

        int MpIParameterHostViewModel.IconId => 0;
        public string PluginGuid =>
            PluginFormat == null ? string.Empty : PluginFormat.guid;

        public MpPluginFormat PluginFormat { get; set; }

        public MpParameterHostBaseFormat ComponentFormat => ActionComponentFormat;
        MpParameterHostBaseFormat MpIParameterHostViewModel.BackupComponentFormat =>
            PluginFormat == null || PluginFormat.backupCheckPluginFormat == null || PluginFormat.backupCheckPluginFormat.action == null ?
                null : PluginFormat.backupCheckPluginFormat.action;

        public virtual MpActionPluginFormat ActionComponentFormat { get; protected set; }

        public MpIPluginComponentBase PluginComponent =>
            PluginFormat == null ? null : PluginFormat.Component as MpIPluginComponentBase;

        public Dictionary<object, MpAvParameterViewModelBase> ArgLookup =>
           ActionArgs.ToDictionary(x => x.ParamId, x => x);
        public virtual ObservableCollection<MpAvParameterViewModelBase> ActionArgs { get; protected set; } = new ObservableCollection<MpAvParameterViewModelBase>();

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
                } else if (this is MpAvContentAddTriggerViewModel) {
                    toolTipStr = "Content Added - Triggered when content of the selected type is added";
                } else if (this is MpAvContentTaggedTriggerViewModel) {
                    toolTipStr = "Content Classified - Triggered when content is added to the selected collection";
                } else if (this is MpAvFolderWatcherTriggerViewModel) {
                    toolTipStr = "Folder Changed - Triggered when a file is added to the selected directory (or subdirectory if checked)";
                } else if (this is MpAvShortcutTriggerViewModel) {
                    toolTipStr = "Shortcut - Triggered when the recorded shortcut is pressed at anytime with the current clipboard";
                }

                return toolTipStr;
            }
        }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected {
            get => Parent.FocusAction == null ? false : Parent.FocusAction.ActionId == ActionId;
            //set {

            //    if(IsSelected != value) {
            //        Parent.FocusAction = this;
            //        OnPropertyChanged(nameof(IsSelected));
            //    }
            //}
        }

        #endregion

        #region MpIContextMenuViewModel Implementation

        public bool IsContextMenuOpen { get; set; }
        public MpMenuItemViewModel ContextMenuViewModel => PopupMenuViewModel;

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
                                        IconResourceKey = GetDefaultActionIconResourceKey(x),
                                        Command = AddChildActionCommand,
                                        CommandParameter = x
                                    }).ToList()
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
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

        #endregion

        #region Properties

        #region View Models       

        private MpAvActionViewModelBase _parentActionViewModel;
        public MpAvActionViewModelBase ParentActionViewModel {
            get {
                if (ParentActionId == 0 || Parent == null) {
                    return null;
                }
                if (_parentActionViewModel == null ||
                    _parentActionViewModel.ActionId != ParentActionId) {
                    if (_parentActionViewModel != null) {
                        // re-parent 
                        MpConsole.WriteLine($"Re-parenting detected for action: {Action}");
                    }
                    // find parent by model
                    _parentActionViewModel = Parent.Items.FirstOrDefault(x => x.ActionId == ParentActionId);
                }
                return _parentActionViewModel;
            }
        }
        public IEnumerable<MpAvActionViewModelBase> Children =>
            Parent.Items
            .Where(x => x.ParentActionId == ActionId).OrderBy(x => x.SortOrderIdx);

        public IEnumerable<MpAvActionViewModelBase> AllDescendants =>
            Parent.Items
            .Where(x => x.AncestorActionIds.Contains(ActionId))
            .OrderBy(x => x.TreeLevel)
            .ThenBy(x => x.SortOrderIdx);
        public IEnumerable<MpAvActionViewModelBase> SelfAndAllDescendants =>
            Parent.Items
            .Where(x => x.AncestorActionIds.Contains(ActionId) || x.ActionId == ActionId)
            .OrderBy(x => x.TreeLevel)
            .ThenBy(x => x.SortOrderIdx);

        public IEnumerable<int> AncestorActionIds {
            get {
                var cur_avm = ParentActionViewModel;
                while (cur_avm != null) {
                    yield return cur_avm.ActionId;
                    cur_avm = cur_avm.ParentActionViewModel;
                }
            }
        }

        public int TreeLevel => AncestorActionIds.Count();

        public MpAvTriggerActionViewModelBase RootTriggerActionViewModel {
            get {
                var avm = this;
                while (avm.ParentActionId > 0) {
                    avm = avm.ParentActionViewModel;
                }
                return avm as MpAvTriggerActionViewModelBase;
            }
        }

        #endregion

        #region Appearance
        public string ActionBackgroundHexColor {
            get {
                string keyStr = $"{ActionType}ActionBrush";
                var brush = Mp.Services.PlatformResource.GetResource(keyStr) as IBrush;
                if (brush == null) {
                    Debugger.Break();
                    return MpSystemColors.Black;
                }
                return brush.ToHex();
            }
        }

        public string IconBackgroundHexColor {
            get {
                if (IconResourceObj is string &&
                    GetDefaultActionIconResourceKey(this) == IconResourceObj.ToString()) {
                    return ActionBackgroundHexColor;
                }
                // icon is overriden by action, avoid tinting
                return MpSystemColors.Transparent;
            }
        }

        public virtual object IconResourceObj {
            get {
                string resourceKey;
                if (IsValid) {
                    resourceKey = GetDefaultActionIconResourceKey(ActionType);
                } else {
                    resourceKey = "WarningImage";
                }
                return resourceKey;
            }
        }


        #endregion

        #region State

        public bool HasArgsChanged { get; set; }
        public string FullName {
            get {
                return this.ToString();
            }
        }
        public bool IsActionDesignerVisible => Parent.SelectedTrigger == RootTriggerActionViewModel;

        public bool IsAnyBusy {
            get {
                if (IsBusy) {
                    return true;
                }
                return Children.Any(x => x.IsAnyBusy);
            }
        }

        public bool HasDescription => !string.IsNullOrEmpty(Description);

        public bool IsTrigger => ParentActionId == 0 && this is MpAvTriggerActionViewModelBase;


        public bool IsActionTextBoxFocused { get; set; } = false;

        public bool IsValid => string.IsNullOrEmpty(ValidationText);

        private string _validationText;
        public string ValidationText {
            get => _validationText;
            set {
                if (ValidationText != value) {
                    if (!string.IsNullOrEmpty(_validationText)) {
                        // clear invalidations
                        Parent.SetErrorToolTip(ActionId, 0, null);
                    }
                    _validationText = value;
                    OnPropertyChanged(nameof(ValidationText));
                }
            }
        }

        public bool IsPerformingActionFromCommand { get; set; } = false;

        public bool IsTriggerEnabled => RootTriggerActionViewModel.IsEnabled.IsTrue();

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

        #region Args
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
                    HasArgsChanged = true;
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
                    HasArgsChanged = true;
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
                    HasArgsChanged = true;
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
                    HasArgsChanged = true;
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
                    HasArgsChanged = true;
                    OnPropertyChanged(nameof(Arg5));
                }
            }
        }

        #endregion

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

            ActionArgs.Clear();
            if (ComponentFormat != null &&
                ComponentFormat.parameters != null) {
                var param_values = await MpAvPluginParameterValueLocator.LocateValuesAsync(
                    MpParameterHostType.Action, ActionId, this);
                foreach (var param_format in param_values) {
                    var param_vm =
                        await CreateActionParameterViewModel(param_format);
                    ActionArgs.Add(param_vm);
                }
                ActionArgs.CollectionChanged += ActionArgs_CollectionChanged;
                OnPropertyChanged(nameof(ActionArgs));
            }

            if (Parent.Items.All(x => x.ActionId != ActionId)) {
                // only add if new
                Parent.Items.Add(this);
            }

            var cal = await MpDataModelProvider.GetChildActionsAsync(ActionId);
            foreach (var ca in cal.OrderBy(x => x.SortOrderIdx)) {
                await CreateActionViewModel(ca);
            }

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Tooltip));

            OnPropertyChanged(nameof(ActionArgs));

            while (Children.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(IconResourceObj));


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
                case MpActionType.Repeater:
                    avm = new MpAvRepeaterActionViewModel(Parent);
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

        public async Task<MpAvParameterViewModelBase> CreateActionParameterViewModel(MpParameterValue pppv) {
            var naipvm = await MpAvPluginParameterBuilder.CreateParameterViewModelAsync(pppv, this);
            naipvm.OnValidate += ActionParam_OnValidate;

            return naipvm;
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
            //if (!IsEnabled.HasValue) {
            //    //if action has errors halt 
            //    return;
            //}
            //if (!IsEnabled.Value) {
            //    //if action is disabled pass parent output to child
            //    OnActionComplete?.Invoke(this, args);
            //    return;
            //}
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

            NotifyActionComplete(arg);
        }

        protected virtual void NotifyActionComplete(object outputArg) {
            OnActionComplete?.Invoke(this, outputArg);

        }

        public async Task UpdateSortOrderAsync() {
            Children.ForEach(x => x.SortOrderIdx = Children.IndexOf(x));
            await Task.WhenAll(Children.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        public override string ToString() {
            if (Action == null) {
                return null;
            }
            if (ParentActionViewModel == null) {
                return Label;
            }

            return $"{RootTriggerActionViewModel.Label} - {Label}";
        }

        #endregion

        #region Protected Methods

        protected void ShowValidationNotification(int focusArgNum = 0) {
            Dispatcher.UIThread.Post(async () => {
                if (_isShowingValidationMsg) {
                    return;
                }
                _isShowingValidationMsg = true;

                Func<object, object> retryFunc = (args) => {
                    Dispatcher.UIThread.Post(async () => {
                        await ValidateActionAsync();
                    });

                    return null;
                };

                var result = await MpNotificationBuilder.ShowNotificationAsync(
                    notificationType: MpNotificationType.InvalidAction,
                    body: ValidationText,
                    retryAction: retryFunc,
                    fixCommand: Parent.SelectActionCommand,
                    fixCommandArgs: new object[] { ActionId, focusArgNum, ValidationText });

                _isShowingValidationMsg = false;
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

        protected virtual async Task ValidateActionAsync() {
            // is always valid
            await Task.Delay(1);
        }


        protected virtual bool CanPerformAction(object arg) {
            if (!IsValid || !IsTriggerEnabled) {
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

        #endregion

        #region Private Methods


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
                        Parent.SelectActionCommand.Execute(this);
                    }
                    OnPropertyChanged(nameof(IsTrigger));

                    //if (this is MpAvIParameterCollectionViewModel ppcvm) {
                    //    ppcvm.OnPropertyChanged(nameof(ppcvm.Items));
                    //}
                    break;
                case nameof(HasArgsChanged):
                    if (HasArgsChanged) {
                        HasArgsChanged = false;
                        if (IsTriggerEnabled || !IsValid) {
                            // NOTE only when args change and trigger is active or invalid already, 
                            // invoke validation to avoid unnecessary warnings during create or while
                            // its disabled
                            ValidateActionAsync().FireAndForgetSafeAsync(this);
                        }
                    }
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        //HasModelChanged = false;

                        Task.Run(async () => {
                            IsBusy = true;

                            await Action.WriteToDatabaseAsync();
                            HasModelChanged = false;

                            IsBusy = false;
                        });
                    }
                    break;
                //case nameof(CanSaveOrCancel):
                //    if(CanSaveOrCancel) {
                //        SaveCommand.Execute(null);

                //    }
                //    break;
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    if (ParentActionViewModel != null) {
                        ParentActionViewModel.OnPropertyChanged(nameof(ParentActionViewModel.IsAnyBusy));
                    }
                    break;
                case nameof(IsValid):
                    RootTriggerActionViewModel.OnPropertyChanged(nameof(RootTriggerActionViewModel.IsAllValid));
                    if (!IsValid && IsSelected) {
                        Dispatcher.UIThread.Post(async () => {
                            // wait for content control to bind to primary action...
                            await Task.Delay(300);
                            var apv = MpAvMainView.Instance.GetVisualDescendant<MpAvActionPropertyHeaderView>();
                            if (apv != null) {
                                var icc = apv.FindControl<ContentControl>("ActionPropertyIconContentControl");

                                icc.ApplyTemplate();
                            }
                        });
                    }
                    break;
                case nameof(ActionArgs):
                    if (this is MpAvIParameterCollectionViewModel ppcvm) {
                        ppcvm.OnPropertyChanged(nameof(ppcvm.Items));
                    }
                    break;
                case nameof(IconResourceObj):
                    OnPropertyChanged(nameof(IconBackgroundHexColor));
                    break;
            }
        }

        private void ActionArgs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (this is MpAvIParameterCollectionViewModel ppcvm) {
                ppcvm.OnPropertyChanged(nameof(ppcvm.Items));
            }
        }
        private void ActionParam_OnValidate(object sender, EventArgs e) {
            Dispatcher.UIThread.Post(() => {
                ValidateActionAsync().FireAndForgetSafeAsync(this);
            });
        }

        #region DesignerItem Placement Methods

        private MpPoint FindOpenDesignerLocation(MpPoint anchorPoint, object ignoreItem = null) {
            int attempts = 0;
            int maxAttempts = 10;
            int count = 4;
            double dtheta = (2 * Math.PI) / count;
            double r = Parent.DesignerItemDiameter * 2;
            while (attempts <= maxAttempts) {
                double theta = 0;
                for (int i = 0; i < count; i++) {
                    var tp = new MpPoint();
                    tp.X = (double)(anchorPoint.X + r * Math.Cos(theta));
                    tp.Y = (double)(anchorPoint.Y + r * Math.Sin(theta));
                    if (!OverlapsItem(tp)) {
                        return tp;
                    }
                    theta += dtheta;
                }
                r += Parent.DesignerItemDiameter * 2;

                attempts++;
            }

            return new MpPoint(
                MpRandom.Rand.NextDouble() * Parent.ObservedDesignerBounds.Width,
                MpRandom.Rand.NextDouble() * Parent.ObservedDesignerBounds.Height);
        }

        private bool OverlapsItem(MpPoint targetTopLeft) {
            return GetItemNearPoint(targetTopLeft) != null;
        }

        private MpIBoxViewModel GetItemNearPoint(MpPoint targetTopLeft, object ignoreItem = null, double radius = 50) {
            MpPoint targetMid = new MpPoint(targetTopLeft.X, targetTopLeft.Y);
            foreach (var avm in SelfAndAllDescendants.Cast<MpIBoxViewModel>()) {
                MpPoint sourceMid = new MpPoint(avm.X, avm.Y);
                double dist = targetMid.Distance(sourceMid);
                if (dist < radius && avm != ignoreItem) {
                    return avm;
                }
            }
            return null;
        }

        //public void ClearAreaAtPoint(MpPoint p, object ignoreItem = null) {
        //    var overlapItem = GetItemNearPoint(p, ignoreItem);
        //    if (overlapItem != null) {
        //        MpPoint tempLoc = p;
        //        do {
        //            var overlapLoc = new MpPoint(overlapItem.X, overlapItem.Y);
        //            double distToMove = overlapLoc.Distance(tempLoc) + 10;

        //            var dir = overlapLoc - tempLoc;
        //            dir.Normalize();
        //            dir = new Vector(-dir.Y, dir.X);
        //            overlapLoc += dir * distToMove;
        //            overlapItem.X = overlapLoc.X;
        //            overlapItem.Y = overlapLoc.Y;

        //            overlapItem = GetItemNearPoint(overlapLoc, overlapItem);
        //            tempLoc = overlapLoc;
        //        } while (overlapItem != null && overlapItem != ignoreItem);
        //    }
        //}

        //public void ClearAllOverlaps() {
        //    foreach (var avm in AllSelectedTriggerActions) {
        //        ClearAreaAtPoint(avm.Location, avm);
        //    }
        //}
        //public void NotifyViewportChanged() {
        //    //CollectionViewSource.GetDefaultView(AllSelectedTriggerActions).Refresh();
        //    //CollectionViewSource.GetDefaultView(AllSelectedTriggerActions).Refresh();
        //    OnPropertyChanged(nameof(AllSelectedItemActions));
        //}
        #endregion

        #endregion

        #region Commands



        public ICommand ShowActionSelectorMenuCommand => new MpCommand<object>(
             (args) => {
                 var fe = args as Control;
                 Parent.FocusAction = this;
                 MpAvMenuExtension.ShowMenu(fe, PopupMenuViewModel);
             }, (args) => args is Control);

        public ICommand AddChildActionCommand => new MpCommand<object>(
             async (args) => {
                 IsBusy = true;

                 MpActionType at = (MpActionType)args;
                 MpAction na = await MpAction.CreateAsync(
                                         actionType: at,
                                         label: GetUniqueActionName(at.ToString()),
                                         parentId: ActionId,
                                         sortOrderIdx: Children.Count(),
                                         location: FindOpenDesignerLocation(Location));

                 var navm = await CreateActionViewModel(na);

                 while (navm.IsBusy) {
                     await Task.Delay(100);
                 }

                 navm.OnPropertyChanged(nameof(navm.X));
                 navm.OnPropertyChanged(nameof(navm.Y));
                 Parent.FocusAction = navm;

                 //if(RootTriggerActionViewModel.IsEnabled.IsTrue()) {
                 //    navm.ValidateActionAsync().FireAndForgetSafeAsync(navm);
                 //}

                 IsBusy = false;
             }, (args) => ActionType != MpActionType.None);

        public ICommand DeleteThisActionCommand => new MpCommand(
            () => {
                Parent.DeleteActionCommand.Execute(this);
            }, () => !IsAnyBusy);


        public ICommand FinishMoveCommand => new MpCommand(() => {
            HasModelChanged = true;
        });

        public ICommand InvokeThisActionCommand => new MpAsyncCommand(
            async () => {
                IsPerformingActionFromCommand = true;
                await Task.Delay(300);
                var ctrvm = MpAvClipTrayViewModel.Instance;
                while (ctrvm.IsAddingClipboardItem) {
                    // wait for any new item to be logged
                    await Task.Delay(100);
                }
                MpCopyItem ci = null;
                if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    // when mw open assume user will select item before issuing cmd,
                    // if none selected grab most recent pinned item as fallback
                    if (ctrvm.SelectedItem == null) {
                        var newest_ctvm = ctrvm.PinnedItems.OrderByDescending(x => x.CopyItemCreatedDateTime).FirstOrDefault();
                        if (newest_ctvm != null) {
                            ci = newest_ctvm.CopyItem;
                        }
                    } else {
                        ci = ctrvm.SelectedItem.CopyItem;
                    }

                } else {
                    // select most recently copied item
                    ci = ctrvm.PendingNewModels.OrderByDescending(x => x.CopyDateTime).FirstOrDefault();
                }
                if (ci == null) {
                    // no item could be selected so ignore shortcut trigger
                    IsPerformingActionFromCommand = false;
                    return;
                }

                var ao = GetInput(ci);
                await PerformActionAsync(ao);
                IsPerformingActionFromCommand = false;
            });





        #endregion
    }
}
