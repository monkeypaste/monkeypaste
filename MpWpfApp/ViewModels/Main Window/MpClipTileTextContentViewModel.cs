using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpClipTileTextContentViewModel : MpViewModelBase {

        #region Properties
        private string _content = string.Empty;
        public string Content {
            get {
                return _content;
            }
            set {
                if (_content != value) {
                    _content = value;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }


        #endregion

        #region Public Methods
        public MpClipTileTextContentViewModel() {

        }

        public void ClipTileTextContent_Loaded(object sender, RoutedEventArgs e) {
            var cttcuie = (UIElement)sender;

        }
        #endregion
    }
}
