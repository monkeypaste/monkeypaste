using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISyncableDbObject {
        Task<object> DeserializeDbObjectAsync(string objStr);
        Task<string> SerializeDbObjectAsync();
        Type GetDbObjectType();

        Task<Dictionary<string, string>> DbDiffAsync(object drOrModel);
        Task<object> CreateFromLogsAsync(string dboGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid);

        //string Guid { get; set; }

        //SQLite.TableMapping GetTableMapping();
        //void SetTableMapping(SQLite.TableMapping tmap);
    }
}
