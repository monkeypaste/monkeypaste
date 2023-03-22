using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDataGridRefreshExtension {
        private static List<DataGrid> _enabledGrids = new List<DataGrid>();

        static MpAvDataGridRefreshExtension() {
            IsEnabledProperty.Changed.AddClassHandler<DataGrid>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties


        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, DataGrid, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(DataGrid element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is DataGrid control) {
                    if (!_enabledGrids.Contains(control)) {
                        _enabledGrids.Add(control);
                    }
                    control.DetachedFromVisualTree += DetachedFromVisualHandler;
                }
            } else {
                DetachedFromVisualHandler(element, null);
            }


        }

        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is DataGrid dg) {
                _enabledGrids.Remove(dg);
            }
        }


        #endregion

        public static void RefreshDataGrid(object dataContext) {
            var pdg =
                _enabledGrids
                .FirstOrDefault(x => x.DataContext == dataContext);
            if (pdg == null) {
                MpConsole.WriteLine($"DataGrid not found for '{dataContext}' cannot refresh");
                return;
            }
            // BUG can't get dataGrid to resize w/ row changes so hardsetting height (RowHeight=40)
            if (pdg == null) {
                return;
            }
            pdg.ApplyTemplate();
            double nh = pdg.ColumnHeaderHeight;
            foreach (var item in pdg.Items) {
                nh += pdg.RowHeight;
            }
            pdg.Height = nh;
            //pdg.InvalidateMeasure();
            var sv = pdg.GetVisualDescendant<ScrollViewer>();
            if (sv == null) {
                //Debugger.Break();
                return;
            }
            sv.ScrollByPointDelta(new MpPoint(0, 5));
            sv.ScrollByPointDelta(new MpPoint(0, -5));
        }

        #endregion
    }
}
