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
    using MonkeyPaste.Common.Plugin; 
    using MonkeyPaste.Common; 
    using MonkeyPaste.Common.Wpf;
    using System.Speech.Synthesis;
    using System.Windows.Documents;
using System.Text.RegularExpressions;
using MpProcessHelper;

    public class MpClipTileViewModel : 
        MpViewModelBase<MpClipTrayViewModel>, 
        MpIShortcutCommandViewModel,
        MpISelectableViewModel,
        MpIUserColorViewModel,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpITextSelectionRange,
        MpIFindAndReplaceViewModel,
        MpITooltipInfoViewModel,
        MpIPortableContentDataObject, 
        MpISizeViewModel{
        #region Private Variables

        private List<string> _tempFileList = new List<string>();
        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private int _lastQueryOffset = -1;

        private DispatcherTimer _timer;

        private int _detailIdx = 0;

        #endregion

        #region Statics

        public static double DefaultBorderWidth = MpMeasurements.Instance.ClipTileMinSize - MpMeasurements.Instance.ClipTileMargin;
        public static double DefaultBorderHeight = MpMeasurements.Instance.ClipTileMinSize;

        public static ObservableCollection<string> EditorToolbarIcons => new ObservableCollection<string>() {

        };
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

        #region MpITooltipInfoViewModel Implementation

        public object Tooltip { get; set; }

        #endregion

        #region MpISizeViewModel Implementation

        double MpISizeViewModel.Width => UnformattedContentSize.Width;
        double MpISizeViewModel.Height => UnformattedContentSize.Height;

        #endregion

        #region MpITextSelectionRangeViewModel Implementation 

        public int SelectionStart => MpTextSelectionRangeExtension.GetSelectionStart(this);
        public int SelectionLength => MpTextSelectionRangeExtension.GetSelectionLength(this);

        public string SelectedPlainText {
            get => MpTextSelectionRangeExtension.GetSelectedPlainText(this);
            set => MpTextSelectionRangeExtension.SetSelectionText(this, value);
        }

        public MpRichTextFormatInfoFormat SelectedRichTextFormat {
            get => MpTextSelectionRangeExtension.GetSelectionFormat(this);
        }


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
                var cv = Application.Current.MainWindow.GetVisualDescendents<MpRtbContentView>().FirstOrDefault(x => x.DataContext == this);
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

                SelectedPlainText = _replaceText;

                //MpTextSelectionRangeExtension.SetSelectionText(this,_replaceText);

            }, HasMatch && IsReplaceValid);

        public ICommand ReplacePreviousCommand => new RelayCommand(
            () => {
                FindPreviousCommand.Execute(null);

                SelectedPlainText = _replaceText;
                //MpTextSelectionRangeExtension.SetSelectionText(this, _replaceText);
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

        public ObservableCollection<MpFileItemViewModel> FileItems { get; set; } = new ObservableCollection<MpFileItemViewModel>();
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

        //#region MpIShortcutCommand Implementation

        //public MpShortcutType ShortcutType => MpShortcutType.PasteCopyItem;

        //public MpShortcutViewModel ShortcutViewModel {
        //    get {
        //        if (Parent == null || CopyItem == null) {
        //            return null;
        //        }
        //        var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.CommandId == CopyItemId && x.ShortcutType == ShortcutType);

        //        if (scvm == null) {
        //            scvm = new MpShortcutViewModel(MpShortcutCollectionViewModel.Instance);
        //        }

        //        return scvm;
        //    }
        //}

        //public string ShortcutKeyString => ShortcutViewModel == null ? string.Empty : ShortcutViewModel.KeyString;

        //public ICommand AssignCommand => AssignHotkeyCommand;

        //#endregion

        #region MpIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType => MpShortcutType.PasteCopyItem;

        public string ShortcutLabel => "Paste " + CopyItemTitle;
        public int ModelId => CopyItemId;
        public ICommand ShortcutCommand => Parent == null ? null : Parent.PasteCopyItemByIdCommand;
        public object ShortcutCommandParameter => CopyItemId;

        #endregion

        #region Layout

        public Thickness ContentMarginThickness {
            get {
                if(IsTitleVisible) {
                    return new Thickness();
                }
                return new Thickness(10, 3, 10, 3);
            }
        }

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
                }
            }
        }

        public double TileTitleHeight => IsTitleVisible ? MpMeasurements.Instance.ClipTileTitleHeight : 0;
        public double TileDetailHeight => MpMeasurements.Instance.ClipTileDetailHeight;

        private double _tileEditToolbarHeight = MpMeasurements.Instance.ClipTileEditToolbarDefaultHeight;
        public double TileEditToolbarHeight {
            get {
                if(IsContentReadOnly) {
                    return 0;
                }
                return _tileEditToolbarHeight;
            }
            set {
                if(_tileEditToolbarHeight != value) {
                    _tileEditToolbarHeight = value;
                    OnPropertyChanged(nameof(TileEditToolbarHeight));
                }
            }
        }

        public double TileContentWidth => 
            TileBorderWidth - 
            MpMeasurements.Instance.ClipTileContentMargin - 
            (MpMeasurements.Instance.ClipTileMargin * 2);

        public double TileContentHeight =>
            TileBorderHeight -
            TileTitleHeight -
            MpMeasurements.Instance.ClipTileMargin -
            MpMeasurements.Instance.ClipTileBorderThickness -
            TileDetailHeight;


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



        public double TrayX {
            get {
                if(IsPinned || Parent == null) {
                    return 0;
                }
                return Parent.FindTileOffsetX(QueryOffsetIdx);
            }
        }

        public double PasteTemplateToolbarHeight => MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                     


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
                    ch -= TileEditToolbarHeight;
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
                if (IsChromiumEditor) {
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
                        return TileContentHeight - TileEditToolbarHeight - 15;
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

        public bool IsHorizontalScrollbarVisibile {
            get {
                if(!IsContentReadOnly) {
                    return EditableContentSize.Width > ContentWidth;
                }
                return false;
            }
        }

        public bool IsVerticalScrollbarVisibile {
            get {
                if (IsContentReadOnly && !IsSubSelectionEnabled) {
                    return false;
                }

                return EditableContentSize.Height > ContentHeight;
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
        public string TileBackgroundHexColor {
            get {
                if(IsTitleVisible) {
                    return MpSystemColors.White;
                }
                return CopyItemHexColor;
            }
        }
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

        public double TileBorderBrushTranslateOffsetX { get;set; }

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

        public bool IsChromiumEditor => PreferredFormat != null && PreferredFormat.Name == MpPortableDataFormats.Html;

        public int LineCount { get; private set; } = -1;
        public int CharCount { get; private set; } = -1;

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

        public MpPortableDataFormat PreferredFormat {
            get {
                if (CopyItem == null) {
                    return null;
                }
                return CopyItem.PreferredFormat;
            }
            set {
                if (CopyItem != null && CopyItem.PreferredFormat != value) {
                    CopyItem.PreferredFormat = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PreferredFormat));
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

        private string _curItemRandomHexColor;
        public string CopyItemHexColor {
            get {
                if (CopyItem == null || string.IsNullOrEmpty(CopyItem.ItemColor)) {
                    if(string.IsNullOrEmpty(_curItemRandomHexColor)) {
                        _curItemRandomHexColor = MpColorHelpers.GetRandomHexColor();
                    }
                    return _curItemRandomHexColor;
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
            _curItemRandomHexColor = string.Empty;

            QueryOffsetIdx = queryOffset < 0 && ci != null ? QueryOffsetIdx : queryOffset;
            
            IsBusy = true;

            if (ci != null && Parent.TryGetByPersistentWidthById(ci.Id, out double uniqueWidth)) {
                TileBorderWidth = uniqueWidth;
            } else {
                TileBorderWidth = DefaultBorderHeight;
            }
            TileBorderHeight = DefaultBorderHeight;

            MpMessenger.Unregister<MpMessageType>(typeof(MpDragDropManager), ReceivedDragDropManagerMessage);

            CopyItem = ci;
            
            FileItems.Clear();
            TemplateCollection = new MpTemplateCollectionViewModel(this);
            if (TitleSwirlViewModel == null) {
                TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);
            }

            await TitleSwirlViewModel.InitializeAsync();

            _detailIdx = 0;
            DetailText = string.Empty;
            

            if (ItemType == MpCopyItemType.Image) {
                DetectedImageObjectCollectionViewModel = new MpImageAnnotationCollectionViewModel(this);
                await DetectedImageObjectCollectionViewModel.InitializeAsync(CopyItem);
                OnPropertyChanged(nameof(HasDetectedObjects));
            }

            //RequestUiUpdate();
            //OnPropertyChanged(nameof(EditorHeight));
            OnPropertyChanged(nameof(TileBorderBrush));
            OnPropertyChanged(nameof(TileBorderBrushRect));

            MpMessenger.Register<MpMessageType>(typeof(MpDragDropManager), ReceivedDragDropManagerMessage);

            OnPropertyChanged(nameof(EditorHeight));
            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(TrayX));
            OnPropertyChanged(nameof(TileBorderBrush));
            OnPropertyChanged(nameof(CanVerticallyScroll));
            OnPropertyChanged(nameof(IsTextItem));
            OnPropertyChanged(nameof(IsFileListItem));
            OnPropertyChanged(nameof(TileBackgroundHexColor));
            OnPropertyChanged(nameof(ContentMarginThickness));

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

        public void ResetExpensiveDetails() {
            LineCount = CharCount = -1;
        }

        private string GetDetailText(MpCopyItemDetailType detailType) {
            if (CopyItem == null) {
                return string.Empty;
            }

            switch (detailType) {
                //created
                case MpCopyItemDetailType.DateTimeCreated:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc

                    return "Copied " + CopyItemCreatedDateTime.ToReadableTimeSpan();
                //chars/lines
                case MpCopyItemDetailType.DataSize:
                    if (CopyItem.ItemType == MpCopyItemType.Image) {
                        return "(" + (int)UnformattedContentSize.Width + "px) x (" + (int)UnformattedContentSize.Height + "px)";
                    } else if (CopyItem.ItemType == MpCopyItemType.Text) {
                        if(LineCount < 0 && CharCount < 0) {
                            //Line and Char count are set to -1 when initialized so they're lazy loaded
                            var textTuple = MpContentDocumentRtfExtension.GetLineAndCharCount(this);
                            LineCount = textTuple.Item1;
                            CharCount = textTuple.Item2;
                        }                        
                        
                        return CharCount + " chars | " + LineCount + " lines";
                    } else if (CopyItem.ItemType == MpCopyItemType.FileList) {
                        break;// return GetDetailText((MpCopyItemDetailType)(++_detailIdx));
                    }
                    break;
                //# copies/# pastes
                case MpCopyItemDetailType.UsageStats:
                    return CopyItem.CopyCount + " copies | " + CopyItem.PasteCount + " pastes";
                case MpCopyItemDetailType.UrlInfo:
                    if (UrlViewModel == null) {
                        break;// return GetDetailText((MpCopyItemDetailType)(++_detailIdx));
                    }
                    return UrlViewModel.UrlPath;
                //case MpCopyItemDetailType.AppInfo:
                //    if (AppViewModel == null) {
                //        return GetDetailText((MpCopyItemDetailType)(++_detailIdx));
                //    }
                //    return AppViewModel.AppPath;
                default:
                    break;
            }

            return string.Empty;
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


        public async Task<MpPortableDataObject>  ConvertToPortableDataObject(
            bool isDragDrop, 
            object targetHandleObj, 
            bool ignoreSubSelection = false, 
            bool isDropping = false) {
            
            IntPtr targetHandle = targetHandleObj == null ? IntPtr.Zero : 
                                    targetHandleObj is MpProcessInfo ? 
                                        (targetHandleObj as MpProcessInfo).Handle : (IntPtr)targetHandleObj;
            bool isToExternalApp = targetHandle != IntPtr.Zero && targetHandle != MpProcessManager.GetThisApplicationMainWindowHandle();

            MpPortableDataObject d = new MpPortableDataObject();
            string rtf = string.Empty;

            //check for model templates
            bool needsTemplateData = HasTemplates;

            IEnumerable<MpTextTemplateViewModel> templatesInSelection = null;

            if (needsTemplateData) {
                if ((isDragDrop && !isDropping) || !isToExternalApp) {
                    // Drag Drop:
                    // when initially dragging onto external app DragDrop needs DataObject but 
                    // ignore filling templates until drop is performed
                    
                    // CopySelectedClipsCommand:
                    // Templates are passed as templates internally
                    needsTemplateData = false;
                }
                if(needsTemplateData) {
                    templatesInSelection = MpTextSelectionRangeExtension.SelectedTextTemplates(this);
                    if (!ignoreSubSelection && templatesInSelection.Count() == 0) {
                        // if dropping or pasting and sub-selection doesn't contain templates they don't need to be filled
                        needsTemplateData = false;
                    }
                }                
            }

            if (needsTemplateData) {
                //if(!ignoreSubSelection) {
                //    ClearSelection(false);                    
                //}                
                IsSelected = true;

                if (!MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                    MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                }
                await FillTemplates(templatesInSelection);
                rtf = TemplateRichText;
                
                if (!IsContentReadOnly) {
                    ClearEditing();
                }

            } else if(ItemType == MpCopyItemType.Text) {
                bool isInUi = Parent.GetClipTileViewModelById(CopyItemId) != null;
                if(!isInUi) {
                    // handle special case when pasting item by id (like from a hotkey)
                    // and it has no templates (if it did tray would set manual query and show it)
                    // so since its not in ui need to use model data which is ok because it won't have any modifications
                    rtf = CopyItemData.ToRichText();
                } else {
                    rtf = MpContentDocumentRtfExtension.GetEncodedContent(
                            MpContentDocumentRtfExtension.FindRtbByViewModel(this),
                            ignoreSubSelection);
                }
            }
            string pt = string.Empty;
            string bmpBase64 = string.Empty;
            var sctfl = new List<string>();
            switch (ItemType) {
                case MpCopyItemType.Text:
                    pt = rtf.ToPlainText();
                    bmpBase64 = rtf.ToFlowDocument().ToBitmapSource().ToBase64String();
                    break;
                case MpCopyItemType.Image:
                    pt = string.Empty;//CopyItemData.ToBitmapSource().ToAsciiImage();
                    bmpBase64 = CopyItemData;
                    break;
                case MpCopyItemType.FileList:
                    if(FileItems.All(x => x.IsSelected == false)) {
                        FileItems.ForEach(x => x.IsSelected = true);
                    }
                    pt = string.Join(Environment.NewLine,FileItems.Select(x=>x.Path));
                    rtf = pt.ToRichText();
                    bmpBase64 = rtf.ToFlowDocument().ToBitmapSource().ToBase64String();
                    break;
            }

            if (isToExternalApp) {
                foreach (string format in MpPortableDataFormats.Formats) {
                    switch (format) {
                        case MpPortableDataFormats.FileDrop:
                            if (ItemType == MpCopyItemType.FileList) {
                                foreach(var fp in FileItems.Where(x=>x.IsSelected).Select(x=>x.Path)) {
                                    if(fp.IsFileOrDirectory()) {
                                        sctfl.Add(fp);
                                    }                                    
                                }
                            } else {
                                sctfl.Add(CopyItemData.ToFile(null, CopyItemTitle));
                            }
                            d.SetData(MpPortableDataFormats.FileDrop, string.Join(Environment.NewLine, sctfl));
                            break;
                        case MpPortableDataFormats.Rtf:
                            d.SetData(MpPortableDataFormats.Rtf, rtf);
                            break;
                        case MpPortableDataFormats.Text:
                            d.SetData(MpPortableDataFormats.Text, pt);
                            break;
                        case MpPortableDataFormats.Bitmap:
                            d.SetData(MpPortableDataFormats.Bitmap, bmpBase64);
                            break;
                        case MpPortableDataFormats.Csv:
                            if(ItemType == MpCopyItemType.Image) {
                                continue;
                            }
                            if(ItemType == MpCopyItemType.FileList) {
                                d.SetData
                                    (MpPortableDataFormats.Csv, 
                                    string.Join(
                                        ",",
                                        FileItems.Where(x => x.IsSelected).Select(x => x.Path)));
                            } else {
                                d.SetData(MpPortableDataFormats.Csv, CopyItemData.ToCsv());
                            }
                            break;
                        default:
                            continue;
                    }
                }

            } else {
                // TODO set internal data stuff here
            }

            //d.SetData(MpPortableDataFormats.InternalContent, this);
            return d;
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
                    OnPropertyChanged(nameof(SelfBindingRef));
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
                    OnPropertyChanged(nameof(SelfBindingRef));
                }
            } else if (e is MpCopyItem ci && ci.Id == CopyItemId) {
                if (ci.Id == CopyItemId) {
                    if(HasModelChanged) {
                        // this means the model has been updated from the view model so ignore
                    } else {
                        await InitializeAsync(ci);
                    }
                }
            }
        }

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            //if(MpDragDropManager.IsDragAndDrop) {
            //    return;
            //}
            if(e is MpCopyItem ci && CopyItemId == ci.Id) {

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


        private async Task FillTemplates(IEnumerable<MpTextTemplateViewModel> templatesToFill) {
            if (HasTemplates) {
                IsSelected = true;
                
                IsContentReadOnly = false;
                //rtbvm.OnPropertyChanged(nameof(rtbvm.IsEditingContent));
                TemplateCollection.PastableItems = new ObservableCollection<MpTextTemplateViewModel>(templatesToFill);

                IsPasting = true;
                TemplateCollection.OnPropertyChanged(nameof(TemplateCollection.Items));
                TemplateCollection.OnPropertyChanged(nameof(TemplateCollection.HasMultipleTemplates));

                TemplateCollection.SelectedItem = TemplateCollection.PastableItems[0];
                
                await Task.Delay(300);
                //TemplateCollection.SelectedItem.IsPasteTextBoxFocused = true;
                TemplateRichText = null;
                await Task.Run(async () => {
                    while (string.IsNullOrEmpty(TemplateRichText)) {
                        await Task.Delay(100);
                    }
                });

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
                        }
                        if(Parent.SelectedItem != this) {
                            Parent.SelectedItem = this;
                        }

                        Parent.RequestScrollIntoView(this);
                        //if (!IsTitleFocused && !Parent.IsPasting) {
                        //    // NOTE checking Parent.IsPasting because setting focus will clear current selection
                        //    IsContentFocused = true;
                        //}
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
                        FileItems.ForEach(x => x.IsSelected = false);
                        //LastSelectedDateTime = DateTime.MinValue;
                        //ClearSelection();
                    }
                    

                    Parent.NotifySelectionChanged();
                    OnPropertyChanged(nameof(TileBorderBrush));
                    break;
                case nameof(CopyItem):
                    if (CopyItem == null) {
                        break;
                    }
                    OnPropertyChanged(nameof(CopyItemData));
                    OnPropertyChanged(nameof(CurrentSize));
                    //UpdateDetails();
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
                    Parent.OnPropertyChanged(nameof(Parent.CanAnyResize));
                    break;
                case nameof(IsResizing):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyResizing));
                    break;
                case nameof(TileBorderWidth):
                    if (Parent.TryGetByPersistentWidthById(CopyItemId, out double uniqueWidth)) {
                        //this occurs when mainwindow is resized and user gives tile unique width
                        Parent.AddOrReplacePersistentWidthById(CopyItemId, TileBorderWidth);
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
                case nameof(IsSubSelectionEnabled):
                    OnPropertyChanged(nameof(IsHorizontalScrollbarVisibile));
                    OnPropertyChanged(nameof(IsVerticalScrollbarVisibile));
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
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTitle));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTile));
                    break;

                case nameof(IsContentReadOnly):
                    if(!IsContentReadOnly && !IsSelected) {
                        IsSelected = true;
                    }
                    MpMessenger.Send<MpMessageType>(IsContentReadOnly ? MpMessageType.IsReadOnly : MpMessageType.IsEditable, this);
                    Parent.OnPropertyChanged(nameof(Parent.IsHorizontalScrollBarVisible));

                    OnPropertyChanged(nameof(IsHorizontalScrollbarVisibile));
                    OnPropertyChanged(nameof(IsVerticalScrollbarVisibile));
                    OnPropertyChanged(nameof(EditorHeight));
                    OnPropertyChanged(nameof(CanVerticallyScroll));
                    IsSubSelectionEnabled = !IsContentReadOnly;
                    OnPropertyChanged(nameof(IsSubSelectionEnabled));

                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTile));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTile));
                    break;
                case nameof(IsContextMenuOpen):
                    OnPropertyChanged(nameof(TileBorderBrush));
                    //Parent.OnPropertyChanged(nameof(Parent.TileBorderBrush));
                    OnPropertyChanged(nameof(TileBorderBrushRect));
                    OnPropertyChanged(nameof(IsContextMenuOpen));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyTileContextMenuOpened));
                    break;
                case nameof(IsItemDragging):
                    //Parent.OnPropertyChanged(nameof(Parent.TileBorderBrush));
                    if (IsItemDragging) {
                        StartAnimation();
                        if(!IsSubSelectionEnabled) {
                            // BUG checking selection length here (when IsSubSelectionEnabled=false)
                            // to see if partial selection always returns some size when
                            // none is actually selected. So force it to select all and 
                            // make sure selection extension updates ui of selection
                            IsContentFocused = true;
                            MpTextSelectionRangeExtension.SelectAll(this);
                        }

                        if(MpTextSelectionRangeExtension.IsSelectionContainTemplate(this)) {
                            TemplateCollection.ClearAllEditing();
                        }
                    } else {
                        StopAnimation();
                    }
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(TileBorderBrushRect));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyItemDragging));
                    break;
                case nameof(IsHovering):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyHovering));
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
                case nameof(CopyItemData):
                    ResetExpensiveDetails();
                    break;
                case nameof(TrayX):
                    //if(QueryOffsetIdx == Parent.TailQueryIdx) {

                    //}
                    if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                        return;
                    }
                    Parent.ValidateItemsTrayX();
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
            //return;
            if (_timer == null) {
                _timer = new DispatcherTimer(DispatcherPriority.Render);                
                _timer.Interval = TimeSpan.FromMilliseconds(30);
                _timer.Tick += _timer_Tick;
            }
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e) {
            if(!IsItemDragging) {
                StopAnimation();
                return;
            }
            if(TileBorderBrushTranslateOffsetX > 50) {
                TileBorderBrushTranslateOffsetX = 0;
            }
            TileBorderBrushTranslateOffsetX += 0.01d;
        }

        private void StopAnimation() {
            TileBorderBrushTranslateOffsetX = 0.0d;
            OnPropertyChanged(nameof(TileBorderBrushRect));
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

        public ICommand CycleDetailCommand => new RelayCommand(
            () => {
                do {
                    _detailIdx++;
                    if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                        _detailIdx = 1;
                    }

                    // TODO this should aggregate details over all sub items 
                    DetailText = GetDetailText((MpCopyItemDetailType)_detailIdx);
                } while (string.IsNullOrEmpty(DetailText));                
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
