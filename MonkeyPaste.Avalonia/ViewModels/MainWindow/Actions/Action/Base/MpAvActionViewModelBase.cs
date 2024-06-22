
using AngleSharp.Dom;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpAnalyzerChildInputType {
        LastOutput,
        SourceClip
    }

    public enum MpActionDesignerShapeType {
        None = 0,
        Circle,
        Box,
        Diamond
    }

    public abstract class MpAvActionViewModelBase :
        MpAvViewModelBase<MpAvTriggerCollectionViewModel>,
        MpIActionPluginComponent,
        MpITreeItemViewModel,
        MpIDraggable,
        MpIHoverableViewModel,
        MpIPopupMenuViewModel,
        MpIContextMenuViewModel,
        MpIIconResourceViewModel,
        MpILabelTextViewModel,
        MpIBoxViewModel,
        MpIMovableViewModel,
        MpIInvokableAction,
        MpIParameterHostViewModel,
        MpAvIParameterCollectionViewModel,
        MpIPopupMenuPicker,
        MpAvIFocusHeaderMenuViewModel {

        #region Private Variables

        private bool _isShowingValidationMsg = false;
        private bool _isSettingChildRestorePoint = false;
        #endregion

        #region Constants

        public const string INPUT_TYPE_PARAM_ID = "InputType";

        #endregion

        #region Statics

        public static string GetDefaultActionIconResourceKey(object actionOrTriggerVmOrTriggerType) {
            if (actionOrTriggerVmOrTriggerType is MpAvActionViewModelBase avmb) {
                if (avmb is MpAvTriggerActionViewModelBase tvmb) {
                    actionOrTriggerVmOrTriggerType = tvmb.TriggerType;
                } else {
                    actionOrTriggerVmOrTriggerType = avmb.ActionType;
                }
            }
            if (actionOrTriggerVmOrTriggerType is MpTriggerType tt) {
                switch (tt) {
                    case MpTriggerType.ClipAdded:
                        return "ClipboardImage";
                    case MpTriggerType.ClipTagged:
                        return "BooksImage";
                    case MpTriggerType.FileSystemChanged:
                        return "FolderImage";
                    case MpTriggerType.Shortcut:
                        return "JoystickImage";
                    case MpTriggerType.MonkeyCopyShortcut:
                        return "MonkeyWinkImage";
                }
            }
            if (actionOrTriggerVmOrTriggerType is MpActionType at) {
                switch (at) {
                    case MpActionType.Analyze:
                        return "BrainImage";
                    case MpActionType.Classify:
                        return "TagColorImage";
                    case MpActionType.Conditional:
                        return "ScalesImage";
                    case MpActionType.Repeater:
                        return "ResetImage";
                    case MpActionType.FileWriter:
                        return "FolderImage";
                    case MpActionType.SetClipboard:
                        return "KeyboardImage";
                    case MpActionType.Delay:
                        return "AlarmClockImage";
                    case MpActionType.Alert:
                        return "BellImage";
                    case MpActionType.ApplicationCommand:
                        return "CommandImage";
                }
            }
            // whats params?
            MpDebug.Break();
            return "QuestiongMarkImage";
        }

        public static string GetActionHexColor(MpActionType actionType, MpTriggerType tt = MpTriggerType.None) {
            double bright_factor = 0.3;
            switch (actionType) {
                case MpActionType.None:
                    return MpSystemColors.Transparent;
                case MpActionType.Trigger:
                    switch (tt) {
                        case MpTriggerType.Shortcut:
                            return MpSystemColors.cyan2;
                        case MpTriggerType.ClipAdded:
                            return MpSystemColors.purple2;
                        case MpTriggerType.ClipTagged:
                            return MpSystemColors.orange2;
                        case MpTriggerType.FileSystemChanged:
                            return MpSystemColors.coral3;
                        case MpTriggerType.MonkeyCopyShortcut:
                            return MpSystemColors.rosybrown4;
                        default:
                            return MpSystemColors.maroon;
                    }
                case MpActionType.Analyze:
                    return MpSystemColors.magenta;
                case MpActionType.Classify:
                    return MpSystemColors.tomato1;
                case MpActionType.SetClipboard:
                    return MpColorHelpers.GetLighterHexColor(MpSystemColors.olivedrab, bright_factor);
                case MpActionType.Conditional:
                    return MpSystemColors.darkturquoise;
                case MpActionType.Repeater:
                    return MpColorHelpers.GetLighterHexColor(MpSystemColors.steelblue, bright_factor);
                case MpActionType.FileWriter:
                    return MpSystemColors.forestgreen;
                case MpActionType.Delay:
                    return MpSystemColors.orange2;
                case MpActionType.Alert:
                    return MpSystemColors.royalblue2;
                case MpActionType.ApplicationCommand:
                    return MpColorHelpers.GetLighterHexColor(MpSystemColors.deeppink4, bright_factor);
                default:
                    throw new Exception($"Unknow action type: '{actionType}'");
            }
        }

        #endregion

        #region Interfaces

        #region MpAvIFocusHeaderMenuViewModel Implementation
        MpAvHeaderBackButtonType MpAvIHeaderMenuViewModel.BackButtonType =>
            MpAvHeaderBackButtonType.Close;
        public bool IsFocused { get; set; }
        IBrush MpAvIHeaderMenuViewModel.HeaderBackground =>
           ActionBackgroundHexColor.ToAvBrush(force_alpha: 1);
        IBrush MpAvIHeaderMenuViewModel.HeaderForeground =>
            (this as MpAvIHeaderMenuViewModel).HeaderBackground.ToHex().ToContrastForegoundColor().ToAvBrush();

        string MpAvIHeaderMenuViewModel.HeaderTitle =>
            Label;
        public IEnumerable<MpAvIMenuItemViewModel> HeaderMenuItems =>
            new MpAvMenuItemViewModel[] {
                new MpAvMenuItemViewModel() {
                    IconSourceObj = "PlusImage",
                    Command = ShowAddChildMenuCommand
                },
                new MpAvMenuItemViewModel() {
                    IconSourceObj = "Dots3x1Image",
                    Command = ShowContextMenuCommand
                }
            };
        ICommand MpAvIHeaderMenuViewModel.BackCommand => new MpCommand(
            () => {
                if(MpAvWindowManager.ActiveWindow is not { } aw ||
                aw.GetVisualDescendant<MpAvTriggerActionChooserView>() is not { } tac) {                    
                    return;
                }
                if(Parent.IsWindowOpen) {
                    MpAvOverlayContainerView.Instance.TopWindow.HeaderViewModel = Parent;
                } else {
                    tac.FocusThisHeader();
                }
                
            });
        object MpAvIHeaderMenuViewModel.BackCommandParameter =>
            null;

        #endregion

        #region MpIDraggableViewModel Implementation

        bool MpIDraggable.IsDragging {
            get => IsMoving;
            set => throw new NotImplementedException();
        }

        #endregion

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

        bool MpIActionPluginComponent.CanPerformAction(object arg) => ValidateStartAction(arg, false);

        Task MpIActionPluginComponent.ValidateActionAsync() => ValidateActionAndDescendantsAsync();

        string MpIActionPluginComponent.ValidationText => ValidationText;
        #endregion

        #region MpIPopupMenuViewModel Implementation

        public MpAvMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedActionIds, bool recursive) {
            return new MpAvMenuItemViewModel() {
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

        public IEnumerable<MpAvParameterViewModelBase> Items =>
            ActionArgs.ToList();
        public MpAvParameterViewModelBase SelectedItem { get; set; }

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
        bool MpISaveOrCancelableViewModel.IsSaveCancelEnabled =>
            false;

        public bool CanSaveOrCancel {
            get {
                var toSave_pvml = ActionArgs.Where(x => x.HasModelChanged);
                bool has_changed = toSave_pvml.Any();
                toSave_pvml.ForEach(x => x.SaveCurrentValueCommand.Execute(null));
                if (has_changed) {
                    HasArgsChanged = true;
                }
                //bool result = ActionArgs.Any(x => x.HasModelChanged);
                //if (toSave_pvml.Any()) {
                //    OnPropertyChanged(nameof(ActionArgs));
                //}
                return false;
            }
        }

        #endregion

        #endregion

        #region MpIParameterHost Implementation

        int MpIParameterHostViewModel.IconId => 0;
        public string PluginGuid =>
            PluginFormat == null ? string.Empty : PluginFormat.guid;

        public MpRuntimePlugin PluginFormat { get; set; }

        public MpPresetParamaterHostBase ComponentFormat => ActionComponentFormat;
        MpPresetParamaterHostBase MpIParameterHostViewModel.BackupComponentFormat =>
            PluginFormat == null ||
            PluginFormat.backupCheckPluginFormat == null ||
            PluginFormat.backupCheckPluginFormat.headless == null ?
                null : PluginFormat.backupCheckPluginFormat.headless;

        public virtual MpHeadlessComponent ActionComponentFormat { get; protected set; }

        //public MpIPluginComponentBase PluginComponent =>
        //    PluginFormat == null || PluginFormat.Components == null ?
        //        null :
        //        PluginFormat.Components.FirstOrDefault() as MpIPluginComponentBase;

        public Dictionary<string, MpAvParameterViewModelBase> ArgLookup =>
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

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected {
            get => Parent == null || Parent.FocusAction == null ? false : Parent.FocusAction.ActionId == ActionId;
            //set {

            //    if(IsSelected != paramValue) {
            //        Parent.FocusAction = this;
            //        OnPropertyChanged(nameof(IsSelected));
            //    }
            //}
        }

        #endregion

        #region MpIContextMenuViewModel Implementation

        public bool IsContextMenuOpen { get; set; }
        public MpAvMenuItemViewModel ContextMenuViewModel => PopupMenuViewModel;

        #endregion

        #region MpIPopupMenuViewModel Implementation

        protected virtual MpAvMenuItemViewModel GetAddContextMenuItem(bool isFromPopup) {
            if(isFromPopup && MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                // add menu shown in separate header action on mobile
                return new MpAvMenuItemViewModel() { IsVisible = false };
            }
            return new MpAvMenuItemViewModel() {
                Header = UiStrings.CommonAddLabel,
                HasLeadingSeparator = true,
                IconResourceKey = "AddGreenImage",
                SubItems =
                        typeof(MpActionType)
                        .EnumerateEnum<MpActionType>()
                        .Where(x => x != MpActionType.None && x != MpActionType.Trigger)
                        .OrderBy(x => x.EnumToUiString())
                        .Select(x =>
                            new MpAvMenuItemViewModel() {
                                Header = x.EnumToUiString(),
                                //IconResourceKey = GetDefaultActionIconResourceKey(x),
                                //IconTintHexStr = GetActionHexColor(x),
                                IconTintHexStr = GetActionHexColor(x),
                                IconSourceObj = GetDefaultActionIconResourceKey(x),
                                Command = AddChildActionCommand,
                                CommandParameter = x
                            }).ToList()
            };
        }

        public virtual MpAvMenuItemViewModel PopupMenuViewModel {
            get {
                IEnumerable<MpAvMenuItemViewModel> move_items =
                    RootTriggerActionViewModel == this ? new MpAvMenuItemViewModel[] { } :
                    RootTriggerActionViewModel.SelfAndAllDescendants
                                .Where(x =>
                                        !SelfAndAllDescendants.Contains(x) &&
                                        x != ParentActionViewModel)
                                .Select(x =>
                                    new MpAvMenuItemViewModel() {
                                        Header = x.Label,
                                        IconResourceKey = GetDefaultActionIconResourceKey(x),
                                        IconTintHexStr = GetActionHexColor(x.ActionType, x.ActionType == MpActionType.Trigger ? (x as MpAvTriggerActionViewModelBase).TriggerType : MpTriggerType.None),
                                        Command = ChangeParentCommand,
                                        CommandParameter = x.ActionId
                                    });

                return new MpAvMenuItemViewModel() {
                    ParentObj = this,
                    SubItems = new List<MpAvMenuItemViewModel>() {
                         new MpAvMenuItemViewModel() {
                             IsVisible = !IsTrigger,
                            Header = UiStrings.CommonCutOpLabel,
                            IconResourceKey = "ScissorsImage",
                            ShortcutArgs = new object[] { MpShortcutType.CutSelection },
                            Command = CutActionCommand
                        },
                         new MpAvMenuItemViewModel() {
                             IsVisible = !IsTrigger,
                            Header = UiStrings.CommonCopyOpLabel,
                            IconResourceKey = "CopyImage",
                            ShortcutArgs = new object[] { MpShortcutType.CopySelection },
                            Command = CopyActionCommand
                        },
                         new MpAvMenuItemViewModel() {
                             IsVisible = CanPaste,
                            Header = UiStrings.CommonPasteOpLabel,
                            IconResourceKey = "PasteImage",
                            ShortcutArgs = new object[] { MpShortcutType.PasteSelection },
                            Command = PasteActionCommand,
                        },
                        new MpAvMenuItemViewModel() {
                            HasLeadingSeparator = true,
                            Header = UiStrings.ActionMoveLabel,
                            IconResourceKey = "ChainImage",
                            IsVisible = move_items.Any() && !IsTrigger,
                            SubItems = move_items.ToList()
                        },
                        GetAddContextMenuItem(true),
                        new MpAvMenuItemViewModel() {
                            HasLeadingSeparator = true,
                            IsVisible = !IsTrigger && CanDelete,
                            Header = UiStrings.CommonDeleteLabel,
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

        #region Layout

        private int _designerZIndex = 0;
        public int DesignerZIndex {
            get {
                if (IsSelected) {
                    return int.MaxValue;
                }
                return _designerZIndex;
            }
            set {
                if (_designerZIndex != value) {
                    _designerZIndex = value;
                    OnPropertyChanged(nameof(DesignerZIndex));
                }
            }
        }

        #endregion

        #region Appearance
        public virtual MpActionDesignerShapeType DesignerShapeType =>
            MpActionDesignerShapeType.Box;
        public virtual string ActionBackgroundHexColor =>
            GetActionHexColor(ActionType);

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
        public abstract string ActionHintText { get; }

        #endregion

        #region State
        public bool IsParentActionSupportLastOutput =>
            ParentActionViewModel is MpAvAnalyzeActionViewModel;
        bool CanPaste { get; set; }
        public bool IsValidating { get; set; }
        public virtual bool AllowNullArg =>
            false;

        public bool IsDragOver { get; set; }
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

        public bool IsValid =>
            string.IsNullOrEmpty(ValidationText);

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

        public bool IsPerformingAction { get; set; } = false;

        public bool IsSelfOrAnyDescendantPerformingAction =>
            SelfAndAllDescendants.Any(x => x.IsPerformingAction);

        public bool IsTriggerEnabled =>
            RootTriggerActionViewModel.IsEnabled;

        public bool IsDefaultAction =>
            Action != null &&
            (Action.Guid == MpAvTriggerCollectionViewModel.DEFAULT_ANNOTATOR_TRIGGER_GUID ||
             Action.Guid == MpAvTriggerCollectionViewModel.DEFAULT_ANNOTATOR_ANALYZE_GUID);

        public bool CanDelete =>
            Parent != null && Action != null && !Parent.DefaultActionGuids.Contains(Action.Guid);

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
                    Action.X = Math.Clamp(value,-MpAvTriggerCollectionViewModel.MAX_TRANSLATE_OFFSET_MAGNITUDE, MpAvTriggerCollectionViewModel.MAX_TRANSLATE_OFFSET_MAGNITUDE);
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
                    Action.Y = Math.Clamp(value, -MpAvTriggerCollectionViewModel.MAX_TRANSLATE_OFFSET_MAGNITUDE, MpAvTriggerCollectionViewModel.MAX_TRANSLATE_OFFSET_MAGNITUDE);
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

        #region Undoables

        private List<MpAction> _undoableChildren;
        public List<MpAction> UndoableChildren {
            get {
                //if (_undoableChildren == null) {
                //    _undoableChildren = Children.Select(x => x.Action).ToList();
                //}
                return _undoableChildren;
            }
            set {
                if (UndoableChildren != value) {
                    AddUndo(UndoableChildren, value, $"{MpPortableDataFormats.INTERNAL_ACTION_ITEM_FORMAT} '{FullName}' child changed");
                    _undoableChildren = value;
                    OnPropertyChanged(nameof(UndoableChildren));
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
                if (Action.Guid == MpAvTriggerCollectionViewModel.DEFAULT_ANNOTATOR_TRIGGER_GUID) {
                    return UiStrings.TriggerAnnTriggerLabel;
                }
                if (Action.Guid == MpAvTriggerCollectionViewModel.DEFAULT_ANNOTATOR_ANALYZE_GUID) {
                    return UiStrings.TriggerAnnAnalyzeLabel;
                }

                if (string.IsNullOrEmpty(Action.Label)) {
                    return ActionType.EnumToUiString();
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
            ActionArgs.CollectionChanged += ActionArgs_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpAction a) {
            bool was_busy = IsBusy;
            IsBusy = was_busy;

            Action = a;

            if (Parent.Items.All(x => x.ActionId != ActionId)) {
                // only add if new
                Parent.Items.Add(this);
            }

            ActionArgs.Clear();

            //AddAncestorParams();

            if (ComponentFormat != null &&
                ComponentFormat.parameters != null) {
                // locate param values
                var param_values = await MpAvPluginParameterValueLocator.LocateValuesAsync(
                    MpParameterHostType.Action, ActionId, this);
                foreach (var param_format in param_values) {
                    var param_vm = await CreateActionParameterViewModel(param_format);
                    param_vm.PropertyChanged += Param_vm_PropertyChanged;
                    ActionArgs.Add(param_vm);
                }
                OnPropertyChanged(nameof(ActionArgs));
            }

            // attach validation handlers
            ActionArgs.ForEach(x => x.OnParamValidate += ActionArg_OnValidate);

            var cal = await MpDataModelProvider.GetChildActionsAsync(ActionId);
            foreach (var ca in cal.OrderBy(x => x.SortOrderIdx)) {
                await CreateActionViewModel(ca);
            }

            OnPropertyChanged(nameof(Children));

            OnPropertyChanged(nameof(ActionArgs));

            while (Children.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(IconResourceObj));


            IsBusy = was_busy;
        }

        protected virtual void Param_vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        }

        private void ActionArg_OnValidate(object sender, EventArgs e) {
            // trigger on enable
            var aipvm = sender as MpAvParameterViewModelBase;
            ValidationText = aipvm.GetValidationMessage(true);
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
                case MpActionType.Conditional:
                    avm = new MpAvConditionalActionViewModel(Parent);
                    break;
                case MpActionType.Repeater:
                    avm = new MpAvRepeaterActionViewModel(Parent);
                    break;
                case MpActionType.SetClipboard:
                    avm = new MpAvSetClipboardActionViewModel(Parent);
                    break;
                case MpActionType.FileWriter:
                    avm = new MpAvFileWriterActionViewModel(Parent);
                    break;
                case MpActionType.Delay:
                    avm = new MpAvDelayActionViewModel(Parent);
                    break;
                case MpActionType.Alert:
                    avm = new MpAvAlertActionViewModel(Parent);
                    break;
                case MpActionType.ApplicationCommand:
                    avm = new MpAvAppCommandActionViewModel(Parent);
                    break;
                default:
                    MpDebug.Break($"Unhandled action type '{a.ActionType}'");
                    return null;
            }
            //avm.ParentTreeItem = this;
            OnActionComplete += avm.OnActionInvoked;

            await avm.InitializeAsync(a);

            return avm;
        }

        public async Task<MpAvParameterViewModelBase> CreateActionParameterViewModel(MpParameterValue pppv) {
            var naipvm = await MpAvPluginParameterBuilder.CreateParameterViewModelAsync(pppv, this);
            //naipvm.OnValidate += ActionParam_OnValidate;

            return naipvm;
        }
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
            Dispatcher.UIThread.Post(async () => {
                try {
                    await PerformActionAsync(args);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error performing action '{this}'.", ex);
                }
            });
        }

        protected abstract Task PerformActionAsync(object arg);
        protected async Task FinishActionAsync(object arg) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(() => FinishActionAsync(arg));
                return;
            }
            // trigger children so current item added BEFORE removing from here
            // to avoid a gap for blocking threads (tag drop progress spinner)

            OnActionComplete?.Invoke(this, arg);
            // wait briefly for children to start before flagging as done
            await Task.Delay(50);

            IsPerformingAction = false;

            MpConsole.WriteLine($"Action({ActionId}) '{Label}' Completed ", true);
            if (arg is MpAvActionOutput ao) {
                MpConsole.WriteLine(ao.ActionDescription, false, true);
            }
        }


        public async Task UpdateSortOrderAsync() {
            Children.ForEach(x => x.SortOrderIdx = Children.IndexOf(x));
            await Task.WhenAll(Children.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        public async Task SetChildRestoreStateAsync() {
            _isSettingChildRestorePoint = true;
            var action_clone = await Action.CloneDbModelAsync(true, true);
            UndoableChildren = action_clone.Children.ToList();
            //if (UndoableChildren == null) {
            //    MpConsole.WriteLine($"Child restore state for '{FullName}' SET to NULL", true, true);
            //} else {
            //    MpConsole.WriteLine($"Child restore state for '{FullName}' SET to:", true);
            //    UndoableChildren.ForEach(x => MpConsole.WriteLine(x.SerializeJsonObject().ToPrettyPrintJson()));
            //    MpConsole.WriteLine("");
            //}

            _isSettingChildRestorePoint = false;
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
            // NOTE omitting because it doesn't hide or retry right and trying to keeping 
            // validation silent
            //Dispatcher.UIThread.Post(async () => {
            //    if (_isShowingValidationMsg) {
            //        return;
            //    }
            //    _isShowingValidationMsg = true;

            //    Func<object, object> retryFunc = (args) => {
            //        Dispatcher.UIThread.Post(async () => {
            //            await ValidateActionAsync();
            //        });

            //        return null;
            //    };

            //    var result = await MpNotificationBuilder.ShowNotificationAsync(
            //        notificationType: MpNotificationType.InvalidAction,
            //        body: ValidationText,
            //        retryAction: retryFunc,
            //        fixCommand: Parent.SelectActionCommand,
            //        fixCommandArgs: new object[] { ActionId, focusArgNum, ValidationText });

            //    _isShowingValidationMsg = false;
            //});
        }


        protected virtual MpAvActionOutput GetInput(object arg) {
            if (arg == null && AllowNullArg) {
                if (this is MpAvTriggerActionViewModelBase) {
                    return new MpAvTriggerInput();
                }
                return null;
            }
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

        protected virtual async Task ValidateActionAndDescendantsAsync() {
            if (IsValidating || _isShowingValidationMsg) {
                return;
            }
            IsValidating = true;
            // clear validation
            ValidationText = string.Empty;
            foreach (var param in ActionArgs) {
                bool result = param.Validate();

                if (!string.IsNullOrEmpty(ValidationText)) {
                    // wait for OnValidate to complete..
                    //await Task.Delay(20);
                    // when any parameter is invalid halt any further validation
                    // and by convention inheritor will halt as well
                    //ValidationText = param.ValidationMessage;
                    ShowValidationNotification();
                    IsValidating = false;
                    return;
                }
            }
            //validate children
            await Task.WhenAll(Children.Select(x => x.ValidateActionAndDescendantsAsync()));
            // let inheritors perform validation...
            while (Children.Any(x => x.IsValidating)) {
                await Task.Delay(100);
            }
            IsValidating = false;
        }


        protected virtual bool ValidateStartAction(object arg, bool is_starting = true) {
            bool can_start = true;
            if (arg == null && !AllowNullArg) {
                can_start = false;
            } else if (!IsValid || !IsTriggerEnabled) {
                can_start = false;
            }

            if (is_starting) {
                if (arg is MpAvActionOutput co && !co.CanExecutionContinue) {
                    MpConsole.WriteLine($"Action for '{ParentActionViewModel}' says children cannot execute. '{this}' will not execute");
                    can_start = false;
                }
                IsPerformingAction = can_start;
            }
            return can_start;
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

        private void MpAvActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(Label):
                    ActionArgs.ForEach(x => x.OnPropertyChanged(nameof(x.FullLabel)));
                    if(this is MpAvIHeaderMenuViewModel hmvm) {
                        hmvm.OnPropertyChanged(nameof(hmvm.HeaderTitle));
                    }
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        Parent.SelectActionCommand.Execute(this);
                        LastSelectedDateTime = DateTime.Now;
                        UpdateCanPasteAsync().FireAndForgetSafeAsync();
                    }
                    OnPropertyChanged(nameof(DesignerZIndex));
                    OnPropertyChanged(nameof(IsTrigger));

                    //if (this is MpAvIParameterCollectionViewModel ppcvm) {
                    //    ppcvm.OnPropertyChanged(nameof(ppcvm.Items));
                    //}
                    OnPropertyChanged(nameof(Items));
                    break;
                case nameof(HasArgsChanged):
                    if (HasArgsChanged) {
                        HasArgsChanged = false;

                        if (!IsValid || IsTriggerEnabled) {
                            // NOTE only when args change and trigger is active or invalid already, 
                            // invoke validation to avoid unnecessary warnings during create or while
                            // its disabled
                            ValidateActionAndDescendantsAsync().FireAndForgetSafeAsync(this);
                        }
                    }
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        Dispatcher.UIThread.Post(async () => {
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
                    if (!IsValid && IsSelected && MpAvMainView.Instance is MpAvMainView mv) {
                        Dispatcher.UIThread.Post(async () => {
                            // wait for content control to bind to primary action...
                            await Task.Delay(300);
                            var apv = mv.GetVisualDescendant<MpAvActionPropertyHeaderView>();
                            if (apv != null) {
                                var icc = apv.FindControl<ContentControl>("ActionPropertyIconContentControl");

                                icc.ApplyTemplate();
                            }
                        });
                    }
                    break;
                case nameof(ActionArgs):
                    //if (this is MpAvIParameterCollectionViewModel ppcvm) {
                    //    ppcvm.OnPropertyChanged(nameof(ppcvm.Items));
                    //}
                    OnPropertyChanged(nameof(Items));
                    break;
                case nameof(IconResourceObj):
                    OnPropertyChanged(nameof(IconBackgroundHexColor));
                    break;
                case nameof(UndoableChildren):
                    RestoreChildrenAsync().FireAndForgetSafeAsync();
                    break;
            }
        }

        private void ActionArgs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //if (this is MpAvIParameterCollectionViewModel ppcvm) {
            //    ppcvm.OnPropertyChanged(nameof(ppcvm.Items));
            //}
            OnPropertyChanged(nameof(Items));
        }

        private void ActionParam_OnValidate(object sender, EventArgs e) {
            Dispatcher.UIThread.Post(() => {
                ValidateActionAndDescendantsAsync().FireAndForgetSafeAsync(this);
            });
        }

        #region DesignerItem Placement Methods

        private MpPoint FindOpenDesignerLocation(MpPoint anchorPoint) {
            int attempts = 0;
            int maxAttempts = 10;
            int count = 4;
            double dtheta = (2 * Math.PI) / count;
            double delta_r = Parent.DesignerItemDiameter * 3;
            double r = delta_r;
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
                r += delta_r;

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

        private void AddAncestorParams() {
            if (ParentActionViewModel is MpAvAnalyzeActionViewModel aavm) {
                // NOTE for any child of analyzer (and maybe conditional?)
                // insert inputType parameter to args w/ options of 'Source' or 'LastOutput'
                // then if lastoutput is input type
                // update GetInput to set CopyItem to last output (may need to check at end of analyze perform)
                if (ActionComponentFormat == null) {
                    ActionComponentFormat = new MpHeadlessComponent() {
                        parameters = new List<MpParameterFormat>()
                    };
                }
                ActionComponentFormat.parameters.Insert(
                    0,
                    new MpParameterFormat() {
                        label = UiStrings.ActionAnalyzerChildInputParamLabel,
                        controlType = MpParameterControlType.ComboBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        description = UiStrings.ActionAnalyzerChildInputParamHint,
                        isRequired = true,
                        paramId = INPUT_TYPE_PARAM_ID,
                        values = new List<MpParameterValueFormat>() {
                            new MpParameterValueFormat() {
                                isDefault = true,
                                value = MpAnalyzerChildInputType.LastOutput.ToString(),
                                label = MpAnalyzerChildInputType.LastOutput.EnumToUiString()
                            },
                            new MpParameterValueFormat() {
                                value = MpAnalyzerChildInputType.SourceClip.ToString(),
                                label = MpAnalyzerChildInputType.SourceClip.EnumToUiString()
                            },
                        }.ToList()
                    }); ;
            }
        }
        private async Task UpdateCanPasteAsync() {
            var dfl = await Mp.Services.DeviceClipboard.GetFormatsAsync();
            CanPaste = dfl.Contains(MpPortableDataFormats.INTERNAL_ACTION_ITEM_FORMAT);
        }
        private async Task<MpCopyItem> GetPrimaryCopyItemToProcessAsync() {
            MpCopyItem ci = null;
            var ctrvm = MpAvClipTrayViewModel.Instance;
            while (ctrvm.IsAddingClipboardItem) {
                // wait for any new item to be logged
                await Task.Delay(100);
            }
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                // when mw open assume user will select item before issuing cmd,
                // if none selected grab most recent pinned item as fallback
                if (ctrvm.SelectedItem == null) {
                    var newest_ctvm = ctrvm.PinnedItems.OrderByDescending(x => x.CopyDateTime).FirstOrDefault();
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
            return ci;
        }


        public string GetUniqueActionName(string prefix) {
            int uniqueIdx = 1;
            string testName = string.Format(
                                        @"{0}{1}",
                                        prefix,
                                        uniqueIdx);

            while (RootTriggerActionViewModel.SelfAndAllDescendants.Any(x => x.Label == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        prefix,
                                        uniqueIdx);
            }

            return prefix + uniqueIdx;
        }

        private async Task RestoreChildrenAsync() {
            if (_isSettingChildRestorePoint) {
                return;
            }
            if (UndoableChildren == null) {
                // initial case, ignore
                MpConsole.WriteLine($"Child restore state for '{FullName}' RESTORING to NULL", true, true);
                return;
            }

            MpConsole.WriteLine($"Child restore state for '{FullName}' RESTORING to:", true);
            UndoableChildren.ForEach(x => MpConsole.WriteLine(x.SerializeJsonObject().ToPrettyPrintJson()));
            MpConsole.WriteLine("");

            // delete all descendants
            await Task.WhenAll(AllDescendants.Select(x => x.Action.DeleteFromDatabaseAsync()));

            // undoable children and params need key assignments
            await Task.WhenAll(UndoableChildren.Select((x, idx) => AssignCloneAsChildAsync(x, ActionId, idx)));

            await InitializeAsync(Action);

        }

        private async Task AssignCloneAsChildAsync(MpAction clone_action, int parent_id, int sort_idx) {
            clone_action.ParentActionId = parent_id;
            clone_action.SortOrderIdx = sort_idx;

            // get clone's parent
            if (RootTriggerActionViewModel.SelfAndAllDescendants.FirstOrDefault(x => x.ActionId == parent_id) is { } avm) {
                // position clone somewhere near parent
                var clone_loc = FindOpenDesignerLocation(avm.Location);
                clone_action.X = clone_loc.X;
                clone_action.Y = clone_loc.Y;
            }
            await clone_action.WriteToDatabaseAsync();

            clone_action.ParameterValues.ForEach(x => x.ParameterHostId = clone_action.Id);
            await Task.WhenAll(clone_action.ParameterValues.Select(x => x.WriteToDatabaseAsync()));

            await Task.WhenAll(clone_action.Children.Select((x, idx) => AssignCloneAsChildAsync(x, clone_action.Id, idx)));
        }
        #endregion

        #region Commands

        public ICommand AddChildActionCommand => new MpCommand<object>(
             async (args) => {
                 IsBusy = true;
                 await SetChildRestoreStateAsync();

                 MpActionType at = (MpActionType)args;
                 MpAction na = await MpAction.CreateAsync(
                                         actionType: at,
                                         label: GetUniqueActionName(at.EnumToUiString()),
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

                 await SetChildRestoreStateAsync();
                 IsBusy = false;
             }, (args) => ActionType != MpActionType.None);

        public ICommand DeleteThisActionCommand => new MpCommand(
            () => {
                Parent.DeleteActionCommand.Execute(this);
            },
            () => {
                return !IsAnyBusy && CanDelete;
            });


        public ICommand FinishMoveCommand => new MpCommand(() => {
            HasModelChanged = true;
        });

        public virtual ICommand InvokeThisActionCommand => new MpAsyncCommand<object>(
            async (args) => {
                //await Task.Delay(300);
                MpCopyItem ci = null;
                if (args is MpCopyItem arg_ci) {
                    ci = arg_ci;
                } else if (!AllowNullArg) {
                    ci = await GetPrimaryCopyItemToProcessAsync();
                    if (ci == null) {
                        // no item could be selected so ignore shortcut trigger
                        return;
                    }
                }
                var ao = GetInput(ci);
                await PerformActionAsync(ao);
            });


        public ICommand ShowContextMenuCommand => new MpCommand<object>(
            (args) => {
                if (Parent.FocusAction != this) {
                    Parent.FocusAction = this;
                }
                MpAvMenuView.ShowMenu(
                    target: args as Control,
                    dc: ContextMenuViewModel);
            });

        public ICommand ShowAddChildMenuCommand => new MpCommand<object>(
            (args) => {
                if (Parent.FocusAction != this) {
                    Parent.FocusAction = this;
                }
                MpAvMenuView.ShowMenu(
                    target: args as Control,
                    dc: GetAddContextMenuItem(false));
            });

        public ICommand ChangeParentCommand => new MpAsyncCommand<object>(
            async (args) => {
                int new_parent_id = 0;
                if (args is int) {
                    new_parent_id = (int)args;
                } else if (args is MpAvActionViewModelBase npavm) {
                    new_parent_id = npavm.ActionId;
                }
                if (new_parent_id == 0 ||
                    new_parent_id == ActionId) {
                    return;
                }

                // store trigger child state for move
                await RootTriggerActionViewModel.SetChildRestoreStateAsync();

                if (ParentActionViewModel != null) {
                    ParentActionViewModel.UnregisterTrigger(this);
                }

                ParentActionId = new_parent_id;

                if (ParentActionViewModel != null) {
                    ParentActionViewModel.RegisterTrigger(this);
                }
                await Task.Delay(50);
                while (IsBusy) {
                    await Task.Delay(100);
                }

                await RootTriggerActionViewModel.InitializeAsync(RootTriggerActionViewModel.Action);

                await ValidateActionAndDescendantsAsync();
            }, (args) => {
                if (args == null ||
                    this is MpAvTriggerActionViewModelBase) {
                    return false;
                }
                return true;
            });

        public ICommand CopyActionCommand => new MpAsyncCommand(
            async () => {
                var unwritten_action_clone = await Action.CloneDbModelAsync(true, true);
                var avdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_ACTION_ITEM_FORMAT, unwritten_action_clone.SerializeJsonObject());
                await Mp.Services.DataObjectTools.WriteToClipboardAsync(avdo, true);
                //wait MpAvCommonTools.Services.DeviceClipboard.SetDataObjectAsync(avdo);
                MpConsole.WriteLine("Copied action avdo: ");
                MpConsole.WriteLine(avdo.ToString());
            },
            () => {
                return !IsTrigger && !MpAvFocusManager.Instance.IsTextInputControlFocused;
            });

        public ICommand CutActionCommand => new MpAsyncCommand(
            async () => {
                // clone action to use children prop for undo which contains all children and params before cut
                var unwritten_action_clone = await Action.CloneDbModelAsync(true, true);

                await Parent.DeleteActionCommand.ExecuteAsync(new object[] { this, true });
                var avdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_ACTION_ITEM_FORMAT, unwritten_action_clone.SerializeJsonObject());
                await Mp.Services.DataObjectTools.WriteToClipboardAsync(avdo, true);
                MpConsole.WriteLine("Cut action avdo: ");
                MpConsole.WriteLine(avdo.ToString());

                await RootTriggerActionViewModel.InitializeAsync(RootTriggerActionViewModel.Action);
            },
            () => {
                return !IsTrigger && !MpAvFocusManager.Instance.IsTextInputControlFocused;
            });

        public ICommand PasteActionCommand => new MpAsyncCommand(
            async () => {
                //var avdo = await Mp.Services.DataObjectHelperAsync.GetPlatformClipboardDataObjectAsync(true) as IDataObject;
                //if (avdo == null) {
                //    return;
                //}
                object action_data = await Mp.Services.DeviceClipboard.GetDataAsync(MpPortableDataFormats.INTERNAL_ACTION_ITEM_FORMAT);
                if (action_data is not byte[] json_bytes ||
                    json_bytes.ToDecodedString() is not string action_json ||
                    MpJsonExtensions.DeserializeObject<MpAction>(action_json) is not MpAction child_to_assign) {
                    return;
                }
                MpConsole.WriteLine("Pasting action avdo: ");
                MpConsole.WriteLine(action_json);
                await SetChildRestoreStateAsync();
                await AssignCloneAsChildAsync(child_to_assign, ActionId, Children.Count());

                await RootTriggerActionViewModel.InitializeAsync(RootTriggerActionViewModel.Action);
            },
            () => {
                return CanPaste && !MpAvFocusManager.Instance.IsTextInputControlFocused;
            });

        #endregion
    }
}
