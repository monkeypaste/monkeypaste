using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;

namespace MonkeyPaste {

    public interface MpIMenuItemViewModel {
        MpMenuItemViewModel MenuItemViewModel { get; }
    }

    public class MpMenuItemViewModel : MpViewModelBase {
        #region Properties

        public bool IsPasteToPathRuntimeItem { get; set; }

        public bool IsSeparator { get; set; }

        public bool IsHeaderedSeparator { get; set; }

        public int HeaderIndentLevel { get; set; }

        public bool IsColorPallete { get; set; }

        public bool IsSelected { get; set; } = false;

        public bool IsPartiallySelected { get; set; } = false; // for multi-select tag ischecked overlay

        public bool IsHovering { get; set; }

        public bool CanHide { get; set; } // for eye button on paste to path

        public bool IsVisible { get; set; } = true;

        public string BorderHexColor {
            get {
                if(IsSelected) {
                    return MpSystemColors.IsSelectedBorderColor;
                } else if(IsHovering) {
                    return MpSystemColors.IsHoveringBorderColor;
                }
                return MpSystemColors.DarkGray;
            }
        }
        //public Visibility MenuItemVisibility { get; set; }

        public string Header { get; set; }

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public string InputGestureText { get; set; }

        public int ShortcutId { get; set; }

        //public string IconSource { get; set; }

        //public Brush IconBackgroundBrush { get; set; } = Brushes.Transparent;

        //public Image Icon { get; set; }

        public int IconId { get; set; } = 0;

        public string IconResourceKey { get; set; } = string.Empty;

        public string IconHexStr { get; set; } = string.Empty;

        public IList<MpMenuItemViewModel> SubItems { get; set; }

        #endregion

        #region Public Methods

        public static MpMenuItemViewModel GetColorPalleteMenuItemViewModel(MpIUserColorViewModel ucvm) {
            bool isAnySelected = false;
            var colors = new List<MpMenuItemViewModel>();
            string selectedHexStr = ucvm.GetColor();
            for (int i = 0; i < MpSystemColors.ContentColors.Count; i++) {
                string cc = MpSystemColors.ContentColors[i].ToUpper();
                bool isCustom = i == MpSystemColors.ContentColors.Count - 1;
                bool isSelected = selectedHexStr.ToUpper() == cc;
                if (isSelected) {
                    isAnySelected = true;
                }
                ICommand command = null;
                object commandArg = null;
                string header = cc;
                if (isCustom) {
                    if (!isAnySelected) {
                        isSelected = true;
                        // if selected color is custom make background of custom icon that color (default white)
                        header = selectedHexStr;
                    }
                    command = MpNativeWrapper.GetCustomColorChooserMenu().SelectCustomColorCommand;
                    commandArg = ucvm;
                } else {
                    command = ucvm.SetColorCommand;
                    commandArg = cc;
                }
                colors.Add(new MpMenuItemViewModel() {
                    IsSelected = isSelected,
                    Header = header,
                    Command = command,
                    CommandParameter = commandArg,
                    IsVisible = isCustom
                });
            }
            
            return new MpMenuItemViewModel() {
                IsColorPallete = true,
                SubItems = colors
            };
        }

        public MpMenuItemViewModel() : base(null)  {
            PropertyChanged += MpContextMenuItemViewModel_PropertyChanged;
            //IsSeparator = true;
        }
        
        //public MpMenuItemViewModel(
        //    string header, 
        //    ICommand command,
        //    object commandParameter,
        //    bool? isChecked,
        //    string iconSource = "",
        //    ObservableCollection<MpMenuItemViewModel> subItems = null,
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
            
        //    SubItems = subItems ?? new ObservableCollection<MpMenuItemViewModel>();
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
