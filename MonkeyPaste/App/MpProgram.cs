using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;

namespace MonkeyPaste {
    static class MpProgram {
        public static string AppName = "MonkeyPaste";
        public static string SettingsFileName = "Settings.txt";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            if(!SingleInstance.Start()) { return; }
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            try {
                var applicationContext = new MpApplicationContext();
                 Application.Run(applicationContext);
            }
            catch(Exception ex) {
                MessageBox.Show(ex.ToString(),"Program Terminated Unexpectedly",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            SingleInstance.Stop();
        }
        static void CurrentDomain_ProcessExit(object sender,EventArgs e) {
            MessageBox.Show("Exiting");
        }
    }
}
