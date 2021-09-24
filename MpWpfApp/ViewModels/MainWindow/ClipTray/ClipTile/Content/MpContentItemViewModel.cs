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
                    OnPropertyChanged(nameof(TitleSwirlViewModel));
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
                    OnPropertyChanged(nameof(TemplateCollection));
                }
            }
        }

        private ObservableCollection<MpContextMenuItemViewModel> _tagMenuItems = new ObservableCollection<MpContextMenuItemViewModel>();
        public ObservableCollection<MpContextMenuItemViewModel> TagMenuItems {
            get {
                if (MainWindowViewModel == null || MainWindowViewModel.TagTrayViewModel == null) {
                    return _tagMenuItems;
                }
                _tagMenuItems.Clear();
                foreach (var tagTile in MpTagTrayViewModel.Instance.TagTileViewModels) {
                    if (tagTile.IsSudoTag) {
                        continue;
                    }
                    _tagMenuItems.Add(
                        new MpContextMenuItemViewModel(
                            tagTile.TagName,
                            MpClipTrayViewModel.Instance.LinkTagToCopyItemCommand,
                            tagTile,
                            tagTile.IsLinked(CopyItem),
                            string.Empty,
                            null,
                            tagTile.ShortcutKeyString,
                            tagTile.Color));
                }
                return _tagMenuItems;
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
                    OnPropertyChanged(nameof(TitleBackgroundColor));
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

        #endregion

        #region Visibility 



        #endregion

        #region Layout

        #endregion

        #region State

        public bool IsNewAndFirstLoad { get; set; } = false;

        public Cursor RtbCursor {
            get {
                if(IsEditingContent) {
                    return Cursors.IBeam;
                }
                return Cursors.Arrow;
            }
        }
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


        public DateTime LastSubSelectedDateTime { get; set; }

        private bool _isPrimarySubSelected = false;
        public bool IsPrimarySubSelected {
            get {
                return _isPrimarySubSelected;
            }
            set {
                if (_isPrimarySubSelected != value) {
                    _isPrimarySubSelected = value;
                    OnPropertyChanged(nameof(IsPrimarySubSelected));
                    //OnPropertyChanged(nameof(RtbListBoxItemBorderBrush));
                }
            }
        }

        public bool IsSelected { get; set; } = false;
        public bool IsHovering { get; set; } = false;
        public bool IsContextMenuOpen { get; set; } = false;

        public bool IsEditingContent { get; set; } = false;
        public bool IsEditingTemplate {
            get {
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

        public bool HasModelChanged { get; set; } = false;
        public bool HasTemplates {
            get {
                return TemplateCollection.Templates.Count > 0;
            }
        }

        #region Drag & Drop

        public bool IsOverDragButton { get; set; } = false;
        public bool IsSubDragging { get; set; } = false;
        public bool IsSubDropping { get; set; } = false;
        public Point MouseDownPosition { get; set; }
        public IDataObject DragDataObject { get; set; }

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
                if(CopyItem == null) {
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
                    OnPropertyChanged(nameof(ShortcutKeyString));
                    OnPropertyChanged(nameof(HotkeyIconSource));
                    OnPropertyChanged(nameof(HotkeyIconTooltip));
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

        public MpCopyItem CopyItem { get; set; }

        #endregion

        #endregion

        #region Events

        public event EventHandler<int> OnScrollWheelRequest;
        public event EventHandler OnUiUpdateRequest;
        public event EventHandler OnSubSelected;


        public event EventHandler<bool> OnUiResetRequest;
        public event EventHandler OnClearTemplatesRequest;
        public event EventHandler OnCreateTemplatesRequest;
        public event EventHandler OnSyncModels;
        #endregion

        #region Public Methods

        public MpContentItemViewModel() : base(null) { }

        public MpContentItemViewModel(MpClipTileViewModel container, MpCopyItem ci) : base(container) {
            PropertyChanged += MpContentItemViewModel_PropertyChanged;
            CopyItem = ci;

            IsNewAndFirstLoad = !MpMainWindowViewModel.IsMainWindowLoading;
            TemplateCollection = new MpTemplateCollectionViewModel(this);
            TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);

            CycleDetailCommand.Execute(null);
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

        public void Resize(Rect newSize) {
            //throw new Exception("Unemplemented");
        }

        public Size GetExpandedSize() {
            var ds = CopyItem.ItemData.ToFlowDocument().GetDocumentSize();
            return new Size(
                Math.Max(ds.Width, MpMeasurements.Instance.ClipTileContentMinMaxWidth),
                Math.Max(ds.Height, MpMeasurements.Instance.ClipTileContentHeight)
                );
        }

        public Size GetUnexpandedSize() {
            double h = base.Parent.ItemViewModels.Count > 1 ?
                            MpMeasurements.Instance.RtbCompositeItemMinHeight :
                            MpMeasurements.Instance.ClipTileContentHeight;

            return new Size(MpMeasurements.Instance.ClipTileContentMinWidth, h);
        }

        public string GetDetail(MpCopyItemDetailType detailType) {
            if(CopyItem == null) {
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

        public void ClearSubDragState() {
            IsSubDragging = false;
            DragDataObject = null;
            MouseDownPosition = new Point();
        }

        public void ClearEditing() {
            IsEditingContent = false;
            IsEditingTitle = false;
            TemplateCollection.ClearAllEditing();
            if(IsPastingTemplate) {
                IsPastingTemplate = false;
                MainWindowViewModel.ShrinkClipTile(Parent);                
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
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpCopyItem ci) {
                if(ci.Id == CopyItem.Id) {
                }
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if (ci.Id == CopyItem.Id) {
                    CopyItem = ci;
                    MpConsole.WriteTraceLine("Reset model from db callback");
                }
            }
        }
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
                        OnSubSelected?.Invoke(this, null);
                    }
                    break;
                case nameof(IsEditingContent):
                    if(IsEditingContent) {
                        MainWindowViewModel.ExpandClipTile(Parent);
                    } else {
                        Parent.SaveToDatabase();
                        MainWindowViewModel.ShrinkClipTile(Parent);
                    }
                    break;
                case nameof(CopyItem):
                    UpdateDetails();
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
                DetailText = GetDetail((MpCopyItemDetailType)_detailIdx);                
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
                        return MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1 &&
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
            return MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1;
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
                    ()=> {
                        RequestSyncModels();
                    },
                    ()=> {
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
            return MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1 &&
                   base.Parent.SelectedItems.Count == 1;
        }
        private void EditSubTitle() {
            IsEditingTitle = !IsEditingTitle;
        }


        public ICommand ChangeColorCommand {
            get {
                return new RelayCommand<Brush>(
                    (b)=> {
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
