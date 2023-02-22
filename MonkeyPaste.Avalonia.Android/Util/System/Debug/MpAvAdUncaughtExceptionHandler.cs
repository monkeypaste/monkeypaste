using Android.Runtime;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Java.Lang.Thread;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdUncaughtExceptionHandler : Java.Lang.Object, IUncaughtExceptionHandler {
        private static MpAvAdUncaughtExceptionHandler _instance;
        public static MpAvAdUncaughtExceptionHandler Instance => _instance ?? (_instance = new MpAvAdUncaughtExceptionHandler());

        public void Init() {
            AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidEnvironmentUnhandledExceptionRaiser;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;

            if (DefaultUncaughtExceptionHandler is not MpAvAdUncaughtExceptionHandler) {
                DefaultUncaughtExceptionHandler = Instance;
            }
            //var currentHandler = DefaultUncaughtExceptionHandler;
            //var exceptionHandler = currentHandler as MpAvAdUncaughtExceptionHandler;
            //if (exceptionHandler != null) {
            //    exceptionHandler.SetHandler(HandleUncaughtException);
            //} else {
            //    Java.Lang.Thread.DefaultUncaughtExceptionHandler = new MpAvAdUncaughtExceptionHandler(currentHandler, HandleUncaughtException);
            //}
        }

        private void OnAndroidEnvironmentUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e) {
            AndroidEnvironment.UnhandledExceptionRaiser -= OnAndroidEnvironmentUnhandledExceptionRaiser;

            MpConsole.WriteTraceLine($"AndroidEnvironment.UnhandledExceptionRaiser.", e.Exception);
            e.Handled = true;
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;

            var ex = e.ExceptionObject as Exception;
            if (ex != null) {
                MpConsole.WriteTraceLine("AppDomain.CurrentDomain.UnhandledException.", ex);
            } else {
                MpConsole.WriteTraceLine($"AppDomain.CurrentDomain.UnhandledException. ---> {e.ExceptionObject}");
            }
        }

        private void OnTaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
            MpConsole.WriteTraceLine("TaskScheduler.UnobservedTaskException.", e.Exception);
        }

        private bool HandleUncaughtException(Java.Lang.Throwable ex) {
            MpConsole.WriteTraceLine("Thread.DefaultUncaughtExceptionHandler.", ex);
            return true;
        }

        public void UncaughtException(Java.Lang.Thread t, Java.Lang.Throwable e) {
            HandleUncaughtException(e);
        }
    }
}
