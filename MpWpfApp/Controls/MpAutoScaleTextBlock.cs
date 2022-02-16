using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpAutoScaleTextBlock : Canvas {
        public string Text {
            get { return (string)GetValue(TextProperty); }
            set {
                SetValue(TextProperty, value);

                InvalidateVisual();
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MpAutoScaleTextBlock), new UIPropertyMetadata(""));

        public FontFamily Font {
            get { return (FontFamily)GetValue(FontProperty); }
            set { SetValue(FontProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Font.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontProperty =
            DependencyProperty.Register("Font", typeof(FontFamily), typeof(MpAutoScaleTextBlock), new UIPropertyMetadata(new FontFamily("Tahoma")));

        public FontWeight FontWeight {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(MpAutoScaleTextBlock), new UIPropertyMetadata(FontWeights.Normal));

        public FontStyle FontStyle {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(MpAutoScaleTextBlock), new UIPropertyMetadata(FontStyles.Normal));

        public FontStretch FontStretch {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontStretch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontStretchProperty =
            DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(MpAutoScaleTextBlock), new UIPropertyMetadata(FontStretches.Normal));



        public Brush Foreground {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(MpAutoScaleTextBlock), new UIPropertyMetadata(Brushes.Black));


        protected override void OnRender(DrawingContext dc) {
            double fontSize = 72;
            FormattedText text = null;
            string alteredText;

            do {
                alteredText = Text;

                fontSize--;

                text = new FormattedText(alteredText, new CultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(Font, FontStyle, FontWeight, FontStretch), fontSize, Foreground);

                while (text.Width > this.Width) {
                    string line = GetMaximumTextForWidth(alteredText, this.Width, fontSize);

                    if (line.Length == 0) break;
                    alteredText = string.Format("{0}{1}{2}", line, Environment.NewLine, alteredText.Substring(line.Length));
                    alteredText = alteredText.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    alteredText = alteredText.Replace(Environment.NewLine + " ", Environment.NewLine);

                    text = new FormattedText(alteredText, new CultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(Font, FontStyle, FontWeight, FontStretch), fontSize, Foreground);

                    if (alteredText.Contains(Environment.NewLine + Environment.NewLine)) {
                        break;
                    }
                }

            } while (text.Height > this.Height || text.Width > this.Width);

            dc.DrawText(text, new Point(0, 0));
        }

        string GetMaximumTextForWidth(string s, double width, double fontSize) {
            for (int n = s.Length; n >= 0; n--) {
                FormattedText text = new FormattedText(s.Substring(0, n), new CultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(Font, FontStyle, FontWeight, FontStretch), fontSize, Foreground);

                if (text.Width < width) {
                    // roll back to next space
                    while (s[n] != ' ') {
                        n--;

                        if (n == 0) break;
                    }
                    return s.Substring(0, n);
                }
            }

            return s;
        }
    }
}
