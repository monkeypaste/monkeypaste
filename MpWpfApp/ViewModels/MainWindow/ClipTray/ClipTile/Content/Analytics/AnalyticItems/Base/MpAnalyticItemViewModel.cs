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

namespace MpWpfApp {
    public abstract class MpAnalyticItemViewModel : MpViewModelBase<MpAnalyticItemCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemPresetViewModel> PresetViewModels { get; set; } = new ObservableCollection<MpAnalyticItemPresetViewModel>();

        public MpAnalyticItemPresetViewModel DefaultPresetViewModel => PresetViewModels.FirstOrDefault(x => x.IsDefault);

        public MpAnalyticItemPresetViewModel SelectedPresetViewModel => PresetViewModels.FirstOrDefault(x => x.IsSelected);     

        public ObservableCollection<MpContextMenuItemViewModel> ContextMenuItems => new ObservableCollection<MpContextMenuItemViewModel>(PresetViewModels.Select(x => x.ContextMenuItemViewModel));

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

        public bool IsAnyEditing => PresetViewModels.Any(x => x.IsEditing);

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

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


        public MpCopyItemType InputContentType { 
            get {
                if(AnalyticItem == null) {
                    return MpCopyItemType.None;
                }
                return AnalyticItem.InputFormatType;
            }
        }

        public MpOutputFormatType OutputFormatType {
            get {
                if (AnalyticItem == null) {
                    return MpOutputFormatType.None;
                }
                return AnalyticItem.OutputFormatType;
            }
        }

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

        public string ParameterFormatResourcePath {
            get {
                if (AnalyticItem == null) {
                    return string.Empty;
                }
                return AnalyticItem.ParameterFormatResourcePath;
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

            if(AnalyticItem.Icon == null) {
                var url = await MpUrlBuilder.Create(AnalyticItem.EndPoint, null);
                AnalyticItem.Icon = url.Icon;
                AnalyticItem.IconId = url.IconId;
                await AnalyticItem.WriteToDatabaseAsync();
                OnPropertyChanged(nameof(ItemIconBase64));
            }
            // Init Presets
            PresetViewModels.Clear();

            if(AnalyticItem.Presets.Count == 0) {
                var naipvm = await CreatePresetViewModel(null);
                PresetViewModels.Add(naipvm);
            } else {
                foreach (var preset in AnalyticItem.Presets.OrderBy(x => x.SortOrderIdx)) {
                    var naipvm = await CreatePresetViewModel(preset);
                    PresetViewModels.Add(naipvm);
                }
            }            

            PresetViewModels.OrderBy(x => x.SortOrderIdx);

            var defPreset = PresetViewModels.FirstOrDefault(x => x.IsDefault);
            MpAssert.Assert(defPreset, $"Error no default preset for anayltic item {AnalyticItem.Title}");


            OnPropertyChanged(nameof(ItemIconBase64));
            OnPropertyChanged(nameof(PresetViewModels));

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

            while(PresetViewModels.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
            }
            return uniqueName + uniqueIdx;
        }

        public virtual async Task<MpAnalyticItemParameter> DeferredCreateParameterModel(MpAnalyticItemParameter aip) {
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
                if (aip.AnalyticItemId == AnalyticItemId) {
                    
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
                            OnPropertyChanged(nameof(PresetViewModels));
                            OnPropertyChanged(nameof(SelectedPresetViewModel));
                            OnPropertyChanged(nameof(ContextMenuItems));
                            OnPropertyChanged(nameof(QuickActionPresetMenuItems));
                        }
                    }
                }
            }
        }

        #endregion


        protected virtual async Task ConvertToCopyItem(int parentCopyItemId, object resultData, object reqStr) {
            var app = MpPreferences.Instance.ThisAppSource.App;
            var url = await MpUrlBuilder.Create(AnalyticItem.EndPoint, null, reqStr.ToString());
            var source = await MpSource.Create(app, url);
            var ci = await MpCopyItem.Create(source, resultData.ToString(), MpCopyItemType.RichText);

            var scivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(parentCopyItemId);
            if (scivm == null) {
                return;
            }
            var sml = scivm.Parent.ItemViewModels.Select(x => x.CopyItem).OrderBy(x => x.CompositeSortOrderIdx).ToList();
            for (int i = 0; i < sml.Count; i++) {
                if (i == scivm.CompositeSortOrderIdx) {
                    ci.CompositeParentCopyItemId = sml[0].Id;
                    ci.CompositeSortOrderIdx = i + 1;
                    await ci.WriteToDatabaseAsync();
                } else if (i > scivm.CompositeSortOrderIdx) {
                    sml[i].CompositeSortOrderIdx += 1;
                    await sml[i].WriteToDatabaseAsync();
                }
            }

            await scivm.Parent.InitializeAsync(sml[0]);
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
                        //MpHelpers.Instance.RunOnMainThread(async () => {
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

        #endregion

        #region Commands

        public ICommand ExecuteAnalysisCommand => new RelayCommand<object>(
            async (args) => {
                if(args != null && args is MpAnalyticItemPresetViewModel aipvm) {
                    PresetViewModels.ForEach(x => x.IsSelected = x == aipvm);
                    OnPropertyChanged(nameof(SelectedPresetViewModel));
                }
                int sourceCopyItemId = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItemId;
                string analysisStr = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItemData.ToPlainText();
                var result = await ExecuteAnalysis(analysisStr) as Tuple<object,object>;

                await ConvertToCopyItem(sourceCopyItemId,result.Item1, result.Item2);
            },(args)=>CanExecuteAnalysis(args));

        protected abstract Task<object> ExecuteAnalysis(object obj);

        public virtual bool CanExecuteAnalysis(object obj) {
            var spvm = obj == null ? SelectedPresetViewModel : obj as MpAnalyticItemPresetViewModel;
            return spvm != null &&
                   spvm.IsAllValid && 
                   MpClipTrayViewModel.Instance.SelectedContentItemViewModels.Count > 0 &&
                   MpClipTrayViewModel.Instance.SelectedContentItemViewModels.All(x=>x.CopyItemType == InputContentType);
        }

        public ICommand CreateNewPresetCommand => new RelayCommand(
            async () => {
                MpAnalyticItemPreset newPreset = await MpAnalyticItemPreset.Create(
                        AnalyticItem,
                        GetUniquePresetName());


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
                     Parent.Items.ForEach(x => x.IsSelected = x.AnalyticItemId == AnalyticItemId);
                 }
                 if (SelectedPresetViewModel == null && PresetViewModels.Count > 0) {
                     PresetViewModels.ForEach(x => x.IsSelected = x == PresetViewModels[0]);
                 }
                 if(!Parent.IsVisible) {
                     Parent.IsVisible = true;
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
