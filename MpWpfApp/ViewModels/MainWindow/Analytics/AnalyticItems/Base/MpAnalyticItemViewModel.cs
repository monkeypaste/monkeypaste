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

namespace MpWpfApp {
    public class MpAnalyticItemViewModel : 
        MpViewModelBase<MpAnalyticItemCollectionViewModel>, 
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpITreeItemViewModel, 
        MpIMenuItemViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemPresetViewModel> PresetViewModels { get; set; } = new ObservableCollection<MpAnalyticItemPresetViewModel>();

        public MpAnalyticItemPresetViewModel DefaultPresetViewModel => PresetViewModels.FirstOrDefault(x => x.IsDefault);

        public MpAnalyticItemPresetViewModel SelectedPresetViewModel => PresetViewModels.FirstOrDefault(x => x.IsSelected);     

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                var subItems = PresetViewModels.Select(x => x.MenuItemViewModel).ToList();
                subItems.Add(
                    new MpMenuItemViewModel() {
                        IconResourceKey = Application.Current.Resources["CogIcon"] as string,
                        Header = $"Manage '{Title}'",
                        Command = Parent.ManageItemCommand,
                        CommandParameter = AnalyzerPluginSudoId
                    });
                return new MpMenuItemViewModel() {
                    Header = Title,
                    IconId = IconId,
                    SubItems = subItems
                };
            }
        }

        public IEnumerable<MpMenuItemViewModel> QuickActionPresetMenuItems => PresetViewModels.Where(x => x.IsQuickAction).Select(x => x.MenuItemViewModel);

        public MpITreeItemViewModel ParentTreeItem => Parent;

        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(PresetViewModels.Cast<MpITreeItemViewModel>());

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
        #endregion

        #region State

        public virtual bool IsLoaded => PresetViewModels.Count > 0 && PresetViewModels[0].ParameterViewModels.Count > 0;

        public bool IsAnyEditingParameters => PresetViewModels.Any(x => x.IsEditingParameters);

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        public MpAnalyzerTransaction LastTransaction { get; private set; } = null;

        public MpCopyItem LastResultContentItem { get; set; } = null;
        #endregion

        #region Models

        #region MpBillableItem

        public string ApiName {
            get {
                if (AnalyticItem == null) {
                    return string.Empty;
                }
                return AnalyticItem.Title;
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
                if (AnalyzerPluginFormat.outputType.box) {
                    flags |= MpAnalyzerOutputFormatFlags.BoundingBox;
                }

                return flags;
            }
        }

        public int IconId {
            get {
                if (AnalyticItem == null) {
                    return 0;
                }
                return AnalyticItem.IconId;
            }
        }

        public string Title {
            get {
                if (AnalyticItem == null) {
                    return string.Empty;
                }
                return AnalyticItem.Title;
            }
            set {
                if (Title != value) {
                    Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Description {
            get {
                if (AnalyticItem == null) {
                    return string.Empty;
                }
                return AnalyticItem.Description;
            }
        }

        public string ParameterFormatResourcePath {
            get {
                if (AnalyticItem == null) {
                    return string.Empty;
                }
                return AnalyticItem.ParameterFormatResourcePath;
            }
        }

        public int AnalyzerPluginSudoId => AnalyticItem == null ? 0 : AnalyticItem.Id;

        public MpBillableItem BillableItem {
            get {
                if (AnalyticItem == null) {
                    return null;
                }
                return AnalyticItem.BillableItem;
            }
        }

        public MpAnalyticItem AnalyticItem { get; private set; }

        #region Plugin

        public MpPluginFormat PluginFormat { get; set; }

        public MpAnalyzerPluginFormat AnalyzerPluginFormat => PluginFormat == null ? null : PluginFormat.analyzer;

        public MpIAnalyzerPluginComponent AnalyzerPluginComponent => PluginFormat == null ? null : PluginFormat.LoadedComponent as MpIAnalyzerPluginComponent;
        
        #endregion

        #endregion

        #region Http
        //public abstract MpHttpResponseBase ResponseObj { get; }

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
            PresetViewModels.CollectionChanged += PresetViewModels_CollectionChanged;
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

            MpIcon icon = null;

            if (!string.IsNullOrEmpty(analyzerPlugin.iconUrl)) {
                var bytes = await MpFileIo.ReadBytesFromUriAsync(analyzerPlugin.iconUrl);
                icon = await MpIcon.Create(bytes.ToBitmapSource().ToBase64String(), false);
            }
            icon = icon == null ? MpPreferences.ThisAppSource.App.Icon : icon;

            AnalyticItem = new MpAnalyticItem() {
                Guid = PluginFormat.guid,
                Title = PluginFormat.title,
                Description = PluginFormat.description,
                EndPoint = AnalyzerPluginFormat.endpoint,
                ApiKey = AnalyzerPluginFormat.apiKey,
                Icon = icon,
                IconId = icon.Id
            };

            AnalyticItem.Presets = await MpDataModelProvider.GetAnalyticItemPresetsByAnalyzerGuid(PluginFormat.guid);
            bool isNew = AnalyticItem.Presets == null || AnalyticItem.Presets.Count == 0;
            AnalyticItem.Id = isNew ? MpDbModelBase.GetUniqueId() : AnalyticItem.Presets[0].AnalyzerPluginSudoId;
                       

            PresetViewModels.Clear();

            if (isNew) {
                //for new plugins create default presets
                foreach(var preset in AnalyzerPluginFormat.presets) {
                    var aip = await MpAnalyticItemPreset.Create(
                        analyzerSudoId: AnalyticItem.Id,
                        isDefault: preset.isDefault,
                        label: preset.label,
                        icon: AnalyticItem.Icon,
                        sortOrderIdx: AnalyzerPluginFormat.presets.IndexOf(preset),
                        description: preset.description,
                        parameters: AnalyzerPluginFormat.parameters);

                    AnalyticItem.Presets.Add(aip);
                }
            } 
            if (AnalyticItem.Presets.All(x => x.IsDefault == false)) {
                //this ensures at least one preset exists and not all can be deleted
                AnalyticItem.Presets[0].IsDefault = true;
            }

            foreach (var preset in AnalyticItem.Presets) {
                var naipvm = await CreatePresetViewModel(preset);
                PresetViewModels.Add(naipvm);
            }
            PresetViewModels.OrderBy(x => x.SortOrderIdx);

            var defPreset = PresetViewModels.FirstOrDefault(x => x.IsDefault);
            MpAssert.Assert(defPreset, $"Error no default preset for anayltic item {AnalyticItem.Title}");


            OnPropertyChanged(nameof(IconId));
            OnPropertyChanged(nameof(PresetViewModels));

            IsBusy = false;
        }

        //public async Task InitializeAsync(MpAnalyticItem ai) {
        //    if (IsLoaded) {
        //        return;
        //    }
        //    IsBusy = true;
        //    AnalyticItem = await MpDb.GetItemAsync<MpAnalyticItem>(ai.Id);

        //    if(AnalyticItem.Icon == null) {
        //        var url = await MpUrlBuilder.Create(AnalyticItem.EndPoint, null);
        //        AnalyticItem.Icon = url.Icon;
        //        AnalyticItem.IconId = url.IconId;
        //        await AnalyticItem.WriteToDatabaseAsync();
        //        OnPropertyChanged(nameof(IconId));
        //    }
        //    // Init Presets
        //    PresetViewModels.Clear();

        //    if(AnalyticItem.Presets.Count == 0) {
        //        var naipvm = await CreatePresetViewModel(null);
        //        PresetViewModels.Add(naipvm);
        //    } else {
        //        foreach (var preset in AnalyticItem.Presets.OrderBy(x => x.SortOrderIdx)) {
        //            var naipvm = await CreatePresetViewModel(preset);
        //            PresetViewModels.Add(naipvm);
        //        }
        //    }            

        //    PresetViewModels.OrderBy(x => x.SortOrderIdx);

        //    var defPreset = PresetViewModels.FirstOrDefault(x => x.IsDefault);
        //    MpAssert.Assert(defPreset, $"Error no default preset for anayltic item {AnalyticItem.Title}");


        //    OnPropertyChanged(nameof(IconId));
        //    OnPropertyChanged(nameof(PresetViewModels));

        //    IsBusy = false;
        //}

        public async Task<MpAnalyticItemPresetViewModel> CreatePresetViewModel(MpAnalyticItemPreset aip) {
            MpAnalyticItemPresetViewModel naipvm = new MpAnalyticItemPresetViewModel(this);
            await naipvm.InitializeAsync(aip);
            return naipvm;
        }

        //public async Task<MpAnalyticItemPresetViewModel> CreatePresetViewModel(MpAnalyzerPresetFormat aip) {
        //    MpAnalyticItemPresetViewModel naipvm = new MpAnalyticItemPresetViewModel(this);
        //    await naipvm.InitializeAsync(aip);
        //    return naipvm;
        //}

        public string GetUniquePresetName() {
            int uniqueIdx = 1;
            string uniqueName = $"Preset";
            string testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);

            while(PresetViewModels.Any(x => x.Label.ToLower() == testName)) {
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
            if(SelectedPresetViewModel == null) {
                return true;
            }
            return SelectedPresetViewModel.IsAllValid;
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
                if(aip.AnalyzerPluginSudoId == AnalyzerPluginSudoId) {
                    var presetVm = PresetViewModels.FirstOrDefault(x => x.Preset.Id == aip.Id);
                    if(presetVm != null) {
                        int presetIdx = PresetViewModels.IndexOf(presetVm);
                        if(presetIdx >= 0) {
                            PresetViewModels.RemoveAt(presetIdx);
                            OnPropertyChanged(nameof(PresetViewModels));
                            OnPropertyChanged(nameof(SelectedPresetViewModel));
                            //OnPropertyChanged(nameof(ContextMenuItems));
                            OnPropertyChanged(nameof(QuickActionPresetMenuItems));
                        }
                    }
                }
            }
        }

        #endregion

        protected virtual async Task<MpCopyItem> ApplyAnalysisToContent(
            MpCopyItem sourceCopyItem, 
            MpAnalyzerTransaction trans, 
            bool suppressWrite) {
            object request = trans.Request;
            object response = trans.Response;
            MpCopyItem targetCopyItem = null;

            if (response is List<MpIImageDescriptorBox> idbl) {
                var diol = idbl.Cast<MpDetectedImageObject>().ToList();
                diol.ForEach(x => x.CopyItemId = sourceCopyItem.Id);
                if(!suppressWrite) {
                    await Task.WhenAll(diol.Select(x => x.WriteToDatabaseAsync()));
                    targetCopyItem = sourceCopyItem;
                }
                
            }
            
            List<MpITextDescriptorRange> tdrl = new List<MpITextDescriptorRange>();
            if(response is MpITextDescriptorRange tdr) {
                tdrl.Add(tdr);
            } else if(response is List<MpITextDescriptorRange> temp) {
                tdrl = temp;
            }
            if(tdrl.Count > 0) {
                var app = MpPreferences.ThisAppSource.App;
                var url = await MpUrlBuilder.Create(AnalyticItem.EndPoint, request.ToString());
                var source = await MpSource.Create(app, url);

                targetCopyItem = await MpCopyItem.Create(
                    source: source,
                    data: response.ToString(),
                    itemType: MpCopyItemType.Text,
                    suppressWrite: suppressWrite);

                
            }

            if (suppressWrite == false && targetCopyItem != null) {
                //create is suppressed when its part of a match expression
                if(sourceCopyItem.Id != targetCopyItem.Id) {
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
                if (scivm == null) {
                    //analysis content is  linked with visible item in tray
                    await scivm.Parent.InitializeAsync(scivm.Parent.HeadItem.CopyItem, scivm.Parent.QueryOffsetIdx);

                    MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                }
            }


            return targetCopyItem;
        }

        protected virtual async Task TransformContent() {

        }

        protected virtual async Task AppendContent() {

        }

        #endregion

        #region Private Methods

        private async void PresetViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            await UpdatePresetSortOrder();
        }

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsExpanded):
                    if(IsExpanded) {
                        //MpHelpers.RunOnMainThread(async () => {
                        //    await LoadChildren();
                        //});
                    }
                    break;
                case nameof(IsSelected):
                    //if (IsSelected) {
                    //    Parent.Items.ForEach(x => x.IsSelected = x.AnalyticItemId == AnalyticItemId);
                    //}
                    //if(IsSelected && Parent.SelectedItemIdx != Parent.Items.IndexOf(this)) {
                    //    Parent.SelectedItemIdx = Parent.Items.IndexOf(this);
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
                case nameof(SelectedPresetViewModel):
                    //if(SelectedPresetViewModel != null) {
                    //    SelectedPresetViewModel.OnPropertyChanged(nameof(SelectedPresetViewModel.IsEditing));
                    //}
                    break;
                case nameof(IsAnyEditingParameters):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingParameters));
                    break;
            }
        }


        private async Task UpdatePresetSortOrder(bool fromModel = false) {
            if(fromModel) {
                PresetViewModels.Sort(x => x.SortOrderIdx);
            } else {
                foreach(var aipvm in PresetViewModels) {
                    aipvm.SortOrderIdx = PresetViewModels.IndexOf(aipvm);
                }
                if(!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    foreach (var pvm in PresetViewModels) {
                        await pvm.Preset.WriteToDatabaseAsync();
                    }
                }
            }
        }

        private string CreateRequest(MpCopyItem ci) {
            var requestItems = new List<MpAnalyzerPluginRequestItemFormat>();

            foreach (var kvp in SelectedPresetViewModel.ParamLookup) {
                MpAnalyzerPluginRequestItemFormat requestItem = new MpAnalyzerPluginRequestItemFormat();

                var paramFormat = AnalyzerPluginFormat.parameters.FirstOrDefault(x => x.enumId == kvp.Key);
                if (paramFormat == null) {
                    continue;
                }
                if (paramFormat.parameterControlType == MpAnalyticItemParameterControlType.Hidden) {
                    // TODO (maybe)need to implement a request format so other properties can be passed
                    
                    requestItem = new MpAnalyzerPluginRequestItemFormat() {
                        enumId = kvp.Key,
                        value = ci.ToString()
                    };
                } else {
                    requestItem = new MpAnalyzerPluginRequestItemFormat() {
                        enumId = kvp.Key,
                        value = kvp.Value.CurrentValue
                    };
                }
                requestItems.Add(requestItem);
            }

            return JsonConvert.SerializeObject(requestItems);
        }

        private async Task<object> GetResponse(string requestStr) {
            var resultObj = await AnalyzerPluginComponent.AnalyzeAsync(requestStr);

            if (resultObj == null) {
                Debugger.Break();
                return null;
            }


            if (AnalyzerPluginFormat.outputType.box) {
                var boxes = JsonConvert.DeserializeObject<List<MpAnalyzerPluginBoxResponseValueFormat>>
                                (resultObj.ToString()).Cast<MpIImageDescriptorBox>().ToList();
                return boxes;
            }
            return null;
        }

        #endregion

        #region Commands

        public ICommand ExecuteAnalysisCommand => new RelayCommand<object>(
            async (args) => {
                bool suppressCreateItem = false;

                IsBusy = true;

                MpCopyItem sourceCopyItem = null;
                MpAnalyticItemPresetViewModel targetAnalyzer = null;

                if (args != null && args is object[] argParts) {
                    // when analyzer trigger from action not user selection
                    suppressCreateItem = true;
                    targetAnalyzer = argParts[0] as MpAnalyticItemPresetViewModel;
                    sourceCopyItem = argParts[1] as MpCopyItem;
                } else {
                    if (args is MpAnalyticItemPresetViewModel aipvm) {
                        targetAnalyzer = aipvm;
                    } else {
                        targetAnalyzer = SelectedPresetViewModel;
                    }                    
                    sourceCopyItem = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem;
                }

                PresetViewModels.ForEach(x => x.IsSelected = x == targetAnalyzer);
                OnPropertyChanged(nameof(SelectedPresetViewModel));

                MpAnalyzerTransaction transaction = new MpAnalyzerTransaction();
                string requestStr = CreateRequest(sourceCopyItem);
                object responseData = await GetResponse(requestStr);

                LastTransaction = new MpAnalyzerTransaction() {
                    Request = requestStr,
                    Response = responseData
                };

                LastResultContentItem = await ApplyAnalysisToContent(sourceCopyItem, LastTransaction, suppressCreateItem);

                OnAnalysisCompleted?.Invoke(SelectedPresetViewModel, LastResultContentItem);

                IsBusy = false;
            },(args)=>CanExecuteAnalysis(args));

        protected virtual Task<MpAnalyzerTransaction> ExecuteAnalysis(object obj) { return null; }

        public virtual bool CanExecuteAnalysis(object args) {
            MpAnalyticItemPresetViewModel spvm = null;
            MpCopyItem sci = null;
            if(args == null) {
                spvm = SelectedPresetViewModel;
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

            bool isOkType = false;
            switch(sci.ItemType) {
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

            return spvm != null &&
                   spvm.IsAllValid && 
                   sci != null &&
                   isOkType;
        }

        public ICommand CreateNewPresetCommand => new RelayCommand(
            async () => {
                MpAnalyticItemPreset newPreset = await MpAnalyticItemPreset.Create(
                        analyzerSudoId: AnalyzerPluginSudoId,
                        label: GetUniquePresetName());


                var npvm = await CreatePresetViewModel(newPreset);
                PresetViewModels.Add(npvm);
                PresetViewModels.ForEach(x => x.IsSelected = x == npvm);

                OnPropertyChanged(nameof(PresetViewModels));
            });

        public ICommand SelectPresetCommand => new RelayCommand<MpAnalyticItemPresetViewModel>(
             (selectedPresetVm) => {
                //if(!IsLoaded) {
                //    await LoadChildren();
                //}
                PresetViewModels.ForEach(x => x.IsSelected = false);
                selectedPresetVm.IsSelected = true;
            });

        public ICommand ManageAnalyticItemCommand => new RelayCommand(
             () => {
                 if (!IsSelected) {
                     Parent.Items.ForEach(x => x.IsSelected = x.AnalyzerPluginSudoId == AnalyzerPluginSudoId);
                 }
                 if (SelectedPresetViewModel == null && PresetViewModels.Count > 0) {
                     PresetViewModels.ForEach(x => x.IsSelected = x == PresetViewModels[0]);
                 }
                 if(!Parent.IsSidebarVisible) {
                     Parent.IsSidebarVisible = true;
                 }
                 OnPropertyChanged(nameof(SelectedPresetViewModel));
             });

        public ICommand DeletePresetCommand => new RelayCommand<MpAnalyticItemPresetViewModel>(
            async (presetVm) => {
                if(presetVm.IsDefault) {
                    return;
                }
                foreach(var presetVal in presetVm.Preset.PresetParameterValues) {
                    await presetVal.DeleteFromDatabaseAsync();
                }
                await presetVm.Preset.DeleteFromDatabaseAsync();
            },
            (presetVm) => presetVm != null && 
            presetVm is MpAnalyticItemPresetViewModel aipsvm);

        public ICommand ShiftPresetCommand => new RelayCommand<object>(
            // [0] = shift dir [1] = presetvm
            async (args) => {
                var argParts = args as object[];
                int dir = (int)Convert.ToInt32(argParts[0].ToString());
                MpAnalyticItemPresetViewModel pvm = argParts[1] as MpAnalyticItemPresetViewModel;
                int curSortIdx = PresetViewModels.IndexOf(pvm);
                int newSortIdx = curSortIdx + dir;

                PresetViewModels.Move(curSortIdx, newSortIdx);
                for (int i = 0; i < PresetViewModels.Count; i++) {
                    PresetViewModels[i].SortOrderIdx = i;
                    await PresetViewModels[i].Preset.WriteToDatabaseAsync();
                }
            },
            (args) => {
                if (args == null) {
                    return false;
                }
                if(args is object[] argParts) {
                    int dir = (int)Convert.ToInt32(argParts[0].ToString());
                    MpAnalyticItemPresetViewModel pvm = argParts[1] as MpAnalyticItemPresetViewModel;
                    int curSortIdx = PresetViewModels.IndexOf(pvm);
                    int newSortIdx = curSortIdx + dir;
                    if(newSortIdx < 0 || newSortIdx >= PresetViewModels.Count || newSortIdx == curSortIdx) {
                        return false;
                    }
                    return true;
                }
                return false;
            });

        #endregion
    }
}
