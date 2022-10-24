using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public class MpAvOleAccessor {
        public object LockObj { get; set; }

        public List<string> AccessRequests { get; set; } = new List<string>();

    }
    public static class MpAvSafeOleAccessExtensions {
        private static Dictionary<IDataObject, List<MpAvOleAccessor>> _accessLookup = new Dictionary<IDataObject, List<MpAvOleAccessor>>();

        public static async Task<IEnumerable<string>> GetDataFormats_safe(this IDataObject ido, object safeLock) {
            AddOrUpdateAccessor(ido, safeLock, "getdataformats");
            await WaitForAccessAsync(ido, safeLock, "getdataformats");
            
            var result = ido.GetDataFormats();

            FinishAccess(ido, safeLock, "getdataformats");
            return result;
        }

        public static async Task<bool> Contains_safe(this IDataObject ido, object safeLock, string dataFormat) {
            AddOrUpdateAccessor(ido, safeLock, "contains");
            await WaitForAccessAsync(ido, safeLock, "contains");
            
            var result = ido.Contains(dataFormat);

            FinishAccess(ido, safeLock, "contains");
            return result;
        }

        public static async Task<string> GetText_safe(this IDataObject ido, object safeLock) {
            AddOrUpdateAccessor(ido, safeLock, "gettext");
            await WaitForAccessAsync(ido, safeLock, "gettext");

            var result = ido.GetText();

            FinishAccess(ido, safeLock, "gettext");
            return result;
        }

        public static async Task<IEnumerable<string>?> GetFileNames_safe(this IDataObject ido, object safeLock) {
            AddOrUpdateAccessor(ido, safeLock, "getfilenames");
            await WaitForAccessAsync(ido, safeLock, "getfilenames");

            var result = ido.GetFileNames();

            FinishAccess(ido, safeLock, "getfilenames");
            return result;
        }

        public static async Task<object?> Get_safe(this IDataObject ido, object safeLock, string dataFormat) {            
            AddOrUpdateAccessor(ido, safeLock, "get");
            await WaitForAccessAsync(ido, safeLock, "get");

            var result = ido.Get(dataFormat);

            FinishAccess(ido, safeLock, "get");
            return result;
        }


        private static void AddOrUpdateAccessor(IDataObject ido, object safeLock, string accessType) {
            if (!_accessLookup.ContainsKey(ido)) {
                _accessLookup.Add(ido, new List<MpAvOleAccessor>());
            }
            MpAvOleAccessor accessor = _accessLookup[ido].FirstOrDefault(x => x.LockObj == safeLock);
            if(accessor == null) {
                accessor = new MpAvOleAccessor() {
                    LockObj = safeLock
                };
                _accessLookup[ido].Add(accessor);
            } else {
                if(accessor.AccessRequests.Contains(accessType)) {
                    // duplicate of same request, how come?
                    //Debugger.Break();
                    return;
                }

            }
            accessor.AccessRequests.Add(accessType);
        }

        private static void FinishAccess(IDataObject ido, object safeLock, string accessType) {
            _accessLookup[ido][0].AccessRequests.RemoveAt(0);
            if (_accessLookup[ido][0].AccessRequests.Count == 0) {
                _accessLookup[ido].RemoveAt(0);
                if (_accessLookup[ido].Count == 0) {
                    _accessLookup.Remove(ido);
                }
            }
            
        }

        private static async Task WaitForAccessAsync(IDataObject ido, object safeLock, string accessType) {
            return;
            while (_accessLookup[ido][0] != safeLock) {                
                await Task.Delay(50);
            }
            while (_accessLookup[ido][0].AccessRequests[0] != accessType) {
                await Task.Delay(50);
            }
            await Task.Delay(10);
        }
    }
}
