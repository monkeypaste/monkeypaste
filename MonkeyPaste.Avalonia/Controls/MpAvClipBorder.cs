using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvClipBorder : Border {
    }
    [DoNotNotify]
    public class MpAvClipBorder2 : Border, IStyleable {

        Type IStyleable.StyleKey => typeof(Border);

        private Geometry clipGeometry = null;
        private object oldClip;

        public override void Render(DrawingContext context) {
            OnApplyChildClip();
            base.Render(context);
        }

        public new Control? Child {
            get {
                return base.Child;
            }
            set {
                if (this.Child != value) {
                    if (this.Child != null) {
                        // Restore original clipping of the old child
                        this.Child.SetValue(Control.ClipProperty, oldClip);
                    }

                    if (value != null) {
                        // Store the current clipping of the new child
                        oldClip = value.GetValue(Control.ClipProperty);
                    } else {
                        // If we dont set it to null we could leak a Geometry object
                        oldClip = null;
                    }

                    base.Child = value;
                }
            }
        }
        protected override Size ArrangeOverride(Size finalSize) {
            return base.ArrangeOverride(finalSize);
        }

        protected virtual void OnApplyChildClip() {
            Control? child = this.Child;
            if (child != null) {
                // Get the geometry of a rounded rectangle border based on the BorderThickness and CornerRadius
                clipGeometry = GeometryHelper.GetRoundRectangle(
                    new Rect(Child.Bounds.Size), this.BorderThickness, this.CornerRadius);

                //clipGeometry.Freeze();
                //child.Clip = clipGeometry;

                Dispatcher.UIThread.Post(() => { child.Clip = clipGeometry; });
            }
        }
    }

    public static class GeometryHelper {
        public static Geometry GetRoundRectangle(Rect baseRect, Thickness thickness, CornerRadius cornerRadius) {
            // Normalizing the corner radius
            if (cornerRadius.TopLeft < Double.Epsilon) {
                //cornerRadius.TopLeft = 0.0;
                cornerRadius = new CornerRadius(0, cornerRadius.TopRight, cornerRadius.BottomRight, cornerRadius.BottomLeft);
            }
            if (cornerRadius.TopRight < Double.Epsilon) {
                //cornerRadius.TopRight = 0.0;
                cornerRadius = new CornerRadius(cornerRadius.TopLeft, 0, cornerRadius.BottomRight, cornerRadius.BottomLeft);
            }
            if (cornerRadius.BottomLeft < Double.Epsilon) {
                //cornerRadius.BottomLeft = 0.0;
                cornerRadius = new CornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, cornerRadius.BottomRight, 0);
            }
            if (cornerRadius.BottomRight < Double.Epsilon) {
                //cornerRadius.BottomRight = 0.0;
                cornerRadius = new CornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, 0, cornerRadius.BottomLeft);
            }

            // Taking the border thickness into account
            double leftHalf = thickness.Left * 0.5;
            if (leftHalf < Double.Epsilon)
                leftHalf = 0.0;
            double topHalf = thickness.Top * 0.5;
            if (topHalf < Double.Epsilon)
                topHalf = 0.0;
            double rightHalf = thickness.Right * 0.5;
            if (rightHalf < Double.Epsilon)
                rightHalf = 0.0;
            double bottomHalf = thickness.Bottom * 0.5;
            if (bottomHalf < Double.Epsilon)
                bottomHalf = 0.0;

            // Create the rectangles for the corners that needs to be curved in the base rectangle 
            // TopLeft Rectangle 
            Rect topLeftRect = new Rect(baseRect.Position.X,
                                        baseRect.Position.Y,
                                        Math.Max(0.0, cornerRadius.TopLeft - leftHalf),
                                        Math.Max(0.0, cornerRadius.TopLeft - rightHalf));
            // TopRight Rectangle 
            Rect topRightRect = new Rect(baseRect.Position.X + baseRect.Width - cornerRadius.TopRight + rightHalf,
                                         baseRect.Position.Y,
                                         Math.Max(0.0, cornerRadius.TopRight - rightHalf),
                                         Math.Max(0.0, cornerRadius.TopRight - topHalf));
            // BottomRight Rectangle
            Rect bottomRightRect = new Rect(baseRect.Position.X + baseRect.Width - cornerRadius.BottomRight + rightHalf,
                                            baseRect.Position.Y + baseRect.Height - cornerRadius.BottomRight + bottomHalf,
                                            Math.Max(0.0, cornerRadius.BottomRight - rightHalf),
                                            Math.Max(0.0, cornerRadius.BottomRight - bottomHalf));
            // BottomLeft Rectangle 
            Rect bottomLeftRect = new Rect(baseRect.Position.X,
                                           baseRect.Position.Y + baseRect.Height - cornerRadius.BottomLeft + bottomHalf,
                                           Math.Max(0.0, cornerRadius.BottomLeft - leftHalf),
                                           Math.Max(0.0, cornerRadius.BottomLeft - bottomHalf));

            // Adjust the width of the TopLeft and TopRight rectangles so that they are proportional to the width of the baseRect 
            if (topLeftRect.Right > topRightRect.Left) {
                double newWidth = (topLeftRect.Width / (topLeftRect.Width + topRightRect.Width)) * baseRect.Width;
                topLeftRect = new Rect(topLeftRect.Position.X, topLeftRect.Position.Y, newWidth, topLeftRect.Height);
                topRightRect = new Rect(baseRect.Left + newWidth, topRightRect.Position.Y, Math.Max(0.0, baseRect.Width - newWidth), topRightRect.Height);
            }

            // Adjust the height of the TopRight and BottomRight rectangles so that they are proportional to the height of the baseRect
            if (topRightRect.Bottom > bottomRightRect.Top) {
                double newHeight = (topRightRect.Height / (topRightRect.Height + bottomRightRect.Height)) * baseRect.Height;
                topRightRect = new Rect(topRightRect.Position.X, topRightRect.Position.Y, topRightRect.Width, newHeight);
                bottomRightRect = new Rect(bottomRightRect.Position.X, baseRect.Top + newHeight, bottomRightRect.Width, Math.Max(0.0, baseRect.Height - newHeight));
            }

            // Adjust the width of the BottomLeft and BottomRight rectangles so that they are proportional to the width of the baseRect
            if (bottomRightRect.Left < bottomLeftRect.Right) {
                double newWidth = (bottomLeftRect.Width / (bottomLeftRect.Width + bottomRightRect.Width)) * baseRect.Width;
                bottomLeftRect = new Rect(bottomLeftRect.Position.X, bottomLeftRect.Position.Y, newWidth, bottomLeftRect.Height);
                bottomRightRect = new Rect(baseRect.Left + newWidth, bottomRightRect.Position.Y, Math.Max(0.0, baseRect.Width - newWidth), bottomRightRect.Height);
            }

            // Adjust the height of the TopLeft and BottomLeft rectangles so that they are proportional to the height of the baseRect
            if (bottomLeftRect.Top < topLeftRect.Bottom) {
                double newHeight = (topLeftRect.Height / (topLeftRect.Height + bottomLeftRect.Height)) * baseRect.Height;
                topLeftRect = new Rect(topLeftRect.Position.X, topLeftRect.Position.Y, topLeftRect.Width, newHeight);
                bottomLeftRect = new Rect(bottomLeftRect.Position.X, baseRect.Top + newHeight, bottomLeftRect.Width, Math.Max(0.0, baseRect.Height - newHeight));
            }

            StreamGeometry roundedRectGeometry = new StreamGeometry();

            using (StreamGeometryContext context = roundedRectGeometry.Open()) {
                // Begin from the Bottom of the TopLeft Arc and proceed clockwise
                context.BeginFigure(topLeftRect.BottomLeft, true);
                // TopLeft Arc
                context.ArcTo(topLeftRect.TopRight, topLeftRect.Size, 0, false, SweepDirection.Clockwise);
                // Top Line
                context.LineTo(topRightRect.TopLeft);
                // TopRight Arc
                context.ArcTo(topRightRect.BottomRight, topRightRect.Size, 0, false, SweepDirection.Clockwise);
                // Right Line
                context.LineTo(bottomRightRect.TopRight);
                // BottomRight Arc
                context.ArcTo(bottomRightRect.BottomLeft, bottomRightRect.Size, 0, false, SweepDirection.Clockwise);
                // Bottom Line
                context.LineTo(bottomLeftRect.BottomRight);
                // BottomLeft Arc
                context.ArcTo(bottomLeftRect.TopLeft, bottomLeftRect.Size, 0, false, SweepDirection.Clockwise);

                context.EndFigure(true);
            }

            return roundedRectGeometry;
        }

        public static Geometry GetRoundRectangleGeometry(Rect baseRect, CornerRadius cornerRadius) {
            if (cornerRadius.TopLeft < Double.Epsilon) {
                //cornerRadius.TopLeft = 0.0;
                cornerRadius = new CornerRadius(0, cornerRadius.TopRight, cornerRadius.BottomRight, cornerRadius.BottomLeft);
            }
            if (cornerRadius.TopRight < Double.Epsilon) {
                //cornerRadius.TopRight = 0.0;
                cornerRadius = new CornerRadius(cornerRadius.TopLeft, 0, cornerRadius.BottomRight, cornerRadius.BottomLeft);
            }
            if (cornerRadius.BottomLeft < Double.Epsilon) {
                //cornerRadius.BottomLeft = 0.0;
                cornerRadius = new CornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, cornerRadius.BottomRight, 0);
            }
            if (cornerRadius.BottomRight < Double.Epsilon) {
                //cornerRadius.BottomRight = 0.0;
                cornerRadius = new CornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, 0, cornerRadius.BottomLeft);
            }

            // Create the rectangles for the corners that needs to be curved in the base rectangle
            // TopLeft Rectangle
            Rect topLeftRect = new Rect(baseRect.Position.X,
                                        baseRect.Position.Y,
                                        cornerRadius.TopLeft,
                                        cornerRadius.TopLeft);
            // TopRight Rectangle
            Rect topRightRect = new Rect(baseRect.Position.X + baseRect.Width - cornerRadius.TopRight,
                                         baseRect.Position.Y,
                                         cornerRadius.TopRight,
                                         cornerRadius.TopRight);
            // BottomRight Rectangle
            Rect bottomRightRect = new Rect(baseRect.Position.X + baseRect.Width - cornerRadius.BottomRight,
                                            baseRect.Position.Y + baseRect.Height - cornerRadius.BottomRight,
                                            cornerRadius.BottomRight,
                                            cornerRadius.BottomRight);
            // BottomLeft Rectangle
            Rect bottomLeftRect = new Rect(baseRect.Position.X,
                                           baseRect.Position.Y + baseRect.Height - cornerRadius.BottomLeft,
                                           cornerRadius.BottomLeft,
                                           cornerRadius.BottomLeft);

            // Adjust the width of the TopLeft and TopRight rectangles so that they are proportional to the width of the baseRect
            if (topLeftRect.Right > topRightRect.Left) {
                double newWidth = (topLeftRect.Width / (topLeftRect.Width + topRightRect.Width)) * baseRect.Width;
                topLeftRect = new Rect(topLeftRect.Position.X, topLeftRect.Position.Y, newWidth, topLeftRect.Height);
                topRightRect = new Rect(baseRect.Left + newWidth, topRightRect.Position.Y, Math.Max(0.0, baseRect.Width - newWidth), topRightRect.Height);
            }

            // Adjust the height of the TopRight and BottomRight rectangles so that they are proportional to the height of the baseRect
            if (topRightRect.Bottom > bottomRightRect.Top) {
                double newHeight = (topRightRect.Height / (topRightRect.Height + bottomRightRect.Height)) * baseRect.Height;
                topRightRect = new Rect(topRightRect.Position.X, topRightRect.Position.Y, topRightRect.Width, newHeight);
                bottomRightRect = new Rect(bottomRightRect.Position.X, baseRect.Top + newHeight, bottomRightRect.Width, Math.Max(0.0, baseRect.Height - newHeight));
            }

            // Adjust the width of the BottomLeft and BottomRight rectangles so that they are proportional to the width of the baseRect
            if (bottomRightRect.Left < bottomLeftRect.Right) {
                double newWidth = (bottomLeftRect.Width / (bottomLeftRect.Width + bottomRightRect.Width)) * baseRect.Width;
                bottomLeftRect = new Rect(bottomLeftRect.Position.X, bottomLeftRect.Position.Y, newWidth, bottomLeftRect.Height);
                bottomRightRect = new Rect(baseRect.Left + newWidth, bottomRightRect.Position.Y, Math.Max(0.0, baseRect.Width - newWidth), bottomRightRect.Height);
            }

            // Adjust the height of the TopLeft and BottomLeft rectangles so that they are proportional to the height of the baseRect
            if (bottomLeftRect.Top < topLeftRect.Bottom) {
                double newHeight = (topLeftRect.Height / (topLeftRect.Height + bottomLeftRect.Height)) * baseRect.Height;
                topLeftRect = new Rect(topLeftRect.Position.X, topLeftRect.Position.Y, topLeftRect.Width, newHeight);
                bottomLeftRect = new Rect(bottomLeftRect.Position.X, baseRect.Top + newHeight, bottomLeftRect.Width, Math.Max(0.0, baseRect.Height - newHeight));
            }

            StreamGeometry roundedRectGeometry = new StreamGeometry();

            using (StreamGeometryContext context = roundedRectGeometry.Open()) {
                // Begin from the Bottom of the TopLeft Arc and proceed clockwise
                context.BeginFigure(topLeftRect.BottomLeft, true);
                // TopLeft Arc
                context.ArcTo(topLeftRect.TopRight, topLeftRect.Size, 0, false, SweepDirection.Clockwise);
                // Top Line
                context.LineTo(topRightRect.TopLeft);
                // TopRight Arc
                context.ArcTo(topRightRect.BottomRight, topRightRect.Size, 0, false, SweepDirection.Clockwise);
                // Right Line
                context.LineTo(bottomRightRect.TopRight);
                // BottomRight Arc
                context.ArcTo(bottomRightRect.BottomLeft, bottomRightRect.Size, 0, false, SweepDirection.Clockwise);
                // Bottom Line
                context.LineTo(bottomLeftRect.BottomRight);
                // BottomLeft Arc
                context.ArcTo(bottomLeftRect.TopLeft, bottomLeftRect.Size, 0, false, SweepDirection.Clockwise);

                context.EndFigure(true);
            }

            return roundedRectGeometry;
        }
    }
}
