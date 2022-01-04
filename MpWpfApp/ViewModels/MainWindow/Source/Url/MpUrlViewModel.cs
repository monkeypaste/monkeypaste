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
    public class MpUrlViewModel : MpViewModelBase<MpUrlCollectionViewModel> {
        #region Properties

        #region View Models

        public MpIconViewModel IconViewModel {
            get {
                if (Url == null) {
                    return null;
                }
                return MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == IconId);
            }
        }

        #endregion

        #region MpISourceItemViewModel Implementation

        public MpIcon SourceIcon {
            get {
                if (IconViewModel == null) {
                    return null;
                }
                return IconViewModel.Icon;
            }
        }

        public string SourcePath {
            get {
                if (Url == null) {
                    return null;
                }
                return Url.UrlPath;
            }
        }

        public string SourceName {
            get {
                if (Url == null) {
                    return null;
                }
                return Url.UrlPath;
            }
        }

        public int RootId {
            get {
                if (Url == null) {
                    return 0;
                }
                return Url.Id;
            }
        }

        public bool IsUrl {
            get {
                if (Url == null) {
                    return false;
                }
                return Url.IsUrl;
            }
        }

        #endregion

        #region State

        public bool IsSelected { get; set; }

        public bool IsHovering { get; set; }

        //public bool IsNew {
        //    get {
        //        return Url != null && UrlId == 0;
        //    }
        //}
        #endregion

        #region Model

        public int UrlId {
            get {
                if(Url == null) {
                    return 0;
                }
                return Url.Id;
            }
        }


        public int IconId {
            get {
                if (Url == null) {
                    return 0;
                }
                return Url.IconId;
            }
        }

        public string UrlPath {
            get {
                if (Url == null) {
                    return string.Empty;
                }
                return Url.UrlPath;
            }
        }

        public string UrlDomainPath {
            get {
                if (Url == null) {
                    return string.Empty;
                }
                return Url.UrlDomainPath;
            }
        }

        public bool IsRejected {
            get {
                if (Url == null) {
                    return false;
                }
                return Url.IsRejected;
            }
            set {
                if(Url != null && Url.IsRejected != value) {
                    Url.IsDomainRejected = value;
                    OnPropertyChanged(nameof(IsRejected));
                    OnPropertyChanged(nameof(Url));
                    HasModelChanged = true;
                }
            }
        }

        public bool IsSubRejected {
            get {
                if (Url == null) {
                    return false;
                }
                return Url.IsUrlRejected;
            }
            set {
                if (Url != null && Url.IsUrlRejected != value) {
                    Url.IsUrlRejected = value;
                    OnPropertyChanged(nameof(IsSubRejected));
                    OnPropertyChanged(nameof(Url));
                    HasModelChanged = true;
                }
            }
        }


        public MpUrl Url { get; set; }

        #endregion

        #endregion

        #region Public Methods

        public MpUrlViewModel() : base(null) { }

        public MpUrlViewModel(MpUrlCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpUrlViewModel_PropertyChanged;
        }
        
        public async Task InitializeAsync(MpUrl url) {
            IsBusy = true;
            Url = url;

            OnPropertyChanged(nameof(IconId));
            await Task.Delay(1);

            IsBusy = false;
        }

        private void MpUrlViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsRejected):
                    if(IsBusy || Parent.IsBusy) {
                        return;
                    }
                    Task.Run(async () => {
                        bool isRejected = await MpUrlCollectionViewModel.Instance.UpdateDomainRejection(this, IsRejected);
                        if(isRejected != Url.IsRejected) {
                            await Url.WriteToDatabaseAsync();
                        }
                    });
                    break;
                case nameof(IsSubRejected):
                    if (IsBusy || Parent.IsBusy) {
                        return;
                    }
                    Task.Run(async () => {
                        bool isRejected = await MpUrlCollectionViewModel.Instance.UpdateUrlRejection(this, IsSubRejected);
                        if (isRejected != Url.IsUrlRejected) {
                            await Url.WriteToDatabaseAsync();
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
