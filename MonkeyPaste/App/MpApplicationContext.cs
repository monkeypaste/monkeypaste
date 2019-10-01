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
        private static readonly string IconFileName = "monkeyIcon16x16.ico";
        private static readonly string DefaultTooltip = "MonkeyPaste";

        private MpKeyboardHook _toggleSettingsHook;
        private bool _skipAuth = true;
                
        private MpLogFormController _logFormController;

        private MpSettingsForm settingsForm;
        private MpHelpForm helpForm;
        private NotifyIcon notifyIcon;				            // the icon that sits in the system tray      
        private bool _showingModal = false;

        private System.ComponentModel.IContainer components;	// a list of components to dispose when the context is disposed
        //MpResizableBorderlessForm testForm = null;
        /// <summary>
		///////////// This class should be created and passed into Application.Run( ... )
		/// </summary>
		public MpApplicationContext() {
            InitializeContext();

            _toggleSettingsHook = new MpKeyboardHook();
            _toggleSettingsHook.RegisterHotKey(ModifierKeys.Alt,Keys.D);
            _toggleSettingsHook.KeyPressed += _toggleSettingsHook_KeyPressed;
            
            //testForm = new MpResizableBorderlessForm();
            //testForm.Show();
            if(MpHelperFunctions.Instance.CheckForInternetConnection()) {
                ShowLoginForm();
            }
            else {
                MessageBox.Show("Error, must be connected to internet to use, exiting");
                ExitThreadCore();
            }
        }
        private void InitTrayMenu() {
            notifyIcon.ContextMenuStrip.Items.Clear();
            /*if(MpSingletonController.Instance.GetMpData().GetMpClient() != null) {
                notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Logout",logoutItem_Click));
                notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }
            else {
                notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Login",loginItem_Click));
                notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }*/
            ToolStripMenuItem settingsSubMenu = new ToolStripMenuItem("&Settings");
            settingsSubMenu.Font = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize"));

            ToolStripMenuItem fileSubMenu = new ToolStripMenuItem("&File");
            //fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Open History File...",openFile_Click));
            //fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Save History",toggleSaveHistory_Click));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Clear History",clearHistory_Click));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Set Password",toggleEncrypt_Click));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Details",ShowDetails_Click));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Show File Location",showFileLocation_Click));
            settingsSubMenu.DropDownItems.Add(fileSubMenu);

            ToolStripMenuItem systemSubMenu = new ToolStripMenuItem("&System");
            systemSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Load on login",autoLoad_Click));
            settingsSubMenu.DropDownItems.Add(systemSubMenu);

            notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Pause",toggleActive_Click));
            notifyIcon.ContextMenuStrip.Items.Add(settingsSubMenu);//ToolStripMenuItemWithHandler("&Settings", showSettingsItem_Click));
            notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Help/About",showHelpItem_Click));
            notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Exit",exitItem_Click));
        }
        private void _toggleSettingsHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            Console.WriteLine("Pressed settings toggle");
            
            if(notifyIcon.ContextMenuStrip != null) {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("HideContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon,null);
                notifyIcon.ContextMenuStrip = null;
            } else {
                InitTrayMenu();
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon,null);
                notifyIcon.ContextMenuStrip.Items[0].Select();
            }
        }

        private void Init(string idToken,string accessToken) {
            string dbPath = (string)MpRegistryHelper.Instance.GetValue("DBPath");
            string dbPassword = (string)MpRegistryHelper.Instance.GetValue("DBPassword");

            if(dbPath == null) {
                DialogResult result = MessageBox.Show("No Database found would you like to load a file?","No DB Found",MessageBoxButtons.YesNo);
                if(result == DialogResult.Yes) {
                    OpenFileDialog openFileDialog = new OpenFileDialog() {
                        FileName = "Select a db file",
                        Filter = "Db files (*.db)|*.db",
                        Title = "Open DB File"
                    };
                    DialogResult openResult = openFileDialog.ShowDialog();
                    if(openResult == DialogResult.OK) {
                        dbPath = openFileDialog.FileName;
                        DialogResult autoLoadResult = MessageBox.Show("Would you like to remember this next time?","Remember Database?",MessageBoxButtons.YesNo);
                        if(autoLoadResult == DialogResult.Yes) {
                            MpRegistryHelper.Instance.SetValue("DBPath",dbPath);
                        }
                    }
                } else {
                    DialogResult newDbResult = MessageBox.Show("Would you like to create a new database and store your history?","New Database?",MessageBoxButtons.YesNo);                    
                    if(newDbResult == DialogResult.Yes) {
                        dbPath = AppDomain.CurrentDomain.BaseDirectory + @"\mp.db";
                        MpRegistryHelper.Instance.SetValue("DBPath",dbPath);
                        DialogResult newDbPasswordResult = MessageBox.Show("Would you like to encrypt database with a password?","Encrypt?",MessageBoxButtons.YesNo);
                        if(newDbPasswordResult == DialogResult.Yes) {
                            MpSetDbPasswordForm setDbPasswordForm = new MpSetDbPasswordForm();
                            setDbPasswordForm.ShowDialog();
                            dbPassword = setDbPasswordForm.PasswordTextBox.Text;
                        }
                    }
                }
                
            }
            MpSingletonController.Instance.Init(dbPath,dbPassword,idToken,accessToken);
            MpSingletonController.Instance.SetKeyboardHook(MpInputCommand.ToggleSettings,_toggleSettingsHook);

            InitTrayMenu();
            //notifyIcon.ContextMenuStrip.Closing += ContextMenuStrip_Closing;
            //notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            notifyIcon.MouseUp += NotifyIcon_MouseUp;
            notifyIcon.MouseDoubleClick += NotifyIcon_DoubleClick;

            _logFormController = new MpLogFormController();
            // NOTE this is run after lfc because it creates a form and lastwindowwatcher needs its handle            
        }

        private async void ShowLoginForm() {
            if(_skipAuth) {
                Console.WriteLine("Skipping auth0 authorization");
                Init("Test","root");
            }
            else {
                var client = new Auth0Client(new Auth0ClientOptions {
                    Domain = "monkeypaste.auth0.com",
                    ClientId = "mlb0PEtuS7QSKIU7LOKOqOLxDKvs1Fk8"
                });
                var loginResult = await client.LoginAsync(null);//client.LoginAsync();

                if(!loginResult.IsError) {
                    Init(loginResult.IdentityToken,loginResult.AccessToken);
                }
            }            
        }
        #region tray
        
        private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, EventHandler eventHandler) {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null) {
                item.Click += eventHandler;
            }
            return item;
        }          

        private void ShowSettingsForm() {
            if (settingsForm == null || settingsForm.IsDisposed)  {
                settingsForm = new MpSettingsForm();
                settingsForm.Closed += settingsForm_Closed;
                settingsForm.Show();
            }
            else {
                settingsForm.Activate();
                settingsForm.Show();
            }
        }
        private void ShowHelpForm() {
            if (helpForm == null){
                helpForm = new MpHelpForm();
                helpForm.Closed += helpForm_Closed; // avoid reshowing a disposed form
                helpForm.Show();
            }
            else {
                helpForm.Show();
                helpForm.Activate();
            }
        }

        
        // attach to context menu items
        private void showHelpItem_Click(object sender, EventArgs e) { ShowHelpForm(); }
        private void showSettingsItem_Click(object sender, EventArgs e) { ShowSettingsForm(); }

        private void toggleActive_Click(object sender,EventArgs e) { Console.WriteLine("Pause/Resume clicked"); }
        private void openFile_Click(object sender,EventArgs e) { Console.WriteLine("Open File clicked"); }
        private void toggleSaveHistory_Click(object sender,EventArgs e) { Console.WriteLine("Save History clicked"); }
        private void clearHistory_Click(object sender,EventArgs e) {
            Console.WriteLine("Clear History clicked");
            MpSingletonController.Instance.GetMpData().DeleteDb();
        }
        private void toggleEncrypt_Click(object sender,EventArgs e) {
            Console.WriteLine("Encrypt clicked");
            MpSetDbPasswordForm setPwDialog = new MpSetDbPasswordForm();
            setPwDialog.Load += Setting_Load;
            setPwDialog.FormClosed += Setting_FormClosed;

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if(setPwDialog.ShowDialog() == DialogResult.OK) {
                // Read the contents of testDialog's TextBox.
                //this.txtResult.Text = setPwDialog.TextBox1.Text;
            }
            else {
               // this.txtResult.Text = "Cancelled";
            }
            setPwDialog.Dispose();
        }

        private void Setting_Load(object sender,EventArgs e) {
            _showingModal = true;
        }

        private void Setting_FormClosed(object sender,FormClosedEventArgs e) {
            _showingModal = false;
        }

        private void showFileLocation_Click(object sender,EventArgs e) { Console.WriteLine("Show File Location clicked"); }
        private void ShowDetails_Click(object sender,EventArgs e) { Console.WriteLine("Show Details clicked"); }

        private void autoLoad_Click(object sender,EventArgs e) { Console.WriteLine("Auto-Load clicked"); }
        #endregion

        #region events
        private void settingsForm_Closed(object sender, EventArgs e) {
            settingsForm = null;
        }
        private void helpForm_Closed(object sender, EventArgs e) {
            helpForm = null;
        }
        private void exitItem_Click(object sender,EventArgs e) {
            ExitThread();
        }
        protected override void ExitThreadCore() {
            // before we exit, let forms clean themselves up.
            if(helpForm != null) { helpForm.Close(); }
            if(settingsForm != null) { settingsForm.Close(); }
            if(_logFormController != null) { _logFormController.CloseLogForm(); }
            notifyIcon.Visible = false; // should remove lingering tray icon
            base.ExitThreadCore();
        }
        #endregion events

        #region generic code framework
        //private void notifyIcon_DoubleClick(object sender, EventArgs e) { _mpLogFormController.ShowLogForm(); }
        // From http://stackoverflow.com/questions/2208690/invoke-notifyicons-context-menu

        private void NotifyIcon_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button == MouseButtons.Right) {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon,null);
            }
        }
        private void NotifyIcon_DoubleClick(object sender,MouseEventArgs e) {
            _logFormController.ToggleLogForm();
            //logForm._animationTimer.Start();
            //MethodInfo mi = typeof(MpLogFormController).GetMethod("ToggleLogForm",BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
            //mi.Invoke(_logFormController,null);
        }
        private void ContextMenuStrip_Closing(object sender,ToolStripDropDownClosingEventArgs e) {            
            notifyIcon.ContextMenuStrip.Hide();
            notifyIcon.ContextMenuStrip = null;
        }
        private void ContextMenuStrip_Opening(object sender,System.ComponentModel.CancelEventArgs e) {
            
        }
        private void InitializeContext() {
            components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = new Icon(IconFileName),
                Text = DefaultTooltip,
                Visible = true
               
            };            
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
