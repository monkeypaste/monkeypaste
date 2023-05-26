using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvTemplateModelHelper {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvTemplateModelHelper _instance;
        public static MpAvTemplateModelHelper Instance => _instance ?? (_instance = new MpAvTemplateModelHelper());
        #endregion

        #region Properties
        #endregion

        #region Constructors

        private MpAvTemplateModelHelper() { }

        #endregion

        #region Public Methods

        public async Task AddOrUpdateTemplateAsync(int notifier_ciid, MpTextTemplate tt) {
            // get db ref to template
            tt.Id = await MpDataModelProvider.GetItemIdByGuidAsync<MpTextTemplate>(tt.Guid);

            bool is_new = tt.Id == 0;
            await tt.WriteToDatabaseAsync();

            if (is_new) {
                // no other items could share this template yet since its new so be done
                return;
            }

            // get all OTHER active items w/ templates 
            var active_template_ctvms =
                MpAvClipTrayViewModel.Instance
                .AllActiveItems
                .Where(x => x.HasTemplates && x.CopyItemId != notifier_ciid);

            if (active_template_ctvms.Any()) {
                // ntf OTHER active shared templates of changed guid
                var shared_template_changed_msg = new MpQuillSharedTemplateDataChangedMessage() {
                    changedTemplateFragmentStr = tt.SerializeJsonObjectToBase64()
                };
                active_template_ctvms
                    .Select(x => x.GetContentView() as MpAvContentWebView)
                    .Where(x => x != null)
                    .ForEach(x =>
                        x.SendMessage($"sharedTemplateChanged_ext('{shared_template_changed_msg.SerializeJsonObjectToBase64()}')"));
            }

            // remaining is a background activity and needs notifier to current in db so wait 5 seconds since
            // knowing when complete maybe wrong
            await Task.Delay(5_000);

            // scan whole db for items w/ tt
            var cil_with_tt = await MpDataModelProvider.GetCopyItemsByTextTemplateGuid(tt.Guid);
            // exclude all active items
            var cil_to_manually_update = cil_with_tt.Where(x => !active_template_ctvms.Any(y => y.CopyItemId == x.Id) && x.Id != notifier_ciid);
            if (!cil_to_manually_update.Any()) {
                // no others to update so done
                return;
            }

            // get current ci (should be updated after template change ntf)
            var notifier_ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(notifier_ciid);

            var html_doc = new HtmlDocument();
            html_doc.LoadHtml(notifier_ci.ItemData);
            var tt_node = html_doc.DocumentNode.SelectSingleNode($"//span[@templateguid = '{tt.Guid}']");
            MpDebug.Assert(tt_node != null, $"xpath error finding template '{tt.Guid}' in ciid {notifier_ciid}");

            foreach (var other_ci_with_tt in cil_to_manually_update) {
                var cur_html_doc = new HtmlDocument();
                cur_html_doc.LoadHtml(other_ci_with_tt.ItemData);
                var cur_tt_nodes = html_doc.DocumentNode.SelectNodes($"//span[@templateguid = '{tt.Guid}']");
                for (int i = 0; i < cur_tt_nodes.Count; i++) {
                    //string org_state = cur_tt_node.GetAttributeValue("templateState", tt_node.GetAttributeValue("templateState",string.Empty));
                    //cur_tt_node.ParentNode.Repl
                }
            }
        }

        public async Task DeleteTemplateAsync(int notifier_ciid, string tguid) {
            var t_to_delete = await MpDataModelProvider.GetItemAsync<MpTextTemplate>(tguid);
            if (t_to_delete == null) {
                return;
            }
            await t_to_delete.DeleteFromDatabaseAsync();

            // get all OTHER active items w/ templates 
            var active_template_ctvms =
                MpAvClipTrayViewModel.Instance
                .AllActiveItems
                .Where(x => x.HasTemplates && x.CopyItemId != notifier_ciid);

            if (active_template_ctvms.Any()) {
                // ntf OTHER active shared templates of deleted guid
                var shared_template_changed_msg = new MpQuillSharedTemplateDataChangedMessage() {
                    deletedTemplateGuid = tguid
                };
                active_template_ctvms
                    .Select(x => x.GetContentView() as MpAvContentWebView)
                    .Where(x => x != null)
                    .ForEach(x =>
                        x.SendMessage($"sharedTemplateChanged_ext('{shared_template_changed_msg.SerializeJsonObjectToBase64()}')"));
            }
        }

        public async Task<IEnumerable<MpContact>> GetContactsAsync() {
            var contacts = new List<MpIContact>();

            var fetchers =
                MpPluginLoader.Plugins
                .Where(x => x.Value.Component is MpIContactFetcherComponentBase)
                .Select(x => x.Value.Component).Distinct();

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

        #endregion
    }
}
