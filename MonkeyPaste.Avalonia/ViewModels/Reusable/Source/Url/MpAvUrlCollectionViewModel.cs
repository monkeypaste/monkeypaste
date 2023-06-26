using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            .Where(x => (x as MpIFilterMatch).IsMatch(MpAvSettingsViewModel.Instance.FilterText));

        #endregion

        #region State

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
            return Items.FirstOrDefault(x => x.UrlDomainPath.ToLower() == domain.ToLower() && x.IsRejected) != null;
        }

        public bool IsUrlRejected(string url) {
            return Items.FirstOrDefault(x => x.UrlPath.ToLower() == url.ToLower() && x.IsSubRejected) != null;
        }

        public void Remove(MpAvUrlViewModel avm) {
            Items.Remove(avm);
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
                    OnPropertyChanged(nameof(Items));
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
            OnPropertyChanged(nameof(Items));
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsFilterTextChanged:
                    OnPropertyChanged(nameof(FilteredItems));
                    break;
            }
        }
        private void ValidateUrlViewModels() {
            var dups = Items.Where(x => Items.Any(y => y != x && x.IsValueEqual(y)));
            if (dups.Any()) {
                // dup app view models, check db to see if dup app model
                MpDebug.Break();
            }

        }

        #endregion

        #region Commands

        public ICommand AddUrlCommand => new MpAsyncCommand(
            async () => {
                string urlPath = null;

                while (urlPath == null) {
                    urlPath = await Mp.Services.PlatformMessageBox.ShowTextBoxMessageBoxAsync(
                        title: "Add url",
                        message: "Enter full url:",
                        iconResourceObj: "WebImage");
                    if (urlPath == null) {
                        // canceled, break
                        break;
                    }
                    if (!Uri.IsWellFormedUriString(urlPath, UriKind.Absolute)) {
                        var invalid_result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                            title: "Error",
                            message: "Url must be in valid format");
                        urlPath = null;
                        if (invalid_result) {
                            // try again
                        } else {
                            // cancel
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(urlPath)) {
                    return;
                }

                var uvm = Items.FirstOrDefault(x => x.UrlPath.ToLower() == urlPath.ToLower());
                if (uvm == null) {
                    var url = await Mp.Services.UrlBuilder.CreateAsync(urlPath);
                    while (uvm == null) {
                        uvm = Items.FirstOrDefault(x => x.UrlPath.ToLower() == urlPath.ToLower());
                        await Task.Delay(300);
                    }
                } else {
                    await Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                            title: "Duplicate",
                            message: $"Url already exists: '{urlPath}'",
                            iconResourceObj: "WarningImage");
                }

                SelectedItem = uvm;
            });
        #endregion
    }
}
