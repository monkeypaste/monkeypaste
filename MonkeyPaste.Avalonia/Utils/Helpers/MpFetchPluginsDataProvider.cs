using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpFetchPluginsDataProvider {
        static IEnumerable<MpRuntimePlugin> FetcherPlugins =>
            MpPluginLoader.Plugins.Where(x => x.pluginType == MpPluginType.Fetcher);
        public static async Task<IEnumerable<MpIContact>> GetContactsAsync() {
            // TODO passing args as null cause dunno what should use
            var contact_fetches = await Task.WhenAll(FetcherPlugins.Select(x => IssueFetchRequestAsync(x, null)));
            var contacts = contact_fetches.SelectMany(x => x.Contacts).Distinct();
            return contacts;
        }

        private static async Task<MpPluginContactFetchResponseFormat> IssueFetchRequestAsync(MpRuntimePlugin plugin, MpPluginContactFetchRequestFormat req) {
            string method_name = nameof(MpIContactFetcherComponent.Fetch);
            string on_type = typeof(MpIContactFetcherComponent).FullName;
            var resp = await plugin.IssueRequestAsync<MpPluginContactFetchResponseFormat>(method_name, on_type, req);
            return resp;
        }
    }
}
