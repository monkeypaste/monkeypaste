
using MpWinFormsClassLibrary;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase {
        #region View/Model Collection Properties
        private ObservableCollection<MpClipTileViewModel> _clipTiles = new ObservableCollection<MpClipTileViewModel>();
        public ObservableCollection<MpClipTileViewModel> ClipTiles {
            get {
                return _clipTiles;
            }
            set {
                if(_clipTiles != value) {
                    _clipTiles = value;
                }
            }
        }

        public List<MpClipTileViewModel> SelectedClipTiles {
            get {
                return ClipTiles.Where(ct => ct.IsSelected).ToList();
            }
        }

        private ObservableCollection<MpTagTileViewModel> _tagTiles = new ObservableCollection<MpTagTileViewModel>();
        public ObservableCollection<MpTagTileViewModel> TagTiles {
            get {
                return _tagTiles;
            }
            set {
                if(_tagTiles != value) {
                    _tagTiles = value;
                }
            }
        }
        #endregion

        #region Business Logic Properties
        private bool _isInAppendMode = false;
        public bool IsInAppendMode {
            get {
                return _isInAppendMode;
            }
            set {
                if(_isInAppendMode != value) {
                    _isInAppendMode = value;
                    OnPropertyChanged("IsInAppendMode");
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
                    OnPropertyChanged("SearchText");
                }
            }
        }

        private MpHotKeyHost _hotkeyHost = null;
        public MpHotKeyHost HotKeyHost {
            get {
                return _hotkeyHost;
            }
            set {
                if(_hotkeyHost != value) {
                    _hotkeyHost = value;
                    OnPropertyChanged("HotKeyHost");
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
                    OnPropertyChanged("IsAppPaused");
                }
            }
        }
        #endregion

        #region View Properties
        public double AppStateButtonGridWidth {
            get {
                return MpMeasurements.Instance.AppStateButtonPanelWidth;
            }
        }

        public double TrayHeight {
            get {
                return MpMeasurements.Instance.TrayHeight;
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

        public double MainWindowWidth {
            get {
                return SystemParameters.PrimaryScreenWidth;
            }
        }

        public double MainWindowHeight {
            get {
                return MpMeasurements.Instance.MainWindowRect.Height;
            }
        }
        #endregion

        #region Constructor
        public MpMainWindowViewModel() {
            base.DisplayName = "MpMainWindowViewModel";

            //clears model data and loads everything from db and setups clipboard listener
            MpDataStore.Instance.Init();

            //init ClipTiles

            //when clipboard changes add a cliptile
            MpDataStore.Instance.ClipList.CollectionChanged += (s1, e1) => {
                if(e1.NewItems != null) {
                    foreach(MpClip c in e1.NewItems) {
                        AddClipTile(c);
                    }
                }
                if(e1.OldItems != null) {
                    foreach(MpClip c in e1.OldItems) {
                        RemoveClipTile(ClipTiles.Where(ct => ct.CopyItem == c).ToList()[0]);
                    }
                }
            };
            //create tiles for all clips in the database
            foreach(MpClip c in MpDataStore.Instance.ClipList) {
                AddClipTile(c);
            }
            //select first tile by default
            if(ClipTiles.Count > 0) {
                ClipTiles[0].IsSelected = true;
                ClipTiles[0].IsFocused = true;
            }

            //init TagTiles

            //when a tag is added or deleted reflect it in the tiles
            MpDataStore.Instance.TagList.CollectionChanged += (s2, e2) => {
                if(e2.NewItems != null) {
                    foreach(MpTag t in e2.NewItems) {
                        AddTagTile(t,true);
                    }
                }
                if(e2.OldItems != null) {
                    foreach(MpTag t in e2.OldItems) {
                        RemoveTagTile(TagTiles.Where(tt => tt.Tag == t).ToList()[0]);
                    }
                }
            };
            //create tiles for all the tags
            foreach(MpTag t in MpDataStore.Instance.TagList) {
                AddTagTile(t);
            }
            //select history tag by default
            TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;

            //init SearchBox

            //perform filter method anytime search text changes
            PropertyChanged += (s, e) => {
                if(e.PropertyName == "SearchText") {
                    FilterTiles(SearchText);
                    Sort("CopyItemId", false);

                    var visibleClipTiles = ClipTiles.Where(ct => ct.Visibility == Visibility.Visible).ToList();
                    if(visibleClipTiles != null && visibleClipTiles.Count > 0) {
                        foreach(var visibleClipTile in visibleClipTiles) {
                            visibleClipTile.IsSelected = false;
                        }
                        visibleClipTiles[0].IsSelected = true;
                    }
                }
            };

        }
        #endregion

        #region View Event Handlers
       
        #endregion

        #region Private Methods
        private void AddTagTile(MpTag t, bool isNew = false) {
            var newTagTile = new MpTagTileViewModel(t);
            //watches Tag IsSelected so History is selected if none are
            newTagTile.PropertyChanged += (s, e) => {
                if(e.PropertyName == "IsSelected") {
                    var tagChanged = ((MpTagTileViewModel)s);
                    //ensure at least history is selected
                    if(tagChanged.IsSelected == false) {
                        //find all selected tag tiles
                        var selectedTagTiles = TagTiles.Where(tt => tt.IsSelected == true).ToList();
                        //if none selected select history tag
                        if(selectedTagTiles == null || selectedTagTiles.Count == 0) {
                            TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;
                        }
                    } else {
                        foreach(MpClipTileViewModel clipTile in ClipTiles) {
                            if(tagChanged.Tag.IsLinkedWithCopyItem(clipTile.CopyItem)) {
                                clipTile.Visibility = Visibility.Visible;
                            } else {
                                clipTile.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            };
            TagTiles.Add(newTagTile);
            if(isNew) {
                newTagTile.IsEditing = true;
            }
        }

        private void RemoveTagTile(MpTagTileViewModel tagTileToRemove) {
            if(tagTileToRemove.IsSelected) {
                tagTileToRemove.IsSelected = false;
                TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;
            }
            TagTiles.Remove(tagTileToRemove);
        }

        private void AddClipTile(MpClip ci) {
            //always make new cliptile the only one selected
            //first clear all selections
            foreach(var ct in ClipTiles) {
                ct.IsSelected = false;
            }
            //then create/add new tile with selected = true
            var newClipTile = new MpClipTileViewModel(ci);
            ClipTiles.Insert(0, newClipTile);
            newClipTile.IsSelected = true;
        }

        private void RemoveClipTile(MpClipTileViewModel clipTileToRemove) {
            //when the clip is selected change selection to previous tile or next if it is first tile
            if(clipTileToRemove.IsSelected) {
                clipTileToRemove.IsSelected = false;
                if(ClipTiles.Count > 1) {
                    if(ClipTiles.IndexOf(clipTileToRemove) == 0) {
                        ClipTiles[1].IsSelected = true;
                    } else {
                        ClipTiles[ClipTiles.IndexOf(clipTileToRemove) - 1].IsSelected = true;
                    }
                }
            }
            ClipTiles.Remove(clipTileToRemove);
        }

        private void FilterTiles(string searchStr) {
            List<int> filteredTileIdxList = new List<int>();
            //search ci's from newest to oldest for filterstr, adding idx to list
            for(int i = ClipTiles.Count - 1;i >= 0;i--) {
                //when search string is empty add each item to list so all shown
                if(searchStr == string.Empty) {
                    filteredTileIdxList.Add(i);
                    continue;
                }
                MpClip ci = ClipTiles[i].CopyItem;
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
                if(ci.CopyItemType == MpCopyItemType.Text) {
                    if(((string)ci.GetData()).ToLower().Contains(searchStr.ToLower())) {
                        filteredTileIdxList.Add(i);
                    }
                }
                //lastly add filelist clips if search string found in it's path(s)
                else if(ci.CopyItemType == MpCopyItemType.FileList) {
                    foreach(string p in (string[])ci.GetData()) {
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
                    ClipTiles[i].Visibility = Visibility.Visible;
                    vcount++;
                } else {
                    ClipTiles[i].Visibility = Visibility.Collapsed;
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
        #endregion

        #region Commands
        private DelegateCommand<MouseWheelEventArgs> _scrollClipTrayCommand;
        public ICommand ScrollClipTrayCommand {
            get {
                if(_scrollClipTrayCommand == null) {
                    _scrollClipTrayCommand = new DelegateCommand<MouseWheelEventArgs>(ScrollClipTray);
                }
                return _scrollClipTrayCommand;
            }
        }
        private void ScrollClipTray(MouseWheelEventArgs e) {
            var clipTrayListBox = ((ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray"));
            var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();
            double lastOffset = scrollViewer.HorizontalOffset;
            scrollViewer.ScrollToHorizontalOffset(lastOffset - (double)(e.Delta * 0.3));
        }

        private MpHotKey _showMainWindowHotKey = null;
        public MpHotKey ShowMainWindowHotKey {
            get {
                return _showMainWindowHotKey;
            }
            set {
                if(_showMainWindowHotKey != value) {
                    _showMainWindowHotKey = value;
                    OnPropertyChanged("ShowMainWindowHotKey");
                }
            }
        }
        private DelegateCommand _showWindowCommand;
        public ICommand ShowWindowCommand {
            get {
                if(_showWindowCommand == null) {
                    _showWindowCommand = new DelegateCommand(ShowWindow, CanShowWindow);
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
            var mw = ((MpMainWindow)Application.Current.MainWindow);
            mw.Show();
            mw.Activate();
            mw.Visibility = Visibility.Visible;
            mw.Topmost = true;

            ShowMainWindowHotKey.Enabled = false;
            HideMainWindowHotKey.Enabled = true;
        }

        private MpHotKey _hideMainWindowHotKey = null;
        public MpHotKey HideMainWindowHotKey {
            get {
                return _hideMainWindowHotKey;
            }
            set {
                if(_hideMainWindowHotKey != value) {
                    _hideMainWindowHotKey = value;
                    OnPropertyChanged("ShowMainWindowHotKey");
                }
            }
        }
        private DelegateCommand _hideWindowCommand;
        public ICommand HideWindowCommand {
            get {
                if(_hideWindowCommand == null) {
                    _hideWindowCommand = new DelegateCommand(HideWindow, CanHideWindow);
                }
                return _hideWindowCommand;
            }
        }
        private bool CanHideWindow() {
            return Application.Current.MainWindow != null && Application.Current.MainWindow.Visibility == Visibility.Visible;
        }
        private void HideWindow() {
            var mw = ((MpMainWindow)Application.Current.MainWindow);
            mw.Visibility = Visibility.Collapsed;
            ShowMainWindowHotKey.Enabled = true;
            HideMainWindowHotKey.Enabled = false;
        }

        private DelegateCommand _addTagCommand;
        public ICommand AddTagCommand {
            get {
                if(_addTagCommand == null) {
                    _addTagCommand = new DelegateCommand(CreateNewTag);
                }
                return _addTagCommand;
            }
        }
        private void CreateNewTag() {
            //add tag to datastore so TagTile collection will automatically add the tile
            MpTag newTag = new MpTag("Untitled", MpHelperSingleton.Instance.GetRandomColor());
            newTag.WriteToDatabase();
            MpDataStore.Instance.TagList.Add(newTag);
        }

        private MpHotKey _toggleAppendModeHotKey;
        public MpHotKey ToggleAppendModeHotKey {
            get {
                return _toggleAppendModeHotKey;
            }
            set {
                if(_toggleAppendModeHotKey != value) {
                    _toggleAppendModeHotKey = value;
                    OnPropertyChanged("ToggleAppendModeHotKey");
                }
            }
        }
        private DelegateCommand _toggleAppendModeCommand;
        public ICommand ToggleAppendModeCommand {
            get {
                if(_toggleAppendModeCommand == null) {
                    _toggleAppendModeCommand = new DelegateCommand(ToggleAppendMode, CanToggleAppendMode);
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
        #endregion

        #region Overrides
        protected override void Loaded() {
            base.Loaded();

            var mw = ((MpMainWindow)Application.Current.MainWindow);
            mw.Width = SystemParameters.PrimaryScreenWidth;
            mw.Height = SystemParameters.PrimaryScreenHeight * 0.35;
            mw.Left = 0;
            mw.Top = SystemParameters.WorkArea.Height - mw.Height;

            mw.Deactivated += (s, e) => {
                HideWindowCommand.Execute(null);
            };

            HotKeyHost = new MpHotKeyHost(HwndSource.FromHwnd(new WindowInteropHelper(Application.Current.MainWindow).Handle));

            ShowMainWindowHotKey = new MpHotKey(Key.D, ModifierKeys.Control | ModifierKeys.Shift);
            ShowMainWindowHotKey.HotKeyPressed += (s, e1) => {
                if(ShowWindowCommand.CanExecute(null)) {
                    ShowWindowCommand.Execute(null);
                }
            };

            HideMainWindowHotKey = new MpHotKey(Key.Escape, ModifierKeys.None);
            HideMainWindowHotKey.HotKeyPressed += (s, e) => {
                if(HideWindowCommand.CanExecute(null)) {
                    HideWindowCommand.Execute(null);
                }
            };

            ToggleAppendModeHotKey = new MpHotKey(Key.A, ModifierKeys.Control | ModifierKeys.Shift);
            ToggleAppendModeHotKey.HotKeyPressed += (s, e2) => {
                if(ToggleAppendModeCommand.CanExecute(null)) {
                    ToggleAppendModeCommand.Execute(null);
                }
            };

            HotKeyHost.AddHotKey(ShowMainWindowHotKey);
            HotKeyHost.AddHotKey(HideMainWindowHotKey);
            HotKeyHost.AddHotKey(ToggleAppendModeHotKey);

#if DEBUG
            //ShowWindowCommand.Execute(null);
#else
            HideWindowCommand.Execute(null);
#endif
        }

#endregion
    }
}