namespace MonkeyPaste {
    public class MpRect : MpShape {
        public MpPoint Location { get; set; } = new MpPoint();

        public MpSize Size { get; set; } = new MpSize();

        public MpRect() { }
        public MpRect(MpPoint location,MpSize size) {
            Location = location;
            Size = size;
        }
    }
}
