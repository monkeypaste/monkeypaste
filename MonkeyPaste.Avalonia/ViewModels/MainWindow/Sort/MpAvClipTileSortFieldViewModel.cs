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
    public class MpAvClipTileSortFieldViewModel : 
        MpViewModelBase, 
        MpISingletonViewModel<MpAvClipTileSortFieldViewModel>,
        MpIQueryInfoValueProvider {
        #region Statics
        private static MpAvClipTileSortFieldViewModel _instance;
        public static MpAvClipTileSortFieldViewModel Instance => _instance ?? (_instance = new MpAvClipTileSortFieldViewModel());

        #endregion

        #region MpIQueryInfoProvider Implementation

        object MpIQueryInfoValueProvider.Source => this;
        string MpIQueryInfoValueProvider.SourcePropertyName => nameof(SelectedSortType);

        string MpIQueryInfoValueProvider.QueryValueName => nameof(MpAvQueryInfoViewModel.Current.SortType);

        #endregion

        #region Properties

        //#region MpIQueryInfoProvider Implementation
        //public void RestoreQueryInfo() {
        //    SelectedSortTypeIdx = (int)MpAvQueryInfoViewModel.Current.SortType;
        //    IsSortDescending = MpAvQueryInfoViewModel.Current.IsDescending;
        //}

        //public void SetQueryInfo() {
        //    MpAvQueryInfoViewModel.Current.SortType = (MpContentSortType)SelectedSortTypeIdx;
        //    MpAvQueryInfoViewModel.Current.IsDescending = IsSortDescending;
        //    MpAvQueryInfoViewModel.Current.NotifyQueryChanged();
        //}

        //#endregion

        #region Layout
        #endregion

        #region State

        public int SelectedSortTypeIdx { get; set; } = (int)MpContentSortType.CopyDateTime;
        public MpContentSortType SelectedSortType {
            get => (MpContentSortType)SelectedSortTypeIdx;
            set => SelectedSortTypeIdx = (int)value;
        }


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
            //await Task.Delay(1);
            MpAvQueryInfoViewModel.Current.RegisterProvider(this);

            //ResetToDefault(true);
        }

        #endregion

        #region Private Methods       
        private void MpClipTileSortViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {                
                case nameof(SelectedSortType):
                    MpAvQueryInfoViewModel.Current.NotifyQueryChanged();
                    break;
            }
            
        }

        #endregion

        #region Commands
        #endregion
    }
}
