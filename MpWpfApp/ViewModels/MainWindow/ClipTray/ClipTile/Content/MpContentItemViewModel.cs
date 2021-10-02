using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AsyncAwaitBestPractices.MVVM;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using SQLite;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace MpWpfApp {
    public class MpContentItemViewModel : MpViewModelBase<MpClipTileViewModel>, MpIContentCommands {
        private static string _unsetJoystickIcon64 = "";
        private static string _setJoyStickIcon64 = "";

        #region Private Variables
        Size itemSize;
        int fc = 0, lc = 0, cc = 0;
        double ds = 0;

        private int _detailIdx = 0;
        #endregion

        #region Properties

        #region View Models

        private MpClipTileTitleSwirlViewModel _clipTileTitleSwirlViewModel = null;
        public MpClipTileTitleSwirlViewModel TitleSwirlViewModel {
            get {
                return _clipTileTitleSwirlViewModel;
            }
            set {
                if (_clipTileTitleSwirlViewModel != value) {
                    _clipTileTitleSwirlViewModel = value;
                    OnPropertyChanged_old(nameof(TitleSwirlViewModel));
                }
            }
        }


        private MpTemplateCollectionViewModel _templateCollection;
        public MpTemplateCollectionViewModel TemplateCollection {
            get {
                return _templateCollection;
            }
            set {
                if (_templateCollection != value) {
                    _templateCollection = value;
                    OnPropertyChanged_old(nameof(TemplateCollection));
                }
            }
        }

        public MpContentContextMenuViewModel ContextMenuViewModel { get; set; }


        public List<MpContextMenuItemViewModel> TagMenuItems {
            get {
                var tmil = new List<MpContextMenuItemViewModel>();

                if (Parent == null) {
                    return tmil;
                }
                foreach (var tagTile in MpTagTrayViewModel.Instance.TagTileViewModels) {
                    if (tagTile.IsSudoTag) {
                        continue;
                    }
                    tmil.Add(
                        new MpContextMenuItemViewModel(
                            ContextMenuViewModel,
                            tagTile.TagName,
                            MpClipTrayViewModel.Instance.LinkTagToCopyItemCommand,
                            tagTile,
                            tagTile.IsLinked(CopyItem),
                            string.Empty,
                            null,
                            tagTile.ShortcutKeyString,
                            tagTile.TagColor));
                }
                return tmil;
            }
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

        public Brush TitleBackgroundColor {
            get {
                if (CopyItem == null) {
                    return Brushes.Transparent;
                }
                return new SolidColorBrush(MpHelpers.Instance.ConvertHexToColor(CopyItem.ItemColor));
            }
            set {
                if (CopyItem != null && CopyItem.ItemColor != MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color)) {
                    CopyItem.ItemColor = MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color);
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged_old(nameof(TitleBackgroundColor));
                }
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

        public Brush ItemBackgroundBrush {
            get {
                if (Parent.IsExpanded) {
                    return Brushes.White;
                }
                if (IsHovering && 
                    Parent.Count > 1) {
                    return MpHelpers.Instance.GetLighterBrush(
                        new SolidColorBrush(
                            CopyItem.ItemColor.ToWinMediaColor()),0.75
                        );
                }

                return Brushes.White;

            }
        }

        public Brush ItemBorderBrush {
            get {
                if (Parent == null) {
                    return Brushes.Transparent;
                }
                if (Parent.SelectedItems.Count > 1 && IsSelected) {
                    return Brushes.Red;
                }
                return Brushes.Black;
            }
        }

        
        #endregion

        #region Visibility 



        #endregion

        #region Layout

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
                var ds = CopyItemData.ToFlowDocument().GetDocumentSize();
                double h = Math.Min(
                                ds.Height,
                                Math.Max(
                                        MpMeasurements.Instance.ClipTileContentHeight / Parent.VisibleItems.Count,
                                        MpMeasurements.Instance.ClipTileContentItemMinHeight));
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

        //public Thickness ContentPadding {
        //    get {
        //        double dp = MpMeasurements.Instance.ClipTileContentItemRtbViewPadding;
        //        if (IsHovering && Parent.Count > 1 && !Parent.IsExpanded) {
        //            double dbw = MpMeasurements.Instance.ClipTileContentItemDragButtonSize;
        //            return new Thickness(dp + dbw, dp, dp, dp);
        //        }
        //        return new Thickness(dp);
        //    }
        //}

        public Thickness ItemBorderThickness {
            get {
                if (Parent == null || Parent.ItemViewModels.Count == 1) {
                    return new Thickness(0);
                }
                double bt = MpMeasurements.Instance.ClipTileContentItemBorderThickness;
                if (IsSelected) {
                    return new Thickness(bt, bt, bt, bt);
                }
                if (ItemIdx == 0) {
                    return new Thickness(0, 0, 0, bt);
                } else if (ItemIdx == Parent.Count - 1) {
                    return new Thickness(0, 0, 0, bt);
                }
                return new Thickness(0, 0, 0, bt);
            }
        }
        #endregion

        #region State

        public bool IsNewAndFirstLoad { get; set; } = false;

        private bool _isHoveringOnTitleTextGrid = false;
        public bool IsHoveringOnTitleTextGrid {
            get {
                return _isHoveringOnTitleTextGrid;
            }
            set {
                if (_isHoveringOnTitleTextGrid != value) {
                    _isHoveringOnTitleTextGrid = value;
                    OnPropertyChanged_old(nameof(IsHoveringOnTitleTextGrid));
                    OnPropertyChanged_old(nameof(TileTitleTextGridBackgroundBrush));
                    OnPropertyChanged_old(nameof(TitleTextColor));
                }
            }
        }


        public DateTime LastSubSelectedDateTime { get; set; }

        public bool IsSelected { get; set; } = false;
        public bool IsHovering { get; set; } = false;
        public bool IsContextMenuOpen { get; set; } = false;

        public bool IsEditingContent { get; set; } = false;
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
                    OnPropertyChanged_old(nameof(IsEditingTitle));
                    OnPropertyChanged_old(nameof(TileTitleTextGridBackgroundBrush));
                }
            }
        }

        public bool HasModelChanged { get; set; } = false;
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

        public bool IsPlaceholder { get; set; } = false;


        #region Drag & Drop
        public bool IsOverDragButton { get; set; } = false;
        public bool IsSubDragging { get; set; } = false;
        public bool IsSubDropping { get; set; } = false;
        public Point MouseDownPosition { get; set; }
        public IDataObject DragDataObject { get; set; }

        //public bool IsDragButtonVisible {
        //    get {
        //        if (Parent == null) {
        //            return false;
        //        }
        //        if (Parent.IsExpanded) {
        //            if (IsEditingContent) {
        //                return false;
        //            }
        //        }
        //        return IsHovering && Parent.Count > 1;
        //    }
        //}
        #endregion

        #endregion

        #region Business Logic
        public string TemplateRichText { get; set; }

        public string DetailText { get; set; }

        #endregion

        #region Icons

        public string HotkeyIconSource {
            get {
                if (string.IsNullOrEmpty(_unsetJoystickIcon64)) {
                    _unsetJoystickIcon64 = new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/joystick.png")).ToBase64String();
                }
                if (string.IsNullOrEmpty(_setJoyStickIcon64)) {
                    _setJoyStickIcon64 = new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/joystickactive.png")).ToBase64String();
                }
                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                    return _unsetJoystickIcon64;
                }
                return _setJoyStickIcon64;
            }
        }

        //public BitmapSource HotkeyIconSource {
        //    get {
        //        if (string.IsNullOrEmpty(ShortcutKeyString)) {
        //            return new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/joystick.png"));
        //        }
        //        return new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/joystickactive.png"));
        //    }
        //}

        #endregion

        #region Model

        public string[] ColorPallete {
            get {
                if(CopyItem == null) {
                    return new string[] { };
                }
                return new string[] {
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor1,
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor3,
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor3,
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor4,
                    CopyItem.Source.PrimarySource.SourceIcon.HexColor5
                };
            }
        }

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

        private string _shortcutKeyString = string.Empty;
        public string ShortcutKeyString {
            get {
                return _shortcutKeyString;
            }
            set {
                if (_shortcutKeyString != value) {
                    _shortcutKeyString = value;
                    OnPropertyChanged_old(nameof(ShortcutKeyString));
                    OnPropertyChanged_old(nameof(HotkeyIconSource));
                    OnPropertyChanged_old(nameof(HotkeyIconTooltip));
                }
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

        public bool CopyItemIsChanged {
            get {
                if (CopyItem == null) {
                    return true;
                }
                return CopyItem.IsChanged;
            }
            set {
                if (CopyItem != null && CopyItem.IsChanged != value) {
                    CopyItem.IsChanged = value;
                    OnPropertyChanged(nameof(CopyItemIsChanged));
                }
            }
        }

        public int CompositeSortOrderIdx {
            get {
                if(CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeSortOrderIdx;
            }
            set {
                if(CopyItem != null && CopyItem.CompositeSortOrderIdx != value) {
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

        public event EventHandler<int> OnScrollWheelRequest;
        public event EventHandler OnUiUpdateRequest;
        //public event EventHandler OnSubSelected;


        public event EventHandler<bool> OnUiResetRequest;
        public event EventHandler OnClearTemplatesRequest;
        public event EventHandler OnCreateTemplatesRequest;
        public event EventHandler OnSyncModels;

        #endregion

        #region Public Methods

        public MpContentItemViewModel() : base(null) { }

        public MpContentItemViewModel(MpClipTileViewModel container, MpCopyItem ci) : base(container) {
            PropertyChanged += MpContentItemViewModel_PropertyChanged;
            Initialize(ci);
        }

        public void Initialize(MpCopyItem ci) {
            if (ci == null) {
                IsPlaceholder = true;
            } else {
                IsPlaceholder = false;
            }
            CopyItem = ci;

            IsBusy = true;
            IsNewAndFirstLoad = !MpMainWindowViewModel.IsMainWindowLoading;

            ContextMenuViewModel = new MpContentContextMenuViewModel(this);
            TemplateCollection = new MpTemplateCollectionViewModel(this);
            TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);

            CycleDetailCommand.Execute(null);
            IsBusy = false;
        }
        public async Task GatherAnalytics() {
            var analyticTasks = new List<Task>();
            Task<string> urlTask = null, ocrTask = null, cvTask = null;
            if (CopyItem.SsDbImage != null) {
                urlTask = MpBrowserUrlDetector.Instance.FindUrlAddressFromScreenshot(CopyItem.SsDbImage.ImageBase64.ToBitmapSource());
                analyticTasks.Add(urlTask);
            }

            if (CopyItem.ItemType == MpCopyItemType.Image) {
                var itemBmpBytes = MpHelpers.Instance.ConvertBitmapSourceToByteArray(CopyItem.ItemData.ToBitmapSource());
                ocrTask = MpImageOcr.Instance.OcrImageForText(itemBmpBytes);
                analyticTasks.Add(ocrTask);
                cvTask = MpImageAnalyzer.Instance.AnalyzeImage(itemBmpBytes);
                analyticTasks.Add(cvTask);
            }

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
        public void RequestSyncModels() {
            OnSyncModels?.Invoke(this, null);
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

        #endregion




        public void Resize(Rect newSize) {
            //throw new Exception("Unemplemented");
        }



        public string GetDetailText(MpCopyItemDetailType detailType) {
            if (CopyItem == null) {
                return string.Empty;
            }

            string info = string.Empty;
            switch (detailType) {
                //created
                case MpCopyItemDetailType.DateTimeCreated:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc

                    info = "Copied " + CopyItem.CopyDateTime.ToString();
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
                        info = " Platform | " + CopyItem.Source.App.AppPath;
                    }

                    break;
                default:
                    info = "Unknown detailId: " + (int)detailType;
                    break;
            }

            return info;
        }

        #region View Request Invokers

        public void RequestScrollWheelChange(int delta) {
            //var sv = (ScrollViewer)rtbvm.HostClipTileViewModel.ClipBorder.FindName("ClipTileRichTextBoxListBoxScrollViewer");//RtbLbAdornerLayer.GetVisualAncestor<ScrollViewer>();
            //sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta);
            OnScrollWheelRequest?.Invoke(this, delta);
        }

        public void RequestUiUpdate() {
            OnUiUpdateRequest?.Invoke(this, null);
        }

        #endregion


        public void ClearEditing() {
            IsEditingContent = false;
            IsEditingTitle = false;
            TemplateCollection.ClearAllEditing();
            if (IsPastingTemplate) {
                IsPastingTemplate = false;
                Parent.RequestUnexpand();
            }
        }

        public void SaveToDatabase() {
            CopyItem.WriteToDatabase();
        }

        public void RemoveFromDatabase() {
            Parent.RemoveRange(new List<MpCopyItem>() { CopyItem });
            CopyItem.DeleteFromDatabase();
        }

        public void MoveToArchive() {
            // TODO maybe add archiving
        }

        #endregion

        #region Protected Methods


        #region Db Events
        #endregion

        #endregion

        #region Private Methods
        private void UpdateDetails() {
            _detailIdx = 1;
            switch (CopyItem.ItemType) {
                case MpCopyItemType.Image:
                    var bmp = CopyItem.ItemData.ToBitmapSource();
                    itemSize = new Size(bmp.Width, bmp.Height);
                    break;
                case MpCopyItemType.FileList:
                    var fl = MpCopyItemMerger.Instance.GetFileList(CopyItem);
                    fc = fl.Count;
                    ds = MpHelpers.Instance.FileListSize(fl.ToArray());
                    break;
                case MpCopyItemType.RichText:
                    lc = MpHelpers.Instance.GetRowCount(CopyItem.ItemData.ToPlainText());
                    cc = CopyItem.ItemData.ToPlainText().Length;
                    itemSize = CopyItem.ItemData.ToFlowDocument().GetDocumentSize();
                    break;
            }
        }
        private void MpContentItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSubSelectedDateTime = DateTime.Now;
                    }
                    break;
                case nameof(IsEditingContent):
                    if (IsEditingContent) {
                        Parent.RequestExpand();
                    } else {
                        Parent.RequestUnexpand();
                        RequestSyncModels();
                    }
                    break;
                case nameof(CopyItem):
                    OnPropertyChanged(nameof(CopyItemData));
                    OnPropertyChanged(nameof(CurrentSize));
                    UpdateDetails();
                    break;
                case nameof(IsHovering):
                    Parent.OnPropertyChanged(nameof(Parent.PrimaryItem));
                    break;
                case nameof(IsPlaceholder):
                    ItemVisibility = IsPlaceholder ? Visibility.Hidden : Visibility.Visible;
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand CycleDetailCommand => new RelayCommand(
            () => {
                _detailIdx++;
                if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    _detailIdx = 1;
                }

                // TODO this should aggregate details over all sub items 
                DetailText = GetDetailText((MpCopyItemDetailType)_detailIdx);
            });

        public ICommand EditSubContentCommand {
            get {
                return new RelayCommand(
                    () => {
                        if (!IsEditingContent) {
                            Parent.ClearEditing();
                            Parent.ClearClipSelection();
                            IsSelected = true;
                        }
                    },
                    () => {
                        return MpClipTrayViewModel.Instance.SelectedItems.Count == 1 &&
                               base.Parent.SelectedItems.Count == 1;
                    });
            }
        }

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
            return CopyItem.ItemType == MpCopyItemType.RichText && CopyItem.ItemData.ToPlainText().Length <= Properties.Settings.Default.MaxQrCodeCharLength;
        }
        private void CreateQrCodeFromSubSelectedItem() {
            var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(CopyItem.ItemData.ToPlainText());
            System.Windows.Clipboard.SetImage(bmpSrc);
        }

        private AsyncCommand<string> _translateSubSelectedItemTextAsyncCommand;
        public IAsyncCommand<string> TranslateSubSelectedItemTextAsyncCommand {
            get {
                if (_translateSubSelectedItemTextAsyncCommand == null) {
                    _translateSubSelectedItemTextAsyncCommand = new AsyncCommand<string>(TranslateSubSelectedItemTextAsync, CanTranslateSubSelectedItemText);
                }
                return _translateSubSelectedItemTextAsyncCommand;
            }
        }
        private bool CanTranslateSubSelectedItemText(object args) {
            return CopyItem.ItemType == MpCopyItemType.RichText;
        }
        private async Task TranslateSubSelectedItemTextAsync(string toLanguage) {
            var translatedText = await MpLanguageTranslator.Instance.Translate(CopyItem.ItemData.ToPlainText(), toLanguage, false);
            if (!string.IsNullOrEmpty(translatedText)) {
                CopyItem.ItemData = MpHelpers.Instance.ConvertPlainTextToRichText(translatedText);
            }
        }

        private RelayCommand _excludeSubSelectedItemApplicationCommand;
        public ICommand ExcludeSubSelectedItemApplicationCommand {
            get {
                if (_excludeSubSelectedItemApplicationCommand == null) {
                    _excludeSubSelectedItemApplicationCommand = new RelayCommand(ExcludeSubSelectedItemApplication, CanExcludeSubSelectedItemApplication);
                }
                return _excludeSubSelectedItemApplicationCommand;
            }
        }
        private bool CanExcludeSubSelectedItemApplication() {
            return MpClipTrayViewModel.Instance.SelectedItems.Count == 1;
        }
        private void ExcludeSubSelectedItemApplication() {
            MpAppCollectionViewModel.Instance.UpdateRejection(MpAppCollectionViewModel.Instance.GetAppViewModelByAppId(CopyItem.Source.AppId), true);
        }

        private RelayCommand _pasteSubItemCommand;
        public ICommand PasteSubItemCommand {
            get {
                if (_pasteSubItemCommand == null) {
                    _pasteSubItemCommand = new RelayCommand(PasteSubItem);
                }
                return _pasteSubItemCommand;
            }
        }
        private void PasteSubItem() {
            MpClipTrayViewModel.Instance.ClearClipSelection();
            base.Parent.IsSelected = true;
            base.Parent.ClearClipSelection();
            IsSelected = true;
            MpClipTrayViewModel.Instance.PasteSelectedClipsCommand.Execute(null);
        }

        private RelayCommand _assignHotkeyToSubSelectedItemCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if (_assignHotkeyToSubSelectedItemCommand == null) {
                    _assignHotkeyToSubSelectedItemCommand = new RelayCommand(AssignHotkeyToSubSelectedItem);
                }
                return _assignHotkeyToSubSelectedItemCommand;
            }
        }
        private void AssignHotkeyToSubSelectedItem() {
            ShortcutKeyString = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(
                this,
                "Paste " + CopyItem.Title,
                ShortcutKeyString,
                 MpClipTrayViewModel.Instance.HotkeyPasteCommand, CopyItem.Id);
        }

        public ICommand RefreshDocumentCommand {
            get {
                return new RelayCommand(
                    () => {
                        RequestSyncModels();
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
            if (MpMainWindowViewModel.IsMainWindowLoading) {
                return false;
            }
            return MpClipTrayViewModel.Instance.SelectedItems.Count == 1 &&
                   base.Parent.SelectedItems.Count == 1;
        }
        private void EditSubTitle() {
            IsEditingTitle = !IsEditingTitle;
        }


        public ICommand ChangeColorCommand {
            get {
                return new RelayCommand<Brush>(
                    (b) => {
                        CopyItem.ItemColor = b.ToHex();
                        TitleSwirlViewModel.ForceBrush(b);
                        Task.Run(CopyItem.WriteToDatabase);
                    });
            }
        }

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

        public ICommand CreateQrCodeCommand => throw new NotImplementedException();

        public ICommand DeleteCommand => throw new NotImplementedException();

        public ICommand DuplicateCommand => throw new NotImplementedException();

        public ICommand EditContentCommand => throw new NotImplementedException();

        public ICommand EditTitleCommand => throw new NotImplementedException();

        public ICommand ExcludeApplicationCommand => throw new NotImplementedException();

        public ICommand HotkeyPasteCommand => throw new NotImplementedException();

        public ICommand InvertSelectionCommand => throw new NotImplementedException();

        public ICommand LinkTagToContentCommand => throw new NotImplementedException();

        public ICommand LoadMoreClipsCommand => throw new NotImplementedException();

        public ICommand MergeCommand => throw new NotImplementedException();

        public ICommand PasteCommand => throw new NotImplementedException();

        public ICommand SearchWebCommand => throw new NotImplementedException();

        public ICommand SelectAllCommand => throw new NotImplementedException();

        public ICommand SelectNextCommand => throw new NotImplementedException();

        public ICommand SelectPreviousCommand => throw new NotImplementedException();

        public ICommand SendToEmailCommand => throw new NotImplementedException();

        public ICommand SendToBackCommand => throw new NotImplementedException();

        public ICommand SpeakCommand => throw new NotImplementedException();

        public ICommand TranslateCommand => throw new NotImplementedException();


        #endregion
    }
}
