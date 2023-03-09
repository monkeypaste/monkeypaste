using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvUrlViewModel :
        MpViewModelBase<MpAvUrlCollectionViewModel>,
        MpIIsValueEqual<MpAvUrlViewModel>,
        MpIHoverableViewModel,
        MpIFilterMatch,
        MpISelectableViewModel
        //MpISourceItemViewModel
        {


        #region Interfaces

        #region MpIFilterMatch Implementation
        bool MpIFilterMatch.IsMatch(string filter) {
            if (string.IsNullOrEmpty(filter)) {
                return true;
            }
            return
                UrlTitle.ToLower().Contains(filter.ToLower()) ||
                UrlPath.ToLower().Contains(filter.ToLower()) ||
                UrlDomainPath.ToLower().Contains(filter.ToLower());
        }

        #endregion

        #region MpIIsValueEqual Implementation

        public bool IsValueEqual(MpAvUrlViewModel ouvm) {
            if (ouvm == null) {
                return false;
            }
            return
                UrlPath.ToLower() == ouvm.UrlPath.ToLower();
        }
        #endregion

        #endregion
        #region Properties

        #region View Models
        #endregion

        #region State

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        public bool IsHovering { get; set; }

        #endregion

        #region Model
        public int IconId {
            get {
                if (Url == null) {
                    return 0;
                }
                return Url.IconId;
            }
        }
        public int UrlId {
            get {
                if (Url == null) {
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
                return Url.IsDomainRejected;
            }
            set {
                if (Url != null && Url.IsDomainRejected != value) {
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

        public MpAvUrlViewModel() : base(null) { }

        public MpAvUrlViewModel(MpAvUrlCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpUrlViewModel_PropertyChanged;
        }

        public async Task InitializeAsync(MpUrl url) {
            IsBusy = true;
            Url = url;

            OnPropertyChanged(nameof(IconId));
            await Task.Delay(1);

            IsBusy = false;
        }
        public async Task<bool> VerifyRejectAsync(bool isDomain) {
            bool rejectContent = false;

            IEnumerable<MpCopyItem> to_delete_cil = null;
            if (isDomain) {
                var domain_uvml = Parent.Items.Where(x => x.UrlDomainPath.ToLower() == UrlDomainPath.ToLower());
                var all_results = await Task.WhenAll(domain_uvml.Select(x => MpDataModelProvider.GetCopyItemsBySourceTypeAndIdAsync(MpTransactionSourceType.Url, x.UrlId)));
                to_delete_cil = all_results.SelectMany(x => x);
            } else {
                to_delete_cil = await MpDataModelProvider.GetCopyItemsBySourceTypeAndIdAsync(MpTransactionSourceType.Url, UrlId);
            }

            if (to_delete_cil != null && to_delete_cil.Any()) {
                var result = await Mp.Services.NativeMessageBox.ShowYesNoCancelMessageBoxAsync(
                    title: $"Remove associated clips?",
                    message: $"Would you also like to remove all clips from '{(isDomain ? UrlDomainPath : UrlPath)}'",
                    iconResourceObj: IconId);
                if (result.IsNull()) {
                    // flag as cancel so cmd will untoggle reject
                    return false;
                }
                rejectContent = result.IsTrue();
            }
            if (!rejectContent) {
                return true;
            }

            IsBusy = true;
            await Task.WhenAll(to_delete_cil.Select(x => x.DeleteFromDatabaseAsync()));
            IsBusy = false;
            return true;
        }


        private void MpUrlViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                //case nameof(IsRejected):
                //    if (IsRejected) {
                //        Dispatcher.UIThread.Post(async () => { await VerifyRejectAsync(true); });
                //    }
                //    break;
                //case nameof(IsSubRejected):
                //    if (IsSubRejected) {
                //        Dispatcher.UIThread.Post(async () => { await VerifyRejectAsync(false); });
                //    }
                //    break;
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
                } else if (url.UrlDomainPath.ToLower() == UrlDomainPath.ToLower()) {
                    if (url.IsDomainRejected && !IsRejected) {
                        //when this url's domain is rejected this url needs to know without notifying user
                        SupressPropertyChangedNotification = true;
                        IsRejected = true;
                        SupressPropertyChangedNotification = false;
                        HasModelChanged = true;
                    } else if (!url.IsDomainRejected && IsRejected) {
                        IsRejected = false;
                    }

                }

            }
        }

        #endregion

        #endregion

        #region Commands
        public ICommand ToggleIsRejectedCommand => new MpAsyncCommand(
            async () => {
                IsSubRejected = !IsSubRejected;
                if (IsSubRejected) {
                    bool was_confirmed = await VerifyRejectAsync(false);
                    if (!was_confirmed) {
                        // canceled from delete content msgbox
                        IsSubRejected = false;
                    }
                }
            });

        public ICommand ToggleIsDomainRejectedCommand => new MpAsyncCommand(
            async () => {
                IsRejected = !IsRejected;
                if (IsRejected) {
                    bool was_confirmed = await VerifyRejectAsync(true);
                    if (!was_confirmed) {
                        // canceled from delete content msgbox
                        IsRejected = false;
                    }
                }
            });


        #endregion
    }
}
