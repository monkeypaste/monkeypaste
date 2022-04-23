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
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpContentItemViewModel : 
        MpViewModelBase<MpClipTileViewModel>, 
        MpIShortcutCommand,
        MpIHoverableViewModel,
        MpIUserColorViewModel,
        MpISelectableViewModel {
        #region Private Variables
        private DispatcherTimer _timer;

        Size itemSize;
        int fc = 0, lc = 0, cc = 0;
        double ds = 0;

        private int _detailIdx = 0;

        #endregion

        public static bool USING_BROWSER = false;

        #region Properties

        #region View Models

        public MpImageAnnotationCollectionViewModel DetectedImageObjectCollectionViewModel { get; set; }

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

        public MpSourceViewModel SourceViewModel {
            get {
                //if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                //    return null;
                //}
                var svm = MpSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == SourceId); 
                if (svm == null) {
                    return MpSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == MpPreferences.ThisAppSource.Id);
                }
                return svm;
            }
        }

        public MpAppViewModel AppViewModel {
            get {
                //if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                //    return null;
                //}
                return SourceViewModel.AppViewModel;
            }
        }

        public MpUrlViewModel UrlViewModel {
            get {
                //if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                //    return null;
                //}
                return SourceViewModel.UrlViewModel;
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

        #region MpIUserColorViewModel Implementation

        public string UserHexColor {
            get => CopyItemHexColor;
            set => CopyItemHexColor = value;
        }

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

        public string ItemBackgroundHexColor {
            get {
                if(MpDragDropManager.IsDragAndDrop || Parent == null) {
                    return MpSystemColors.White;
                }
                if (IsHovering &&
                    ((!Parent.IsContentReadOnly && !IsSelected) || Parent.IsContentReadOnly) &&
                    Parent.Count > 1) {
                    return MpColorHelpers.GetLighterHexColor(CopyItemHexColor, 0.75);
                }
                return MpSystemColors.White;
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
                    CopyItemType == MpCopyItemType.FileList ||
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
                if(USING_BROWSER) {
                    double h;
                    if (!Parent.IsContentReadOnly) {
                        if (Parent.Count > 1) {
                            h = Double.NaN;
                        }
                        //return Parent.TileContentHeight; //quil editor height
                        h = Parent.TileContentHeight;// - MpMeasurements.Instance.ClipTileEditToolbarHeight - 15;
                    } else {

                        h = ReadOnlyContentSize.Height;
                    }
                    if (double.IsInfinity(h)) {
                        return Double.NaN;
                    }
                    return h;
                } else {

                    if (!Parent.IsContentReadOnly) {
                        if (Parent.Count > 1) {
                            return Double.NaN;
                        }
                        return Parent.TileContentHeight - MpMeasurements.Instance.ClipTileEditToolbarHeight - 15;
                    }
                    return ReadOnlyContentSize.Height;
                }
            }
        }

        public Size EditableContentSize {
            get {
                if (Parent == null || CopyItem == null) {
                    return new Size();
                }
                //get contents actual size
                var ds = UnformattedContentSize;//CopyItemData.ToFlowDocument().GetDocumentSize();

                //if item's content is larger than expanded width make sure it gets that width (will show scroll bars)
                double w = Math.Max(ds.Width, MpMeasurements.Instance.ClipTileContentMinMaxWidth);

                //let height in expanded mode match content's height
                double h = ds.Height;

                return new Size(w, h);
            }
        }

        public Size ReadOnlyContentSize {
            get {
                if (Parent == null || CopyItem == null) {
                    return new Size();
                }
                //item height is divided evenly by items but if there are many (more than 5) 
                //their will only be 5 visible

                double h;
                if(Parent.Count == 1) {
                    h = Parent.TileContentHeight; //MpMeasurements.Instance.ClipTileContentHeight;
                } else {
                    h = Math.Max(
                                        Parent.TileContentHeight / Parent.VisibleItems.Count,
                                        MpMeasurements.Instance.ClipTileContentItemMinHeight);
                }
                return new Size(
                            MpMeasurements.Instance.ClipTileContentDefaultWidth,
                            h);
            }
        }

        public Size CurrentSize {
            get {
                if (Parent == null) {
                    return new Size();
                }
                if (!Parent.IsContentReadOnly) {
                    return EditableContentSize;
                }
                return ReadOnlyContentSize;
            }
        }

        public Size UnformattedContentSize { get; set; }
        //    get {
        //        if(CopyItem == null) {
        //            return new Size();
        //        }
        //        if(CopyItemType == MpCopyItemType.Text) {
        //            CopyItemData.ToFlowDocument(out Size docSize);
        //            return docSize;
        //        } else if(CopyItemType == MpCopyItemType.Image) {
        //            return itemSize;
        //        }
        //        return new Size(CopyItemData.Length * 12, 12);
        //    }
        //}

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

        public bool HasBeenSeen { get; set; } = false;

        public bool IsVisible {
            get {
                if(Parent == null) {
                    return false;
                }
                double screenX = Parent.TrayX - Parent.Parent.ScrollOffset;
                return screenX >= 0 && screenX < Parent.Parent.ClipTrayScreenWidth;
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

        public bool IsTitleReadOnly { get; set; } = true;

        public bool IsContentFocused { get; set; } = false;

        public bool IsTitleFocused { get; set; } = false;

        //public bool IsEditingContent => IsContentFocused && !IsContentReadOnly;

        public bool IsEditingTitle => IsTitleFocused && IsSelected;

        public bool IsEditingTemplate {
            get {
                if(CopyItem == null || TemplateCollection == null) {
                    return false;
                }

                return TemplateCollection.Templates.Any(x => x.IsEditingTemplate);
            }
        }

        public bool IsPasting { get; set; } = false;

        public bool IsPastingTemplate => IsPasting && HasTemplates;


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
                return Parent.Items.IndexOf(this);
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

        public Uri QuillEditorUri => new Uri(Path.Combine(Environment.CurrentDirectory, "Resources/Html/Editor/index.html"));

        public string QuillEditorPath => Path.Combine(Environment.CurrentDirectory, "Resources/Html/Editor/index.html");

        public string TemplateRichText { get; set; }

        public string DetailText { get; set; }

        #endregion

        #region Icons

        public string HotkeyIconSource {
            get {
                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                    return MpBase64Images.JoystickUnset;
                }
                return MpBase64Images.JoystickActive;
            }
        }

        #endregion

        #region Model

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
                    HasModelChanged = true;
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
                    HasModelChanged = true;
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
                    HasModelChanged = true;
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

        public string CopyItemGuid {
            get {
                if(CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Guid;
            }
        }

        public string RootCopyItemGuid {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.RootCopyItemGuid;
            }
            set {
                if (RootCopyItemGuid != value) {
                    CopyItem.RootCopyItemGuid = value;
                    OnPropertyChanged(nameof(RootCopyItemGuid));
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
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemData));
                    OnPropertyChanged(nameof(CurrentSize));
                }
            }
        }


        public int IconId {
            get {
                if(CopyItem == null) {
                    return 0;
                }
                if(CopyItem.IconId > 0) {
                    return CopyItem.IconId;
                }
                if (SourceViewModel == null) {
                    // BUG currently when plugin creates new content it is not setting source info
                    // so return app icon
                    return MpPreferences.ThisAppSource.PrimarySource.IconId;
                }
                return SourceViewModel.PrimarySource.IconId;                
            }
            set {
                if(IconId != value) {
                    CopyItem.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        public string CopyItemHexColor {
            get {
                if(CopyItem == null || string.IsNullOrEmpty(CopyItem.ItemColor)) {
                    return MpColorHelpers.GetRandomHexColor();
                }
                return CopyItem.ItemColor;
            }
            set {
                if (CopyItemHexColor != value) {
                    CopyItem.ItemColor = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemHexColor));
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

            MpMessenger.Unregister<MpMessageType>(typeof(MpDragDropManager), ReceivedDragDropManagerMessage);

            //if (ci != null && ci.Source == null) {
            //    ci.Source = await MpDb.GetItemAsync<MpSource>(ci.SourceId);
            //}
            CopyItem = ci;

            IsNewAndFirstLoad = !MpMainWindowViewModel.Instance.IsMainWindowLoading;

            TemplateCollection = new MpTemplateCollectionViewModel(this);
            TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);

            await TitleSwirlViewModel.InitializeAsync();

            DetailText = GetDetailText((MpCopyItemDetailType)_detailIdx);

            if(CopyItemType == MpCopyItemType.Image) {
                DetectedImageObjectCollectionViewModel = new MpImageAnnotationCollectionViewModel(this);
                await DetectedImageObjectCollectionViewModel.InitializeAsync(CopyItem);
                OnPropertyChanged(nameof(HasDetectedObjects));
            }

            RequestUiUpdate();
            OnPropertyChanged(nameof(EditorHeight));
            OnPropertyChanged(nameof(ItemBorderBrush));
            OnPropertyChanged(nameof(ShortcutKeyString));

            MpMessenger.Register<MpMessageType>(typeof(MpDragDropManager), ReceivedDragDropManagerMessage);

            IsBusy = false;
        }

        #region UI Invokers

        public void RequestFitContent() {
            OnFitContentRequest?.Invoke(this, null);
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

                    info = "Copied " + CopyItemCreatedDateTime.ToReadableTimeSpan();
                    break;
                //chars/lines
                case MpCopyItemDetailType.DataSize:
                    if (CopyItem.ItemType == MpCopyItemType.Image) {
                        info = "(" + (int)itemSize.Width + "px) x (" + (int)itemSize.Height + "px)";
                    } else if (CopyItem.ItemType == MpCopyItemType.Text) {
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
                    if (SourceViewModel == null || SourceViewModel.UrlViewModel == null) {
                        _detailIdx++;
                        info = GetDetailText((MpCopyItemDetailType)_detailIdx);
                    } else {
                        info = SourceViewModel.UrlViewModel.UrlPath;
                    }
                    break;
                case MpCopyItemDetailType.AppInfo:
                    if (SourceViewModel == null || SourceViewModel.AppViewModel == null) {
                        _detailIdx++;
                        info = GetDetailText((MpCopyItemDetailType)_detailIdx);
                    } else {
                        info = SourceViewModel.AppViewModel.AppPath;
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
            //IsEditingTitle = false;
            IsTitleReadOnly = true;
            Parent.IsContentReadOnly = true;
            TemplateCollection?.ClearAllEditing();
            if (IsPasting) {
                IsPasting = false;
                //Parent.RequestUnexpand();
            }
        }
        
        public void MoveToArchive() {
            // TODO maybe add archiving
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
            } else if(e is MpImageAnnotation dio) {
                if(dio.CopyItemId == CopyItemId) {
                    MpHelpers.RunOnMainThread(async () => {
                        if(DetectedImageObjectCollectionViewModel == null) {
                            DetectedImageObjectCollectionViewModel = new MpImageAnnotationCollectionViewModel(this);
                        }
                        await DetectedImageObjectCollectionViewModel.InitializeAsync(CopyItem);
                        OnPropertyChanged(nameof(HasDetectedObjects));
                    });
                }
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandId == CopyItemId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if(e is MpCopyItem ci && ci.Id == CopyItemId) {
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
            _detailIdx = 1;
            switch (CopyItem.ItemType) {
                case MonkeyPaste.MpCopyItemType.Image:
                    var bmp = CopyItem.ItemData.ToBitmapSource();
                    itemSize = new Size(bmp.Width, bmp.Height);
                    break;
                case MonkeyPaste.MpCopyItemType.FileList:
                    var fl = MpCopyItemMerger.Instance.GetFileList(CopyItem);
                    fc = fl.Count;
                    ds = MpHelpers.FileListSize(fl.ToArray());
                    break;
                case MonkeyPaste.MpCopyItemType.Text:
                    lc = MpWpfStringExtensions.GetRowCount(CopyItem.ItemData.ToPlainText());
                    cc = CopyItem.ItemData.ToPlainText().Length;
                    itemSize = UnformattedContentSize;//CopyItem.ItemData.ToFlowDocument().GetDocumentSize();
                    break;
            }
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
                            Parent.Items
                                .Where(x => x != this)
                                .ForEach(y => y.IsSelected = false);
                        }
                        //Parent.RequestScrollIntoView(this);

                        if(!IsTitleFocused) {
                            IsContentFocused = true;
                        }
                    }
                    if (ItemIdx > 0) {
                        //trigger so prev item shows/hides separator line
                        var pcivm = Parent.Items[ItemIdx - 1];
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
                    if (IsEditingTitle) {
                        if (!IsSelected) {
                            IsSelected = true;
                        }
                        if(!IsTitleFocused) {
                            IsTitleFocused = true;
                        }
                    } else {
                        IsTitleFocused = false;
                    }
                    break;
                case nameof(IsTitleFocused):
                    if(IsTitleFocused) {
                        if(!IsEditingTitle) {

                        }
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
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        Task.Run(async () => {
                            await CopyItem.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(CopyItemHexColor):
                    if(TitleSwirlViewModel != null) {
                        //is null on init when CopyItem is set
                        MpHelpers.RunOnMainThread(async () => {
                            await TitleSwirlViewModel.InitializeAsync();
                        });
                    }
                    break;
            }
        }

        private void StartAnimation() {
            return;
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
            //MpConsole.WriteLine("Border Brush loc: " + dbr.Location);
            MpMeasurements.Instance.DottedBorderRect = dbr;
            OnPropertyChanged(nameof(ItemBorderBrushRect));
            OnPropertyChanged(nameof(ItemBorderBrush));
        }

        private void StopAnimation() {
            return;
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


        public ICommand CycleDetailCommand => new RelayCommand(
            () => {
                _detailIdx++;
                if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    _detailIdx = 1;
                }

                // TODO this should aggregate details over all sub items 
                DetailText = GetDetailText((MpCopyItemDetailType)_detailIdx);
            });



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
            return true;//CopyItem.ItemType == MpCopyItemType.RichText && CopyItem.ItemData.ToPlainText().Length <= MpPreferences.MaxQrCodeCharLength;
        }
        private void CreateQrCodeFromSubSelectedItem() {
            var bmpSrc = MpHelpers.ConvertUrlToQrCode(CopyItem.ItemData.ToPlainText());
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
            //IsEditingTitle = !IsEditingTitle;
            IsTitleReadOnly = false;
        }

        //public string GetColor() {
        //    return CopyItemHexColor;
        //}

        //public ICommand SetColorCommand => new RelayCommand<string>(
        //    async (args) => {
        //        CopyItemHexColor = args as string;
        //        await CopyItem.WriteToDatabaseAsync();

        //        TitleSwirlViewModel.ForceBrush(CopyItemHexColor.ToSolidColorBrush());

        //        MpContextMenuView.Instance.CloseMenu();
        //    });

        public ICommand ChangeColorCommand => new RelayCommand<Brush>(
            (b) => {
                CopyItemHexColor = b.ToHex();
            });



        public ICommand CopyCommand {
            get {
                return new RelayCommand(
                    () => {
                       // MpClipboardManager.Instance.CopyItemsToClipboard(new List<MpCopyItem> { CopyItem });
                    });
            }
        }

        #endregion

    }
}
