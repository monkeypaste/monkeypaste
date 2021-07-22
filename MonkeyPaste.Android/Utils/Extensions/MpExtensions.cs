using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Droid {
    public static class MpExtensions {
        #region Java
        public static Task<Java.Lang.Object> ToAwaitableTask(this Android.Gms.Tasks.Task task) {
            var taskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();
            var taskCompleteListener = new TaskCompleteListener(taskCompletionSource);
            task.AddOnCompleteListener(taskCompleteListener);

            return taskCompletionSource.Task;
        }
        #endregion
    }
}