using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        string[] GetAllLocalIp4Addresses();
        string GetPrimaryLocalIp4Address();
        string GetExternalIp4Address();

        Task<List<MonkeyPaste.MpDbLog>> GetDbObjectLogs(string dboGuid, DateTime fromDtUtc);

        Task<DateTime> GetLastSyncForRemoteDevice(string otherDeviceGuid);

        Task<string> GetLocalLogFromSyncDate(DateTime fromDateTime, string ignoreGuid = "");

        Task UpdateSyncHistory(string otherDeviceGuid, DateTime utcDtSentLocalChanges);

        Task<Dictionary<Guid, List<MpDbLog>>> PrepareRemoteLogForSyncing(string dbLogMessageStr);

        Task PerformSync(
            Dictionary<Guid, List<MpDbLog>> changeLookup,
            string remoteClientGuid);

        MpIStringToSyncObjectTypeConverter GetTypeConverter();

        string GetDbFileAsBase64();

        ObservableCollection<MpRemoteDevice> GetRemoteDevices();

        event EventHandler<object> OnSyncableChange;
    }
}
