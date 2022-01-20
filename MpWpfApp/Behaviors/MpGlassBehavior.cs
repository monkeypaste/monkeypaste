
using Microsoft.Xaml.Behaviors;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using static MpWpfApp.WinApi;

namespace MpWpfApp {
    public class MpGlassBehavior : MpBehavior<Window> {

        #region Properties

        private bool _isGlassEnabled = false;
        public bool IsGlassEnabled {
            get => _isGlassEnabled;
            set {
                if (_isGlassEnabled != value) {
                    _isGlassEnabled = value;
                    MpHelpers.RunOnMainThread(async () => {
                        while(!_isLoaded) { await Task.Delay(100); }

                        if (_isGlassEnabled) {
                            EnableBlur();
                        } else {
                            DisableBlur();
                        }
                    });                
                }
            }
        }

        #endregion

        #region Constructors

        #endregion

        #region Private Methods

        private void EnableBlur() {
            SetAccentPolicy(AccentState.ACCENT_ENABLE_BLURBEHIND);
        }

        private void DisableBlur() {
            SetAccentPolicy(AccentState.ACCENT_DISABLED);
        }

        private void SetAccentPolicy(AccentState accentState) {
            var windowHelper = new WindowInteropHelper(AssociatedObject);

            var accent = new AccentPolicy {
                AccentState = accentState,
                AccentFlags = GetAccentFlagsForTaskbarPosition()
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);


            var data = new WindowCompositionAttributeData {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private AccentFlags GetAccentFlagsForTaskbarPosition() {
            return AccentFlags.DrawAllBorders;
        }

        #endregion

        #region P/Invoke

        //from https://web.archive.org/web/20160712234338/https://withinrafael.com/adding-the-aero-glass-blur-to-your-windows-10-apps/

        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        public enum WindowCompositionAttribute {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        public enum AccentState {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy {
            public AccentState AccentState;
            public AccentFlags AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        //[StructLayout(LayoutKind.Sequential)]
        //public struct AccentPolicy {
        //    public AccentState AccentState;
        //    public int AccentFlags;
        //    public int GradientColor;
        //    public int AnimationId;
        //}

        [Flags]
        public enum AccentFlags {
            // ... 
            DrawLeftBorder = 0x20,
            DrawTopBorder = 0x40,
            DrawRightBorder = 0x80,
            DrawBottomBorder = 0x100,
            DrawAllBorders = (DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder)
            // ... 
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS {
            public MARGINS(Thickness t) {
                Left = (int)t.Left;
                Right = (int)t.Right;
                Top = (int)t.Top;
                Bottom = (int)t.Bottom;
            }
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern bool DwmIsCompositionEnabled();

        public static bool ExtendGlassFrame(Window window, Thickness margin) {
            if (!DwmIsCompositionEnabled())
                return false;

            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException(
                "The Window must be shown before extending glass.");

            // Set the background to transparent from both the WPF and Win32 perspectives
            window.Background = Brushes.Transparent;
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;

            MARGINS margins = new MARGINS(margin);
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
            return true;
        }

        #endregion
    }
}
