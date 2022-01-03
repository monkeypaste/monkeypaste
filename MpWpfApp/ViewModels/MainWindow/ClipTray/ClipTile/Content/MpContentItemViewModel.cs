using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;
using PropertyChanged;

namespace MpWpfApp {
    public class MpContentItemViewModel : MpViewModelBase<MpClipTileViewModel>, MpIShortcutCommand {
        #region Private Variables
        private DispatcherTimer _timer;

        Size itemSize;
        int fc = 0, lc = 0, cc = 0;
        double ds = 0;

        private int _detailIdx = 0;

        #endregion

        #region Properties

        #region View Models

        public MpDetectedImageObjectCollectionViewModel DetectedImageObjectCollectionViewModel { get; set; }

        public MpClipTileTitleSwirlViewModel TitleSwirlViewModel { get; set; }

        private MpTemplateCollectionViewModel _templateCollection;
        [MpChildViewModel(typeof(MpTemplateCollectionViewModel), false)]
        public MpTemplateCollectionViewModel TemplateCollection {
            get {
                return _templateCollection;
            }
            set {
                if (_templateCollection != value) {
                    _templateCollection = value;
                    OnPropertyChanged(nameof(TemplateCollection));
                }
            }
        }

        #endregion

        #region MpIShortcutCommand Implementation

        public MpShortcutType ShortcutType => MpShortcutType.PasteCopyItem;

        public MpShortcutViewModel ShortcutViewModel {
            get {
                if (Parent == null || CopyItem == null) {
                    return null;
                }
                var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.CommandId == CopyItemId && x.ShortcutType == ShortcutType);

                if (scvm == null) {
                    scvm = new MpShortcutViewModel(MpShortcutCollectionViewModel.Instance);
                }

                return scvm;
            }
        }

        public string ShortcutKeyString => ShortcutViewModel.KeyString;

        public ICommand AssignCommand => AssignHotkeyCommand;

        #endregion

        #region Appearance

        public Brush DetailTextColor {
            get {
                if (IsSelected || Parent.IsSelected) {
                    return Brushes.Black;//Brushes.DarkGray;
                }
                if (IsHovering || Parent.IsHovering) {
                    return Brushes.Black;//Brushes.DimGray;
                }
                return Brushes.Transparent;
            }
        }

        public Brush TileTitleTextGridBackgroundBrush {
            get {
                if (IsHoveringOnTitleTextGrid && !IsEditingTitle) {
                    return new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                }
                return Brushes.Transparent;
            }
        }

        public Brush TitleTextColor {
            get {
                if (IsHoveringOnTitleTextGrid) {
                    return Brushes.DarkGray;
                }
                return Brushes.White;
            }
        }

        private Brush _itemBackgroundBrush;
        public Brush ItemBackgroundBrush {
            get {
                if (MpDragDropManager.Instance.IsDragAndDrop) {
                    return MpThemeColors.Instance.CurrentTheme[MpThemeItemType.Clip_Tile_Content_Item_Background_Color];
                }
                if (IsHovering &&
                    ((Parent.IsExpanded && !IsSelected) || !Parent.IsExpanded) &&
                    Parent.Count > 1) {
                    if(string.IsNullOrEmpty(CopyItem.ItemColor)) {
                        if(_itemBackgroundBrush == null) {
                            _itemBackgroundBrush = MpHelpers.Instance.GetRandomBrushColor();
                        }
                    } else if(_itemBackgroundBrush != CopyItemColorBrush) {
                        _itemBackgroundBrush = CopyItemColorBrush;
                    }
                    return MpHelpers.Instance.GetLighterBrush(_itemBackgroundBrush, 0.75);
                }

                return MpThemeColors.Instance.CurrentTheme[MpThemeItemType.Clip_Tile_Content_Item_Background_Color];

            }
        }

        public Brush ItemBorderBrush {
            get {
                if(Parent == null || 
                   Parent.Count <= 1 || 
                   !IsSelected || 
                   IsItemDragging) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        }

        public Brush ItemSeparatorBrush {
            get {
                if(//MpContentDropManager.Instance.IsDragAndDrop ||
                   Parent == null ||
                   Parent.Count == 1 ||
                   ItemIdx == Parent.Count - 1 ||
                   //(ItemIdx == Parent.DropIdx + 1 && Parent.IsDroppingOnTile) || // NOTE drop line uses adorner since DropIdx 0 won't have seperator
                   IsSelected ||
                   Parent.SelectedItems.Any(x=>x.ItemIdx == ItemIdx + 1)) {
                    return Brushes.Transparent;
                }
                return Brushes.DimGray;
            }
        }

        public Rect ItemBorderBrushRect {
            get {
                if (IsContextMenuOpen || IsItemDragging) {
                    return MpMeasurements.Instance.DottedBorderRect;
                }
                return MpMeasurements.Instance.SolidBorderRect;
            }
        }

        public Rect ItemSeparatorBrushRect {
            get {
                if(CopyItemType == MpCopyItemType.FileList) {
                    return MpMeasurements.Instance.SolidBorderRect;
                }
                return MpMeasurements.Instance.DottedBorderRect;
            }
        }
        #endregion

        #region Visibility 

        #endregion

        #region Layout

        public double EditorHeight {
            get {
                if(Parent == null || CopyItem == null) {
                    return 0;
                }
                if(Parent.IsExpanded) {
                    if(Parent.Count > 1) {
                        return Double.NaN;
                    }
                    return Parent.TileContentHeight - MpMeasurements.Instance.ClipTileEditToolbarHeight - 15;
                }
                return UnexpandedSize.Height;
            }
        }

        public Size ExpandedSize {
            get {
                if (Parent == null || CopyItem == null) {
                    return new Size();
                }
                //get contents actual size
                var ds = CopyItemData.ToFlowDocument().GetDocumentSize();

                //if item's content is larger than expanded width make sure it gets that width (will show scroll bars)
                double w = Math.Max(ds.Width, MpMeasurements.Instance.ClipTileContentMinMaxWidth);

                //let height in expanded mode match content's height
                double h = ds.Height;

                return new Size(w, h);
            }
        }

        public Size UnexpandedSize {
            get {
                if (Parent == null || CopyItem == null) {
                    return new Size();
                }
                //item height is divided evenly by items but if there are many (more than 5) 
                //their will only be 5 visible

                double h = 0;
                if(Parent.Count == 1) {
                    h = Parent.TileContentHeight; //MpMeasurements.Instance.ClipTileContentHeight;
                } else {
                    h = Math.Max(
                                        Parent.TileContentHeight / Parent.VisibleItems.Count,
                                        MpMeasurements.Instance.ClipTileContentItemMinHeight);
                }
                return new Size(
                            MpMeasurements.Instance.ClipTileContentMinWidth,
                            h);
            }
        }

        public Size CurrentSize {
            get {
                if (Parent == null) {
                    return new Size();
                }
                if (Parent.IsExpanded) {
                    return ExpandedSize;
                }
                return UnexpandedSize;
            }
        }

        public Size UnformattedContentSize { get; set; }

        #endregion

        #region State

        public bool HasDetectedObjects => DetectedImageObjectCollectionViewModel != null && DetectedImageObjectCollectionViewModel.Items.Count > 0;

        public bool IsOverHyperlink { get; set; } = false;

        public MpCopyItemDetailType CurDetailType { 
            get {
                return (MpCopyItemDetailType)_detailIdx;
            }
        }

        public bool IsNewAndFirstLoad { get; set; } = false;

        private bool _isHoveringOnTitleTextGrid = false;
        public bool IsHoveringOnTitleTextGrid {
            get {
                return _isHoveringOnTitleTextGrid;
            }
            set {
                if (_isHoveringOnTitleTextGrid != value) {
                    _isHoveringOnTitleTextGrid = value;
                    OnPropertyChanged(nameof(IsHoveringOnTitleTextGrid));
                    OnPropertyChanged(nameof(TileTitleTextGridBackgroundBrush));
                    OnPropertyChanged(nameof(TitleTextColor));
                }
            }
        }

        #region Scroll

        public double NormalizedVerticalScrollOffset { get; set; } = 0;

        public bool IsScrolledToHome => Math.Abs(NormalizedVerticalScrollOffset) <= 0.1;

        public bool IsScrolledToEnd => Math.Abs(NormalizedVerticalScrollOffset) >= 0.9;

        public double KeyboardScrollAmount { get; set; } = 0.2;

        #endregion

        public DateTime LastSubSelectedDateTime { get; set; }

        [MpAffectsParent]
        public bool IsSelected {  get; set; }

        [MpAffectsParent]
        public bool IsHovering { get; set; } = false;

        [MpAffectsParent]
        public bool IsContextMenuOpen { get; set; } = false;

        public bool IsEditingContent { 
            get {
                if(Parent == null || !Parent.IsExpanded) {
                    return false;
                }
                return IsSelected && !IsPastingTemplate;
            }
        }

        public bool IsEditingTemplate {
            get {
                if(CopyItem == null || TemplateCollection == null) {
                    return false;
                }

                return TemplateCollection.Templates.Any(x => x.IsEditingTemplate);
            }
        }

        public bool IsPastingTemplate { get; set; } = false;

        private bool _isEditingTitle = false;
        public bool IsEditingTitle {
            get {
                return _isEditingTitle;
            }
            set {
                if (_isEditingTitle != value) {
                    _isEditingTitle = value;
                    OnPropertyChanged(nameof(IsEditingTitle));
                    OnPropertyChanged(nameof(TileTitleTextGridBackgroundBrush));
                }
            }
        }

        public bool HasTemplates {
            get {
                return TemplateCollection.Templates.Count > 0;
            }
        }

        public int ItemIdx {
            get {
                if (Parent == null) {
                    return -1;
                }
                return Parent.ItemViewModels.IndexOf(this);
            }
        }

        [MpAffectsParent]
        public bool IsPlaceholder => CopyItem == null;

        #region Drag & Drop
        //[MpAffectsParent]
        public bool IsItemDragging { get; set; } = false;


        //public bool IsDragOverItem { get; set; } = false;

        //public Point MouseDownPosition { get; set; }
        //public IDataObject DragDataObject { get; set; }
        #endregion

        #endregion

        #region Business Logic
        public string TemplateRichText { get; set; }

        public string DetailText { get; set; }

        #endregion

        #region Icons

        public string HotkeyIconSource {
            get {
                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                    return MpBase64Images.Instance.JoystickUnset;
                }
                return MpBase64Images.Instance.JoystickActive;
            }
        }

        #endregion

        #region Model

        public string[] ColorPallete { get; set; }

        public string RelativePalleteColor {
            //since items will have the same source a lot this will choose relative to list order
            get {
                if(CopyItem == null) {
                    return string.Empty;
                }
                int idx = ItemIdx;
                if(Parent.Count >= ColorPallete.Length) {
                    idx = Math.Min((int)((ColorPallete.Length / Parent.Count) * idx) - 1,ColorPallete.Length-1);
                }
                return ColorPallete[idx];
            }
        }

        public bool IsCompositeChild {
            get {
                if (CopyItem == null || base.Parent == null) {
                    return false;
                }
                return CopyItem.CompositeParentCopyItemId > 0 || base.Parent.Count > 1;
            }
        }

        public DateTime CopyItemCreatedDateTime {
            get {
                if (CopyItem == null) {
                    return DateTime.MinValue;
                }
                return CopyItem.CopyDateTime;
            }
        }

        public string HotkeyIconTooltip {
            get {
                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                    return @"Assign Shortcut";
                }
                return ShortcutKeyString;
            }
        }

        public int RelevanceScore {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CopyCount + CopyItem.PasteCount;
            }
        }
        public int CompositeSortOrderIdx {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeSortOrderIdx;
            }
            set {
                if (CopyItem != null && CopyItem.CompositeSortOrderIdx != value) {
                    CopyItem.CompositeSortOrderIdx = value;
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                }
            }
        }

        public int CompositeParentCopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeParentCopyItemId;
            }
            set {
                if (CopyItem != null && CopyItem.CompositeParentCopyItemId != value) {
                    CopyItem.CompositeParentCopyItemId = value;
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                }
            }
        }
        public string CopyItemTitle {
            get {
                if(CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Title;
            }
            set {
                if(CopyItem != null && CopyItem.Title != value) {
                    CopyItem.Title = value;
                    OnPropertyChanged(nameof(CopyItemTitle));
                }
            }
        }

        public MpCopyItemType CopyItemType {
            get {
                if(CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.ItemType;
            }
            set {
                if(CopyItem != null && CopyItem.ItemType != value) {
                    CopyItem.ItemType = value;
                    OnPropertyChanged(nameof(CopyItemType));
                }
            }
        }

        public int CopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.Id;
            }
            set {
                if (CopyItem != null && CopyItem.Id != value) {
                    CopyItem.Id = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        public int SourceId {
            get {
                if(CopyItem == null) {
                    return 0;
                }
                return CopyItem.SourceId;
            }
        }

        public Brush CopyItemColorBrush {
            get {
                if (CopyItem == null) {
                    return Brushes.Red;
                }
                return new SolidColorBrush(CopyItem.ItemColor.ToWinMediaColor());
            }
            set {
                if (CopyItem != null && CopyItem.ItemColor != value.ToHex()) {
                    CopyItem.ItemColor = value.ToHex();
                    OnPropertyChanged(nameof(CopyItemColorBrush));
                }
            }
        }

        public string CopyItemData {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.ItemData;
            }
            set {
                if (CopyItem != null && CopyItem.ItemData != value) {
                    CopyItem.ItemData = value;
                    OnPropertyChanged(nameof(CopyItemData));
                    OnPropertyChanged(nameof(CurrentSize));
                }
            }
        }


        public MpCopyItem CopyItem { get; set; }

        #endregion

        #endregion

        #region Events

        public event EventHandler<double> OnScrollWheelRequest;
        public event EventHandler OnUiUpdateRequest;
        public event EventHandler OnFitContentRequest;
        //public event EventHandler OnSubSelected;

        public event EventHandler OnMergeRequest;
        public event EventHandler<bool> OnUiResetRequest;
        public event EventHandler OnClearTemplatesRequest;
        public event EventHandler OnCreateTemplatesRequest;
        public event EventHandler OnSyncModels;

        #endregion

        #region Constructors

        public MpContentItemViewModel() : base(null) { }

        public MpContentItemViewModel(MpClipTileViewModel container) : base(container) {
            PropertyChanged += MpContentItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpCopyItem ci) {
            IsBusy = true;

            MpMessenger.Instance.Unregister<MpMessageType>(MpDragDropManager.Instance, ReceivedDragDropManagerMessage);

            if (ci != null && ci.Source == null) {
                ci.Source = await MpDb.Instance.GetItemAsync<MpSource>(ci.SourceId);
            }
            CopyItem = ci;


            IsNewAndFirstLoad = !MpMainWindowViewModel.Instance.IsMainWindowLoading;

            TemplateCollection = new MpTemplateCollectionViewModel(this);
            TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);

            await UpdateColorPallete();
            //AnalyticItemCollectionViewModel = new MpAnalyticItemCollectionViewModel(this);

            CycleDetailCommand.Execute(null);

            if(CopyItemType == MpCopyItemType.Image) {
                DetectedImageObjectCollectionViewModel = new MpDetectedImageObjectCollectionViewModel(this);
                await DetectedImageObjectCollectionViewModel.InitializeAsync(CopyItem);
                OnPropertyChanged(nameof(HasDetectedObjects));
            }

            RequestUiUpdate();
            OnPropertyChanged(nameof(EditorHeight));
            OnPropertyChanged(nameof(ItemBorderBrush));
            OnPropertyChanged(nameof(ShortcutKeyString));

            MpMessenger.Instance.Register<MpMessageType>(MpDragDropManager.Instance, ReceivedDragDropManagerMessage);

            IsBusy = false;
        }

        public async Task GatherAnalytics() {
            var analyticTasks = new List<Task>();
            Task<string> urlTask = null, ocrTask = null, cvTask = null;
            if (CopyItem.SsDbImage != null) {
                urlTask = MpBrowserUrlDetector.Instance.FindUrlAddressFromScreenshot(CopyItem.SsDbImage.ImageBase64.ToBitmapSource());
                analyticTasks.Add(urlTask);
            }

            //if (CopyItem.ItemType == MpCopyItemType.Image) {
            //    var itemBmpBytes = MpHelpers.Instance.ConvertBitmapSourceToByteArray(CopyItem.ItemData.ToBitmapSource());
            //    ocrTask = MpImageOcr.Instance.OcrImageForText(itemBmpBytes);
            //    analyticTasks.Add(ocrTask);
            //    cvTask = MpImageAnalyzer.Instance.AnalyzeImage(itemBmpBytes);
            //    analyticTasks.Add(cvTask);
            //}

            await Task.WhenAll(analyticTasks.ToArray());

            if (urlTask != null) {
                string detectedUrl = await urlTask;
                if (!string.IsNullOrEmpty(detectedUrl)) {
                    string urlTitle = await MpHelpers.Instance.GetUrlTitle(detectedUrl);
                    //CopyItemUrl = MpUrl.Create(detectedUrl, urlTitle);
                    //if (CopyItemUrlDomain == null) {
                    //    string urlDomain = MpHelpers.Instance.GetUrlDomain(detectedUrl);
                    //    string urlDomainTitle = await MpHelpers.Instance.GetUrlTitle(urlDomain);
                    //    CopyItemUrlDomain = MpUrlDomain.Create(urlDomain,  urlDomainTitle);
                    //}
                }
                MonkeyPaste.MpConsole.WriteLine("Detected Browser Address: " + detectedUrl);
            }

            if (ocrTask != null) {
                CopyItem.ItemDescription = await ocrTask;
            }

            if (cvTask != null) {
                CopyItem.ItemDescription = await cvTask;
                //var imgAnalysis = JsonConvert.DeserializeObject<MpImageAnalysis>(cvContent);
            }

            //OnPropertyChanged(nameof(AppIcon));
        }

        #region UI Invokers

        public void RequestFitContent() {
            OnFitContentRequest?.Invoke(this, null);
        }
        public void RequestSyncModel() {
            OnSyncModels?.Invoke(this, null);
        }
        
        public void RequestMerge() {
            OnMergeRequest?.Invoke(this, null);
        }
        public void RequestUiReset() {
            OnUiResetRequest?.Invoke(this, IsSelected);
        }

        public void RequestClearHyperlinks() {
            OnClearTemplatesRequest?.Invoke(this, null);
        }

        public void RequestCreateHyperlinks() {
            OnCreateTemplatesRequest?.Invoke(this, null);
        }

        public void RequestScrollWheelChange(double delta) {
            OnScrollWheelRequest?.Invoke(this, delta);
        }

        public void RequestUiUpdate() {
            OnUiUpdateRequest?.Invoke(this, null);
        }

        #endregion


        public string GetDetailText(MpCopyItemDetailType detailType) {
            if (CopyItem == null) {
                return string.Empty;
            }

            string info = string.Empty;
            switch (detailType) {
                //created
                case MpCopyItemDetailType.DateTimeCreated:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc

                    info = "Copied " + CopyItem.CopyDateTime.ToReadableTimeSpan();
                    break;
                //chars/lines
                case MpCopyItemDetailType.DataSize:
                    if (CopyItem.ItemType == MpCopyItemType.Image) {
                        info = "(" + (int)itemSize.Width + "px) x (" + (int)itemSize.Height + "px)";
                    } else if (CopyItem.ItemType == MpCopyItemType.RichText) {
                        info = cc + " chars | " + lc + " lines";
                    } else if (CopyItem.ItemType == MpCopyItemType.FileList) {
                        info = fc + " files | " + ds + " MB";
                    }
                    break;
                //# copies/# pastes
                case MpCopyItemDetailType.UsageStats:
                    info = CopyItem.CopyCount + " copies | " + CopyItem.PasteCount + " pastes";
                    break;
                case MpCopyItemDetailType.UrlInfo:
                    if (CopyItem.Source.Url == null) {
                        _detailIdx++;
                        info = GetDetailText((MpCopyItemDetailType)_detailIdx);
                    } else {
                        info = CopyItem.Source.Url.UrlPath;
                    }
                    break;
                case MpCopyItemDetailType.AppInfo:
                    if (CopyItem.Source.App.UserDevice.Guid == MpPreferences.Instance.ThisDeviceGuid) {
                        info = CopyItem.Source.App.AppPath;
                    } else {
                        info = CopyItem.Source.App.AppPath;
                    }

                    break;
                default:
                    info = "Unknown detailId: " + (int)detailType;
                    break;
            }

            return info;
        }

        public void ClearEditing() {
            //IsEditingContent = false;
            IsEditingTitle = false;
            TemplateCollection?.ClearAllEditing();
            if (IsPastingTemplate) {
                IsPastingTemplate = false;
                //Parent.RequestUnexpand();
            }
        }

        
        public void MoveToArchive() {
            // TODO maybe add archiving
        }


        public async Task UpdateColorPallete() {
            var pallete = new List<string>{
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor1,
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor3,
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor3,
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor4,
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor5
                };

            var tagColors = await MpDataModelProvider.Instance.GetTagColorsForCopyItem(CopyItemId);

            pallete.InsertRange(0, tagColors);

            ColorPallete = pallete.Take(5).ToArray();

            await TitleSwirlViewModel.InitializeAsync();
        }

        #region IDisposable

        public override void Dispose() {
            base.Dispose();
            PropertyChanged -= MpContentItemViewModel_PropertyChanged;
            TemplateCollection.Dispose();
            TitleSwirlViewModel.Dispose();
        }

        #endregion

        #endregion

        #region Protected Methods

        #region Db Events
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpShortcut sc) {
                if(sc.CommandId == CopyItemId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandId == CopyItemId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if(e is MpCopyItem ci) {
                if(ci.Id == CopyItemId) {

                }
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandId == CopyItemId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private void ReceivedDragDropManagerMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ItemDragBegin:
                    if(IsSelected) {
                        IsItemDragging = true;
                    }
                    break;
                case MpMessageType.ItemDragEnd:
                    IsItemDragging = false;
                    break;
            }
        }

        private void UpdateDetails() {
            MpHelpers.Instance.RunOnMainThread((System.Action)(async () => {
                _detailIdx = 1;
                switch (CopyItem.ItemType) {
                    case MonkeyPaste.MpCopyItemType.Image:
                        var bmp = CopyItem.ItemData.ToBitmapSource();
                        itemSize = new Size(bmp.Width, bmp.Height);
                        break;
                    case MonkeyPaste.MpCopyItemType.FileList:
                        var fl = await MpCopyItemMerger.Instance.GetFileList(CopyItem);
                        fc = fl.Count;
                        ds = MpHelpers.Instance.FileListSize(fl.ToArray());
                        break;
                    case MonkeyPaste.MpCopyItemType.RichText:
                        lc = MpHelpers.Instance.GetRowCount(CopyItem.ItemData.ToPlainText());
                        cc = CopyItem.ItemData.ToPlainText().Length;
                        itemSize = CopyItem.ItemData.ToFlowDocument().GetDocumentSize();
                        break;
                }
            }));
        }

        private void MpContentItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):                    
                    if (IsSelected) {
                        LastSubSelectedDateTime = DateTime.Now;
                        Parent.IsSelected = true;
                        if (!MpShortcutCollectionViewModel.Instance.IsMultiSelectKeyDown &&
                            !Parent.Parent.IsRestoringSelection &&
                            !Parent.AllowMultiSelect) {
                            //isolate selection to this tile/item

                            //deselect other tiles
                            MpClipTrayViewModel.Instance.Items
                                .Where(x => x != Parent)
                                .ForEach(y => y.IsSelected = false);

                            //deselect other items
                            Parent.ItemViewModels
                                .Where(x => x != this)
                                .ForEach(y => y.IsSelected = false);
                        }
                        Parent.RequestScrollIntoView(this);
                    }
                    if (ItemIdx > 0) {
                        //trigger so prev item shows/hides separator line
                        var pcivm = Parent.ItemViewModels[ItemIdx - 1];
                        pcivm.OnPropertyChanged(nameof(pcivm.ItemSeparatorBrush));
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsSelected));

                    if (!Parent.Parent.IsRestoringSelection &&
                        Parent.IsSelected && 
                        IsSelected) {
                        Parent.Parent.StoreSelectionState(Parent);
                    }

                    Parent.OnPropertyChanged(nameof(Parent.TileBorderBrush));

                    Parent.Parent.NotifySelectionChanged();
                    break;
                case nameof(CopyItem):
                    if(CopyItem == null) {
                        break;
                    }
                    OnPropertyChanged(nameof(CopyItemData));
                    OnPropertyChanged(nameof(CurrentSize));
                    UpdateDetails();
                    RequestUiUpdate();
                    break;
                case nameof(IsEditingTitle):
                    if(!IsEditingTitle) {
                        Task.Run(async () => {
                            await CopyItem.WriteToDatabaseAsync();
                        });
                    }
                    break;
                case nameof(IsBusy):
                    if(Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    }
                    break;
                case nameof(IsContextMenuOpen):
                    OnPropertyChanged(nameof(ItemBorderBrushRect));
                    //Parent.OnPropertyChanged(nameof(Parent.TileBorderBrush));
                    Parent.OnPropertyChanged(nameof(Parent.TileBorderBrushRect));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyItemContextMenuOpened));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.IsAnyTileContextMenuOpened));
                    break;
                case nameof(IsItemDragging):
                    //Parent.OnPropertyChanged(nameof(Parent.TileBorderBrush));
                    if(IsItemDragging) {
                        StartAnimation();
                    } else {
                        StopAnimation();
                    }
                    OnPropertyChanged(nameof(ItemBorderBrushRect));
                    OnPropertyChanged(nameof(ItemBorderBrush));
                    Parent.OnPropertyChanged(nameof(Parent.TileBorderBrushRect));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyItemDragging));
                    break;
                case nameof(IsHovering):
                    Parent.OnPropertyChanged(nameof(Parent.PrimaryItem));
                    break;
                case nameof(ShortcutKeyString):
                    OnPropertyChanged(nameof(HotkeyIconSource));
                    OnPropertyChanged(nameof(HotkeyIconTooltip));
                    break;
            }
        }

        private void StartAnimation() {
            if(_timer == null) {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(300);
                _timer.Tick += _timer_Tick;
            }
            MpMeasurements.Instance.DottedBorderRect = MpMeasurements.Instance.DottedBorderDefaultRect;

            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e) {
            Rect dbr = MpMeasurements.Instance.DottedBorderRect;
            dbr.Location = new Point(dbr.X + 5, dbr.Y);
            if(dbr.Location.X >= dbr.Width) {
                dbr.Location = new Point(0, dbr.Y);
            }
            MpMeasurements.Instance.DottedBorderRect = dbr;
            OnPropertyChanged(nameof(ItemBorderBrushRect));
            OnPropertyChanged(nameof(ItemBorderBrush));
        }

        private void StopAnimation() {
            MpMeasurements.Instance.DottedBorderRect = MpMeasurements.Instance.DottedBorderDefaultRect;
            OnPropertyChanged(nameof(ItemBorderBrushRect));
            OnPropertyChanged(nameof(ItemBorderBrush));
            _timer.Stop();
        }
        #endregion

        #region Commands

        public ICommand ScrollUpCommand => new RelayCommand(
             () => {
                 NormalizedVerticalScrollOffset = Math.Min(0, NormalizedVerticalScrollOffset - KeyboardScrollAmount);
                 RequestScrollWheelChange(NormalizedVerticalScrollOffset);
             },
             ()=>IsSelected);

        public ICommand ScrollDownCommand => new RelayCommand(
            () => {
                NormalizedVerticalScrollOffset = Math.Min(0, NormalizedVerticalScrollOffset + KeyboardScrollAmount);
                RequestScrollWheelChange(NormalizedVerticalScrollOffset);
            },
            () => IsSelected);


        public ICommand ScrollToHomeCommand => new RelayCommand(
             () => {
                 NormalizedVerticalScrollOffset = 0;
                 RequestScrollWheelChange(NormalizedVerticalScrollOffset);
             },
             () => IsSelected);

        public ICommand ScrollToEndCommand => new RelayCommand(
            () => {
                NormalizedVerticalScrollOffset = 1;
                RequestScrollWheelChange(NormalizedVerticalScrollOffset);
            },
            () => IsSelected);

        public ICommand UnexpandItemCommand => new RelayCommand(
            () => {
                RequestSyncModel();
                ClearEditing();
                OnPropertyChanged(nameof(EditorHeight));
                OnPropertyChanged(nameof(IsEditingContent));
            });

        public ICommand CycleDetailCommand => new RelayCommand(
            () => {
                _detailIdx++;
                if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    _detailIdx = 1;
                }

                // TODO this should aggregate details over all sub items 
                DetailText = GetDetailText((MpCopyItemDetailType)_detailIdx);
            });


        private RelayCommand _sendSubSelectedToEmailCommand;
        public ICommand SendSubSelectedToEmailCommand {
            get {
                if (_sendSubSelectedToEmailCommand == null) {
                    _sendSubSelectedToEmailCommand = new RelayCommand(SendSubSelectedToEmail, CanSendSubSelectedToEmail);
                }
                return _sendSubSelectedToEmailCommand;
            }
        }
        private bool CanSendSubSelectedToEmail() {
            return !IsEditingContent;
        }
        private void SendSubSelectedToEmail() {
            MpHelpers.Instance.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, CopyItem.Title, CopyItem.ItemData.ToPlainText()));
            //MpClipTrayViewModel.Instance.ClearClipSelection();
            //IsSelected = true;
            //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        }

        private RelayCommand _createQrCodeFromSubSelectedItemCommand;
        public ICommand CreateQrCodeFromSubSelectedItemCommand {
            get {
                if (_createQrCodeFromSubSelectedItemCommand == null) {
                    _createQrCodeFromSubSelectedItemCommand = new RelayCommand(CreateQrCodeFromSubSelectedItem, CanCreateQrCodeFromSubSelectedItem);
                }
                return _createQrCodeFromSubSelectedItemCommand;
            }
        }
        private bool CanCreateQrCodeFromSubSelectedItem() {
            return true;//CopyItem.ItemType == MpCopyItemType.RichText && CopyItem.ItemData.ToPlainText().Length <= Properties.Settings.Default.MaxQrCodeCharLength;
        }
        private void CreateQrCodeFromSubSelectedItem() {
            var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(CopyItem.ItemData.ToPlainText());
            //MpClipboardManager.Instance.SetImageWrapper(bmpSrc);
        }


        public ICommand AssignHotkeyCommand => new RelayCommand(
            async () => {
                await MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                    "Paste " + CopyItem.Title,
                    MpClipTrayViewModel.Instance.PasteCopyItemByIdCommand,
                    ShortcutType, CopyItem.Id, ShortcutKeyString);
                OnPropertyChanged(nameof(ShortcutKeyString));
            });

        public ICommand RefreshDocumentCommand {
            get {
                return new RelayCommand(
                    () => {
                        RequestSyncModel();
                        //MessageBox.Show(TemplateCollection.ToString());
                    },
                    () => {
                        return true;// HasModelChanged
                    });
            }
        }

        private RelayCommand _editSubTitleCommand;
        public ICommand EditSubTitleCommand {
            get {
                if (_editSubTitleCommand == null) {
                    _editSubTitleCommand = new RelayCommand(EditSubTitle, CanEditSubTitle);
                }
                return _editSubTitleCommand;
            }
        }
        private bool CanEditSubTitle() {
            if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return false;
            }
            return MpClipTrayViewModel.Instance.SelectedItems.Count == 1 &&
                   base.Parent.SelectedItems.Count == 1;
        }
        private void EditSubTitle() {
            IsEditingTitle = !IsEditingTitle;
        }

        public ICommand ChangeColorCommand => new RelayCommand<Brush>(
            async (b) => {
                CopyItem.ItemColor = b.ToHex();
                TitleSwirlViewModel.ForceBrush(b);
                await CopyItem.WriteToDatabaseAsync();
            });

        public ICommand BringToFrontCommand {
            get {
                return new RelayCommand(
                    () => {
                        Parent.BringToFrontCommand.Execute(null);
                    },
                    () => {
                        return Parent.BringToFrontCommand.CanExecute(null);
                    });
            }
        }


        public ICommand CopyCommand {
            get {
                return new RelayCommand(
                    () => {
                        MpClipboardManager.Instance.CopyItemsToClipboard(new List<MpCopyItem> { CopyItem });
                    });
            }
        }

        #endregion

    }
}
