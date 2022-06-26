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
using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpUrlCollectionViewModel : 
        MpSelectorViewModelBase<object,MpUrlViewModel>, 
        MpIAsyncSingletonViewModel<MpUrlCollectionViewModel> {
        #region Properties

        #region View Models

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors

        private static MpUrlCollectionViewModel _instance;
        public static MpUrlCollectionViewModel Instance => _instance ?? (_instance = new MpUrlCollectionViewModel());


        public MpUrlCollectionViewModel() : base(null) {
        }

        public async Task InitAsync() {
            IsBusy = true;
            while (MpIconCollectionViewModel.Instance.IsAnyBusy) {
                // wait for icons to load since url vm depends on icon vm
                await Task.Delay(100);
            }

            var urll = await MpDb.GetItemsAsync<MpUrl>();
            Items.Clear();
            foreach (var url in urll) {
                var uvm = await CreateUrlViewModel(url);
                Items.Add(uvm);
            }

            while(Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }
            //await Task.WhenAll(Items.Select(x => UpdateRejection(x)));
            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public async Task<MpUrlViewModel> CreateUrlViewModel(MpUrl url) {
            var uvm = new MpUrlViewModel(this);
            await uvm.InitializeAsync(url);
            return uvm;
        }

        public bool IsRejected(string domain) {
            return Items.FirstOrDefault(x => x.UrlDomainPath.ToLower() == domain.ToLower() && x.IsRejected) != null;
        }

        public bool IsUrlRejected(string url) {
            return Items.FirstOrDefault(x => x.UrlPath.ToLower() == url.ToLower() && x.IsSubRejected) != null;
        }

        public void Remove(MpUrlViewModel avm) {
            Items.Remove(avm);
        }

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpUrl url) {
                MpHelpers.RunOnMainThread(async () => {
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

        #region Commands

        public ICommand AddUrlCommand => new RelayCommand(
            async () => {
                string UrlPath = MpTextBoxMessageBox.ShowCustomMessageBox("");

                if(string.IsNullOrEmpty(UrlPath)) {
                    return;
                }

                MpUrl url = null;
                var uvm = Items.FirstOrDefault(x => x.UrlPath.ToLower() == UrlPath.ToLower());
                if (uvm == null) {
                    string iconBase64Str = await MpUrlHelpers.GetUrlFavIconAsync(UrlPath);
                    string title = await MpUrlHelpers.GetUrlTitle(UrlPath);
                    var icon = await MpIcon.Create(iconBase64Str);
                    url = await MpUrl.Create(UrlPath, title);
                    uvm = await CreateUrlViewModel(url);
                }
                SelectedItem = uvm;
            });

        
        #endregion
    }
}
