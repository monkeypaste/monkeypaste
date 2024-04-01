using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvUrlViewModel :
        MpAvViewModelBase<MpAvUrlCollectionViewModel>,
        MpIIsValueEqual<MpAvUrlViewModel>,
        MpIHoverableViewModel,
        MpIFilterMatch,
        MpISelectableViewModel
        //MpISourceItemViewModel
        {
        #region Interfaces

        #region MpIFilterMatch Implementation
        bool MpIFilterMatch.IsFilterMatch(string filter) {
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

        #region Appearance
        public string DisableUnrejectPageToolTipText =>
            UiStrings.InteropDisabledUnrejectPageToolTip.Format(UrlDomainPath);
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

        public bool IsDomainRejected {
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
                    OnPropertyChanged(nameof(IsDomainRejected));
                }
            }
        }

        public bool IsUrlRejected {
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
                    OnPropertyChanged(nameof(IsUrlRejected));
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

            if (IconId == 0 &&
                !Mp.Services.StartupState.IsPlatformLoaded) {
                _ = Task.Run(async () => {
                    // NOTE run in bg not to slow down startup more...
                    // very edge case here of copying html on a page that was loaded
                    // with internet than subsequently went offline when added here
                    // so try it again on startup 
                    var url_props = await MpUrlHelpers.DiscoverUrlPropertiesAsync(UrlPath);
                    if (url_props != null &&
                        !string.IsNullOrEmpty(url_props.FavIconBase64)) {
                        var icon = await Mp.Services.IconBuilder.CreateAsync(url_props.FavIconBase64);
                        if (icon != null) {
                            // update model directly since multiple properties
                            Url.IconId = icon.Id;
                            Url.UrlTitle = url_props.Title;
                            await Url.WriteToDatabaseAsync();
                            Dispatcher.UIThread.Post(() => {
                                OnPropertyChanged(nameof(UrlTitle));
                                OnPropertyChanged(nameof(IconId));
                            });

                        }
                    }
                });
            }
            OnPropertyChanged(nameof(IconId));
            await Task.Delay(1);

            IsBusy = false;
        }
        public async Task<bool> VerifyAndApplyRejectToContentAsync(bool isDomain) {
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
                var result = await Mp.Services.PlatformMessageBox.ShowYesNoCancelMessageBoxAsync(
                    title: UiStrings.NtfRejectRemoveClipsTitle,
                    message: string.Format(UiStrings.NtfRejectRemoveClipsBody, to_delete_cil.Count(), (isDomain ? UrlDomainPath : UrlPath)),
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
                case nameof(IsDomainRejected):
                    OnPropertyChanged(nameof(IsUrlRejected));
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        Task.Run(async () => {
                            await Url.WriteToDatabaseAsync();
                            Dispatcher.UIThread.Post(() => {
                                HasModelChanged = false;
                            });
                        });
                    }
                    break;
            }
        }
        #endregion

        #region Protected Methods

        #region Db Event Handlers
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpUrl url) {
                //if (url.Id == UrlId) {
                //    Url = url;
                //} 
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private async Task ApplyRejectStateToOtherDomainUrlsAsync(bool? urlReject, bool? domainReject, bool isSource = true) {
            // NOTE since this may change multiple properties it needs to do it 
            // in the model or weird updates happen

            if (urlReject.HasValue) {
                Url.IsUrlRejected = urlReject.Value;
            }
            if (domainReject.HasValue) {
                Url.IsDomainRejected = domainReject.Value;
            }
            await Url.WriteToDatabaseAsync();
            OnPropertyChanged(nameof(IsUrlRejected));
            OnPropertyChanged(nameof(IsDomainRejected));
            if (!isSource || !domainReject.HasValue) {
                // only propagate domain changes from source call
                if(isSource && Parent != null) {
                    Parent.RefreshItemSources();
                }
                return;
            }

            var other_domain_url_vml =
                MpAvUrlCollectionViewModel.Instance.Items
                    .Where(x => x.UrlId != UrlId && x.UrlDomainPath.ToLower() == UrlDomainPath.ToLower());

            await Task.WhenAll(
                other_domain_url_vml.Select(x =>
                    x.ApplyRejectStateToOtherDomainUrlsAsync(
                        urlReject: null,  // ignore implicit change if domain is not rejected
                        domainReject: IsDomainRejected,
                        isSource: false)));

            if (isSource && Parent != null) {
                Parent.RefreshItemSources();
            }
        }

        #endregion

        #region Commands

        public MpIAsyncCommand<object> RejectCommand => new MpAsyncCommand<object>(
            async (args) => {
                if(args is not string rejectType) {
                    return;
                }
                bool is_domain = rejectType == "domain";
                bool was_confirmed = await VerifyAndApplyRejectToContentAsync(is_domain);
                if (!was_confirmed) {
                    // canceled 
                    return;
                }
                bool? block_url = is_domain ? null : true;
                bool? block_domain = is_domain ? true : null;
                await ApplyRejectStateToOtherDomainUrlsAsync(block_url, block_domain);

            }, (args) => {
                if(args is not string rejectType) {
                    return false;
                }
                if(rejectType == "domain") {
                    return !IsDomainRejected;
                }
                return !IsUrlRejected;
            });
        
        public MpIAsyncCommand<object> UnrejectCommand => new MpAsyncCommand<object>(
            async (args) => {
                if(args is not string rejectType) {
                    return;
                }
                bool is_domain = rejectType == "domain";
                bool? unblock_url = is_domain ? null : false;
                bool? unblock_domain = is_domain ? false : null;
                await ApplyRejectStateToOtherDomainUrlsAsync(unblock_url, unblock_domain);

            }, (args) => {
                if(args is not string rejectType) {
                    return false;
                }
                if(rejectType == "domain") {
                    return IsDomainRejected;
                }
                return IsUrlRejected && !IsDomainRejected;
            });


        public MpIAsyncCommand ToggleIsUrlRejectedCommand => new MpAsyncCommand(
            async () => {
                if(IsUrlRejected) {
                    await UnrejectCommand.ExecuteAsync("page");
                } else {
                    await RejectCommand.ExecuteAsync("page");
                }
            });

        public MpIAsyncCommand ToggleIsDomainRejectedCommand => new MpAsyncCommand(
            async () => {
                if (IsUrlRejected) {
                    await UnrejectCommand.ExecuteAsync("domain");
                } else {
                    await RejectCommand.ExecuteAsync("domain");
                }
            });

        #endregion
    }
}
