using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using MonkeyPaste;
using System.Threading.Tasks;
using Avalonia;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileSortViewModel : MpViewModelBase, MpIAsyncSingletonViewModel<MpAvClipTileSortViewModel> {
        #region Statics
        private static MpAvClipTileSortViewModel _instance;
        public static MpAvClipTileSortViewModel Instance => _instance ?? (_instance = new MpAvClipTileSortViewModel());

        #endregion

        #region Properties

        #region Layout
        public double ClipTileSortViewWidth { get; set; }
        public MpRect ClipTileSortViewBounds { get; set; } = new MpRect();
        #endregion

        #region State

        public int SelectedSortTypeIdx { get; set; } = (int)MpContentSortType.CopyDateTime;
        public MpContentSortType SelectedSortType => (MpContentSortType)SelectedSortTypeIdx; 

        public bool IsSortDescending { get; set; } = true;

        public bool IsReseting { get; private set; } = false;
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors



        public MpAvClipTileSortViewModel() : base(null) {
            PropertyChanged += MpClipTileSortViewModel_PropertyChanged;
        }
        #endregion

        #region Public Methods
        public async Task InitAsync() {
            await Task.Delay(1);
            ResetToDefault(true);
        }

        public void ResetToDefault(bool suppressNotifyQueryChanged = false) {
            IsReseting = true;

            SelectedSortTypeIdx = (int)MpContentSortType.CopyDateTime;
            IsSortDescending = true;

            IsReseting = false;

            if(!suppressNotifyQueryChanged) {
                MpDataModelProvider.QueryInfo.NotifyQueryChanged();
            }
        }
        #endregion

        #region Private Methods       
        private void MpClipTileSortViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ClipTileSortViewWidth):
                    MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.TagTrayScreenWidth));
                    break;
                case nameof(ClipTileSortViewBounds):
                    MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.TagTrayScreenWidth));
                    break;
                case nameof(SelectedSortType):
                    if (!IsReseting) {
                        MpDataModelProvider.QueryInfo.NotifyQueryChanged();
                    }
                    break;
                case nameof(IsSortDescending):
                    if (!IsReseting) {
                        MpDataModelProvider.QueryInfo.NotifyQueryChanged();
                    }
                    break;
            }
            
        }
        #endregion

        #region Commands
        #endregion
    }
}
