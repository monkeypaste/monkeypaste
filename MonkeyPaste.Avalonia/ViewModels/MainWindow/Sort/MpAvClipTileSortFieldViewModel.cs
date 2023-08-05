using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileSortFieldViewModel :
        MpAvViewModelBase {
        #region Statics
        private static MpAvClipTileSortFieldViewModel _instance;
        public static MpAvClipTileSortFieldViewModel Instance => _instance ?? (_instance = new MpAvClipTileSortFieldViewModel());

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region View Models

        private ObservableCollection<string> _sortLabels;
        public ObservableCollection<string> SortLabels {
            get {
                if (_sortLabels == null) {
                    _sortLabels = new ObservableCollection<string>(
                        typeof(MpContentSortType)
                        .EnumToLabels());
                }
                return _sortLabels;
            }
        }
        #endregion

        #region Layout
        #endregion

        #region State

        public int SelectedSortTypeIdx { get; set; } = (int)MpContentSortType.CopyDateTime;
        public MpContentSortType SelectedSortType {
            get => (MpContentSortType)SelectedSortTypeIdx;
            set => SelectedSortTypeIdx = (int)value;
        }

        public bool IsSortDropDownOpen { get; set; }
        //public bool IsReseting { get; private set; } = false;
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors



        public MpAvClipTileSortFieldViewModel() : base(null) {
            PropertyChanged += MpClipTileSortViewModel_PropertyChanged;
        }
        #endregion

        #region Public Methods
        public void Init() {
        }

        #endregion

        #region Private Methods       
        private void MpClipTileSortViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedSortType):
                    MpMessenger.SendGlobal(MpMessageType.QuerySortChanged);
                    Mp.Services.Query.NotifyQueryChanged();
                    break;
            }

        }

        #endregion

        #region Commands
        #endregion
    }
}
