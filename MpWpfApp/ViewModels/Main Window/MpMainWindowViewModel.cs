
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
    public class MpMainWindowViewModel : MpViewModelBase {
        #region Private Variables
        private double _startMainWindowTop, _endMainWindowTop;
        #endregion

        #region Public Variables
        public MpClipboardMonitor ClipboardMonitor { get; private set; }
        public MpHotKeyHost HotkeyHost { get; set; } = null;
        public IKeyboardMouseEvents GlobalHook { get; set; } = null;

        #endregion

        #region Properties
        private MpSystemTrayViewModel _systemTrayViewModel = null;
        public MpSystemTrayViewModel SystemTrayViewModel {
            get {
                return _systemTrayViewModel;
            }
            set {
                if (_systemTrayViewModel != value) {
                    _systemTrayViewModel = value;
                    OnPropertyChanged(nameof(SystemTrayViewModel));
                }
            }
        }

        private MpClipTrayViewModel _clipTrayViewModel = null;
        public MpClipTrayViewModel ClipTrayViewModel {
            get {
                return _clipTrayViewModel;
            }
            set {
                if(_clipTrayViewModel != value) {
                    _clipTrayViewModel = value;
                    OnPropertyChanged(nameof(ClipTrayViewModel));
                }
            }
        }

        private MpTagTrayViewModel _tagTrayViewModel = null;
        public MpTagTrayViewModel TagTrayViewModel {
            get {
                return _tagTrayViewModel;
            }
            set {
                if (_tagTrayViewModel != value) {
                    _tagTrayViewModel = value;
                    OnPropertyChanged(nameof(TagTrayViewModel));
                }
            }
        }

        private MpClipTileSortViewModel _clipTileSortViewModel = null;
        public MpClipTileSortViewModel ClipTileSortViewModel {
            get {
                return _clipTileSortViewModel;
            }
            set {
                if (_clipTileSortViewModel != value) {
                    _clipTileSortViewModel = value;
                    OnPropertyChanged(nameof(ClipTileSortViewModel));
                }
            }
        }

        private MpSearchBoxViewModel _searchBoxViewModel = null;
        public MpSearchBoxViewModel SearchBoxViewModel {
            get {
                return _searchBoxViewModel;
            }
            set {
                if (_searchBoxViewModel != value) {
                    _searchBoxViewModel = value;
                    OnPropertyChanged(nameof(SearchBoxViewModel));
                }
            }
        }

        private MpAppModeViewModel _appModeViewModel = null;
        public MpAppModeViewModel AppModeViewModel {
            get {
                return _appModeViewModel;
            }
            set {
                if (_appModeViewModel != value) {
                    _appModeViewModel = value;
                    OnPropertyChanged(nameof(AppModeViewModel));
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

        #endregion

        #region Constructor
        public MpMainWindowViewModel() {
            IsLoading = true;
            ClipTileSortViewModel = new MpClipTileSortViewModel(this);
            TagTrayViewModel = new MpTagTrayViewModel(this);
            SearchBoxViewModel = new MpSearchBoxViewModel(this);
            AppModeViewModel = new MpAppModeViewModel(this);
            ClipTrayViewModel = new MpClipTrayViewModel(this);
        }
        #endregion

        #region View Event Handlers
        public void MainWindow_Loaded(object sender,RoutedEventArgs e) {
            var mw = ((MpMainWindow)Application.Current.MainWindow);
            mw.Deactivated += (s, e2) => {
                HideWindowCommand.Execute(null);
            };
            mw.PreviewKeyDown += MainWindow_KeyDown;

            SetupMainWindowRect();

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
            if(SearchBoxViewModel.IsSearchTextBoxFocused || TagTrayViewModel.IsTagTextBoxFocused) {
                return;
            }
            Key key = e.Key;
            if (key == Key.Delete || key == Key.Back) {
                if(ClipTrayViewModel.SelectedClipTiles.Count == 1 && ClipTrayViewModel.SelectedClipTiles[0].ClipTileTitleViewModel.IsEditingTitle) {
                    return;
                }
                //delete clip which shifts focus to neighbor
                ClipTrayViewModel.DeleteSelectedClipsCommand.Execute(null);
            } else if (key == Key.Enter) {
                if (ClipTrayViewModel.SelectedClipTiles.Count == 1 && ClipTrayViewModel.SelectedClipTiles[0].ClipTileTitleViewModel.IsEditingTitle) {
                    ClipTrayViewModel.SelectedClipTiles[0].ClipTileTitleViewModel.IsEditingTitle = false;
                    e.Handled = true;
                    return;
                } else {
                    ClipTrayViewModel.PasteSelectedClipsCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region App Mode Event Handlers
        
        #endregion

        #region Public Methods        

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
            GlobalHook = Hook.GlobalEvents();
            GlobalHook.MouseMove += (s, e) => {
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
                if (AppModeViewModel.IsInAppendMode && newCopyItem.CopyItemType == MpCopyItemType.RichText) {
                    //when in append mode just append the new items text to selecteditem
                    ClipTrayViewModel.SelectedClipTiles[0].RichText += Environment.NewLine + (string)newCopyItem.DataObject;
                    return;
                }
                
                if (newCopyItem != null) {
                    //check if copyitem is duplicate
                    var existingClipTile = FindClipTileByModel(newCopyItem);
                    if(existingClipTile == null) {
                        ClipTrayViewModel.AddClipTile(newCopyItem,true);
                    } else {
                        existingClipTile.CopyItem.CopyCount++;
                        existingClipTile.CopyItem.CopyDateTime = DateTime.Now;
                        ClipTrayViewModel.MoveClipTile(existingClipTile, 0);
                    }

                    ClipTrayViewModel.ResetClipSelection();
                }
            };
        }

        private MpClipTileViewModel FindClipTileByModel(MpCopyItem ci) {
            foreach(var ctvm in ClipTrayViewModel) {
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
            foreach(var sctvm in ClipTrayViewModel.SelectedClipTiles) {
                int curIdx = ClipTrayViewModel.VisibileClipTiles.IndexOf(sctvm);
                if (curIdx < firstIdx) {
                    curIdx = firstIdx;
                }
            }
            return firstIdx < int.MaxValue ? firstIdx : -1;
        }

        private int GetLastSelectedClipTileIdx() {
            int lastIdx = int.MinValue;
            foreach (var sctvm in ClipTrayViewModel.SelectedClipTiles) {
                int curIdx = ClipTrayViewModel.VisibileClipTiles.IndexOf(sctvm);
                if (curIdx > lastIdx) {
                    curIdx = lastIdx;
                }
            }
            return lastIdx > int.MinValue ? lastIdx : -1;
        }

        
        #endregion

        #region Commands

        private RelayCommand exitCommand;
        public ICommand ExitCommand {
            get {
                if (exitCommand == null) {
                    exitCommand = new RelayCommand(Exit);
                }
                return exitCommand;
            }
        }
        private void Exit() {
            Application.Current.Shutdown();
        }

        private RelayCommand closedCommand;
        public ICommand ClosedCommand {
            get {
                if (closedCommand == null) {
                    closedCommand = new RelayCommand(Closed);
                }
                return closedCommand;
            }
        }
        private void Closed() {
            //log.Add("You won't see this of course! Closed command executed");
            //MessageBox.Show("Closed");
        }

        private RelayCommand closingCommand;
        public ICommand ClosingCommand {
            get {
                if (closingCommand == null) {
                    closingCommand = new RelayCommand(
                        ExecuteClosing, CanExecuteClosing);
                }
                return closingCommand;
            }
        }
        private void ExecuteClosing() {
            //log.Add("Closing command executed");
            MessageBox.Show("Closing");
        }
        private bool CanExecuteClosing() {
            //log.Add("Closing command execution check");

            return MessageBox.Show("OK to close?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private RelayCommand cancelClosingCommand;
        public ICommand CancelClosingCommand {
            get {
                if (cancelClosingCommand == null) {
                    cancelClosingCommand = new RelayCommand(CancelClosing);
                }
                return cancelClosingCommand;
            }
        }
        private void CancelClosing() {
            MessageBox.Show("CancelClosing");
        }

        public ICommand ExitApplicationCommand {
            get {
                return new RelayCommand(Application.Current.Shutdown);
            }
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

            ClipTrayViewModel.ResetClipSelection();

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
                if(ClipTrayViewModel.DoPaste) {
                    ClipTrayViewModel.DoPaste = false;
                    foreach (var clipTile in ClipTrayViewModel.SelectedClipTiles) {
                        ClipboardMonitor.PasteCopyItem(clipTile.CopyItem);
                        ClipTrayViewModel.MoveClipTile(clipTile, 0);
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

        #endregion

        
    }
}