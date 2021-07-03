using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISync {
        string GetThisClientGuid();
        string GetLocalIp4Address();
        string GetExternalIp4Address();

        Task<string> GetLocalLog();

        Task<List<object>> GetLocalData();
        Task ProcessRemoteData(List<object> remoteData);
        Task<MpStreamMessage> ProcessRemoteDbLog(MpDbMessage dbLogMessage);
        string ConvertToJson(List<object> objList);
    }
}
