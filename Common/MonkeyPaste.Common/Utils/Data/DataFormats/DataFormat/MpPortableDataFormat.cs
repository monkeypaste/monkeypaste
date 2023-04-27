using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

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

    public class MpPortableDataFormatConverter : JsonConverter<MpPortableDataFormat> {
        public override void WriteJson(JsonWriter writer, MpPortableDataFormat value, JsonSerializer serializer) {
            writer.WriteValue(value.ToString());
        }

        public override MpPortableDataFormat ReadJson(JsonReader reader, Type objectType, MpPortableDataFormat existingValue, bool hasExistingValue, JsonSerializer serializer) {
            string s = (string)reader.Value;

            return new MpPortableDataFormat(s, 0);
        }
    }
}
