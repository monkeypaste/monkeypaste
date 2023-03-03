using Avalonia.Controls;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSidebarItemCollectionViewModel :
        MpViewModelBase,
        MpIBoundSizeViewModel {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics

        private static MpAvSidebarItemCollectionViewModel _instance;
        public static MpAvSidebarItemCollectionViewModel Instance => _instance ?? (_instance = new MpAvSidebarItemCollectionViewModel());

        #endregion

        #region MpIBoundSizeViewModel Implementation

        public double ContainerBoundWidth { get; set; }
        public double ContainerBoundHeight { get; set; }

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpISidebarItemViewModel> Items { get; private set; } = new ObservableCollection<MpISidebarItemViewModel>();
        //public override MpISidebarItemViewModel LastSelectedItem {
        //    get {
        //        if(base.LastSelectedItem == null) {
        //            return null;
        //        }
        //        if (MpPlatformWrapper.Services.StartupState.LoadedDateTime == null) {
        //            return null;
        //        }
        //        if(base.LastSelectedItem.LastSelectedDateTime < MpPlatformWrapper.Services.StartupState.LoadedDateTime) {
        //            return null;
        //        }
        //        return base.LastSelectedItem;
        //    }
        //}

        public int SelectedItemIdx {
            get => Items.IndexOf(SelectedItem);
            set {
                //if(value < -1) {
                //    return;
                //}
                SelectedItem = value < 0 || value >= Items.Count ? null : Items[value];
            }
        }
        public MpISidebarItemViewModel SelectedItem { get; private set; }
        public MpISidebarItemViewModel LastSelectedItem { get; private set; }

        #endregion

        #region Layout

        public double ButtonGroupFixedDimensionLength => 40;

        public double SelectedItemWidth {
            get {
                if (SelectedItem == null) {
                    return 0;
                }
                return
                    SelectedItem.SidebarWidth +
                    (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                        MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength : 0);
            }
        }

        public double SelectedItemHeight {
            get {
                if (SelectedItem == null) {
                    return 0;
                }
                return
                    SelectedItem.SidebarHeight +
                    (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                        0 : MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength);
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


        private void MpAvSidebarItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedItem):
                    if (MpAvMainView.Instance == null) {
                        return;
                    }

                    if (SelectedItem != null) {
                        LastSelectedItem = SelectedItem;
                    }
                    MpAvMainView.Instance.UpdateContentLayout();
                    OnPropertyChanged(nameof(SelectedItemIdx));
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
                case nameof(SelectedItemIdx):
                    MpMessenger.SendGlobal(MpMessageType.SelectedSidebarItemChanged);
                    break;
            }
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowLoadComplete:
                    Init();
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand SelectSidebarItemCommand => new MpCommand<object>(
            (args) => {
                var sbivm = args as MpISidebarItemViewModel;
                if (sbivm == null) {
                    return;
                }
                SelectedItem = sbivm;
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
        #endregion
    }
}
