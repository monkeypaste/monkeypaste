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
        
        private const int _ANALYZE_TIMEOUT_MS = 10000;
        private const int _PROCESS_TIMEOUT_MS = 5000;

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

        public bool IsSelected { get; set; } = false;

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

        public MpIAnalyzerPluginComponent AnalyzerPluginComponent => PluginFormat == null ? null : PluginFormat.Component as MpIAnalyzerPluginComponent;
        
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

            if(string.IsNullOrEmpty(PluginFormat.iconUrl)) {
                IconId = MpPreferences.ThisAppIcon.Id;
            } else {
                var bytes = await MpFileIoHelpers.ReadBytesFromUriAsync(PluginFormat.iconUrl);
                var icon = await MpIcon.Create(
                    iconImgBase64: bytes.ToBase64String(),
                    createBorder: false);
                IconId = icon.Id;
            }
            
            var presets = await MpDataModelProvider.GetAnalyticItemPresetsByAnalyzerGuid(PluginFormat.guid);
            
            bool isNew = presets == null || presets.Count == 0;

            if (!isNew && presets.Any(x => x.ManifestLastModifiedDateTime < PluginFormat.manifestLastModifiedDateTime)) {
                //if manifest has been modified
                
                // TODO maybe less forceably handle add/remove/update of presets when manifest changes

                foreach (var preset in presets) {
                    var vals = await MpDataModelProvider.GetAnalyticItemPresetValuesByPresetId(preset.Id);
                    await Task.WhenAll(vals.Select(x => x.DeleteFromDatabaseAsync()));
                }
                await Task.WhenAll(presets.Select(x => x.DeleteFromDatabaseAsync()));
                presets = new List<MpAnalyticItemPreset>();
                AnalyzerPluginFormat.presets = new List<MpAnalyzerPresetFormat>();
                isNew = true;
            }

            Items.Clear();

            if (isNew) {
                //for new plugins create default presets
                if (AnalyzerPluginFormat.presets == null || AnalyzerPluginFormat.presets.Count == 0) {
                    AnalyzerPluginFormat.presets = new List<MpAnalyzerPresetFormat>();

                    //var paramPresetValues = new List<MpAnalyzerPresetValueFormat>();
                    ////derive default preset & preset values from parameter formats
                    //if (AnalyzerPluginFormat.parameters != null) {
                    //    foreach (var param in AnalyzerPluginFormat.parameters) {
                    //        string defVal = string.Empty;

                    //        if (param.values != null) {
                    //            defVal = string.Join(",", param.values.Where(x => x.isDefault).Select(x => x.value).ToList());
                    //            if (string.IsNullOrEmpty(defVal) && param.values.Count > 0) {
                    //                defVal = param.values[0].value;
                    //            }
                    //        }
                    //        var presetVal = new MpAnalyzerPresetValueFormat() {
                    //            enumId = param.enumId,
                    //            value = defVal
                    //        };
                    //        paramPresetValues.Add(presetVal);
                    //    }
                    //}
                    //MpAnalyzerPresetFormat apf = new MpAnalyzerPresetFormat() {
                    //    description = $"Auto-generated default preset for '{Title}'",
                    //    isDefault = true,
                    //    label = $"{Title} - Default",
                    //    values = paramPresetValues
                    //};
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
                            parameters: AnalyzerPluginFormat.parameters,
                            values: preset.values,
                            manifestLastModifiedDateTime: PluginFormat.manifestLastModifiedDateTime);

                        presets.Add(aip);
                    }
                }
                
            } 

            if (presets.All(x => x.IsDefault == false)) {
                //this ensures at least one preset exists and not all can be deleted
                presets[0].IsDefault = true;
            }

            foreach (var preset in presets) {
                var naipvm = await CreatePresetViewModel(preset);
                Items.Add(naipvm);
            }
            Items.OrderBy(x => x.SortOrderIdx);

            var defPreset = Items.FirstOrDefault(x => x.IsDefault);
            MpAssert.Assert(defPreset, $"Error no default preset for anayltic item {Title}");

            OnPropertyChanged(nameof(IconId));
            OnPropertyChanged(nameof(Items));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            MpMessenger.Register(MpClipTrayViewModel.Instance, ReceivedClipTrayMessage);

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

        public virtual async Task<MpAnalyticItemParameterFormat> DeferredCreateParameterModel(MpAnalyticItemParameterFormat aip) {
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
                            //OnPropertyChanged(nameof(ContextMenuItems));
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
            var paramPresetValues = new List<MpAnalyzerPresetValueFormat>();
            //derive default preset & preset values from parameter formats
            if (AnalyzerPluginFormat.parameters != null) {
                foreach (var param in AnalyzerPluginFormat.parameters) {
                    string defVal = string.Empty;

                    if (param.values != null) {
                        defVal = string.Join(",", param.values.Where(x => x.isDefault).Select(x => x.value).ToList());
                        if (string.IsNullOrEmpty(defVal) && param.values.Count > 0) {
                            defVal = param.values[0].value;
                        }
                    }
                    var presetVal = new MpAnalyzerPresetValueFormat() {
                        enumId = param.enumId,
                        value = defVal
                    };
                    paramPresetValues.Add(presetVal);
                }
            }

            var aip = await MpAnalyticItemPreset.Create(
                                analyzerPluginGuid: PluginFormat.guid,
                                isDefault: true,
                                label: $"{Title} - Default",
                                iconId: IconId,
                                sortOrderIdx: existingDefaultPresetId == 0 ? 0 : Items.FirstOrDefault(x=>x.IsDefault).SortOrderIdx,
                                description: $"Auto-generated default preset for '{Title}'",
                                parameters: AnalyzerPluginFormat.parameters,
                                values: paramPresetValues,
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

        private async void PresetViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            await UpdatePresetSortOrder();
        }

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(IsSelected) {
                        if(SelectedItem == null) {
                            SelectedItem = Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
                        }
                        //Items.ForEach(x => x.IsEditingParameters = false);
                        //SelectedItem.IsEditingParameters = true;
                    } 
                    //else {
                    //    Items.ForEach(x => x.IsEditingParameters = false);
                    //}
                    
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
                    Parent.OnPropertyChanged(nameof(Parent.SelectedPresetViewModel));
                    break;
                //case nameof(IsAnyEditingParameters):
                //    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingParameters));
                //    break;
            }
        }

        private async Task<MpCopyItem> ProcessTransaction(
            MpAnalyzerTransaction trans, 
            MpCopyItem sourceContent, 
            bool suppressWrite = false) {                    
            
            if(trans.Response == null) {
                trans.Response = new MpPluginResponseFormat();
            }

            if(trans.Response is MpPluginResponseFormat prf) {                
                if (prf.message == MpPluginResponseFormat.RETRY_MESSAGE) {
                    ExecuteAnalysisCommand.Execute(trans.CommandParameter);
                    //return null to ignore and retry will loop or continue;
                    return null;
                }
                MpCopyItemTransactionType transType = MpCopyItemTransactionType.None;
                MpISourceTransaction transModel = null;
                if (AnalyzerPluginFormat.http != null) {
                    string urlPath;
                    if (string.IsNullOrEmpty(trans.Request.ToString())) {
                        urlPath = AnalyzerPluginFormat.http.request.url.raw;
                    } else if (PluginFormat.Component is MpHttpPlugin httpPlugin) {
                        urlPath = httpPlugin.GetRequestUri(trans.Request.ToString());
                    } else {
                        throw new MpUserNotifiedException("Http Plugin Component does not exist");
                    }
                    transType = MpCopyItemTransactionType.Http;

                    transModel = await MpHttpTransaction.Create(
                        presetId: SelectedItem.AnalyticItemPresetId,
                        url: urlPath,
                        urlName: SelectedItem.FullName,
                        ip: MpNetworkHelpers.GetExternalIp4Address(),
                        timeSent: trans.RequestTime,
                        timeReceived: trans.ResponseTime,
                        bytesSent: trans.Request.ByteCount(),
                        bytesReceived: trans.Response == null ? 0 : trans.Response.ByteCount(),
                        errorMsg: trans.TransactionErrorMessage);

                } else {
                    var pf = MpPluginManager.Plugins.FirstOrDefault(x => x.Value.guid == PluginFormat.guid);
                    if (!string.IsNullOrWhiteSpace(pf.Key)) {
                        string manifestPath = pf.Key;
                        string pluginDir = Path.GetDirectoryName(manifestPath);
                        string pluginName = Path.GetFileName(pluginDir);
                        string processPath = Path.Combine(pluginDir, pluginName + ".exe");

                        if(PluginFormat.ioType.isCli) {
                            transType = MpCopyItemTransactionType.Cli;

                            transModel = await MpCliTransaction.Create(
                                presetId: SelectedItem.AnalyticItemPresetId,
                                cliPath: pf.Value.ComponentPath,
                                cliName: SelectedItem.FullName,
                                workingDirectory: pluginDir,
                                args: trans.Request.ToString(),
                                transDateTime: trans.RequestTime,
                                errorMsg: trans.TransactionErrorMessage);
                        } else if(PluginFormat.ioType.isDll) {
                            transType = MpCopyItemTransactionType.Dll;
                            processPath = processPath.Replace(".exe", ".dll");

                            transModel = await MpDllTransaction.Create(
                                presetId: SelectedItem.AnalyticItemPresetId,
                                dllPath: processPath,
                                dllName: SelectedItem.FullName,
                                args: trans.Request.ToString(),
                                transDateTime: trans.RequestTime,
                                errorMsg: trans.TransactionErrorMessage);
                        } else {
                            throw new MpUserNotifiedException($"Uknown ioType for plugin defined in '{manifestPath}'");
                        }
                    }
                } 

                if(transModel == null) {
                    throw new Exception("Unknown error processing analyzer transaction");
                }                

                if(string.IsNullOrEmpty(trans.TransactionErrorMessage)) {
                    var cit = await MpCopyItemTransaction.Create(
                    transType: transType,
                    transObjId: transModel.RootId,
                    copyItemId: sourceContent.Id,
                    responseJson: JsonConvert.SerializeObject(trans.Response));

                    var source = await MpSource.Create(
                        copyItemTransactionId: cit.Id);

                    var ci = await ApplyResponseToContent(
                        trans, sourceContent, source.Id, suppressWrite);

                    return ci;
                }
            }
            return null;
        }

        private async Task<MpCopyItem> ApplyResponseToContent(
            MpAnalyzerTransaction trans,
            MpCopyItem sourceCopyItem,
            int transSourceId,
            bool suppressWrite) {
            MpCopyItem targetCopyItem = null;

            if (trans.Response is MpPluginResponseFormat prf) {
                targetCopyItem = await ProcessNewContentItem(prf, sourceCopyItem, transSourceId, suppressWrite);                
            } else {
                targetCopyItem = sourceCopyItem;
            }
            await ProcessAnnotations(trans, targetCopyItem, transSourceId, suppressWrite);

            if (suppressWrite == false && targetCopyItem != null) {
                //create is suppressed when its part of a match expression
                if (sourceCopyItem.Id != targetCopyItem.Id) {
                    var pci = await MpDb.GetItemAsync<MpCopyItem>(sourceCopyItem.Id);

                    int parentSortOrderIdx = pci.CompositeSortOrderIdx;
                    List<MpCopyItem> ppccil = null;

                    if (pci.CompositeParentCopyItemId > 0) {
                        //when this items parent is a composite child, adjust fk/sort so theres single parent
                        var ppci = await MpDb.GetItemAsync<MpCopyItem>(pci.CompositeParentCopyItemId);
                        ppccil = await MpDataModelProvider.GetCompositeChildrenAsync(pci.CompositeParentCopyItemId);
                        ppccil.Insert(0, ppci);
                    } else {
                        ppccil = await MpDataModelProvider.GetCompositeChildrenAsync(pci.Id);
                        ppccil.Insert(0, pci);
                    }
                    ppccil = ppccil.OrderBy(x => x.CompositeSortOrderIdx).ToList();
                    for (int i = 0; i < ppccil.Count; i++) {
                        var cci = ppccil[i];
                        if (cci.Id == sourceCopyItem.Id) {
                            targetCopyItem.CompositeParentCopyItemId = sourceCopyItem.Id;
                            targetCopyItem.CompositeSortOrderIdx = i + 1;
                            await targetCopyItem.WriteToDatabaseAsync();
                        } else if (i > parentSortOrderIdx) {
                            ppccil[i].CompositeSortOrderIdx += 1;
                            await ppccil[i].WriteToDatabaseAsync();
                        }
                    }
                }

                var scivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(sourceCopyItem.Id);
                if (scivm != null) {
                    await MpHelpers.RunOnMainThreadAsync(async () => {
                        //analysis content is  linked with visible item in tray
                        await scivm.Parent.InitializeAsync(scivm.Parent.HeadItem.CopyItem, scivm.Parent.QueryOffsetIdx);
                        MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                    });
                }
            }

            if (targetCopyItem == null) {
                //this should only occur during an action sequece
                targetCopyItem = sourceCopyItem;
            }

            return targetCopyItem;
        }

        private async Task<MpCopyItem> ProcessNewContentItem(
            MpPluginResponseFormat prf, 
            MpCopyItem sourceCopyItem, 
            int transSourceId, 
            bool suppressWrite = false) {
            if(prf == null || prf.newContentItem == null) {
                return sourceCopyItem;
            }
            var app = MpPreferences.ThisAppSource.App;
            string endpoint = AnalyzerPluginFormat.http != null ? AnalyzerPluginFormat.http.request.url.raw : null;
            MpUrl url = null;
            if (!string.IsNullOrEmpty(endpoint)) {
                url = await MpUrlBuilder.Create(endpoint, $"{Title} Analysis");
            }
            var source = await MpSource.Create(app, url);

            var targetCopyItem = await MpCopyItem.Create(
                source: source,
                title: prf.newContentItem.content.value,
                data:prf.newContentItem.content.value,
                itemType: MpCopyItemType.Text,
                suppressWrite: suppressWrite);

            return targetCopyItem;
        }

        private async Task ProcessAnnotations(MpAnalyzerTransaction trans, MpCopyItem sourceCopyItem, int transSourceId, bool suppressWrite = false) {
            if (trans == null || trans.Response == null) {
                return;
            }
            if(trans.Response is MpPluginResponseFormat prf && prf.annotations != null) {

                await Task.WhenAll(prf.annotations.Select(x => ProcesseAnnotation(x, sourceCopyItem.Id, sourceCopyItem.ItemType, trans.RequestContent, transSourceId, suppressWrite)));
            }
            
        }

        private async Task ProcesseAnnotation(
            MpPluginResponseAnnotationFormat a, 
            int copyItemId, 
            MpCopyItemType copyItemType, 
            object reqContent, 
            int transSourceId, 
            bool suppressWrite = false) {
            if (a == null) {
                return;
            }

            double score = 1;

            if (a.score != null) {
                score = a.score.value;
                if(a.minScore != 0 || a.maxScore != 1) {
                    //normalize scoring from 0-1
                    score = (a.maxScore - a.minScore) / a.score.value;
                }                
            }

            string label = a.text == null ? string.Empty : a.text.value;

            if(copyItemType == MpCopyItemType.Image && a.box != null) {
                Size boxSize = new Size();
                if(a.box == null) {
                    var bmpSrc = reqContent.ToString().ToBitmapSource();
                    boxSize = new Size(bmpSrc.PixelWidth, bmpSrc.PixelHeight);
                } else {
                    boxSize.Width = a.box.width.value;
                    boxSize.Height = a.box.height.value;
                }

                MpImageAnnotation contentBox = await MpImageAnnotation.Create(
                   cid: copyItemId,
                   x: a.box == null ? 0 : a.box.x.value,
                   y: a.box == null ? 0 : a.box.y.value,
                   w: boxSize.Width,
                   h: boxSize.Height,
                   label: label,
                   c: score,
                   hexColor: a.appearance == null || a.appearance.foregroundColor == null ? null : a.appearance.foregroundColor.value,
                   suppressWrite: suppressWrite);
            } else if(copyItemType == MpCopyItemType.Text) {                
                int sIdx = 0;
                int eIdx = reqContent.ToString().Length;

                if (a.range != null &&
                    a.range.rangeStart.value >= 0 && 
                    a.range.rangeStart.value < reqContent.ToString().Length &&
                    a.range.rangeStart.value + a.range.rangeLength.value >= 0 && 
                    a.range.rangeStart.value + a.range.rangeLength.value < reqContent.ToString().Length) {
                    // NOTE these range checks are more to cover up issue w/ reqContent not always having data...
                    sIdx = a.range.rangeStart.value; 
                    eIdx = a.range.rangeLength.value;
                }
                var ta = await MpTextAnnotation.Create(
                        copyItemId: copyItemId,
                        sourceId: transSourceId,
                        matchValue: reqContent.ToString().Substring(sIdx, eIdx - sIdx - 1),
                        label: label,
                        score: score,
                        suppressWrite: suppressWrite);
            }
            
            if (a.children != null) {
                await Task.WhenAll(a.children.Select(x => ProcesseAnnotation(x, copyItemId,copyItemType, reqContent,transSourceId, suppressWrite)));
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

        private string CreateRequest(MpCopyItem ci, out object requestContent) {
            object outRequestContent = string.Empty;

            var requestItems = new List<MpAnalyzerPluginRequestItemFormat>();

            foreach (var kvp in SelectedItem.ParamLookup) {
                MpAnalyzerPluginRequestItemFormat requestItem = new MpAnalyzerPluginRequestItemFormat();

                var paramFormat = AnalyzerPluginFormat.parameters.FirstOrDefault(x => x.enumId == kvp.Key);
                if (paramFormat == null) {
                    continue;
                }

                requestItem.enumId = kvp.Key;
                requestItem.value = GetParameterRequestValue(
                    paramFormat.parameterValueType,
                    paramFormat.isMultiSelect,
                    kvp.Value.CurrentValue,
                    ci);

                if(string.IsNullOrEmpty(outRequestContent.ToString()) && 
                   paramFormat.parameterValueType == MpAnalyticItemParameterValueUnitType.ContentQuery) {
                    outRequestContent = requestItem.value;
                }
                
                requestItems.Add(requestItem);
            }
            requestContent = outRequestContent;
            return JsonConvert.SerializeObject(requestItems);
        }

        private string GetParameterRequestValue(MpAnalyticItemParameterValueUnitType valueType, bool isMultiSelect, string curVal, MpCopyItem ci) {
            switch (valueType) {
                case MpAnalyticItemParameterValueUnitType.ContentQuery:
                    curVal = GetParameterQueryResult(isMultiSelect, curVal, ci);
                    break;
                case MpAnalyticItemParameterValueUnitType.Base64Text:
                    curVal = curVal.ToByteArray().ToBase64String();
                    break;
                case MpAnalyticItemParameterValueUnitType.FileSystemPath:
                    curVal = curVal.ToFile();
                    break;
                default:
                    curVal = curVal.ToPlainText();
                    break;
            }
            return curVal;
        }

        private string GetParameterQueryResult(bool isMultiSelect, string curVal, MpCopyItem ci) {
            for (int i = 1; i < Enum.GetNames(typeof(MpComparePropertyPathType)).Length; i++) {
                // example content query: '{Title} is a story about {ItemData}'
                var ppt = (MpComparePropertyPathType)i;
                string pptPathEnumName = ppt.ToString();
                string pptToken = "{" + pptPathEnumName + "}";

                if (curVal.Contains(pptToken)) {
                    string physicalPropertyPath = MpCompareActionViewModelBase.PhysicalComparePropertyPaths[i];
                    string contentValue = ci.GetPropertyValue(physicalPropertyPath) as string;
                    contentValue = GetParameterRequestValue(MpAnalyticItemParameterValueUnitType.PlainText, false, contentValue, ci);
                    string pptTokenBackup = "{@" + ppt.ToString() + "@}";
                    if (curVal.Contains(pptTokenBackup)) {
                        //this content query token has conflicts so use the backup
                        // example content query needing backup: '{Title} is {Title} but {@Title@} is content'
                        pptToken = pptTokenBackup;
                    }
                    curVal = curVal.Replace(pptToken, contentValue);
                }
            }
            return curVal;
        }

        private async Task<object> GetResponse(string requestStr) {
            var result = await AnalyzerPluginComponent.AnalyzeAsync(requestStr);
            return result;
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
                    sourceCopyItem = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem;
                }
                Items.ForEach(x => x.IsSelected = x == targetAnalyzer);
                OnPropertyChanged(nameof(SelectedItem));                      
                LastTransaction = new MpAnalyzerTransaction() {
                    RequestTime = DateTime.Now,
                    CommandParameter = args
                };

                // CREATE REQUEST
                try {
                    LastTransaction.Request = CreateRequest(sourceCopyItem, out object requestContent);
                    LastTransaction.RequestContent = requestContent;
                }catch(Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    LastTransaction.TransactionErrorMessage = ex.ToString();

                    LastResultContentItem = await ProcessTransaction(LastTransaction, sourceCopyItem)
                                    .TimeoutAfter(TimeSpan.FromMilliseconds(_PROCESS_TIMEOUT_MS));

                    var userAction = await MpNotificationCollectionViewModel.Instance.ShowUserAction(
                        dialogType: MpNotificationDialogType.InvalidRequest,
                        exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
                        msg: ex.Message,
                        maxShowTimeMs: 5000);
                    if (userAction == MpDialogResultType.Retry) {
                        ExecuteAnalysisCommand.Execute(args);
                        return;
                    } else if (userAction == MpDialogResultType.Ignore) {
                        OnAnalysisCompleted?.Invoke(SelectedItem, LastResultContentItem);
                        IsBusy = false;
                        return;
                    }
                }

                // GET RESPONSE
                try {
                    LastTransaction.Response = await GetResponse(LastTransaction.Request.ToString())
                                .TimeoutAfter(TimeSpan.FromMilliseconds(_ANALYZE_TIMEOUT_MS));
                    LastTransaction.ResponseTime = DateTime.Now;

                } catch(TimeoutException te) {
                    MpConsole.WriteTraceLine(te);
                    LastTransaction.TransactionErrorMessage = te.ToString();

                    LastResultContentItem = await ProcessTransaction(LastTransaction, sourceCopyItem)
                                    .TimeoutAfter(TimeSpan.FromMilliseconds(_PROCESS_TIMEOUT_MS));

                    var userAction = await MpNotificationCollectionViewModel.Instance.ShowUserAction(
                        dialogType: MpNotificationDialogType.AnalyzerTimeout,
                        exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
                        msg: targetAnalyzer.FullName + " failed to respond",
                        maxShowTimeMs: 5000);
                    if(userAction == MpDialogResultType.Retry) {
                        ExecuteAnalysisCommand.Execute(args);
                        return;
                    } else if(userAction == MpDialogResultType.Ignore) {
                        OnAnalysisCompleted?.Invoke(SelectedItem, LastResultContentItem);
                        IsBusy = false;
                        return;
                    }
                }

                // PROCESS RESPONSE
                try {
                    LastResultContentItem = await ProcessTransaction(LastTransaction, sourceCopyItem)
                                    .TimeoutAfter(TimeSpan.FromMilliseconds(_PROCESS_TIMEOUT_MS));
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    LastTransaction.TransactionErrorMessage = ex.ToString();

                    try {

                        LastResultContentItem = await ProcessTransaction(LastTransaction, sourceCopyItem)
                                        .TimeoutAfter(TimeSpan.FromMilliseconds(_PROCESS_TIMEOUT_MS));
                    } catch(Exception ex2) {
                        LastTransaction.TransactionErrorMessage += Environment.NewLine + ex2.ToString();

                        var userAction = await MpNotificationCollectionViewModel.Instance.ShowUserAction(
                        dialogType: MpNotificationDialogType.InvalidResponse,
                        exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
                        msg: ex.Message + Environment.NewLine + ex2.Message,
                        maxShowTimeMs: 5000);
                        if (userAction == MpDialogResultType.Retry) {
                            ExecuteAnalysisCommand.Execute(args);
                            return;
                        } else if (userAction == MpDialogResultType.Ignore) {
                            OnAnalysisCompleted?.Invoke(SelectedItem, LastResultContentItem);
                            IsBusy = false;
                            return;
                        }
                    }
                }
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
                if(MpClipTrayViewModel.Instance.PrimaryItem != null && 
                   MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem != null) {
                    sci = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem;
                }
            } else if(args is MpAnalyticItemPresetViewModel) {
                if (MpClipTrayViewModel.Instance.PrimaryItem == null || 
                    MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem == null) {
                    return false;
                }
                spvm = args as MpAnalyticItemPresetViewModel;
                sci = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem;
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
                        iconId: IconId,
                        parameters: AnalyzerPluginFormat.parameters,
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

                foreach(var presetVal in presetVm.Preset.PresetParameterValues) {
                    await presetVal.DeleteFromDatabaseAsync();
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
