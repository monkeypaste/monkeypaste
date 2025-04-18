﻿using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste.Avalonia {

    public partial class MpAvProcessWatcher : MpIProcessWatcher {
        #region Private Variables
#if DESKTOP
        private DispatcherTimer _timer; 
#endif

#if MAC
        //private int _lastActiveWindowNum;
#endif
        #endregion

        #region Protected Variables
        protected nint _lastActiveHandle;
        #endregion

        #region Constants

        protected const int POLL_INTERVAL_MS = 300;
        #endregion

        #region Statics

        private static MpAvProcessWatcher _instance;
        public static MpAvProcessWatcher Instance => _instance;

        #endregion

        #region Interfaces

        #region MpIActionComponent Implementation

        void MpIActionComponent.RegisterActionComponent(MpIInvokableAction mvm) {
            if (OnAppActivated.HasInvoker(mvm)) {
                return;
            }
            OnAppActivated += mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnAppActivated)} Registered {mvm.Label}");
        }

        void MpIActionComponent.UnregisterActionComponent(MpIInvokableAction mvm) {
            if (!OnAppActivated.HasInvoker(mvm)) {
                return;
            }
            OnAppActivated -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnAppActivated)} Unregistered {mvm.Label}");
        }
        #endregion

        #endregion

        #region Properties
        public bool IsWatching =>
#if MOBILE
            false;
#else
            _timer != null && _timer.IsEnabled; 
#endif

        private nint _thisAppHandle = IntPtr.Zero;
        protected nint ThisAppHandle {
            get {
                if (_thisAppHandle == IntPtr.Zero) {
                    _thisAppHandle = GetThisAppHandle();
                }
                return _thisAppHandle;
            }
        }

        private MpPortableProcessInfo _thisAppProcessInfo;
        public MpPortableProcessInfo ThisAppProcessInfo =>
            _thisAppProcessInfo ?? (_thisAppProcessInfo = GetProcessInfoByHandle(ThisAppHandle));

        private MpPortableProcessInfo _lastProcessInfo;
        public MpPortableProcessInfo LastProcessInfo {
            get {
                if (_lastProcessInfo == null) {
                    if (_lastActiveHandle == IntPtr.Zero) {
                        return ThisAppProcessInfo;
                    }
                    _lastProcessInfo = GetProcessInfoByHandle(_lastActiveHandle);
                    if (_lastProcessInfo == null) {
                        return ThisAppProcessInfo;
                    }
                }
                if (_lastProcessInfo.Handle != _lastActiveHandle) {
                    if (IsProcessPathEqual(_lastProcessInfo.Handle, _lastActiveHandle)) {
                        // only upate new handle to reduce processing
                        _lastProcessInfo.Handle = _lastActiveHandle;
                    } else {
                        // active just changed, refresh info
                        _lastProcessInfo = GetProcessInfoByHandle(_lastActiveHandle);
                    }
                }
                return _lastProcessInfo;
            }
        }
        public bool BreakNextTick { get; set; }

        #endregion

        #region Events


        public event EventHandler<MpPortableProcessInfo> OnAppActivated;

        #endregion

        #region Constructors

        public MpAvProcessWatcher() {
            _instance = this;
#if LINUX
            Init();
#endif
            StartWatcher();
        }

        #endregion

        #region Public Methods

        public void StartWatcher() {
#if MOBILE
            return;
#else
             if (_timer == null) {
                // initial start

                _timer = new DispatcherTimer(DispatcherPriority.Background) {
                    Interval = TimeSpan.FromMilliseconds(POLL_INTERVAL_MS)
                };
                _timer.Tick += ProcessWatcherTimer_tick;
            } else {
                _timer.Stop();
            }
            _timer.Start();
#endif
        }
        public void StopWatcher() {
#if MOBILE
            return;
#else
            _timer?.Stop(); 
#endif
        }

        public MpPortableProcessInfo GetProcessInfoFromScreenPoint(MpPoint pixelPoint) {
            var handle = GetParentHandleAtPoint(pixelPoint);
            return GetProcessInfoByHandle(handle);
        }
        public MpPortableProcessInfo GetProcessInfoFromHandle(nint handle) {
            return GetProcessInfoByHandle(handle);
        }



        #endregion

        #region Protected Methods
        protected virtual MpPortableProcessInfo GetProcessInfoByHandle(nint handle, MpIconSize iconSize = MpIconSize.ExtraLargeIcon128) {
            if (handle == IntPtr.Zero) {
                return null;
            }
            var ppi = new MpPortableProcessInfo() {
                Handle = handle,
                ProcessPath = GetProcessPath(handle),
                ApplicationName = GetAppNameByProessPath(GetProcessPath(handle)),
                MainWindowTitle = GetProcessTitle(handle)
            };
            ppi.MainWindowIconBase64 = 
                iconSize == MpIconSize.None ? null : Mp.Services.IconBuilder.GetPathIconBase64(ppi.ProcessPath, ppi.Handle, iconSize);
            return ppi;
        }
        protected virtual void ProcessWatcherTimer_tick(object sender, EventArgs e) {
            if (BreakNextTick) {
                BreakNextTick = false;
                MpDebug.BreakAll();
            }
            nint activeHandle = GetActiveProcessHandle();
            if (activeHandle == IntPtr.Zero ||
                activeHandle == _lastActiveHandle) {
                return;
            }

            if (IsProcessPathEqual(ThisAppHandle, activeHandle)) {
                // when this app is active ignore update
                return;
            }
            if (!IsHandleWindowProcess(activeHandle)) {
                // some weird process, ignore
                return;
            }

            // should be valid window process here
            nint prevActiveHandle = _lastActiveHandle;
            _lastActiveHandle = activeHandle;

            if (IsProcessPathEqual(prevActiveHandle, activeHandle)) {
                // ignore inner-process window changes not relevant for this event
                return;
            }
            MpConsole.WriteLine($"Active Window Changed: {LastProcessInfo}");
            OnAppActivated?.Invoke(this, LastProcessInfo);
        }

        #endregion

        #region Private Methods
        public bool IsProcessPathEqual(MpPortableProcessInfo p1, MpPortableProcessInfo p2) {
            return IsProcessPathEqual(p1.Handle, p2.Handle);
        }
        protected bool IsProcessPathEqual(nint h1, nint h2) {
            return GetProcessPath(h1) == GetProcessPath(h2);
        }

        #endregion
    }
}

