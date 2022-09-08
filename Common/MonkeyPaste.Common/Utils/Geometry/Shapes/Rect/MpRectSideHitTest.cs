namespace MonkeyPaste.Common {
    public class MpRectSideHitTest {

        public static string GetSideLabel(int sideIdx) {
            if (sideIdx == 0) {
                return "l";
            }
            if (sideIdx == 1) {
                return "t";
            }
            if (sideIdx == 2) {
                return "r";
            }
            if (sideIdx == 3) {
                return "b";
            }
            return null;
        }
        public static int GetSideIdx(string sideLabel) {
            if (string.IsNullOrEmpty(sideLabel)) {
                return -1;
            }
            sideLabel = sideLabel.ToLower();
            if (sideLabel == "l") {
                return 0;
            }
            if (sideLabel == "t") {
                return 1;
            }
            if (sideLabel == "r") {
                return 2;
            }
            if (sideLabel == "b") {
                return 3;
            }
            return -1;
        }

        public MpRect TestRect { get; set; }
        public string ClosestSideLabel { get; set; }
        public double ClosestSideDistance { get; set; }
    }
}
