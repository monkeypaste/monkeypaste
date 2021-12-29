using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
using System.IO;

namespace MpWpfApp {
    public class MpUrlCollectionViewModel : MpSingletonViewModel<MpUrlCollectionViewModel> {
        #region Properties

        #region View Models
                
        public ObservableCollection<MpUrlViewModel> UrlViewModels { get; set; } = new ObservableCollection<MpUrlViewModel>();

        public MpUrlViewModel SelectedUrlViewModel {
            get => UrlViewModels.FirstOrDefault(x => x.IsSelected);
            set {
                UrlViewModels.ForEach(x => x.IsSelected = false);
                if(value != null) {
                    UrlViewModels.ForEach(x => x.IsSelected = x.UrlId == value.UrlId);
                }
            }
        }

        #endregion
        #endregion

        #region Constructors

        public async Task Init() {
            await Refresh();
        }
        public MpUrlCollectionViewModel() : base() { }

        #endregion

        #region Public Methods

        public MpUrlViewModel GetUrlViewModelByUrlId(int UrlId) {
            foreach(var avm in UrlViewModels.Where(x => x.UrlId == UrlId)) {
                return avm;
            }
            return null;
        }

        public MpUrlViewModel GetUrlViewModelByUrlPath(string urlPath) {
            foreach (var avm in UrlViewModels.Where(x => x.UrlPath.ToLower() == urlPath.ToLower())) {
                return avm;
            }
            return null;
        }

        public MpUrlViewModel GetUrlViewModelByDomainPath(string domainPath) {
            foreach (var avm in UrlViewModels.Where(x => x.UrlDomainPath.ToLower() == domainPath.ToLower())) {
                return avm;
            }
            return null;
        }

        public async Task<bool> UpdateUrlRejection(MpUrlViewModel url, bool rejectUrl) {
            if (GetUrlViewModelByUrlPath(url.UrlPath) != null) {
                bool wasCanceled = false;
                if (rejectUrl) {
                    List<MpCopyItem> clipsFromUrl = new List<MpCopyItem>();
                    MessageBoxResult confirmExclusionResult = MessageBox.Show("Would you also like to remove all clips from '" + url.UrlPath + "'", "Remove associated clips?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                    if (confirmExclusionResult == MessageBoxResult.Yes) {
                        IsBusy = true;
                        clipsFromUrl = await MpDataModelProvider.Instance.GetCopyItemsByUrlId(url.UrlId);
                    } else if(confirmExclusionResult == MessageBoxResult.Cancel) {
                        wasCanceled = true;
                    }

                    await Task.WhenAll(clipsFromUrl.Select(x => x.DeleteFromDatabaseAsync()));
                }
                if (wasCanceled) {
                    IsBusy = false;
                    return url.IsUrlRejected;
                }
                int UrlIdx = UrlViewModels.IndexOf(url);
                UrlViewModels[UrlIdx].Url.IsUrlRejected = rejectUrl;
                await UrlViewModels[UrlIdx].Url.WriteToDatabaseAsync();

            } else {
                MonkeyPaste.MpConsole.WriteLine("UrlCollection.UpdateRejection error, Url: " + url.UrlDomainPath + " is not in collection");
            }

            IsBusy = false;
            return rejectUrl;
        }

        public async Task<bool> UpdateDomainRejection(MpUrlViewModel url, bool rejectDomain) {
            if (GetUrlViewModelByUrlPath(url.UrlPath) != null) {
                bool wasCanceled = false;
                if (rejectDomain) {
                    List<MpCopyItem> clipsFromUrl = new List<MpCopyItem>();
                    MessageBoxResult confirmExclusionResult = MessageBox.Show("Would you also like to remove all clips from '" + url.UrlDomainPath + "'", "Remove associated clips?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                    if (confirmExclusionResult == MessageBoxResult.Yes) {
                        IsBusy = true;
                        clipsFromUrl = await MpDataModelProvider.Instance.GetCopyItemsByUrlDomain(url.UrlDomainPath);
                    } else if(confirmExclusionResult == MessageBoxResult.Cancel) {
                            wasCanceled = true;
                    } 

                    await Task.WhenAll(clipsFromUrl.Select(x => x.DeleteFromDatabaseAsync()));



                }
                if (wasCanceled) {
                    IsBusy = false;
                    return url.IsDomainRejected;
                }
                int UrlIdx = UrlViewModels.IndexOf(url);
                UrlViewModels[UrlIdx].Url.IsDomainRejected = rejectDomain;
                await UrlViewModels[UrlIdx].Url.WriteToDatabaseAsync();

            } else {
                MonkeyPaste.MpConsole.WriteLine("UrlCollection.UpdateRejection error, Url: " + url.UrlDomainPath + " is not in collection");
            }
            IsBusy = true;
            UrlViewModels.Where(x => x.UrlDomainPath.ToLower() == url.UrlDomainPath.ToLower()).ForEach(x => x.IsDomainRejected = rejectDomain);
            UrlViewModels.Where(x => x.UrlDomainPath.ToLower() == url.UrlDomainPath.ToLower()).ForEach(x => x.IsUrlRejected = rejectDomain);
            IsBusy = false;
            return rejectDomain;
        }

        public async Task AddUrl(MpUrlViewModel avm) {
            var dupCheck = GetUrlViewModelByUrlPath(avm.UrlPath);
            if (dupCheck == null) {
                await avm.Url.WriteToDatabaseAsync();
                UrlViewModels.Add(avm);
            }

            await UpdateUrlRejection(avm, avm.IsDomainRejected);

            await Refresh();
        }

        public bool IsDomainRejected(string domain) {
            var avm = GetUrlViewModelByUrlPath(domain);
            if(avm == null) {
                return false;
            }
            return avm.IsDomainRejected;
        }

        public bool IsUrlRejected(string url) {
            var avm = GetUrlViewModelByUrlPath(url);
            if (avm == null) {
                return false;
            }
            return avm.IsDomainRejected;
        }

        public void Remove(MpUrlViewModel avm) {
            UrlViewModels.Remove(avm);
        }

        public async Task Refresh() {
            var Urll = await MpDb.Instance.GetItemsAsync<MpUrl>();
            UrlViewModels.Clear();
            foreach (var Url in Urll) {
                UrlViewModels.Add(new MpUrlViewModel(this,Url));
            }
            OnPropertyChanged(nameof(UrlViewModels));
        }
        #endregion

        #region Commands

        public ICommand AddUrlCommand => new RelayCommand(
            async () => {
                string UrlPath = MpTextBoxMessageBox.ShowCustomMessageBox("");

                if(string.IsNullOrEmpty(UrlPath)) {
                    return;
                }

                MpUrl Url = null;
                var avm = UrlViewModels.FirstOrDefault(x => x.UrlPath.ToLower() == UrlPath.ToLower());
                if (avm == null) {
                    var iconBmpSrc = MpHelpers.Instance.GetUrlFavicon(UrlPath);
                    string title = await MpHelpers.Instance.GetUrlTitle(UrlPath);
                    var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                    Url = await MpUrl.Create(UrlPath, title, MpPreferences.Instance.ThisAppSource.App);
                    avm = new MpUrlViewModel(this, Url);
                    await AddUrl(avm);
                }

                SelectedUrlViewModel = avm;
            });

        
        #endregion
    }
}
