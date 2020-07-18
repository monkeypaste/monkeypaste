
using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase, IDragSource {
        #region Private Variables
        private double _startMainWindowTop, _endMainWindowTop;

        
        private MpHotKeyHost _hotkeyHost = null;
        private IKeyboardMouseEvents _globalHook = null;

        //private MpPhysicsBody _clipTrayPhysicsBody;

        #endregion

        #region Public Variables
        public MpClipboardMonitor ClipboardMonitor { get; private set; }
        #endregion

        #region Properties
        
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

        public List<string> SortTypeList {
            get {
                var sortTypes = new List<string>();
                sortTypes.Add("Date");
                sortTypes.Add("Application");
                sortTypes.Add("Title");
                sortTypes.Add("Content");
                sortTypes.Add("Usage");
                return sortTypes;
            }
        }
        #endregion

        #region Business Logic Properties
        private string _selectedSortType;
        public string SelectedSortType {
            get {
                return _selectedSortType;
            }
             set {
                if(_selectedSortType != value) {
                    _selectedSortType = value;
                    OnPropertyChanged(nameof(SelectedSortType));
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

        public double AppStateButtonGridWidth {
            get {
                return MpMeasurements.Instance.AppStateButtonPanelWidth;
            }
        }

        private double _clipTrayHeight = MpMeasurements.Instance.TrayHeight;
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
        public void MainWindowLoaded(object sender,RoutedEventArgs e) {
            SearchText = Properties.Settings.Default.SearchPlaceHolderText;
            
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
                AddClipTile(c);
            }

            ResetSelection();

            //create tiles for all the tags
            foreach (MpTag t in MpTag.GetAllTags()) {
                AddTagTile(t);
            }
            //select history tag by default
            TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;

            //init SearchBox

            PropertyChanged += MainWindowViewModel_PropertyChanged;

            ((MpMainWindow)Application.Current.MainWindow).Deactivated += (s, e1) => {
                HideWindowCommand.Execute(null);
            };


            InitClipboard();
            InitHotKeys();

#if DEBUG
            ShowWindow();
#else
            HideWindowCommand.Execute(null);
#endif
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SearchText):
                    if (SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                        return;
                    }
                    FilterTiles(SearchText);
                    Sort("CopyItemId", false);

                    ResetSelection();
                    break;
                case nameof(IsAutoCopyMode):
                    if (IsAutoCopyMode) {
                        _globalHook.MouseUp += GlobalMouseUpEvent;
                    } else {
                        _globalHook.MouseUp -= GlobalMouseUpEvent;
                    }
                    break;
            }
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
        }
        public void SearchTextboxLostFocus(object sender, RoutedEventArgs e) {
            //var searchTextBox = (TextBox)e.Source;
            if (string.IsNullOrEmpty(SearchText)) {
                SearchText = Properties.Settings.Default.SearchPlaceHolderText;
                SearchTextBoxFontStyle = FontStyles.Italic;
                SearchTextBoxTextBrush = Brushes.DimGray;
            }
        }
        public void SearchTextboxBorderPassFocus() {
            ((TextBox)((MpMainWindow)Application.Current.MainWindow).FindName("SearchTextBox")).Focus();
        }
        public void ScrollClipTray(object sender, MouseWheelEventArgs e) {
            var clipTrayListBox = (ListBox)sender;
            var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();
            double lastOffset = scrollViewer.HorizontalOffset;

            //_clipTrayPhysicsBody.AddForce(e.Delta);
            scrollViewer.ScrollToHorizontalOffset(lastOffset - (double)(e.Delta * 0.3));

            e.Handled = true;
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
                                TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;
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
                VisibileClipTiles[0].IsSelected = true;
                VisibileClipTiles[0].IsFocused = true;
                var clipTrayListBox = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");
                if(clipTrayListBox != null && clipTrayListBox.Items != null && clipTrayListBox.Items.Count > 0) {
                    clipTrayListBox.ScrollIntoView(clipTrayListBox.Items[0]);
                }
            }
        }

        public void AddClipTile(MpCopyItem ci) {
            ClipTiles.Insert(0, new MpClipTileViewModel(ci, this));
            //update cliptray visibility if this is the first cliptile added
            ClipListVisibility = Visibility.Visible;
            EmptyListMessageVisibility = Visibility.Collapsed;
            
            ResetSelection();
        }

        public void MoveClipTile(MpClipTileViewModel clipTile, int newIdx) {
            if (newIdx > ClipTiles.Count) {
                throw new Exception("Cannot insert tile clip tile at index: " + newIdx + " with list of length: " + ClipTiles.Count);
            }
            ClipTiles.Remove(clipTile);
            ClipTiles.Insert(newIdx, clipTile);
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
                TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;
            }
            TagTiles.Remove(tagTileToRemove);
            tagTileToRemove.Tag.DeleteFromDatabase();
        }

        private void RemoveClipTile(MpClipTileViewModel clipTileToRemove) {
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
                var ct = ClipTiles[i];
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
                    Button b = new Button();
                    b.MouseLeftButtonUp += (s, e) => {

                    };
                } else {
                    ClipTiles[i].TileVisibility = Visibility.Collapsed;
                }
            }            
        }


        private void Sort(string sortBy, bool ascending) {
            if(ascending) {
                ClipTiles.OrderBy(x => MpTypeHelper.GetPropertyValue(x.CopyItem, sortBy));
            } else {
                ClipTiles.OrderByDescending(x => MpTypeHelper.GetPropertyValue(x.CopyItem, sortBy));
            }
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
                MpCopyItem newClip = MpCopyItem.CreateFromClipboard(ClipboardMonitor.LastWindowWatcher.LastHandle);
                if (IsInAppendMode && newClip.CopyItemType == MpCopyItemType.RichText) {
                    //when in append mode just append the new items text to selecteditem
                    SelectedClipTiles[0].RichText += Environment.NewLine + (string)newClip.DataObject;
                    return;
                }
                if (newClip != null) {
                    newClip.WriteToDatabase();
                    MpTag historyTag = new MpTag(1);
                    historyTag.LinkWithCopyItem(newClip);
                    AddClipTile(newClip);
                }
            };
        }
        #endregion

        #region Commands
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
                //ShowMainWindowHotKey.Enabled = true;
                //HideMainWindowHotKey.Enabled = false;
                //_clipTrayPhysicsBody.Stop();
            };
            CubicEase easing = new CubicEase();  // or whatever easing class you want
            easing.EasingMode = EasingMode.EaseIn;
            ta.EasingFunction = easing;
            mw.BeginAnimation(Window.TopProperty, ta);            
        }

        private RelayCommand _deleteClipCommand;
        public ICommand DeleteClipCommand {
            get {
                if (_deleteClipCommand == null) {
                    _deleteClipCommand = new RelayCommand(DeleteClip);
                }
                return _deleteClipCommand;
            }
        }
        private void DeleteClip() {
            foreach (var ct in SelectedClipTiles) {
                RemoveClipTile(ct);
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
            return SelectedTagTile.TagName != "History";
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
        public void StartDrag(IDragInfo dragInfo) {            
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.StartDrag(dragInfo);
            string selectedText = "";
            foreach (MpClipTileViewModel ctvm in SelectedClipTiles) {
                selectedText += ctvm.RichText + "\r\n";
            }

            string tempFilePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\temp.txt";
            System.IO.StreamWriter tempFile = new System.IO.StreamWriter(tempFilePath);
            tempFile.Write(selectedText);
            tempFile.Close();
            List<String> tempFileList = new List<string>();
            tempFileList.Add(tempFilePath);

            DataObject d = new DataObject();            
            d.SetData(DataFormats.FileDrop, tempFileList.ToArray<string>());
            d.SetData(DataFormats.Text, selectedText);
            System.Windows.DragDrop.DoDragDrop(dragInfo.VisualSource, d, DragDropEffects.Copy);
        }

        public bool CanStartDrag(IDragInfo dragInfo) {
            return GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.CanStartDrag(dragInfo);
        }

        public void Dropped(IDropInfo dropInfo) {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.Dropped(dropInfo);
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo) {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.DragDropOperationFinished(operationResult, dragInfo);
            string tempFilePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\temp.txt";
            if(File.Exists(tempFilePath)) {
                File.Delete(tempFilePath);
            }
        }

        public void DragCancelled() {

            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.DragCancelled();
        }

        public bool TryCatchOccurredException(Exception exception) {
            return GongSolutions.Wpf.DragDrop.DragDrop.DefaultDragHandler.TryCatchOccurredException(exception);
        }
        #endregion
    }
}