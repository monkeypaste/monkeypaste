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

        public async Task AddUpdateOrDeleteTemplateAsync(int notifier_ciid, MpTextTemplate tt, bool isDelete) {
            MpConsole.WriteLine($"Shared template '{tt.TemplateName}' update started from tile id {notifier_ciid}");
            // get db ref to template
            tt.Id = await MpDataModelProvider.GetItemIdByGuidAsync<MpTextTemplate>(tt.Guid);

            bool is_new = tt.Id == 0 && !isDelete;
            string label = isDelete ? "deleted" : is_new ? "add" : "updated";
            await tt.WriteToDatabaseAsync();

            if (is_new) {
                // no other items could share this template yet since its new so be done
                MpConsole.WriteLine($"Shared template '{tt.TemplateName}' is new. No other tiles to notify");
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
                    changedTemplateFragmentStr = isDelete ? null : tt.SerializeJsonObjectToBase64(),
                    deletedTemplateGuid = isDelete ? tt.Guid : null
                };
                active_template_ctvms
                    .Select(x => x.GetContentView() as MpAvContentWebView)
                    .Where(x => x != null)
                    .ForEach(x =>
                        x.SendMessage($"sharedTemplateChanged_ext('{shared_template_changed_msg.SerializeJsonObjectToBase64()}')"));

                active_template_ctvms
                    .ForEach(x => MpConsole.WriteLine($"Active Template Tile '{x}' was notified of {label} template '{tt.TemplateName}'"));
            }

            _ = Task.Run(async () => {
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
                HtmlNode tt_node = null;
                if (!isDelete) {
                    tt_node = html_doc.DocumentNode.SelectSingleNode($"//span[@templateguid = '{tt.Guid}']");
                    MpDebug.Assert(tt_node != null, $"xpath error finding template '{tt.Guid}' in ciid {notifier_ciid}");
                }

                foreach (var other_ci_with_tt in cil_to_manually_update) {
                    var cur_html_doc = new HtmlDocument();
                    cur_html_doc.LoadHtml(other_ci_with_tt.ItemData);
                    var cur_tt_nodes = html_doc.DocumentNode.SelectNodes($"//span[@templateguid = '{tt.Guid}']").ToList();
                    //for (int i = 0; i < cur_tt_nodes.Count; i++) {
                    foreach (var cur_tt_node in cur_tt_nodes) {
                        if (isDelete) {
                            cur_tt_node.Remove();
                            continue;
                        }
                        cur_tt_node.SetAttributeValue("templateBgColor", tt_node.GetAttributeValue("templateBgColor", string.Empty));
                        cur_tt_node.SetAttributeValue("templateName", tt_node.GetAttributeValue("templateName", string.Empty));
                        cur_tt_node.SetAttributeValue("templateData", tt_node.GetAttributeValue("templateData", string.Empty));
                    }
                    string oldData = other_ci_with_tt.ItemData;
                    other_ci_with_tt.ItemData = cur_html_doc.DocumentNode.InnerHtml;
                    MpConsole.WriteLine($"Template Tile '{other_ci_with_tt}' content {label}", true);
                    MpConsole.WriteLine("Old data: ");
                    MpConsole.WriteLine(oldData);
                    MpConsole.WriteLine("New Data:");
                    MpConsole.WriteLine(other_ci_with_tt.ItemData, false, true);
                }
                await Task.WhenAll(cil_to_manually_update.Select(x => x.WriteToDatabaseAsync()));
            });
        }

        public bool HasHtmlTemplate(string text) {
            if (text == null) {
                return false;
            }
            return text.Contains("<span class=\"template-blot");
        }

        public async Task<IEnumerable<MpIContact>> GetContactsAsync() {
            var contacts = new List<MpIContact>();

            var fetchers =
                MpPluginLoader.Plugins
                .Where(x => x.Value.Component is MpIAnalyzeComponent)
                .Select(x => x);
            //.Select(x => x.Value.Component).Distinct();

            foreach (var fetcher_kvp in fetchers) {
                string guid = fetcher_kvp.Key;
                var fetcher = fetcher_kvp.Value.Component;
                //MpPluginRequestFormatBase req = null;
                //if(MpAvAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.PluginGuid == guid) is MpAvAnalyticItemViewModel aivm) {
                //    if(aivm.Items.FirstOrDefault(x=>x.IsGeneratedDefaultPreset) is MpAvAnalyticItemPresetViewModel aipvm) {
                //        aipvm.Ex
                //        aivm.ExecuteAnalysisCommand
                //    }
                //}
                string fetcher_dir = fetcher_kvp.Value.RootDirectory;
                if (fetcher is MpIContactFetcherComponent cfc) {
                    contacts.AddRange(cfc.Fetch(fetcher_dir));
                } else if (fetcher is MpIContactFetcherComponentAsync cfac) {
                    var results = await cfac.FetchAsync(fetcher_dir);
                    contacts.AddRange(results);
                }
            }
            return contacts;
            //return contacts.Select(x => new MpContact(x));
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
