namespace MonkeyPaste.Common {
    public static class MpAnimationHelpers {

        public static void Spring(ref double x, ref double v, double xt, double h, double zeta = 0.22d, double omega = 25.0d) {
            /*
              from https://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/
              x     - paramValue             (input/output)
              v     - velocity          (input/output)
              xt    - target paramValue      (input)
              zeta  - damping ratio     (input)
              omega - angular frequency (input)
              h     - time step         (input)
            */
            double f = 1.0d + 2.0d * h * zeta * omega;
            double oo = omega * omega;
            double hoo = h * oo;
            double hhoo = h * hoo;
            double detInv = 1.0d / (f + hhoo);
            double detX = f * x + h * v + hhoo * xt;
            double detV = v + hoo * (xt - x);
            x = detX * detInv;
            v = detV * detInv;
        }
    }
}
