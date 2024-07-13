using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvUrlCollectionViewModel :
        MpAvSelectorViewModelBase<object, MpAvUrlViewModel> {
        #region Statics

        private static MpAvUrlCollectionViewModel _instance;
        public static MpAvUrlCollectionViewModel Instance => _instance ?? (_instance = new MpAvUrlCollectionViewModel());

        #endregion

        #region Properties

        #region View Models
        public IEnumerable<MpAvUrlViewModel> FilteredItems =>
            Items
            .Where(x => (x as MpIFilterMatch).IsFilterMatch(MpAvSettingsViewModel.Instance.FilterText))
            .OrderBy(x => x.UrlDomainPath)
            .ThenBy(x => x.UrlTitle);

        public IEnumerable<MpAvUrlViewModel> RejectedDomains =>
            FilteredItems
            .Where(x => x.IsDomainRejected).DistinctBy(x => x.UrlDomainPath);
        public IEnumerable<MpAvUrlViewModel> RejectedPages =>
            FilteredItems
            .Where(x => x.IsUrlRejected);

        #endregion

        #region State

        public bool CanRejectUrls =>
            !MpAvThemeViewModel.Instance.IsMobileOrWindowed;

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors


        public MpAvUrlCollectionViewModel() : base(null) {
            Items.CollectionChanged += Items_CollectionChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods
        public async Task InitAsync() {
            IsBusy = true;
            while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                // wait for icons to load since url vm depends on icon vm
                await Task.Delay(100);
            }

            var urll = await MpDataModelProvider.GetItemsAsync<MpUrl>();
            Items.Clear();
            foreach (var url in urll) {
                var uvm = await CreateUrlViewModel(url);
                Items.Add(uvm);
            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
            //await Task.WhenAll(Items.Select(x => UpdateRejection(x)));
            OnPropertyChanged(nameof(Items));

            ValidateUrlViewModels();
            IsBusy = false;
        }
        public async Task<MpAvUrlViewModel> CreateUrlViewModel(MpUrl url) {
            var uvm = new MpAvUrlViewModel(this);
            await uvm.InitializeAsync(url);
            return uvm;
        }

        public bool IsRejected(string domain) {
            return Items.FirstOrDefault(x => x.UrlDomainPath.ToLower() == domain.ToLower() && x.IsDomainRejected) != null;
        }

        public bool IsUrlRejected(string url) {
            return IsUrlDomainRejected(url) || 
                Items.Where(x=>x.IsUrlRejected).FirstOrDefault(x => x.UrlPath.ToLower() == url.ToLower()) != null;
        }
        
        public bool IsUrlDomainRejected(string url) {
            return Items.Where(x=>x.IsDomainRejected).FirstOrDefault(x => x.UrlDomainPath.ToLower() == MpUrlHelpers.GetUrlDomain(url.ToLower())) != null;
        }

        public void RefreshItemSources() {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(FilteredItems));
            OnPropertyChanged(nameof(RejectedDomains));
            OnPropertyChanged(nameof(RejectedPages));
        }

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpUrl url) {
                Dispatcher.UIThread.Post(async () => {
                    IsBusy = true;
                    var uvm = await CreateUrlViewModel(url);
                    Items.Add(uvm);
                    IsBusy = false;
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpUrl url) {
                var uvm = Items.FirstOrDefault(x => x.UrlId == url.Id);
                if (uvm != null) {
                    Items.Remove(uvm);
                }
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            ValidateUrlViewModels();
            RefreshItemSources();
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsFilterTextChanged:
                    OnPropertyChanged(nameof(FilteredItems));
                    break;
            }
        }
        private void ValidateUrlViewModels() {
            //var dups = Items.Where(x => Items.Any(y => y != x && x.IsValueEqual(y)));
            //if (dups.LastOrDefault() is { } dup_uvm) {
                //MpDebug.Break($"Url collection error, dups detected (removing newest ones!): {string.Join(Environment.NewLine,dups.Select(x=>x.UrlPath).Distinct())}",silent: true);
                //Dispatcher.UIThread.Post(async () => {
                //    await dup_uvm.Url.DeleteFromDatabaseAsync();
                //    Items.Remove(dup_uvm);
                //});
            //}
        }

        #endregion

        #region Commands

        public MpIAsyncCommand<object> AddRejectUrlCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not string rejectType) {
                    return;
                }
                string url_or_domain_path = null;

                // show url entry box until valid result or cancel
                while (url_or_domain_path == null) {
                    url_or_domain_path = await Mp.Services.PlatformMessageBox.ShowTextBoxMessageBoxAsync(
                        title: UiStrings.AddUrlNtfTitle,
                        message: UiStrings.AddUrlNtfText,
                        iconResourceObj: "WebImage");
                    if (url_or_domain_path == null) {
                        // canceled, break
                        break;
                    }
                    url_or_domain_path = MpUrlHelpers.GetFullyFormattedUrl(url_or_domain_path);

                    if (!Uri.IsWellFormedUriString(url_or_domain_path, UriKind.Absolute)) {
                        var invalid_result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                            title: UiStrings.CommonErrorLabel,
                            message: UiStrings.AddUrlNtfErrorText);
                        url_or_domain_path = null;
                        if (invalid_result) {
                            // try again
                        } else {
                            // cancel
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(url_or_domain_path)) {
                    return;
                }
                bool is_domain_reject = rejectType == "domain";
                url_or_domain_path = is_domain_reject ? MpUrlHelpers.GetUrlDomain(url_or_domain_path) : url_or_domain_path;

                MpAvUrlViewModel uvm = is_domain_reject ?
                    Items.FirstOrDefault(x => x.UrlDomainPath.ToLowerInvariant() == url_or_domain_path.ToLowerInvariant()) :
                    Items.FirstOrDefault(x => x.UrlPath.ToLowerInvariant() == url_or_domain_path.ToLowerInvariant());

                if (uvm == null) {
                    // if url or domain not found create new entry and wait for it to be added
                    var url = await Mp.Services.UrlBuilder.CreateAsync(url_or_domain_path);
                    while (uvm == null) {
                        uvm = Items.FirstOrDefault(x => x.UrlId == url.Id);
                        await Task.Delay(300);
                    }
                } 
                if(uvm == null) {
                    return;
                }
                bool is_already_rejected = is_domain_reject ? uvm.IsDomainRejected : uvm.IsUrlRejected;
                if(is_already_rejected) {
                    // warn that page/domain already rejected
                    string warning_msg = is_domain_reject ?
                        UiStrings.InteropDomainAlreadyRejectedText.Format(url_or_domain_path) :
                        UiStrings.InteropPageAlreadyRejectedText;

                    await Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                            title: UiStrings.CommonDuplicateLabel,
                            message: warning_msg,
                            iconResourceObj: "WarningImage");
                } else {
                    // reject page/domain
                    await uvm.RejectCommand.ExecuteAsync(is_domain_reject ? "domain" : "page");
                }

                SelectedItem = uvm;
            },
            (args) => {
                return CanRejectUrls;
            });
        #endregion
    }
}
