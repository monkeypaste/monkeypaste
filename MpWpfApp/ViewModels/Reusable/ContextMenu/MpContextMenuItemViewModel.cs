using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {

    public interface MpIContextMenuItemViewModel {
        MpContextMenuItemViewModel MenuItemViewModel { get; }
    }

    public class MpContextMenuItemViewModel : MpViewModelBase {
        #region Properties

        public bool IsPasteToPathRuntimeItem { get; set; }

        public bool IsSeparator { get; set; }

        public bool IsColorPallete { get; set; }

        public bool IsSelected { get; set; } = false;

        public bool IsPartiallySelected { get; set; } = false; // for multi-select tag ischecked overlay

        public bool IsHovering { get; set; }

        public bool CanHide { get; set; } // for eye button on paste to path

        public bool IsVisible { get; set; } = true;

        public string BorderHexColor {
            get {
                Brush b = Brushes.DarkGray;
                if(IsSelected) {
                    b = Brushes.Red;
                } else if(IsHovering) {
                    b = Brushes.DimGray;
                }
                return b.ToHex();
            }
        }
        //public Visibility MenuItemVisibility { get; set; }

        public string Header { get; set; }

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public string InputGestureText { get; set; }

        //public string IconSource { get; set; }

        //public Brush IconBackgroundBrush { get; set; } = Brushes.Transparent;

        //public Image Icon { get; set; }

        public int IconId { get; set; } = 0;

        public string IconResourceKey { get; set; } = string.Empty;

        public string IconHexStr { get; set; } = string.Empty;

        public IList<MpContextMenuItemViewModel> SubItems { get; set; }

        #endregion

        #region Public Methods

        public MpContextMenuItemViewModel() : base(null)  {
            PropertyChanged += MpContextMenuItemViewModel_PropertyChanged;
            //IsSeparator = true;
        }
        
        //public MpContextMenuItemViewModel(
        //    string header, 
        //    ICommand command,
        //    object commandParameter,
        //    bool? isChecked,
        //    string iconSource = "",
        //    ObservableCollection<MpContextMenuItemViewModel> subItems = null,
        //    string inputGestureText = "",
        //    Brush bgBrush = null,
        //    BitmapSource bmpSrc = null) : this() {
        //    IsSeparator = false;

        //    Header = header;
        //    Command = command;
        //    CommandParameter = commandParameter;
        //    IsChecked = isChecked;
        //    if(bmpSrc == null) {
        //        IconSource = iconSource;
        //    } else {
        //        Icon = new Image();
        //        Icon.Source = bmpSrc;
        //        Icon.Stretch = Stretch.Fill;
        //    }
            
        //    SubItems = subItems ?? new ObservableCollection<MpContextMenuItemViewModel>();
        //    InputGestureText = inputGestureText;
        //    IconBackgroundBrush = bgBrush == null ? Brushes.Transparent : bgBrush;
        //}

        
        #endregion

        #region Private Methods

        private void MpContextMenuItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                //case nameof(IconSource):
                //    if (!string.IsNullOrEmpty(IconSource)) {
                //        var icon = new Image();
                //        if (!IconSource.IsBase64String()) {
                //            icon.Source = (BitmapSource)new BitmapImage(new Uri(IconSource));

                //        } else {
                //            icon.Source = IconSource.ToBitmapSource();
                //            //icon.Height = icon.Width = 20;
                //        }
                //        Icon = icon;
                //        Icon.Stretch = Stretch.Fill;
                //    }
                //    break;
                //case nameof(IconBackgroundBrush):
                //    if (IconBackgroundBrush != null) {
                //        var bgBmp = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/texture.png"));
                //        bgBmp = MpWpfImagingHelper.TintBitmapSource(bgBmp, ((SolidColorBrush)IconBackgroundBrush).Color, false);
                //        var borderBmp = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/textureborder.png"));
                //        if (!MpWpfColorHelpers.IsBright((IconBackgroundBrush as SolidColorBrush).Color)) {
                //            borderBmp = MpWpfImagingHelper.TintBitmapSource(borderBmp, Colors.White, false);
                //        }
                //        var icon = new Image();
                //        icon.Source = MpWpfImagingHelper.MergeImages(new List<BitmapSource> { bgBmp, borderBmp });
                //        if (!IsChecked.HasValue || IsChecked.Value) {
                //            string checkPath = !IsChecked.HasValue ? @"/Images/check_partial.png" : @"/Images/check.png";
                //            var checkBmp = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + checkPath));
                //            if (!MpWpfColorHelpers.IsBright((IconBackgroundBrush as SolidColorBrush).Color)) {
                //                checkBmp = MpWpfImagingHelper.TintBitmapSource(checkBmp, Colors.White, false);
                //            }
                //            icon.Source = MpWpfImagingHelper.MergeImages(new List<BitmapSource> { (BitmapSource)icon.Source, checkBmp });
                //        }
                //        Icon = icon;
                //    }
                //    break;
            }
        }


        #endregion
    }
}
