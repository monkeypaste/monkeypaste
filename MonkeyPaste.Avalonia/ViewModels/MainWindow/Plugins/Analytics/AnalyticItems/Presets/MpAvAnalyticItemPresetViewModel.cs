using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 
using Newtonsoft.Json;
using Avalonia.Controls;
using Avalonia.Animation;
using Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyticItemPresetViewModel : 
        MpAvTreeSelectorViewModelBase<MpAvAnalyticItemViewModel, MpAvParameterViewModelBase>,
        MpISelectableViewModel,
        MpILabelTextViewModel,
        MpIHoverableViewModel, 
        MpIMenuItemViewModel,
        MpIActionComponent, 
        MpIUserIconViewModel, 
        MpIContentTypeDependant,
        MpAvIShortcutCommandViewModel, 
        MpIPopupMenuPicker,
        MpIParameterHostViewModel,
        MpAvIParameterCollectionViewModel {

        #region Interfaces

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

        public MpPluginFormat PluginFormat => Parent == null ? null : Parent.PluginFormat;

        MpParameterHostBaseFormat MpIParameterHostViewModel.ComponentFormat => AnalyzerComponentFormat;

        MpParameterHostBaseFormat MpIParameterHostViewModel.BackupComponentFormat =>
            PluginFormat == null || PluginFormat.backupCheckPluginFormat == null || PluginFormat.backupCheckPluginFormat.analyzer == null ?
                null : PluginFormat.backupCheckPluginFormat.analyzer;

        public MpAnalyzerPluginFormat AnalyzerComponentFormat =>
            PluginFormat == null ? null : PluginFormat.analyzer;

        public MpIPluginComponentBase PluginComponent =>
            PluginFormat == null ? null : PluginFormat.Component as MpIPluginComponentBase;

        #endregion

        #region MpIPopupMenuPicker Implementation

        public MpMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedAnalyticItemPresetIds, bool recursive) {
            return new MpMenuItemViewModel() {
                MenuItemId = AnalyticItemPresetId,
                Header = Label,
                IconId = IconId,
                Command = cmd,
                CommandParameter = AnalyticItemPresetId,
                IsChecked = selectedAnalyticItemPresetIds.Contains(AnalyticItemPresetId)
            };
        }

        #endregion

        #region MpIActionComponent Implementation

        void MpIActionComponent.RegisterActionComponent(MpIInvokableAction mvm) {
            Parent.OnAnalysisCompleted += mvm.OnActionInvoked;
            MpConsole.WriteLine($"Analyzer {Parent.Title}-{Label} Registered {mvm.Label} matcher");
        }


        void MpIActionComponent.UnregisterActionComponent(MpIInvokableAction mvm) {
            Parent.OnAnalysisCompleted -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"Analyzer {Parent.Title}-{Label} unregistered {mvm.Label} matcher");
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
        public MpParameterHostBaseFormat ComponentFormat => AnalyzerFormat;

        #endregion


        #region MpILabelTextViewModel Implementation

        string MpILabelText.LabelText => Label;
        #endregion


        #region MpAvIParameterCollectionViewModel Implementation

        IEnumerable<MpAvParameterViewModelBase>
            MpAvIParameterCollectionViewModel.Items => VisibleItems;

        MpAvParameterViewModelBase
            MpAvIParameterCollectionViewModel.SelectedItem {
            get => SelectedItem;
            set => SelectedItem = value;
        }

        #region MpISaveOrCancelableViewModel Implementation

        public ICommand SaveCommand => new MpCommand(
            () => {
                Items.ForEach(x => x.SaveCurrentValueCommand.Execute(null));
            },
            () => {
                return CanSaveOrCancel;
            }, new[] { this });
        public ICommand CancelCommand => new MpCommand(
            () => {
                Items.ForEach(x => x.RestoreLastValueCommand.Execute(null));
            },
            () => {
                return CanSaveOrCancel;
            }, new[] {this});

        private bool _canSaveOrCancel = false;
        public bool CanSaveOrCancel {
            get {
                bool result = Items.Any(x => x.HasModelChanged);
                if(result != _canSaveOrCancel) {
                    _canSaveOrCancel = result;
                    OnPropertyChanged(nameof(CanSaveOrCancel));
                }
                return _canSaveOrCancel;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Properties

        #region MpAvTreeSelectorViewModelBase Overrides

        public override MpAvAnalyticItemViewModel ParentTreeItem => Parent;

        #endregion

        #region View Models

        public Dictionary<object, MpAvParameterViewModelBase> ParamLookup => 
            Items.ToDictionary(x => x.ParamId,x => x); 
        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    Header = Label,
                    Command = MpAvClipTrayViewModel.Instance.AnalyzeSelectedItemCommand,
                    CommandParameter = AnalyticItemPresetId,
                    IconId = IconId,
                    //ShortcutType = MpShortcutType.AnalyzeCopyItemWithPreset,
                    //ShortcutObjId = AnalyticItemPresetId,
                    ShortcutArgs = new object[] {
                        MpShortcutType.AnalyzeCopyItemWithPreset,
                        AnalyticItemPresetId}
                };
            }
        }

        public IEnumerable<MpAvParameterViewModelBase> VisibleItems => Items.Where(x => x.IsVisible);

        #endregion

        #region Appearance

        public string ResetOrDeleteLabel => $"{(IsManifestPreset ? "Reset":"Delete")} '{Label}'";
        #endregion

        #region State

        public bool HasAnyParameterValueChange => Items.Any(x => x.HasModelChanged);
        public bool IsLabelTextBoxFocused { get; set; } = false;
        public bool IsLabelReadOnly { get; set; } = true;
        public bool IsAllValid => Items.All(x => x.IsValid);
        public bool IsManifestPreset =>
            Parent == null ? 
                false : 
                Parent.AnalyzerComponentFormat.presets != null &&
                    Parent.AnalyzerComponentFormat.presets.Any(x => x.guid == PresetGuid);


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
                if(IsActionPreset != value) {
                    Preset.IsActionPreset = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsActionPreset));
                }
            }
        }

        public string FullName {
            get {
                if(Preset == null || Parent == null) {
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
                if(Preset.Label != value) {
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
                if(Preset != null && SortOrderIdx != value) {
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
                if(IsQuickAction != value) {
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
                if(IconId != value) {
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
                if(Preset == null) {
                    return 0;
                }
                return Preset.Id;
            }
        }


        public MpAnalyzerPluginFormat AnalyzerFormat {
            get {
                if (Parent == null) {
                    return null;
                }
                return Parent.AnalyzerComponentFormat;
            }
        }

        public MpPluginPreset Preset { get; protected set; }
        
        #endregion

        #region MpAvIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType => MpShortcutType.AnalyzeCopyItemWithPreset;

        public MpAvShortcutViewModel ShortcutViewModel {
            get {
                if(Parent == null || Preset == null) {
                    return null;
                }
                var scvm = MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.CommandParameter == Preset.Id.ToString() && x.ShortcutType == ShortcutType);

                if(scvm == null) {
                    scvm = new MpAvShortcutViewModel(MpAvShortcutCollectionViewModel.Instance);
                }

                return scvm;
            }
        }

        public string ShortcutKeyString => ShortcutViewModel.KeyString;

        public ICommand AssignCommand => AssignHotkeyCommand;

        #endregion

        #endregion

        #region Constructors

        public MpAvAnalyticItemPresetViewModel() : base (null) { }

        public MpAvAnalyticItemPresetViewModel(MpAvAnalyticItemViewModel parent) : base(parent) {
            PropertyChanged += MpPresetParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPluginPreset aip) {
            IsBusy = true;

            Items.Clear();

            Preset = aip;
            //if (AnalyticItemPresetId == 774) {
            //    Debugger.Break();
            //}

            // get all preset values from db
            //var presetValues = await PrepareParameterValueModelsAsync();
            var presetValues = await MpAvPluginParameterValueLocator.LocateValuesAsync(
                MpParameterHostType.Preset, AnalyticItemPresetId, Parent);
            var paramLookup = new Dictionary<string, List<MpParameterValue>>();
            foreach(var pv in presetValues) {
                if(!paramLookup.ContainsKey(pv.ParamId)) {
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

            OnPropertyChanged(nameof(ShortcutViewModel));
            OnPropertyChanged(nameof(Items));
            Items.ForEach(x => x.Validate());
            HasModelChanged = false;

            IsBusy = false;
        }

        public async Task<MpAvParameterViewModelBase> CreateParameterViewModel(MpParameterValue aipv) {
            var naipvm = await MpAvPluginParameterBuilder.CreateParameterViewModelAsync(aipv, this);
            naipvm.OnValidate += ParameterViewModel_OnValidate;
            return naipvm;
        }



        #endregion

        #region Protected Methods

        protected virtual void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            var aipvm = sender as MpAvParameterViewModelBase;
            if (aipvm.IsRequired && string.IsNullOrWhiteSpace(aipvm.CurrentValue)) {
                aipvm.ValidationMessage = $"{aipvm.Label} is required";
            } else {
                aipvm.ValidationMessage = string.Empty;
            }
            Parent.Validate();
        }

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == AnalyticItemPresetId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == AnalyticItemPresetId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == AnalyticItemPresetId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } 
        }
        #endregion

        #endregion

        #region Private Methods

        private void MpPresetParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        Parent.OnPropertyChanged(nameof(Parent.IsSelected));
                        if(Parent.SelectedItem != this) {
                            Parent.SelectedItem = this;
                        }

                        Items.Where(x => x is MpAvEnumerableParameterViewModel)
                            .Cast<MpAvEnumerableParameterViewModel>()
                           .SelectMany(x => x.Items)
                           .ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));

                    } else {
                        IsLabelReadOnly = true;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    break;
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        Task.Run(async () => { 
                            await Preset.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(IsLabelReadOnly):
                    if(!IsLabelReadOnly) {
                        IsLabelTextBoxFocused = true;
                        IsSelected = true;
                    }
                    break;
            } 
        }

        #endregion

        #region Commands

        //public ICommand ManagePresetCommand => new MpCommand(
        //    () => {
        //        Parent.Items.ForEach(x => x.IsSelected = x == this);
        //        Parent.Items.ForEach(x => x.IsEditingParameters = x == this);
        //        Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
        //    }, !IsEditingParameters && !Parent.IsAnyEditingParameters);

        public ICommand AssignHotkeyCommand => new MpAsyncCommand(
            async () => {
                await MpAvShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                    $"Use {Label} Analyzer",
                    MpAvClipTrayViewModel.Instance.AnalyzeSelectedItemCommand,
                    MpShortcutType.AnalyzeCopyItemWithPreset,
                    Preset.Id.ToString(),
                    ShortcutKeyString);

                OnPropertyChanged(nameof(ShortcutViewModel));

                OnPropertyChanged(nameof(ShortcutKeyString));


                if (ShortcutViewModel != null) {
                    ShortcutViewModel.OnPropertyChanged(nameof(ShortcutViewModel.KeyItems));
                }
            });

        public ICommand ToggleEditLabelCommand => new MpCommand(
            () => {
                IsLabelReadOnly = !IsLabelReadOnly;
            });



        #endregion
    }
}
