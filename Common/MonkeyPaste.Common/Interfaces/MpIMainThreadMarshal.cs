﻿using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {

    public interface MpIMainThreadMarshal {
        void RunOnMainThread(Action action, MpDispatcherPriority priority = MpDispatcherPriority.Normal);
        TResult RunOnMainThread<TResult>(Func<TResult> action, MpDispatcherPriority priority = MpDispatcherPriority.Normal);

        Task RunOnMainThreadAsync(Action action, MpDispatcherPriority priority = MpDispatcherPriority.Normal);
        //Task<TResult> RunOnMainThreadAsync<TResult>(Func<TResult> action, MpDispatcherPriority priority = MpDispatcherPriority.Normal);
    }


    public enum MpDispatcherPriority {
        //
        // Summary:
        //     The enumeration paramValue is -1. This is an invalid priority.
        Invalid = -1,
        //
        // Summary:
        //     The enumeration paramValue is 0. Operations are not processed.
        Inactive = 0,
        //
        // Summary:
        //     The enumeration paramValue is 1. Operations are processed when the system is idle.
        SystemIdle = 1,
        //
        // Summary:
        //     The enumeration paramValue is 2. Operations are processed when the application is
        //     idle.
        ApplicationIdle = 2,
        //
        // Summary:
        //     The enumeration paramValue is 3. Operations are processed after background operations
        //     have completed.
        ContextIdle = 3,
        //
        // Summary:
        //     The enumeration paramValue is 4. Operations are processed after all other non-idle
        //     operations are completed.
        Background = 4,
        //
        // Summary:
        //     The enumeration paramValue is 5. Operations are processed at the same priority as
        //     input.
        Input = 5,
        //
        // Summary:
        //     The enumeration paramValue is 6. Operations are processed when layout and render has
        //     finished but just before items at input priority are serviced. Specifically this
        //     is used when raising the Loaded event.
        Loaded = 6,
        //
        // Summary:
        //     The enumeration paramValue is 7. Operations processed at the same priority as rendering.
        Render = 7,
        //
        // Summary:
        //     The enumeration paramValue is 8. Operations are processed at the same priority as
        //     data binding.
        DataBind = 8,
        //
        // Summary:
        //     The enumeration paramValue is 9. Operations are processed at normal priority. This
        //     is the typical application priority.
        Normal = 9,
        //
        // Summary:
        //     The enumeration paramValue is 10. Operations are processed before other asynchronous
        //     operations. This is the highest priority.
        Send = 10
    }
}
