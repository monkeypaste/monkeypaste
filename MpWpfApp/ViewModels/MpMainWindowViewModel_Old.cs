
using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpMainWindowViewModel_Old : MpViewModelBase {
        #region Private Variables
        private double _startMainWindowTop, _endMainWindowTop;
        private bool _doPaste = false;
        private MpHotKeyHost _hotkeyHost = null;
        private IKeyboardMouseEvents _globalHook = null;

        private List<DispatcherOperation> _performSearchHandlerResults = new List<DispatcherOperation>();

        #endregion

        #region Public Variables
        public MpClipboardMonitor ClipboardMonitor { get; private set; }

        #endregion

        #region Properties

        private ObservableCollection<MpClipTileViewModel_Old> _clipTiles = new ObservableCollection<MpClipTileViewModel_Old>();
        public ObservableCollection<MpClipTileViewModel_Old> ClipTiles {
            get {
                return _clipTiles;
            }
            set {
                if(_clipTiles != value) {
                    _clipTiles = value;
                    OnPropertyChanged(nameof(ClipTiles));
                }
            }
        }

        public List<MpClipTileViewModel_Old> SelectedClipTiles {
            get {
                return ClipTiles.Where(ct => ct.IsSelected).ToList();
            }
        }

        public List<MpClipTileViewModel_Old> VisibileClipTiles {
            get {
                return ClipTiles.Where(ct => ct.TileVisibility == Visibility.Visible).ToList();
            }
        }

        public MpClipTileViewModel_Old FocusedClipTile {
            get {
                var tempList = ClipTiles.Where(ct => ct.IsSelected && ct.IsFocused).ToList();
                if(tempList == null || tempList.Count == 0) {
                    return null;
                }
                return tempList[0];
            }
        }

        private ObservableCollection<MpTagTileViewModel> _tagTiles = new ObservableCollection<MpTagTileViewModel>();
        public ObservableCollection<MpTagTileViewModel> TagTiles {
            get {
                return _tagTiles;
            }
            set {
                if (_tagTiles != value) {
                    _tagTiles = value;
                    OnPropertyChanged(nameof(TagTiles));
                }
            }
        }

        public MpTagTileViewModel SelectedTagTile {
            get {
                return TagTiles.Where(tt => tt.IsSelected).ToList()[0];
            }
        }

        private ObservableCollection<MpSortTypeComboBoxItemViewModel> _sortTypes = new ObservableCollection<MpSortTypeComboBoxItemViewModel>();
        public ObservableCollection<MpSortTypeComboBoxItemViewModel> SortTypes {
            get {
                if(_sortTypes.Count == 0) {
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Date",null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Application", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Title", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Content", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Type", null));
                    _sortTypes.Add(new MpSortTypeComboBoxItemViewModel("Usage", null));
                }
                return _sortTypes;
            }
            set {
                if(_sortTypes != value) {
                    _sortTypes = value;
                    OnPropertyChanged(nameof(SortTypes));
                }
            }
        }

        private MpSortTypeComboBoxItemViewModel _selectedSortType;
        public MpSortTypeComboBoxItemViewModel SelectedSortType {
            get {
                return _selectedSortType;
            }
            set {
                if (_selectedSortType != value) {
                    _selectedSortType = value;
                    OnPropertyChanged(nameof(SelectedSortType));
                }
            }
        }

        private string _taskbarIconToolTipText = Properties.Settings.Default.ApplicationName;
        public string TaskbarIconToolTipText {
            get {
                return _taskbarIconToolTipText;
            }
            set {
                if(_taskbarIconToolTipText != value) {
                    _taskbarIconToolTipText = value;
                    OnPropertyChanged(nameof(TaskbarIconToolTipText));
                }
            }
        }

        private Combination _showMainWindowCombination;
        public Combination ShowMainWindowCombination {
            get {
                return _showMainWindowCombination;
            }
            set {
                if(_showMainWindowCombination != value) {
                    _showMainWindowCombination = value;
                    OnPropertyChanged(nameof(ShowMainWindowCombination));
                }
            }
        }

        private Combination _hideMainWindowCombination;
        public Combination HideMainWindowCombination {
            get {
                return _hideMainWindowCombination;
            }
            set {
                if (_hideMainWindowCombination != value) {
                    _hideMainWindowCombination = value;
                    OnPropertyChanged(nameof(HideMainWindowCombination));
                }
            }
        }

        private bool _isInAppendMode = false;
        public bool IsInAppendMode {
            get {
                return _isInAppendMode;
            }
            set {
                if(_isInAppendMode != value) {
                    _isInAppendMode = value;
                    Console.WriteLine("IsInAppendMode changed to: " + _isInAppendMode);
                    OnPropertyChanged(nameof(IsInAppendMode));
                }
            }
        }

        private bool _isAutoCopyMode = false;
        public bool IsAutoCopyMode {
            get {
                return _isAutoCopyMode;
            }
            set {
                if (_isAutoCopyMode != value) {
                    _isAutoCopyMode = value;
                    OnPropertyChanged(nameof(IsAutoCopyMode));
                }
            }
        }

        private string _searchText;
        public string SearchText {
            get {
                return _searchText;
            }
            set {
                if(_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        private bool _isLoading = true;
        public bool IsLoading {
            get {
                return _isLoading;
            }
            set {
                if(_isLoading != value) {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        private MpHotKey _showMainWindowHotKey = null;
        public MpHotKey ShowMainWindowHotKey {
            get {
                return _showMainWindowHotKey;
            }
            set {
                if(_showMainWindowHotKey != value) {
                    _showMainWindowHotKey = value;
                    OnPropertyChanged(nameof(ShowMainWindowHotKey));
                }
            }
        }

        private MpHotKey _hideMainWindowHotKey = null;
        public MpHotKey HideMainWindowHotKey {
            get {
                return _hideMainWindowHotKey;
            }
            set {
                if(_hideMainWindowHotKey != value) {
                    _hideMainWindowHotKey = value;
                    OnPropertyChanged(nameof(HideMainWindowHotKey));
                }
            }
        }

        private MpHotKey _toggleAppendModeHotKey;
        public MpHotKey ToggleAppendModeHotKey {
            get {
                return _toggleAppendModeHotKey;
            }
            set {
                if(_toggleAppendModeHotKey != value) {
                    _toggleAppendModeHotKey = value;
                    OnPropertyChanged(nameof(ToggleAppendModeHotKey));
                }
            }
        }

        private bool _isAppPaused = false;
        public bool IsAppPaused {
            get {
                return _isAppPaused;
            }
            set {
                if(_isAppPaused != value) {
                    _isAppPaused = value;
                    OnPropertyChanged(nameof(IsAppPaused));
                }
            }
        }

        public bool IsTagTextBoxFocused {
            get {
                return SelectedTagTile.IsTextBoxFocused || SelectedTagTile.IsEditing;
            }
        }

        private bool _isSearchTextBoxFocused = false;
        public bool IsSearchTextBoxFocused {
            get {
                return _isSearchTextBoxFocused;
            }
            set {
                if(_isSearchTextBoxFocused != value) {
                    _isSearchTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsSearchTextBoxFocused));
                }
            }
        }

        private Brush _searchTextBoxBorderBrush = Brushes.Transparent;
        public Brush SearchTextBoxBorderBrush {
            get {
                return _searchTextBoxBorderBrush;
            }
            set {
                if(_searchTextBoxBorderBrush != value) {
                    _searchTextBoxBorderBrush = value;
                    OnPropertyChanged(nameof(SearchTextBoxBorderBrush));
                }
            }
        }

        private Visibility _emptyListMessageVisibility = Visibility.Collapsed;
        public Visibility EmptyListMessageVisibility {
            get {
                return _emptyListMessageVisibility;
            }
            set {
                if(_emptyListMessageVisibility != value) {
                    _emptyListMessageVisibility = value;
                    OnPropertyChanged(nameof(EmptyListMessageVisibility));
                }
            }
        }

        private Visibility _clipListVisibility = Visibility.Visible;
        public Visibility ClipListVisibility {
            get {
                return _clipListVisibility;
            }
            set {
                if (_clipListVisibility != value) {
                    _clipListVisibility = value;
                    OnPropertyChanged(nameof(ClipListVisibility));
                }
            }
        }

        private Visibility _mergeClipsCommandVisibility = Visibility.Collapsed;
        public Visibility MergeClipsCommandVisibility {
            get {
                return _mergeClipsCommandVisibility;
            }
            set {
                if (_mergeClipsCommandVisibility != value) {
                    _mergeClipsCommandVisibility = value;
                    OnPropertyChanged(nameof(MergeClipsCommandVisibility));
                }
            }
        }

        private Visibility _ascSortOrderButtonImageVisibility = Visibility.Collapsed;
        public Visibility AscSortOrderButtonImageVisibility {
            get {
                return _ascSortOrderButtonImageVisibility;
            }
            set {
                if (_ascSortOrderButtonImageVisibility != value) {
                    _ascSortOrderButtonImageVisibility = value;
                    OnPropertyChanged(nameof(AscSortOrderButtonImageVisibility));
                }
            }
        }

        private Visibility _descSortOrderButtonImageVisibility = Visibility.Visible;
        public Visibility DescSortOrderButtonImageVisibility {
            get {
                return _descSortOrderButtonImageVisibility;
            }
            set {
                if (_descSortOrderButtonImageVisibility != value) {
                    _descSortOrderButtonImageVisibility = value;
                    OnPropertyChanged(nameof(DescSortOrderButtonImageVisibility));
                }
            }
        }

        public double AppStateButtonGridWidth {
            get {
                return MpMeasurements.Instance.AppStateButtonPanelWidth;
            }
        }

        private double _clipTrayHeight = MpMeasurements.Instance.ClipTrayHeight;
        public double ClipTrayHeight {
            get {
                return _clipTrayHeight;
            }
            set {
                if(_clipTrayHeight != value) {
                    _clipTrayHeight = value;
                    OnPropertyChanged(nameof(ClipTrayHeight));
                }
            }
        }

        public double TitleMenuHeight {
            get {
                return MpMeasurements.Instance.TitleMenuHeight;
            }
        }

        public double FilterMenuHeight {
            get {
                return MpMeasurements.Instance.FilterMenuHeight;
            }
        }

        private SolidColorBrush _searchTextBoxTextBrush = Brushes.DimGray;
        public SolidColorBrush SearchTextBoxTextBrush {
            get {
                return _searchTextBoxTextBrush;
            }
            set {
                if(_searchTextBoxTextBrush != value) {
                    _searchTextBoxTextBrush = value;
                    OnPropertyChanged(nameof(SearchTextBoxTextBrush));
                }
            }
        }

        private FontStyle _searchTextBoxFontStyle = FontStyles.Italic;
        public FontStyle SearchTextBoxFontStyle {
            get { 
                return _searchTextBoxFontStyle; 
            }
            set { 
                if(_searchTextBoxFontStyle != value) {
                    _searchTextBoxFontStyle = value;
                    OnPropertyChanged(nameof(SearchTextBoxFontStyle));
                }
            }
        }

        #endregion

        #region Constructor
        public MpMainWindowViewModel_Old() {
            base.DisplayName = "MpMainWindowViewModel_Old";
            IsLoading = true;
            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(SearchText):
                        break;
                    case nameof(IsAutoCopyMode):
                        if (IsAutoCopyMode) {
                            _globalHook.MouseUp += GlobalMouseUpEvent;
                        } else {
                            _globalHook.MouseUp -= GlobalMouseUpEvent;
                        }
                        break;
                    case nameof(AscSortOrderButtonImageVisibility):
                    case nameof(DescSortOrderButtonImageVisibility):
                    case nameof(SelectedSortType):
                        SortClipTiles();
                        break;
                }
            };
        }
        #endregion

        #region View Event Handlers
        public void MainWindow_Loaded(object sender,RoutedEventArgs e) {
            var mw = ((MpMainWindow)Application.Current.MainWindow);
            var searchBox = (TextBox)mw.FindName("SearchTextBox");
            searchBox.KeyDown += (s, e3) => {
                if (e3.Key == Key.Return) {
                    PerformSearch();
                }
            };
            searchBox.GotFocus += (s, e4) => {
                //make text
                if (SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                    SearchText = "";
                }
                SearchTextBoxFontStyle = FontStyles.Normal;
                SearchTextBoxTextBrush = Brushes.Black;
                IsSearchTextBoxFocused = true;
            };
            searchBox.LostFocus += (s, e5) => {
                //var searchTextBox = (TextBox)e.Source;
                if (string.IsNullOrEmpty(SearchText)) {
                    SearchText = Properties.Settings.Default.SearchPlaceHolderText;
                    SearchTextBoxFontStyle = FontStyles.Italic;
                    SearchTextBoxTextBrush = Brushes.DimGray;
                }
                IsSearchTextBoxFocused = false;
            };
            SearchText = Properties.Settings.Default.SearchPlaceHolderText;

            SelectedSortType = SortTypes[0];

            var clipTray = (ListBox)mw.FindName("ClipTray");
            clipTray.DragEnter += ClipTray_DragEnter;
            clipTray.Drop += ClipTray_Drop;
            clipTray.SelectionChanged += (s, e8) => {
                MergeClipsCommandVisibility = MergeClipsCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;
            };

            SetupMainWindowRect();

            //create tiles for all clips in the database
            foreach (MpCopyItem c in MpCopyItem.GetAllCopyItems()) {
                AddClipTile(c,false);
            }

            //create tiles for all the tags
            foreach (MpTag t in MpTag.GetAllTags()) {
                AddTagTile(t,false);
            }
            //select history tag by default
            GetHistoryTagTileViewModel().IsSelected = true;

            SortClipTiles();

            mw.Deactivated += (s, e2) => {
                HideWindowCommand.Execute(null);
            };
            mw.PreviewKeyDown += MainWindow_KeyDown;

            InitClipboard();
            InitHotKeys();

#if DEBUG
            ShowWindowCommand.Execute(null);
            //HideWindowCommand.Execute(null);
#else
            HideWindowCommand.Execute(null);
#endif
            var taskbarIcon = (TaskbarIcon)mw.FindName("TaskbarIcon");
        }

        public void ScrollClipTray(object sender, MouseWheelEventArgs e) {
            e.Handled = true;

            var clipTrayListBox = (ListBox)sender;
            var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (e.Delta * -1) / 5);
        }
        //public void TestAnimatedScroll() {
        //    var mw = ((MpMainWindow)Application.Current.MainWindow);
        //    var clipTrayListBox = (ListBox)mw.FindName("ClipTray");

        //    var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();

        //    DoubleAnimation ta = new DoubleAnimation();
        //    ta.From = scrollViewer.HorizontalOffset;
        //    ta.To = scrollViewer.HorizontalOffset + 10000;
        //    ta.Duration = new Duration(TimeSpan.FromSeconds(7));
        //    CubicEase easing = new CubicEase();
        //    easing.EasingMode = EasingMode.EaseIn;
        //    ta.EasingFunction = easing;
        //    ta.Completed += (s, e1) => {
        //        //_clipTrayPhysicsBody.Start();
        //        //ShowMainWindowHotKey.Enabled = false;
        //        //HideMainWindowHotKey.Enabled = true;
        //    };

        //    //Storyboard storyboard = new Storyboard();

        //    //storyboard.Children.Add(ta);
        //    //Storyboard.SetTarget(ta, scrollViewer);
        //    //Storyboard.SetTargetProperty(ta, new PropertyPath(MpScrollViewFixer.VerticalOffsetProperty));
        //    //storyboard.Begin();
        //    //scrollViewer.BeginAnimation(MpScrollViewFixer.CurrentHorizontalOffsetProperty, ta);
        //    //mw.BeginAnimation(Window.TopProperty, ta);
        //}
        // NOTE KeyUp cannot handle Enter key so KeyDown MUST be used
        public void MainWindow_KeyDown(object sender, KeyEventArgs e) {
            if(IsSearchTextBoxFocused || IsTagTextBoxFocused) {
                return;
            }
            Key key = e.Key;
            if (key == Key.Delete || key == Key.Back) {
                if(SelectedClipTiles.Count == 1 && SelectedClipTiles[0].IsEditingTitle) {
                    return;
                }
                //delete clip which shifts focus to neighbor
                DeleteSelectedClipsCommand.Execute(null);
            } else if (key == Key.Enter) {
                if (SelectedClipTiles.Count == 1 && SelectedClipTiles[0].IsEditingTitle) {
                    SelectedClipTiles[0].IsEditingTitle = false;
                    e.Handled = true;
                    return;
                } else {
                    PasteSelectedClips();
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region App Mode Event Handlers
        private void GlobalMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e) {
            if(e.Button == System.Windows.Forms.MouseButtons.Left && !MpHelpers.ApplicationIsActivated()) {
                System.Windows.Forms.SendKeys.SendWait("^c");
            }
        }
        #endregion

        #region Public Methods

        public void AddTagTile(MpTag t, bool isNew = false) {
            var newTagTile = new MpTagTileViewModel(t, this,isNew);
            TagTiles.Add(newTagTile);
            //watches Tag IsSelected so History is selected if none are
            newTagTile.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case "IsSelected":
                        var tagChanged = ((MpTagTileViewModel)s);
                        //ensure at least history is selected
                        if (tagChanged.IsSelected == false) {
                            //find all selected tag tiles
                            var selectedTagTiles = TagTiles.Where(tt => tt.IsSelected == true).ToList();
                            //if none selected select history tag
                            if (selectedTagTiles == null || selectedTagTiles.Count == 0) {
                                GetHistoryTagTileViewModel().IsSelected = true;
                            }
                        } else {
                            foreach (MpClipTileViewModel_Old clipTile in ClipTiles) {
                                //this ensures when switching between tags the last selected tag in a list reset
                                clipTile.IsSelected = false;
                                if (tagChanged.Tag.IsLinkedWithCopyItem(clipTile.CopyItem)) {
                                    clipTile.TileVisibility = Visibility.Visible;
                                } else {
                                    clipTile.TileVisibility = Visibility.Collapsed;
                                }
                            }
                            if (VisibileClipTiles.Count == 0) {
                                ClipListVisibility = Visibility.Collapsed;
                                EmptyListMessageVisibility = Visibility.Visible;
                            } else {
                                ClipListVisibility = Visibility.Visible;
                                EmptyListMessageVisibility = Visibility.Collapsed;

                                ResetClipSelection();
                            }
                        }
                        break;
                }
            };
            if (!isNew) {
                foreach(var ctvm in ClipTiles) {
                    if(newTagTile.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                        newTagTile.TagClipCount++;
                    }
                }
            }
        }

        public void ClearClipSelection() {
            foreach (var clip in ClipTiles) {
                clip.IsSelected = false;
                clip.IsFocused = false;
            }
        }

        public void ResetClipSelection() {
            ClearClipSelection();
            if(VisibileClipTiles.Count > 0) {
                VisibileClipTiles[0].IsSelected = true;
            }
        }

        public void ClearTagSelection() {
            foreach (var tagTile in TagTiles) {
                tagTile.IsSelected = false;
                tagTile.IsFocused = false;
            }
        }

        public void ResetTagSelection() {
            ClearTagSelection();
            GetHistoryTagTileViewModel().IsSelected = true;
            GetHistoryTagTileViewModel().IsFocused = true;
        }

        public void AddClipTile(MpCopyItem ci,bool isNew = false) {
            if (isNew) {
                ci.WriteToDatabase();
                MpTag historyTag = new MpTag(1);
                historyTag.LinkWithCopyItem(ci);
                GetHistoryTagTileViewModel().TagClipCount++;
            }

            MpClipTileViewModel_Old newClipTile = new MpClipTileViewModel_Old(ci, this);

            ClipTiles.Insert(0, newClipTile);

            //update cliptray visibility if this is the first cliptile added
            ClipListVisibility = Visibility.Visible;
            EmptyListMessageVisibility = Visibility.Collapsed;            
        }

        public void MoveClipTile(MpClipTileViewModel_Old clipTile, int newIdx) {
            if(ClipTiles == null) {
                throw new Exception("MoveClipTile exception ClipTiles is null");
            }
            if (newIdx > ClipTiles.Count || newIdx < 0) {
                throw new Exception("Cannot insert tile clip tile at index: " + newIdx + " with list of length: " + ClipTiles.Count);
            }
            int removeIdx = VisibileClipTiles.IndexOf(clipTile);
            if(removeIdx < 0) {
                throw new Exception("MoveClipTile error can only move visible clip tiles");
            }
            ClipTiles.Remove(clipTile);
            //SortClipTiles();
            ClipTiles.Insert(newIdx, clipTile);
        }

        public MpTagTileViewModel GetHistoryTagTileViewModel() {
            return TagTiles.Where(tt => tt.Tag.TagName == Properties.Settings.Default.HistoryTagTitle).ToList()[0];
        }

        #endregion

        #region Private Methods

        private void SetupMainWindowRect() {
            var mw = ((MpMainWindow)Application.Current.MainWindow);

            mw.Left = SystemParameters.WorkArea.Left;
            mw.Height = SystemParameters.PrimaryScreenHeight * 0.35;
            _startMainWindowTop = SystemParameters.PrimaryScreenHeight;
            if (SystemParameters.WorkArea.Top == 0) {
                //if taskbar is at the bottom
                mw.Width = SystemParameters.PrimaryScreenWidth;
                _endMainWindowTop = SystemParameters.WorkArea.Height - mw.Height;
            } else if(SystemParameters.WorkArea.Left != 0) {
                //if taskbar is on the right
                mw.Width = SystemParameters.WorkArea.Width;
                _endMainWindowTop = SystemParameters.PrimaryScreenHeight - mw.Height;
            } else if(SystemParameters.WorkArea.Right != SystemParameters.PrimaryScreenWidth) {
                //if taskbar is on the left
                mw.Width = SystemParameters.WorkArea.Width;
                _endMainWindowTop = SystemParameters.WorkArea.Height - mw.Height;
            } else {
                //if taskbar is on the top
                mw.Width = SystemParameters.PrimaryScreenWidth;
                _endMainWindowTop = SystemParameters.PrimaryScreenHeight - mw.Height;
            }            
        }

        private void RemoveTagTile(MpTagTileViewModel tagTileToRemove) {
            //when removing a tag auto-select the history tag
            if(tagTileToRemove.IsSelected) {
                tagTileToRemove.IsSelected = false;
                TagTiles.Where(tt => tt.Tag.TagName == Properties.Settings.Default.HistoryTagTitle).ToList()[0].IsSelected = true;
            }
            TagTiles.Remove(tagTileToRemove);
            tagTileToRemove.Tag.DeleteFromDatabase();
        }

        private void RemoveClipTile(MpClipTileViewModel_Old clipTileToRemove) {
            foreach(var ttvm in TagTiles) {
                if(ttvm.Tag.IsLinkedWithCopyItem(clipTileToRemove.CopyItem)) {
                    ttvm.TagClipCount--;
                }
            }
            ClipTiles.Remove(clipTileToRemove);
            clipTileToRemove.CopyItem.DeleteFromDatabase();

            //if this was the last visible clip update the cliptray visibility
            if (VisibileClipTiles.Count == 0) {
                ClipListVisibility = Visibility.Collapsed;
                EmptyListMessageVisibility = Visibility.Visible;
            }
        }

        private void PerformSearch() {
            if (SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                FilterTiles(string.Empty);
            } else {
                FilterTiles(SearchText);
            }
            
            foreach (var vctvm in VisibileClipTiles) {
                vctvm.Highlight(SearchText);
            }
            if (VisibileClipTiles.Count > 0) {
                SearchTextBoxBorderBrush = Brushes.Transparent;
                EmptyListMessageVisibility = Visibility.Collapsed;
                SortClipTiles();
            } else {
                SearchTextBoxBorderBrush = Brushes.Red;
                EmptyListMessageVisibility = Visibility.Visible;
            }
        }
        private void FilterTiles(string searchStr) {
            List<int> filteredTileIdxList = new List<int>();
            //search ci's from newest to oldest for filterstr, adding idx to list
            for(int i = ClipTiles.Count - 1;i >= 0;i--) {
                //when search string is empty add each item to list so all shown
                if(string.IsNullOrEmpty(searchStr)) {
                    filteredTileIdxList.Add(i);
                    continue;
                }
                MpCopyItem ci = ClipTiles[i].CopyItem;
                //add clips where searchStr is in clip title or part of the app path ( TODO also check application name since usually different than exe)
                if(ci.Title.ToLower().Contains(searchStr.ToLower()) || ci.App.AppPath.ToLower().Contains(searchStr.ToLower())) {
                    filteredTileIdxList.Add(i);
                    continue;
                }
                //do not search through image tiles
                if(ci.CopyItemType == MpCopyItemType.Image) {
                    continue;
                }
                //add clips where search is part of clip's content
                if(ci.CopyItemType == MpCopyItemType.RichText) {
                    if(ci.GetPlainText().ToLower().Contains(searchStr.ToLower())) {
                        filteredTileIdxList.Add(i);
                    }
                }
                //lastly add filelist clips if search string found in it's path(s)
                else if(ci.CopyItemType == MpCopyItemType.FileList) {
                    foreach(string p in (string[])ci.DataObject) {
                        if(p.ToLower().Contains(searchStr.ToLower())) {
                            filteredTileIdxList.Add(i);
                        }
                    }
                }
            }
            //only show tiles w/ an idx in list
            int vcount = 0;
            for(int i = ClipTiles.Count - 1;i >= 0;i--) {
                if(filteredTileIdxList.Contains(i)) {
                    ClipTiles[i].TileVisibility = Visibility.Visible;
                    vcount++;
                } else {
                    ClipTiles[i].TileVisibility = Visibility.Collapsed;
                }
            }            
        }

        private void SortClipTiles() {
            string sortBy = string.Empty;
            bool ascending = AscSortOrderButtonImageVisibility == Visibility.Visible;

            if (SelectedSortType.Header == "Date") {
                sortBy = "CopyItemCreatedDateTime";
            } else if (SelectedSortType.Header == "Application") {
                sortBy = "CopyItemAppId";
            } else if (SelectedSortType.Header == "Title") {
                sortBy = "Title";
            } else if (SelectedSortType.Header == "Content") {
                sortBy = "Text";
            } else if (SelectedSortType.Header == "Type") {
                sortBy = "CopyItemType";
            } else if (SelectedSortType.Header == "Usage") {
                sortBy = "CopyItemUsageScore";
            }
            ClearClipSelection();
            var sortStart = DateTime.Now;
            ClipTiles.Sort(x => x[sortBy], !ascending);
            var sortDur = DateTime.Now - sortStart;
            Console.WriteLine("Sort for " + VisibileClipTiles.Count + " items: " + sortDur.TotalMilliseconds + " ms");
            ResetClipSelection();            
        }

        private int FindMaxIdxByProperty(ObservableCollection<MpClipTileViewModel_Old> list,string propertyName,int startIdx = 0) {
            if(list == null || list.Count <= startIdx) {
                return -1;
            }
            var maxItem = (MpClipTileViewModel_Old)list[startIdx];
            for (int i = startIdx+1; i < list.Count; i++) {
                var curItem = (MpClipTileViewModel_Old)list[i];
                if (curItem[propertyName].GetType() == typeof(int)) {
                    if ((int)curItem[propertyName] > (int)maxItem[propertyName]) {
                        maxItem = curItem;
                    }
                } else if (curItem[propertyName].GetType() == typeof(string)) {
                    if (string.Compare((string)curItem[propertyName], (string)maxItem[propertyName]) == -1) {
                        maxItem = curItem;
                    }
                }
            }
            return list.IndexOf(maxItem);
        }

        private int FindMinIdxByProperty(ObservableCollection<MpClipTileViewModel_Old> list, string propertyName, int startIdx = 0) {
            if (list == null || list.Count <= startIdx) {
                return -1;
            }
            var maxItem = (MpClipTileViewModel_Old)list[startIdx];
            for (int i = startIdx + 1; i < list.Count; i++) {
                var curItem = (MpClipTileViewModel_Old)list[i];
                if (curItem[propertyName].GetType() == typeof(int)) {
                    if ((int)curItem[propertyName] < (int)maxItem[propertyName]) {
                        maxItem = curItem;
                    }
                } else if (curItem[propertyName].GetType() == typeof(string)) {
                    if (string.Compare((string)curItem[propertyName], (string)maxItem[propertyName]) == 1) {
                        maxItem = curItem;
                    }
                }
            }
            return list.IndexOf(maxItem);
        }

        private void InitHotKeys() {
            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseMove += (s, e) => {
                if (e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                    if (ShowWindowCommand.CanExecute(null)) {
                        ShowWindowCommand.Execute(null);
                    }
                }
            };

            ShowMainWindowCombination = Combination.FromString("Control+Shift+D");
            HideMainWindowCombination = Combination.FromString("Escape");
            var PasteCombination = Combination.FromString("Control+V");
            var assignment = new Dictionary<Combination, Action>{
                {ShowMainWindowCombination, ()=>ShowWindowCommand.Execute(null)},
                {HideMainWindowCombination, ()=>HideWindowCommand.Execute(null)},
                {PasteCombination,()=>Console.WriteLine("Pasted")}
            };
            Hook.GlobalEvents().OnCombination(assignment);
        }

        private void InitClipboard() {
            ClipboardMonitor = new MpClipboardMonitor((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));

            // Attach the handler to the event raising on WM_DRAWCLIPBOARD message is received
            ClipboardMonitor.ClipboardChanged += (s, e) => {
                MpCopyItem newCopyItem = MpCopyItem.CreateFromClipboard(ClipboardMonitor.LastWindowWatcher.LastHandle);
                if (IsInAppendMode && newCopyItem.CopyItemType == MpCopyItemType.RichText) {
                    //when in append mode just append the new items text to selecteditem
                    SelectedClipTiles[0].RichText += Environment.NewLine + (string)newCopyItem.DataObject;
                    return;
                }
                
                if (newCopyItem != null) {
                    //check if copyitem is duplicate
                    var existingClipTile = FindClipTileByModel(newCopyItem);
                    if(existingClipTile == null) {
                        AddClipTile(newCopyItem,true);
                    } else {
                        existingClipTile.CopyItem.CopyCount++;
                        existingClipTile.CopyItem.CopyDateTime = DateTime.Now;
                        MoveClipTile(existingClipTile, 0);
                    }

                    ResetClipSelection();
                }
            };
        }

        private MpClipTileViewModel_Old FindClipTileByModel(MpCopyItem ci) {
            foreach(var ctvm in ClipTiles) {
                if(ctvm.CopyItemType != ci.CopyItemType || ctvm.CopyItemAppId != ci.AppId) {
                    continue;
                }
                switch(ci.CopyItemType) {
                    case MpCopyItemType.Image:
                    case MpCopyItemType.FileList:
                        if (ctvm.CopyItem.DataObject == ci.DataObject) {
                            return ctvm;
                        }
                        break;
                    case MpCopyItemType.RichText:
                        if (ctvm.CopyItem.GetPlainText() == ci.GetPlainText()) {
                            return ctvm;
                        }
                        break;
                }
                
            }
            return null;
        }

        private int GetFirstSelectedClipTileIdx() {
            int firstIdx = int.MaxValue;
            foreach(var sctvm in SelectedClipTiles) {
                int curIdx = VisibileClipTiles.IndexOf(sctvm);
                if (curIdx < firstIdx) {
                    curIdx = firstIdx;
                }
            }
            return firstIdx < int.MaxValue ? firstIdx : -1;
        }

        private int GetLastSelectedClipTileIdx() {
            int lastIdx = int.MinValue;
            foreach (var sctvm in SelectedClipTiles) {
                int curIdx = VisibileClipTiles.IndexOf(sctvm);
                if (curIdx > lastIdx) {
                    curIdx = lastIdx;
                }
            }
            return lastIdx > int.MinValue ? lastIdx : -1;
        }

        private void WriteClipsToFile(List<MpClipTileViewModel_Old> clipList, string rootPath) {
            foreach (MpClipTileViewModel_Old ctvm in clipList) {
                ctvm.WriteCopyItemToFile(rootPath);
            }
        }

        private void WriteClipsToCsvFile(List<MpClipTileViewModel_Old> clipList, string filePath) {
            string csvText = string.Empty;
            foreach (MpClipTileViewModel_Old ctvm in clipList) {
                csvText += ctvm.CopyItem.GetPlainText() + ",";
            }
            StreamWriter of = new StreamWriter(filePath);
            of.Write(csvText);
            of.Close();
        }

        #endregion

        #region Commands
        private RelayCommand _pasteSelectedClipsCommand;
        public ICommand PasteSelectedClipsCommand {
            get {
                if (_pasteSelectedClipsCommand == null) {
                    _pasteSelectedClipsCommand = new RelayCommand(PasteSelectedClips);
                }
                return _pasteSelectedClipsCommand;
            }
        }
        private void PasteSelectedClips() {
            //In order to paste the app must hide first
            //((MpMainWindow)Application.Current.MainWindow).Visibility = Visibility.Collapsed;

            //this triggers hidewindow to paste selected items
            _doPaste = true;
            HideWindowCommand.Execute(null);
        }

        private RelayCommand _bringSelectedClipTilesToFrontCommand;
        public ICommand BringSelectedClipTilesToFrontCommand {
            get {
                if(_bringSelectedClipTilesToFrontCommand == null) {
                    _bringSelectedClipTilesToFrontCommand = new RelayCommand(BringSelectedClipTilesToFront, CanBringSelectedClipTilesToFront);
                }
                return _bringSelectedClipTilesToFrontCommand;
            }
        }
        private bool CanBringSelectedClipTilesToFront() {
            bool canBringForward = false;
            for (int i = 0; i < SelectedClipTiles.Count; i++) {
                if(!SelectedClipTiles.Contains(VisibileClipTiles[i])) {
                    canBringForward = true;
                    break;
                }
            }
            return canBringForward;
        }
        private void BringSelectedClipTilesToFront() {
            //MpClipTileViewModel_Old[] selectedClipTile_copies = new MpClipTileViewModel_Old[SelectedClipTiles.Count];
            //SelectedClipTiles.CopyTo(selectedClipTile_copies);
            //for (int i = selectedClipTile_copies.Length - 1; i >= 0; i--) {
            //    MoveClipTile(SelectedClipTiles[i], 0);
            //}
            for (int i = SelectedClipTiles.Count - 1; i >= 0; i--) {
                MoveClipTile(SelectedClipTiles[i], 0);
            }
        }

        private RelayCommand _toggleSortOrderCommand;
        public ICommand ToggleSortOrderCommand {
            get {
                if (_toggleSortOrderCommand == null) {
                    _toggleSortOrderCommand = new RelayCommand(ToggleSortOrder);
                }
                return _toggleSortOrderCommand;
            }
        }
        private void ToggleSortOrder() {
            if(AscSortOrderButtonImageVisibility == Visibility.Visible) {
                AscSortOrderButtonImageVisibility = Visibility.Collapsed;
                DescSortOrderButtonImageVisibility = Visibility.Visible;
            } else {
                AscSortOrderButtonImageVisibility = Visibility.Visible;
                DescSortOrderButtonImageVisibility = Visibility.Collapsed;
            }
        }

        private RelayCommand<bool> _exportSelectedClipTilesCommand;
        public ICommand ExportSelectedClipTilesCommand {
            get {
                if(_exportSelectedClipTilesCommand == null) {
                    _exportSelectedClipTilesCommand = new RelayCommand<bool> (ExportSelectedClipTiles,CanExportSelectedClipTiles);
                }
                return _exportSelectedClipTilesCommand;
            }
        }
        private bool CanExportSelectedClipTiles(bool toCsv) {
            if(!toCsv) {
                return true;
            }
            foreach(var sctvm in SelectedClipTiles) {
                if(sctvm.CopyItemType != MpCopyItemType.RichText) {
                    return false;
                }
            }
            return true;
        }
        private void ExportSelectedClipTiles(bool toCsv) {
            CommonFileDialog dlg = toCsv ? new CommonSaveFileDialog() as CommonFileDialog : new CommonOpenFileDialog();
            dlg.Title = toCsv ? "Export CSV" : "Export Items to Directory...";
            if(toCsv) {
                dlg.DefaultFileName = "Mp_Exported_Data_" + DateTime.Now.ToString().Replace(@"/","-");
                dlg.DefaultExtension = "csv";
            } else {
                ((CommonOpenFileDialog)dlg).IsFolderPicker = !toCsv;
            }
            dlg.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            dlg.AddToMostRecentlyUsedList = false;
            //dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            //dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
                if(toCsv) {
                    WriteClipsToCsvFile(SelectedClipTiles.ToList(), dlg.FileName);
                } else {
                    WriteClipsToFile(SelectedClipTiles.ToList(), dlg.FileName);
                }
            }
        }

        private RelayCommand _mergeClipsCommand;
        public ICommand MergeClipsCommand {
            get {
                if(_mergeClipsCommand == null) {
                    _mergeClipsCommand = new RelayCommand(MergeClips,CanMergeClips);
                }
                return _mergeClipsCommand;
            }
        }
        private bool CanMergeClips() {
            if (SelectedClipTiles.Count <= 1) {
                return false;
            }
            bool areAllSameType = true;
            foreach (var sctvm in SelectedClipTiles) {
                if (sctvm.CopyItemType != SelectedClipTiles[0].CopyItemType) {
                    areAllSameType = false;
                }
            }
            return areAllSameType;
        }
        private void MergeClips() {
            var focusedClip = SelectedClipTiles[0];
            List<MpClipTileViewModel_Old> clipTilesToRemove = new List<MpClipTileViewModel_Old>();
            foreach(MpClipTileViewModel_Old selectedClipTile in SelectedClipTiles) {
                if(selectedClipTile == focusedClip) {
                    continue;
                }
                focusedClip.RichText += Environment.NewLine + selectedClipTile.RichText;
                clipTilesToRemove.Add(selectedClipTile);
            }
            foreach(MpClipTileViewModel_Old tileToRemove in clipTilesToRemove) {
                RemoveClipTile(tileToRemove);
            }
            focusedClip.IsSelected = true;
            focusedClip.IsFocused = true;
        }

        private RelayCommand _showWindowCommand;
        public ICommand ShowWindowCommand {
            get {
                if (_showWindowCommand == null) {
                    _showWindowCommand = new RelayCommand(ShowWindow, CanShowWindow);
                }
                return _showWindowCommand;
            }
        }
        private bool CanShowWindow() {
            return Application.Current.MainWindow == null || Application.Current.MainWindow.Visibility != Visibility.Visible || IsLoading;
        }
        private void ShowWindow() {
            if(Application.Current.MainWindow == null) {
                Application.Current.MainWindow = new MpMainWindow();
            }
            SetupMainWindowRect();

            var mw = ((MpMainWindow)Application.Current.MainWindow);
            mw.Show();
            mw.Activate();
            mw.Visibility = Visibility.Visible;
            mw.Topmost = true;

            ResetClipSelection();

            DoubleAnimation ta = new DoubleAnimation();
            ta.From = _startMainWindowTop;
            ta.To = _endMainWindowTop;
            ta.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
            CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseIn;
            ta.EasingFunction = easing;
            ta.Completed += (s, e1) => {
                //_clipTrayPhysicsBody.Start();
                //ShowMainWindowHotKey.Enabled = false;
                //HideMainWindowHotKey.Enabled = true;

                //var mw = ((MpMainWindow)Application.Current.MainWindow);
                //var ct = (ListBox)mw.FindName("ClipTray");
                //ct.Focus();
                //Keyboard.Focus(ct);
                IsLoading = false;
            };
            mw.BeginAnimation(Window.TopProperty, ta);            
        }

        private RelayCommand _hideWindowCommand;
        public ICommand HideWindowCommand {
            get {
                if (_hideWindowCommand == null) {
                    _hideWindowCommand = new RelayCommand(HideWindow, CanHideWindow);
                }
                return _hideWindowCommand;
            }
        }
        private bool CanHideWindow() {
            return Application.Current.MainWindow != null && Application.Current.MainWindow.Visibility == Visibility.Visible;
        }
        private void HideWindow() {
            if(!CanHideWindow()) {
                return;
            }
            var mw = ((MpMainWindow)Application.Current.MainWindow);

            DoubleAnimation ta = new DoubleAnimation();
            ta.From = _endMainWindowTop;
            ta.To = _startMainWindowTop;
            ta.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
            ta.Completed += (s, e) => {
                mw.Visibility = Visibility.Collapsed;
                if(_doPaste) {
                    _doPaste = false;
                    foreach (var clipTile in SelectedClipTiles) {
                        ClipboardMonitor.PasteCopyItem(clipTile.CopyItem);
                        MoveClipTile(clipTile, 0);
                    }
                }
                //ShowMainWindowHotKey.Enabled = true;
                //HideMainWindowHotKey.Enabled = false;
                //_clipTrayPhysicsBody.Stop();
            };
            CubicEase easing = new CubicEase();  // or whatever easing class you want
            easing.EasingMode = EasingMode.EaseIn;
            ta.EasingFunction = easing;
            mw.BeginAnimation(Window.TopProperty, ta);            
        }

        private RelayCommand _deleteSelectedClipsCommand;
        public ICommand DeleteSelectedClipsCommand {
            get {
                if (_deleteSelectedClipsCommand == null) {
                    _deleteSelectedClipsCommand = new RelayCommand(DeleteSelectedClips);
                }
                return _deleteSelectedClipsCommand;
            }
        }
        private void DeleteSelectedClips() {
            int lastSelectedClipTileIdx = -1;
            foreach (var ct in SelectedClipTiles) {
                lastSelectedClipTileIdx = VisibileClipTiles.IndexOf(ct);
                RemoveClipTile(ct);
            }
            if(VisibileClipTiles.Count > 0) {
                if (lastSelectedClipTileIdx == 0) {
                    VisibileClipTiles[0].IsSelected = true;
                } else {
                    VisibileClipTiles[lastSelectedClipTileIdx - 1].IsSelected = true;
                }
            }
        }

        private RelayCommand _renameClipCommand;
        public ICommand RenameClipCommand {
            get {
                if (_renameClipCommand == null) {
                    _renameClipCommand = new RelayCommand(RenameClip,CanRenameClip);
                }
                return _renameClipCommand;
            }
        }
        private bool CanRenameClip() {
            return SelectedClipTiles.Count == 1;
        }
        private void RenameClip() {
            SelectedClipTiles[0].IsEditingTitle = true;
            SelectedClipTiles[0].IsTitleTextBoxFocused = true;
        }

        private RelayCommand _deleteTagCommand;
        public ICommand DeleteTagCommand {
            get {
                if (_deleteTagCommand == null) {
                    _deleteTagCommand = new RelayCommand(DeleteTag, CanDeleteTag);
                }
                return _deleteTagCommand;
            }
        }
        private bool CanDeleteTag() {
            //allow delete if any tag besides history tag is selected, delete method will ignore history\
            return SelectedTagTile.TagName != Properties.Settings.Default.HistoryTagTitle;
        }
        private void DeleteTag() {
            RemoveTagTile(SelectedTagTile);
        }

        private RelayCommand _createTagCommand;
        public ICommand CreateTagCommand {
            get {
                if(_createTagCommand == null) {
                    _createTagCommand = new RelayCommand(CreateTag);
                }
                return _createTagCommand;
            }
        }
        private void CreateTag() {
            //add tag to datastore so TagTile collection will automatically add the tile
            MpTag newTag = new MpTag("Untitled", MpHelpers.GetRandomColor());
            newTag.WriteToDatabase();
            AddTagTile(newTag, true);

        }

        private RelayCommand<MpTagTileViewModel> _linkTagToCopyItemCommand;
        public ICommand LinkTagToCopyItemCommand {
            get {
                if (_linkTagToCopyItemCommand == null) {
                    _linkTagToCopyItemCommand = new RelayCommand<MpTagTileViewModel>(LinkTagToCopyItem,CanLinkTagToCopyItem);
                }
                return _linkTagToCopyItemCommand;
            }
        }
        private bool CanLinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            //this checks the selected clips association with tagToLink
            //and only returns if ALL selecteds clips are linked or unlinked 
            if(tagToLink == null || SelectedClipTiles == null || SelectedClipTiles.Count == 0) {
                return false;
            }
            if(SelectedClipTiles.Count == 1) {
                return true;
            }
            bool isLastClipTileLinked = tagToLink.Tag.IsLinkedWithCopyItem(SelectedClipTiles[0].CopyItem);
            foreach(var selectedClipTile in SelectedClipTiles) {
                if (tagToLink.Tag.IsLinkedWithCopyItem(selectedClipTile.CopyItem) != isLastClipTileLinked)
                    return false;
            }
            return true;
        }
        private void LinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            bool isUnlink = tagToLink.Tag.IsLinkedWithCopyItem(SelectedClipTiles[0].CopyItem);
            foreach(var selectedClipTile in SelectedClipTiles) {
                if(isUnlink) {
                    tagToLink.Tag.UnlinkWithCopyItem(selectedClipTile.CopyItem);
                    tagToLink.TagClipCount--;
                } else {
                    tagToLink.Tag.LinkWithCopyItem(selectedClipTile.CopyItem);
                    tagToLink.TagClipCount++;
                }
            }
            //tags and clips have 1-to-1 relationship so remove all other links if it exists before creating new one
            //so loop through all selected clips and sub-loop through all tags and remove links if found            
            //foreach (var clipToRemoveOldLink in SelectedClipTiles) {
            //    foreach (var tagTile in TagTiles) {
            //        if (tagTile.Tag.IsLinkedWithCopyItem(clipToRemoveOldLink.CopyItem) && tagTile.TagName != Properties.Settings.Default.HistoryTagTitle) {
            //            tagTile.Tag.UnlinkWithCopyItem(clipToRemoveOldLink.CopyItem);
            //            tagTile.TagClipCount--;
            //            //if tagToLink was already linked this is an unlink so don't do linking loop
            //            if(tagTile == tagToLink) {
            //                return;
            //            }
            //        }
            //    }
            //}
            ////now loop over all selected clips and link to this tag
            //foreach (var clipToLink in SelectedClipTiles) {
            //    tagToLink.Tag.LinkWithCopyItem(clipToLink.CopyItem);
            //    tagToLink.TagClipCount++;
            //    clipToLink.TitleColor = new SolidColorBrush(tagToLink.Tag.TagColor.Color);
            //}
        }

        private RelayCommand _toggleAppendModeCommand;
        public ICommand ToggleAppendModeCommand {
            get {
                if(_toggleAppendModeCommand == null) {
                    _toggleAppendModeCommand = new RelayCommand(ToggleAppendMode, CanToggleAppendMode);
                }
                return _toggleAppendModeCommand;
            }
        }
        private bool CanToggleAppendMode() {
            //only allow append mode to activate if app is not paused and only ONE clip is selected
            return !IsAppPaused && SelectedClipTiles.Count == 1;
        }
        private void ToggleAppendMode() {
            IsInAppendMode = !IsInAppendMode;
        }

        private RelayCommand _toggleAutoCopyModeCommand;
        public ICommand ToggleAutoCopyModeCommand {
            get {
                if (_toggleAutoCopyModeCommand == null) {
                    _toggleAutoCopyModeCommand = new RelayCommand(ToggleAutoCopyMode, CanToggleAutoCopyMode);
                }
                return _toggleAutoCopyModeCommand;
            }
        }
        private bool CanToggleAutoCopyMode() {
            //only allow append mode to activate if app is not paused and only ONE clip is selected
            return !IsAppPaused;
        }
        private void ToggleAutoCopyMode() {
            IsAutoCopyMode = !IsAutoCopyMode;
        }

        #endregion

        #region Drag and Drop Support
        private void ClipTray_DragEnter(object sender, DragEventArgs e) {
            //var dragClipBorder = (MpClipBorder)e.Data.GetData(typeof(MpClipBorder));
            e.Effects = e.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName) ? DragDropEffects.Move : DragDropEffects.None;
        }
        private void ClipTray_Drop(object sender, DragEventArgs e) {
            var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");

            var dragClipViewModel = (List<MpClipTileViewModel_Old>)e.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);

            
            var mpo = e.GetPosition(clipTray);
            if(mpo.X - dragClipViewModel[0].StartDragPoint.X > 0) {
                mpo.X -= MpMeasurements.Instance.ClipTileMargin * 5;
            } else {
                mpo.X += MpMeasurements.Instance.ClipTileMargin * 5;
            }

            MpClipTileViewModel_Old dropVm = null;
            var item = VisualTreeHelper.HitTest(clipTray, mpo).VisualHit;
            dropVm = (MpClipTileViewModel_Old)item.GetVisualAncestor<MpClipBorder>().DataContext;
            int dropIdx = item == null || item == clipTray ? 0 : ClipTiles.IndexOf(dropVm);
            //if(item.GetType() == typeof(ScrollViewer)) {
            //    dropVm = (MpClipTileViewModel_Old)((ItemsPresenter)((ScrollViewer)item).Content).DataContext;
            //} else if(item.GetType() == typeof(MpClipBorder)) {
            //    dropVm = (MpClipTileViewModel_Old)((MpClipBorder)item).DataContext;
            //}
            if (dropIdx >= 0) {
                ClearClipSelection();
                for (int i = 0; i < dragClipViewModel.Count; i++) {
                    ClipTiles.Remove(dragClipViewModel[i]);
                    ClipTiles.Insert(dropIdx, dragClipViewModel[i]);
                    dragClipViewModel[i].IsSelected = true;
                    if (i == 0) {
                        dragClipViewModel[i].IsFocused = true;
                    }
                }
            } else {
                Console.WriteLine("MainWindow drop error cannot find lasrt moused over tile");
            }
        }
        #endregion
    }
}