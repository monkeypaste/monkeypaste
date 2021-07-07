using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISync {
        bool IsWpf();
        Task RunOnMainThread(Action action);


        bool IsConnectedToNetwork();
        bool IsConnectedToInternet();

        int GetSyncPort();

        string GetThisClientGuid();
        string GetLocalIp4Address();
        string GetExternalIp4Address();

        Task<DateTime> GetLastSyncForRemoteDevice(string otherDeviceGuid);
        //this needs to return db object info 
        Task<object> ProcessRemoteDbLog(string dbLogMessage);

        Task<string> GetLocalLog(DateTime fromDateTime);

        Task<List<object>> GetLocalData();
        Task ProcessRemoteData(List<object> remoteData);
        string ConvertToJson(List<object> objList);
    }
}
