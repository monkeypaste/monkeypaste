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
    public class MpContentItemViewModel : MpViewModelBase<MpClipTileViewModel> {
        private static string _unsetJoystickIcon64 = "";
        private static string _setJoyStickIcon64 = "";

        #region Abstract Methods
        

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
                if (IsSelected) {
                    return Brushes.DarkGray;
                }
                if (IsHovering) {
                    return Brushes.DimGray;
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

        public Visibility TileDetailGridVisibility {
            get {
                if (Parent.IsAnyEditingContent || Parent.IsAnyEditingTemplate || Parent.IsAnyPastingTemplate) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        #endregion

        #region Layout

        #endregion

        #region State

        public bool WasAddedAtRuntime {
            get {
                if(CopyItem == null) {
                    return true;
                }
                return MpPreferences.Instance.StartupDateTime < CopyItem.CopyDateTime;
            }
        }

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

        private int _detailIdx = 1;
        public int DetailIdx {
            get {
                return _detailIdx;
            }
            set {
                if (_detailIdx != value) {
                    _detailIdx = value;
                    OnPropertyChanged(nameof(DetailIdx));
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

        public string DetailText {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                _detailIdx++;
                if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    _detailIdx = 1;
                }
                // TODO this should aggregate details over all sub items 
                return GetDetail((MpCopyItemDetailType)_detailIdx);
            }
        }
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

            TemplateCollection = new MpTemplateCollectionViewModel(this);
            TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);
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
            Size itemSize;
            int fc = 0, lc = 0, cc = 0;
            double ds = 0;
            switch (CopyItem.ItemType) {
                case MpCopyItemType.Image:
                    var bmp = CopyItem.ItemData.ToBitmapSource();
                    itemSize = new Size(bmp.Width, bmp.Height);
                    break;
                case MpCopyItemType.FileList:
                    fc = GetFileList().Count;
                    ds = MpHelpers.Instance.FileListSize(GetFileList().ToArray());
                    break;
                case MpCopyItemType.RichText:
                    lc = MpHelpers.Instance.GetRowCount(CopyItem.ItemData.ToPlainText());
                    cc = CopyItem.ItemData.ToPlainText().Length;
                    itemSize = CopyItem.ItemData.ToFlowDocument().GetDocumentSize();
                    break;
            }
            string info = "I dunno";// string.Empty;
            switch (detailType) {
                //created
                case MpCopyItemDetailType.DateTimeCreated:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc
                    info = "Copied " + CopyItem.CopyDateTime.ToString();
                    break;
                //chars/lines
                case MpCopyItemDetailType.DataSize:
                    if (CopyItem.ItemType == MpCopyItemType.Image) {
                        var bmp = CopyItem.ItemData.ToBitmapSource();
                        info = "(" + (int)bmp.Width + "px) x (" + (int)bmp.Height + "px)";
                    } else if (CopyItem.ItemType == MpCopyItemType.RichText) {
                        info = cc + " chars | " + lc + " lines";
                    } else if (CopyItem.ItemType == MpCopyItemType.FileList) {
                        info = fc + " files | " + ds + " MB";
                    }
                    break;
                //# copies/# pastes
                case MpCopyItemDetailType.UsageStats:
                    info = cc + " copies | " + CopyItem.PasteCount + " pastes";
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

        private void UpdateDetails() {
            Size itemSize;
            int fc, lc, cc;
            double ds;
            switch (CopyItem.ItemType) {
                case MpCopyItemType.Image:
                    var bmp = CopyItem.ItemData.ToBitmapSource();
                    itemSize = new Size(bmp.Width, bmp.Height);
                    break;
                case MpCopyItemType.FileList:
                    fc = GetFileList().Count;
                    ds = MpHelpers.Instance.FileListSize(GetFileList().ToArray());
                    break;
                case MpCopyItemType.RichText:
                    lc = MpHelpers.Instance.GetRowCount(CopyItem.ItemData.ToPlainText());
                    cc = CopyItem.ItemData.ToPlainText().Length;
                    itemSize = CopyItem.ItemData.ToFlowDocument().GetDocumentSize();
                    break;
            }
        }

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

        public List<string> GetFileList(string baseDir = "", MpCopyItemType forceType = MpCopyItemType.None) {
            //returns path of tmp file for rt or img and actual paths of filelist
            var fileList = new List<string>();
            if (CopyItem.ItemType == MpCopyItemType.FileList) {
                if (forceType == MpCopyItemType.Image) {
                    fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(Path.GetTempFileName(), CopyItem.ItemData.ToBitmapSource()));
                } else if (forceType == MpCopyItemType.RichText) {
                    fileList.Add(MpHelpers.Instance.WriteTextToFile(Path.GetTempFileName(), CopyItem.ItemData.ToRichText()));
                } else {
                    var splitArray = CopyItem.ItemData.ToPlainText().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitArray == null || splitArray.Length == 0) {
                        throw new Exception("CopyItem GetFileList error, file list should not be empty");
                    } else {
                        foreach (string p in splitArray) {
                            if (!string.IsNullOrEmpty(p.Trim())) {
                                fileList.Add(p);
                            }
                        }
                    }
                }
            } else {
                string op = Path.GetTempFileName();// MpHelpers.Instance.GetUniqueFileName((forceType == MpCopyItemType.None ? CopyItemType:forceType),Title,baseDir);
                //file extension
                switch (CopyItem.ItemType) {
                    case MpCopyItemType.RichText:
                        if (forceType == MpCopyItemType.Image) {
                            fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, CopyItem.ItemData.ToBitmapSource()));
                        } else {
                            fileList.Add(MpHelpers.Instance.WriteTextToFile(op, CopyItem.ItemData.ToRichText()));
                        }
                        foreach (var cci in MpCopyItem.GetCompositeChildren(CopyItem)) {
                            if (forceType == MpCopyItemType.Image) {
                                fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, CopyItem.ItemData.ToBitmapSource()));
                            } else {
                                fileList.Add(MpHelpers.Instance.WriteTextToFile(op, CopyItem.ItemData.ToRichText()));
                            }
                            op = Path.GetTempFileName(); //MpHelpers.Instance.GetUniqueFileName((forceType == MpCopyItemType.None ? CopyItemType : forceType), Title, baseDir);
                        }
                        break;
                    case MpCopyItemType.Image:
                        if (forceType == MpCopyItemType.RichText) {
                            fileList.Add(MpHelpers.Instance.WriteTextToFile(op, CopyItem.ItemData.ToPlainText()));
                        } else {
                            fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, CopyItem.ItemData.ToBitmapSource()));
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(baseDir) && Application.Current.MainWindow.DataContext != null) {
                //for temporary files add to mwvm list for shutdown cleanup
                foreach (var fp in fileList) {
                    ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).AddTempFile(fp);
                }
            }
            // add temp files to 
            return fileList;
        }


        public void SaveToDatabase() {
            CopyItem.WriteToDatabase();
        }

        public void RemoveFromDatabase() {
            CopyItem.DeleteFromDatabase();
        }

        public void MoveToArchive() {
            // TODO maybe add archiving
        }

        #endregion

        #region Private Methods

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
            }
        }

        #endregion

        #region Commands
        

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
        #endregion
    }
}
