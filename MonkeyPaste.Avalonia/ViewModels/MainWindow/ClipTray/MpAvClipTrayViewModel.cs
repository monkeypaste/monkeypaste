using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpAvClipTrayLayoutType { 
        Stack,
        Grid
    }

    public class MpAvClipTrayViewModel : MpSelectorViewModelBase<object,MpAvClipTileViewModel>,
        MpIBootstrappedItem {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvClipTrayViewModel _instance;
        public static MpAvClipTrayViewModel Instance => _instance ?? (_instance = new MpAvClipTrayViewModel());

        #endregion

        #region Properties

        #region MpIBoostrappedItem Implementation

        string MpIBootstrappedItem.Label => "Content Tray";
        #endregion

        #region View Models

        #endregion

        #region Layout

        public MpPoint ScrollOffset => new MpPoint(ScrollOffsetX, ScrollOffsetY);

        public double ScrollOffsetX { get; set; }

        public double ScrollOffsetY { get; set; }

        #endregion

        #region Appearance

        public ScrollBarVisibility HorizontalScrollBarVisibility {
            get {
                //var mwvm = MpAvMainWindowViewModel.Instance;
                //if(LayoutType == MpAvClipTrayLayoutType.Stack) {
                //    if(mwvm.MainWindowOrientationType == MpMainWindowOrientationType.)
                //}
                return ScrollBarVisibility.Auto;
            }
        }

        public MpAvClipTrayLayoutType LayoutType { get; set; } = MpAvClipTrayLayoutType.Stack;

        #endregion

        #region State

        public MpPoint ScrollVelocity => new MpPoint(ScrollVelocityX, ScrollVelocityY);

        public double ScrollVelocityX { get; set; }

        public double ScrollVelocityY { get; set; }

        public bool HasScrollVelocity => ScrollVelocity.Length > 0;

        public bool IsScrollingIntoView { get; set; }

        public bool IsGridLayout { get; set; }

        public bool CanScroll {
            get {
                return true;

                if(MpAvMainWindowViewModel.Instance.IsMainWindowOpening ||
                   !MpAvMainWindowViewModel.Instance.IsMainWindowOpen ||
                    IsRequery ||
                   IsScrollingIntoView) {
                    return false;
                }
                if(SelectedItem == null) {
                    return true;
                }
                if (SelectedItem.IsVerticalScrollbarVisibile &&
                    SelectedItem.IsHovering &&
                    SelectedItem.IsVisible) {
                    return false;
                }
                return true;
            }
        }

        public bool IsRequery { get; set; } = false;

        public bool IsThumbDragging { get; set; } = false;

        #endregion


        #endregion

        #region Constructors

        private MpAvClipTrayViewModel() : base() {
            PropertyChanged += MpAvClipTrayViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            IsBusy = true;

            for(int i = 1;i <= 100;i++) {
                var test_ctvm = await CreateClipTileViewModel(
                    new MpCopyItem() {
                        Id = i,
                        ItemType = MpCopyItemType.Text,
                        ItemData = "This is test "+i,
                        Title = "Test"+i
                    });
                Items.Add(test_ctvm);
            }

            SelectedItem = Items[0];

            OnPropertyChanged(nameof(Items));

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalkMessage);
            

            IsBusy = false;
        }

        

        #endregion

        #region Private Methods

        private async Task<MpAvClipTileViewModel> CreateClipTileViewModel(MpCopyItem ci) {
            MpAvClipTileViewModel ctvm = new MpAvClipTileViewModel(this);
            await ctvm.InitializeAsync(ci);
            return ctvm;
        }


        private void MpAvClipTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(Items):
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(CanScroll));
                    break;
                case nameof(IsGridLayout):
                    ToggleLayoutTypeCommand.Execute(null);
                    break;
            }
        }

        private void ReceivedGlobalkMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowOrientationChanged:
                    
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ToggleLayoutTypeCommand => new MpCommand(
            () => {
                if(IsGridLayout) {
                    LayoutType = MpAvClipTrayLayoutType.Grid;
                } else {
                    LayoutType = MpAvClipTrayLayoutType.Stack;
                }
                MpMessenger.SendGlobal(MpMessageType.TrayLayoutChanged);
            });
        #endregion
    }
}
