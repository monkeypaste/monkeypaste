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
    public enum MpContextMenuType {
        None = 0,
        ContentItem,
        Tag,
        TextEditor
    }

    public enum MpContextMenuItemsSourceType {
        None = 0,
        ContentItems,
        Tags,
        Analyzers,
        Matchers
    }

    public interface MpIContextMenuItemViewModel {
        Dictionary<string,ObservableCollection<MpContentItemViewModel>> ContextMenuItemsLookup { get; }
    }

    public class MpContextMenuItemViewModel : MpViewModelBase<object> {
        #region Properties
        public MenuItem MenuItem {
            get {
                var mi = new MenuItem() {
                    Header = this.Header,
                    Icon = this.Icon,
                    InputGestureText = this.InputGestureText,
                    Command = this.Command,
                    CommandParameter = this.CommandParameter
                };
                return mi;
            }
        }

        private bool _isSeparator = false;
        public bool IsSeparator {
            get {
                return _isSeparator;
            }
            set {
                if (_isSeparator != value) {
                    _isSeparator = value;
                    OnPropertyChanged(nameof(IsSeparator));
                }
            }
        }

        private bool? _isChecked = null;
        public bool? IsChecked {
            get {
                if(!IsCheckable) {
                    return false;
                }
                return _isChecked;
            }
            set {
                if (_isChecked != value) {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        public bool IsCheckable {
            get {
                return _isChecked.HasValue;
            }
        }

        public Visibility MenuItemVisibility { get; set; }

        public string Header { get; set; }

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public string InputGestureText { get; set; }

        public string IconSource { get; set; }

        public Brush IconBackgroundBrush { get; set; } = Brushes.Transparent;

        public Image Icon { get; set; }

        public ObservableCollection<MpContextMenuItemViewModel> SubItems { get; set; }

        #endregion

        #region Public Methods

        public MpContextMenuItemViewModel() : base(null)  {
            PropertyChanged += MpContextMenuItemViewModel_PropertyChanged;
            IsSeparator = true;
        }

        
        public MpContextMenuItemViewModel(
            string header, 
            ICommand command,
            object commandParameter,
            bool? isChecked,
            string iconSource = "",
            ObservableCollection<MpContextMenuItemViewModel> subItems = null,
            string inputGestureText = "",
            Brush bgBrush = null,
            BitmapSource bmpSrc = null) : this() {
            IsSeparator = false;

            Header = header;
            Command = command;
            CommandParameter = commandParameter;
            IsChecked = isChecked;
            if(bmpSrc == null) {
                IconSource = iconSource;
            } else {
                Icon = new Image();
                Icon.Source = bmpSrc;
                Icon.Stretch = Stretch.Fill;
            }
            
            SubItems = subItems ?? new ObservableCollection<MpContextMenuItemViewModel>();
            InputGestureText = inputGestureText;
            IconBackgroundBrush = bgBrush == null ? Brushes.Transparent : bgBrush;
        }

        
        #endregion

        #region Private Methods

        private void MpContextMenuItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IconSource):
                    if (!string.IsNullOrEmpty(IconSource)) {
                        var icon = new Image();
                        if (!IconSource.IsBase64String()) {
                            icon.Source = (BitmapSource)new BitmapImage(new Uri(IconSource));

                        } else {
                            icon.Source = IconSource.ToBitmapSource();
                            //icon.Height = icon.Width = 20;
                        }
                        Icon = icon;
                        Icon.Stretch = Stretch.Fill;
                    }
                    break;
                case nameof(IconBackgroundBrush):
                    if (IconBackgroundBrush != null) {
                        var bgBmp = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/texture.png"));
                        bgBmp = MpHelpers.Instance.TintBitmapSource(bgBmp, ((SolidColorBrush)IconBackgroundBrush).Color, false);
                        var borderBmp = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/textureborder.png"));
                        if (!MpHelpers.Instance.IsBright((IconBackgroundBrush as SolidColorBrush).Color)) {
                            borderBmp = MpHelpers.Instance.TintBitmapSource(borderBmp, Colors.White, false);
                        }
                        var icon = new Image();
                        icon.Source = MpHelpers.Instance.MergeImages(new List<BitmapSource> { bgBmp, borderBmp });
                        if (!IsChecked.HasValue || IsChecked.Value) {
                            string checkPath = !IsChecked.HasValue ? @"/Images/check_partial.png" : @"/Images/check.png";
                            var checkBmp = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + checkPath));
                            if (!MpHelpers.Instance.IsBright((IconBackgroundBrush as SolidColorBrush).Color)) {
                                checkBmp = MpHelpers.Instance.TintBitmapSource(checkBmp, Colors.White, false);
                            }
                            icon.Source = MpHelpers.Instance.MergeImages(new List<BitmapSource> { (BitmapSource)icon.Source, checkBmp });
                        }
                        Icon = icon;
                    }
                    break;
            }
        }


        #endregion
    }
}
