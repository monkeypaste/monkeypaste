namespace iosKeyboardTest.Android {
    public class AutoCompleteViewModel : ViewModelBase, IKeyboardViewRenderer, IKeyboardRenderSource {

        #region Private Variables
        IKeyboardViewRenderer _renderer;
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IKeyboardViewRenderer Implementation
        public void Layout(bool invalidate) {
            
        }

        public void Measure(bool invalidate) {
            
        }

        public void Paint(bool invalidate) {
            
        }

        public void Render(bool invalidate) {
            
        }

        #endregion

        #region Interfaces

        public void SetRenderer(IKeyboardViewRenderer renderer) {
            _renderer = renderer;
        }
        #endregion

        #endregion

        #region Properties

        #region Members
        IKeyboardViewRenderer Renderer =>
            _renderer;
        #endregion

        #region View Models
        public MenuViewModel Parent { get; private set; }
        #endregion

        #region Appearance
        #endregion

        #region Layout
        #endregion

        #region State
        #endregion

        #region Models
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors
        public AutoCompleteViewModel(MenuViewModel parent) {
            Parent = parent;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
