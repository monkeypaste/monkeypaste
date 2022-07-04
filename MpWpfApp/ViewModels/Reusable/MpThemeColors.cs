using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using System.Windows.Data;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MpWpfApp {
    public enum MpThemeItemType {
        Title_Menu_Background_Color,
        Filter_Menu_Background_Color,
        Clip_Tray_Background_Color,
        Clip_Tile_Content_Item_Background_Color,
        Clip_Tile_Selected_Tile_Border_Color,
        Clip_Tile_Primary_Selected_Tile_Border_Color,
        Clip_Tile_Unselected_Hovering_Tile_Border_Color,
        App_Mode_Grid_Background_Color,
        SearchBox_Background_Color,
        Tag_Tree_Background_Color
    }

    public enum MpThemeType {
        None = 0,
        Dark,
        Light,
        Jungle,
        Custom
    }

    public class MpThemeColors : MpViewModelBase, MpIAsyncSingletonViewModel<MpThemeColors> {

        #region Private Variables

        private static double _defaultOpacity = 0.73;

        private Dictionary<MpThemeItemType, Brush> _lightThemeColors =
            new Dictionary<MpThemeItemType, Brush> {
            {MpThemeItemType.Title_Menu_Background_Color, new SolidColorBrush(Colors.DimGray){ Opacity=_defaultOpacity } },
            {MpThemeItemType.Filter_Menu_Background_Color, new SolidColorBrush(Colors.DarkViolet){ Opacity=_defaultOpacity } },
            {MpThemeItemType.Clip_Tray_Background_Color, new SolidColorBrush(Colors.MediumPurple){ Opacity=_defaultOpacity } },
            {MpThemeItemType.Clip_Tile_Content_Item_Background_Color, new SolidColorBrush(Colors.White){ Opacity=1 } },
            {MpThemeItemType.Clip_Tile_Selected_Tile_Border_Color, new SolidColorBrush(Colors.DarkViolet){ Opacity=_defaultOpacity } },
            {MpThemeItemType.Clip_Tile_Primary_Selected_Tile_Border_Color, new SolidColorBrush(Colors.DarkViolet){ Opacity=_defaultOpacity } },
            {MpThemeItemType.Clip_Tile_Unselected_Hovering_Tile_Border_Color, new SolidColorBrush(Colors.DarkViolet){ Opacity=_defaultOpacity } },
            {MpThemeItemType.App_Mode_Grid_Background_Color, new SolidColorBrush(Colors.DimGray){ Opacity=_defaultOpacity } },
            {MpThemeItemType.Tag_Tree_Background_Color, new SolidColorBrush(Colors.DarkViolet){ Opacity=_defaultOpacity } },
        };

        #endregion

        #region Properties

        public Dictionary<string, Brush> CurrentTheme { get; set; }

        public List<List<Brush>> ContentColors {
            get {
                
                return new List<List<Brush>> {
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(248, 160, 174)),
                        new SolidColorBrush(Color.FromRgb(243, 69, 68)),
                        new SolidColorBrush(Color.FromRgb(229, 116, 102)),
                        new SolidColorBrush(Color.FromRgb(211, 159, 161)),
                        new SolidColorBrush(Color.FromRgb(191, 53, 50))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(252, 168, 69)),
                        new SolidColorBrush(Color.FromRgb(251, 108, 40)),
                        new SolidColorBrush(Color.FromRgb(253, 170, 130)),
                        new SolidColorBrush(Color.FromRgb(189, 141, 103)),
                        new SolidColorBrush(Color.FromRgb(177, 86, 55))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(215, 157, 60)),
                        new SolidColorBrush(Color.FromRgb(168, 123, 82)),
                        new SolidColorBrush(Color.FromRgb(214, 182, 133)),
                        new SolidColorBrush(Color.FromRgb(162, 144, 122)),
                        new SolidColorBrush(Color.FromRgb(123, 85, 72))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(247, 245, 144)),
                        new SolidColorBrush(Color.FromRgb(252, 240, 78)),
                        new SolidColorBrush(Color.FromRgb(239, 254, 185)),
                        new SolidColorBrush(Color.FromRgb(198, 193, 127)),
                        new SolidColorBrush(Color.FromRgb(224, 200, 42))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(189, 254, 40)),
                        new SolidColorBrush(Color.FromRgb(143, 254, 115)),
                        new SolidColorBrush(Color.FromRgb(217, 231, 170)),
                        new SolidColorBrush(Color.FromRgb(172, 183, 38)),
                        new SolidColorBrush(Color.FromRgb(140, 157, 45))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(50, 255, 76)),
                        new SolidColorBrush(Color.FromRgb(68, 199, 33)),
                        new SolidColorBrush(Color.FromRgb(193, 214, 135)),
                        new SolidColorBrush(Color.FromRgb(127, 182, 99)),
                        new SolidColorBrush(Color.FromRgb(92, 170, 58))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(54, 255, 173)),
                        new SolidColorBrush(Color.FromRgb(32, 195, 178)),
                        new SolidColorBrush(Color.FromRgb(170, 206, 160)),
                        new SolidColorBrush(Color.FromRgb(160, 201, 197)),
                        new SolidColorBrush(Color.FromRgb(32, 159, 148))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(96, 255, 227)),
                        new SolidColorBrush(Color.FromRgb(46, 238, 249)),
                        new SolidColorBrush(Color.FromRgb(218, 253, 233)),
                        new SolidColorBrush(Color.FromRgb(174, 193, 208)),
                        new SolidColorBrush(Color.FromRgb(40, 103, 146))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(149, 204, 243)),
                        new SolidColorBrush(Color.FromRgb(43, 167, 237)),
                        new SolidColorBrush(Color.FromRgb(215, 244, 248)),
                        new SolidColorBrush(Color.FromRgb(153, 178, 198)),
                        new SolidColorBrush(Color.FromRgb(30, 51, 160))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(99, 141, 227)),
                        new SolidColorBrush(Color.FromRgb(22, 127, 193)),
                        new SolidColorBrush(Color.FromRgb(201, 207, 233)),
                        new SolidColorBrush(Color.FromRgb(150, 163, 208)),
                        new SolidColorBrush(Color.FromRgb(52, 89, 170))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(157, 176, 255)),
                        new SolidColorBrush(Color.FromRgb(148, 127, 220)),
                        new SolidColorBrush(Color.FromRgb(216, 203, 233)),
                        new SolidColorBrush(Color.FromRgb(180, 168, 192)),
                        new SolidColorBrush(Color.FromRgb(109, 90, 179))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(221, 126, 230)),
                        new SolidColorBrush(Color.FromRgb(186, 141, 200)),
                        new SolidColorBrush(Color.FromRgb(185, 169, 231)),
                        new SolidColorBrush(Color.FromRgb(203, 178, 200)),
                        new SolidColorBrush(Color.FromRgb(170, 90, 179))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(225, 103, 164)),
                        new SolidColorBrush(Color.FromRgb(252, 74, 210)),
                        new SolidColorBrush(Color.FromRgb(238, 233, 237)),
                        new SolidColorBrush(Color.FromRgb(195, 132, 163)),
                        new SolidColorBrush(Color.FromRgb(205, 60, 117))
                    },
                    new List<Brush> {
                        new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        new SolidColorBrush(Color.FromRgb(223, 223, 223)),
                        new SolidColorBrush(Color.FromRgb(187, 187, 187)),
                        new SolidColorBrush(Color.FromRgb(137, 137, 137)),
                        new ImageBrush(new BitmapImage(new Uri(MpJsonPreferenceIO.Instance.AbsoluteResourcesPath + @"/Images/add2.png")))
                    }
                };
            }
        }

        #endregion

        public MpThemeType CurrentThemeType { get; private set; }


        private static MpThemeColors _instance;
        public static MpThemeColors Instance => _instance ?? (_instance = new MpThemeColors());

        public async Task InitAsync() {
            await Task.Delay(1);
            InitDefaultThemes();
            LoadTheme(MpThemeType.Light);
        }

        public MpThemeColors() : base(null) {
        }


        private void InitDefaultThemes() {
        }

        public void LoadTheme(MpThemeType themeType) {
            var ct = new Dictionary<string, Brush>();
            
            //CurrentTheme = _lightThemeColors;

            switch (themeType) {
                case MpThemeType.Light:
                   // CurrentTheme = _lightThemeColors;
                   foreach(var kvp in _lightThemeColors) {
                        ct.Add(Enum.GetName(typeof(MpThemeItemType), kvp.Key), kvp.Value);
                    }
                    break;
            }
            CurrentTheme = ct;
            CurrentThemeType = themeType;
        }
    }

    public abstract class MpBindingExtension : Binding, IValueConverter {
        protected MpBindingExtension() {
            Source = Converter = this;
        }

        protected MpBindingExtension(object source) // Source, RelativeSource, null for DataContext
        {
            var relativeSource = source as RelativeSource;
            if (relativeSource == null && source != null) Source = source;
            else RelativeSource = relativeSource;
            Converter = this;
        }

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    //https://thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
    //public class MpThemeColorsBindingExtension : MpBindingExtension {
    //    public MpThemeColorsBindingExtension() {
    //        Source = MpThemeColors.Instance;
    //    }

    //    public MpThemeColorsBindingExtension(string path) : this() {
    //        Key = path;
    //    }

    //    public List<List<Brush>> ContentColors {
    //        get {
    //            return MpThemeColors.Instance.ContentColors;
    //        }
    //    }

    //    public string Key { get; set; }

    //    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    //        if (Enum.TryParse<MpThemeItemType>(Key, out MpThemeItemType itemType)) {
    //            return MpThemeColors.Instance.CurrentTheme[itemType];
    //        }
    //        return Brushes.Red;
    //    }
    //}

    public sealed class DictionaryAdapter<T, TU> : INotifyPropertyChanged {
        private TU _value;
        private readonly TU[] _values;

        public T Key { get; }

        public TU Value {
            get => _value;
            set {
                _value = value;
                OnPropertyChanged();
            }
        }


        public DictionaryAdapter(T key, params TU[] values) {
            Key = key;
            _values = values;
            Value = values[0];
        }


        public override string ToString() {
            return Value.ToString();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
