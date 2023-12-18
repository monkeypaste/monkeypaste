namespace MonkeyPaste.Common.Plugin {
    public class MpAnalyzerPluginResponseFormat : MpPluginResponseFormatBase {

        public override string ToString() {
            if (dataObjectLookup != null) {
                // NOTE this is a crude way to get all response text (as json)
                var avdo = new MpPortableDataObject(dataObjectLookup);
                return avdo.SerializeData();
            }
            return dataObjectLookup.ToStringOrDefault();
        }
    }
}
