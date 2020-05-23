using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Data;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using System.Data.SQLite;
using System.IO;

namespace MonkeyPaste {

    public class MpAppContext : ApplicationContext  {   
        public static Action ExitApp { get; set; }

        public System.ComponentModel.IContainer components;
        public MpAppContext() {
            InitializeContext();
            ExitApp = () => ExitAppCore();
            MpAppManager.Instance.Init();
        }
        public void ExitAppCore() {
            Console.WriteLine("Application exiting " + DateTime.Now.ToString());
            ExitThreadCore();
        }
        #region generic code framework
        private void InitializeContext() {
            components = new System.ComponentModel.Container();            
        }
        /// <summary>
        /// When the application context is disposed, dispose things like the notify icon.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing) {
            //MpDBQuery.Instance.Logout();
            if (disposing && components != null) {
                components.Dispose();
            }
        }
        # endregion generic code framework
    }
}
