﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIWebViewInterop {
        void SendMessage(MpAvIPlatformHandleHost nwvh, string msg);
        Task<string> SendMessageAsync(MpAvIPlatformHandleHost nwvh, string msg);

        void ReceiveMessage(string bindingName, string msg);

        void Bind(MpIWebViewBindable handler);

    }
    public interface MpIOffscreenRenderSourceHost {
        MpIOffscreenRenderSource RenderSource { get; }
    }
    public interface MpIOffscreenRenderSource {
        byte[] Buffer { get; }
    }
    public interface MpIWebViewBindable {
        event EventHandler<string> OnNavigateRequest;
        void OnNavigated(string url);
    }
    public interface MpIAsyncJsEvalTracker {
        int PendingEvals { get; set; }
    }
    public interface MpAvIPlatformHandleHost {
        IPlatformHandle PlatformHandle { get; }
    }
    public interface MpIWebViewNavigator {
        void Navigate(string url);
    }

    public interface MpIEmbedHost { }
    public interface MpIWebViewHost : MpIEmbedHost {
        void Render();
        MpAvIWebViewBindingResponseHandler BindingHandler { get; }
    }

    [DoNotNotify]
    public abstract class MpAvNativeWebViewHost :
        NativeControlHost,
        MpIWebViewHost,
        MpIWebViewBindable,
        MpIAsyncJsEvalTracker,
        MpAvIPlatformHandleHost,
        MpIHasDevTools {


        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        public static MpAvINativeControlBuilder Implementation { get; set; }
        public static MpAvIWebViewInterop Interop =>
            Implementation as MpAvIWebViewInterop;

        static MpAvNativeWebViewHost() {

        }
        #endregion

        #region Interfaces

        #region MpIWebViewHost Implementation
        void MpIWebViewHost.Render() {
            Dispatcher.UIThread.Post(this.InvalidateVisual);
        }


        public abstract MpAvIWebViewBindingResponseHandler BindingHandler { get; }
        #endregion

        #region MpIAsyncJsEvalTracker Implementation

        public int PendingEvals { get; set; }

        #endregion

        #region MpIWebViewBindable Implementation
        public event EventHandler<string> OnNavigateRequest;
        public virtual void OnNavigated(string url) {
            Dispatcher.UIThread.Post(this.InvalidateVisual);
        }

        #endregion

        #region MpIHasDevTools Implementation
        public void ShowDevTools() { }
        #endregion

        #endregion

        #region Properties

        #region State
        public IPlatformHandle PlatformHandle { get; private set; }

        #endregion

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public void Navigate(string url) {
            OnNavigateRequest?.Invoke(this, url);
        }

        public override void Render(global::Avalonia.Media.DrawingContext context) {
            if (!MpAvMainWindowViewModel.Instance.IsMainWindowActive) {
                return;
            }
            if (PlatformHandle is MpIOffscreenRenderSourceHost osrsh &&
                    osrsh.RenderSource is MpIOffscreenRenderSource osrs &&
                    osrs.Buffer != null &&
                    osrs.Buffer.Length > 0) {

                context.DrawImage(osrs.Buffer.ToAvBitmap(), this.Bounds);
            } else {
                //base.Render(context);
            }
        }
        #endregion

        #region Protected Methods
        protected int PendingEvalCount() {
            return PendingEvals;
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
            if (Implementation == null) {
                PlatformHandle = base.CreateNativeControlCore(parent);
            } else {
                PlatformHandle =
                    Implementation.Build(
                        parent,
                        () => base.CreateNativeControlCore(parent),
                        this);

            }
            if (Interop != null &&
                PlatformHandle != null) {
                Interop.Bind(this);
                Navigate(MpAvClipTrayViewModel.EditorUri);
            }

            return PlatformHandle;
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control) {
            base.DestroyNativeControlCore(control);
        }

        //protected override void OnPointerPressed(PointerPressedEventArgs e) {
        //    Dispatcher.UIThread.Post(() => {
        //        base.OnPointerPressed(e);
        //        if (DataContext is MpAvClipTileViewModel ctvm) {
        //            MpAvClipTrayViewModel.Instance.SelectClipTileCommand.Execute(ctvm);
        //        }
        //    });
        //}

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}