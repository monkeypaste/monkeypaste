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
                SelectedItem = value < 0 || value > Items.Count ? null : Items[value];
            }
        }
        public MpISidebarItemViewModel SelectedItem { get; private set; }
        public MpISidebarItemViewModel LastSelectedItem { get; private set; }

        #endregion

        #region Layout

        public double ButtonGroupFixedDimensionLength => 40;

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
                } else if (args is string) {
                    itemIdx = int.Parse(args.ToString());
                }
                if (SelectedItemIdx == itemIdx) {
                    SelectedItemIdx = -1;
                } else {
                    SelectedItemIdx = itemIdx;
                }
                OnPropertyChanged(nameof(SelectedItemIdx));
            });
        #endregion
    }
}
