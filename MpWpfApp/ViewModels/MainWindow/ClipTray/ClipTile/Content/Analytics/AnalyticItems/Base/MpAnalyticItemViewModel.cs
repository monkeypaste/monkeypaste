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
using MonkeyPaste;
using System.Web;

namespace MpWpfApp {
    public abstract class MpAnalyticItemViewModel : MpViewModelBase<MpAnalyticItemCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemPresetViewModel> PresetViewModels { get; set; } = new ObservableCollection<MpAnalyticItemPresetViewModel>();

        public MpAnalyticItemPresetViewModel SelectedPresetViewModel => PresetViewModels.FirstOrDefault(x => x.IsSelected);     

        public ObservableCollection<MpAnalyticItemParameterViewModel> ParameterViewModels { get; set; } = new ObservableCollection<MpAnalyticItemParameterViewModel>();

        public Dictionary<int, MpAnalyticItemParameterViewModel> ParamLookup  {
            get {
                var paraDict = new Dictionary<int, MpAnalyticItemParameterViewModel>();
                foreach(var pvm in ParameterViewModels) {
                    paraDict.Add(pvm.ParamEnumId, pvm);
                }
                return paraDict;
            }
        }

        public MpAnalyticItemParameterViewModel SelectedParameter => ParameterViewModels.FirstOrDefault(x => x.IsSelected);

        public ObservableCollection<MpAnalyticItemComponentViewModel> Children {
            get {
                var children = new List<MpAnalyticItemComponentViewModel>();
                
                ParameterViewModels.ForEach(x => children.Add(x));
                //if (ExecuteViewModel == null) {
                //    ExecuteViewModel = new MpAnalyticItemExecuteButtonViewModel(this);
                //}
                //children.Add(ExecuteViewModel);

                //if (ResultViewModel == null) {
                //    ResultViewModel = new MpAnalyticItemResultViewModel(this);
                //}
                //children.Add(ResultViewModel);

                return new ObservableCollection<MpAnalyticItemComponentViewModel>(children);
            }
        }

        public ObservableCollection<MpContextMenuItemViewModel> ContextMenuItems {
            get {
                var menuItems = new List<MpContextMenuItemViewModel>();

                foreach(var p in PresetViewModels) {
                    var pmi = new MpContextMenuItemViewModel(
                        header: p.Label,
                        command: MpClipTrayViewModel.Instance.AnalyzeSelectedItemCommand,
                        commandParameter: p.Preset.Id,
                        isChecked: null,
                        iconSource: p.PresetIcon,
                        subItems: null,
                        inputGestureText: p.ShortcutKeyString,
                        bgBrush: null);
                    menuItems.Add(pmi);
                }

                return new ObservableCollection<MpContextMenuItemViewModel>(menuItems);
            }
        }

        public ObservableCollection<MpContextMenuItemViewModel> QuickActionPresetMenuItems {
            get {
                var children = new List<MpContextMenuItemViewModel>();

                foreach (var p in PresetViewModels.Where(x=>x.IsQuickAction)) {
                    var pmi = new MpContextMenuItemViewModel(
                        header: p.Label,
                        command: MpClipTrayViewModel.Instance.AnalyzeSelectedItemCommand,
                        commandParameter: p.Preset.Id,
                        isChecked: null,
                        iconSource: p.PresetIcon,
                        subItems: null,
                        inputGestureText: p.ShortcutKeyString,
                        bgBrush: null);
                    children.Add(pmi);
                }

                return new ObservableCollection<MpContextMenuItemViewModel>(children);
            }
        }

        #endregion

        #region Appearance

        public string ResetLabel {
            get {
                if(SelectedPresetViewModel == null) {
                    return "Reset";
                }
                return SelectedPresetViewModel.ResetLabel;
            }
        }

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

        public int SourceCopyItemId { get; private set; } = 0;

        public virtual bool IsLoaded => Children.Count > 0;

        public bool IsAnyEditing => PresetViewModels.Any(x => x.IsEditing);

        public bool HasPresets => PresetViewModels.Count > 0;

        public bool HasAnyChanged => ParameterViewModels.Any(x => x.HasChanged);

        public bool IsAllValid => ParameterViewModels.All(x => x.IsValid);

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        protected string UnformattedResponse { get; set; } = string.Empty;

        public HttpStatusCode ResponseCode { get; set; }

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

        public string ItemIconBase64 {
            get {
                if (AnalyticItem == null || AnalyticItem.Icon == null) {
                    return null;
                }
                return AnalyticItem.Icon.IconImage.ImageBase64;
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

        public int AnalyticItemId {
            get {
                if (AnalyticItem == null) {
                    return 0;
                }
                return AnalyticItem.Id;
            }
        }

        public MpBillableItem BillableItem {
            get {
                if (AnalyticItem == null) {
                    return null;
                }
                return AnalyticItem.BillableItem;
            }
        }

        public MpAnalyticItem AnalyticItem { get; set; }



#endregion
#region Http
        protected abstract string FormatSourcePath { get; }
        //public abstract MpHttpResponseBase ResponseObj { get; }

        #endregion

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemViewModel() : base(null) { }

        public MpAnalyticItemViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemViewModel_PropertyChanged;
            PresetViewModels.CollectionChanged += PresetViewModels_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpAnalyticItem ai) {
            if (IsLoaded) {
                return;
            }
            IsBusy = true;
            AnalyticItem = await MpDb.Instance.GetItemAsync<MpAnalyticItem>(ai.Id);

            string paramJson = MpHelpers.Instance.ReadTextFromResource(FormatSourcePath, MpDb.Instance.GetType().Assembly);
            var paramlist = JsonConvert.DeserializeObject<MpAnalyticItemFormat>(paramJson, new MpJsonEnumConverter()).ParameterFormats;

            foreach (var aip in paramlist.OrderByDescending(x => x.SortOrderIdx)) {
                var naipvm = await CreateParameterViewModel(aip);
                ParameterViewModels.Insert(0, naipvm);
            }

            var presets = await MpDataModelProvider.Instance.GetAnalyticItemPresetsById(AnalyticItemId);

            // Init Presets
            PresetViewModels.Clear();

            foreach (var preset in presets.OrderBy(x => x.SortOrderIdx)) {
                var naipvm = await CreatePresetViewModel(preset);
                PresetViewModels.Add(naipvm);
            }

            if (PresetViewModels.Count == 0) {
                var daipvm = await CreateDefaultPresetViewModel();
                PresetViewModels.Add(daipvm);
            }

            PresetViewModels.OrderBy(x => x.SortOrderIdx);

            var defPreset = PresetViewModels.FirstOrDefault(x => x.IsDefault);
            MpAssert.Assert(defPreset, $"Error no default preset for anayltic item {AnalyticItem.Title}");


            OnPropertyChanged(nameof(ItemIconBase64));
            OnPropertyChanged(nameof(PresetViewModels));
            OnPropertyChanged(nameof(ParameterViewModels));
            OnPropertyChanged(nameof(HasPresets));
            OnPropertyChanged(nameof(Children));

            ParameterViewModels.ForEach(x => x.Validate());

            IsBusy = false;
        }

        public async Task<MpAnalyticItemPresetViewModel> CreatePresetViewModel(MpAnalyticItemPreset aip) {
            MpAnalyticItemPresetViewModel naipvm = new MpAnalyticItemPresetViewModel(this);
            await naipvm.InitializeAsync(aip);
            return naipvm;
        }

        public async Task<MpAnalyticItemPresetViewModel> CreateDefaultPresetViewModel() {
            var defPreset = await MpAnalyticItemPreset.Create(
                analyticItem: AnalyticItem,
                label: "Default",
                icon: AnalyticItem.Icon,
                isDefault: true,
                isQuickAction: false,
                sortOrderIdx: 0,
                description: $"This is the default preset for '{AnalyticItem.Title}' and cannot be removed");

            foreach (var paramVm in ParameterViewModels) {
                var ppv = await MpAnalyticItemPresetParameterValue.Create(
                    parentItem: defPreset,
                    paramEnumId: paramVm.ParamEnumId,
                    value: paramVm.DefaultValue,
                    defaultValue: paramVm.DefaultValue);;

                defPreset.PresetParameterValues.Add(ppv);
            }
            var daipvm = await CreatePresetViewModel(defPreset);
            return daipvm;            
        }

        public async Task<MpAnalyticItemParameterViewModel> CreateParameterViewModel(MpAnalyticItemParameter aip) {
            MpAnalyticItemParameterViewModel naipvm = null;

            switch (aip.ParameterType) {
                case MpAnalyticItemParameterType.ComboBox:
                    naipvm = new MpComboBoxParameterViewModel(this);
                    break;
                case MpAnalyticItemParameterType.Text:
                    naipvm = new MpTextBoxParameterViewModel(this);
                    break;
                case MpAnalyticItemParameterType.CheckBox:
                    naipvm = new MpCheckBoxParameterViewModel(this);
                    break;
                case MpAnalyticItemParameterType.Slider:
                    naipvm = new MpSliderParameterViewModel(this);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpAnalyticItemParameterType), aip.ParameterType));
            }

            naipvm.PropertyChanged += ParameterViewModels_PropertyChanged;
            naipvm.OnValidate += ParameterViewModel_OnValidate;

            if(aip.IsValueDeferred) {
                aip = await DeferredCreateParameterModel(aip);
            }
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

            while(PresetViewModels.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
            }
            return uniqueName + uniqueIdx;
        }

        #endregion

        #region Protected Methods

        protected virtual void ParameterViewModels_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var pvm = sender as MpAnalyticItemParameterViewModel;
            switch(e.PropertyName) {
                case nameof(pvm.CurrentValue):
                    if(SelectedPresetViewModel == null) {
                        return;
                    }
                    MpAnalyticItemPresetParameterValue ppv = SelectedPresetViewModel.Preset.PresetParameterValues.FirstOrDefault(x => x.ParameterEnumId == pvm.ParamEnumId);
                    if(ppv != null) {
                        ppv.Value = pvm.CurrentValue;
                    }
                    break;
            }
        }

        public virtual async Task<MpAnalyticItemParameter> DeferredCreateParameterModel(MpAnalyticItemParameter aip) { 
            await Task.Delay(1);
            return aip;
        }

        protected virtual void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            var aipvm = sender as MpAnalyticItemParameterViewModel;
            if (aipvm.IsRequired && string.IsNullOrEmpty(aipvm.CurrentValue)) {
                aipvm.ValidationMessage = $"{aipvm.Label} is required";
            } else {
                aipvm.ValidationMessage = string.Empty;
            }
        }

        #region Db Event Handlers

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpAnalyticItemPreset aip) {
                if (aip.AnalyticItemId == AnalyticItemId) {
                    var presetVm = PresetViewModels.FirstOrDefault(x => x.Preset.Id == aip.Id);
                    if (presetVm != null) {
                        MpHelpers.Instance.RunOnMainThread(async () => {
                            aip = await MpDb.Instance.GetItemAsync<MpAnalyticItemPreset>(aip.Id);
                            await presetVm.InitializeAsync(aip);
                            OnPropertyChanged(nameof(ContextMenuItems));
                            OnPropertyChanged(nameof(QuickActionPresetMenuItems));
                        });
                    }
                }
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpAnalyticItemPreset aip) {
                if(aip.AnalyticItemId == AnalyticItemId) {
                    var presetVm = PresetViewModels.FirstOrDefault(x => x.Preset.Id == aip.Id);
                    if(presetVm != null) {
                        int presetIdx = PresetViewModels.IndexOf(presetVm);
                        if(presetIdx >= 0) {
                            PresetViewModels.RemoveAt(presetIdx);
                            OnPropertyChanged(nameof(HasPresets));
                            OnPropertyChanged(nameof(PresetViewModels));
                            OnPropertyChanged(nameof(SelectedPresetViewModel));
                            OnPropertyChanged(nameof(ContextMenuItems));
                            OnPropertyChanged(nameof(QuickActionPresetMenuItems));
                            OnPropertyChanged(nameof(ResetLabel));
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private async void PresetViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            await UpdatePresetSortOrder();
        }

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsExpanded):
                    if(IsExpanded) {
                        //MpHelpers.Instance.RunOnMainThread(async () => {
                        //    await LoadChildren();
                        //});
                    }
                    break;
                case nameof(IsSelected):
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
                    UpdateParameterValues();
                    break;
            }
        }

        private void UpdateParameterValues(bool isReset = false) {
            foreach (var paramVm in ParameterViewModels) {
                if (SelectedPresetViewModel == null) {
                    paramVm.ResetToDefault();
                } else {
                    var presetValue = SelectedPresetViewModel.Preset.PresetParameterValues.FirstOrDefault(x => x.ParameterEnumId == paramVm.ParamEnumId);

                    if (presetValue == null) {
                        paramVm.ResetToDefault();
                    } else {
                        if (isReset) {
                            paramVm.SetValueFromPreset(presetValue.DefaultValue);
                        } else {
                            paramVm.SetValueFromPreset(presetValue.Value);
                        }
                    }
                }
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

        private void CreatePreset(string presetName) {
            Task.Run(async () => {

            });
        }

        private async Task ConvertToCopyItem(string resultData, string reqStr) {
            var app = MpPreferences.Instance.ThisAppSource.App;
            var url = await MpUrlBuilder.Create(AnalyticItem.EndPoint, null, reqStr);
            var source = await MpSource.Create(app, url);
            var ci = await MpCopyItem.Create(source, resultData, MpCopyItemType.RichText);

            var scivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(SourceCopyItemId);

            var sml = scivm.Parent.ItemViewModels.Select(x => x.CopyItem).OrderBy(x => x.CompositeSortOrderIdx).ToList();
            for (int i = 0; i < sml.Count; i++) {
                if (i == scivm.CompositeSortOrderIdx) {
                    ci.CompositeParentCopyItemId = sml[0].Id;
                    ci.CompositeSortOrderIdx = i + 1;
                } else if (i > scivm.CompositeSortOrderIdx) {
                    sml[i].CompositeSortOrderIdx += 1;
                }
            }

            await ci.WriteToDatabaseAsync();

            await scivm.Parent.InitializeAsync(sml[0]);
        }
        #endregion

        #region Commands

        public ICommand ExecuteAnalysisCommand => new RelayCommand(
            async () => {
                SourceCopyItemId = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItemId;
                string analysisStr = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItemData.ToPlainText();
                var result = await ExecuteAnalysis(analysisStr) as Tuple<string,MpJsonMessage>;

                await ConvertToCopyItem(result.Item1, result.Item2.Serialize());
            },
            CanExecuteAnalysis);

        protected abstract Task<object> ExecuteAnalysis(object obj);

        protected virtual bool CanExecuteAnalysis() {
            return IsAllValid && MpClipTrayViewModel.Instance.SelectedContentItemViewModels.Count > 0;
        }

        public ICommand CreateNewPresetCommand => new RelayCommand(
            async () => {
                //if(!IsLoaded) {
                //    await LoadChildren();
                //}

                MpAnalyticItemPreset newPreset = await MpAnalyticItemPreset.Create(
                        AnalyticItem,
                        GetUniquePresetName());

                foreach (var paramVm in ParameterViewModels.OrderBy(x => x.Parameter.SortOrderIdx)) {
                    var naippv = await MpAnalyticItemPresetParameterValue.Create(
                        newPreset,
                        paramVm.ParamEnumId,
                        paramVm.CurrentValue);

                    newPreset.PresetParameterValues.Add(naippv);
                }
                await newPreset.WriteToDatabaseAsync();

                var npvm = await CreatePresetViewModel(newPreset);
                PresetViewModels.Add(npvm);
                PresetViewModels.ForEach(x => x.IsSelected = false);
                npvm.IsSelected = true;

                OnPropertyChanged(nameof(PresetViewModels));
                OnPropertyChanged(nameof(HasPresets));
            },
            ()=>IsAllValid);

        public ICommand SelectPresetCommand => new RelayCommand<MpAnalyticItemPresetViewModel>(
            async (selectedPresetVm) => {
                //if(!IsLoaded) {
                //    await LoadChildren();
                //}
                PresetViewModels.ForEach(x => x.IsSelected = false);
                selectedPresetVm.IsSelected = true;
            });

        public ICommand ResetCommand => new RelayCommand(
            () => {
                UpdateParameterValues(true);
            },
            () => HasAnyChanged);

        public ICommand ManageAnalyticItemCommand => new RelayCommand(
             async() => {
                 //if (!IsLoaded) {
                 //    await LoadChildren();
                 //}
                 if (!IsSelected) {
                     Parent.Items.ForEach(x => x.IsSelected = x.AnalyticItemId == AnalyticItemId);
                 }
                 if (SelectedPresetViewModel == null && PresetViewModels.Count > 0) {
                     PresetViewModels.ForEach(x => x.IsSelected = false);
                     PresetViewModels[0].IsSelected = true;
                 }
                 var manageWindow = new MpManageAnalyticItemModalWindow();

                 MpMainWindowViewModel.Instance.IsShowingDialog = true;

                 var result = manageWindow.ShowDialog();
                 MpMainWindowViewModel.Instance.IsShowingDialog = false;

                 if (result == false) {
                     return;
                 }
                 foreach (var pvm in PresetViewModels) {
                     await pvm.Preset.WriteToDatabaseAsync();
                     await Task.WhenAll(pvm.Preset.PresetParameterValues.Select(x => x.WriteToDatabaseAsync()));
                 }
             });

        public ICommand DeletePresetCommand => new RelayCommand<MpAnalyticItemPresetViewModel>(
            async (presetVm) => {
                foreach(var presetVal in presetVm.Preset.PresetParameterValues) {
                    await presetVal.DeleteFromDatabaseAsync();
                }
                await presetVm.Preset.DeleteFromDatabaseAsync();
            },
            (presetVm) => presetVm != null && presetVm is MpAnalyticItemPresetViewModel aipsvm && !aipsvm.IsDefault);

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
