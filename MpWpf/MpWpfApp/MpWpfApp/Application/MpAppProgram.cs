﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;

namespace MpWpfApp {
   
    public static class MpAppProgram {
        static MpAppContext applicationContext = null;
        [STAThread]
        static void Main() {
            if(Environment.OSVersion.Version.Major >= 6) {
                //bool result = WinApi.SetProcessDPIAware();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try {
                applicationContext = new MpAppContext();
                Application.Run(applicationContext);
            }
            catch(Exception ex) {
                
                Console.WriteLine("Program terminated: " + ex.ToString());
            }
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }
        static void CurrentDomain_ProcessExit(object sender,EventArgs e) {
            
            Console.WriteLine("Exiting "+DateTime.Now.ToString());
        }
    }
}