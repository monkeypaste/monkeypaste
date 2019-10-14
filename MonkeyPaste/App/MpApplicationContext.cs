using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using MonkeyPaste.Model;
using MonkeyPaste.View;
using System.Data;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using System.Data.SQLite;
using System.IO;

namespace MonkeyPaste {

    public class MpApplicationContext : ApplicationContext  {
        MpTaskbarIconController _taskbarController;
        public System.ComponentModel.IContainer components;    // a list of components to dispose when the context is disposed
                                                                /// <summary>
        ///////////// This class should be created and passed into Application.Run( ... )
        /// </summary>
        public MpApplicationContext() {
            InitializeContext();
            _taskbarController = new MpTaskbarIconController(this,null,(string)MpRegistryHelper.Instance.GetValue("DBPath"),(string)MpRegistryHelper.Instance.GetValue("DBPassword"));
        }

        public void ExitCore() {
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
