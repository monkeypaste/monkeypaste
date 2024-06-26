﻿using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipboardFormatPresetViewModel :
        MpAvSelectorViewModelBase<MpAvHandledClipboardFormatViewModel, MpAvParameterViewModelBase>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIUserIconViewModel,
        MpILabelTextViewModel,
        MpITreeItemViewModel,
        MpAvIParameterCollectionViewModel {

        #region Interfaces

        #region MpISelectableViewModel Implementation
        public bool IsSelected { get; set; }

        #endregion

        #region MpILabelText Implementation
        string MpILabelText.LabelText => Label;

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => Parent;
        public IEnumerable<MpITreeItemViewModel> Children => Items;

        #endregion

        #region MpIPluginComponentViewModel Implementation
        public MpPresetParamaterHostBase ComponentFormat => ClipboardFormat;

        #endregion

        #region MpAvIParameterCollectionViewModel Implementation

        IEnumerable<MpAvParameterViewModelBase> MpAvIParameterCollectionViewModel.Items =>
            VisibleItems.ToList();

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
            }, new[] { this });

        private bool _canSaveOrCancel = false;
        public bool CanSaveOrCancel {
            get {
                bool result = Items.Any(x => x.HasModelChanged);
                if (result != _canSaveOrCancel) {
                    _canSaveOrCancel = result;
                    OnPropertyChanged(nameof(CanSaveOrCancel));
                }
                return _canSaveOrCancel;
            }
        }

        bool MpISaveOrCancelableViewModel.IsSaveCancelEnabled =>
            true;
        #endregion
        #endregion

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvParameterViewModelBase> VisibleItems => Items.Where(x => x.IsVisible);
        #endregion

        #region Appearance

        #endregion

        #region State
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
                return Parent.Items.OrderBy(x => x.PresetId).FirstOrDefault() == this;
            }
        }
        public bool IsManifestPreset =>
            Parent == null ?
                false :
                Parent.ClipboardPluginFormat.presets != null &&
                    Parent.ClipboardPluginFormat.presets.Any(x => x.guid == PresetGuid);
        public bool IsDropItemHovering { get; set; } = false;
        public bool IsLabelTextBoxFocused { get; set; } = false;
        public bool IsLabelReadOnly { get; set; } = true;

        public bool IsAllValid => Items.All(x => x.IsValid);

        public bool IsReader => Parent == null ? false : Parent.IsReader;
        public bool IsWriter => Parent == null ? false : Parent.IsWriter;

        public bool IsFormatOnSourceDragObject {
            get {
                if (MpAvContentWebViewDragHelper.DragDataObject == null) {
                    return false;
                }
                bool format_exists =
                    MpAvContentWebViewDragHelper.DragDataObject.ContainsData(ClipboardFormat.formatName);
                return format_exists;
            }
        }

        public bool IsFormatPlaceholderOnTargetDragObject =>
            false;
        #endregion

        #region Model

        #region Db
        public string FullName {
            get {
                if (Preset == null || Parent == null) {
                    return string.Empty;
                }
                return $"{Parent.Title} - {Label}";
            }
        }

        public bool IsDefault {
            get {
                if (Preset == null) {
                    return false;
                }
                return Preset.IsDefault;
            }
            set {
                if (IsDefault != value) {
                    Preset.IsDefault = value;
                    OnPropertyChanged(nameof(IsDefault));
                }
            }
        }

        public bool IsEnabled {
            get {
                if (Preset == null) {
                    return false;
                }
                return Preset.IsEnabled;
            }
            set {
                if (IsEnabled != value) {
                    Preset.IsEnabled = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsEnabled));
                }
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
        public int PresetId {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.Id;
            }
        }

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

        public MpPreset Preset { get; set; }

        #endregion

        #region Plugin

        public string FormatName =>
            ClipboardFormat == null ? string.Empty : ClipboardFormat.formatName;

        public MpClipboardHandlerFormat ClipboardFormat {
            get {
                if (Parent == null) {
                    return null;
                }
                return Parent.ClipboardPluginFormat;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Events
        public static event EventHandler<(bool, object)> OnFormatPresetEnabledChanged;
        #endregion

        #region Constructors

        public MpAvClipboardFormatPresetViewModel(MpAvHandledClipboardFormatViewModel parent) : base(parent) {
            PropertyChanged += MpClipboardFormatPresetViewModel_PropertyChanged;
            OnFormatPresetEnabledChanged += MpAvClipboardFormatPresetViewModel_OnFormatPresetIsEnabledToggled;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPreset aip) {
            IsBusy = true;

            Items.Clear();

            Preset = aip;
            var presetValues = await MpAvPluginParameterValueLocator.LocateValuesAsync(MpParameterHostType.Preset, PresetId, Parent);

            foreach (var paramVal in presetValues) {
                var naipvm = await CreateParameterViewModelAsync(paramVal);
                Items.Add(naipvm);
            }


            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));
            Items.ForEach(x => x.Validate());
            HasModelChanged = false;

            IsBusy = false;
        }
        public async Task<MpAvParameterViewModelBase> CreateParameterViewModelAsync(MpParameterValue aipv) {
            var naipvm = await MpAvPluginParameterBuilder.CreateParameterViewModelAsync(aipv, Parent);
            naipvm.OnParamValidate += ParameterViewModel_OnValidate;

            return naipvm;
        }

        public string GetPresetParamJson() {
            return MpJsonExtensions.SerializeObject(Items.Select(x => new[] { x.ParamId, x.CurrentValue }).ToList());
        }

        public bool CanDelete(object args) {
            if (args == null) {
                return !IsManifestPreset && !IsGeneratedDefaultPreset;
            }
            return false;
        }
        #endregion

        #region Protected Methods

        protected virtual void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            var aipvm = sender as MpAvParameterViewModelBase;
            //var aipvm = sender as MpAvParameterViewModelBase;
            //if (aipvm.IsRequired && string.IsNullOrWhiteSpace(aipvm.CurrentValue)) {
            //    aipvm.ValidationMessage = $"{aipvm.LabelText} is required";
            //} else {
            //    aipvm.ValidationMessage = string.Empty;
            //}
            aipvm.ValidationMessage = aipvm.GetValidationMessage(false);
            Parent.ValidateParameters();
        }

        #endregion

        #region Private Methods

        private void MpClipboardFormatPresetViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged && IsAllValid) {
                        Task.Run(async () => {
                            await Preset.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(Label):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.FullLabel)));
                    break;
                case nameof(IsEnabled):
                    if(IsEnabled) {
                        MpDataFormatRegistrar.RegisterDataFormat(FormatName,IsReader,IsWriter);
                    } else {
                        MpDataFormatRegistrar.UnregisterDataFormat(FormatName, IsReader, IsWriter);
                    }
                    break;
            }
        }

        private async Task<bool> SetIsEnabledAsync(bool is_enabled, MpAvAppViewModel avm) {
            bool did_change;
            bool result;
            if (avm == null) {
                did_change = is_enabled != IsEnabled;
                IsEnabled = is_enabled;
                result = IsEnabled;
            } else {
                bool was_enabled = avm.OleFormatInfos.IsFormatEnabledByPresetId(PresetId);
                result = await avm.OleFormatInfos.SetIsEnabledAsync(PresetId, is_enabled);
                did_change = was_enabled != result;
            }
            if (did_change) {
                // this updates context menu if open
                MpMessenger.SendGlobal(MpMessageType.ClipboardPresetEnabledChanged);
            }
            return result;
        }

        private void MpAvClipboardFormatPresetViewModel_OnFormatPresetIsEnabledToggled(object sender, (bool wasEnabled, object args) e) {
            if (e.IsDefault() ||
                !e.wasEnabled ||
                sender is not MpAvClipboardFormatPresetViewModel cfpvm ||
                cfpvm.FormatName != FormatName ||
                cfpvm.IsReader != IsReader ||
                cfpvm.PresetId == PresetId) {
                return;
            }
            // only get here if event trigger from matching format preset griup that was enabled and isn't this preset

            Dispatcher.UIThread.Post(async () => {
                // disable this preset (if enabled) for the app or default (when avm is null)

                MpAvAppViewModel avm = await MpAvAppCollectionViewModel.Instance.AddOrGetAppByArgAsync(e.args);

                await SetIsEnabledAsync(false, avm);
            });

        }

        private async Task<bool?> ToggleIsEnabledAsync(object args) {
            MpAvAppViewModel avm = await MpAvAppCollectionViewModel.Instance.AddOrGetAppByArgAsync(args);
            bool will_enable = !IsEnabled;
            if (avm != null) {
                will_enable = !avm.OleFormatInfos.IsFormatEnabledByPresetId(PresetId);
            }
            bool result = await SetIsEnabledAsync(will_enable, avm);
            return result;
        }
        #endregion

        #region Commands

        public MpIAsyncCommand<object> TogglePresetIsEnabledCommand => new MpAsyncCommand<object>(
            async (args) => {
                bool? was_enabled = await ToggleIsEnabledAsync(args);

                if (was_enabled.HasValue) {
                    // only cmd invoked preset issues toggle event
                    OnFormatPresetEnabledChanged?.Invoke(this, (was_enabled.Value, args));
                } else {
                    // error creating/fetching app vm
                }
            });

        public ICommand ToggleIsLabelReadOnlyCommand => new MpCommand(
            () => {
                IsLabelReadOnly = !IsLabelReadOnly;
            });

        public ICommand ResetThisPresetCommand => new MpCommand(
            () => {
                Parent.ResetOrDeletePresetCommand.Execute(this);
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
                    // called from sidebar preset grid
                    Parent.ResetOrDeletePresetCommand.Execute(this);
                    return;
                }
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
