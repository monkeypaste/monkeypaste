using Avalonia.Threading;
using Cairo;
using MonoMac.AppKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileSortDirectionViewModel : 
        MpViewModelBase, 
        MpIExpandableViewModel,
        MpIQueryInfoValueProvider {
        #region Private Variables

        #endregion

        #region Statics
        private static MpAvClipTileSortDirectionViewModel _instance;
        public static MpAvClipTileSortDirectionViewModel Instance => _instance ?? (_instance = new MpAvClipTileSortDirectionViewModel());


        #endregion

        #region MpIExpandableViewModel Implementation

        public bool IsExpanded { get; set; }

        #endregion
        #region MpIQueryInfoProvider Implementation
        object MpIQueryInfoValueProvider.Source => this;
        string MpIQueryInfoValueProvider.SourcePropertyName => nameof(IsSortDescending);

        string MpIQueryInfoValueProvider.QueryValueName => nameof(MpIQueryInfo.IsDescending);

        #endregion

        #region Properties

        #region State
        public bool IsSortDescending { get; set; } = true;

        public bool IsSortDirOrFieldFocused { get; set; }

        #endregion
        #endregion

        #region Constructors
        public MpAvClipTileSortDirectionViewModel() :base(null) {
            PropertyChanged += MpAvClipTileSortDirectionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public void Init() {
            //await Task.Delay(1);
            MpPlatform.Services.Query.RegisterProvider(this);

            //ResetToDefault(true);
        }
        #endregion

        #region Private Methods

        private void MpAvClipTileSortDirectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSortDescending):
                    MpMessenger.SendGlobal(MpMessageType.QuerySortChanged);
                    MpPlatform.Services.Query.NotifyQueryChanged();
                    break;
                case nameof(IsSortDirOrFieldFocused):
                    if(IsSortDirOrFieldFocused) {
                        break;
                    }
                    if(!IsExpanded) {
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        // when field or dir looses focus wait a little to see if returns 
                        await Task.Delay(3000);
                        if(IsSortDirOrFieldFocused) {
                            return;
                        }
                        IsExpanded = false;
                    });
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand ClickCommand => new MpCommand(() => {
            if(IsExpanded) {
                IsSortDescending = !IsSortDescending;
            } else {
                IsExpanded = true;
            }
        });
        public ICommand DoubleClickCommand => new MpCommand(() => {
            if(IsExpanded) {
                IsExpanded = false;
            } else {
                IsSortDescending = !IsSortDescending;
            }
        });
        #endregion
    }

}
