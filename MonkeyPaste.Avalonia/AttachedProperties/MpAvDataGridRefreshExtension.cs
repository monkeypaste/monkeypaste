using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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
                if (element is DataGrid dg) {
                    if (!_enabledGrids.Contains(dg)) {
                        _enabledGrids.Add(dg);
                    }
                    dg.DetachedFromVisualTree += DetachedFromVisualHandler;
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
                // work around for shortcut grid since shortcutview's datacontext is the collection itself 
                // so check parent
                pdg = _enabledGrids
                    .FirstOrDefault(x => x.DataContext is IEnumerable<MpAvViewModelBase> dcl && dcl.Any() && dcl.FirstOrDefault().ParentObj == dataContext);
                if (pdg == null) {

                    return;
                } else {

                }

            }
            // BUG can't get dataGrid to resize w/ row changes so hardsetting height (RowHeight=40)
            if (pdg == null) {
                return;
            }
            pdg.ApplyTemplate();
            double nh = pdg.ColumnHeaderHeight;
            foreach (var item in pdg.ItemsSource) {
                nh += pdg.RowHeight;
            }
            pdg.Height = nh;

            //pdg.InvalidateMeasure();
            var sv = pdg.GetVisualDescendant<ScrollViewer>();
            if (sv == null) {
                //MpDebug.Break();
                return;
            }
            sv.ScrollByPointDelta(new MpPoint(0, 5));
            sv.ScrollByPointDelta(new MpPoint(0, -5));

        }

        #endregion
    }
}
