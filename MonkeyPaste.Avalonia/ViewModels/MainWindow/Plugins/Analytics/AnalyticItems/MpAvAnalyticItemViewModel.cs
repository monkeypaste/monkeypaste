﻿using MonkeyPaste.Common;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyticItemViewModel :
        MpAvPresetParamHostViewModelBase<MpAvAnalyticItemCollectionViewModel, MpAvAnalyticItemPresetViewModel>,
        MpISelectableViewModel,
        MpIAsyncCollectionObject,
        MpIHoverableViewModel,
        MpIAsyncComboBoxItemViewModel,
        MpIMenuItemViewModel
        //MpIParameterHostViewModel 
        {
        #region Private Variables
        #endregion

        #region Interfaces

        #region MpIParameterHost Implementation

        public override int IconId => PluginIconId;

        public override MpPresetParamaterHostBase ComponentFormat => AnalyzerComponentFormat;

        public override MpPresetParamaterHostBase BackupComponentFormat =>
            PluginFormat == null || PluginFormat.backupCheckPluginFormat == null || PluginFormat.backupCheckPluginFormat.analyzer == null ?
                null : PluginFormat.backupCheckPluginFormat.analyzer;

        public MpAnalyzerComponent AnalyzerComponentFormat =>
            PluginFormat == null ? null : PluginFormat.analyzer;

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; } = false;

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region MpIAsyncComboBoxItemViewModel Implementation


        int MpIComboBoxItemViewModel.IconId => PluginIconId;
        string MpIComboBoxItemViewModel.Label => Title;

        #endregion

        #endregion

        #region Properties

        #region MpAvTreeSelectorViewModelBase Overrides

        public override MpAvAnalyticItemCollectionViewModel ParentTreeItem => Parent;

        #endregion

        #region View Models

        public MpAvMenuItemViewModel ContextMenuItemViewModel {
            get {
                var subItems = SortedItems.Where(x => !x.IsActionPreset).Select(x => x.ContextMenuItemViewModel).ToList();
                if (subItems.Count > 0) {
                    subItems.Add(new MpAvMenuItemViewModel() { IsSeparator = true });
                }
                subItems.Add(
                    new MpAvMenuItemViewModel() {
                        IconResourceKey = Mp.Services.PlatformResource.GetResource("CogImage") as string,
                        Header = string.Format(UiStrings.CommonManageLabel2, Title),
                        Command = ManageAnalyticItemCommand,
                        CommandParameter = PluginGuid
                    });
                return new MpAvMenuItemViewModel() {
                    Header = Title,
                    IconId = PluginIconId,
                    SubItems = subItems
                };
            }
        }

        public IEnumerable<MpAvAnalyticItemPresetViewModel> SortedItems =>
            Items.OrderBy(x => x.SortOrderIdx);
        public IEnumerable<MpAvMenuItemViewModel> QuickActionPresetMenuItems => SortedItems.Where(x => x.IsQuickAction).Select(x => x.ContextMenuItemViewModel);
        #endregion        

        #region Appearance


        public string CannotExecuteTooltip { get; set; }

        #endregion

        #region State

        public bool IsAnyBusy =>
            Items.Any(x => x.IsAnyBusy) || IsBusy;

        public override bool IsLoaded =>
            Items.Count > 0 &&
            Items[0].Items.Count > 0;

        public bool IsHovering { get; set; } = false;

        public MpAnalyzerTransaction LastTransaction { get; private set; } = null;

        public bool CanAnalyzerExecute { get; set; }

        public object CurrentExecuteArgs { get; set; }
        #endregion

        #region Models        

        #region Db

        public int PluginIconId { get; private set; }

        #endregion

        #region Analyzer Plugin

        public MpAnalyzerInputFormatFlags InputFormatFlags {
            get {
                MpAnalyzerInputFormatFlags flags = MpAnalyzerInputFormatFlags.None;
                if (AnalyzerComponentFormat == null || AnalyzerComponentFormat.inputType == null) {
                    return flags;
                }
                if (AnalyzerComponentFormat.inputType.text) {
                    flags |= MpAnalyzerInputFormatFlags.Text;
                }
                if (AnalyzerComponentFormat.inputType.image) {
                    flags |= MpAnalyzerInputFormatFlags.Image;
                }
                if (AnalyzerComponentFormat.inputType.file) {
                    flags |= MpAnalyzerInputFormatFlags.File;
                }

                return flags;
            }
        }

        public MpAnalyzerOutputFormatFlags OutputFormatFlags {
            get {
                MpAnalyzerOutputFormatFlags flags = MpAnalyzerOutputFormatFlags.None;
                if (AnalyzerComponentFormat == null || AnalyzerComponentFormat.outputType == null) {
                    return flags;
                }
                if (AnalyzerComponentFormat.outputType.text) {
                    flags |= MpAnalyzerOutputFormatFlags.Text;
                }
                if (AnalyzerComponentFormat.outputType.image) {
                    flags |= MpAnalyzerOutputFormatFlags.Image;
                }
                if (AnalyzerComponentFormat.outputType.file) {
                    flags |= MpAnalyzerOutputFormatFlags.File;
                }
                if (AnalyzerComponentFormat.outputType.imageAnnotation) {
                    flags |= MpAnalyzerOutputFormatFlags.BoundingBox;
                }

                return flags;
            }
        }

        public string Title {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.title;
            }
        }

        public string Description {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.description;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public MpAvAnalyticItemViewModel() : base(null) { }

        public MpAvAnalyticItemViewModel(MpAvAnalyticItemCollectionViewModel parent) : base(parent) {
            MpMessenger.RegisterGlobal(ReceivedClipTrayMessage);
            PropertyChanged += MpAnalyticItemViewModel_PropertyChanged;
            Items.CollectionChanged += PresetViewModels_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(string plugin_guid) {
            PluginGuid = plugin_guid;
            bool is_valid = await ValidateAnalyzerAsync(PluginFormat);
            if (!is_valid) {
                PluginGuid = null;
                return;
            }
            if (IsLoaded) {
                return;
            }
            IsBusy = true;


            PluginIconId = await MpAvPluginIconLocator.LocatePluginIconIdAsync(PluginFormat);

            while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            Items.Clear();

            // NOTE ignoring plugin update msgs on initial load cause its a bunch and not relevant
            var presets = await MpAvPluginPresetLocator.LocatePresetsAsync(
                this,
                showMessages: false);// !Mp.Services.StartupState.StartupFlags.HasFlag(MpStartupFlags.Initial));

            foreach (var preset in presets) {
                var naipvm = await CreatePresetViewModelAsync(preset);
                Items.Add(naipvm);
            }
            await MpAvPluginParameterBuilder.CleanupMissingParamsAsync(Items);

            OnPropertyChanged(nameof(Items));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            await UpdatePresetSortOrderAsync();

            if (MpPluginLoader.UpdatedPluginGuids.Contains(PluginGuid)) {
                // show plugin updated ntf
                Mp.Services.NotificationBuilder
                    .ShowMessageAsync(
                        msgType: MpNotificationType.PluginUpdated,
                        title: UiStrings.PluginUpdatedNtfTitle,
                        body: string.Format(UiStrings.PluginUpdatedNtfText, Title, PluginFormat.version),
                        iconSourceObj: IconId).FireAndForgetSafeAsync();
            }


            IsBusy = false;
        }

        public async Task<MpAvAnalyticItemPresetViewModel> CreatePresetViewModelAsync(MpPreset aip) {
            MpAvAnalyticItemPresetViewModel naipvm = new MpAvAnalyticItemPresetViewModel(this);
            await naipvm.InitializeAsync(aip);
            return naipvm;
        }

        public void UpdateCanExecute() {
            CanAnalyzerExecute = CanExecuteAnalysis(CurrentExecuteArgs);
        }

        public string GetUniquePresetName() {
            int uniqueIdx = 1;
            string uniqueName = $"{Title} {UiStrings.CommonPresetLabel}";
            string testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);

            while (Items.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
            }
            return uniqueName + uniqueIdx;
        }


        public virtual bool Validate() {
            if (SelectedItem == null) {
                return true;
            }
            return SelectedItem.IsAllValid;
        }

        public bool IsContentTypeValid(MpCopyItemType cit) {
            bool isOkType = false;
            switch (cit) {
                case MpCopyItemType.Text:
                    isOkType = InputFormatFlags.HasFlag(MpAnalyzerInputFormatFlags.Text);
                    break;
                case MpCopyItemType.Image:
                    isOkType = InputFormatFlags.HasFlag(MpAnalyzerInputFormatFlags.Image);
                    break;
                case MpCopyItemType.FileList:
                    isOkType = InputFormatFlags.HasFlag(MpAnalyzerInputFormatFlags.File);
                    break;
            }
            return isOkType;
        }

        public async Task<MpAnalyzerTransaction> PerformAnalysisAsync(object args) {
            bool suppressWrite = false;

            MpCopyItem sourceCopyItem = null;
            MpAvAnalyticItemPresetViewModel targetAnalyzer = null;

            if (args is object[] argParts &&
                argParts.Length >= 0 &&
                argParts[0] is MpAvAnalyticItemPresetViewModel action_aipvm) {
                targetAnalyzer = action_aipvm;
                // when analyzer is triggered from action not user selection 
                if (argParts.Length > 1 &&
                    argParts[1] is MpCopyItem ci) {
                    sourceCopyItem = ci;
                }
            } else {
                if (args is MpAvAnalyticItemPresetViewModel aipvm) {
                    targetAnalyzer = aipvm;
                } else {
                    targetAnalyzer = SelectedItem;
                }
                sourceCopyItem = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
            }

            // show exec params if present and validate them
            bool can_begin = await PrepareAnalysisAsync(targetAnalyzer, args);
            if (!can_begin) {
                FinishAnalysis(targetAnalyzer);
                return null;
            }

            // rollup target params and cont
            MpAnalyzerTransaction this_transaction = await MpPluginTransactor.PerformTransactionAsync(
                                       PluginFormat,
                                       targetAnalyzer.ParamLookup,
                                       sourceCopyItem,
                                       targetAnalyzer.Preset,
                                       suppressWrite) as MpAnalyzerTransaction;
            if (this_transaction == null) {
                FinishAnalysis(targetAnalyzer);
                return null; ;
            }

            Func<Task<MpAnalyzerPluginResponseFormat>> retryAnalyzerFunc = () => {
                PerformAnalysisCommand.Execute(args);
                return null;
            };

            this_transaction.Response = await MpPluginTransactor.ValidatePluginResponseAsync(
                targetAnalyzer.Label,
                this_transaction.Request,
                this_transaction.Response,
                retryAnalyzerFunc);

            if (this_transaction.Response != null &&
                this_transaction.Response.invalidParams != null &&
                this_transaction.Response.invalidParams.Any()) {
                // retry and return that output
                foreach (var inv_kvp in this_transaction.Response.invalidParams) {
                    if (targetAnalyzer.Items.FirstOrDefault(x => x.ParamId.Equals(inv_kvp.Key)) is not { } aipvm) {
                        continue;
                    }
                    // flag invalid params w/ plugin provided invalid text
                    aipvm.ValidationMessage = inv_kvp.Value;
                }
                var retry_result = await PerformAnalysisAsync(args);
                return retry_result;
            }

            if (this_transaction.ResponseContent is MpCopyItem rci) {
                rci.WasDupOnCreate = rci.Id == sourceCopyItem.Id;
                await MpAvClipTrayViewModel.Instance.AddUpdateOrAppendCopyItemAsync(rci);
            }
            LastTransaction = this_transaction;

            FinishAnalysis(targetAnalyzer);
            return this_transaction;
        }
        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpPreset aip) {

            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpPreset aip) {
                if (aip.PluginGuid == PluginGuid) {
                    var presetVm = Items.FirstOrDefault(x => x.Preset.Id == aip.Id);
                    if (presetVm != null) {
                        int presetIdx = Items.IndexOf(presetVm);
                        if (presetIdx >= 0) {
                            Items.RemoveAt(presetIdx);
                            OnPropertyChanged(nameof(Items));
                            OnPropertyChanged(nameof(SelectedItem));
                            OnPropertyChanged(nameof(QuickActionPresetMenuItems));
                        }
                    }
                }
            }
        }

        #endregion
        #endregion

        #region Private Methods


        private void ReceivedClipTrayMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TraySelectionChanged:
                    UpdateCanExecute();
                    break;
            }
        }

        private void PresetViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdatePresetSortOrderAsync().FireAndForgetSafeAsync();
            OnPropertyChanged(nameof(SortedItems));
        }

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        if (SelectedItem == null) {
                            SelectedItem = Items.AggregateOrDefault((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                        }
                        SelectedItem.OnPropertyChanged(nameof(SelectedItem.Items));
                        UpdateCanExecute();
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    break;
                case nameof(SelectedItem):
                    if (SelectedItem != null) {
                        if (!SelectedItem.IsSelected) {
                            SelectedItem.IsSelected = true;
                        }
                    } else {
                        Items.ForEach(x => x.IsSelected = false);
                    }
                    Parent.OnPropertyChanged(nameof(Parent.SelectedPresetViewModel));
                    break;
                case nameof(IsHovering):
                    // hacky way to refresh execute button for some cases...
                    UpdateCanExecute();
                    break;
                case nameof(CanAnalyzerExecute):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.CanDataGridPresetExecute)));
                    break;
            }
        }
        private async Task UpdatePresetSortOrderAsync(bool fromModel = false) {
            if (fromModel) {
                OnPropertyChanged(nameof(SortedItems));
            } else {
                var sil = SortedItems.ToList();
                for (int i = 0; i < sil.Count; i++) {
                    sil[i].SortOrderIdx = i;
                }
                while (sil.Any(x => x.HasModelChanged)) {
                    await Task.Delay(50);
                }
            }
        }
        private async Task<bool> ValidateAnalyzerAsync(MpRuntimePlugin pf) {
            // NOTE validate should all be handled in loader, 
            // but just leaving this for continuity
            await Task.Delay(1);

            return true;
        }

        private async Task<bool> PrepareAnalysisAsync(MpAvAnalyticItemPresetViewModel targetAnalyzer, object args) {
            // returns false if exec params invalid on submit
            bool is_retry = targetAnalyzer.IsExecuting;
            targetAnalyzer.IsExecuting = true;
            if (!targetAnalyzer.ExecuteItems.Any()) {
                return targetAnalyzer.IsAllValid;
            }
            if (is_retry) {
                // retain any validation msgs from plugin
            } else {
                targetAnalyzer.Validate();
            }

            // always show if has non-required params or missing req'd
            bool needs_to_show =
                targetAnalyzer.ExecuteItems.Any(x => !x.IsValid) ||
                !CanExecuteAnalysis(args);

            while (needs_to_show) {
                // show execute params
                var exec_ntf_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                    notificationType: MpNotificationType.ExecuteParametersRequest,
                    title: UiStrings.AnalyzerExecuteParamNtfTitle,
                    body: targetAnalyzer,
                    iconSourceObj: targetAnalyzer.IconId);

                if (exec_ntf_result == MpNotificationDialogResultType.Cancel) {
                    // halt analysis
                    CurrentExecuteArgs = null;
                    targetAnalyzer.IsExecuting = false;
                    //IsBusy = false;
                    return false;
                }

                needs_to_show = !targetAnalyzer.Validate();
                if (!needs_to_show) {
                    targetAnalyzer.SaveCommand.Execute(null);
                }
            }
            return true;
        }
        private void FinishAnalysis(MpAvAnalyticItemPresetViewModel targetAnalyzer, MpAnalyzerTransaction result = null) {
            CurrentExecuteArgs = null;
            if (targetAnalyzer == null) {
                return;
            }
            // clear any overrriden validations and revalidate
            targetAnalyzer.ResetExecutionState();
        }
        private bool CanExecuteAnalysis(object args) {
            CurrentExecuteArgs = args;
            CannotExecuteTooltip = string.Empty;

            MpAvAnalyticItemPresetViewModel exec_aipvm = null;
            MpCopyItem exec_ci = null;
            string exec_action_data = null;
            if (args is MpAvAnalyticItemPresetViewModel) {
                // analyzer request from MpAnalyticItemPresetDataGridView
                // or
                // analyzer request from MpAvClipTrayViewModel.Instance.AnalyzeSelectedItemCommand

                if (MpAvClipTrayViewModel.Instance.SelectedItem != null) {
                    exec_ci = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
                }
                exec_aipvm = args as MpAvAnalyticItemPresetViewModel;
            } else if (args is object[] argParts) {
                // analyzer request from MpAnalyzerActionViewModel

                exec_aipvm = argParts[0] as MpAvAnalyticItemPresetViewModel;
                exec_ci = argParts[1] as MpCopyItem;
                if (exec_ci == null) {
                    exec_action_data = argParts[1] as string;
                }
            } else if (args is int presetId) {
                // analyze by shortcut
                exec_aipvm = Items.FirstOrDefault(x => x.AnalyticItemPresetId == presetId);
                if (MpAvClipTrayViewModel.Instance.SelectedItem != null) {
                    exec_ci = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
                }
            } else if (args == null) {
                // tile selection change
                // hover in preset grid
                if (MpAvClipTrayViewModel.Instance.SelectedItem != null) {
                    exec_ci = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
                }
                exec_aipvm = SelectedItem ?? Items.FirstOrDefault();
            }

            var sb = new StringBuilder();
            if ((exec_ci == null && exec_action_data == null) || exec_aipvm == null) {
                sb.AppendLine(UiStrings.AnalyzerCannotExecuteNoSelectionText);
            }

            bool isOkType = true;
            if (exec_ci != null) {
                isOkType = IsContentTypeValid(exec_ci.ItemType);
            } else {
                isOkType = IsContentTypeValid(MpCopyItemType.Text);
            }


            if (!isOkType && exec_aipvm != null) {
                string accept_text =
                    string.Format(
                        UiStrings.AnalyzerCannotExecuteMessage,
                        exec_aipvm.Label,
                        string.Join(",", InputFormatFlags.All().Select(x => x.EnumToUiString())));
                sb.AppendLine(accept_text);
            }
            if (exec_aipvm != null) {
                exec_aipvm.FormItems.ForEach(x => x.Validate());
                exec_aipvm.FormItems.Where(x => !x.IsValid).ForEach(x => sb.AppendLine(x.ValidationMessage));

            }

            CannotExecuteTooltip = sb.ToString().Trim();

            return string.IsNullOrEmpty(CannotExecuteTooltip);
        }
        #endregion

        #region Commands        

        public MpIAsyncCommand<object> PerformAnalysisCommand => new MpAsyncCommand<object>(
            async (args) => {
                _ = await PerformAnalysisAsync(args);
            }, (args) => CanExecuteAnalysis(args));


        public ICommand SelectPresetCommand => new MpCommand<object>(
             (args) => {
                 MpAvAnalyticItemPresetViewModel preset_to_select = null;
                 if (args is MpAvAnalyticItemPresetViewModel) {
                     preset_to_select = args as MpAvAnalyticItemPresetViewModel;
                 } else if (args is object[] argParts) {
                     // select from tile transaction context menu

                     preset_to_select = argParts[0] as MpAvAnalyticItemPresetViewModel;
                     if (argParts[1] is MpParameterMessageRequestFormat prf) {
                         // apply provided parameter configuration to preset
                         foreach (var ppri in prf.items) {
                             if (preset_to_select.Items.FirstOrDefault(x => x.ParamId.ToString() == ppri.paramId.ToString()) is MpAvParameterViewModelBase pvm) {
                                 pvm.CurrentValue = ppri.paramValue;
                             }
                         }
                     }
                 }
                 SelectedItem = preset_to_select;
                 if (preset_to_select != null) {
                     Parent.SelectedItem = this;
                     MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(Parent);
                 }
             });

        public ICommand ManageAnalyticItemCommand => new MpCommand(
             () => {
                 if (!IsSelected && Parent != null) {
                     Parent.SelectedItem = this;

                 }
                 if (SelectedItem == null && Items.Count > 0) {
                     SelectedItem = Items.AggregateOrDefault((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                 }
                 MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(Parent);
                 OnPropertyChanged(nameof(SelectedItem));

             });

        public ICommand DeletePresetCommand => new MpAsyncCommand<object>(
            async (presetVmArg) => {
                // NOTE delete will never get trnasaction type of parameter so it can be ignored
                IsBusy = true;

                var presetVm = presetVmArg as MpAvAnalyticItemPresetViewModel;
                foreach (var presetVal in presetVm.Items) {
                    await presetVal.ParameterValue.DeleteFromDatabaseAsync();
                }
                await presetVm.Preset.DeleteFromDatabaseAsync();

                IsBusy = false;
            },
            (presetVmArg) => {
                if (presetVmArg is MpAvAnalyticItemPresetViewModel aipvm &&
                     aipvm.CanDelete(null)) {
                    return true;
                } else if (presetVmArg is object[] argParts &&
                        argParts[0] is MpAvAnalyticItemPresetViewModel trans_aipvm) {
                    return trans_aipvm.CanDelete(argParts[1]);
                }
                return false;
            });

        public ICommand ResetPresetCommand => new MpAsyncCommand<object>(
            async (presetVmArg) => {
                IsBusy = true;

                MpAvAnalyticItemPresetViewModel aipvm = null;
                if (presetVmArg is MpAvAnalyticItemPresetViewModel arg_vm) {
                    aipvm = arg_vm;
                } else if (presetVmArg is object[] argParts &&
                        argParts[0] is MpAvAnalyticItemPresetViewModel trans_aipvm) {
                    aipvm = trans_aipvm;
                    if (argParts[1] is MpAnalyzerPluginRequestFormat aprf) {
                        // loop through req settings and set preset to those values
                        foreach (var req_kvp in aprf.items) {
                            if (aipvm.Items.FirstOrDefault(x => x.ParamId == req_kvp.paramId) is MpAvParameterViewModelBase pvm) {
                                pvm.CurrentValue = req_kvp.paramValue;
                            } else {
                                MpConsole.WriteLine($"Param req item id '{req_kvp.paramId}' w/ paramValue '{req_kvp.paramValue}' not found on preset '{aipvm}'");
                            }
                        }
                        IsBusy = false;
                        return;
                    }
                }
                if (aipvm == null) {
                    IsBusy = false;
                    return;
                }

                // get copy of all current values shared/unshared separated
                Dictionary<string, string> shared_params = new Dictionary<string, string>();
                foreach (var sp in aipvm.Items.Where(x => x.IsSharedValue)) {
                    shared_params.Add(sp.ParamId.ToString(), sp.CurrentValue);
                }

                Dictionary<string, string> unshared_params = new Dictionary<string, string>();
                foreach (var sp in aipvm.Items.Where(x => !x.IsSharedValue)) {
                    unshared_params.Add(sp.ParamId.ToString(), sp.CurrentValue);
                }

                MpNotificationDialogResultType share_reset_type = MpNotificationDialogResultType.None;
                if (shared_params.Any()) {
                    // ntf reset all, shared, unshared or cancel
                    share_reset_type = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                        notificationType: MpNotificationType.ModalResetSharedValuePreset,
                        title: UiStrings.CommonConfirmLabel,
                        body: string.Format(UiStrings.NtfResetSharedValueText, aipvm.Label),
                        iconSourceObj: "QuestionMarkImage");
                    if (share_reset_type == MpNotificationDialogResultType.Cancel) {
                        // user canceled
                        IsBusy = false;
                        return;
                    }
                }

                bool retain_shared_vals =
                    share_reset_type == MpNotificationDialogResultType.None ||
                    share_reset_type == MpNotificationDialogResultType.ResetUnshared;
                bool retain_unshared_vals = share_reset_type == MpNotificationDialogResultType.ResetShared;

                // reset presets name, icon, etc.
                var def_preset_model = await MpAvPluginPresetLocator.CreateOrResetManifestPresetModelAsync(aipvm, aipvm.PresetGuid, aipvm.SortOrderIdx);
                // before initializing preset remove current values from db or it won't reset values
                await Task.WhenAll(aipvm.Items.Select(x => x.ParameterValue.DeleteFromDatabaseAsync()));

                // put preset back in default state
                await aipvm.InitializeAsync(def_preset_model);

                Dictionary<string, string> updated_param_vals = new();
                foreach (var param_vm in aipvm.Items) {
                    // set to new default val
                    string updated_value = param_vm.CurrentValue;
                    if (retain_shared_vals &&
                        shared_params.FirstOrDefault(x => x.Key == param_vm.ParamId) is { } shared_kvp &&
                        !shared_kvp.IsDefault()) {
                        // use orignal shared val
                        updated_value = shared_kvp.Value;
                    }
                    if (retain_unshared_vals &&
                        unshared_params.FirstOrDefault(x => x.Key == param_vm.ParamId) is { } unshared_kvp &&
                        !unshared_kvp.IsDefault()) {
                        // use original unshared val
                        updated_value = unshared_kvp.Value;
                    }
                    updated_param_vals.Add(param_vm.ParamId, updated_value);
                }
                foreach (var param_vm in aipvm.Items) {
                    if (updated_param_vals.TryGetValue(param_vm.ParamId, out string new_val)) {
                        param_vm.CurrentValue = new_val;
                        await param_vm.SaveParameterAsync();
                    }
                }
                aipvm.SaveCommand.Execute(null);
                // re-initialize preset so all visible values are right
                var updated_preset = await MpDataModelProvider.GetItemAsync<MpPreset>(aipvm.AnalyticItemPresetId);
                await aipvm.InitializeAsync(updated_preset);
                Items.ForEach(x => x.IsSelected = x.AnalyticItemPresetId == aipvm.AnalyticItemPresetId);
                OnPropertyChanged(nameof(SelectedItem));

                IsBusy = false;
            },
            (presetVmArg) => {
                if (presetVmArg is MpAvAnalyticItemPresetViewModel aipvm &&
                     !aipvm.CanDelete(null)) {
                    return true;
                } else if (presetVmArg is object[] argParts &&
                        argParts[0] is MpAvAnalyticItemPresetViewModel trans_aipvm) {
                    return !trans_aipvm.CanDelete(argParts[1]);
                }
                return false;
            });
        public ICommand ResetOrDeletePresetCommand => new MpCommand<object>(
            (args) => {
                if (ResetPresetCommand.CanExecute(args)) {
                    ResetPresetCommand.Execute(args);
                } else {
                    DeletePresetCommand.Execute(args);
                }
            });
        public ICommand ShiftPresetCommand => new MpCommand<object>(
            // [0] = new_idx [1] = presetvm
            (args) => {
                if (args is object[] argParts &&
                    argParts.Length == 2 &&
                    argParts[0] is int new_idx &&
                    argParts[1] is MpAvAnalyticItemPresetViewModel pvm) {

                    new_idx = Math.Max(0, Math.Min(Items.Count - 1, new_idx));

                    int curSortIdx = Items.IndexOf(pvm);
                    Items.Move(curSortIdx, new_idx);
                    for (int i = 0; i < Items.Count; i++) {
                        Items[i].SortOrderIdx = i;
                    }
                }
            });

        public ICommand CreateNewPresetCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;
                bool isActionPreset = false;
                if (args != null) {
                    isActionPreset = (bool)args;
                }

                var def_icon = await MpDataModelProvider.GetItemAsync<MpIcon>(PluginIconId);
                if (def_icon == null) {
                    def_icon = await MpDataModelProvider.GetItemAsync<MpIcon>(MpDefaultDataModelTools.UnknownIconId);
                }
                var np_icon = await def_icon.CloneDbModelAsync();

                MpPreset newPreset = await MpPreset.CreateOrUpdateAsync(
                        pluginGuid: PluginGuid,
                        isActionPreset: isActionPreset,
                        sortOrderIdx: Items.Count,
                        iconId: np_icon.Id,
                        label: GetUniquePresetName());

                var npvm = await CreatePresetViewModelAsync(newPreset);
                Items.Add(npvm);
                Items.ForEach(x => x.IsSelected = x == npvm);

                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });

        public ICommand DuplicatePresetCommand => new MpAsyncCommand<object>(
                async (args) => {
                    if (args is not MpAvAnalyticItemPresetViewModel aipvm) {
                        return;
                    }
                    IsBusy = true;
                    var p_to_clone = aipvm.Preset;
                    p_to_clone.Label += $" - {UiStrings.CommonCloneLabel}";
                    p_to_clone.SortOrderIdx = Items.Count;
                    p_to_clone.IsModelReadOnly = false;
                    var dp = await aipvm.Preset.CloneDbModelAsync();

                    var dpvm = await CreatePresetViewModelAsync(dp);
                    Items.Add(dpvm);
                    ShiftPresetCommand.Execute(new object[] { aipvm.SortOrderIdx + 1, dpvm });
                    Items.ForEach(x => x.IsSelected = x == dpvm);
                    OnPropertyChanged(nameof(Items));

                    IsBusy = false;
                });

        #endregion
    }
}
