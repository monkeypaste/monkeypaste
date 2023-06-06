using MonkeyPaste.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
//using Avalonia.Win32;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpFifoAsyncQueue {
        private static ConcurrentDictionary<object, int> _lockCountLookup;

        public static async Task WaitByConditionAsync(
            object lockObj,
            Func<bool> waitWhenTrueFunc,
            string debug_label = "",
            int wait_step = 100,
            int locked_step = 200,
            int enter_step = 300) {
            if (lockObj == null) {
                throw new NullReferenceException("must have lockObj");
            }
            if (!waitWhenTrueFunc.Invoke()) {
                return;
            }


            int this_sim_id = GetOrCreateWaitCountByLock(lockObj, debug_label);
            debug_label = string.IsNullOrEmpty(debug_label) ? "Unknown" + this_sim_id : debug_label;

            MpConsole.WriteLine($"Item '{debug_label}' waiting at queue: {this_sim_id}...");

            while (GetOrCreateWaitCountByLock(lockObj, debug_label) >= this_sim_id) {
                if (waitWhenTrueFunc()) {
                    await Task.Delay(wait_step);
                }
                if (GetOrCreateWaitCountByLock(lockObj, debug_label) > this_sim_id) {
                    // not next (wait extra 100 for next to start)
                    await Task.Delay(locked_step);
                    continue;
                }
                // add wait cause repetetive pasting is leaking last gesture for some reason
                await Task.Delay(enter_step);
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
        }

        private static int GetOrCreateWaitCountByLock(object lockObj, string debug_label) {
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
