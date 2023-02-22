using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Avalonia.Android;
using Avalonia.Platform;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Avalonia.Android;
using System;

namespace ControlCatalog.Android;

public class MpAvAdWebViewContainerBuilder : MpAvINativeControlBuilder {
    public IPlatformHandle Build(IPlatformHandle parent, Func<IPlatformHandle> createDefault, object args) {
        var parentContext = (parent as AndroidViewControlHandle)?.View.Context
            ?? global::Android.App.Application.Context;

        var container = new MpAvFrameLayout(parentContext);
        container.Left = 0;
        container.Right = 900;
        container.Top = 0;
        container.Bottom = 900;
        return new AndroidViewControlHandle(container);
    }
}

public class MpAvFrameLayout : RelativeLayout, MpIHasModifiableContent {
    public void SetContent(object content) {
        this.RemoveAllViews();
        if (content is MpAvIPlatformHandleHost host &&
            host.PlatformHandle is AndroidViewControlHandle handle &&
            handle.View is View v) {
            this.AddView(v);
        }
    }
    public MpAvFrameLayout(Context context) : base(context) {
    }

    public MpAvFrameLayout(Context context, IAttributeSet attrs) : base(context, attrs) {
    }

    public MpAvFrameLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) {
    }

    public MpAvFrameLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) {
    }

    protected MpAvFrameLayout(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
    }
}
