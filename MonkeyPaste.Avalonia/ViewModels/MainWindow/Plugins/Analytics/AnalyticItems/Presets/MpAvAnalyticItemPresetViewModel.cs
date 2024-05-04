using MonkeyPaste.Common;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyticItemPresetViewModel :
        MpAvTreeSelectorViewModelBase<MpAvAnalyticItemViewModel, MpAvParameterViewModelBase>,
        MpISelectableViewModel,
        MpILabelTextViewModel,
        MpIHoverableViewModel,
        MpIAsyncCollectionObject,
        MpIMenuItemViewModel,
        MpIIconResourceViewModel,
        MpIUserIconViewModel,
        MpIContentTypeDependant,
        MpIShortcutCommandViewModel,
        MpIPopupMenuPicker,
        MpIParameterHostViewModel,
        MpAvIParameterCollectionViewModel {

        #region Interfaces

        #region MpIIconResourceViewModel Implementation

        object MpIIconResource.IconResourceObj =>
            IconId;

        #endregion

        #region MpIContentTypeDependant Implementation

        bool MpIContentTypeDependant.IsContentTypeValid(MpCopyItemType cit) {
            if (Parent == null) {
                return false;
            }
            return Parent.IsContentTypeValid(cit);
        }

        #endregion

        #region MpIParameterHost Implementation

        int MpIParameterHostViewModel.IconId => IconId;
        string MpIParameterHostViewModel.PluginGuid =>
            PluginFormat == null ? string.Empty : PluginFormat.guid;

        public MpRuntimePlugin PluginFormat => Parent == null ? null : Parent.PluginFormat;

        MpPresetParamaterHostBase MpIParameterHostViewModel.ComponentFormat => AnalyzerComponentFormat;

        MpPresetParamaterHostBase MpIParameterHostViewModel.BackupComponentFormat =>
            PluginFormat == null || PluginFormat.backupCheckPluginFormat == null || PluginFormat.backupCheckPluginFormat.analyzer == null ?
                null : PluginFormat.backupCheckPluginFormat.analyzer;

        public MpAnalyzerComponent AnalyzerComponentFormat =>
            PluginFormat == null ? null : PluginFormat.analyzer;


        #endregion

        #region MpIPopupMenuPicker Implementation

        public MpAvMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedAnalyticItemPresetIds, bool recursive) {
            return new MpAvMenuItemViewModel() {
                MenuItemId = AnalyticItemPresetId,
                Header = Label,
                IconId = IconId,
                Command = cmd,
                CommandParameter = AnalyticItemPresetId,
                IsChecked = selectedAnalyticItemPresetIds.Contains(AnalyticItemPresetId)
            };
        }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; } = false;


        public DateTime LastSelectedDateTime {
            get {
                if (Preset == null) {
                    return DateTime.MinValue;
                }
                return Preset.LastSelectedDateTime;
            }
            set {
                if (LastSelectedDateTime != value) {
                    Preset.LastSelectedDateTime = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(LastSelectedDateTime));
                }
            }
        }

        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; } = false;

        #endregion

        #region MpIPluginComponentViewModel Implementation
        public MpPresetParamaterHostBase ComponentFormat => AnalyzerFormat;

        #endregion

        #region MpILabelTextViewModel Implementation

        string MpILabelText.LabelText => Label;
        #endregion

        #region MpAvIParameterCollectionViewModel Implementation

        IEnumerable<MpAvParameterViewModelBase> MpAvIParameterCollectionViewModel.Items =>
            VisibleItems.Where(x => !x.IsExecuteParameter).ToList();

        MpAvParameterViewModelBase
            MpAvIParameterCollectionViewModel.SelectedItem {
            get => SelectedItem;
            set => SelectedItem = value;
        }

        #region MpISaveOrCancelableViewModel Implementation
        bool MpISaveOrCancelableViewModel.IsSaveCancelEnabled =>
            true;
        public ICommand SaveCommand => new MpCommand(
            () => {
                Items.ForEach(x => x.SaveCurrentValueCommand.Execute(null));
                OnPropertyChanged(nameof(CanSaveOrCancel));
            },
            () => {
                return CanSaveOrCancel;
            }, new[] { this });
        public ICommand CancelCommand => new MpCommand(
            () => {
                Items.ForEach(x => x.RestoreLastValueCommand.Execute(null));
                OnPropertyChanged(nameof(CanSaveOrCancel));
            },
            () => {
                return CanSaveOrCancel;
            }, new[] { this });

        private bool _canSaveOrCancel = false;
        public bool CanSaveOrCancel {
            get {
                bool result = FormItems.Any(x => x.HasModelChanged);
                if (result != _canSaveOrCancel) {
                    _canSaveOrCancel = result;
                    OnPropertyChanged(nameof(CanSaveOrCancel));
                }
                return _canSaveOrCancel;
            }
        }

        #endregion

        #endregion

        #region MpIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType =>
            MpShortcutType.AnalyzeCopyItemWithPreset;
        public string KeyString =>
            MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutKeyString(this);

        public object ShortcutCommandParameter =>
            AnalyticItemPresetId;
        ICommand MpIShortcutCommandViewModel.ShortcutCommand =>
            Parent == null ? null : Parent.PerformAnalysisCommand;

        #endregion

        #endregion

        #region Properties

        #region MpAvTreeSelectorViewModelBase Overrides

        public override MpAvAnalyticItemViewModel ParentTreeItem => Parent;

        #endregion

        #region View Models

        public override ObservableCollection<MpAvParameterViewModelBase> Items {
            get => base.Items;
            set => base.Items = value;
        }

        public override MpAvParameterViewModelBase SelectedItem {
            get => base.SelectedItem;
            set => base.SelectedItem = value;
        }

        public Dictionary<string, string> ParamLookup =>
            Items.ToDictionary(x => x.ParamId, x => x.CurrentValue);
        public MpAvMenuItemViewModel ContextMenuItemViewModel {
            get {
                return new MpAvMenuItemViewModel() {
                    Header = Label,
                    Command = MpAvClipTrayViewModel.Instance.AnalyzeSelectedItemCommand,
                    CommandParameter = AnalyticItemPresetId,
                    IconId = IconId,
                    //ShortcutType = MpShortcutType.AnalyzeCopyItemWithPreset,
                    //ShortcutObjId = AnalyticItemPresetId,
                    ShortcutArgs = new object[] { MpShortcutType.AnalyzeCopyItemWithPreset, this }
                };
            }
        }

        public IEnumerable<MpAvParameterViewModelBase> VisibleItems =>
            Items.Where(x => x.IsVisible);

        public IEnumerable<MpAvParameterViewModelBase> FormItems =>
            Items.Where(x => !x.IsExecuteParameter);
        public IEnumerable<MpAvParameterViewModelBase> ExecuteItems =>
            Items.Where(x => x.IsExecuteParameter);
        
        public IEnumerable<MpAvParameterViewModelBase> SharedItems =>
            Items.Where(x => x.IsSharedValue);

        #endregion

        #region Appearance
        public string DataGridPresetExecuteToolTip {
            get {
                if (string.IsNullOrEmpty(Parent.CannotExecuteTooltip)) {
                    return UiStrings.CommonAnalyzeButtonLabel;
                }
                return Parent.CannotExecuteTooltip;
            }
        }
        //public string ResetOrDeleteLabel => $"{(CanDelete ? "Delete" : "Reset")} '{LabelText}'";
        #endregion

        #region State

        public bool IsAnyBusy =>
            Items.Any(x => x.IsAnyBusy) || IsBusy;
        public bool CanDataGridPresetExecute =>
            Parent != null && Parent.CanAnalyzerExecute;
        public bool IsExecuting { get; set; }
        public string ShortcutTooltipText =>
            string.IsNullOrEmpty(KeyString) ?
                string.Format(UiStrings.AnalyzerShortcutUnassignedTooltip, Label) :
                UiStrings.AnalyzerShortcutTooltip;

        public bool HasAnyParameterValueChange => Items.Any(x => x.HasModelChanged);
        public bool IsLabelTextBoxFocused { get; set; } = false;
        public bool IsLabelReadOnly { get; set; } = true;
        public bool IsAllValid => Items.All(x => x.IsValid);
        public bool IsManifestPreset =>
            Parent == null ?
                false :
                Parent.AnalyzerComponentFormat.presets != null &&
                    Parent.AnalyzerComponentFormat.presets.Any(x => x.guid == PresetGuid);

        public bool IsGeneratedDefaultPreset {
            get {
                if (Parent == null) {
                    return false;
                }
                if (Parent.Items.Any(x => x.IsManifestPreset)) {
                    // default won't be generated if plugin comes w/ presets
                    return false;
                }
                // lowest id will be generated preset
                return Parent.Items.OrderBy(x => x.AnalyticItemPresetId).FirstOrDefault() == this;
            }
        }
        public bool IsSystemPreset =>
            IsManifestPreset || IsGeneratedDefaultPreset;
        #endregion

        #region Model 

        public bool IsActionPreset {
            get {
                if (Preset == null) {
                    return false;
                }
                return Preset.IsActionPreset;
            }
            set {
                if (IsActionPreset != value) {
                    Preset.IsActionPreset = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsActionPreset));
                }
            }
        }

        public string FullName {
            get {
                if (Preset == null || Parent == null) {
                    return string.Empty;
                }
                return $"{Parent.Title}/{Label}";
            }
        }

        public string Label {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(Preset.Label)) {
                    return Preset.Label;
                }
                return Preset.Label;
            }
            set {
                if (Preset.Label != value) {
                    Preset.Label = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public string Description {
            get {
                if (Preset == null) {
                    return null;
                }
                if (string.IsNullOrEmpty(Preset.Description)) {
                    return null;
                }
                return Preset.Description;
            }
            set {
                if (Description != value) {
                    Preset.Description = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public int SortOrderIdx {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.SortOrderIdx;
            }
            set {
                if (Preset != null && SortOrderIdx != value) {
                    Preset.SortOrderIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        public bool IsQuickAction {
            get {
                if (Preset == null) {
                    return true;
                }
                return Preset.IsQuickAction;
            }
            set {
                if (IsQuickAction != value) {
                    Preset.IsQuickAction = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsQuickAction));
                }
            }
        }

        public int IconId {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.IconId;
            }
            set {
                if (IconId != value) {
                    Preset.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        public string PluginGuid {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                return Preset.PluginGuid;
            }
        }

        public string PresetGuid {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                return Preset.Guid;
            }
        }

        public int AnalyticItemPresetId {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.Id;
            }
        }


        public MpAnalyzerComponent AnalyzerFormat {
            get {
                if (Parent == null) {
                    return null;
                }
                return Parent.AnalyzerComponentFormat;
            }
        }

        public MpPreset Preset { get; protected set; }

        #endregion

        #endregion

        #region Events
        public event EventHandler<MpAvParameterViewModelBase> OnParameterValuesChanged;
        #endregion

        #region Constructors

        public MpAvAnalyticItemPresetViewModel() : base(null) { }

        public MpAvAnalyticItemPresetViewModel(MpAvAnalyticItemViewModel parent) : base(parent) {
            PropertyChanged += MpPresetParameterViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPreset aip) {
            IsBusy = true;

            Items.Clear();

            Preset = aip;

            // get all preset values from db
            IEnumerable<MpParameterValue> presetValues = new List<MpParameterValue>();

            try {
                presetValues = await MpAvPluginParameterValueLocator.LocateValuesAsync(
                    hostType: MpParameterHostType.Preset,
                    paramHostId: AnalyticItemPresetId,
                    pluginHost: Parent);
            }
            catch (Exception ex) {
                // currently only managed exception is for missing deferredValueComponent when paramFormat is flagged w/ isDeferredValue

                // show exception but continue loading
                _ = Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                notificationType: MpNotificationType.PluginResponseWarning,
                                body: ex.Message);
            }
            var paramLookup = new Dictionary<string, List<MpParameterValue>>();
            foreach (var pv in presetValues) {
                if (!paramLookup.ContainsKey(pv.ParamId)) {
                    paramLookup.Add(pv.ParamId, new List<MpParameterValue>());
                }
                paramLookup[pv.ParamId].Add(pv);
            }

            // var valLookup = presetValues.ToDictionary<string, List<MpParameterValue>>(x => x.ParamId, x=>x.Sele)
            foreach (var paramValGroup in presetValues) {
                var naipvm = await CreateParameterViewModel(paramValGroup);
                Items.Add(naipvm);
            }


            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(ShortcutTooltipText));
            OnPropertyChanged(nameof(Items));
            Items.ForEach(x => x.Validate());
            HasModelChanged = false;

            IsBusy = false;
        }

        public async Task<MpAvParameterViewModelBase> CreateParameterViewModel(MpParameterValue aipv) {
            var naipvm = await MpAvPluginParameterBuilder.CreateParameterViewModelAsync(aipv, this);
            naipvm.OnParamValidate += ParameterViewModel_OnValidate;
            naipvm.OnParamValueChanged += (s, e) => {
                OnParameterValuesChanged?.Invoke(this, naipvm);
            };
            return naipvm;
        }

        public bool Validate() {
            foreach (var pvm in Items) {
                pvm.ValidationMessage = pvm.GetValidationMessage(IsExecuting);
            }
            OnPropertyChanged(nameof(IsAllValid));
            return IsAllValid;
        }
        public void ResetExecutionState() {
            IsExecuting = false;
            ClearAllValidations();
            Validate();
        }
        public void ClearAllValidations() {
            foreach (var pvm in Items) {
                pvm.ClearValidation();
            }
            OnPropertyChanged(nameof(IsAllValid));
        }
        public bool CanDelete(object args) {
            if (args == null) {
                return !IsSystemPreset;
            }
            return false;
        }
        #endregion

        #region Protected Methods

        protected virtual void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            if (sender is MpAvParameterViewModelBase aipvm) {
                aipvm.ValidationMessage = aipvm.GetValidationMessage(IsExecuting);
            }
            Parent.Validate();
        }

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == AnalyticItemPresetId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(KeyString));
                }
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == AnalyticItemPresetId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(KeyString));
                }
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == AnalyticItemPresetId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(KeyString));
                }
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private void MpPresetParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        Parent.OnPropertyChanged(nameof(Parent.IsSelected));
                        if (Parent.SelectedItem != this) {
                            Parent.SelectedItem = this;
                        }

                        Items.Where(x => x is MpAvEnumerableParameterViewModelBase)
                            .Cast<MpAvEnumerableParameterViewModelBase>()
                           .SelectMany(x => x.Items)
                           .ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                        if (this is MpAvIParameterCollectionViewModel pcvm) {
                            // intermittently params not updating on selection change, just stays empty
                            pcvm.OnPropertyChanged(nameof(pcvm.Items));
                        }
                    } else {
                        IsLabelReadOnly = true;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        Task.Run(async () => {
                            await Preset.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(IsLabelReadOnly):
                    if (!IsLabelReadOnly) {
                        IsLabelTextBoxFocused = true;
                        IsSelected = true;
                    }
                    break;
                case nameof(Label):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.FullLabel)));
                    break;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (this is MpAvIParameterCollectionViewModel pcvm) {
                pcvm.OnPropertyChanged(nameof(pcvm.Items));
            }
        }

        #endregion

        #region Commands

        public ICommand ToggleIsQuickActionCommand => new MpCommand(
            () => {
                IsQuickAction = !IsQuickAction;
            });

        public MpIAsyncCommand ExecutePresetAnalysisOnSelectedContentCommand => new MpAsyncCommand(
            async () => {
                await Parent.PerformAnalysisCommand.ExecuteAsync(this);

            }
            //, () => {
            //    if (Parent == null) {
            //        return false;
            //    }
            //    return Parent.PerformAnalysisCommand.CanExecute(this);
            //}
            );
        public ICommand ToggleIsLabelReadOnlyCommand => new MpCommand(
            () => {
                IsLabelReadOnly = !IsLabelReadOnly;
            });

        public ICommand ResetThisPresetCommand => new MpCommand(
            () => {
                Parent.ResetPresetCommand.Execute(this);
            }, () => {
                if (Parent == null) {
                    return false;
                }
                return Parent.ResetPresetCommand.CanExecute(this);
            });

        public ICommand DeleteThisPresetCommand => new MpCommand(
            () => {
                Parent.DeletePresetCommand.Execute(this);
            }, () => {
                if (Parent == null) {
                    return false;
                }
                return Parent.DeletePresetCommand.CanExecute(this);
            });

        public ICommand ResetOrDeleteThisPresetCommand => new MpCommand<object>(
            (args) => {
                if (args == null) {
                    // called from sidebar preset grid and trans unselect
                    Parent.ResetOrDeletePresetCommand.Execute(this);
                    return;
                }
                // called from trans select
                Parent.ResetOrDeletePresetCommand.Execute(new object[] { this, args });
            });

        public ICommand DuplicateThisPresetCommand => new MpCommand(
            () => {
                if (Parent == null) {
                    return;
                }
                Parent.DuplicatePresetCommand.Execute(this);
            });

        #endregion
    }
}
