using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace iosKeyboardTest.iOS {
    public enum TouchEventType_fallback {
        None,
        Press,
        Move,
        Release
    }
    public class TouchEventArgs_fallback : EventArgs {
        public Point Location { get; private set; }
        public TouchEventType_fallback TouchEventType { get; private set; }
        public TouchEventArgs_fallback(Point location, TouchEventType_fallback touchEventType) {
            Location = location;
            TouchEventType = touchEventType;
        }
    }
    public static class Touches_fallback {

        private static List<Touch_fallback> _touches = [];
        public static Touch_fallback Locate(Point p) {
            if(!_touches.Any()) {
                return null;
            }
            return _touches.Aggregate((a, b) => DistSquared(a.Location, p) < DistSquared(b.Location, p) ? a : b);
        }
        public static Touch_fallback Locate(string id) {
            return _touches.FirstOrDefault(x => x.Id == id);
        }
        public static Touch_fallback Update(Point p, TouchEventType_fallback touchType) {
            // returns touch at p loc
            if(touchType == TouchEventType_fallback.Press) {
                _touches.Add(new Touch_fallback(p));
                return _touches.Last();
            }
            if(Locate(p) is not { } t) {
                if(touchType == TouchEventType_fallback.Move) {
                    // probably shouldn't happen but when pointer moves onto surface
                    t = new Touch_fallback(p);
                    _touches.Add(t);
                    return _touches.Last();
                }
                return null;
            }
            if(touchType == TouchEventType_fallback.Move) {
                t.SetLocation(p);
            } else {
                RemoveTouch(t);
            }
            return t;

        }
        public static void Clear() {
            _touches.Clear();
        }
        public static double Dist(Point p1, Point p2) {
            return Math.Sqrt(DistSquared(p1,p2));
        }
        public static double DistSquared(Point p1, Point p2) {
            return Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2);
        }
        static void RemoveTouch(Touch_fallback t) {
            var up_time = DateTime.Now;
            _touches.Remove(t);
            Debug.WriteLine($"Touch time: {(DateTime.Now - t.CreatedDt).Milliseconds}ms");
        }
    }
    public class Touch_fallback {
        
        public string Id { get; set; }
        public DateTime LastEventDt { get; private set; }
        public DateTime CreatedDt { get; private set; }
        public Point Location { get; private set; }
        public Point PressLocation { get; private set; }

        private Touch_fallback() { }
        public Touch_fallback(Point p) {
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

        public Touch_fallback Clone() {
            return new Touch_fallback() {
                Id = Id,
                LastEventDt = LastEventDt,
                CreatedDt = CreatedDt,
                Location = Location,
                PressLocation = PressLocation
            };
        }
    }
}
