﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FFImageLoading.Work;
using SkiaSharp;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpHelpers {
        #region Singleton
        private static readonly Lazy<MpHelpers> _Lazy = new Lazy<MpHelpers>(() => new MpHelpers());
        public static MpHelpers Instance { get { return _Lazy.Value; } }

        private MpHelpers() {
            Rand = new Random((int)DateTime.Now.Ticks);
        }
        #endregion

        public Random Rand { get; set; }

        #region Documents
        public const string AlphaNumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private string _passwordChars = null;
        public string PasswordChars {
            get {
                if(_passwordChars == null) {
                    var sb = new StringBuilder();
                    for (int i = char.MinValue; i <= char.MaxValue; i++) {
                        char c = Convert.ToChar(i);
                        if (!char.IsControl(c)) {
                            sb.Append(c);
                        }
                    }
                    _passwordChars = sb.ToString();
                }
                return _passwordChars;
            }
        }

        public string GetRandomString(int length, string chars = AlphaNumericChars) {
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

        public bool IsStringCsv(string text) {
            if (string.IsNullOrEmpty(text) || IsStringRichText(text)) {
                return false;
            }
            return text.Contains(",");
        }

        public bool IsStringRichText(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"{\rtf");
        }

        public bool IsStringXaml(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=") || text.StartsWith(@"<Span xmlns=");
        }

        public bool IsStringSpan(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Span xmlns=");
        }

        public bool IsStringSection(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=");
        }

        public bool IsStringPlainText(string text) {
            //returns true for csv
            if (text == null) {
                return false;
            }
            if (text == string.Empty) {
                return true;
            }
            if (IsStringRichText(text) || IsStringSection(text) || IsStringSpan(text) || IsStringXaml(text)) {
                return false;
            }
            return true;
        }
        #endregion

        #region System
        public string AppStorageFilePath {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }

        public double ConvertBytesToMegabytes(long bytes, int precision = 2) {
            return Math.Round((bytes / 1024f) / 1024f, precision);
        }

        public double ConvertMegaBytesToBytes(long megabytes, int precision = 2) {
            return Math.Round((megabytes * 1024f) * 1024f, precision);
        }

        public double GetFileSizeInBytes(string filePath) {
            try {
                if(File.Exists(filePath)) {
                    FileInfo fi = new FileInfo(filePath);
                    return fi.Length;
                }
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error checking size of path {filePath}", ex);
            }
            return -1;
        }

        public void AppendTextToFile(string path, string textToAppend) {
            try {
                if (!File.Exists(path)) {
                    // Create a file to write to.
                    using (var sw = File.CreateText(path)) {
                        sw.WriteLine(textToAppend);
                    }
                } else {
                    // This text is always added, making the file longer over time
                    // if it is not deleted.
                    using (StreamWriter sw = File.AppendText(path)) {
                        sw.WriteLine(textToAppend);
                    }
                }
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error appending text '{textToAppend}' to path '{path}'");
                MpConsole.WriteTraceLine($"With exception: {ex}");
            }            
        }

        public string ReadTextFromFile(string filePath) {
            try {
                using (StreamReader f = new StreamReader(filePath)) {
                    string outStr = string.Empty;
                    outStr = f.ReadToEnd();
                    f.Close();
                    return outStr;
                }
            } catch (Exception ex) {
                Console.WriteLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return null;
            }
        }

        public string WriteTextToFile(string filePath, string text, bool isTemporary = false) {
            try {
                if (filePath.ToLower().Contains(@".tmp")) {
                    string extension = string.Empty;
                    if (MpHelpers.Instance.IsStringRichText(text)) {
                        extension = @".rtf";
                    } else if (MpHelpers.Instance.IsStringCsv(text)) {
                        extension = @".csv";
                    } else {
                        extension = @".txt";
                    }
                    filePath = filePath.ToLower().Replace(@".tmp", extension);
                }
                using (var of = new StreamWriter(filePath)) {
                    of.Write(text);
                    of.Close();
                    if (isTemporary) {
                        MpTempFileManager.Instance.AddTempFilePath(filePath);
                    }
                    return filePath;
                }
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path '{filePath}' with text '{text}'",ex);
                return null;
            }            
        }

        public string WriteByteArrayToFile(string filePath, byte[] byteArray, bool isTemporary = false) {
            try {
                if (filePath.ToLower().Contains(@".tmp")) {
                    filePath = filePath.ToLower().Replace(@".tmp", @".png");
                }
                File.WriteAllBytes(filePath, byteArray);
                return filePath;
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path {filePath} for byte array " + (byteArray == null ? "which is null" : "which is NOT null"), ex);
                return null;
            }
        }
        #endregion

        #region Visual
        private List<List<Brush>> _ContentColors = new List<List<Brush>> {
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
                    new SolidColorBrush(Color.FromRgb(65, 65, 65))
                }
            };

        public Brush GetContentColor(int c, int r)
        {
            return _ContentColors[c][r];
        }

        public double ColorDistance(Color e1, Color e2)
        {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = ((long)e1.R + (long)e2.R) / 2;
            long r = (long)e1.R - (long)e2.R;
            long g = (long)e1.G - (long)e2.G;
            long b = (long)e1.B - (long)e2.B;
            double max = 764.83331517396653;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }

        public bool IsBright(Color c, int brightThreshold = 150)
        {
            int grayVal = (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114);
            return grayVal > brightThreshold;
        }

        public SolidColorBrush ChangeBrushAlpha(SolidColorBrush solidColorBrush, byte alpha)
        {
            var c = solidColorBrush.Color;
            solidColorBrush.Color = Color.FromRgba(c.R,c.G,c.B,(double)alpha);
            return solidColorBrush;
        }

        public SolidColorBrush ChangeBrushBrightness(SolidColorBrush b, double correctionFactor)
        {
            if (correctionFactor == 0.0f)
            {
                return b;
            }
            double red = (double)b.Color.R;
            double green = (double)b.Color.G;
            double blue = (double)b.Color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return new SolidColorBrush(Color.FromRgba((byte)red, (byte)green, (byte)blue, b.Color.A));
        }

        public Brush GetDarkerBrush(Brush b)
        {
            return ChangeBrushBrightness((SolidColorBrush)b, -0.5);
        }

        public Brush GetLighterBrush(Brush b)
        {
            return ChangeBrushBrightness((SolidColorBrush)b, 0.5);
        }

        public Color GetRandomColor(byte alpha = 255)
        {
            //if (alpha == 255) {
            //    return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            //}
            //return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            int x = Rand.Next(0, _ContentColors.Count);
            int y = Rand.Next(0, _ContentColors[0].Count);
            return ((SolidColorBrush)GetContentColor(x, y)).Color;
        }

        public Brush GetRandomBrushColor(byte alpha = 255)
        {
            return (Brush)new SolidColorBrush() { Color = GetRandomColor(alpha) };
        }
        #endregion

        #region Http
        public string GetFullyFormattedUrl(string str)
        {
            //returns url so it has protocol prefix
            if (str.StartsWith(@"http://"))
            {
                return str;
            }
            if (str.StartsWith(@"https://"))
            {
                return str;
            }
            //use http without s because if it is https then it will resolve to but otherwise will not load
            return @"http://" + str;
        }

        public string GetUrlDomain(string url)
        {
            //returns protocol prefixed domain url text
            try
            {
                url = GetFullyFormattedUrl(url);
                int domainStartIdx = url.IndexOf(@"//") + 2;
                if (url.Length <= domainStartIdx)
                {
                    return string.Empty;
                }
                if (!url.Substring(domainStartIdx).Contains(@"/"))
                {
                    return url.Substring(domainStartIdx);
                }
                int domainEndIdx = url.Substring(domainStartIdx).IndexOf(@"/");
                return url.Substring(domainStartIdx).Substring(0, domainEndIdx);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MpHelpers.GetUrlDomain error for url: " + url + " with exception: " + ex);
            }
            return null;
        }
        #endregion

        #region Converters
        #endregion
    }
}