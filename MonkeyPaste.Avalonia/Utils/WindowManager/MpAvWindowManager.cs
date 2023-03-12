using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvWindowManager {

        public static ObservableCollection<MpAvWindow> AllWindows { get; set; } = new ObservableCollection<MpAvWindow>();

        static MpAvWindowManager() {
            AllWindows.CollectionChanged += AllWindows_CollectionChanged;
        }

        private static void AllWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (Window nw in e.NewItems) {
                    nw.Opened += Nw_Opened;
                    nw.Closed += Nw_Closed;
                    nw.Closing += Nw_Closing;
                }
            }
        }

        private static void Nw_Opened(object sender, System.EventArgs e) {
            if (sender is Window w) {
                if (w.DataContext is MpIChildWindowViewModel cwvm) {
                    cwvm.IsOpen = true;
                }
            }
        }

        private static void Nw_Closing(object sender, WindowClosingEventArgs e) {
            if (sender is Window w) {
                if (w.DataContext is MpIDisposableObject disp_obj) {

                    // NOTE used to dispose webview and cancel js
                    //disp_obj.Dispose();
                    //w.DataContext = null;
                }
                if (w.GetVisualDescendants<Control>().Where(x => x is IDisposable).Cast<IDisposable>() is IEnumerable<IDisposable> disp_controls) {
                    disp_controls.ForEach(x => x.Dispose());
                }
            }
        }

        private static void Nw_Closed(object sender, System.EventArgs e) {
            if (sender is Window w && w.DataContext is MpIChildWindowViewModel cwvm) {
                cwvm.IsOpen = false;
            }

        }
    }
}
