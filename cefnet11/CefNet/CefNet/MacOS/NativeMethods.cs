using System;
using System.Runtime.InteropServices;

namespace CefNet.MacOS
{
	static class NativeMethods
	{
		[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
		public static extern uint CGMainDisplayID();

		[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
		public static extern nint CGDisplayPixelsWide(uint display);

		[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
		public static extern nint CGDisplayPixelsHigh(uint display);
	}
}

