using MonkeyPaste;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpDiameterToStartPointConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Point outPoint = new Point();
            double diameter = 100;

            if(parameter is string paramStr) {
                try {
                    diameter = System.Convert.ToDouble(paramStr);
                }
                catch { }
            }
            outPoint.X = diameter / 2;
            return outPoint;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }

    public class MpScoreToSegmentPointConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Point outPoint = new Point();
            double diameter = 100;
            double score = 0;

            if (value == null) {
                return outPoint;
            }
            try {
                score = System.Convert.ToDouble(value.ToString());
            }
            catch { }

            if (parameter is string paramStr) {
                try {
                    diameter = System.Convert.ToDouble(paramStr);
                }
                catch { }
            }
            double radius = diameter / 2;
            score = score == 1 ? 0.9999 : score;

            double radians = score * 360 * Math.PI / 180;

            outPoint.X = radius + (Math.Sin(radians) * radius);
            outPoint.Y = radius + (-Math.Cos(radians) * radius);
            return outPoint;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }

    public class MpScoreToIsLargeArcConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {            
            if (value == null) {
                return false;
            }
            double score = 0;
            try {
                score = System.Convert.ToDouble(value.ToString());
            }
            catch { }

            return score >= 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }

    public class MpScoreLimiterConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return false;
            }
            double score = 0;
            try {
                score = System.Convert.ToDouble(value.ToString());
            }
            catch { }

            return Math.Min(0.94, score);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }

    public class ProgressArc : MpViewModelBase {
        public Point StartPosition { get; set; } = new Point(50, 0);
        public Point EndPosition { get; set; } = new Point(100, 0);
        public Size Radius { get; set; }
        public double Thickness { get; set; } = 10;
        public double Angle { get; set; }

        public ProgressArc() { }

        public ProgressArc(double score) {
            score = score == 1 ? 0.9999 : score;
            
            var arcCalculator = new CenteredArcCalculator(Thickness, Thickness);
            arcCalculator.Calculate(0, 1, score);

            Radius = arcCalculator.ValueCircleRadius;
            StartPosition = arcCalculator.ValueCircleStartPosition;
            EndPosition = arcCalculator.ValueCircleEndPosition;
            Angle = score * 360;
        }
    }

    public abstract class ArcCalculatorBase {
        protected const double ORIGIN = 50;
        protected double _backgroundCircleThickness;
        protected double _valueCircleThickness;
        public ArcCalculatorBase(double backgroundCircleThickness, double valueCircleThickness) {
            _backgroundCircleThickness = backgroundCircleThickness;
            _valueCircleThickness = valueCircleThickness;
        }

        public Size BackgroundCircleRadius { get; protected set; }
        public Size ValueCircleRadius { get; protected set; }

        public Point BackgroundCircleStartPosition { get; protected set; }
        public Point BackgroundCircleEndPosition { get; protected set; }

        public Point ValueCircleStartPosition { get; protected set; }
        public Point ValueCircleEndPosition { get; protected set; }

        public double ValueAngle { get; set; }

        protected double GetAngleForValue(double minValue, double maxValue, double currentValue) {
            var percent = (currentValue - minValue) * 100 / (maxValue - minValue);
            var valueInAngle = (percent / 100) * 360;
            return valueInAngle;
        }
        public abstract void Calculate(double minValue, double maxValue, double currentValue);
        protected Point GetPointForAngle(Size radiusInSize, double angle) {
            var radius = radiusInSize.Height;
            angle = angle == 360 ? 359.99 : angle;
            double angleInRadians = angle * Math.PI / 180;

            double px = ORIGIN + (Math.Sin(angleInRadians) * radius);
            double py = ORIGIN + (-Math.Cos(angleInRadians) * radius);

            return new Point(px, py);
        }
    }

    public class OutsetArcCalculator : ArcCalculatorBase {
        public OutsetArcCalculator(double backgroundCircleThickness, double valueCircleThickness) : base(backgroundCircleThickness, valueCircleThickness) {

        }

        public override void Calculate(double minValue, double maxValue, double currentValue) {
            BackgroundCircleRadius = new Size(ORIGIN - _backgroundCircleThickness / 2, ORIGIN - _backgroundCircleThickness / 2);
            ValueCircleRadius = new Size(ORIGIN - _valueCircleThickness / 2, ORIGIN - _valueCircleThickness / 2); ;

            BackgroundCircleStartPosition = GetPointForAngle(BackgroundCircleRadius, 0);
            BackgroundCircleEndPosition = GetPointForAngle(BackgroundCircleRadius, 360);

            ValueAngle = GetAngleForValue(minValue, maxValue, currentValue);

            ValueCircleStartPosition = GetPointForAngle(ValueCircleRadius, 0);
            ValueCircleEndPosition = GetPointForAngle(ValueCircleRadius, ValueAngle);
        }
    }

    public class CenteredArcCalculator : ArcCalculatorBase {
        public CenteredArcCalculator(double backgroundCircleThickness, double valueCircleThickness) : base(backgroundCircleThickness, valueCircleThickness) {

        }

        public override void Calculate(double minValue, double maxValue, double currentValue) {
            var maxThickness = Math.Max(_backgroundCircleThickness, _valueCircleThickness);

            BackgroundCircleRadius = new Size(ORIGIN - maxThickness / 2, ORIGIN - maxThickness / 2);
            ValueCircleRadius = new Size(ORIGIN - maxThickness / 2, ORIGIN - maxThickness / 2); ;

            BackgroundCircleStartPosition = GetPointForAngle(BackgroundCircleRadius, 0);
            BackgroundCircleEndPosition = GetPointForAngle(BackgroundCircleRadius, 360);

            ValueAngle = GetAngleForValue(minValue, maxValue, currentValue);

            ValueCircleStartPosition = GetPointForAngle(ValueCircleRadius, 0);
            ValueCircleEndPosition = GetPointForAngle(ValueCircleRadius, ValueAngle);
        }
    }

    public class InsetArcCalculator : ArcCalculatorBase {
        public InsetArcCalculator(double backgroundCircleThickness, double valueCircleThickness) : base(backgroundCircleThickness, valueCircleThickness) {

        }

        public override void Calculate(double minValue, double maxValue, double currentValue) {
            var maxThickness = Math.Max(_backgroundCircleThickness, _valueCircleThickness);

            BackgroundCircleRadius = new Size((ORIGIN - maxThickness) + (_backgroundCircleThickness / 2), (ORIGIN - maxThickness) + (_backgroundCircleThickness / 2));
            ValueCircleRadius = new Size((ORIGIN - maxThickness) + (_valueCircleThickness / 2), (ORIGIN - maxThickness) + (_valueCircleThickness / 2));

            BackgroundCircleStartPosition = GetPointForAngle(BackgroundCircleRadius, 0);
            BackgroundCircleEndPosition = GetPointForAngle(BackgroundCircleRadius, 360);

            ValueAngle = GetAngleForValue(minValue, maxValue, currentValue);

            ValueCircleStartPosition = GetPointForAngle(ValueCircleRadius, 0);
            ValueCircleEndPosition = GetPointForAngle(ValueCircleRadius, ValueAngle);
        }
    }
}
