
using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase, IDragSource {
        #region Private Variables
        private double _startMainWindowTop, _endMainWindowTop;
        private bool _doPaste = false;
        private MpHotKeyHost _hotkeyHost = null;
        private IKeyboardMouseEvents _globalHook = null;

        //private MpPhysicsBody _clipTrayPhysicsBody;

        #endregion

        #region Public Variables
        public MpClipboardMonitor ClipboardMonitor { get; private set; }
        #endregion

        #region Collection Properties
        
        private ObservableCollection<MpClipTileViewModel> _clipTiles = new ObservableCollection<MpClipTileViewModel>();
        public ObservableCollection<MpClipTileViewModel> ClipTiles {
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

        public List<MpClipTileViewModel> SelectedClipTiles {
            get {
                return ClipTiles.Where(ct => ct.IsSelected).ToList();
            }
        }

        public List<MpClipTileViewModel> VisibileClipTiles {
            get {
                return ClipTiles.Where(ct => ct.TileVisibility == Visibility.Visible).ToList();
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

        #endregion

        #region Business Logic Properties

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
        #endregion

        #region View Properties
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
        public MpMainWindowViewModel() {
            base.DisplayName = "MpMainWindowViewModel";
            
        }
        #endregion

        #region View Event Handlers
        public void MainWindow_Loaded(object sender,RoutedEventArgs e) {
            SearchText = Properties.Settings.Default.SearchPlaceHolderText;
            SelectedSortType = SortTypes[0];
            SetupMainWindowRect();

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
                AddTagTile(new MpTag("Home", MpHelpers.GetRandomColor()));
                AddTagTile(new MpTag("Favorites", MpHelpers.GetRandomColor()));
                AddTagTile(new MpTag("C#", MpHelpers.GetRandomColor()));
                for (int i = 0; i < 15; i++) {
                    AddClipTile(MpCopyItem.CreateCopyItem(MpCopyItemType.RichText, MpCopyItem.PlainTextToRtf(MpHelpers.GetRandomString(100, 100)), MpHelpers.GetProcessPath(Process.GetCurrentProcess().Handle), MpHelpers.GetRandomColor()));
                }

                return;
            }
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

            PropertyChanged += (s,e1) => {
                switch (e1.PropertyName) {
                    case nameof(SearchText):
                        if (SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                            return;
                        }
                        FilterTiles(SearchText);
                        //SortClipTiles();
                        if (!string.IsNullOrEmpty(SearchText.Trim())) {
                            if (VisibileClipTiles.Count > 0) {
                                SearchTextBoxBorderBrush = Brushes.Transparent;
                            } else {
                                SearchTextBoxBorderBrush = Brushes.Red;
                            }
                        }
                        ResetSelection();
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

            ((MpMainWindow)Application.Current.MainWindow).Deactivated += (s, e2) => {
                HideWindowCommand.Execute(null);
            };
            ((MpMainWindow)Application.Current.MainWindow).PreviewKeyDown += MainWindow_KeyDown;
            
            InitClipboard();
            InitHotKeys();

#if DEBUG
            ShowWindow();
#else
            HideWindowCommand.Execute(null);
#endif
        }

        public void ClipTray_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            BindingExpression be = (BindingExpression)((ListBox)sender).GetBindingExpression(ListBox.SelectedItemsProperty);
            if (be != null) {
                be.UpdateTarget();
            }
            MergeClipsCommandVisibility = SelectedClipTiles.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public void SearchTextboxGotFocus(object sender,RoutedEventArgs e) {
            //make text
            if(SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                SearchText = "";
            }
            SearchTextBoxFontStyle = FontStyles.Normal;
            SearchTextBoxTextBrush = Brushes.Black;
            IsSearchTextBoxFocused = true;

            TestAnimatedScroll();
        }
        public void SearchTextboxLostFocus(object sender, RoutedEventArgs e) {
            //var searchTextBox = (TextBox)e.Source;
            if (string.IsNullOrEmpty(SearchText)) {
                SearchText = Properties.Settings.Default.SearchPlaceHolderText;
                SearchTextBoxFontStyle = FontStyles.Italic;
                SearchTextBoxTextBrush = Brushes.DimGray;
            }
            IsSearchTextBoxFocused = false;
        }

        public void ScrollClipTray(object sender, MouseWheelEventArgs e) {
            e.Handled = true;

            var clipTrayListBox = (ListBox)sender;
            var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (e.Delta * -1) / 5);
        }
        public void TestAnimatedScroll() {
            var mw = ((MpMainWindow)Application.Current.MainWindow);
            var clipTrayListBox = (ListBox)mw.FindName("ClipTray");

            var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();

            DoubleAnimation ta = new DoubleAnimation();
            ta.From = scrollViewer.HorizontalOffset;
            ta.To = scrollViewer.HorizontalOffset + 10000;
            ta.Duration = new Duration(TimeSpan.FromSeconds(7));
            CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseIn;
            ta.EasingFunction = easing;
            ta.Completed += (s, e1) => {
                //_clipTrayPhysicsBody.Start();
                //ShowMainWindowHotKey.Enabled = false;
                //HideMainWindowHotKey.Enabled = true;
            };

            //Storyboard storyboard = new Storyboard();

            //storyboard.Children.Add(ta);
            //Storyboard.SetTarget(ta, scrollViewer);
            //Storyboard.SetTargetProperty(ta, new PropertyPath(MpScrollViewFixer.VerticalOffsetProperty));
            //storyboard.Begin();
            //scrollViewer.BeginAnimation(MpScrollViewFixer.CurrentHorizontalOffsetProperty, ta);
            //mw.BeginAnimation(Window.TopProperty, ta);
        }
        // NOTE KeyUp cannot handle Enter key so KeyDown MUST be used
        public void MainWindow_KeyDown(object sender, KeyEventArgs e) {
            if(IsSearchTextBoxFocused) {
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
                            foreach (MpClipTileViewModel clipTile in ClipTiles) {
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

                                ResetSelection();
                            }
                        }
                        break;
                }
            };
            if (isNew) {
                //newTagTile.RenameTagCommand.Execute(null);
                //newTagTile.IsSelected = true;
                //newTagTile.IsEditing = true;
            } else {
                foreach(var ctvm in ClipTiles) {
                    if(newTagTile.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                        newTagTile.TagClipCount++;
                    }
                }
            }
        }

        public void ClearSelection() {
            foreach (var clip in ClipTiles) {
                clip.IsSelected = false;
            }
        }

        public void ResetSelection() {
            ClearSelection();
            if(VisibileClipTiles.Count > 0) {
                var clipTray = (ListBox)Application.Current.MainWindow.FindName("ClipTray");
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(clipTray.ItemsSource);
                ((MpClipTileViewModel)view.GetItemAt(0)).IsSelected = true;
            }
        }

        public void AddClipTile(MpCopyItem ci,bool isNew = false) {
            if (isNew) {
                ci.WriteToDatabase();
                MpTag historyTag = new MpTag(1);
                historyTag.LinkWithCopyItem(ci);
                GetHistoryTagTileViewModel().TagClipCount++;
            }

            MpClipTileViewModel newClipTile = new MpClipTileViewModel(ci, this);

            ClipTiles.Insert(0, newClipTile);

            //update cliptray visibility if this is the first cliptile added
            ClipListVisibility = Visibility.Visible;
            EmptyListMessageVisibility = Visibility.Collapsed;            
        }

        public void MoveClipTile(MpClipTileViewModel clipTile, int newIdx) {
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

        private void RemoveClipTile(MpClipTileViewModel clipTileToRemove) {
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
            if (ClipTiles == null) {
                return;
            }
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

            //ClearSelection();

            var clipTray = (ListBox)Application.Current.MainWindow.FindName("ClipTray");
            //int itemCount = clipTray.Items.Count;
            //ensures current item is head of collection
            //ResetSelection();
            ICollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(clipTray.ItemsSource);
            using (view.DeferRefresh()) {
                view.SortDescriptions.Clear();
                if (ascending) {
                    //for (int i = 0; i < itemCount; i++) {
                    //    int minIdx = FindMinIdxByProperty(ClipTiles, sortBy, i);
                    //    MoveClipTile(ClipTiles[minIdx], i);
                    //}
                    view.SortDescriptions.Add(new SortDescription(sortBy, ListSortDirection.Ascending));
                } else {
                    view.SortDescriptions.Add(new SortDescription(sortBy, ListSortDirection.Descending));
                    //for (int i = 0; i < itemCount; i++) {
                    //    int maxIdx = FindMaxIdxByProperty(ClipTiles, sortBy, i);
                    //    MoveClipTile(ClipTiles[maxIdx], i);
                    //}
                }
            }
            ResetSelection();
        }

        private int FindMaxIdxByProperty(ObservableCollection<MpClipTileViewModel> list,string propertyName,int startIdx = 0) {
            if(list == null || list.Count <= startIdx) {
                return -1;
            }
            var maxItem = (MpClipTileViewModel)list[startIdx];
            for (int i = startIdx+1; i < list.Count; i++) {
                var curItem = (MpClipTileViewModel)list[i];
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

        private int FindMinIdxByProperty(ObservableCollection<MpClipTileViewModel> list, string propertyName, int startIdx = 0) {
            if (list == null || list.Count <= startIdx) {
                return -1;
            }
            var maxItem = (MpClipTileViewModel)list[startIdx];
            for (int i = startIdx + 1; i < list.Count; i++) {
                var curItem = (MpClipTileViewModel)list[i];
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

                    ResetSelection();
                }
            };
        }

        private MpClipTileViewModel FindClipTileByModel(MpCopyItem ci) {
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
            //MpClipTileViewModel[] selectedClipTile_copies = new MpClipTileViewModel[SelectedClipTiles.Count];
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
            bool canMerge = true;
            foreach(var sc in SelectedClipTiles) {
                if(sc.CopyItem.CopyItemType != MpCopyItemType.RichText) {
                    canMerge = false;
                }
            }
            return SelectedClipTiles.Count > 1 && canMerge;
        }
        private void MergeClips() {
            var focusedClip = SelectedClipTiles[0];
            List<MpClipTileViewModel> clipTilesToRemove = new List<MpClipTileViewModel>();
            foreach(MpClipTileViewModel selectedClipTile in SelectedClipTiles) {
                if(selectedClipTile == focusedClip) {
                    continue;
                }
                focusedClip.RichText += "\r\n" + selectedClipTile.RichText;
                clipTilesToRemove.Add(selectedClipTile);
            }
            foreach(MpClipTileViewModel tileToRemove in clipTilesToRemove) {
                RemoveClipTile(tileToRemove);
            }
            focusedClip.IsSelected = true;
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
            return Application.Current.MainWindow == null || Application.Current.MainWindow.Visibility != Visibility.Visible;
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

            ResetSelection();

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
                    _linkTagToCopyItemCommand = new RelayCommand<MpTagTileViewModel>(LinkTagToCopyItem);
                }
                return _linkTagToCopyItemCommand;
            }
        }
        private void LinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            //tags and clips have 1-to-1 relationship so remove all other links if it exists before creating new one
            //so loop through all selected clips and sub-loop through all tags and remove links if found            
            foreach (var clipToRemoveOldLink in SelectedClipTiles) {
                foreach (var tagTile in TagTiles) {
                    if (tagTile.Tag.IsLinkedWithCopyItem(clipToRemoveOldLink.CopyItem) && tagTile.TagName != Properties.Settings.Default.HistoryTagTitle) {
                        tagTile.UnlinkWithClipTile(clipToRemoveOldLink);
                        //tagTile.Tag.UnlinkWithCopyItem(clipToRemoveOldLink.CopyItem);
                    }
                }
            }
            //now loop over all selected clips and link to this tag
            foreach (var clipToLink in SelectedClipTiles) {
                SelectedTagTile.LinkToClipTile(clipToLink);
                clipToLink.TitleColor = new SolidColorBrush(SelectedTagTile.Tag.TagColor.Color);
            }
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
        //public void ClipTray_Drop(object sender, DragEventArgs e) {
        //    if (e.Data.GetDataPresent("MonkeyPasteFormat")) {
        //        var dropClipTileViewModel = (MpClipTileViewModel)e.Data.GetData("MonkeyPasteFormat");
        //        MpClipTileViewModel hoverClipTileViewModel = null;
        //        foreach (var vctvm in VisibileClipTiles) {
        //            if (vctvm.IsHovering) {
        //                hoverClipTileViewModel = vctvm;
        //            }
        //        }
        //        if (hoverClipTileViewModel != null) {
        //            int dragIdx = VisibileClipTiles.IndexOf(dropClipTileViewModel);
        //            int dropIdx = VisibileClipTiles.IndexOf(hoverClipTileViewModel);
        //            MoveClipTile(dropClipTileViewModel, dropIdx);
        //        }
        //    }
        //}

        public void StartDrag(IDragInfo dragInfo) {
            dragInfo.Effects = DragDropEffects.Move | DragDropEffects.Copy;
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.StartDrag(dragInfo);
            return;

            string selectedRichText = string.Empty;
            string selectedRawText = string.Empty;
            foreach (MpClipTileViewModel ctvm in SelectedClipTiles) {
                selectedRichText += ctvm.RichText + Environment.NewLine;
                selectedRawText += ctvm.CopyItem.GetPlainText() + Environment.NewLine;
            }

            string tempFilePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\temp.txt";
            if (File.Exists(tempFilePath)) {
                File.Delete(tempFilePath);
            }
            System.IO.StreamWriter tempFile = new System.IO.StreamWriter(tempFilePath);
            tempFile.Write(selectedRawText);
            tempFile.Close();
            List<String> tempFileList = new List<string>();
            tempFileList.Add(tempFilePath);

            IDataObject d = new DataObject();// Properties.Settings.Default.ClipTileDragDropFormatName, dragInfo.SourceItems);
            d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, (MpClipTileViewModel) dragInfo.SourceItem);
            switch (SelectedClipTiles[0].CopyItem.CopyItemType) {
                case MpCopyItemType.RichText:
                    d.SetData(DataFormats.Text, selectedRawText);
                    d.SetData(DataFormats.Rtf, selectedRichText);
                    d.SetData(DataFormats.FileDrop, tempFileList.ToArray<string>());
                    break;
                case MpCopyItemType.Image:
                    d.SetData(DataFormats.Bitmap, (BitmapSource)SelectedClipTiles[0].CopyItem.DataObject);
                    break;
                case MpCopyItemType.FileList:
                    d.SetData(DataFormats.FileDrop,(StringCollection)SelectedClipTiles[0].CopyItem.DataObject);
                    break;
            }
            dragInfo.DataObject = d;
            //dragInfo.DataObject = SelectedClipTiles;
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.StartDrag(dragInfo);
            //System.Windows.DragDrop.DoDragDrop(dragInfo.VisualSource, d, DragDropEffects.Copy);
        }

        public bool CanStartDrag(IDragInfo dragInfo) {

            //need to update canstartdrag to ensure all selected tiles are richtext or 
            //selection is single filelist or image
            return GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.CanStartDrag(dragInfo);
        }

        public void Dropped(IDropInfo dropInfo) {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.Dropped(dropInfo);
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo) {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.DragDropOperationFinished(operationResult, dragInfo);
            //string tempFilePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\temp.txt";
            //if (File.Exists(tempFilePath)) {
            //    File.Delete(tempFilePath);
            //}
        }

        public void DragCancelled() {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.DragCancelled();
            //string tempFilePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\temp.txt";
            //if (File.Exists(tempFilePath)) {
            //    File.Delete(tempFilePath);
            //}
        }

        public bool TryCatchOccurredException(Exception exception) {
            return GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.TryCatchOccurredException(exception);
        }


        #endregion
    }
}