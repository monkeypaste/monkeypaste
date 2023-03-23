using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpContactCollectionViewModel : MpSelectorViewModelBase<object, MpContactViewModel> {

        #region Statics

        private static MpContactCollectionViewModel _instance;
        public static MpContactCollectionViewModel Instance => _instance ?? (_instance = new MpContactCollectionViewModel());

        #endregion

        #region Properties

        #region State

        public bool IsLoaded => Items != null && Items.Count > 0;

        #endregion
        #endregion

        #region Public Methods

        public async Task InitAsync() {
            IsBusy = true;
            //var cl = await GetContacts();
            await Task.Delay(3000);

            //contact_cmb.ItemsSource = await MpMasterTemplateModelCollectionViewModel.Instance.GetContacts();
            var cl = new List<MpContact>() {
                        new MpContact() {
                            FirstName = "Mikey",
                            LastName = "Underwood",
                            FullName = "Mikey Underwood",
                            Email = "munderwood@yahoo.com",
                            Address = "12312 Hot dog Rd, Clement Georgia 12234",
                            PhoneNumber = "802-234-5132"
                        },
                        new MpContact() {
                            FirstName = "Nina",
                            LastName = "Jacobson",
                            FullName = "Nina Jacobson",
                            Email = "ninaaaaaaa@yahoo.com",
                            Address = "123 Fort Hunt St, Lake Patunia Mississippi 41207",
                            PhoneNumber = "213-542-5223"
                        }
                    };
            foreach (var c in cl) {
                var cvm = await CreateConactViewModel(c);
                Items.Add(cvm);
            }
            if (Items.Count == 0) {
                var ecvm = await CreateConactViewModel(MpContact.EmptyContact);
                Items.Add(ecvm);
            }

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        public async Task<MpContactViewModel> CreateConactViewModel(MpContact c) {
            var cvm = new MpContactViewModel(this);
            await cvm.InitializeAsync(c);
            return cvm;
        }
        #endregion
        #region Private Methods
        public async Task<IEnumerable<MpContact>> GetContacts() {
            var contacts = new List<MpIContact>();

            var fetchers = MpPluginLoader.Plugins.Where(x => x.Value.Component is MpIContactFetcherComponentBase).Select(x => x.Value.Component).Distinct();
            foreach (var fetcher in fetchers) {
                if (fetcher is MpIContactFetcherComponent cfc) {
                    contacts.AddRange(cfc.FetchContacts(null));
                } else if (fetcher is MpIContactFetcherComponentAsync cfac) {
                    var results = await cfac.FetchContactsAsync(null);
                    contacts.AddRange(results);
                }
            }
            return contacts.Select(x => new MpContact(x));
        }

        #endregion
    }
}
