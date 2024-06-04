using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSidebarItemCollectionViewModel :
        MpAvViewModelBase,
        MpIAnimatedSizeViewModel {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics

        private static MpAvSidebarItemCollectionViewModel _instance;
        public static MpAvSidebarItemCollectionViewModel Instance => _instance ?? (_instance = new MpAvSidebarItemCollectionViewModel());

        #endregion

        #region MpIAnimatedSizeViewModel Implementation

        public double ContainerBoundWidth { get; set; }
        public double ContainerBoundHeight { get; set; }

        public bool IsAnimating { get; set; }
        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpISidebarItemViewModel> Items { get; private set; } = new ObservableCollection<MpISidebarItemViewModel>();


        public int SelectedItemIdx {
            get => Items.IndexOf(SelectedItem);
            set {
                //if(paramValue < -1) {
                //    return;
                //}
                SelectedItem = value < 0 || value >= Items.Count ? null : Items[value];
            }
        }
        public MpISidebarItemViewModel SelectedItem { get; private set; }
        public MpISidebarItemViewModel LastSelectedItem { get; set; }

        #endregion

        #region Layout

        public double ButtonGroupFixedDimensionLength =>
#if MOBILE_OR_WINDOWED
            60;
#else
            40;
#endif

        public double SelectedItemWidth {
            get {
                if (SelectedItem == null) {
                    return 0;
                }
                return
                    SelectedItem.SidebarWidth;// +
                    //(MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                    //    MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength : 0);
            }
        }

        public double SelectedItemHeight {
            get {
                if (SelectedItem == null) {
                    return 0;
                }
                return
                    SelectedItem.SidebarHeight;// +
                    //(MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                    //    0 : MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength);
            }
        }

        public double TotalSidebarWidth =>
            MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
               ButtonGroupFixedDimensionLength + SelectedItemWidth :
               MpAvMainWindowViewModel.Instance.MainWindowWidth;

        public double TotalSidebarHeight =>
            MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
               MpAvMainWindowViewModel.Instance.MainWindowHeight -
               MpAvMainWindowTitleMenuViewModel.Instance.DefaultTitleMenuFixedLength -
               MpAvFilterMenuViewModel.Instance.DefaultFilterMenuFixedSize :
               ButtonGroupFixedDimensionLength + SelectedItemHeight;

        public int MouseModeHorizontalOffset =>
            MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                20 : 10;

        public int MouseModeVerticalOffset =>
            MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                0 : 10;

        public PlacementMode MouseModeFlyoutPlacement =>
            MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                PlacementMode.Right :
                PlacementMode.Top;

#endregion

        #region State

        bool HasSetStartupSelection { get; set; } = false;
        #endregion

#endregion

        #region Constructors
        private MpAvSidebarItemCollectionViewModel() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            PropertyChanged += MpAvSidebarItemCollectionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void Init() {
            var items = new List<MpISidebarItemViewModel>() {
                MpAvTagTrayViewModel.Instance,
                MpAvClipboardHandlerCollectionViewModel.Instance,
                MpAvAnalyticItemCollectionViewModel.Instance,
                MpAvTriggerCollectionViewModel.Instance
            };

            items.ForEach(x => AddSidebarItem(x));
        }
        private void AddSidebarItem(MpISidebarItemViewModel sbivm) {
            if (Items.Contains(sbivm)) {
                return;
            }
            Items.Add(sbivm);
            sbivm.PropertyChanged += Sbivm_PropertyChanged;
        }
        private void NotifySidebarSelectionChanging() {
            Dispatcher.UIThread.Post(async () => {
                //if (MpAvMainView.Instance.SidebarGridSplitter is not { } gs) {
                //    return;
                //}
                //double t_s = 2;
                //double fps = 50;
                //double dt = 0;
                //double time_step = fps.FpsToTimeStep();
                //int delay_ms = fps.FpsToDelayTime();
                //bool is_opening = SelectedItem != null;
                //double dir = is_opening ? 1 : -1;
                //var ss_start = is_opening ? MpPoint.Zero : 
                //    MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                //        new MpPoint(LastSelectedItem.SidebarWidth,0) :
                //        new MpPoint(0,LastSelectedItem.SidebarHeight);
                //var ss_end = !is_opening ? MpPoint.Zero :
                //    MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                //        new MpPoint(SelectedItem.SidebarWidth, 0) :
                //        new MpPoint(0, SelectedItem.SidebarHeight);
                //var ss_d = ss_end - ss_start;
                //var ss_v = (ss_d / t_s) * time_step * dir;
                //while (true) {
                //    gs.RaiseEvent(new VectorEventArgs() {
                //        RoutedEvent = GridSplitter.DragDeltaEvent,
                //        Source = gs,
                //        Vector = new Vector(ss_v.X, ss_v.Y)
                //    });
                //    MpAvMainView.Instance.UpdateMainViewLayout(is_opening ? MpMainViewUpdateType.SidebarOpen : MpMainViewUpdateType.SidebarClose);
                //    await Task.Delay(delay_ms);
                //    dt += time_step;
                //    if (dt >= t_s) {
                //        // animation complete, ensure it uses end props 
                //        break;
                //    }
                //}

                MpMessenger.SendGlobal(MpMessageType.SelectedSidebarItemChangeBegin);
                MpMessenger.SendGlobal(MpMessageType.SelectedSidebarItemChangeEnd);
            });
        }

        private void MpAvSidebarItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedItem):
                    if (MpAvMainView.Instance == null) {
                        return;
                    }

                    if (SelectedItem != null) {
                        LastSelectedItem = SelectedItem;
                        SelectedItem.OnPropertyChanged(nameof(SelectedItem.IsSelected));
                    }
                    NotifySidebarSelectionChanging();

                    MpAvMainView.Instance.UpdateMainViewLayout(SelectedItem == null ? MpMainViewUpdateType.SidebarClose: MpMainViewUpdateType.SidebarOpen);
                    OnPropertyChanged(nameof(SelectedItemIdx));

                    if (SelectedItem is MpICloseWindowViewModel cwvm &&
                        cwvm.IsWindowOpen &&
                        MpAvWindowManager.LocateWindow(SelectedItem) is MpAvWindow w) {
                        // trigger sidebar pop out
                        w.WindowState = WindowState.Normal;
                        w.Activate();
                        //w.Topmost = true;
                    }
                    break;
            }
        }


        private void Sbivm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(MpISidebarItemViewModel.SidebarWidth):
                    OnPropertyChanged(nameof(SelectedItemWidth));
                    MpMessenger.SendGlobal(MpMessageType.SidebarItemSizeChanged);
                    break;
                case nameof(MpISidebarItemViewModel.SidebarHeight):
                    OnPropertyChanged(nameof(SelectedItemHeight));
                    MpMessenger.SendGlobal(MpMessageType.SidebarItemSizeChanged);
                    break;
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(SelectedItemIdx));
                    break;
                case nameof(SelectedItemIdx):

                    break;
            }
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowLoadComplete:
                    Init();
                    break;
                case MpMessageType.MainWindowOrientationChangeEnd:
                    OnPropertyChanged(nameof(MouseModeFlyoutPlacement));
                    OnPropertyChanged(nameof(MouseModeHorizontalOffset));
                    OnPropertyChanged(nameof(MouseModeVerticalOffset));
                    break;
                case MpMessageType.MainWindowOpened:
//#if MULTI_WINDOW
                    if (MpAvMainWindowViewModel.Instance.IsMainWindowInHiddenLoadState ||
                                    HasSetStartupSelection) {
                        break;
                    }
                    HasSetStartupSelection = true;
                    //SelectSidebarItemCommand.Execute(MpAvTagTrayViewModel.Instance);
//#endif
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand SelectSidebarItemCommand => new MpAsyncCommand<object>(
            async(args) => {
                var sbivm = args as MpISidebarItemViewModel;
                if (sbivm == null) {
                    return;
                }
                SelectedItem = sbivm;

                // BUG pre-selecting tags doesn't show selected in sidebar buttons, waiting a bit..
                await Task.Delay(300);
                OnPropertyChanged(nameof(SelectedItemIdx));
                OnPropertyChanged(nameof(SelectedItem));
            });

        public ICommand ToggleIsSidebarItemSelectedCommand => new MpCommand<object>(
            (args) => {
                int itemIdx = -1;
                if (args is int) {
                    itemIdx = (int)args;
                } else if (args is string argStr) {
                    if (argStr.Contains(",") &&
                        argStr.SplitNoEmpty(",") is string[] argParts) {
                        // for now only dragEnter is a prefix arg
                        itemIdx = int.Parse(argParts[1]);

                        if (itemIdx == SelectedItemIdx) {
                            //MpConsole.WriteLine($"Sidebar DragEnter for idx '{itemIdx}' IGNORED. Its already selected.");
                            return;
                        }
                    } else {
                        itemIdx = int.Parse(argStr);
                    }
                } else if (Items.FirstOrDefault(x => x == args) is MpISidebarItemViewModel sbivm) {
                    itemIdx = Items.IndexOf(sbivm);
                }

                if (itemIdx == 4) {
                    // don't actually select mouse mode since its not a real sidebar
                    return;
                }

                if (SelectedItemIdx == itemIdx) {
                    SelectedItemIdx = -1;
                } else {
                    SelectedItemIdx = itemIdx;
                }
                OnPropertyChanged(nameof(SelectedItemIdx));
            });

        public ICommand SidebarButtonDragEnterCommand => new MpCommand<object>(
            (args) => {
                var button = args as Button;
                if (button == null) {
                    return;
                }
                object param = button.CommandParameter;
                if (param is string paramStr) {
                    param = "DragEnter," + paramStr;
                }
                button.Command?.Execute(param);
            });


        public ICommand ResetSelectedSidebarSplitterCommand => new MpCommand<object>(
            (args) => {
                if (args is not MpAvMovableGridSplitter mgs) {
                    return;
                }

                var mwvm = MpAvMainWindowViewModel.Instance;
                double dw = mwvm.IsHorizontalOrientation ?
                    SelectedItem.DefaultSidebarWidth - SelectedItem.SidebarWidth : 0;
                double dh = mwvm.IsHorizontalOrientation ?
                    0 : SelectedItem.DefaultSidebarHeight - SelectedItem.SidebarHeight;
                //mgs.ApplyDelta(new Vector(dw, dh));
                MpAvClipTrayViewModel.Instance.ContainerBoundWidth -= dw;
                MpAvSidebarItemCollectionViewModel.Instance.ContainerBoundWidth += dw;

                MpAvClipTrayViewModel.Instance.ContainerBoundHeight -= dh;
                MpAvSidebarItemCollectionViewModel.Instance.ContainerBoundHeight += dh;
            });

        #endregion
    }
}
