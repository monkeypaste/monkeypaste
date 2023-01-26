using Cairo;
using MonoMac.AppKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileSortDirectionViewModel : 
        MpViewModelBase, 
        MpIQueryInfoValueProvider {
        #region Private Variables

        #endregion

        #region Statics
        private static MpAvClipTileSortDirectionViewModel _instance;
        public static MpAvClipTileSortDirectionViewModel Instance => _instance ?? (_instance = new MpAvClipTileSortDirectionViewModel());


        #endregion

        #region MpIQueryInfoProvider Implementation
        async Task<IEnumerable<MpSearchCriteriaItem>> MpIQueryInfoValueProvider.SaveAsCriteriaItemsAsync(int tagId, int sortIdx) {
            return null;
        }
        object MpIQueryInfoValueProvider.Source => this;
        string MpIQueryInfoValueProvider.SourcePropertyName => nameof(IsSortDescending);

        string MpIQueryInfoValueProvider.QueryValueName => nameof(MpPlatform.Services.QueryInfo.IsDescending);

        #endregion
        #region Properties

        #region State

        public bool IsSortDescending { get; set; } = true;

        #endregion
        #endregion

        #region Constructors
        public MpAvClipTileSortDirectionViewModel() :base(null) {
            PropertyChanged += MpAvClipTileSortDirectionViewModel_PropertyChanged;
        }

        private void MpAvClipTileSortDirectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSortDescending):
                    MpPlatform.Services.QueryInfo.NotifyQueryChanged();
                    break;
            }
        }
        #endregion

        #region Public Methods
        public void Init() {
            //await Task.Delay(1);
            MpPlatform.Services.QueryInfo.RegisterProvider(this);

            //ResetToDefault(true);
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }

}
