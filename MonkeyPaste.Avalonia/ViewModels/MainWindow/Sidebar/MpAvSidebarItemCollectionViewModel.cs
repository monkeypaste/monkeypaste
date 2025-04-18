﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSidebarItemCollectionViewModel :
        MpAvViewModelBase,
        MpIAnimatedSizeViewModel {
        #region Private Variable
        private Control _currentSidebarItem;
        #endregion

        #region Constants
        #endregion

        #region Statics

        private static MpAvSidebarItemCollectionViewModel _instance;
        public static MpAvSidebarItemCollectionViewModel Instance => _instance ?? (_instance = new MpAvSidebarItemCollectionViewModel());

        #endregion

        #region MpIAnimatedSizeViewModel Implementation
        private double _containerBoundWidth;
        public double ContainerBoundWidth { 
            get => _containerBoundWidth;
            set {
                if(ContainerBoundWidth != value && value >= 0) {
                    _containerBoundWidth = value;
                    OnPropertyChanged(nameof(ContainerBoundWidth));
                }
            }
        }
        private double _containerBoundHeight;
        public double ContainerBoundHeight {
            get => _containerBoundHeight;
            set {
                if (ContainerBoundHeight != value && value >= 0) {
                    _containerBoundHeight = value;
                    OnPropertyChanged(nameof(ContainerBoundHeight));
                }
            }
        }

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
        public MpISidebarItemViewModel BoundItem { get; private set; }
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
        private async Task HandleSidebarSelectionChangedAsync() {
            double start_w,start_h,end_w,end_h;
            bool is_horiz = MpAvMainWindowViewModel.Instance.IsHorizontalOrientation;
            bool is_closing = SelectedItem == null;
            if(is_closing) {
                // closing
                start_w = LastSelectedItem == null ? 0 : LastSelectedItem.SidebarWidth;
                start_h = LastSelectedItem == null ? 0 : LastSelectedItem.SidebarHeight;
                if(is_horiz) {
                    end_w = 0;
                    end_h = start_h;
                } else {
                    end_w = start_w;
                    end_h = 0;
                }
            } else {
                // opening
                if(is_horiz) {
                    start_w = 0;
                    start_h = SelectedItem.DefaultSidebarHeight;
                    end_w = SelectedItem.DefaultSidebarWidth;
                    end_h = start_h;
                } else {
                    start_w = SelectedItem.DefaultSidebarWidth;
                    start_h = 0;
                    end_w = start_w;
                    end_h = SelectedItem.DefaultSidebarHeight;
                }
            }
            var ss_start = new MpSize(start_w, start_h);
            var ss_end = new MpSize(end_w, end_h);
            IsAnimating = true;
            await AnimateSidebarAsync(ss_start, ss_end, is_closing, 0.1, 100);

            if(is_closing) {
                // manually clear both dim AFTER animation (or constraints could be wrong after orientation change)
                ResetSize();
            }
            MpAvMainView.Instance.UpdateMainViewLayout();
            IsAnimating = false;
            // handle sidebar focus
            if (SelectedItem is MpICloseWindowViewModel cwvm &&
                cwvm.IsWindowOpen &&
                MpAvWindowManager.LocateWindow(SelectedItem) is MpAvWindow w) {
                // trigger sidebar pop out
                w.WindowState = WindowState.Normal;
                w.Activate();
            } 

            if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                FocusSidebarOrFallbackCommand.Execute(null);
            }
        }
        private void ResetSize() {
            SetSidebarSize(-ContainerBoundWidth, -ContainerBoundHeight);
        }
        private async Task AnimateSidebarAsync(
            MpSize start, 
            MpSize end, 
            bool isClosing,
            double tt = 0.25, 
            double fps = 120) {
            double op_start = isClosing ? 1 : 0;
            double op_end = isClosing ? 0 : 1;

            await Task.WhenAll([
                start.AnimatePointAsync(
                    end: end,
                    tts: tt,
                    fps: fps,
                    tick: (d) => {
                        SetSidebarSize(d.X,d.Y);
                    }),
                op_start.AnimateDoubleAsync(
                    end: op_end,
                    tts: tt,
                    fps: fps,
                    tick: (d) => {
                        SetSidebarContentOpacity(d);
                    })
                ]);
            _currentSidebarItem = null;
        }
        private void MpAvSidebarItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedItem):
                    Items.ForEach(x => x.IsSelected = x == SelectedItem);
                    if (SelectedItem == null) {
                    } else {
                        BoundItem = SelectedItem;
                        LastSelectedItem = SelectedItem;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        MpMessenger.SendGlobal(MpMessageType.SelectedSidebarItemChangeBegin);
                        await HandleSidebarSelectionChangedAsync();
                        OnPropertyChanged(nameof(SelectedItemIdx));
                        BoundItem = SelectedItem;
                        MpMessenger.SendGlobal(MpMessageType.SelectedSidebarItemChangeEnd);
                    });
                  
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
                case MpMessageType.MainWindowOrientationChangeBegin:
                    ResetSize();
                    break;
                case MpMessageType.MainWindowOrientationChangeEnd:
                    HandleSidebarSelectionChangedAsync().FireAndForgetSafeAsync();
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



        public void SetSidebarSizeByDelta(double dw, double dh) {
            SetSidebarSize(ContainerBoundWidth + dw, ContainerBoundHeight + dh);
        }
        public void SetSidebarSize(double w, double h) {
            double dw = w - ContainerBoundWidth;
            double dh = h - ContainerBoundHeight;
            ContainerBoundWidth = w;
            ContainerBoundHeight = h;

            if (MpAvMainWindowViewModel.Instance.IsVerticalOrientation) {
                MpAvClipTrayViewModel.Instance.ContainerBoundHeight -= dh;
            } else {
                MpAvClipTrayViewModel.Instance.ContainerBoundWidth -= dw;
            }
        }

        private void SetSidebarContentOpacity(double opacity) {
            if (_currentSidebarItem == null &&
                MpAvMainView.Instance.SelectedSidebarContainerBorder.GetVisualDescendants<MpAvUserControl>().OfType<MpAvISidebarContentView>().FirstOrDefault() is Control sbc) {
                _currentSidebarItem = sbc;
            }
            if(_currentSidebarItem == null) {
                return;
            }
            _currentSidebarItem.Opacity = opacity;
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

        public ICommand FocusSidebarOrFallbackCommand => new MpCommand(
            () => {
                if (SelectedItem is MpAvTriggerCollectionViewModel &&
                        MpAvMainView.Instance.GetVisualDescendant<MpAvTriggerActionChooserView>() is { } tw) {

                    tw.FocusThisHeader();
                } else if (SelectedItem == null) {
                    MpAvMainView.Instance.FocusThisHeader();
                } else {

                }
            });

        #endregion
    }
}
