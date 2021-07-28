using System;
using System.Windows.Input;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;

namespace MonkeyPaste {
    public class MpContextMenuItemViewModel {
        #region Properties
        public string Title { get; set; }

        public string IconImageResourceName { get; set; }

        public ImageSource IconImageSource { 
            get {
                if(string.IsNullOrEmpty(IconImageResourceName)) {
                    return null;
                }
                if(IsEnabled) {
                    return Application.Current.Resources[IconImageResourceName] as FontImageSource;
                }
                return Application.Current.Resources[IconImageResourceName+"_Disabled"] as FontImageSource;
            }
        }

        public ICommand Command { get; set; }

        public bool IsEnabled {
            get {
                if(Command == null) {
                    return false;
                }
                return Command.CanExecute(null);
            }
        }

        public Color TitleColor {
            get {
                if(IsEnabled) {
                    return Color.Black;
                }
                return Color.DimGray;
            }
        }

        public FontAttributes TitleAttributes {
            get {
                if(IsEnabled) {
                    return FontAttributes.None;
                }
                return FontAttributes.Italic;
            }
        }
        #endregion

        #region Public Methods
        public MpContextMenuItemViewModel() : base() { }

        #endregion

        #region Commands
        public ICommand PerformContextCommand => new Command(
            async () => {
                await PopupNavigation.Instance.PopAllAsync();
                if (Command != null) {
                    Command.Execute(null);
                }
            },
            ()=> {
                return IsEnabled;
            });
        #endregion
    }
}
