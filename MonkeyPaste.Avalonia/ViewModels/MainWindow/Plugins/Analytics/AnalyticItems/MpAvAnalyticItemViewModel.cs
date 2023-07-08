using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyticItemViewModel :
        MpAvTreeSelectorViewModelBase<MpAvAnalyticItemCollectionViewModel, MpAvAnalyticItemPresetViewModel>,
        MpISelectableViewModel,
        MpIAsyncCollectionObject,
        MpIHoverableViewModel,
        MpIAsyncComboBoxItemViewModel,
        MpIMenuItemViewModel,
        MpIParameterHostViewModel {
        #region Private Variables
        #endregion

        #region Interfaces

        #region MpIParameterHost Implementation

        int MpIParameterHostViewModel.IconId => PluginIconId;
        public string PluginGuid =>
            PluginFormat == null ? string.Empty : PluginFormat.guid;

        public MpPluginFormat PluginFormat { get; set; }

        MpParameterHostBaseFormat MpIParameterHostViewModel.ComponentFormat => AnalyzerComponentFormat;

        MpParameterHostBaseFormat MpIParameterHostViewModel.BackupComponentFormat =>
            PluginFormat == null || PluginFormat.backupCheckPluginFormat == null || PluginFormat.backupCheckPluginFormat.analyzer == null ?
                null : PluginFormat.backupCheckPluginFormat.analyzer;

        public MpAnalyzerPluginFormat AnalyzerComponentFormat =>
            PluginFormat == null ? null : PluginFormat.analyzer;

        public MpIPluginComponentBase PluginComponent =>
            PluginFormat == null ? null : PluginFormat.Component as MpIPluginComponentBase;

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

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                var subItems = Items.Where(x => !x.IsActionPreset).Select(x => x.ContextMenuItemViewModel).ToList();
                if (subItems.Count > 0) {
                    subItems.Add(new MpMenuItemViewModel() { IsSeparator = true });
                }
                subItems.Add(
                    new MpMenuItemViewModel() {
                        IconResourceKey = Mp.Services.PlatformResource.GetResource("CogImage") as string,
                        Header = $"Manage '{Title}'",
                        Command = ManageAnalyticItemCommand,
                        CommandParameter = PluginGuid
                    });
                return new MpMenuItemViewModel() {
                    Header = Title,
                    IconId = PluginIconId,
                    SubItems = subItems
                };
            }
        }

        public IEnumerable<MpMenuItemViewModel> QuickActionPresetMenuItems => Items.Where(x => x.IsQuickAction).Select(x => x.ContextMenuItemViewModel);

        #endregion        

        #region Appearance


        public string CannotExecuteTooltip { get; set; }

        #endregion

        #region State

        public bool IsAnyBusy =>
            Items.Any(x => x.IsAnyBusy) || IsBusy;

        public virtual bool IsLoaded => Items.Count > 0 && Items[0].Items.Count > 0;

        //public bool IsAnyEditingParameters => Items.Any(x => x.IsEditingParameters);

        public bool IsHovering { get; set; } = false;

        public MpAnalyzerTransaction LastTransaction { get; private set; } = null;

        public bool CanAnalyzerExecute { get; set; }

        public object CurrentExecuteArgs { get; set; }
        #endregion

        #region Models

        #region MpBillableItem

        public string ApiName {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.title;
            }
        }

        public DateTime NextPaymentDateTime {
            get {
                if (BillableItem == null) {
                    return DateTime.MaxValue;
                }
                return BillableItem.NextPaymentDateTime;
            }
            set {
                if (NextPaymentDateTime != value) {
                    BillableItem.NextPaymentDateTime = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(NextPaymentDateTime));
                }
            }
        }

        public MpPeriodicCycleType CycleType {
            get {
                if (BillableItem == null) {
                    return MpPeriodicCycleType.None;
                }
                return BillableItem.CycleType;
            }
        }

        public int MaxRequestCountPerCycle {
            get {
                if (BillableItem == null) {
                    return int.MaxValue;
                }
                return BillableItem.MaxRequestCountPerCycle;
            }
        }


        public int MaxRequestByteCount {
            get {
                if (BillableItem == null) {
                    return int.MaxValue;
                }
                return BillableItem.MaxRequestByteCount;
            }
        }

        public int MaxRequestByteCountPerCycle {
            get {
                if (BillableItem == null) {
                    return int.MaxValue;
                }
                return BillableItem.MaxRequestByteCountPerCycle;
            }
        }

        public int MaxResponseBytesPerCycle {
            get {
                if (BillableItem == null) {
                    return int.MaxValue;
                }
                return BillableItem.MaxRequestByteCountPerCycle;
            }
        }

        public int CurrentCycleRequestByteCount {
            get {
                if (BillableItem == null) {
                    return 0;
                }
                return BillableItem.CurrentCycleRequestByteCount;
            }
            set {
                if (BillableItem.CurrentCycleRequestByteCount != value) {
                    BillableItem.CurrentCycleRequestByteCount = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CurrentCycleRequestByteCount));
                }
            }
        }


        public int CurrentCycleResponseByteCount {
            get {
                if (BillableItem == null) {
                    return 0;
                }
                return BillableItem.CurrentCycleResponseByteCount;
            }
            set {
                if (BillableItem.CurrentCycleResponseByteCount != value) {
                    BillableItem.CurrentCycleResponseByteCount = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CurrentCycleResponseByteCount));
                }
            }
        }


        public int CurrentCycleRequestCount {
            get {
                if (BillableItem == null) {
                    return 0;
                }
                return BillableItem.CurrentCycleRequestByteCount;
            }
            set {
                if (BillableItem.CurrentCycleRequestCount != value) {
                    BillableItem.CurrentCycleRequestCount = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CurrentCycleRequestCount));
                }
            }
        }

        #endregion

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


        public MpBillableItem BillableItem { get; set; }


        #endregion

        #endregion

        #endregion

        #region Events

        public event EventHandler<MpCopyItem> OnAnalysisCompleted;

        #endregion

        #region Constructors

        public MpAvAnalyticItemViewModel() : base(null) { }

        public MpAvAnalyticItemViewModel(MpAvAnalyticItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemViewModel_PropertyChanged;
            Items.CollectionChanged += PresetViewModels_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPluginFormat analyzerPlugin) {
            if (!ValidateAnalyzer(analyzerPlugin)) {
                return;
            }
            if (IsLoaded) {
                return;
            }
            IsBusy = true;

            PluginFormat = analyzerPlugin;

            if (PluginComponent == null) {
                throw new Exception("Cannot find component");
            }

            PluginIconId = await MpAvPluginIconLocator.LocatePluginIconIdAsync(this);

            while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            Items.Clear();
            //if(Title == "Yolo") {
            //    MpDebug.Break();
            //}

            // NOTE ignoring plugin update msgs on initial load cause its a bunch and not relevant
            var presets = await MpAvPluginPresetLocator.LocatePresetsAsync(
                this,
                showMessages: !MpPrefViewModel.Instance.IsInitialLoad);

            foreach (var preset in presets) {
                var naipvm = await CreatePresetViewModelAsync(preset);
                Items.Add(naipvm);
            }

            OnPropertyChanged(nameof(Items));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            MpMessenger.Register<MpMessageType>(MpAvClipTrayViewModel.Instance, ReceivedClipTrayMessage);

            IsBusy = false;
        }

        public async Task<MpAvAnalyticItemPresetViewModel> CreatePresetViewModelAsync(MpPluginPreset aip) {
            MpAvAnalyticItemPresetViewModel naipvm = new MpAvAnalyticItemPresetViewModel(this);
            await naipvm.InitializeAsync(aip);
            return naipvm;
        }

        public void UpdateCanExecute() {
            CanAnalyzerExecute = CanExecuteAnalysis(CurrentExecuteArgs);
        }

        public string GetUniquePresetName() {
            int uniqueIdx = 1;
            string uniqueName = $"{Title} Preset";
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

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpPluginPreset aip) {

            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpPluginPreset aip) {
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
            UpdatePresetSortOrder();
        }

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        if (SelectedItem == null) {
                            SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
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
            }
        }
        private void UpdatePresetSortOrder(bool fromModel = false) {
            if (fromModel) {
                Items.Sort(x => x.SortOrderIdx);
            } else {
                foreach (var aipvm in Items) {
                    aipvm.SortOrderIdx = Items.IndexOf(aipvm);
                }
            }
        }
        private bool ValidateAnalyzer(MpPluginFormat pf) {
            if (pf == null) {
                MpConsole.WriteTraceLine("plugin error, not registered");
                return false;
            }

            bool isValid = true;
            var sb = new StringBuilder();

            if (isValid) {
                return true;
            }
            MpConsole.WriteLine(sb.ToString());
            return false;
        }

        #endregion

        #region Commands
        public virtual bool CanExecuteAnalysis(object args) {
            //if (Mp.Services.AccountTools.IsContentAddPausedByAccount) {
            //    MpConsole.WriteLine($"Analyzer '{this}' execute analysis rejected. Account capped");
            //    return false;
            //}

            CurrentExecuteArgs = args;
            //if (IsBusy && (SelectedItem == null || !SelectedItem.IsExecuting)) {
            //    return false;
            //}
            CannotExecuteTooltip = string.Empty;

            MpAvAnalyticItemPresetViewModel spvm = null;
            MpCopyItem sci = null;
            string sstr = null;
            if (args == null) {
                // analyzer request from MpAvClipTrayViewModel.Instance.AnalyzeSelectedItemCommand

                spvm = SelectedItem;
                if (MpAvClipTrayViewModel.Instance.SelectedItem != null) {
                    sci = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
                }
            } else if (args is MpAvAnalyticItemPresetViewModel) {
                // analyzer request from MpAnalyticItemPresetDataGridView

                if (MpAvClipTrayViewModel.Instance.SelectedItem == null) {
                    return false;
                }
                spvm = args as MpAvAnalyticItemPresetViewModel;
                sci = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
            } else if (args is object[] argParts) {
                // analyzer request from MpAnalyzerActionViewModel

                spvm = argParts[0] as MpAvAnalyticItemPresetViewModel;
                sci = argParts[1] as MpCopyItem;
                if (sci == null) {
                    sstr = argParts[1] as string;
                }
            }

            if ((sci == null && sstr == null) || spvm == null) {
                return false;
            }
            bool isOkType = true;
            if (sci != null) {
                isOkType = IsContentTypeValid(sci.ItemType);
            } else {
                isOkType = IsContentTypeValid(MpCopyItemType.Text);
            }

            var sb = new StringBuilder();
            if (!isOkType) {
                sb.AppendLine($"{SelectedItem.FullName} only accepts input of type(s): {InputFormatFlags}");
            }
            spvm.FormItems.ForEach(x => x.Validate());
            spvm.FormItems.Where(x => !x.IsValid).ForEach(x => sb.AppendLine(x.ValidationMessage));

            CannotExecuteTooltip = sb.ToString().Trim();

            return string.IsNullOrEmpty(CannotExecuteTooltip);
        }

        public MpIAsyncCommand<object> ExecuteAnalysisCommand => new MpAsyncCommand<object>(
            async (args) => {

                //IsBusy = true;
                bool suppressWrite = false;

                MpCopyItem sourceCopyItem = null;
                MpAvAnalyticItemPresetViewModel targetAnalyzer = null;
                Func<string> lastOutputCallback = null;

                if (args is object[] argParts) {
                    targetAnalyzer = argParts[0] as MpAvAnalyticItemPresetViewModel;
                    // when analyzer is triggered from action not user selection 
                    if (argParts[1] is string) {
                        suppressWrite = true;
                        //sourceCopyItem = await MpCopyItem.CreateAsync(
                        //                                    data: argParts[1] as string,
                        //                                    suppressWrite: true);
                        sourceCopyItem = await Mp.Services.CopyItemBuilder.BuildAsync(
                            pdo: new MpAvDataObject(MpPortableDataFormats.Text, argParts[1] as string),
                            suppressWrite: true,
                            transType: MpTransactionType.Analyzed,
                            force_allow_dup: true,
                            force_ext_sources: false);
                    } else if (argParts[1] is MpCopyItem) {
                        sourceCopyItem = argParts[1] as MpCopyItem;
                    }

                    if (argParts.Length == 3 &&
                        argParts[2] != null) {
                        lastOutputCallback = argParts[2] as Func<string>;
                    }
                } else {
                    if (args is MpAvAnalyticItemPresetViewModel aipvm) {
                        targetAnalyzer = aipvm;
                    } else {
                        targetAnalyzer = SelectedItem;
                    }
                    sourceCopyItem = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
                }
                Items.ForEach(x => x.IsSelected = x == targetAnalyzer);
                OnPropertyChanged(nameof(SelectedItem));

                SelectedItem.IsExecuting = true;
                if (SelectedItem.ExecuteItems.Any()) {
                    // always show if has non-required params or missing req'd
                    bool needs_to_show =
                        SelectedItem.ExecuteItems.Any(x => x.IsVisible && !x.IsRequired) ||
                        !CanExecuteAnalysis(args);

                    while (needs_to_show) {
                        // show execute params
                        var exec_ntf_result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.ExecuteParametersRequest,
                            title: "Enter Values",
                            body: SelectedItem,
                            iconSourceObj: SelectedItem.IconId);

                        if (exec_ntf_result == MpNotificationDialogResultType.Cancel) {
                            // halt analysis
                            CurrentExecuteArgs = null;
                            SelectedItem.IsExecuting = false;
                            //IsBusy = false;
                            return;
                        }

                        needs_to_show = !CanExecuteAnalysis(args);
                        if (!needs_to_show) {
                            SelectedItem.SaveCommand.Execute(null);
                        }
                    }
                }
                MpAvClipTileViewModel source_ctvm = null;
                if (sourceCopyItem != null) {
                    source_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == sourceCopyItem.Id);
                    if (source_ctvm != null) {
                        source_ctvm.IsBusy = true;
                    }
                }

                MpPluginTransactionBase result = await MpPluginTransactor.PerformTransactionAsync(
                                           PluginFormat,
                                           PluginComponent,
                                           SelectedItem.ParamLookup
                                               .ToDictionary(k => k.Key, v => v.Value.CurrentValue),
                                           sourceCopyItem,
                                           SelectedItem.Preset,
                                           lastOutputCallback,
                                           suppressWrite);
                if (result == null) {
                    CurrentExecuteArgs = null;
                    SelectedItem.IsExecuting = false;
                    //IsBusy = false;
                    return;
                }

                Func<Task<MpAnalyzerPluginResponseFormat>> retryAnalyzerFunc = () => {
                    ExecuteAnalysisCommand.Execute(args);
                    return null;
                };

                result.Response = await MpPluginTransactor.ValidatePluginResponseAsync(
                    result.Request,
                    result.Response,
                    retryAnalyzerFunc);

                if (result is MpAnalyzerTransaction) {
                    LastTransaction = result as MpAnalyzerTransaction;
                    OnAnalysisCompleted?.Invoke(SelectedItem, LastTransaction.ResponseContent);
                }
                if (source_ctvm != null) {
                    source_ctvm.IsBusy = false;
                }
                if (LastTransaction.ResponseContent is MpCopyItem rci) {
                    rci.WasDupOnCreate = rci.Id == sourceCopyItem.Id;
                    MpAvClipTrayViewModel.Instance
                        .AddUpdateOrAppendCopyItemAsync(rci).FireAndForgetSafeAsync(this);
                }

                CurrentExecuteArgs = null;
                SelectedItem.IsExecuting = false;
                //IsBusy = false;
            }, (args) => CanExecuteAnalysis(args));


        public ICommand SelectPresetCommand => new MpCommand<object>(
             (args) => {
                 MpAvAnalyticItemPresetViewModel preset_to_select = null;
                 if (args is MpAvAnalyticItemPresetViewModel) {
                     preset_to_select = args as MpAvAnalyticItemPresetViewModel;
                 } else if (args is object[] argParts) {
                     // select from tile transaction context menu

                     preset_to_select = argParts[0] as MpAvAnalyticItemPresetViewModel;
                     if (argParts[1] is MpPluginParameterRequestFormat prf) {
                         // apply provided parameter configuration to preset
                         foreach (var ppri in prf.items) {
                             if (preset_to_select.Items.FirstOrDefault(x => x.ParamId.ToString() == ppri.paramId.ToString()) is MpAvParameterViewModelBase pvm) {
                                 pvm.CurrentValue = ppri.value;
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
                     SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
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
                    await presetVal.PresetValueModel.DeleteFromDatabaseAsync();
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
                MpConsole.WriteLine("Resetting...");
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
                                pvm.CurrentValue = req_kvp.value;
                            } else {
                                MpConsole.WriteLine($"Param req item id '{req_kvp.paramId}' w/ value '{req_kvp.value}' not found on preset '{aipvm}'");
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

                Dictionary<string, string> shared_params_to_clear_or_restore = new Dictionary<string, string>();
                foreach (var sp in aipvm.Items.Where(x => x.IsSharedValue)) {
                    shared_params_to_clear_or_restore.Add(sp.ParamId.ToString(), sp.CurrentValue);
                }

                if (shared_params_to_clear_or_restore.Any()) {
                    // ntf w/ yes/no/cancel to reset shared values
                    var result = await Mp.Services.PlatformMessageBox.ShowYesNoCancelMessageBoxAsync(
                        title: "Confirm",
                        message: $"'{aipvm.Label}' contains shared values. Would you like to reset those as well?",
                        iconResourceObj: "QuestionMarkImage",
                        owner: MpAvWindowManager.MainWindow);
                    if (result.IsNull()) {
                        // cancel
                        IsBusy = false;
                        return;
                    }
                    if (result.IsTrue()) {
                        // clear shared values
                        foreach (var kvp in shared_params_to_clear_or_restore) {
                            shared_params_to_clear_or_restore[kvp.Key] = string.Empty;
                        }
                    }
                }
                // recreate default preset record (name, icon, etc.)
                var defaultPresetModel = await MpAvPluginPresetLocator.CreateOrResetManifestPresetModelAsync(
                    this, aipvm.PresetGuid, Items.IndexOf(aipvm));

                // before initializing preset remove current values from db or it won't reset values
                await Task.WhenAll(aipvm.Items.Select(x => x.PresetValueModel.DeleteFromDatabaseAsync()));

                await aipvm.InitializeAsync(defaultPresetModel);

                if (shared_params_to_clear_or_restore.Any()) {
                    // update shared values and trigger db write for other instance updates
                    foreach (var shared_param_to_restore in shared_params_to_clear_or_restore) {
                        var cur_param = aipvm.Items.FirstOrDefault(x => x.ParamId.ToString() == shared_param_to_restore.Key);
                        MpDebug.Assert(cur_param != null, $"Can't find shared param '{shared_param_to_restore.Key}'");
                        cur_param.CurrentValue = shared_param_to_restore.Value;
                        cur_param.SaveCurrentValueCommand.Execute(null);
                    }
                }
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

                var np_icon = await def_icon.CloneDbModelAsync();

                MpPluginPreset newPreset = await MpPluginPreset.CreateOrUpdateAsync(
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
                    IsBusy = true;

                    var aipvm = args as MpAvAnalyticItemPresetViewModel;
                    if (aipvm == null) {
                        throw new Exception("DuplicatedPresetCommand must have preset as argument");
                    }
                    var p_to_clone = aipvm.Preset;
                    p_to_clone.Label += " - Clone";
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
