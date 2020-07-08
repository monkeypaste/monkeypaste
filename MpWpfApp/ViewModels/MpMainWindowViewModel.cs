
using Gma.System.MouseKeyHook;
using GongSolutions.Wpf.DragDrop;
using MpWinFormsClassLibrary;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MpClipboardMonitor ClipboardMonitor { get; private set; }
        #endregion

        #region View/Model Collection Properties
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
        #endregion

        #region Constructor
        public MpMainWindowViewModel() {
            base.DisplayName = "MpMainWindowViewModel";
            if(DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
                return;
            }
            //create tiles for all clips in the database
            foreach (MpCopyItem c in MpCopyItem.GetAllCopyItems()) {
                AddClipTile(c);
            }

            ResetSelection();

            //create tiles for all the tags
            foreach(MpTag t in MpTag.GetAllTags()) {
                AddTagTile(t);
            }
            //select history tag by default
            TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;

            //init SearchBox

            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SearchText):
                        //perform filter method anytime search text changes
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
            };
            ((MpMainWindow)Application.Current.MainWindow).Deactivated += (s, e) => {
                HideWindowCommand.Execute(null);
            };

            SetupMainWindowRect();
        }
        #endregion

        #region Overrides
        protected override void Loaded() {
            base.Loaded();
            //SetupMainWindowRect();

            

            InitHotKeys();
            InitClipboard();

#if DEBUG
            ShowWindowCommand.Execute(null);
#else
            HideWindowCommand.Execute(null);
#endif
        }
        #endregion

        #region View Event Handlers
        public void ClipTray_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            BindingExpression be = (BindingExpression)((ListBox)sender).GetBindingExpression(ListBox.SelectedItemsProperty);
            if (be != null) {
                be.UpdateTarget();
            }
            MergeClipsCommandVisibility = SelectedClipTiles.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region App Mode Event Handlers
        private void GlobalMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e) {
            if(e.Button == System.Windows.Forms.MouseButtons.Left && !MpHelperSingleton.Instance.ApplicationIsActivated()) {
                System.Windows.Forms.SendKeys.SendWait("^c");
            }
        }
        #endregion

        #region Public Methods

        public void AddTagTile(MpTag t, bool isNew = false) {
            var newTagTile = new MpTagTileViewModel(t, this);
            TagTiles.Add(newTagTile);
            //watches Tag IsSelected so History is selected if none are
            newTagTile.PropertyChanged += (s, e) => {
                if (e.PropertyName == "IsSelected") {
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
                }
            };
            if (isNew) {
                //newTagTile.RenameTagCommand.Execute(null);
                //newTagTile.IsSelected = true;
                newTagTile.IsEditing = true;
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
                    if((ct.Text).ToLower().Contains(searchStr.ToLower())) {
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
            _hotkeyHost = new MpHotKeyHost(HwndSource.FromHwnd(new WindowInteropHelper(Application.Current.MainWindow).Handle));

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

            _hotkeyHost.AddHotKey(ShowMainWindowHotKey);
            _hotkeyHost.AddHotKey(HideMainWindowHotKey);
            _hotkeyHost.AddHotKey(ToggleAppendModeHotKey);

            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseMove += (s, e) => {
                if(e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                    if (ShowWindowCommand.CanExecute(null)) {
                        ShowWindowCommand.Execute(null);
                    }
                }
            };
        }

        private void InitClipboard() {
            ClipboardMonitor = new MpClipboardMonitor((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));

            // Attach the handler to the event raising on WM_DRAWCLIPBOARD message is received
            ClipboardMonitor.ClipboardChanged += (s, e) => {
                MpCopyItem newClip = MpCopyItem.CreateFromClipboard(ClipboardMonitor.LastWindowWatcher.LastHandle);
                if (IsInAppendMode && newClip.CopyItemType == MpCopyItemType.RichText) {
                    //when in append mode just append the new items text to selecteditem
                    SelectedClipTiles[0].RichText += Environment.NewLine + (string)newClip.GetData();
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
        private DelegateCommand _mergeClipsCommand;
        public ICommand MergeClipsCommand {
            get {
                if(_mergeClipsCommand == null) {
                    _mergeClipsCommand = new DelegateCommand(MergeClips,CanMergeClips);
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
            //if mouse is over a selected clip that has scrollbars ignore scrolling the tray
            foreach(var clipTile in VisibileClipTiles) {
                if(clipTile.IsSelected && clipTile.IsHovering && clipTile.HasScrollBars) {
                    return;
                }
            }
            var clipTrayListBox = ((ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray"));
            var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();
            double lastOffset = scrollViewer.HorizontalOffset;
            scrollViewer.ScrollToHorizontalOffset(lastOffset - (double)(e.Delta * 0.3));
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
            mw.BeginAnimation(Window.TopProperty, ta);

            ShowMainWindowHotKey.Enabled = false;
            HideMainWindowHotKey.Enabled = true;
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

            DoubleAnimation ta = new DoubleAnimation();
            ta.From = _endMainWindowTop;
            ta.To = _startMainWindowTop;
            ta.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
            ta.Completed += (s, e) => {
                mw.Visibility = Visibility.Collapsed;
            };
            CubicEase easing = new CubicEase();  // or whatever easing class you want
            easing.EasingMode = EasingMode.EaseIn;
            ta.EasingFunction = easing;
            mw.BeginAnimation(Window.TopProperty, ta);

            ShowMainWindowHotKey.Enabled = true;
            HideMainWindowHotKey.Enabled = false;
        }

        private DelegateCommand _deleteClipCommand;
        public ICommand DeleteClipCommand {
            get {
                if (_deleteClipCommand == null) {
                    _deleteClipCommand = new DelegateCommand(DeleteClip);
                }
                return _deleteClipCommand;
            }
        }
        private void DeleteClip() {
            foreach (var ct in SelectedClipTiles) {
                RemoveClipTile(ct);
            }
        }

        private DelegateCommand _renameClipCommand;
        public ICommand RenameClipCommand {
            get {
                if (_renameClipCommand == null) {
                    _renameClipCommand = new DelegateCommand(RenameClip,CanRenameClip);
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

        private DelegateCommand _deleteTagCommand;
        public ICommand DeleteTagCommand {
            get {
                if (_deleteTagCommand == null) {
                    _deleteTagCommand = new DelegateCommand(DeleteTag, CanDeleteTag);
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

        private DelegateCommand _createTagCommand;
        public ICommand CreateTagCommand {
            get {
                if(_createTagCommand == null) {
                    _createTagCommand = new DelegateCommand(CreateTag);
                }
                return _createTagCommand;
            }
        }
        private void CreateTag() {
            //add tag to datastore so TagTile collection will automatically add the tile
            MpTag newTag = new MpTag("Untitled", MpHelperSingleton.Instance.GetRandomColor());
            newTag.WriteToDatabase();
            AddTagTile(newTag, true);
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

        private DelegateCommand _toggleAutoCopyModeCommand;
        public ICommand ToggleAutoCopyModeCommand {
            get {
                if (_toggleAutoCopyModeCommand == null) {
                    _toggleAutoCopyModeCommand = new DelegateCommand(ToggleAutoCopyMode, CanToggleAutoCopyMode);
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