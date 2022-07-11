using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;
using SQLite;

namespace MpWpfApp {
    public class MpUrlViewModel : 
        MpViewModelBase<MpUrlCollectionViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpISourceItemViewModel {
        #region Properties

        #region View Models
        #endregion

        #region State

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        public bool IsHovering { get; set; }

        #endregion

        #region Model


        #region MpISourceItemViewModel Implementation

        public bool IsUser => false;
        public bool IsDll => false;

        public bool IsExe => false;

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


        public int IconId {
            get {
                if (Url == null) {
                    return 0;
                }
                return Url.IconId;
            }
        }

        #endregion

        public int UrlId {
            get {
                if(Url == null) {
                    return 0;
                }
                return Url.Id;
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

        public string UrlTitle {
            get {
                if (Url == null) {
                    return string.Empty;
                }
                return Url.UrlTitle;
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
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsRejected));
                    OnPropertyChanged(nameof(Url));
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
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsSubRejected));
                    OnPropertyChanged(nameof(Url));
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
        public async Task RejectUrlOrDomain(bool isDomain) {
            IsBusy = true;

            bool wasCanceled = false;

            List<MpCopyItem> clipsFromUrl = new List<MpCopyItem>();
            MessageBoxResult confirmExclusionResult = MessageBox.Show("Would you also like to remove all clips from '" + UrlPath + "'", "Remove associated clips?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
            if (confirmExclusionResult == MessageBoxResult.Yes) {
                IsBusy = true;
                if(isDomain) {
                    clipsFromUrl = await MpDataModelProvider.GetCopyItemsByUrlDomainAsync(UrlDomainPath);
                } else {
                    clipsFromUrl = await MpDataModelProvider.GetCopyItemsByUrlIdAsync(UrlId);
                }                
            } else if (confirmExclusionResult == MessageBoxResult.Cancel) {
                wasCanceled = true;
            }


            if (wasCanceled) {
                IsBusy = false;
                return;
            }

            await Task.WhenAll(clipsFromUrl.Select(x => x.DeleteFromDatabaseAsync()));

            IsBusy = false;
            return;
        }


        private void MpUrlViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsRejected):
                    if(IsRejected) {
                        MpHelpers.RunOnMainThread(async() => { await RejectUrlOrDomain(true); });
                    }
                    break;
                case nameof(IsSubRejected):
                    if (IsSubRejected) {
                        MpHelpers.RunOnMainThread(async () => { await RejectUrlOrDomain(false); });
                    }
                    break;
                case nameof(HasModelChanged):
                    Task.Run(Url.WriteToDatabaseAsync);
                    break;
            }
        }
        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpUrl url) {
                if (url.Id == UrlId) {
                    Url = url;
                } else if(url.UrlDomainPath.ToLower() == UrlDomainPath.ToLower()) {
                    if(url.IsRejected && !IsRejected) {
                        //when this url's domain is rejected this url needs to know without notifying user
                        SupressPropertyChangedNotification = true;
                        IsRejected = true;
                        SupressPropertyChangedNotification = false;
                        HasModelChanged = true;
                    } else if(!url.IsRejected && IsRejected) {
                        IsRejected = false;
                    }
                    
                }

            }
        }

        #endregion

        #endregion

        #region Commands

        #endregion
    }
}
