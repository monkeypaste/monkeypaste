namespace MonkeyPaste {
    public static class MpMathHelpers {
        public static double WrapValue(double x, double x_min, double x_max) {
            return ((x - x_min) % (x_max - x_min)) + x_min;
        }

        public static int WrapValue(int x, int x_min, int x_max) {
            return ((x - x_min) % (x_max - x_min)) + x_min;
        }
    }
}
