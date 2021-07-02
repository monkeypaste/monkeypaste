using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISyncData {
        Task<List<object>> GetLocalData();
        Task ProcessRemoteData(List<object> remoteData);
        string ConvertToJson(List<object> objList);
    }
}
