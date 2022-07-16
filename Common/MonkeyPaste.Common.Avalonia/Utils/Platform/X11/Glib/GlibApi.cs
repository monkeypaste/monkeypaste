using Avalonia.Platform.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public static unsafe class Glib {
        private const string GlibName = "libglib-2.0.so.0";
        private const string GObjectName = "libgobject-2.0.so.0";

        [DllImport(GlibName)]
        public static extern void g_slist_free(GSList* data);

        [DllImport(GObjectName)]
        public static extern void g_object_ref(IntPtr instance);

        [DllImport(GObjectName)]
        public static extern ulong g_signal_connect_object(IntPtr instance, Utf8Buffer signal,
            IntPtr handler, IntPtr userData, int flags);

        [DllImport(GObjectName)]
        public static extern void g_object_unref(IntPtr instance);

        [DllImport(GObjectName)]
        public static extern ulong g_signal_handler_disconnect(IntPtr instance, ulong connectionId);

        public delegate bool timeout_callback(IntPtr data);

        [DllImport(GlibName)]
        public static extern ulong g_timeout_add_full(int prio, uint interval, timeout_callback callback, IntPtr data,
            IntPtr destroy);

        public static IDisposable ConnectSignal<T>(IntPtr obj, string name, T handler) {
            var handle = GCHandle.Alloc(handler);
            var ptr = Marshal.GetFunctionPointerForDelegate<T>(handler);
            using (var utf = new Utf8Buffer(name)) {
                var id = g_signal_connect_object(obj, utf, ptr, IntPtr.Zero, 0);
                if (id == 0)
                    throw new ArgumentException("Unable to connect to signal " + name);
                return new ConnectedSignal(obj, handle, id);
            }
        }


        static bool TimeoutHandler(IntPtr data) {
            var handle = GCHandle.FromIntPtr(data);
            var cb = (Func<bool>)handle.Target;
            if (!cb()) {
                handle.Free();
                return false;
            }

            return true;
        }

        private static readonly timeout_callback s_pinnedHandler;

        static Glib() {
            s_pinnedHandler = TimeoutHandler;
        }

        static void AddTimeout(int priority, uint interval, Func<bool> callback) {
            var handle = GCHandle.Alloc(callback);
            g_timeout_add_full(priority, interval, s_pinnedHandler, GCHandle.ToIntPtr(handle), IntPtr.Zero);
        }

        public static Task<T> RunOnGlibThread<T>(Func<T> action) {
            var tcs = new TaskCompletionSource<T>();
            AddTimeout(0, 0, () => {

                try {
                    tcs.SetResult(action());
                }
                catch (Exception e) {
                    tcs.TrySetException(e);
                }

                return false;
            });
            return tcs.Task;
        }
    }


    public class ConnectedSignal : IDisposable {
        private readonly IntPtr _instance;
        private GCHandle _handle;
        private readonly ulong _id;

        public ConnectedSignal(IntPtr instance, GCHandle handle, ulong id) {
            _instance = instance;
            Glib.g_object_ref(instance);
            _handle = handle;
            _id = id;
        }

        public void Dispose() {
            if (_handle.IsAllocated) {
                Glib.g_signal_handler_disconnect(_instance, _id);
                Glib.g_object_unref(_instance);
                _handle.Free();
            }
        }
    }
}
