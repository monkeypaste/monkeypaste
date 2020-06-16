using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSingleInstanceApplicationWrapper : Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase {
        public MpSingleInstanceApplicationWrapper() {
            // Enable single-instance mode.
            this.IsSingleInstance = true;
        }
        // Create the WPF application class.
        private MpApplication app;
        protected override bool OnStartup(
        Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e) {
            app = new Mp();
            app.Run();
            return false;
        }
        // Direct multiple instances.
        protected override void OnStartupNextInstance(
        Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e) {
            if(e.CommandLine.Count > 0) {
                app.ShowDocument(e.CommandLine[0]);
            }
        }

    }
}
