using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MpWpfApp {

    public static class MpRichTextToImageConverter {
        private static RichTextBoxDrawer rtfDrawer;
        public static void DrawRtfText(this Graphics graphics, string rtf, RectangleF layoutArea, float xFactor) {
            if (MpRichTextToImageConverter.rtfDrawer == null) {
                MpRichTextToImageConverter.rtfDrawer = new RichTextBoxDrawer();
            }
            if (MpHelpers.Instance.IsStringRichText(rtf)) {
                MpRichTextToImageConverter.rtfDrawer.Rtf = rtf;
            } else {
                MpRichTextToImageConverter.rtfDrawer.Rtf = MpHelpers.Instance.ConvertPlainTextToRichText(rtf);
            }
            MpRichTextToImageConverter.rtfDrawer.Draw(graphics, layoutArea, xFactor);
            //graphics.Dispose();
        }

        private class RichTextBoxDrawer : RichTextBox {
            //Code converted from code found here: http://support.microsoft.com/kb/812425/en-us

            //Convert the unit used by the .NET framework (1/100 inch)
            //and the unit used by Win32 API calls (twips 1/1440 inch)

            public void LineSpace() {
                SafeNativeMethods.PARAFORMAT2 fmt = new SafeNativeMethods.PARAFORMAT2();
                fmt.cbSize = Marshal.SizeOf(fmt);
                fmt.dwMask |= SafeNativeMethods.PFM_LINESPACING | SafeNativeMethods.PFM_SPACEAFTER;

                fmt.dyLineSpacing = (int)(22 * this.SelectionFont.SizeInPoints); // in twips
                                                                                 // specify exact line spacing
                fmt.bLineSpacingRule = Convert.ToByte(4);
                SafeNativeMethods.SendMessage(this.Handle, SafeNativeMethods.EM_SETPARAFORMAT, 0, ref fmt); // 0 - to all text. 1 - only to seleted
            }

            public void LineSpace(int _linespace) {
                SafeNativeMethods.PARAFORMAT2 fmt = new SafeNativeMethods.PARAFORMAT2();
                fmt.cbSize = Marshal.SizeOf(fmt);
                fmt.dwMask |= SafeNativeMethods.PFM_LINESPACING | SafeNativeMethods.PFM_SPACEAFTER;

                fmt.dyLineSpacing = _linespace; // in twips
                                                // specify exact line spacing
                fmt.bLineSpacingRule = Convert.ToByte(4);
                SafeNativeMethods.SendMessage(this.Handle, SafeNativeMethods.EM_SETPARAFORMAT, 0, ref fmt); // 0 - to all text. 1 - only to seleted
            }

            protected override CreateParams CreateParams {
                get {
                    CreateParams createParams = base.CreateParams;
                    if (SafeNativeMethods.LoadLibrary("msftedit.dll") != IntPtr.Zero) {
                        createParams.ExStyle |= SafeNativeMethods.WS_EX_TRANSPARENT; // transparent
                        createParams.ClassName = "RICHEDIT50W";
                    }
                    return createParams;
                }
            }

            private void DPToHIMETRIC(Graphics graphics, ref SizeF size) {
                size.Width = (size.Width * 2540.0f) / graphics.DpiX;
                size.Height = (size.Height * 2540.0f) / graphics.DpiY;
                //graphics.Dispose();
            }

            public void Draw(Graphics graphics, RectangleF layoutArea, float xFactor) {
                //System.Diagnostics.Debug.WriteLine("LayoutArea " + layoutArea);

                SizeF metaSize = layoutArea.Size;
                DPToHIMETRIC(graphics, ref metaSize);

                //System.Diagnostics.Debug.WriteLine("MetaSize " + metaSize);

                IntPtr hdc = graphics.GetHdc();

                //create a metafile, convert the size to himetrics
                Metafile metafile = new Metafile(hdc, new RectangleF(0, 0, metaSize.Width, metaSize.Height));

                graphics.ReleaseHdc(hdc);

                Graphics g = Graphics.FromImage(metafile);
                IntPtr hDCEMF = g.GetHdc();

                //Calculate the area to render.
                SafeNativeMethods.RECT rectLayoutArea;
                rectLayoutArea.Left = 0;
                rectLayoutArea.Top = 0;
                rectLayoutArea.Right = (int)((1440 * metaSize.Width + 2540 / 2) / 2540);
                rectLayoutArea.Bottom = (int)((1440 * metaSize.Height + 2540 / 2) / 2540);

                //System.Diagnostics.Debug.WriteLine(String.Format("RectLayoutArea ({0},{1})", rectLayoutArea.Right, rectLayoutArea.Bottom));

                SafeNativeMethods.FORMATRANGE fmtRange;
                fmtRange.chrg.cpMax = -1;            //Indicate character from to character to
                fmtRange.chrg.cpMin = 0;
                fmtRange.hdc = hDCEMF;                  //Use the same DC for measuring and rendering
                fmtRange.hdcTarget = hDCEMF;         //Point at printer hDC
                fmtRange.rc = rectLayoutArea;        //Indicate the area on page to print
                fmtRange.rcPage = rectLayoutArea;    //Indicate size of page

                IntPtr wParam = IntPtr.Zero;
                wParam = new IntPtr(1);

                //Get the pointer to the FORMATRANGE structure in memory
                IntPtr lParam = IntPtr.Zero;
                lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange));
                Marshal.StructureToPtr(fmtRange, lParam, false);

                SafeNativeMethods.SendMessage(this.Handle, SafeNativeMethods.EM_FORMATRANGE, wParam, lParam);
                SafeNativeMethods.SendMessage(this.Handle, SafeNativeMethods.EM_FORMATRANGE, wParam, IntPtr.Zero);

                //Free the block of memory allocated
                Marshal.FreeCoTaskMem(lParam);

                //Release the device context handle obtained by a previous call
                g.ReleaseHdc(hDCEMF);
                g.Dispose();

                hdc = graphics.GetHdc();
                int nHorzSize = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.HORZSIZE);
                int nVertSize = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.VERTSIZE);
                int nHorzRes = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.HORZRES);
                int nVertRes = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.VERTRES);
                int nLogPixelsX = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.LOGPIXELSX);
                int nLogPixelsY = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.LOGPIXELSY);
                graphics.ReleaseHdc(hdc);

                float fHorzSizeInches = nHorzSize / 25.4f;
                float fVertSizeInches = nVertSize / 25.4f;
                float fHorzFudgeFactor = (nHorzRes / fHorzSizeInches) / nLogPixelsX;
                float fVertFudgeFactor = (nVertRes / fVertSizeInches) / nLogPixelsY;

                //System.Diagnostics.Debug.WriteLine("Fudge Factor " + fHorzFudgeFactor.ToString() + " " + fVertFudgeFactor.ToString() + " XFactor " + xFactor.ToString());

                Pen redPen = new Pen(Color.Red);
                graphics.DrawRectangle(redPen, layoutArea.X * xFactor, layoutArea.Y * xFactor, layoutArea.Width * xFactor, layoutArea.Height * xFactor);

                float left = layoutArea.Left;
                float top = layoutArea.Top;
                //layoutArea.X = layoutArea.Y = 0;
                layoutArea.Offset(-left, -top);
                layoutArea.Offset(left / fHorzFudgeFactor, top / fVertFudgeFactor);

                System.Drawing.Drawing2D.GraphicsState state = graphics.Save();
                graphics.ScaleTransform(fHorzFudgeFactor * xFactor, fVertFudgeFactor * xFactor);
                graphics.DrawImage(metafile, layoutArea);
                graphics.Restore(state);
                //System.Diagnostics.Debug.WriteLine("Layout Aread : " + layoutArea);
            }

            #region SafeNativeMethods
            private static class SafeNativeMethods {
                [DllImport("USER32.dll")]
                public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

                [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
                public static extern IntPtr LoadLibrary(string lpFileName);

                [DllImport("gdi32.dll")]
                public static extern int GetDeviceCaps(IntPtr hdc, DeviceCap nIndex);

                [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
                public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref PARAFORMAT2 lParam);

                [StructLayout(LayoutKind.Sequential)]
                public struct RECT {
                    public int Left;
                    public int Top;
                    public int Right;
                    public int Bottom;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct CHARRANGE {
                    public int cpMin;        //First character of range (0 for start of doc)
                    public int cpMax;        //Last character of range (-1 for end of doc)
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct FORMATRANGE {
                    public IntPtr hdc;                //Actual DC to draw on
                    public IntPtr hdcTarget;    //Target DC for determining text formatting
                    public RECT rc;                        //Region of the DC to draw to (in twips)
                    public RECT rcPage;                //Region of the whole DC (page size) (in twips)
                    public CHARRANGE chrg;        //Range of text to draw (see earlier declaration)
                }

                public enum DeviceCap : int {
                    /// &lt;summary&gt;
                    /// Device driver version
                    /// &lt;/summary&gt;
                    DRIVERVERSION = 0,
                    /// &lt;summary&gt;
                    /// Device classification
                    /// &lt;/summary&gt;
                    TECHNOLOGY = 2,
                    /// &lt;summary&gt;
                    /// Horizontal size in millimeters
                    /// &lt;/summary&gt;
                    HORZSIZE = 4,
                    /// &lt;summary&gt;
                    /// Vertical size in millimeters
                    /// &lt;/summary&gt;
                    VERTSIZE = 6,
                    /// &lt;summary&gt;
                    /// Horizontal width in pixels
                    /// &lt;/summary&gt;
                    HORZRES = 8,
                    /// &lt;summary&gt;
                    /// Vertical height in pixels
                    /// &lt;/summary&gt;
                    VERTRES = 10,
                    /// &lt;summary&gt;
                    /// Number of bits per pixel
                    /// &lt;/summary&gt;
                    BITSPIXEL = 12,
                    /// &lt;summary&gt;
                    /// Number of planes
                    /// &lt;/summary&gt;
                    PLANES = 14,
                    /// &lt;summary&gt;
                    /// Number of brushes the device has
                    /// &lt;/summary&gt;
                    NUMBRUSHES = 16,
                    /// &lt;summary&gt;
                    /// Number of pens the device has
                    /// &lt;/summary&gt;
                    NUMPENS = 18,
                    /// &lt;summary&gt;
                    /// Number of markers the device has
                    /// &lt;/summary&gt;
                    NUMMARKERS = 20,
                    /// &lt;summary&gt;
                    /// Number of fonts the device has
                    /// &lt;/summary&gt;
                    NUMFONTS = 22,
                    /// &lt;summary&gt;
                    /// Number of colors the device supports
                    /// &lt;/summary&gt;
                    NUMCOLORS = 24,
                    /// &lt;summary&gt;
                    /// Size required for device descriptor
                    /// &lt;/summary&gt;
                    PDEVICESIZE = 26,
                    /// &lt;summary&gt;
                    /// Curve capabilities
                    /// &lt;/summary&gt;
                    CURVECAPS = 28,
                    /// &lt;summary&gt;
                    /// Line capabilities
                    /// &lt;/summary&gt;
                    LINECAPS = 30,
                    /// &lt;summary&gt;
                    /// Polygonal capabilities
                    /// &lt;/summary&gt;
                    POLYGONALCAPS = 32,
                    /// &lt;summary&gt;
                    /// Text capabilities
                    /// &lt;/summary&gt;
                    TEXTCAPS = 34,
                    /// &lt;summary&gt;
                    /// Clipping capabilities
                    /// &lt;/summary&gt;
                    CLIPCAPS = 36,
                    /// &lt;summary&gt;
                    /// Bitblt capabilities
                    /// &lt;/summary&gt;
                    RASTERCAPS = 38,
                    /// &lt;summary&gt;
                    /// Length of the X leg
                    /// &lt;/summary&gt;
                    ASPECTX = 40,
                    /// &lt;summary&gt;
                    /// Length of the Y leg
                    /// &lt;/summary&gt;
                    ASPECTY = 42,
                    /// &lt;summary&gt;
                    /// Length of the hypotenuse
                    /// &lt;/summary&gt;
                    ASPECTXY = 44,
                    /// &lt;summary&gt;
                    /// Shading and Blending caps
                    /// &lt;/summary&gt;
                    SHADEBLENDCAPS = 45,

                    /// &lt;summary&gt;
                    /// Logical pixels inch in X
                    /// &lt;/summary&gt;
                    LOGPIXELSX = 88,
                    /// &lt;summary&gt;
                    /// Logical pixels inch in Y
                    /// &lt;/summary&gt;
                    LOGPIXELSY = 90,

                    /// &lt;summary&gt;
                    /// Number of entries in physical palette
                    /// &lt;/summary&gt;
                    SIZEPALETTE = 104,
                    /// &lt;summary&gt;
                    /// Number of reserved entries in palette
                    /// &lt;/summary&gt;
                    NUMRESERVED = 106,
                    /// &lt;summary&gt;
                    /// Actual color resolution
                    /// &lt;/summary&gt;
                    COLORRES = 108,

                    // Printing related DeviceCaps. These replace the appropriate Escapes
                    /// &lt;summary&gt;
                    /// Physical Width in device units
                    /// &lt;/summary&gt;
                    PHYSICALWIDTH = 110,
                    /// &lt;summary&gt;
                    /// Physical Height in device units
                    /// &lt;/summary&gt;
                    PHYSICALHEIGHT = 111,
                    /// &lt;summary&gt;
                    /// Physical Printable Area x margin
                    /// &lt;/summary&gt;
                    PHYSICALOFFSETX = 112,
                    /// &lt;summary&gt;
                    /// Physical Printable Area y margin
                    /// &lt;/summary&gt;
                    PHYSICALOFFSETY = 113,
                    /// &lt;summary&gt;
                    /// Scaling factor x
                    /// &lt;/summary&gt;
                    SCALINGFACTORX = 114,
                    /// &lt;summary&gt;
                    /// Scaling factor y
                    /// &lt;/summary&gt;
                    SCALINGFACTORY = 115,

                    /// &lt;summary&gt;
                    /// Current vertical refresh rate of the display device (for displays only) in Hz
                    /// &lt;/summary&gt;
                    VREFRESH = 116,
                    /// &lt;summary&gt;
                    /// Horizontal width of entire desktop in pixels
                    /// &lt;/summary&gt;
                    DESKTOPVERTRES = 117,
                    /// &lt;summary&gt;
                    /// Vertical height of entire desktop in pixels
                    /// &lt;/summary&gt;
                    DESKTOPHORZRES = 118,
                    /// &lt;summary&gt;
                    /// Preferred blt alignment
                    /// &lt;/summary&gt;
                    BLTALIGNMENT = 119,
                }

                public struct PARAFORMAT2 {

                    public int cbSize;

                    public uint dwMask;

                    public short wNumbering;

                    public short wReserved;

                    public int dxStartIndent;

                    public int dxRightIndent;

                    public int dxOffset;

                    public short wAlignment;

                    public short cTabCount;

                    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]

                    public int[] rgxTabs;

                    // PARAFORMAT2 from here onwards.

                    public int dySpaceBefore;

                    public int dySpaceAfter;

                    public int dyLineSpacing;

                    public short sStyle;

                    public byte bLineSpacingRule;

                    public byte bOutlineLevel;

                    public short wShadingWeight;

                    public short wShadingStyle;

                    public short wNumberingStart;

                    public short wNumberingStyle;

                    public short wNumberingTab;

                    public short wBorderSpace;

                    public short wBorderWidth;

                    public short wBorders;
                }

                public const int EM_FORMATRANGE = WM_USER + 57;

                public const int PFM_SPACEAFTER = 128;

                public const int PFM_LINESPACING = 256;

                public const int EM_SETPARAFORMAT = 1095;

                public const int WM_USER = 0x0400;

                public const int WS_EX_TRANSPARENT = 0x20;
            }
            #endregion
        }
    }


    public static class Graphics_DrawRtfText {
        private static RichTextBoxDrawer rtfDrawer;
        public static void DrawRtfText(this Graphics graphics, string rtf, RectangleF layoutArea, float xFactor) {
            if (Graphics_DrawRtfText.rtfDrawer == null) {
                Graphics_DrawRtfText.rtfDrawer = new RichTextBoxDrawer();
            }
            Graphics_DrawRtfText.rtfDrawer.Rtf = rtf;
            Graphics_DrawRtfText.rtfDrawer.Draw(graphics, layoutArea, xFactor);
        }

        private class RichTextBoxDrawer : RichTextBox {
            //Code converted from code found here: http://support.microsoft.com/kb/812425/en-us

            //Convert the unit used by the .NET framework (1/100 inch) 
            //and the unit used by Win32 API calls (twips 1/1440 inch)
            private const double anInch = 14.4;

            public void LineSpace() {
                SafeNativeMethods.PARAFORMAT2 fmt = new SafeNativeMethods.PARAFORMAT2();
                fmt.cbSize = Marshal.SizeOf(fmt);
                fmt.dwMask |= SafeNativeMethods.PFM_LINESPACING | SafeNativeMethods.PFM_SPACEAFTER;

                fmt.dyLineSpacing = (int)(22 * this.SelectionFont.SizeInPoints); // in twips
                                                                                 // specify exact line spacing
                fmt.bLineSpacingRule = Convert.ToByte(4);
                SafeNativeMethods.SendMessage(this.Handle, SafeNativeMethods.EM_SETPARAFORMAT, 0, ref fmt); // 0 - to all text. 1 - only to seleted
            }

            public void LineSpace(int _linespace) {
                SafeNativeMethods.PARAFORMAT2 fmt = new SafeNativeMethods.PARAFORMAT2();
                fmt.cbSize = Marshal.SizeOf(fmt);
                fmt.dwMask |= SafeNativeMethods.PFM_LINESPACING | SafeNativeMethods.PFM_SPACEAFTER;

                fmt.dyLineSpacing = _linespace; // in twips
                                                // specify exact line spacing
                fmt.bLineSpacingRule = Convert.ToByte(4);
                SafeNativeMethods.SendMessage(this.Handle, SafeNativeMethods.EM_SETPARAFORMAT, 0, ref fmt); // 0 - to all text. 1 - only to seleted
            }

            protected override CreateParams CreateParams {
                get {
                    CreateParams createParams = base.CreateParams;
                    if (SafeNativeMethods.LoadLibrary("msftedit.dll") != IntPtr.Zero) {
                        createParams.ExStyle |= SafeNativeMethods.WS_EX_TRANSPARENT; // transparent
                        createParams.ClassName = "RICHEDIT50W";
                    }
                    return createParams;
                }
            }

            private void DPToHIMETRIC(Graphics graphics, ref SizeF size) {
                size.Width = (size.Width * 2540.0f) / graphics.DpiX;
                size.Height = (size.Height * 2540.0f) / graphics.DpiY;
            }

            public void Draw(Graphics graphics, RectangleF layoutArea, float xFactor) {
                System.Diagnostics.Debug.WriteLine("LayoutArea " + layoutArea);

                SizeF metaSize = layoutArea.Size;
                DPToHIMETRIC(graphics, ref metaSize);

                System.Diagnostics.Debug.WriteLine("MetaSize " + metaSize);

                IntPtr hdc = graphics.GetHdc();

                //create a metafile, convert the size to himetrics
                Metafile metafile = new Metafile(hdc, new RectangleF(0, 0, metaSize.Width, metaSize.Height));

                graphics.ReleaseHdc(hdc);

                Graphics g = Graphics.FromImage(metafile);
                IntPtr hDCEMF = g.GetHdc();

                //Calculate the area to render.
                SafeNativeMethods.RECT rectLayoutArea;
                rectLayoutArea.Left = 0;
                rectLayoutArea.Top = 0;
                rectLayoutArea.Right = (int)((1440 * metaSize.Width + 2540 / 2) / 2540);
                rectLayoutArea.Bottom = (int)((1440 * metaSize.Height + 2540 / 2) / 2540);

                System.Diagnostics.Debug.WriteLine(String.Format("RectLayoutArea ({0},{1})", rectLayoutArea.Right, rectLayoutArea.Bottom));

                SafeNativeMethods.FORMATRANGE fmtRange;
                fmtRange.chrg.cpMax = -1;            //Indicate character from to character to 
                fmtRange.chrg.cpMin = 0;
                fmtRange.hdc = hDCEMF;                  //Use the same DC for measuring and rendering
                fmtRange.hdcTarget = hDCEMF;         //Point at printer hDC
                fmtRange.rc = rectLayoutArea;        //Indicate the area on page to print
                fmtRange.rcPage = rectLayoutArea;    //Indicate size of page

                IntPtr wParam = IntPtr.Zero;
                wParam = new IntPtr(1);

                //Get the pointer to the FORMATRANGE structure in memory
                IntPtr lParam = IntPtr.Zero;
                lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange));
                Marshal.StructureToPtr(fmtRange, lParam, false);

                SafeNativeMethods.SendMessage(this.Handle, SafeNativeMethods.EM_FORMATRANGE, wParam, lParam);
                SafeNativeMethods.SendMessage(this.Handle, SafeNativeMethods.EM_FORMATRANGE, wParam, IntPtr.Zero);

                //Free the block of memory allocated
                Marshal.FreeCoTaskMem(lParam);

                //Release the device context handle obtained by a previous call
                g.ReleaseHdc(hDCEMF);
                g.Dispose();

                hdc = graphics.GetHdc();
                int nHorzSize = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.HORZSIZE);
                int nVertSize = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.VERTSIZE);
                int nHorzRes = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.HORZRES);
                int nVertRes = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.VERTRES);
                int nLogPixelsX = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.LOGPIXELSX);
                int nLogPixelsY = SafeNativeMethods.GetDeviceCaps(hdc, SafeNativeMethods.DeviceCap.LOGPIXELSY);
                graphics.ReleaseHdc(hdc);

                float fHorzSizeInches = nHorzSize / 25.4f;
                float fVertSizeInches = nVertSize / 25.4f;
                float fHorzFudgeFactor = (nHorzRes / fHorzSizeInches) / nLogPixelsX;
                float fVertFudgeFactor = (nVertRes / fVertSizeInches) / nLogPixelsY;

                System.Diagnostics.Debug.WriteLine("Fudge Factor " + fHorzFudgeFactor.ToString() + " " + fVertFudgeFactor.ToString() + " XFactor " + xFactor.ToString());

                Pen RedPen = new Pen(Color.Red);
                graphics.DrawRectangle(RedPen, layoutArea.X * xFactor, layoutArea.Y * xFactor, layoutArea.Width * xFactor, layoutArea.Height * xFactor);

                float Left = layoutArea.Left;
                float Top = layoutArea.Top;
                //layoutArea.X = layoutArea.Y = 0;
                layoutArea.Offset(-Left, -Top);
                layoutArea.Offset(Left / fHorzFudgeFactor, Top / fVertFudgeFactor);

                System.Drawing.Drawing2D.GraphicsState state = graphics.Save();
                graphics.ScaleTransform(fHorzFudgeFactor * xFactor, fVertFudgeFactor * xFactor);
                graphics.DrawImage(metafile, layoutArea);
                graphics.Restore(state);


                System.Diagnostics.Debug.WriteLine("Layout Aread : " + layoutArea);
            }

            #region SafeNativeMethods
            private static class SafeNativeMethods {
                [DllImport("USER32.dll")]
                public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

                [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
                public static extern IntPtr LoadLibrary(string lpFileName);

                [DllImport("gdi32.dll")]
                public static extern int GetDeviceCaps(IntPtr hdc, DeviceCap nIndex);

                [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
                public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref PARAFORMAT2 lParam);

                [StructLayout(LayoutKind.Sequential)]
                public struct RECT {
                    public int Left;
                    public int Top;
                    public int Right;
                    public int Bottom;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct CHARRANGE {
                    public int cpMin;        //First character of range (0 for start of doc)
                    public int cpMax;        //Last character of range (-1 for end of doc)
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct FORMATRANGE {
                    public IntPtr hdc;                //Actual DC to draw on
                    public IntPtr hdcTarget;    //Target DC for determining text formatting
                    public RECT rc;                        //Region of the DC to draw to (in twips)
                    public RECT rcPage;                //Region of the whole DC (page size) (in twips)
                    public CHARRANGE chrg;        //Range of text to draw (see earlier declaration)
                }

                public enum DeviceCap : int {
                    /// &lt;summary&gt;
                    /// Device driver version
                    /// &lt;/summary&gt;
                    DRIVERVERSION = 0,
                    /// &lt;summary&gt;
                    /// Device classification
                    /// &lt;/summary&gt;
                    TECHNOLOGY = 2,
                    /// &lt;summary&gt;
                    /// Horizontal size in millimeters
                    /// &lt;/summary&gt;
                    HORZSIZE = 4,
                    /// &lt;summary&gt;
                    /// Vertical size in millimeters
                    /// &lt;/summary&gt;
                    VERTSIZE = 6,
                    /// &lt;summary&gt;
                    /// Horizontal width in pixels
                    /// &lt;/summary&gt;
                    HORZRES = 8,
                    /// &lt;summary&gt;
                    /// Vertical height in pixels
                    /// &lt;/summary&gt;
                    VERTRES = 10,
                    /// &lt;summary&gt;
                    /// Number of bits per pixel
                    /// &lt;/summary&gt;
                    BITSPIXEL = 12,
                    /// &lt;summary&gt;
                    /// Number of planes
                    /// &lt;/summary&gt;
                    PLANES = 14,
                    /// &lt;summary&gt;
                    /// Number of brushes the device has
                    /// &lt;/summary&gt;
                    NUMBRUSHES = 16,
                    /// &lt;summary&gt;
                    /// Number of pens the device has
                    /// &lt;/summary&gt;
                    NUMPENS = 18,
                    /// &lt;summary&gt;
                    /// Number of markers the device has
                    /// &lt;/summary&gt;
                    NUMMARKERS = 20,
                    /// &lt;summary&gt;
                    /// Number of fonts the device has
                    /// &lt;/summary&gt;
                    NUMFONTS = 22,
                    /// &lt;summary&gt;
                    /// Number of colors the device supports
                    /// &lt;/summary&gt;
                    NUMCOLORS = 24,
                    /// &lt;summary&gt;
                    /// Size required for device descriptor
                    /// &lt;/summary&gt;
                    PDEVICESIZE = 26,
                    /// &lt;summary&gt;
                    /// Curve capabilities
                    /// &lt;/summary&gt;
                    CURVECAPS = 28,
                    /// &lt;summary&gt;
                    /// Line capabilities
                    /// &lt;/summary&gt;
                    LINECAPS = 30,
                    /// &lt;summary&gt;
                    /// Polygonal capabilities
                    /// &lt;/summary&gt;
                    POLYGONALCAPS = 32,
                    /// &lt;summary&gt;
                    /// Text capabilities
                    /// &lt;/summary&gt;
                    TEXTCAPS = 34,
                    /// &lt;summary&gt;
                    /// Clipping capabilities
                    /// &lt;/summary&gt;
                    CLIPCAPS = 36,
                    /// &lt;summary&gt;
                    /// Bitblt capabilities
                    /// &lt;/summary&gt;
                    RASTERCAPS = 38,
                    /// &lt;summary&gt;
                    /// Length of the X leg
                    /// &lt;/summary&gt;
                    ASPECTX = 40,
                    /// &lt;summary&gt;
                    /// Length of the Y leg
                    /// &lt;/summary&gt;
                    ASPECTY = 42,
                    /// &lt;summary&gt;
                    /// Length of the hypotenuse
                    /// &lt;/summary&gt;
                    ASPECTXY = 44,
                    /// &lt;summary&gt;
                    /// Shading and Blending caps
                    /// &lt;/summary&gt;
                    SHADEBLENDCAPS = 45,

                    /// &lt;summary&gt;
                    /// Logical pixels inch in X
                    /// &lt;/summary&gt;
                    LOGPIXELSX = 88,
                    /// &lt;summary&gt;
                    /// Logical pixels inch in Y
                    /// &lt;/summary&gt;
                    LOGPIXELSY = 90,

                    /// &lt;summary&gt;
                    /// Number of entries in physical palette
                    /// &lt;/summary&gt;
                    SIZEPALETTE = 104,
                    /// &lt;summary&gt;
                    /// Number of reserved entries in palette
                    /// &lt;/summary&gt;
                    NUMRESERVED = 106,
                    /// &lt;summary&gt;
                    /// Actual color resolution
                    /// &lt;/summary&gt;
                    COLORRES = 108,

                    // Printing related DeviceCaps. These replace the appropriate Escapes
                    /// &lt;summary&gt;
                    /// Physical Width in device units
                    /// &lt;/summary&gt;
                    PHYSICALWIDTH = 110,
                    /// &lt;summary&gt;
                    /// Physical Height in device units
                    /// &lt;/summary&gt;
                    PHYSICALHEIGHT = 111,
                    /// &lt;summary&gt;
                    /// Physical Printable Area x margin
                    /// &lt;/summary&gt;
                    PHYSICALOFFSETX = 112,
                    /// &lt;summary&gt;
                    /// Physical Printable Area y margin
                    /// &lt;/summary&gt;
                    PHYSICALOFFSETY = 113,
                    /// &lt;summary&gt;
                    /// Scaling factor x
                    /// &lt;/summary&gt;
                    SCALINGFACTORX = 114,
                    /// &lt;summary&gt;
                    /// Scaling factor y
                    /// &lt;/summary&gt;
                    SCALINGFACTORY = 115,

                    /// &lt;summary&gt;
                    /// Current vertical refresh rate of the display device (for displays only) in Hz
                    /// &lt;/summary&gt;
                    VREFRESH = 116,
                    /// &lt;summary&gt;
                    /// Horizontal width of entire desktop in pixels
                    /// &lt;/summary&gt;
                    DESKTOPVERTRES = 117,
                    /// &lt;summary&gt;
                    /// Vertical height of entire desktop in pixels
                    /// &lt;/summary&gt;
                    DESKTOPHORZRES = 118,
                    /// &lt;summary&gt;
                    /// Preferred blt alignment
                    /// &lt;/summary&gt;
                    BLTALIGNMENT = 119
                }

                public struct PARAFORMAT2 {

                    public int cbSize;

                    public uint dwMask;

                    public short wNumbering;

                    public short wReserved;

                    public int dxStartIndent;

                    public int dxRightIndent;

                    public int dxOffset;

                    public short wAlignment;

                    public short cTabCount;

                    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]

                    public int[] rgxTabs;

                    // PARAFORMAT2 from here onwards.

                    public int dySpaceBefore;

                    public int dySpaceAfter;

                    public int dyLineSpacing;

                    public short sStyle;

                    public byte bLineSpacingRule;

                    public byte bOutlineLevel;

                    public short wShadingWeight;

                    public short wShadingStyle;

                    public short wNumberingStart;

                    public short wNumberingStyle;

                    public short wNumberingTab;

                    public short wBorderSpace;

                    public short wBorderWidth;

                    public short wBorders;

                }

                public const int EM_FORMATRANGE = WM_USER + 57;

                public const int PFM_SPACEAFTER = 128;

                public const int PFM_LINESPACING = 256;

                public const int EM_SETPARAFORMAT = 1095;

                public const int WM_USER = 0x0400;

                public const int WS_EX_TRANSPARENT = 0x20;

            }
            #endregion
        }
    }
}
