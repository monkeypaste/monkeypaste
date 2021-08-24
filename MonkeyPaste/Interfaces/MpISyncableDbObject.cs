using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISyncableDbObject {
        Task<object> DeserializeDbObject(string objStr);
        string SerializeDbObject();
        Type GetDbObjectType();

        Dictionary<string, string> DbDiff(object drOrModel);
        Task<object> CreateFromLogs(string dboGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid);

        bool DoesChangeTriggerSync();
        //SQLite.TableMapping GetTableMapping();
        //void SetTableMapping(SQLite.TableMapping tmap);
    }
}
