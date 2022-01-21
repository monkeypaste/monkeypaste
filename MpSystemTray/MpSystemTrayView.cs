using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpSystemTray {
    public interface MpISystemTrayMenuItem {
        Icon GetIcon();
        string GetHeader();
        void OnSystemTrayItemClicked(string header);
    }

    public interface MpISystemTray {
        Icon GetIcon();

        string GetAppStatus();
        string GetAccountStatus();
        string GetTotalItemCountLabel();
        string GetDbSizeLabel();
        string GetToolTipText();

        void OnLeftClick();
        void OnRightClick();
        void OnDoubleClick();

        MpISystemTrayMenuItem[] MenuItems { get; }
    }

    public class MpSystemTrayBalloon {
        public string Title { get; set; }

    }

    public class MpSystemTrayView : Form {
        protected MpISystemTray _sysTrayData;
        protected NotifyIcon notifyIcon;
        private System.ComponentModel.IContainer components = null;


        private static MpSystemTrayView _instance;

        public static void Start(MpISystemTray sysTrayData) {
            // we can only have one instance if this class
            if (_instance != null) {
                return;
            }

            var t = new Thread(new ParameterizedThreadStart(x => Application.Run(new MpSystemTrayView(sysTrayData))));
            t.SetApartmentState(ApartmentState.STA); // give the [STAThread] attribute
            t.Start();
        }

        public static void ShowBalloon(ToolTipIcon icon, string msg, string title, int timeout = 1000) {
            _instance.notifyIcon.BalloonTipIcon = icon;
            _instance.notifyIcon.BalloonTipText = msg;
            _instance.notifyIcon.BalloonTipTitle = title;

            _instance.notifyIcon.ShowBalloonTip(timeout);
        }

        private MpSystemTrayView(MpISystemTray sysTrayData) {
            _sysTrayData = sysTrayData;
        }

        private void Init() {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MpSystemTrayView));
            notifyIcon = new NotifyIcon(components);

            SuspendLayout();

            notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            notifyIcon.BalloonTipText = "Veni vidi vici";
            notifyIcon.BalloonTipTitle = "Balloon title";
            notifyIcon.Icon = _sysTrayData.GetIcon();
            notifyIcon.Text = _sysTrayData.GetToolTipText();            
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            ResumeLayout(false);
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Left) {
                _sysTrayData.OnLeftClick();
            } else {
                _sysTrayData.OnRightClick();
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                _sysTrayData.OnDoubleClick();
            }            
        }

        public static void Stop() {
            _instance.Invoke(new System.Windows.Forms.MethodInvoker(_instance.Close));

            _instance.Dispose();

            _instance = null;
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
