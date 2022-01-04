using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAppViewModel : MpViewModelBase<MpAppCollectionViewModel> {
        #region Properties

        #region View Models

        public MpIconViewModel IconViewModel {
            get {
                if(App == null) {
                    return null;
                }
                return MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == IconId);
            }
        }

        #endregion

        #region MpISourceItemViewModel Implementation

        public MpIcon SourceIcon {
            get {
                if(IconViewModel == null) {
                    return null;
                }
                return IconViewModel.Icon;
            }
        } 

        public string SourcePath {
            get {
                if (App == null) {
                    return null;
                }
                return App.AppPath;
            }
        }

        public string SourceName {
            get {
                if (App == null) {
                    return null;
                }
                return App.AppName;
            }
        }

        public int RootId {
            get {
                if (App == null) {
                    return 0;
                }
                return App.Id;
            }
        }

        public bool IsUrl {
            get {
                if (App == null) {
                    return false;
                }
                return App.IsUrl;
            }
        }

        #endregion

        #region Appearance

        #endregion

        #region State

        public bool IsSelected { get; set; }

        public bool IsHovering { get; set; }

        //public bool IsNew {
        //    get {
        //        return App != null && AppId == 0;
        //    }
        //}

        #endregion

        #region Model

        public int AppId {
            get {
                if(App == null) {
                    return 0;
                }
                return App.Id;
            }
        }

        public int IconId {
            get {
                if (App == null) {
                    return 0;
                }
                return App.IconId;
            }
        }

        public string AppPath {
            get {
                if (App == null) {
                    return string.Empty;
                }
                return App.AppPath;
            }
        }

        public string AppName {
            get {
                if (App == null) {
                    return string.Empty;
                }
                return App.AppName;
            }
        }

        public bool IsRejected {
            get {
                if (App == null) {
                    return false;
                }
                return App.IsAppRejected;
            }
            set {
                if(App != null && App.IsAppRejected != value) {
                    App.IsAppRejected = value;
                    OnPropertyChanged(nameof(IsRejected));
                    OnPropertyChanged(nameof(App));
                    HasModelChanged = true;
                }
            }
        }

        public bool IsSubRejected => IsRejected;

        public MpApp App { get; set; }

        #endregion

        #endregion

        #region Public Methods
        public MpAppViewModel() : base(null) { }

        public MpAppViewModel(MpAppCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAppViewModel_PropertyChanged;
        }

        public async Task InitializeAsync(MpApp app) {
            IsBusy = true;

            App = app;

            OnPropertyChanged(nameof(IconId));
            await Task.Delay(1);

            IsBusy = false;
        }

        private void MpAppViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsRejected):
                    if(IsBusy) {
                        return;
                    }
                    Task.Run(async () => {
                        bool isRejected = await MpAppCollectionViewModel.Instance.UpdateRejection(this, IsRejected);
                        if(isRejected != App.IsAppRejected) {
                            await App.WriteToDatabaseAsync();
                        }
                    });
                    break;
            }
        }

        #endregion

        #region Commands

        #endregion
    }
}
