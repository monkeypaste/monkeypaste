using Avalonia;
using DynamicData;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace iosKeyboardTest.Android {
    public enum MenuPageType {
        None = 0,
        TabSelector,
        Completions,
        OtherTab
    }
    public class MenuViewModel : ViewModelBase, IKeyboardViewRenderer, IKeyboardRenderSource {

        #region Private Variables
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
            RaisePropertyChanged(nameof(MenuRect));
        }

        public void Paint(bool invalidate) {
        }

        public void Render(bool invalidate) {
        }

        #endregion
        #region IKeyboardRenderSource Implementation
        public void SetRenderer(IKeyboardViewRenderer renderer) {
            _renderer = renderer;
        }
        #endregion
        #endregion

        #region Properties

        #region Members
        IKeyboardViewRenderer _renderer;
        IKeyboardViewRenderer Renderer =>
            _renderer ?? this;
        IKeyboardInputConnection InputConnection { get; set; }
        #endregion

        #region View Models
        public KeyboardViewModel Parent { get; private set; }
        public ObservableCollection<string> CompletionItems { get; private set; } = [];

        #endregion

        #region Appearance
        #endregion

        #region Layout
        public Rect MenuRect =>
            Parent.MenuRect;
        double ButtonWidthRatio => 0.1;
        public Rect BackButtonRect {
            get {
                if(!IsBackButtonVisible) {
                    return new();
                }
                double x = 0;
                double y = 0;
                double w = MenuRect.Width * ButtonWidthRatio;
                double h = MenuRect.Height;
                return new Rect(x, y, w, h);
            }
        }
        public Rect OptionsButtonRect {
            get {
                double w = MenuRect.Width * ButtonWidthRatio;
                double h = MenuRect.Height;
                double x = MenuRect.Width - w;
                double y = 0;
                return new Rect(x, y, w, h);
            }
        }
        
        public Rect InnerMenuRect {
            get {
                double w = MenuRect.Width - OptionsButtonRect.Width - BackButtonRect.Width;
                double h = MenuRect.Height;
                double x = BackButtonRect.Right;
                double y = 0;
                return new Rect(x, y, w, h);
            }
        }
        #endregion

        #region State
        MenuPageType MenuPageType { get; set; } = MenuPageType.TabSelector;
        string LastInput { get; set; }
        bool IsBackButtonVisible =>
            MenuPageType != MenuPageType.TabSelector; 
        #endregion

        #region Models
        #endregion

        #endregion

        #region Events
        public event EventHandler<string> OnAutoCorrectAvailable;
        public event EventHandler OnScrollToHomeRequested;
        #endregion

        #region Constructors
        public MenuViewModel(KeyboardViewModel parent) {
            Parent = parent;
        }

        public void SetInputConnection(IKeyboardInputConnection conn) {
            InputConnection = conn;

            Task.Run(() => {
                TextCorrector.Init(InputConnection.AssetLoader);

            }).FireAndForgetSafeAsync();
        }
        #endregion

        #region Public Methods
        public void ShowCompletion(string input) {
            Task.Run(() => {
                if (input == LastInput ||
                    !TextCorrector.IsLoaded) {
                    return;
                }
                LastInput = input;
                CompletionItems.Clear();
                if(string.IsNullOrEmpty(input) && !Parent.IsNextWordCompletionEnabled) {
                    // don't do beginning of word
                    return;
                }
                MenuPageType = MenuPageType.Completions;
                var results = TextCorrector.GetResults(input, Parent.IsAutoCorrectEnabled, Parent.MaxCompletionResults, out string autoCorrectResult);
                if (Parent.IsAutoCorrectEnabled && !string.IsNullOrEmpty(autoCorrectResult)) {
                    // TODO this should be picked up by keyboard i think
                    OnAutoCorrectAvailable?.Invoke(this, autoCorrectResult);
                }
                CompletionItems.AddRange(results);
                InputConnection.MainThread.Post(() => {
                    OnScrollToHomeRequested?.Invoke(this, EventArgs.Empty);
                    this.Renderer.Render(true);
                });
            });
        }

        public void GoBack() {
            MenuPageType back_to_page = MenuPageType;
            switch (MenuPageType) {
                case MenuPageType.Completions:
                    back_to_page = MenuPageType.TabSelector;
                    break;
                case MenuPageType.OtherTab:
                    back_to_page = MenuPageType.TabSelector;
                    break;
            }
            if(back_to_page == MenuPageType) {
                return;
            }
            MenuPageType = back_to_page;
            this.Renderer.Render(true);
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
