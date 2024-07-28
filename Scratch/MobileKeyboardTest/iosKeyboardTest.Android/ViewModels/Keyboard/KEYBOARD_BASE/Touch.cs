using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace iosKeyboardTest.Android {
    public enum TouchEventType {
        None,
        Press,
        Move,
        Release
    }
    public class TouchEventArgs : EventArgs {
        public string TouchId { get; private set; } = "UNDEFINED";
        public Point Location { get; private set; }
        public TouchEventType TouchEventType { get; private set; }
        public TouchEventArgs(Point location, TouchEventType touchEventType, string touchId) : this(location, touchEventType) {
            TouchId = touchId;
        }
        public TouchEventArgs(Point location, TouchEventType touchEventType) {
            Location = location;
            TouchEventType = touchEventType;
        }
        public override string ToString() {
            return $"[{TouchId}] {TouchEventType} {Location}";
        }
    }
    public static class Touches {

        public static List<Touch> _touches = [];
        public static Touch Locate(Point p) {
            return _touches.OrderBy(x => Dist(x.Location, p)).FirstOrDefault();
        }
        public static int Count =>
            _touches.Count;
        public static Touch Locate(string id) {
            return _touches.FirstOrDefault(x => x.Id == id);
        }
        public static Touch Update(string id, Point p, TouchEventType touchType) {
            // returns touch at p loc
            if(touchType == TouchEventType.Press) {
                var nt = new Touch(id,p);
                _touches.Add(nt);
                return nt;
            }
            if(Count > 1 && touchType == TouchEventType.Release) {

            }
            if(Locate(id) is not { } t) {
                if(touchType == TouchEventType.Move) {
                    // probably shouldn't happen but when pointer moves onto surface
                    t = new Touch(id,p);
                    _touches.Add(t);
                    return t;
                }
                return null;
            }
            if(touchType == TouchEventType.Move) {
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
        static void RemoveTouch(Touch t) {
            var up_time = DateTime.Now;
            _touches.Remove(t);
            //Debug.WriteLine($"Touch time: {(DateTime.Now - t.CreatedDt).Milliseconds}ms");
        }
    }
    public class Touch {
        public string Id { get; set; }
        public Point Location { get; private set; }
        public Point PressLocation { get; private set; }

        public Touch(string id, Point p)  {
            PressLocation = p;
            Location = p;
            Id = id ?? System.Guid.NewGuid().ToString();
        }
        public void SetLocation(Point p) {
            Location = p;
        }
    }
}
