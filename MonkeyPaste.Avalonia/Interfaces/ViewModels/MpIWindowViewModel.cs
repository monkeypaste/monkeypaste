﻿using Avalonia.Controls;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public interface MpIWindowViewModel : MpIViewModel {
        MpWindowType WindowType { get; }
    }
    public interface MpIWindowStateViewModel : MpIWindowViewModel {
        WindowState WindowState { get; set; }
    }
    public interface MpIIsAnimatedWindowViewModel : MpIWindowViewModel {
        bool IsAnimated { get; }
        bool IsAnimating { get; set; }
        bool IsComplete { get; }
    }
    public interface MpIWindowBoundsObserverViewModel : MpIWindowViewModel {
        MpRect Bounds { get; set; }
        MpRect LastBounds { get; set; }
    }
    public interface MpIActiveWindowViewModel : MpIWindowViewModel {
        bool IsWindowActive { get; set; }
    }
    public interface MpICloseWindowViewModel : MpIWindowViewModel {
        bool IsWindowOpen { get; set; }
    }
    public interface MpIWindowHandlesClosingViewModel : MpIWindowViewModel {
        bool IsWindowCloseHandled { get; }
    }
    public interface MpIWantsTopmostWindowViewModel : MpIWindowViewModel {
        bool WantsTopmost { get; }
    }

}
