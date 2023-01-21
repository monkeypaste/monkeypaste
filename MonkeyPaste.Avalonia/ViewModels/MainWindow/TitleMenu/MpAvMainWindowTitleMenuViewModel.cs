using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvMainWindowTitleMenuViewModel : MpViewModelBase {
        #region Statics

        private static MpAvMainWindowTitleMenuViewModel _instance;
        public static MpAvMainWindowTitleMenuViewModel Instance => _instance ?? (_instance = new MpAvMainWindowTitleMenuViewModel());

        #endregion

        #region Properties

        #region Appearance


        public string ZoomSliderHexColor {
            get {
                if (IsZoomSliderHovering) {
                    return MpSystemColors.Yellow;
                }
                return MpSystemColors.White;
            }
        }

        #endregion

        #region Layout
        public double TitleDragHandleShortLength => 7;
        public double TitleDragHandleLongLength => 103;
        public double DefaultTitleMenuFixedLength => 20;
        public double TitleMenuWidth { get; set; }
        public double TitleMenuHeight { get; set; }

        public double SettingsButtonWidth { get; set; }
        public double LockButtonWidth { get; set; }
        public double ZoomSliderWidth { get; set; }
        public double LayoutButtonWidth { get; set; }
        public double OrientationButtonWidth { get; set; }

        #endregion

        #region State
        public bool IsZoomSliderHovering { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvMainWindowTitleMenuViewModel() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            PropertyChanged += MpAvTitleMenuViewModel_PropertyChanged;
        }

        #endregion

        #region Private Methods

        private void MpAvTitleMenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {

            //}
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowActivated:
                case MpMessageType.MainWindowDeactivated:
                case MpMessageType.MainWindowOpened:
                    //OnPropertyChanged(nameof(TitleBarBackgroundHexColor));
                    break;
            }
        }


        #endregion
    }
}
