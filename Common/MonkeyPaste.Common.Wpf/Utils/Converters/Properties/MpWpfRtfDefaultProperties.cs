using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace MonkeyPaste.Common.Wpf {
    public class MpWpfRtfDefaultProperties {
        #region Singleton
        private static readonly Lazy<MpWpfRtfDefaultProperties> _Lazy = new Lazy<MpWpfRtfDefaultProperties>(() => new MpWpfRtfDefaultProperties());
        public static MpWpfRtfDefaultProperties Instance { get { return _Lazy.Value; } }

        private MpWpfRtfDefaultProperties() {
            FontFamilys = new ObservableCollection<string>(_defaultFontFamilys);
            FontSizes = new ObservableCollection<double>(_defaultFontSizes);
            FontColors = new ObservableCollection<Color>(_defaultColors);
        }
        #endregion

        #region Private Variables
        private List<Color> _defaultColors = new List<Color> {
            Colors.Black,
            Colors.White
        };

        private List<string> _defaultFontFamilys = new List<string> {
            "arial",
            "courier",
            "garamond",
            "georgia",
            "tahoma",
            "times new roman",
            "verdana"
        };

        private List<double> _defaultFontSizes = new List<double> {
             8, 9, 10, 12, 14, 16, 20, 24, 32, 42, 54, 68, 84, 98
        };

        private int _defaultFontIdx = 0;
        private int _defaultFontSizeIdx = 0;
        private int _defaultFgColorIdx = 0;
        private int _defaultBgColorIdx = 1;
        #endregion

        #region Properties
        public ObservableCollection<string> FontFamilys { get; private set; }

        public ObservableCollection<double> FontSizes { get; private set; }

        public ObservableCollection<Color> FontColors { get; private set; }

        public string DefaultFont {
            get {
                return FontFamilys[_defaultFontIdx];
            }
        }

        public int DefaultFontIdx {
            get {
                return _defaultFontIdx;
            }
        }

        public double DefaultFontSize {
            get {
                return FontSizes[_defaultFontSizeIdx];
            }
        }

        public int DefaultFontSizeIdx {
            get {
                return _defaultFontSizeIdx;
            }
        }

        public Color DefaultFgColor {
            get {
                return FontColors[_defaultFgColorIdx];
            }
        }

        public Color DefaultBgColor {
            get {
                return FontColors[_defaultBgColorIdx];
            }
        }

        public int DefaultFgColorIdx {
            get {
                return _defaultFgColorIdx;
            }
        }

        public int DefaultBgColorIdx {
            get {
                return _defaultBgColorIdx;
            }
        }
        #endregion

        #region Public Methods
        public void AddFont(string fontName) {
            if (!FontFamilys.Contains(fontName)) {
                FontFamilys.Add(fontName);
                FontFamilys = new ObservableCollection<string>(FontFamilys.OrderBy(x => x));
            }
        }

        public void AddFontSize(double newSize) {
            if (!FontSizes.Contains(newSize)) {
                FontSizes.Add(newSize);
                FontSizes = new ObservableCollection<double>(FontSizes.OrderBy(x => x));
            }
        }

        public void AddFontColor(Color c) {
            if (!FontColors.Contains(c)) {
                FontColors.Add(c);
            }
        }

        public void SetDefaultFont(string fontName) {
            if (!FontFamilys.Contains(fontName)) {
                AddFont(fontName);
            }
            _defaultFontIdx = FontFamilys.IndexOf(fontName);
        }

        public void SetDefaultFontSize(int fontSize) {
            if (!FontSizes.Contains(fontSize)) {
                AddFontSize(fontSize);
            }
            _defaultFontSizeIdx = FontSizes.IndexOf(fontSize);
        }
        #endregion
    }
}
