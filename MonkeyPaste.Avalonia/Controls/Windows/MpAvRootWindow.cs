using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using PropertyChanged;
using ReactiveUI;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvRootWindow : Window {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvRootWindow _instance;
        public static MpAvRootWindow Instance => _instance ?? (_instance = new MpAvRootWindow());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        Canvas ContentCanvas =>
            Content as Canvas;
        #endregion

        #region Constructors
        public MpAvRootWindow() {
            Content = new Canvas();
            SystemDecorations = SystemDecorations.Full;
        }
        #endregion

        #region Public Methods
        public void AddChild(MpAvChildWindow cw) {
            if(ContentCanvas.Children.Contains(cw)) {
                return;
            }

            cw.Bind(
                Canvas.LeftProperty,
                new Binding() {
                    Source = cw,
                    Path = nameof(cw.CanvasX),
                    Mode = BindingMode.OneWay
                });
            
            cw.Bind(
                Canvas.TopProperty,
                new Binding() {
                    Source = cw,
                    Path = nameof(cw.CanvasY),
                    Mode = BindingMode.OneWay
                });

            ContentCanvas.Children.Add(cw);
        }
        
        public bool RemoveChild(MpAvChildWindow cw) {
            return ContentCanvas.Children.Remove(cw);
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
