﻿using MonkeyPaste.Common;

namespace MonkeyPaste {
    public abstract class MpDeviceWrapper : MpIDeviceWrapper {
        #region Statics

        protected static MpDeviceWrapper _instance;
        public static MpDeviceWrapper Instance =>
            _instance;

        #endregion

        #region Interfaces

        #region MpIDeviceWrapper Implementation
        public abstract MpIPlatformInfo PlatformInfo { get; set; }
        public abstract MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }

        #endregion
        #endregion

        #region Public Methods

        public abstract void CreateDeviceInstance(object args);

        #endregion


    }
}