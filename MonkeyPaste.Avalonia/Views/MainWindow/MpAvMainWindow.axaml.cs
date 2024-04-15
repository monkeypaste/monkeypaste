using Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMainWindow : MpAvWindow {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        static MpAvMainWindow() {
            //BoundsProperty.Changed.AddClassHandler<MpAvMainWindow>((x, y) => x.BoundsChangedHandler(y as AvaloniaPropertyChangedEventArgs<Rect>));
        }

        #endregion

        #region Interfaces
        #endregion

        #region Properties

        public override MpAvMainWindowViewModel BindingContext => MpAvMainWindowViewModel.Instance;

        #endregion

        #region Constructors

        public MpAvMainWindow() {
            InitializeComponent();
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Overrides
        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.MainWindowClosed, "MainWindow Close");
        }
        #endregion

        #region Private Methods
        #region Event Handlers
        #endregion

        #endregion
    }
}
