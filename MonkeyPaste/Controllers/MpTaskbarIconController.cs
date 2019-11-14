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
        private static readonly string DefaultTooltip = "Monkey Paste";

        private MpKeyboardHook _toggleSettingsHook,_toggleAppendModeHook;
        private MpMouseHook _mouseHitScreenTopHook,_mouseUpHook;
        private bool _skipAuth = true;

        public MpLogFormController LogFormController { get; set; }

        public MpSettingsForm SettingsForm { get; set; }
        public MpHelpForm HelpForm { get; set; }
        public NotifyIcon NotifyIcon { get; set; }                          // the icon that sits in the system tray      

        public static IntPtr AppHandle;
        
        public MpTaskbarIconController(object context,MpController parent) : base(parent) {            
            MpSingletonController.Instance.Init(context);//,(string)MpSingletonController.Instance.Rh.GetValue("DBPath"),(string)MpSingletonController.Instance.Rh.GetValue("DBPassword"),null,null);

            //client rect of active screen
            Rectangle ascr = MpHelperSingleton.Instance.GetScreenBoundsWithMouse();
            _mouseHitScreenTopHook = new MpMouseHook();
            _mouseHitScreenTopHook.RegisterMouseEvent(MpMouseEvent.HitBox,new Rectangle(0,0,ascr.Width,15));
            _mouseHitScreenTopHook.MouseEvent += _mouseHitScreenTopHook_MouseEvent;

            _mouseUpHook = new MpMouseHook();
            _mouseUpHook.RegisterMouseEvent(MpMouseEvent.UpL);
            _mouseUpHook.MouseEvent += _mouseUpHook_MouseEvent;

            _toggleSettingsHook = new MpKeyboardHook();
            _toggleSettingsHook.RegisterHotKey(ModifierKeys.Alt,Keys.D);
            _toggleSettingsHook.KeyPressed += _toggleSettingsHook_KeyPressed;

            _toggleAppendModeHook = new MpKeyboardHook();
            _toggleAppendModeHook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Shift,Keys.A);
            _toggleAppendModeHook.KeyPressed += _toggleAppendModeHook_KeyPressed;

            HelpForm = new MpHelpForm();
            SettingsForm = new MpSettingsForm();
            InitTrayMenu();

            LogFormController = new MpLogFormController(
                this,
                (string)MpSingletonController.Instance.Rh.GetValue("DBPath"),
                (string)MpSingletonController.Instance.Rh.GetValue("DBPassword")
            );
            LogFormController.ShowLogForm();
            AppHandle = LogFormController.LogForm.Handle;
            //if(MpHelperSingleton.Instance.CheckForInternetConnection()) {
            //    ShowLoginForm();
            //}
            //else {
            //    MessageBox.Show("Error, must be connected to internet to use, exiting");
            //    Exit();
            //}
            Link(new List<MpIView>()/* { helpForm,settingsForm,notifyIcon }*/);
        }
        private void _mouseUpHook_MouseEvent(object sender,Gma.System.MouseKeyHook.MouseEventExtArgs e) {
            if(e.Button == MouseButtons.Left) {
                Console.WriteLine("Mouse up event occured");
            }            
        }
        
        private void _mouseHitScreenTopHook_MouseEvent(object sender,Gma.System.MouseKeyHook.MouseEventExtArgs e) {
            LogFormController.ShowLogForm();
        }

        private void _toggleAppendModeHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            MpSingletonController.Instance.InAppendMode = !MpSingletonController.Instance.InAppendMode;
            if(MpSingletonController.Instance.InAppendMode) {
                NotifyIcon.BalloonTipText = "Append mode activated";
                NotifyIcon.ShowBalloonTip(3000);
            } else {
                MpSingletonController.Instance.AppendItem = null;
                NotifyIcon.BalloonTipText = "Append mode deactivated";
                NotifyIcon.ShowBalloonTip(3000);
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
            NotifyIcon.Visible = false; // should remove lingering tray icon
            MpSingletonController.Instance.ExitApplication();            
        }
        private void InitTrayMenu() {
            NotifyIcon = new NotifyIcon() {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = MpHelperSingleton.Instance.GetIconFromBitmap(Properties.Resources.monkey3),
                Text = DefaultTooltip,
                Visible = true
            };
            NotifyIcon.MouseUp += NotifyIcon_MouseUp;
            NotifyIcon.MouseDoubleClick += NotifyIcon_DoubleClick;

            NotifyIcon.ContextMenuStrip.Items.Clear();
            NotifyIcon.BalloonTipText = "Howdy there";
            NotifyIcon.ShowBalloonTip(30000);
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

            NotifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Pause",_toggleActive_Click));
            NotifyIcon.ContextMenuStrip.Items.Add(settingsSubMenu);
            NotifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Help/About",showHelpItem_Click));
            NotifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            NotifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Exit",exitItem_Click));
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
                var loginResult = await client.LoginAsync(null);//client.LoginAsync();

                if(!loginResult.IsError) {
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
        private void _toggleSettingsHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            Console.WriteLine("Pressed settings toggle");

            if(NotifyIcon.ContextMenuStrip != null) {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("HideContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(NotifyIcon,null);
                NotifyIcon.ContextMenuStrip = null;
            }
            else {
                InitTrayMenu();
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(NotifyIcon,null);
                NotifyIcon.ContextMenuStrip.Items[0].Select();
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
            MpLogFormController.Db.ResetDb();
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
                mi.Invoke(NotifyIcon,null);
            }
        }
        private void NotifyIcon_DoubleClick(object sender,MouseEventArgs e) {
            LogFormController.ToggleLogForm();
        }
        private void ContextMenuStrip_Closing(object sender,ToolStripDropDownClosingEventArgs e) {
            NotifyIcon.ContextMenuStrip.Hide();
            NotifyIcon.ContextMenuStrip = null;
        }
        private void ContextMenuStrip_Opening(object sender,System.ComponentModel.CancelEventArgs e) { }

        public override void Update() {
            throw new NotImplementedException();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}
