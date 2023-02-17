using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMainWindow : Window {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        static MpAvMainWindow() {
            BoundsProperty.Changed.AddClassHandler<MpAvMainWindow>((x, y) => x.BoundsChangedHandler(y as AvaloniaPropertyChangedEventArgs<Rect>));
        }

        #endregion



        #region Properties

        public MpAvMainWindowViewModel BindingContext => MpAvMainWindowViewModel.Instance;

        #endregion

        #region Constructors

        public MpAvMainWindow() {


            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.Activated += MainWindow_Activated;
            this.Deactivated += MainWindow_Deactivated;

        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Overrides

        #endregion

        #region Private Methods
        #region Event Handlers
        private void BoundsChangedHandler(AvaloniaPropertyChangedEventArgs<Rect> e) {
            var oldAndNewVals = e.GetOldAndNewValue<Rect>();
            MpAvMainWindowViewModel.Instance.LastMainWindowRect = oldAndNewVals.oldValue.ToPortableRect();
            MpAvMainWindowViewModel.Instance.ObservedMainWindowRect = oldAndNewVals.newValue.ToPortableRect();
        }

        private void MainWindow_Activated(object? sender, System.EventArgs e) {
            //MpConsole.WriteLine("MainWindow ACTIVATED");
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = true;
        }

        private void MainWindow_Deactivated(object? sender, System.EventArgs e) {
            //MpConsole.WriteLine("MainWindow DEACTIVATED");
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = false;
        }

        #endregion

        #endregion
    }
}
