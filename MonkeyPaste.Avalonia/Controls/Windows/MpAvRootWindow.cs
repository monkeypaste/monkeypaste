using Avalonia.Controls;
using PropertyChanged;

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
        }
        #endregion

        #region Public Methods
        public void AddChild(MpAvChildWindow cw) {
            if(ContentCanvas.Children.Contains(cw)) {
                return;
            }
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
