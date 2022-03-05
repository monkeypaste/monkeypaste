using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpWpfApp {
    /// <summary>
    /// An extension of the notifyIcon Windows Forms class, unfortunately its a 
    //  sealed class so it cannot be inherited. This class adds a timer and additional 
    //  methods and events to allow for monitoring when a mouse enters and leaves the icon area. 
    /// </summary>
    /// 
    public class MpNotifyIcon : IDisposable {
        public NotifyIcon targetNotifyIcon;

        public event EventHandler<bool> MouseClick;

        public MpNotifyIcon() {
            // Configure and show a notification icon in the system tray
            targetNotifyIcon = new NotifyIcon();
            targetNotifyIcon.Visible = true;
            targetNotifyIcon.MouseClick += TargetNotifyIcon_MouseClick;
        }

        private void TargetNotifyIcon_MouseClick(object sender, MouseEventArgs e) {
            MouseClick?.Invoke(sender, e.Button == MouseButtons.Right);
        }

        #region IDisposable Members

        /// <summary>
        /// Standard IDisposable interface implementation. If you dont dispose the windows notify icon, the application
        /// closes but the icon remains in the task bar until such time as you mouse over it.
        /// </summary>
        private bool _IsDisposed = false;

        ~MpNotifyIcon() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            // Tell the garbage collector not to call the finalizer
            // since all the cleanup will already be done.
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool IsDisposing) {
            if (_IsDisposed)
                return;

            if (IsDisposing) {
                targetNotifyIcon.Dispose();
            }

            // Free any unmanaged resources in this section
            _IsDisposed = true;

            #endregion
        }
    }
}
