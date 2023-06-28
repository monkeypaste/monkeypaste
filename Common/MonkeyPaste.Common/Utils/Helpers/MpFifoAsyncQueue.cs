using MonkeyPaste.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
//using Avalonia.Win32;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpFifoAsyncQueue {
        private static ConcurrentDictionary<object, int> _lockCountLookup;

        public static async Task WaitByConditionAsync(
            object lockObj,
            Func<bool> waitWhenTrueFunc,
            string debug_label = "",
            int time_out_ms = 10_000,
            int wait_step = 100,
            int locked_step = 100,
            int enter_step = 0) {
            if (lockObj == null) {
                throw new NullReferenceException("must have lockObj");
            }
            if (!waitWhenTrueFunc.Invoke()) {
                // effectively waited 0 ms
                return;
            }
            Stopwatch time_out_sw =
                time_out_ms >= 0 ?
                    Stopwatch.StartNew() :
                    null;

            // initial id will be 1 
            int this_sim_id = AddWaiterByLock(lockObj, debug_label);
            debug_label = string.IsNullOrEmpty(debug_label) ? "Unknown" + this_sim_id : debug_label;

            MpConsole.WriteLine($"Item '{debug_label}' waiting at queue: {this_sim_id}...");

            while (true) {
                int wait_count = GetWaitCountByLock(lockObj);
                if (time_out_sw != null &&
                        time_out_sw.ElapsedMilliseconds >= time_out_ms) {
                    // this waiter has timed out and will exit
                    // remove its tick in the count

                    DecrementWaitByLock(lockObj, debug_label);
                    throw new TimeoutException($"Lock for Item '{debug_label}' timed out waiting. Wait count reduced to {wait_count}");
                }

                if (waitWhenTrueFunc()) {
                    // can't execute so keep waiting
                    await Task.Delay(wait_step);
                    continue;
                }
                // can execute so check count
                if (wait_count > this_sim_id) {
                    // not first keep waiting
                    await Task.Delay(locked_step);
                    continue;
                }
                await Task.Delay(enter_step);
                DecrementWaitByLock(lockObj, debug_label);
            }
        }

        private static void DecrementWaitByLock(object lockObj, string debug_label) {
            _lockCountLookup[lockObj]--;

            MpConsole.WriteLine($"Item '{debug_label}' waiting DONE");
            if (_lockCountLookup[lockObj] <= 0) {
                if (!_lockCountLookup.TryRemove(lockObj, out _)) {
                    MpConsole.WriteLine($"Lock for Item '{debug_label}' removal FAILED");
                } else {
                    MpConsole.WriteLine($"Lock for Item '{debug_label}' removal SUCCEEDED");
                }
            }
        }

        public static int GetWaitCountByLock(object lockObj) {
            if (_lockCountLookup == null ||
                !_lockCountLookup.ContainsKey(lockObj)) {
                return 0;
            }
            return _lockCountLookup[lockObj];
        }
        private static int AddWaiterByLock(object lockObj, string debug_label) {
            if (_lockCountLookup == null) {
                _lockCountLookup = new ConcurrentDictionary<object, int>();
            }
            if (!_lockCountLookup.ContainsKey(lockObj)) {
                if (!_lockCountLookup.TryAdd(lockObj, 1)) {
                    MpConsole.WriteLine($"Lock for Item '{debug_label}' create FAILED");
                }
                return 1;
            }
            _lockCountLookup[lockObj]++;
            return _lockCountLookup[lockObj];
        }
    }
}
