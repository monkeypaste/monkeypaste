using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;

namespace MonkeyPaste {
   
    public static class MpProgram {
        public static string AppName = "MonkeyPaste";

        [STAThread]
        static void Main() {
            if(Environment.OSVersion.Version.Major >= 6) {
                bool result = WinApi.SetProcessDPIAware();
                Console.WriteLine("DPI Awareness: " + result.ToString());
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try {
                var applicationContext = new MpApplicationContext();
                Application.Run(applicationContext);
            }
            catch(Exception ex) {
                Console.WriteLine("Program terminated: " + ex.ToString());
                //MessageBox.Show(ex.ToString(),"Program Terminated Unexpectedly",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
           // MpSingleInstance.Stop();
        }
        static void CurrentDomain_ProcessExit(object sender,EventArgs e) {
            MessageBox.Show("Exiting");
        }
    }
}
