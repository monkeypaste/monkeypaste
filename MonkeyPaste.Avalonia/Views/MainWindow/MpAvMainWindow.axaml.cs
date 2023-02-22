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
    public interface MpIMainView : MpIHasSettableDataContext {
        bool IsActive { get; }
        nint Handle { get; }
        void Show();
        void Hide();
        void SetPosition(MpPoint p, double scale);
    }

    [DoNotNotify]
    public partial class MpAvMainWindow : Window, MpIMainView {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        static MpAvMainWindow() {
            BoundsProperty.Changed.AddClassHandler<MpAvMainWindow>((x, y) => x.BoundsChangedHandler(y as AvaloniaPropertyChangedEventArgs<Rect>));
        }

        #endregion

        #region Interfaces

        #region MpIMainView Implementation

        public nint Handle =>
            PlatformImpl.Handle.Handle;

        public void SetPosition(MpPoint p, double scale) {
            Position = p.ToAvPixelPoint(scale);
        }
        #endregion

        #endregion

        #region Properties

        public MpAvMainWindowViewModel BindingContext => MpAvMainWindowViewModel.Instance;

        #endregion

        #region Constructors

        public MpAvMainWindow() {
            //App.MainView = this;

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
