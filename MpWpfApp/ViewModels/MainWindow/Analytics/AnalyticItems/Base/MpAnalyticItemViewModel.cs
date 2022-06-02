using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using System.Net;
using System.Windows.Media;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Newtonsoft.Json;
using System.Web;
using System.Windows;
using MonkeyPaste.Plugin;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Azure.Core;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Data;

namespace MpWpfApp {
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
    public class MpAnalyticItemViewModel : 
        MpSelectorViewModelBase<MpAnalyticItemCollectionViewModel,MpAnalyticItemPresetViewModel>, 
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpITreeItemViewModel, 
        MpIMenuItemViewModel {
        #region Private Variables
        

        #endregion

        #region Properties

        #region View Models

        public MpAnalyticItemPresetViewModel DefaultPresetViewModel => Items.FirstOrDefault(x => x.IsDefault);

        public MpMenuItemViewModel MenuItemViewModel {
            get { 
                var subItems = Items.Select(x => x.MenuItemViewModel).ToList();
                if(subItems.Count > 0) {
                    subItems.Add(new MpMenuItemViewModel() { IsSeparator = true });
                }
                subItems.Add(
                    new MpMenuItemViewModel() {
                        IconResourceKey = Application.Current.Resources["CogIcon"] as string,
                        Header = $"Manage '{Title}'",
                        Command = ManageAnalyticItemCommand,
                        CommandParameter = AnalyzerPluginGuid
                    });
                return new MpMenuItemViewModel() {
                    Header = Title,
                    IconId = IconId,
                    SubItems = subItems
                };
            }
        }

        public IEnumerable<MpMenuItemViewModel> QuickActionPresetMenuItems => Items.Where(x => x.IsQuickAction).Select(x => x.MenuItemViewModel);

        public MpITreeItemViewModel ParentTreeItem => Parent;

        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; } = false;

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Appearance

        public string ManageLabel => $"{Title} Preset Manager";

        public Brush ItemBackgroundBrush {
            get {
                if (IsSelected) {
                    return Brushes.DimGray;
                }
                if (IsHovering) {
                    return Brushes.LightGray;
                }
                return Brushes.Transparent;
            }
        }

        public Brush ItemTitleForegroundBrush {
            get {
                if (IsSelected) {
                    return Brushes.White;
                }
                if (IsHovering) {
                    return Brushes.Black;
                }
                return Brushes.White;
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

        

        public bool IsExpanded { get; set; } = false;

        public MpAnalyzerTransaction LastTransaction { get; private set; } = null;

        public MpCopyItem LastResultContentItem { get; set; } = null;

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

        #region MpAnalyticItem

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

        public int IconId { get; private set; }

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

        public string AnalyzerPluginGuid => PluginFormat == null ? string.Empty : PluginFormat.guid;

        public MpBillableItem BillableItem { get; set; }

        #region Plugin

        public MpPluginFormat PluginFormat { get; set; }

        public MpAnalyzerPluginFormat AnalyzerPluginFormat => PluginFormat == null ? null : PluginFormat.analyzer;

        public MpIAnalyzerComponent AnalyzerPluginComponent => PluginFormat == null ? null : PluginFormat.Component as MpIAnalyzerComponent;
        
        #endregion

        #endregion

        #endregion

        #endregion

        #region Events

        public event EventHandler<MpCopyItem> OnAnalysisCompleted;

        #endregion

        #region Constructors

        public MpAnalyticItemViewModel() : base(null) { }

        public MpAnalyticItemViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemViewModel_PropertyChanged;
            Items.CollectionChanged += PresetViewModels_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPluginFormat analyzerPlugin) {
            if (IsLoaded) {
                return;
            }
            IsBusy = true;

            PluginFormat = analyzerPlugin;
            if (AnalyzerPluginComponent == null) {
                throw new Exception("Cannot find component");
            }

            
            var presets = await MpDataModelProvider.GetAnalyticItemPresetsByAnalyzerGuid(PluginFormat.guid);

            if (string.IsNullOrEmpty(PluginFormat.iconUrl)) {
                IconId = MpPreferences.ThisAppIcon.Id;
            } else if (presets.Count > 0 &&
                presets.FirstOrDefault(x => x.IsDefault) != default &&
                      presets.FirstOrDefault(x => x.IsDefault).IconId > 0) {
                IconId = presets.FirstOrDefault(x => x.IsDefault).IconId;
            } else {
                var bytes = await MpFileIo.ReadBytesFromUriAsync(PluginFormat.iconUrl);
                var icon = await MpIcon.Create(
                    iconImgBase64: bytes.ToBase64String(),
                    createBorder: false);
                IconId = icon.Id;
            }

            bool isNew = presets.Count == 0;
            bool isManifestModified = presets.Any(x => x.ManifestLastModifiedDateTime < PluginFormat.manifestLastModifiedDateTime);
            bool needsReset = isNew || isManifestModified;
            if (needsReset) {
                MpNotificationCollectionViewModel.Instance.ShowUserAction(
                    MpNotificationDialogType.AnalyzerUpdated,
                    MpNotificationExceptionSeverityType.Warning,
                    $"Analyzer '{Title}' Updated",
                    "Reseting presets to default...",
                    3000).FireAndForgetSafeAsync(this);

                presets = await ResetPresets(presets);
                isNew = true;
            }

            presets.ForEach(x => x.AnalyzerFormat = AnalyzerPluginFormat);

            Items.Clear();

            foreach (var preset in presets.OrderBy(x=>x.SortOrderIdx)) {
                var naipvm = await CreatePresetViewModel(preset);
                Items.Add(naipvm);
            }

            OnPropertyChanged(nameof(IconId));
            OnPropertyChanged(nameof(Items));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            MpMessenger.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayMessage);

            IsBusy = false;
        }

        public async Task<MpAnalyticItemPresetViewModel> CreatePresetViewModel(MpAnalyticItemPreset aip) {
            MpAnalyticItemPresetViewModel naipvm = new MpAnalyticItemPresetViewModel(this);
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
            if (e is MpAnalyticItemPreset aip) {
                
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpAnalyticItemPreset aip) {
                if(aip.AnalyzerPluginGuid == AnalyzerPluginGuid) {
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

        private async Task<MpAnalyticItemPreset> CreateDefaultPresetModel(int existingDefaultPresetId = 0) {
            if(AnalyzerPluginFormat.parameters == null) {
                throw new Exception($"Parameters for '{Title}' not found");
            }

            var aip = await MpAnalyticItemPreset.Create(
                                analyzerPluginGuid: PluginFormat.guid,
                                isDefault: true,
                                label: $"{Title} - Default",
                                iconId: IconId,
                                sortOrderIdx: existingDefaultPresetId == 0 ? 0 : Items.FirstOrDefault(x => x.IsDefault).SortOrderIdx,
                                description: $"Auto-generated default preset for '{Title}'",
                                format: AnalyzerPluginFormat,
                                manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime,
                                existingDefaultPresetId: existingDefaultPresetId);

            //var paramPresetValues = new List<MpAnalyzerPresetValueFormat>();
            ////derive default preset & preset values from parameter formats
            //if (AnalyzerPluginFormat.parameters != null) {
            //    foreach (var param in AnalyzerPluginFormat.parameters) {
            //        string defVal = string.Join(",", param.values.Where(x=>x.isDefault).Select(x=>x.value));
            //        var presetVal = await MpAnalyticItemPresetParameterValue.Create(presetId: aip.Id, paramEnumId: param.paramId, value: defVal);

            //        paramPresetValues.Add(presetVal);
            //    }
            //}

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

        private async void PresetViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            await UpdatePresetSortOrder();
        }

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(IsSelected && Parent.IsSidebarVisible) {
                        LastSelectedDateTime = DateTime.Now;

                        if(SelectedItem == null) {
                            SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                        }
                        CollectionViewSource.GetDefaultView(SelectedItem.Items).Refresh();
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

        private async Task UpdatePresetSortOrder(bool fromModel = false) {
            if(fromModel) {
                Items.Sort(x => x.SortOrderIdx);
            } else {
                foreach(var aipvm in Items) {
                    aipvm.SortOrderIdx = Items.IndexOf(aipvm);
                }
                if(!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    foreach (var pvm in Items) {
                        await pvm.Preset.WriteToDatabaseAsync();
                    }
                }
            }
        }

        private async Task<List<MpAnalyticItemPreset>> ResetPresets(List<MpAnalyticItemPreset> presets = null) {
            //if manifest has been modified
            //(for now clear all presets and either load predefined presets or create from parameter default values)

            // TODO maybe less forceably handle add/remove/update of presets when manifest changes
            presets = presets == null ? await MpDataModelProvider.GetAnalyticItemPresetsByAnalyzerGuid(PluginFormat.guid) : presets;
            foreach (var preset in presets) {
                var vals = await MpDataModelProvider.GetAnalyticItemPresetValuesByPresetId(preset.Id);
                await Task.WhenAll(vals.Select(x => x.DeleteFromDatabaseAsync()));
            }
            await Task.WhenAll(presets.Select(x => x.DeleteFromDatabaseAsync()));

            presets.Clear();
            if(AnalyzerPluginFormat.presets.IsNullOrEmpty()) {
                //only generate default preset if no presets defined in manifest
                var defualtPreset = await CreateDefaultPresetModel();
                presets.Add(defualtPreset);
            } else {
                foreach (var preset in AnalyzerPluginFormat.presets) {
                    var aip = await MpAnalyticItemPreset.Create(
                        analyzerPluginGuid: PluginFormat.guid,
                        isDefault: preset.isDefault,
                        label: preset.label,
                        iconId: IconId,
                        sortOrderIdx: AnalyzerPluginFormat.presets.IndexOf(preset),
                        description: preset.description,
                        format: AnalyzerPluginFormat,
                        manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime);
                    presets.Add(aip);
                }
                if(presets.All(x=>x.IsDefault == false) && presets.Count > 0) {
                    presets[0].IsDefault = true;
                }
            }
            return presets;
        }

        #endregion

        #region Commands

        public MpIAsyncCommand<object> ExecuteAnalysisCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;

                MpCopyItem sourceCopyItem = null;
                MpAnalyticItemPresetViewModel targetAnalyzer = null;

                if (args != null && args is object[] argParts) {
                    // when analyzer is triggered from action not user selection 
                    //suppressCreateItem = true;
                    targetAnalyzer = argParts[0] as MpAnalyticItemPresetViewModel;
                    sourceCopyItem = argParts[1] as MpCopyItem;
                } else {
                    if (args is MpAnalyticItemPresetViewModel aipvm) {
                        targetAnalyzer = aipvm;
                    } else {
                        targetAnalyzer = SelectedItem;
                    }                    
                    sourceCopyItem = MpClipTrayViewModel.Instance.SelectedItem.CopyItem;
                }
                Items.ForEach(x => x.IsSelected = x == targetAnalyzer);
                OnPropertyChanged(nameof(SelectedItem));

                object result = await MpPluginTransactor.PerformTransaction(
                                           PluginFormat,
                                           AnalyzerPluginComponent,
                                           SelectedItem.ParamLookup
                                               .ToDictionary(k => k.Key, v => v.Value.CurrentValue),
                                           sourceCopyItem,
                                           SelectedItem.Preset);

                if(result != null && result.ToString() == MpPluginResponseFormat.RETRY_MESSAGE) {
                    ExecuteAnalysisCommand.Execute(args);
                    return;
                }
                if(result != null && result.ToString() == MpPluginResponseFormat.ERROR_MESSAGE) {
                    OnAnalysisCompleted?.Invoke(SelectedItem, LastResultContentItem);
                    IsBusy = false;
                    return;
                }

                LastTransaction = result as MpAnalyzerTransaction;
                OnAnalysisCompleted?.Invoke(SelectedItem, LastResultContentItem);

                IsBusy = false;
            },(args)=>CanExecuteAnalysis(args));

        protected virtual Task<MpAnalyzerTransaction> ExecuteAnalysis(object obj) { return null; }

        public virtual bool CanExecuteAnalysis(object args) {
            if(IsBusy) {
                return false;
            }

            MpAnalyticItemPresetViewModel spvm = null;
            MpCopyItem sci = null;
            if(args == null) {
                spvm = SelectedItem;
                if(MpClipTrayViewModel.Instance.SelectedItem != null) {
                    sci = MpClipTrayViewModel.Instance.SelectedItem.CopyItem;
                }
            } else if(args is MpAnalyticItemPresetViewModel) {
                if (MpClipTrayViewModel.Instance.SelectedItem == null) {
                    return false;
                }
                spvm = args as MpAnalyticItemPresetViewModel;
                sci = MpClipTrayViewModel.Instance.SelectedItem.CopyItem;
            } else if(args is object[] argParts) {
                spvm = argParts[0] as MpAnalyticItemPresetViewModel;
                sci = argParts[1] as MpCopyItem;
            }

            if(sci == null || spvm == null) {
                return false;
            }

            bool isOkType = IsContentTypeValid(sci.ItemType);

            spvm.Items.ForEach(x => x.Validate());
            return spvm.IsAllValid && 
                   isOkType;
        }

        public ICommand CreateNewPresetCommand => new RelayCommand(
            async () => {
                IsBusy = true;

                MpAnalyticItemPreset newPreset = await MpAnalyticItemPreset.Create(
                        analyzerPluginGuid: AnalyzerPluginGuid,
                        format:AnalyzerPluginFormat,
                        iconId: IconId,
                        label: GetUniquePresetName());

                var npvm = await CreatePresetViewModel(newPreset);
                Items.Add(npvm);
                Items.ForEach(x => x.IsSelected = x == npvm);

                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });

        public ICommand SelectPresetCommand => new RelayCommand<MpAnalyticItemPresetViewModel>(
             (selectedPresetVm) => {
                //if(!IsLoaded) {
                //    await LoadChildren();
                //}
                if(!IsSelected) {
                     Parent.SelectedItem = this;
                 }
                 SelectedItem = selectedPresetVm;
            });

        public ICommand ManageAnalyticItemCommand => new RelayCommand(
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

        public ICommand DeletePresetCommand => new RelayCommand<MpAnalyticItemPresetViewModel>(
            async (presetVm) => {
                if(presetVm.IsDefault) {
                    return;
                }
                IsBusy = true;

                foreach(var presetVal in presetVm.Items) {
                    await presetVal.PresetValue.DeleteFromDatabaseAsync();
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

                var defaultPresetModel = await CreateDefaultPresetModel(defvm.AnalyticItemPresetId);

                await defvm.InitializeAsync(defaultPresetModel);

                Items.ForEach(x => x.IsSelected = x.AnalyticItemPresetId == defvm.AnalyticItemPresetId);
                OnPropertyChanged(nameof(SelectedItem));

                IsBusy = false;
            });

        public ICommand ShiftPresetCommand => new RelayCommand<object>(
            // [0] = shift dir [1] = presetvm
            async (args) => {
                var argParts = args as object[];
                int dir = (int)Convert.ToInt32(argParts[0].ToString());
                MpAnalyticItemPresetViewModel pvm = argParts[1] as MpAnalyticItemPresetViewModel;
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
                    MpAnalyticItemPresetViewModel pvm = argParts[1] as MpAnalyticItemPresetViewModel;
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

                    var aipvm = args as MpAnalyticItemPresetViewModel;
                    if(aipvm == null) {
                        throw new Exception("DuplicatedPresetCommand must have preset as argument");
                    }
                    var dp = await aipvm.Preset.CloneDbModel();
                    var dpvm = await CreatePresetViewModel(dp);
                    Items.Add(dpvm);
                    Items.ForEach(x => x.IsSelected = x == dpvm);
                    OnPropertyChanged(nameof(Items));

                    IsBusy = false;
                });
        #endregion
    }
}
