using System;
using System.Collections.Concurrent;
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
            int enter_step = 0,
            int fail_step = 100,
            bool continue_after_timeout = false) {
            if (lockObj == null) {
                throw new NullReferenceException("must have lockObj");
            }
            Stopwatch time_out_sw =
                time_out_ms >= 0 ?
                    Stopwatch.StartNew() :
                    null;

            // initial id will be 1 

            int this_sim_id = await DoAddWaiterByLockAsync(lockObj, debug_label, fail_step);
            debug_label = string.IsNullOrEmpty(debug_label) ? "Unknown" + this_sim_id : debug_label;

            if (!waitWhenTrueFunc.Invoke()) {
                // effectively waited 0 ms
                await DoRemoveWaiterByLockAsync(lockObj, debug_label, fail_step);
                return;
            }
            //MpConsole.WriteLine($"Item '{debug_label}' waiting at queue: {this_sim_id}...");

            while (true) {
                int wait_count = await DoGetWaitCountByLockAsync(lockObj, debug_label, fail_step);
                if (time_out_sw != null &&
                        time_out_sw.ElapsedMilliseconds >= time_out_ms) {
                    // this waiter has timed out and will exit
                    // remove its tick in the count
                    await DoRemoveWaiterByLockAsync(lockObj, debug_label, fail_step);
                    if (continue_after_timeout) {
                        // waiting for something that failed or broke? (used in cliptray additem)
                        break;
                    } else {

                        throw new TimeoutException($"Lock for Item '{debug_label}' timed out waiting. Wait count reduced to {wait_count}");
                    }
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
                // its our turn!
                await Task.Delay(enter_step);
                await DoRemoveWaiterByLockAsync(lockObj, debug_label, fail_step);
                break;
            }
        }


        private static bool TryAdjustWaitByLock(object lockObj, int count, string debug_label, out int new_val) {
            new_val = 0;
            if (_lockCountLookup == null ||
                !_lockCountLookup.ContainsKey(lockObj)) {
                return false;
            }
            if (_lockCountLookup.TryGetValue(lockObj, out int old_val)) {
                //MpConsole.WriteLine($"Item '{debug_label}' waiting DONE");
                new_val = old_val + count;
                if (_lockCountLookup.TryUpdate(lockObj, new_val, old_val)) {
                    return true;
                }
            }
            return false;
        }

        public static int GetWaitCountByLock(object lockObj, string debug_label) {
            if (_lockCountLookup.TryGetValue(lockObj, out int count)) {
                return count;
            }
            MpConsole.WriteLine($"Lock for Item '{debug_label}' count fetch FAILED");
            return -1;
        }
        private static int RemoveWaiterByLock(object lockObj, string debug_label) {
            if (TryAdjustWaitByLock(lockObj, -1, debug_label, out int new_val)) {

                if (new_val <= 0) {
                    if (_lockCountLookup.TryRemove(lockObj, out _)) {
                        // MpConsole.WriteLine($"Lock for Item '{debug_label}' removal SUCCEEDED");
                    } else {
                        MpConsole.WriteLine($"Lock for Item '{debug_label}' removal FAILED");
                        return -1;
                    }
                }
                return Math.Max(0, new_val);
            }
            return -1;
        }
        private static int AddWaiterByLock(object lockObj, string debug_label) {
            if (_lockCountLookup == null) {
                _lockCountLookup = new ConcurrentDictionary<object, int>();
            }
            if (TryAdjustWaitByLock(lockObj, 1, debug_label, out int new_val)) {
                return new_val;
            } else if (_lockCountLookup.TryAdd(lockObj, 1)) {
                return 1;
            }
            MpConsole.WriteLine($"Lock for Item '{debug_label}' create FAILED");
            return 0;
        }
        private static async Task<int> DoGetWaitCountByLockAsync(object lockObj, string debug_label, int fail_step) {
            int count = GetWaitCountByLock(lockObj, debug_label);
            while (count < 0) {
                MpConsole.WriteLine($"Item '{debug_label}' waiting {fail_step}ms to re-attempt count fetch..");
                await Task.Delay(fail_step);
                count = GetWaitCountByLock(lockObj, debug_label);
            }
            return count;
        }
        private static async Task<int> DoAddWaiterByLockAsync(object lockObj, string debug_label, int fail_step) {
            int this_sim_id = AddWaiterByLock(lockObj, debug_label);
            while (this_sim_id == 0) {
                MpConsole.WriteLine($"Item '{debug_label}' waiting {fail_step}ms to re-attempt add..");
                await Task.Delay(fail_step);
                this_sim_id = AddWaiterByLock(lockObj, debug_label);
            }
            return this_sim_id;
        }

        private static async Task<bool> DoRemoveWaiterByLockAsync(object lockObj, string debug_label, int fail_step) {
            bool success = RemoveWaiterByLock(lockObj, debug_label) >= 0;
            while (!success) {
                MpConsole.WriteLine($"Item '{debug_label}' waiting {fail_step}ms to re-attempt remove..");
                await Task.Delay(fail_step);
                success = RemoveWaiterByLock(lockObj, debug_label) >= 0;
            }
            return success;
        }
    }
}
