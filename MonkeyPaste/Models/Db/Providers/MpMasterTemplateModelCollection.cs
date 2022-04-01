using MonkeyPaste;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpMasterTemplateModelCollection {
        #region Properties

        #region View Models

        public static ObservableCollection<MpTextTemplate> AllTemplates { get; set; } = new ObservableCollection<MpTextTemplate>();

        #endregion

        #endregion

        #region Public Methods

        public static async Task Init() {
            var ttl = await MpDb.GetItemsAsync<MpTextTemplate>();
            AllTemplates = new ObservableCollection<MpTextTemplate>(ttl);
        }

        public static async Task Update(List<MpTextTemplate> updatedTemplates, List<string> removedGuids) {            
            if(removedGuids != null) {
                var rtl = await MpDataModelProvider.GetTextTemplatesByGuids(removedGuids);
                await Task.WhenAll(rtl.Select(x => x.DeleteFromDatabaseAsync()));
            }

            if(updatedTemplates != null) {
                await Task.WhenAll(updatedTemplates.Select(x => x.WriteToDatabaseAsync()));
                AllTemplates = new ObservableCollection<MpTextTemplate>(updatedTemplates);
            } else {
                // NOTE not sure how to handle this..
                await Init();
            }
        }

        #endregion
    }
}
