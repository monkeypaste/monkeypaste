using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISyncableDbObject {
        Task<object> DeserializeDbObject(object obj);
        string SerializeDbObject();
        Type GetDbObjectType();
    }
}
