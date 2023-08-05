using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
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
    public partial class MpAvMainWindow : MpAvWindow, MpIMainView {
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

        #region MpIMainView Implementation

        public nint Handle =>
            TryGetPlatformHandle().Handle;

        public void SetPosition(MpPoint p, double scale) {
            Position = p.ToAvPixelPoint(scale);
        }
        #endregion

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
            Mp.Services.ShutdownHelper.ShutdownApp("MainWindow Closed");
        }
        #endregion

        #region Private Methods
        #region Event Handlers
        #endregion

        #endregion
    }
}
