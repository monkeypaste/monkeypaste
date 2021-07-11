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
        Task<string> GetLocalLogFromSyncDate(DateTime fromDateTime);
        Task<string> GetDbObjRequestFromRemoteLogStr(string dbLogMessageStr);
        Task<string> GetDbObjResponseFromRequestStr(string dbObjReqStr);
        Task<object> ProcessDbObjResponse(string dbObjRespStr);
        Task CommitSync(object newObjs, string otherGuid, DateTime newSyncDt);

        MpIStringToSyncObjectTypeConverter GetTypeConverter();

        string ConvertToJson(List<object> objList);
    }
}
