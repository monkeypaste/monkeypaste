using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace MonkeyPaste.Avalonia {
    public class MpAvSidebarItemCollectionViewModel : 
        MpAvSelectorViewModelBase<object,MpISidebarItemViewModel>,
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

        public double BoundWidth { get; set; }
        public double BoundHeight { get; set; }

        #endregion

        #region Properties

        #region View Models

        public override MpISidebarItemViewModel LastSelectedItem {
            get {
                if(base.LastSelectedItem == null) {
                    return null;
                }
                if (MpPlatformWrapper.Services.StartupState.LoadedDateTime == null) {
                    return null;
                }
                if(base.LastSelectedItem.LastSelectedDateTime < MpPlatformWrapper.Services.StartupState.LoadedDateTime) {
                    return null;
                }
                return base.LastSelectedItem;
            }
        }

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
            sbivm.PropertyChanged += Sbivm_PropertyChanged;
        }
        private void MpAvSidebarItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedItem):
                    if(MpAvMainWindow.Instance == null) {
                        return;
                    }
                    if(SelectedItem != null) {
                        SelectedItem.LastSelectedDateTime = DateTime.Now;
                    }
                    OnPropertyChanged(nameof(LastSelectedItem));
                    MpAvMainWindow.Instance.UpdateContentLayout();
                    break;
            }
        }

        private void Sbivm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var sbivm = sender as MpISidebarItemViewModel;
            switch(e.PropertyName) {
                case nameof(sbivm.IsSelected):
                    OnPropertyChanged(nameof(SelectedItem));
                    break;
            }
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
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
                if(sbivm == null) {
                    return;
                }
                SelectedItem = sbivm;
            });
        #endregion
    }
}
