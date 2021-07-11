using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISyncableDbObject {
        Task<object> DeserializeDbObject(string objStr,string parseToken = @"^(@!@");
        string SerializeDbObject(string parseToken = @"^(@!@");
        Type GetDbObjectType();

        // TODO Add the following for per column syncing
        //Dictionary<string, string> GetAlteredColumns();
    }
}
