using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using System.Net;

using System.Windows.Input;

using Newtonsoft.Json;
using System.Web;
using System.Windows;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;


namespace MonkeyPaste.Avalonia {
    [Flags]
    public enum MpAnalyzerInputFormatFlags {
        None = 0,
        Text = 1,
        Image = 2,
        File = 4
    }

    [Flags]
    public enum MpAnalyzerOutputFormatFlags {
        None = 0,
        Text = 1,
        Image = 2,
        BoundingBox = 4,
        File = 8
    }
    public class MpAvAnalyticItemViewModel : 
        MpAvSelectorViewModelBase<MpAvAnalyticItemCollectionViewModel,MpAvAnalyticItemPresetViewModel>, 
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpITreeItemViewModel, 
        MpIMenuItemViewModel {
        #region Private Variables


        #endregion

        #region Properties

        #region MpITreeItemViewModel Implementation

        public IEnumerable<MpITreeItemViewModel> Children => Items;

        public MpITreeItemViewModel ParentTreeItem => Parent;
        public bool IsExpanded { get; set; }
        #endregion

        #region View Models

        public MpAvAnalyticItemPresetViewModel DefaultPresetViewModel => Items.FirstOrDefault(x => x.IsDefault);

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get { 
                var subItems = Items.Where(x=>!x.IsActionPreset).Select(x => x.ContextMenuItemViewModel).ToList();
                if(subItems.Count > 0) {
                    subItems.Add(new MpMenuItemViewModel() { IsSeparator = true });
                }
                subItems.Add(
                    new MpMenuItemViewModel() {
                        IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("CogImage") as string,
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

        //public MpITreeItemViewModel ParentTreeItem => Parent;

        //public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; } = false;

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Appearance

        public string ManageLabel => $"{Title} Preset Manager";

        public string ItemBackgroundBrush {
            get {
                if (IsSelected) {
                    return MpSystemColors.dimgray;
                }
                if (IsHovering) {
                    return MpSystemColors.LightGray;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string ItemTitleForegroundBrush {
            get {
                if (IsSelected) {
                    return MpSystemColors.White;
                }
                if (IsHovering) {
                    return MpSystemColors.Black;
                }
                return MpSystemColors.White;
            }
        }

        public string CannotExecuteTooltip {
            get {
                if(CanAnalyzerExecute) {
                    return string.Empty;
                }

                string outStr = string.Empty;
                if(SelectedItem != null) {
                    outStr = SelectedItem.FullName + " only accepts input of type(s): ";
                }

                if(InputFormatFlags.HasFlag(MpAnalyzerInputFormatFlags.File)) {
                    outStr += "Files,";
                }
                if (InputFormatFlags.HasFlag(MpAnalyzerInputFormatFlags.Image)) {
                    outStr += "Image,";
                }
                if (InputFormatFlags.HasFlag(MpAnalyzerInputFormatFlags.Text)) {
                    outStr += "Text,";
                }

                return outStr.Substring(0, outStr.Length - 1);
            }
        }

        #endregion

        #region State

        public virtual bool IsLoaded => Items.Count > 0 && Items[0].Items.Count > 0;

        //public bool IsAnyEditingParameters => Items.Any(x => x.IsEditingParameters);

        public bool IsHovering { get; set; } = false;


        public MpAnalyzerTransaction LastTransaction { get; private set; } = null;

        public bool CanAnalyzerExecute => CanExecuteAnalysis(null);


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
                if(AnalyzerPluginFormat == null || AnalyzerPluginFormat.inputType == null) {
                    return flags;
                }
                if(AnalyzerPluginFormat.inputType.text) {
                    flags |= MpAnalyzerInputFormatFlags.Text;
                }
                if(AnalyzerPluginFormat.inputType.image) {
                    flags |= MpAnalyzerInputFormatFlags.Image;
                }
                if(AnalyzerPluginFormat.inputType.file) {
                    flags |= MpAnalyzerInputFormatFlags.File;
                }

                return flags;
            }
        }

        public MpAnalyzerOutputFormatFlags OutputFormatFlags {
            get {
                MpAnalyzerOutputFormatFlags flags = MpAnalyzerOutputFormatFlags.None;
                if (AnalyzerPluginFormat == null || AnalyzerPluginFormat.outputType == null) {
                    return flags;
                }
                if (AnalyzerPluginFormat.outputType.text) {
                    flags |= MpAnalyzerOutputFormatFlags.Text;
                }
                if (AnalyzerPluginFormat.outputType.image) {
                    flags |= MpAnalyzerOutputFormatFlags.Image;
                }
                if (AnalyzerPluginFormat.outputType.file) {
                    flags |= MpAnalyzerOutputFormatFlags.File;
                }
                if (AnalyzerPluginFormat.outputType.imageToken) {
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

        public string PluginGuid => PluginFormat == null ? string.Empty : PluginFormat.guid;

        public MpBillableItem BillableItem { get; set; }

        #region Plugin

        public MpPluginFormat PluginFormat { get; set; }

        public MpAnalyzerPluginFormat AnalyzerPluginFormat => PluginFormat == null ? null : PluginFormat.analyzer;

        public MpIPluginComponentBase AnalyzerPluginComponent => PluginFormat == null ? null : PluginFormat.Component as MpIPluginComponentBase;
        
        #endregion

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
            if(!ValidateAnalyzer(analyzerPlugin)) {
                return;
            }
            if (IsLoaded) {
                return;
            }
            IsBusy = true;

            PluginFormat = analyzerPlugin;

            if (AnalyzerPluginComponent == null) {
                throw new Exception("Cannot find component");
            }

            PluginIconId = await GetOrCreateIconIdAsync();

            while(MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }
                       
            Items.Clear();

            var presets = await PreparePresetModelsAsync();
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

        

        public string GetUniquePresetName() {
            int uniqueIdx = 1;
            string uniqueName = $"Preset";
            string testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);

            while(Items.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
            }
            return uniqueName + uniqueIdx;
        }

        public virtual async Task<MpPluginParameterFormat> DeferredCreateParameterModel(MpPluginParameterFormat aip) {
            //used to load remote content and called from CreateParameterViewModel in preset
            await Task.Delay(1);
            return aip;
        }

        public virtual bool Validate() {
            if(SelectedItem == null) {
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
            if(e is MpPluginPreset aip) {
                if(aip.PluginGuid == PluginGuid) {
                    var presetVm = Items.FirstOrDefault(x => x.Preset.Id == aip.Id);
                    if(presetVm != null) {
                        int presetIdx = Items.IndexOf(presetVm);
                        if(presetIdx >= 0) {
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

        

        protected virtual async Task TransformContent() {
            await Task.Delay(1);
        }

        protected virtual async Task AppendContent() {
            await Task.Delay(1);
        }

        #endregion

        #region Private Methods

        private async Task<MpPluginPreset> CreateDefaultPresetModelAsync(int existingDefaultPresetId = 0) {
            if(AnalyzerPluginFormat.parameters == null) {
                throw new Exception($"Parameters for '{Title}' not found");
            }

            var aip = await MpPluginPreset.CreateAsync(
                                pluginGuid: PluginFormat.guid,
                                isDefault: true,
                                label: $"{Title} - Default",
                                iconId: PluginIconId,
                                sortOrderIdx: existingDefaultPresetId == 0 ? 0 : Items.FirstOrDefault(x => x.IsDefault).SortOrderIdx,
                                description: $"Auto-generated default preset for '{Title}'",
                                //format: AnalyzerPluginFormat,
                                manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime,
                                existingDefaultPresetId: existingDefaultPresetId);

            return aip;
        }

        private void ReceivedClipTrayMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.TraySelectionChanged:
                    OnPropertyChanged(nameof(CanAnalyzerExecute));
                    OnPropertyChanged(nameof(CannotExecuteTooltip));
                    break;
            }
        }

        private void PresetViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdatePresetSortOrder();
        }

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(IsSelected && Parent.IsSidebarVisible) {
                        LastSelectedDateTime = DateTime.Now;

                        if(SelectedItem == null) {
                            SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                        }
                        //CollectionViewSource.GetDefaultView(SelectedItem.Items).Refresh();
                        SelectedItem.OnPropertyChanged(nameof(SelectedItem.Items));

                        //Items.ForEach(x => x.IsEditingParameters = false);
                        //SelectedIt em.IsEditingParameters = true;
                    }                    
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    OnPropertyChanged(nameof(ItemBackgroundBrush));
                    OnPropertyChanged(nameof(ItemTitleForegroundBrush));

                    break;
                case nameof(IsHovering):
                    OnPropertyChanged(nameof(ItemBackgroundBrush));
                    OnPropertyChanged(nameof(ItemTitleForegroundBrush));
                    break;
                case nameof(SelectedItem):
                    if(SelectedItem != null) {
                        if(!SelectedItem.IsSelected) {
                            SelectedItem.IsSelected = true;
                        }                        
                    } else {
                        Items.ForEach(x => x.IsSelected = false);
                    }
                    Parent.OnPropertyChanged(nameof(Parent.SelectedPresetViewModel));
                    break;
            }
        }
        private void UpdatePresetSortOrder(bool fromModel = false) {
            if(fromModel) {
                Items.Sort(x => x.SortOrderIdx);
            } else {
                foreach(var aipvm in Items) {
                    aipvm.SortOrderIdx = Items.IndexOf(aipvm);
                }
            }
        }

        private async Task<int> GetOrCreateIconIdAsync() {
            var bytes = await MpFileIo.ReadBytesFromUriAsync(PluginFormat.iconUri, PluginFormat.RootDirectory); ;
            var icon = await MpIcon.Create(
                iconImgBase64: bytes.ToBase64String(),
                createBorder: false);

            return icon.Id;
        }

        private async Task<IEnumerable<MpPluginPreset>> PreparePresetModelsAsync() {
            var presets = await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(PluginFormat.guid);

            bool isNew = presets.Count == 0;
            bool isManifestModified = presets.Any(x => x.ManifestLastModifiedDateTime < PluginFormat.manifestLastModifiedDateTime);
            bool needsReset = isNew || isManifestModified;
            if (needsReset) {
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == PluginIconId);

                MpNotificationBuilder.ShowMessageAsync(
                    msgType: MpNotificationType.PluginUpdated,
                    title: $"Analyzer '{Title}' Updated",
                    iconSourceStr: ivm.IconBase64,
                    body: "Reseting presets to default...")
                    .FireAndForgetSafeAsync(this);

                presets = await ResetPresetsAsync(presets);
                isNew = true;
            }

            //presets.ForEach(x => x.ComponentFormat = AnalyzerPluginFormat);
            return presets.OrderBy(x=>x.SortOrderIdx);
        }

        private async Task<List<MpPluginPreset>> ResetPresetsAsync(List<MpPluginPreset> presets = null) {
            //if manifest has been modified
            //(for now clear all presets and either load predefined presets or create from parameter default values)

            // TODO maybe less forceably handle add/remove/update of presets when manifest changes
            presets = presets == null ? await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(PluginFormat.guid) : presets;
            foreach (var preset in presets) {
                var vals = await MpDataModelProvider.GetPluginPresetValuesByPresetIdAsync(preset.Id);
                await Task.WhenAll(vals.Select(x => x.DeleteFromDatabaseAsync()));
            }
            await Task.WhenAll(presets.Select(x => x.DeleteFromDatabaseAsync()));

            presets.Clear();
            if(AnalyzerPluginFormat.presets.IsNullOrEmpty()) {
                //only generate default preset if no presets defined in manifest
                var defualtPreset = await CreateDefaultPresetModelAsync();
                presets.Add(defualtPreset);
            } else {
                //when presets are defined in manifest create the preset and its values in the db
                foreach (var presetFormat in AnalyzerPluginFormat.presets) {
                    var presetModel = await MpPluginPreset.CreateAsync(
                        pluginGuid: PluginFormat.guid,
                        isDefault: presetFormat.isDefault,
                        label: presetFormat.label,
                        iconId: PluginIconId,
                        sortOrderIdx: AnalyzerPluginFormat.presets.IndexOf(presetFormat),
                        description: presetFormat.description,
                        //format: AnalyzerPluginFormat,
                        manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime);

                    foreach(var presetValueModel in presetFormat.values) {
                        // only creat preset values in db, they will then be picked up when the preset vm is initialized
                        var aipv = await MpPluginPresetParameterValue.CreateAsync(
                            presetId: presetModel.Id, 
                            paramName: presetValueModel.paramName,
                            value: presetValueModel.value
                            //format: AnalyzerPluginFormat.parameters.FirstOrDefault(x => x.paramName == presetValueModel.paramName)
                            );                        
                    }

                    presets.Add(presetModel);
                }
                if(presets.All(x=>x.IsDefault == false) && presets.Count > 0) {
                    presets[0].IsDefault = true;
                }
            }
            return presets;
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

        public MpIAsyncCommand<object> ExecuteAnalysisCommand => new MpAsyncCommand<object>(
            async (args) => {

                IsBusy = true;
                bool suppressWrite = false;

                MpCopyItem sourceCopyItem = null;
                MpAvAnalyticItemPresetViewModel targetAnalyzer = null;
                bool isUserExecutedAnalysis = true;

                if (args != null && args is object[] argParts) {
                    isUserExecutedAnalysis = false;
                    // when analyzer is triggered from action not user selection 
                    //suppressCreateItem = true;
                    targetAnalyzer = argParts[0] as MpAvAnalyticItemPresetViewModel;
                    if (argParts[1] is string) {
                        suppressWrite = true;
                        sourceCopyItem = await MpCopyItem.Create(
                                                            data: argParts[1] as string,
                                                            suppressWrite: true);
                    } else if (argParts[1] is MpCopyItem) {
                        sourceCopyItem = argParts[1] as MpCopyItem;
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

                MpPluginTransactionBase result = await MpPluginTransactor.PerformTransaction(
                                           PluginFormat,
                                           AnalyzerPluginComponent,
                                           SelectedItem.ParamLookup
                                               .ToDictionary(k => k.Key, v => v.Value.CurrentValue),
                                           sourceCopyItem,
                                           SelectedItem.Preset,
                                           suppressWrite);
                if (result == null) {
                    IsBusy = false;
                    return;
                }
                if (!string.IsNullOrEmpty(result.TransactionErrorMessage)) {
                    if (!string.IsNullOrEmpty(result.Response.retryMessage)) {
                        MpConsole.WriteTraceLine("Retrying " + result.Response.retryMessage);
                        ExecuteAnalysisCommand.Execute(args);
                        return;
                    } else if (!string.IsNullOrEmpty(result.Response.errorMessage)) {
                        OnAnalysisCompleted?.Invoke(SelectedItem, null);
                        IsBusy = false;
                        return;
                    } else {
                        throw new Exception("Unhandled transaction response: " + result.TransactionErrorMessage);
                    }
                }

                if (result is MpAnalyzerTransaction) {
                    LastTransaction = result as MpAnalyzerTransaction;
                    //OnAnalysisCompleted?.Invoke(SelectedItem, LastTransaction.ResponseContent);

                    if(isUserExecutedAnalysis) {

                    }
                }
                                

                IsBusy = false;
            },(args)=>CanExecuteAnalysis(args));

        public virtual bool CanExecuteAnalysis(object args) {
            if(IsBusy) {
                return false;
            }

            MpAvAnalyticItemPresetViewModel spvm = null;
            MpCopyItem sci = null;
            string sstr = null;
            if(args == null) {
                // analyzer request from MpAvClipTrayViewModel.Instance.AnalyzeSelectedItemCommand

                spvm = SelectedItem;
                if(MpAvClipTrayViewModel.Instance.SelectedItem != null) {
                    sci = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
                }
            } else if(args is MpAvAnalyticItemPresetViewModel) {
                // analyzer request from MpAnalyticItemPresetDataGridView

                if (MpAvClipTrayViewModel.Instance.SelectedItem == null) {
                    return false;
                }
                spvm = args as MpAvAnalyticItemPresetViewModel;
                sci = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem;
            } else if(args is object[] argParts) {
                // analyzer request from MpAnalyzerActionViewModel
                
                spvm = argParts[0] as MpAvAnalyticItemPresetViewModel;
                sci = argParts[1] as MpCopyItem;
                if(sci == null) {
                    sstr = argParts[1] as string;
                }
            }

            if((sci == null && sstr == null) || spvm == null) {
                return false;
            }
            bool isOkType = true;
            if(sci != null) {
                isOkType = IsContentTypeValid(sci.ItemType);
            } else {
                isOkType = IsContentTypeValid(MpCopyItemType.Text);
            }

            spvm.Items.ForEach(x => x.Validate());
            return spvm.IsAllValid && 
                   isOkType;
        }

        public ICommand CreateNewPresetCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;
                bool isActionPreset = false;
                if(args != null) {
                    isActionPreset = (bool)args;
                }

                MpPluginPreset newPreset = await MpPluginPreset.CreateAsync(
                        pluginGuid: PluginGuid,
                        //format:AnalyzerPluginFormat,
                        isActionPreset: isActionPreset,
                        iconId: PluginIconId,
                        label: GetUniquePresetName());

                var npvm = await CreatePresetViewModelAsync(newPreset);
                Items.Add(npvm);
                Items.ForEach(x => x.IsSelected = x == npvm);

                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });

        public ICommand SelectPresetCommand => new MpCommand<MpAvAnalyticItemPresetViewModel>(
             (selectedPresetVm) => {
                //if(!IsLoaded) {
                //    await LoadChildren();
                //}
                if(!IsSelected) {
                     Parent.SelectedItem = this;
                 }
                 SelectedItem = selectedPresetVm;
            });

        public ICommand ManageAnalyticItemCommand => new MpCommand(
             () => {
                 if (!IsSelected) {
                     Parent.SelectedItem = this;
                 }
                 if (SelectedItem == null && Items.Count > 0) {
                     SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                 }
                 if(!Parent.IsSidebarVisible) {
                     Parent.IsSidebarVisible = true;
                 }
                 OnPropertyChanged(nameof(SelectedItem));

             });

        public ICommand DeletePresetCommand => new MpAsyncCommand<MpAvAnalyticItemPresetViewModel>(
            async (presetVm) => {
                if(presetVm.IsDefault) {
                    return;
                }
                IsBusy = true;

                foreach(var presetVal in presetVm.Items) {
                    await presetVal.PresetValueModel.DeleteFromDatabaseAsync();
                }
                await presetVm.Preset.DeleteFromDatabaseAsync();

                IsBusy = false;
            });

        public MpIAsyncCommand ResetDefaultPresetCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;

                var defvm = Items.FirstOrDefault(x => x.IsDefault);
                if(defvm == null) {
                    throw new Exception("Analyzer is supposed to have a default preset");
                }
                                
                // recreate default preset record (name, icon, etc.)
                var defaultPresetModel = await CreateDefaultPresetModelAsync(defvm.AnalyticItemPresetId);

                // before initializing preset remove current values from db or it won't reset values
                await Task.WhenAll(defvm.Items.Select(x => x.PresetValueModel.DeleteFromDatabaseAsync()));

                await defvm.InitializeAsync(defaultPresetModel);

                Items.ForEach(x => x.IsSelected = x.AnalyticItemPresetId == defvm.AnalyticItemPresetId);
                OnPropertyChanged(nameof(SelectedItem));

                IsBusy = false;
            });

        public ICommand ShiftPresetCommand => new MpAsyncCommand<object>(
            // [0] = shift dir [1] = presetvm
            async (args) => {
                var argParts = args as object[];
                int dir = (int)Convert.ToInt32(argParts[0].ToString());
                MpAvAnalyticItemPresetViewModel pvm = argParts[1] as MpAvAnalyticItemPresetViewModel;
                int curSortIdx = Items.IndexOf(pvm);
                int newSortIdx = curSortIdx + dir;

                Items.Move(curSortIdx, newSortIdx);
                for (int i = 0; i < Items.Count; i++) {
                    Items[i].SortOrderIdx = i;
                    await Items[i].Preset.WriteToDatabaseAsync();
                }
            },
            (args) => {
                if (args == null) {
                    return false;
                }
                if(args is object[] argParts) {
                    int dir = (int)Convert.ToInt32(argParts[0].ToString());
                    MpAvAnalyticItemPresetViewModel pvm = argParts[1] as MpAvAnalyticItemPresetViewModel;
                    int curSortIdx = Items.IndexOf(pvm);
                    int newSortIdx = curSortIdx + dir;
                    if(newSortIdx < 0 || newSortIdx >= Items.Count || newSortIdx == curSortIdx) {
                        return false;
                    }
                    return true;
                }
                return false;
            });


            public MpIAsyncCommand<object> DuplicatePresetCommand => new MpAsyncCommand<object>(
                async (args) => {
                    IsBusy = true;

                    var aipvm = args as MpAvAnalyticItemPresetViewModel;
                    if(aipvm == null) {
                        throw new Exception("DuplicatedPresetCommand must have preset as argument");
                    }
                    var dp = await aipvm.Preset.CloneDbModelAsync(
                        deepClone: true,
                        suppressWrite: false);

                    var dpvm = await CreatePresetViewModelAsync(dp);
                    Items.Add(dpvm);
                    Items.ForEach(x => x.IsSelected = x == dpvm);
                    OnPropertyChanged(nameof(Items));

                    IsBusy = false;
                });
        #endregion
    }
}
