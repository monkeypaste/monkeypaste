using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iosKeyboardTest {
    public enum TouchEventType {
        Press,
        Move,
        Release
    }
    public class TouchEventArgs : EventArgs {
        public Point Location { get; private set; }
        public TouchEventType TouchEventType { get; private set; }
        public TouchEventArgs(Point location, TouchEventType touchEventType) {
            Location = location;
            TouchEventType = touchEventType;
        }
    }
    public static class Touches {

        private static List<Touch> _touches = [];
        public static Touch Primary =>
            _touches.OrderBy(x => x.CreatedDt).FirstOrDefault();
        public static Touch Locate(Point p) {
            if(!_touches.Any()) {
                return null;
            }
            return _touches.Aggregate((a, b) => Dist(a.Location, p) < Dist(b.Location, p) ? a : b);
        }
        public static Touch Locate(string id) {
            return _touches.FirstOrDefault(x => x.Id == id);
        }
        public static Touch Update(Point p, TouchEventType touchType) {
            // returns touch at p loc
            if(touchType == TouchEventType.Press) {
                _touches.Add(new Touch(p));
                return _touches.Last();
            }
            if(Locate(p) is not { } t) {
                if(touchType == TouchEventType.Move) {
                    // probably shouldn't happen but when pointer moves onto surface
                    t = new Touch(p);
                    _touches.Add(t);
                    return _touches.Last();
                }
                return null;
            }
            if(touchType == TouchEventType.Move) {
                t.SetLocation(p);
            } else {

                _touches.Remove(t);
            }
            return t;

        }
        public static void Clear() {
            _touches.Clear();
        }
        public static double Dist(Point p1, Point p2) {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }
    public class Touch {
        
        public string Id { get; set; }
        public DateTime LastEventDt { get; private set; }
        public DateTime CreatedDt { get; private set; }
        public Point Location { get; private set; }
        public Point PressLocation { get; private set; }

        private Touch() { }
        public Touch(Point p) {
            PressLocation = p;
            Location = p;
            CreatedDt = DateTime.Now;
            LastEventDt = DateTime.Now;
            Id = System.Guid.NewGuid().ToString();
        }
        public void SetLocation(Point p) {
            Location = p;
            LastEventDt = DateTime.Now;
        }

        public Touch Clone() {
            return new Touch() {
                Id = Id,
                LastEventDt = LastEventDt,
                CreatedDt = CreatedDt,
                Location = Location,
                PressLocation = PressLocation
            };
        }
    }
}
