using AngleSharp.Dom;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvOrientedGridExtension {
        #region Private Variables

        #endregion

        #region Statics
        static MpAvOrientedGridExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            OrientationProperty.Changed.AddClassHandler<Control>((x, y) => OrientGrid(x as Grid));

#if DEBUG
            // NOTE only handle these in debug for perf since they're readonly values
            HorizontalRowDefinitionsProperty.Changed.AddClassHandler<Control>((x, y) => OrientGrid(x as Grid));   
            HorizontalColumnDefinitionsProperty.Changed.AddClassHandler<Control>((x, y) => OrientGrid(x as Grid));   
            VerticalRowDefinitionsProperty.Changed.AddClassHandler<Control>((x, y) => OrientGrid(x as Grid));   
            VerticalColumnDefinitionsProperty.Changed.AddClassHandler<Control>((x, y) => OrientGrid(x as Grid));  

            HorizontalRowProperty.Changed.AddClassHandler<Control>((x, y) => OrientChild(x));  
            HorizontalColumnProperty.Changed.AddClassHandler<Control>((x, y) => OrientChild(x));  
            VerticalRowProperty.Changed.AddClassHandler<Control>((x, y) => OrientChild(x));  
            VerticalColumnProperty.Changed.AddClassHandler<Control>((x, y) => OrientChild(x));  
            HorizontalRowSpanProperty.Changed.AddClassHandler<Control>((x, y) => OrientChild(x));
            HorizontalColumnSpanProperty.Changed.AddClassHandler<Control>((x, y) => OrientChild(x));
            VerticalRowSpanProperty.Changed.AddClassHandler<Control>((x, y) => OrientChild(x));
            VerticalColumnSpanProperty.Changed.AddClassHandler<Control>((x, y) => OrientChild(x));
#endif
        }

        #endregion

        #region Properties

        #region Definitions

        #region HorizontalRowDefinitions AvaloniaProperty
        public static RowDefinitions GetHorizontalRowDefinitions(AvaloniaObject obj) {
            return obj.GetValue(HorizontalRowDefinitionsProperty);
        }

        public static void SetHorizontalRowDefinitions(AvaloniaObject obj, RowDefinitions value) {
            obj.SetValue(HorizontalRowDefinitionsProperty, value);
        }

        public static readonly AttachedProperty<RowDefinitions> HorizontalRowDefinitionsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, RowDefinitions>(
                "HorizontalRowDefinitions",
                default);

        #endregion
        
        #region VerticalRowDefinitions AvaloniaProperty
        public static RowDefinitions GetVerticalRowDefinitions(AvaloniaObject obj) {
            return obj.GetValue(VerticalRowDefinitionsProperty);
        }

        public static void SetVerticalRowDefinitions(AvaloniaObject obj, RowDefinitions value) {
            obj.SetValue(VerticalRowDefinitionsProperty, value);
        }

        public static readonly AttachedProperty<RowDefinitions> VerticalRowDefinitionsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, RowDefinitions>(
                "VerticalRowDefinitions",
                default);

        #endregion

        #region HorizontalColumnDefinitions AvaloniaProperty
        public static ColumnDefinitions GetHorizontalColumnDefinitions(AvaloniaObject obj) {
            return obj.GetValue(HorizontalColumnDefinitionsProperty);
        }

        public static void SetHorizontalColumnDefinitions(AvaloniaObject obj, ColumnDefinitions value) {
            obj.SetValue(HorizontalColumnDefinitionsProperty, value);
        }

        public static readonly AttachedProperty<ColumnDefinitions> HorizontalColumnDefinitionsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ColumnDefinitions>(
                "HorizontalColumnDefinitions",
                default);

        #endregion

        #region VerticalColumnDefinitions AvaloniaProperty
        public static ColumnDefinitions GetVerticalColumnDefinitions(AvaloniaObject obj) {
            return obj.GetValue(VerticalColumnDefinitionsProperty);
        }

        public static void SetVerticalColumnDefinitions(AvaloniaObject obj, ColumnDefinitions value) {
            obj.SetValue(VerticalColumnDefinitionsProperty, value);
        }

        public static readonly AttachedProperty<ColumnDefinitions> VerticalColumnDefinitionsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ColumnDefinitions>(
                "VerticalColumnDefinitions",
                default);

        #endregion

        #endregion

        #region Assignments

        #region Cell

        #region HorizontalRow AvaloniaProperty
        public static int GetHorizontalRow(AvaloniaObject obj) {
            return obj.GetValue(HorizontalRowProperty);
        }

        public static void SetHorizontalRow(AvaloniaObject obj, int value) {
            obj.SetValue(HorizontalRowProperty, value);
        }

        public static readonly AttachedProperty<int> HorizontalRowProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "HorizontalRow",
                0);

        #endregion

        #region VerticalRow AvaloniaProperty
        public static int GetVerticalRow(AvaloniaObject obj) {
            return obj.GetValue(VerticalRowProperty);
        }

        public static void SetVerticalRow(AvaloniaObject obj, int value) {
            obj.SetValue(VerticalRowProperty, value);
        }

        public static readonly AttachedProperty<int> VerticalRowProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "VerticalRow",
                0);

        #endregion

        #region HorizontalColumn AvaloniaProperty
        public static int GetHorizontalColumn(AvaloniaObject obj) {
            return obj.GetValue(HorizontalColumnProperty);
        }

        public static void SetHorizontalColumn(AvaloniaObject obj, int value) {
            obj.SetValue(HorizontalColumnProperty, value);
        }

        public static readonly AttachedProperty<int> HorizontalColumnProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "HorizontalColumn",
                0);

        #endregion

        #region VerticalColumn AvaloniaProperty
        public static int GetVerticalColumn(AvaloniaObject obj) {
            return obj.GetValue(VerticalColumnProperty);
        }

        public static void SetVerticalColumn(AvaloniaObject obj, int value) {
            obj.SetValue(VerticalColumnProperty, value);
        }

        public static readonly AttachedProperty<int> VerticalColumnProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "VerticalColumn",
                0);

        #endregion
        #endregion

        #region Spans

        #region HorizontalRowSpan AvaloniaProperty
        public static int GetHorizontalRowSpan(AvaloniaObject obj) {
            return obj.GetValue(HorizontalRowSpanProperty);
        }

        public static void SetHorizontalRowSpan(AvaloniaObject obj, int value) {
            obj.SetValue(HorizontalRowSpanProperty, value);
        }

        public static readonly AttachedProperty<int> HorizontalRowSpanProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "HorizontalRowSpan",
                1);

        #endregion

        #region VerticalRowSpan AvaloniaProperty
        public static int GetVerticalRowSpan(AvaloniaObject obj) {
            return obj.GetValue(VerticalRowSpanProperty);
        }

        public static void SetVerticalRowSpan(AvaloniaObject obj, int value) {
            obj.SetValue(VerticalRowSpanProperty, value);
        }

        public static readonly AttachedProperty<int> VerticalRowSpanProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "VerticalRowSpan",
                1);

        #endregion

        #region HorizontalColumnSpan AvaloniaProperty
        public static int GetHorizontalColumnSpan(AvaloniaObject obj) {
            return obj.GetValue(HorizontalColumnSpanProperty);
        }

        public static void SetHorizontalColumnSpan(AvaloniaObject obj, int value) {
            obj.SetValue(HorizontalColumnSpanProperty, value);
        }

        public static readonly AttachedProperty<int> HorizontalColumnSpanProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "HorizontalColumnSpan",
                1);

        #endregion

        #region VerticalColumnSpan AvaloniaProperty
        public static int GetVerticalColumnSpan(AvaloniaObject obj) {
            return obj.GetValue(VerticalColumnSpanProperty);
        }

        public static void SetVerticalColumnSpan(AvaloniaObject obj, int value) {
            obj.SetValue(VerticalColumnSpanProperty, value);
        }

        public static readonly AttachedProperty<int> VerticalColumnSpanProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "VerticalColumnSpan",
                1);

        #endregion

        #endregion

        #endregion

        #region Orientation AvaloniaProperty
        public static Orientation? GetOrientation(AvaloniaObject obj) {
            return obj.GetValue(OrientationProperty);
        }

        public static void SetOrientation(AvaloniaObject obj, Orientation? value) {
            obj.SetValue(OrientationProperty, value);
        }

        public static readonly AttachedProperty<Orientation?> OrientationProperty =
            AvaloniaProperty.RegisterAttached<object, Control, Orientation?>(
                "Orientation",
                null);

        private static void HandleOrientationChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if(!GetIsEnabled(element) ||
                element is not Grid grid) {
                return;
            }
            MpConsole.WriteLine($"Grid '{grid.Name}' orientation changed to: {GetOrientation(grid)}");
            OrientGrid(grid);
        }
        #endregion

        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEnabled",
                false);

        private static void HandleIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if(element is not Grid) {
                // Only grid handles IsEnabledChanged
                return;
            }

            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Grid grid) {
                    grid.Loaded += Grid_LoadedOrEnabled;
                    if (grid.IsLoaded) {
                        Grid_LoadedOrEnabled(grid, null);
                    }
                }
            } else {
                Grid_UnloadedOrDisabled(element, null);
            }
        }
        #endregion

        #endregion
        
        #region Private Methods

        private static void Grid_LoadedOrEnabled(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not Grid grid) {
                return;
            }
            grid.Unloaded += Grid_UnloadedOrDisabled;
            grid.Children.CollectionChanged += Children_CollectionChanged;
            OrientGrid(grid);
        }


        private static void Grid_UnloadedOrDisabled(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not Grid grid) {
                return;
            }
            grid.Loaded -= Grid_LoadedOrEnabled;
            grid.Unloaded -= Grid_UnloadedOrDisabled;
            grid.Children.CollectionChanged -= Children_CollectionChanged;
        }

        private static void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if(sender is not Grid grid) {
                return;
            }
            e.NewItems.OfType<Control>().ForEach(x => OrientChild(x));
        }


        #region Helpers

        private static void OrientGrid(Grid grid) {
            if(GetOrientation(grid) is not { } orientation) {
                return;
            }
            
            var rd =
                orientation == Orientation.Horizontal ?
                    GetHorizontalRowDefinitions(grid) :
                    GetVerticalRowDefinitions(grid);
            var cd =
                orientation == Orientation.Horizontal ?
                    GetHorizontalColumnDefinitions(grid) :
                    GetVerticalColumnDefinitions(grid);
            grid.RowDefinitions.Clear();
            grid.RowDefinitions.AddRange(rd ?? new RowDefinitions("*"));
            grid.ColumnDefinitions.Clear();
            grid.ColumnDefinitions.AddRange(cd ?? new ColumnDefinitions("*"));

            MpConsole.WriteLine($"Grid '{grid.Name}' Orientation: '{orientation}'");
            MpConsole.WriteLine($"Grid '{grid.Name}' RowDefs: '{grid.RowDefinitions}'");
            MpConsole.WriteLine($"Grid '{grid.Name}' ColDefs: '{grid.ColumnDefinitions}'");

            foreach (var child in grid.Children) {
                OrientChild(child);
            }
        }
        private static void OrientChild(Control child) {
            if(GetContainerGrid(child) is not Grid grid ||
                GetOrientation(grid) is not { } orientation) {
                return;
            }

            int r = orientation == Orientation.Horizontal ? GetHorizontalRow(child) : GetVerticalRow(child);
            int c = orientation == Orientation.Horizontal ? GetHorizontalColumn(child) : GetVerticalColumn(child);
            int rs = orientation == Orientation.Horizontal ? GetHorizontalRowSpan(child) : GetVerticalRowSpan(child);
            int cs = orientation == Orientation.Horizontal ? GetHorizontalColumnSpan(child) : GetVerticalColumnSpan(child);

            Grid.SetRow(child, r);
            Grid.SetColumn(child, c);
            Grid.SetRowSpan(child, rs);
            Grid.SetColumnSpan(child, cs);
            MpConsole.WriteLine($"Control '{child.Name}' R: {r} C: {c} RS: {rs} CS: {cs}");
        }
        private static Grid GetContainerGrid(Control element) {
            if (element.Parent is not Grid grid) {
                return null;
            }
            return grid;
        }
        #endregion
        #endregion
    }
}
