using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpWpfApp {
    public class MpApplication : System.Windows.Application {
        private static readonly Lazy<MpApplication> lazy = new Lazy<MpApplication>(() => new MpApplication());
        public static MpApplication Instance { get { return lazy.Value; } }

        public static MpApplication current;
        public MpSplashFormController SplashFormController { get; set; }
        public MpTaskbarIconController TaskbarController { get; set; } = null;

        // TODO Add NetworkController to gather all db init parameters

        public MpDataModel DataModel { get; set; }

        private bool _authorizeUser = false;

        protected override void OnStartup(System.Windows.StartupEventArgs e) {
            base.OnStartup(e);
            if(MpApplication.current == null) {
                Init();
            }
            MpApplication.current = this;
        }
        public void Init() {
            DataModel = new MpDataModel();
            SplashFormController = new MpSplashFormController(null);
            SplashFormController.ShowSplash();

            InitUI();
            InitDb();
            SplashFormController.HideSplash();

            TaskbarController.PrepareUI();
            DataModel.ClipboardManager.Init();
        }

        private void InitDb() {
            if(_authorizeUser) {
                //first check for internet connection
                if(MpHelperSingleton.Instance.CheckForInternetConnection()) {
                    TaskbarController.ShowLoginForm();
                } else {
                    MessageBox.Show("Error, must be connected to internet to use, exiting");
                    MpAppContext.ExitApp();
                }
            }
            // TODO Add logic to use info from login form
            DataModel.LoadAllData((string)MpRegistryHelper.Instance.GetValue("DBPath"), (string)MpRegistryHelper.Instance.GetValue("DBPassword"));
        }
        private void InitUI() {
            TaskbarController = new MpTaskbarIconController();
        }

        
        //public Rectangle GetScreenWorkingAreaWithMouse() {
        //    foreach(Screen screen in Screen.AllScreens) {
        //        //get cursor pos
        //        WinApi.PointInter lpPoint;
        //        WinApi.GetCursorPos(out lpPoint);
        //        Point mp = (Point)lpPoint;
        //        if(screen.WorkingArea.Contains(mp)) {
        //            return screen.WorkingArea;
        //        }
        //    }
        //    return Screen.FromHandle(Process.GetCurrentProcess().Handle).WorkingArea;
        //}
        //public Rectangle GetScreenBoundsWithMouse() {
        //    foreach(Screen screen in Screen.AllScreens) {
        //        //get cursor pos
        //        WinApi.PointInter lpPoint;
        //        WinApi.GetCursorPos(out lpPoint);
        //        Point mp = (Point)lpPoint;
        //        if(screen.WorkingArea.Contains(mp)) {
        //            return screen.Bounds;
        //        }
        //    }
        //    return Screen.FromHandle(Process.GetCurrentProcess().Handle).Bounds;
        //}
    }
}
