using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvOverlayContainerView : MpAvUserControl {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvOverlayContainerView _instance;
        public static MpAvOverlayContainerView Instance =>
            _instance;
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        ObservableCollection<MpAvChildWindow> Children { get; set; } = [];

        #endregion

        #region Constructors
        public MpAvOverlayContainerView() {
            MpDebug.Assert(_instance == null, "Singleton error");
            _instance = this;
            InitializeComponent();

            this.Bind(
                Control.IsHitTestVisibleProperty,
                new Binding() {
                    Source = OverlayContentControl,
                    Path = nameof(OverlayContentControl.Content),
                    Converter = ObjectConverters.IsNotNull
                });
        }

        #endregion

        #region Public Methods
        public void ShowWindow(MpAvChildWindow cw) {
#if WINDOWED
            if (cw is MpAvMainView || cw is MpAvMainWindow) {
                return;
            }
#endif
            if(Children.Contains(cw)) {
                Children.Move(Children.IndexOf(cw), 0);
            } else {
                Children.Insert(0, cw);
            }
            MpConsole.WriteLine($"Child window '{cw.Title}' shown");

            SetContent(Children.FirstOrDefault(),true);
        }

        public bool RemoveWindow(MpAvChildWindow cw) {
#if WINDOWED
            if (cw is MpAvMainView) {
                return false;
            }
#endif
            int idx = Children.IndexOf(cw);
            if(idx < 0) {
                return false;
            }
            Children.RemoveAt(idx);
            SetContent(Children.FirstOrDefault(),false);

            if(Children.FirstOrDefault() is MpAvChildWindow ncw) {
                ncw.Activate();
            } else {
#if MOBILE_OR_WINDOWED
                MpAvMainView.Instance.Activate(); 
#endif
            }
            return true;
        }
        #endregion

        #region Protected Methods

        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);

            if (e.Source is not Control sc ||
                sc.GetSelfAndVisualAncestors().Any(x => x == OverlayContentControl.Content) ||
                OverlayContentControl.Content is not MpAvChildWindow cw) {
                // touch on overlay don't close
                return;
            }
            cw.Close();
        }
        #endregion

        #region Private Methods
        private void SetContent(MpAvChildWindow newContent, bool isShow) {
            MpAvChildWindow last_content = OverlayContentControl.Content as MpAvChildWindow;
            IPageTransition transition =
                isShow ?
                    newContent.OpenTransition :
                    last_content == null ?
                        default :
                        last_content.CloseTransition;
            OverlayContentControl.PageTransition = transition;
            OverlayContentControl.Content = newContent;
        }

        #endregion

        #region Commands
        #endregion



        
    }
}
