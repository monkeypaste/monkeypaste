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

namespace MpWpfApp {
    public enum MpThemeItemType {
        None = 0,
        Filter_Menu_Background_Color,
        Clip_Tray_Background_Color,
        Clip_Tile_Title_Text_Foreground_Color,
        Clip_Tile_Title_Text_Background_Color,
        Clip_Tile_Content_Item_Background_Color,
        Clip_Tile_Selected_Tile_Border_Color,
        Clip_Tile_Primary_Selected_Tile_Border_Color,
        Clip_Tile_Unselected_Hovering_Tile_Border_Color        
    }

    public enum MpThemeType {
        None = 0,
        Dark,
        Light,
        Jungle,
        Custom
    }

    public class MpThemeColors {
        private static readonly Lazy<MpThemeColors> _Lazy = new Lazy<MpThemeColors>(() => new MpThemeColors());
        public static MpThemeColors Instance { get { return _Lazy.Value; } }

        public MpThemeType CurrentThemeType { get; private set; }


        private List<Brush> _currentThemeColors;
        private List<Brush> _lightThemeColors;

        public void Init() {
            InitDefaultThemes();
            LoadTheme(MpThemeType.Light);            
        }

        private void InitDefaultThemes() {
            _lightThemeColors = new List<Brush> {
                Brushes.Transparent, //None = 0,
                Brushes.DarkViolet, //Filter_Menu_Background_Color,
                Brushes.MediumPurple,//Clip_Tray_Background_Color,
                Brushes.White,//Clip_Tile_Title_Text_Foreground_Color,
                Brushes.Black,//Clip_Tile_Title_Text_Background_Color,
                Brushes.White,//Clip_Tile_Content_Item_Background_Color,
                Brushes.Red,//Clip_Tile_Selected_Tile_Border_Color,
                Brushes.Blue,//Clip_Tile_Primary_Selected_Tile_Border_Color,
                Brushes.Yellow//Clip_Tile_Unselected_Hovering_Tile_Border_Color
            };
        }

        public void LoadTheme(MpThemeType themeType) {
            _currentThemeColors = new List<Brush>();

            switch(themeType) {
                case MpThemeType.Light:
                    _currentThemeColors = _lightThemeColors;
                    break;
            }
            CurrentThemeType = themeType;
        }
        public Brush Filter_Menu_Background_Color {
            get {
                return _currentThemeColors[(int)MpThemeItemType.Filter_Menu_Background_Color];
            }
            set {
                _currentThemeColors[(int)MpThemeItemType.Filter_Menu_Background_Color] = value;
            }
        }

        public Brush Clip_Tray_Background_Color {
            get {
                return _currentThemeColors[(int)MpThemeItemType.Clip_Tray_Background_Color];
            }
            set {
                _currentThemeColors[(int)MpThemeItemType.Clip_Tray_Background_Color] = value;
            }
        }

        public Brush Clip_Tile_Title_Text_Foreground_Color {
            get {
                return _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Title_Text_Foreground_Color];
            }
            set {
                _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Title_Text_Foreground_Color] = value;
            }
        }

        public Brush Clip_Tile_Title_Text_Background_Color {
            get {
                return _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Title_Text_Background_Color];
            }
            set {
                _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Title_Text_Background_Color] = value;
            }
        }

        public Brush Clip_Tile_Content_Item_Background_Color {
            get {
                return _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Content_Item_Background_Color];
            }
            set {
                _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Content_Item_Background_Color] = value;
            }
        }

        public Brush Clip_Tile_Selected_Tile_Border_Color {
            get {
                return _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Selected_Tile_Border_Color];
            }
            set {
                _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Selected_Tile_Border_Color] = value;
            }
        }
        

        public Brush Clip_Tile_Primary_Selected_Tile_Border_Color {
            get {
                return _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Primary_Selected_Tile_Border_Color];
            }
            set {
                _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Primary_Selected_Tile_Border_Color] = value;
            }
        }

        public Brush Clip_Tile_Unselected_Hovering_Tile_Border_Color {
            get {
                return _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Unselected_Hovering_Tile_Border_Color];
            }
            set {
                _currentThemeColors[(int)MpThemeItemType.Clip_Tile_Unselected_Hovering_Tile_Border_Color] = value;
            }
        }
    }
}
