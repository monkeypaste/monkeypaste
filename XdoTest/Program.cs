using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace XdoTest {
    public static class Program {
        static void Main(string[] args) {
            //terminal 44040208
            // barrier 35651590
            // firefox 29360190
            Console.WriteLine("sleeping...");
            //Thread.Sleep(5_000);
            //nint xdo_context = XdoLib.xdo_new(null);

            //var test = XdoLib.xdo_get_pid_window(xdo_context, 44040208);
            //int test = default;
            //Console.WriteLine("checking now!");
            //_ = XdoLib.xdo_get_active_window(xdo_context, ref test);
            //XdoLib.xdo_activate_window(xdo_context, 29360190);
            //XdoLib.xdo_focus_window(xdo_context, 29360190);
            //string test = default;
            var test = new byte[256];
            //int name_len = default;
            //int name_type = default;
            //_ = XdoLib.xdo_get_window_name(xdo_context, 29360190, ref test, ref name_len, ref name_type);
            PidTools.get_exe_for_pid(1766, test);
            Console.WriteLine($"yo wuddup {Encoding.Default.GetString(test)}");
        }
        public static class PidTools {
            const string PidName = "pid.so";
            [DllImport(PidName)]
            public static extern int get_exe_for_pid(int pid, [Out] byte[] exe_path_return);
        }
    }
}