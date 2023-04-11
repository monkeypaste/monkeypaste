
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvPanel : Control, IChildIndexProvider {
        /// <summary>
        /// Defines the <see cref="Background"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> BackgroundProperty =
            Border.BackgroundProperty.AddOwner<MpAvPanel>();

        /// <summary>
        /// Initializes static members of the <see cref="MpAvPanel"/> class.
        /// </summary>
        static MpAvPanel() {
            AffectsRender<MpAvPanel>(BackgroundProperty);
        }

        private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MpAvPanel"/> class.
        /// </summary>
        public MpAvPanel() {
            Children.CollectionChanged += ChildrenChanged;
        }

        /// <summary>
        /// Gets the children of the <see cref="MpAvPanel"/>.
        /// </summary>
        [Content]
        public Controls Children { get; } = new Controls();

        /// <summary>
        /// Gets or Sets MpAvPanel background brush.
        /// </summary>
        public IBrush? Background {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged {
            add => _childIndexChanged += value;
            remove => _childIndexChanged -= value;
        }

        /// <summary>
        /// Renders the visual to a <see cref="DrawingContext"/>.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        public override void Render(DrawingContext context) {
            var background = Background;
            if (background != null) {
                var renderSize = Bounds.Size;
                context.FillRectangle(background, new Rect(renderSize));
            }

            base.Render(context);
        }

        /// <summary>
        /// Marks a property on a child as affecting the parent panel's arrangement.
        /// </summary>
        /// <param name="properties">The properties.</param>
        protected static void AffectsParentArrange<TPanel>(params AvaloniaProperty[] properties)
            where TPanel : MpAvPanel {
            var invalidateObserver = new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(
                static e => AffectsParentArrangeInvalidate<TPanel>(e));
            foreach (var property in properties) {
                property.Changed.Subscribe(invalidateObserver);
            }
        }

        /// <summary>
        /// Marks a property on a child as affecting the parent panel's measurement.
        /// </summary>
        /// <param name="properties">The properties.</param>
        protected static void AffectsParentMeasure<TPanel>(params AvaloniaProperty[] properties)
            where TPanel : MpAvPanel {
            var invalidateObserver = new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(
                static e => AffectsParentMeasureInvalidate<TPanel>(e));
            foreach (var property in properties) {
                property.Changed.Subscribe(invalidateObserver);
            }
        }

        /// <summary>
        /// Called when the <see cref="Children"/> collection changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            List<Control> controls;

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    controls = e.NewItems!.OfType<Control>().ToList();
                    LogicalChildren.InsertRange(e.NewStartingIndex, controls);
                    VisualChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Visual>());
                    break;

                case NotifyCollectionChangedAction.Move:
                    LogicalChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                    VisualChildren.MoveRange(e.OldStartingIndex, e.OldItems.Count, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    controls = e.OldItems!.OfType<Control>().ToList();
                    LogicalChildren.RemoveAll(controls);
                    VisualChildren.RemoveAll(e.OldItems!.OfType<Visual>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.OldItems!.Count; ++i) {
                        var index = i + e.OldStartingIndex;
                        var child = (Control)e.NewItems![i]!;
                        LogicalChildren[index] = child;
                        VisualChildren[index] = child;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }

            _childIndexChanged?.Invoke(this, (ChildIndexChangedEventArgs)ChildIndexChangedEventArgs.Empty);
            InvalidateMeasureOnChildrenChanged();
        }

        private protected virtual void InvalidateMeasureOnChildrenChanged() {
            InvalidateMeasure();
        }

        private static void AffectsParentArrangeInvalidate<TPanel>(AvaloniaPropertyChangedEventArgs e)
            where TPanel : MpAvPanel {
            var control = e.Sender as Control;
            var panel = control?.GetVisualParent() as TPanel;
            panel?.InvalidateArrange();
        }

        private static void AffectsParentMeasureInvalidate<TPanel>(AvaloniaPropertyChangedEventArgs e)
            where TPanel : MpAvPanel {
            var control = e.Sender as Control;
            var panel = control?.GetVisualParent() as TPanel;
            panel?.InvalidateMeasure();
        }

        int IChildIndexProvider.GetChildIndex(ILogical child) {
            return child is Control control ? Children.IndexOf(control) : -1;
        }

        public bool TryGetTotalCount(out int count) {
            count = Children.Count;
            return true;
        }
    }
    /// <summary>
    /// A panel that displays child controls at arbitrary locations.
    /// </summary>
    /// <remarks>
    /// Unlike other <see cref="MpAvPanel"/> implementations, the <see cref="Canvas"/> doesn't lay out
    /// its children in any particular layout. Instead, the positioning of each child control is
    /// defined by the <code>Canvas.Left</code>, <code>Canvas.Top</code>, <code>Canvas.Right</code>
    /// and <code>Canvas.Bottom</code> attached properties.
    /// </remarks>
    /// 
    [DoNotNotify]
    public class MpAvCanvas : MpAvPanel, INavigableContainer {
        /// <summary>
        /// Defines the Left attached property.
        /// </summary>
        public static readonly AttachedProperty<double> LeftProperty =
            AvaloniaProperty.RegisterAttached<MpAvCanvas, Control, double>("Left", double.NaN);

        /// <summary>
        /// Defines the Top attached property.
        /// </summary>
        public static readonly AttachedProperty<double> TopProperty =
            AvaloniaProperty.RegisterAttached<MpAvCanvas, Control, double>("Top", double.NaN);

        /// <summary>
        /// Defines the Right attached property.
        /// </summary>
        public static readonly AttachedProperty<double> RightProperty =
            AvaloniaProperty.RegisterAttached<MpAvCanvas, Control, double>("Right", double.NaN);

        /// <summary>
        /// Defines the Bottom attached property.
        /// </summary>
        public static readonly AttachedProperty<double> BottomProperty =
            AvaloniaProperty.RegisterAttached<MpAvCanvas, Control, double>("Bottom", double.NaN);

        /// <summary>
        /// Initializes static members of the <see cref="MpAvCanvas"/> class.
        /// </summary>
        static MpAvCanvas() {
            ClipToBoundsProperty.OverrideDefaultValue<MpAvCanvas>(false);
            AffectsParentArrange<MpAvCanvas>(LeftProperty, TopProperty, RightProperty, BottomProperty);
        }

        /// <summary>
        /// Gets the value of the Left attached property for a control.
        /// </summary>
        /// <param name="element">The control.</param>
        /// <returns>The control's left coordinate.</returns>
        public static double GetLeft(AvaloniaObject element) {
            return element.GetValue(LeftProperty);
        }

        /// <summary>
        /// Sets the value of the Left attached property for a control.
        /// </summary>
        /// <param name="element">The control.</param>
        /// <param name="value">The left value.</param>
        public static void SetLeft(AvaloniaObject element, double value) {
            element.SetValue(LeftProperty, value);
        }

        /// <summary>
        /// Gets the value of the Top attached property for a control.
        /// </summary>
        /// <param name="element">The control.</param>
        /// <returns>The control's top coordinate.</returns>
        public static double GetTop(AvaloniaObject element) {
            return element.GetValue(TopProperty);
        }

        /// <summary>
        /// Sets the value of the Top attached property for a control.
        /// </summary>
        /// <param name="element">The control.</param>
        /// <param name="value">The top value.</param>
        public static void SetTop(AvaloniaObject element, double value) {
            element.SetValue(TopProperty, value);
        }

        /// <summary>
        /// Gets the value of the Right attached property for a control.
        /// </summary>
        /// <param name="element">The control.</param>
        /// <returns>The control's right coordinate.</returns>
        public static double GetRight(AvaloniaObject element) {
            return element.GetValue(RightProperty);
        }

        /// <summary>
        /// Sets the value of the Right attached property for a control.
        /// </summary>
        /// <param name="element">The control.</param>
        /// <param name="value">The right value.</param>
        public static void SetRight(AvaloniaObject element, double value) {
            element.SetValue(RightProperty, value);
        }

        /// <summary>
        /// Gets the value of the Bottom attached property for a control.
        /// </summary>
        /// <param name="element">The control.</param>
        /// <returns>The control's bottom coordinate.</returns>
        public static double GetBottom(AvaloniaObject element) {
            return element.GetValue(BottomProperty);
        }

        /// <summary>
        /// Sets the value of the Bottom attached property for a control.
        /// </summary>
        /// <param name="element">The control.</param>
        /// <param name="value">The bottom value.</param>
        public static void SetBottom(AvaloniaObject element, double value) {
            element.SetValue(BottomProperty, value);
        }

        /// <summary>
        /// Gets the next control in the specified direction.
        /// </summary>
        /// <param name="direction">The movement direction.</param>
        /// <param name="from">The control from which movement begins.</param>
        /// <param name="wrap">Whether to wrap around when the first or last item is reached.</param>
        /// <returns>The control.</returns>
        IInputElement? INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap) {
            // TODO: Implement this
            return null;
        }

        /// <summary>
        /// Measures the control.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The desired size of the control.</returns>
        protected override Size MeasureOverride(Size availableSize) {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (Control child in Children) {
                child.Measure(availableSize);
            }

            return new Size();
        }

        /// <summary>
        /// Arranges a single child.
        /// </summary>
        /// <param name="child">The child to arrange.</param>
        /// <param name="finalSize">The size allocated to the canvas.</param>
        protected virtual void ArrangeChild(Control child, Size finalSize) {
            double x = 0.0;
            double y = 0.0;
            double elementLeft = GetLeft(child);

            if (!double.IsNaN(elementLeft)) {
                x = elementLeft;
            } else {
                // Arrange with right.
                double elementRight = GetRight(child);
                if (!double.IsNaN(elementRight)) {
                    x = finalSize.Width - child.DesiredSize.Width - elementRight;
                }
            }

            double elementTop = GetTop(child);
            if (!double.IsNaN(elementTop)) {
                y = elementTop;
            } else {
                double elementBottom = GetBottom(child);
                if (!double.IsNaN(elementBottom)) {
                    y = finalSize.Height - child.DesiredSize.Height - elementBottom;
                }
            }

            child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
        }

        /// <summary>
        /// Arranges the control's children.
        /// </summary>
        /// <param name="finalSize">The size allocated to the control.</param>
        /// <returns>The space taken.</returns>
        protected override Size ArrangeOverride(Size finalSize) {
            foreach (Control child in Children) {
                ArrangeChild(child, finalSize);
            }

            return finalSize;
        }
    }
}
