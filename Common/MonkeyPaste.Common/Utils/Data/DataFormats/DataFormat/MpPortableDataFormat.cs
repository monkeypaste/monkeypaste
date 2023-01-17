namespace MonkeyPaste.Common {
    public class MpPortableDataFormat {
        public string Name { get; private set; }

        public int Id { get; private set; }

        public MpPortableDataFormat(string name, int id) {
            Name = name;
            Id = id;
        }

        public override string ToString() {
            return $"{Name}";
        }
    }
}
