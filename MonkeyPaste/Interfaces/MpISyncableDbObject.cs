using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISyncableDbObject {
        Task<object> PopulateDbObjectFromJson(object obj);
        string SerializeDbObject();
        Type GetDbObjectType();
    }
}
