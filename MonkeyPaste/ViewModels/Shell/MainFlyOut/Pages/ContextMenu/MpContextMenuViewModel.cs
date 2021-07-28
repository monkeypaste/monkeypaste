using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpContextMenuViewModel : MpViewModelBase {
        #region Properties

        public double Width { get; set; }

        public double MinWidth { get; set; } = 150;

        public double ItemWidth {
            get {
                return Width - Padding.Left - Padding.Right;
            }
        }

        public double FontSize { get; set; } = 14;

        public double IconSize { get; set; } = 25;

        public Thickness Padding { get; set; } = new Thickness(5);

        public double Height { get; set; }

        public double ItemHeight { get; set; } = 30;

        public ObservableCollection<MpContextMenuItemViewModel> Items { get; set; } = new ObservableCollection<MpContextMenuItemViewModel>();

        #endregion        

        #region Public Methods

        public MpContextMenuViewModel() : base() {
            Items.CollectionChanged += Items_CollectionChanged;        
        }

        #endregion

        #region Private Methods

        private int GetMaxItemTitleLength() {
            int maxLength = -1;
            foreach(var cmivm in Items) {
                if(!string.IsNullOrEmpty(cmivm.Title)) {
                    if(cmivm.Title.Length > maxLength) {
                        maxLength = cmivm.Title.Length;
                    }
                }
            }
            return maxLength;
        }

        #region Event Handlers
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //not sure why i need to do below but if more than 1 item it will not be
            //sized correctly
            int count = Items.Count > 1 ? Items.Count + 1 : Items.Count;
            Height = (count * ItemHeight) + Padding.Top + Padding.Bottom;
            Width = 0;
            if(Items.Any(x=>x.IconImageSource != null)) {
                Width += IconSize;
            }
            if(Items.Any(x=>x is MpColorChooserContextMenuItemViewModel)) {
                Height = ((count-1) * ItemHeight) + Padding.Top + Padding.Bottom;
                Height += (12 * 5);
                Width = (12 * 14);
            } else {
                Width += (GetMaxItemTitleLength() * FontSize) + Padding.Left + Padding.Right;
                Width = Math.Max(Width, MinWidth);
            }            
        }        
        #endregion

        #endregion
    }
}
