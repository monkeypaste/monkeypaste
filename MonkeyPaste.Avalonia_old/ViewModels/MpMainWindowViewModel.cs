using System;
using System.Collections.Generic;
using System.Text;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {

    public enum MpMainWindowOrientationType { 
        Left,
        Right,
        Bottom,
        Top
    };
    public class MpMainWindowViewModel : MpViewModelBase {
        #region Properties

        #region Layout

        public double MainWindowWidth { get; set; }
        public double MainWindowHeight { get; set; }

        public double MainWindowLeft { get; set; }
        public double MainWindowRight { get; set; }
        public double MainWindowTop { get; set; }
        public double MainWindowBottom { get; set; }

        public MpRect MainWindowOpenRect {
            get {
                switch(MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Bottom:
                        return new MpRect();
                }
                return null;
            }
        }

        #endregion

        #region State

        public MpMainWindowOrientationType MainWindowOrientationType { get; set; }

        #endregion

        #endregion

    }

}
