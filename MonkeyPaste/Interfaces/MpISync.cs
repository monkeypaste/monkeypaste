using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISync {
        bool IsWpf();

        object GetMainThreadObj();

        bool IsConnectedToNetwork();
        bool IsConnectedToInternet();

        int GetSyncPort();

        string GetThisClientGuid();
        string GetLocalIp4Address();
        string GetExternalIp4Address();

        Task<List<MonkeyPaste.MpDbLog>> GetDbObjectLogs(string dboGuid, DateTime fromDtUtc);

        Task<DateTime> GetLastSyncForRemoteDevice(string otherDeviceGuid);
        Task<string> GetLocalLogFromSyncDate(DateTime fromDateTime, string ignoreGuid = "");
        Task<Dictionary<Guid, List<MpDbLog>>> PrepareRemoteLogForSyncing(string dbLogMessageStr);
        Task PerformSync(
            Dictionary<Guid, List<MpDbLog>> changeLookup,
            DateTime newSyncDate,
            string remoteClientGuid);

        MpIStringToSyncObjectTypeConverter GetTypeConverter();

        string GetDbFileAsBase64();

        

        event EventHandler<object> OnSyncableChange;
    }
}
