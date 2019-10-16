using Auth0.OidcClient;
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
        private static readonly string IconFileName = "monkeyIcon16x16.ico";
        private static readonly string DefaultTooltip = "Monkey Paste";

        private MpKeyboardHook _toggleSettingsHook;
        private bool _skipAuth = true;

        private MpLogFormController _logFormController;

        private MpSettingsForm settingsForm;
        private MpHelpForm helpForm;
        private NotifyIcon notifyIcon;				            // the icon that sits in the system tray      
        private MpApplicationContext _context;

        public MpTaskbarIconController(MpApplicationContext _context,MpController parent,string dbPath = null,string dbPassword = null) : base(parent) {
            MpSingletonController.Instance.Init(dbPath,dbPassword,null,null);
            _toggleSettingsHook = new MpKeyboardHook();
            _toggleSettingsHook.RegisterHotKey(ModifierKeys.Alt,Keys.D);
            _toggleSettingsHook.KeyPressed += _toggleSettingsHook_KeyPressed;
            _logFormController = new MpLogFormController(null);
            
            InitTrayMenu();
            //testForm = new MpResizableBorderlessForm();
            //testForm.Show();
            if(MpHelperSingleton.Instance.CheckForInternetConnection()) {
                ShowLoginForm();
            }
            else {
                MessageBox.Show("Error, must be connected to internet to use, exiting");
                Exit();
            }
        }
        private void Exit() {
            // before we exit, let forms clean themselves up.
            if(helpForm != null) { helpForm.Close(); }
            if(settingsForm != null) { settingsForm.Close(); }
            if(_logFormController != null) { _logFormController.CloseLogForm(); }
            notifyIcon.Visible = false; // should remove lingering tray icon

            _context.ExitCore();
        }
        private void InitTrayMenu() {
            MpSingletonController.Instance.SetKeyboardHook(MpInputCommand.ToggleSettings,_toggleSettingsHook);
            
            notifyIcon = new NotifyIcon() {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = MpHelperSingleton.Instance.GetIconFromBitmap(Properties.Resources.monkey3),
                Text = DefaultTooltip,
                Visible = true
            };
            notifyIcon.MouseUp += NotifyIcon_MouseUp;
            notifyIcon.MouseDoubleClick += NotifyIcon_DoubleClick;

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
            settingsSubMenu.Font = new Font((string)MpSingletonController.Instance.GetSetting("LogFont"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize"));

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
            systemSubMenu.DropDownItems.Add(ToolStripMenuItemWithHandler("&Preferences",preferences_Click));
            settingsSubMenu.DropDownItems.Add(systemSubMenu);

            notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Pause",toggleActive_Click));
            notifyIcon.ContextMenuStrip.Items.Add(settingsSubMenu);//ToolStripMenuItemWithHandler("&Settings", showSettingsItem_Click));
            notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Help/About",showHelpItem_Click));
            notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Exit",exitItem_Click));
        }
        private async void ShowLoginForm() {
            if(_skipAuth) {
                Console.WriteLine("Skipping auth0 authorization");
                //Init("Test","root");
            }
            else {
                var client = new Auth0Client(new Auth0ClientOptions {
                    Domain = "monkeypaste.auth0.com",
                    ClientId = "mlb0PEtuS7QSKIU7LOKOqOLxDKvs1Fk8"
                });
                var loginResult = await client.LoginAsync(null);//client.LoginAsync();

                if(!loginResult.IsError) {
                    //Init(loginResult.IdentityToken,loginResult.AccessToken);
                }
            }
        }
        private void ShowSettingsForm() {
            if(settingsForm == null || settingsForm.IsDisposed) {
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
            if(helpForm == null) {
                helpForm = new MpHelpForm();
                helpForm.Closed += helpForm_Closed; // avoid reshowing a disposed form
                helpForm.Show();
            }
            else {
                helpForm.Show();
                helpForm.Activate();
            }
        }
        private void _toggleSettingsHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            Console.WriteLine("Pressed settings toggle");

            if(notifyIcon.ContextMenuStrip != null) {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("HideContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon,null);
                notifyIcon.ContextMenuStrip = null;
            }
            else {
                InitTrayMenu();
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon,null);
                notifyIcon.ContextMenuStrip.Items[0].Select();
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
        private void toggleActive_Click(object sender,EventArgs e) { Console.WriteLine("Pause/Resume clicked"); }
        private void openFile_Click(object sender,EventArgs e) { Console.WriteLine("Open File clicked"); }
        private void toggleSaveHistory_Click(object sender,EventArgs e) { Console.WriteLine("Save History clicked"); }
        private void clearHistory_Click(object sender,EventArgs e) {
            Console.WriteLine("Clear History clicked");
            MpSingletonController.Instance.GetMpData().ResetData();
        }
        private void toggleEncrypt_Click(object sender,EventArgs e) {
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
            settingsForm = null;
        }
        private void helpForm_Closed(object sender,EventArgs e) {
            helpForm = null;
        }
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
        private void ContextMenuStrip_Opening(object sender,System.ComponentModel.CancelEventArgs e) { }

        public override void UpdateBounds() {
            throw new NotImplementedException();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}
