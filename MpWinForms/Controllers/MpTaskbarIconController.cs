using Auth0.OidcClient;
using Gma.System.MouseKeyHook;
using NonInvasiveKeyboardHookLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTaskbarIconController : MpController {
        public MpLogFormController LogFormController { get; set; }

        public MpSettingsForm SettingsForm { get; set; }
        public MpHelpForm HelpForm { get; set; }
        public NotifyIcon TrayIcon;                       // the icon that sits in the system tray      

        public MpKeyboardHook _showMainFormHook;
        
        private bool _skipAuth = true;

        public MpTaskbarIconController(MpController parent = null) : base(parent) {
            InitTrayMenu();                       
            //HelpForm = new MpHelpForm();
            //SettingsForm = new MpSettingsForm();
                       
            if(_skipAuth == false) {
                if(MpHelperSingleton.Instance.CheckForInternetConnection()) {
                    ShowLoginForm();
                }
                else {
                    MessageBox.Show("Error, must be connected to internet to use, exiting");
                    Exit();
                }
            }

            LogFormController = new MpLogFormController(this);
            ActivateHotKeys();
            //only temporary 
            LogFormController.ShowLogForm();
        }
        public void ActivateHotKeys() {
            if(_showMainFormHook != null) {
                DeactivateHotKeys();
            }
            _showMainFormHook = new MpKeyboardHook();
            _showMainFormHook.RegisterHotKey(ModifierKeys.Control, Keys.D);
            _showMainFormHook.KeyPressed += _showMainFormHook_KeyPressed;
        }

        private void _showMainFormHook_KeyPressed(object sender, KeyPressedEventArgs e) {
            LogFormController.ShowLogForm();
        }       

        public void DeactivateHotKeys() {
            if(_showMainFormHook == null) {
                return;
            }
            _showMainFormHook.UnregisterHotKey();
            _showMainFormHook.Dispose();
            _showMainFormHook = null;
        }
        public override void Update() {
            throw new NotImplementedException();
        }
        
        public void MouseUpHook_MouseEvent() {
            Console.WriteLine("Mouse up event occured");
                      
        }

        public void MouseHitScreenTopHook_MouseEvent() {
            LogFormController.ShowLogForm();
        }

        public void ToggleAppendModeHook_KeyPressed() {
            Properties.Settings.Default.IsAppendModeActive = !Properties.Settings.Default.IsAppendModeActive;
            //LogFormController.LogForm.Invoke((MethodInvoker)delegate {
            //    if(Properties.Settings.Default.IsAppendModeActive) {
            //        NotifyIcon.BalloonTipText = "Append mode activated";
            //        NotifyIcon.ShowBalloonTip(3000);
            //    }
            //    else {
            //        MpSingletonController.Instance.AppendItem = null;
            //        NotifyIcon.BalloonTipText = "Append mode deactivated";
            //        NotifyIcon.ShowBalloonTip(3000);
            //    }
            //});
            Console.WriteLine("Append Mode: " + Properties.Settings.Default.IsAppendModeActive);
            if(Properties.Settings.Default.IsAppendModeActive) {
                TrayIcon.BalloonTipText = "Append mode activated";
                TrayIcon.ShowBalloonTip(5000);
                if(LogFormController.LogFormPanelController.TileChooserPanelController.SelectedTilePanelController != null && LogFormController.LogFormPanelController.TileChooserPanelController.SelectedTilePanelController.CopyItem.CopyItemType == MpCopyItemType.Text) {
                    MpSingletonController.Instance.AppendItem = LogFormController.LogFormPanelController.TileChooserPanelController.SelectedTilePanelController.CopyItem;
                }
            }
            else {
                MpSingletonController.Instance.AppendItem = null;
                TrayIcon.BalloonTipText = "Append mode deactivated";
                TrayIcon.ShowBalloonTip(5000);
            }

        }

        private void Exit() {
            // before we exit, let forms clean themselves up.
            if(HelpForm != null) {
                HelpForm.Close();
            }
            if(SettingsForm != null) {
                SettingsForm.Close();
            }
            if(LogFormController != null) {
                LogFormController.CloseLogForm();
            }
            TrayIcon.Visible = false; // should remove lingering tray icon
            MpAppContext.ExitApp();          
        }
        private void InitTrayMenu() {
            TrayIcon = new NotifyIcon() {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = MpHelperSingleton.Instance.GetIconFromBitmap(Properties.Resources.monkey3),
                Text = Properties.Settings.Default.AppName,
                Visible = true
            };
            TrayIcon.MouseUp += NotifyIcon_MouseUp;
            //TrayIcon.MouseDoubleClick += NotifyIcon_DoubleClick;
            TrayIcon.BalloonTipTitle = "Monkey Paste";
            TrayIcon.BalloonTipText = "Howdy there";
            TrayIcon.ShowBalloonTip(5000);
            TrayIcon.ContextMenuStrip.Items.Clear();
            
            ToolStripMenuItem settingsSubMenu = new ToolStripMenuItem("&Settings");
            settingsSubMenu.Font = new Font(Properties.Settings.Default.LogFont,Properties.Settings.Default.LogPanelTileFontSize);
            
            ToolStripMenuItem fileSubMenu = new ToolStripMenuItem("&File");
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Clear History",clearHistory_Click));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Set Password",_toggleEncrypt_Click));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Details",ShowDetails_Click));
            fileSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Show File Location",showFileLocation_Click));
            settingsSubMenu.DropDownItems.Add(fileSubMenu);

            ToolStripMenuItem systemSubMenu = new ToolStripMenuItem("&System");
            systemSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Load on login",autoLoad_Click));
            systemSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Preferences",preferences_Click));
            settingsSubMenu.DropDownItems.Add(systemSubMenu);

            TrayIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Pause",_toggleActive_Click));
            TrayIcon.ContextMenuStrip.Items.Add(settingsSubMenu);
            TrayIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Help/About",showHelpItem_Click));
            TrayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            TrayIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Exit",exitItem_Click));
        }
        private async void ShowLoginForm() {
            if(_skipAuth) {
                Console.WriteLine("Skipping auth0 authorization");
            }
            else {
                var client = new Auth0Client(new Auth0ClientOptions {
                    Domain = "monkeypaste.auth0.com",
                    ClientId = "mlb0PEtuS7QSKIU7LOKOqOLxDKvs1Fk8"
                });
                //var loginResult = await client.LoginAsync(null);//client.LoginAsync();

                //if(!loginResult.IsError)
                {
                    //Init(loginResult.IdentityToken,loginResult.AccessToken);
                }
            }
        }
        private void ShowSettingsForm() {
            if(SettingsForm == null || SettingsForm.IsDisposed) {
                SettingsForm = new MpSettingsForm();
                SettingsForm.Closed += settingsForm_Closed;
                SettingsForm.Show();
            }
            else {
                SettingsForm.Activate();
                SettingsForm.Show();
            }
        }
        private void ShowHelpForm() {
            if(HelpForm == null) {
                HelpForm = new MpHelpForm();
                HelpForm.Closed += helpForm_Closed; // avoid reshowing a disposed form
                HelpForm.Show();
            }
            else {
                HelpForm.Show();
                HelpForm.Activate();
            }
        }
        public void _toggleSettingsHook_KeyPressed() {
            Console.WriteLine("Pressed settings toggle");

            if(TrayIcon.ContextMenuStrip != null) {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("HideContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(TrayIcon,null);
                TrayIcon.ContextMenuStrip = null;
            }
            else {
                InitTrayMenu();
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(TrayIcon,null);
                TrayIcon.ContextMenuStrip.Items[0].Select();
            }
        }
        private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText,EventHandler eventHandler) {
            var item = new ToolStripMenuItem(displayText);
            if(eventHandler != null) {
                item.Click += eventHandler;
            }
            return item;
        }
        private void showHelpItem_Click(object sender,EventArgs e) { ShowHelpForm(); } 
        private void showSettingsItem_Click(object sender,EventArgs e) { ShowSettingsForm(); }
        private void exitItem_Click(object sender,EventArgs e) { Exit(); }        
        private void _toggleActive_Click(object sender,EventArgs e) { Console.WriteLine("Pause/Resume clicked"); }
        private void openFile_Click(object sender,EventArgs e) { Console.WriteLine("Open File clicked"); }
        private void toggleSaveHistory_Click(object sender,EventArgs e) { Console.WriteLine("Save History clicked"); }
        private void clearHistory_Click(object sender,EventArgs e) {
            Console.WriteLine("Clear History clicked");
            MpAppManager.Instance.DataModel.Db.ResetDb();
        }
        private void _toggleEncrypt_Click(object sender,EventArgs e) {
            Console.WriteLine("Encrypt clicked");
            MpSetDbPasswordForm setPwDialog = new MpSetDbPasswordForm();
            DialogResult setPwResult = setPwDialog.ShowDialog();
        }
        private void showFileLocation_Click(object sender,EventArgs e) {
            Console.WriteLine("Show File Location clicked");
        }
        private void ShowDetails_Click(object sender,EventArgs e) {
            Console.WriteLine("Show Details clicked");
        }
        private void autoLoad_Click(object sender,EventArgs e) {
            Console.WriteLine("Auto-Load clicked");
        }
        private void preferences_Click(object sender,EventArgs e) {
            MpSettingsForm settingsForm = new MpSettingsForm();
            settingsForm.ShowDialog();
        }
        private void settingsForm_Closed(object sender,EventArgs e) {
            SettingsForm = null;
        }
        private void helpForm_Closed(object sender,EventArgs e) {
            HelpForm = null;
        }
        //private void notifyIcon_DoubleClick(object sender, EventArgs e) { _mpLogFormController.ShowLogForm(); }
        // From http://stackoverflow.com/questions/2208690/invoke-notifyicons-context-menu
        private void NotifyIcon_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button == MouseButtons.Right) {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(TrayIcon,null);
            }
        }
        private void NotifyIcon_DoubleClick(object sender,MouseEventArgs e) {
            //LogFormController.Togg();
        }
        private void ContextMenuStrip_Closing(object sender,ToolStripDropDownClosingEventArgs e) {
            TrayIcon.ContextMenuStrip.Hide();
            TrayIcon.ContextMenuStrip = null;
        }
        private void ContextMenuStrip_Opening(object sender,System.ComponentModel.CancelEventArgs e) { }

        

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}
