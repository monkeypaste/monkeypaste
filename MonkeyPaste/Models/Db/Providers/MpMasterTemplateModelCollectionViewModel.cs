using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpMasterTemplateModelCollectionViewModel :
        MpViewModelBase,
        MpIMenuItemViewModel {
        #region Private Variables

        #endregion
        #region Statics

        private static MpMasterTemplateModelCollectionViewModel _instance;
        public static MpMasterTemplateModelCollectionViewModel Instance => _instance ?? (_instance = new MpMasterTemplateModelCollectionViewModel());
        #endregion

        #region Properties

        #region View Models

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                var mivm = new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>()
                };
                //foreach (var tvm in AllTemplates) {
                //    var tvm_mi = new MpMenuItemViewModel() {
                //        Header = tvm.TemplateName,
                //        IconHexStr = tvm.TemplateHexColor,
                //        Command = CreateTemplateViewModelCommand,
                //        CommandParameter = tvm
                //    };
                //    mivm.SubItems.Add(tvm_mi);
                //}
                //var ntvm_mi = new MpMenuItemViewModel() {
                //    Header = "Add New",
                //    IconResourceKey = Application.Current.Resources["AddIcon"] as string,
                //    Command = CreateTemplateViewModelCommand
                //};
                //mivm.SubItems.Add(ntvm_mi);
                return mivm;
            }
        }

        #endregion

        #region Models

        public ObservableCollection<MpTextTemplate> AllTemplates { get; set; } = new ObservableCollection<MpTextTemplate>();

        #endregion

        #endregion

        #region Constructors

        public MpMasterTemplateModelCollectionViewModel() : base(null) { }

        #endregion

        #region Public Methods

        public async Task InitAsync() {
            var ttl = await MpDataModelProvider.GetItemsAsync<MpTextTemplate>();
            AllTemplates = new ObservableCollection<MpTextTemplate>(ttl);

        }

        public async Task UpdateAsync(List<MpTextTemplate> updatedTemplates, List<string> removedGuids) {
            if (removedGuids != null) {
                var rtl = await MpDataModelProvider.GetTextTemplatesByGuidsAsync(removedGuids);
                await Task.WhenAll(rtl.Select(x => x.DeleteFromDatabaseAsync()));
            }

            if (updatedTemplates != null) {
                await Task.WhenAll(updatedTemplates.Select(x => x.WriteToDatabaseAsync()));
                AllTemplates = new ObservableCollection<MpTextTemplate>(updatedTemplates);
            } else {
                // NOTE not sure how to handle this..
                await InitAsync();
            }
        }

        public async Task<IEnumerable<MpContact>> GetContactsAsync() {
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

        public string GetTemplateTypeIconResourceStr(MpTextTemplateType templateType) {
            switch (templateType) {
                case MpTextTemplateType.Contact:
                    return "ContactIcon";
                case MpTextTemplateType.DateTime:
                    return "AlarmClockIcon";
                case MpTextTemplateType.Dynamic:
                    return "YinYangIcon";
                case MpTextTemplateType.Static:
                    return "IceCubeIcon";
            }
            return string.Empty;
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTextTemplate tt) {
                // TODO will need to scan CopyItemData for TemplateGuid and remove span tag here
                // also note ciid then notify clip tray items where match exists

                Task.Run(async () => {
                    var cil = await MpDataModelProvider.GetCopyItemsByTextTemplateGuid(tt.Guid);
                    foreach (var ci in cil) {

                    }
                });
            }
        }
        #endregion
    }
}
