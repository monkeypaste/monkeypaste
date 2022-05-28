namespace MpWpfApp {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using GalaSoft.MvvmLight.CommandWpf;
    using MonkeyPaste;
    using MonkeyPaste.Plugin;
    using System.Speech.Synthesis;
    using System.Windows.Documents;
using System.Text.RegularExpressions;

    public class MpClipTileViewModel : 
        MpViewModelBase<MpClipTrayViewModel>, 
        MpIShortcutCommand,
        MpISelectableViewModel,
        MpIUserColorViewModel,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpITextSelectionRange,
        MpIFindAndReplaceViewModel {
        #region Private Variables

        private List<string> _tempFileList = new List<string>();
        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private int _lastQueryOffset = -1;

        private DispatcherTimer _timer;

        Size itemSize;
        int fc = 0, lc = 0, cc = 0;
        double ds = 0;

        private int _detailIdx = 0;

        #endregion
        public static bool USING_BROWSER = false;

        #region Statics

        public static double DefaultBorderWidth = MpMeasurements.Instance.ClipTileMinSize - MpMeasurements.Instance.ClipTileMargin;
        public static double DefaultBorderHeight = MpMeasurements.Instance.ClipTileMinSize;

        #endregion

        #region Properties

        #region Property Reflection Referencer
        //public object this[string propertyName] {
        //    get {
        //        // probably faster without reflection:
        //        // like:  return MpPreferences.PropertyValues[propertyName] 
        //        // instead of the following
        //        Type myType = typeof(MpClipTileViewModel);
        //        PropertyInfo myPropInfo = myType.GetProperty(propertyName);
        //        if (myPropInfo == null) {
        //            throw new Exception("Unable to find property: " + propertyName);
        //        }
        //        return myPropInfo.GetValue(this, null);
        //    }
        //    set {
        //        Type myType = typeof(MpClipTileViewModel);
        //        PropertyInfo myPropInfo = myType.GetProperty(propertyName);
        //        myPropInfo.SetValue(this, value, null);
        //    }
        //}
        #endregion

        #region MpIConditionalContentReadOnlyViewModel Implementation
        //bool MpIConditionalContentReadOnlyViewModel.IsReadOnly { 
        //    get; 
        //    set; 
        //}

        //object MpIConditionalContentReadOnlyViewModel.Content { 
        //    get; 
        //    set; 
        //}
        //object MpIConditionalTitleReadOnlyViewModel.Title { get; set; }
        //#endregion

        //#region MpIConditionalTitleReadOnlyViewModel Implementation
        //bool MpIConditionalTitleReadOnlyViewModel.IsReadOnly {
        //    get;
        //    set;
        //}
        #endregion

        #region MpITextSelectionRangeViewModel Implementation 

        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }

        public string SelectedPlainText { get; set; }

        public bool IsAllSelected { get; set; }

        #endregion

        #region MpIUserColorViewModel Implementation

        public string UserHexColor {
            get => CopyItemHexColor;
            set => CopyItemHexColor = value;
        }

        #endregion

        #region MpIFindAndReplaceViewModel Implementation

        private int _currentMatchIdx = 0;
        private List<TextRange> _matches;
        private MpRtbHighlightBehavior _rtbHighligher {
            get {
                var cv = Application.Current.MainWindow.GetVisualDescendents<MpContentView>().FirstOrDefault(x => x.DataContext == this);
                if(cv == null) {
                    return null;
                }
                return cv.RtbHighlightBehavior;
            }
        }
        public bool IsFindAndReplaceVisible { get; set; }
        
        private string _findText;
        public string FindText {
            get {
                if(string.IsNullOrEmpty(_findText) && !IsFindTextBoxFocused) {
                    return FindPlaceholderText;
                }
                return _findText;
            }
            set {
                if(_findText != value && value != FindPlaceholderText) {
                    _findText = value;
                }
                OnPropertyChanged(nameof(FindText));
                OnPropertyChanged(nameof(IsFindValid));
            }
        }
        private string _replaceText;
        public string ReplaceText { 
            get {
                if (string.IsNullOrEmpty(_replaceText) && !IsReplaceTextBoxFocused) {
                    return ReplacePlaceholderText;
                }
                return _replaceText;
            }
            set {
                if(_replaceText != value && value != ReplacePlaceholderText) {
                    _replaceText = value;
                }
                OnPropertyChanged(nameof(ReplaceText));
            }
        }

        public string FindPlaceholderText => "Find...";
        public string ReplacePlaceholderText => "Replace...";

        private bool _isFindTextBoxFocused;
        public bool IsFindTextBoxFocused {
            get => _isFindTextBoxFocused;
            set {
                if(_isFindTextBoxFocused != value) {                    
                    _isFindTextBoxFocused = value;
                    if (IsFindTextBoxFocused && FindText == FindPlaceholderText) {
                        OnPropertyChanged(nameof(FindText));
                    }
                    OnPropertyChanged(nameof(IsFindTextBoxFocused));
                }
            }
        }
        private bool _isReplaceTextBoxFocused;
        public bool IsReplaceTextBoxFocused {
            get => _isReplaceTextBoxFocused;
            set {
                if (_isReplaceTextBoxFocused != value) {
                    _isReplaceTextBoxFocused = value;
                    if (IsReplaceTextBoxFocused && ReplaceText == ReplacePlaceholderText) {
                        OnPropertyChanged(nameof(ReplaceText));
                    }
                    OnPropertyChanged(nameof(IsReplaceTextBoxFocused));
                }
            }
        }

        public bool HasFindText => !string.IsNullOrEmpty(FindText) && FindText != FindPlaceholderText;
        public bool HasReplaceText => !string.IsNullOrEmpty(ReplaceText) && ReplaceText != ReplacePlaceholderText;

        public bool IsReplaceMode { get; set; }
        public bool IsFindValid => string.IsNullOrEmpty(_findText) || (!string.IsNullOrEmpty(_findText) && HasMatch);
        public bool IsReplaceValid => !IsReplaceMode || (IsReplaceMode && _replaceText != null);
        public bool HasMatch => _matches != null && _matches.Count > 0;
        public bool MatchCase { get; set; }
        public bool MatchWholeWord { get; set; }
        public bool UseRegEx { get; set; }

        public ObservableCollection<string> RecentFindTexts {
            get => new ObservableCollection<string>(MpPreferences.RecentFindTexts.Split(new string[] { MpPreferences.STRING_ARRAY_SPLIT_TOKEN },StringSplitOptions.RemoveEmptyEntries));
            set => MpPreferences.RecentFindTexts = string.Join(MpPreferences.STRING_ARRAY_SPLIT_TOKEN, value);
        }
        public ObservableCollection<string> RecentReplaceTexts {
            get => new ObservableCollection<string>(MpPreferences.RecentReplaceTexts.Split(new string[] { MpPreferences.STRING_ARRAY_SPLIT_TOKEN }, StringSplitOptions.RemoveEmptyEntries));
            set => MpPreferences.RecentReplaceTexts = string.Join(MpPreferences.STRING_ARRAY_SPLIT_TOKEN, value);
        }
        public ICommand ToggleFindAndReplaceVisibleCommand => new RelayCommand(
            () => {
                IsFindAndReplaceVisible = !IsFindAndReplaceVisible;

                if(IsFindAndReplaceVisible) {
                    if(IsContentReadOnly) {
                        MpContentDocumentRtfExtension.ExpandContent(this);
                    }
                } else {
                    if (IsContentReadOnly) {
                        MpContentDocumentRtfExtension.UnexpandContent(this);

                        MpContentDocumentRtfExtension.SaveTextContent(
                            MpContentDocumentRtfExtension.FindRtbByViewModel(this))
                        .FireAndForgetSafeAsync(this);
                    }
                    _rtbHighligher.Reset();
                }
                
            },IsTextItem);
        public void UpdateFindAndReplaceMatches() {
            _currentMatchIdx = -1;
            if(_findText == null) {
                _matches = new List<TextRange>();
            } else {
                _matches = MpContentDocumentRtfExtension.FindContent(this, _findText, MatchCase, MatchWholeWord, UseRegEx);
            }
            _rtbHighligher.Reset();
            if (HasMatch) {
                _rtbHighligher.InitHighlighting(_matches);
                FindNextCommand.Execute(null);
            }
            OnPropertyChanged(nameof(IsFindValid));
        }
        public ICommand UpdateFindAndReplaceRecentsCommand => new RelayCommand(() => {
            UpdateFindAndReplaceMatches();

            if (!string.IsNullOrEmpty(_findText)) {
                var rftl = RecentFindTexts;
                int recentFindIdx = rftl.IndexOf(_findText);
                if (recentFindIdx < 0) {
                    rftl.Insert(0, _findText);
                    rftl = new ObservableCollection<string>(rftl.Take(MpPreferences.MaxRecentTextsCount));
                } else {
                    rftl.RemoveAt(recentFindIdx);
                    rftl.Insert(0, _findText);
                }
                RecentFindTexts = rftl;
            }


            if (IsReplaceMode && _replaceText != null) {
                var rrtl = RecentReplaceTexts.ToList();
                int recentReplaceIdx = rrtl.IndexOf(_replaceText);
                if (recentReplaceIdx < 0) {
                    rrtl.Insert(0, _replaceText);
                } else {
                    rrtl.RemoveAt(recentReplaceIdx);
                    rrtl.Insert(0, _replaceText);
                }
                RecentReplaceTexts = new ObservableCollection<string>(rrtl.Take(MpPreferences.MaxRecentTextsCount));
            }
        });
        public ICommand FindNextCommand => new RelayCommand(
            () => {
                if (_matches == null || _matches.Count == 0) {
                    return;
                }
                _currentMatchIdx++;
                if(_currentMatchIdx >= _matches.Count) {
                    _currentMatchIdx = 0;
                }
                _rtbHighligher.SelectedIdx = _currentMatchIdx;
                _rtbHighligher.ApplyHighlighting();
                MpTextSelectionRangeExtension.SetTextSelection(this,_matches[_currentMatchIdx]);
                
            }, HasMatch);

        public ICommand FindPreviousCommand => new RelayCommand(
            () => {
                if (_matches == null || _matches.Count == 0) {
                    return;
                }
                _currentMatchIdx--;
                if (_currentMatchIdx < 0) {
                    _currentMatchIdx = _matches.Count - 1;
                }
                _rtbHighligher.SelectedIdx = _currentMatchIdx; 
                _rtbHighligher.ApplyHighlighting();
                MpTextSelectionRangeExtension.SetTextSelection(this, _matches[_currentMatchIdx]);
            }, HasMatch);

        public ICommand ReplaceNextCommand => new RelayCommand(
            () => {
                FindNextCommand.Execute(null);

                MpTextSelectionRangeExtension.SetSelectionText(this,_replaceText);

            }, HasMatch && IsReplaceValid);

        public ICommand ReplacePreviousCommand => new RelayCommand(
            () => {
                FindPreviousCommand.Execute(null);

                MpTextSelectionRangeExtension.SetSelectionText(this, _replaceText);
            }, HasMatch && IsReplaceValid);
        public ICommand ReplaceAllCommand => new RelayCommand(
            () => {
                _currentMatchIdx = -1;
                for(int i = 0;i < _matches.Count;i++) {
                    ReplaceNextCommand.Execute(null);
                }
                
            }, HasMatch && IsReplaceValid);

        #endregion

        #region View Models

        public MpImageAnnotationCollectionViewModel DetectedImageObjectCollectionViewModel { get; set; }

        public MpClipTileTitleSwirlViewModel TitleSwirlViewModel { get; set; }

        public MpTemplateCollectionViewModel TemplateCollection { get; set; }

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

        public MpAppViewModel AppViewModel => SourceViewModel.AppViewModel;

        public MpUrlViewModel UrlViewModel => SourceViewModel.UrlViewModel;

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

        public string ShortcutKeyString => ShortcutViewModel == null ? string.Empty : ShortcutViewModel.KeyString;

        public ICommand AssignCommand => AssignHotkeyCommand;

        #endregion

        #region Layout

        //public double TileBorderWidth => IsExpanded ? 
        //                    MpMainWindowViewModel.Instance.MainWindowWidth - (MpMeasurements.Instance.AppStateButtonPanelWidth * 2) :
        //                    TileBorderHeight - (MpMeasurements.Instance.ClipTileMargin * 2);
        private double _tileBorderWidth = DefaultBorderWidth;
        public double TileBorderWidth {
            get {
                return _tileBorderWidth;
            }
            set {
                if (_tileBorderWidth != value) {
                    _tileBorderWidth = Math.Max(0, value);
                    OnPropertyChanged(nameof(TileContentWidth));
                    OnPropertyChanged(nameof(TileBorderWidth));
                    //OnPropertyChanged(nameof(TrayX));
                    //Items.ForEach(x => x.OnPropertyChanged(nameof(x.EditorHeight)));
                }
            }
        }

        public double TileContentWidth => TileBorderWidth - MpMeasurements.Instance.ClipTileContentMargin - (MpMeasurements.Instance.ClipTileMargin * 2);


        private double _tileBorderHeight = DefaultBorderHeight;
        public double TileBorderHeight {
            get {
                return _tileBorderHeight;
            }
            set {
                if (_tileBorderHeight != value) {
                    _tileBorderHeight = Math.Max(0, value);
                    OnPropertyChanged(nameof(TileBorderHeight));
                    //OnPropertyChanged(nameof(TileBorderWidth));
                    //OnPropertyChanged(nameof(TileContentWidth));
                    OnPropertyChanged(nameof(TileContentHeight));
                    //OnPropertyChanged(nameof(TrayX));
                    OnPropertyChanged(nameof(EditorHeight));
                }
            }
        }        

        public double TileContentHeight => TileBorderHeight - TileTitleHeight - MpMeasurements.Instance.ClipTileMargin - MpMeasurements.Instance.ClipTileBorderThickness - TileDetailHeight;


        public double TrayX {
            get {
                if(IsPinned || Parent == null) {
                    return 0;
                }
                return Parent.FindTileOffsetX(QueryOffsetIdx);
            }
        }

        public double PasteTemplateToolbarHeight => MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                     

        public double TileTitleHeight => IsTitleVisible ? MpMeasurements.Instance.ClipTileTitleHeight : 0;
                
        public double TileDetailHeight => MpMeasurements.Instance.ClipTileDetailHeight;

        public double LoadingSpinnerSize => MpMeasurements.Instance.ClipTileLoadingSpinnerSize;


        //content container
        public Size ContainerSize {
            get {
                var cs = new Size(MpMeasurements.Instance.ClipTileScrollViewerWidth, 0);
                if (Parent == null) {
                    return cs;
                }
                double ch = MpMeasurements.Instance.ClipTileContentDefaultHeight;
                if (!IsContentReadOnly) {
                    ch -= MpMeasurements.Instance.ClipTileEditToolbarHeight;
                }
                if (IsPastingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                }
                if (IsEditingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTileEditTemplateToolbarHeight;
                }
                cs.Height = ch;
                return cs;
            }
        }

        public Size ContentSize => new Size(
                    ContainerSize.Width - MpMeasurements.Instance.ClipTileBorderThickness,
                    ContainerSize.Height - MpMeasurements.Instance.ClipTileBorderThickness);

        public double ContainerWidth {
            get {
                return ContainerSize.Width;
            }
        }

        public double ContentHeight => ContentSize.Height;

        public double ContentWidth => ContentSize.Width;


        public Size ReadOnlyContentSize => new Size(
                    MpMeasurements.Instance.ClipTileContentDefaultWidth,
                    MpMeasurements.Instance.ClipTileContentDefaultHeight);


        public double EditorHeight {
            get {
                if (Parent == null || CopyItem == null) {
                    return 0;
                }
                if (USING_BROWSER) {
                    double h;
                    if (!IsContentReadOnly) {
                        //return Parent.TileContentHeight; //quil editor height
                        h = TileContentHeight;// - MpMeasurements.Instance.ClipTileEditToolbarHeight - 15;
                    } else {

                        h = ReadOnlyContentSize.Height;
                    }
                    if (double.IsInfinity(h)) {
                        return Double.NaN;
                    }
                    return h;
                } else {

                    if (!IsContentReadOnly) {
                        return TileContentHeight - MpMeasurements.Instance.ClipTileEditToolbarHeight - 15;
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

        public Size CurrentSize {
            get {
                if (Parent == null) {
                    return new Size();
                }
                if (!IsContentReadOnly) {
                    return EditableContentSize;
                }
                return ReadOnlyContentSize;
            }
        }

        public Size UnformattedContentSize { get; set; }

        #endregion

        #region Visibility

        public Visibility FrontVisibility { get; set; } = Visibility.Visible;

        public Visibility BackVisibility { get; set; } = Visibility.Collapsed;

        public Visibility SideVisibility { get; set; } = Visibility.Collapsed;

        public ScrollBarVisibility HorizontalScrollbarVisibility {
            get {
                if (Parent == null) {
                    return ScrollBarVisibility.Hidden;
                }
                if (!IsContentReadOnly) {
                     if (EditableContentSize.Width > ContentWidth) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility VerticalScrollbarVisibility {
            get {
                if (Parent == null) {
                    return ScrollBarVisibility.Hidden;
                }
                if (!IsContentReadOnly) {
                    if (EditableContentSize.Height > ContainerSize.Height) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public Visibility PinButtonVisibility {
            get {
                return IsSelected || IsHovering ? Visibility.Visible : Visibility.Hidden;
            }
        }


        public Visibility ToolTipVisibility {
            get {
                if (!MpPreferences.ShowItemPreview) {
                    return Visibility.Collapsed;
                }
                return (Parent.HasScrollVelocity || IsSelected) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility TrialOverlayVisibility {
            get {
                return MpPreferences.IsTrialExpired ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        #region Appearance       
        public Brush DetailTextColor {
            get {
                if (IsSelected || IsHovering) {
                    return Brushes.Black;//Brushes.DimGray;
                }
                
                return Brushes.Transparent;
            }
        }

        public Brush TileTitleTextGridBackgroundBrush {
            get {
                if (IsHoveringOnTitleTextGrid && !IsTitleReadOnly) {
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
                if (ItemType == MpCopyItemType.FileList) {
                    if (IsHovering) {
                        return MpSystemColors.gainsboro;
                    }
                    return MpSystemColors.Transparent;
                }

                if (MpDragDropManager.IsDragAndDrop || Parent == null || CopyItem == null) {
                    return MpSystemColors.White;
                }

                return ItemEditorBackgroundHexColor;
            }
        }
        public string ItemBorderBrushHexColor {
            get {
                if (ItemType == MpCopyItemType.FileList) {
                    if (IsHovering) {
                        return MpSystemColors.black;
                    }
                    return MpSystemColors.Transparent;
                }
                return MpSystemColors.Transparent;
            }
        }
        public double ItemBorderBrushThickness {
            get {
                if (ItemType == MpCopyItemType.FileList) {
                    if (IsHovering) {
                        return 0.5;
                    }
                }
                return 0;
            }
        }
        public string ItemEditorBackgroundHexColor { get; set; } = MpSystemColors.Transparent;


        public Brush ItemBorderBrush {
            get {

                if (!IsSelected ||
                   IsItemDragging) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        }

        public Brush ItemSeparatorBrush {
            get {
                if (//MpContentDropManager.Instance.IsDragAndDrop ||
                    ItemType == MpCopyItemType.FileList ||
                   //(ItemIdx == Parent.DropIdx + 1 && Parent.IsDroppingOnTile) || // NOTE drop line uses adorner since DropIdx 0 won't have seperator
                   IsSelected) {
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

        public string SelectedTextHexColor {
            get {
                if (ItemType == MpCopyItemType.Text) {
                    return MpSystemColors.lightblue;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string InactiveSelectedTextHexColor {
            get {
                if (ItemType == MpCopyItemType.Text) {
                    return MpSystemColors.purple;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string CaretBrushHexColor {
            get {
                if(IsContentReadOnly) {
                    if(IsSubSelectionEnabled || IsFindAndReplaceVisible) {
                        return MpSystemColors.Red;
                    }
                    return MpSystemColors.Transparent;                    
                }
                return MpSystemColors.Black;
            }
        }

        public Size UnformattedAndDecodedContentSize {
            get {
                if(IsPlaceholder) {
                    return new Size();
                }
                return UnformattedContentSize;
            }
        }

        public string PinIconSourcePath {
            get {
                string path = "PinIcon";
                if(IsPinned) {
                    if(IsOverPinButton) {
                        path = "PinDownOverIcon";
                    } else {
                        path = "PinDownIcon";
                    }
                } else {
                    if(IsOverPinButton) {
                        path = "PinOverIcon";
                    }
                }
                return Application.Current.Resources[path] as string;
            }
        }

        public string HideTitleIconSourcePath {
            get {
                string path = "OpenEyeIcon";
                if (IsTitleVisible) {
                    if (IsOverHideTitleButton) {
                        path = "OpenEyeIcon";
                    } else {
                        path = "OpenEyeIcon";
                    }
                } else {
                    if (IsOverPinButton) {
                        path = "ClosedEyeIcon";
                    } else {
                        path = "ClosedEyeIcon";
                    }
                }
                return Application.Current.Resources[path] as string;
            }
        }

        public Rect TileBorderBrushRect {
            get {
                if (IsItemDragging || IsContextMenuOpen) {
                    return MpMeasurements.Instance.DottedBorderRect;
                }
                return MpMeasurements.Instance.SolidBorderRect;
            }
        }

        public Brush TileBorderBrush {
            get {
                if(IsResizing) {
                    return Brushes.Pink;
                }
                if(CanResize) {
                    return Brushes.Orange;
                }
                if (IsSelected) {
                    return Brushes.Red;
                }
                if (Parent.HasScrollVelocity || Parent.HasScrollVelocity) {
                    return Brushes.Transparent;
                }
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }

        #endregion

        #region State 

        public bool IsAnyBusy {
            get {
                if (IsBusy) {
                    return true;
                }
                if (TitleSwirlViewModel != null && TitleSwirlViewModel.IsAnyBusy) {
                    return true;
                }
                if (DetectedImageObjectCollectionViewModel != null && DetectedImageObjectCollectionViewModel.IsAnyBusy) {
                    return true;
                }
                if (TemplateCollection != null && TemplateCollection.IsAnyBusy) {
                    return true;
                }
                if (AppViewModel != null && AppViewModel.IsBusy) {
                    return true;
                }
                if (UrlViewModel != null && UrlViewModel.IsBusy) {
                    return true;
                }
                if (SourceViewModel != null && SourceViewModel.IsBusy) {
                    return true;
                }
                if (ShortcutViewModel != null && ShortcutViewModel.IsBusy) {
                    return true;
                }
                return false;
            }
        }
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
                if (Parent == null) {
                    return false;
                }
                double screenX = TrayX - Parent.ScrollOffset;
                return screenX >= 0 && 
                       screenX < Parent.ClipTrayScreenWidth && 
                       screenX + TileBorderWidth <= Parent.ClipTrayScreenWidth;
            }
        }

        #region Scroll

        public double NormalizedVerticalScrollOffset { get; set; } = 0;

        public bool IsScrolledToHome => Math.Abs(NormalizedVerticalScrollOffset) <= 0.1;

        public bool IsScrolledToEnd => Math.Abs(NormalizedVerticalScrollOffset) >= 0.9;

        public double KeyboardScrollAmount { get; set; } = 0.2;

        #endregion

        public bool IsSelected { get; set; }

        public bool IsHovering { get; set; } = false;

        public bool IsContextMenuOpen { get; set; } = false;

        public bool IsTitleReadOnly { get; set; } = true;

        public bool IsTitleFocused { get; set; } = false;


        public bool IsEditingTemplate {
            get {
                if (CopyItem == null || TemplateCollection == null) {
                    return false;
                }

                return TemplateCollection.Items.Any(x => x.IsEditingTemplate);
            }
        }

        public bool IsPasting { get; set; } = false;

        public bool IsPastingTemplate => IsPasting && HasTemplates;


        public bool HasTemplates {
            get {
                return TemplateCollection.Items.Count > 0;
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

        public bool IsPlaceholder => CopyItem == null || IsPinned;

        #region Drag & Drop

        public bool IsItemDragging { get; set; } = false;
        public bool IsCurrentDropTarget { get; set; } = false;

        #endregion
        public bool IsSubSelectionEnabled { get; set; } = false;

        public bool IsContentFocused { get; set; } = false;

        public bool IsOverPinButton { get; set; } = false;

        public bool IsOverHideTitleButton { get; set; } = false;

        public bool IsPinned => Parent != null && 
                                Parent.PinnedItems.Any(x => x.CopyItemId == CopyItemId);

        public bool CanVerticallyScroll => !IsContentReadOnly ?
                                                EditableContentSize.Height > TileContentHeight :
                                                UnformattedContentSize.Height > TileContentHeight;

        public bool CanResize { get; set; } = false;

        public bool IsResizing { get; set; } = false;

        public int QueryOffsetIdx { get; set; } = -1;


        public bool IsFileListItem => ItemType == MpCopyItemType.FileList;

        public bool IsTextItem => ItemType == MpCopyItemType.Text;

        public bool IsFlipping { get; set; } = false;

        public bool IsFlipped { get; set; } = false;

        public bool IsTitleVisible { get; set; } = true;

        public bool IsDetailGridVisibile {
            get {
                if(Parent.HasScrollVelocity) {
                    return false;
                }
                if(IsFindAndReplaceVisible) {
                    return false;
                }


                if (!IsContentReadOnly) {
                    if (IsEditingTemplate ||
                        IsPastingTemplate) {
                        return false;
                    }
                } else {
                    if (!IsSelected && !IsHovering) {
                        return false;
                    }
                }
                return true;
            }
        }


        public bool IsContentReadOnly { get; set; } = true;


        public bool IsContentAndTitleReadOnly => IsContentReadOnly && IsTitleReadOnly;

        public DateTime LastSelectedDateTime { get; set; }

        public bool IsContextMenuOpened { get; set; }

        public bool AllowMultiSelect { get; set; } = false;

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

        public string CopyItemTitle {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Title;
            }
            set {
                if (CopyItem != null && CopyItem.Title != value) {
                    CopyItem.Title = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemTitle));
                }
            }
        }

        public MpCopyItemType ItemType {
            get {
                if (CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.ItemType;
            }
            set {
                if (CopyItem != null && CopyItem.ItemType != value) {
                    CopyItem.ItemType = value;
                    OnPropertyChanged(nameof(ItemType));
                }
            }
        }

        public string CopyItemGuid {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Guid;
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
                if (CopyItem == null) {
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
                }
            }
        }


        public int IconId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                if (CopyItem.IconId > 0) {
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
                if (IconId != value) {
                    CopyItem.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        public string CopyItemHexColor {
            get {
                if (CopyItem == null || string.IsNullOrEmpty(CopyItem.ItemColor)) {
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

        public event EventHandler OnUiUpdateRequest;
        public event EventHandler OnScrollToHomeRequest;
        public event EventHandler OnFocusRequest;
        public event EventHandler OnSyncModels;

        public event EventHandler<Point> OnScrollOffsetRequest;
        public event EventHandler<object> OnPastePortableDataObject;

        public event EventHandler<double> OnScrollWheelRequest;
        public event EventHandler OnFitContentRequest;
        //public event EventHandler OnSubSelected;

        public event EventHandler OnMergeRequest;
        public event EventHandler<bool> OnUiResetRequest;
        public event EventHandler OnClearTemplatesRequest;
        public event EventHandler OnCreateTemplatesRequest;
        #endregion

        #region Static Builder


        #endregion

        #region Constructors

        public MpClipTileViewModel() : base(null) {
            IsBusy = true;
        }

        public MpClipTileViewModel(MpClipTrayViewModel parent) : base(parent) {
            IsBusy = true;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpCopyItem ci, int queryOffset = -1) {
            PropertyChanged -= MpClipTileViewModel_PropertyChanged;
            PropertyChanged += MpClipTileViewModel_PropertyChanged;

            QueryOffsetIdx = queryOffset < 0 && ci != null ? QueryOffsetIdx : queryOffset;
            
            IsBusy = true;

            if (ci != null && Parent.PersistentUniqueWidthTileLookup.TryGetValue(QueryOffsetIdx, out double uniqueWidth)) {
                TileBorderWidth = uniqueWidth;
            } else {
                TileBorderWidth = DefaultBorderHeight;
            }

            MpMessenger.Unregister<MpMessageType>(typeof(MpDragDropManager), ReceivedDragDropManagerMessage);

            //if (ci != null && ci.Source == null) {
            //    ci.Source = await MpDb.GetItemAsync<MpSource>(ci.SourceId);
            //}
            CopyItem = ci;

            IsNewAndFirstLoad = !MpMainWindowViewModel.Instance.IsMainWindowLoading;

            TemplateCollection = new MpTemplateCollectionViewModel(this);
            if (TitleSwirlViewModel == null) {
                TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);
            }

            await TitleSwirlViewModel.InitializeAsync();

            DetailText = GetDetailText((MpCopyItemDetailType)_detailIdx);

            if (ItemType == MpCopyItemType.Image) {
                DetectedImageObjectCollectionViewModel = new MpImageAnnotationCollectionViewModel(this);
                await DetectedImageObjectCollectionViewModel.InitializeAsync(CopyItem);
                OnPropertyChanged(nameof(HasDetectedObjects));
            }

            //RequestUiUpdate();
            //OnPropertyChanged(nameof(EditorHeight));
            OnPropertyChanged(nameof(ItemBorderBrush));
            OnPropertyChanged(nameof(ShortcutKeyString));

            MpMessenger.Register<MpMessageType>(typeof(MpDragDropManager), ReceivedDragDropManagerMessage);


            OnPropertyChanged(nameof(ItemSeparatorBrush));
            OnPropertyChanged(nameof(EditorHeight));
            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(TrayX));
            OnPropertyChanged(nameof(TileBorderBrush));
            OnPropertyChanged(nameof(CanVerticallyScroll));
            OnPropertyChanged(nameof(IsTextItem));
            OnPropertyChanged(nameof(IsFileListItem));
            

            while (TitleSwirlViewModel.IsBusy) {
                await Task.Delay(100);
            }

            
            RequestUiUpdate();

            MpMessenger.Send<MpMessageType>(MpMessageType.ContentItemsChanged, this);

            OnPropertyChanged(nameof(CopyItemTitle));

            IsBusy = false;
        }

        public void ResetSubSelection(bool clearEditing = true, bool reqFocus = false) {
            ClearSelection(clearEditing);
            Parent.IsSelectionReset = true;
            IsSelected = true;
            Parent.IsSelectionReset = false;
            if(reqFocus) {
                IsContentFocused = true;
            }
        }


        public void SubSelectAll() {
            AllowMultiSelect = true;
            IsSelected = true;
            AllowMultiSelect = false;
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

        #region View Event Invokers


        public void RequestScrollToHome() {
            OnScrollToHomeRequest?.Invoke(this, null);
        }

        public void RequestUiUpdate() {
            OnUiUpdateRequest?.Invoke(this, null);
        }


        public void RequestSyncModel() {
            OnSyncModels?.Invoke(this, null);
        }

        public void RequestScrollOffset(Point p) {
            OnScrollOffsetRequest?.Invoke(this, p);
        }

        public void RequestPastePortableDataObject(object portableDataObjectOrCopyItem) {
            OnPastePortableDataObject?.Invoke(this, portableDataObjectOrCopyItem);
        }

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


        #endregion

        public void ClearSelection(bool clearEditing = true) {
            IsSelected = false;
            LastSelectedDateTime = DateTime.MaxValue;
            if(clearEditing) {
                ClearEditing();
            }
        }

        public void ClearEditing() {
            IsTitleReadOnly = true;
            IsContentReadOnly = true;
            TemplateCollection?.ClearAllEditing();
            if (IsPasting) {
                IsPasting = false;
                //Parent.RequestUnexpand();
            }
        }

        public async Task<string> GetSubSelectedPastableRichText(bool isToExternalApp = false) {
            if(IsTextItem) {
                if (HasTemplates) {
                    IsPasting = true;
                    if (!MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                        MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                    await FillAllTemplates();
                }


                var sw = new Stopwatch();
                sw.Start();
                string rtf = string.Empty;
                if(HasTemplates) {
                    rtf = TemplateRichText;
                } else {
                    rtf = CopyItemData.ToFlowDocument().ToRichText();
                }
                sw.Stop();
                MpConsole.WriteLine(@"Time to combine richtext: " + sw.ElapsedMilliseconds + "ms");

                if (!IsContentReadOnly) {
                    ClearEditing();
                }
                return rtf;
            }

            return string.Empty;
            //both return to ClipTray.GetDataObjectFromSelectedClips
        }

        public async Task FillAllTemplates() {
            bool hasExpanded = false;
            IsPasting = true;
            if (HasTemplates) {
                IsSelected = true;
                if (!hasExpanded) {
                    //tile will be shrunk in on completed of hide window
                    IsContentReadOnly = false;
                    //rtbvm.OnPropertyChanged(nameof(rtbvm.IsEditingContent));
                    TemplateCollection.UpdateCommandsCanExecute();
                    TemplateCollection.OnPropertyChanged(nameof(TemplateCollection.Items));
                    TemplateCollection.OnPropertyChanged(nameof(TemplateCollection.HasMultipleTemplates));
                    hasExpanded = true;
                }
                TemplateCollection.SelectedItem = TemplateCollection.Items[0];
                await Task.Delay(300);
                TemplateCollection.SelectedItem.IsPasteTextBoxFocused = true;
                TemplateRichText = null;
                await Task.Run(async () => {
                    while (string.IsNullOrEmpty(TemplateRichText)) {
                        await Task.Delay(100);
                    }
                });

                TemplateCollection.ClearSelection();
            }
        }

        public void DeleteTempFiles() {
            foreach (var f in _tempFileList) {
                if (File.Exists(f)) {
                    File.Delete(f);
                }
            }
        }

        public void ResetContentScroll() {
            RequestScrollToHome();
        }

        public void RefreshAsyncCommands() {
            if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }

            //(Parent.FlipTileCommand as RelayCommand<object>).NotifyCanExecuteChanged();
            //(Parent.BringSelectedClipTilesToFrontCommand as RelayCommand).NotifyCanExecuteChanged();
            //(Parent.SendSelectedClipTilesToBackCommand as RelayCommand).NotifyCanExecuteChanged();
            //(Parent.SpeakSelectedClipsCommand as RelayCommand).NotifyCanExecuteChanged();
            //(Parent.MergeSelectedClipsCommand as RelayCommand).NotifyCanExecuteChanged();
            //(Parent.TranslateSelectedClipTextAsyncCommand as RelayCommand<string>).NotifyCanExecuteChanged();
            //(Parent.CreateQrCodeFromSelectedClipsCommand as RelayCommand).NotifyCanExecuteChanged();
        }

        #region IDisposable

        public override void Dispose() {
            base.Dispose();
            PropertyChanged -= MpClipTileViewModel_PropertyChanged;
            ClearSelection();
            TemplateCollection.Dispose();
            TitleSwirlViewModel.Dispose();
        }

        #endregion


        #endregion

        #region Protected Methods

        #region DB Overrides

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandId == CopyItemId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if (e is MpImageAnnotation dio) {
                if (dio.CopyItemId == CopyItemId) {
                    MpHelpers.RunOnMainThread(async () => {
                        if (DetectedImageObjectCollectionViewModel == null) {
                            DetectedImageObjectCollectionViewModel = new MpImageAnnotationCollectionViewModel(this);
                        }
                        await DetectedImageObjectCollectionViewModel.InitializeAsync(CopyItem);
                        OnPropertyChanged(nameof(HasDetectedObjects));
                    });
                }
            }
        }

        protected override async void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandId == CopyItemId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if (e is MpCopyItem ci && ci.Id == CopyItemId) {
                if (ci.Id == CopyItemId) {

                }
            }
        }

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(MpDragDropManager.IsDragAndDrop) {
                return;
            }
            if(e is MpCopyItem ci && CopyItemId == ci.Id) {
                if(IsPinned) {
                    var pctvm = Parent.PinnedItems.FirstOrDefault(x => x.CopyItemId == ci.Id);
                    if (pctvm != null) {
                        // Flag QueryOffsetIdx = -1 so it tray doesn't attempt to return it to tray
                        pctvm.QueryOffsetIdx = -1;
                        MpHelpers.RunOnMainThread(() => {
                            Parent.ToggleTileIsPinnedCommand.Execute(pctvm);
                        });
                    }
                } else {
                    await MpDataModelProvider.RemoveQueryItem(ci.Id);
                    MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                }
                OnPropertyChanged(nameof(IsPlaceholder));
            }
        }


        #endregion

        #endregion

        #region Private Methods

        private void ReceivedDragDropManagerMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ItemDragBegin:
                    if (IsSelected) {
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
                    var fl = MpCopyItemMerger.GetFileList(CopyItem);
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

        private void MpClipTileViewModel_PropertyChanged(object s, System.ComponentModel.PropertyChangedEventArgs e1) {
            switch (e1.PropertyName) {
                case nameof(IsAnyBusy):
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    }
                    break;
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        if (IsPinned) {
                            Parent.ClearClipSelection(false);
                        } else {
                            Parent.ClearPinnedSelection(false);
                            Parent.RequestScrollIntoView(this);
                        }
                        if(Parent.SelectedItem != this) {
                            Parent.SelectedItem = this;
                        }

                        if (!IsTitleFocused) {
                            IsContentFocused = true;
                        }
                        if (!Parent.IsRestoringSelection) {
                            Parent.StoreSelectionState(this);
                        }
                    } else {
                        if (IsFlipped) {
                            Parent.FlipTileCommand.Execute(this);
                        }
                        if (IsContentReadOnly) {
                            if(IsSubSelectionEnabled) {
                                IsSubSelectionEnabled = false;
                                RequestUiUpdate();
                            }
                        }
                        //LastSelectedDateTime = DateTime.MinValue;
                        //ClearSelection();
                    }
                    

                    Parent.NotifySelectionChanged();
                    OnPropertyChanged(nameof(ItemSeparatorBrush));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    break;
                case nameof(CopyItem):
                    if (CopyItem == null) {
                        break;
                    }
                    OnPropertyChanged(nameof(CopyItemData));
                    OnPropertyChanged(nameof(CurrentSize));
                    UpdateDetails();
                    RequestUiUpdate();
                    break;
                case nameof(IsFlipping):
                    if (IsFlipping) {
                        FrontVisibility = Visibility.Collapsed;
                        BackVisibility = Visibility.Collapsed;
                    }
                    break;
                case nameof(IsFlipped):
                    FrontVisibility = IsFlipped ? Visibility.Collapsed : Visibility.Visible;
                    BackVisibility = IsFlipped ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case nameof(IsEditingTemplate):
                    //OnPropertyChanged(nameof(De))
                    break;
                case nameof(CanResize):
                    OnPropertyChanged(nameof(TileBorderBrush));
                    break;
                case nameof(TileBorderWidth):
                    if (Parent.PersistentUniqueWidthTileLookup.TryGetValue(QueryOffsetIdx, out double uniqueWidth)) {
                        //this occurs when mainwindow is resized and user gives tile unique width
                        Parent.PersistentUniqueWidthTileLookup[QueryOffsetIdx] = TileBorderWidth;
                    }
                    break;
                case nameof(IsOverPinButton):
                case nameof(IsPinned):
                    OnPropertyChanged(nameof(PinIconSourcePath));
                    OnPropertyChanged(nameof(IsPlaceholder));
                    break;
                case nameof(IsOverHideTitleButton):
                case nameof(IsTitleVisible):
                    OnPropertyChanged(nameof(HideTitleIconSourcePath));
                    OnPropertyChanged(nameof(TileTitleHeight));
                    OnPropertyChanged(nameof(TileBorderHeight));
                    OnPropertyChanged(nameof(TileContentHeight));
                    break;
                case nameof(IsContentReadOnly):
                    MpMessenger.Send<MpMessageType>(IsContentReadOnly ? MpMessageType.IsReadOnly : MpMessageType.IsEditable, this);
                    Parent.OnPropertyChanged(nameof(Parent.IsHorizontalScrollBarVisible));

                    OnPropertyChanged(nameof(HorizontalScrollbarVisibility));
                    OnPropertyChanged(nameof(VerticalScrollbarVisibility));
                    OnPropertyChanged(nameof(EditorHeight));
                    OnPropertyChanged(nameof(CanVerticallyScroll));
                    IsSubSelectionEnabled = !IsContentReadOnly;
                    OnPropertyChanged(nameof(IsSubSelectionEnabled));
                    break;
                case nameof(IsContentFocused):
                    if(IsContentFocused) {
                        if(IsEditingTemplate) {
                            TemplateCollection.Items.FirstOrDefault(x => x.IsEditingTemplate).FinishEditTemplateCommand.Execute(null);
                        }
                    }
                    break;
                case nameof(IsTitleReadOnly):
                    if(!IsTitleReadOnly) {
                        IsTitleFocused = true;
                        IsSelected = true;
                    }
                    break;
                case nameof(IsContextMenuOpen):
                    OnPropertyChanged(nameof(ItemBorderBrushRect));
                    //Parent.OnPropertyChanged(nameof(Parent.TileBorderBrush));
                    OnPropertyChanged(nameof(TileBorderBrushRect));
                    OnPropertyChanged(nameof(IsContextMenuOpen));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyTileContextMenuOpened));
                    break;
                case nameof(IsItemDragging):
                    //Parent.OnPropertyChanged(nameof(Parent.TileBorderBrush));
                    if (IsItemDragging) {
                        StartAnimation();
                    } else {
                        StopAnimation();
                    }
                    OnPropertyChanged(nameof(ItemBorderBrushRect));
                    OnPropertyChanged(nameof(ItemBorderBrush));
                    OnPropertyChanged(nameof(TileBorderBrushRect));
                    break;
                case nameof(IsHovering):
                    OnPropertyChanged(nameof(ItemBorderBrushHexColor));
                    OnPropertyChanged(nameof(ItemBorderBrushThickness));
                    OnPropertyChanged(nameof(ItemBackgroundHexColor));
                    break;
                case nameof(ShortcutKeyString):
                    OnPropertyChanged(nameof(HotkeyIconSource));
                    OnPropertyChanged(nameof(HotkeyIconTooltip));
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        Task.Run(async () => {
                            await CopyItem.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(CopyItemHexColor):
                    if (TitleSwirlViewModel != null) {
                        //is null on init when CopyItem is set
                        MpHelpers.RunOnMainThread(async () => {
                            await TitleSwirlViewModel.InitializeAsync();
                        });
                    }
                    break;
                case nameof(FindText):
                case nameof(ReplaceText):
                case nameof(MatchCase):
                case nameof(UseRegEx):
                case nameof(MatchWholeWord):
                    UpdateFindAndReplaceMatches();
                    break;

            }
        }


        private void ExpandedKeyDown_Handler(object sender, KeyEventArgs e) {
            if(MpDragDropManager.IsDragAndDrop) {
                return;
            }
             if(e.Key == Key.Escape) {
                //ToggleReadOnlyCommand.Execute(null);
                ClearEditing();
            }
        }

        private void StartAnimation() {
            return;
            if (_timer == null) {
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
            if (dbr.Location.X >= dbr.Width) {
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
        public ICommand ChangeColorCommand => new RelayCommand<Brush>(
            (b) => {
                CopyItemHexColor = b.ToHex();
            });
        public ICommand SendSubSelectedToEmailCommand => new RelayCommand(
            () => {
                MpHelpers.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, CopyItemTitle, CopyItemData.ToPlainText()));
            });


        

        private RelayCommand<object> _searchWebCommand;
        public ICommand SearchWebCommand {
            get {
                if (_searchWebCommand == null) {
                    _searchWebCommand = new RelayCommand<object>(SearchWeb);
                }
                return _searchWebCommand;
            }
        }
        private void SearchWeb(object args) {
            if (args == null || args.GetType() != typeof(string)) {
                return;
            }
            MpHelpers.OpenUrl(args.ToString() + System.Uri.EscapeDataString(CopyItem.ItemData.ToPlainText()));
        }

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

        public ICommand AssignHotkeyCommand => new RelayCommand(
            async() => {
                await MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                    "Paste " + CopyItem.Title,
                    MpClipTrayViewModel.Instance.PasteCopyItemByIdCommand,
                    ShortcutType, CopyItem.Id, ShortcutKeyString);
                OnPropertyChanged(nameof(ShortcutKeyString));
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

        public ICommand ToggleEditContentCommand => new RelayCommand(
            () => {
                if(!IsSelected && IsContentReadOnly) {
                    Parent.SelectedItem = this;
                }
                IsContentReadOnly = !IsContentReadOnly;

            }, IsTextItem);

        public ICommand ToggleHideTitleCommand => new RelayCommand(
            () => {
                IsTitleVisible = !IsTitleVisible;
            }, !IsPlaceholder);


        #endregion
    }
}
