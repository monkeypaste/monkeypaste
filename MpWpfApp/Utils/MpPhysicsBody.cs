using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpPhysicsBody {
        private double _mass = 1.0;
        private double _drag = 0.1; // rho*C*Area - simplified drag for this example
        private double _px = 0, _vx = 0, _ax = 0, _fx = 0;

        private DispatcherTimer _timer;
        private DateTime _startTime,_lastTime;
        private int _tickRate = 10;
        private int _dt = 0;

        private ListBox _lb;
        private ScrollViewer _sv;
        public MpPhysicsBody(ListBox lb) {
            _lb = lb;
            _sv = _lb.GetChildOfType<ScrollViewer>();

            _timer = new DispatcherTimer();

            _timer.Tick += new EventHandler(dispatcherTimer_Tick);
            _timer.Interval = new TimeSpan(0, 0, 0,0,_tickRate);
        }
        public void Start() {
            _startTime = DateTime.Now;
            _lastTime = _startTime;
            _timer.Start();
        }
        public void Stop() {
            _timer.Stop();
        }
        public void AddForce(double f) {
            //_fx += f / _mass;
            _ax = -f / 10;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            DateTime curTime = DateTime.Now;
            TimeSpan deltaSpan = curTime - _lastTime;
            double dt = deltaSpan.TotalMilliseconds;

            //new position x
            double npx = _px + _vx * dt + _ax * (dt * dt * 0.5);
            //drag force x
            double dfx = 0.5 * _drag * (_vx * Math.Abs(_vx));// D = 0.5 * (rho * C * Area * vel^2)
            //new acceleration x
            double nax = (dfx / _mass) + _fx;
            //clear new forces
            //_fx = 0;
            //new velocity x
            double nvx = _vx + (_ax + nax) * (dt * 0.5);

            _px = npx;
            _vx = nvx;
            _ax = nax;

            _sv.ScrollToHorizontalOffset(_px);

            //Console.WriteLine("POsition offset: "+_px+" Scroll Delta: "+_fx);
            _lastTime = curTime;
        }
    }
}
